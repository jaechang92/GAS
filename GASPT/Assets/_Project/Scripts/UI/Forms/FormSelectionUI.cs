using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GASPT.Gameplay.Form;

namespace GASPT.UI.Forms
{
    /// <summary>
    /// 폼 선택 팝업 UI
    /// 슬롯이 가득 찼을 때 교체할 폼을 선택
    /// </summary>
    public class FormSelectionUI : BaseUI
    {
        // ====== 이벤트 ======

        /// <summary>슬롯 선택 이벤트 (슬롯 인덱스)</summary>
        public event Action<int> OnSlotSelected;

        /// <summary>취소 이벤트</summary>
        public event Action OnCancelled;


        // ====== UI 요소 ======

        [Header("새 폼 정보")]
        [SerializeField] private Image newFormIcon;
        [SerializeField] private TextMeshProUGUI newFormName;
        [SerializeField] private TextMeshProUGUI newFormDescription;
        [SerializeField] private TextMeshProUGUI newFormStats;

        [Header("슬롯 선택")]
        [SerializeField] private FormSlotUI slot1UI;
        [SerializeField] private FormSlotUI slot2UI;
        [SerializeField] private Button slot1Button;
        [SerializeField] private Button slot2Button;
        [SerializeField] private Button cancelButton;

        [Header("비교 표시")]
        [SerializeField] private TextMeshProUGUI slot1CompareText;
        [SerializeField] private TextMeshProUGUI slot2CompareText;

