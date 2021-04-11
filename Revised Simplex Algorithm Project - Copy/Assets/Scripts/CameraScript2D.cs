using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript2D : MonoBehaviour
{
    private Camera cam;
    public Material mat;
    Vector3 mousePos;
    private float cameraSize;
    private Vector2 cameraOffset;
    
    // Start is called before the first frame update
    void Start()
    {
        cam = gameObject.GetComponent<Camera>();
        cameraSize = cam.orthographicSize;
    }

    // Update is called once per frame
    void Update()
    {
        //mousePos = Input.mousePosition;
        // Press space to update startVertex

        
        float zoom = (float)0.98;
        float move = (float) 0.002;
        move *= cam.orthographicSize;

        if(Input.GetAxisRaw("Mouse ScrollWheel") > 0)
        {
            cameraSize *= zoom;
            cam.orthographicSize *= zoom;
        }
        else if(Input.GetAxisRaw("Mouse ScrollWheel") < 0)
        {
            cameraSize *= 1/zoom;
            cam.orthographicSize *= 1/zoom;
        }

        if (Input.GetKey(KeyCode.D))
        {
            transform.Translate(new Vector3(move, 0, 0));
            cameraOffset += new Vector2(move, 0);
        }

        if (Input.GetKey(KeyCode.W))
        {
            transform.Translate(new Vector3(0, move, 0));
            cameraOffset += new Vector2(0, move);
        }
        
        if (Input.GetKey(KeyCode.S))
        {
            transform.Translate(new Vector3(0, -move, 0));
            cameraOffset += new Vector2(0, -move);
        }
        
        if (Input.GetKey(KeyCode.A))
        {
            transform.Translate(new Vector3(-move, 0, 0));
            cameraOffset += new Vector2(-move, 0);
        }
    }

    
  
      // Will be called from camera after regular rendering is done.
      void OnPostRender()
      {
          
          if (!mat)
          {
              Debug.LogError("Please Assign a material on the inspector");
              return;
          }
          GL.PushMatrix();
          mat.SetPass(0);
          GL.LoadOrtho();
  
          GL.Begin(GL.LINES);
          GL.Color(Color.black);
  
          float w = Screen.width;
          float h = Screen.height;
          
          for (int i = 0; i < 10; i++)
          {
              GL.Vertex(new Vector3(0, (9999999+(float)i/(10)-cameraOffset.y)%1, 0));
              GL.Vertex(new Vector3(1, (9999999+(float)i/(10)-cameraOffset.y)%1, 0)); 
          }
  
          for (int j = 0; j < 20; j++)
          {
              GL.Vertex(new Vector3((9999999+(float)j/(20)-cameraOffset.x)%1, 0,0));
              GL.Vertex(new Vector3((9999999+(float)j/(20)-cameraOffset.x)%1, 1, 0)); 
          }

        GL.End();
  
        GL.PopMatrix();
      }
}
