using Platformer.Core;
using Platformer.Mechanics;
using UnityEngine;

public class RecoveryFinishedN : SimulationNetick.Event<RecoveryFinishedN>
{
    public NetworkedPlayerController player;

    public override void Execute()
    {
        player.OnRecoveryFinished();
    }
}