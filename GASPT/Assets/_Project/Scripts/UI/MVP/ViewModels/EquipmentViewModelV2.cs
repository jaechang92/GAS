using System.Collections.Generic;
using UnityEngine;
using GASPT.Core.Enums;
using GASPT.Data;
using GASPT.Inventory;

namespace GASPT.UI.MVP.ViewModels
{
    /// <summary>
    /// 장비 ViewModel V2 (7슬롯 지원)
    /// </summary>
    public class EquipmentViewModelV2
    {
        // ====== 슬롯별 아이템 ======

        /// <summary>
        /// 무기 슬롯
        /// </summary>
        public EquipmentSlotViewModel Weapon { get; set; }

        /// <summary>
        /// 갑옷 슬롯
        /// </summary>
        public EquipmentSlotViewModel Armor { get; set; }

        /// <summary>
        /// 투구 슬롯
        /// </summary>
        public EquipmentSlotViewModel Helmet { get; set; }

        /// <summary>
        /// 장갑 슬롯
        /// </summary>
        public EquipmentSlotViewModel Gloves { get; set; }

        /// <summary>
        /// 부츠 슬롯
        /// </summary>
        public EquipmentSlotViewModel Boots { get; set; }

        /// <summary>
        /// 악세서리 1
        /// </summary>
        public EquipmentSlotViewModel Accessory1 { get; set; }

        /// <summary>
        /// 악세서리 2
        /// </summary>
        public EquipmentSlotViewModel Accessory2 { get; set; }


        // ====== 생성자 ======

        public EquipmentViewModelV2()
        {
            Weapon = new EquipmentSlotViewModel(EquipmentSlot.Weapon);
            Armor = new EquipmentSlotViewModel(EquipmentSlot.Armor);
            Helmet = new EquipmentSlotViewModel(EquipmentSlot.Helmet);
            Gloves = new EquipmentSlotViewModel(EquipmentSlot.Gloves);
            Boots = new EquipmentSlotViewModel(EquipmentSlot.Boots);
            Accessory1 = new EquipmentSlotViewModel(EquipmentSlot.Accessory1);
            Accessory2 = new EquipmentSlotViewModel(EquipmentSlot.Accessory2);
        }


        // ====== 메서드 ======

        /// <summary>
        /// 슬롯별 뷰모델 가져오기
        /// </summary>
        public EquipmentSlotViewModel GetSlot(EquipmentSlot slot)
        {
            return slot switch
            {
                EquipmentSlot.Weapon => Weapon,
                EquipmentSlot.Armor => Armor,
                EquipmentSlot.Helmet => Helmet,
                EquipmentSlot.Gloves => Gloves,
                EquipmentSlot.Boots => Boots,
                EquipmentSlot.Accessory1 => Accessory1,
                EquipmentSlot.Accessory2 => Accessory2,
                _ => null
            };
        }

        /// <summary>
        /// 슬롯에 아이템 설정
        /// </summary>
        public void SetSlot(EquipmentSlot slot, EquipmentSlotViewModel vm)
        {
            switch (slot)
            {
                case EquipmentSlot.Weapon:
                    Weapon = vm;
                    break;
                case EquipmentSlot.Armor:
                    Armor = vm;
                    break;
                case EquipmentSlot.Helmet:
                    Helmet = vm;
                    break;
                case EquipmentSlot.Gloves:
                    Gloves = vm;
                    break;
                case EquipmentSlot.Boots:
                    Boots = vm;
                    break;
                case EquipmentSlot.Accessory1:
                    Accessory1 = vm;
                    break;
                case EquipmentSlot.Accessory2:
                    Accessory2 = vm;
                    break;
            }
        }

        /// <summary>
        /// 모든 슬롯 목록
        /// </summary>
        public List<EquipmentSlotViewModel> GetAllSlots()
        {
            return new List<EquipmentSlotViewModel>
            {
                Weapon, Armor, Helmet, Gloves, Boots, Accessory1, Accessory2
            };
        }

        /// <summary>
        /// 장착된 슬롯 수
        /// </summary>
        public int EquippedCount
        {
            get
            {
                int count = 0;
                foreach (var slot in GetAllSlots())
                {
                    if (!slot.IsEmpty)
                        count++;
                }
                return count;
            }
        }


        // ====== 팩토리 메서드 ======

        /// <summary>
        /// EquipmentManager로부터 ViewModel 생성
        /// </summary>
        public static EquipmentViewModelV2 FromEquipmentManager()
        {
            if (!EquipmentManager.HasInstance)
                return new EquipmentViewModelV2();

            EquipmentViewModelV2 vm = new EquipmentViewModelV2();
            var equipped = EquipmentManager.Instance.GetAllEquipped();

            foreach (var kvp in equipped)
            {
                if (kvp.Value != null && kvp.Value.IsValid)
                {
                    vm.SetSlot(kvp.Key, EquipmentSlotViewModel.FromItemInstance(kvp.Value, kvp.Key));
                }
            }

            return vm;
        }
    }


    /// <summary>
    /// 장비 슬롯 뷰모델
    /// </summary>
    public class EquipmentSlotViewModel
    {
        /// <summary>
        /// 슬롯 타입
        /// </summary>
        public EquipmentSlot SlotType { get; set; }

        /// <summary>
        /// 슬롯이 비어있는지
        /// </summary>
        public bool IsEmpty { get; set; }

        /// <summary>
        /// 아이템 인스턴스 ID
        /// </summary>
        public string InstanceId { get; set; }

        /// <summary>
        /// 아이템 이름
        /// </summary>
        public string ItemName { get; set; }

        /// <summary>
        /// 아이템 아이콘
        /// </summary>
        public Sprite Icon { get; set; }

        /// <summary>
        /// 등급
        /// </summary>
        public ItemRarity Rarity { get; set; }

        /// <summary>
        /// 등급 색상
        /// </summary>
        public Color RarityColor { get; set; }

        /// <summary>
        /// 내구도 비율 (0-1)
        /// </summary>
        public float DurabilityRatio { get; set; }


        /// <summary>
        /// 기본 생성자
        /// </summary>
        public EquipmentSlotViewModel()
        {
            IsEmpty = true;
            RarityColor = Color.white;
            DurabilityRatio = 1f;
        }

        /// <summary>
        /// 슬롯 타입 지정 생성자
        /// </summary>
        public EquipmentSlotViewModel(EquipmentSlot slotType) : this()
        {
            SlotType = slotType;
        }


        /// <summary>
        /// ItemInstance로부터 생성
        /// </summary>
        public static EquipmentSlotViewModel FromItemInstance(ItemInstance instance, EquipmentSlot slotType)
        {
            if (instance == null || !instance.IsValid)
            {
                return new EquipmentSlotViewModel(slotType);
            }

            ItemData itemData = instance.cachedItemData;
            EquipmentData equipData = instance.EquipmentData;

            float durRatio = 1f;
            if (equipData != null && equipData.HasDurability)
            {
                durRatio = instance.currentDurability > 0
                    ? (float)instance.currentDurability / equipData.maxDurability
                    : 0f;
            }

            return new EquipmentSlotViewModel
            {
                SlotType = slotType,
                IsEmpty = false,
                InstanceId = instance.instanceId,
                ItemName = itemData.itemName,
                Icon = itemData.icon,
                Rarity = itemData.rarity,
                RarityColor = itemData.RarityColor,
                DurabilityRatio = durRatio
            };
        }


        /// <summary>
        /// 슬롯 이름
        /// </summary>
        public string SlotName => SlotType.ToString();
    }
}
