using System;
using System.Collections.Generic;
using UnityEngine;
using GASPT.Data;
using GASPT.Core;
using GASPT.Core.Enums;

namespace GASPT.Inventory
{
    /// <summary>
    /// 세트 아이템 보너스 시스템
    /// 장착된 세트 아이템 추적 및 세트 효과 적용
    /// </summary>
    public class SetItemBonusSystem : SingletonManager<SetItemBonusSystem>
    {
        // ====== 세트 데이터 ======

        [Header("세트 데이터")]
        [SerializeField]
        [Tooltip("모든 세트 데이터 목록")]
        private List<SetItemData> allSetData = new List<SetItemData>();

        /// <summary>
        /// 세트 ID → SetItemData 매핑
        /// </summary>
        private Dictionary<string, SetItemData> setDataMap = new Dictionary<string, SetItemData>();

        /// <summary>
        /// 장착된 세트별 피스 수
        /// </summary>
        private Dictionary<string, int> equippedSetPieces = new Dictionary<string, int>();

        /// <summary>
        /// 활성화된 세트 보너스
        /// </summary>
        private Dictionary<string, List<SetBonus>> activeBonuses = new Dictionary<string, List<SetBonus>>();


        // ====== 이벤트 ======

        /// <summary>
        /// 세트 보너스 변경 이벤트
        /// 매개변수: (세트 ID, 장착된 피스 수, 활성화된 보너스)
        /// </summary>
        public event Action<string, int, List<SetBonus>> OnSetBonusChanged;

        /// <summary>
        /// 전체 스탯 변경 이벤트
        /// </summary>
        public event Action OnSetStatsChanged;


        // ====== Unity 생명주기 ======

        protected override void OnAwake()
        {
            InitializeSetDataMap();
            SubscribeToEquipmentEvents();
            Debug.Log($"[SetItemBonusSystem] 초기화 완료 ({setDataMap.Count}개 세트 등록)");
        }

        private void OnDestroy()
        {
            UnsubscribeFromEquipmentEvents();
        }


        // ====== 초기화 ======

        /// <summary>
        /// 세트 데이터 맵 초기화
        /// </summary>
        private void InitializeSetDataMap()
        {
            setDataMap.Clear();

            foreach (var setData in allSetData)
            {
                if (setData != null && !string.IsNullOrEmpty(setData.setId))
                {
                    setDataMap[setData.setId] = setData;
                }
            }
        }

        /// <summary>
        /// EquipmentManager 이벤트 구독
        /// </summary>
        private void SubscribeToEquipmentEvents()
        {
            if (EquipmentManager.HasInstance)
            {
                EquipmentManager.Instance.OnEquipped += OnItemEquipped;
                EquipmentManager.Instance.OnUnequipped += OnItemUnequipped;
            }
        }

        /// <summary>
        /// EquipmentManager 이벤트 구독 해제
        /// </summary>
        private void UnsubscribeFromEquipmentEvents()
        {
            if (EquipmentManager.HasInstance)
            {
                EquipmentManager.Instance.OnEquipped -= OnItemEquipped;
                EquipmentManager.Instance.OnUnequipped -= OnItemUnequipped;
            }
        }


        // ====== 세트 데이터 등록 ======

        /// <summary>
        /// 세트 데이터 등록 (런타임)
        /// </summary>
        /// <param name="setData">등록할 세트 데이터</param>
        public void RegisterSetData(SetItemData setData)
        {
            if (setData == null || string.IsNullOrEmpty(setData.setId))
            {
                Debug.LogWarning("[SetItemBonusSystem] 유효하지 않은 SetItemData");
                return;
            }

            setDataMap[setData.setId] = setData;
            Debug.Log($"[SetItemBonusSystem] 세트 등록: {setData.setName} ({setData.setId})");
        }

        /// <summary>
        /// 세트 데이터 목록 등록
        /// </summary>
        /// <param name="setDataList">등록할 세트 데이터 목록</param>
        public void RegisterSetDataList(IEnumerable<SetItemData> setDataList)
        {
            foreach (var setData in setDataList)
            {
                RegisterSetData(setData);
            }
        }


        // ====== 장비 이벤트 핸들러 ======

        /// <summary>
        /// 아이템 장착 시
        /// </summary>
        private void OnItemEquipped(EquipmentSlot slot, ItemInstance item)
        {
            if (item == null || item.cachedItemData == null)
                return;

            string itemId = item.cachedItemData.itemId;

            // 세트에 포함된 아이템인지 확인
            foreach (var setData in setDataMap.Values)
            {
                if (setData.ContainsItem(itemId))
                {
                    IncrementSetPiece(setData.setId);
                }
            }
        }

