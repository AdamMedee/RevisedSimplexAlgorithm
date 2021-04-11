using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ButtonFuncs : MonoBehaviour
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
            add2dbutton = GameObject.Find("AddEqnButton2D");
        }
        int g = 1;
        while (true)
        {
            var tmp = GameObject.Find("InpEqn2d" + g);
            if (tmp == null)
            {
                break;
            }
            g++;
        }
        //print(add2dbutton.transform.position);
        var a = Instantiate(eqn2dpre, add2dbutton.transform.position, Quaternion.identity, eqn2dholder.transform);
        a.name = "InpEqn2d" + g;
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
            var tmp = GameObject.Find("InpEqn2d" + a);
            if (tmp == null)
            {
                print(a);
                break;
            }
            tmp.transform.Translate(new Vector3(0, -gap, 0));
            tmp.transform.name = "InpEqn2d" + (a-1);
            a++;
        }
    }
    

    private void Start()
    {
        add2dbutton = GameObject.Find("AddEqnButton2D");
    }

    private void Update()
    {

    }
}
