using Fusion;
using Fusion.Addons.SimpleKCC;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Fusion_Player : NetworkBehaviour
{
    [SerializeField] private SimpleKCC kcc;
    [SerializeField] private Transform camTarget;
    [SerializeField] private float speed = 500f;
    [SerializeField] private float jumpImpulse = 500f;
    [SerializeField] private bool invertVertical = true;
    [SerializeField] private float deadzone = 0.7f;

    public GameObject gameManager;
    public string team;
    public Material defaultMaterial;
    public List<SkinnedMeshRenderer> skinnedMeshRenderer;
    List<Material> materials = null;

    //private Fusion_Player_StateMachine playerStateMachine;//------------------

    [Networked] public string Name { get; private set; }
    [Networked] private NetworkButtons previousButtons { get; set; }

    public override void Spawned()
    {
        gameManager = GameObject.Find("GameManager");
        kcc.SetGravity(Physics.gravity.y * 2f);

        if (HasInputAuthority) 
        {

            Name = PlayerPrefs.GetString("Username");
            RPC_PlayerName(Name);
            Fusion_Camera_Follow.Singleton.SetTarget(camTarget);

            //playerStateMachine = GetComponent<Fusion_Player_StateMachine>();//------------------
        }

        
        if(gameManager.GetComponent<GameManager>().redTeam <= gameManager.GetComponent<GameManager>().blueTeam)
        {
            team = "red";
            gameManager.GetComponent<GameManager>().redTeam++;
            foreach (var allMaterials in skinnedMeshRenderer)
            {
                materials = allMaterials.materials.ToList();
                foreach (var material in materials)
                {
                    if (material.name == "Dessert_Paint (Instance)")
                    {
                        material.color = Color.red;
                    }
                }
            }
            materials.Clear();
        }
        else
        {
            team = "blue";
            gameManager.GetComponent<GameManager>().blueTeam++;
            foreach (var allMaterials in skinnedMeshRenderer)
            {
                materials = allMaterials.materials.ToList();
                foreach (var material in materials)
                {
                    if (material.name == "Dessert_Paint (Instance)")
                    {
                        material.color = Color.blue;
                    }
                }
            }
            materials.Clear();
        }
    }

    public override void FixedUpdateNetwork() 
    {
        if (GetInput(out Fusion_NetInput input)) 
        {

            //UpdateMovementState(input);//------------------

            //Look inversion:
            int inversion = invertVertical ? -1 : 1;

            //Deadzone:
            if (Mathf.Abs(input.LookDelta.x) < deadzone)
            {
                input.LookDelta.x = 0f; // Ignore input if within deadzone

            }
            else
            {
                input.LookDelta.x *= inversion;
            }

            if (Mathf.Abs(input.LookDelta.y) < deadzone)
            {
                input.LookDelta.y = 0f; // Ignore input if within deadzone

            }

            kcc.AddLookRotation(input.LookDelta);
            UpdateCamTarget();

            Vector3 worldDirection = kcc.TransformRotation * new Vector3(input.Direction.x, 0f, input.Direction.y);
            float jump = 0f;

            if (input.Buttons.WasPressed(previousButtons, InputButton.Jump) && kcc.IsGrounded) 
            {
                jump = jumpImpulse;
            }

            kcc.Move(worldDirection.normalized * speed, jump);
            previousButtons = input.Buttons;
        }

    }

    public override void Render()
    {
        UpdateCamTarget();
    }

    private void UpdateCamTarget() 
    {
        camTarget.localRotation = Quaternion.Euler(kcc.GetLookRotation().x, 0f, 0f);
    }

    //private void UpdateMovementState(Fusion_NetInput input)//------------------
    //{
    //    // Check if the left thumbstick is being moved
    //    if (input.Direction.magnitude > 0.1f)  // Adjust threshold if needed
    //    {

    //        Debug.LogWarning("Player: " + Name + "Is trying to move or is moving.");
    //        // Player is moving, set to run state
    //        playerStateMachine.TransitionToState(Fusion_Player_StateMachine.PlayerState.Run);
    //    }
    //    else
    //    {
    //        // No significant input, set to idle state
    //        playerStateMachine.TransitionToState(Fusion_Player_StateMachine.PlayerState.Idle);
    //        Debug.LogWarning("Player: " + Name + "Is idle.");
    //    }
    //}

    // Holds Player Name:
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_PlayerName(string name) 
    {
        Name = name;
    }
}
