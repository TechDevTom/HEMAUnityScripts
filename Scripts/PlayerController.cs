using System;
using System.Collections;
using System.IO.Ports;

using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private float[,] orb;
    private float[] v, p;
    //private float[] rpos = {0,0,0};
    //private float x=0,y=0,z=0,vx=0,vy=0,vz=0;
    private Rigidbody rb;
    private Quaternion orientation;
    Vector3 origin;
    SerialPort sp;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        origin = rb.position;
        
        orb = new float[,] { {1,0,0},{0,1,0},{0,0,1} };
        v = new float[] { 0, 0, 0 };
        p = new float[] { 0, 0, 0 };

        sp = new SerialPort("////.//COM4", 9600);
        if (!sp.IsOpen)
        {
            sp.Open();
            sp.ReadTimeout = 50;
            sp.Handshake = Handshake.None;
            if (sp.IsOpen) { print("Open"); }
            
        }
    }

    void Reset()
    {
        rb.position = origin;
    }

    void Update()
    {
        
        float[] data;
        float[,] a = { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, 9 } };
        float[,] b = { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, 9 } };

        float r = Input.GetAxis("Cancel");

        if (!sp.IsOpen)
        {   
            sp.Open();
        }
        if (r != 0.0f)
        {
            Reset();
        }


        if (sp.IsOpen)
        {
            //print("SP is OPEN");
            //sp.Write("b");

            
            //UpdateOrb(10f,0f,0f);
            data = ParseData();
            UpdateOrb(data[3], data[4], data[5]);
            UpdatePos(data[0], data[1], data[2]);
            //print(data[3]+" "+ data[4]+" "+ data[5]);
            /*if (data[0]>0.2f) {
                vx = vx + data[0];
                x = x + vx + ((data[0] * data[0]) / 2);
            }
            if (data[2] > 0.2f){
                vz = vz + data[2];
                z = z + vz + ((data[2] * data[2]) / 2);
            }*/


        }

    }

    private void UpdatePos(float x, float y, float z) {
        float[] ag = {0,0,0};
        float[] ab = {x,y,z};
        Vector3 pos;

        ag = Matrix33x31Mul(orb,ab);
        print("Ab:"+ag[0] + "," + ag[1] + "," + ag[2]);
        print("Ag:" + ag[0]+","+ag[1]+","+ag[2]);
        v[0] = v[0] + (Time.deltaTime * ag[0])*9.81f;
        v[1] = v[1] + (Time.deltaTime * ag[1]) * 9.81f;
        v[2] = v[2] + (Time.deltaTime * (ag[2]-1)) * 9.81f;
        print("v:" + v[0] + "," + v[1] + "," + v[2]);

        p[0] = p[0] + (Time.deltaTime * v[0]);
        p[1] = p[1] + (Time.deltaTime * v[1]);
        p[2] = p[2] + (Time.deltaTime * v[2]);
        print("p:" + p[0] + "," + p[1] + "," + p[2]);
        pos = new Vector3(p[0], p[1], p[2]);
        //rb.position = pos;

    }

    private Quaternion dirCosToQuat(float[,]dC) {
        float q0, q1, q2, q3, d;
        Quaternion ret;
        q3 = 0.5f * Mathf.Sqrt(dC[0, 0] + dC[1, 1]+ dC[2, 2] + 1);
        q0 = (dC[1, 2] - dC[2, 1]) / (4 * q3);
        q1 = (dC[2, 0] - dC[0, 2]) / (4 * q3);
        q2 = (dC[0, 1] - dC[1, 0]) / (4 * q3);
        d = Mathf.Sqrt((q0*q0)+(q1*q1)+(q2*q2)+(q3*q3));
        q0 = (q0 / d);
        q1 = (q1 / d);
        q2 = (q2 / d);
        q3 = (q3 / d);
        ret = new Quaternion(q0, q1, q2, q3);
        return ret;
    }

    private float[] Matrix33x31Mul(float[,] a, float[] b) {
        float[] ret= {0,0,0};
        ret[0] = a[0, 0] * b[0] + a[0, 1] * b[1] + a[0, 2] * b[2];
        ret[1] = a[1, 0] * b[0] + a[1, 1] * b[1] + a[1, 2] * b[2];
        ret[2] = a[2, 0] * b[0] + a[2, 1] * b[1] + a[2, 2] * b[2];
        return ret;
    }

    private float[,] Matrix3x3Mul(float[,] a, float[,] b) {
        float[,] ret = {{0,0,0},{0,0,0},{0,0,0}};
        ret[0, 0] = a[0, 0] * b[0, 0] + a[0, 1] * b[1, 0] + a[0, 2] * b[2, 0];
        ret[0, 1] = a[0, 0] * b[0, 1] + a[0, 1] * b[1, 1] + a[0, 2] * b[2, 1];
        ret[0, 2] = a[0, 0] * b[0, 2] + a[0, 1] * b[1, 2] + a[0, 2] * b[2, 2];


        ret[1, 0] = a[1, 0] * b[0, 0] + a[1, 1] * b[1, 0] + a[1, 2] * b[2, 0];
        ret[1, 1] = a[1, 0] * b[0, 1] + a[1, 1] * b[1, 1] + a[1, 2] * b[2, 1];
        ret[1, 2] = a[1, 0] * b[0, 2] + a[1, 1] * b[1, 2] + a[1, 2] * b[2, 2];

        ret[2, 0] = a[2, 0] * b[0, 0] + a[2, 1] * b[1, 0] + a[2, 2] * b[2, 0];
        ret[2, 1] = a[2, 0] * b[0, 1] + a[2, 1] * b[1, 1] + a[2, 2] * b[2, 1];
        ret[2, 2] = a[2, 0] * b[0, 2] + a[2, 1] * b[1, 2] + a[2, 2] * b[2, 2];
        return ret;
    }

    private void UpdateOrb(float x, float y, float z){
        Quaternion q;
        x = x * Mathf.Deg2Rad * Time.deltaTime;
        y = y * Mathf.Deg2Rad * Time.deltaTime;
        z = z * Mathf.Deg2Rad * Time.deltaTime;
        float[,] b = { { 0, 0, 0 }, { 0, 0, 0 }, { 0, 0, 0 } };
        float[,] bSq = { { 0, 0, 0 }, { 0, 0, 0 }, { 0, 0, 0 } };
        float[,] I = {{1,0,0}, {0,1,0}, {0,0,1}};
        b[0, 1] = -z;
        b[0, 2] = y;
        b[1, 0] = z;
        b[1, 2] = -x;
        b[2, 0] = -y;
        b[2, 1] = x;
        bSq = Matrix3x3Mul(b, b);
        float sigma = Mathf.Sqrt((x * x) + (y * y) + (z * z));

        I[0, 0] =1+ ((Mathf.Sin(sigma)) / sigma) * b[0, 0] + ((1 - Mathf.Cos(sigma)) / (sigma * sigma)) * bSq[0, 0];
        I[0, 1] = ((Mathf.Sin(sigma)) / sigma) * b[0, 1] + ((1 - Mathf.Cos(sigma)) / (sigma * sigma)) * bSq[0, 1];
        I[0, 2] = ((Mathf.Sin(sigma)) / sigma) * b[0, 2] + ((1 - Mathf.Cos(sigma)) / (sigma * sigma)) * bSq[0, 2];

        I[1, 0] = ((Mathf.Sin(sigma)) / sigma) * b[1, 0] + ((1 - Mathf.Cos(sigma)) / (sigma * sigma)) * bSq[1, 0];
        I[1, 1] =1+ ((Mathf.Sin(sigma)) / sigma) * b[1, 1] + ((1 - Mathf.Cos(sigma)) / (sigma * sigma)) * bSq[1, 1];
        I[1, 2] = ((Mathf.Sin(sigma)) / sigma) * b[1, 2] + ((1 - Mathf.Cos(sigma)) / (sigma * sigma)) * bSq[1, 2];
    
        I[2, 0] = ((Mathf.Sin(sigma)) / sigma) * b[2, 0] + ((1 - Mathf.Cos(sigma)) / (sigma * sigma)) * bSq[2, 0];
        I[2, 1] = ((Mathf.Sin(sigma)) / sigma) * b[2, 1] + ((1 - Mathf.Cos(sigma)) / (sigma * sigma)) * bSq[2, 1];
        I[2, 2] =1+ ((Mathf.Sin(sigma)) / sigma) * b[2, 2] + ((1 - Mathf.Cos(sigma)) / (sigma * sigma)) * bSq[2, 2];
        

        orb = Matrix3x3Mul(orb,I);
        q = dirCosToQuat(orb);
        //print(q.x+" "+q.y+" "+q.z+" "+q.w);
        rb.rotation = q;
    }

    private void OnApplicationQuit()
    {
        sp.Close();
    }

    private float[] ParseData() {
        float[] data = {0, 0, 0, 0, 0, 0, 0, 0, 0}, intaccel= {0, 0, 0},intgyro = {0, 0, 0}, intmagno = {0, 0, 0};
        string rawdata="";
        string[] splitdata,accel,gyro,magno;
        if (sp.IsOpen)
        {
            sp.Write("b");
            rawdata = sp.ReadLine();
            splitdata = rawdata.Split(';');
            accel = splitdata[1].Split('/');
            gyro = splitdata[2].Split('/');
            magno = splitdata[3].Split('/');

            intaccel[0] = float.Parse(accel[0]);
            intaccel[1] = float.Parse(accel[1]);
            intaccel[2] = float.Parse(accel[2]);

            intaccel[0] = intaccel[0] / 16384;
            intaccel[1] = intaccel[1] / 16384;
            intaccel[2] = intaccel[2] / 16384;

             intgyro[0] = float.Parse(gyro[0]);
             intgyro[1] = float.Parse(gyro[1]);
             intgyro[2] = float.Parse(gyro[2]);

             intgyro[0] = intgyro[0] / 32.8f;
             intgyro[1] = intgyro[1] / 32.8f;
             intgyro[2] = intgyro[2] / 32.8f;

             intmagno[0] = float.Parse(magno[0]);
             intmagno[1] = float.Parse(magno[1]);
             intmagno[2] = float.Parse(magno[2]);

             data[0] = intaccel[0];
             data[1] = intaccel[1];
             data[2] = intaccel[2];
             data[3] = intgyro[0];
             data[4] = intgyro[1];
             data[5] = intgyro[2];
             data[6] = intmagno[0];
             data[7] = intmagno[1];
             data[8] = intmagno[2];
            
        }
        
        return data;
    }
}
