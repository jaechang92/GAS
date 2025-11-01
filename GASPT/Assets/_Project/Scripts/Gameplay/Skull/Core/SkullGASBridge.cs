using UnityEngine;
using GAS.Core;
using Gameplay.Common;
using System.Collections.Generic;
using System.Linq;

namespace Skull.Core
{
    /// <summary>
    /// 스컬 시스템과 GAS_Core 간의 브리지 클래스
    /// 스컬 변경 시 GAS 시스템의 어빌리티와 리소스를 동적으로 관리
    /// </summary>
    [RequireComponent(typeof(AbilitySystem))]
    public class SkullGASBridge : MonoBehaviour
    {
        [Header("GAS 연동 설정")]
        [SerializeField] private bool enableAutoSync = true;
        [SerializeField] private bool enableDebugLogs = true;
        [SerializeField] private bool preserveResourceRatio = true;

        [Header("리소스 매핑")]
        [SerializeField] private string healthResourceName = "Health";
        [SerializeField] private string manaResourceName = "Mana";
        [SerializeField] private string staminaResourceName = "Stamina";

        // 컴포넌트 참조
        private AbilitySystem abilitySystem;
        private SkullManager skullManager;

        // 상태 관리
        private ISkullController currentManagedSkull;
        private Dictionary<string, float> previousResourceRatios = new Dictionary<string, float>();
        private List<Ability> previousSkullAbilities = new List<Ability>();

        #region Unity 생명주기

        private void Awake()
        {
            InitializeComponents();
        }

        private void Start()
        {
            InitializeBridge();
        }

        private void OnDestroy()
        {
            CleanupBridge();
        }

        #endregion

        #region 초기화

        /// <summary>
        /// 컴포넌트 참조 초기화
        /// </summary>
        private void InitializeComponents()
        {
            abilitySystem = GetComponent<AbilitySystem>();
            skullManager = GetComponent<SkullManager>();

            if (abilitySystem == null)
            {
                LogError("AbilitySystem 컴포넌트가 필요합니다.");
            }

            if (skullManager == null)
            {
                LogError("SkullManager 컴포넌트가 필요합니다.");
            }
        }

        /// <summary>
        /// 브리지 초기화
        /// </summary>
        private void InitializeBridge()
        {
            if (skullManager == null || abilitySystem == null) return;

            // 이벤트 구독
            skullManager.OnSkullChanged += OnSkullChanged;
            skullManager.OnSkullEquipped += OnSkullEquipped;
            skullManager.OnSkullUnequipped += OnSkullUnequipped;

            // 초기 스컬이 있다면 동기화
            if (skullManager.CurrentSkull != null)
            {
                SyncSkullToGAS(skullManager.CurrentSkull);
            }

            LogDebug("GAS 브리지 초기화 완료");
        }

        #endregion

        #region 스컬 이벤트 핸들러

        /// <summary>
        /// 스컬 변경 이벤트 핸들러
        /// </summary>
        private void OnSkullChanged(ISkullController previousSkull, ISkullController newSkull)
        {
            LogDebug($"스컬 변경 감지: {previousSkull?.SkullData?.SkullName} → {newSkull?.SkullData?.SkullName}");

            if (enableAutoSync)
            {
                // 이전 스컬 정리
                if (previousSkull != null)
                {
                    CleanupSkullFromGAS(previousSkull);
                }

                // 새 스컬 동기화
                if (newSkull != null)
                {
                    SyncSkullToGAS(newSkull);
                }
            }
        }

        /// <summary>
        /// 스컬 장착 이벤트 핸들러
        /// </summary>
        private void OnSkullEquipped(ISkullController skull)
        {
            LogDebug($"스컬 장착: {skull?.SkullData?.SkullName}");

            if (enableAutoSync && skull != null)
            {
                SyncSkullToGAS(skull);
            }
        }

        /// <summary>
        /// 스컬 해제 이벤트 핸들러
        /// </summary>
        private void OnSkullUnequipped(ISkullController skull)
        {
            LogDebug($"스컬 해제: {skull?.SkullData?.SkullName}");

            if (enableAutoSync && skull != null)
            {
                CleanupSkullFromGAS(skull);
            }
        }

        #endregion

        #region GAS 동기화

