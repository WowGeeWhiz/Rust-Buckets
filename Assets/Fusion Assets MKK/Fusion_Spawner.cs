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

            NetworkObject PlayerObject = Runner.Spawn(PlayerPrefab, spawnPosition, Quaternion.identity, player);
            Players.Add(player, PlayerObject.GetComponent<Fusion_Player>());
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
