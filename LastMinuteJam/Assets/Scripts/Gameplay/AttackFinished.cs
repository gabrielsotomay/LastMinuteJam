using Platformer.Core;
using Platformer.Mechanics;
using UnityEngine;

public class AttackFinished : Simulation.Event<AttackFinished>
{
    public PlayerController player;

    public override void Execute()
    {
        player.OnAttackFinished();

        // TODO : Animate finish attack (return to normal)
    }
}