        /// <summary>
        /// 스컬을 GAS 시스템에 동기화
        /// </summary>
        public void SyncSkullToGAS(ISkullController skull)
        {
            if (skull?.SkullData == null || abilitySystem == null) return;

            LogDebug($"GAS 동기화 시작: {skull.SkullData.SkullName}");

            // 리소스 비율 저장 (선택적)
            if (preserveResourceRatio)
            {
                SaveCurrentResourceRatios();
            }

            // 능력치 동기화
            SyncSkullStats(skull.SkullData.BaseStats);

            // 어빌리티 동기화
            SyncSkullAbilities(skull.SkullData.SkullAbilities);

            // 리소스 비율 복원 (선택적)
            if (preserveResourceRatio)
            {
                RestoreResourceRatios();
            }

            currentManagedSkull = skull;

            LogDebug($"GAS 동기화 완료: {skull.SkullData.SkullName}");
        }

        /// <summary>
        /// 스컬 능력치를 GAS 리소스에 동기화
        /// </summary>
        private void SyncSkullStats(SkullStats stats)
        {
            if (stats == null) return;

            // 체력 동기화
            SyncResourceMaxAmount(healthResourceName, stats.MaxHealth);

            // 마나 동기화
            SyncResourceMaxAmount(manaResourceName, stats.MaxMana);

            LogDebug($"능력치 동기화: HP={stats.MaxHealth}, MP={stats.MaxMana}");
        }

        /// <summary>
        /// 리소스 최대값 동기화
        /// </summary>
        private void SyncResourceMaxAmount(string resourceName, float maxAmount)
        {
            if (string.IsNullOrEmpty(resourceName) || maxAmount <= 0f) return;

            try
            {
                abilitySystem.SetMaxResource(resourceName, maxAmount);
                LogDebug($"리소스 최대값 설정: {resourceName}={maxAmount}");
            }
            catch (System.Exception e)
            {
                LogWarning($"리소스 동기화 실패: {resourceName}, 오류: {e.Message}");
            }
        }

        /// <summary>
        /// 스컬 어빌리티를 GAS에 동기화
        /// </summary>
        private void SyncSkullAbilities(Ability[] skullAbilities)
        {
            if (skullAbilities == null) return;

            // 이전 어빌리티 제거
            RemovePreviousAbilities();

            // 새 어빌리티 추가
            foreach (var ability in skullAbilities)
            {
                if (ability != null)
                {
                    abilitySystem.AddAbility(ability);
                    previousSkullAbilities.Add(ability);
                    LogDebug($"어빌리티 추가: {ability.Name}");
                }
            }

            LogDebug($"어빌리티 동기화 완료: {skullAbilities.Length}개");
        }

        /// <summary>
        /// 이전 어빌리티들 제거
        /// </summary>
        private void RemovePreviousAbilities()
        {
            foreach (var ability in previousSkullAbilities)
            {
                if (ability != null)
                {
                    abilitySystem.RemoveAbility(ability.Id);
                    LogDebug($"어빌리티 제거: {ability.Name}");
                }
            }

            previousSkullAbilities.Clear();
        }

        #endregion

        #region 리소스 비율 관리

        /// <summary>
        /// 현재 리소스 비율 저장
        /// </summary>
        private void SaveCurrentResourceRatios()
        {
            previousResourceRatios.Clear();

            string[] resourceNames = { healthResourceName, manaResourceName, staminaResourceName };

            foreach (string resourceName in resourceNames)
            {
                if (string.IsNullOrEmpty(resourceName)) continue;

                try
                {
                    float currentAmount = abilitySystem.GetResource(resourceName);
                    float maxAmount = abilitySystem.GetMaxResource(resourceName);

                    if (maxAmount > 0f)
                    {
                        float ratio = currentAmount / maxAmount;
                        previousResourceRatios[resourceName] = ratio;
                        LogDebug($"리소스 비율 저장: {resourceName}={ratio:P}");
                    }
                }
                catch (System.Exception e)
                {
                    LogWarning($"리소스 비율 저장 실패: {resourceName}, 오류: {e.Message}");
                }
            }
        }

        /// <summary>
        /// 리소스 비율 복원
        /// </summary>
        private void RestoreResourceRatios()
        {
            foreach (var kvp in previousResourceRatios)
            {
                string resourceName = kvp.Key;
                float ratio = kvp.Value;

                try
                {
                    float maxAmount = abilitySystem.GetMaxResource(resourceName);
                    float targetAmount = maxAmount * ratio;

                    abilitySystem.SetResource(resourceName, targetAmount);
                    LogDebug($"리소스 비율 복원: {resourceName}={ratio:P} ({targetAmount}/{maxAmount})");
                }
                catch (System.Exception e)
                {
                    LogWarning($"리소스 비율 복원 실패: {resourceName}, 오류: {e.Message}");
                }
            }
        }

