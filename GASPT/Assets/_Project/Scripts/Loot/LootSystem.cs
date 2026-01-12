using System;
using UnityEngine;
using GASPT.Data;
using GASPT.Inventory;
using GASPT.ResourceManagement;

namespace GASPT.Loot
{
    /// <summary>
    /// 루트 시스템 싱글톤 (V1 - 레거시)
    /// 아이템 드롭 및 월드 아이템 생성 관리
    /// </summary>
    /// <remarks>
    /// ⚠️ 레거시 코드입니다. ItemDropManager를 사용하세요.
    /// V2(ItemDropManager)는 ItemInstance 기반 드롭을 지원합니다.
    /// </remarks>
    [System.Obsolete("Use ItemDropManager instead. V2 supports ItemInstance-based drops.")]
    public class LootSystem : SingletonManager<LootSystem>
    {
        // ====== 설정 ======

        [Header("드롭 설정")]
        [Tooltip("드롭된 아이템이 생성될 높이 오프셋 (Y축)")]
        [SerializeField] private float dropHeightOffset = 0.5f;

        [Tooltip("드롭 시 랜덤 분산 범위 (반지름)")]
        [SerializeField] private float dropScatterRadius = 1f;

        [Tooltip("디버그 로그 출력 여부")]
        [SerializeField] private bool debugLog = true;


        // ====== 프리팹 참조 ======

        private GameObject droppedItemPrefab;


        // ====== 이벤트 ======

        /// <summary>
        /// 아이템 드롭 시 발생하는 이벤트
        /// 매개변수: (Item, Vector3 position)
        /// </summary>
        public event Action<Item, Vector3> OnItemDropped;

        /// <summary>
        /// 아이템 획득 시 발생하는 이벤트
        /// 매개변수: (Item)
        /// </summary>
        public event Action<Item> OnItemPickedUp;


        // ====== 초기화 ======

        protected override void OnAwake()
        {
            LoadDroppedItemPrefab();
            Debug.Log("[LootSystem] 초기화 완료");
        }


        // ====== 프리팹 로딩 ======

        /// <summary>
        /// DroppedItem 프리팹 로딩 (GameResourceManager 사용)
        /// </summary>
        private void LoadDroppedItemPrefab()
        {
            if (GameResourceManager.HasInstance)
            {
                droppedItemPrefab = GameResourceManager.Instance.LoadPrefab(ResourcePaths.Prefabs.UI.DroppedItem);

                if (droppedItemPrefab != null)
                {
                    if (debugLog)
                        Debug.Log($"[LootSystem] DroppedItem 프리팹 로딩 완료: {ResourcePaths.Prefabs.UI.DroppedItem}");
                }
                else
                {
                    Debug.LogError($"[LootSystem] DroppedItem 프리팹을 찾을 수 없습니다: {ResourcePaths.Prefabs.UI.DroppedItem}");
                }
            }
            else
            {
                Debug.LogError("[LootSystem] GameResourceManager가 존재하지 않습니다.");
            }
        }


        // ====== 드롭 메서드 ======

        /// <summary>
        /// LootTable에서 아이템 드롭
        /// </summary>
        /// <param name="lootTable">드롭 테이블</param>
        /// <param name="position">드롭 위치</param>
        public void DropLoot(LootTable lootTable, Vector3 position)
        {
            if (lootTable == null)
            {
                Debug.LogWarning("[LootSystem] DropLoot(): lootTable이 null입니다.");
                return;
            }

            // 랜덤 아이템 선택
            Item droppedItem = lootTable.GetRandomDrop();

            if (droppedItem != null)
            {
                DropItem(droppedItem, position);
            }
            else
            {
                if (debugLog)
                    Debug.Log($"[LootSystem] {lootTable.name}: 드롭 없음");
            }
        }