        /// <summary>
        /// 아이템 해제 시
        /// </summary>
        private void OnItemUnequipped(EquipmentSlot slot, ItemInstance item)
        {
            if (item == null || item.cachedItemData == null)
                return;

            string itemId = item.cachedItemData.itemId;

            // 세트에 포함된 아이템인지 확인
            foreach (var setData in setDataMap.Values)
            {
                if (setData.ContainsItem(itemId))
                {
                    DecrementSetPiece(setData.setId);
                }
            }
        }


        // ====== 세트 피스 추적 ======

        /// <summary>
        /// 세트 피스 증가
        /// </summary>
        private void IncrementSetPiece(string setId)
        {
            if (!equippedSetPieces.ContainsKey(setId))
            {
                equippedSetPieces[setId] = 0;
            }

            equippedSetPieces[setId]++;
            UpdateSetBonus(setId);
        }

        /// <summary>
        /// 세트 피스 감소
        /// </summary>
        private void DecrementSetPiece(string setId)
        {
            if (!equippedSetPieces.ContainsKey(setId))
                return;

            equippedSetPieces[setId]--;

            if (equippedSetPieces[setId] <= 0)
            {
                equippedSetPieces.Remove(setId);
            }

            UpdateSetBonus(setId);
        }

        /// <summary>
        /// 세트 보너스 업데이트
        /// </summary>
        private void UpdateSetBonus(string setId)
        {
            if (!setDataMap.TryGetValue(setId, out SetItemData setData))
                return;

            int pieceCount = equippedSetPieces.GetValueOrDefault(setId, 0);
            List<SetBonus> newBonuses = setData.GetActiveBonuses(pieceCount);

            // 기존 보너스와 비교
            List<SetBonus> oldBonuses = activeBonuses.GetValueOrDefault(setId, new List<SetBonus>());

            // 보너스 변경 여부 확인
            bool bonusChanged = newBonuses.Count != oldBonuses.Count;

            if (!bonusChanged)
            {
                for (int i = 0; i < newBonuses.Count; i++)
                {
                    if (newBonuses[i].requiredPieces != oldBonuses[i].requiredPieces)
                    {
                        bonusChanged = true;
                        break;
                    }
                }
            }

            // 보너스 업데이트
            if (newBonuses.Count > 0)
            {
                activeBonuses[setId] = newBonuses;
            }
            else
            {
                activeBonuses.Remove(setId);
            }

            // 이벤트 발생
            if (bonusChanged)
            {
                Debug.Log($"[SetItemBonusSystem] {setData.setName}: {pieceCount}피스, {newBonuses.Count}개 보너스 활성화");
                OnSetBonusChanged?.Invoke(setId, pieceCount, newBonuses);
                OnSetStatsChanged?.Invoke();
            }
        }


        // ====== 조회 ======

        /// <summary>
        /// 특정 세트의 장착된 피스 수
        /// </summary>
        /// <param name="setId">세트 ID</param>
        /// <returns>장착된 피스 수</returns>
        public int GetEquippedPieceCount(string setId)
        {
            return equippedSetPieces.GetValueOrDefault(setId, 0);
        }

        /// <summary>
        /// 특정 세트의 활성화된 보너스
        /// </summary>
        /// <param name="setId">세트 ID</param>
        /// <returns>활성화된 보너스 목록</returns>
        public List<SetBonus> GetActiveBonuses(string setId)
        {
            if (activeBonuses.TryGetValue(setId, out List<SetBonus> bonuses))
            {
                return new List<SetBonus>(bonuses);
            }
            return new List<SetBonus>();
        }

        /// <summary>
        /// 모든 활성화된 세트 보너스
        /// </summary>
        /// <returns>세트ID → 보너스 목록 딕셔너리</returns>
        public Dictionary<string, List<SetBonus>> GetAllActiveBonuses()
        {
            var result = new Dictionary<string, List<SetBonus>>();

            foreach (var kvp in activeBonuses)
            {
                result[kvp.Key] = new List<SetBonus>(kvp.Value);
            }

            return result;
        }

        /// <summary>
        /// 세트 보너스로 인한 총 스탯 계산
        /// </summary>
        /// <returns>스탯별 보너스 딕셔너리</returns>
        public Dictionary<StatType, float> GetTotalSetBonusStats()
        {
            var totalStats = new Dictionary<StatType, float>();

            foreach (var bonusList in activeBonuses.Values)
            {
                foreach (var bonus in bonusList)
                {
                    foreach (var stat in bonus.statBonuses)
                    {
                        if (!totalStats.ContainsKey(stat.statType))
                        {
                            totalStats[stat.statType] = 0f;
                        }

                        // Flat은 직접 더하기, Percent는 별도 처리 필요
                        if (stat.modifierType == ModifierType.Flat)
                        {
                            totalStats[stat.statType] += stat.value;
                        }
                        else if (stat.modifierType == ModifierType.Percent)
                        {
                            // Percent는 별도 키로 저장하거나 PlayerStats에서 처리
                            totalStats[stat.statType] += stat.value;
                        }
                    }
                }
            }

            return totalStats;
        }

