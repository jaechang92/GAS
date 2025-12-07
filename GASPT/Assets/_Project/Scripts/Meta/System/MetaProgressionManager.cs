using System;
using System.Collections.Generic;
using UnityEngine;
using GASPT.Save;

namespace GASPT.Meta
{
    /// <summary>
    /// 메타 진행 시스템 핵심 관리자
    /// - 메타 재화 (Bone/Soul) 관리
    /// - 영구 업그레이드 관리
    /// - 저장/로드 (ISaveable)
    /// </summary>
    public class MetaProgressionManager : SingletonManager<MetaProgressionManager>, ISaveable
    {
        // ====== 설정 ======

        [Header("업그레이드 데이터")]
        [Tooltip("모든 업그레이드 ScriptableObject를 Resources에서 로드")]
        [SerializeField]
        private string upgradesResourcePath = "Data/Meta/Upgrades";


        // ====== 런타임 데이터 ======

        /// <summary>
        /// 메타 재화 관리
        /// </summary>
        private MetaCurrency currency;

        /// <summary>
        /// 저장 데이터
        /// </summary>
        private PlayerMetaProgress progress;

        /// <summary>
        /// 로드된 업그레이드 목록
        /// </summary>
        private Dictionary<string, PermanentUpgrade> upgrades;


        // ====== 프로퍼티 ======

        /// <summary>
        /// 재화 접근자
        /// </summary>
        public MetaCurrency Currency => currency;

        /// <summary>
        /// 진행 데이터 접근자
        /// </summary>
        public PlayerMetaProgress Progress => progress;

        /// <summary>
        /// 현재 런 중인지 여부
        /// </summary>
        public bool IsInRun { get; private set; }


        // ====== 이벤트 ======

        /// <summary>
        /// 업그레이드 구매 완료 이벤트 (업그레이드ID, 새 레벨)
        /// </summary>
        public event Action<string, int> OnUpgradePurchased;

        /// <summary>
        /// 런 시작 이벤트
        /// </summary>
        public event Action OnRunStarted;

        /// <summary>
        /// 런 종료 이벤트 (클리어 여부)
        /// </summary>
        public event Action<bool> OnRunEnded;

        /// <summary>
        /// 진행 데이터 로드 완료 이벤트
        /// </summary>
        public event Action OnProgressLoaded;


        // ====== 초기화 ======

        protected override void OnAwake()
        {
            currency = new MetaCurrency();
            progress = new PlayerMetaProgress();
            upgrades = new Dictionary<string, PermanentUpgrade>();

            LoadUpgradeDefinitions();
            LoadProgress();

            Debug.Log($"[MetaProgressionManager] 초기화 완료 - {currency}");
        }

        /// <summary>
        /// Resources에서 업그레이드 정의 로드
        /// </summary>
        private void LoadUpgradeDefinitions()
        {
            upgrades.Clear();

            PermanentUpgrade[] loadedUpgrades = Resources.LoadAll<PermanentUpgrade>(upgradesResourcePath);

            foreach (var upgrade in loadedUpgrades)
            {
                if (upgrade != null && !string.IsNullOrEmpty(upgrade.upgradeId))
                {
                    if (!upgrades.ContainsKey(upgrade.upgradeId))
                    {
                        upgrades[upgrade.upgradeId] = upgrade;
                        Debug.Log($"[MetaProgressionManager] 업그레이드 로드: {upgrade.upgradeId} - {upgrade.upgradeName}");
                    }
                    else
                    {
                        Debug.LogWarning($"[MetaProgressionManager] 중복 업그레이드 ID: {upgrade.upgradeId}");
                    }
                }
            }

            Debug.Log($"[MetaProgressionManager] 총 {upgrades.Count}개 업그레이드 로드 완료");
        }


        // ====== 업그레이드 관리 ======

        /// <summary>
        /// 업그레이드 정보 가져오기
        /// </summary>
        public PermanentUpgrade GetUpgrade(string upgradeId)
        {
            upgrades.TryGetValue(upgradeId, out var upgrade);
            return upgrade;
        }

