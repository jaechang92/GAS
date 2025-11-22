using System;
using System.Collections.Generic;
using UnityEngine;
using GASPT.Data;
using GASPT.Inventory;
using GASPT.Stats;
using GASPT.Core;
using Core.Enums;

namespace GASPT.UI.MVP
{
    /// <summary>
    /// Inventory Presenter (비즈니스 로직 담당)
    /// Plain C# 클래스 - Unity 없이 테스트 가능
    /// </summary>
    public class InventoryPresenter
    {
        // ====== 참조 ======

        private readonly IInventoryView view;
        private InventorySystem inventorySystem;
        private PlayerStats playerStats;


        // ====== 생성자 ======

        /// <summary>
        /// Presenter 생성자
        /// </summary>
        /// <param name="view">View 인터페이스</param>
        public InventoryPresenter(IInventoryView view)
        {
            this.view = view;

            // View 이벤트 구독
            view.OnOpenRequested += HandleOpenRequest;
            view.OnCloseRequested += HandleCloseRequest;
            view.OnItemEquipRequested += HandleItemEquipRequest;
            view.OnEquipmentSlotUnequipRequested += HandleEquipmentSlotUnequipRequest;
        }


        // ====== 초기화 ======

        /// <summary>
        /// Presenter 초기화 (Model 참조 획득)
        /// </summary>
        public void Initialize()
        {
            // InventorySystem 참조
            inventorySystem = InventorySystem.Instance;
            if (inventorySystem == null)
            {
                Debug.LogError("[InventoryPresenter] InventorySystem을 찾을 수 없습니다.");
                return;
            }

            // PlayerStats 참조
            playerStats = GameManager.Instance?.PlayerStats;
            if (playerStats == null)
            {
                Debug.LogWarning("[InventoryPresenter] PlayerStats를 찾을 수 없습니다. 나중에 재시도합니다.");
            }

            // Model 이벤트 구독
            inventorySystem.OnItemAdded += HandleItemAdded;
            inventorySystem.OnItemRemoved += HandleItemRemoved;

            // GameManager 이벤트 구독 (Player 등록/해제)
            if (GameManager.HasInstance)
            {
                GameManager.Instance.OnPlayerRegistered += HandlePlayerRegistered;
                GameManager.Instance.OnPlayerUnregistered += HandlePlayerUnregistered;
            }

            Debug.Log("[InventoryPresenter] 초기화 완료");
        }

        /// <summary>
        /// Presenter 정리
        /// </summary>
        public void Cleanup()
        {
            // View 이벤트 구독 해제
            view.OnOpenRequested -= HandleOpenRequest;
            view.OnCloseRequested -= HandleCloseRequest;
            view.OnItemEquipRequested -= HandleItemEquipRequest;
            view.OnEquipmentSlotUnequipRequested -= HandleEquipmentSlotUnequipRequest;

            // Model 이벤트 구독 해제
            if (inventorySystem != null)
            {
                inventorySystem.OnItemAdded -= HandleItemAdded;
                inventorySystem.OnItemRemoved -= HandleItemRemoved;
            }

            // GameManager 이벤트 구독 해제
            if (GameManager.HasInstance)
            {
                GameManager.Instance.OnPlayerRegistered -= HandlePlayerRegistered;
                GameManager.Instance.OnPlayerUnregistered -= HandlePlayerUnregistered;
            }
        }


        // ====== View 이벤트 핸들러 ======

        /// <summary>
        /// 인벤토리 열기 요청 처리
        /// </summary>
        private void HandleOpenRequest()
        {
            // PlayerStats 재확인 (씬 전환 후 등록되었을 수 있음)
            if (playerStats == null && GameManager.HasInstance)
            {
                playerStats = GameManager.Instance.PlayerStats;
            }

            // Model에서 데이터 가져오기
            var items = inventorySystem?.GetItems() ?? new List<Item>();

            // ViewModel로 변환
            var itemViewModels = ConvertToItemViewModels(items);

            // 장비 ViewModel 생성
            var equipmentViewModel = CreateEquipmentViewModel();

            // View 업데이트
            view.DisplayItems(itemViewModels);
            view.DisplayEquipment(equipmentViewModel);
            view.ShowUI();

            Debug.Log($"[InventoryPresenter] 인벤토리 열기: 아이템 {items.Count}개");
        }

        /// <summary>
        /// 인벤토리 닫기 요청 처리
        /// </summary>
        private void HandleCloseRequest()
        {
            view.HideUI();
            Debug.Log("[InventoryPresenter] 인벤토리 닫기");
        }

