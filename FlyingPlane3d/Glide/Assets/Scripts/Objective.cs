using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Objective : MonoBehaviour
{
    private List<Transform> rings = new List<Transform>();

    public Material activeRing;
    public Material inactiveRing;
    public Material finalRing;

    private int ringPassed = 0;

    private void Start()
    {
        //Set the objective field in game scene
        FindObjectOfType<GameScene>().objective = this;

        //At the started off the game, Assaign inactive to all rings
        foreach(Transform t in transform)
        {
            rings.Add(t);
            t.GetComponent<MeshRenderer>().material = inactiveRing;
        }
        //Make sure we are not stupid
        if(rings.Count==0)
        {
            Debug.Log("There is no objectives assaign o n this level, Make sure you put some ring under the objective");
            return;
        }
        //Activate the first ring
        rings[ringPassed].GetComponent<MeshRenderer>().material = activeRing;
        rings[ringPassed].GetComponent<Ring>().ActivateRing();

    }
    public void NextRing()
    {
        //play fx on the current ring
        rings[ringPassed].GetComponent<Animator>().SetTrigger("Collection Trigger");

        //Up the int
        ringPassed++;

        //If it is the final ring ,Lets call the victory
        if(ringPassed == rings.Count)
        {
            Victory();
            return;

        }
        //If this is the preview last, give the next ring The "Final Ring" material
        if (ringPassed == rings.Count - 1)
            rings[ringPassed].GetComponent<MeshRenderer>().material = finalRing;
        else
            rings[ringPassed].GetComponent<MeshRenderer>().material = activeRing;

        //In both cases, we need to activate trhe ring
        rings[ringPassed].GetComponent<Ring>().ActivateRing();
    }
    public Transform GetCurrentRing()
    {
        return rings[ringPassed];
    }

    private void Victory()
    {
        FindObjectOfType<GameScene>().CompleteLevel();
    }


}
