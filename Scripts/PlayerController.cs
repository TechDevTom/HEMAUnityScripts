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
    string textMAG,textACC,textGYRO,textMi,textMa;
    int trials = 0;
    float speed;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        origin = rb.position;
        sp = new SerialPort("\\\\.\\COM4", 9600);
        if (!sp.IsOpen)
        {
            sp.Open();
            sp.ReadTimeout = 500;
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

        float r = Input.GetAxis("Cancel");
        float[] lastRot = {0,0,0};
        float[] minData = {0,0,0,0,0,0,0,0,0};
        float[] maxData = { 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        if (!sp.IsOpen)
        {   trials++;
            sp.Open();
            print(trials);
        }
        if (r != 0.0f)
        {
            Reset();
        }


        if (sp.IsOpen)
        {
            //print("SP is OPEN");
            sp.Write("b");
            data = ProcData();
            for (int i=6;i<9;i+=1) {
                if (data[i] > maxData[i]) {
                    maxData[i] = data[i];
                }
                if (data[i]>minData[i]) {
                    minData[i] = data[i];
                }
            }
            for (int k=6;k<9;k+=1) {
                textMi+=(minData[k].ToString())+":";
            }
            for (int k = 6; k < 9; k += 1)
            {
                textMa += (maxData[k].ToString()) + ":";
            }
            textMAG = "MagDATA: " + data[6].ToString() + ":" + data[7].ToString() + ":" + data[8].ToString();
            textACC = "AccelDATA: " + data[0].ToString() + ":" + data[1].ToString() + ":" + data[2].ToString();
            textGYRO = "GyroDATA: "+data[3].ToString()+":"+data[4].ToString()+":"+data[5].ToString();
            /*transform.Rotate(
                data[6] - lastRot[0],
                data[7] - lastRot[1],
                data[8] - lastRot[2],
                Space.Self
                );*/
            lastRot[0] = data[6];
            lastRot[1] = data[7];
            lastRot[2] = data[8];

        }

    }

    private void OnGUI()
    {
        textMAG=GUI.TextField(new Rect(10, 10, 200, 20), textMAG);
        textACC = GUI.TextField(new Rect(200, 30, 400, 40), textACC);
        textGYRO = GUI.TextField(new Rect(400, 50, 600, 60), textGYRO);
    }


    private void OnApplicationQuit()
    {
        sp.Close();
    }
    private float[] ProcData() {
        float[] data = {0, 0, 0, 0, 0, 0, 0, 0, 0}, intaccel= {0, 0, 0},intgyro = {0, 0, 0}, intmagno = {0, 0, 0};
        string[] splitdata,accel,gyro,magno;
        if (sp.IsOpen)
        {
            sp.Write("b");
            string rawdata = sp.ReadLine();
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
