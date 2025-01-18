using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Platformer.Gameplay;
using static Platformer.Core.Simulation;
using Platformer.Model;
using Platformer.Core;
using UnityEngine.InputSystem;
using LastMinuteJam;
using UnityEngine.Splines;

namespace Platformer.Mechanics
{
    /// <summary>
    /// This is the main class used to implement control of the player.
    /// It is a superset of the AnimationController class, but is inlined to allow for any kind of customisation.
    /// </summary>
    public class PlayerController : KinematicObject
    {
        // Audio
        public AudioClip jumpAudio;
        public AudioClip respawnAudio;
        public AudioClip ouchAudio;

        // Components
        private Collider2D collider2d;
        private AudioSource audioSource;
        private SpriteRenderer spriteRenderer;
        internal Animator animator;
        private PlayerInput playerInput;
        private Bounds bounds;

        // Other local scripts
        public Health health;

        public JumpState jumpState = JumpState.Grounded;
        public AttackState attackState = AttackState.None;
        public HealthState healthState = HealthState.Normal;
        public Direction direction = Direction.None;

        // Variables
        private bool stopJump;
        public bool controlEnabled = true;
        bool jump;
        Vector2 move;
        private Vector2 movementInput = Vector2.zero;


        readonly PlatformerModel model = GetModel<PlatformerModel>();
        
        // Inputs
        private InputAction moveAction;
        private InputAction jumpAction;
        private InputAction basicAttackAction;
        private InputAction heavyAttackAction;

        // Data
        [SerializeField] PlayerStats playerStats;

        // Prefabs
        [SerializeField] GameObject hitboxPrefab;

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

        // TODO: input queue
        // private Queue<InputAction> actionQueue = new Queue<InputAction>();

        // Player info
        public bool isPlayer1 = false;
        ulong id;



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

        void Awake()
        {
            health = GetComponent<Health>();
            audioSource = GetComponent<AudioSource>();
            collider2d = GetComponent<Collider2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();
            //playerInputs = new PlayerInputs();
            playerInput = GetComponent<PlayerInput>();
            bounds = collider2d.bounds;
            /*
            if (isPlayer1)
            {
                playerInput.SwitchCurrentControlScheme("KeyboardPlayer1");
            }
            else
            {
                playerInput.SwitchCurrentControlScheme("KeyboardPlayer2");

            }
            */
            Debug.Log("Awoke player ");
            // Player inputs

            basicAttackAction = playerInput.actions["BasicAttack"];
            basicAttackAction.Enable();
            heavyAttackAction = playerInput.actions["HeavyAttack"];
            // TODO : heavyAttackAction.performed += OnHeavyAttack;
            jumpAction = playerInput.actions["Jump"];
            jumpAction.Enable();
            moveAction = playerInput.actions["Movement"];
            moveAction.Enable();

            id = (ulong)Random.Range(0,10000);
            direction = Direction.Right; // TODO: Change so facing left for right character at start?
        }
        
        
        protected override void OnEnable()
        {
            base.OnEnable();
            Debug.Log("Ran enable");
            basicAttackAction.performed += OnBasicAttack;
            jumpAction.performed += OnJump;
            moveAction.performed += OnMove;

            Spawn();
            /*
            playerInputs.Enable();
            playerInputs.Basic.BasicAttack.performed += OnBasicAttack;
            playerInputs.Basic.LookLeft.performed += OnLookLeft;
            playerInputs.Basic.LookRight.performed += OnLookRight;
            playerInputs.Basic.LookUp.performed += OnLookUp;
            playerInputs.Basic.Jump.performed += OnJump;
            */
        }
        protected override void OnDisable() 
            {
            base.OnDisable();
            basicAttackAction.performed -= OnBasicAttack;
            jumpAction.performed -= OnJump;
            moveAction.performed -= OnMove;
            /*
            playerInputs.Disable();
            playerInputs.Basic.BasicAttack.performed -= OnBasicAttack;
            playerInputs.Basic.LookLeft.performed -= OnLookLeft;
            playerInputs.Basic.LookRight.performed -= OnLookRight;
            playerInputs.Basic.LookUp.performed -= OnLookUp;
            playerInputs.Basic.Jump.performed -= OnJump;
            */
        }

        public void Spawn()
        {
            collider2d.enabled = true;
            controlEnabled = false;
            if (audioSource && respawnAudio)
                audioSource.PlayOneShot(respawnAudio);
            health.Increment();
            Teleport(model.spawnPoint.transform.position);
            jumpState = PlayerController.JumpState.Grounded;
            animator.SetBool("dead", false);
            model.virtualCamera.Follow = transform;
            model.virtualCamera.LookAt = transform;
            Simulation.Schedule<EnablePlayerInput>(2f).player = this;
        }

        protected override void Update()
        {
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
                    move.x = 0;

            }
            else
            {
                move.x = 0;
            }
            UpdateJumpState();
            base.Update();
        }

