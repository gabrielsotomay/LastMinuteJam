using System.Collections.Generic;
using UnityEngine;
using Platformer.Gameplay;
using static Platformer.Core.Simulation;
using Platformer.Model;
using Platformer.Core;
using UnityEngine.InputSystem;
using LastMinuteJam;
using Netick;
using Unity.VisualScripting;
using System;


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
        private Collider2D collider2d;
        [SerializeField] AudioSource audioSource;
        [SerializeField] SpriteRenderer spriteRenderer;
        [SerializeField] Animator animator;
        private Transform rayCastBeginPoint;
        private PlayerInput playerInput;
        private Bounds bounds;

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

        // Data
        [SerializeField] PlayerStats playerStats;

        // Prefabs
        [SerializeField] GameObject hitboxPrefab;
        [SerializeField] List<NetworkAttackController> attacks;
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
            collider2d = GetComponent<Collider2D>();
        }

        public override void NetworkStart()
        {
            int attackId = 0;
            foreach (NetworkAttackController attack in attacks)
            {
                attack.Init(this, attackId++);
            }
            spawnPosition = transform.position;
            bounds = collider2d.bounds;

            id = (ulong)UnityEngine.Random.Range(0, 10000);
            direction = Direction.Right;
            // we store the spawn pos so that we use it later during respawn.
            spawnPosition = transform.position;

            Spawn();
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
            if (FetchInput(out FighterInput input))
            {
                movementInput = input.movement;

                UpdateDirection();

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
                    Debug.Log("Attempted Attack at state " + attackState);
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
            
            directionVector = GetDirectionVector(direction);
            RaycastHit2D ray = Sandbox.Physics2D.Raycast(rayCastBeginPoint.transform.position, 
                directionVector, 1f, layerMask);
            
            Debug.DrawRay(rayCastBeginPoint.transform.position, directionVector * 1f, Color.red);
            if (ray.collider != null) 
            {
                ray.collider.GetComponent<CollectableItem>().CollectItem();
            }
            
            
            UpdateJumpState();

            base.NetworkFixedUpdate();

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
            
            jumpState = JumpState.Grounded;
            animator.SetBool("dead", false);
            model.virtualCamera.Follow = transform;
            model.virtualCamera.LookAt = transform;
            Schedule<EnablePlayerInputN>(2f).player = this;
        }




        private void UpdateDirection()
        {
            if (movementInput.x > 0)
            {
                if (Mathf.Abs(movementInput.x) > Mathf.Abs(movementInput.y))
                {
                    direction = Direction.Right;
                }
                else if (movementInput.y > 0)
                {
                    direction = Direction.Up;
                }
                else if (movementInput.y < 0)
                {
                    direction = Direction.Down;
                }
            }
            else if (movementInput.x < 0)
            {
                if (Mathf.Abs(movementInput.x) > Mathf.Abs(movementInput.y))
                {
                    direction = Direction.Left;
                }
                else if (movementInput.y > 0)
                {
                    direction = Direction.Up;
                }
                else if (movementInput.y < 0)
                {
                    direction = Direction.Down;
                }
            }
            else if (movementInput.y < 0)
            {
                direction = Direction.Up;
            }
            else if (movementInput.y > 0)
            {
                direction = Direction.Down;
            }
            
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
            else if (!jumped)
            {
                stopJump = true;
                Schedule<PlayerStopJumpN>().player = this;
            }
        }


        public void OnAttack(AttackTypes type)
        {
            if (!controlEnabled || healthState != HealthState.Normal)
            {
                return;
            }
            if (attackState == AttackState.None)
            { 
                SetAttackState(AttackState.WindUp);
                UpdateAttackAnimations((int)type);
                currentAttack = GetAttackType(type);
                Schedule<WindupFinishedN>(currentAttack.windupTime).player = this;
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
                Attack(currentAttack);
                Schedule<ActiveFinishedN>(currentAttack.activeTime).player = this;
            }
        }
        
        private void Attack(PlayerAttack attack)
        {
            activeAttacks.Add(attack.id);
            NetworkAttackController newAttack;
            int nextAttack = GetNextInactiveAttack();
            if (nextAttack == -1)
            {
                throw new Exception("somehow made an attack while another attack was being made");
            }
            newAttack = attacks[nextAttack];
            activeAttacks.Add(nextAttack);
            if (attack.type == PlayerAttack.Type.Projectile)
            {
                newAttack.FireProjectile(transform.position + new Vector3(attack.position.x, attack.position.y, 0), Quaternion.Euler(0, 0, attack.rotation), attack, hitboxSprites[attack.id]);
            }
            else
            {
                newAttack.FireMelee(transform.position + new Vector3(attack.position.x, attack.position.y, 0), Quaternion.Euler(0, 0, attack.rotation), attack, hitboxSprites[attack.id] );
            }
            // TODO: Select the correct attack


            //hitboxController.playerAttack = attack;
            //hitboxController.Fire(transform.position + new Vector3(attack.position.x, attack.position.y, 0), Quaternion.Euler(0, 0, attack.rotation), attack);
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
                Schedule<AttackFinishedN>(currentAttack.recoverTime).player = this;
            }
        }
        public void OnAttackFinished()
        {
            SetAttackState(AttackState.None);
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
                Schedule<AttackFinishedN>(currentAttack.recoverTime).player = this;
            }
        }

        public void ClearAttack(int id)
        {
            activeAttacks.Remove(id);
        }
        PlayerAttack GetAttackType(AttackTypes attackType)
        {
            // TODO: do this properly
            PlayerAttack attack ;
            if (attackType == AttackTypes.Light)
            {
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
                        Schedule<PlayerJumpedN>().oneShotAudio = new AudioHelper.OneShot(audioSource, jumpAudio);
                        jumpState = JumpState.InFlight;
                    }
                    break;
                case JumpState.InFlight:
                    if (IsGrounded)
                    {
                        Schedule<PlayerLandedN>().player = this;
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
            animator.SetFloat("velocityX", Mathf.Abs(velocity.x) / playerStats.maxSpeed);

            targetVelocity = move * playerStats.maxSpeed;
        }

        private void OnTriggerEnter2D(Collider2D cldr)
        {
            HitboxController hitboxController = cldr.GetComponent<HitboxController>();

            // Check parry
            if (hitboxController == null)
            {
                return;
            }
            if (CheckParry(hitboxController))
            {
                // TODO: Don't get hit and do cool effect
            }
            else if (hitboxController.playerId != id && !impactList.Contains(hitboxController.GetInstanceID()))
            { 
                impactVector = Vector3.Normalize(transform.position - cldr.transform.position);
                TakeHit(hitboxController);
                hitboxController.HitEnemy();
            }
        }

        private bool CheckParry(HitboxController hitboxController)
        {
            return false;
            // TODO : Fix this to make super cool parry combos
            if (
                (attackState == AttackState.WindUp || attackState == AttackState.Active) &&
                hitboxController.playerAttack.id != currentAttack.id) 
            {
                return true;
            }
            return false;
        }


        private void TakeHit(HitboxController hitboxController)
        {
            impactList.Add(hitboxController.GetInstanceID());
            SetHealthState(HealthState.Impacted);
            health.Decrement();
            Schedule<PlayerImpactedN>().player = this;
            ImpactFinishedN impactFinishedEvent = Schedule<ImpactFinishedN>(hitboxController.playerAttack.impactTime);
            impactFinishedEvent.player = this;
            impactFinishedEvent.impactId = ++impactNumber;
            lastAttackTaken = hitboxController.playerAttack;
            controlEnabled = false;

        }

        public void OnImpactFinished(int impactId)
        {
            if (impactNumber != impactId)
            {
                return;
            }
            SetHealthState(HealthState.Disabled);
            Schedule<PlayerDisableN>().player = this;
            Schedule<DisableFinishedN>(lastAttackTaken.disableTime).player = this;
        }

        private void SetHealthState(HealthState newState)
        {
            AnimateHealthState(healthState, false);
            AnimateHealthState(newState, true);
            healthState = newState;

        }
        private void AnimateHealthState(HealthState newState, bool isActive)
        {
            switch (newState)
            {
                case HealthState.Normal:
                    
                    break;
                case HealthState.Disabled:
                    animator.SetBool("disabled", isActive);
                    jumpState = JumpState.InFlight;
                    break;
                case HealthState.Impacted:
                    animator.SetBool("impacted", isActive);
                    if (isActive)
                    {
                        SetAttackState(AttackState.None);
                        animator.SetBool("windup", false);
                        animator.SetBool("active", false);
                        animator.SetBool("recover", false);
                    }
                    break;
                case HealthState.Recovering:
                    animator.SetBool("reenable", isActive);
                    break;
                case HealthState.Dead:
                    animator.SetBool("dead", isActive);
                    break;
                default:
                    break;
            }
        }

        private void SetAttackState(AttackState newState)
        {
            if(newState == AttackState.WindUp)
            {

            }
            AnimateAttackState(attackState, false);
            AnimateAttackState(newState, true);
            attackState = newState;

        }
        private void AnimateAttackState(AttackState newState, bool isActive)
        {
            switch (newState)
            {
                case AttackState.None:

                    break;
                case AttackState.WindUp:
                    animator.SetBool("windup", isActive);
                    break;
                case AttackState.Active:
                    animator.SetBool("active", isActive);
                    break;
                case AttackState.Recovery:
                    animator.SetBool("recover", isActive);
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
            SetHealthState(HealthState.Normal);
            controlEnabled = true;
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
                Schedule<PlayerSpawn>(2);
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