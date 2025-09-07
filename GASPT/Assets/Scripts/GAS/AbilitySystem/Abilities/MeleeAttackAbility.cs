// ================================
// File: Assets/Scripts/GAS/AbilitySystem/Abilities/MeleeAttackAbility.cs
// ���� ���� �����Ƽ ����
// ================================
using System.Threading.Tasks;
using UnityEngine;
using GAS.Core;
using GAS.EffectSystem;
using GAS.TagSystem;
using static GAS.Core.GASConstants;

namespace GAS.AbilitySystem.Abilities
{
    /// <summary>
    /// ���� ���� �����Ƽ
    /// </summary>
    [CreateAssetMenu(fileName = "MeleeAttackAbility", menuName = "GAS/Abilities/Melee Attack")]
    public class MeleeAttackAbility : GameplayAbility
    {
        [Header("Melee Attack Settings")]
        [SerializeField] private float attackRange = 2f;
        [SerializeField] private float attackAngle = 90f;
        [SerializeField] private float damageAmount = 10f;
        [SerializeField] private LayerMask targetLayer = -1;

        [Header("Attack Timing")]
        [SerializeField] private float attackDelay = 0.1f;
        [SerializeField] private float attackDuration = 0.3f;

        [Header("Effects")]
        [SerializeField] private GameplayEffect damageEffect;
        [SerializeField] private GameObject hitEffectPrefab;
        [SerializeField] private AudioClip attackSound;
        [SerializeField] private AudioClip hitSound;

        [Header("Combo")]
        [SerializeField] private bool enableCombo = true;
        [SerializeField] private int maxComboCount = 3;
        [SerializeField] private float comboWindow = 0.5f;
        [SerializeField] private float[] comboDamageMultipliers = { 1f, 1.2f, 1.5f };

        // Runtime
        private int currentComboCount = 0;
        private float lastAttackTime = 0f;

        public override void InitializeAbility()
        {
            base.InitializeAbility();

            // �⺻ ����
            if (string.IsNullOrEmpty(abilityName))
                abilityName = "Melee Attack";

            if (string.IsNullOrEmpty(description))
                description = "Performs a melee attack in front of the character";

            // �±� ����
            if (abilityTags == null)
                abilityTags = new GameplayTag[0];

            // ��ٿ� ����
            if (cooldownTime <= 0)
                cooldownTime = 0.5f;
        }

        public override bool CanActivateAbility(AbilitySystemComponent source)
        {
            if (!base.CanActivateAbility(source))
                return false;

            // �߰� üũ (���¹̳� ��)
            var attributes = source.GetComponent<AttributeSystem.AttributeSetComponent>();
            if (attributes != null)
            {
                float stamina = attributes.GetAttributeValue(AttributeType.Stamina);
                if (stamina < GetCostValue())
                {
                    if (debugMode)
                        Debug.Log($"[MeleeAttack] Not enough stamina: {stamina}/{GetCostValue()}");
                    return false;
                }
            }

            return true;
        }

        public override async void ActivateAbility(AbilitySystemComponent source, AbilityActivationInfo activationInfo)
        {
            base.ActivateAbility(source, activationInfo);

            if (debugMode)
                Debug.Log($"[MeleeAttack] Activated by {source.name}");

            // �޺� üũ
            UpdateCombo();

            // ���� ���� ���
            if (attackSound != null)
            {
                AudioSource.PlayClipAtPoint(attackSound, source.transform.position);
            }

            // ���� �ִϸ��̼� Ʈ���� (�ִٸ�)
            var animator = source.GetComponentInChildren<Animator>();
            if (animator != null)
            {
                animator.SetTrigger($"Attack{currentComboCount}");
            }

            // ���� ������
            await Task.Delay((int)(attackDelay * 1000));

            // ���� ����
            PerformAttack(source, activationInfo);

            // ���� ���� �ð�
            await Task.Delay((int)(attackDuration * 1000));

            // �����Ƽ ����
            EndAbility(source);
        }

        private void PerformAttack(AbilitySystemComponent source, AbilityActivationInfo activationInfo)
        {
            // ���� ���� ����
            Vector3 attackDirection = GetAttackDirection(source, activationInfo);
            Vector3 attackOrigin = source.transform.position;

            // Ÿ�� Ž��
            Collider2D[] hits = Physics2D.OverlapCircleAll(
                attackOrigin + attackDirection * (attackRange / 2),
                attackRange / 2,
                targetLayer
            );

            int hitCount = 0;
            foreach (var hit in hits)
            {
                // �ڱ� �ڽ� ����
                if (hit.gameObject == source.gameObject)
                    continue;

                // ���� üũ
                Vector3 toTarget = (hit.transform.position - attackOrigin).normalized;
                float angle = Vector3.Angle(attackDirection, toTarget);

                if (angle > attackAngle / 2)
                    continue;

                // ������ ����
                ApplyDamageToTarget(source, hit.gameObject);
                hitCount++;

                // ��Ʈ ����Ʈ
                if (hitEffectPrefab != null)
                {
                    var effect = GameObject.Instantiate(hitEffectPrefab, hit.transform.position, Quaternion.identity);
                    GameObject.Destroy(effect, 1f);
                }
            }

            // ��Ʈ ����
            if (hitCount > 0 && hitSound != null)
            {
                AudioSource.PlayClipAtPoint(hitSound, attackOrigin);
            }

            if (debugMode)
                Debug.Log($"[MeleeAttack] Hit {hitCount} targets");
        }