        #endregion

        #region 정리

        /// <summary>
        /// 스컬을 GAS에서 정리
        /// </summary>
        private void CleanupSkullFromGAS(ISkullController skull)
        {
            if (skull != currentManagedSkull) return;

            LogDebug($"GAS에서 스컬 정리: {skull.SkullData?.SkullName}");

            // 어빌리티 제거
            RemovePreviousAbilities();

            currentManagedSkull = null;
        }

        /// <summary>
        /// 브리지 정리
        /// </summary>
        private void CleanupBridge()
        {
            // 이벤트 구독 해제
            if (skullManager != null)
            {
                skullManager.OnSkullChanged -= OnSkullChanged;
                skullManager.OnSkullEquipped -= OnSkullEquipped;
                skullManager.OnSkullUnequipped -= OnSkullUnequipped;
            }

            // 어빌리티 정리
            RemovePreviousAbilities();

            LogDebug("GAS 브리지 정리 완료");
        }

        #endregion

        #region 공개 API

        /// <summary>
        /// 수동 동기화 실행
        /// </summary>
        public void ManualSync()
        {
            if (skullManager?.CurrentSkull != null)
            {
                SyncSkullToGAS(skullManager.CurrentSkull);
            }
        }

        /// <summary>
        /// 자동 동기화 활성화/비활성화
        /// </summary>
        public void SetAutoSync(bool enabled)
        {
            enableAutoSync = enabled;
            LogDebug($"자동 동기화: {enabled}");
        }

        /// <summary>
        /// 리소스 비율 보존 활성화/비활성화
        /// </summary>
        public void SetPreserveResourceRatio(bool enabled)
        {
            preserveResourceRatio = enabled;
            LogDebug($"리소스 비율 보존: {enabled}");
        }

        /// <summary>
        /// 현재 관리 중인 스컬 정보
        /// </summary>
        public ISkullController GetCurrentManagedSkull()
        {
            return currentManagedSkull;
        }

        /// <summary>
        /// GAS 시스템 참조 가져오기
        /// </summary>
        public AbilitySystem GetAbilitySystem()
        {
            return abilitySystem;
        }

        #endregion

        #region 디버그 및 로깅

        private void LogDebug(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[SkullGASBridge] {message}");
            }
        }

        private void LogWarning(string message)
        {
            Debug.LogWarning($"[SkullGASBridge] {message}");
        }

        private void LogError(string message)
        {
            Debug.LogError($"[SkullGASBridge] {message}");
        }

        #endregion

        #region 에디터 전용

        [ContextMenu("수동 동기화")]
        private void EditorManualSync()
        {
            if (Application.isPlaying)
            {
                ManualSync();
            }
        }

        [ContextMenu("브리지 정보 출력")]
        private void PrintBridgeInfo()
        {
            var currentSkull = GetCurrentManagedSkull();

            Debug.Log($"=== GAS 브리지 정보 ===\n" +
                     $"자동 동기화: {enableAutoSync}\n" +
                     $"리소스 비율 보존: {preserveResourceRatio}\n" +
                     $"현재 관리 스컬: {currentSkull?.SkullData?.SkullName ?? "없음"}\n" +
                     $"이전 어빌리티 수: {previousSkullAbilities.Count}\n" +
                     $"저장된 리소스 비율: {previousResourceRatios.Count}개");

            if (abilitySystem != null)
            {
                Debug.Log($"GAS 시스템 연결: {abilitySystem.name}");

                // 현재 리소스 상태 출력
                try
                {
                    float health = abilitySystem.GetResource(healthResourceName);
                    float maxHealth = abilitySystem.GetMaxResource(healthResourceName);
                    float mana = abilitySystem.GetResource(manaResourceName);
                    float maxMana = abilitySystem.GetMaxResource(manaResourceName);

                    Debug.Log($"현재 리소스: HP={health}/{maxHealth}, MP={mana}/{maxMana}");
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"리소스 정보 조회 실패: {e.Message}");
                }
            }
        }

        #endregion
    }
}
