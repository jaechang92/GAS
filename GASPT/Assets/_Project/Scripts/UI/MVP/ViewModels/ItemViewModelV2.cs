using System.Collections.Generic;
using UnityEngine;
using GASPT.Core.Enums;
using GASPT.Data;
using GASPT.Inventory;

namespace GASPT.UI.MVP.ViewModels
{
    /// <summary>
    /// 아이템 상세 뷰모델 V2 (ItemInstance 기반)
    /// 툴팁 및 상세 정보 표시용
    /// </summary>
    public class ItemViewModelV2
    {
        // ====== 기본 정보 ======

        /// <summary>
        /// 아이템 인스턴스 ID
        /// </summary>
        public string InstanceId { get; set; }

        /// <summary>
        /// 아이템 이름
        /// </summary>
        public string ItemName { get; set; }

        /// <summary>
        /// 아이템 설명
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 아이템 아이콘
        /// </summary>
        public Sprite Icon { get; set; }


        // ====== 분류 ======

        /// <summary>
        /// 카테고리
        /// </summary>
        public ItemCategory Category { get; set; }

        /// <summary>
        /// 등급
        /// </summary>
        public ItemRarity Rarity { get; set; }

        /// <summary>
        /// 등급 색상
        /// </summary>
        public Color RarityColor { get; set; }

        /// <summary>
        /// 등급 이름
        /// </summary>
        public string RarityName => Rarity.ToString();


        // ====== 장비 정보 ======

        /// <summary>
        /// 장비 여부
        /// </summary>
        public bool IsEquipment { get; set; }

        /// <summary>
        /// 장비 슬롯
        /// </summary>
        public EquipmentSlot EquipSlot { get; set; }

        /// <summary>
        /// 필요 레벨
        /// </summary>
        public int RequiredLevel { get; set; }

        /// <summary>
        /// 필요 폼
        /// </summary>
        public string RequiredForm { get; set; }

        /// <summary>
        /// 세트 ID
        /// </summary>
        public string SetId { get; set; }

        /// <summary>
        /// 세트 이름
        /// </summary>
        public string SetName { get; set; }


        // ====== 스탯 정보 ======

        /// <summary>
        /// 기본 스탯 목록
        /// </summary>
        public List<StatDisplayInfo> BaseStats { get; set; } = new List<StatDisplayInfo>();

        /// <summary>
        /// 랜덤 스탯 목록
        /// </summary>
        public List<StatDisplayInfo> RandomStats { get; set; } = new List<StatDisplayInfo>();


        // ====== 내구도 ======

        /// <summary>
        /// 내구도 사용 여부
        /// </summary>
        public bool HasDurability { get; set; }

        /// <summary>
        /// 현재 내구도
        /// </summary>
        public int CurrentDurability { get; set; }

        /// <summary>
        /// 최대 내구도
        /// </summary>
        public int MaxDurability { get; set; }

        /// <summary>
        /// 내구도 비율 (0-1)
        /// </summary>
        public float DurabilityRatio => MaxDurability > 0 ? (float)CurrentDurability / MaxDurability : 1f;


        // ====== 소비 정보 ======

        /// <summary>
        /// 소비 아이템 여부
        /// </summary>
        public bool IsConsumable { get; set; }

        /// <summary>
        /// 소비 효과 타입
        /// </summary>
        public ConsumeType ConsumeType { get; set; }

        /// <summary>
        /// 효과 수치
        /// </summary>
        public float EffectValue { get; set; }

        /// <summary>
        /// 효과 지속 시간
        /// </summary>
        public float EffectDuration { get; set; }

        /// <summary>
        /// 쿨다운
        /// </summary>
        public float Cooldown { get; set; }


        // ====== 경제 ======

        /// <summary>
        /// 판매 가격
        /// </summary>
        public int SellPrice { get; set; }

        /// <summary>
        /// 구매 가격
        /// </summary>
        public int BuyPrice { get; set; }


        // ====== 비교 정보 ======

        /// <summary>
        /// 현재 장비와 비교 (양수 = 업그레이드)
        /// </summary>
        public Dictionary<StatType, float> ComparedStats { get; set; } = new Dictionary<StatType, float>();


        // ====== 생성자 ======

        public ItemViewModelV2()
        {
            RarityColor = Color.white;
        }


        // ====== 팩토리 메서드 ======

