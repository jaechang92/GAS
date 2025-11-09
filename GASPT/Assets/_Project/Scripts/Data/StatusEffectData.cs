using UnityEngine;
using Core.Enums;

namespace GASPT.Data
{
    /// <summary>
    /// 상태 이상 효과 데이터 ScriptableObject
    /// 버프/디버프 효과 정의 및 설정
    /// </summary>
    [CreateAssetMenu(fileName = "StatusEffectData", menuName = "GASPT/StatusEffects/StatusEffect")]
    public class StatusEffectData : ScriptableObject
    {
        // ====== 타입 ======

        [Header("타입")]
        [Tooltip("상태 이상 효과 타입")]
        public StatusEffectType effectType;


        // ====== 기본 정보 ======

        [Header("기본 정보")]
        [Tooltip("효과 표시 이름")]
        public string displayName;

        [Tooltip("효과 설명")]
        [TextArea(2, 4)]
        public string description;

        [Tooltip("효과 아이콘 (UI 표시용)")]
        public Sprite icon;

        [Tooltip("버프 여부 (true: 버프, false: 디버프)")]
        public bool isBuff = true;


        // ====== 효과 수치 ======

        [Header("효과 수치")]
        [Tooltip("효과 값 (공격력 +10, 방어력 -5 등)")]
        public float value = 10f;

        [Tooltip("지속 시간 (초)")]
        [Range(1f, 60f)]
        public float duration = 5f;

        [Tooltip("틱 간격 (DoT의 경우, 0이면 즉시 적용)")]
        [Range(0f, 5f)]
        public float tickInterval = 0f;


        // ====== 중첩 설정 ======

        [Header("중첩 설정")]
        [Tooltip("최대 중첩 수")]
        [Range(1, 10)]
        public int maxStack = 1;


        // ====== 시각 효과 ======

        [Header("시각 효과")]
        [Tooltip("파티클 이펙트 (선택사항)")]
        public GameObject particleEffect;

        [Tooltip("효과 색상 (UI 표시용)")]
        public Color effectColor = Color.white;


        // ====== 유틸리티 메서드 ======

        /// <summary>
        /// StatusEffect 인스턴스 생성
        /// </summary>
        public GASPT.StatusEffects.StatusEffect CreateInstance()
        {
            return new GASPT.StatusEffects.StatusEffect(
                effectType,
                displayName,
                description,
                value,
                duration,
                tickInterval,
                maxStack
            );
        }


        // ====== 검증 ======

        private void OnValidate()
        {
            // displayName이 비어있으면 effectType으로 자동 설정
            if (string.IsNullOrEmpty(displayName))
            {
                displayName = effectType.ToString();
            }

            // 값 검증
            if (value < 0 && isBuff)
            {
                Debug.LogWarning($"[StatusEffectData] {displayName}: 버프인데 값이 음수입니다.");
            }

            if (value > 0 && !isBuff && effectType != StatusEffectType.Regeneration)
            {
                Debug.LogWarning($"[StatusEffectData] {displayName}: 디버프인데 값이 양수입니다.");
            }
        }
    }
}
