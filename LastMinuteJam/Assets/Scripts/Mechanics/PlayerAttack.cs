using UnityEngine;

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
        [SerializeField] public float baseAttack;
        [SerializeField] public float knockback;
        [SerializeField] public float impactTime;
        [SerializeField] public float disableTime;
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
    }
}
