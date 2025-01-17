using UnityEngine;

namespace LastMinuteJam
{
    public class PlayerAttack
    {
        public readonly int id;
        public readonly GameObject hitbox;
        public readonly Vector2 position;
        public readonly float rotation;
        public readonly float windupTime;
        public readonly float activeTime;
        public readonly float recoverTime;
        public readonly float baseAttack;
    }
}
