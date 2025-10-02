using UnityEngine;
using System.Collections.Generic;
using Core.Enums;

namespace Combat.Core
{
    /// <summary>
    /// 데미지 계산 및 처리를 담당하는 중앙 시스템
    /// 데미지 배율, 방어력, 속성 상성 등을 처리
    /// </summary>
    public class DamageSystem : MonoBehaviour
    {
        [Header("전역 데미지 설정")]
        [SerializeField] private float globalDamageMultiplier = 1f;
        [SerializeField] private bool enableCriticalHits = true;
        [SerializeField] private float baseCriticalChance = 0.1f;  // 10% 기본 크리티컬 확률

        [Header("디버그")]
        [SerializeField] private bool enableDebugLogs = true;

        // 싱글톤 인스턴스
        private static DamageSystem instance;
        public static DamageSystem Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<DamageSystem>();

                    if (instance == null)
                    {
                        var go = new GameObject("DamageSystem");
                        instance = go.AddComponent<DamageSystem>();
                    }
                }
                return instance;
            }
        }

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;

                // PlayMode에서만 DontDestroyOnLoad 호출
                if (Application.isPlaying)
                {
                    DontDestroyOnLoad(gameObject);
                }
            }
            else if (instance != this)
            {
                if (Application.isPlaying)
                {
                    Destroy(gameObject);
                }
                else
                {
                    DestroyImmediate(gameObject);
                }
            }
        }

        #region 데미지 처리

        /// <summary>
        /// 데미지 적용 (자동으로 HealthSystem 찾아서 적용)
        /// </summary>
        public static bool ApplyDamage(GameObject target, DamageData damage)
        {
            if (target == null)
            {
                Debug.LogWarning("[DamageSystem] Target is null!");
                return false;
            }

            var healthSystem = target.GetComponent<HealthSystem>();
            if (healthSystem == null)
            {
                Debug.LogWarning($"[DamageSystem] {target.name}에 HealthSystem이 없습니다!");
                return false;
            }

            // 데미지 계산
            var processedDamage = Instance.CalculateDamage(damage);

            // 적용
            return healthSystem.TakeDamage(processedDamage);
        }

        /// <summary>
        /// 간단한 데미지 적용
        /// </summary>
        public static bool ApplyDamage(GameObject target, float amount, DamageType type, GameObject source)
        {
            var damage = DamageData.Create(amount, type, source);
            return ApplyDamage(target, damage);
        }

        /// <summary>
        /// 넉백 포함 데미지 적용
        /// </summary>
        public static bool ApplyDamageWithKnockback(GameObject target, float amount, DamageType type, GameObject source, Vector2 knockback)
        {
            var damage = DamageData.CreateWithKnockback(amount, type, source, knockback);
            return ApplyDamage(target, damage);
        }

        #endregion

        #region 데미지 계산

        /// <summary>
        /// 데미지 계산 (배율, 크리티컬 등 적용)
        /// </summary>
        private DamageData CalculateDamage(DamageData baseDamage)
        {
            var damage = baseDamage;

            // 1. 전역 배율 적용
            damage.ApplyMultiplier(globalDamageMultiplier);

            // 2. 크리티컬 판정
            if (enableCriticalHits && damage.canCritical)
            {
                if (Random.value <= baseCriticalChance)
                {
                    damage.ApplyCritical();
                    LogDebug($"크리티컬 히트! 데미지: {damage.amount}");
                }
            }

            // 3. 데미지 타입별 처리
            damage = ApplyDamageTypeModifiers(damage);

            return damage;
        }

        /// <summary>
        /// 데미지 타입별 수정자 적용
        /// </summary>
        private DamageData ApplyDamageTypeModifiers(DamageData damage)
        {
            switch (damage.damageType)
            {
                case DamageType.Physical:
                    // 물리 데미지는 방어력 영향을 받음 (추후 구현)
                    break;

                case DamageType.Magical:
                    // 마법 데미지는 마법 방어력 영향을 받음 (추후 구현)
                    break;

                case DamageType.True:
                    // 고정 데미지는 모든 방어 무시
                    break;

                case DamageType.Environmental:
                    // 환경 데미지는 특수 처리
                    damage.ignoreInvincibility = true;
                    break;
            }

            return damage;
        }

        #endregion

        #region 범위 데미지

        /// <summary>
        /// 구형 범위 데미지
        /// </summary>
        public static List<GameObject> ApplyRadialDamage(Vector3 center, float radius, DamageData damage, LayerMask targetLayers)
        {
            var hitTargets = new List<GameObject>();
            var colliders = Physics2D.OverlapCircleAll(center, radius, targetLayers);

            foreach (var col in colliders)
            {
                if (ApplyDamage(col.gameObject, damage))
                {
                    hitTargets.Add(col.gameObject);
                }
            }

            Instance.LogDebug($"범위 데미지: {hitTargets.Count}개 타격");
            return hitTargets;
        }

        /// <summary>
        /// 박스 범위 데미지
        /// </summary>
        public static List<GameObject> ApplyBoxDamage(Vector3 center, Vector2 size, float angle, DamageData damage, LayerMask targetLayers)
        {
            var hitTargets = new List<GameObject>();
            var colliders = Physics2D.OverlapBoxAll(center, size, angle, targetLayers);

            foreach (var col in colliders)
            {
                if (ApplyDamage(col.gameObject, damage))
                {
                    hitTargets.Add(col.gameObject);
                }
            }

            Instance.LogDebug($"박스 데미지: {hitTargets.Count}개 타격");
            return hitTargets;
        }

        #endregion

        #region 설정

        /// <summary>
        /// 전역 데미지 배율 설정
        /// </summary>
        public void SetGlobalDamageMultiplier(float multiplier)
        {
            globalDamageMultiplier = multiplier;
            LogDebug($"전역 데미지 배율 설정: {multiplier}");
        }

        /// <summary>
        /// 크리티컬 확률 설정
        /// </summary>
        public void SetCriticalChance(float chance)
        {
            baseCriticalChance = Mathf.Clamp01(chance);
            LogDebug($"크리티컬 확률 설정: {baseCriticalChance * 100}%");
        }

        #endregion

        #region 디버그

        private void LogDebug(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[DamageSystem] {message}");
            }
        }

        private void OnDrawGizmos()
        {
            // 범위 데미지 시각화 (디버그용)
        }

        #endregion
    }
}