        /// <summary>
        /// 특정 스탯에 대한 세트 보너스
        /// </summary>
        /// <param name="statType">스탯 종류</param>
        /// <returns>보너스 수치</returns>
        public float GetSetBonus(StatType statType)
        {
            float total = 0f;

            foreach (var bonusList in activeBonuses.Values)
            {
                foreach (var bonus in bonusList)
                {
                    foreach (var stat in bonus.statBonuses)
                    {
                        if (stat.statType == statType)
                        {
                            total += stat.value;
                        }
                    }
                }
            }

            return total;
        }

        /// <summary>
        /// 세트 데이터 가져오기
        /// </summary>
        /// <param name="setId">세트 ID</param>
        /// <returns>세트 데이터 (없으면 null)</returns>
        public SetItemData GetSetData(string setId)
        {
            setDataMap.TryGetValue(setId, out SetItemData data);
            return data;
        }


        // ====== 현재 장비 기반 재계산 ======

        /// <summary>
        /// 현재 장비 기반으로 세트 보너스 재계산
        /// EquipmentManager에서 직접 조회하여 상태 갱신
        /// </summary>
        public void RecalculateFromEquipment()
        {
            equippedSetPieces.Clear();
            activeBonuses.Clear();

            if (!EquipmentManager.HasInstance)
                return;

            var equipped = EquipmentManager.Instance.GetAllEquipped();

            foreach (var item in equipped.Values)
            {
                if (item?.cachedItemData == null)
                    continue;

                string itemId = item.cachedItemData.itemId;

                foreach (var setData in setDataMap.Values)
                {
                    if (setData.ContainsItem(itemId))
                    {
                        if (!equippedSetPieces.ContainsKey(setData.setId))
                        {
                            equippedSetPieces[setData.setId] = 0;
                        }
                        equippedSetPieces[setData.setId]++;
                    }
                }
            }

            // 모든 세트 보너스 업데이트
            foreach (var setId in equippedSetPieces.Keys)
            {
                if (setDataMap.TryGetValue(setId, out SetItemData setData))
                {
                    int pieceCount = equippedSetPieces[setId];
                    List<SetBonus> bonuses = setData.GetActiveBonuses(pieceCount);

                    if (bonuses.Count > 0)
                    {
                        activeBonuses[setId] = bonuses;
                    }
                }
            }

            OnSetStatsChanged?.Invoke();
            Debug.Log($"[SetItemBonusSystem] 재계산 완료: {equippedSetPieces.Count}개 세트, {activeBonuses.Count}개 활성 보너스");
        }


        // ====== 디버그 ======

        /// <summary>
        /// 세트 보너스 상태 출력
        /// </summary>
        [ContextMenu("Print Set Bonuses")]
        public void DebugPrintSetBonuses()
        {
            Debug.Log("[SetItemBonusSystem] ========== 세트 보너스 ==========");

            foreach (var kvp in equippedSetPieces)
            {
                string setId = kvp.Key;
                int pieceCount = kvp.Value;

                if (setDataMap.TryGetValue(setId, out SetItemData setData))
                {
                    Debug.Log($"[SetItemBonusSystem] {setData.setName}: {pieceCount}/{setData.PieceCount}피스");

                    if (activeBonuses.TryGetValue(setId, out List<SetBonus> bonuses))
                    {
                        foreach (var bonus in bonuses)
                        {
                            Debug.Log($"[SetItemBonusSystem]   [{bonus.requiredPieces}세트] {bonus.bonusDescription}");
                        }
                    }
                }
            }

            Debug.Log("[SetItemBonusSystem] =====================================");
        }

        /// <summary>
        /// 세트 스탯 출력
        /// </summary>
        [ContextMenu("Print Set Stats")]
        public void DebugPrintSetStats()
        {
            Debug.Log("[SetItemBonusSystem] ========== 세트 스탯 보너스 ==========");

            var stats = GetTotalSetBonusStats();

            foreach (var kvp in stats)
            {
                if (kvp.Value != 0)
                {
                    Debug.Log($"[SetItemBonusSystem] {kvp.Key}: +{kvp.Value}");
                }
            }

            Debug.Log("[SetItemBonusSystem] =====================================");
        }
    }
}
