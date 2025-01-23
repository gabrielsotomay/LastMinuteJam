using System.Collections.Generic;
using UnityEngine;
using Platformer.Gameplay;
using static Platformer.Core.SimulationNetick;
using Platformer.Model;
using Platformer.Core;
using UnityEngine.InputSystem;
using LastMinuteJam;
using Netick;
using Unity.VisualScripting;
using System;
using UnityEngine.Windows;
using Netick.Unity;
using System.Collections;

namespace Platformer.Mechanics
{
    /// <summary>
    /// This is the main class used to implement control of the player.
    /// It is a superset of the AnimationController class, but is inlined to allow for any kind of customisation.
    /// </summary>
    public class NetworkedPlayerController : NetworkedKinematicObject
    {
        // Audio
        public AudioClip jumpAudio;
        public AudioClip respawnAudio;
        public AudioClip ouchAudio;

        // Components
        private BoxCollider2D collider2d;
        [SerializeField] AudioSource audioSource;
        [SerializeField] SpriteRenderer spriteRenderer;
        [SerializeField] Animator animator;
        private Transform rayCastBeginPoint;
        private PlayerInput playerInput;
        private Bounds bounds;

        List<NetworkAttackController> allAttacks = new ();


        // Other local scripts
        public Health health;
        [SerializeField] private ComboInput comboInput;
        
        public JumpState jumpState = JumpState.Grounded;
        public AttackState attackState = AttackState.None;
        public HealthState healthState = HealthState.Normal;
        public Direction direction = Direction.None;

        // Variables
        private bool stopJump = false;
        public bool controlEnabled = true;
        bool jump = false;
        Vector2 move = Vector2.zero;
        public Vector2 movementInput = Vector2.zero;
        public int difficulty;
        private Vector2 directionVector = Vector2.zero;
        private float rayCastX = 0.3f;
        private float rayCastY = -0.4f;
        readonly PlatformerModel model = GetModel<PlatformerModel>();
        private int layerMask = 1 << 6; // only check for layer 6
        private bool flagVariable = false;

        [SerializeField]
        GameObject attackPrefab;
        // Data
        [SerializeField] PlayerStats playerStats;

        // Prefabs
        [SerializeField] GameObject hitboxPrefab;
        [SerializeField] List<NetworkAttackController> attacks = new ();
        List<int> activeAttacks = new List<int>(); // list of attacks in "attacks" array that are active

        // Attacks
        [SerializeField] PlayerAttackTypes playerAttackTypes; // TODO: Give this to character on setup
        List<PlayerAttack> playerAttacks;
        PlayerAttack currentAttack;
        GameObject attackHitbox;
        int attackNumber = 0;
        int impactNumber = -1;
        bool impactFinished = false;
        PlayerAttack lastAttackTaken;
        Vector2 impactVector;
        List<int> impactList = new List<int>();
        
        // Networked things?!


        // Attack animations
        [SerializeField] List<AnimationClip> attackAnimations = new List<AnimationClip>();
        [SerializeField] List<Sprite> hitboxSprites = new List<Sprite>();

        // TODO: input queue
        // private Queue<InputAction> actionQueue = new Queue<InputAction>();

        // Player info
        public bool isPlayer1 = false;
        ulong id;

        public Vector3 spawnPosition;

        [Networked]
        public FighterInput MyInput
        {
            get; set;
        }

        NetworkBool inputAvailabile = false;

        public enum JumpState
        {
            Grounded,
            PrepareToJump,
            Jumping,
            InFlight,
            Landed
        }

        public enum AttackState
        {
            None,
            WindUp,
            Active,
            Recovery
        }

        public enum HealthState
        {
            Normal,
            Impacted,
            Disabled,
            Recovering,
            Dead
        }

        public enum Direction
        {
            Left,
            Right,
            Up,
            Down,
            None
        }

        public enum AttackTypes
        {
            Light,
            Heavy,
            Special,
            None
        }
        private void Awake()
        {
            health = GetComponent<Health>();
            collider2d = GetComponent<BoxCollider2D>();
            
        }
        
