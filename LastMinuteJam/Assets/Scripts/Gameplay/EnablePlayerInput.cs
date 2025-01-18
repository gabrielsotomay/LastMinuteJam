using Platformer.Core;
using Platformer.Mechanics;
using Platformer.Model;

namespace Platformer.Gameplay
{
    /// <summary>
    /// This event is fired when user input should be enabled.
    /// </summary>
    public class EnablePlayerInput : Simulation.Event<EnablePlayerInput>
    {
        public PlayerController player = Simulation.GetModel<PlayerController>();

        public override void Execute()
        {
            player.controlEnabled = true;
        }
    }
}