using System;
using System.Collections.Generic;

namespace GASPT.DTOs
{
    /// <summary>
    /// 런 중 플레이어 데이터 (순수 데이터 클래스)
    /// - RunManager에서 보관 (DontDestroyOnLoad)
    /// - 씬 전환 시에도 데이터 유지
    /// - Player 생성 시 주입, 파괴 시 저장
    /// </summary>
    [Serializable]
    public class PlayerRunData
    {
        // ===== 기본 스탯 =====
        public int maxHP;
        public int currentHP;
        public int maxMana;
        public int currentMana;
        public int baseAttack;
        public int baseDefense;

        // ===== 레벨/경험치 =====
        public int level;
        public int currentExp;

        // ===== 경제 =====
        public int gold;

        // ===== 인벤토리 =====
        public List<string> itemIds;
        public List<EquippedItemData> equippedItems;

        // ===== 스킬 =====
        public List<string> skillIds;
        public string[] equippedSkillSlots;

        // ===== 런 진행도 =====
        public int currentStage;
        public int clearedRooms;
        public float playTime;

        // ===== 생성자 =====
        public PlayerRunData()
        {
            itemIds = new List<string>();
            equippedItems = new List<EquippedItemData>();
            skillIds = new List<string>();
            equippedSkillSlots = new string[4];
        }

        /// <summary>
        /// 기본값으로 새 런 데이터 생성
        /// </summary>
        public static PlayerRunData CreateDefault()
        {
            return new PlayerRunData
            {
                // 기본 스탯
                maxHP = 100,
                currentHP = 100,
                maxMana = 100,
                currentMana = 100,
                baseAttack = 10,
                baseDefense = 5,

                // 레벨
                level = 1,
                currentExp = 0,

                // 경제
                gold = 0,

                // 인벤토리
                itemIds = new List<string>(),
                equippedItems = new List<EquippedItemData>(),

                // 스킬
                skillIds = new List<string>(),
                equippedSkillSlots = new string[4],

                // 런 진행도
                currentStage = 1,
                clearedRooms = 0,
                playTime = 0f
            };
        }

        /// <summary>
        /// 메타 업그레이드 적용된 런 데이터 생성
        /// </summary>
        public static PlayerRunData CreateWithMetaUpgrades(int bonusHP, int bonusAttack, int bonusDefense)
        {
            var data = CreateDefault();
            data.maxHP += bonusHP;
            data.currentHP = data.maxHP;
            data.baseAttack += bonusAttack;
            data.baseDefense += bonusDefense;
            return data;
        }

        /// <summary>
        /// 데이터 복사본 생성
        /// </summary>
        public PlayerRunData Clone()
        {
            return new PlayerRunData
            {
                maxHP = this.maxHP,
                currentHP = this.currentHP,
                maxMana = this.maxMana,
                currentMana = this.currentMana,
                baseAttack = this.baseAttack,
                baseDefense = this.baseDefense,
                level = this.level,
                currentExp = this.currentExp,
                gold = this.gold,
                itemIds = new List<string>(this.itemIds),
                equippedItems = new List<EquippedItemData>(this.equippedItems),
                skillIds = new List<string>(this.skillIds),
                equippedSkillSlots = (string[])this.equippedSkillSlots.Clone(),
                currentStage = this.currentStage,
                clearedRooms = this.clearedRooms,
                playTime = this.playTime
            };
        }

        public override string ToString()
        {
            return $"[PlayerRunData] HP:{currentHP}/{maxHP}, Mana:{currentMana}/{maxMana}, " +
                   $"Atk:{baseAttack}, Def:{baseDefense}, Lv:{level}, Gold:{gold}, Stage:{currentStage}";
        }
    }

    /// <summary>
    /// 장착 아이템 데이터
    /// </summary>
    [Serializable]
    public class EquippedItemData
    {
        public string slotName;
        public string itemId;

        public EquippedItemData() { }

        public EquippedItemData(string slot, string id)
        {
            slotName = slot;
            itemId = id;
        }
    }
}
