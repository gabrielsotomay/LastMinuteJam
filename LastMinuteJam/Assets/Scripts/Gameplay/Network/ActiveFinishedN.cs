using Platformer.Core;
using Platformer.Gameplay;
using Platformer.Mechanics;
using UnityEngine;

public class ActiveFinishedN : Simulation.Event<ActiveFinishedN>
{
    public NetworkedPlayerController player;

    public override void Execute()
    {
        player.OnActiveFinished();
    }
}
