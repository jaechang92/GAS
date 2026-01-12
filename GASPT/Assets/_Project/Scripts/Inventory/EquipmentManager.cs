using System;
using System.Collections.Generic;
using UnityEngine;
using GASPT.Data;
using GASPT.Core.Enums;
using GASPT.Core;
using GASPT.Core.Utilities;
using GASPT.Stats;
using GASPT.Gameplay.Form;

namespace GASPT.Inventory
{
    /// <summary>
    /// 장비 매니저
    /// 7슬롯 장비 장착/해제 및 스탯 계산 관리
    /// </summary>
    public class EquipmentManager : SingletonManager<EquipmentManager>
    {
        // ====== 장비 데이터 ======

        /// <summary>
        /// 장착된 아이템 (슬롯 → 아이템 인스턴스)
        /// </summary>
        private Dictionary<EquipmentSlot, ItemInstance> equippedItems = new Dictionary<EquipmentSlot, ItemInstance>();

        /// <summary>
        /// 장비 스탯 캐시 (DirtyCache 패턴)
        /// </summary>
        private DirtyCache<Dictionary<StatType, float>> statsCache;


        // ====== 이벤트 ======

        /// <summary>
        /// 장비 장착 이벤트
        /// 매개변수: (슬롯, 새 아이템)
        /// </summary>
        public event Action<EquipmentSlot, ItemInstance> OnEquipped;

        /// <summary>
        /// 장비 해제 이벤트
        /// 매개변수: (슬롯, 해제된 아이템)
        /// </summary>
        public event Action<EquipmentSlot, ItemInstance> OnUnequipped;

        /// <summary>
        /// 장비 스탯 변경 이벤트
        /// </summary>
        public event Action OnEquipmentStatsChanged;


        // ====== 프로퍼티 ======

        /// <summary>
        /// 장착된 아이템 수
        /// </summary>
        public int EquippedCount => equippedItems.Count;


        // ====== Unity 생명주기 ======

        protected override void OnAwake()
        {
            InitializeSlots();
            Debug.Log("[EquipmentManager] 초기화 완료 (7슬롯)");
        }


        // ====== 초기화 ======

        /// <summary>
        /// 장비 슬롯 초기화
        /// </summary>
        private void InitializeSlots()
        {
            equippedItems.Clear();

            // DirtyCache 초기화 (장비 변경 시 재계산)
            statsCache = new DirtyCache<Dictionary<StatType, float>>(
                () => StatCalculator.CalculateEquipmentStats(equippedItems.Values)
            );
        }


        // ====== 장착 조건 검증 ======

        /// <summary>
        /// 장착 가능 여부 확인
        /// </summary>
        /// <param name="itemInstance">장착할 아이템</param>
        /// <param name="playerLevel">플레이어 레벨</param>
        /// <param name="currentForm">현재 폼</param>
        /// <returns>장착 결과</returns>
        public EquipResult CanEquip(ItemInstance itemInstance, int playerLevel = 1, FormType currentForm = FormType.None)
        {
            if (itemInstance == null || !itemInstance.IsValid)
            {
                return EquipResult.InvalidSlot;
            }

            EquipmentData equipData = itemInstance.EquipmentData;

            if (equipData == null)
            {
                return EquipResult.InvalidSlot;
            }

            // 레벨 조건
            if (playerLevel < equipData.requiredLevel)
            {
                return EquipResult.LevelTooLow;
            }

            // 폼 조건
            if (equipData.requiredForm != FormType.None && equipData.requiredForm != currentForm)
            {
                return EquipResult.WrongForm;
            }

            // 슬롯 유효성
            if (equipData.equipSlot == EquipmentSlot.None)
            {
                return EquipResult.InvalidSlot;
            }

            return EquipResult.Success;
        }


        // ====== 장착 ======

        /// <summary>
        /// 아이템 장착
        /// </summary>
        /// <param name="itemInstance">장착할 아이템</param>
        /// <param name="playerLevel">플레이어 레벨</param>
        /// <param name="currentForm">현재 폼</param>
        /// <returns>장착 결과</returns>
        public EquipResult Equip(ItemInstance itemInstance, int playerLevel = 1, FormType currentForm = FormType.None)
        {
            // 조건 검증
            EquipResult result = CanEquip(itemInstance, playerLevel, currentForm);

            if (result != EquipResult.Success)
            {
                Debug.LogWarning($"[EquipmentManager] 장착 실패: {result}");
                return result;
            }

            EquipmentData equipData = itemInstance.EquipmentData;
            EquipmentSlot slot = equipData.equipSlot;

            // 기존 아이템 해제
            if (equippedItems.TryGetValue(slot, out ItemInstance oldItem))
            {
                UnequipInternal(slot, oldItem);
            }

            // 새 아이템 장착
            equippedItems[slot] = itemInstance;
            itemInstance.isEquipped = true;

            // 스탯 캐시 무효화
            InvalidateStatsCache();

            // 이벤트 발생
            OnEquipped?.Invoke(slot, itemInstance);
            OnEquipmentStatsChanged?.Invoke();

            Debug.Log($"[EquipmentManager] 장착: {equipData.itemName} → {slot}");

            return EquipResult.Success;
        }