        public override void NetworkStart()
        {
            id = (ulong)UnityEngine.Random.Range(0, 10000);
            int attackId = 0;
            
            foreach (NetworkAttackController attack in attacks)
            {
                attack.Init(this, attackId++, id);
            }
            spawnPosition = transform.position;
            bounds = collider2d.bounds;

            direction = Direction.Right;
            // we store the spawn pos so that we use it later during respawn.
            spawnPosition = transform.position;
            SetAttackState(AttackState.None);
            SetHealthState(HealthState.Normal);
            lastAttackTaken = new PlayerAttack(false);
            Spawn();
            model.UIcontroller.PrepareGame();
            base.NetworkStart();
        }
        public override void OnInputSourceLeft()
        {
            Sandbox.GetComponent<JamGameEventHandler>().KillPlayer(this);
            // destroy the player object when its input source (controller player) leaves the game.
            Sandbox.Destroy(Object);
        }


        public override void NetworkFixedUpdate()
        {
            FighterInput input = new FighterInput();
            if (FetchInput(out FighterInput frameInput))
            {
                input = frameInput;
                if (IsServer && isPlayer1)
                {
                    SetInputRpc(input);
                }
                inputAvailabile = true;
            }
            else if(InputSource == null && inputAvailabile)
            {
                input = MyInput;
            } 
            else if (InputSource != null)
            {
                ResetInputRpc();
                MyInput = new FighterInput();
                inputAvailabile = false;
            }
            /*
            if (IsServer)
            {
                bounds = collider2d.bounds;
                foreach ( NetworkAttackController attack in allAttacks)
                {
                    CheckAttackIntersection(attack);
                }
            }
            */

            if (inputAvailabile)
            {
                movementInput = input.movement;

                direction = UpdateDirection(movementInput, direction);

                if (input.jumpPress )
                {
                    OnJump(true);
                }
                else if (input.jumpRelease)
                {
                    OnJump(false);
                }

                // Attacks

                if (input.lightAttack)
                {
                    OnAttack(AttackTypes.Light);                    
                }
                if (input.heavyAttack )
                {
                    OnAttack(AttackTypes.Heavy);
                }
            }
            if (controlEnabled)
            {
                // TODO: change direction based on movement
                if (movementInput.x > 0)
                {
                    move.x = playerStats.moveSpeed;
                }
                else if (movementInput.x < 0)
                {
                    move.x = -playerStats.moveSpeed;
                }
                else
                {
                    move.x = 0;
                }
            }
            else
            {
                move.x = 0;
            }
            if (IsServer)
            {
                directionVector = GetDirectionVector(direction);
                RaycastHit2D ray = Sandbox.Physics2D.Raycast(rayCastBeginPoint.transform.position,
                    directionVector, 1f, layerMask);

                if (ray.collider != null && !ray.collider.IsDestroyed())
                {
                    ray.collider.GetComponent<CollectableItem>()?.CollectItem(InputSource);
                }
            }
            
            
            
            UpdateJumpState();
            CheckTickActions();
            base.NetworkFixedUpdate();

        }

        public void CheckTickActions()
        {

        }
        [Rpc(target: RpcPeers.Everyone)]
        public void SetInputRpc(FighterInput input)        
        {
            
            MyInput = input;
            inputAvailabile = true;
        }
        [Rpc(target:  RpcPeers.Everyone)]
        public void ResetInputRpc()
        {
            
            MyInput = new FighterInput();
            inputAvailabile = false;
        }

        public void Respawn()
        {
            
            Sandbox.GetComponent<JamGameEventHandler>().RespawnPlayer(this);

            transform.position = spawnPosition;
            
        }
        void Setup()
        {
            // TODO : Setup all player data
        }
        
