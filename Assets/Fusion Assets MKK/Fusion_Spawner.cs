using Fusion;
using UnityEngine;

public class Fusion_Spawner : SimulationBehaviour, IPlayerJoined
{

    public GameObject PlayerPrefab;

    public GameObject[] spawnPoints;

    public void PlayerJoined(PlayerRef player) 
    {

        if (Runner.LocalPlayer == player) 
        {

            Vector3 spawnPosition = GetSpawnPosition();

            Runner.Spawn(PlayerPrefab, spawnPosition, Quaternion.identity, player);
        }

    }

    private Vector3 GetSpawnPosition()
    {

        // Randomly select a spawn point from the array
        return spawnPoints[Random.Range(0, spawnPoints.Length)].transform.position;

    }

}
