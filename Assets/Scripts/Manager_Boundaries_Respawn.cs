using UnityEngine;
using Unity.Netcode;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 
/// Author: Michael Knighen
/// 
/// Date Started: 8_31_2024
/// 
/// Last Updated: 9_1_2024
/// 
///  <<<DON'T TOUCH MY CODE>>>
///  
/// --------------------------------------------------------------------------------------------------------
/// 
/// Initialization 8_31_2024:
/// 
/// Description:
/// 
/// Tracks all active players at initialization, all boundaries, and all respawners. Constantly checks if there
/// are any players that are out of bounds and respawns them at a random respawner.
/// 
/// Package:
/// 
/// N/A
/// 
/// Note:
/// 
/// -All boundary wall objects must 
/// 
/// -This process is expensive and is a work in progress.
/// 
/// -Boundary objects must be set to triggers.
/// 
/// -This should be attached to a persistent manager.
/// 
/// -All NetworkBehaviour classes should be initialized after the natwork manager or it will be ignored. Reason it is set up in update.
/// 
/// -Respawns have to accounted for or issues can arise.
/// 
/// --------------------------------------------------------------------------------------------------------
/// Patch: 9_1_2024:
/// 
/// Description:
/// 
/// Added spawntimer.
/// 
/// Package:
/// 
/// N/A
/// 
/// Note:
/// 
/// -Spawn timer added to hud.
/// 
/// -Fixed rng for spawnpoints.
/// 
/// --------------------------------------------------------------------------------------------------------
/// </summary>

public class Manager_Boundaries_Respawn : NetworkBehaviour
{
    // Attributes:=--------------------------------------------------------------------------------------------------------------

    [Header("Boundary/Respawn Variables")]
    public GameObject[] OutOfBoundsWalls;
    public GameObject[] RespawnPoints;
    public GameObject[] Player;
    public int depthBoundary;
    public float respawnCooldown;
    public TextMeshProUGUI countdownText;

    private bool playerListInitialized = false;
    private Dictionary<GameObject, Coroutine> activeRespawns = new Dictionary<GameObject, Coroutine>(); // Track active respawn coroutines per player

    [Header("Reticle Variables")]
    public RawImage ReticleDynamic;
    public RawImage ReticleStatic;

    // Networking Methods:-------------------------------------------------------------------------------------------------------

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        Debug.Log("Manager_Boundaries_Respawn initialized.");
    }

    // Life_Cycle Methods:-------------------------------------------------------------------------------------------------------

    private void Update()
    {
        if (IsServer)
        {
            if (!playerListInitialized)
            {
                PlayerList();
                playerListInitialized = true;
                Debug.Log("Player list initialized.");
            }

            CheckDepthOutOfBounds();

            // Uncomment if you want to check standard boundaries as well (DONT NOT READY):
            // CheckBoundaryBarriers();
        }
    }

    // PlayerList Method:-----------------------------------------------------------------------------------------------------------

    private void PlayerList()
    {
        Player = GameObject.FindGameObjectsWithTag("Player");

        for (int i = 0; i < Player.Length; i++)
        {
            Debug.Log($"Player {i + 1}: {Player[i].name} joined lobby.");
        }
    }

    // Boundary Methods:-------------------------------------------------------------------------------------------------------

    private void CheckBoundaryBarriers()
    {
        foreach (GameObject player in Player)
        {
            foreach (GameObject boundary in OutOfBoundsWalls)
            {
                if (IsPlayerInteractingWithBoundary(player, boundary))
                {
                    RespawnPlayerServerRpc(player.GetComponent<NetworkObject>());
                }
            }
        }
    }

    private bool IsPlayerInteractingWithBoundary(GameObject player, GameObject boundary)
    {
        Collider boundaryCollider = boundary.GetComponent<Collider>();
        if (boundaryCollider != null)
        {
            return boundaryCollider.bounds.Contains(player.transform.position);
        }
        return false;
    }

    private void CheckDepthOutOfBounds()
    {
        foreach (GameObject player in Player)
        {
            if (player.transform.position.y <= depthBoundary)
            {
                RespawnPlayerServerRpc(player.GetComponent<NetworkObject>());
            }
        }
    }

    // Respawn Method:-------------------------------------------------------------------------------------------------------

    [ServerRpc]
    private void RespawnPlayerServerRpc(NetworkObjectReference playerNetworkObjectReference)
    {
        // Find the player with the given NetworkObjectReference:
        if (playerNetworkObjectReference.TryGet(out NetworkObject playerNetworkObject))
        {
            GameObject player = playerNetworkObject.gameObject;

            // Check if the player already has an active respawn coroutine:
            if (activeRespawns.ContainsKey(player))
            {
                //Debug.Log($"Respawn already in progress for player: {player.name}");

                // Exit if there's already an active respawn coroutine for this player:
                return;
            }

            var controllerMovement = player.GetComponent<Controller_Movement>();

            // Lock controls
            if (controllerMovement != null)
            {
                controllerMovement.LockControls(true);

                //Hide Reticle:
                ReticleDynamic.enabled = false;
                ReticleStatic.enabled = false;
            }

            // Start the respawn coroutine for this specific player and store it in the dictionary:
            Coroutine respawnCoroutine = StartCoroutine(RespawnAfterCooldown(player));

            // Track active respawn coroutine:
            activeRespawns[player] = respawnCoroutine;
        }
    }

    private IEnumerator RespawnAfterCooldown(GameObject player)
    {
        var controllerMovement = player.GetComponent<Controller_Movement>();
        var playerStats = player.GetComponent<Player_Stats>();//====================

        //Death:
        playerStats.TakeDamage(100f);//====================

        // Countdown timer:
        float countdown = respawnCooldown;

        while (countdown > 0)
        {
            // Update the TextMeshProUGUI component with the countdown value
            if (countdownText != null)
            {
                countdownText.text = ""+countdown;
            }

            // Wait for 1 second:
            yield return new WaitForSeconds(1f);
            countdown--;
        }

        // Clear the text or update it when countdown finishes:
        if (countdownText != null)
        {
            countdownText.text = "";
        }

        // Choose a random Respawn Point:
        int randomIndex = Random.Range(0, RespawnPoints.Length);
        GameObject randomRespawn = RespawnPoints[randomIndex];

        // Respawn Player:
        player.transform.position = randomRespawn.transform.position;
        player.transform.rotation = Quaternion.identity;

        playerStats.RestoreFullHP();//====================

        // Update Network Position:
        if (controllerMovement != null)
        {
            controllerMovement.UpdateNetworkPosition();
        }

        // Unlock controls after respawn:
        if (controllerMovement != null)
        {
            controllerMovement.LockControls(false);

            // Show Reticle:
            ReticleDynamic.enabled = true;
            ReticleStatic.enabled = true;
        }

        // Remove player from active respawn tracking once complete:
        if (activeRespawns.ContainsKey(player))
        {
            activeRespawns.Remove(player);
        }
    }

}