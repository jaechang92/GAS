using UnityEngine;
using Combat.Core;

namespace Combat.Hitbox
{
    /// <summary>
    /// 허트박스 컨트롤러
    /// 피격 판정을 담당하는 컴포넌트
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class HurtboxController : MonoBehaviour
    {
        [Header("연결")]
        [SerializeField] private HealthSystem healthSystem;
        [SerializeField] private bool autoFindHealthSystem = true;

        [Header("피격 설정")]
        [SerializeField] private float damageMultiplier = 1f;
        [SerializeField] private bool canBeHit = true;

        [Header("디버그")]
        [SerializeField] private bool enableDebugLogs = false;
        [SerializeField] private bool drawGizmos = true;

        private Collider2D hurtboxCollider;

        // 이벤트
        public event System.Action<DamageData> OnHit;
        public event System.Action<GameObject> OnHitBy;

        #region 프로퍼티

        /// <summary>
        /// 피격 가능 여부
        /// </summary>
        public bool CanBeHit
        {
            get => canBeHit;
            set => canBeHit = value;
        }

        /// <summary>
        /// 체력 시스템
        /// </summary>
        public HealthSystem HealthSystem => healthSystem;

        #endregion

        #region Unity 생명주기

        private void Awake()
        {
            hurtboxCollider = GetComponent<Collider2D>();
            hurtboxCollider.isTrigger = true;

            // HealthSystem 자동 탐색
            if (autoFindHealthSystem && healthSystem == null)
            {
                healthSystem = GetComponentInParent<HealthSystem>();

                if (healthSystem == null)
                {
                    healthSystem = GetComponent<HealthSystem>();
                }

                if (healthSystem == null)
                {
                    Debug.LogWarning($"[HurtboxController - {gameObject.name}] HealthSystem을 찾을 수 없습니다!");
                }
            }
        }

        #endregion

        #region 피격 처리

        /// <summary>
        /// 히트박스로부터 타격받기
        /// </summary>
        public bool TakeHit(HitboxController hitbox)
        {
            if (!canBeHit) return false;
            if (healthSystem == null) return false;

            // 히트박스의 데미지 정보를 가져와서 적용
            // (HitboxController가 직접 DamageSystem을 통해 처리하므로 여기서는 이벤트만 발생)

            OnHitBy?.Invoke(hitbox.gameObject);
            LogDebug($"타격받음: {hitbox.gameObject.name}");

            return true;
        }

        /// <summary>
        /// 데미지 받기 (직접 호출)
        /// </summary>
        public bool TakeDamage(DamageData damage)
        {
            if (!canBeHit) return false;
            if (healthSystem == null) return false;

            // 데미지 배율 적용
            damage.ApplyMultiplier(damageMultiplier);

            // 체력 시스템에 적용
            bool success = healthSystem.TakeDamage(damage);

            if (success)
            {
                OnHit?.Invoke(damage);
                LogDebug($"데미지 받음: {damage.amount}");
            }

            return success;
        }

        #endregion

        #region 허트박스 제어

        /// <summary>
        /// 허트박스 활성화
        /// </summary>
        public void EnableHurtbox()
        {
            canBeHit = true;
            hurtboxCollider.enabled = true;
            LogDebug("허트박스 활성화");
        }

        /// <summary>
        /// 허트박스 비활성화
        /// </summary>
        public void DisableHurtbox()
        {
            canBeHit = false;
            hurtboxCollider.enabled = false;
            LogDebug("허트박스 비활성화");
        }

        /// <summary>
        /// HealthSystem 설정
        /// </summary>
        public void SetHealthSystem(HealthSystem health)
        {
            healthSystem = health;
        }

        /// <summary>
        /// 데미지 배율 설정
        /// </summary>
        public void SetDamageMultiplier(float multiplier)
        {
            damageMultiplier = multiplier;
        }

        #endregion

        #region 디버그

        private void LogDebug(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[HurtboxController - {gameObject.name}] {message}");
            }
        }

        private void OnDrawGizmos()
        {
            if (!drawGizmos) return;

            var col = GetComponent<Collider2D>();
            if (col == null) return;

            Gizmos.color = canBeHit ? Color.green : Color.gray;

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
