// ===================================
// 파일: Assets/Scripts/Ability/Integration/PlatformerAbilityController.cs
// ===================================
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AbilitySystem.Platformer
{
    /// <summary>
    /// 2D 플랫포머용 어빌리티 컨트롤러
    /// </summary>
    public class PlatformerAbilityController : MonoBehaviour
    {
        [Header("컴포넌트 참조")]
        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private Collider2D playerCollider;
        [SerializeField] private Transform attackPoint;
        [SerializeField] private LayerMask enemyLayer;
        [SerializeField] private LayerMask groundLayer;

        [Header("현재 스컬")]
        [SerializeField] private SkulData currentSkul;
        [SerializeField] private SkulData subSkul;  // 교체용 서브 스컬

        [Header("상태")]
        [SerializeField] private CharacterState currentState = CharacterState.Idle;
        [SerializeField] private bool isGrounded = true;
        [SerializeField] private bool isFacingRight = true;

        [Header("입력 상태")]
        [SerializeField] private Vector2 currentMoveInput;
        [SerializeField] private Vector2 rawInputValue;
        [SerializeField] private bool isReceivingInput;
        [SerializeField] private string lastInputTime;

        [Header("콤보 시스템")]
        [SerializeField] private int currentCombo = 0;
        [SerializeField] private float lastAttackTime = 0f;

        [Header("쿨다운 관리")]
        private Dictionary<string, float> cooldowns = new Dictionary<string, float>();

        // Input System
        private AbilityInputActions playerInput;
        private InputAction moveAction;
        private InputAction jumpAction;
        private InputAction attackAction;
        private InputAction skill1Action;
        private InputAction skill2Action;
        private InputAction dashAction;
        private InputAction swapSkulAction;

        // 내부 상태
        private bool isAttacking = false;
        private bool isDashing = false;
        private float dashCooldown = 0f;

        // 차징 관련
        private bool isCharging = false;
        private float chargeTime = 0f;
        private string chargingAbilityId;

        /// <summary>
        /// 초기화
        /// </summary>
        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();

            InitializeInputActions();
            LoadSkul(currentSkul);
        }

        /// <summary>
        /// Input Actions 초기화
        /// </summary>
        [ContextMenu("Initialize Input Actions")]
        private void InitializeInputActions()
        {
            if (playerInput == null)
            {
                playerInput = new AbilityInputActions();
                playerInput.Player.Enable();
            }

            // 이동
            moveAction = playerInput.Player.Move;
            if (moveAction != null)
            {
                moveAction.performed += OnMovePerformed;
                moveAction.canceled += OnMoveCanceled;
            }

            // 기본 공격
            attackAction = playerInput.Player.Attack;
            if (attackAction != null)
            {
                attackAction.performed += OnAttack;
            }

            jumpAction = playerInput.Player.Jump;
            if(jumpAction != null)
            {
                jumpAction.performed += OnJump;
            }

            skill1Action = playerInput.Player.Skill1;
            if(skill1Action != null)
            {
                skill1Action.performed += ctx => OnSkill(ctx, 1);
            }

            skill2Action = playerInput.Player.Skill2;
            if (skill2Action != null)
            {
                skill2Action.performed += ctx => OnSkill(ctx, 2);
            }

            dashAction = playerInput.Player.Dash;
            if(dashAction != null)
            {
                dashAction.performed += OnDash;
            }

            swapSkulAction = playerInput.Player.SwapSkul;
            if(swapSkulAction != null)
            {
                swapSkulAction.performed += OnSwapSkulHead;
            }


        }

        /// <summary>
        /// 물리 업데이트
        /// </summary>
        private void FixedUpdate()
        {
            if (!isDashing && currentState != CharacterState.Hit)
            {
                // 이동 처리
                float moveSpeed = currentSkul != null ? currentSkul.moveSpeed : 5f;
                rb.linearVelocity = new Vector2(currentMoveInput.x * moveSpeed, rb.linearVelocityY);
                // 방향 전환
                if (currentMoveInput.x > 0 && !isFacingRight)
                    Flip();
                else if (currentMoveInput.x < 0 && isFacingRight)
                    Flip();

                // 상태 업데이트
                UpdateCharacterState();
            }

            // 쿨다운 업데이트
            UpdateCooldowns();
        }

        /// <summary>
        /// 스컬 로드
        /// </summary>
        private void LoadSkul(SkulData skulData)
        {
            if (skulData == null) return;

            currentSkul = skulData;
            cooldowns.Clear();
            currentCombo = 0;

            // 스컬별 초기화
            Debug.Log($"스컬 로드: {skulData.skulName}");
        }

        /// <summary>
        /// 스컬 교체
        /// </summary>
        private void SwapSkul()
        {
            if (subSkul == null || isAttacking || isDashing) return;

            // 스컬 교체
            var temp = currentSkul;
            currentSkul = subSkul;
            subSkul = temp;

            LoadSkul(currentSkul);

            // 교체 이펙트
            ShowSwapEffect();
        }

        /// <summary>
        /// 기본 공격 수행
        /// </summary>
        private async Awaitable PerformBasicAttack()
        {
            if (isAttacking || currentState == CharacterState.Hit) return;
            if (currentSkul?.basicAttack == null) return;

            // 콤보 체크
            if (Time.time - lastAttackTime > currentSkul.comboResetTime)
            {
                currentCombo = 0;
            }

            isAttacking = true;
            currentState = CharacterState.Attacking;

            // 공격 방향 결정
            AttackDirection attackDir = DetermineAttackDirection();

            // 콤보에 따른 데미지 배율
            float damageMultiplier = currentSkul.comboDamageMultipliers[Mathf.Min(currentCombo, currentSkul.comboDamageMultipliers.Length - 1)];

            // 히트박스 생성
            await CreateHitbox(currentSkul.basicAttack, damageMultiplier);

            // 콤보 증가
            currentCombo = (currentCombo + 1) % currentSkul.maxComboCount;
            lastAttackTime = Time.time;

            // 공격 종료
            await Awaitable.WaitForSecondsAsync(1f / currentSkul.attackSpeed);
            isAttacking = false;
        }

        /// <summary>
        /// 스킬 사용
        /// </summary>
        private async Awaitable UseSkill(int skillNumber)
        {
            PlatformerAbilityData skillData = skillNumber == 1 ? currentSkul?.skill1 : currentSkul?.skill2;
            if (skillData == null) return;

            // 쿨다운 체크
            if (IsOnCooldown(skillData.abilityId)) return;

            // 차징 스킬 처리
            if (skillData.isChargeSkill)
            {
                StartCharging(skillData.abilityId);
                return;
            }

            // 스킬 실행
            await ExecuteSkill(skillData);
        }

        /// <summary>
        /// 스킬 실행
        /// </summary>
        private async Awaitable ExecuteSkill(PlatformerAbilityData skillData)
        {
            // 쿨다운 시작
            StartCooldown(skillData.abilityId, skillData.cooldownTime);

            // 이동 가능 여부 체크
            if (!skillData.canMoveWhileUsing)
            {
                isAttacking = true;
                currentState = CharacterState.Attacking;
            }

            // 다단히트 처리
            if (skillData.isMultiHit)
            {
                for (int i = 0; i < skillData.hitCount; i++)
                {
                    await CreateHitbox(skillData, skillData.damageMultiplier);
                    await Awaitable.WaitForSecondsAsync(0.1f);
                }
            }
            else
            {
                await CreateHitbox(skillData, skillData.damageMultiplier);
            }

            // 스킬 종료
            if (!skillData.canMoveWhileUsing)
            {
                isAttacking = false;
            }
        }

        /// <summary>
        /// 차징 시작
        /// </summary>
        private void StartCharging(string abilityId)
        {
            isCharging = true;
            chargeTime = 0f;
            chargingAbilityId = abilityId;
        }

        /// <summary>
        /// 스킬 버튼 해제 (차징 스킬용)
        /// </summary>
        private async void OnSkillReleased(int skillNumber)
        {
            if (!isCharging) return;

            PlatformerAbilityData skillData = skillNumber == 1 ? currentSkul?.skill1 : currentSkul?.skill2;
            if (skillData == null || skillData.abilityId != chargingAbilityId) return;

            // 차징 시간에 따른 데미지 배율
            float chargeRatio = Mathf.Clamp01(chargeTime / skillData.maxChargeTime);
            float damageMultiplier = skillData.damageMultiplier * (1f + chargeRatio);

            // 차징 스킬 실행
            StartCooldown(skillData.abilityId, skillData.cooldownTime);
            await CreateHitbox(skillData, damageMultiplier);

            isCharging = false;
            chargeTime = 0f;
            chargingAbilityId = null;
        }

        /// <summary>
        /// 대시 수행
        /// </summary>
        private async Awaitable PerformDash()
        {
            if (isDashing || dashCooldown > 0) return;
            if (currentSkul?.dashAbility == null) return;

            isDashing = true;
            currentState = CharacterState.Dashing;
            dashCooldown = currentSkul.dashAbility.cooldownTime;

            // 대시 방향
            float dashDirection = isFacingRight ? 1f : -1f;
            if (currentMoveInput.x != 0)
            {
                dashDirection = Mathf.Sign(currentMoveInput.x);
            }

            // 대시 실행
            float dashSpeed = currentSkul.dashAbility.range;
            rb.linearVelocity = new Vector2(dashDirection * dashSpeed, 0);

            // 무적 시간
            SetInvulnerable(true);

            await Awaitable.WaitForSecondsAsync(0.3f);

            isDashing = false;
            SetInvulnerable(false);
        }

        /// <summary>
        /// 점프
        /// </summary>
        private void Jump()
        {
            if (!isGrounded || currentState == CharacterState.Hit) return;

            float jumpPower = currentSkul != null ? currentSkul.jumpPower : 10f;
            rb.linearVelocity = new Vector2(rb.linearVelocityX, jumpPower);
            currentState = CharacterState.Jumping;
            isGrounded = false;
        }

        /// <summary>
        /// 히트박스 생성
        /// </summary>
        private async Awaitable CreateHitbox(PlatformerAbilityData abilityData, float damageMultiplier)
        {
            // 히트박스 위치 계산
            Vector2 hitboxPosition = (Vector2)attackPoint.position + abilityData.hitboxOffset;
            if (!isFacingRight)
            {
                hitboxPosition.x = attackPoint.position.x - abilityData.hitboxOffset.x;
            }

            // 적 감지
            Collider2D[] hits = Physics2D.OverlapBoxAll(
                hitboxPosition,
                abilityData.hitboxSize,
                0f,
                enemyLayer
            );

            // 데미지 적용
            float finalDamage = currentSkul.attackPower * damageMultiplier;
            foreach (var hit in hits)
            {
                var target = hit.GetComponent<IAbilityTarget>();
                if (target != null && target.IsTargetable)
                {
                    target.TakeDamage(finalDamage, gameObject);

                    // 넉백 적용
                    if (abilityData.knockbackPower > 0)
                    {
                        ApplyKnockback(hit.gameObject, abilityData.knockbackPower);
                    }
                }
            }

            // 이펙트 생성
            if (abilityData.effectPrefab != null)
            {
                var effect = Instantiate(abilityData.effectPrefab, hitboxPosition, Quaternion.identity);
                if (!isFacingRight)
                {
                    effect.transform.localScale = new Vector3(-1, 1, 1);
                }
                Destroy(effect, 2f);
            }

            // 사운드 재생
            if (abilityData.soundEffect != null)
            {
                AudioSource.PlayClipAtPoint(abilityData.soundEffect, transform.position);
            }

            await Awaitable.WaitForSecondsAsync(abilityData.hitboxDuration);
        }

        /// <summary>
        /// 공격 방향 결정
        /// </summary>
        private AttackDirection DetermineAttackDirection()
        {
            if (!isGrounded)
            {
                if (currentMoveInput.y < -0.5f)
                    return AttackDirection.Down;
                else
                    return AttackDirection.Air;
            }
            else if (currentMoveInput.y > 0.5f)
            {
                return AttackDirection.Up;
            }

            return AttackDirection.Forward;
        }

        /// <summary>
        /// 넉백 적용
        /// </summary>
        private void ApplyKnockback(GameObject target, float force)
        {
            var targetRb = target.GetComponent<Rigidbody2D>();
            if (targetRb != null)
            {
                Vector2 knockbackDir = (target.transform.position - transform.position).normalized;
                knockbackDir.y = 0.5f; // 약간 위로 띄우기
                targetRb.AddForce(knockbackDir * force, ForceMode2D.Impulse);
            }
        }

        /// <summary>
        /// 방향 전환
        /// </summary>
        private void Flip()
        {
            isFacingRight = !isFacingRight;
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
        }

        /// <summary>
        /// 캐릭터 상태 업데이트
        /// </summary>
        private void UpdateCharacterState()
        {
            if (isAttacking || isDashing) return;

            if (isGrounded)
            {
                if (Mathf.Abs(currentMoveInput.x) > 0.1f)
                    currentState = CharacterState.Moving;
                else
                    currentState = CharacterState.Idle;
            }
            else
            {
                if (rb.linearVelocityY > 0)
                    currentState = CharacterState.Jumping;
                else
                    currentState = CharacterState.Falling;
            }
        }

        /// <summary>
        /// 쿨다운 업데이트
        /// </summary>
        private void UpdateCooldowns()
        {
            List<string> keys = new List<string>(cooldowns.Keys);
            foreach (string key in keys)
            {
                cooldowns[key] -= Time.fixedDeltaTime;
                if (cooldowns[key] <= 0)
                {
                    cooldowns.Remove(key);
                }
            }

            // 대시 쿨다운
            if (dashCooldown > 0)
            {
                dashCooldown -= Time.fixedDeltaTime;
            }

            // 차징 시간 업데이트
            if (isCharging)
            {
                chargeTime += Time.fixedDeltaTime;
            }
        }

        /// <summary>
        /// 쿨다운 확인
        /// </summary>
        private bool IsOnCooldown(string abilityId)
        {
            return cooldowns.ContainsKey(abilityId) && cooldowns[abilityId] > 0;
        }

        /// <summary>
        /// 쿨다운 시작
        /// </summary>
        private void StartCooldown(string abilityId, float duration)
        {
            cooldowns[abilityId] = duration;
        }

        /// <summary>
        /// 무적 설정
        /// </summary>
        private void SetInvulnerable(bool invulnerable)
        {
            // 레이어 변경으로 무적 처리
            gameObject.layer = invulnerable ? LayerMask.NameToLayer("Invulnerable") : LayerMask.NameToLayer("Player");
        }

        /// <summary>
        /// 스컬 교체 이펙트
        /// </summary>
        private void ShowSwapEffect()
        {
            // 교체 이펙트 표시
            Debug.Log("스컬 교체!");
        }

        /// <summary>
        /// 지면 체크
        /// </summary>
        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (((1 << collision.gameObject.layer) & groundLayer) != 0)
            {
                isGrounded = true;
            }
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if (((1 << collision.gameObject.layer) & groundLayer) != 0)
            {
                isGrounded = false;
            }
        }

        /// <summary>
        /// 디버그 기즈모
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            if (attackPoint == null) return;

            // 기본 공격 히트박스 표시
            if (currentSkul?.basicAttack != null)
            {
                Gizmos.color = Color.red;
                Vector3 pos = attackPoint.position + (Vector3)currentSkul.basicAttack.hitboxOffset;
                Gizmos.DrawWireCube(pos, currentSkul.basicAttack.hitboxSize);
            }
        }

        #region MoveMentFuntion

        /// <summary>
        /// 이동 입력 받음
        /// </summary>
        private void OnMovePerformed(InputAction.CallbackContext context)
        {
            rawInputValue = context.ReadValue<Vector2>();
            currentMoveInput = rawInputValue;
            isReceivingInput = true;
            lastInputTime = Time.time.ToString("F2");

            Debug.Log($" Move Input: {rawInputValue}");
        }

        /// <summary>
        /// 이동 입력 해제
        /// </summary>
        private void OnMoveCanceled(InputAction.CallbackContext context)
        {
            rawInputValue = Vector2.zero;
            currentMoveInput = Vector2.zero;
            isReceivingInput = false;

            Debug.Log(" Move Input 해제");
        }

        private void OnJump(InputAction.CallbackContext context)
        {
            Jump();
        }

        private void OnAttack(InputAction.CallbackContext context)
        {
            _ = PerformBasicAttack();
        }

        private void OnSkill(InputAction.CallbackContext context, int skillIndex)
        {
            _ = UseSkill(skillIndex);
        }

        private void OnDash(InputAction.CallbackContext context)
        {
            _ = PerformDash();
        }

        private void OnSwapSkulHead(InputAction.CallbackContext context)
        {
            SwapSkul();
        }

        #endregion


    }
}