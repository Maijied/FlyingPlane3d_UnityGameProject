using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager : MonoBehaviour
{
    public static Manager Instance { set; get; }

    public Material playerMaterial;
    public Color[] PlayerColors = new Color[10];
    public GameObject[] PlayerTrails = new GameObject[10];

    public int currentLevel = 0;  //Used when changing from menu to game scene 
    public int menuFocus = 0;    //Used when entering the menu scene

    private Dictionary<int, Vector2> activeTouches = new Dictionary<int, Vector2>();



    private void Awake()
    {
        

        DontDestroyOnLoad(gameObject);
        Instance = this;
    }

    



    //About Accelerometer
    public Vector3 GetPlayerInput()
    {
         //are we using the accelerometer
         if(SaveManager.Instance.state.usingAccelerometer)
        {
            //if we can use it replace the y param by z, we don'r need that y
            Vector3 a = Input.acceleration;
            a.y = a.z;
            return a;
        } 

        //read all touches from the user
        Vector3 r = Vector3.zero;
        foreach(Touch touch in Input.touches)
        {
            //If we just started pressing on the screen
           if(touch.phase == TouchPhase.Began)
            {
                activeTouches.Add(touch.fingerId, touch.position);
            }
           //if we remove our finger
           else if(touch.phase == TouchPhase.Ended)
            {
                if(activeTouches.ContainsKey(touch.fingerId))
                activeTouches.Remove(touch.fingerId);
            }
           //Our finger  is either moving, or stationary, in both case, lets use the delta
           else
            {
                float mag = 0;
                r = (touch.position - activeTouches[touch.fingerId]);
                mag = r.magnitude / 10   ;
                r = r.normalized * mag;
               


            }
        }
        return r;
    }
}
