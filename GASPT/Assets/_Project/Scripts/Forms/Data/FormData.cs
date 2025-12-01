using UnityEngine;

namespace GASPT.Forms
{
    /// <summary>
    /// 폼의 정적 데이터를 정의하는 ScriptableObject
    /// 각 폼의 기본 정보, 스탯, 외형 등을 설정
    /// </summary>
    [CreateAssetMenu(fileName = "NewForm", menuName = "GASPT/Forms/Form Data", order = 1)]
    public class FormData : ScriptableObject
    {
        [Header("기본 정보")]
        [Tooltip("폼의 고유 ID")]
        public string formId;

        [Tooltip("폼의 표시 이름")]
        public string formName;

        [Tooltip("폼의 설명")]
        [TextArea(2, 4)]
        public string description;

        [Tooltip("폼의 속성 타입")]
        public FormType formType = FormType.Basic;

        [Tooltip("기본 등급")]
        public FormRarity baseRarity = FormRarity.Common;


        [Header("스탯")]
        [Tooltip("폼의 기본 스탯")]
        public FormStats baseStats = FormStats.Default;

        [Tooltip("각성 시 스탯 증가율 (등급당)")]
        [Range(1.1f, 1.5f)]
        public float awakeningStatMultiplier = 1.2f;

        [Tooltip("최대 각성 단계")]
        [Range(1, 5)]
        public int maxAwakeningLevel = 3;


        [Header("외형")]
        [Tooltip("폼 아이콘 (UI용)")]
        public Sprite icon;

        [Tooltip("폼 스프라이트 (게임 내 표시)")]
        public Sprite formSprite;

        [Tooltip("애니메이터 컨트롤러")]
        public RuntimeAnimatorController animatorController;

        [Tooltip("폼 색상 (파티클/이펙트용)")]
        public Color formColor = Color.white;


        [Header("이펙트")]
        [Tooltip("폼 교체 시 재생되는 이펙트 프리팹")]
        public GameObject swapEffectPrefab;

        [Tooltip("각성 시 재생되는 이펙트 프리팹")]
        public GameObject awakeningEffectPrefab;

        [Tooltip("최대 각성 시 재생되는 특별 이펙트")]
        public GameObject maxAwakeningEffectPrefab;


        [Header("사운드")]
        [Tooltip("폼 교체 시 재생되는 사운드")]
        public AudioClip swapSound;

        [Tooltip("각성 시 재생되는 사운드")]
        public AudioClip awakeningSound;

        [Tooltip("최대 각성 시 재생되는 특별 사운드")]
        public AudioClip maxAwakeningSound;


        [Header("스킬 (향후 연동)")]
        [Tooltip("기본 공격 스킬 ID")]
        public string primarySkillId;

        [Tooltip("특수 공격 스킬 ID")]
        public string secondarySkillId;


        [Header("밸런스")]
        [Tooltip("드롭 가중치 (높을수록 자주 등장)")]
        [Range(1, 100)]
        public int dropWeight = 50;

        [Tooltip("권장 플레이 스타일 설명")]
        [TextArea(1, 2)]
        public string playstyleHint;


        /// <summary>
        /// 특정 각성 단계의 스탯 계산
        /// </summary>
        /// <param name="awakeningLevel">각성 단계 (0~3)</param>
        /// <returns>해당 단계의 스탯</returns>
        public FormStats GetStatsAtAwakening(int awakeningLevel)
        {
            if (awakeningLevel <= 0)
                return baseStats;

            float multiplier = Mathf.Pow(awakeningStatMultiplier, awakeningLevel);
            return baseStats.ApplyMultiplier(multiplier);
        }

        /// <summary>
        /// 특정 각성 단계의 등급 반환
        /// </summary>
        public FormRarity GetRarityAtAwakening(int awakeningLevel)
        {
            int rarityValue = (int)baseRarity + awakeningLevel;
            return (FormRarity)Mathf.Clamp(rarityValue, 0, (int)FormRarity.Legendary);
        }

        /// <summary>
        /// 유효성 검증
        /// </summary>
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(formId) &&
                   !string.IsNullOrEmpty(formName);
        }

        /// <summary>
        /// 에디터에서 ID 자동 생성
        /// </summary>
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(formId))
            {
                formId = $"form_{formType.ToString().ToLower()}_{System.Guid.NewGuid().ToString().Substring(0, 8)}";
            }
        }
    }
}
