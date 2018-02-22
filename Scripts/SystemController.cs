using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SystemController : MonoBehaviour {

    public void Awake()
    {
        Application.targetFrameRate = 60; //Cap the engine at 60 frames per second, to allow better use of Update()
    }

    public void LoadLevel(string name)
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

}
