// ===================================
// ����: Assets/Scripts/Ability/Integration/PlatformerAbilityController.cs
// ===================================
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AbilitySystem.Platformer
{
    /// <summary>
    /// 2D �÷����ӿ� �����Ƽ ��Ʈ�ѷ�
    /// </summary>
    public class PlatformerAbilityController : MonoBehaviour
    {
        [Header("������Ʈ ����")]
        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private Collider2D playerCollider;
        [SerializeField] private Transform attackPoint;
        [SerializeField] private LayerMask enemyLayer;
        [SerializeField] private LayerMask groundLayer;

        [Header("���� ����")]
        [SerializeField] private SkulData currentSkul;
        [SerializeField] private SkulData subSkul;  // ��ü�� ���� ����

        [Header("����")]
        [SerializeField] private CharacterState currentState = CharacterState.Idle;
        [SerializeField] private bool isGrounded = true;
        [SerializeField] private bool isFacingRight = true;

        [Header("�Է� ����")]
        [SerializeField] private Vector2 currentMoveInput;
        [SerializeField] private Vector2 rawInputValue;
        [SerializeField] private bool isReceivingInput;
        [SerializeField] private string lastInputTime;

        [Header("�޺� �ý���")]
        [SerializeField] private int currentCombo = 0;
        [SerializeField] private float lastAttackTime = 0f;

        [Header("��ٿ� ����")]
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

        // ���� ����
        private bool isAttacking = false;
        private bool isDashing = false;
        private float dashCooldown = 0f;

        // ��¡ ����
        private bool isCharging = false;
        private float chargeTime = 0f;
        private string chargingAbilityId;

        /// <summary>
        /// �ʱ�ȭ
        /// </summary>
        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();

            InitializeInputActions();
            LoadSkul(currentSkul);
        }

        /// <summary>
        /// Input Actions �ʱ�ȭ
        /// </summary>
        [ContextMenu("Initialize Input Actions")]
        private void InitializeInputActions()
        {
            if (playerInput == null)
            {
                playerInput = new AbilityInputActions();
                playerInput.Player.Enable();
            }

            // �̵�
            moveAction = playerInput.Player.Move;
            if (moveAction != null)
            {
                moveAction.performed += OnMovePerformed;
                moveAction.canceled += OnMoveCanceled;
            }

            // �⺻ ����
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
        /// ���� ������Ʈ
        /// </summary>
        private void FixedUpdate()
        {
            if (!isDashing && currentState != CharacterState.Hit)
            {
                // �̵� ó��
                float moveSpeed = currentSkul != null ? currentSkul.moveSpeed : 5f;
                rb.linearVelocity = new Vector2(currentMoveInput.x * moveSpeed, rb.linearVelocityY);
                // ���� ��ȯ
                if (currentMoveInput.x > 0 && !isFacingRight)
                    Flip();
                else if (currentMoveInput.x < 0 && isFacingRight)
                    Flip();

                // ���� ������Ʈ
                UpdateCharacterState();
            }

            // ��ٿ� ������Ʈ
            UpdateCooldowns();
        }

        /// <summary>
        /// ���� �ε�
        /// </summary>
        private void LoadSkul(SkulData skulData)
        {
            if (skulData == null) return;

            currentSkul = skulData;
            cooldowns.Clear();
            currentCombo = 0;

            // ���ú� �ʱ�ȭ
            Debug.Log($"���� �ε�: {skulData.skulName}");
        }

        /// <summary>
        /// ���� ��ü
        /// </summary>
        private void SwapSkul()
        {
            if (subSkul == null || isAttacking || isDashing) return;

            // ���� ��ü
            var temp = currentSkul;
            currentSkul = subSkul;
            subSkul = temp;

            LoadSkul(currentSkul);

            // ��ü ����Ʈ
            ShowSwapEffect();
        }

        /// <summary>
        /// �⺻ ���� ����
        /// </summary>
        private async Awaitable PerformBasicAttack()
        {
            if (isAttacking || currentState == CharacterState.Hit) return;
            if (currentSkul?.basicAttack == null) return;

            // �޺� üũ
            if (Time.time - lastAttackTime > currentSkul.comboResetTime)
            {
                currentCombo = 0;
            }

            isAttacking = true;
            currentState = CharacterState.Attacking;

            // ���� ���� ����
            AttackDirection attackDir = DetermineAttackDirection();

            // �޺��� ���� ������ ����
            float damageMultiplier = currentSkul.comboDamageMultipliers[Mathf.Min(currentCombo, currentSkul.comboDamageMultipliers.Length - 1)];

            // ��Ʈ�ڽ� ����
            await CreateHitbox(currentSkul.basicAttack, damageMultiplier);

            // �޺� ����
            currentCombo = (currentCombo + 1) % currentSkul.maxComboCount;
            lastAttackTime = Time.time;

            // ���� ����
            await Awaitable.WaitForSecondsAsync(1f / currentSkul.attackSpeed);
            isAttacking = false;
        }

        /// <summary>
        /// ��ų ���
        /// </summary>
        private async Awaitable UseSkill(int skillNumber)
        {
            PlatformerAbilityData skillData = skillNumber == 1 ? currentSkul?.skill1 : currentSkul?.skill2;
            if (skillData == null) return;

            // ��ٿ� üũ
            if (IsOnCooldown(skillData.abilityId)) return;

            // ��¡ ��ų ó��
            if (skillData.isChargeSkill)
            {
                StartCharging(skillData.abilityId);
                return;
            }

            // ��ų ����
            await ExecuteSkill(skillData);
        }

        /// <summary>
        /// ��ų ����
        /// </summary>
        private async Awaitable ExecuteSkill(PlatformerAbilityData skillData)
        {
            // ��ٿ� ����
            StartCooldown(skillData.abilityId, skillData.cooldownTime);

            // �̵� ���� ���� üũ
            if (!skillData.canMoveWhileUsing)
            {
                isAttacking = true;
                currentState = CharacterState.Attacking;
            }

            // �ٴ���Ʈ ó��
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

            // ��ų ����
            if (!skillData.canMoveWhileUsing)
            {
                isAttacking = false;
            }
        }

        /// <summary>
        /// ��¡ ����
        /// </summary>
        private void StartCharging(string abilityId)
        {
            isCharging = true;
            chargeTime = 0f;
            chargingAbilityId = abilityId;
        }

        /// <summary>
        /// ��ų ��ư ���� (��¡ ��ų��)
        /// </summary>
        private async void OnSkillReleased(int skillNumber)
        {
            if (!isCharging) return;

            PlatformerAbilityData skillData = skillNumber == 1 ? currentSkul?.skill1 : currentSkul?.skill2;
            if (skillData == null || skillData.abilityId != chargingAbilityId) return;

            // ��¡ �ð��� ���� ������ ����
            float chargeRatio = Mathf.Clamp01(chargeTime / skillData.maxChargeTime);
            float damageMultiplier = skillData.damageMultiplier * (1f + chargeRatio);

            // ��¡ ��ų ����
            StartCooldown(skillData.abilityId, skillData.cooldownTime);
            await CreateHitbox(skillData, damageMultiplier);

            isCharging = false;
            chargeTime = 0f;
            chargingAbilityId = null;
        }

        /// <summary>
        /// ��� ����
        /// </summary>
        private async Awaitable PerformDash()
        {
            if (isDashing || dashCooldown > 0) return;
            if (currentSkul?.dashAbility == null) return;

            isDashing = true;
            currentState = CharacterState.Dashing;
            dashCooldown = currentSkul.dashAbility.cooldownTime;

            // ��� ����
            float dashDirection = isFacingRight ? 1f : -1f;
            if (currentMoveInput.x != 0)
            {
                dashDirection = Mathf.Sign(currentMoveInput.x);
            }

            // ��� ����
            float dashSpeed = currentSkul.dashAbility.range;
            rb.linearVelocity = new Vector2(dashDirection * dashSpeed, 0);

            // ���� �ð�
            SetInvulnerable(true);

            await Awaitable.WaitForSecondsAsync(0.3f);

            isDashing = false;
            SetInvulnerable(false);
        }

        /// <summary>
        /// ����
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
        /// ��Ʈ�ڽ� ����
        /// </summary>
        private async Awaitable CreateHitbox(PlatformerAbilityData abilityData, float damageMultiplier)
        {
            // ��Ʈ�ڽ� ��ġ ���
            Vector2 hitboxPosition = (Vector2)attackPoint.position + abilityData.hitboxOffset;
            if (!isFacingRight)
            {
                hitboxPosition.x = attackPoint.position.x - abilityData.hitboxOffset.x;
            }

            // �� ����
            Collider2D[] hits = Physics2D.OverlapBoxAll(
                hitboxPosition,
                abilityData.hitboxSize,
                0f,
                enemyLayer
            );

            // ������ ����
            float finalDamage = currentSkul.attackPower * damageMultiplier;
            foreach (var hit in hits)
            {
                var target = hit.GetComponent<IAbilityTarget>();
                if (target != null && target.IsTargetable)
                {
                    target.TakeDamage(finalDamage, gameObject);

                    // �˹� ����
                    if (abilityData.knockbackPower > 0)
                    {
                        ApplyKnockback(hit.gameObject, abilityData.knockbackPower);
                    }
                }
            }

            // ����Ʈ ����
            if (abilityData.effectPrefab != null)
            {
                var effect = Instantiate(abilityData.effectPrefab, hitboxPosition, Quaternion.identity);
                if (!isFacingRight)
                {
                    effect.transform.localScale = new Vector3(-1, 1, 1);
                }
                Destroy(effect, 2f);
            }

            // ���� ���
            if (abilityData.soundEffect != null)
            {
                AudioSource.PlayClipAtPoint(abilityData.soundEffect, transform.position);
            }

            await Awaitable.WaitForSecondsAsync(abilityData.hitboxDuration);
        }

        /// <summary>
        /// ���� ���� ����
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
        /// �˹� ����
        /// </summary>
        private void ApplyKnockback(GameObject target, float force)
        {
            var targetRb = target.GetComponent<Rigidbody2D>();
            if (targetRb != null)
            {
                Vector2 knockbackDir = (target.transform.position - transform.position).normalized;
                knockbackDir.y = 0.5f; // �ణ ���� ����
                targetRb.AddForce(knockbackDir * force, ForceMode2D.Impulse);
            }
        }

        /// <summary>
        /// ���� ��ȯ
        /// </summary>
        private void Flip()
        {
            isFacingRight = !isFacingRight;
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
        }

        /// <summary>
        /// ĳ���� ���� ������Ʈ
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
        /// ��ٿ� ������Ʈ
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

            // ��� ��ٿ�
            if (dashCooldown > 0)
            {
                dashCooldown -= Time.fixedDeltaTime;
            }

            // ��¡ �ð� ������Ʈ
            if (isCharging)
            {
                chargeTime += Time.fixedDeltaTime;
            }
        }

        /// <summary>
        /// ��ٿ� Ȯ��
        /// </summary>
        private bool IsOnCooldown(string abilityId)
        {
            return cooldowns.ContainsKey(abilityId) && cooldowns[abilityId] > 0;
        }

        /// <summary>
        /// ��ٿ� ����
        /// </summary>
        private void StartCooldown(string abilityId, float duration)
        {
            cooldowns[abilityId] = duration;
        }

        /// <summary>
        /// ���� ����
        /// </summary>
        private void SetInvulnerable(bool invulnerable)
        {
            // ���̾� �������� ���� ó��
            gameObject.layer = invulnerable ? LayerMask.NameToLayer("Invulnerable") : LayerMask.NameToLayer("Player");
        }

        /// <summary>
        /// ���� ��ü ����Ʈ
        /// </summary>
        private void ShowSwapEffect()
        {
            // ��ü ����Ʈ ǥ��
            Debug.Log("���� ��ü!");
        }

        /// <summary>
        /// ���� üũ
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
        /// ����� �����
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            if (attackPoint == null) return;

            // �⺻ ���� ��Ʈ�ڽ� ǥ��
            if (currentSkul?.basicAttack != null)
            {
                Gizmos.color = Color.red;
                Vector3 pos = attackPoint.position + (Vector3)currentSkul.basicAttack.hitboxOffset;
                Gizmos.DrawWireCube(pos, currentSkul.basicAttack.hitboxSize);
            }
        }

        #region MoveMentFuntion

        /// <summary>
        /// �̵� �Է� ����
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
        /// �̵� �Է� ����
        /// </summary>
        private void OnMoveCanceled(InputAction.CallbackContext context)
        {
            rawInputValue = Vector2.zero;
            currentMoveInput = Vector2.zero;
            isReceivingInput = false;

            Debug.Log(" Move Input ����");
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