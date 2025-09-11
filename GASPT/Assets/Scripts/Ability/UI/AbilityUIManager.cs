// ===================================
// ����: Assets/Scripts/Ability/UI/AbilityUIManager.cs
// ===================================
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AbilitySystem
{
    /// <summary>
    /// �����Ƽ UI ��ü ����
    /// </summary>
    public class AbilityUIManager : MonoBehaviour
    {
        [Header("UI ����")]
        [SerializeField] private List<AbilitySlotUI> abilitySlots;
        [SerializeField] private GameObject cooldownOverlay;
        [SerializeField] private TextMeshProUGUI tooltipText;
        [SerializeField] private GameObject targetingModeUI;

        [Header("���ҽ� UI")]
        [SerializeField] private Slider manaBar;
        [SerializeField] private TextMeshProUGUI manaText;
        [SerializeField] private Slider staminaBar;
        [SerializeField] private TextMeshProUGUI staminaText;

        private AbilitySystem abilitySystem;
        private AbilityController controller;

        /// <summary>
        /// UI �Ŵ��� �ʱ�ȭ
        /// </summary>
        private void Start()
        {
            // UI ������Ʈ ���� �� �̺�Ʈ ���
        }

        /// <summary>
        /// �����Ƽ ���� UI ������Ʈ
        /// </summary>
        public void UpdateSlotUI(int slotIndex, Ability ability)
        {
            // Ư�� ������ UI ����
        }

        /// <summary>
        /// ��� ���� UI ������Ʈ
        /// </summary>
        public void UpdateAllSlots()
        {
            // ��ü ���� UI ����
        }

        /// <summary>
        /// ��ٿ� ǥ�� ������Ʈ
        /// </summary>
        private void UpdateCooldownDisplay(int slotIndex, float progress)
        {
            // ��ٿ� �������� �� Ÿ�̸� ǥ��
        }

        /// <summary>
        /// ���ҽ� �� ������Ʈ
        /// </summary>
        public void UpdateResourceBars(int currentMana, int maxMana, int currentStamina, int maxStamina)
        {
            // ����/���¹̳� �� ����
        }

        /// <summary>
        /// ���� ǥ��
        /// </summary>
        public void ShowTooltip(AbilityData abilityData, Vector3 position)
        {
            // �����Ƽ ���� ���� ǥ��
        }

        /// <summary>
        /// ���� �����
        /// </summary>
        public void HideTooltip()
        {
            // ���� ��Ȱ��ȭ
        }

        /// <summary>
        /// Ÿ���� ��� UI ǥ��
        /// </summary>
        public void ShowTargetingMode(string abilityName)
        {
            // Ÿ�� ���� ��� UI Ȱ��ȭ
        }

        /// <summary>
        /// Ÿ���� ��� UI �����
        /// </summary>
        public void HideTargetingMode()
        {
            // Ÿ�� ���� ��� UI ��Ȱ��ȭ
        }

        /// <summary>
        /// ��� �Ұ� �ǵ��
        /// </summary>
        public void ShowCannotUseEffect(int slotIndex, string reason)
        {
            // ��� �Ұ� �ð��� �ǵ��
        }
    }
}