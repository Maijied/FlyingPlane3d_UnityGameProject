using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMotor : MonoBehaviour
{
    private CharacterController controller;
    private float baseSpeed = 10.0f;
    private float rotSpeedX = 3.0f;
    private float rotSpeedY = 1.5f;

    private float deathTime;
    private float deathDuration = 2;

    public GameObject deathExplosion;

	
	void Start ()
    {
        controller = GetComponent<CharacterController>();

        //Create the trail
        GameObject trail = Instantiate(Manager.Instance.PlayerTrails[SaveManager.Instance.state.activeTrail]);

        //Set the trail as a children of the model
        trail.transform.SetParent(transform.GetChild(0));

        //Fix thr rotation of the trail
        trail.transform.localEulerAngles = Vector3.forward * -90.0f;
	}
	
	
	void Update ()
    {
        //if the player is death (has a death time basically)
        if(deathTime !=0)
        {
            //wait x time and restart the level
            if(Time.time - deathTime > deathDuration)
            {
                SceneManager.LoadScene("Game");
            }
            return;
        }

        //Give the player forward velocity
        Vector3 moveVector = transform.forward * baseSpeed;

        //Gather player input
        Vector3 inputs = Manager.Instance.GetPlayerInput();

        //Het the delta direction
        Vector3 yaw = inputs.x * transform.right * rotSpeedX * Time.deltaTime;
        Vector3 pitch = inputs.y * transform.up * rotSpeedY * Time.deltaTime;
        Vector3 dir = yaw + pitch;

        //make sure we limit the player from doing a loop
        float maxX = Quaternion.LookRotation(moveVector + dir).eulerAngles.x;


        //If the plane is going torr far up/down, and the direction to the move vector
        if(maxX<90 && maxX>70 || maxX>270 && maxX<290)
        {
            //Too far don't do anything

        }
        else
        {
            //Add the direction to the current move
            moveVector += dir;

            //Have the player face where is he going
            transform.rotation = Quaternion.LookRotation(moveVector);
        }
        //move him
        controller.Move(moveVector * Time.deltaTime);
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        //Set a death timestamp
        deathTime = Time.time;

        //Player explosion Effect
        GameObject go = Instantiate(deathExplosion) as GameObject;
        go.transform.position = transform.position;

        //Hide the player mesh
        transform.GetChild(0).gameObject.SetActive(false);
    }
}
