using UnityEngine;

using Platformer.Core;
using Platformer.Mechanics;
using UnityEngine;
using LastMinuteJam;

public class TakeHitN : SimulationNetick.Event<TakeHitN>
{
    public NetworkedPlayerController player;
    public NetworkAttackController attackController;
    public override void Execute()
    {
        player.TakeHit(attackController);
        // TODO: animate character picking themselves up from floor?
    }
}
