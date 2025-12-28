using System.Collections.Generic;
using UnityEngine;
using GASPT.Core.Enums;
using GASPT.Data;

namespace GASPT.Stats
{
    /// <summary>
    /// 스탯 계산 유틸리티 클래스
    /// 장비 스탯, 세트 효과, 최종 스탯 계산
    /// </summary>
    public static class StatCalculator
    {
        // ====== 장비 스탯 계산 ======

        /// <summary>
        /// 장비 아이템들의 스탯 합계 계산
        /// </summary>
        /// <param name="equippedItems">장착된 아이템 목록</param>
        /// <returns>스탯별 보너스 딕셔너리</returns>
        public static Dictionary<StatType, float> CalculateEquipmentStats(IEnumerable<ItemInstance> equippedItems)
        {
            Dictionary<StatType, float> flatBonuses = new Dictionary<StatType, float>();
            Dictionary<StatType, float> percentBonuses = new Dictionary<StatType, float>();

            if (equippedItems == null)
                return flatBonuses;

            // 모든 스탯 타입 초기화
            foreach (StatType statType in System.Enum.GetValues(typeof(StatType)))
            {
                flatBonuses[statType] = 0f;
                percentBonuses[statType] = 0f;
            }

            // 각 장비의 스탯 합산
            foreach (ItemInstance itemInstance in equippedItems)
            {
                if (itemInstance == null || !itemInstance.IsValid)
                    continue;

                // 모든 스탯 수정자 가져오기 (기본 + 랜덤)
                List<StatModifier> modifiers = itemInstance.GetAllStatModifiers();

                foreach (StatModifier modifier in modifiers)
                {
                    if (modifier.modifierType == ModifierType.Flat)
                    {
                        flatBonuses[modifier.statType] += modifier.value;
                    }
                    else if (modifier.modifierType == ModifierType.Percent)
                    {
                        percentBonuses[modifier.statType] += modifier.value;
                    }
                }
            }

            // 최종 값 계산: flat + (flat * percent / 100)
            Dictionary<StatType, float> finalBonuses = new Dictionary<StatType, float>();

            foreach (StatType statType in System.Enum.GetValues(typeof(StatType)))
            {
                float flat = flatBonuses.GetValueOrDefault(statType, 0f);
                float percent = percentBonuses.GetValueOrDefault(statType, 0f);

                // 퍼센트 보너스 적용
                float bonus = flat + (flat * percent / 100f);
                finalBonuses[statType] = bonus;
            }

            return finalBonuses;
        }


        // ====== 최종 스탯 계산 ======

        /// <summary>
        /// 기본 스탯에 보너스를 적용하여 최종 스탯 계산
        /// </summary>
        /// <param name="baseStat">기본 스탯</param>
        /// <param name="flatBonus">고정 보너스</param>
        /// <param name="percentBonus">퍼센트 보너스</param>
        /// <returns>최종 스탯</returns>
        public static float CalculateFinalStat(float baseStat, float flatBonus, float percentBonus)
        {
            // 순서: base + flat, 그 후 percent 적용
            float afterFlat = baseStat + flatBonus;
            float finalValue = afterFlat * (1f + percentBonus / 100f);

            return Mathf.Max(0f, finalValue);
        }

        /// <summary>
        /// 기본 스탯에 장비 보너스를 적용하여 최종 스탯 계산
        /// </summary>
        /// <param name="baseStat">기본 스탯</param>
        /// <param name="equipmentBonuses">장비 보너스 딕셔너리</param>
        /// <param name="statType">스탯 종류</param>
        /// <returns>최종 스탯</returns>
        public static float CalculateFinalStat(float baseStat, Dictionary<StatType, float> equipmentBonuses, StatType statType)
        {
            float bonus = equipmentBonuses.GetValueOrDefault(statType, 0f);
            return baseStat + bonus;
        }


        // ====== 세트 효과 계산 ======

