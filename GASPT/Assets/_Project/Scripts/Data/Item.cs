using UnityEngine;
using Core.Enums;

namespace GASPT.Data
{
    /// <summary>
    /// 아이템 데이터 ScriptableObject
    /// 장비 시 스탯 보너스를 제공하는 아이템
    /// </summary>
    [CreateAssetMenu(fileName = "Item", menuName = "GASPT/Items/Item")]
    public class Item : ScriptableObject
    {
        // ====== 기본 정보 ======

        [Header("기본 정보")]
        [Tooltip("아이템 이름")]
        public string itemName;

        [Tooltip("아이템 설명")]
        [TextArea(2, 4)]
        public string description;

        [Tooltip("아이템 아이콘 (UI 표시용)")]
        public Sprite icon;

        [Tooltip("장비 슬롯 타입")]
        public EquipmentSlot slot;


        // ====== 스탯 보너스 ======

        [Header("스탯 보너스")]
        [Tooltip("HP 보너스")]
        [Range(0, 100)]
        public int hpBonus = 0;

        [Tooltip("공격력 보너스")]
        [Range(0, 50)]
        public int attackBonus = 0;

        [Tooltip("방어력 보너스")]
        [Range(0, 50)]
        public int defenseBonus = 0;


        // ====== 헬퍼 메서드 ======

        /// <summary>
        /// 아이템 데이터 유효성 검증
        /// </summary>
        public virtual bool Validate()
        {
            if (string.IsNullOrEmpty(itemName))
            {
                Debug.LogError($"[Item] itemName이 비어있습니다: {name}");
                return false;
            }

            if (hpBonus == 0 && attackBonus == 0 && defenseBonus == 0)
            {
                Debug.LogWarning($"[Item] {itemName}: 모든 스탯 보너스가 0입니다.");
            }

            return true;
        }

        /// <summary>
        /// 아이템 정보 출력 (디버그용)
        /// </summary>
        public void DebugPrint()
        {
            Debug.Log($"[Item] {itemName}");
            Debug.Log($"  - Slot: {slot}");
            Debug.Log($"  - HP Bonus: {hpBonus}");
            Debug.Log($"  - Attack Bonus: {attackBonus}");
            Debug.Log($"  - Defense Bonus: {defenseBonus}");
        }
    }
}
