using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSettings : MonoBehaviour
{
    public int speed = 5;
    public GameObject player;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        player = GameObject.Find("Player(Clone)");
        player.transform.GetComponent<Fusion_Player>().speed = speed;
    }

    public void SpeedChange(int num)
    {
        speed = num;
    }
}
