// 파일 위치: Assets/Scripts/Ability/Integration/PlatformerAbilityController.cs
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Threading;
using System;
using AbilitySystem.Core;
using AbilitySystem.Platformer;

namespace AbilitySystem.Platformer
{
    /// <summary>
    /// 플랫포머 캐릭터의 모든 시스템을 통합 관리하는 메인 컨트롤러
    /// </summary>
    [RequireComponent(typeof(PlatformerMovement))]
    [RequireComponent(typeof(GroundChecker))]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(BoxCollider2D))]
    public class PlatformerAbilityController : MonoBehaviour
    {
        [Header("Component References")]
        private PlatformerMovement movement;
        private GroundChecker groundChecker;
        //private Rigidbody2D rb;
        private BoxCollider2D playerCollider;
        private AbilitySystem abilitySystem;

        [Header("Combat Settings")]
        [SerializeField] private Transform attackPoint;
        [SerializeField] private LayerMask enemyLayer;

        [Header("Skul System")]
        [SerializeField] private SkulData currentSkul;
        [SerializeField] private SkulData subSkul;

        [Header("State")]
        [SerializeField] private CharacterState currentState = CharacterState.Idle;

        // Input System
        private AbilityInputActions inputActions;
        private CancellationTokenSource cancellationTokenSource;

        // Combat State
        private int currentCombo = 0;
        private float lastAttackTime = 0f;
        private bool isAttacking = false;

        // Properties
        public CharacterState CurrentState => currentState;
        public bool IsGrounded => groundChecker.IsGrounded;
        public bool IsFacingRight => movement.IsFacingRight;
        public SkulData CurrentSkul => currentSkul;
        public SkulData SubSkul => subSkul;

        [SerializeField]
        public bool isDashing { get; set; }

        // Events
        public event Action<CharacterState> OnStateChanged;
        public event Action<SkulData> OnSkulChanged;
        public event Action<int> OnComboChanged;
        public event Action<float> OnHealthChanged;
        public event Action<float> OnManaChanged;

        #region Unity Lifecycle

        private void Awake()
        {
            InitializeComponents();
            InitializeInputSystem();
            InitializeAbilitySystem();
        }

        private void Start()
        {
            if (currentSkul != null)
            {
                ApplySkulStats();

                // AbilitySystem에 초기 스컬 등록
                if (abilitySystem != null)
                {
                    abilitySystem.RegisterSkul(currentSkul);
                    if (subSkul != null)
                    {
                        abilitySystem.RegisterSkul(subSkul);
                    }
                }
            }
        }

        private void Update()
        {
            UpdateState();
            UpdateComboTimer();
        }

        private void OnDestroy()
        {
            CleanupInputSystem();
            cancellationTokenSource?.Cancel();
            cancellationTokenSource?.Dispose();
        }

        #endregion

        #region Initialization

        private void InitializeComponents()
        {
            movement = GetComponent<PlatformerMovement>();
            groundChecker = GetComponent<GroundChecker>();
            //rb = GetComponent<Rigidbody2D>();
            playerCollider = GetComponent<BoxCollider2D>();

            cancellationTokenSource = new CancellationTokenSource();

            // Movement 이벤트 구독
            movement.OnFacingDirectionChanged += HandleFacingDirectionChanged;
            movement.OnJumpPerformed += HandleJumpPerformed;
            movement.OnDoubleJumpPerformed += HandleDoubleJumpPerformed;
            movement.OnWallHit += HandleWallHit;

            // GroundChecker 이벤트 구독
            groundChecker.OnLanded += HandleLanded;
            groundChecker.OnLeftGround += HandleLeftGround;
        }

        private void InitializeInputSystem()
        {
            inputActions = new AbilityInputActions();
            inputActions.Enable();

            // Movement Input (이미 PlatformerMovement에서 처리)
            inputActions.Player.Move.performed += movement.OnMove;
            inputActions.Player.Move.canceled += movement.OnMove;
            inputActions.Player.Jump.performed += movement.OnJump;
            inputActions.Player.Jump.canceled += movement.OnJump;

            // Combat Input
            inputActions.Player.Attack.performed += OnAttack;
            inputActions.Player.Skill1.performed += OnSkill1;
            inputActions.Player.Skill2.performed += OnSkill2;

            // System Input
            inputActions.Player.Dash.performed += OnDash;
            inputActions.Player.SwapSkul.performed += OnSwapSkul;
            inputActions.Player.Pause.performed += OnPause;
        }

        private void InitializeAbilitySystem()
        {
            abilitySystem = GetComponent<AbilitySystem>();
            if (abilitySystem == null)
            {
                abilitySystem = gameObject.AddComponent<AbilitySystem>();
            }
        }

        #endregion

        #region Input Handlers

        private void OnAttack(InputAction.CallbackContext context)
        {
            if (!CanPerformAction()) return;

            PerformAttack();
        }

        private void OnSkill1(InputAction.CallbackContext context)
        {
            if (!CanPerformAction() || currentSkul == null || currentSkul.skill1 == null) return;

            ExecuteAbilityAsync(currentSkul.skill1);
        }

        private void OnSkill2(InputAction.CallbackContext context)
        {
            if (!CanPerformAction() || currentSkul == null || currentSkul.skill2 == null) return;

            ExecuteAbilityAsync(currentSkul.skill2);
        }

        private void OnDash(InputAction.CallbackContext context)
        {
            if (!CanPerformAction()) return;

            // 스컬의 대시 어빌리티가 있으면 사용
            PlatformerAbilityData dashAbility = currentSkul?.dashAbility;

            if(dashAbility == null)
            {
                dashAbility = abilitySystem.Abilities[$"{currentSkul?.skulId}_basic_dash"].Data;
            }

            // 어빌리티 시스템을 통해 실행 (통합된 로직)
            ExecuteAbilityAsync(dashAbility);
        }

        private void OnSwapSkul(InputAction.CallbackContext context)
        {
            if (subSkul != null && !isAttacking)
            {
                SwapSkuls();
            }
        }

        private void OnPause(InputAction.CallbackContext context)
        {
            // TODO: Pause menu implementation
            Debug.Log("Pause pressed");
        }

        #endregion

        #region Combat System

        private void PerformAttack()
        {
            if (isAttacking) return;

            // Combo check
            float timeSinceLastAttack = Time.time - lastAttackTime;

            if (timeSinceLastAttack > (currentSkul?.comboResetTime ?? 1f))
            {
                currentCombo = 0;
            }

            if (currentCombo >= (currentSkul?.maxComboCount ?? 3))
            {
                currentCombo = 0;
            }

            isAttacking = true;
            lastAttackTime = Time.time;

            // Execute attack (비동기로 실행)
            ExecuteAttack();

            // 콤보는 공격 후 증가
            currentCombo++;
            OnComboChanged?.Invoke(currentCombo);
        }

        private async void ExecuteAttack()
        {
            ChangeState(CharacterState.Attacking);

            // 스컬의 기본 공격 어빌리티 사용
            if (currentSkul?.basicAttack != null)
            {
                // 어빌리티 ID 설정
                if (string.IsNullOrEmpty(currentSkul.basicAttack.abilityId))
                {
                    currentSkul.basicAttack.abilityId = $"{currentSkul.skulId}_basic";
                }

                // AbilitySystem을 통해 실행
                if (abilitySystem != null && abilitySystem.CanUseAbility(currentSkul.basicAttack.abilityId))
                {
                    await abilitySystem.ExecuteAbility(currentSkul.basicAttack, gameObject);
                }
                else
                {
                    // 폴백: 기본 공격 로직
                    await ExecuteBasicAttack();
                }
            }
            else
            {
                // 어빌리티 데이터가 없으면 기본 공격 실행
                await ExecuteBasicAttack();
            }

            isAttacking = false;

            if (currentState == CharacterState.Attacking)
            {
                ChangeState(CharacterState.Idle);
            }
        }

        private async Awaitable ExecuteBasicAttack()
        {
            // 기본 공격 로직 (어빌리티 데이터가 없을 때)
            float baseDamage = currentSkul?.attackPower ?? 10f;

            // 콤보 인덱스 조정 (콤보는 1부터 시작, 배열 인덱스는 0부터)
            int comboIndex = Mathf.Clamp(currentCombo - 1, 0, 2); // 최대 3콤보
            float comboMultiplier = 1f;

            if (currentSkul != null && currentSkul.comboDamageMultipliers != null && currentSkul.comboDamageMultipliers.Length > comboIndex)
            {
                comboMultiplier = currentSkul.comboDamageMultipliers[comboIndex];
            }

            float finalDamage = baseDamage * comboMultiplier;

            Debug.Log($"Basic Attack - Combo: {currentCombo}, Index: {comboIndex}, Multiplier: {comboMultiplier}, Damage: {finalDamage}");

            // Hit detection
            if (attackPoint != null)
            {
                Vector2 attackSize = new Vector2(1f, 1f);
                Collider2D[] hitEnemies = Physics2D.OverlapBoxAll(
                    attackPoint.position,
                    attackSize,
                    0f,
                    enemyLayer
                );

                foreach (Collider2D enemy in hitEnemies)
                {
                    IAbilityTarget target = enemy.GetComponent<IAbilityTarget>();
                    target?.TakeDamage(finalDamage, gameObject);
                }
            }

            await Awaitable.WaitForSecondsAsync(0.2f, cancellationTokenSource.Token);
        }

        private async void ExecuteAbilityAsync(PlatformerAbilityData ability)
        {
            if (ability == null || isAttacking) return;

            // Check cooldown (새로운 구조 호환)
            string abilityId = ability.abilityId;

            // 어빌리티 ID가 없으면 스컬 ID 기반으로 생성
            if (string.IsNullOrEmpty(abilityId))
            {
                if (ability == currentSkul?.skill1)
                    abilityId = $"{currentSkul.skulId}_skill1";
                else if (ability == currentSkul?.skill2)
                    abilityId = $"{currentSkul.skulId}_skill2";
                else if (ability == currentSkul?.dashAbility)
                    abilityId = $"{currentSkul.skulId}_dash";
                else
                    abilityId = System.Guid.NewGuid().ToString();

                ability.abilityId = abilityId;
            }

            // Check cooldown
            if (abilitySystem != null && !abilitySystem.CanUseAbility(abilityId))
            {
                Debug.Log($"Ability {ability.abilityName} is on cooldown or cannot be used");
                return;
            }

            isAttacking = true;
            ChangeState(CharacterState.UsingAbility);

            // Start ability
            if (abilitySystem != null)
            {
                await abilitySystem.ExecuteAbility(ability, gameObject);
            }

            isAttacking = false;

            if (currentState == CharacterState.UsingAbility)
            {
                ChangeState(CharacterState.Idle);
            }
        }

        #endregion

        #region Skul System

        private void SwapSkuls()
        {
            if (subSkul == null) return;

            SkulData temp = currentSkul;
            currentSkul = subSkul;
            subSkul = temp;

            ApplySkulStats();
            OnSkulChanged?.Invoke(currentSkul);
        }

        private void ApplySkulStats()
        {
            if (currentSkul == null) return;

            // Apply movement stats
            if (movement != null)
            {
                movement.UpdateMovementStats(currentSkul.moveSpeed, currentSkul.jumpPower);
            }

            // Register new skul abilities to AbilitySystem
            if (abilitySystem != null)
            {
                abilitySystem.ChangeSkul(currentSkul);
            }

            // Reset combo
            currentCombo = 0;
            OnComboChanged?.Invoke(0);

            Debug.Log($"Applied {currentSkul.skulName} stats - ATK: {currentSkul.attackPower}, SPD: {currentSkul.moveSpeed}");
        }

        #endregion

        #region State Management

        private void UpdateState()
        {
            if (isAttacking) return; // Don't change state while attacking

            CharacterState newState = DetermineState();

            if (newState != currentState)
            {
                ChangeState(newState);
            }
        }

        private CharacterState DetermineState()
        {
            if (currentState == CharacterState.Dashing) return currentState;
            if (currentState == CharacterState.UsingAbility) return currentState;
            if (currentState == CharacterState.Attacking) return currentState;

            if (!groundChecker.IsGrounded)
            {
                return movement.IsFalling ? CharacterState.Falling : CharacterState.Jumping;
            }

            if (movement.IsMoving)
            {
                return CharacterState.Running;
            }

            return CharacterState.Idle;
        }

        public void ChangeState(CharacterState newState)
        {
            if (currentState == newState) return;

            CharacterState oldState = currentState;
            currentState = newState;

            OnStateChanged?.Invoke(newState);

            Debug.Log($"State changed: {oldState} -> {newState}");
        }

        #endregion

        #region Combat Helpers

        private void UpdateComboTimer()
        {
            if (currentCombo > 0 && Time.time - lastAttackTime > (currentSkul?.comboResetTime ?? 1f))
            {
                currentCombo = 0;
                OnComboChanged?.Invoke(0);
            }
        }

        private bool CanPerformAction()
        {
            // Can't perform actions while in certain states
            if (currentState == CharacterState.Dashing) return false;
            if (currentState == CharacterState.Stunned) return false;
            if (currentState == CharacterState.Dead) return false;

            return true;
        }

        #endregion

        #region Event Handlers

        private void HandleFacingDirectionChanged(bool isFacingRight)
        {
            // Flip attack point
            if (attackPoint != null)
            {
                Vector3 localPos = attackPoint.localPosition;
                localPos.x = Mathf.Abs(localPos.x) * (isFacingRight ? 1 : -1);
                attackPoint.localPosition = localPos;
            }
        }

        private void HandleJumpPerformed()
        {
            // Play jump sound/effect
            if (currentSkul?.basicAttack?.soundEffect != null)
            {
                // AudioSource.PlayClipAtPoint(currentSkul.basicAttack.soundEffect, transform.position);
            }
        }

        private void HandleDoubleJumpPerformed()
        {
            // Play double jump effect
        }

        private void HandleWallHit()
        {
            // Handle wall collision effects
        }

        private void HandleLanded()
        {
            // Play landing effect
        }

        private void HandleLeftGround()
        {
            // Handle leaving ground
        }

        #endregion

        #region Cleanup

        private void CleanupInputSystem()
        {
            if (inputActions != null)
            {
                inputActions.Player.Move.performed -= movement.OnMove;
                inputActions.Player.Move.canceled -= movement.OnMove;
                inputActions.Player.Jump.performed -= movement.OnJump;
                inputActions.Player.Jump.canceled -= movement.OnJump;

                inputActions.Player.Attack.performed -= OnAttack;
                inputActions.Player.Skill1.performed -= OnSkill1;
                inputActions.Player.Skill2.performed -= OnSkill2;
                inputActions.Player.Dash.performed -= OnDash;
                inputActions.Player.SwapSkul.performed -= OnSwapSkul;
                inputActions.Player.Pause.performed -= OnPause;

                inputActions.Disable();
                inputActions.Dispose();
            }

            if (movement != null)
            {
                movement.OnFacingDirectionChanged -= HandleFacingDirectionChanged;
                movement.OnJumpPerformed -= HandleJumpPerformed;
                movement.OnDoubleJumpPerformed -= HandleDoubleJumpPerformed;
                movement.OnWallHit -= HandleWallHit;
            }

            if (groundChecker != null)
            {
                groundChecker.OnLanded -= HandleLanded;
                groundChecker.OnLeftGround -= HandleLeftGround;
            }
        }

        #endregion
    }

    /// <summary>
    /// 캐릭터 상태 열거형
    /// </summary>
    public enum CharacterState
    {
        Idle,
        Running,
        Jumping,
        Falling,
        Attacking,
        UsingAbility,
        Dashing,
        WallSliding,
        Stunned,
        Dead
    }
}