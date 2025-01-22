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

        public PlayerAttack playerAttack = new PlayerAttack();
        public ulong playerId = 0;
        bool moving = true;
        Rigidbody2D rb;
        Vector2 storedVelocity = Vector2.zero;
        public int id = 0;
        Vector3 awayPosition = new Vector3(100, 100, 0);
        public SpriteRenderer spriteRenderer;
        //private int localId = 0; // Id inside the list of attacks in the player controller
        Coroutine delayDestroy;
        
        private void Awake()
        {
            gravityModifier = 0;
            networkTransform = GetComponent<NetworkTransform>();
            rb = GetComponent<Rigidbody2D>();
            groundNormal = Vector2.up;
            SimpleMovement = true;
        }
        public void Init(NetworkedPlayerController playerController_, int id_, ulong playerId_)
        {
            playerController = playerController_;
            id = id_;
            playerId = playerId_;
            Teleport(awayPosition);
        }

        public void FireProjectile(Vector3 spawnPosition, Quaternion spawnRotation, PlayerAttack playerAttack_, Sprite attackSprite)
        {
            GetComponent<BoxCollider2D>().isTrigger = true;
            playerAttack = playerAttack_;
            transform.SetParent(null);
            Teleport(spawnPosition, spawnRotation);            
            storedVelocity = playerAttack.velocity;
            delayDestroy = StartCoroutine(DisableDelayed(playerAttack.lifeTime));


            spriteRenderer.sprite = attackSprite;
            transform.localScale = new Vector3(playerAttack.hitboxScale.x, playerAttack.hitboxScale.y, transform.localScale.z);
            GetComponent<BoxCollider2D>().size = new Vector2(Mathf.Abs(playerAttack.hitboxScale.x), Mathf.Abs(playerAttack.hitboxScale.y));

        }

        public void FireMelee(Vector3 spawnPosition, Quaternion spawnRotation, PlayerAttack playerAttack_, Sprite attackSprite)
        {
            GetComponent<BoxCollider2D>().isTrigger = true;
            playerAttack = playerAttack_;
            storedVelocity = Vector2.zero;
            Teleport(spawnPosition, spawnRotation);
            transform.SetParent(playerController.transform, true);
            delayDestroy = StartCoroutine(DisableDelayed(playerAttack.lifeTime));
            spriteRenderer.sprite = attackSprite;
            transform.localScale = new Vector3(playerAttack.hitboxScale.x, playerAttack.hitboxScale.y, transform.localScale.z);
            GetComponent<BoxCollider2D>().size = new Vector2(Mathf.Abs(playerAttack.hitboxScale.x), Mathf.Abs(playerAttack.hitboxScale.y));
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
            if (delayDestroy != null)
            {
                StopCoroutine(delayDestroy);
            }
        }


        private void DisableAttack()
        {
            transform.SetParent(null);
            Teleport(awayPosition);
            playerController.ClearAttack(id);
            GetComponent<BoxCollider2D>().isTrigger = false;
        }


        public static NetworkAttackController FindAttackByNetworkId(int networkId)
        {
            NetworkAttackController[] attacks = FindObjectsByType<NetworkAttackController>(FindObjectsInactive.Exclude, FindObjectsSortMode.InstanceID);
            foreach (NetworkAttackController attack in attacks)
            {
                Debug.Log("Found attack of id " + attack.GetComponent<NetworkObject>().Id + ", looking for " + networkId);
                if (networkId == attack.GetComponent<NetworkObject>().Id)
                {
                    Debug.Log("Found matching attack");
                    return attack;
                }
            }
            return null;
        }
    }
}