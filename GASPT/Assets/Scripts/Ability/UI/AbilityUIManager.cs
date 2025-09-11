// ===================================
// 파일: Assets/Scripts/Ability/UI/AbilityUIManager.cs
// ===================================
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AbilitySystem
{
    /// <summary>
    /// 어빌리티 UI 전체 관리
    /// </summary>
    public class AbilityUIManager : MonoBehaviour
    {
        [Header("UI 참조")]
        [SerializeField] private List<AbilitySlotUI> abilitySlots;
        [SerializeField] private GameObject cooldownOverlay;
        [SerializeField] private TextMeshProUGUI tooltipText;
        [SerializeField] private GameObject targetingModeUI;

        [Header("리소스 UI")]
        [SerializeField] private Slider manaBar;
        [SerializeField] private TextMeshProUGUI manaText;
        [SerializeField] private Slider staminaBar;
        [SerializeField] private TextMeshProUGUI staminaText;

        private AbilitySystem abilitySystem;
        private AbilityController controller;

        /// <summary>
        /// UI 매니저 초기화
        /// </summary>
        private void Start()
        {
            // UI 컴포넌트 연결 및 이벤트 등록
        }

        /// <summary>
        /// 어빌리티 슬롯 UI 업데이트
        /// </summary>
        public void UpdateSlotUI(int slotIndex, Ability ability)
        {
            // 특정 슬롯의 UI 갱신
        }

        /// <summary>
        /// 모든 슬롯 UI 업데이트
        /// </summary>
        public void UpdateAllSlots()
        {
            // 전체 슬롯 UI 갱신
        }

        /// <summary>
        /// 쿨다운 표시 업데이트
        /// </summary>
        private void UpdateCooldownDisplay(int slotIndex, float progress)
        {
            // 쿨다운 오버레이 및 타이머 표시
        }

        /// <summary>
        /// 리소스 바 업데이트
        /// </summary>
        public void UpdateResourceBars(int currentMana, int maxMana, int currentStamina, int maxStamina)
        {
            // 마나/스태미나 바 갱신
        }

        /// <summary>
        /// 툴팁 표시
        /// </summary>
        public void ShowTooltip(AbilityData abilityData, Vector3 position)
        {
            // 어빌리티 정보 툴팁 표시
        }

        /// <summary>
        /// 툴팁 숨기기
        /// </summary>
        public void HideTooltip()
        {
            // 툴팁 비활성화
        }

        /// <summary>
        /// 타겟팅 모드 UI 표시
        /// </summary>
        public void ShowTargetingMode(string abilityName)
        {
            // 타겟 선택 모드 UI 활성화
        }

        /// <summary>
        /// 타겟팅 모드 UI 숨기기
        /// </summary>
        public void HideTargetingMode()
        {
            // 타겟 선택 모드 UI 비활성화
        }

        /// <summary>
        /// 사용 불가 피드백
        /// </summary>
        public void ShowCannotUseEffect(int slotIndex, string reason)
        {
            // 사용 불가 시각적 피드백
        }
    }
}