        public void Spawn()
        {
            collider2d.enabled = true;
            controlEnabled = false;
            if (audioSource && respawnAudio)
                audioSource.PlayOneShot(respawnAudio);
            health.Increment();
            //Teleport(model.spawnPoint.transform.position);

            rayCastBeginPoint = transform.Find("RayCast");
            Debug.Log(rayCastBeginPoint.transform.position);
            
            jumpState = JumpState.Grounded;
            animator.SetBool("dead", false);
            /*
            model.virtualCamera.Follow = transform;
            model.virtualCamera.LookAt = transform;
            */
            Schedule<EnablePlayerInputN>(Sandbox.Tick.TickValue, (int)(2/Sandbox.FixedDeltaTime)).player = this;
        }




        public static Direction UpdateDirection(Vector2 movement, Direction oldDirection)
        {
            Direction newDirection = oldDirection;
            if (movement.x > 0)
            {
                if (Mathf.Abs(movement.x) > Mathf.Abs(movement.y))
                {
                    newDirection = Direction.Right;
                }
                else if (movement.y > 0)
                {
                    newDirection = Direction.Up;
                }
                else if (movement.y < 0)
                {
                    newDirection = Direction.Down;
                }
            }
            else if (movement.x < 0)
            {
                if (Mathf.Abs(movement.x) > Mathf.Abs(movement.y))
                {
                    newDirection = Direction.Left;
                }
                else if (movement.y > 0)
                {
                    newDirection = Direction.Up;
                }
                else if (movement.y < 0)
                {
                    newDirection = Direction.Down;
                }
            }
            else if (movement.y < 0)
            {
                newDirection = Direction.Up;
            }
            else if (movement.y > 0)
            {
                newDirection = Direction.Down;
            }
            return newDirection;
            
        }


        private void OnJump(bool jumped)
        {

            if (!controlEnabled)
            {
                return;
            }
            if (jumpState == JumpState.Grounded && jumped)
            {
                jumpState = JumpState.PrepareToJump;
            }
            else if (!jumped && jumpState == JumpState.Jumping)
            {
                stopJump = true;
                Schedule<PlayerStopJumpN>(Sandbox.Tick.TickValue).player = this;
            }
        }


        public void OnAttack(AttackTypes type)
        {
            if (!controlEnabled || healthState != HealthState.Normal)
            {
                return;
            }
            if (attackState == AttackState.None && attacks.Count <= 4)
            { 
                SetAttackState(AttackState.WindUp);
                UpdateAttackAnimations((int)type);
                currentAttack = GetAttackType(type);
                Schedule<WindupFinishedN>(Sandbox.Tick.TickValue, (int)(currentAttack.windupTime/Sandbox.FixedDeltaTime)).player = this;
            }
        }




        private void UpdateAttackAnimations(int id)
        {
            AnimatorOverrideController animatorOverrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
            if (id == 0)
            {
                // 0,1,2 windup, active, recovery for basic, next for heavy attack
                animatorOverrideController[attackAnimations[0]] = attackAnimations[0];
                animatorOverrideController[attackAnimations[1]] = attackAnimations[1];
                animatorOverrideController[attackAnimations[2]] = attackAnimations[2];
                animatorOverrideController[attackAnimations[3]] = attackAnimations[0];
                animatorOverrideController[attackAnimations[4]] = attackAnimations[1];
                animatorOverrideController[attackAnimations[5]] = attackAnimations[2];

            }
            else if (id == 1)
            {
                animatorOverrideController[attackAnimations[0]] = attackAnimations[3];
                animatorOverrideController[attackAnimations[1]] = attackAnimations[4];
                animatorOverrideController[attackAnimations[2]] = attackAnimations[5];
                animatorOverrideController[attackAnimations[3]] = attackAnimations[3];
                animatorOverrideController[attackAnimations[4]] = attackAnimations[4];
                animatorOverrideController[attackAnimations[5]] = attackAnimations[5];
            }
            animator.runtimeAnimatorController = animatorOverrideController;
        }


