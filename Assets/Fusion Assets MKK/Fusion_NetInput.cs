using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum InputButton 
{
    Jump,
}

public struct Fusion_NetInput : INetworkInput
{
    public NetworkButtons Buttons;
    public Vector2 Direction;
    public Vector2 LookDelta;
}
