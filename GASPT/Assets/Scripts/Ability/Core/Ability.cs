// ���� ��ġ: Assets/Scripts/Ability/Core/Ability.cs
using AbilitySystem.Platformer;
using System;
using UnityEngine;
using System.Threading;

namespace AbilitySystem
{
    /// <summary>
    /// ��Ÿ�� �����Ƽ �ν��Ͻ� Ŭ����
    /// </summary>
    public class Ability
    {
        // �����Ƽ ������ ���� (������: SkulData �� PlatformerAbilityData)
        private PlatformerAbilityData data;

        // ��ٿ� ����
        private AbilityCooldown cooldown;

        // ���� ����
        private AbilityState currentState;

        // ������ ����
        private GameObject owner;
        private AbilitySystem abilitySystem;

        // ���� ����
        private CancellationTokenSource cancellationTokenSource;

        // �̺�Ʈ
        public event Action<Ability> OnAbilityStarted;
        public event Action<Ability> OnAbilityCompleted;
        public event Action<Ability> OnCooldownStarted;
        public event Action<Ability> OnCooldownCompleted;

        // ������Ƽ
        public string Id => data?.abilityId;
        public string Name => data?.abilityName;
        public PlatformerAbilityData Data => data;
        public AbilityState State => currentState;
        public GameObject Owner => owner;
        public bool IsReady => currentState == AbilityState.Ready && cooldown.CanUse();
        public bool IsOnCooldown => cooldown.IsOnCooldown;
        public float CooldownProgress => cooldown.Progress;
        public float CooldownRemaining => cooldown.RemainingTime;

        /// <summary>
        /// �����Ƽ �ʱ�ȭ
        /// </summary>
        public void Initialize(PlatformerAbilityData abilityData, GameObject abilityOwner)
        {
            data = abilityData;
            owner = abilityOwner;
            abilitySystem = owner.GetComponent<AbilitySystem>();

            // ��ٿ� �ʱ�ȭ
            cooldown = new AbilityCooldown();
            cooldown.Initialize(data.cooldownTime);

            // ��ٿ� �̺�Ʈ ����
            cooldown.OnCooldownStarted += () => OnCooldownStarted?.Invoke(this);
            cooldown.OnCooldownCompleted += () => {
                currentState = AbilityState.Ready;
                OnCooldownCompleted?.Invoke(this);
            };

            // �ʱ� ����
            currentState = AbilityState.Ready;
            cancellationTokenSource = new CancellationTokenSource();

            Debug.Log($"Ability initialized: {data.abilityName}");
        }

        /// <summary>
        /// �����Ƽ ��� ���� ���� üũ
        /// </summary>
        public bool CanUse()
        {
            // ���� üũ
            if (currentState != AbilityState.Ready) return false;

            // ��ٿ� üũ
            if (!cooldown.CanUse()) return false;

            // �ڽ�Ʈ üũ (����)
            if (abilitySystem != null && !data.CanAfford(abilitySystem.CurrentMana))
            {
                Debug.Log($"Not enough mana for {data.abilityName}");
                return false;
            }

            // ������ ��ȿ�� üũ
            if (!data.ValidateData()) return false;

            return true;
        }

        /// <summary>
        /// �����Ƽ ����
        /// </summary>
        public async Awaitable ExecuteAsync()
        {
            if (!CanUse())
            {
                Debug.Log($"Cannot use ability: {data.abilityName}");
                return;
            }

            try
            {
                // ���� ����
                currentState = AbilityState.Active;
                OnAbilityStarted?.Invoke(this);

                Debug.Log($"Executing ability: {data.abilityName}");

                // �ִϸ��̼� Ʈ����
                if (!string.IsNullOrEmpty(data.animationTrigger))
                {
                    var animator = owner.GetComponent<Animator>();
                    if (animator != null)
                    {
                        animator.SetTrigger(data.animationTrigger);
                        animator.speed = data.animationSpeed;
                    }
                }

                // ���� ���
                if (data.soundEffect != null)
                {
                    AudioSource.PlayClipAtPoint(data.soundEffect, owner.transform.position);
                }

                // �����Ƽ Ÿ�Ժ� ����
                await ExecuteByType();

                // ��ٿ� ����
                StartCooldown();

                // �Ϸ� �̺�Ʈ
                OnAbilityCompleted?.Invoke(this);
            }
            catch (OperationCanceledException)
            {
                Debug.Log($"Ability cancelled: {data.abilityName}");
                currentState = AbilityState.Ready;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error executing ability {data.abilityName}: {e.Message}");
                currentState = AbilityState.Ready;
            }
        }

