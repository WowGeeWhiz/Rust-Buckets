using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flag : MonoBehaviour
{
    /*
     * Script that attaches to the flags themselves
     * Handles collision, capturing, and returning flags
     */
    public GameObject flagDeliveryPoint;
    public GameObject flagReturnPoint;
    public GameObject flagReturnAura;

    public GameObject gameManager;

    public GameObject CTFParent;

    Vector3 initialFlagPosition;

    public float flagReturnTime = 10.0f;

    // Start is called before the first frame update
    void Start()
    {
        initialFlagPosition = this.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        //returns flag after it's on the ground for 10 seconds
        //currently untested. probably works
        //checks if flag is attached to aan object
        if(!this.transform.IsChildOf(transform))
        {
            flagReturnTime -= Time.fixedDeltaTime;
        }
        if(flagReturnTime <= 0 )
        {
            flagReturnTime = 10.0f;
            this.gameObject.transform.SetParent(CTFParent.gameObject.transform);
            this.transform.position = initialFlagPosition;
        }


        /*
         * makes the flag fall to the ground if a player dies while holding it
         * psuedocode right now because I'm not really sure how a player death is handled or can be called like this
         * the games also not really in a state where that's easily tested
         * if(player.dies){
         *      this.transform.parent = null;
         *      this.gameObject.GetComponent<Rigidbody>().useGravity = true;
         * }
         */
    }

    public void OnCollisionEnter(Collision collision)
    {
        this.gameObject.GetComponent<Rigidbody>().useGravity = false;
        //As far as I can tell there isn't anything in the player script that differentiates teams, so this will need to be reworked
        if (collision.gameObject.tag == "Player")
        {
            this.gameObject.transform.SetParent(collision.gameObject.transform, false);
            this.gameObject.transform.position = collision.gameObject.transform.position + new Vector3(0, 2, 1);
        }
    }

    //handles when a flag is "delivered"
    //increments team points and resets flag position
    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "blueFlagPointRadius" && this.gameObject.name == "redFlag")
        {
            this.gameObject.transform.SetParent(CTFParent.gameObject.transform);
            this.gameObject.transform.position = initialFlagPosition;
            gameManager.GetComponent<GameManager>().BlueTeamScoreUpdate(1);
        }
        if (other.gameObject.name == "redFlagPointRadius" && this.gameObject.name == "blueFlag")
        {
            this.gameObject.transform.SetParent(CTFParent.gameObject.transform);
            this.gameObject.transform.position = initialFlagPosition;
            gameManager.GetComponent<GameManager>().RedTeamScoreUpdate(1);
        }
    }
}
