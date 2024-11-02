using Fusion;
using UnityEngine;

public class Fusion_Spawner : NetworkBehaviour, IPlayerJoined, IPlayerLeft
{

    [SerializeField] private NetworkPrefabRef PlayerPrefab;

    [Networked, Capacity(6)] private NetworkDictionary<PlayerRef, Fusion_Player> Players => default;

    public GameObject[] spawnPoints;

    public void PlayerJoined(PlayerRef player)
    {
        if (HasStateAuthority)
        {
            Vector3 spawnPosition = GetSpawnPosition();
            Debug.LogError($"Player {player} joined, spawning at position: {spawnPosition}");

            NetworkObject playerObject = Runner.Spawn(PlayerPrefab, spawnPosition, Quaternion.identity, player);
            if (playerObject == null)
            {
                Debug.LogError("Failed to spawn player object.");
                return;
            }

            Players.Add(player, playerObject.GetComponent<Fusion_Player>());
            Debug.Log($"Player {player} spawned successfully.");
        }
    }


    public void PlayerLeft(PlayerRef player)
    {
        if (!HasStateAuthority) 
            return;

        if (Players.TryGet(player, out Fusion_Player playerBehaviour)) 
        {
            Players.Remove(player);
            Runner.Despawn(playerBehaviour.Object);
        }
    }

    private Vector3 GetSpawnPosition()
    {

        // Randomly select a spawn point from the array
        return spawnPoints[Random.Range(0, spawnPoints.Length)].transform.position;

    }
}
