using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public int redTeamScore;
    public int blueTeamScore;

    public int targetScore;

    public string mode;

    // Start is called before the first frame update
    void Start()
    {
        mode = "CTF";
        //DontDestroyOnLoad(this.gameObject);
        redTeamScore = 0;
        blueTeamScore = 0;

        if (mode.Equals("CTF"))
        {
            //enable all CTF game objects
            GameObject.Find("CTF").SetActive(true);
            targetScore = 3;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(blueTeamScore >= targetScore)
        {
            EndScreen("blue");
        }
        if(redTeamScore >= targetScore)
        {
            EndScreen("red");
        }
    }

    public void EndScreen(string team)
    {

    }

    public void RedTeamScoreUpdate(int value)
    {
        redTeamScore += value;
    }

    public void BlueTeamScoreUpdate(int value)
    {
        blueTeamScore += value;
    }

}
