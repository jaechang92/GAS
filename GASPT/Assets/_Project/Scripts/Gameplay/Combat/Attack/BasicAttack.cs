using UnityEngine;
using System.Threading;
using GAS.Core;
using Combat.Hitbox;
using Combat.Core;
using Core.Enums;

namespace Combat.Attack
{
    /// <summary>
    /// 기본 공격 어빌리티
    /// GAS 시스템 기반 근접/원거리 공격 구현
    /// </summary>
    public class BasicAttack : Ability
    {
        [Header("공격 설정")]
        [SerializeField] private GameObject hitboxPrefab;
        [SerializeField] private Transform attackPoint;
        [SerializeField] private Vector2 hitboxSize = new Vector2(1.5f, 1f);
        [SerializeField] private Vector2 hitboxOffset = new Vector2(0.5f, 0f);
        [SerializeField] private float hitboxLifetime = 0.2f;

        [Header("데미지 설정")]
        [SerializeField] private float baseDamage = 10f;
        [SerializeField] private DamageType damageType = DamageType.Physical;
        [SerializeField] private bool canCritical = true;
        [SerializeField] private float criticalMultiplier = 1.5f;

        [Header("넉백 설정")]
        [SerializeField] private bool applyKnockback = true;
        [SerializeField] private Vector2 knockbackForce = new Vector2(5f, 2f);
        [SerializeField] private bool useHitDirection = true;

        [Header("타겟 설정")]
        [SerializeField] private LayerMask targetLayers;
        [SerializeField] private bool hitMultipleTargets = true;
        [SerializeField] private int maxTargets = 5;

        [Header("콤보 설정")]
        [SerializeField] private int comboIndex = 0;
        [SerializeField] private float comboWindow = 0.5f;

        [Header("디버그")]
        [SerializeField] private bool drawDebugGizmos = true;

        private GameObject currentHitbox;
        private ComboSystem comboSystem;

        /// <summary>
        /// 초기화 (ComboSystem 연결)
        /// </summary>
        public void InitializeWithCombo(ComboSystem combo)
        {
            comboSystem = combo;
        }

        /// <summary>
        /// 공격 실행
        /// </summary>
        protected override async Awaitable ExecuteActiveAbility(CancellationToken cancellationToken)
        {
            // 애니메이션 트리거
            TriggerAnimation();

            // 사운드 재생
            PlaySound();

            // 이펙트 생성
            SpawnEffect();

            // 히트박스 생성 및 활성화
            await CreateAndActivateHitbox(cancellationToken);

            // 콤보 시스템 업데이트
            if (comboSystem != null)
            {
                comboSystem.RegisterHit(comboIndex);
            }
        }

        /// <summary>
        /// 히트박스 생성 및 활성화
        /// </summary>
        private async Awaitable CreateAndActivateHitbox(CancellationToken cancellationToken)
        {
            // 히트박스 위치 계산
            Vector3 spawnPosition = CalculateHitboxPosition();
            Quaternion spawnRotation = CalculateHitboxRotation();

            // 히트박스 오브젝트 생성
            if (hitboxPrefab != null)
            {
                currentHitbox = GameObject.Instantiate(hitboxPrefab, spawnPosition, spawnRotation);
            }
            else
            {
                currentHitbox = new GameObject($"Hitbox_{owner.name}_{comboIndex}");
                currentHitbox.transform.position = spawnPosition;
                currentHitbox.transform.rotation = spawnRotation;
            }

            // 히트박스 설정
            SetupHitbox(currentHitbox);

            // 히트박스 활성 시간 대기
            await Awaitable.WaitForSecondsAsync(hitboxLifetime, cancellationToken);

            // 히트박스 제거
            if (currentHitbox != null)
            {
                GameObject.Destroy(currentHitbox);
            }
        }