        private void ApplyDamageToTarget(AbilitySystemComponent source, GameObject target)
        {
            // ������ ���
            float finalDamage = CalculateDamage(source);

            // ����Ʈ ����
            if (damageEffect != null)
            {
                var effectComponent = target.GetComponent<EffectComponent>();
                if (effectComponent != null)
                {
                    var context = new EffectContext
                    {
                        source = source.gameObject,
                        target = target,
                        ability = this,
                        power = finalDamage,
                        level = level
                    };

                    effectComponent.ApplyEffect(damageEffect, context);
                }
            }
            else
            {
                // ���� ������ (����)
                var attributes = target.GetComponent<AttributeSystem.AttributeSetComponent>();
                if (attributes != null)
                {
                    attributes.ModifyAttribute(AttributeType.Health, -finalDamage);

                    if (debugMode)
                        Debug.Log($"[MeleeAttack] Dealt {finalDamage} damage to {target.name}");
                }
            }
        }

        private float CalculateDamage(AbilitySystemComponent source)
        {
            float baseDamage = damageAmount;

            // �޺� ���
            if (enableCombo && currentComboCount > 0 && currentComboCount <= comboDamageMultipliers.Length)
            {
                baseDamage *= comboDamageMultipliers[currentComboCount - 1];
            }

            // ���ݷ� ���� ����
            var attributes = source.GetComponent<AttributeSystem.AttributeSetComponent>();
            if (attributes != null)
            {
                float attackPower = attributes.GetAttributeValue(AttributeType.AttackPower);
                baseDamage *= (1f + attackPower / 100f);
            }

            return baseDamage;
        }

        private Vector3 GetAttackDirection(AbilitySystemComponent source, AbilityActivationInfo activationInfo)
        {
            // Ÿ���� ������ Ÿ�� ����
            if (activationInfo.target != null)
            {
                return (activationInfo.target.transform.position - source.transform.position).normalized;
            }

            // �Է� ������ ������ �Է� ����
            if (activationInfo.activationDirection != Vector3.zero)
            {
                return activationInfo.activationDirection.normalized;
            }

            // �⺻������ ĳ���Ͱ� �ٶ󺸴� ����
            // 2D�� ��� ��������Ʈ ���� üũ
            var spriteRenderer = source.GetComponentInChildren<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                return spriteRenderer.flipX ? Vector3.left : Vector3.right;
            }

            return source.transform.right;
        }

        private void UpdateCombo()
        {
            if (!enableCombo)
            {
                currentComboCount = 1;
                return;
            }

            float timeSinceLastAttack = Time.time - lastAttackTime;

            if (timeSinceLastAttack <= comboWindow)
            {
                currentComboCount = Mathf.Min(currentComboCount + 1, maxComboCount);
            }
            else
            {
                currentComboCount = 1;
            }

            lastAttackTime = Time.time;

            if (debugMode)
                Debug.Log($"[MeleeAttack] Combo: {currentComboCount}/{maxComboCount}");
        }

        public override void EndAbility(AbilitySystemComponent source)
        {
            base.EndAbility(source);

            // ���� �޺� �Է� ���
            if (enableCombo && currentComboCount >= maxComboCount)
            {
                // �޺� ����
                currentComboCount = 0;
            }
        }

        public override float GetCostValue()
        {
            // ���¹̳� �Һ�
            return 5f * (enableCombo ? currentComboCount : 1);
        }

        public override void OnDrawGizmos(Transform source)
        {
            if (source == null) return;

            // ���� ���� ǥ��
            Gizmos.color = new Color(1, 0, 0, 0.3f);
            Vector3 attackPos = source.position + source.right * (attackRange / 2);
            Gizmos.DrawWireSphere(attackPos, attackRange / 2);

            // ���� ���� ǥ��
            Gizmos.color = Color.red;
            Vector3 leftBound = Quaternion.Euler(0, 0, attackAngle / 2) * source.right * attackRange;
            Vector3 rightBound = Quaternion.Euler(0, 0, -attackAngle / 2) * source.right * attackRange;

            Gizmos.DrawLine(source.position, source.position + leftBound);
            Gizmos.DrawLine(source.position, source.position + rightBound);
        }

        protected override Awaitable<AbilityExecutionResult> ExecuteAbility()
        {
            throw new System.NotImplementedException();
        }
    }
}