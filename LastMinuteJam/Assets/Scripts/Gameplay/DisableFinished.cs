using Platformer.Core;
using Platformer.Mechanics;
using UnityEngine;

public class DisableFinished : Simulation.Event<DisableFinished>
{
    public PlayerController player;

    public override void Execute()
    {
        player.OnDisableFinished();
        // TODO: animate character picking themselves up from floor?
    }
}
