using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using TMPro;

public class PlayerDataComponent : NetworkBehaviour
{
    [SerializeField] private PlayerData playerData;
    [SerializeField] private TextMeshPro NameTag; // Reference to the name tag UI

    public PlayerData PlayerData
    {
        get => playerData;
        set
        {
            playerData = value;
            UpdatePlayerName();
        }
    }

    public void UpdatePlayerName()
    {
        if (Knighten_NetDataManager.Instance != null)
        {
            // Retrieve the player's current name from the manager
            PlayerData updatedData = Knighten_NetDataManager.Instance.GetPlayerData(Object.Id.GetHashCode());

            if (updatedData != null)
            {
                playerData.Name = updatedData.Name;

                // Update the name tag if this component has input authority
                if (Object.HasInputAuthority)
                {
                    NameTag.text = playerData.Name;
                }
            }
        }
    }

    public override void Spawned()
    {
        if (Object.HasInputAuthority)
        {
            InitializePlayerData();
        }

        // Always register player data with the manager
        Knighten_NetDataManager.Instance?.RegisterPlayer(this);
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        // Unregister from Knighten_NetDataManager to ensure clean-up
        Knighten_NetDataManager.Instance?.UnregisterPlayer(this);
    }

    public override void FixedUpdateNetwork()
    {
        // Update the name tag text only if this instance has input authority
        if (Object.HasInputAuthority)
        {
            NameTag.text = playerData.Name;
        }
    }

    private void InitializePlayerData()
    {
        playerData = new PlayerData
        {
            Name = "Player_" + Object.Id, // Use the NetworkId or other identifiers
            HP = 100, // Set the initial HP
            State = "Idle" // Set the initial state
        };

        Debug.Log($"Initialized player name: {playerData.Name}"); // Debug log
        UpdatePlayerName(); // Update the UI with the initial name
    }
}
