using Fusion;
using Fusion.Addons.SimpleKCC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fusion_Player : NetworkBehaviour
{
    [SerializeField] private SimpleKCC kcc;
    [SerializeField] private Transform camTarget;
    [SerializeField] private float speed = 500f;
    [SerializeField] private float jumpImpulse = 500f;
    [SerializeField] private bool invertVertical = true;
    [SerializeField] private float deadzone = 0.7f;

    [Networked] public string Name { get; private set; }
    [Networked] private NetworkButtons previousButtons { get; set; }

    public override void Spawned()
    {
        kcc.SetGravity(Physics.gravity.y * 2f);

        if (HasInputAuthority) 
        {
            Name = PlayerPrefs.GetString("Username");
            RPC_PlayerName(Name);
            Fusion_Camera_Follow.Singleton.SetTarget(camTarget);
        }
    }

    public override void FixedUpdateNetwork() 
    {
        if (GetInput(out Fusion_NetInput input)) 
        {
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

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_PlayerName(string name) 
    {
        Name = name;
    }
}
