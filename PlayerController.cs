using System;
using System.Text;
using System.Collections;
using System.IO.Ports;

using UnityEngine;

public class PlayerController : MonoBehaviour {
	private Rigidbody rb;
	Vector3 origin;
	void Start ()
	{	rb = GetComponent<Rigidbody>();
		origin = rb.position;
	}

	void reset(){
		rb.position=origin;
	}

	void FixedUpdate (){
		float r = Input.GetAxis ("Cancel");
		if(r!=0.0f){
			reset();
		}
		string[] ports = SerialPort.GetPortNames();
		for (int i=0;i<ports.Length;i++){
			print (ports[i]);
		}
	}


		
}