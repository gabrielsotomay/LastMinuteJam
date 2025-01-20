using Platformer.Core;
using Platformer.Mechanics;

namespace Platformer.Gameplay
{
    /// <summary>
    /// Fired when the player performs a Jump.
    /// </summary>
    /// <typeparam name="PlayerJumped"></typeparam>
    public class PlayerJumpedN : Simulation.Event<PlayerJumpedN>
    {
        public AudioHelper.OneShot oneShotAudio;

        public override void Execute()
        {
            if (oneShotAudio.source && oneShotAudio.clip)
            {
                oneShotAudio.source.PlayOneShot(oneShotAudio.clip);

            }
        }
    }
}