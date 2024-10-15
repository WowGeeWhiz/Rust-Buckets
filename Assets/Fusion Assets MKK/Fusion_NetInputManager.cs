using Fusion;
using Fusion.Sockets;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Fusion_NetInputManager : SimulationBehaviour, IBeforeUpdate, INetworkRunnerCallbacks
{

    private Fusion_NetInput accumulatedInput;
    private bool resetInput;
    private readonly int speed = 7000;

    public void BeforeUpdate()
    {
        if (resetInput) 
        {
            resetInput = false;
            accumulatedInput = default;
        }

        Gamepad gamepad = Gamepad.current;
        if (gamepad != null && (
            gamepad.leftStickButton.wasPressedThisFrame ||
            gamepad.rightStickButton.wasPressedThisFrame ||
            gamepad.buttonSouth.wasPressedThisFrame || // A (or Cross on PS)
            gamepad.buttonNorth.wasPressedThisFrame || // Y (or Triangle on PS)
            gamepad.buttonEast.wasPressedThisFrame || // B (or Circle on PS)
            gamepad.buttonWest.wasPressedThisFrame || // X (or Square on PS)
            gamepad.leftTrigger.wasPressedThisFrame || // Left trigger
            gamepad.rightTrigger.wasPressedThisFrame || // Right trigger
            gamepad.leftShoulder.wasPressedThisFrame || // Left bumper
            gamepad.rightShoulder.wasPressedThisFrame || // Right bumper
            gamepad.dpad.up.wasPressedThisFrame || // D-Pad up
            gamepad.dpad.down.wasPressedThisFrame || // D-Pad down
            gamepad.dpad.left.wasPressedThisFrame || // D-Pad left
            gamepad.dpad.right.wasPressedThisFrame || // D-Pad right
            gamepad.leftStick.ReadValue().magnitude > 0.1f ||  // Detect left stick movement
            gamepad.rightStick.ReadValue().magnitude > 0.1f    // Detect right stick movement
        ))
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        //accumulate input only if the cursor is locked:
        if (Cursor.lockState != CursorLockMode.Locked)
            return;

        NetworkButtons buttons = default;

        if (gamepad != null) 
        {
            Vector2 rightStickDelta = gamepad.rightStick.ReadValue();
            Vector2 lookRotationDelta = new(-rightStickDelta.y, rightStickDelta.x);
            accumulatedInput.LookDelta += lookRotationDelta;

            //Debug.LogWarning($"Right Stick Delta: {rightStickDelta}, Look Rotation Delta: {lookRotationDelta}");
        }

        if (gamepad != null) 
        {
            Vector2 moveDirection = Vector2.zero;

            //Left Thumbstick:
            if (gamepad.leftStick.ReadValue().magnitude > 0.1f) 
            {
                Vector2 leftStick = gamepad.leftStick.ReadValue();
                float horizontalLeft = leftStick.x * speed * Time.fixedDeltaTime;
                float verticalLeft = leftStick.y * speed * Time.fixedDeltaTime;

                moveDirection = new Vector2(horizontalLeft, verticalLeft);
            }

            accumulatedInput.Direction += moveDirection;
            buttons.Set(InputButton.Jump, Gamepad.current.aButton.isPressed);
        }

        accumulatedInput.Buttons = new NetworkButtons(accumulatedInput.Buttons.Bits | buttons.Bits);

    }

    public void OnConnectedToServer(NetworkRunner runner)
    {

    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {

    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {

    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {

    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {

    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {

    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        accumulatedInput.Direction.Normalize();
        input.Set(accumulatedInput);
        resetInput = true;

        //We have to reset the look delta immediately because we don't want mouse input being reused if another tick is executed during this same frame;
        accumulatedInput.LookDelta = default;
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {

    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {

    }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {

    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (player == runner.LocalPlayer)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {

    }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {

    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {

    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {

    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {

    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {

    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {

    }

}
