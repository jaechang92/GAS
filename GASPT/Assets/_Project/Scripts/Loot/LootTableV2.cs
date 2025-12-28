using System;
using System.Collections.Generic;
using UnityEngine;
using GASPT.Data;

namespace GASPT.Loot
{
    /// <summary>
    /// 드롭 테이블 V2 (ItemData 기반)
    /// </summary>
    [CreateAssetMenu(fileName = "LootTableV2", menuName = "GASPT/Loot/LootTableV2")]
    public class LootTableV2 : ScriptableObject
    {
        // ====== 드롭 항목 ======

        [Header("드롭 항목")]
        [Tooltip("드롭 가능한 아이템 목록")]
        public List<LootEntryV2> entries = new List<LootEntryV2>();


        // ====== 드롭 설정 ======

        [Header("드롭 설정")]
        [Tooltip("최소 드롭 수")]
        [Min(0)]
        public int minDrops = 1;

        [Tooltip("최대 드롭 수")]
        [Min(1)]
        public int maxDrops = 3;

        [Tooltip("확정 드롭 항목 (항상 드롭)")]
        public List<LootEntryV2> guaranteedDrops = new List<LootEntryV2>();

        [Tooltip("디버그 로그 출력")]
        public bool debugLog = false;


        // ====== 드롭 메서드 ======

        /// <summary>
        /// 드롭 롤링
        /// </summary>
        /// <returns>드롭 결과 목록</returns>
        public List<LootDropResult> Roll()
        {
            List<LootDropResult> results = new List<LootDropResult>();

            // 확정 드롭 추가
            foreach (var entry in guaranteedDrops)
            {
                if (entry != null && entry.itemData != null)
                {
                    int qty = UnityEngine.Random.Range(entry.minQuantity, entry.maxQuantity + 1);
                    results.Add(new LootDropResult(entry.itemData, qty));

                    if (debugLog)
                    {
                        Debug.Log($"[LootTableV2] 확정 드롭: {entry.itemData.itemName} x{qty}");
                    }
                }
            }

            // 일반 드롭 수 결정
            int dropCount = UnityEngine.Random.Range(minDrops, maxDrops + 1);

            // 일반 드롭 롤링
            for (int i = 0; i < dropCount; i++)
            {
                LootEntryV2 selectedEntry = RollSingleDrop();

                if (selectedEntry != null && selectedEntry.itemData != null)
                {
                    int qty = UnityEngine.Random.Range(selectedEntry.minQuantity, selectedEntry.maxQuantity + 1);
                    results.Add(new LootDropResult(selectedEntry.itemData, qty));

                    if (debugLog)
                    {
                        Debug.Log($"[LootTableV2] 드롭: {selectedEntry.itemData.itemName} x{qty}");
                    }
                }
            }

            return results;
        }

        /// <summary>
        /// 단일 드롭 롤링
        /// </summary>
        private LootEntryV2 RollSingleDrop()
        {
            if (entries == null || entries.Count == 0)
                return null;

            float totalWeight = 0f;

            foreach (var entry in entries)
            {
                if (entry != null && entry.itemData != null)
                {
                    totalWeight += entry.weight;
                }
            }

            if (totalWeight <= 0f)
                return null;

            float random = UnityEngine.Random.Range(0f, totalWeight);
            float cumulativeWeight = 0f;

            foreach (var entry in entries)
            {
                if (entry == null || entry.itemData == null)
                    continue;

                cumulativeWeight += entry.weight;

                if (random <= cumulativeWeight)
                {
                    return entry;
                }
            }

            return null;
        }

        /// <summary>
        /// 단일 아이템 롤링 (구 방식 호환)
        /// </summary>
        public ItemData GetRandomDrop()
        {
            LootEntryV2 entry = RollSingleDrop();
            return entry?.itemData;
        }


        // ====== 검증 ======

        /// <summary>
        /// 테이블 유효성 검증
        /// </summary>
        public bool Validate()
        {
            if (entries == null || entries.Count == 0)
            {
                Debug.LogWarning($"[LootTableV2] {name}: entries가 비어있습니다.");
                return false;
            }

            int validCount = 0;
            float totalWeight = 0f;

            foreach (var entry in entries)
            {
                if (entry != null && entry.itemData != null)
                {
                    validCount++;
                    totalWeight += entry.weight;
                }
            }

            if (validCount == 0)
            {
                Debug.LogError($"[LootTableV2] {name}: 유효한 항목이 없습니다.");
                return false;
            }

            Debug.Log($"[LootTableV2] {name}: {validCount}개 항목, 총 가중치 {totalWeight}");
            return true;
        }


        // ====== Unity 콜백 ======

        private void OnValidate()
        {
            // minDrops/maxDrops 보정
            if (minDrops < 0) minDrops = 0;
            if (maxDrops < minDrops) maxDrops = minDrops;

            // 엔트리 보정
            foreach (var entry in entries)
            {
                entry?.Validate();
            }

            foreach (var entry in guaranteedDrops)
            {
                entry?.Validate();
            }
        }


        // ====== 디버그 ======

        [ContextMenu("Print Table Info")]
        public void PrintInfo()
        {
            Debug.Log($"========== LootTableV2: {name} ==========");
            Debug.Log($"Drops: {minDrops}-{maxDrops}");
            Debug.Log($"Entries: {entries.Count}");
            Debug.Log($"Guaranteed: {guaranteedDrops.Count}");

            float totalWeight = 0f;
            foreach (var entry in entries)
            {
                if (entry?.itemData != null)
                {
                    float chance = (entry.weight / GetTotalWeight()) * 100f;
                    Debug.Log($"  - {entry.itemData.itemName}: 가중치 {entry.weight} ({chance:F1}%)");
                    totalWeight += entry.weight;
                }
            }

            Debug.Log($"Total Weight: {totalWeight}");
            Debug.Log("=============================================");
        }

        private float GetTotalWeight()
        {
            float total = 0f;
            foreach (var entry in entries)
            {
                if (entry?.itemData != null)
                {
                    total += entry.weight;
                }
            }
            return total > 0f ? total : 1f;
        }

        [ContextMenu("Test: Roll 10 Times")]
        public void TestRoll10Times()
        {
            debugLog = true;

            Debug.Log($"========== 10회 롤링 테스트: {name} ==========");

            for (int i = 0; i < 10; i++)
            {
                Debug.Log($"--- 롤 #{i + 1} ---");
                List<LootDropResult> results = Roll();

                foreach (var result in results)
                {
                    Debug.Log($"  {result.itemData.itemName} x{result.quantity}");
                }

                if (results.Count == 0)
                {
                    Debug.Log("  (드롭 없음)");
                }
            }

            debugLog = false;
            Debug.Log("==============================================");
        }
    }


    /// <summary>
    /// 드롭 엔트리 V2
    /// </summary>
    [Serializable]
    public class LootEntryV2
    {
        [Tooltip("드롭할 아이템 데이터")]
        public ItemData itemData;

        [Tooltip("가중치 (높을수록 잘 나옴)")]
        [Min(0.01f)]
        public float weight = 1f;

        [Tooltip("최소 수량")]
        [Min(1)]
        public int minQuantity = 1;

        [Tooltip("최대 수량")]
        [Min(1)]
        public int maxQuantity = 1;


        /// <summary>
        /// 유효성 검증 및 보정
        /// </summary>
        public void Validate()
        {
            if (weight < 0.01f) weight = 0.01f;
            if (minQuantity < 1) minQuantity = 1;
            if (maxQuantity < minQuantity) maxQuantity = minQuantity;
        }
    }
}