        public void OnWindupFinished()
        {
            if (attackState == AttackState.WindUp)
            {
                SetAttackState(AttackState.Active);
                if (IsServer)
                {
                    Attack(currentAttack);
                }
                Schedule<ActiveFinishedN>(Sandbox.Tick.TickValue, (int)(currentAttack.activeTime/Sandbox.FixedDeltaTime)).player = this;
            }
        }
        
        private void Attack(PlayerAttack attack)
        {
            //activeAttacks.Add(attack.id);


            Vector3 attackPosition = transform.position + new Vector3(attack.position.x, attack.position.y, 0);
            Quaternion attackRotation = Quaternion.Euler(0, 0, attack.rotation);

            int attackId = Sandbox.NetworkInstantiate(attackPrefab,attackPosition, attackRotation).Id;
            FireAttackRpc(attack.id, attackId);
            if (attacks.Count >= 5)
            {
                throw new Exception("somehow made an attack while another attack was being made");
            }
            // TODO: Select the correct attack


            //hitboxController.playerAttack = attack;
            //hitboxController.Fire(transform.position + new Vector3(attack.position.x, attack.position.y, 0), Quaternion.Euler(0, 0, attack.rotation), attack);
        }

        [Rpc(target: RpcPeers.Everyone, localInvoke:true)]
        public void FireAttackRpc(int type, int networkId)
        {
            NetworkAttackController newAttack = NetworkAttackController.FindAttackByNetworkId(networkId);
            PlayerAttack attack = GetAttackType((AttackTypes)type);
            newAttack.Init(this, attacks.Count, id);
            if (attack.type == PlayerAttack.Type.Projectile)
            {
                newAttack.FireProjectile( attack, hitboxSprites[attack.id]);
            }
            else
            {
                newAttack.FireMelee(attack, hitboxSprites[attack.id]);
            }
            attacks.Add(newAttack);
            activeAttacks.Add(attacks.Count-1);
        }
        private int GetNextInactiveAttack()
        {
            // returns -1 if all attacks are used
            int nextAttack = 0;
            while (activeAttacks.Contains(nextAttack))
            {
                nextAttack++;
            }
            if (nextAttack >= attacks.Count)
            {
                return -1;
            }
            return nextAttack;
        }



        public void OnActiveFinished()
        {
            if (attackState == AttackState.Active)
            {
                SetAttackState(AttackState.Recovery);
                EndActiveAttack();
                Schedule<RecoveryFinishedN>(Sandbox.Tick.TickValue, (int)(currentAttack.recoverTime/Sandbox.FixedDeltaTime)).player = this;
            }
        }
        public void OnAttackFinished()
        {
            //SetAttackState(AttackState.None);
        }
        private void EndActiveAttack()
        {
            attackHitbox = null;
        }
        public void OnRecoveryFinished()
        {
            if (attackState == AttackState.Recovery)
            {
                SetAttackState(AttackState.None);
                Schedule<AttackFinishedN>(Sandbox.Tick.TickValue).player = this;
            }
        }

        public void ClearAttack(int id)
        {

            activeAttacks.Remove(id);
            NetworkAttackController attackToRemove = null;
            foreach (NetworkAttackController attack in attacks)
            {
                if(attack != null && attack.id == id)
                {
                    attackToRemove = attack;
                }

            }
            if (attackToRemove != null)
            {
                attacks.Remove(attackToRemove);
            }
        }
        PlayerAttack GetAttackType(AttackTypes attackType)
        {
            // TODO: do this properly
            PlayerAttack attack ;
            if (attackType == AttackTypes.Light)
            {
                Debug.Log("Got basic attack");
                attack = playerAttackTypes.basicAttack;
            }
            else if (attackType == AttackTypes.Heavy)
            {
                attack = playerAttackTypes.heavyAttack;
            }
            else
            {
                attack = playerAttackTypes.basicAttack;
            }
            attack = SetAttackDirection(attack);
            attack.SetInstanceId(attackNumber++);
            return attack;
        }