        private void OnMove(InputAction.CallbackContext context)
        {
            movementInput = context.ReadValue<Vector2>();
            UpdateDirection();
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


        private void OnJump(InputAction.CallbackContext context)
        {

            if (!controlEnabled)
            {
                return;
            }
            if (jumpState == JumpState.Grounded && context.action.triggered)
            {
                jumpState = JumpState.PrepareToJump;
            }
            else if (!context.action.triggered)
            {
                stopJump = true;
                Schedule<PlayerStopJump>().player = this;
            }
        }


        public void OnBasicAttack(InputAction.CallbackContext context)
        {
            if (!controlEnabled || healthState != HealthState.Normal)
            {
                return;
            }
            if (attackState == AttackState.None)
            {
                SetAttackState(AttackState.WindUp);
                currentAttack = GetBasicAttackType();
                Schedule<WindupFinished>(currentAttack.windupTime).player = this;
            }
        }

        public void OnWindupFinished()
        {
            if (attackState == AttackState.WindUp)
            {
                SetAttackState(AttackState.Active);
                Attack(currentAttack);
                Schedule<ActiveFinished>(currentAttack.activeTime).player = this;
            }
        }
        
        private void Attack(PlayerAttack attack)
        {
            attackHitbox = Instantiate(hitboxPrefab, transform.position + new Vector3(attack.position.x, attack.position.y, 0), Quaternion.Euler(0,0,attack.rotation), transform);
            attackHitbox.transform.localScale = new Vector3(attack.hitboxScale.x, attack.hitboxScale.y, attackHitbox.transform.localScale.z);
            HitboxController hitboxController = attackHitbox.GetComponent<HitboxController>();
            hitboxController.playerAttack = attack;
            hitboxController.playerId = id;
        }


        public void OnActiveFinished()
        {
            if (attackState == AttackState.Active)
            {
                SetAttackState(AttackState.Recovery);
                EndActiveAttack();
                Schedule<AttackFinished>(currentAttack.recoverTime).player = this;
            }
        }
        public void OnAttackFinished()
        {
            SetAttackState(AttackState.None);
        }
        private void EndActiveAttack()
        {
            if (attackHitbox != null)
            {
                Destroy(attackHitbox);
            }
        }
        public void OnRecoveryFinished()
        {
            if (attackState == AttackState.Recovery)
            {
                attackState = AttackState.None;
                Schedule<AttackFinished>(currentAttack.recoverTime).player = this;
            }
        }

        PlayerAttack GetBasicAttackType()
        {
            // TODO: do this properly
            PlayerAttack attack = playerAttackTypes.basicAttack;
            switch (direction)
            {
                case Direction.Left:
                    attack.position.x = -attack.position.x;
                    attack.hitboxScale.x = -attack.hitboxScale.x;
                    break;
                case Direction.None:
                case Direction.Right:
                    break;
                case Direction.Up:
                    attack.position.y = attack.position.x;
                    attack.rotation = attack.rotation + 90;
                    break;
                default:
                    break;
            }
            attack.SetInstanceId(attackNumber++);
            return attack;
        }

        void UpdateJumpState()
        {
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
                        Schedule<PlayerJumped>().oneShotAudio = new AudioHelper.OneShot(audioSource, jumpAudio);
                        jumpState = JumpState.InFlight;
                    }
                    break;
                case JumpState.InFlight:
                    if (IsGrounded)
                    {
                        Schedule<PlayerLanded>().player = this;
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

            if (direction == Direction.Right)
            {
                spriteRenderer.flipX = false;
            }
            else if (direction == Direction.Left)
            {
                spriteRenderer.flipX = true;
            }

            animator.SetBool("grounded", IsGrounded);
            animator.SetFloat("velocityX", Mathf.Abs(velocity.x) / playerStats.maxSpeed);

            targetVelocity = move * playerStats.maxSpeed;
        }

        private void OnTriggerEnter2D(Collider2D cldr)
        {
            HitboxController hitboxController = cldr.GetComponent<HitboxController>();
            if (hitboxController != null && 
                    (attackState == AttackState.WindUp || attackState == AttackState.Active) &&
                    hitboxController.playerAttack.id == currentAttack.id)
            {
                impactVector = Vector3.Normalize(transform.position - cldr.transform.position);
                TakeHit(hitboxController);
                
            }
        }
        private void TakeHit(HitboxController hitboxController)
        {
            SetHealthState(HealthState.Impacted);
            health.Decrement();
            Schedule<PlayerImpacted>().player = this;
            ImpactFinished impactFinishedEvent = Schedule<ImpactFinished>(hitboxController.playerAttack.impactTime);
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
            Schedule<PlayerDisable>().player = this;
            Schedule<DisableFinished>(lastAttackTaken.disableTime).player = this;
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
                Simulation.Schedule<PlayerSpawn>(2);
            }
        }



    }
}