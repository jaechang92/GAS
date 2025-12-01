using System;
using UnityEngine;

namespace GASPT.Forms
{
    /// <summary>
    /// 폼의 런타임 인스턴스
    /// 플레이어가 보유한 폼의 현재 상태 (각성 단계 등)를 관리
    /// </summary>
    [Serializable]
    public class FormInstance
    {
        /// <summary>
        /// 각성 이벤트 (각성 단계, 새 등급)
        /// </summary>
        public event Action<int, FormRarity> OnAwakened;

        /// <summary>
        /// 최대 각성 도달 이벤트
        /// </summary>
        public event Action OnMaxAwakeningReached;

        /// <summary>
        /// 스탯 변경 이벤트
        /// </summary>
        public event Action<FormStats> OnStatsChanged;


        [SerializeField] private FormData formData;
        [SerializeField] private int awakeningLevel;
        [SerializeField] private FormStats currentStats;


        /// <summary>
        /// 폼 데이터 (읽기 전용)
        /// </summary>
        public FormData Data => formData;

        /// <summary>
        /// 폼 ID
        /// </summary>
        public string FormId => formData?.formId ?? string.Empty;

        /// <summary>
        /// 폼 이름
        /// </summary>
        public string FormName => formData?.formName ?? "Unknown";

        /// <summary>
        /// 폼 타입
        /// </summary>
        public FormType FormType => formData?.formType ?? FormType.Basic;

        /// <summary>
        /// 현재 각성 단계 (0~3)
        /// </summary>
        public int AwakeningLevel => awakeningLevel;

        /// <summary>
        /// 현재 등급 (각성 상태 반영)
        /// </summary>
        public FormRarity CurrentRarity => formData?.GetRarityAtAwakening(awakeningLevel) ?? FormRarity.Common;

        /// <summary>
        /// 현재 스탯 (각성 보너스 적용됨)
        /// </summary>
        public FormStats CurrentStats => currentStats;

        /// <summary>
        /// 최대 각성 여부
        /// </summary>
        public bool IsMaxAwakening => awakeningLevel >= 3;

        /// <summary>
        /// 아이콘 스프라이트
        /// </summary>
        public Sprite Icon => formData?.icon;

        /// <summary>
        /// 폼 스프라이트
        /// </summary>
        public Sprite FormSprite => formData?.formSprite;

        /// <summary>
        /// 애니메이터 컨트롤러
        /// </summary>
        public RuntimeAnimatorController AnimatorController => formData?.animatorController;

        /// <summary>
        /// 폼 색상
        /// </summary>
        public Color FormColor => formData?.formColor ?? Color.white;


        /// <summary>
        /// 새 폼 인스턴스 생성
        /// </summary>
        /// <param name="data">폼 데이터</param>
        /// <param name="initialAwakeningLevel">초기 각성 단계</param>
        public FormInstance(FormData data, int initialAwakeningLevel = 0)
        {
            formData = data;
            awakeningLevel = Mathf.Clamp(initialAwakeningLevel, 0, 3);
            RecalculateStats();
        }

        /// <summary>
        /// 기본 생성자 (직렬화용)
        /// </summary>
        public FormInstance() { }


        /// <summary>
        /// 각성 실행
        /// </summary>
        /// <returns>각성 성공 여부</returns>
        public bool Awaken()
        {
            if (IsMaxAwakening)
            {
                Debug.LogWarning($"[FormInstance] {FormName}은 이미 최대 각성 상태입니다.");
                return false;
            }

            int previousLevel = awakeningLevel;
            awakeningLevel++;
            RecalculateStats();

            Debug.Log($"[FormInstance] {FormName} 각성! 단계: {awakeningLevel}, 등급: {CurrentRarity}");

            OnAwakened?.Invoke(awakeningLevel, CurrentRarity);

            // 최대 각성 도달 체크
            if (IsMaxAwakening)
            {
                Debug.Log($"[FormInstance] {FormName} 최대 각성 달성!");
                OnMaxAwakeningReached?.Invoke();
            }

            return true;
        }

        /// <summary>
        /// 최대 각성 단계 (FormData에서 가져옴)
        /// </summary>
        public int MaxAwakeningLevel => formData?.maxAwakeningLevel ?? 3;

        /// <summary>
        /// 현재 각성 진행률 (0~1)
        /// </summary>
        public float AwakeningProgress => MaxAwakeningLevel > 0 ? (float)awakeningLevel / MaxAwakeningLevel : 0f;

        /// <summary>
        /// 스탯 재계산
        /// </summary>
        public void RecalculateStats()
        {
            if (formData == null)
            {
                currentStats = FormStats.Default;
                return;
            }

            currentStats = formData.GetStatsAtAwakening(awakeningLevel);
            OnStatsChanged?.Invoke(currentStats);
        }

        /// <summary>
        /// 동일 폼인지 확인 (각성 합성용)
        /// </summary>
        public bool IsSameForm(FormInstance other)
        {
            if (other == null || formData == null || other.formData == null)
                return false;

            return formData.formId == other.formData.formId;
        }

        /// <summary>
        /// 동일 폼인지 확인 (FormData 비교)
        /// </summary>
        public bool IsSameForm(FormData otherData)
        {
            if (formData == null || otherData == null)
                return false;

            return formData.formId == otherData.formId;
        }

        /// <summary>
        /// 유효성 검증
        /// </summary>
        public bool IsValid()
        {
            return formData != null && formData.IsValid();
        }

        /// <summary>
        /// 복제 (새 인스턴스 생성)
        /// </summary>
        public FormInstance Clone()
        {
            return new FormInstance(formData, awakeningLevel);
        }

        /// <summary>
        /// 디버그 정보
        /// </summary>
        public override string ToString()
        {
            return $"[{FormName}] Type:{FormType} Rarity:{CurrentRarity} Awakening:{awakeningLevel}/3 Stats:{currentStats}";
        }
    }
}
