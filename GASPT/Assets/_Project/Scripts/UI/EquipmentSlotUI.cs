using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;
using GASPT.Data;
using Core.Enums;

namespace GASPT.UI
{
    /// <summary>
    /// 장비 슬롯 UI
    /// 특정 장비 슬롯의 아이템을 표시하고 클릭으로 장착 해제
    /// Phase C-4: 아이템 드롭 및 장착 시스템
    /// </summary>
    public class EquipmentSlotUI : MonoBehaviour, IPointerClickHandler
    {
        // ====== UI 참조 ======

        [Header("UI 참조")]
        [Tooltip("슬롯 이름 텍스트")]
        [SerializeField] private TextMeshProUGUI slotNameText;

        [Tooltip("아이템 아이콘 이미지")]
        [SerializeField] private Image iconImage;

        [Tooltip("아이템 이름 텍스트")]
        [SerializeField] private TextMeshProUGUI itemNameText;

        [Tooltip("빈 슬롯 표시 오브젝트 (아이템이 없을 때)")]
        [SerializeField] private GameObject emptySlotObject;


        // ====== 설정 ======

        [Header("설정")]
        [Tooltip("빈 슬롯 색상")]
        [SerializeField] private Color emptySlotColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

        [Tooltip("아이템 장착 시 색상")]
        [SerializeField] private Color filledSlotColor = Color.white;


        // ====== 슬롯 정보 ======

        private EquipmentSlot slotType;
        private Item currentItem;
        private Action<EquipmentSlot> onSlotClicked;


        // ====== Unity 생명주기 ======

        private void Awake()
        {
            // 초기 상태는 빈 슬롯
            SetEmpty();
        }


        // ====== 초기화 ======

        /// <summary>
        /// 장비 슬롯 초기화
        /// </summary>
        /// <param name="slot">장비 슬롯 타입</param>
        /// <param name="clickCallback">슬롯 클릭 시 호출될 콜백</param>
        public void Initialize(EquipmentSlot slot, Action<EquipmentSlot> clickCallback)
        {
            slotType = slot;
            onSlotClicked = clickCallback;

            // 슬롯 이름 설정
            if (slotNameText != null)
            {
                slotNameText.text = GetSlotDisplayName(slot);
            }

            SetEmpty();
        }


        // ====== 아이템 설정 ======

        /// <summary>
        /// 아이템 설정 (장착)
        /// </summary>
        /// <param name="item">장착할 아이템 (null이면 빈 슬롯)</param>
        public void SetItem(Item item)
        {
            currentItem = item;

            if (item == null)
            {
                SetEmpty();
            }
            else
            {
                SetFilled(item);
            }
        }

        /// <summary>
        /// 빈 슬롯 설정
        /// </summary>
        private void SetEmpty()
        {
            currentItem = null;

            // 아이콘 색상
            if (iconImage != null)
            {
                iconImage.sprite = null;
                iconImage.color = emptySlotColor;
            }

            // 아이템 이름 텍스트
            if (itemNameText != null)
            {
                itemNameText.text = "비어있음";
                itemNameText.color = emptySlotColor;
            }

            // 빈 슬롯 오브젝트 표시
            if (emptySlotObject != null)
            {
                emptySlotObject.SetActive(true);
            }
        }

        /// <summary>
        /// 아이템 장착 상태 설정
        /// </summary>
        private void SetFilled(Item item)
        {
            // 아이콘 설정
            if (iconImage != null)
            {
                if (item.icon != null)
                {
                    iconImage.sprite = item.icon;
                    iconImage.color = filledSlotColor;
                }
                else
                {
                    // 아이콘이 없으면 기본 색상
                    iconImage.sprite = null;
                    iconImage.color = emptySlotColor;
                }
            }

            // 아이템 이름 텍스트
            if (itemNameText != null)
            {
                itemNameText.text = item.itemName;
                itemNameText.color = Color.white;
            }

            // 빈 슬롯 오브젝트 숨김
            if (emptySlotObject != null)
            {
                emptySlotObject.SetActive(false);
            }
        }


        // ====== 클릭 이벤트 ======

        /// <summary>
        /// 슬롯 클릭 이벤트 (IPointerClickHandler 구현)
        /// </summary>
        public void OnPointerClick(PointerEventData eventData)
        {
            // 좌클릭: 장착 해제
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                if (currentItem != null)
                {
                    onSlotClicked?.Invoke(slotType);
                    Debug.Log($"[EquipmentSlotUI] {slotType} 슬롯 클릭 - 장착 해제 시도");
                }
                else
                {
                    Debug.Log($"[EquipmentSlotUI] {slotType} 슬롯은 비어있습니다.");
                }
            }
        }


        // ====== 헬퍼 메서드 ======

        /// <summary>
        /// 슬롯 타입에 따른 표시 이름 반환
        /// </summary>
        private string GetSlotDisplayName(EquipmentSlot slot)
        {
            return slot switch
            {
                EquipmentSlot.Weapon => "무기",
                EquipmentSlot.Armor => "방어구",
                EquipmentSlot.Ring => "반지",
                _ => "알 수 없음"
            };
        }


        // ====== Getters ======

        public EquipmentSlot SlotType => slotType;
        public Item CurrentItem => currentItem;
        public bool IsEmpty => currentItem == null;


        // ====== Context Menu (테스트용) ======

        [ContextMenu("Test: Set Empty")]
        private void TestSetEmpty()
        {
            SetEmpty();
        }

        [ContextMenu("Print Slot Info")]
        private void PrintSlotInfo()
        {
            Debug.Log("========== EquipmentSlotUI Info ==========");
            Debug.Log($"Slot Type: {slotType}");
            Debug.Log($"Current Item: {(currentItem != null ? currentItem.itemName : "None")}");
            Debug.Log($"Is Empty: {IsEmpty}");
            Debug.Log("=========================================");
        }
    }
}
