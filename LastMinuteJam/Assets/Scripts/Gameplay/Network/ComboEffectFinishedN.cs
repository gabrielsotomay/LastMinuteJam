
using Platformer.Core;
using Platformer.Mechanics;
using Platformer.Model;

namespace Platformer.Gameplay
{
    /// <summary>
    /// This event is fired when user input should be enabled.
    /// </summary>
    public class ComboEffectFinishedN : SimulationNetick.Event<ComboEffectFinishedN>
    {
        public NetworkedPlayerController player;

        public override void Execute()
        {
            player.ComboEffectFinished();
        }
    }
}