using Platformer.Core;
using Platformer.Mechanics;
using UnityEngine;

public class PlayerDisableN : SimulationNetick.Event<PlayerDisableN>
{
    public NetworkedPlayerController player;

    public override void Execute()
    {
        // TODO: animate character going into hurt animation?
    }
}
