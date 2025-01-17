using Platformer.Core;
using Platformer.Mechanics;
using UnityEngine;

public class PlayerImpacted : Simulation.Event<PlayerImpacted>
{
    public PlayerController player;

    public override void Execute()
    {
        // TODO: ANIMATE IMPACT (shake the player horizontally while keeping their hitbox in the centre (see smash bros))
    }
}