        /// <summary>
        /// 아이템 장착 요청 처리
        /// </summary>
        private void HandleItemEquipRequest(Item item)
        {
            if (item == null)
                return;

            // 검증 1: 아이템 소유 확인 (InventorySystem 책임)
            if (!inventorySystem.HasItem(item))
            {
                view.ShowError($"{item.itemName}을(를) 보유하고 있지 않습니다.");
                return;
            }

            // 검증 2: PlayerStats 확인
            if (playerStats == null)
            {
                view.ShowError("플레이어를 찾을 수 없습니다.");
                return;
            }

            // 이미 장착된 아이템인지 확인
            Item equippedItem = playerStats.GetEquippedItem(item.slot);

            if (equippedItem == item)
            {
                // 장착 해제
                bool success = playerStats.UnequipItem(item.slot);
                if (success)
                {
                    view.ShowSuccess($"{item.itemName} 장착 해제");
                    RefreshView();
                }
                else
                {
                    view.ShowError("장착 해제에 실패했습니다.");
                }
            }
            else
            {
                // 장착 (PlayerStats 책임)
                bool success = playerStats.EquipItem(item);
                if (success)
                {
                    view.ShowSuccess($"{item.itemName} 장착 완료");
                    RefreshView();
                }
                else
                {
                    view.ShowError("장착에 실패했습니다.");
                }
            }
        }

        /// <summary>
        /// 장비 슬롯 해제 요청 처리
        /// </summary>
        private void HandleEquipmentSlotUnequipRequest(EquipmentSlot slot)
        {
            if (playerStats == null)
            {
                view.ShowError("플레이어를 찾을 수 없습니다.");
                return;
            }

            // 장착 해제 (PlayerStats 책임)
            bool success = playerStats.UnequipItem(slot);
            if (success)
            {
                view.ShowSuccess($"{slot} 슬롯 장착 해제");
                RefreshView();
            }
            else
            {
                view.ShowError("장착 해제에 실패했습니다.");
            }
        }


        // ====== Model 이벤트 핸들러 ======

        /// <summary>
        /// 아이템 추가 시 호출
        /// </summary>
        private void HandleItemAdded(Item item)
        {
            RefreshView();
        }

        /// <summary>
        /// 아이템 제거 시 호출
        /// </summary>
        private void HandleItemRemoved(Item item)
        {
            RefreshView();
        }

        /// <summary>
        /// Player 등록 시 호출 (씬 전환 후)
        /// </summary>
        private void HandlePlayerRegistered(PlayerStats player)
        {
            playerStats = player;
            Debug.Log("[InventoryPresenter] PlayerStats 참조 갱신");
        }

        /// <summary>
        /// Player 해제 시 호출 (씬 전환 전)
        /// </summary>
        private void HandlePlayerUnregistered()
        {
            playerStats = null;
            Debug.Log("[InventoryPresenter] PlayerStats 참조 해제");
        }


        // ====== ViewModel 변환 ======

        /// <summary>
        /// Item 목록을 ItemViewModel 목록으로 변환
        /// </summary>
        private List<ItemViewModel> ConvertToItemViewModels(List<Item> items)
        {
            var viewModels = new List<ItemViewModel>();

            foreach (var item in items)
            {
                if (item == null)
                    continue;

                // 장착 중인지 확인
                bool isEquipped = false;
                if (playerStats != null)
                {
                    Item equippedItem = playerStats.GetEquippedItem(item.slot);
                    isEquipped = (equippedItem == item);
                }

                viewModels.Add(ItemViewModel.FromItem(item, isEquipped));
            }

            return viewModels;
        }

        /// <summary>
        /// PlayerStats로부터 EquipmentViewModel 생성
        /// </summary>
        private EquipmentViewModel CreateEquipmentViewModel()
        {
            var equipment = new EquipmentViewModel();

            if (playerStats != null)
            {
                equipment.WeaponItem = playerStats.GetEquippedItem(EquipmentSlot.Weapon);
                equipment.ArmorItem = playerStats.GetEquippedItem(EquipmentSlot.Armor);
                equipment.RingItem = playerStats.GetEquippedItem(EquipmentSlot.Ring);
            }

            return equipment;
        }


        // ====== View 갱신 ======

        /// <summary>
        /// View 전체 갱신
        /// </summary>
        private void RefreshView()
        {
            // Model에서 데이터 가져오기
            var items = inventorySystem?.GetItems() ?? new List<Item>();

            // ViewModel로 변환
            var itemViewModels = ConvertToItemViewModels(items);
            var equipmentViewModel = CreateEquipmentViewModel();

            // View 업데이트
            view.DisplayItems(itemViewModels);
            view.DisplayEquipment(equipmentViewModel);
        }
    }
}