        [Header("텍스트")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI instructionText;

        [Header("설정")]
        [SerializeField] private KeyCode slot1Key = KeyCode.Alpha1;
        [SerializeField] private KeyCode slot2Key = KeyCode.Alpha2;
        [SerializeField] private KeyCode cancelKey = KeyCode.Escape;
        [SerializeField] private bool pauseGameOnShow = true;


        // ====== 상태 ======

        private FormData newFormData;
        private FormInstance currentSlot1;
        private FormInstance currentSlot2;
        private FormPickup pendingPickup;


        // ====== Unity 생명주기 ======

        protected override void Initialize()
        {
            base.Initialize();

            // 버튼 이벤트 연결
            if (slot1Button != null)
                slot1Button.onClick.AddListener(() => SelectSlot(0));

            if (slot2Button != null)
                slot2Button.onClick.AddListener(() => SelectSlot(1));

            if (cancelButton != null)
                cancelButton.onClick.AddListener(Cancel);
        }

        private void Update()
        {
            if (!IsVisible) return;

            HandleKeyboardInput();
        }

        private void OnDestroy()
        {
            if (slot1Button != null)
                slot1Button.onClick.RemoveAllListeners();

            if (slot2Button != null)
                slot2Button.onClick.RemoveAllListeners();

            if (cancelButton != null)
                cancelButton.onClick.RemoveAllListeners();
        }


        // ====== 공개 메서드 ======

        /// <summary>
        /// 선택 UI 표시
        /// </summary>
        public void ShowSelection(FormData newForm, FormInstance slot1, FormInstance slot2, FormPickup pickup = null)
        {
            newFormData = newForm;
            currentSlot1 = slot1;
            currentSlot2 = slot2;
            pendingPickup = pickup;

            UpdateNewFormDisplay();
            UpdateSlotDisplays();
            UpdateCompareTexts();

            if (pauseGameOnShow)
            {
                Time.timeScale = 0f;
            }

            Show();
        }

        /// <summary>
        /// 슬롯 선택
        /// </summary>
        public void SelectSlot(int slotIndex)
        {
            Debug.Log($"[FormSelectionUI] 슬롯 {slotIndex + 1} 선택됨");

            // 픽업이 있으면 교체 완료 호출
            if (pendingPickup != null)
            {
                pendingPickup.CompletePickupWithReplacement(slotIndex);
            }

            OnSlotSelected?.Invoke(slotIndex);

            CloseSelection();
        }

        /// <summary>
        /// 선택 취소
        /// </summary>
        public void Cancel()
        {
            Debug.Log("[FormSelectionUI] 선택 취소됨");

            OnCancelled?.Invoke();

            CloseSelection();
        }


        // ====== 내부 메서드 ======

        private void CloseSelection()
        {
            newFormData = null;
            currentSlot1 = null;
            currentSlot2 = null;
            pendingPickup = null;

            if (pauseGameOnShow)
            {
                Time.timeScale = 1f;
            }

            Hide();
        }

        private void HandleKeyboardInput()
        {
            if (Input.GetKeyDown(slot1Key))
            {
                SelectSlot(0);
            }
            else if (Input.GetKeyDown(slot2Key))
            {
                SelectSlot(1);
            }
            else if (Input.GetKeyDown(cancelKey))
            {
                Cancel();
            }
        }

        private void UpdateNewFormDisplay()
        {
            if (newFormData == null) return;

            if (newFormIcon != null && newFormData.icon != null)
            {
                newFormIcon.sprite = newFormData.icon;
                newFormIcon.color = newFormData.formColor;
            }

            if (newFormName != null)
            {
                newFormName.text = newFormData.formName;
                newFormName.color = GetRarityColor(newFormData.baseRarity);
            }

            if (newFormDescription != null)
            {
                newFormDescription.text = newFormData.description;
            }

            if (newFormStats != null)
            {
                newFormStats.text = FormatStats(newFormData.GetStatsAtAwakening(0));
            }
        }

        private void UpdateSlotDisplays()
        {
            if (slot1UI != null)
            {
                slot1UI.SetForm(currentSlot1);
            }

            if (slot2UI != null)
            {
                slot2UI.SetForm(currentSlot2);
            }
        }

        private void UpdateCompareTexts()
        {
            if (newFormData == null) return;

            if (slot1CompareText != null && currentSlot1 != null)
            {
                slot1CompareText.text = CompareStats(currentSlot1.CurrentStats, newFormData.GetStatsAtAwakening(0));
            }

            if (slot2CompareText != null && currentSlot2 != null)
            {
                slot2CompareText.text = CompareStats(currentSlot2.CurrentStats, newFormData.GetStatsAtAwakening(0));
            }
        }

        private string FormatStats(FormStats stats)
        {
            return $"공격력: {stats.attackPower:F0}\n" +
                   $"공속: {stats.attackSpeed:F1}\n" +
                   $"치확: {stats.criticalChance:P0}\n" +
                   $"이속: {stats.moveSpeed:F1}";
        }

        private string CompareStats(FormStats current, FormStats newStats)
        {
            string result = "";

            float atkDiff = newStats.attackPower - current.attackPower;
            if (Mathf.Abs(atkDiff) > 0.1f)
            {
                result += FormatDifference("공격력", atkDiff) + "\n";
            }

            float spdDiff = newStats.attackSpeed - current.attackSpeed;
            if (Mathf.Abs(spdDiff) > 0.01f)
            {
                result += FormatDifference("공속", spdDiff) + "\n";
            }

            float moveDiff = newStats.moveSpeed - current.moveSpeed;
            if (Mathf.Abs(moveDiff) > 0.01f)
            {
                result += FormatDifference("이속", moveDiff) + "\n";
            }

            return string.IsNullOrEmpty(result) ? "변화 없음" : result.TrimEnd('\n');
        }

        private string FormatDifference(string statName, float diff)
        {
            string sign = diff > 0 ? "+" : "";
            string color = diff > 0 ? "green" : "red";
            return $"{statName}: <color={color}>{sign}{diff:F1}</color>";
        }

        private Color GetRarityColor(FormRarity rarity)
        {
            return rarity switch
            {
                FormRarity.Common => Color.white,
                FormRarity.Rare => new Color(0.3f, 0.5f, 1f),
                FormRarity.Unique => new Color(0.7f, 0.3f, 0.9f),
                FormRarity.Legendary => new Color(1f, 0.8f, 0.2f),
                _ => Color.white
            };
        }
    }
}
