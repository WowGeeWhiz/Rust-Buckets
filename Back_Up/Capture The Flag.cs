using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaptureTheFlag : MonoBehaviour
{
    /*
     * This script was intended to be attached to the game manager if the selected gamemode was CTF
     * I am currently thinking that this isn't needed and the game manager script can be made to handle all gamemodes
     */
    public GameObject redFlag;
    public GameObject blueFlag;

    public GameObject redFlagPoint;
    public GameObject blueFlagPoint;

    public GameObject redFlagPointRadius;
    public GameObject blueFlagPointRadius;

    // Start is called before the first frame update
    void Start()
    {
        //enable all CTF game objects
        GameObject.Find("CTF").SetActive(true);
        
        //find flag points 
        redFlagPoint = GameObject.Find("redFlagPoint");
        blueFlagPoint = GameObject.Find("blueFlagPoint");
        
        //find flags 
        redFlag = GameObject.Find("redFlag");
        blueFlag = GameObject.Find("blueFlag");
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
