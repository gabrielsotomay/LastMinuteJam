using Platformer.Core;
using Platformer.Mechanics;
using UnityEngine;

public class ImpactFinished : Simulation.Event<ImpactFinished>
{
    public PlayerController player;
    public int impactId;
    public override void Execute()
    {
        player.OnImpactFinished(impactId);
        // TODO: animate character picking themselves up from floor?
    }
}
