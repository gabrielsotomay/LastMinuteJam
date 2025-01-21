using Platformer.Core;
using Platformer.Mechanics;
using Platformer.Model;

namespace Platformer.Gameplay
{
    /// <summary>
    /// This event is fired when user input should be enabled.
    /// </summary>
    public class EnablePlayerInputN : SimulationNetick.Event<EnablePlayerInputN>
    {
        public NetworkedPlayerController player;

        public override void Execute()
        {
            player.controlEnabled = true;
        }
    }
}