        /// <summary>
        /// ItemInstance로부터 ViewModel 생성
        /// </summary>
        public static ItemViewModelV2 FromItemInstance(ItemInstance instance)
        {
            if (instance == null || !instance.IsValid)
                return null;

            ItemData itemData = instance.cachedItemData;

            ItemViewModelV2 vm = new ItemViewModelV2
            {
                InstanceId = instance.instanceId,
                ItemName = itemData.itemName,
                Description = itemData.description,
                Icon = itemData.icon,
                Category = itemData.category,
                Rarity = itemData.rarity,
                RarityColor = itemData.RarityColor,
                SellPrice = itemData.sellPrice,
                BuyPrice = itemData.buyPrice,
                IsEquipment = instance.IsEquipment,
                IsConsumable = instance.IsConsumable
            };

            // 장비 정보
            if (instance.IsEquipment)
            {
                EquipmentData equipData = instance.EquipmentData;
                vm.EquipSlot = equipData.equipSlot;
                vm.RequiredLevel = equipData.requiredLevel;
                vm.RequiredForm = equipData.requiredForm.ToString();
                vm.SetId = equipData.setId;
                vm.HasDurability = equipData.HasDurability;
                vm.MaxDurability = equipData.maxDurability;
                vm.CurrentDurability = instance.currentDurability;

                // 기본 스탯
                foreach (var stat in equipData.baseStats)
                {
                    vm.BaseStats.Add(new StatDisplayInfo(
                        stat.statType,
                        stat.value,
                        stat.modifierType == ModifierType.Percent
                    ));
                }

                // 랜덤 스탯
                foreach (var stat in instance.randomStats)
                {
                    vm.RandomStats.Add(new StatDisplayInfo(
                        stat.statType,
                        stat.value,
                        stat.modifierType == ModifierType.Percent
                    ));
                }
            }

            // 소비 정보
            if (instance.IsConsumable)
            {
                ConsumableData consumeData = instance.ConsumableData;
                vm.ConsumeType = consumeData.consumeType;
                vm.EffectValue = consumeData.effectValue;
                vm.EffectDuration = consumeData.effectDuration;
                vm.Cooldown = consumeData.cooldown;
            }

            return vm;
        }

        /// <summary>
        /// InventorySlot으로부터 ViewModel 생성
        /// </summary>
        public static ItemViewModelV2 FromInventorySlot(InventorySlot slot)
        {
            if (slot == null || slot.IsEmpty)
                return null;

            return FromItemInstance(slot.Item);
        }


        // ====== 유틸리티 ======

        /// <summary>
        /// 효과 설명 텍스트
        /// </summary>
        public string EffectDescription
        {
            get
            {
                if (!IsConsumable)
                    return "";

                return ConsumeType switch
                {
                    ConsumeType.Heal => $"HP +{EffectValue}",
                    ConsumeType.HealOverTime => $"HP +{EffectValue} / {EffectDuration}초",
                    ConsumeType.RestoreMana => $"MP +{EffectValue}",
                    ConsumeType.Buff => "버프 적용",
                    ConsumeType.Cleanse => "상태이상 해제",
                    ConsumeType.Revive => "부활",
                    _ => ""
                };
            }
        }

        /// <summary>
        /// 내구도 텍스트
        /// </summary>
        public string DurabilityText => HasDurability ? $"{CurrentDurability}/{MaxDurability}" : "";

        /// <summary>
        /// 슬롯 이름 텍스트
        /// </summary>
        public string SlotName => EquipSlot.ToString();
    }


    /// <summary>
    /// 스탯 표시 정보
    /// </summary>
    public class StatDisplayInfo
    {
        /// <summary>
        /// 스탯 종류
        /// </summary>
        public StatType StatType { get; set; }

        /// <summary>
        /// 스탯 이름
        /// </summary>
        public string StatName => StatType.ToString();

        /// <summary>
        /// 수치
        /// </summary>
        public float Value { get; set; }

        /// <summary>
        /// 퍼센트 여부
        /// </summary>
        public bool IsPercent { get; set; }

        /// <summary>
        /// 표시 텍스트
        /// </summary>
        public string DisplayText
        {
            get
            {
                string prefix = Value >= 0 ? "+" : "";
                string suffix = IsPercent ? "%" : "";
                return $"{StatName} {prefix}{Value}{suffix}";
            }
        }

        /// <summary>
        /// 색상 (양수=녹색, 음수=빨강)
        /// </summary>
        public Color ValueColor => Value >= 0 ? Color.green : Color.red;


        public StatDisplayInfo()
        {
        }

        public StatDisplayInfo(StatType statType, float value, bool isPercent = false)
        {
            StatType = statType;
            Value = value;
            IsPercent = isPercent;
        }
    }
}
