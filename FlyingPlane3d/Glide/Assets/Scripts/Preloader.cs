using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Preloader : MonoBehaviour 
{
	private CanvasGroup fadeGroup;
	private float loadTime;
	private float minimumLogoTime = 3.0f;  //minimum time of scene

	private void Start()
	{
		 //Grab the only CAnvas group in Scene
		fadeGroup = FindObjectOfType<CanvasGroup>();

		//Start with a White Screen;
		fadeGroup.alpha = 1; 

		//Preload the game

		//get a timestamp of completion time
		//if loadtime is super, give a buffer for apreciate the logo
		if (Time.time < minimumLogoTime)
			loadTime = minimumLogoTime;
		else
			loadTime = Time.time;
		}
	private void Update()
	{
		//Fade-in
		if (Time.time < minimumLogoTime) 
		{
			fadeGroup.alpha = 1 - Time.time;
		}
		//Fade-out
		if (Time.time > minimumLogoTime && loadTime != 0) 
		{
			fadeGroup.alpha = Time.time - minimumLogoTime;

			if (fadeGroup.alpha >= 1)
			{
				SceneManager.LoadScene ("Menu");
			}
		}
	}
}
