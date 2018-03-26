using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.UI;
using UnityEngine;

public class MovementReader : MonoBehaviour {

    private class Movement
    {
        public Queue<MovementCreatorController.Record> movement;
        public MovementCreatorController.Record start;
        public MovementCreatorController.Record end;
        public float binSize;
        public Movement(Queue<MovementCreatorController.Record> m, MovementCreatorController.Record s, MovementCreatorController.Record e, float b)
        {
            movement = m;
            start = s;
            end = e;
            binSize = b;
        }
    }

    private Movement movement;

    private bool started;
    private bool finished;

    private bool hasMovement;
    private MovementCreatorController.Record currentRecord;
    private Queue<MovementCreatorController.Record> records;

    public GameObject sword;
    private GameObject indicatorSword;
    private GameObject checkSword;
    public GameObject label;

    private Queue<Transform> steadyChecker;
    private float startTime;

    private float finalGrade;
    private int totalMovements;

    private int steadyFrameCount;
    private int steadyDegreeVariation;
    private int steadyPositionVariation;


    // Use this for initialization
    void Start () {
        movement = createMovementFromFile(PlayerPrefs.GetString("name"));
        records = movement.movement;
        totalMovements = records.Count;
        started = false;
        finished = false;
        finalGrade = 0;
        steadyChecker = new Queue<Transform>();

        checkSword = new GameObject();

        //Render start position
        indicatorSword = Instantiate(sword);
        indicatorSword.name = "indicatorSword";
        indicatorSword.AddComponent<GhostSwordController>().sword = indicatorSword;
        indicatorSword.transform.position = movement.start.position;
        indicatorSword.transform.rotation = movement.start.rotation;



        //-- Here lie the magic numbers --//

        //Frames before potentially "steady"
        steadyFrameCount = Application.targetFrameRate * 2;

        //Acceptable variation in degrees before "steady"
        steadyDegreeVariation = 10;

        //Acceptable variation in position before "steady"
        steadyPositionVariation = 10;

        //Cap the engine at 60 frames per second, to allow better use of Update()
        Application.targetFrameRate = 60;
    }
	
	// Update is called once per frame
	void Update ()
    {

        if (Input.GetKeyDown("escape"))
            SystemController.StaticLoadLevel("Start Menu");


        bool stable = isStable();

        if (!started)
        {
            //wait till steady in start pos
            if (stable && areTransformsSimilar(indicatorSword.transform, sword.transform))
            {
                //in start position
                started = true;
                startTime = Time.time;

                Text txt = label.GetComponent<Text>();
                txt.text = "Go!";

                //move indicator sword to end position
                indicatorSword.transform.position = movement.end.position;
                indicatorSword.transform.rotation = movement.end.rotation;
            }
            
        } else
        {
            if (!finished)
            {
                //has started movement

                //get next record if needed, finish if no more records
                if (currentRecord == null)
                {
                    if (records.Count == 0)
                    {
                        finished = true;
                        Text txt = label.GetComponent<Text>();
                        txt.text = "Good Job!\n Accuracy : " + getLetterGrade(finalGrade / totalMovements);
                        return;
                    }
                    else
                    {
                        currentRecord = records.Dequeue();
                    }
                }


                float t = Time.time;
                //check for valid time range for a given record
                if ((t - startTime) <= currentRecord.time + (movement.binSize / 2) && (t - startTime) >= currentRecord.time - (movement.binSize / 2))
                {
                    //at valid time for pos rot check
                    checkSword.transform.position = currentRecord.position;
                    checkSword.transform.rotation = currentRecord.rotation;

                    if (areTransformsSimilar(checkSword.transform, sword.transform))
                    {
                        //valid check within time interval, so remove the current transform
                        currentRecord = null;
                        finalGrade += getAccuracy(checkSword.transform, sword.transform);
                    }
                }
                else if ((t - startTime) > currentRecord.time + (movement.binSize / 2))
                {
                    //failed movement check

                    started = false;

                    //reset movement checking queue
                    records = movement.movement;

                    Text txt = label.GetComponent<Text>();
                    txt.text = "Enter Start Position...";

                    indicatorSword.transform.position = movement.start.position;
                    indicatorSword.transform.rotation = movement.start.rotation;
                    
                }
            }
            

        }

	}

