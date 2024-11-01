using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class Knighten_NetDataManager : NetworkBehaviour
{
    private static Knighten_NetDataManager instance;
    private Dictionary<int, PlayerDataComponent> players = new Dictionary<int, PlayerDataComponent>();

    public static Knighten_NetDataManager Instance
    {
        get
        {
            if (instance == null)
            {
                Debug.LogError("Knighten_NetDataManager instance is not set. Ensure it is added to the scene.");
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);  // Destroy any additional instances
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);  // Keep this instance persistent
    }

    public override void Spawned()
    {
        if (Object.HasStateAuthority && instance == this)
        {
            InitializePlayerData();
        }
    }

    private void InitializePlayerData()
    {
        GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");
        foreach (var playerObject in playerObjects)
        {
            var playerDataComponent = playerObject.GetComponent<PlayerDataComponent>();
            if (playerDataComponent != null)
            {
                int playerId = playerObject.GetComponent<NetworkObject>().Id.GetHashCode();
                players[playerId] = playerDataComponent;
            }
        }
    }

    public void RegisterPlayer(PlayerDataComponent playerDataComponent)
    {
        int playerId = playerDataComponent.GetComponent<NetworkObject>().Id.GetHashCode();
        if (!players.ContainsKey(playerId))
        {
            players[playerId] = playerDataComponent;
            Debug.Log($"Player registered: {playerDataComponent.PlayerData.Name}");
        }
    }

    public void UnregisterPlayer(PlayerDataComponent playerDataComponent)
    {
        int playerId = playerDataComponent.GetComponent<NetworkObject>().Id.GetHashCode();
        if (players.ContainsKey(playerId))
        {
            players.Remove(playerId);
            Debug.Log($"Player unregistered: {playerDataComponent.PlayerData.Name}");
        }
    }

    public PlayerData GetPlayerData(int playerId)
    {
        return players.TryGetValue(playerId, out var dataComponent) ? dataComponent.PlayerData : null;
    }

    public Dictionary<int, PlayerDataComponent> GetAllPlayers()
    {
        return players;
    }
}