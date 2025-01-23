using System.Linq;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

namespace LastMinuteJam
{
    [System.Serializable ]
    public struct PlayerAttack
    {
        [SerializeField] public int id;
        [SerializeField] public Vector2 hitboxScale;
        [SerializeField] public Vector2 position;
        [SerializeField] public float rotation;
        [SerializeField] public float windupTime;
        [SerializeField] public float activeTime;
        [SerializeField] public float recoverTime;
        [SerializeField] public float lifeTime;
        [SerializeField] public float baseAttack;
        [SerializeField] public float knockback;
        [SerializeField] public float impactTime;
        [SerializeField] public float disableTime;
        [SerializeField] public Type type;
        [SerializeField] public Vector2 recoil;
        [SerializeField] public Vector2 velocity;
        [SerializeField] public Vector2 imageScale;

        int _instanceId;

        public void SetInstanceId(int instanceId)
            { _instanceId = instanceId; }
        public int GetInstanceId()
        { return _instanceId; }
        public enum AttackDirection
        {
            Left,
            Right, 
            Up,
            None

        }
        public struct AttackIdsSent
        {
            private int[] _attacks;
            public int[] Attacks
            {
                get => _attacks;
                set => _attacks = value.Length <= 5 ? value : value.Take(5).ToArray();
            }

            public AttackIdsSent(int[] attacks)
            {
                _attacks = new int[attacks.Length];
                Attacks = attacks;
            }
        }
        public PlayerAttack(bool isValid)
        {
            if(!isValid)
            {
                id = -1;
            }
            else
            {
                id = 0;
            }
            hitboxScale = Vector2.zero;
            position = Vector2.zero;
            rotation = 0;
            activeTime = 0;            
            windupTime = 0;
            recoverTime = 0;
            lifeTime = 0;
            baseAttack = 0;
            knockback = 0;
            impactTime = 0;
            disableTime = 0;
            type = Type.None;
            recoil = Vector2.zero;
            velocity = Vector2.zero;
            _instanceId = 0;
            imageScale = Vector2.zero;




        }
        public enum Type
        {
            None,
            Projectile,
            Melee,
            Special
        }

        
    }
}