        /// <summary>
        /// 특정 아이템 드롭 (100% 확정)
        /// </summary>
        /// <param name="item">드롭할 아이템</param>
        /// <param name="position">드롭 위치</param>
        public void DropItem(Item item, Vector3 position)
        {
            if (item == null)
            {
                Debug.LogWarning("[LootSystem] DropItem(): item이 null입니다.");
                return;
            }

            // 랜덤 분산 적용
            Vector3 scatteredPosition = GetScatteredPosition(position);

            // DroppedItem 오브젝트 생성
            CreateDroppedItem(item, scatteredPosition);

            // 이벤트 발생
            OnItemDropped?.Invoke(item, scatteredPosition);

            if (debugLog)
                Debug.Log($"[LootSystem] 아이템 드롭: {item.itemName} at {scatteredPosition}");
        }


        // ====== DroppedItem 생성 ======

        /// <summary>
        /// DroppedItem 오브젝트 생성
        /// </summary>
        private void CreateDroppedItem(Item item, Vector3 position)
        {
            if (droppedItemPrefab == null)
            {
                Debug.LogError("[LootSystem] droppedItemPrefab이 null입니다. 프리팹을 로딩할 수 없습니다.");
                return;
            }

            // 프리팹 인스턴스화
            GameObject droppedObj = Instantiate(droppedItemPrefab, position, Quaternion.identity);
            droppedObj.name = $"DroppedItem_{item.itemName}";

            // DroppedItem 컴포넌트 설정
            DroppedItem droppedItemComponent = droppedObj.GetComponent<DroppedItem>();
            if (droppedItemComponent != null)
            {
                droppedItemComponent.Initialize(item);
            }
            else
            {
                Debug.LogError("[LootSystem] DroppedItem 컴포넌트를 찾을 수 없습니다!");
                Destroy(droppedObj);
            }
        }


        // ====== 위치 계산 ======

        /// <summary>
        /// 랜덤 분산 위치 계산
        /// </summary>
        private Vector3 GetScatteredPosition(Vector3 basePosition)
        {
            // 원형 범위 내 랜덤 위치
            Vector2 randomCircle = UnityEngine.Random.insideUnitCircle * dropScatterRadius;

            return new Vector3(
                basePosition.x + randomCircle.x,
                basePosition.y + dropHeightOffset,
                basePosition.z + randomCircle.y
            );
        }


        // ====== 아이템 획득 ======

        /// <summary>
        /// 아이템 획득 처리 (DroppedItem에서 호출)
        /// </summary>
        /// <param name="item">획득한 아이템</param>
        public void PickUpItem(Item item)
        {
            if (item == null)
            {
                Debug.LogWarning("[LootSystem] PickUpItem(): item이 null입니다.");
                return;
            }

            // 인벤토리에 추가
            if (InventorySystem.HasInstance)
            {
                InventorySystem.Instance.AddItem(item);
            }
            else
            {
                Debug.LogError("[LootSystem] InventorySystem이 존재하지 않습니다.");
            }

            // 이벤트 발생
            OnItemPickedUp?.Invoke(item);

            if (debugLog)
                Debug.Log($"[LootSystem] 아이템 획득: {item.itemName}");
        }


        // ====== Context Menu (테스트용) ======

        [ContextMenu("Test: Drop Random Item at Origin")]
        private void TestDropRandomItem()
        {
            // 테스트용: 임의 아이템 드롭 (실제로는 LootTable을 통해 드롭)
            Debug.LogWarning("[LootSystem] TestDropRandomItem(): 테스트용 메서드입니다. LootTable이 필요합니다.");
        }

        [ContextMenu("Print System Info")]
        private void PrintSystemInfo()
        {
            Debug.Log("========== LootSystem Info ==========");
            Debug.Log($"DroppedItem Prefab: {(droppedItemPrefab != null ? "Loaded" : "NULL")}");
            Debug.Log($"Drop Height Offset: {dropHeightOffset}");
            Debug.Log($"Drop Scatter Radius: {dropScatterRadius}");
            Debug.Log($"Debug Log: {debugLog}");
            Debug.Log("====================================");
        }
    }
}
