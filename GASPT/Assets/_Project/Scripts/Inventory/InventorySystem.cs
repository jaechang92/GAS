using System;
using System.Collections.Generic;
using UnityEngine;
using GASPT.Data;
using GASPT.Stats;
using GASPT.Save;
using Core.Enums;
using GASPT.Core;

namespace GASPT.Inventory
{
    /// <summary>
    /// 인벤토리 시스템 싱글톤
    /// 아이템 저장 및 PlayerStats와 통합하여 장비 관리
    /// ISaveable 인터페이스 구현으로 SaveManager 지원
    /// </summary>
    public class InventorySystem : SingletonManager<InventorySystem>, ISaveable
    {
        // ====== 인벤토리 ======

        /// <summary>
        /// 보유한 아이템 목록
        /// </summary>
        private List<Item> items = new List<Item>();


        // ====== PlayerStats 참조 제거 ======
        // InventorySystem은 아이템 소유권만 관리
        // 장비 장착은 PlayerStats 또는 UI/Presenter가 직접 처리


        // ====== 이벤트 ======

        /// <summary>
        /// 아이템 추가 시 발생하는 이벤트
        /// 매개변수: (추가된 아이템)
        /// </summary>
        public event Action<Item> OnItemAdded;

        /// <summary>
        /// 아이템 제거 시 발생하는 이벤트
        /// 매개변수: (제거된 아이템)
        /// </summary>
        public event Action<Item> OnItemRemoved;


        // ====== Unity 생명주기 ======

        protected override void OnAwake()
        {
            Debug.Log($"[InventorySystem] 초기화 완료 (순수 아이템 관리자)");
        }


        // ====== 아이템 관리 ======

        /// <summary>
        /// 아이템 추가
        /// </summary>
        /// <param name="item">추가할 아이템</param>
        public void AddItem(Item item)
        {
            if (item == null)
            {
                Debug.LogWarning("[InventorySystem] AddItem(): item이 null입니다.");
                return;
            }

            items.Add(item);

            // 이벤트 발생
            OnItemAdded?.Invoke(item);

            Debug.Log($"[InventorySystem] 아이템 추가: {item.itemName} (총 {items.Count}개)");
        }

        /// <summary>
        /// 아이템 제거
        /// </summary>
        /// <param name="item">제거할 아이템</param>
        /// <returns>true: 제거 성공, false: 아이템 없음</returns>
        public bool RemoveItem(Item item)
        {
            if (item == null)
            {
                Debug.LogWarning("[InventorySystem] RemoveItem(): item이 null입니다.");
                return false;
            }

            bool removed = items.Remove(item);

            if (removed)
            {
                // 이벤트 발생
                OnItemRemoved?.Invoke(item);

                Debug.Log($"[InventorySystem] 아이템 제거: {item.itemName} (남은 개수: {items.Count})");
            }
            else
            {
                Debug.LogWarning($"[InventorySystem] 아이템 제거 실패: {item.itemName}을(를) 찾을 수 없습니다.");
            }

            return removed;
        }

        /// <summary>
        /// 아이템 보유 여부 확인
        /// </summary>
        /// <param name="item">확인할 아이템</param>
        /// <returns>true: 보유 중, false: 없음</returns>
        public bool HasItem(Item item)
        {
            return items.Contains(item);
        }

        /// <summary>
        /// 모든 아이템 목록 가져오기 (읽기 전용)
        /// </summary>
        /// <returns>아이템 목록 복사본</returns>
        public List<Item> GetItems()
        {
            return new List<Item>(items);
        }

        /// <summary>
        /// 인벤토리 아이템 개수
        /// </summary>
        public int ItemCount => items.Count;


        // ====== 장비 시스템 제거 ======
        //
        // InventorySystem은 아이템 소유권만 관리합니다.
        // 장비 장착/해제는 PlayerStats가 직접 담당합니다.
        //
        // 사용 예시 (UI/Presenter에서):
        //   Item item = InventorySystem.Instance.GetItems()[0];
        //   if (InventorySystem.Instance.HasItem(item))
        //   {
        //       PlayerStats.EquipItem(item);
        //   }


        // ====== 디버그 ======

