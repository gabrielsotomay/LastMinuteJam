using Platformer.Core;
using Platformer.Mechanics;
using UnityEngine;

public class PlayerDisable : Simulation.Event<PlayerDisable>
{
    public PlayerController player;

    public override void Execute()
    {
        // TODO: animate character going into hurt animation?
    }
}
