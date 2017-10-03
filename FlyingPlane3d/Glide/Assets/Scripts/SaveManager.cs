using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { set; get; }


    public SaveState state;


    private void Awake()
    {
        
        DontDestroyOnLoad(gameObject);
        Instance = this;
        Load();

        //are wun using accelerometer or can we use it..
        if(state.usingAccelerometer && !SystemInfo.supportsAccelerometer)
        {
            //if we cant't make sure we are not trying it next time.
            state.usingAccelerometer = false;
            Save();
        }



    }

    //save the whole state of This SaveState script to the player pref
    public void Save()
    {

        PlayerPrefs.SetString("save", Helper.Serialize<SaveState>(state));
    }
    //load the previous svaestate from the player prefs
    public void Load()
    {
        //do we alrealdy have a save?
        if (PlayerPrefs.HasKey("save"))
        {
            state = Helper.Deserialize<SaveState>(PlayerPrefs.GetString("save"));
        }
        else
        {
            state = new SaveState();
            Save();
            Debug.Log("No save file Found. Creating a New one...!");
        }
    }

    //check if the color is own
    public bool IsColorOwned(int index)
    {
        //check if the bit is set or ownder
        return (state.colorOwned & (1 << index)) != 0;

    }

    //check if the Trail is own
    public bool IsTrailOwned(int index)
    {
        //check if the bit is set or ownder
        return (state.trailOwned & (1 << index)) != 0;

    }

    //Attempt buying a color, return True/False
    public bool BuyColor(int index, int cost)
    {
        if(state.gold >= cost)
        {
            //enough money. remove from the gold stack
            state.gold -= cost;
            UnlockColor(index);

            //Save progress
            Save();

            return true;
        }
        else
        {
            //not enough money. Return false
            return false;
        }
    }
    //Attempt buying a trail, return True/False
    public bool BuyTrail(int index, int cost)
    {
        if (state.gold >= cost)
        {
            //enough money. remove from the gold stack
            state.gold -= cost;
            UnlockTrail(index);

            //Save progress
            Save();

            return true;
        }
        else
        {
            //not enough money. Return false
            return false;
        }
    }

    //unlock a color in the 'colorOwned' int
    public void UnlockColor(int index)
    {

        //toggole on the bit at index
        state.colorOwned |= 1 << index;
    }

    //unlock a trail in the 'trailOwned' int
    public void UnlockTrail(int index)
    {

        //toggole on the bit at index
        state.trailOwned |= 1 << index;
    }

    //Complete Level
    public void CompleteLevel(int index)
    {
        //if this is the current active level
        if(state.completedLevel == index)
        {
            state.completedLevel++;
            Save();
        }
    }

    //reset the whole file
    public void ResetSave()
    {
        PlayerPrefs.DeleteKey("save");

    }



}

