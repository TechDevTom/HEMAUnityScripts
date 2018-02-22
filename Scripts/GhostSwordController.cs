using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostSwordController : MonoBehaviour {

    public GameObject sword;

    private float timer;
    private bool inverter;

	// Use this for initialization
	void Start () {
        timer = 0;

    }
	
	// Update is called once per frame
	void Update () {
        //This will cause any object with a bound GhostSwordController to blink repeatedly
		if(inverter)
        {
            timer += 0.05f;
            if(timer > 1)
            {
                inverter = false;
                sword.GetComponent<Renderer>().enabled = true;
                Renderer[] renderers = GetComponentsInChildren<Renderer>();
                foreach (Renderer r in renderers)
                {
                    r.enabled = true;
                }
            }
        } else
        {
            timer -= 0.05f;
            if (timer < 0)
            {
                inverter = true;
                sword.GetComponent<Renderer>().enabled = false;
                Renderer[] renderers = GetComponentsInChildren<Renderer>();
                foreach (Renderer r in renderers)
                {
                    r.enabled = false;
                }
            }
        }
        
	}
}
