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
        // Only initialize player data if this instance is controlled by the server
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

    // Only allow the server to register players
    public void RegisterPlayer(PlayerDataComponent playerDataComponent)
    {
        if (!Object.HasStateAuthority)
        {
            Debug.LogWarning("Attempt to register player from non-authoritative client.");
            return;
        }

        int playerId = playerDataComponent.GetComponent<NetworkObject>().Id.GetHashCode();
        if (!players.ContainsKey(playerId))
        {
            players[playerId] = playerDataComponent;
            Debug.Log($"Player registered: {playerDataComponent.PlayerData.Name}");
        }

        LogCurrentPlayers();
    }

    private void LogCurrentPlayers()
    {
        Debug.LogWarning("IMPORTANT: Current Players:");
        foreach (var player in players)
        {
            Debug.Log($"- {player.Value.PlayerData.Name} (ID: {player.Key})");
        }
    }

    // Only allow the server to unregister players
    public void UnregisterPlayer(PlayerDataComponent playerDataComponent)
    {
        if (!Object.HasStateAuthority)
        {
            Debug.LogWarning("Attempt to unregister player from non-authoritative client.");
            return;
        }

        int playerId = playerDataComponent.GetComponent<NetworkObject>().Id.GetHashCode();
        if (players.ContainsKey(playerId))
        {
            players.Remove(playerId);
            Debug.Log($"Player unregistered: {playerDataComponent.PlayerData.Name}");

            // Optionally log current players after unregistration as well
            LogCurrentPlayers();
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
