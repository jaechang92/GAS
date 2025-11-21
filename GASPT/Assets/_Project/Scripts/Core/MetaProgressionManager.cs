using System.Collections.Generic;
using UnityEngine;
using GASPT.Save;

namespace GASPT.Core
{
    /// <summary>
    /// 런 간 유지되는 영구 진행도 관리
    /// 메타 골드, Form 언락, 메타 업그레이드 등
    /// </summary>
    public class MetaProgressionManager : SingletonManager<MetaProgressionManager>
    {
        // ====== 메타 데이터 (영구) ======

        /// <summary>
        /// 총 메타 골드
        /// </summary>
        public int TotalGold { get; private set; }

        /// <summary>
        /// 언락된 Form 목록
        /// </summary>
        public HashSet<string> UnlockedForms { get; private set; } = new HashSet<string>();

        /// <summary>
        /// 메타 업그레이드 레벨
        /// Key: 업그레이드 ID, Value: 레벨
        /// </summary>
        public Dictionary<string, int> MetaUpgrades { get; private set; } = new Dictionary<string, int>();

        /// <summary>
        /// 업적 달성 여부
        /// Key: 업적 ID, Value: 달성 여부
        /// </summary>
        public Dictionary<string, bool> Achievements { get; private set; } = new Dictionary<string, bool>();


        // ====== 초기화 ======

        private void Awake()
        {
            // 기본 Form 언락
            UnlockedForms.Add("MageForm");
        }


        // ====== 골드 관리 ======

        /// <summary>
        /// 메타 골드 추가
        /// </summary>
        /// <param name="amount">추가할 골드 양</param>
        public void AddGold(int amount)
        {
            if (amount <= 0)
            {
                Debug.LogWarning($"[MetaProgressionManager] 유효하지 않은 골드 양: {amount}");
                return;
            }

            TotalGold += amount;
            Save();  // 자동 저장

            Debug.Log($"[MetaProgressionManager] 골드 추가: +{amount} (총: {TotalGold})");
        }

        /// <summary>
        /// 메타 골드 소비
        /// </summary>
        /// <param name="amount">소비할 골드 양</param>
        /// <returns>성공 여부</returns>
        public bool SpendGold(int amount)
        {
            if (amount <= 0)
            {
                Debug.LogWarning($"[MetaProgressionManager] 유효하지 않은 골드 양: {amount}");
                return false;
            }

            if (TotalGold >= amount)
            {
                TotalGold -= amount;
                Save();  // 자동 저장

                Debug.Log($"[MetaProgressionManager] 골드 소비: -{amount} (잔액: {TotalGold})");
                return true;
            }
            else
            {
                Debug.LogWarning($"[MetaProgressionManager] 골드 부족: 필요 {amount}, 보유 {TotalGold}");
                return false;
            }
        }


        // ====== Form 관리 ======

        /// <summary>
        /// Form 언락
        /// </summary>
        /// <param name="formId">Form ID (예: "MageForm", "WarriorForm")</param>
        public void UnlockForm(string formId)
        {
            if (string.IsNullOrEmpty(formId))
            {
                Debug.LogWarning("[MetaProgressionManager] 유효하지 않은 Form ID");
                return;
            }

            if (UnlockedForms.Add(formId))
            {
                Save();  // 자동 저장
                Debug.Log($"[MetaProgressionManager] Form 언락: {formId}");
            }
            else
            {
                Debug.Log($"[MetaProgressionManager] 이미 언락된 Form: {formId}");
            }
        }

        /// <summary>
        /// Form이 언락되었는지 확인
        /// </summary>
        /// <param name="formId">Form ID</param>
        /// <returns>언락 여부</returns>
        public bool IsFormUnlocked(string formId)
        {
            return UnlockedForms.Contains(formId);
        }

        /// <summary>
        /// 언락된 Form 개수
        /// </summary>
        public int UnlockedFormCount => UnlockedForms.Count;


        // ====== 메타 업그레이드 관리 ======

        /// <summary>
        /// 메타 스탯 업그레이드
        /// </summary>
        /// <param name="statId">스탯 ID (예: "MaxHP", "StartGold", "CritChance")</param>
        /// <param name="cost">업그레이드 비용</param>
        /// <returns>성공 여부</returns>
        public bool UpgradeMetaStat(string statId, int cost)
        {
            if (string.IsNullOrEmpty(statId))
            {
                Debug.LogWarning("[MetaProgressionManager] 유효하지 않은 스탯 ID");
                return false;
            }

            if (!SpendGold(cost))
            {
                return false;
            }

            // 업그레이드 레벨 증가
            if (!MetaUpgrades.ContainsKey(statId))
            {
                MetaUpgrades[statId] = 0;
            }

            MetaUpgrades[statId]++;
            Save();  // 자동 저장

            Debug.Log($"[MetaProgressionManager] 메타 업그레이드: {statId} Lv.{MetaUpgrades[statId]}");
            return true;
        }

        /// <summary>
        /// 메타 업그레이드 레벨 가져오기
        /// </summary>
        /// <param name="statId">스탯 ID</param>
        /// <returns>레벨 (업그레이드 안 했으면 0)</returns>
        public int GetMetaUpgradeLevel(string statId)
        {
            return MetaUpgrades.GetValueOrDefault(statId, 0);
        }


        // ====== 업적 관리 ======