        /// <summary>
        /// 모든 업그레이드 가져오기
        /// </summary>
        public IEnumerable<PermanentUpgrade> GetAllUpgrades()
        {
            return upgrades.Values;
        }

        /// <summary>
        /// 현재 업그레이드 레벨 가져오기
        /// </summary>
        public int GetUpgradeLevel(string upgradeId)
        {
            return progress.GetUpgradeLevel(upgradeId);
        }

        /// <summary>
        /// 업그레이드 구매 시도
        /// </summary>
        /// <param name="upgradeId">업그레이드 ID</param>
        /// <returns>성공 여부</returns>
        public bool TryPurchaseUpgrade(string upgradeId)
        {
            if (!upgrades.TryGetValue(upgradeId, out var upgrade))
            {
                Debug.LogError($"[MetaProgressionManager] 업그레이드를 찾을 수 없습니다: {upgradeId}");
                return false;
            }

            int currentLevel = progress.GetUpgradeLevel(upgradeId);

            // 최대 레벨 확인
            if (currentLevel >= upgrade.maxLevel)
            {
                Debug.LogWarning($"[MetaProgressionManager] 이미 최대 레벨입니다: {upgradeId} (Lv{currentLevel})");
                return false;
            }

            // 비용 확인
            int cost = upgrade.GetUpgradeCost(currentLevel);
            if (cost <= 0)
            {
                Debug.LogError($"[MetaProgressionManager] 잘못된 업그레이드 비용: {upgradeId}");
                return false;
            }

            // 선행 조건 확인
            if (!CheckPrerequisites(upgrade))
            {
                Debug.LogWarning($"[MetaProgressionManager] 선행 조건 미충족: {upgradeId}");
                return false;
            }

            // 재화 소비
            if (!currency.TrySpend(upgrade.currencyType, cost))
            {
                Debug.LogWarning($"[MetaProgressionManager] 재화 부족: {upgradeId} (필요: {cost} {upgrade.currencyType})");
                return false;
            }

            // 레벨 증가
            int newLevel = currentLevel + 1;
            progress.SetUpgradeLevel(upgradeId, newLevel);

            // 진행 데이터 동기화
            SyncCurrencyToProgress();

            Debug.Log($"[MetaProgressionManager] 업그레이드 구매 완료: {upgradeId} Lv{currentLevel} → Lv{newLevel}");

            // 이벤트 발생
            OnUpgradePurchased?.Invoke(upgradeId, newLevel);

            // 자동 저장
            SaveProgress();

            return true;
        }

        /// <summary>
        /// 선행 조건 확인
        /// </summary>
        private bool CheckPrerequisites(PermanentUpgrade upgrade)
        {
            if (upgrade.prerequisiteIds == null || upgrade.prerequisiteIds.Length == 0)
            {
                return true;
            }

            foreach (string prereqId in upgrade.prerequisiteIds)
            {
                if (string.IsNullOrEmpty(prereqId)) continue;

                if (!upgrades.TryGetValue(prereqId, out var prereq))
                {
                    Debug.LogWarning($"[MetaProgressionManager] 선행 업그레이드를 찾을 수 없습니다: {prereqId}");
                    continue;
                }

                int prereqLevel = progress.GetUpgradeLevel(prereqId);
                if (prereqLevel < prereq.maxLevel)
                {
                    return false;
                }
            }

            return true;
        }


        // ====== 업그레이드 효과 계산 ======

        /// <summary>
        /// 특정 타입의 총 업그레이드 효과 계산
        /// </summary>
        public float GetTotalUpgradeEffect(UpgradeType type)
        {
            float total = 0f;

            foreach (var kvp in upgrades)
            {
                var upgrade = kvp.Value;
                if (upgrade.upgradeType == type)
                {
                    int level = progress.GetUpgradeLevel(upgrade.upgradeId);
                    if (level > 0)
                    {
                        total += upgrade.GetEffectValue(level);
                    }
                }
            }

            return total;
        }

        /// <summary>
        /// 최대 HP 보너스
        /// </summary>
        public int GetMaxHPBonus()
        {
            return Mathf.RoundToInt(GetTotalUpgradeEffect(UpgradeType.MaxHP));
        }

