using Platformer.Core;
using Platformer.Mechanics;
using UnityEngine;

public class AttackFinishedN : Simulation.Event<AttackFinishedN>
{
    public NetworkedPlayerController player;

    public override void Execute()
    {
        player.OnAttackFinished();

        // TODO : Animate finish attack (return to normal)
    }
}
