using Platformer.Core;
using Platformer.Mechanics;
using UnityEngine;

public class RecoveryFinishedN : Simulation.Event<RecoveryFinishedN>
{
    public NetworkedPlayerController player;

    public override void Execute()
    {
        player.OnRecoveryFinished();
    }
}