        /// <summary>
        /// 공격력 보너스 (%)
        /// </summary>
        public float GetAttackBonus()
        {
            return GetTotalUpgradeEffect(UpgradeType.Attack) / 100f;
        }

        /// <summary>
        /// 방어력 보너스 (받는 피해 감소 %)
        /// </summary>
        public float GetDefenseBonus()
        {
            return GetTotalUpgradeEffect(UpgradeType.Defense) / 100f;
        }

        /// <summary>
        /// 이동속도 보너스 (%)
        /// </summary>
        public float GetMoveSpeedBonus()
        {
            return GetTotalUpgradeEffect(UpgradeType.MoveSpeed) / 100f;
        }

        /// <summary>
        /// 골드 획득량 보너스 (%)
        /// </summary>
        public float GetGoldBonus()
        {
            return GetTotalUpgradeEffect(UpgradeType.GoldBonus) / 100f;
        }

        /// <summary>
        /// 경험치 획득량 보너스 (%)
        /// </summary>
        public float GetExpBonus()
        {
            return GetTotalUpgradeEffect(UpgradeType.ExpBonus) / 100f;
        }

        /// <summary>
        /// 시작 골드
        /// </summary>
        public int GetStartGold()
        {
            return Mathf.RoundToInt(GetTotalUpgradeEffect(UpgradeType.StartGold));
        }

        /// <summary>
        /// 추가 대시 횟수
        /// </summary>
        public int GetExtraDash()
        {
            return Mathf.RoundToInt(GetTotalUpgradeEffect(UpgradeType.ExtraDash));
        }

        /// <summary>
        /// 부활 횟수
        /// </summary>
        public int GetReviveCount()
        {
            return Mathf.RoundToInt(GetTotalUpgradeEffect(UpgradeType.Revive));
        }


        // ====== 런 관리 ======

        /// <summary>
        /// 런 시작
        /// </summary>
        public void StartRun()
        {
            if (IsInRun)
            {
                Debug.LogWarning("[MetaProgressionManager] 이미 런 진행 중입니다.");
                return;
            }

            IsInRun = true;
            currency.ResetTempCurrency();
            progress.totalRuns++;

            Debug.Log($"[MetaProgressionManager] 런 시작 (총 {progress.totalRuns}회차)");

            OnRunStarted?.Invoke();
        }

        /// <summary>
        /// 런 종료
        /// </summary>
        /// <param name="cleared">클리어 여부</param>
        /// <param name="stageReached">도달 스테이지</param>
        /// <param name="enemiesKilled">처치한 적 수</param>
        public void EndRun(bool cleared, int stageReached = 0, int enemiesKilled = 0)
        {
            if (!IsInRun)
            {
                Debug.LogWarning("[MetaProgressionManager] 런 진행 중이 아닙니다.");
                return;
            }

            IsInRun = false;

            // 임시 재화 확정
            currency.ConfirmTempCurrency();

            // 통계 업데이트
            if (cleared)
            {
                progress.totalClears++;
            }
            else
            {
                progress.totalDeaths++;
            }

            if (stageReached > progress.highestStage)
            {
                progress.highestStage = stageReached;
            }

            progress.totalEnemiesKilled += enemiesKilled;

            // 진행 데이터 동기화
            SyncCurrencyToProgress();

            Debug.Log($"[MetaProgressionManager] 런 종료 (클리어: {cleared}, 스테이지: {stageReached}, 처치: {enemiesKilled})");
            Debug.Log($"[MetaProgressionManager] {currency}");

            // 자동 저장
            SaveProgress();

            OnRunEnded?.Invoke(cleared);
        }


        // ====== 재화 동기화 ======

        /// <summary>
        /// 재화를 진행 데이터에 동기화
        /// </summary>
        private void SyncCurrencyToProgress()
        {
            progress.bone = currency.Bone;
            progress.soul = currency.Soul;
        }

        /// <summary>
        /// 진행 데이터를 재화에 동기화
        /// </summary>
        private void SyncProgressToCurrency()
        {
            currency.SetFromSaveData(progress.bone, progress.soul);
        }


