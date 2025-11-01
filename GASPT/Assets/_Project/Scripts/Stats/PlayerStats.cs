using System;
using System.Collections.Generic;
using UnityEngine;
using Core.Enums;
using GASPT.Data;

namespace GASPT.Stats
{
    /// <summary>
    /// 플레이어 스탯 관리 MonoBehaviour
    /// 기본 스탯 + 장비 보너스 = 최종 스탯
    /// Dirty flag 최적화로 변경 시에만 재계산 (<50ms)
    /// </summary>
    public class PlayerStats : MonoBehaviour
    {
        // ====== 기본 스탯 ======

        [Header("기본 스탯")]
        [SerializeField] [Tooltip("기본 HP")]
        private int baseHP = 100;

        [SerializeField] [Tooltip("기본 공격력")]
        private int baseAttack = 10;

        [SerializeField] [Tooltip("기본 방어력")]
        private int baseDefense = 5;


        // ====== 장비 슬롯 ======

        /// <summary>
        /// 장비된 아이템 (슬롯 → 아이템)
        /// </summary>
        private Dictionary<EquipmentSlot, Item> equippedItems = new Dictionary<EquipmentSlot, Item>();


        // ====== 최종 스탯 (캐시) ======

        private int finalHP;
        private int finalAttack;
        private int finalDefense;

        /// <summary>
        /// Dirty flag: true면 재계산 필요
        /// </summary>
        private bool isDirty = true;


        // ====== 프로퍼티 (외부 접근) ======

        /// <summary>
        /// 최종 HP (기본 + 장비 보너스)
        /// </summary>
        public int HP
        {
            get
            {
                RecalculateIfDirty();
                return finalHP;
            }
        }

        /// <summary>
        /// 최종 공격력 (기본 + 장비 보너스)
        /// </summary>
        public int Attack
        {
            get
            {
                RecalculateIfDirty();
                return finalAttack;
            }
        }

        /// <summary>
        /// 최종 방어력 (기본 + 장비 보너스)
        /// </summary>
        public int Defense
        {
            get
            {
                RecalculateIfDirty();
                return finalDefense;
            }
        }


        // ====== 이벤트 ======

        /// <summary>
        /// 스탯 변경 시 발생하는 이벤트
        /// 매개변수: (StatType, 이전 값, 새 값)
        /// </summary>
        public event Action<StatType, int, int> OnStatChanged;


        // ====== Unity 생명주기 ======

        private void Awake()
        {
            // 장비 슬롯 초기화
            equippedItems = new Dictionary<EquipmentSlot, Item>();

            // 초기 스탯 계산
            RecalculateStats();

            Debug.Log($"[PlayerStats] 초기화 완료 - HP: {HP}, Attack: {Attack}, Defense: {Defense}");
        }


        // ====== 스탯 계산 (Dirty Flag 최적화) ======

        /// <summary>
        /// Dirty flag 확인 후 필요시 재계산
        /// </summary>
        private void RecalculateIfDirty()
        {
            if (isDirty)
            {
                RecalculateStats();
            }
        }

        /// <summary>
        /// 최종 스탯 재계산
        /// 기본 스탯 + 모든 장비 보너스 합산
        /// </summary>
        private void RecalculateStats()
        {
            // 이전 값 저장 (이벤트용)
            int oldHP = finalHP;
            int oldAttack = finalAttack;
            int oldDefense = finalDefense;

            // 기본 스탯으로 초기화
            finalHP = baseHP;
            finalAttack = baseAttack;
            finalDefense = baseDefense;

            // 장비 보너스 합산
            foreach (var item in equippedItems.Values)
            {
                if (item != null)
                {
                    finalHP += item.hpBonus;
                    finalAttack += item.attackBonus;
                    finalDefense += item.defenseBonus;
                }
            }

            isDirty = false;

            // 변경된 스탯에 대해 이벤트 발생
            NotifyStatChangedIfDifferent(StatType.HP, oldHP, finalHP);
            NotifyStatChangedIfDifferent(StatType.Attack, oldAttack, finalAttack);
            NotifyStatChangedIfDifferent(StatType.Defense, oldDefense, finalDefense);

            Debug.Log($"[PlayerStats] 스탯 재계산 완료 - HP: {finalHP}, Attack: {finalAttack}, Defense: {finalDefense}");
        }

        /// <summary>
        /// 값이 변경된 경우에만 이벤트 발생
        /// </summary>
        private void NotifyStatChangedIfDifferent(StatType statType, int oldValue, int newValue)
        {
            if (oldValue != newValue)
            {
                OnStatChanged?.Invoke(statType, oldValue, newValue);
                Debug.Log($"[PlayerStats] {statType} 변경: {oldValue} → {newValue}");
            }
        }


        // ====== 장비 착용/해제 ======

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
            if (equippedItems.ContainsKey(item.slot))
            {
                Item oldItem = equippedItems[item.slot];
                Debug.Log($"[PlayerStats] {item.slot} 슬롯의 {oldItem.itemName}을(를) {item.itemName}(으)로 교체합니다.");
            }

            // 아이템 장착
            equippedItems[item.slot] = item;
            isDirty = true;

            // 스탯 재계산 (프로퍼티 접근 시 자동 수행됨)
            RecalculateIfDirty();

            Debug.Log($"[PlayerStats] {item.itemName} 장착 완료 ({item.slot})");
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

            Debug.Log($"[PlayerStats] {removedItem.itemName} 장착 해제 완료 ({slot})");
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
        /// 모든 아이템 장착 해제
        /// </summary>
        public void UnequipAll()
        {
            equippedItems.Clear();
            isDirty = true;
            RecalculateIfDirty();

            Debug.Log("[PlayerStats] 모든 아이템 장착 해제 완료");
        }


        // ====== 디버그 ======

        /// <summary>
        /// 현재 스탯 정보 출력
        /// </summary>
        public void DebugPrintStats()
        {
            Debug.Log("========== PlayerStats ==========");
            Debug.Log($"기본 스탯: HP {baseHP}, Attack {baseAttack}, Defense {baseDefense}");
            Debug.Log($"최종 스탯: HP {HP}, Attack {Attack}, Defense {Defense}");
            Debug.Log($"장착 아이템 수: {equippedItems.Count}");

            foreach (var kvp in equippedItems)
            {
                Debug.Log($"  - {kvp.Key}: {kvp.Value.itemName}");
            }

            Debug.Log("=================================");
        }
    }
}
