using System;
using UnityEngine;
using GASPT.Data;

namespace GASPT.Loot
{
    /// <summary>
    /// 드롭 항목 정의
    /// LootTable의 개별 드롭 항목을 나타냄
    /// </summary>
    [Serializable]
    public class LootEntry
    {
        // ====== 드롭 아이템 ======

        [Tooltip("드롭할 아이템")]
        public Item item;


        // ====== 드롭 확률 ======

        [Tooltip("드롭 확률 (0.0 ~ 1.0, 0% ~ 100%)")]
        [Range(0f, 1f)]
        public float dropChance = 0.5f;


        // ====== 드롭 수량 (추후 확장) ======

        [Tooltip("최소 드롭 수량 (향후 스택 시스템용)")]
        [Range(1, 99)]
        public int minQuantity = 1;

        [Tooltip("최대 드롭 수량 (향후 스택 시스템용)")]
        [Range(1, 99)]
        public int maxQuantity = 1;


        // ====== 생성자 ======

        /// <summary>
        /// 기본 생성자
        /// </summary>
        public LootEntry()
        {
            item = null;
            dropChance = 0.5f;
            minQuantity = 1;
            maxQuantity = 1;
        }

        /// <summary>
        /// 매개변수 생성자
        /// </summary>
        public LootEntry(Item item, float dropChance, int minQuantity = 1, int maxQuantity = 1)
        {
            this.item = item;
            this.dropChance = Mathf.Clamp01(dropChance);
            this.minQuantity = Mathf.Max(1, minQuantity);
            this.maxQuantity = Mathf.Max(minQuantity, maxQuantity);
        }


        // ====== 검증 ======

        /// <summary>
        /// 드롭 항목 유효성 검증
        /// </summary>
        public bool Validate()
        {
            if (item == null)
            {
                Debug.LogWarning("[LootEntry] item이 null입니다.");
                return false;
            }

            if (dropChance < 0f || dropChance > 1f)
            {
                Debug.LogWarning($"[LootEntry] {item.itemName}: dropChance가 범위를 벗어났습니다: {dropChance}");
                return false;
            }

            if (minQuantity < 1 || maxQuantity < minQuantity)
            {
                Debug.LogWarning($"[LootEntry] {item.itemName}: 수량 범위가 올바르지 않습니다 (min: {minQuantity}, max: {maxQuantity})");
                return false;
            }

            return true;
        }


        // ====== 디버그 ======

        /// <summary>
        /// 드롭 항목 정보 출력
        /// </summary>
        public override string ToString()
        {
            string itemName = item != null ? item.itemName : "None";
            return $"[LootEntry] {itemName} - {dropChance * 100f:F1}% (x{minQuantity}~{maxQuantity})";
        }
    }
}
