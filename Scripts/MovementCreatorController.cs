using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class MovementCreatorController : MonoBehaviour {

    public GameObject sword;
    public GameObject label;

    private GameObject startSword;
    private GameObject endSword;

    private Queue<Transform> steadyChecker;
    private Queue<Transform> recordingSampler;

    private struct Record
    {
        public Vector3 position;
        public Quaternion rotation;

        public Record(Vector3 v, Quaternion q)
        {
            position = v;
            rotation = q;
        }
    }

    private Queue<Record> officialRecording;

    private bool firstPassStarted;
    private bool firstPassFinished;

   private int steadyFrameCount;


    // Use this for initialization
    void Start () {
        steadyChecker = new Queue<Transform>();

        recordingSampler = new Queue<Transform>();
        officialRecording = new Queue<Record>();

        firstPassStarted = false;
        firstPassFinished = false;

        //Increase or decrease to set time before "steady"
        steadyFrameCount = Application.targetFrameRate * 4;

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


    //Some code from: https://support.unity3d.com/hc/en-us/articles/115000341143-How-do-I-read-and-write-data-from-a-text-file-
    private void persistMovement()
    {
        string path = "Assets/Resources/test.txt";

        //Write some text to the test.txt file
        StreamWriter writer = new StreamWriter(path, false);
        while(officialRecording.Count > 0)
        {
            Record r = officialRecording.Dequeue();
            writer.WriteLine(r.position.ToString() + r.rotation.ToString());
        }
        writer.Close();


        //This is just to prove it's working, not to remain in the persist call

        //Re-import the file to update the reference in the editor
        AssetDatabase.ImportAsset(path);
        TextAsset asset = (TextAsset) Resources.Load("test");

        //Print the text from the file
        Debug.Log(asset.text);
    }

    // Update is called once per frame
    void Update () {

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

        bool stable = isStable();

        if(!firstPassStarted)
        {
            if(stable)
            {
                firstPassStarted = true;

                Text txt = label.GetComponent<Text>();
                txt.text = "Recording";

                //Render start position
                startSword = Instantiate(sword);
                startSword.name = "startSword";
                startSword.AddComponent<GhostSwordController>().sword = startSword;
                
            }
        } else
        {
            if(!firstPassFinished)
            {
                if(!stable)
                {
                    //This is where first recording happens

                    recordingSampler.Enqueue(sword.transform);

                    if(recordingSampler.Count >= 30)
                    {
                        //Get average position of last 30 frames
                        Transform[] sampleArray = recordingSampler.ToArray();
                        Vector3[] forPositionAveraging = new Vector3[sampleArray.Length];
                        for (int i = 0; i < sampleArray.Length; i++)
                        {
                            forPositionAveraging[i] = sampleArray[i].position;
                        }
                        Vector3 truePosition = getMeanVector(forPositionAveraging);


                        //Get average rotation of last 30 frames
                        Quaternion[] forRotationAveraging = new Quaternion[sampleArray.Length];
                        for (int i = 0; i < sampleArray.Length; i++)
                        {
                            forRotationAveraging[i] = sampleArray[i].rotation;
                        }
                        Quaternion trueRotation = getMeanQuaternion(forRotationAveraging);
                        
                        while(recordingSampler.Count >= 15)
                        {
                            recordingSampler.Dequeue();
                        }

                        //Add the compiled position and rotation to the official recording log
                        officialRecording.Enqueue(new Record(truePosition, trueRotation));

                    } 
                

                } else
                {
                    firstPassFinished = true;

                    Text txt = label.GetComponent<Text>();
                    txt.text = "Movement Locked";


                    //Render end position
                    endSword = Instantiate(sword);
                    endSword.name = "endSword";
                    endSword.AddComponent<GhostSwordController>().sword = endSword;

                    persistMovement();

                }
            } else
            {
                //This is where the multiple passes will go

            }
        }
        
    }


    //From: https://answers.unity.com/questions/164257/find-the-average-of-10-vectors.html
    Vector3 getMeanVector(Vector3[] positions)
    {
        if (positions.Length == 0)
            return Vector3.zero;
        float x = 0f;
        float y = 0f;
        float z = 0f;
        foreach (Vector3 pos in positions)
        {
            x += pos.x;
            y += pos.y;
            z += pos.z;
        }
        return new Vector3(x / positions.Length, y / positions.Length, z / positions.Length);
    }

    //Returns true if the sword object has remained mostly stable for the past 30 frames (0.5 seconds at 60fps), will only return true up to once every (steadyFrameCount/framerate) seconds, false otherwise
    bool isStable()
    {
        //Add the current sword location to the steadyChecker
        steadyChecker.Enqueue(sword.transform);

        //Return false if there aren't enough sample points yet
        if (steadyChecker.Count < steadyFrameCount)
            return false;

        //Assumed true
        bool isStable = true;

        //remove excess positions if present
        while (steadyChecker.Count > steadyFrameCount)
        {
            steadyChecker.Dequeue();
        }

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

        if (isStable)
            steadyChecker.Clear();

        return isStable;
    }

    public Quaternion getMeanQuaternion(Quaternion[] inputs)
    {
        Vector4 accumulator = Vector4.zero;
        Quaternion first = inputs[0];
        Quaternion toReturn = Quaternion.identity;
        for(int i = 1; i < inputs.Length; i++)
        {
            toReturn = AverageQuaternion(ref accumulator, inputs[i], first, i);
        }
        return toReturn;
    }

    //From :http://wiki.unity3d.com/index.php/Averaging_Quaternions_and_Vectors

    //Get an average (mean) from more then two quaternions (with two, slerp would be used).
    //Note: this only works if all the quaternions are relatively close together.
    //Usage: 
    //-Cumulative is an external Vector4 which holds all the added x y z and w components.
    //-newRotation is the next rotation to be added to the average pool
    //-firstRotation is the first quaternion of the array to be averaged
    //-addAmount holds the total amount of quaternions which are currently added
    //This function returns the current average quaternion
    public static Quaternion AverageQuaternion(ref Vector4 cumulative, Quaternion newRotation, Quaternion firstRotation, int addAmount)
    {

        float w = 0.0f;
        float x = 0.0f;
        float y = 0.0f;
        float z = 0.0f;

        //Before we add the new rotation to the average (mean), we have to check whether the quaternion has to be inverted. Because
        //q and -q are the same rotation, but cannot be averaged, we have to make sure they are all the same.
        if (!Math3d.AreQuaternionsClose(newRotation, firstRotation))
        {

            newRotation = Math3d.InverseSignQuaternion(newRotation);
        }

        //Average the values
        float addDet = 1f / (float)addAmount;
        cumulative.w += newRotation.w;
        w = cumulative.w * addDet;
        cumulative.x += newRotation.x;
        x = cumulative.x * addDet;
        cumulative.y += newRotation.y;
        y = cumulative.y * addDet;
        cumulative.z += newRotation.z;
        z = cumulative.z * addDet;

        //note: if speed is an issue, you can skip the normalization step
        return NormalizeQuaternion(x, y, z, w);
    }

    public static Quaternion NormalizeQuaternion(float x, float y, float z, float w)
    {

        float lengthD = 1.0f / (w * w + x * x + y * y + z * z);
        w *= lengthD;
        x *= lengthD;
        y *= lengthD;
        z *= lengthD;

        return new Quaternion(x, y, z, w);
    }

    //Changes the sign of the quaternion components. This is not the same as the inverse.
    public static Quaternion InverseSignQuaternion(Quaternion q)
    {

        return new Quaternion(-q.x, -q.y, -q.z, -q.w);
    }

    //Returns true if the two input quaternions are close to each other. This can
    //be used to check whether or not one of two quaternions which are supposed to
    //be very similar but has its component signs reversed (q has the same rotation as
    //-q)
    public static bool AreQuaternionsClose(Quaternion q1, Quaternion q2)
    {

        float dot = Quaternion.Dot(q1, q2);

        if (dot < 0.0f)
        {

            return false;
        }

        else
        {

            return true;
        }
    }
}
