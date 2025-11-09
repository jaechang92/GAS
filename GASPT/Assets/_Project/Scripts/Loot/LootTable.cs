using System.Collections.Generic;
using UnityEngine;
using GASPT.Data;

namespace GASPT.Loot
{
    /// <summary>
    /// 드롭 테이블 ScriptableObject
    /// 확률 기반 아이템 드롭 정의
    /// </summary>
    [CreateAssetMenu(fileName = "LootTable", menuName = "GASPT/Loot/LootTable")]
    public class LootTable : ScriptableObject
    {
        // ====== 드롭 항목 ======

        [Header("드롭 항목")]
        [Tooltip("드롭 가능한 아이템 목록")]
        public List<LootEntry> lootEntries = new List<LootEntry>();


        // ====== 드롭 설정 ======

        [Header("드롭 설정")]
        [Tooltip("드롭 없음 허용 여부 (true: 확률 합이 100% 미만일 때 드롭 없을 수 있음)")]
        public bool allowNoDrop = true;

        [Tooltip("디버그 로그 출력 여부")]
        public bool debugLog = false;


        // ====== 드롭 메서드 ======

        /// <summary>
        /// 확률 기반 랜덤 드롭 아이템 선택
        /// </summary>
        /// <returns>선택된 아이템 (드롭 없음 시 null)</returns>
        public Item GetRandomDrop()
        {
            if (lootEntries == null || lootEntries.Count == 0)
            {
                if (debugLog)
                    Debug.LogWarning($"[LootTable] {name}: lootEntries가 비어있습니다.");
                return null;
            }

            // 확률 계산 (0.0 ~ 1.0)
            float random = Random.value;
            float cumulativeChance = 0f;

            if (debugLog)
                Debug.Log($"[LootTable] {name}: 랜덤 값 = {random:F3}");

            foreach (var entry in lootEntries)
            {
                if (entry == null || entry.item == null)
                    continue;

                cumulativeChance += entry.dropChance;

                if (random <= cumulativeChance)
                {
                    if (debugLog)
                        Debug.Log($"[LootTable] {name}: {entry.item.itemName} 드롭! (누적 확률: {cumulativeChance:F3})");
                    return entry.item;
                }
            }

            // 드롭 없음
            if (debugLog)
                Debug.Log($"[LootTable] {name}: 드롭 없음 (누적 확률: {cumulativeChance:F3})");

            return null;
        }


        // ====== 검증 ======

        /// <summary>
        /// 드롭 테이블 유효성 검증
        /// </summary>
        public bool ValidateTable()
        {
            if (lootEntries == null || lootEntries.Count == 0)
            {
                Debug.LogWarning($"[LootTable] {name}: lootEntries가 비어있습니다.");
                return false;
            }

            float totalChance = 0f;
            int validEntries = 0;

            for (int i = 0; i < lootEntries.Count; i++)
            {
                var entry = lootEntries[i];

                if (entry == null)
                {
                    Debug.LogWarning($"[LootTable] {name}: Entry #{i}가 null입니다.");
                    continue;
                }

                if (!entry.Validate())
                {
                    Debug.LogWarning($"[LootTable] {name}: Entry #{i} 검증 실패");
                    continue;
                }

                totalChance += entry.dropChance;
                validEntries++;
            }

            if (validEntries == 0)
            {
                Debug.LogError($"[LootTable] {name}: 유효한 항목이 없습니다.");
                return false;
            }

            // 확률 합계 검증
            if (totalChance > 1f)
            {
                Debug.LogWarning($"[LootTable] {name}: 확률 합계가 100%를 초과합니다: {totalChance * 100f:F1}%");
            }
            else if (totalChance < 1f && !allowNoDrop)
            {
                Debug.LogWarning($"[LootTable] {name}: 확률 합계가 100% 미만입니다: {totalChance * 100f:F1}% (allowNoDrop=false)");
            }

            return true;
        }


        // ====== Unity 콜백 ======

        private void OnValidate()
        {
            // Inspector에서 값 변경 시 자동 보정
            FixLootEntries();

            // 자동 검증
            ValidateTable();
        }

        /// <summary>
        /// LootEntry 수량 자동 보정
        /// </summary>
        private void FixLootEntries()
        {
            if (lootEntries == null)
                return;

            foreach (var entry in lootEntries)
            {
                if (entry == null)
                    continue;

                // minQuantity가 0이면 1로 보정
                if (entry.minQuantity < 1)
                {
                    entry.minQuantity = 1;
                }

                // maxQuantity가 0이면 1로 보정
                if (entry.maxQuantity < 1)
                {
                    entry.maxQuantity = 1;
                }

                // maxQuantity가 minQuantity보다 작으면 보정
                if (entry.maxQuantity < entry.minQuantity)
                {
                    entry.maxQuantity = entry.minQuantity;
                }
            }
        }


        // ====== 디버그 ======

        /// <summary>
        /// 드롭 테이블 정보 출력
        /// </summary>
        [ContextMenu("Print Loot Table Info")]
        public void PrintInfo()
        {
            Debug.Log($"========== Loot Table: {name} ==========");
            Debug.Log($"Total Entries: {lootEntries.Count}");

            float totalChance = 0f;

            for (int i = 0; i < lootEntries.Count; i++)
            {
                var entry = lootEntries[i];
                if (entry != null && entry.item != null)
                {
                    Debug.Log($"  {i}: {entry.ToString()}");
                    totalChance += entry.dropChance;
                }
            }

            Debug.Log($"Total Drop Chance: {totalChance * 100f:F1}%");
            Debug.Log($"No Drop Chance: {(1f - totalChance) * 100f:F1}%");
            Debug.Log("=========================================");
        }

        /// <summary>
        /// 테스트: 드롭 시뮬레이션 (N회)
        /// </summary>
        [ContextMenu("Test: Simulate 100 Drops")]
        public void TestSimulate100Drops()
        {
            Dictionary<string, int> dropCounts = new Dictionary<string, int>();
            int noDrop = 0;

            for (int i = 0; i < 100; i++)
            {
                Item dropped = GetRandomDrop();
                if (dropped != null)
                {
                    if (!dropCounts.ContainsKey(dropped.itemName))
                        dropCounts[dropped.itemName] = 0;
                    dropCounts[dropped.itemName]++;
                }
                else
                {
                    noDrop++;
                }
            }

            Debug.Log($"========== 100회 드롭 시뮬레이션: {name} ==========");
            foreach (var kvp in dropCounts)
            {
                Debug.Log($"  {kvp.Key}: {kvp.Value}회 ({kvp.Value}%)");
            }
            Debug.Log($"  드롭 없음: {noDrop}회 ({noDrop}%)");
            Debug.Log("=========================================");
        }
    }
}
