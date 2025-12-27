using UnityEngine;
using GASPT.Meta.Enums;

namespace GASPT.Meta
{
    /// <summary>
    /// 업적 ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "NewAchievement", menuName = "GASPT/Meta/Achievement", order = 2)]
    public class Achievement : ScriptableObject
    {
        [Header("기본 정보")]
        [Tooltip("업적 고유 ID")]
        public string achievementId;

        [Tooltip("업적 이름")]
        public string achievementName;

        [Tooltip("업적 설명")]
        [TextArea(2, 4)]
        public string description;

        [Tooltip("업적 아이콘")]
        public Sprite icon;

        [Tooltip("업적 등급")]
        public AchievementTier tier = AchievementTier.Bronze;


        [Header("조건")]
        [Tooltip("업적 타입")]
        public AchievementType achievementType;

        [Tooltip("목표 값 (처치 수, 클리어 횟수 등)")]
        public int targetValue = 1;

        [Tooltip("특수 조건 설명 (Special 타입용)")]
        [TextArea(1, 2)]
        public string specialCondition;


        [Header("보상")]
        [Tooltip("보상 Bone")]
        public int rewardBone = 0;

        [Tooltip("보상 Soul")]
        public int rewardSoul = 0;

        [Tooltip("해금되는 업그레이드 ID (선택적)")]
        public string unlockUpgradeId;

        [Tooltip("해금되는 폼 ID (선택적)")]
        public string unlockFormId;


        [Header("표시")]
        [Tooltip("숨김 업적 (달성 전 내용 비공개)")]
        public bool isHidden = false;

        [Tooltip("진행도 표시 여부")]
        public bool showProgress = true;


        // ====== 프로퍼티 ======

        /// <summary>
        /// 유효한 업적인지 확인
        /// </summary>
        public bool IsValid => !string.IsNullOrEmpty(achievementId) &&
                               !string.IsNullOrEmpty(achievementName) &&
                               targetValue > 0;

        /// <summary>
        /// 보상이 있는지 확인
        /// </summary>
        public bool HasReward => rewardBone > 0 || rewardSoul > 0 ||
                                 !string.IsNullOrEmpty(unlockUpgradeId) ||
                                 !string.IsNullOrEmpty(unlockFormId);


        // ====== 메서드 ======

        /// <summary>
        /// 진행도 계산 (0~1)
        /// </summary>
        public float GetProgress(int currentValue)
        {
            if (targetValue <= 0) return 0f;
            return Mathf.Clamp01((float)currentValue / targetValue);
        }

        /// <summary>
        /// 완료 여부 확인
        /// </summary>
        public bool IsCompleted(int currentValue)
        {
            return currentValue >= targetValue;
        }

        /// <summary>
        /// 진행도 텍스트 생성
        /// </summary>
        public string GetProgressText(int currentValue)
        {
            return $"{Mathf.Min(currentValue, targetValue)} / {targetValue}";
        }

        /// <summary>
        /// 보상 설명 텍스트 생성
        /// </summary>
        public string GetRewardText()
        {
            string rewards = "";

            if (rewardBone > 0)
                rewards += $"{rewardBone} Bone";

            if (rewardSoul > 0)
            {
                if (!string.IsNullOrEmpty(rewards)) rewards += ", ";
                rewards += $"{rewardSoul} Soul";
            }

            if (!string.IsNullOrEmpty(unlockUpgradeId))
            {
                if (!string.IsNullOrEmpty(rewards)) rewards += ", ";
                rewards += "업그레이드 해금";
            }

            if (!string.IsNullOrEmpty(unlockFormId))
            {
                if (!string.IsNullOrEmpty(rewards)) rewards += ", ";
                rewards += "폼 해금";
            }

            return string.IsNullOrEmpty(rewards) ? "없음" : rewards;
        }

        /// <summary>
        /// 등급 색상 가져오기
        /// </summary>
        public Color GetTierColor()
        {
            return tier switch
            {
                AchievementTier.Bronze => new Color(0.8f, 0.5f, 0.2f),
                AchievementTier.Silver => new Color(0.75f, 0.75f, 0.75f),
                AchievementTier.Gold => new Color(1f, 0.84f, 0f),
                AchievementTier.Platinum => new Color(0.9f, 0.95f, 1f),
                _ => Color.white
            };
        }


        // ====== 에디터 ======

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(achievementId))
            {
                achievementId = $"ACH_{name.GetHashCode():X8}";
            }
        }
#endif


        // ====== 디버그 ======

        public override string ToString()
        {
            return $"[Achievement] {achievementId}: {achievementName} ({tier}, Target: {targetValue})";
        }
    }
}
