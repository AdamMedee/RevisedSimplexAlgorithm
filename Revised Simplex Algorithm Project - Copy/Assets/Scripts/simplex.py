import numpy as np
import pprint
np.set_printoptions(linewidth=np.inf)

'''
TODO:
- Add option to toggle storing history / just report value
    - Only display the variables corresponding to the given variables (Not slack / artificial)
- Implement certificates (optimality, unbounded, infeasible)

- Generate large systems and test the 3 conditions (random + remove redundant ineq)
- Pertubation to prevent degenerate solutions
- Different pivot rules (Prevent cycling)
- LU factorization?

'''

class SimplexSolver:

    def initialize(self, A, b, c):
        '''
        Initialize the LP max(c^T x : Ax <= b)
        '''
        m = len(A)
        n = len(A[0])

        assert len(b) == m
        assert len(c) == n

        A = np.array(A, dtype='float64')
        b = np.array(b, dtype='float64').reshape((m,1)) 
        c = np.array(c, dtype='float64').reshape((n,1))

        return self.preprocess(A, b, c)

    def preprocess(self, A, b, c):
        '''
        Convert the system Ax <= b into slack (equality) form [A|s]x = b
        '''
        m = len(A)
        n = len(A[0])
        auxCount = 0    #Number of artificial variables to add

        newA = np.zeros((m, m+n))
        newA[:, :n] = A
        for i in range(m):
            newA[i][n+i] = 1
            if b[i] < 0:            #If b[i] < 0, flip the inequality and mark that we need an artificial variable
                newA[i] = -newA[i]
                auxCount += 1

        newb = abs(b)

        newC = np.zeros((n+m, 1))   #Extend c so the dimensions match
        newC[:n, :] = c
        return newA, newb, newC, auxCount

    def phase1(self, A, b, auxCount):
        '''
        Solves the auxilary problem to find a basic solution
        Assumes the system is in the form Ax = b, x >= 0 and slack variables have been added in each equation

        IN: A, b        -Ax = b
            auxCount    -Number of artificial variables to add in the auxilary system

        OUT: result{
                status : 0 if successful, 
                         2 if infeasible
                basis : Indices of columns in A corresponding to a basic feasible soluton, if it exists
                certtificate :  y so that yA >= 0 but yb < 0 if infeasible
                verdict : String representing status
            }
        '''

        m = len(A)
        n = len(A[0])

        #Set up Auxilary problem
        A_aux = np.zeros((m, n + auxCount))
        A_aux[:, :n] = A 
        c_aux = np.zeros((n + auxCount, 1))
        basis = []
        flip = np.ones(m)   #Keep track of which equations we add artificial variables to
        cnt = 0
        for i in range(m):  #Create a basis of slack and artificial variables
            lastIdx = np.nonzero(A[i])[0][-1]
            if(A[i][lastIdx] < 0):      #If we flipped the inequality, add artificial variabale in equation i
                A_aux[i][n + cnt] = 1
                c_aux[n + cnt] = -1
                basis.append(n + cnt)
                cnt += 1
                flip[i] = -1
            else:
                basis.append(lastIdx)

        aux_result = self.phase2(A_aux, b, c_aux, np.sort(basis, kind="stable"), keepHistory=False, keepVars=True)
        aux_basis = aux_result['basis']
        result = dict()

        if abs(aux_result['val']) > 1/1000:
            result['status'] = 2    #not feasible -- artificial variables cannot all be 0
            result['verdict'] = "Infeasible"
            result['certificate'] = np.linalg.solve(A_aux[:, aux_basis].T, c_aux[aux_basis]).reshape(m) * flip
            return result

        #Return a basis from the aux problem 
        result['status'] = 0
        for i in range(len(aux_basis)):
            if aux_basis[i] >= n:   #There is an artifcial variable in the basis, remove it 
                mask = np.ones(n+auxCount, dtype = bool)
                mask[aux_basis] = False
                nonBasicIdx = np.arange(0, n + auxCount, 1)[mask]

                e_i = np.zeros((m, 1))
                e_i[i] = 1
                row = np.dot( np.dot(e_i.T, np.linalg.inv(A_aux[:,aux_basis])), A_aux[:, mask]) # e_i^T * inv(A_B) * A_N - Row of artifical variable

                idx = np.nonzero(row[0])[0]     #Convert row to 1D array, get nonzero indices
                if len(idx) == 0:
                    print("Redundant system")   #Should not happen since we added slack variables
                    aux_basis[i] = -1           #Will be deleted -> Need to somehow remove the i^th row from A as well**
                else:
                    aux_basis[i] = nonBasicIdx[idx[0]] #Swap i with a non-basic variable corresponding to a linearly indep column
                    aux_basis = np.sort(aux_basis, kind="stable") 

        result['basis'] = np.sort(aux_basis[aux_basis >= 0], kind="stable")
        return result

    def phase2(self, A, b, c, basis, A_vars = -1, keepHistory = True, keepVars = False):
        ''' 
        max(c^Tx | Ax = b, x >= 0)
        Solves the LP given a basis corresponding to a basic solution

        IN:     The system Ax = b
                The objectve function c^Tx
                A basis corresponsing to a BFS
                A_vars - The number of variables to keep if keepVars if off
                keepHistory - Toggles if history is stored
                keepVars - Toggles if artificial and slack variables are presented in the result

        OUT: result{
                status : 0 if sucessful, 1 if unbounded
                val : Optimal value if it exists
                solution : Optimal solution if it exists
                certificate :   If optimal   : y so that c^Tx = yb
                                If unbounded : d so that x(t) = result['solution'] + t * d is feasible for any t>0 with c^T * x(t) increasing
                basis : final basis
                history{ (if keepHistory == True)
                    steps : Array of solutions / vertices traversed
                    costs : Array of costs encountered
                    bases : Array of bases considered
                }
                pivots : number of pivots used
                verdict : string representing status
            }
        '''

        m = len(A)
        n = len(A[0])
        result = {
            'status' : -1,
            'pivots' : 0
        }
        if keepHistory:
            result['history'] = {
                'steps' : [],
                'costs' : [],
                'bases' : []
            }
        
        assert len(basis) == m
        assert len(b) == m
        assert len(c) == n
        if not keepVars:
            assert A_vars > 0

        A = np.array(A)
        b = np.array(b).reshape((m,1)) 
        basis = np.array(basis)          
        c = np.array(c).reshape((n,1))
        solution = np.zeros((n, 1))
    
        A_B = A[:, basis]
        x_b = np.linalg.solve(A_B, b) #Initial basic sol : x_b = inv(A_b) * b
        solution[basis] = x_b

        while True:
            #Store current iteration in history
            result['solution'] = solution.reshape(n) if keepVars else solution.reshape(n)[:A_vars]
            result['basis'] = basis
            result['val'] = np.sum(np.dot(c.T, solution))

            if keepHistory:
                result['history']['steps'].append(np.copy(result['solution']))
                result['history']['costs'].append(result['val'])
                result['history']['bases'].append(np.copy(result['basis']))

            #Pricing
            mask = np.ones(c.shape[0], dtype = bool)
            mask[basis] = False                                 #True for each non-basic variable
            nonBasicIdx = np.arange(0, c.shape[0], 1)[mask]     #map frpm nonbasic indices to all indices
            
            A_n = A[:, mask]
            c_n = c[mask]
            tmp = np.linalg.solve(A_B.T, c[basis])
            D_n = c_n - np.dot(A_n.T, tmp) 

            if sum(D_n > 0)[0] == 0:    #All reduced costs are non-positive
                #Optimal solution found
                result['status'] = 0
                result['verdict'] = "Optimal"
                result['certificate'] = tmp.reshape(m)
                return result
            
            j = np.argmax(D_n > 0)  #Pick the first positive entry; the jth non-basic variable enters
            j = nonBasicIdx[j]      #Convert nonBasic index to index in the system
            
            #FTRAN
            y = np.linalg.solve(A_B, A[:, j])
            y = y[:, np.newaxis]    #force column vector

            #Ratio Test
            if sum(y > 0)[0] == 0:
                result['status'] = 1
                result['val'] = np.Infinity
                result['verdict'] = "Unbounded"
                result['certificate'] = (-y).reshape(m)
                return result
            
            t = -1 #new value for entering variable
            l = -1 #index of leaving variable
            for i in range(m):
                if y[i] > 0:
                    rt = solution[basis[i]] / y[i]
                    if t == -1 or rt < t:
                        t = rt
                        l = i 
            assert t >= 0

            #Update
            solution[basis] -= t * y
            solution[j] = t
            basis[l] = j
            basis = np.sort(basis, kind="stable")
            A_B = A[:, basis]
            result['pivots'] += 1

        result['verdict'] = "ERROR"
        return result
    
    def solve(self, A, b, c, keepHistory = True, keepVars = False):
        A_vars = len(A[0])      #Number of variables initially in the system

        A, b, c, auxCount = self.initialize(A, b, c)
        phase1Result = self.phase1(A, b, auxCount)

        if phase1Result['status'] != 0:
            return phase1Result

        return self.phase2(A, b, c, phase1Result['basis'], A_vars, keepHistory, keepVars)


