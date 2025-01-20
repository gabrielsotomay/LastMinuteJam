using Platformer.Core;
using Platformer.Mechanics;

namespace Platformer.Gameplay
{
    /// <summary>
    /// Fired when the Jump Input is deactivated by the user, cancelling the upward velocity of the jump.
    /// </summary>
    /// <typeparam name="PlayerStopJump"></typeparam>
    public class PlayerStopJumpN : Simulation.Event<PlayerStopJumpN>
    {
        public NetworkedPlayerController player;

        public override void Execute()
        {

        }
    }
}