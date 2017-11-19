
using UnityEngine;
using System;
using System.Text;
using System.Collections;
using System.IO.Ports;


public class AIController : MonoBehaviour
{
    private Rigidbody rb;
    Vector3 origin;
    SerialPort sp, spR;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        origin = rb.position;
        sp = new SerialPort("\\\\.\\COM4", 9600);
        if (!sp.IsOpen)
        {
            sp.Open();
            sp.ReadTimeout = 50;
        }
    }

    void reset()
    {
        rb.position = origin;
    }

    void FixedUpdate()
    {
        float r = Input.GetAxis("Cancel");
        if (!sp.IsOpen)
        {
            sp.Open();
            sp.ReadTimeout = 50;
        }
        if (r != 0.0f)
        {
            reset();
        }

        
        if (sp.IsOpen)
        {
            sp.Write("B");
            string data = sp.ReadLine();
            if (data[0]=='B') {
                print(sp.ReadLine());
            }
        }
        
    }
    private void OnApplicationQuit()
    {
        //sp.Close();
    }
}