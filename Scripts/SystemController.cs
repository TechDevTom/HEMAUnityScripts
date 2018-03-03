using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;

public class SystemController : MonoBehaviour {

    public Text text;

    public GameObject notification;

    public void Awake()
    {
        Application.targetFrameRate = 60; //Cap the engine at 60 frames per second, to allow better use of Update()
    }

    public void LoadMovementLevel(string name)
    {
        PlayerPrefs.SetString("name", text.text);
        LoadLevel(name);
    }

    public void LoadLevel(string name)
    {
        Debug.Log("New Level load: " + name);
        SceneManager.LoadScene(name);
    }

    public static void StaticLoadLevel(string name)
    {
        Debug.Log("New Level load: " + name);
        SceneManager.LoadScene(name);
    }

    public void Update()
    {
        if (Input.GetKeyDown("escape"))
            LoadLevel("Start Menu");
    }

    public void QuitRequest()
    {
        Debug.Log("Quit requested");
        Application.Quit();
    }

    public void AttemptMovementLoad()
    {
        string fullPath = "Assets/Resources/" + text.text + ".txt";

        if (!File.Exists(fullPath))
        {
            Text txt = notification.GetComponent<Text>();
            txt.text = text.text + ".txt is not a valid HEMA movement file!";
        } else
        {
            PlayerPrefs.SetString("name", text.text);
            SceneManager.LoadScene("TrainingScenario");
        }
    }

}
