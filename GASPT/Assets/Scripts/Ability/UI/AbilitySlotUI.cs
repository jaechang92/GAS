// ===================================
// 파일: Assets/Scripts/Ability/UI/AbilitySlotUI.cs
// ===================================
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace AbilitySystem
{
    /// <summary>
    /// 개별 어빌리티 슬롯 UI
    /// </summary>
    public class AbilitySlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [Header("UI 컴포넌트")]
        [SerializeField] private Image iconImage;
        [SerializeField] private Image cooldownOverlay;
        [SerializeField] private TextMeshProUGUI cooldownText;
        [SerializeField] private TextMeshProUGUI hotkeyText;
        [SerializeField] private GameObject readyEffect;
        [SerializeField] private GameObject lockedOverlay;

        [Header("슬롯 설정")]
        [SerializeField] private int slotIndex;
        [SerializeField] private KeyCode hotkey;

        private Ability assignedAbility;
        private AbilityUIManager uiManager;
        private bool isOnCooldown;

        /// <summary>
        /// 슬롯 초기화
        /// </summary>
        public void Initialize(int index, AbilityUIManager manager)
        {
            // 슬롯 인덱스 및 매니저 설정
        }

        /// <summary>
        /// 어빌리티 할당
        /// </summary>
        public void AssignAbility(Ability ability)
        {
            // 슬롯에 어빌리티 설정
        }

        /// <summary>
        /// 어빌리티 제거
        /// </summary>
        public void ClearAbility()
        {
            // 슬롯 비우기
        }

        /// <summary>
        /// UI 업데이트
        /// </summary>
        public void UpdateDisplay()
        {
            // 아이콘, 쿨다운 등 표시 갱신
        }

        /// <summary>
        /// 쿨다운 표시 업데이트
        /// </summary>
        public void UpdateCooldown(float remainingTime, float totalTime)
        {
            // 쿨다운 오버레이 및 텍스트 갱신
        }

        /// <summary>
        /// 슬롯 활성화 효과
        /// </summary>
        public void ShowActivationEffect()
        {
            // 어빌리티 사용 시 시각 효과
        }

        /// <summary>
        /// 준비 완료 효과
        /// </summary>
        public void ShowReadyEffect()
        {
            // 쿨다운 완료 시 효과
        }

        /// <summary>
        /// 마우스 호버 처리
        /// </summary>
        public void OnPointerEnter(PointerEventData eventData)
        {
            // 툴팁 표시
        }

        /// <summary>
        /// 마우스 호버 해제
        /// </summary>
        public void OnPointerExit(PointerEventData eventData)
        {
            // 툴팁 숨기기
        }

        /// <summary>
        /// 클릭 처리
        /// </summary>
        public void OnPointerClick(PointerEventData eventData)
        {
            // 마우스 클릭으로 어빌리티 사용
        }

        /// <summary>
        /// 잠금 상태 설정
        /// </summary>
        public void SetLocked(bool locked)
        {
            // 슬롯 잠금/해제
        }
    }
}