using Platformer.Core;
using Platformer.Mechanics;
using UnityEngine;

public class PlayerImpactedN : SimulationNetick.Event<PlayerImpactedN>
{
    public NetworkedPlayerController player;

    public override void Execute()
    {
        // TODO: ANIMATE IMPACT (shake the player horizontally while keeping their hitbox in the centre (see smash bros))
    }
}
