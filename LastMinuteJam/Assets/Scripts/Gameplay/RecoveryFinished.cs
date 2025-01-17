using Platformer.Core;
using Platformer.Mechanics;
using UnityEngine;

public class RecoveryFinished : Simulation.Event<RecoveryFinished>
{
    public PlayerController player;

    public override void Execute()
    {
        player.OnRecoveryFinished();
    }
}