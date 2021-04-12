using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonScripts : MonoBehaviour
{
    public GameObject a;
    public void fixBound()
    {
        if (a.transform.position.y < 0)
        {
            a.transform.Translate(new Vector3(0, -a.transform.position.y, 0));
        }
    }

    void Start()
    {
        
    }
}