        /// <summary>
        /// ItemData로부터 장착 (인스턴스 자동 생성)
        /// </summary>
        /// <param name="equipmentData">장비 데이터</param>
        /// <param name="playerLevel">플레이어 레벨</param>
        /// <param name="currentForm">현재 폼</param>
        /// <returns>장착 결과</returns>
        public EquipResult EquipFromData(EquipmentData equipmentData, int playerLevel = 1, FormType currentForm = FormType.None)
        {
            if (equipmentData == null)
            {
                return EquipResult.InvalidSlot;
            }

            ItemInstance instance = ItemInstance.CreateFromData(equipmentData);
            return Equip(instance, playerLevel, currentForm);
        }


        // ====== 해제 ======

        /// <summary>
        /// 아이템 해제
        /// </summary>
        /// <param name="slot">해제할 슬롯</param>
        /// <returns>해제된 아이템 (없으면 null)</returns>
        public ItemInstance Unequip(EquipmentSlot slot)
        {
            if (!equippedItems.TryGetValue(slot, out ItemInstance item))
            {
                Debug.LogWarning($"[EquipmentManager] 해제 실패: {slot} 슬롯에 장비 없음");
                return null;
            }

            UnequipInternal(slot, item);
            equippedItems.Remove(slot);

            // 스탯 캐시 무효화
            InvalidateStatsCache();

            // 이벤트 발생
            OnUnequipped?.Invoke(slot, item);
            OnEquipmentStatsChanged?.Invoke();

            Debug.Log($"[EquipmentManager] 해제: {item.cachedItemData?.itemName} ← {slot}");

            return item;
        }

        /// <summary>
        /// 내부 해제 처리
        /// </summary>
        private void UnequipInternal(EquipmentSlot slot, ItemInstance item)
        {
            if (item != null)
            {
                item.isEquipped = false;
            }
        }

        /// <summary>
        /// 모든 장비 해제
        /// </summary>
        /// <returns>해제된 아이템 목록</returns>
        public List<ItemInstance> UnequipAll()
        {
            List<ItemInstance> unequippedItems = new List<ItemInstance>();

            foreach (var kvp in equippedItems)
            {
                UnequipInternal(kvp.Key, kvp.Value);
                unequippedItems.Add(kvp.Value);
            }

            equippedItems.Clear();
            InvalidateStatsCache();

            OnEquipmentStatsChanged?.Invoke();

            Debug.Log($"[EquipmentManager] 전체 해제: {unequippedItems.Count}개");

            return unequippedItems;
        }


        // ====== 조회 ======

        /// <summary>
        /// 특정 슬롯의 장비 가져오기
        /// </summary>
        /// <param name="slot">장비 슬롯</param>
        /// <returns>장착된 아이템 (없으면 null)</returns>
        public ItemInstance GetEquipped(EquipmentSlot slot)
        {
            equippedItems.TryGetValue(slot, out ItemInstance item);
            return item;
        }

        /// <summary>
        /// 모든 장착된 아이템 가져오기
        /// </summary>
        /// <returns>장착된 아이템 딕셔너리 복사본</returns>
        public Dictionary<EquipmentSlot, ItemInstance> GetAllEquipped()
        {
            return new Dictionary<EquipmentSlot, ItemInstance>(equippedItems);
        }

        /// <summary>
        /// 장착된 아이템 목록 가져오기
        /// </summary>
        /// <returns>장착된 아이템 리스트</returns>
        public List<ItemInstance> GetEquippedList()
        {
            return new List<ItemInstance>(equippedItems.Values);
        }

        /// <summary>
        /// 슬롯에 장비가 있는지 확인
        /// </summary>
        /// <param name="slot">장비 슬롯</param>
        /// <returns>true = 장비 있음</returns>
        public bool HasEquipped(EquipmentSlot slot)
        {
            return equippedItems.ContainsKey(slot);
        }


        // ====== 스탯 계산 (DirtyCache 위임) ======

