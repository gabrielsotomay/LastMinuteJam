using UnityEngine;
using Netick;
using Netick.Unity;

public struct FighterInput : INetworkInput
{

    [Networked] public Vector2 movement;

    public NetworkBool lightAttack;
    public NetworkBool heavyAttack;
    public NetworkBool jumpPress;
    public NetworkBool jumpRelease;
}