        /// <summary>
        /// 업적 달성
        /// </summary>
        /// <param name="achievementId">업적 ID</param>
        public void UnlockAchievement(string achievementId)
        {
            if (string.IsNullOrEmpty(achievementId))
            {
                Debug.LogWarning("[MetaProgressionManager] 유효하지 않은 업적 ID");
                return;
            }

            if (!Achievements.ContainsKey(achievementId) || !Achievements[achievementId])
            {
                Achievements[achievementId] = true;
                Save();  // 자동 저장

                Debug.Log($"[MetaProgressionManager] 업적 달성: {achievementId}");
            }
        }

        /// <summary>
        /// 업적이 달성되었는지 확인
        /// </summary>
        /// <param name="achievementId">업적 ID</param>
        /// <returns>달성 여부</returns>
        public bool IsAchievementUnlocked(string achievementId)
        {
            return Achievements.GetValueOrDefault(achievementId, false);
        }


        // ====== 저장/로드 ======

        /// <summary>
        /// 메타 데이터 저장
        /// </summary>
        public void Save()
        {
            SaveSystem saveSystem = SaveSystem.Instance;
            if (saveSystem == null)
            {
                Debug.LogError("[MetaProgressionManager] SaveSystem이 없습니다!");
                return;
            }

            MetaProgressionData saveData = GetSaveData();
            saveSystem.SaveMetaData(saveData);
        }

        /// <summary>
        /// 메타 데이터 로드
        /// </summary>
        public void Load()
        {
            SaveSystem saveSystem = SaveSystem.Instance;
            if (saveSystem == null)
            {
                Debug.LogError("[MetaProgressionManager] SaveSystem이 없습니다!");
                return;
            }

            MetaProgressionData saveData = saveSystem.LoadMetaData();
            LoadFromSaveData(saveData);

            Debug.Log($"[MetaProgressionManager] 메타 데이터 로드 완료 - 골드: {TotalGold}, Form: {UnlockedFormCount}");
        }

        /// <summary>
        /// 현재 메타 데이터를 저장용 구조로 변환
        /// </summary>
        public MetaProgressionData GetSaveData()
        {
            MetaProgressionData data = new MetaProgressionData
            {
                totalGold = TotalGold,
                unlockedForms = new List<string>(UnlockedForms)
            };

            // 메타 업그레이드 변환
            foreach (var upgrade in MetaUpgrades)
            {
                data.metaUpgrades.Add(new MetaUpgradeEntry(upgrade.Key, upgrade.Value));
            }

            // 업적 변환
            foreach (var achievement in Achievements)
            {
                data.achievements.Add(new AchievementEntry(achievement.Key, achievement.Value));
            }

            return data;
        }

        /// <summary>
        /// 저장된 데이터로부터 메타 데이터를 복원
        /// </summary>
        public void LoadFromSaveData(MetaProgressionData data)
        {
            if (data == null)
            {
                Debug.LogWarning("[MetaProgressionManager] LoadFromSaveData(): data가 null입니다.");
                return;
            }

            // 골드 복원
            TotalGold = data.totalGold;

            // Form 복원
            UnlockedForms.Clear();
            foreach (var form in data.unlockedForms)
            {
                UnlockedForms.Add(form);
            }

            // Form이 없으면 기본 Form 추가
            if (UnlockedForms.Count == 0)
            {
                UnlockedForms.Add("MageForm");
            }

            // 메타 업그레이드 복원
            MetaUpgrades.Clear();
            foreach (var upgrade in data.metaUpgrades)
            {
                MetaUpgrades[upgrade.statId] = upgrade.level;
            }

            // 업적 복원
            Achievements.Clear();
            foreach (var achievement in data.achievements)
            {
                Achievements[achievement.achievementId] = achievement.unlocked;
            }
        }


        // ====== 데이터 리셋 (디버그용) ======

        /// <summary>
        /// 모든 메타 데이터 리셋 (디버그용)
        /// </summary>
        [ContextMenu("디버그: 메타 데이터 리셋")]
        private void ResetAllMetaData()
        {
            TotalGold = 0;
            UnlockedForms.Clear();
            UnlockedForms.Add("MageForm");  // 기본 Form은 유지
            MetaUpgrades.Clear();
            Achievements.Clear();

            Save();

            Debug.LogWarning("[MetaProgressionManager] 모든 메타 데이터 리셋됨!");
        }


        // ====== Context Menu (테스트용) ======

        [ContextMenu("테스트: 골드 1000 추가")]
        private void TestAddGold()
        {
            AddGold(1000);
        }

        [ContextMenu("테스트: 테스트 Form 언락")]
        private void TestUnlockForm()
        {
            UnlockForm("TestForm_" + Random.Range(1, 100));
        }

        [ContextMenu("테스트: MaxHP 업그레이드")]
        private void TestUpgradeMaxHP()
        {
            UpgradeMetaStat("MaxHP", 100);
        }

        [ContextMenu("디버그: 메타 데이터 출력")]
        private void DebugLogMetaData()
        {
            Debug.Log("========== Meta Data ==========");
            Debug.Log($"Total Gold: {TotalGold}");
            Debug.Log($"Unlocked Forms: {UnlockedFormCount}");
            foreach (var form in UnlockedForms)
            {
                Debug.Log($"  - {form}");
            }
            Debug.Log($"Meta Upgrades: {MetaUpgrades.Count}");
            foreach (var upgrade in MetaUpgrades)
            {
                Debug.Log($"  - {upgrade.Key}: Lv.{upgrade.Value}");
            }
            Debug.Log($"Achievements: {Achievements.Count}");
            Debug.Log("==============================");
        }
    }
}
