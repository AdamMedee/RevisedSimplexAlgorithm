using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.U2D;
using Quaternion = System.Numerics.Quaternion;

public class Camera3dcontrol : MonoBehaviour
{
    private Camera cam;

    
    // Start is called before the first frame update
    void Start()
    {
        cam = gameObject.GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {

        
    }
}