        /// <summary>
        /// 세트 효과 보너스 계산
        /// </summary>
        /// <param name="equippedItems">장착된 아이템 목록</param>
        /// <param name="setItemDatabase">세트 아이템 데이터베이스</param>
        /// <returns>세트 효과 보너스 딕셔너리</returns>
        public static Dictionary<StatType, float> CalculateSetBonuses(
            IEnumerable<ItemInstance> equippedItems,
            IEnumerable<SetItemData> setItemDatabase)
        {
            Dictionary<StatType, float> setBonuses = new Dictionary<StatType, float>();

            if (equippedItems == null || setItemDatabase == null)
                return setBonuses;

            // 스탯 타입 초기화
            foreach (StatType statType in System.Enum.GetValues(typeof(StatType)))
            {
                setBonuses[statType] = 0f;
            }

            // 세트별 장착 수 계산
            Dictionary<string, int> setEquipCount = new Dictionary<string, int>();

            foreach (ItemInstance itemInstance in equippedItems)
            {
                if (itemInstance == null || !itemInstance.IsValid)
                    continue;

                EquipmentData equipData = itemInstance.EquipmentData;
                if (equipData == null || !equipData.IsSetItem)
                    continue;

                string setId = equipData.setId;
                if (!setEquipCount.ContainsKey(setId))
                {
                    setEquipCount[setId] = 0;
                }
                setEquipCount[setId]++;
            }

            // 세트 효과 적용
            foreach (SetItemData setData in setItemDatabase)
            {
                if (setData == null)
                    continue;

                if (!setEquipCount.TryGetValue(setData.setId, out int count))
                    continue;

                // 활성화된 세트 보너스 적용
                foreach (SetBonus bonus in setData.bonuses)
                {
                    if (count >= bonus.requiredPieces)
                    {
                        foreach (StatModifier modifier in bonus.statBonuses)
                        {
                            if (modifier.modifierType == ModifierType.Flat)
                            {
                                setBonuses[modifier.statType] += modifier.value;
                            }
                        }
                    }
                }
            }

            return setBonuses;
        }


        // ====== 유틸리티 ======

        /// <summary>
        /// 스탯 수정자 목록의 특정 스탯 합계
        /// </summary>
        /// <param name="modifiers">스탯 수정자 목록</param>
        /// <param name="statType">스탯 종류</param>
        /// <param name="modifierType">수정자 타입</param>
        /// <returns>합계</returns>
        public static float SumModifiers(List<StatModifier> modifiers, StatType statType, ModifierType modifierType)
        {
            if (modifiers == null)
                return 0f;

            float sum = 0f;

            foreach (StatModifier mod in modifiers)
            {
                if (mod.statType == statType && mod.modifierType == modifierType)
                {
                    sum += mod.value;
                }
            }

            return sum;
        }

        /// <summary>
        /// 장비 스탯 비교 (업그레이드 여부 확인)
        /// </summary>
        /// <param name="currentItem">현재 장비</param>
        /// <param name="newItem">새 장비</param>
        /// <param name="statType">비교할 스탯</param>
        /// <returns>양수: 새 장비가 우수, 음수: 현재 장비가 우수, 0: 동일</returns>
        public static float CompareEquipment(ItemInstance currentItem, ItemInstance newItem, StatType statType)
        {
            float currentValue = 0f;
            float newValue = 0f;

            if (currentItem != null && currentItem.IsValid)
            {
                List<StatModifier> mods = currentItem.GetAllStatModifiers();
                currentValue = SumModifiers(mods, statType, ModifierType.Flat);
            }

            if (newItem != null && newItem.IsValid)
            {
                List<StatModifier> mods = newItem.GetAllStatModifiers();
                newValue = SumModifiers(mods, statType, ModifierType.Flat);
            }

            return newValue - currentValue;
        }

        /// <summary>
        /// 장비 총 전투력 계산 (간단한 점수화)
        /// </summary>
        /// <param name="itemInstance">장비 인스턴스</param>
        /// <returns>전투력 점수</returns>
        public static float CalculateCombatPower(ItemInstance itemInstance)
        {
            if (itemInstance == null || !itemInstance.IsValid)
                return 0f;

            List<StatModifier> mods = itemInstance.GetAllStatModifiers();

            float power = 0f;

            foreach (StatModifier mod in mods)
            {
                if (mod.modifierType != ModifierType.Flat)
                    continue;

                // 스탯별 가중치
                float weight = mod.statType switch
                {
                    StatType.Attack => 1.5f,
                    StatType.Defense => 1.2f,
                    StatType.HP => 0.5f,
                    StatType.Mana => 0.3f,
                    StatType.CriticalRate => 3.0f,
                    StatType.Speed => 2.0f,
                    _ => 1.0f
                };

                power += mod.value * weight;
            }

            // 등급 보너스
            if (itemInstance.cachedItemData != null)
            {
                power *= ItemConstants.GetStatMultiplier(itemInstance.cachedItemData.rarity);
            }

            return power;
        }
    }
}
