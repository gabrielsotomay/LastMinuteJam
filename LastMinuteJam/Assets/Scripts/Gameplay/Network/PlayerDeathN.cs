using System.Collections;
using System.Collections.Generic;
using Platformer.Core;
using Platformer.Mechanics;
using Platformer.Model;
using UnityEngine;

namespace Platformer.Gameplay
{
    /// <summary>
    /// Fired when the player has died.
    /// </summary>
    /// <typeparam name="PlayerDeath"></typeparam>
    public class PlayerDeathN : SimulationNetick.Event<PlayerDeathN>
    {
        public NetworkedPlayerController player;
        public override void Execute()
        {
            player.OnDeath();
        }
    }
}