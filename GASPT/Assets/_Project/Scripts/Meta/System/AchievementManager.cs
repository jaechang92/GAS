using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GASPT.Meta.Enums;

namespace GASPT.Meta
{
    /// <summary>
    /// 업적 관리자
    /// 업적 진행도 추적, 완료 처리, 보상 지급
    /// </summary>
    public class AchievementManager : MonoBehaviour
    {
        // ====== 싱글톤 ======

        private static AchievementManager instance;
        public static AchievementManager Instance => instance;


        // ====== 설정 ======

        [Header("리소스 경로")]
        [SerializeField]
        private string achievementsPath = "Data/Meta/Achievements";


        // ====== 런타임 ======

        private Dictionary<string, Achievement> achievements;
        private Dictionary<string, int> progressCache;
        private HashSet<string> completedCache;
        private MetaProgressionManager metaManager;


        // ====== 이벤트 ======

        /// <summary>업적 진행도 업데이트 이벤트 (업적ID, 현재값, 목표값)</summary>
        public event Action<string, int, int> OnProgressUpdated;

        /// <summary>업적 완료 이벤트 (업적)</summary>
        public event Action<Achievement> OnAchievementCompleted;

        /// <summary>보상 수령 이벤트 (업적)</summary>
        public event Action<Achievement> OnRewardClaimed;


        // ====== 프로퍼티 ======

        /// <summary>모든 업적</summary>
        public IEnumerable<Achievement> AllAchievements => achievements.Values;

        /// <summary>완료된 업적 수</summary>
        public int CompletedCount => completedCache.Count;

        /// <summary>전체 업적 수</summary>
        public int TotalCount => achievements.Count;


        // ====== Unity 생명주기 ======

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;

            achievements = new Dictionary<string, Achievement>();
            progressCache = new Dictionary<string, int>();
            completedCache = new HashSet<string>();
        }

        private void Start()
        {
            metaManager = MetaProgressionManager.Instance;

            LoadAchievements();
            LoadProgress();
            SubscribeToEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();

            if (instance == this)
            {
                instance = null;
            }
        }


        // ====== 초기화 ======

        /// <summary>
        /// 업적 정의 로드
        /// </summary>
        private void LoadAchievements()
        {
            achievements.Clear();

            Achievement[] loaded = Resources.LoadAll<Achievement>(achievementsPath);

            foreach (var achievement in loaded)
            {
                if (achievement != null && achievement.IsValid)
                {
                    if (!achievements.ContainsKey(achievement.achievementId))
                    {
                        achievements[achievement.achievementId] = achievement;
                    }
                }
            }

            Debug.Log($"[AchievementManager] {achievements.Count}개 업적 로드 완료");
        }

        /// <summary>
        /// 진행도 로드
        /// </summary>
        private void LoadProgress()
        {
            progressCache.Clear();
            completedCache.Clear();

            if (metaManager != null && metaManager.Progress != null)
            {
                // 진행도 로드
                foreach (var entry in metaManager.Progress.achievementProgress)
                {
                    progressCache[entry.achievementId] = entry.progress;
                }

                // 완료 상태 로드
                foreach (string id in metaManager.Progress.completedAchievements)
                {
                    completedCache.Add(id);
                }
            }

            Debug.Log($"[AchievementManager] 진행도 로드: {progressCache.Count}개, 완료: {completedCache.Count}개");
        }

        /// <summary>
        /// 진행도 저장
        /// </summary>
        private void SaveProgress()
        {
            if (metaManager == null || metaManager.Progress == null) return;

            // 진행도 저장
            metaManager.Progress.achievementProgress.Clear();
            foreach (var kvp in progressCache)
            {
                metaManager.Progress.SetAchievementProgress(kvp.Key, kvp.Value);
            }

            // 완료 상태 저장
            metaManager.Progress.completedAchievements = completedCache.ToList();

            metaManager.SaveProgress();
        }


        // ====== 이벤트 구독 ======

