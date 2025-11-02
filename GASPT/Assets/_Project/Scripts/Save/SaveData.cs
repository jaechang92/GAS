using System;
using System.Collections.Generic;
using Core.Enums;

namespace GASPT.Save
{
    /// <summary>
    /// 게임 전체 저장 데이터
    /// JSON으로 직렬화되어 파일에 저장됨
    /// </summary>
    [Serializable]
    public class GameSaveData
    {
        /// <summary>
        /// 플레이어 스탯 데이터
        /// </summary>
        public PlayerStatsData playerStats;

        /// <summary>
        /// 화폐 시스템 데이터
        /// </summary>
        public CurrencyData currency;

        /// <summary>
        /// 인벤토리 시스템 데이터
        /// </summary>
        public InventoryData inventory;

        /// <summary>
        /// 저장 시간 (Ticks)
        /// </summary>
        public long saveTimeTicks;

        /// <summary>
        /// 저장 시간 (DateTime)
        /// </summary>
        public DateTime SaveTime
        {
            get => new DateTime(saveTimeTicks);
            set => saveTimeTicks = value.Ticks;
        }


        /// <summary>
        /// 기본 생성자
        /// </summary>
        public GameSaveData()
        {
            playerStats = new PlayerStatsData();
            currency = new CurrencyData();
            inventory = new InventoryData();
            SaveTime = DateTime.Now;
        }
    }


    // ====== PlayerStats 저장 데이터 ======

    /// <summary>
    /// 플레이어 스탯 저장 데이터
    /// </summary>
    [Serializable]
    public class PlayerStatsData
    {
        /// <summary>
        /// 현재 HP
        /// </summary>
        public int currentHP;

        /// <summary>
        /// 장착된 아이템 목록 (슬롯 → 아이템 경로)
        /// </summary>
        public List<EquippedItemEntry> equippedItems;


        /// <summary>
        /// 기본 생성자
        /// </summary>
        public PlayerStatsData()
        {
            currentHP = 100;
            equippedItems = new List<EquippedItemEntry>();
        }
    }

    /// <summary>
    /// 장착된 아이템 항목 (직렬화 가능)
    /// </summary>
    [Serializable]
    public class EquippedItemEntry
    {
        /// <summary>
        /// 장비 슬롯
        /// </summary>
        public EquipmentSlot slot;

        /// <summary>
        /// 아이템 에셋 경로 (예: "Assets/_Project/Data/Items/FireSword.asset")
        /// </summary>
        public string itemPath;


        /// <summary>
        /// 기본 생성자
        /// </summary>
        public EquippedItemEntry()
        {
        }

        /// <summary>
        /// 생성자
        /// </summary>
        public EquippedItemEntry(EquipmentSlot slot, string itemPath)
        {
            this.slot = slot;
            this.itemPath = itemPath;
        }
    }


    // ====== Currency 저장 데이터 ======

    /// <summary>
    /// 화폐 시스템 저장 데이터
    /// </summary>
    [Serializable]
    public class CurrencyData
    {
        /// <summary>
        /// 현재 골드
        /// </summary>
        public int gold;


        /// <summary>
        /// 기본 생성자
        /// </summary>
        public CurrencyData()
        {
            gold = 0;
        }
    }


    // ====== Inventory 저장 데이터 ======

    /// <summary>
    /// 인벤토리 시스템 저장 데이터
    /// </summary>
    [Serializable]
    public class InventoryData
    {
        /// <summary>
        /// 보유 아이템 경로 목록
        /// </summary>
        public List<string> itemPaths;


        /// <summary>
        /// 기본 생성자
        /// </summary>
        public InventoryData()
        {
            itemPaths = new List<string>();
        }
    }
}
