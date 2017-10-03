using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuScene : MonoBehaviour
{
    private CanvasGroup fadeGroup;
    private float fadeInSpeed = 033f;

    public RectTransform menuContainer;
    public Transform levelPanel;
    public Transform colorPanel;
    public Transform trailPanel;

    public Button tiltControlButton;
    public Color tiltControlEnabled;
    public Color tiltControlDisabled;

    public Text colorBuyText;
    public Text trailBuyText;
    public Text goldText;

    private MenuCamera menuCam;

    private int[] colorCost = new int[] { 0, 5, 20, 40, 60, 80, 100, 120, 150, 200 };
    private int[] trailCost = new int[] { 0, 20, 40, 40, 60, 60, 80, 80, 100, 100 };
    private int selectedColorIndex;
    private int selectedTrailIndex;
    private int activeColorIndex;
    private int activeTrailIndex;

    private Vector3 desiredMenuPosition;

    private GameObject currentTrail;

    public AnimationCurve enteringLevelZoomCurve;
    private bool isEnteringLevel = false;
    private float zoomDuration = 3.0f;
    private float zoomTransition;

    private Texture previousTrail;
    private GameObject lastPreviewObject;

    public Transform trailPreviewObject;
    public RenderTexture trailPreviewTexture;

	private void Start()
	{

        //$$ temporary
       //  SaveManager.Instance.state.gold = 9999;

        //Check if we have an accelerometer
        if(SystemInfo.supportsAccelerometer)
        {
            //is it currently enable?>
            tiltControlButton.GetComponent<Image>().color = (SaveManager.Instance.state.usingAccelerometer) ? tiltControlEnabled : tiltControlDisabled;

        }
        else
        {
            tiltControlButton.gameObject.SetActive(false);
        }

        // Find the only manu camera and assaign it
        menuCam = FindObjectOfType<MenuCamera>();


        //Position our camera to the focused menu
        SetCameraTo(Manager.Instance.menuFocus);

        //Tell our gold text , how much he should displaying
        UpdateGoldText();
		//Grab the only CAnvas group in Scene
		fadeGroup = FindObjectOfType<CanvasGroup>();

		//Start with a White Screen;
		fadeGroup.alpha = 1; 

		//add button on-click events to shop buttons
		InitShop();

        //add button on-click button events to levels
        InitLevel();

        //Set player preferences (color And trail)
        OnColorSelect(SaveManager.Instance.state.activeColor);
        SetColor(SaveManager.Instance.state.activeColor);

        OnTrailSelect(SaveManager.Instance.state.activeTrail);
        SetTrail(SaveManager.Instance.state.activeTrail);

        //Make the button bigger for selected item
        colorPanel.GetChild(SaveManager.Instance.state.activeColor).GetComponent<RectTransform>().localScale = Vector3.one *1.125f;
        trailPanel.GetChild(SaveManager.Instance.state.activeTrail).GetComponent<RectTransform>().localScale = Vector3.one * 1.125f;

        //Create the trail preview
        lastPreviewObject = GameObject.Instantiate(Manager.Instance.PlayerTrails[SaveManager.Instance.state.activeTrail]) as GameObject;
        lastPreviewObject.transform.SetParent(trailPreviewObject);
        lastPreviewObject.transform.localPosition = Vector3.zero;
    }
	private void Update()
	{
		//Fade in
		fadeGroup.alpha = 1 - Time.timeSinceLevelLoad * fadeInSpeed;

        //Menu navigation smoothly
        menuContainer.anchoredPosition3D = Vector3.Lerp(menuContainer.anchoredPosition3D, desiredMenuPosition, 0.1f);

        //entering Level Zoom
        if(isEnteringLevel)
        {
            //Add to the zoom transition float
            zoomTransition += (1 / zoomDuration) * Time.deltaTime;


            //change the scale, following the animation curve
            menuContainer.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 5, enteringLevelZoomCurve.Evaluate(zoomTransition));

            //Change the desire position off the canvas, So it can follow the scale up
            //this zoom into the center
            Vector3 newDesiredPosition = desiredMenuPosition * 5;

            //This adds to the specific position of the level on the canvas
            RectTransform rt = levelPanel.GetChild(Manager.Instance.currentLevel).GetComponent<RectTransform>();
            newDesiredPosition -= rt.anchoredPosition3D * 5;

            // This line will override the previous  position update
            menuContainer.anchoredPosition3D = Vector3.Lerp(desiredMenuPosition, newDesiredPosition, enteringLevelZoomCurve.Evaluate(zoomTransition));

            // Fade to white screen, this will overrride the first line  of this update
            fadeGroup.alpha = zoomTransition;

            // Are we done with the animation
            if(zoomTransition >= 1)
            {
                // Enter the level
                SceneManager.LoadScene("Game");
            }
        }
	}


	private void InitShop()
	{
		//We have assaigned the references
		if (colorPanel == null || trailPanel == null)
			Debug.Log ("you did not a  assaign the color/trail panel in the inspactor");

		//foe everry children transform under our color panel, find the button and add on-click
		int i = 0;
		foreach (Transform t in colorPanel) 
		{
			int currentIndex = i;
			Button b = t.GetComponent<Button>();
			b.onClick.AddListener (() => OnColorSelect(currentIndex));

            //Set the color based on if owned or not
            Image img = t.GetComponent<Image>();
            img.color = SaveManager.Instance.IsColorOwned(i) ? Manager.Instance.PlayerColors[currentIndex]
                : Color.Lerp(Manager.Instance.PlayerColors[currentIndex], new Color(0, 0, 0, 1), 0.25f);

           


			i++;
		}
		//reset index;
		i =  0;
		//do the same for the trail panel.
		foreach (Transform t in trailPanel) 
		{
			int currentIndex = i;
			Button b = t.GetComponent<Button>();
			b.onClick.AddListener (() => OnTrailSelect(currentIndex));

            //Set the trail based on if owned or not
           RawImage img = t.GetComponent<RawImage>();
            img.color = SaveManager.Instance.IsTrailOwned(i) ? Color.white : new Color(0.7f, 0.7f, 0.7f);


            i++;
		}

        //set the previous trail,to prevent bug when swaping later 
        previousTrail = trailPanel.GetChild(SaveManager.Instance.state.activeTrail).GetComponent<RawImage>().texture;
            
            }

    private void InitLevel()
    {
        //We have assaigned the references
        if (levelPanel == null)
            Debug.Log("you did not a  assaign the Level panel in the inspactor");

        //foe everry children transform under our Level panel, find the button and add on-click
        int i = 0;
        foreach (Transform t in levelPanel)
        {
            int currentIndex = i;
            Button b = t.GetComponent<Button>();
            b.onClick.AddListener(() => OnLevelSelect(currentIndex));

            Image img = t.GetComponent<Image>();

            // Is it unlocked?
            if(i <= SaveManager.Instance.state.completedLevel)
            {
                //it is unlocked
                if(i == SaveManager.Instance.state.completedLevel)
                {
                    //On the middle of this level
                    img.color = Color.white;
                }
                else
                {
                    //Level is already completed
                    img.color = Color.green;
                }
            }
            else
            {
                //Level isn't unlocked, Disable the button
                b.interactable = false;

                //Set to a Dark color
                img.color = Color.grey;
            }

            i++;
        }
    }

    private void SetCameraTo(int menuIndex)
    {
        NavigateTo(menuIndex);
        menuContainer.anchoredPosition3D = desiredMenuPosition;
    }

    private void NavigateTo(int menuIndex)
    {
        switch(menuIndex)
        {
            //0 or default case == main menu
            default:
            case 0:
                desiredMenuPosition = Vector3.zero;
                menuCam.BackToMainMenu();
                break;
            //1 == play menu
            case 1:
                desiredMenuPosition = Vector3.right * 1280;
                menuCam.MoveToLevel();
                break;
            //2 == Shop Menu
            case 2:
                desiredMenuPosition = Vector3.left * 1280;
                menuCam.MoveToShop();
                break;
        }
    }

    private void SetColor(int Index)
    {

        //set the active color index

        activeColorIndex = Index;
        SaveManager.Instance.state.activeColor = Index;

        //Change the color on player model
        Manager.Instance.playerMaterial.color = Manager.Instance.PlayerColors[Index];

        //Change buy button text
        colorBuyText.text = "Current";

        //Remember preferences
        SaveManager.Instance.Save();
    }
    private void SetTrail(int Index)
    {
        //set the active trail index

        activeTrailIndex = Index;
        SaveManager.Instance.state.activeTrail = Index;


        //Change the trrail on player model
        if (currentTrail != null)
            Destroy(currentTrail);

        //Create the new Trail
        currentTrail = Instantiate(Manager.Instance.PlayerTrails[Index]) as GameObject;

        //Set it as a children of the player
        currentTrail.transform.SetParent(FindObjectOfType<MenuPlayer>().transform);

        //Fix the wired Scalling and rotation issues
        currentTrail.transform.localPosition = Vector3.zero;
        currentTrail.transform.localRotation = Quaternion.Euler(0, 0, 90);
        currentTrail.transform.localScale = Vector3.one * 0.01f;


        //Change buy button text
        trailBuyText.text = "Current";

        //Remember preferences
        SaveManager.Instance.Save(); 
    }

    private void UpdateGoldText()
    {

        goldText.text = SaveManager.Instance.state.gold.ToString(); 
    }


    //Buttons

    public void OnPlayClick()
		{
        NavigateTo(1);
			Debug.Log("Play Button Has Been Clicked");
		}

	public void OnShopClick()
	    {
        NavigateTo(2);
			Debug.Log("Shop Button Has Been Clicked");
	    }

    public void OnBackClick()
    {
        NavigateTo(0);
        Debug.Log("Back Button Has Been Clicked");
    }

   

    private void OnColorSelect(int currentIndex)
    {
        Debug.Log("Selecting color Button :" + currentIndex);

        //if the clicked button is already selected
        if (selectedColorIndex == currentIndex)
            return;

        //if it's not selected, make the icon bigger
        colorPanel.GetChild(currentIndex).GetComponent<RectTransform>().localScale = Vector3.one * 1.125f;

        //put the previous one on normal scale
        colorPanel.GetChild(selectedColorIndex).GetComponent<RectTransform>().localScale = Vector3.one;

        //Set the selected color
        selectedColorIndex = currentIndex;

        //Change the content of buy button.depending on the state of the color.
        if(SaveManager.Instance.IsColorOwned(currentIndex))
        {
            //Color is owned.
            //is it already our current color
            if(activeColorIndex == currentIndex)
            {
                colorBuyText.text = "Current";
            }
            else
            {
                colorBuyText.text = "Select";
            }
           
        }
        else
        {
            //color isn't owned.
            colorBuyText.text = "Buy: " + colorCost[currentIndex].ToString();
        }

    }
    private void OnTrailSelect(int currentIndex)
    {
        Debug.Log("Selecting trail Button :" + currentIndex);

        //if the clicked button is already selected
        if (selectedTrailIndex == currentIndex)
            return;

        //Preview Trail
        //Get the image of the preview button
        trailPanel.GetChild(selectedTrailIndex).GetComponent<RawImage>().texture = previousTrail;

        //Keep the new trail's previous image in the previous trail
        previousTrail = trailPanel.GetChild(currentIndex).GetComponent<RawImage>().texture;

        //set the new trail preview image to yhe new camera
        trailPanel.GetChild(currentIndex).GetComponent<RawImage>().texture = trailPreviewTexture;

        //Change thep hysical object of trail preview
        if(lastPreviewObject != null)
        Destroy(lastPreviewObject);
        lastPreviewObject = GameObject.Instantiate(Manager.Instance.PlayerTrails[currentIndex]) as GameObject;
        lastPreviewObject.transform.SetParent(trailPreviewObject);
        lastPreviewObject.transform.localPosition = Vector3.zero;


        //if it's not selected, make the icon bigger
        trailPanel.GetChild(currentIndex).GetComponent<RectTransform>().localScale = Vector3.one * 1.125f;

        //put the previous one on normal scale
        trailPanel.GetChild(selectedTrailIndex).GetComponent<RectTransform>().localScale = Vector3.one;

        //Set the selected trail
        selectedTrailIndex = currentIndex;

        //Change the content of buy button.depending on the state of the trail.
        if (SaveManager.Instance.IsTrailOwned(currentIndex))
        {
            //trail is owned.
            //is it already our current color
            if (activeTrailIndex == currentIndex)
            {
                trailBuyText.text = "Current";
            }
            else
            {
                trailBuyText.text = "Select";
            }  
        }
        else
        {
            //trail isn't owned.
            

            trailBuyText.text = "Buy: " + trailCost[currentIndex].ToString();
        }
    }


    private void OnLevelSelect(int currentIndex)
    {
        Manager.Instance.currentLevel = currentIndex;
        isEnteringLevel = true;

        Debug.Log("Selecting level : " + currentIndex);
    } 


    public void OnColorBuy()
    {
        Debug.Log("Buy Color");
        //is the selected color own/
        if(SaveManager.Instance.IsColorOwned(selectedColorIndex))
        {
            //set the color
            SetColor(selectedColorIndex);
        }
        else
        {
            //Attempt to Buy the color
            if(SaveManager.Instance.BuyColor(selectedColorIndex,colorCost[selectedColorIndex]))
            {
                //Success..!!
                SetColor(selectedColorIndex);

                //Change the color of the button
                colorPanel.GetChild(selectedColorIndex).GetComponent<Image>().color = Manager.Instance.PlayerColors[selectedColorIndex];

                //update gold text
                UpdateGoldText();
            }
            else
            {
                //we do not have enough gold...!!
                //Play sound feedback.
                Debug.Log("Not Enough gold");
            }
        }
    }

    public void OnTrailBuy()
    {
        Debug.Log("Buy trail");

        //is the selected trail own/
        if (SaveManager.Instance.IsTrailOwned(selectedTrailIndex))
        {
            //set the trail
            SetTrail(selectedTrailIndex);
        }
        else
        {
            //Attempt to Buy the trail
            if (SaveManager.Instance.BuyTrail(selectedTrailIndex,trailCost[selectedTrailIndex]))
            {
                //Success..!!
                SetTrail(selectedTrailIndex);

                //Change the color of the button
                trailPanel.GetChild(selectedTrailIndex).GetComponent<RawImage>().color = Color.white;

                //update gold text
                UpdateGoldText();
            }
            else
            {
                //we do not have enough gold...!!
                //Play sound feedback.
                Debug.Log("Not Enough gold");
            }
        }
    }

    public void onTiltControl()
    {
        //Toggole the accelorometer bool
        SaveManager.Instance.state.usingAccelerometer = !SaveManager.Instance.state.usingAccelerometer;

        //Make sure we have the player's preferences
        SaveManager.Instance.Save();

        //Changen the display of tilt control button
        tiltControlButton.GetComponent<Image>().color = (SaveManager.Instance.state.usingAccelerometer) ? tiltControlEnabled : tiltControlDisabled;
    }
}


