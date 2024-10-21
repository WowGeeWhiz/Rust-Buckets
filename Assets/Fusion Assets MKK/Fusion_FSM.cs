using Fusion;
using UnityEngine;

public class Fusion_FSM : MonoBehaviour
{
    public enum PlayerState { Idle, Run }
    [SerializeField] private PlayerState currentState = PlayerState.Idle;

    [Header("Animation Clips")]
    [SerializeField] public AnimationClip idleAnimation;
    [SerializeField] public AnimationClip runAnimation;
    [SerializeField] public GameObject Rustbucket;

    private float animationTime;
    private bool isRunning;


}
