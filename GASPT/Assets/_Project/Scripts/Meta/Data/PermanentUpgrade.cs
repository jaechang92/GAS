using UnityEngine;

namespace GASPT.Meta
{
    /// <summary>
    /// 영구 업그레이드 ScriptableObject
    /// 에디터에서 업그레이드 데이터를 정의합니다
    /// </summary>
    [CreateAssetMenu(fileName = "Upgrade", menuName = "GASPT/Meta/PermanentUpgrade")]
    public class PermanentUpgrade : ScriptableObject
    {
        [Header("기본 정보")]
        [Tooltip("업그레이드 고유 ID (예: UP001)")]
        public string upgradeId;

        [Tooltip("업그레이드 이름")]
        public string upgradeName;

        [TextArea(2, 4)]
        [Tooltip("업그레이드 설명")]
        public string description;

        [Tooltip("업그레이드 아이콘")]
        public Sprite icon;


        [Header("업그레이드 타입")]
        [Tooltip("업그레이드 효과 타입")]
        public UpgradeType upgradeType;

        [Tooltip("필요한 재화 타입")]
        public CurrencyType currencyType = CurrencyType.Bone;


        [Header("레벨 설정")]
        [Tooltip("최대 레벨")]
        [Range(1, 10)]
        public int maxLevel = 5;

        [Tooltip("레벨별 비용 (배열 크기 = maxLevel)")]
        public int[] costPerLevel;

        [Tooltip("레벨별 효과 값 (배열 크기 = maxLevel)")]
        public float[] effectPerLevel;


        [Header("선행 조건")]
        [Tooltip("선행 업그레이드 ID 목록")]
        public string[] prerequisiteIds;

        [Tooltip("필요한 업적 ID (선택적)")]
        public string requiredAchievementId;


        // ====== 프로퍼티 ======

        /// <summary>
        /// 유효한 업그레이드인지 검증
        /// </summary>
        public bool IsValid
        {
            get
            {
                if (string.IsNullOrEmpty(upgradeId)) return false;
                if (string.IsNullOrEmpty(upgradeName)) return false;
                if (maxLevel <= 0) return false;
                if (costPerLevel == null || costPerLevel.Length != maxLevel) return false;
                if (effectPerLevel == null || effectPerLevel.Length != maxLevel) return false;
                return true;
            }
        }


        // ====== 메서드 ======

        /// <summary>
        /// 특정 레벨에서 다음 레벨로 업그레이드하는 비용
        /// </summary>
        /// <param name="currentLevel">현재 레벨 (0부터 시작)</param>
        /// <returns>다음 레벨 비용 (-1: 최대 레벨 도달)</returns>
        public int GetUpgradeCost(int currentLevel)
        {
            if (currentLevel >= maxLevel)
            {
                return -1; // 이미 최대 레벨
            }

            if (costPerLevel == null || currentLevel >= costPerLevel.Length)
            {
                Debug.LogError($"[PermanentUpgrade] {upgradeId}: 비용 배열이 잘못 설정되었습니다.");
                return -1;
            }

            return costPerLevel[currentLevel];
        }

        /// <summary>
        /// 특정 레벨에서의 효과 값
        /// </summary>
        /// <param name="level">레벨 (1부터 시작)</param>
        /// <returns>효과 값 (0: 레벨 0 또는 오류)</returns>
        public float GetEffectValue(int level)
        {
            if (level <= 0)
            {
                return 0f;
            }

            int index = level - 1;
            if (effectPerLevel == null || index >= effectPerLevel.Length)
            {
                Debug.LogError($"[PermanentUpgrade] {upgradeId}: 효과 배열이 잘못 설정되었습니다.");
                return 0f;
            }

            return effectPerLevel[index];
        }

        /// <summary>
        /// 총 효과 값 (1레벨부터 현재 레벨까지 누적)
        /// 대부분의 업그레이드는 레벨별 독립 효과이므로 GetEffectValue(level) 사용 권장
        /// </summary>
        public float GetTotalEffectValue(int level)
        {
            return GetEffectValue(level);
        }

        /// <summary>
        /// 업그레이드 가능 여부 확인
        /// </summary>
        /// <param name="currentLevel">현재 레벨</param>
        /// <param name="ownedCurrency">보유 재화</param>
        /// <returns>업그레이드 가능 여부</returns>
        public bool CanUpgrade(int currentLevel, int ownedCurrency)
        {
            if (currentLevel >= maxLevel)
            {
                return false;
            }

            int cost = GetUpgradeCost(currentLevel);
            return cost > 0 && ownedCurrency >= cost;
        }

        /// <summary>
        /// 효과 설명 문자열 생성
        /// </summary>
        public string GetEffectDescription(int level)
        {
            float effect = GetEffectValue(level);

            return upgradeType switch
            {
                UpgradeType.MaxHP => $"최대 HP +{effect:F0}",
                UpgradeType.Attack => $"공격력 +{effect:F0}%",
                UpgradeType.Defense => $"받는 피해 -{effect:F0}%",
                UpgradeType.MoveSpeed => $"이동속도 +{effect:F0}%",
                UpgradeType.GoldBonus => $"골드 획득량 +{effect:F0}%",
                UpgradeType.ExpBonus => $"경험치 획득량 +{effect:F0}%",
                UpgradeType.StartGold => $"시작 골드 +{effect:F0}",
                UpgradeType.ExtraDash => $"대시 횟수 +{effect:F0}",
                UpgradeType.Revive => $"런당 부활 {effect:F0}회",
                _ => $"효과: {effect:F0}"
            };
        }


        // ====== 에디터 검증 ======

#if UNITY_EDITOR
        private void OnValidate()
        {
            // 배열 크기 자동 조정
            if (costPerLevel == null || costPerLevel.Length != maxLevel)
            {
                int[] newCosts = new int[maxLevel];
                for (int i = 0; i < maxLevel; i++)
                {
                    if (costPerLevel != null && i < costPerLevel.Length)
                    {
                        newCosts[i] = costPerLevel[i];
                    }
                    else
                    {
                        // 기본값: 100 * (i+1)^2
                        newCosts[i] = 100 * (i + 1) * (i + 1);
                    }
                }
                costPerLevel = newCosts;
            }

            if (effectPerLevel == null || effectPerLevel.Length != maxLevel)
            {
                float[] newEffects = new float[maxLevel];
                for (int i = 0; i < maxLevel; i++)
                {
                    if (effectPerLevel != null && i < effectPerLevel.Length)
                    {
                        newEffects[i] = effectPerLevel[i];
                    }
                    else
                    {
                        // 기본값: 5 * (i+1)
                        newEffects[i] = 5f * (i + 1);
                    }
                }
                effectPerLevel = newEffects;
            }

            // ID 자동 생성
            if (string.IsNullOrEmpty(upgradeId))
            {
                upgradeId = $"UP{name.GetHashCode():X8}";
            }
        }
#endif


        // ====== 디버그 ======

        public override string ToString()
        {
            return $"[PermanentUpgrade] {upgradeId}: {upgradeName} (Type: {upgradeType}, Max: Lv{maxLevel})";
        }
    }
}
