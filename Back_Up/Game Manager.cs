using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    /*
     * Not sure what to do with this script right now
     * A "game manager" will be needed to determine what game objects need to be enabled on the stage depending on the game mode
     * It will need to be intertwined with the menu so I'm not sure how to implement it until we have a concrete plan for how the menus work
     * I figured this would also keep track of scores and maybe player stats if we decide to track that
     * All of this code will probably be deleted and reworked
     */
    public int redTeamScore;
    public int blueTeamScore;

    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(this.gameObject);
        redTeamScore = 0;
        blueTeamScore = 0;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SelectedGameMode(string mode)
    {
        if (mode.Equals("CTF"))
        {
            this.AddComponent<CaptureTheFlag>();
        }
    }

    public void EndScreen()
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
