using System;
using System.Collections.Generic;
using UnityEngine;
using GASPT.Data;
using GASPT.Economy;
using GASPT.Level;
using GASPT.Meta;
using GASPT.Loot;
using GASPT.Gameplay.Form;

namespace GASPT.Gameplay.Boss
{
    /// <summary>
    /// 보스 보상 시스템
    /// 보스 처치 시 보상 계산 및 지급
    /// </summary>
    public class BossRewardSystem : MonoBehaviour
    {
        // ====== 싱글톤 ======

        private static BossRewardSystem instance;
        public static BossRewardSystem Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindAnyObjectByType<BossRewardSystem>();

                    if (instance == null)
                    {
                        var go = new GameObject("BossRewardSystem");
                        instance = go.AddComponent<BossRewardSystem>();
                    }
                }
                return instance;
            }
        }

        public static bool HasInstance => instance != null;


        // ====== 설정 ======

        [Header("보너스 배율")]
        [SerializeField]
        [Tooltip("노히트 보너스 (골드)")]
        private float noHitGoldMultiplier = 1.5f;

        [SerializeField]
        [Tooltip("빠른 클리어 보너스 (경험치)")]
        private float fastClearExpMultiplier = 1.3f;

        [SerializeField]
        [Tooltip("빠른 클리어 기준 (제한시간의 비율)")]
        private float fastClearThreshold = 0.5f;


        // ====== 첫 클리어 기록 ======

        private HashSet<string> clearedBosses = new HashSet<string>();


        // ====== 이벤트 ======

        /// <summary>
        /// 보상 지급 완료 이벤트
        /// </summary>
        public event Action<BossRewardInfo> OnRewardGranted;


        // ====== Unity 생명주기 ======

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            LoadClearedBosses();
        }

        private void OnDestroy()
        {
            if (instance == this)
                instance = null;
        }


        // ====== 보상 처리 ======

        /// <summary>
        /// 보스 보상 처리
        /// </summary>
        /// <param name="bossData">보스 데이터</param>
        /// <param name="noHit">피해 없이 클리어 여부</param>
        /// <param name="clearTime">클리어 시간 (초)</param>
        public BossRewardInfo ProcessReward(BossData bossData, bool noHit, float clearTime)
        {
            if (bossData == null)
            {
                Debug.LogError("[BossRewardSystem] bossData가 null입니다.");
                return null;
            }

            var rewardInfo = new BossRewardInfo
            {
                bossId = bossData.bossId,
                bossName = bossData.bossName,
                clearTime = clearTime,
                noHit = noHit
            };

            // 1. 기본 보상 계산
            int baseGold = bossData.goldReward;
            int baseExp = bossData.expReward;

            // 2. 노히트 보너스
            if (noHit)
            {
                rewardInfo.goldBonus = Mathf.RoundToInt(baseGold * (noHitGoldMultiplier - 1f));
                rewardInfo.bonusReasons.Add($"노히트 클리어! 골드 +{(noHitGoldMultiplier - 1f) * 100f:F0}%");
            }

            // 3. 빠른 클리어 보너스
            if (bossData.timeLimit > 0 && clearTime < bossData.timeLimit * fastClearThreshold)
            {
                rewardInfo.expBonus = Mathf.RoundToInt(baseExp * (fastClearExpMultiplier - 1f));
                rewardInfo.bonusReasons.Add($"빠른 클리어! 경험치 +{(fastClearExpMultiplier - 1f) * 100f:F0}%");
            }

            // 4. 첫 클리어 보상
            if (IsFirstClear(bossData.bossId))
            {
                rewardInfo.isFirstClear = true;
                rewardInfo.firstClearGold = bossData.firstClearBonusGold;
                rewardInfo.firstClearForm = bossData.firstClearRewardForm;
                rewardInfo.bonusReasons.Add("첫 클리어!");

                MarkAsCleared(bossData.bossId);
            }

            // 5. 최종 보상 계산
            rewardInfo.totalGold = baseGold + rewardInfo.goldBonus + rewardInfo.firstClearGold;
            rewardInfo.totalExp = baseExp + rewardInfo.expBonus;
            rewardInfo.bone = bossData.boneDrop;
            rewardInfo.soul = bossData.soulDrop;

            // 6. 보상 지급
            GrantRewards(rewardInfo);

            // 7. 아이템 드롭 (V2 우선, V1 폴백)
            DropLoot(bossData);

            Debug.Log($"[BossRewardSystem] 보상 지급 완료: {rewardInfo}");

            OnRewardGranted?.Invoke(rewardInfo);

            return rewardInfo;
        }


        // ====== 보상 지급 ======

        private void GrantRewards(BossRewardInfo rewardInfo)
        {
            // 골드 지급
            if (CurrencySystem.Instance != null)
            {
                CurrencySystem.Instance.AddGold(rewardInfo.totalGold);
            }

            // 경험치 지급
            if (PlayerLevel.Instance != null)
            {
                PlayerLevel.Instance.AddExp(rewardInfo.totalExp);
            }

            // 메타 재화 지급
            if (MetaProgressionManager.HasInstance && MetaProgressionManager.Instance.IsInRun)
            {
                var meta = MetaProgressionManager.Instance;

                if (rewardInfo.bone > 0)
                    meta.Currency.AddTempBone(rewardInfo.bone);

                if (rewardInfo.soul > 0)
                    meta.Currency.AddTempSoul(rewardInfo.soul);
            }

            // 첫 클리어 폼 지급
            if (rewardInfo.isFirstClear && rewardInfo.firstClearForm != null)
            {
                GrantFirstClearForm(rewardInfo.firstClearForm);
            }
        }

        private void GrantFirstClearForm(FormData formData)
        {
            // FormData 지급 로직 (FormManager와 연동)
            Debug.Log($"[BossRewardSystem] 첫 클리어 폼 획득: {formData.formName}");

            // TODO: FormManager.Instance.UnlockForm(formData);
        }

        /// <summary>
        /// 아이템 드롭 (V2 우선, V1 폴백)
        /// </summary>
        private void DropLoot(BossData bossData)
        {
            // V2 LootTableV2 우선 사용
            if (bossData.lootTableV2 != null)
            {
                if (ItemDropManager.HasInstance)
                {
                    ItemDropManager.Instance.DropFromTable(bossData.lootTableV2, Vector3.zero);
                    return;
                }
                Debug.LogWarning("[BossRewardSystem] ItemDropManager를 찾을 수 없습니다. V1 폴백 시도...");
            }

            // V1 폴백
            DropLootV1Fallback(bossData);
        }

        /// <summary>
        /// V1 LootSystem 폴백
        /// </summary>
        #pragma warning disable CS0618 // Obsolete 경고 무시
        private void DropLootV1Fallback(BossData bossData)
        {
            if (bossData.lootTable == null) return;

            if (LootSystem.HasInstance)
            {
                LootSystem.Instance.DropLoot(bossData.lootTable, Vector3.zero);
            }
        }
        #pragma warning restore CS0618


        // ====== 첫 클리어 관리 ======

        /// <summary>
        /// 첫 클리어 여부 확인
        /// </summary>
        public bool IsFirstClear(string bossId)
        {
            return !clearedBosses.Contains(bossId);
        }

        /// <summary>
        /// 클리어 기록 저장
        /// </summary>
        private void MarkAsCleared(string bossId)
        {
            clearedBosses.Add(bossId);
            SaveClearedBosses();
        }

        /// <summary>
        /// 클리어 기록 로드
        /// </summary>
        private void LoadClearedBosses()
        {
            string json = PlayerPrefs.GetString("ClearedBosses", "");

            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    var data = JsonUtility.FromJson<ClearedBossesData>(json);
                    if (data?.bossIds != null)
                    {
                        clearedBosses = new HashSet<string>(data.bossIds);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[BossRewardSystem] 클리어 기록 로드 실패: {e.Message}");
                }
            }

            Debug.Log($"[BossRewardSystem] 클리어 기록 로드: {clearedBosses.Count}개");
        }

        /// <summary>
        /// 클리어 기록 저장
        /// </summary>
        private void SaveClearedBosses()
        {
            var data = new ClearedBossesData
            {
                bossIds = new List<string>(clearedBosses)
            };

            string json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString("ClearedBosses", json);
            PlayerPrefs.Save();

            Debug.Log($"[BossRewardSystem] 클리어 기록 저장: {clearedBosses.Count}개");
        }

        /// <summary>
        /// 클리어 기록 초기화 (디버그용)
        /// </summary>
        [ContextMenu("클리어 기록 초기화")]
        public void ResetClearedBosses()
        {
            clearedBosses.Clear();
            PlayerPrefs.DeleteKey("ClearedBosses");
            Debug.Log("[BossRewardSystem] 클리어 기록 초기화됨");
        }


        // ====== 직렬화용 클래스 ======

        [Serializable]
        private class ClearedBossesData
        {
            public List<string> bossIds;
        }
    }


    /// <summary>
    /// 보스 보상 정보
    /// </summary>
    [Serializable]
    public class BossRewardInfo
    {
        public string bossId;
        public string bossName;
        public float clearTime;
        public bool noHit;
        public bool isFirstClear;

        public int totalGold;
        public int totalExp;
        public int goldBonus;
        public int expBonus;
        public int firstClearGold;
        public int bone;
        public int soul;

        public FormData firstClearForm;
        public List<string> bonusReasons = new List<string>();

        public override string ToString()
        {
            string bonuses = bonusReasons.Count > 0
                ? $" ({string.Join(", ", bonusReasons)})"
                : "";

            return $"[{bossName}] 골드={totalGold}, EXP={totalExp}, Bone={bone}, Soul={soul}{bonuses}";
        }
    }
}
