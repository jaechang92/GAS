using System.Collections.Generic;
using GASPT.Core.Enums;
using GASPT.Data;
using GASPT.Gameplay.Form;
using UnityEngine;

namespace GASPT.Stats
{
    /// <summary>
    /// PlayerStats 장비 및 폼 보너스 관련 partial class
    /// </summary>
    public partial class PlayerStats
    {
        // ====== 장비 착용/해제 ======

        [ContextMenu("Equip Test Item")]
        public void EquipTestItem()
        {
            if (testItemToEquip != null)
            {
                EquipItem(testItemToEquip);
            }
            else
            {
                Debug.LogWarning("[PlayerStats] EquipTestItem(): testItemToEquip이 설정되지 않았습니다.");
            }
        }

        /// <summary>
        /// 아이템 장착
        /// </summary>
        /// <param name="item">장착할 아이템</param>
        /// <returns>true: 장착 성공, false: 장착 실패</returns>
        public bool EquipItem(Item item)
        {
            if (item == null)
            {
                Debug.LogWarning("[PlayerStats] EquipItem(): item이 null입니다.");
                return false;
            }

            // 유효성 검증
            if (!item.Validate())
            {
                Debug.LogError($"[PlayerStats] EquipItem(): 유효하지 않은 아이템입니다: {item.name}");
                return false;
            }

            // 해당 슬롯에 이미 아이템이 있으면 자동으로 교체

            // 아이템 장착
            equippedItems[item.slot] = item;
            isDirty = true;

            // 스탯 재계산 (프로퍼티 접근 시 자동 수행됨)
            RecalculateIfDirty();

            return true;
        }

        /// <summary>
        /// 아이템 장착 해제
        /// </summary>
        /// <param name="slot">해제할 슬롯</param>
        /// <returns>true: 해제 성공, false: 해제 실패 (슬롯에 아이템 없음)</returns>
        public bool UnequipItem(EquipmentSlot slot)
        {
            if (!equippedItems.ContainsKey(slot))
            {
                Debug.LogWarning($"[PlayerStats] UnequipItem(): {slot} 슬롯에 장착된 아이템이 없습니다.");
                return false;
            }

            Item removedItem = equippedItems[slot];
            equippedItems.Remove(slot);
            isDirty = true;

            // 스탯 재계산
            RecalculateIfDirty();

            return true;
        }

        /// <summary>
        /// 특정 슬롯에 장착된 아이템 가져오기
        /// </summary>
        /// <param name="slot">확인할 슬롯</param>
        /// <returns>장착된 아이템 (없으면 null)</returns>
        public Item GetEquippedItem(EquipmentSlot slot)
        {
            equippedItems.TryGetValue(slot, out Item item);
            return item;
        }

        /// <summary>
        /// 모든 장착된 아이템 가져오기 (읽기 전용 복사본)
        /// </summary>
        /// <returns>장착된 아이템 딕셔너리 (읽기 전용)</returns>
        public Dictionary<EquipmentSlot, Item> GetAllEquippedItems()
        {
            return new Dictionary<EquipmentSlot, Item>(equippedItems);
        }

        /// <summary>
        /// 장착된 아이템 개수
        /// </summary>
        public int EquippedItemCount => equippedItems.Count;

        /// <summary>
        /// 모든 아이템 장착 해제
        /// </summary>
        public void UnequipAll()
        {
            equippedItems.Clear();
            isDirty = true;
            RecalculateIfDirty();
        }


        // ====== 폼 보너스 관리 ======

        /// <summary>
        /// 폼 보너스 적용 (FormSwapSystem에서 호출)
        /// </summary>
        /// <param name="stats">적용할 폼 스탯</param>
        public void ApplyFormBonus(FormStats stats)
        {
            formBonus = stats;
            hasFormBonus = true;
            isDirty = true;

            RecalculateIfDirty();
        }

        /// <summary>
        /// 폼 보너스 제거 (FormSwapSystem에서 호출)
        /// </summary>
        public void RemoveFormBonus()
        {
            if (!hasFormBonus) return;

            formBonus = FormStats.Default;
            hasFormBonus = false;
            isDirty = true;

            RecalculateIfDirty();
        }

        /// <summary>
        /// 현재 적용된 폼 보너스 조회
        /// </summary>
        public FormStats CurrentFormBonus => formBonus;

        /// <summary>
        /// 폼 보너스 적용 여부
        /// </summary>
        public bool HasFormBonus => hasFormBonus;
    }
}