        private PlayerAttack SetAttackDirection(PlayerAttack attack)
        {
            switch (direction)
            {
                case Direction.Left:
                    attack.position.x = -attack.position.x;
                    attack.hitboxScale.x = -attack.hitboxScale.x;
                    attack.velocity.x = -attack.velocity.x;
                    break;
                case Direction.None:
                case Direction.Right:
                    break;
                case Direction.Up:
                    attack.position.y = attack.position.x;
                    attack.velocity.y = attack.velocity.x;
                    attack.velocity.x = 0;
                    attack.rotation = attack.rotation + 90;
                    break;
                default:
                    break;
            }
            return attack;
        }


        void UpdateJumpState()
        {
            // In a frame, has to happen before ComputeVelocity for jump to work
            jump = false;
            switch (jumpState)
            {
                case JumpState.PrepareToJump:
                    jumpState = JumpState.Jumping;
                    jump = true;
                    stopJump = false;
                    break;
                case JumpState.Jumping:
                    if (!IsGrounded)
                    {
                        Schedule<PlayerJumpedN>(Sandbox.Tick.TickValue).oneShotAudio = new AudioHelper.OneShot(audioSource, jumpAudio);
                        jumpState = JumpState.InFlight;
                    }
                    break;
                case JumpState.InFlight:
                    if (IsGrounded)
                    {
                        Schedule<PlayerLandedN>(Sandbox.Tick.TickValue).player = this;
                        jumpState = JumpState.Landed;
                    }
                    break;
                case JumpState.Landed:
                    jumpState = JumpState.Grounded;
                    break;
            }
        }

        protected override void ComputeVelocity()
        {
            
            if (healthState == HealthState.Normal)
            {
                if (jump && IsGrounded)
                {
                    velocity.y = playerStats.jumpSpeed * model.jumpModifier;
                    jump = false;
                }
                else if (stopJump)
                {
                    stopJump = false;
                    if (velocity.y > 0)
                    {
                        velocity.y = velocity.y * model.jumpDeceleration;
                    }
                }
            }
            else if (impactFinished)
            {
                impactFinished = false;
                velocity.y = lastAttackTaken.knockback * impactVector.x * playerStats.knockbackModifier;
                velocity.x = lastAttackTaken.knockback * impactVector.y * playerStats.knockbackModifier;
            }

            if (attackState == AttackState.None)
            {
                if (direction == Direction.Right)
                {
                    spriteRenderer.flipX = false;
                    
                }
                else if (direction == Direction.Left)
                {
                    spriteRenderer.flipX = true;
                }
            }
            


            animator.SetBool("grounded", IsGrounded);
            if (InputSource != null)
            {
                animator.SetFloat("velocityX", Mathf.Abs(velocity.x) / playerStats.maxSpeed);
                targetVelocity = move * playerStats.maxSpeed;
            }
            else
            {
                animator.SetFloat("velocityX", Mathf.Abs((move * playerStats.maxSpeed).x) / playerStats.maxSpeed);
                targetVelocity = move * playerStats.maxSpeed;

            }

        }
        /*
        public void CheckAttackIntersection(NetworkAttackController attack)
        {

            Bounds otherBounds = attack.GetComponent<BoxCollider2D>().bounds;
            otherBounds.SetMinMax(new Vector3(otherBounds.min.x, otherBounds.min.y, -0.5f), new Vector3(otherBounds.max.x, otherBounds.max.y, 0.5f));
            if (bounds.Intersects(otherBounds))
            {
                Debug.Log("Player " + id + " + got hit by player " + attack.playerId + " with attack" + attack.id);
                if (CheckParry(attack))
                {
                    // TODO: Don't get hit and do cool effect
                }
                else if (attack.playerId != id && !impactList.Contains(attack.GetInstanceID()))
                {
                    impactVector = Vector3.Normalize(transform.position - attack.transform.position);
                    TakeHitRpc(attack.GetComponent<NetworkObject>().Id, impactVector); ;
                }
            }
            if(UnityEngine.Input.GetKeyDown(KeyCode.G))
            {
gg                Debug.Log("Analysing");
            }
        }
        */
        
