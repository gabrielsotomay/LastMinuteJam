using Platformer.Core;
using Platformer.Mechanics;

namespace Platformer.Gameplay
{
    /// <summary>
    /// Fired when the player character lands after being airborne.
    /// </summary>
    /// <typeparam name="PlayerLanded"></typeparam>
    public class PlayerLandedN : Simulation.Event<PlayerLandedN>
    {
        public NetworkedPlayerController player;

        public override void Execute()
        {

        }
    }
}