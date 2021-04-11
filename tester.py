import numpy as np
import random
from simplex import *

from scipy.optimize import linprog
def removeRedundant(A,b):
    '''A takes the form of m x n np array, b is n x 1 np array'''
    
    if len(A) == 0: return [A,b]
    row = 0
    while row < len(A):
        '''
            For an inequality c^Tx <= b_c in Ax <= b, we solve the LP problem
            max c^Tx subject to A'x <= b', c^Tx <= b_c + 1
            where A',b' are A,b excluding c^T,b_c respectively

            Then, if the optimal value is less than or equal to b_c, then
            it means that c^Tx <= b_c is redundant
        '''

        b[row] += 1
        optVal = linprog(-1 * A[row], A, b, method='simplex').fun * -1
        b[row] -= 1

        if optVal <= b[row]:
            A = np.delete(A, row, 0)
            b = np.delete(b, row, 0)
        else:
            row += 1
        
    return A, b

X = SimplexSolver()

def generate(m, n, redundant=False, bNeg = True, feasible = True):
    '''
    Generates a feasible system with m constaints and n variables
    If redundant = False, the system will not be redundant
    If feasible = True, the system will be feasible
    If bNeg = True, the vector b can have negative entries
    '''
    A = np.array([[random.randint(-100, 100) for i in range(n)]])
    b = np.array([random.randint(-100 if bNeg else 0, 100)])
    c = np.array([random.randint(-100, 100) for i in range(n)])

    while A.shape[0] < m:
        row = [random.randint(-100, 100) for i in range(n)]
        b_v = random.randint(-100 if bNeg else 0, 100)

        if feasible:
            try:
                res = X.solve(np.append(A, [row], axis = 0), np.append(b, b_v), c)
            except:
                print("!!!")
                return A, b, c

            if res['status'] != 2:
                A = np.append(A, [row], axis = 0)
                b = np.append(b, b_v)
        else:
            A = np.append(A, [row], axis = 0)
            b = np.append(b, b_v)

        if not redundant:
            removeRedundant(A,b)

    return A, b, c

A, b, c = generate(100, 50, redundant=True, bNeg = False, feasible=False)


print(A)
print()
print(b)
print()
print(c)
print()


import time
st = time.time()
res = X.solve(A, b, c, keepHistory=False, keepVars = False)
print(time.time() - st)
pprint.pprint(res, width = 1)

'''
[[ -58   75   42  -76   28  -50   14   92    7]
 [  34  -28   53   73   35    6  -97   15   35]
 [  57   68  -39  -11    7   98   59  -79  -93]
 [ -57   79   93   21  -59  -17   59  -66   98]
 [  48   11  -21   59  100   36  -94  -31   23]
 [  94  -33  -52   28  -66  -12  -45  -28  -90]
 [  69   18   58  -38  -90   75  -83   84  -40]
 [-100  -77  -88   77   48    8  -66   88  -62]
 [  27   74  -84  -56   24  -27   66  -78    8]]

[-65  17  74  -3 -85 -12  82  48 -71]

[ -91   29   72   75   61  -47    6   46 -100]

{'basis': array([ 0,  3,  6,  7,  8, 13, 14, 15, 17]),
 'certificate': array([-42.26083292,  42.68663682,  41.37943647, -21.42892988,   0.        ,  -0.        ,   0.        ,   2.28396218,  -0.        ]),
 'pivots': 4,
 'solution': array([ 1.84919694e+01, -1.38777878e-17,  0.00000000e+00,  2.46370311e+01, -5.55111512e-17,  0.00000000e+00,  3.06146707e+01,  2.62861194e+01,  4.71717257e+00]),
 'status': 0,
 'val': 1086.1403799657874,
 'verdict': 'Optimal'}
 '''

'''
2D:
 [[ 76 -33]
 [ 22 -81]
 [-51  89]
 [-17  -9]
 [-75 -72]]

[99 18 39 19 72]

[ 84 -25]

{'basis': array([0, 1, 3, 5, 6], dtype=int32),
 'certificate': array([1.22042905e+00, 2.48624530e-17, 1.71619760e-01, 0.00000000e+00, 0.00000000e+00]),
 'history': {'bases': [array([2, 3, 4, 5, 6], dtype=int32),
                       array([0, 2, 4, 5, 6], dtype=int32),
                       array([0, 1, 4, 5, 6], dtype=int32),
                       array([0, 1, 3, 5, 6], dtype=int32)],
             'costs': [0.0,
                       68.72727272727273,
                       111.13259668508287,
                       127.51564652627434],
             'steps': [array([0., 0.]),
                       array([0.81818182, 0.        ]),
                       array([1.36740331, 0.14917127]),
                       array([1.98740405, 1.57705176])]},
 'pivots': 3,
 'solution': array([1.98740405, 1.57705176]),
 'status': 0,
 'val': 127.51564652627434,
 'verdict': 'Optimal'}
 '''