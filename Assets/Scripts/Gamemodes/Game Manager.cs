using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Fusion;

public class GameManager : NetworkBehaviour
{
    public int redTeamScore;
    public int blueTeamScore;

    public int targetScore;

    public string mode;

    public List<GameObject> players;
    public List<GameObject> redTeam;
    public List<GameObject> blueTeam;

    // Start is called before the first frame update
    void Start()
    {
        players.Add(GameObject.Find("Player(Clone)"));

        bool isRedTeam = true;
        foreach(var player in players)
        {
            if(isRedTeam)
            {
                redTeam.Add(player);
                isRedTeam = !isRedTeam;
            }
            else
            {
                blueTeam.Add(player);
                isRedTeam = !isRedTeam;
            }
        }

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
    public override void FixedUpdateNetwork()
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
