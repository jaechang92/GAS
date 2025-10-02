using UnityEngine;
using System.Collections.Generic;
using Combat.Core;
using Core.Enums;

namespace Combat.Hitbox
{
    /// <summary>
    /// 히트박스 컨트롤러
    /// 공격 판정을 담당하는 컴포넌트
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class HitboxController : MonoBehaviour
    {
        [Header("데미지 설정")]
        [SerializeField] private float damageAmount = 10f;
        [SerializeField] private DamageType damageType = DamageType.Physical;
        [SerializeField] private bool canCritical = true;
        [SerializeField] private float criticalMultiplier = 1.5f;

        [Header("넉백 설정")]
        [SerializeField] private bool applyKnockback = false;
        [SerializeField] private Vector2 knockbackForce = Vector2.zero;
        [SerializeField] private bool knockbackUseHitDirection = true;

        [Header("타격 설정")]
        [SerializeField] private LayerMask targetLayers;
        [SerializeField] private bool hitOnlyOnce = true;
        [SerializeField] private bool destroyOnHit = false;
        [SerializeField] private float lifetime = 5f;

        [Header("디버그")]
        [SerializeField] private bool enableDebugLogs = false;
        [SerializeField] private bool drawGizmos = true;

        // 공격 소유자
        private GameObject owner;
        private GameObject instigator;

        // 히트 관리
        private HashSet<GameObject> hitTargets = new HashSet<GameObject>();
        private bool isActive = true;
        private Collider2D hitboxCollider;

        // 이벤트
        public event System.Action<GameObject> OnHitTarget;

        #region 프로퍼티

        /// <summary>
        /// 타격한 대상 수
        /// </summary>
        public int HitCount => hitTargets.Count;

        /// <summary>
        /// 활성화 여부
        /// </summary>
        public bool IsActive => isActive;

        #endregion

        #region Unity 생명주기

        private void Awake()
        {
            hitboxCollider = GetComponent<Collider2D>();
            hitboxCollider.isTrigger = true;
        }

        private void Start()
        {
            if (lifetime > 0f)
            {
                Destroy(gameObject, lifetime);
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!isActive) return;

            // 레이어 체크
            if (!IsInLayerMask(collision.gameObject, targetLayers))
            {
                return;
            }

            // 이미 맞은 대상 체크
            if (hitOnlyOnce && hitTargets.Contains(collision.gameObject))
            {
                return;
            }

            // 타격 처리
            if (HitTarget(collision.gameObject))
            {
                hitTargets.Add(collision.gameObject);

                if (destroyOnHit)
                {
                    Destroy(gameObject);
                }
            }
        }

        #endregion

        #region 히트박스 제어

        /// <summary>
        /// 히트박스 초기화
        /// </summary>
        public void Initialize(GameObject owner, GameObject instigator = null)
        {
            this.owner = owner;
            this.instigator = instigator ?? owner;
        }

        /// <summary>
        /// 데미지 설정
        /// </summary>
        public void SetDamage(float damage, DamageType type = DamageType.Physical)
        {
            damageAmount = damage;
            damageType = type;
        }

        /// <summary>
        /// 넉백 설정
        /// </summary>
        public void SetKnockback(Vector2 force, bool useHitDirection = true)
        {
            applyKnockback = true;
            knockbackForce = force;
            knockbackUseHitDirection = useHitDirection;
        }

        /// <summary>
        /// 히트박스 활성화
        /// </summary>
        public void EnableHitbox()
        {
            isActive = true;
            hitboxCollider.enabled = true;
            LogDebug("히트박스 활성화");
        }

        /// <summary>
        /// 히트박스 비활성화
        /// </summary>
        public void DisableHitbox()
        {
            isActive = false;
            hitboxCollider.enabled = false;
            LogDebug("히트박스 비활성화");
        }

        /// <summary>
        /// 히트박스 리셋 (같은 대상 재타격 가능)
        /// </summary>
        public void ResetHitbox()
        {
            hitTargets.Clear();
            LogDebug("히트박스 리셋");
        }

        #endregion

        #region 타격 처리

        /// <summary>
        /// 타겟 타격 처리
        /// </summary>
        private bool HitTarget(GameObject target)
        {
            // DamageData 생성
            var damageData = CreateDamageData(target);

            // 데미지 적용
            bool success = DamageSystem.ApplyDamage(target, damageData);

            if (success)
            {
                LogDebug($"타격 성공: {target.name} ({damageAmount} 데미지)");
                OnHitTarget?.Invoke(target);
            }

            return success;
        }

        /// <summary>
        /// DamageData 생성
        /// </summary>
        private DamageData CreateDamageData(GameObject target)
        {
            var damage = new DamageData
            {
                amount = damageAmount,
                damageType = damageType,
                source = owner ?? gameObject,
                instigator = instigator ?? owner ?? gameObject,
                hitPoint = transform.position,
                canCritical = canCritical,
                criticalMultiplier = criticalMultiplier
            };

            // 넉백 처리
            if (applyKnockback)
            {
                if (knockbackUseHitDirection)
                {
                    // 타격 방향으로 넉백
                    Vector2 direction = (target.transform.position - transform.position).normalized;
                    damage.knockback = direction * knockbackForce.magnitude;
                }
                else
                {
                    // 설정된 방향으로 넉백
                    damage.knockback = knockbackForce;
                }
            }

            return damage;
        }

        #endregion

        #region 헬퍼 메서드

        /// <summary>
        /// 레이어 마스크 체크
        /// </summary>
        private bool IsInLayerMask(GameObject obj, LayerMask layerMask)
        {
            return ((layerMask.value & (1 << obj.layer)) > 0);
        }

        #endregion

        #region 디버그

        private void LogDebug(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[HitboxController - {gameObject.name}] {message}");
            }
        }

        private void OnDrawGizmos()
        {
            if (!drawGizmos) return;

            var col = GetComponent<Collider2D>();
            if (col == null) return;

            Gizmos.color = isActive ? Color.red : Color.gray;

            if (col is BoxCollider2D boxCol)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawWireCube(boxCol.offset, boxCol.size);
            }
            else if (col is CircleCollider2D circleCol)
            {
                Gizmos.DrawWireSphere(transform.position + (Vector3)circleCol.offset, circleCol.radius);
            }
        }

        #endregion
    }
}
