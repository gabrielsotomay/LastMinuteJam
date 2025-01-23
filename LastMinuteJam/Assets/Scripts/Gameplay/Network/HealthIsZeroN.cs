using Platformer.Core;
using Platformer.Mechanics;
using static Platformer.Core.SimulationNetick;

namespace Platformer.Gameplay
{
    /// <summary>
    /// Fired when the player health reaches 0. This usually would result in a 
    /// PlayerDeath event.
    /// </summary>
    /// <typeparam name="HealthIsZero"></typeparam>
    public class HealthIsZeroN : SimulationNetick.Event<HealthIsZeroN>
    {
        public Health health;
        public float time;

        public override void Execute()
        {
            Schedule<PlayerDeathN>((int)time);
        }
    }
}