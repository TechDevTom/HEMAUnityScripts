using System;
using System.Text;
using System.Collections;
using System.IO.Ports;

using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Rigidbody rb;
    Vector3 origin,newPos;
    SerialPort sp;
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
        float[] data;
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
            data = procData();
            print(data.ToString());
        }

    }
    private void OnApplicationQuit()
    {
        sp.Close();
    }
    private float[] procData() {
        float[] data = { 0, 0, 0 ,0,0,0,0,0,0}, intaccel= {0,0,0},intgyro = { 0, 0, 0 }, intmagno = { 0, 0, 0 };
        string[] splitdata,accel,gyro,magno;
        sp.Write("B");
        string rawdata = sp.ReadLine();
        if (rawdata[0] == 'B') {
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