using Platformer.Core;
using Platformer.Mechanics;
using UnityEngine;

public class DisableFinishedN : SimulationNetick.Event<DisableFinishedN>
{
    public NetworkedPlayerController player;

    public override void Execute()
    {
        player.OnDisableFinished();
        // TODO: animate character picking themselves up from floor?
    }
}
