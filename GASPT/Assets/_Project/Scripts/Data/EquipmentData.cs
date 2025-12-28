using System.Collections.Generic;
using UnityEngine;
using GASPT.Core.Enums;
using GASPT.Gameplay.Form;

namespace GASPT.Data
{
    /// <summary>
    /// 장비 아이템 데이터 (ScriptableObject)
    /// ItemData를 상속받아 장비 전용 필드 추가
    /// </summary>
    [CreateAssetMenu(fileName = "EquipmentData", menuName = "GASPT/Items/EquipmentData")]
    public class EquipmentData : ItemData
    {
        // ====== 장비 정보 ======

        [Header("장비 정보")]
        [Tooltip("장착 슬롯")]
        public EquipmentSlot equipSlot = EquipmentSlot.Weapon;

        [Tooltip("착용 레벨 제한")]
        [Min(1)]
        public int requiredLevel = 1;

        [Tooltip("착용 폼 제한 (None = 제한 없음)")]
        public FormType requiredForm = FormType.None;


        // ====== 기본 스탯 ======

        [Header("기본 스탯")]
        [Tooltip("기본 스탯 보너스 목록")]
        public List<StatModifier> baseStats = new List<StatModifier>();


        // ====== 추가 스탯 생성 ======

        [Header("추가 스탯 생성")]
        [Tooltip("최소 랜덤 스탯 수")]
        [Range(0, 4)]
        public int minRandomStats = 0;

        [Tooltip("최대 랜덤 스탯 수")]
        [Range(0, 4)]
        public int maxRandomStats = 0;

        [Tooltip("가능한 랜덤 스탯 종류")]
        public List<StatType> possibleRandomStats = new List<StatType>();


        // ====== 세트 아이템 ======

        [Header("세트 아이템")]
        [Tooltip("세트 ID (빈 문자열 = 세트 아님)")]
        public string setId = "";


        // ====== 내구도 ======

        [Header("내구도")]
        [Tooltip("최대 내구도 (-1 = 내구도 없음)")]
        public int maxDurability = -1;


        // ====== 프로퍼티 ======

        /// <summary>
        /// 세트 아이템 여부
        /// </summary>
        public bool IsSetItem => !string.IsNullOrEmpty(setId);

        /// <summary>
        /// 내구도 사용 여부
        /// </summary>
        public bool HasDurability => maxDurability > 0;


        // ====== Unity 콜백 ======

        private void OnValidate()
        {
            // 카테고리를 장비로 강제
            category = ItemCategory.Equipment;

            // 장비는 스택 불가
            stackable = false;
            maxStack = 1;

            // maxRandomStats가 minRandomStats보다 작으면 보정
            if (maxRandomStats < minRandomStats)
            {
                maxRandomStats = minRandomStats;
            }

            // itemId 자동 생성 (비어있을 때)
            if (string.IsNullOrEmpty(itemId))
            {
                string prefix = equipSlot switch
                {
                    EquipmentSlot.Weapon => "WPN",
                    EquipmentSlot.Armor => "ARM",
                    EquipmentSlot.Helmet => "HLM",
                    EquipmentSlot.Gloves => "GLV",
                    EquipmentSlot.Boots => "BOT",
                    EquipmentSlot.Accessory1 => "ACC",
                    EquipmentSlot.Accessory2 => "ACC",
                    _ => "EQP"
                };
                itemId = $"{prefix}_{name}";
            }
        }


        // ====== 메서드 ======

        /// <summary>
        /// 기본 스탯 합계 계산
        /// </summary>
        /// <param name="statType">계산할 스탯 종류</param>
        /// <returns>해당 스탯의 합계</returns>
        public float GetBaseStat(StatType statType)
        {
            float total = 0f;

            foreach (var modifier in baseStats)
            {
                if (modifier.statType == statType && modifier.modifierType == ModifierType.Flat)
                {
                    total += modifier.value;
                }
            }

            return total;
        }

        /// <summary>
        /// 기본 스탯 퍼센트 합계 계산
        /// </summary>
        /// <param name="statType">계산할 스탯 종류</param>
        /// <returns>해당 스탯의 퍼센트 합계</returns>
        public float GetBaseStatPercent(StatType statType)
        {
            float total = 0f;

            foreach (var modifier in baseStats)
            {
                if (modifier.statType == statType && modifier.modifierType == ModifierType.Percent)
                {
                    total += modifier.value;
                }
            }

            return total;
        }


        // ====== 디버그 ======

        /// <summary>
        /// 디버그용 문자열 출력
        /// </summary>
        public override string ToString()
        {
            return $"[{rarity}] {itemName} ({equipSlot}) Lv.{requiredLevel}";
        }
    }
}
