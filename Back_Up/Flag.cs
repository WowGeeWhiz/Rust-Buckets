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


    // Start is called before the first frame update
    void Start()
    {
        initialFlagPosition = this.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void ReturnFlag()
    {
        //this is currently empty and this method may not be filled at all.
        //This is just to remember that I need to implement something that makes flags return to default position after x amount of time
        //or a player stands in flagReturnAura for x/someValue amount of time
    }

    public void OnCollisionEnter(Collision collision)
    {
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