        private void OnTriggerEnter2D(Collider2D cldr)
        {
            

            NetworkAttackController attackController = cldr.GetComponent<NetworkAttackController>();

            // Check parry
            if (attackController == null || !IsServer || attackController.playerId == 0)
            {
                //Debug.Log("Player" + id + " entered non-atack trigger");
                return;
            }
            Debug.Log("Player " + id + " + got hit by player " + attackController.playerId + " with attack" + attackController.id);
            if (CheckParry(attackController))
            {
                // TODO: Don't get hit and do cool effect
            }
            else if (attackController.playerId != id && !impactList.Contains(attackController.GetInstanceID()))
            { 
                impactVector = Vector3.Normalize(transform.position - cldr.transform.position);
                TakeHitRpc(attackController.GetComponent<NetworkObject>().Id, impactVector);;
            }
            return;
        }
        
        private bool CheckParry(NetworkAttackController attackController)
        {
            return false;
            // TODO : Fix this to make super cool parry combos
            if (
                (attackState == AttackState.WindUp || attackState == AttackState.Active) &&
                attackController.playerAttack.id != currentAttack.id) 
            {
                return true;
            }
            return false;
        }



        [Rpc(target: RpcPeers.Everyone, localInvoke: true)]
        public void InitAttacksRpc(PlayerAttack.AttackIdsSent attackIds)
        {
            foreach (int attackId in attackIds.Attacks)
            {
                allAttacks.Add(NetworkAttackController.FindAttackByNetworkId(attackId));
            }
        }

        public List<int> GetAttackNetworkIds()
        {
            List<int> ids = new();
            foreach (NetworkAttackController attackController in attacks)
            {
                ids.Add(attackController.GetComponent<NetworkObject>().Id);
            }
            return ids;
        }


        [Rpc(target: RpcPeers.Everyone, localInvoke: true)]
        public void TakeHitRpc(int attackId, Vector3 impactVector_)
        {
            impactVector = impactVector_;
            TakeHitN hitEvent = Schedule<TakeHitN>(Sandbox.Tick.TickValue);
            hitEvent.player = this;
            hitEvent.attackController = NetworkAttackController.FindAttackByNetworkId(attackId);
            //hitEvent.attackController = attacks[attackId];

        }

        public void TakeHit(NetworkAttackController attackController)
        {
            impactList.Add(attackController.GetInstanceID());
            SetHealthState(HealthState.Impacted);
            health.Decrement();
            Schedule<PlayerImpactedN>(Sandbox.Tick.TickValue).player = this;
            ImpactFinishedN impactFinishedEvent = Schedule<ImpactFinishedN>(Sandbox.Tick.TickValue, (int)(attackController.playerAttack.impactTime/Sandbox.FixedDeltaTime));
            impactFinishedEvent.player = this;
            impactFinishedEvent.impactId = ++impactNumber;
            if (lastAttackTaken.id == -1)
            {
                lastAttackTaken = attackController.playerAttack;
            }
            controlEnabled = false;
            attackController.HitEnemy();
        }
        


        public void OnImpactFinished(int impactId)
        {
            if (impactNumber != impactId)
            {
                return;
            }
            SetHealthState(HealthState.Disabled);
            Schedule<PlayerDisableN>(Sandbox.Tick.TickValue).player = this;
            Schedule<DisableFinishedN>(Sandbox.Tick.TickValue, (int)(lastAttackTaken.disableTime/Sandbox.FixedDeltaTime)).player = this;
        }