        /// <summary>
        /// 인벤토리 정보 출력
        /// </summary>
        [ContextMenu("Print Inventory")]
        public void DebugPrintInventory()
        {
            Debug.Log($"[InventorySystem] ========== 인벤토리 ({items.Count}개) ==========");

            for (int i = 0; i < items.Count; i++)
            {
                Item item = items[i];
                Debug.Log($"[InventorySystem] {i + 1}. {item.itemName} ({item.slot})");
            }

            Debug.Log("[InventorySystem] =====================================");
        }

        /// <summary>
        /// 모든 아이템 제거 (테스트용)
        /// </summary>
        [ContextMenu("Clear Inventory (Test)")]
        private void DebugClearInventory()
        {
            items.Clear();
            Debug.Log("[InventorySystem] 인벤토리 비우기 완료");
        }


        // ====== ISaveable 인터페이스 구현 ======

        /// <summary>
        /// ISaveable 인터페이스: 저장 가능 객체 고유 ID
        /// </summary>
        public string SaveID => "InventorySystem";

        /// <summary>
        /// ISaveable.GetSaveData() 명시적 구현
        /// 내부적으로 구체적 타입의 GetSaveData()를 호출합니다
        /// </summary>
        object ISaveable.GetSaveData()
        {
            return GetSaveData();
        }

        /// <summary>
        /// ISaveable.LoadFromSaveData(object) 명시적 구현
        /// 타입 검증 후 구체적 타입의 LoadFromSaveData()를 호출합니다
        /// </summary>
        void ISaveable.LoadFromSaveData(object data)
        {
            if (data is InventoryData inventoryData)
            {
                LoadFromSaveData(inventoryData);
            }
            else
            {
                Debug.LogError($"[InventorySystem] ISaveable.LoadFromSaveData(): 잘못된 데이터 타입입니다. Expected: InventoryData, Got: {data?.GetType().Name}");
            }
        }


        // ====== Save/Load (기존 방식) ======

        /// <summary>
        /// 현재 인벤토리 데이터를 저장용 구조로 반환합니다
        /// </summary>
        public InventoryData GetSaveData()
        {
            InventoryData data = new InventoryData();
            data.itemPaths = new List<string>();

            foreach (Item item in items)
            {
                if (item != null)
                {
                    // ScriptableObject의 에셋 경로 저장
#if UNITY_EDITOR
                    string assetPath = UnityEditor.AssetDatabase.GetAssetPath(item);
                    data.itemPaths.Add(assetPath);
#else
                    Debug.LogWarning($"[InventorySystem] GetSaveData(): 빌드 환경에서는 아이템 저장이 지원되지 않습니다. 아이템: {item.itemName}");
#endif
                }
            }

            Debug.Log($"[InventorySystem] GetSaveData(): Items={data.itemPaths.Count}");

            return data;
        }

        /// <summary>
        /// 저장된 데이터로부터 인벤토리를 복원합니다
        /// </summary>
        public void LoadFromSaveData(InventoryData data)
        {
            if (data == null)
            {
                Debug.LogError("[InventorySystem] LoadFromSaveData(): data가 null입니다.");
                return;
            }

            // 인벤토리 비우기
            items.Clear();

            // 아이템 복원
            if (data.itemPaths != null)
            {
                foreach (string itemPath in data.itemPaths)
                {
                    if (string.IsNullOrEmpty(itemPath))
                    {
                        Debug.LogWarning("[InventorySystem] LoadFromSaveData(): 빈 아이템 경로입니다.");
                        continue;
                    }

#if UNITY_EDITOR
                    // 에셋 경로로부터 아이템 로드
                    Item item = UnityEditor.AssetDatabase.LoadAssetAtPath<Item>(itemPath);

                    if (item != null)
                    {
                        items.Add(item);
                        Debug.Log($"[InventorySystem] LoadFromSaveData(): 아이템 복원 - {item.itemName}");
                    }
                    else
                    {
                        Debug.LogWarning($"[InventorySystem] LoadFromSaveData(): 아이템을 찾을 수 없습니다. 경로: {itemPath}");
                    }
#else
                    Debug.LogWarning($"[InventorySystem] LoadFromSaveData(): 빌드 환경에서는 아이템 불러오기가 지원되지 않습니다. 경로: {itemPath}");
#endif
                }
            }

            Debug.Log($"[InventorySystem] LoadFromSaveData() 완료: 아이템 {items.Count}개 복원");
        }
    }
}