X = SimplexSolver()


#f = open("testdata.txt", "r")#
f = open("Assets/Scripts/testdata.txt", "r")

n = int(f.readline().strip())
m = int(f.readline().strip())

Aq = []
for i in range(m):
    Aq.append([float(j) for j in f.readline().split()])
Bq = [float(j) for j in f.readline().split()]
Cq = [float(j) for j in f.readline().split()]
A = np.array(Aq)
b = np.array(Bq)
c = np.array(Cq)

sol = X.solve(A, b, c, keepHistory=True, keepVars = False)
finalVerd = sol["verdict"]
finalSol = sol["solution"]
finalVal = sol["val"]

f.close()

print(finalVerd)
if finalVerd == "Optimal":
    
    print(finalVal)
    for q in list(finalSol):
        print(q)
    print(len(sol["history"]["steps"]))
    for w in sol["history"]["steps"]:
        for t in w:
            print(t)
    

'''
#Opt 6.5
#(0, 5, 1.5, 0, 0, 3)
A = np.array([
    [-8, -3, 12, 1, 0, 0],
    [-2, -1, 6,  0, 1, 0],
    [ 3,  1,-4,  0, 0, 1],
    [8, 3, -12, -1, 0, 0],
    [2, 1, -6,  0, -1, 0],
    [ -3,  -1,4,  0, 0, -1]
    ])
b = np.array([3,4,2, -3,-4,-2])
c = np.array([-1, 1, -1, -1, -1, 1])
'''
'''
#Opt: 9
# (0,0,3,4)
A = np.array([
    [1, 3, -1, 2],
    [1, -3, 5, -4],
    [-1, -3, 1, -2],
    [-1, 3, -5, 4]
    ])
b = np.array([5, -1, -5, 1])
c = np.array([1, 0, 3, 0])
'''

'''
#Infeasible (Artificial not 0)
A = np.array([
    [1,2,3,4],
    [3, 10, 7, 4],
    [2, 5, 3, 1],
    [-4,-3,2,5],
    [-3, -10, -7, -4],
    [-2,4,5,7],
    [-2, -5, -3, -1],
    ])
b = np.array([1, 2, 3, 6, -2, 4, -3])
c = np.array([2, -1, 0, 0])
'''
'''
#Unbounded
A = np.array([
    [1, -1, -1],
    [7, -8, -11],
    [2, -2, -3]
    ])
b = np.array([1, 2, 1])
c = np.array([3, -2, -3])
'''
'''
A = np.array([[-44,  21],
[-93, -83],
[-98,  11],
[ -6,   2],
[-63,  47],
[-41,  58],
[-36, -25]])
b = np.array([-70,  29,  98,  47, -77,   7, -69])
c = np.array([-28, 34])
'''

#pprint.pprint(sol, width=1)
#pprint.pprint(X.solve(A, b, c, keepHistory=False, keepVars = False), width = 1)


