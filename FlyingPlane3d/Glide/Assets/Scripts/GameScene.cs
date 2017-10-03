using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameScene : MonoBehaviour
{
    private CanvasGroup fadeGroup;
    private float fadeInDuration = 2;
    private bool gameStarted;

    public Transform arrow;
    private Transform playerTransform;
    public Objective objective;

    private void Start()
    {
        //Let's find the player
        playerTransform = FindObjectOfType<PlayerMotor>().transform;

        //Load up the level
        SceneManager.LoadScene(Manager.Instance.currentLevel.ToString(), LoadSceneMode.Additive); 
        //get the only canvas group in the scene
        fadeGroup = FindObjectOfType<CanvasGroup>();

        //set the fade to full opacity
        fadeGroup.alpha = 1;
    }

    private void Update()
    {
        if(objective != null)
        {
            //If we have an objective

            //Rotate the Arrow
            Vector3 dir = playerTransform.InverseTransformPoint(objective.GetCurrentRing().position);
            float a = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
            a += 180;

            arrow.transform.localEulerAngles = new Vector3(0, 180, a);
        }
        if(Time.timeSinceLevelLoad <= fadeInDuration)
        {
            //initial fade in
            fadeGroup.alpha = 1 - (Time.timeSinceLevelLoad / fadeInDuration);
        }
        //if the fade in is completed , The game has not been started yet
        else if(!gameStarted)
        {
            //Ensure that the fade is completly gone
            fadeGroup.alpha = 0;
            gameStarted = true;
        }

    }

    public void CompleteLevel()
    {
        //Complete the level and save the progress
        SaveManager.Instance.CompleteLevel(Manager.Instance.currentLevel);

        //Focus the level selection when we return to the menu scene
        Manager.Instance.menuFocus = 1;

        ExitScene();
    }

    public void ExitScene()
    {
        SceneManager.LoadScene("Menu");
    }
}