        // ====== ISaveable 구현 ======

        public string SaveID => "MetaProgression";

        object ISaveable.GetSaveData()
        {
            SyncCurrencyToProgress();
            progress.UpdateSaveTime();
            return progress;
        }

        void ISaveable.LoadFromSaveData(object data)
        {
            if (data is PlayerMetaProgress loadedProgress)
            {
                progress = loadedProgress;
                SyncProgressToCurrency();

                Debug.Log($"[MetaProgressionManager] 진행 데이터 로드 완료: {progress}");
                OnProgressLoaded?.Invoke();
            }
            else
            {
                Debug.LogError($"[MetaProgressionManager] 잘못된 저장 데이터 타입: {data?.GetType().Name}");
            }
        }


        // ====== 저장/로드 (PlayerPrefs 기반) ======

        private const string SaveKey = "GASPT_MetaProgress";

        /// <summary>
        /// 진행 데이터 저장
        /// </summary>
        public void SaveProgress()
        {
            try
            {
                SyncCurrencyToProgress();
                progress.UpdateSaveTime();

                string json = JsonUtility.ToJson(progress, true);
                PlayerPrefs.SetString(SaveKey, json);
                PlayerPrefs.Save();

                Debug.Log($"[MetaProgressionManager] 저장 완료: {progress.saveTime}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[MetaProgressionManager] 저장 실패: {e.Message}");
            }
        }

        /// <summary>
        /// 진행 데이터 로드
        /// </summary>
        public void LoadProgress()
        {
            try
            {
                if (PlayerPrefs.HasKey(SaveKey))
                {
                    string json = PlayerPrefs.GetString(SaveKey);
                    progress = JsonUtility.FromJson<PlayerMetaProgress>(json);

                    if (progress == null)
                    {
                        progress = new PlayerMetaProgress();
                    }

                    SyncProgressToCurrency();

                    Debug.Log($"[MetaProgressionManager] 로드 완료: {progress}");
                    OnProgressLoaded?.Invoke();
                }
                else
                {
                    Debug.Log("[MetaProgressionManager] 저장 데이터 없음 - 새 게임");
                    progress = new PlayerMetaProgress();
                    currency = new MetaCurrency();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[MetaProgressionManager] 로드 실패: {e.Message}");
                progress = new PlayerMetaProgress();
                currency = new MetaCurrency();
            }
        }

        /// <summary>
        /// 진행 데이터 초기화 (테스트용)
        /// </summary>
        public void ResetProgress()
        {
            progress = new PlayerMetaProgress();
            currency = new MetaCurrency();

            PlayerPrefs.DeleteKey(SaveKey);
            PlayerPrefs.Save();

            Debug.Log("[MetaProgressionManager] 진행 데이터 초기화 완료");
        }


        // ====== 디버그 ======

        [ContextMenu("Debug: Print Status")]
        private void DebugPrintStatus()
        {
            Debug.Log("========== MetaProgressionManager ==========");
            Debug.Log($"재화: {currency}");
            Debug.Log($"진행: {progress}");
            Debug.Log($"업그레이드: {upgrades.Count}개 로드됨");
            Debug.Log($"런 진행 중: {IsInRun}");
            Debug.Log("============================================");
        }

        [ContextMenu("Debug: Add 1000 Bone")]
        private void DebugAddBone()
        {
            currency.DebugAddBone(1000);
            SyncCurrencyToProgress();
            SaveProgress();
        }

        [ContextMenu("Debug: Add 100 Soul")]
        private void DebugAddSoul()
        {
            currency.DebugAddSoul(100);
            SyncCurrencyToProgress();
            SaveProgress();
        }

        [ContextMenu("Debug: Reset All Progress")]
        private void DebugResetProgress()
        {
            ResetProgress();
        }

        [ContextMenu("Debug: Simulate Run End")]
        private void DebugSimulateRunEnd()
        {
            if (!IsInRun)
            {
                StartRun();
                currency.AddTempBone(500);
                currency.AddTempSoul(25);
            }
            EndRun(false, 3, 50);
        }
    }
}