        private void SetHealthState(HealthState newState)
        {
            AnimateHealthState(newState);
            healthState = newState;

        }
        private void AnimateHealthState(HealthState newState)
        {
            switch (newState)
            {
                case HealthState.Normal:
                    animator.SetBool("normal", true);
                    animator.SetBool("disabled", false);
                    animator.SetBool("impacted", false);
                    animator.SetBool("reenable", false);
                    animator.SetBool("dead", false);
                    break;
                case HealthState.Disabled:
                    animator.SetBool("normal", false);
                    animator.SetBool("disabled", true);
                    animator.SetBool("impacted", false);
                    animator.SetBool("reenable", false);
                    animator.SetBool("dead", false);
                    SetAttackState(AttackState.None);
                    jumpState = JumpState.InFlight;
                    break;
                case HealthState.Impacted:
                    animator.SetBool("normal", false);
                    animator.SetBool("disabled", false);
                    animator.SetBool("impacted", true);
                    animator.SetBool("reenable", false);
                    animator.SetBool("dead", false);
                    SetAttackState(AttackState.None);
                    break;
                case HealthState.Recovering:
                    animator.SetBool("normal", false);
                    animator.SetBool("disabled", false);
                    animator.SetBool("impacted", false);
                    animator.SetBool("reenable", true);
                    animator.SetBool("dead", false);
                    SetAttackState(AttackState.None);
                    break;
                case HealthState.Dead:
                    animator.SetBool("normal", false);
                    animator.SetBool("disabled", false);
                    animator.SetBool("impacted", false);
                    animator.SetBool("reenable", false);
                    animator.SetBool("dead", true);
                    break;
                default:
                    break;
            }
        }

        private void SetAttackState(AttackState newState)
        {
            
            AnimateAttackState(newState);
            attackState = newState;

        }
        private void AnimateAttackState(AttackState newState)
        {
            switch (newState)
            {
                case AttackState.None:
                    animator.SetBool("none", true);
                    animator.SetBool("windup", false);
                    animator.SetBool("active", false);
                    animator.SetBool("recover", false); 
                    
                    break;
                case AttackState.WindUp:
                    animator.SetBool("none", false);
                    animator.SetBool("windup", true);
                    animator.SetBool("active", false);
                    animator.SetBool("recover", false);
                    break;
                case AttackState.Active:
                    animator.SetBool("none", false);
                    animator.SetBool("windup", false);
                    animator.SetBool("active", true);
                    animator.SetBool("recover", false);
                    break;
                case AttackState.Recovery:
                    animator.SetBool("none", false);
                    animator.SetBool("windup", false);
                    animator.SetBool("active", false);
                    animator.SetBool("recover", true);
                    break;
                default:
                    break;
            }
        }

        public void OnDisableFinished()
        {
            if (healthState != HealthState.Disabled)
            {
                return;
            }
            SetHealthState(HealthState.Recovering);
            Schedule<ReenableFinishedN>(Sandbox.Tick.TickValue).player = this;
            controlEnabled = true;
        }

        public void OnReenableFinish()
        {
            SetHealthState(HealthState.Normal);
            lastAttackTaken = new PlayerAttack(false);
        }
        

        public void OnDeath()
        {
            if (health.IsAlive)
            {
                health.Die();

                model.virtualCamera.Follow = null;
                model.virtualCamera.LookAt = null;
                // player.collider.enabled = false;
                controlEnabled = false;
                
                if (audioSource && ouchAudio)
                {
                    audioSource.PlayOneShot(ouchAudio);
                }
                animator.SetTrigger("hurt");
                animator.SetBool("dead", true);
                Schedule<PlayerSpawnN>(Sandbox.Tick.TickValue, (int)(2/Sandbox.FixedDeltaTime));
            }
        }



        private Vector2 GetDirectionVector(Direction direction)
        {
            switch (direction)
            {
                case Direction.Left:
                    rayCastBeginPoint.transform.localPosition = new Vector3(-rayCastX, rayCastY, 0);
                    return Vector2.left;
                case Direction.Right:
                    rayCastBeginPoint.transform.localPosition = new Vector3(rayCastX, rayCastY, 0);
                    return Vector2.right;
                default:
                    return Vector2.zero;
            }
        }
    }
}