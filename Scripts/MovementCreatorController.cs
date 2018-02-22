using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementCreatorController : MonoBehaviour {

    public GameObject sword;
    private GameObject startSword;
    private GameObject endSword;

    private Queue<Transform> steadyChecker;


    // Use this for initialization
    void Start () {
        steadyChecker = new Queue<Transform>();

        /*
        
         --- create start location indicator ---
        startSword = Instantiate(sword);
        startSword.name = "startSword";
        startSword.AddComponent<GhostSwordController>().sword = startSword;
        
         --- create end location indicator ---
        endSword = Instantiate(sword);
        endSword.name = "endSword";
        endSword.AddComponent<GhostSwordController>().sword = endSword;
        
         */
    }


    // Update is called once per frame
    void Update () {

        bool stable = isStable();

        Debug.Log(stable);
        /*
        while (position is not stable) { 
            if(position is stable) {
                render beginning position
                indicate to user 
                break while
            }
         }
         while (position is not stable) {
            record position
            if(position is stable) {
                render end position
                indicate to user 
                break while
            }         
         }

        store inputs
        
        while (position is not stable) { 
            if(position is stable and in beginning position) {
                indicate to user 
                break while
            }
         }
         while (position is not stable) {
            record position
            if(position is stable and in end position) {
                indicate to user 
                break while
            }         
         }

         */

        Debug.Log(Application.targetFrameRate);
    }


    //Returns true if the sword object has remained mostly stable for the past 30 frames (0.5 seconds at 60fps), false otherwise
    bool isStable()
    {
        //Add the current sword location to the steadyChecker
        steadyChecker.Enqueue(sword.transform);

        //Return false if there aren't enough sample points yet
        if (steadyChecker.Count < Application.targetFrameRate / 2)
            return false;

        //Assumed true
        bool isStable = true;

        //remove excess positions if present
        if (steadyChecker.Count > Application.targetFrameRate / 2)
            steadyChecker.Dequeue();

        for (int i = 0; i < steadyChecker.Count - 1; i++)
        {
            for (int j = i + 1; j < steadyChecker.Count; j++)
            {
                if (Quaternion.Angle(steadyChecker.ToArray()[i].rotation, steadyChecker.ToArray()[j].rotation) > 10)
                {
                    isStable = false;
                    //The angle between some pair of the last 30 sword positions is greater than 10 degrees
                }
                if (Vector3.Distance(steadyChecker.ToArray()[i].position, steadyChecker.ToArray()[j].position) > 10)
                {
                    isStable = false;
                    //The distance between some pair of the last 30 sword positions is greater than 10 (units unknown)
                }
            }
        }
        return isStable;
    }
}
