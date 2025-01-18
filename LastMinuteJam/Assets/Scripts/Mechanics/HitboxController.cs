using UnityEngine;
using System.Collections;

namespace LastMinuteJam
{
    public class HitboxController : MonoBehaviour
    {
        public PlayerAttack playerAttack;
        public ulong playerId = 0;
        bool moving = true;
        Rigidbody2D rb;

        public void Fire()
        {
            rb = GetComponent<Rigidbody2D>();
            if (playerAttack.type == PlayerAttack.Type.Projectile)
            {
                rb.bodyType = RigidbodyType2D.Dynamic;
                rb.gravityScale = 0; // TODO: Change if allowing bow and arrow? (probably add new type instead)
                rb.linearVelocity = playerAttack.velocity;
            }
            else
            {
                rb.bodyType = RigidbodyType2D.Kinematic;
            }
            StartCoroutine(DestroyDelayed(playerAttack.lifeTime));
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