        private void SubscribeToEvents()
        {
            if (metaManager != null)
            {
                metaManager.OnRunEnded += OnRunEnded;
                metaManager.OnUpgradePurchased += OnUpgradePurchased;
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (metaManager != null)
            {
                metaManager.OnRunEnded -= OnRunEnded;
                metaManager.OnUpgradePurchased -= OnUpgradePurchased;
            }
        }


        // ====== 진행도 추적 ======

        /// <summary>
        /// 진행도 증가
        /// </summary>
        public void AddProgress(AchievementType type, int amount = 1)
        {
            foreach (var achievement in achievements.Values)
            {
                if (achievement.achievementType != type) continue;
                if (IsCompleted(achievement.achievementId)) continue;

                int currentProgress = GetProgress(achievement.achievementId);
                int newProgress = currentProgress + amount;

                SetProgress(achievement.achievementId, newProgress);

                // 완료 체크
                if (achievement.IsCompleted(newProgress))
                {
                    CompleteAchievement(achievement);
                }
            }
        }

        /// <summary>
        /// 진행도 직접 설정
        /// </summary>
        public void SetProgress(string achievementId, int value)
        {
            if (!achievements.TryGetValue(achievementId, out var achievement))
            {
                return;
            }

            int clampedValue = Mathf.Clamp(value, 0, achievement.targetValue);
            progressCache[achievementId] = clampedValue;

            OnProgressUpdated?.Invoke(achievementId, clampedValue, achievement.targetValue);

            SaveProgress();
        }

        /// <summary>
        /// 현재 진행도 가져오기
        /// </summary>
        public int GetProgress(string achievementId)
        {
            progressCache.TryGetValue(achievementId, out int progress);
            return progress;
        }


        // ====== 완료 처리 ======

        /// <summary>
        /// 업적 완료 처리
        /// </summary>
        private void CompleteAchievement(Achievement achievement)
        {
            if (achievement == null) return;
            if (completedCache.Contains(achievement.achievementId)) return;

            completedCache.Add(achievement.achievementId);

            Debug.Log($"[AchievementManager] 업적 완료: {achievement.achievementName}");

            // 보상 자동 지급
            if (achievement.HasReward)
            {
                GiveReward(achievement);
            }

            SaveProgress();

            OnAchievementCompleted?.Invoke(achievement);
        }

        /// <summary>
        /// 완료 여부 확인
        /// </summary>
        public bool IsCompleted(string achievementId)
        {
            return completedCache.Contains(achievementId);
        }

        /// <summary>
        /// 보상 지급
        /// </summary>
        private void GiveReward(Achievement achievement)
        {
            if (metaManager == null) return;

            // Bone 보상
            if (achievement.rewardBone > 0)
            {
                metaManager.Currency.DebugAddBone(achievement.rewardBone);
                Debug.Log($"[AchievementManager] Bone 보상: +{achievement.rewardBone}");
            }

            // Soul 보상
            if (achievement.rewardSoul > 0)
            {
                metaManager.Currency.DebugAddSoul(achievement.rewardSoul);
                Debug.Log($"[AchievementManager] Soul 보상: +{achievement.rewardSoul}");
            }

            // TODO: 업그레이드 해금, 폼 해금 처리

            OnRewardClaimed?.Invoke(achievement);
        }


        // ====== 이벤트 콜백 ======

        private void OnRunEnded(bool cleared)
        {
            // 런 클리어 업적
            if (cleared)
            {
                AddProgress(AchievementType.RunClear, 1);
            }

            // 스테이지 도달 업적 체크
            if (metaManager != null)
            {
                int stage = metaManager.Progress.highestStage;
                CheckStageAchievements(stage);
            }
        }

        private void OnUpgradePurchased(string upgradeId, int newLevel)
        {
            AddProgress(AchievementType.UpgradePurchase, 1);
        }

        /// <summary>
        /// 스테이지 도달 업적 체크
        /// </summary>
        private void CheckStageAchievements(int stage)
        {
            foreach (var achievement in achievements.Values)
            {
                if (achievement.achievementType != AchievementType.StageReach) continue;
                if (IsCompleted(achievement.achievementId)) continue;

                if (stage >= achievement.targetValue)
                {
                    SetProgress(achievement.achievementId, achievement.targetValue);
                    CompleteAchievement(achievement);
                }
            }
        }


        // ====== 외부 호출용 ======

        /// <summary>
        /// 적 처치 보고
        /// </summary>
        public void ReportEnemyKill(bool isBoss = false)
        {
            AddProgress(AchievementType.EnemyKill, 1);

            if (isBoss)
            {
                AddProgress(AchievementType.BossKill, 1);
            }
        }

        /// <summary>
        /// 폼 수집 보고
        /// </summary>
        public void ReportFormCollected()
        {
            AddProgress(AchievementType.FormCollect, 1);
        }

        /// <summary>
        /// 폼 각성 보고
        /// </summary>
        public void ReportFormAwakened()
        {
            AddProgress(AchievementType.FormAwaken, 1);
        }

        /// <summary>
        /// 재화 획득 보고
        /// </summary>
        public void ReportCurrencyEarned(CurrencyType type, int amount)
        {
            if (type == CurrencyType.Bone || type == CurrencyType.Soul)
            {
                AddProgress(AchievementType.CurrencyEarn, amount);
            }
        }


        // ====== 쿼리 ======

        /// <summary>
        /// 업적 가져오기
        /// </summary>
        public Achievement GetAchievement(string achievementId)
        {
            achievements.TryGetValue(achievementId, out var achievement);
            return achievement;
        }

        /// <summary>
        /// 완료된 업적 목록
        /// </summary>
        public IEnumerable<Achievement> GetCompletedAchievements()
        {
            return achievements.Values.Where(a => IsCompleted(a.achievementId));
        }

        /// <summary>
        /// 미완료 업적 목록
        /// </summary>
        public IEnumerable<Achievement> GetIncompleteAchievements()
        {
            return achievements.Values.Where(a => !IsCompleted(a.achievementId));
        }

        /// <summary>
        /// 타입별 업적 목록
        /// </summary>
        public IEnumerable<Achievement> GetAchievementsByType(AchievementType type)
        {
            return achievements.Values.Where(a => a.achievementType == type);
        }

        /// <summary>
        /// 등급별 업적 목록
        /// </summary>
        public IEnumerable<Achievement> GetAchievementsByTier(AchievementTier tier)
        {
            return achievements.Values.Where(a => a.tier == tier);
        }


        // ====== 디버그 ======

        [ContextMenu("Debug: Print Status")]
        private void DebugPrintStatus()
        {
            Debug.Log("========== AchievementManager ==========");
            Debug.Log($"전체: {TotalCount}개, 완료: {CompletedCount}개");

            Debug.Log("--- 업적 목록 ---");
            foreach (var achievement in achievements.Values)
            {
                int progress = GetProgress(achievement.achievementId);
                string status = IsCompleted(achievement.achievementId) ? "완료" : $"{progress}/{achievement.targetValue}";
                Debug.Log($"  [{achievement.tier}] {achievement.achievementName}: {status}");
            }
            Debug.Log("=========================================");
        }

        [ContextMenu("Debug: Complete All")]
        private void DebugCompleteAll()
        {
            foreach (var achievement in achievements.Values)
            {
                if (!IsCompleted(achievement.achievementId))
                {
                    SetProgress(achievement.achievementId, achievement.targetValue);
                    CompleteAchievement(achievement);
                }
            }
        }

        [ContextMenu("Debug: Reset All")]
        private void DebugResetAll()
        {
            progressCache.Clear();
            completedCache.Clear();
            SaveProgress();
            Debug.Log("[AchievementManager] 모든 업적 초기화");
        }
    }
}
