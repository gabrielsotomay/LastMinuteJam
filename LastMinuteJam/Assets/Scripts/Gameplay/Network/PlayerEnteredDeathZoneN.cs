using Platformer.Core;
using Platformer.Mechanics;
using Platformer.Model;

namespace Platformer.Gameplay
{
    /// <summary>
    /// Fired when a player enters a trigger with a DeathZone component.
    /// </summary>
    /// <typeparam name="PlayerEnteredDeathZone"></typeparam>
    public class PlayerEnteredDeathZoneN : SimulationNetick.Event<PlayerEnteredDeathZoneN>
    {
        public DeathZone deathzone;

        PlatformerModel model = SimulationNetick.GetModel<PlatformerModel>();
        public float time;

        public override void Execute()
        {
            SimulationNetick.Schedule<PlayerDeathN>(time);
        }
    }
}