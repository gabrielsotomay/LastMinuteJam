using Platformer.Core;
using Platformer.Gameplay;
using Platformer.Mechanics;
using UnityEngine;

public class ActiveFinished : Simulation.Event<ActiveFinished>
{
    public PlayerController player;

    public override void Execute()
    {
        player.OnActiveFinished();
    }
}
