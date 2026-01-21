using UnityEngine;

namespace MVP_Core
{
    /// <summary>
    /// UI 전역 설정 ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "UIConfig", menuName = "MVP_Core/UIConfig")]
    public class UIConfig : ScriptableObject
    {
        // ====== 애니메이션 설정 ======

        [Header("Animation")]
        [Tooltip("기본 페이드 시간 (초)")]
        public float defaultFadeDuration = 0.2f;

        [Tooltip("기본 스케일 애니메이션 시간 (초)")]
        public float defaultScaleDuration = 0.15f;

        [Tooltip("툴팁 표시 딜레이 (초)")]
        public float tooltipDelay = 0.3f;

        // ====== 등급 색상 ======

        [Header("Rarity Colors")]
        public Color commonColor = new Color(0.8f, 0.8f, 0.8f);
        public Color uncommonColor = new Color(0.3f, 0.8f, 0.3f);
        public Color rareColor = new Color(0.3f, 0.5f, 1f);
        public Color epicColor = new Color(0.7f, 0.3f, 1f);
        public Color legendaryColor = new Color(1f, 0.6f, 0f);
        public Color mythicColor = new Color(1f, 0.3f, 0.3f);

        // ====== 메시지 색상 ======

        [Header("Message Colors")]
        public Color errorColor = new Color(1f, 0.3f, 0.3f);
        public Color successColor = new Color(0.3f, 1f, 0.3f);
        public Color infoColor = new Color(0.3f, 0.8f, 1f);
        public Color warningColor = new Color(1f, 0.8f, 0.3f);

        // ====== 슬롯 설정 ======

        [Header("Slot Settings")]
        [Tooltip("슬롯 선택 테두리 색상")]
        public Color slotSelectedColor = new Color(1f, 0.9f, 0.3f);

        [Tooltip("슬롯 비활성화 색상")]
        public Color slotDisabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

        [Tooltip("쿨다운 오버레이 색상")]
        public Color cooldownOverlayColor = new Color(0f, 0f, 0f, 0.6f);

        // ====== 유틸리티 메서드 ======

        /// <summary>
        /// 등급 인덱스로 색상 반환 (0: Common, 5: Mythic)
        /// </summary>
        public Color GetRarityColor(int rarityIndex)
        {
            return rarityIndex switch
            {
                0 => commonColor,
                1 => uncommonColor,
                2 => rareColor,
                3 => epicColor,
                4 => legendaryColor,
                5 => mythicColor,
                _ => commonColor
            };
        }

        /// <summary>
        /// 등급 이름으로 색상 반환
        /// </summary>
        public Color GetRarityColor(string rarityName)
        {
            return rarityName?.ToLower() switch
            {
                "common" => commonColor,
                "uncommon" => uncommonColor,
                "rare" => rareColor,
                "epic" => epicColor,
                "legendary" => legendaryColor,
                "mythic" => mythicColor,
                _ => commonColor
            };
        }
    }
}
