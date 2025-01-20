using UnityEngine;
using System.Collections;
using Platformer.Mechanics;
using Netick.Unity;

namespace LastMinuteJam
{
    public class NetworkAttackController : NetworkedKinematicObject
    {
        NetworkedPlayerController playerController;
        NetworkTransform networkTransform;

        public PlayerAttack playerAttack = new PlayerAttack();
        public ulong playerId = 0;
        bool moving = true;
        Rigidbody2D rb;
        Vector2 storedVelocity = Vector2.zero;

        public int id = 0;
        private void Awake()
        {
            gravityModifier = 0;
            networkTransform = GetComponent<NetworkTransform>();
        }
        public void Init(NetworkedPlayerController playerController_, int id_)
        {
            playerController = playerController_;
            id = id_;
        }

        public void Fire()
        {
            rb = GetComponent<Rigidbody2D>();
            if (playerAttack.type == PlayerAttack.Type.Projectile)
            {
                storedVelocity = playerAttack.velocity;
            }
            else
            {
                rb.bodyType = RigidbodyType2D.Kinematic;
            }
            StartCoroutine(DestroyDelayed(playerAttack.lifeTime));
        }
        protected virtual void ComputeVelocity()
        {
            targetVelocity = storedVelocity;
        }

        private IEnumerator DestroyDelayed(float timeDelay)
        {
            float lifeTime = 0;
            while (lifeTime <= timeDelay)
            {
                lifeTime += Time.deltaTime;
                yield return null;
            }
            if( gameObject != null)
            {
                Destroy(gameObject);
            }
        }
        public void HitEnemy()
        {
            if (gameObject != null)
            {
                Destroy(gameObject);
            }
        }
    }
}