        /// <summary>
        /// �����Ƽ Ÿ�Ժ� ����
        /// </summary>
        private async Awaitable ExecuteByType()
        {
            switch (data.abilityType)
            {
                case PlatformerAbilityType.BasicAttack:
                    await ExecuteBasicAttack();
                    break;

                case PlatformerAbilityType.Skill:
                    await ExecuteSkill();
                    break;

                case PlatformerAbilityType.Ultimate:
                    await ExecuteUltimate();
                    break;

                case PlatformerAbilityType.Movement:
                    await ExecuteMovement();
                    break;

                case PlatformerAbilityType.Passive:
                    // �нú�� ��� ����
                    ExecutePassive();
                    break;

                default:
                    await Awaitable.NextFrameAsync(cancellationTokenSource.Token);
                    break;
            }
        }

        /// <summary>
        /// �⺻ ���� ����
        /// </summary>
        private async Awaitable ExecuteBasicAttack()
        {
            // ��Ʈ�ڽ� ����
            Vector2 attackPosition = GetAttackPosition();

            // �� ����
            Collider2D[] hits = Physics2D.OverlapBoxAll(
                attackPosition,
                data.hitboxSize,
                0f,
                LayerMask.GetMask("Enemy")
            );

            // ������ ó��
            foreach (var hit in hits)
            {
                ApplyDamageToTarget(hit.gameObject);
            }

            // ����Ʈ ����
            if (data.effectPrefab != null)
            {
                GameObject effect = GameObject.Instantiate(
                    data.effectPrefab,
                    attackPosition,
                    Quaternion.identity
                );
                GameObject.Destroy(effect, 2f);
            }

            // ��Ʈ�ڽ� ���ӽð�
            await Awaitable.WaitForSecondsAsync(
                data.hitboxDuration,
                cancellationTokenSource.Token
            );
        }

        /// <summary>
        /// ��ų ����
        /// </summary>
        private async Awaitable ExecuteSkill()
        {
            // ��¡ ��ų�� ���
            if (data.isChargeSkill)
            {
                float chargeTime = 0f;
                while (chargeTime < data.maxChargeTime)
                {
                    chargeTime += Time.deltaTime;

                    // ��¡ �� ��� üũ
                    if (data.cancelable && Input.GetKeyUp(KeyCode.X))
                    {
                        break;
                    }

                    await Awaitable.NextFrameAsync(cancellationTokenSource.Token);
                }

                // ��¡ ������ ���� ������ ����
                float chargeRatio = chargeTime / data.maxChargeTime;
                // ������ ����� chargeRatio Ȱ��
            }

            // �ٴ� ��Ʈ ó��
            if (data.isMultiHit)
            {
                for (int i = 0; i < data.hitCount; i++)
                {
                    await ExecuteHit();
                    await Awaitable.WaitForSecondsAsync(
                        0.1f,
                        cancellationTokenSource.Token
                    );
                }
            }
            else
            {
                await ExecuteHit();
            }
        }

        /// <summary>
        /// �ñر� ����
        /// </summary>
        private async Awaitable ExecuteUltimate()
        {
            // ȭ�� ȿ��
            Debug.Log($"Ultimate ability activated: {data.abilityName}");

            // ���� ������
            Collider2D[] hits = Physics2D.OverlapCircleAll(
                owner.transform.position,
                data.range,
                LayerMask.GetMask("Enemy")
            );

            foreach (var hit in hits)
            {
                ApplyDamageToTarget(hit.gameObject, 2f); // �ñر�� 2�� ������
            }

            await Awaitable.WaitForSecondsAsync(0.5f, cancellationTokenSource.Token);
        }

        /// <summary>
        /// �̵� ��ų ���� (��� ��)
        /// </summary>
        private async Awaitable ExecuteMovement()
        {
            Rigidbody2D rb = owner.GetComponent<Rigidbody2D>();
            if (rb == null) return;

            // ��� ���� ����
            float direction = owner.transform.localScale.x > 0 ? 1f : -1f;

            // �߷� �ӽ� ��Ȱ��ȭ
            float originalGravity = rb.gravityScale;
            rb.gravityScale = 0f;

            // ��� ����
            rb.linearVelocity = new Vector2(direction * data.range * 10f, 0);

            await Awaitable.WaitForSecondsAsync(0.2f, cancellationTokenSource.Token);

            // �߷� ����
            rb.gravityScale = originalGravity;
        }

        /// <summary>
        /// �нú� ����
        /// </summary>
        private void ExecutePassive()
        {
            Debug.Log($"Passive ability applied: {data.abilityName}");
            // �нú� ȿ���� BuffSystem���� ó��
        }

