using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class MainButtonFuncs : MonoBehaviour
{
    public GameObject pythonpathinp;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void SetPythonPath()
    {
        string txtPath = "Assets/Scripts/python-path.txt";
        using (FileStream fs = File.Create(txtPath)) { }
        StreamWriter writer = new StreamWriter(txtPath, true);
        string newpath = pythonpathinp.GetComponent<InputField>().text;
        writer.WriteLine(@newpath);
        writer.Close();
    }
}
