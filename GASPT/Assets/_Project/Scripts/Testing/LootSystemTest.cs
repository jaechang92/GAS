using UnityEngine;
using GASPT.Loot;
using GASPT.Data;

namespace GASPT.Testing
{
    /// <summary>
    /// LootSystem 테스트 스크립트
    /// Context Menu를 통한 드롭 및 획득 테스트
    /// </summary>
    public class LootSystemTest : MonoBehaviour
    {
        [Header("Test Settings")]
        [SerializeField] private LootTable testLootTable;
        [SerializeField] private Item testItem;
        [SerializeField] private Vector3 dropPosition = Vector3.zero;

        [Header("Test Results")]
        [SerializeField] private int totalDrops = 0;
        [SerializeField] private int successfulDrops = 0;


        // ====== 기본 테스트 ======

        [ContextMenu("Test01: 시스템 초기화 확인")]
        private void Test01_CheckSystemInitialization()
        {
            Debug.Log("========== Test01: 시스템 초기화 확인 ==========");

            if (LootSystem.HasInstance)
            {
                Debug.Log("✓ LootSystem 초기화 완료");
            }
            else
            {
                Debug.LogError("✗ LootSystem 미초기화");
            }

            Debug.Log("===========================================");
        }


        // ====== 단일 아이템 드롭 테스트 ======

        [ContextMenu("Test02: 단일 아이템 100% 드롭")]
        private void Test02_DropSingleItem100Percent()
        {
            Debug.Log("========== Test02: 단일 아이템 100% 드롭 ==========");

            if (testItem == null)
            {
                Debug.LogError("testItem이 null입니다! Inspector에서 설정하세요.");
                return;
            }

            if (!LootSystem.HasInstance)
            {
                Debug.LogError("LootSystem이 초기화되지 않았습니다.");
                return;
            }

            // 100% 확률로 아이템 드롭
            LootSystem.Instance.DropItem(testItem, dropPosition);

            Debug.Log($"✓ {testItem.itemName} 드롭 완료 (위치: {dropPosition})");
            Debug.Log("===========================================");
        }


        // ====== LootTable 테스트 ======

        [ContextMenu("Test03: LootTable 확률 드롭")]
        private void Test03_DropFromLootTable()
        {
            Debug.Log("========== Test03: LootTable 확률 드롭 ==========");

            if (testLootTable == null)
            {
                Debug.LogError("testLootTable이 null입니다! Inspector에서 설정하세요.");
                return;
            }

            if (!LootSystem.HasInstance)
            {
                Debug.LogError("LootSystem이 초기화되지 않았습니다.");
                return;
            }

            // LootTable에서 드롭
            LootSystem.Instance.DropLoot(testLootTable, dropPosition);

            Debug.Log($"✓ LootTable 드롭 시도 완료 (위치: {dropPosition})");
            Debug.Log("===========================================");
        }


        // ====== 연속 드롭 테스트 ======

        [ContextMenu("Test04: 10회 연속 드롭 (확률 검증)")]
        private void Test04_Drop10Times()
        {
            Debug.Log("========== Test04: 10회 연속 드롭 ==========");

            if (testLootTable == null)
            {
                Debug.LogError("testLootTable이 null입니다! Inspector에서 설정하세요.");
                return;
            }

            if (!LootSystem.HasInstance)
            {
                Debug.LogError("LootSystem이 초기화되지 않았습니다.");
                return;
            }

            totalDrops = 0;
            successfulDrops = 0;

            // 이벤트 구독
            LootSystem.Instance.OnItemDropped += OnTestItemDropped;

            // 10회 드롭
            for (int i = 0; i < 10; i++)
            {
                Vector3 pos = dropPosition + new Vector3(i * 2f, 0f, 0f);
                LootSystem.Instance.DropLoot(testLootTable, pos);
                totalDrops++;
            }

            // 이벤트 구독 해제
            LootSystem.Instance.OnItemDropped -= OnTestItemDropped;

            Debug.Log($"✓ 10회 드롭 완료 - 성공: {successfulDrops}/10");
            Debug.Log("===========================================");
        }

        private void OnTestItemDropped(Item item, Vector3 position)
        {
            successfulDrops++;
            Debug.Log($"  드롭 #{successfulDrops}: {item.itemName} at {position}");
        }


        // ====== LootTable 검증 테스트 ======

        [ContextMenu("Test05: LootTable 검증")]
        private void Test05_ValidateLootTable()
        {
            Debug.Log("========== Test05: LootTable 검증 ==========");

            if (testLootTable == null)
            {
                Debug.LogError("testLootTable이 null입니다!");
                return;
            }

            bool isValid = testLootTable.ValidateTable();

            if (isValid)
            {
                Debug.Log("✓ LootTable 검증 성공");
                testLootTable.PrintInfo();
            }
            else
            {
                Debug.LogError("✗ LootTable 검증 실패");
            }

            Debug.Log("===========================================");
        }


        // ====== DroppedItem 테스트 ======

        [ContextMenu("Test06: DroppedItem 생명주기 테스트")]
        private void Test06_DroppedItemLifecycle()
        {
            Debug.Log("========== Test06: DroppedItem 생명주기 ==========");

            if (testItem == null)
            {
                Debug.LogError("testItem이 null입니다!");
                return;
            }

            if (!LootSystem.HasInstance)
            {
                Debug.LogError("LootSystem이 초기화되지 않았습니다.");
                return;
            }

            // 아이템 드롭
            LootSystem.Instance.DropItem(testItem, dropPosition);

            Debug.Log($"✓ {testItem.itemName} 드롭 완료");
            Debug.Log("  - 30초 후 자동 소멸됩니다.");
            Debug.Log("  - Player와 충돌 시 자동 획득됩니다.");
            Debug.Log("===========================================");
        }


        // ====== Context Menu ======

        [ContextMenu("Print Test Settings")]
        private void PrintTestSettings()
        {
            Debug.Log("========== 테스트 설정 ==========");
            Debug.Log($"LootTable: {(testLootTable != null ? testLootTable.name : "NULL")}");
            Debug.Log($"Test Item: {(testItem != null ? testItem.itemName : "NULL")}");
            Debug.Log($"Drop Position: {dropPosition}");
            Debug.Log("==============================");
        }
    }
}