    string getLetterGrade(float grade)
    {
        if(grade < 0.5)
        {
            return "F";
        } else if (grade < 0.5 + (1/24))
        {
            return "D-";
        }
        else if (grade < 0.5 + (2 / 24))
        {
            return "D";
        }
        else if (grade < 0.5 + (3 / 24))
        {
            return "D+";
        }
        else if (grade < 0.5 + (4 / 24))
        {
            return "C-";
        }
        else if (grade < 0.5 + (5 / 24))
        {
            return "C";
        }
        else if (grade < 0.5 + (6 / 24))
        {
            return "C+";
        }
        else if (grade < 0.5 + (7 / 24))
        {
            return "B-";
        }
        else if (grade < 0.5 + (8 / 24))
        {
            return "B";
        }
        else if (grade < 0.5 + (9 / 24))
        {
            return "B+";
        }
        else if (grade < 0.5 + (10 / 24))
        {
            return "A-";
        }
        else if (grade < 0.5 + (11 / 24))
        {
            return "A";
        }
        return "A+";
    }

    float getAccuracy(Transform t1, Transform t2)
    {
        float ret = 0;
        ret += 1-(Quaternion.Angle(t1.rotation, t2.rotation) / steadyDegreeVariation);
        ret += 1-(Vector3.Distance(t1.position, t2.position) / steadyPositionVariation);
        return ret / 2;
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


    Movement createMovementFromFile(string filename)
    {
        
        string fullPath = "Assets/Resources/" + filename + ".txt";

        if (!File.Exists(fullPath))
        {
            throw new Exception();
        }

        //Write some text to file
        StreamReader reader = new StreamReader(fullPath);

        string input = reader.ReadLine();
        string[] inputSplit = input.Split(',');
        float binSize = float.Parse(inputSplit[0]);


        string[] parseObject;
        Vector3 vec;
        Quaternion quat;


        input = reader.ReadLine();
        inputSplit = input.Split('|');

        //create vector from input line
        parseObject = inputSplit[0].Substring(1, inputSplit[0].Length - 2).Split(',');
        vec = new Vector3(float.Parse(parseObject[0]), float.Parse(parseObject[1]), float.Parse(parseObject[2]));

        //create quaternion from input line
        parseObject = inputSplit[1].Substring(1, inputSplit[1].Length - 2).Split(',');
        quat = new Quaternion(float.Parse(parseObject[0]), float.Parse(parseObject[1]), float.Parse(parseObject[2]), float.Parse(parseObject[3]));

        MovementCreatorController.Record startPos = new MovementCreatorController.Record(vec, quat, 0);


        input = reader.ReadLine();
        inputSplit = input.Split('|');

        //create vector from input line
        parseObject = inputSplit[0].Substring(1, inputSplit[0].Length - 2).Split(',');
        vec = new Vector3(float.Parse(parseObject[0]), float.Parse(parseObject[1]), float.Parse(parseObject[2]));

        //create quaternion from input line
        parseObject = inputSplit[1].Substring(1, inputSplit[1].Length - 2).Split(',');
        quat = new Quaternion(float.Parse(parseObject[0]), float.Parse(parseObject[1]), float.Parse(parseObject[2]), float.Parse(parseObject[3]));

        MovementCreatorController.Record endPos = new MovementCreatorController.Record(vec, quat, 0);

        Queue<MovementCreatorController.Record> movementObject = new Queue<MovementCreatorController.Record>();

        while (reader.Peek() != -1)
        {
            input = reader.ReadLine();
            inputSplit = input.Split('|');
            
            parseObject = inputSplit[0].Substring(1, inputSplit[0].Length - 2).Split(',');


            //create vector from input line
            vec = new Vector3(float.Parse(parseObject[0]), float.Parse(parseObject[1]), float.Parse(parseObject[2]));

            parseObject = inputSplit[1].Substring(1, inputSplit[1].Length - 2).Split(',');

            //create quaternion from input line
            quat = new Quaternion(float.Parse(parseObject[0]), float.Parse(parseObject[1]), float.Parse(parseObject[2]), float.Parse(parseObject[3]));
            movementObject.Enqueue(new MovementCreatorController.Record(vec, quat, float.Parse(inputSplit[2])));

        }

        reader.Close();

        return new Movement(movementObject, startPos, endPos, binSize);
    }

}
