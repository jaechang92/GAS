using UnityEngine;

namespace MVP_Core
{
    /// <summary>
    /// 범용 리소스 바 설정 ScriptableObject
    /// HP, Mana, Stamina 등 각 리소스 타입별 설정 저장
    /// </summary>
    [CreateAssetMenu(fileName = "ResourceBarConfig", menuName = "MVP_Core/ResourceBarConfig")]
    public class ResourceBarConfig : ScriptableObject
    {
        // ====== 식별 ======

        [Header("Identification")]
        [Tooltip("리소스 종류 식별자 (예: HP, Mana, Stamina)")]
        public string resourceType = "HP";

        // ====== 기본 색상 ======

        [Header("Normal Colors")]
        [Tooltip("정상 상태 색상")]
        public Color normalColor = new Color(0.2f, 0.8f, 0.2f);

        [Tooltip("저수치 색상 (lowThreshold 이하)")]
        public Color lowColor = new Color(1f, 0.5f, 0f);

        [Tooltip("위험 색상 (criticalThreshold 이하)")]
        public Color criticalColor = new Color(1f, 0.2f, 0.2f);

        // ====== 플래시 색상 ======

        [Header("Flash Colors")]
        [Tooltip("감소 시 플래시 색상")]
        public Color decreaseFlashColor = new Color(1f, 0.3f, 0.3f);

        [Tooltip("증가 시 플래시 색상")]
        public Color increaseFlashColor = new Color(0.3f, 1f, 0.3f);

        // ====== 임계값 ======

        [Header("Thresholds (0.0 ~ 1.0)")]
        [Tooltip("저수치 임계값 (예: 0.3 = 30%)")]
        [Range(0f, 1f)]
        public float lowThreshold = 0.3f;

        [Tooltip("위험 임계값 (예: 0.1 = 10%)")]
        [Range(0f, 1f)]
        public float criticalThreshold = 0.1f;

        // ====== 애니메이션 ======

        [Header("Animation")]
        [Tooltip("플래시 애니메이션 지속 시간 (초)")]
        public float flashDuration = 0.3f;

        [Tooltip("값 변경 시 스무스 애니메이션 사용")]
        public bool useSmoothAnimation = true;

        [Tooltip("스무스 애니메이션 속도")]
        public float smoothSpeed = 10f;

        // ====== 메서드 ======

        /// <summary>
        /// 비율에 따른 색상 반환
        /// </summary>
        /// <param name="ratio">현재 비율 (0.0 ~ 1.0)</param>
        public Color GetColorForRatio(float ratio)
        {
            if (ratio <= criticalThreshold)
            {
                return criticalColor;
            }
            else if (ratio <= lowThreshold)
            {
                return lowColor;
            }
            return normalColor;
        }

        /// <summary>
        /// 비율에 따른 그라데이션 색상 반환 (부드러운 전환)
        /// </summary>
        public Color GetGradientColor(float ratio)
        {
            ratio = Mathf.Clamp01(ratio);

            if (ratio <= criticalThreshold)
            {
                return criticalColor;
            }
            else if (ratio <= lowThreshold)
            {
                float t = (ratio - criticalThreshold) / (lowThreshold - criticalThreshold);
                return Color.Lerp(criticalColor, lowColor, t);
            }
            else
            {
                float t = (ratio - lowThreshold) / (1f - lowThreshold);
                return Color.Lerp(lowColor, normalColor, t);
            }
        }
    }
}
