using System;
using System.Collections.Generic;
using UnityEngine;
using GASPT.Data;
using GASPT.Core;
using GASPT.Core.Enums;
using GASPT.Inventory;

namespace GASPT.Loot
{
    /// <summary>
    /// 아이템 드롭 매니저
    /// ItemData/ItemInstance 기반 드롭 시스템
    /// </summary>
    public class ItemDropManager : SingletonManager<ItemDropManager>
    {
        // ====== 설정 ======

        [Header("드롭 설정")]
        [Tooltip("드롭 아이템 프리팹")]
        [SerializeField] private GameObject droppedItemPrefab;

        [Tooltip("드롭 시 흩뿌리기 반경")]
        [SerializeField] private float dropScatterRadius = 1f;

        [Tooltip("드롭 아이템 최대 유지 시간 (초)")]
        [SerializeField] private float dropLifetime = 300f;


        // ====== 이벤트 ======

        /// <summary>
        /// 아이템 드롭 이벤트
        /// 매개변수: (드롭된 아이템 인스턴스, 위치)
        /// </summary>
        public event Action<ItemInstance, Vector3> OnItemDropped;

        /// <summary>
        /// 아이템 픽업 이벤트
        /// 매개변수: (아이템 인스턴스, 픽업 결과)
        /// </summary>
        public event Action<ItemInstance, PickupResult> OnItemPickedUp;


        // ====== Unity 생명주기 ======

        protected override void OnAwake()
        {
            Debug.Log("[ItemDropManager] 초기화 완료");
        }


        // ====== 드롭 ======

        /// <summary>
        /// ItemData로 아이템 드롭
        /// </summary>
        /// <param name="itemData">드롭할 아이템 데이터</param>
        /// <param name="position">드롭 위치</param>
        /// <param name="quantity">수량</param>
        /// <returns>드롭된 GameObject 목록</returns>
        public List<GameObject> DropItem(ItemData itemData, Vector3 position, int quantity = 1)
        {
            List<GameObject> droppedObjects = new List<GameObject>();

            if (itemData == null)
            {
                Debug.LogWarning("[ItemDropManager] DropItem: itemData가 null입니다.");
                return droppedObjects;
            }

            // 스택 가능 아이템은 하나의 드롭으로
            if (itemData.stackable)
            {
                ItemInstance instance = ItemInstance.CreateFromData(itemData);
                GameObject dropped = CreateDroppedItem(instance, quantity, position);

                if (dropped != null)
                {
                    droppedObjects.Add(dropped);
                    OnItemDropped?.Invoke(instance, position);
                }
            }
            else
            {
                // 스택 불가 아이템은 개별 드롭
                for (int i = 0; i < quantity; i++)
                {
                    ItemInstance instance = ItemInstance.CreateFromData(itemData);
                    Vector3 scatteredPos = GetScatteredPosition(position, i, quantity);
                    GameObject dropped = CreateDroppedItem(instance, 1, scatteredPos);

                    if (dropped != null)
                    {
                        droppedObjects.Add(dropped);
                        OnItemDropped?.Invoke(instance, scatteredPos);
                    }
                }
            }

            Debug.Log($"[ItemDropManager] 드롭: {itemData.itemName} x{quantity}");

            return droppedObjects;
        }

        /// <summary>
        /// ItemInstance로 아이템 드롭
        /// </summary>
        /// <param name="itemInstance">드롭할 아이템 인스턴스</param>
        /// <param name="position">드롭 위치</param>
        /// <param name="quantity">수량</param>
        /// <returns>드롭된 GameObject</returns>
        public GameObject DropItemInstance(ItemInstance itemInstance, Vector3 position, int quantity = 1)
        {
            if (itemInstance == null || !itemInstance.IsValid)
            {
                Debug.LogWarning("[ItemDropManager] DropItemInstance: 유효하지 않은 인스턴스입니다.");
                return null;
            }

            GameObject dropped = CreateDroppedItem(itemInstance, quantity, position);

            if (dropped != null)
            {
                OnItemDropped?.Invoke(itemInstance, position);
            }

            return dropped;
        }

