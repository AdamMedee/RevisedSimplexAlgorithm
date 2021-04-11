using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ButtonFuncs3 : MonoBehaviour
{
    private int CurrentEqn2D = 0;
    private float gap = (float)-0.7;
    private GameObject add2dbutton;
    public GameObject eqn2dpre;
    public GameObject eqn2dholder;
    private int eqnCount = 1;
    
    //add 2d eqn
    public void NewEqn2D()
    {
        if (add2dbutton == null)
        {
            add2dbutton = GameObject.Find("AddEqnButton3D");
        }
        int g = 1;
        while (true)
        {
            var tmp = GameObject.Find("InpEqn3d" + g);
            if (tmp == null)
            {
                break;
            }
            g++;
        }
        //print(add2dbutton.transform.position);
        var a = Instantiate(eqn2dpre, add2dbutton.transform.position, Quaternion.identity, eqn2dholder.transform);
        a.name = "InpEqn3d" + g;
        add2dbutton.transform.Translate(new Vector3(0, gap, 0));
    }

    public void DelEqn2D()
    {
        string n = transform.parent.name;
        string m = n.Substring(8);
        int a = Int32.Parse(m);
        add2dbutton.transform.Translate(new Vector3(0, -gap, 0));
        Destroy(transform.parent.gameObject);
        a++;
        while (true)
        {
            var tmp = GameObject.Find("InpEqn3d" + a);
            if (tmp == null)
            {
                print(a);
                break;
            }
            tmp.transform.Translate(new Vector3(0, -gap, 0));
            tmp.transform.name = "InpEqn3d" + (a-1);
            a++;
        }
    }
    

    private void Start()
    {
        add2dbutton = GameObject.Find("AddEqnButton3D");
    }

    private void Update()
    {

    }
}