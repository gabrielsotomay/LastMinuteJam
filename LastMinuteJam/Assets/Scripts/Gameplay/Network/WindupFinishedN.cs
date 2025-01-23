using Platformer.Core;
using Platformer.Gameplay;
using Platformer.Mechanics;
using UnityEngine;

public class WindupFinishedN : SimulationNetick.Event<WindupFinishedN>
{
    public NetworkedPlayerController player;

    public override void Execute()
    {
        player.OnWindupFinished();
    }
}