        /// <summary>
        /// 드롭 테이블에서 아이템 드롭
        /// </summary>
        /// <param name="lootTableV2">드롭 테이블</param>
        /// <param name="position">드롭 위치</param>
        /// <returns>드롭된 GameObject 목록</returns>
        public List<GameObject> DropFromTable(LootTableV2 lootTableV2, Vector3 position)
        {
            List<GameObject> droppedObjects = new List<GameObject>();

            if (lootTableV2 == null)
            {
                Debug.LogWarning("[ItemDropManager] DropFromTable: lootTable이 null입니다.");
                return droppedObjects;
            }

            // 드롭 롤링
            List<LootDropResult> drops = lootTableV2.Roll();

            int dropIndex = 0;
            foreach (var drop in drops)
            {
                Vector3 scatteredPos = GetScatteredPosition(position, dropIndex, drops.Count);
                List<GameObject> itemDrops = DropItem(drop.itemData, scatteredPos, drop.quantity);
                droppedObjects.AddRange(itemDrops);
                dropIndex++;
            }

            return droppedObjects;
        }


        // ====== 픽업 ======

        /// <summary>
        /// 드롭된 아이템 픽업
        /// </summary>
        /// <param name="droppedItemV2">드롭된 아이템 컴포넌트</param>
        /// <returns>픽업 결과</returns>
        public PickupResult PickupItem(DroppedItemV2 droppedItemV2)
        {
            if (droppedItemV2 == null)
            {
                return PickupResult.InvalidItem;
            }

            ItemInstance itemInstance = droppedItemV2.ItemInstance;

            if (itemInstance == null || !itemInstance.IsValid)
            {
                return PickupResult.InvalidItem;
            }

            // 인벤토리에 추가
            if (!InventoryManager.HasInstance)
            {
                return PickupResult.InventoryFull;
            }

            bool added = InventoryManager.Instance.AddItemInstance(itemInstance, droppedItemV2.Quantity);

            if (!added)
            {
                OnItemPickedUp?.Invoke(itemInstance, PickupResult.InventoryFull);
                return PickupResult.InventoryFull;
            }

            // 드롭 오브젝트 제거
            droppedItemV2.OnPickedUp();

            OnItemPickedUp?.Invoke(itemInstance, PickupResult.Success);

            Debug.Log($"[ItemDropManager] 픽업: {itemInstance.cachedItemData?.itemName}");

            return PickupResult.Success;
        }


        // ====== 헬퍼 ======

        /// <summary>
        /// 드롭된 아이템 GameObject 생성
        /// </summary>
        private GameObject CreateDroppedItem(ItemInstance itemInstance, int quantity, Vector3 position)
        {
            if (droppedItemPrefab == null)
            {
                Debug.LogWarning("[ItemDropManager] droppedItemPrefab이 설정되지 않았습니다.");
                return null;
            }

            GameObject dropObj = Instantiate(droppedItemPrefab, position, Quaternion.identity);

            DroppedItemV2 droppedItem = dropObj.GetComponent<DroppedItemV2>();

            if (droppedItem == null)
            {
                droppedItem = dropObj.AddComponent<DroppedItemV2>();
            }

            droppedItem.Initialize(itemInstance, quantity, dropLifetime);

            return dropObj;
        }

        /// <summary>
        /// 흩뿌리기 위치 계산
        /// </summary>
        private Vector3 GetScatteredPosition(Vector3 center, int index, int total)
        {
            if (total <= 1)
                return center;

            float angle = (360f / total) * index;
            float radian = angle * Mathf.Deg2Rad;

            float x = Mathf.Cos(radian) * dropScatterRadius;
            float y = Mathf.Sin(radian) * dropScatterRadius;

            return center + new Vector3(x, y, 0f);
        }


        // ====== 디버그 ======

        /// <summary>
        /// 테스트 드롭
        /// </summary>
        [ContextMenu("Test: Drop Random Item")]
        private void TestDropRandomItem()
        {
            Debug.Log("[ItemDropManager] 테스트 드롭 - droppedItemPrefab과 ItemData 필요");
        }
    }


    /// <summary>
    /// 드롭 결과
    /// </summary>
    public struct LootDropResult
    {
        public ItemData itemData;
        public int quantity;

        public LootDropResult(ItemData itemData, int quantity)
        {
            this.itemData = itemData;
            this.quantity = quantity;
        }
    }
}
