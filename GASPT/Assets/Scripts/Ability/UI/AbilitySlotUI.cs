// ===================================
// ����: Assets/Scripts/Ability/UI/AbilitySlotUI.cs
// ===================================
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace AbilitySystem
{
    /// <summary>
    /// ���� �����Ƽ ���� UI
    /// </summary>
    public class AbilitySlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [Header("UI ������Ʈ")]
        [SerializeField] private Image iconImage;
        [SerializeField] private Image cooldownOverlay;
        [SerializeField] private TextMeshProUGUI cooldownText;
        [SerializeField] private TextMeshProUGUI hotkeyText;
        [SerializeField] private GameObject readyEffect;
        [SerializeField] private GameObject lockedOverlay;

        [Header("���� ����")]
        [SerializeField] private int slotIndex;
        [SerializeField] private KeyCode hotkey;

        private Ability assignedAbility;
        private AbilityUIManager uiManager;
        private bool isOnCooldown;

        /// <summary>
        /// ���� �ʱ�ȭ
        /// </summary>
        public void Initialize(int index, AbilityUIManager manager)
        {
            // ���� �ε��� �� �Ŵ��� ����
        }

        /// <summary>
        /// �����Ƽ �Ҵ�
        /// </summary>
        public void AssignAbility(Ability ability)
        {
            // ���Կ� �����Ƽ ����
        }

        /// <summary>
        /// �����Ƽ ����
        /// </summary>
        public void ClearAbility()
        {
            // ���� ����
        }

        /// <summary>
        /// UI ������Ʈ
        /// </summary>
        public void UpdateDisplay()
        {
            // ������, ��ٿ� �� ǥ�� ����
        }

        /// <summary>
        /// ��ٿ� ǥ�� ������Ʈ
        /// </summary>
        public void UpdateCooldown(float remainingTime, float totalTime)
        {
            // ��ٿ� �������� �� �ؽ�Ʈ ����
        }

        /// <summary>
        /// ���� Ȱ��ȭ ȿ��
        /// </summary>
        public void ShowActivationEffect()
        {
            // �����Ƽ ��� �� �ð� ȿ��
        }

        /// <summary>
        /// �غ� �Ϸ� ȿ��
        /// </summary>
        public void ShowReadyEffect()
        {
            // ��ٿ� �Ϸ� �� ȿ��
        }

        /// <summary>
        /// ���콺 ȣ�� ó��
        /// </summary>
        public void OnPointerEnter(PointerEventData eventData)
        {
            // ���� ǥ��
        }

        /// <summary>
        /// ���콺 ȣ�� ����
        /// </summary>
        public void OnPointerExit(PointerEventData eventData)
        {
            // ���� �����
        }

        /// <summary>
        /// Ŭ�� ó��
        /// </summary>
        public void OnPointerClick(PointerEventData eventData)
        {
            // ���콺 Ŭ������ �����Ƽ ���
        }

        /// <summary>
        /// ��� ���� ����
        /// </summary>
        public void SetLocked(bool locked)
        {
            // ���� ���/����
        }
    }
}