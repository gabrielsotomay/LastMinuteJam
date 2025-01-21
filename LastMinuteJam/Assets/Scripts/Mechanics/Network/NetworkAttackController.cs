using UnityEngine;
using System.Collections;
using Platformer.Mechanics;
using Netick.Unity;
using System.Collections.Generic;

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
        Vector3 awayPosition = new Vector3(100, 100, 0);
        public SpriteRenderer spriteRenderer;
        //private int localId = 0; // Id inside the list of attacks in the player controller
        
        
        private void Awake()
        {
            gravityModifier = 0;
            networkTransform = GetComponent<NetworkTransform>();
            rb = GetComponent<Rigidbody2D>();
            groundNormal = Vector2.up;
            SimpleMovement = true;
        }
        public void Init(NetworkedPlayerController playerController_, int id_)
        {
            playerController = playerController_;
            id = id_;
            networkTransform.Teleport(awayPosition);
        }

        public void FireProjectile(Vector3 spawnPosition, Quaternion spawnRotation, PlayerAttack playerAttack_, Sprite attackSprite)
        {
            playerAttack = playerAttack_;
            networkTransform.Teleport(spawnPosition, spawnRotation);            
            storedVelocity = playerAttack.velocity;
            StartCoroutine(DisableDelayed(playerAttack.lifeTime));


            spriteRenderer.sprite = attackSprite;
            transform.localScale = new Vector3(playerAttack.hitboxScale.x, playerAttack.hitboxScale.y, transform.localScale.z);
            GetComponent<BoxCollider2D>().size = playerAttack.hitboxScale;

        }

        public void FireMelee(Vector3 spawnPosition, Quaternion spawnRotation, PlayerAttack playerAttack_, Sprite attackSprite)
        {
            playerAttack = playerAttack_;
            storedVelocity = Vector2.zero;
            networkTransform.Teleport(spawnPosition, spawnRotation);
            transform.SetParent(playerController.transform, true);
            StartCoroutine(DisableDelayed(playerAttack.lifeTime));
            spriteRenderer.sprite = attackSprite;
            transform.localScale = new Vector3(playerAttack.hitboxScale.x, playerAttack.hitboxScale.y, transform.localScale.z);
            GetComponent<BoxCollider2D>().size = playerAttack.hitboxScale;
        }


        protected override void ComputeVelocity()
        {
            targetVelocity = storedVelocity;
        }

        private IEnumerator DisableDelayed(float timeDelay)
        {
            float lifeTime = 0;
            while (lifeTime <= timeDelay)
            {
                lifeTime += Time.deltaTime;
                yield return null;
            }
            DisableAttack();
        }
        public void HitEnemy()
        {
            DisableAttack();
        }


        private void DisableAttack()
        {
            transform.SetParent(null);
            networkTransform.Teleport(awayPosition);
            playerController.ClearAttack(id);
        }
    }
}