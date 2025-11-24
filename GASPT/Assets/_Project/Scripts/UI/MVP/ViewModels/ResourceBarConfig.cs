using UnityEngine;

namespace GASPT.UI
{
    /// <summary>
    /// ResourceBar 시각 설정 (ScriptableObject)
    /// HP, Mana 등 각 리소스 타입별 색상 설정을 저장
    /// </summary>
    [CreateAssetMenu(fileName = "ResourceBarConfig", menuName = "GASPT/UI/ResourceBarConfig")]
    public class ResourceBarConfig : ScriptableObject
    {
        [Header("Resource Type")]
        [Tooltip("리소스 타입 (HP, Mana 등)")]
        public ResourceType resourceType = ResourceType.HP;

        [Header("Normal Colors")]
        [Tooltip("정상 상태 색상")]
        public Color normalColor = new Color(0.2f, 0.8f, 0.2f); // 녹색 (HP) / 파란색 (Mana)

        [Tooltip("저수치 색상 (임계값 1 이하)")]
        public Color lowColor = new Color(1f, 0.5f, 0f); // 주황색

        [Tooltip("위험 색상 (임계값 2 이하)")]
        public Color criticalColor = new Color(1f, 0.2f, 0.2f); // 빨간색

        [Header("Flash Colors")]
        [Tooltip("감소 시 플래시 색상 (데미지, 마나 소모)")]
        public Color decreaseFlashColor = new Color(1f, 0.3f, 0.3f); // 빨간색

        [Tooltip("증가 시 플래시 색상 (회복, 마나 회복)")]
        public Color increaseFlashColor = new Color(0.3f, 1f, 0.3f); // 초록색 (HP) / 밝은 파란색 (Mana)

        [Header("Thresholds (0.0 ~ 1.0)")]
        [Tooltip("저수치 임계값 (예: 0.3 = 30%)")]
        [Range(0f, 1f)]
        public float lowThreshold = 0.3f;

        [Tooltip("위험 임계값 (예: 0.1 = 10%)")]
        [Range(0f, 1f)]
        public float criticalThreshold = 0.1f;

        [Header("Animation")]
        [Tooltip("플래시 애니메이션 지속 시간")]
        public float flashDuration = 0.3f;

        /// <summary>
        /// 비율에 따른 색상 반환 (0.0 ~ 1.0)
        /// </summary>
        public Color GetColorForRatio(float ratio)
        {
            if (ratio <= criticalThreshold)
            {
                return criticalColor; // 위험 (10% 이하)
            }
            else if (ratio <= lowThreshold)
            {
                return lowColor; // 저수치 (30% 이하)
            }
            else
            {
                return normalColor; // 정상
            }
        }
    }
}
