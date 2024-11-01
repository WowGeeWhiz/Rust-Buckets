using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using TMPro;

public class PlayerDataComponent : NetworkBehaviour
{
    [SerializeField] private PlayerData playerData;


    [SerializeField] private TextMeshPro NameTag;//-----------

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
        // Check if Knighten_NetDataManager has the current player's name
        if (Knighten_NetDataManager.Instance != null)
        {
            // Retrieve the player's current name from the manager
            PlayerData updatedData = Knighten_NetDataManager.Instance.GetPlayerData(Object.Id.GetHashCode());

            if (updatedData != null)
            {
                // Set the name to the one retrieved from the manager
                playerData.Name = updatedData.Name;

                // Notify the player to update the name tag if this component has input authority
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

        if (Knighten_NetDataManager.Instance != null)
        {
            Knighten_NetDataManager.Instance.RegisterPlayer(this);
        }
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        // Unregister from Knighten_NetDataManager to ensure clean-up
        if (Knighten_NetDataManager.Instance != null)
        {
            Knighten_NetDataManager.Instance.UnregisterPlayer(this);
        }
    }

    public override void FixedUpdateNetwork() 
    {
        NameTag.text = playerData.Name;
    }

    private void InitializePlayerData()
    {
        playerData = new PlayerData
        {
            Name = "Player_" + Object.Id, // Use the NetworkId or other identifiers
            HP = 100, // Set the initial HP
            State = "Idle" // Set the initial state
        };

        Debug.Log($"Initialized player name: {playerData.Name}"); // Add this line to debug
        UpdatePlayerName(); // Update the UI with the initial name
    }

}