        /// <summary>
        /// 히트박스 설정
        /// </summary>
        private void SetupHitbox(GameObject hitbox)
        {
            // Collider2D 추가
            var boxCollider = hitbox.GetComponent<BoxCollider2D>();
            if (boxCollider == null)
            {
                boxCollider = hitbox.AddComponent<BoxCollider2D>();
            }
            boxCollider.size = hitboxSize;
            boxCollider.isTrigger = true;

            // HitboxController 설정
            var hitboxController = hitbox.GetComponent<HitboxController>();
            if (hitboxController == null)
            {
                hitboxController = hitbox.AddComponent<HitboxController>();
            }

            // 데미지 설정
            hitboxController.SetDamage(baseDamage, damageType);

            // 넉백 설정
            if (applyKnockback)
            {
                hitboxController.SetKnockback(knockbackForce, useHitDirection);
            }

            // 소유자 설정
            hitboxController.Initialize(owner, owner);

            // 히트박스 활성화
            hitboxController.EnableHitbox();

            // 타격 이벤트 연결
            hitboxController.OnHitTarget += OnHitTarget;
        }

        /// <summary>
        /// 히트박스 위치 계산
        /// </summary>
        private Vector3 CalculateHitboxPosition()
        {
            if (attackPoint != null)
            {
                return attackPoint.position;
            }

            // 캐릭터 방향에 따른 오프셋 적용
            Vector3 direction = owner.transform.right;
            float facingDirection = Mathf.Sign(owner.transform.localScale.x);
            Vector3 offset = new Vector3(hitboxOffset.x * facingDirection, hitboxOffset.y, 0f);

            return owner.transform.position + offset;
        }

        /// <summary>
        /// 히트박스 회전 계산
        /// </summary>
        private Quaternion CalculateHitboxRotation()
        {
            if (attackPoint != null)
            {
                return attackPoint.rotation;
            }

            return owner.transform.rotation;
        }

        /// <summary>
        /// 타격 이벤트 처리
        /// </summary>
        private void OnHitTarget(GameObject target)
        {
            Debug.Log($"[BasicAttack] Hit target: {target.name}");

            // 추가 효과 적용 가능 (예: 카메라 쉐이크, 스크린 플래시 등)
        }

        /// <summary>
        /// 애니메이션 트리거
        /// </summary>
        private void TriggerAnimation()
        {
            if (Data is AbilityData abilityData && !string.IsNullOrEmpty(abilityData.AnimationTrigger))
            {
                var animator = owner.GetComponent<Animator>();
                if (animator != null)
                {
                    animator.SetTrigger(abilityData.AnimationTrigger);
                }
            }
        }

        /// <summary>
        /// 사운드 재생
        /// </summary>
        private void PlaySound()
        {
            if (Data is AbilityData abilityData && abilityData.SoundEffect != null)
            {
                AudioSource.PlayClipAtPoint(abilityData.SoundEffect, owner.transform.position);
            }
        }

        /// <summary>
        /// 이펙트 생성
        /// </summary>
        private void SpawnEffect()
        {
            if (Data is AbilityData abilityData && abilityData.EffectPrefab != null)
            {
                Vector3 effectPosition = CalculateHitboxPosition();
                var effect = GameObject.Instantiate(
                    abilityData.EffectPrefab,
                    effectPosition,
                    CalculateHitboxRotation()
                );
                GameObject.Destroy(effect, 2f);
            }
        }

        /// <summary>
        /// 디버그 기즈모
        /// </summary>
        private void OnDrawGizmos()
        {
            if (!drawDebugGizmos || owner == null) return;

            Gizmos.color = Color.red;
            Vector3 hitboxPos = CalculateHitboxPosition();
            Gizmos.DrawWireCube(hitboxPos, new Vector3(hitboxSize.x, hitboxSize.y, 0.1f));
        }

        /// <summary>
        /// 콤보 인덱스 설정
        /// </summary>
        public void SetComboIndex(int index)
        {
            comboIndex = index;
        }

        /// <summary>
        /// 베이스 데미지 설정
        /// </summary>
        public void SetBaseDamage(float damage)
        {
            baseDamage = damage;
        }

        /// <summary>
        /// 히트박스 크기 설정
        /// </summary>
        public void SetHitboxSize(Vector2 size)
        {
            hitboxSize = size;
        }

        /// <summary>
        /// 공격 가능 여부 확인 (오버라이드)
        /// </summary>
        public override bool CanExecute()
        {
            // 기본 조건 확인
            if (!base.CanExecute()) return false;

            // 추가 조건: 땅에 있어야 공격 가능 (필요시)
            // if (!IsGrounded()) return false;

            return true;
        }
    }
}
