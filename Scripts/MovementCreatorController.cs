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

    private string movementName;
    private int movementCount;

    private Queue<Transform> steadyChecker;
    private Queue<Transform> recordingSampler;
    private float recordStartTime;

    public class Record
    {
        public Vector3 position;
        public Quaternion rotation;
        public float time;

        public Record(Vector3 v, Quaternion q, float t)
        {
            position = v;
            rotation = q;
            time = t;
        }
    }

    private Queue<Record> officialRecording;
    private Queue<Queue<Record>> officialRecordings;

    private bool firstPassStarted;
    private bool firstPassFinished;
    private bool extraPassStarted;
    private bool extraPassFinished;

    private int steadyFrameCount;
    private int steadyDegreeVariation;
    private int steadyPositionVariation;
    private int recordingSampleKeepCount;
    private int recordingSampleDiscardCount;
    

    public void Awake()
    {
        steadyChecker = new Queue<Transform>();

        recordingSampler = new Queue<Transform>();
        officialRecording = new Queue<Record>();
        officialRecordings = new Queue<Queue<Record>>();

        if (string.IsNullOrEmpty(PlayerPrefs.GetString("name")))
            movementName = "noInputName";
        else
            movementName = PlayerPrefs.GetString("name");

        movementCount = 0;

        firstPassStarted = false;
        firstPassFinished = false;
        extraPassStarted = false;
        extraPassFinished = false;

        //-- Here lie the magic numbers --//

        //Frames before potentially "steady"
        steadyFrameCount = Application.targetFrameRate * 2;

        //Acceptable variation in degrees before "steady"
        steadyDegreeVariation = 10;

        //Acceptable variation in position before "steady"
        steadyPositionVariation = 10;

        //Trailing position tracker count for averaging sword position
        recordingSampleKeepCount = 30;

        //Number of trailing position tracking points to discard upon successful averaging
        recordingSampleDiscardCount = 10;

        //Cap the engine at 60 frames per second, to allow better use of Update()
        Application.targetFrameRate = 60; 
    }

     // Update is called once per frame
    void Update () {

        if (Input.GetKeyDown("escape"))
            completeCreationAndExit();


        bool stable = isStable();
        
        if(!firstPassStarted)
        {
            if(stable)
            {
                firstPassStarted = true;
                recordStartTime = Time.time;

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
                //This is where first recording happens
                if (!stable)
                {

                    recordingSampler.Enqueue(sword.transform);

                    if(recordingSampler.Count >= recordingSampleKeepCount)
                    {
                        processMovementToRecord();
                    } 
                

                } else
                {
                    recordingSampler.Clear();
                    firstPassFinished = true;
                    persistMovement();

                    Text txt = label.GetComponent<Text>();
                    txt.text = "Movement Locked";
                    
                    //Render end position
                    endSword = Instantiate(sword);
                    endSword.name = "endSword";
                    endSword.AddComponent<GhostSwordController>().sword = endSword;
                    

                }
            } else
            {

                if (!extraPassStarted)
                {
                    //extra pass hasn't started, wait for starting position

                    if(stable && isAtStartPosition(sword.transform))
                    {
                        extraPassStarted = true;
                        recordStartTime = Time.time;

                        Text txt = label.GetComponent<Text>();
                        txt.text = "Movement Locked: Recording Additional Input #" + movementCount;
                    }
                } else
                {
                    //extra pass in progress, record inputs
                    //if in end position, flip extraPassFinished and tell user

                    if (!extraPassFinished)
                    {
                        if(!stable)
                        {

                            recordingSampler.Enqueue(sword.transform);

                            if (recordingSampler.Count >= recordingSampleKeepCount)
                            {
                                processMovementToRecord();
                            }

                        } else
                        { 
                            if (isAtEndPosition(sword.transform))
                            {
                                recordingSampler.Clear();
                                extraPassFinished = true;

                                Text txt = label.GetComponent<Text>();
                                txt.text = "Movement Locked: Additional Input #" + movementCount + " Recorded";
                                persistMovement();
                            }
                        }

                    } else
                    {
                        //extra passs finished, reset
                        extraPassStarted = false;
                        extraPassFinished = false;
                    }
                }
            }
        }
        
    }

    //Some code from: https://support.unity3d.com/hc/en-us/articles/115000341143-How-do-I-read-and-write-data-from-a-text-file-
    private void persistMovement()
    {
        Queue<Record> duplicateRecording = new Queue<Record>();
        foreach (Record r in officialRecording)
        {
            duplicateRecording.Enqueue(r);
        }
        officialRecordings.Enqueue(duplicateRecording);
        officialRecording.Clear();
        movementCount++;
    }

    private void completeCreationAndExit()
    {
        float averageTime = 0;
        int sampleCount = 0;

        //Get average time across all recordings
        foreach (Queue<Record> q in officialRecordings)
        {
            averageTime += q.ToArray()[q.Count - 1].time;
            sampleCount++;
        }
        averageTime = averageTime / sampleCount;

        //Standardize all recordings to the sample time
        float sampleTime;
        foreach (Queue<Record> q in officialRecordings)
        {
            sampleTime = q.ToArray()[q.Count - 1].time;
            foreach (Record r in q)
            {
                r.time *= (averageTime / sampleTime);
            }
        }

        //Get average number of samples
        float averageSamples = 0;
        foreach (Queue<Record> q in officialRecordings)
        {
            averageSamples += q.Count;
        }
        averageSamples = averageSamples / sampleCount;

        int binCount = (int)System.Math.Floor(averageSamples);
        float binSize = averageTime / binCount;

        int currentBin = 0;
        List<Record> toAverage = new List<Record>();
        Queue<Record> completedRecords = new Queue<Record>();

        while (currentBin * binSize < averageTime)
        {
            foreach (Queue<Record> q in officialRecordings)
            {
                foreach (Record r in q)
                {
                    //If the current record lies within the current bin
                    float currentBinMin = currentBin * binSize;
                    if (r.time >= currentBinMin && r.time < (currentBinMin + binSize))
                    {
                        toAverage.Add(r);
                    }

                }
            }
            //First bin (t = 0 -> binSize) always comes up empty, but this is without moving test data, this conditional may not be necessary with the proper data
            if (toAverage.Count != 0)
            {
                completedRecords.Enqueue(averageRecord(toAverage.ToArray()));
            }
            currentBin++;
            toAverage.Clear();
        }



        string path = "Assets/Resources/";
        string fullPath = "Assets/Resources/" + movementName + ".txt";

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }


        StreamWriter writer = new StreamWriter(fullPath, false);
        writer.WriteLine(binSize + "," + currentBin);

        //Write start postion to file
        writer.WriteLine(startSword.transform.position.ToString() + "|" + startSword.transform.rotation.ToString());

        //Write end postion to file
        writer.WriteLine(endSword.transform.position.ToString() + "|" + endSword.transform.rotation.ToString());

        //Write recorded checkpoints to file
        while (completedRecords.Count > 0)
        {
            Record r = completedRecords.Dequeue();
            writer.WriteLine(r.position.ToString() + "|" + r.rotation.ToString() + "|" + r.time);
        }
        writer.Close();

        SystemController.StaticLoadLevel("Start Menu");
    }

    //Return the average record of all the passed in records
    Record averageRecord(Record[] records)
    {

        Vector3[] forPositionAveraging = new Vector3[records.Length];
        for (int i = 0; i < records.Length; i++)
        {
            forPositionAveraging[i] = records[i].position;
        }
        Vector3 truePosition = getMeanVector(forPositionAveraging);
        
        Quaternion[] forRotationAveraging = new Quaternion[records.Length];
        for (int i = 0; i < records.Length; i++)
        {
            forRotationAveraging[i] = records[i].rotation;
        }
        Quaternion trueRotation = getMeanQuaternion(forRotationAveraging);

        float averageTime = 0;
        int count = 0;
        for (int i = 0; i < records.Length; i++)
        {
            averageTime += records[i].time;
            count++;
        }

        return new Record(truePosition, trueRotation, (averageTime/count));
    }

    //Creates a record based on the last few frames of movement
    void processMovementToRecord()
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

        while (recordingSampler.Count >= recordingSampleDiscardCount)
        {
            recordingSampler.Dequeue();
        }

        //Add the compiled position and rotation to the official recording log
        officialRecording.Enqueue(new Record(truePosition, trueRotation, (Time.time - recordStartTime)));

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

    bool isAtStartPosition(Transform currentPos)
    {
        bool atStart = true;
        atStart = areTransformsSimilar(startSword.transform, currentPos);
        return atStart;
    }

    bool isAtEndPosition(Transform currentPos)
    {
        bool atStart = true;
        atStart = areTransformsSimilar(endSword.transform, currentPos);
        return atStart;
    }

    bool areTransformsSimilar(Transform t1, Transform t2)
    {
        bool similar = true;


        if (Quaternion.Angle(t1.rotation, t2.rotation) > steadyDegreeVariation)
        {
            //The angle between some pair of the last steadyFrameCount sword positions is greater than steadyDegreeVariation degrees
            similar = false;
        }
        if (Vector3.Distance(t1.position, t2.position) > steadyPositionVariation)
        {
            //The distance between some pair of the last steadyFrameCount sword positions is greater than steadyPositionVariation (units unknown)
            similar = false;
        }

        return similar;

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
                isStable = areTransformsSimilar(steadyChecker.ToArray()[i], steadyChecker.ToArray()[j]);
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