        /// <summary>
        /// 스탯 캐시 무효화
        /// </summary>
        private void InvalidateStatsCache()
        {
            statsCache?.Invalidate();
        }

        /// <summary>
        /// 장비로 인한 스탯 보너스 가져오기
        /// </summary>
        /// <param name="statType">스탯 종류</param>
        /// <returns>보너스 수치</returns>
        public float GetEquipmentBonus(StatType statType)
        {
            return statsCache.Value.GetValueOrDefault(statType, 0f);
        }

        /// <summary>
        /// 모든 장비 스탯 보너스 가져오기
        /// </summary>
        /// <returns>스탯별 보너스 딕셔너리</returns>
        public Dictionary<StatType, float> GetAllEquipmentBonuses()
        {
            return new Dictionary<StatType, float>(statsCache.Value);
        }


        // ====== 저장/로드 데이터 ======

        /// <summary>
        /// 장비 데이터 저장용 구조 생성
        /// </summary>
        public List<EquippedItemSaveData> GetSaveData()
        {
            List<EquippedItemSaveData> saveData = new List<EquippedItemSaveData>();

            foreach (var kvp in equippedItems)
            {
                if (kvp.Value == null)
                    continue;

                EquippedItemSaveData data = new EquippedItemSaveData
                {
                    slot = kvp.Key,
                    instanceId = kvp.Value.instanceId,
                    itemDataPath = kvp.Value.itemDataPath,
                    currentDurability = kvp.Value.currentDurability,
                    randomStats = new List<Save.StatModifierData>()
                };

                // 랜덤 스탯 저장
                foreach (var stat in kvp.Value.randomStats)
                {
                    data.randomStats.Add(new Save.StatModifierData(
                        stat.statType,
                        stat.modifierType,
                        stat.value
                    ));
                }

                saveData.Add(data);
            }

            return saveData;
        }

        /// <summary>
        /// 저장 데이터로부터 장비 복원
        /// </summary>
        public void LoadFromSaveData(List<EquippedItemSaveData> saveData)
        {
            equippedItems.Clear();

            if (saveData == null)
                return;

            foreach (var data in saveData)
            {
                if (string.IsNullOrEmpty(data.itemDataPath))
                    continue;

                // ItemInstance 복원 (팩토리 메서드 사용)
                ItemInstance instance = ItemInstance.RestoreFromSaveData(
                    data.instanceId,
                    data.itemDataPath,
                    data.currentDurability,
                    isEquipped: true,
                    acquireTimeTicks: 0,
                    data.randomStats
                );

                if (instance != null)
                {
                    equippedItems[data.slot] = instance;
                }
            }

            InvalidateStatsCache();
            OnEquipmentStatsChanged?.Invoke();

            Debug.Log($"[EquipmentManager] 로드 완료: {equippedItems.Count}개 장비");
        }


        // ====== 디버그 ======

        /// <summary>
        /// 장비 정보 출력
        /// </summary>
        [ContextMenu("Print Equipment")]
        public void DebugPrintEquipment()
        {
            Debug.Log($"[EquipmentManager] ========== 장비 ({equippedItems.Count}/7) ==========");

            foreach (EquipmentSlot slot in Enum.GetValues(typeof(EquipmentSlot)))
            {
                if (slot == EquipmentSlot.None)
                    continue;

                if (equippedItems.TryGetValue(slot, out ItemInstance item))
                {
                    string name = item.cachedItemData?.itemName ?? "Unknown";
                    Debug.Log($"[EquipmentManager] {slot}: {name}");
                }
                else
                {
                    Debug.Log($"[EquipmentManager] {slot}: (비어있음)");
                }
            }

            Debug.Log("[EquipmentManager] =====================================");
        }

        /// <summary>
        /// 장비 스탯 출력
        /// </summary>
        [ContextMenu("Print Equipment Stats")]
        public void DebugPrintStats()
        {
            Debug.Log($"[EquipmentManager] ========== 장비 스탯 (dirty: {statsCache?.IsDirty ?? true}) ==========");

            foreach (var kvp in statsCache.Value)
            {
                if (kvp.Value != 0)
                {
                    Debug.Log($"[EquipmentManager] {kvp.Key}: +{kvp.Value}");
                }
            }

            Debug.Log("[EquipmentManager] =================================");
        }
    }


    /// <summary>
    /// 장착된 장비 저장 데이터
    /// </summary>
    [Serializable]
    public class EquippedItemSaveData
    {
        public EquipmentSlot slot;
        public string instanceId;
        public string itemDataPath;
        public int currentDurability;
        public List<Save.StatModifierData> randomStats;
    }
}
