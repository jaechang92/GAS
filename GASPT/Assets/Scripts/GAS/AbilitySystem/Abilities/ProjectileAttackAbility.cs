// ================================
// File: Assets/Scripts/GAS/AbilitySystem/Abilities/ProjectileAttackAbility.cs
// 원거리 투사체 공격 어빌리티
// ================================
using System.Threading.Tasks;
using UnityEngine;
using GAS.Core;
using GAS.EffectSystem;
using static GAS.Core.GASConstants;

namespace GAS.AbilitySystem.Abilities
{
    /// <summary>
    /// 투사체 발사 어빌리티
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

            // 마나 체크
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

            // 발사 방향 결정
            Vector3 fireDirection = GetFireDirection(source, activationInfo);

            // 다중 발사
            for (int i = 0; i < projectileCount; i++)
            {
                // 각도 계산
                float angleOffset = 0f;
                if (projectileCount > 1)
                {
                    float totalSpread = spreadAngle * (projectileCount - 1);
                    angleOffset = -totalSpread / 2f + (spreadAngle * i);
                }

                Vector3 shotDirection = Quaternion.Euler(0, 0, angleOffset) * fireDirection;

                // 투사체 발사
                FireProjectile(source, shotDirection, activationInfo);

                // 발사 간격
                if (i < projectileCount - 1 && shotDelay > 0)
                {
                    await Task.Delay((int)(shotDelay * 1000));
                }
            }

            // 마나 소비
            ConsumeMana(source);

            // 어빌리티 종료
            EndAbility(source);
        }

        private void FireProjectile(AbilitySystemComponent source, Vector3 direction, AbilityActivationInfo activationInfo)
        {
            if (projectilePrefab == null)
            {
                Debug.LogError("[ProjectileAttack] No projectile prefab set!");
                return;
            }

            // 발사 위치 계산
            Vector3 spawnPosition = source.transform.position + source.transform.TransformDirection(spawnOffset);

            // 투사체 생성
            GameObject projectile = GameObject.Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);

            // 투사체 컴포넌트 설정
            var projectileComponent = projectile.GetComponent<Projectile>();
            if (projectileComponent == null)
            {
                projectileComponent = projectile.AddComponent<Projectile>();
            }

            // 투사체 초기화
            projectileComponent.Initialize(
                source.gameObject,
                direction,
                projectileSpeed,
                projectileDamage,
                projectileLifetime,
                damageEffect,
                targetLayer
            );

            // 속도 상속
            if (inheritVelocity)
            {
                var sourceRb = source.GetComponent<Rigidbody2D>();
                if (sourceRb != null)
                {
                    projectileComponent.AddVelocity(sourceRb.velocity * inheritVelocityMultiplier);
                }
            }

            // 머즐 플래시
            if (muzzleFlashPrefab != null)
            {
                var muzzleFlash = GameObject.Instantiate(
                    muzzleFlashPrefab,
                    spawnPosition,
                    Quaternion.LookRotation(Vector3.forward, direction)
                );
                GameObject.Destroy(muzzleFlash, 0.5f);
            }

            // 발사 사운드
            if (fireSound != null)
            {
                AudioSource.PlayClipAtPoint(fireSound, spawnPosition);
            }
        }

        private Vector3 GetFireDirection(AbilitySystemComponent source, AbilityActivationInfo activationInfo)
        {
            // 자동 조준
            if (autoAim)
            {
                GameObject nearestTarget = FindNearestTarget(source);
                if (nearestTarget != null)
                {
                    return (nearestTarget.transform.position - source.transform.position).normalized;
                }
            }

            // 타겟이 지정되어 있으면 타겟 방향
            if (activationInfo.target != null)
            {
                return (activationInfo.target.transform.position - source.transform.position).normalized;
            }

            // 입력 방향
            if (activationInfo.activationDirection != Vector3.zero)
            {
                return activationInfo.activationDirection.normalized;
            }

            // 마우스 방향 (2D)
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

            // 기본 방향
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
            return 10f * projectileCount; // 투사체당 10 마나
        }
    }

    /// <summary>
    /// 투사체 컴포넌트
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

            // Rigidbody 설정
            rb = GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody2D>();
            }
            rb.gravityScale = 0;
            rb.velocity = velocity;

            // Collider 설정
            var col = GetComponent<Collider2D>();
            if (col == null)
            {
                col = gameObject.AddComponent<CircleCollider2D>();
            }
            col.isTrigger = true;

            // 자동 파괴
            Destroy(gameObject, lifetime);

            // 방향에 따른 회전
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
            // 소유자는 무시
            if (other.gameObject == owner)
                return;

            // 레이어 체크
            if ((targetLayer.value & (1 << other.gameObject.layer)) == 0)
                return;

            // 데미지 적용
            ApplyDamage(other.gameObject);

            // 투사체 파괴
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
                // 직접 데미지
                var attributes = target.GetComponent<AttributeSystem.AttributeSetComponent>();
                if (attributes != null)
                {
                    attributes.ModifyAttribute(AttributeType.Health, -damage);
                }
            }
        }

        private void CreateHitEffect()
        {
            // 히트 이펙트 생성 (프리팹이 있다면)
            // TODO: 히트 이펙트 구현
        }
    }
}