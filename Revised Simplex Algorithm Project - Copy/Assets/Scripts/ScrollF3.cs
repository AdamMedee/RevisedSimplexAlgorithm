using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Matrix4x4 = System.Numerics.Matrix4x4;
using Object = System.Object;
using Quaternion = System.Numerics.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;



public class ScrollF3 : MonoBehaviour
{
    public GameObject a;
    public GameObject shape;
    public GameObject linep;
    public Material optmat;
    public void fixBound()
    {
        if (a.transform.position.y > 200)
        {
            //a.transform.Translate(new Vector3(0, -a.transform.position.y, 0));
        }
        //print(a.transform.position);
    }

    public void GenerateVisual()
    {
        List<Vector3> ineqs = new List<Vector3>();
        List<float> ineqsvals = new List<float>();
        int num = 1;
        while (true)
        {
            string tmps = "InpEqn2d" + num;
            var t = GameObject.Find(tmps);
            if (t == null)
            {
                break;
            }

            string ta = "a";
            string tb = "b";
            string tc = "c";
            string td = "d";
            float fa, fb, fc, fd;
            foreach(Transform child in t.transform)
            {
                if (child.name == "InputA")
                {
                    ta = child.gameObject.GetComponent<InputField>().text;
                }
                else if (child.name == "InputB")
                {
                    tb = child.gameObject.GetComponent<InputField>().text;
                }
                else if (child.name == "InputC")
                {
                    tc = child.gameObject.GetComponent<InputField>().text;
                }
                else if (child.name == "InputD")
                {
                    td = child.gameObject.GetComponent<InputField>().text;
                }
            }

            if (float.TryParse(ta, out fa) && float.TryParse(tb, out fb) && float.TryParse(tc, out fc) && float.TryParse(td, out fd))
            {
                Vector3 ineqt = new Vector3(fa, fb, fc);
                ineqs.Add(ineqt);
                ineqsvals.Add(fd);
            }

            num++;
        }
        
        
        //-------------------------------------------------------------------
        //convert to 3d shit
        //-------------------------------------------------------------------

        
        List<Vector2> points = new List<Vector2>();
        int lineCount = ineqs.Count;
        for (int i = 0; i < lineCount; i++)
        {
            for (int j = 0; j < i; j++)
            {
                int spot = (i * (i - 1)) / 2 + j;
                float a1 = ineqs[i].x;
                float a2 = ineqs[j].x;
                float b1 = ineqs[i].y;
                float b2 = ineqs[j].y;
                float c1 = ineqs[i].z;
                float c2 = ineqs[j].z;
                if (Math.Abs(a1*b2 - a2*b1) > 0.0001)
                {
                    points.Add(new Vector2((b2*c1-b1*c2)/(a1*b2-a2*b1), (a1*c2-a2*c1)/(a1*b2-a2*b1)));
                    
                }
            }
        }
        float maxmag = 1;
        for (int i = points.Count - 1; i >= 0; i--)
        {
            bool tmpb = false;
            for (int j = 0; j < lineCount; j++)
            {
                if (ineqs[j].x*points[i].x + ineqs[j].y*points[i].y > (ineqs[j].z+0.0001))
                {
                    tmpb = true;
                }
            }

            maxmag = Math.Max(maxmag, Math.Max(Math.Abs(points[i].x), Math.Abs(points[i].y)));
            if (tmpb)
            {
                points.RemoveAt(i);
            }
        }

        maxmag = (int) (maxmag * 100)+1000;
        Vector3[] boundList = new Vector3[4];
        boundList[0] = new Vector3(1, 0, maxmag);
        boundList[1] = new Vector3(0, 1, maxmag);
        boundList[2] = new Vector3(-1, 0, maxmag);
        boundList[3] = new Vector3(0, -1, maxmag);
        
        for (int i = 0; i < lineCount; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                int spot = (i * (i - 1)) / 2 + j;
                float a1 = ineqs[i].x;
                float a2 = boundList[j].x;
                float b1 = ineqs[i].y;
                float b2 = boundList[j].y;
                float c1 = ineqs[i].z;
                float c2 = boundList[j].z;
                if (Math.Abs(a1*b2 - a2*b1) > 0.0001)
                {
                    points.Add(new Vector2((b2*c1-b1*c2)/(a1*b2-a2*b1), (a1*c2-a2*c1)/(a1*b2-a2*b1)));
                    
                }
            }
        }
        
        for (int i = points.Count - 1; i >= 0; i--)
        {
            bool tmpb = false;
            for (int j = 0; j < lineCount; j++)
            {
                if (ineqs[j].x*points[i].x + ineqs[j].y*points[i].y > (ineqs[j].z+0.0001))
                {
                    tmpb = true;
                }
            }
            if (tmpb)
            {
                points.RemoveAt(i);
            }
        }
        
        
        Vector2 centroid = new Vector2(0, 0);
        foreach (Vector2 p in points)
        {
            centroid += p;
        }
        centroid /= points.Count;

