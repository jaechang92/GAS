// ================================
// File: Assets/Scripts/GAS/AbilitySystem/Abilities/ProjectileAttackAbility.cs
// ���Ÿ� ����ü ���� �����Ƽ
// ================================
using System.Threading.Tasks;
using UnityEngine;
using GAS.Core;
using GAS.EffectSystem;
using static GAS.Core.GASConstants;

namespace GAS.AbilitySystem.Abilities
{
    /// <summary>
    /// ����ü �߻� �����Ƽ
    /// </summary>
    [CreateAssetMenu(fileName = "ProjectileAttackAbility", menuName = "GAS/Abilities/Projectile Attack")]
    public class ProjectileAttackAbility : GameplayAbility
    {
        [Header("Projectile Settings")]
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private float projectileSpeed = 10f;
        [SerializeField] private float projectileLifetime = 3f;
        [SerializeField] private float projectileDamage = 15f;

        [Header("Spawn Settings")]
        [SerializeField] private Vector3 spawnOffset = new Vector3(1f, 0, 0);
        [SerializeField] private bool inheritVelocity = false;
        [SerializeField] private float inheritVelocityMultiplier = 0.5f;

        [Header("Multi-Shot")]
        [SerializeField] private int projectileCount = 1;
        [SerializeField] private float spreadAngle = 15f;
        [SerializeField] private float shotDelay = 0.1f;

        [Header("Targeting")]
        [SerializeField] private bool autoAim = false;
        [SerializeField] private float autoAimRange = 10f;
        [SerializeField] private LayerMask targetLayer = -1;

        [Header("Effects")]
        [SerializeField] private GameplayEffect damageEffect;
        [SerializeField] private GameObject muzzleFlashPrefab;
        [SerializeField] private AudioClip fireSound;

        public override void InitializeAbility()
        {
            base.InitializeAbility();

            if (string.IsNullOrEmpty(abilityName))
                abilityName = "Projectile Attack";

            if (string.IsNullOrEmpty(description))
                description = "Fires a projectile at enemies";

            if (cooldownTime <= 0)
                cooldownTime = 0.5f;
        }

        public override bool CanActivateAbility(AbilitySystemComponent source)
        {
            if (!base.CanActivateAbility(source))
                return false;

            // ���� üũ
            var attributes = source.GetComponent<AttributeSystem.AttributeSetComponent>();
            if (attributes != null)
            {
                float mana = attributes.GetAttributeValue(AttributeType.Mana);
                if (mana < GetCostValue())
                {
                    if (debugMode)
                        Debug.Log($"[ProjectileAttack] Not enough mana: {mana}/{GetCostValue()}");
                    return false;
                }
            }

            return true;
        }

        public override async void ActivateAbility(AbilitySystemComponent source, AbilityActivationInfo activationInfo)
        {
            base.ActivateAbility(source, activationInfo);

            if (debugMode)
                Debug.Log($"[ProjectileAttack] Activated by {source.name}");

            // �߻� ���� ����
            Vector3 fireDirection = GetFireDirection(source, activationInfo);

            // ���� �߻�
            for (int i = 0; i < projectileCount; i++)
            {
                // ���� ���
                float angleOffset = 0f;
                if (projectileCount > 1)
                {
                    float totalSpread = spreadAngle * (projectileCount - 1);
                    angleOffset = -totalSpread / 2f + (spreadAngle * i);
                }

                Vector3 shotDirection = Quaternion.Euler(0, 0, angleOffset) * fireDirection;

                // ����ü �߻�
                FireProjectile(source, shotDirection, activationInfo);

                // �߻� ����
                if (i < projectileCount - 1 && shotDelay > 0)
                {
                    await Task.Delay((int)(shotDelay * 1000));
                }
            }

            // ���� �Һ�
            ConsumeMana(source);

            // �����Ƽ ����
            EndAbility(source);
        }

        private void FireProjectile(AbilitySystemComponent source, Vector3 direction, AbilityActivationInfo activationInfo)
        {
            if (projectilePrefab == null)
            {
                Debug.LogError("[ProjectileAttack] No projectile prefab set!");
                return;
            }

            // �߻� ��ġ ���
            Vector3 spawnPosition = source.transform.position + source.transform.TransformDirection(spawnOffset);

            // ����ü ����
            GameObject projectile = GameObject.Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);

            // ����ü ������Ʈ ����
            var projectileComponent = projectile.GetComponent<Projectile>();
            if (projectileComponent == null)
            {
                projectileComponent = projectile.AddComponent<Projectile>();
            }

            // ����ü �ʱ�ȭ
            projectileComponent.Initialize(
                source.gameObject,
                direction,
                projectileSpeed,
                projectileDamage,
                projectileLifetime,
                damageEffect,
                targetLayer
            );

            // �ӵ� ���
            if (inheritVelocity)
            {
                var sourceRb = source.GetComponent<Rigidbody2D>();
                if (sourceRb != null)
                {
                    projectileComponent.AddVelocity(sourceRb.velocity * inheritVelocityMultiplier);
                }
            }

