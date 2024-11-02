

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
        // Ensure only one instance exists
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
        // Check if this instance is controlled by the server
        if (!Object.HasStateAuthority)
        {
            Debug.LogError("Knighten_NetDataManager can only be instantiated by the server. Destroying this instance.");
            Destroy(gameObject);
            return;
        }

        InitializePlayerData();
    }

    private void InitializePlayerData()
    {
        // Initialization code if needed
    }

    // Only allow the server to register players
    public void RegisterPlayer(PlayerDataComponent playerDataComponent)
    {
        // Check if playerDataComponent is null
        if (playerDataComponent == null)
        {
            Debug.LogError("PlayerDataComponent is null. Cannot register player.");
            return;
        }

        // Check if PlayerData is null
        if (playerDataComponent.PlayerData == null)
        {
            Debug.LogError("PlayerData is null in PlayerDataComponent. Cannot register player.");
            return;
        }

        Debug.Log($"Attempting to register player: {playerDataComponent.PlayerData.Name} from {Object.Id.GetHashCode()}");

        // No longer checking for HasStateAuthority
        int playerId = playerDataComponent.GetComponent<NetworkObject>().Id.GetHashCode();
        if (!players.ContainsKey(playerId))
        {
            players[playerId] = playerDataComponent;
            Debug.Log($"Player registered: {playerDataComponent.PlayerData.Name}");
        }

        LogCurrentPlayers();
    }

    public void UnregisterPlayer(PlayerDataComponent playerDataComponent)
    {
        // Only allow the server to unregister players
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

    private void LogCurrentPlayers()
    {
        Debug.LogWarning("IMPORTANT: Current Players:");
        foreach (var player in players)
        {
            Debug.Log($"- {player.Value.PlayerData.Name} (ID: {player.Key})");
        }
    }

    // RPC to ensure all clients update the NameTags for a specific player
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_UpdateNameTag(int playerId, string name)
    {
        if (players.TryGetValue(playerId, out var playerDataComponent))
        {
            // Update the NameTag text if the player component exists in the dictionary
            if (playerDataComponent != null && playerDataComponent.NameTag != null)
            {
                playerDataComponent.NameTag.text = name;
                Debug.Log($"Updated NameTag for player {name} with ID: {playerId}");
            }
        }
        else
        {
            Debug.LogWarning($"Player ID {playerId} not found in dictionary for NameTag update.");
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_NameSynchdonizer() 
    {
        // Find all game objects with the "player" tag
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject player in players)
        {
            // Attempt to get the PlayerDataComponent (or similar script) attached to each player object
            var playerDataComponent = player.GetComponent<PlayerDataComponent>();

            // Check if playerDataComponent and NameTag are not null to avoid errors
            if (playerDataComponent != null && playerDataComponent.NameTag != null)
            {
                // Set the NameTag's text to match PlayerData.Name
                playerDataComponent.NameTag.text = playerDataComponent.PlayerData.Name;
            }
            else
            {
                Debug.LogWarning($"Missing PlayerDataComponent or NameTag on player object: {player.name}");
            }
        }
    }

    public override void FixedUpdateNetwork() 
    {
        RPC_NameSynchdonizer();
    }
}
