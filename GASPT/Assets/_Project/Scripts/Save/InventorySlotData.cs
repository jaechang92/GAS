using System;
using System.Collections.Generic;
using GASPT.Core.Enums;

namespace GASPT.Save
{
    /// <summary>
    /// 인벤토리 슬롯 저장 데이터
    /// </summary>
    [Serializable]
    public class InventorySlotData
    {
        /// <summary>
        /// 슬롯 인덱스
        /// </summary>
        public int slotIndex;

        /// <summary>
        /// ItemData 에셋 경로
        /// </summary>
        public string itemDataPath;

        /// <summary>
        /// 아이템 인스턴스 ID
        /// </summary>
        public string instanceId;

        /// <summary>
        /// 아이템 수량
        /// </summary>
        public int quantity;

        /// <summary>
        /// 현재 내구도
        /// </summary>
        public int currentDurability;

        /// <summary>
        /// 장착 상태
        /// </summary>
        public bool isEquipped;

        /// <summary>
        /// 획득 시간 (UTC ticks)
        /// </summary>
        public long acquireTimeTicks;

        /// <summary>
        /// 랜덤 스탯 목록
        /// </summary>
        public List<StatModifierData> randomStats;


        /// <summary>
        /// 기본 생성자
        /// </summary>
        public InventorySlotData()
        {
            slotIndex = -1;
            itemDataPath = "";
            instanceId = "";
            quantity = 0;
            currentDurability = -1;
            isEquipped = false;
            acquireTimeTicks = 0;
            randomStats = new List<StatModifierData>();
        }
    }


    /// <summary>
    /// 스탯 수정자 저장 데이터
    /// </summary>
    [Serializable]
    public class StatModifierData
    {
        /// <summary>
        /// 스탯 종류
        /// </summary>
        public StatType statType;

        /// <summary>
        /// 수정자 타입 (Flat/Percent)
        /// </summary>
        public ModifierType modifierType;

        /// <summary>
        /// 수치
        /// </summary>
        public float value;


        /// <summary>
        /// 기본 생성자
        /// </summary>
        public StatModifierData()
        {
        }

        /// <summary>
        /// 생성자
        /// </summary>
        public StatModifierData(StatType statType, ModifierType modifierType, float value)
        {
            this.statType = statType;
            this.modifierType = modifierType;
            this.value = value;
        }
    }


    /// <summary>
    /// 인벤토리 저장 데이터 V2 (슬롯 기반)
    /// </summary>
    [Serializable]
    public class InventoryDataV2
    {
        /// <summary>
        /// 인벤토리 용량
        /// </summary>
        public int capacity;

        /// <summary>
        /// 슬롯 데이터 목록
        /// </summary>
        public List<InventorySlotData> slots;


        /// <summary>
        /// 기본 생성자
        /// </summary>
        public InventoryDataV2()
        {
            capacity = 30;
            slots = new List<InventorySlotData>();
        }
    }
}