        List<float> angles = new List<float>();
        List<float> angles2 = new List<float>();
        foreach (Vector2 p in points)
        {
            Vector3 tmpa = new Vector3(-centroid.x + p.x, -centroid.y + p.y, 0);
            float ang = Vector3.Angle(tmpa, new Vector3(1, 0, 0));
            if (tmpa.y < 0)
            {
                ang = 360 - ang;
            }
            angles.Add(ang);
            angles2.Add(ang);
        }
        angles.Sort();
        List<Vector2> finalPoints = new List<Vector2>();
        foreach (float a in angles)
        {
            finalPoints.Add(points[angles2.IndexOf(a)]);
        }
        
        //Add points that intersect with very far away
        
        

        
        DontDestroyOnLoad(shape);
        
        for (int i = 0; i < finalPoints.Count; i++) //Vector2 p in finalPoints)
        {
            var k = Instantiate(linep, shape.transform);
            LineRenderer ltmp = k.GetComponent<LineRenderer>();
            ltmp.SetPosition(0, finalPoints[i]);
            ltmp.SetPosition(1, finalPoints[(i+1)%finalPoints.Count]);
        }
        
        //write into txt file
        string cx = GameObject.Find("c-x").GetComponent<InputField>().text;
        string cy = GameObject.Find("c-y").GetComponent<InputField>().text;
        
        string txtPath = "Assets/Scripts/testdata.txt";
        using (FileStream fs = File.Create(txtPath)) { }
        StreamWriter writer = new StreamWriter(txtPath, true);

        int n = 2;
        int m = lineCount;
        writer.WriteLine(n);
        writer.WriteLine(m);
        
        //writing A
        
        for (int i = 0; i < ineqs.Count; i++)
        {
            writer.WriteLine(ineqs[i].x.ToString() + " " + ineqs[i].y.ToString());
        }
        
        //writing b
        string tmpss = "";
        for (int i = 0; i < ineqs.Count; i++)
        {
            tmpss += ineqs[i].z.ToString()+" ";
        }
        writer.WriteLine(tmpss);
        
        //writing c
        string tmpsc = cx + " " + cy;
        
        writer.WriteLine(tmpsc);
        
        writer.Close();
        
        
        //run python file
        string fileName = "Assets/Scripts/simplex.py";

        string pythonPtxt = "Assets/Scripts/python-path.txt";
        string text = System.IO.File.ReadAllText(pythonPtxt);

        string pythonP = @text;

        
        string progToRun = fileName;
        char[] splitter = {'\r'};
 
        Process proc = new Process();
        proc.StartInfo.FileName = pythonP;
        proc.StartInfo.RedirectStandardOutput = true;
        proc.StartInfo.UseShellExecute = false;
 
        // call hello.py to concatenate passed parameters
        proc.StartInfo.Arguments = progToRun;
        proc.Start();
        proc.WaitForExit();
        StreamReader sReader = proc.StandardOutput;
        string[] output = sReader.ReadToEnd().Split(splitter);

        if (output[0] == "Optimal")
        {
            float val = float.Parse(output[1]);
            float[] soll = new float[2];
            soll[0] = float.Parse(output[2]);
            soll[1] = float.Parse(output[3]);
            int stepl = int.Parse(output[4]);
            Vector2[] steps = new Vector2[stepl];
            for (int i = 0; i < stepl; i++)
            {
                steps[i].x = float.Parse(output[5 + i*2]);
                steps[i].y = float.Parse(output[6 + i * 2]);
            }
            //create optimal line
            List<Vector2> solpoints = new List<Vector2>();
            if (Math.Abs(soll[0]) <= 0.01)
            {
                solpoints.Add(new Vector2(-maxmag, val+soll[0]*maxmag));
                solpoints.Add(new Vector2(maxmag, val-soll[0]*maxmag));
            }
            if (Math.Abs(soll[1]) <= 0.01)
            {
                solpoints.Add(new Vector2(val+soll[0]*maxmag, -maxmag));
                solpoints.Add(new Vector2( val-soll[0]*maxmag, maxmag));
            }
            print(111);
            solpoints.Sort((a, b) => a.x.CompareTo(b.x));
            for (int i = 0; i < solpoints.Count; i++) //Vector2 p in finalPoints)
            {
                var k = Instantiate(linep, shape.transform);
                LineRenderer ltmp = k.GetComponent<LineRenderer>();
                ltmp.SetPosition(0, solpoints[i]);
                ltmp.SetPosition(1, solpoints[(i+1)%solpoints.Count]);
                ltmp.material = optmat;
                ltmp.material.color = Color.blue;
                print(solpoints[i]);
            }
            print(222);
            
            
            for (int i = 0; i < stepl; i++)
            {
                var k = Instantiate(linep, shape.transform);
                LineRenderer ltmp = k.GetComponent<LineRenderer>();
                ltmp.SetPosition(0, new Vector3(steps[i].x, steps[i].y));
                ltmp.SetPosition(1, new Vector3(steps[i].x+(float)0.22, steps[i].y+(float)0.22));
                ltmp.material = optmat;
                ltmp.material.color = Color.cyan;
                ltmp.startWidth = 0.1f;
                ltmp.endWidth = 0.1f;
            }

        }
        
 
        proc.WaitForExit();
        
        
        

        SceneManager.LoadScene("3DVisual");
    }

    void Start()
    {
        
    }
}