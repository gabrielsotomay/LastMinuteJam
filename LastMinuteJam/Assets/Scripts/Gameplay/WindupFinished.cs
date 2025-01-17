using Platformer.Core;
using Platformer.Gameplay;
using Platformer.Mechanics;
using UnityEngine;

public class WindupFinished : Simulation.Event<WindupFinished>
{
    public PlayerController player;

    public override void Execute()
    {
        player.OnWindupFinished();
    }
}
