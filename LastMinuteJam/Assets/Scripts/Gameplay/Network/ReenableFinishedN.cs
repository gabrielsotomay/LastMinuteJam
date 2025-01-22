using Platformer.Core;
using Platformer.Mechanics;
using UnityEngine;

public class ReenableFinishedN : SimulationNetick.Event<ReenableFinishedN>
{
    public NetworkedPlayerController player;

    public override void Execute()
    {
        player.OnReenableFinish();
    }
}