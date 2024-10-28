using Fusion;
using UnityEngine;

public class Fusion_Player_StateMachine : NetworkBehaviour
{
    private Animator playerAnim;  // Reference to the Animator component

    // States can be defined as enums
    public enum PlayerState
    {
        Idle,
        Run
    }

    // This will allow state transitions to be networked
    [Networked]
    public PlayerState NetworkState { get; set; }

    private PlayerState currentState;

    public override void Spawned()
    {
        playerAnim = GetComponentInChildren<Animator>();

        if (playerAnim == null)
        {
            Debug.LogWarning("Animator component not found on child object!");
        }
        else
        {
            currentState = PlayerState.Idle;  // Set initial state
            UpdateAnimationState(currentState);  // Set initial animation
        }
    }

    public override void FixedUpdateNetwork()
    {
        // Every frame, check if the local state matches the networked state
        if (NetworkState != currentState)
        {
            UpdateAnimationState(NetworkState);
            currentState = NetworkState;
        }
    }

    public void TransitionToState(PlayerState newState)
    {
        if (currentState != newState)
        {
            currentState = newState;
            NetworkState = newState;  // Sync over the network
            UpdateAnimationState(newState);  // Update animation locally
        }
    }

    // Update animation state based on the current state using triggers
    private void UpdateAnimationState(PlayerState state)
    {
        switch (state)
        {
            case PlayerState.Idle:
                playerAnim.ResetTrigger("Run");
                playerAnim.SetTrigger("Idle");
                break;
            case PlayerState.Run:
                playerAnim.ResetTrigger("Idle");
                playerAnim.SetTrigger("Run");
                break;
        }
    }
}