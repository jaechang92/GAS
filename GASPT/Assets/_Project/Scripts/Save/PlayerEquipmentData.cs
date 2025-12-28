using System;
using System.Collections.Generic;
using GASPT.Core.Enums;

namespace GASPT.Save
{
    /// <summary>
    /// 플레이어 장비 저장 데이터 (V2)
    /// ItemInstance 기반 저장
    /// </summary>
    [Serializable]
    public class PlayerEquipmentDataV2
    {
        /// <summary>
        /// 장착된 장비 목록
        /// </summary>
        public List<EquippedSlotData> equippedSlots;


        /// <summary>
        /// 기본 생성자
        /// </summary>
        public PlayerEquipmentDataV2()
        {
            equippedSlots = new List<EquippedSlotData>();
        }
    }


    /// <summary>
    /// 장착된 슬롯 데이터
    /// </summary>
    [Serializable]
    public class EquippedSlotData
    {
        /// <summary>
        /// 장비 슬롯
        /// </summary>
        public EquipmentSlot slot;

        /// <summary>
        /// 인벤토리 슬롯 인덱스 (인벤토리 연동용)
        /// </summary>
        public int inventorySlotIndex;

        /// <summary>
        /// 아이템 인스턴스 ID
        /// </summary>
        public string instanceId;

        /// <summary>
        /// ItemData 에셋 경로
        /// </summary>
        public string itemDataPath;

        /// <summary>
        /// 현재 내구도
        /// </summary>
        public int currentDurability;

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
        public EquippedSlotData()
        {
            slot = EquipmentSlot.None;
            inventorySlotIndex = -1;
            instanceId = "";
            itemDataPath = "";
            currentDurability = -1;
            acquireTimeTicks = 0;
            randomStats = new List<StatModifierData>();
        }
    }
}