        /// <summary>
        /// ���� ��Ʈ ����
        /// </summary>
        private async Awaitable ExecuteHit()
        {
            Vector2 attackPosition = GetAttackPosition();

            Collider2D[] hits = Physics2D.OverlapBoxAll(
                attackPosition,
                data.hitboxSize,
                0f,
                LayerMask.GetMask("Enemy")
            );

            foreach (var hit in hits)
            {
                ApplyDamageToTarget(hit.gameObject);
            }

            await Awaitable.WaitForSecondsAsync(
                data.hitboxDuration,
                cancellationTokenSource.Token
            );
        }

        /// <summary>
        /// ���� ��ġ ���
        /// </summary>
        private Vector2 GetAttackPosition()
        {
            Vector2 position = owner.transform.position;

            switch (data.attackDirection)
            {
                case AttackDirection.Forward:
                    float direction = owner.transform.localScale.x > 0 ? 1f : -1f;
                    position += new Vector2(direction, 0) * data.range + data.hitboxOffset;
                    break;

                case AttackDirection.Up:
                    position += Vector2.up * data.range + data.hitboxOffset;
                    break;

                case AttackDirection.Down:
                    position += Vector2.down * data.range + data.hitboxOffset;
                    break;

                case AttackDirection.Air:
                    position += data.hitboxOffset;
                    break;
            }

            return position;
        }

        /// <summary>
        /// ������ ����
        /// </summary>
        private void ApplyDamageToTarget(GameObject target, float multiplier = 1f)
        {
            IAbilityTarget abilityTarget = target.GetComponent<IAbilityTarget>();
            if (abilityTarget != null && abilityTarget.IsTargetable)
            {
                // �⺻ ������ ���
                float damage = 10f * data.damageMultiplier * multiplier;

                // ��Ʈ�ѷ����� ���� ������ �޺� ���� ��������
                var controller = owner.GetComponent<PlatformerAbilityController>();
                if (controller != null && controller.CurrentSkul != null)
                {
                    // ������ ���ݷ� ����
                    damage *= controller.CurrentSkul.attackPower / 10f;

                    // �⺻ ������ ��� �޺� ���� ����
                    if (data.abilityType == PlatformerAbilityType.BasicAttack)
                    {
                        // PlatformerAbilityController���� ���� �޺� ������ �����;� �ϴµ�
                        // ���� ������ ���� ������ �����Ƿ� ������ ����� ��Ʈ�ѷ����� ó���ϵ��� ��
                        // �Ǵ� Ability ������ �޺� ������ ���޹޵��� ���� �ʿ�
                    }
                }

                abilityTarget.TakeDamage(damage, owner);

                // �˹� ����
                if (data.knockbackPower > 0)
                {
                    Vector2 knockbackDir = (target.transform.position - owner.transform.position).normalized;
                    abilityTarget.ApplyKnockback(knockbackDir, data.knockbackPower);
                }

                Debug.Log($"Applied {damage:F1} damage to {target.name}");
            }
        }

        /// <summary>
        /// ��ٿ� ������Ʈ (�� ������ ȣ��)
        /// </summary>
        public void UpdateCooldown(float deltaTime)
        {
            cooldown.Update(deltaTime);
        }

        /// <summary>
        /// ��ٿ� ����
        /// </summary>
        private void StartCooldown()
        {
            currentState = AbilityState.Cooldown;
            cooldown.StartCooldown();
        }

        /// <summary>
        /// �����Ƽ ���� �ߴ�
        /// </summary>
        public void Cancel()
        {
            if (currentState == AbilityState.Active)
            {
                cancellationTokenSource?.Cancel();
                currentState = AbilityState.Ready;
                Debug.Log($"Ability cancelled: {data.abilityName}");
            }
        }

        /// <summary>
        /// �����Ƽ ����
        /// </summary>
        public void Reset()
        {
            currentState = AbilityState.Ready;
            cooldown.Reset();
            cancellationTokenSource?.Cancel();
            cancellationTokenSource = new CancellationTokenSource();

            Debug.Log($"Ability reset: {data.abilityName}");
        }

        /// <summary>
        /// ��ٿ� ����
        /// </summary>
        public void ReduceCooldown(float amount)
        {
            cooldown.ReduceCooldown(amount);
        }

        /// <summary>
        /// ���ҽ� ����
        /// </summary>
        public void Dispose()
        {
            cancellationTokenSource?.Cancel();
            cancellationTokenSource?.Dispose();
        }
    }
}