            // ���� �÷���
            if (muzzleFlashPrefab != null)
            {
                var muzzleFlash = GameObject.Instantiate(
                    muzzleFlashPrefab,
                    spawnPosition,
                    Quaternion.LookRotation(Vector3.forward, direction)
                );
                GameObject.Destroy(muzzleFlash, 0.5f);
            }

            // �߻� ����
            if (fireSound != null)
            {
                AudioSource.PlayClipAtPoint(fireSound, spawnPosition);
            }
        }

        private Vector3 GetFireDirection(AbilitySystemComponent source, AbilityActivationInfo activationInfo)
        {
            // �ڵ� ����
            if (autoAim)
            {
                GameObject nearestTarget = FindNearestTarget(source);
                if (nearestTarget != null)
                {
                    return (nearestTarget.transform.position - source.transform.position).normalized;
                }
            }

            // Ÿ���� �����Ǿ� ������ Ÿ�� ����
            if (activationInfo.target != null)
            {
                return (activationInfo.target.transform.position - source.transform.position).normalized;
            }

            // �Է� ����
            if (activationInfo.activationDirection != Vector3.zero)
            {
                return activationInfo.activationDirection.normalized;
            }

            // ���콺 ���� (2D)
            if (Camera.main != null)
            {
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                mousePos.z = source.transform.position.z;
                Vector3 toMouse = (mousePos - source.transform.position).normalized;
                if (toMouse != Vector3.zero)
                {
                    return toMouse;
                }
            }

            // �⺻ ����
            var spriteRenderer = source.GetComponentInChildren<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                return spriteRenderer.flipX ? Vector3.left : Vector3.right;
            }

            return source.transform.right;
        }

        private GameObject FindNearestTarget(AbilitySystemComponent source)
        {
            Collider2D[] targets = Physics2D.OverlapCircleAll(
                source.transform.position,
                autoAimRange,
                targetLayer
            );

            GameObject nearestTarget = null;
            float nearestDistance = float.MaxValue;

            foreach (var target in targets)
            {
                if (target.gameObject == source.gameObject)
                    continue;

                float distance = Vector3.Distance(source.transform.position, target.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestTarget = target.gameObject;
                }
            }

            return nearestTarget;
        }

        private void ConsumeMana(AbilitySystemComponent source)
        {
            var attributes = source.GetComponent<AttributeSystem.AttributeSetComponent>();
            if (attributes != null)
            {
                attributes.ModifyAttribute(AttributeType.Mana, -GetCostValue());
            }
        }

        public override float GetCostValue()
        {
            return 10f * projectileCount; // ����ü�� 10 ����
        }
    }

    /// <summary>
    /// ����ü ������Ʈ
    /// </summary>
    public class Projectile : MonoBehaviour
    {
        private GameObject owner;
        private Vector3 velocity;
        private float damage;
        private float lifetime;
        private GameplayEffect damageEffect;
        private LayerMask targetLayer;
        private Rigidbody2D rb;

        public void Initialize(GameObject owner, Vector3 direction, float speed, float damage,
            float lifetime, GameplayEffect effect, LayerMask targetLayer)
        {
            this.owner = owner;
            this.velocity = direction * speed;
            this.damage = damage;
            this.lifetime = lifetime;
            this.damageEffect = effect;
            this.targetLayer = targetLayer;

            // Rigidbody ����
            rb = GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody2D>();
            }
            rb.gravityScale = 0;
            rb.velocity = velocity;

            // Collider ����
            var col = GetComponent<Collider2D>();
            if (col == null)
            {
                col = gameObject.AddComponent<CircleCollider2D>();
            }
            col.isTrigger = true;

            // �ڵ� �ı�
            Destroy(gameObject, lifetime);

            // ���⿡ ���� ȸ��
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }

        public void AddVelocity(Vector3 additionalVelocity)
        {
            velocity += additionalVelocity;
            if (rb != null)
            {
                rb.velocity = velocity;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // �����ڴ� ����
            if (other.gameObject == owner)
                return;

            // ���̾� üũ
            if ((targetLayer.value & (1 << other.gameObject.layer)) == 0)
                return;

            // ������ ����
            ApplyDamage(other.gameObject);

            // ����ü �ı�
            CreateHitEffect();
            Destroy(gameObject);
        }

        private void ApplyDamage(GameObject target)
        {
            if (damageEffect != null)
            {
                var effectComponent = target.GetComponent<EffectComponent>();
                if (effectComponent != null)
                {
                    var context = new EffectContext
                    {
                        source = owner,
                        target = target,
                        power = damage,
                        effectLocation = transform.position
                    };

                    effectComponent.ApplyEffect(damageEffect, context);
                }
            }
            else
            {
                // ���� ������
                var attributes = target.GetComponent<AttributeSystem.AttributeSetComponent>();
                if (attributes != null)
                {
                    attributes.ModifyAttribute(AttributeType.Health, -damage);
                }
            }
        }

        private void CreateHitEffect()
        {
            // ��Ʈ ����Ʈ ���� (�������� �ִٸ�)
            // TODO: ��Ʈ ����Ʈ ����
        }
    }
}