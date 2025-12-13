using UnityEngine;
using GASPT.Gameplay.Form;

namespace GASPT.Data
{
    /// <summary>
    /// 스킬 아이템 ScriptableObject
    /// Item을 상속받아 스킬을 부여하는 아이템 구현
    /// 획득 시 Form의 지정된 슬롯에 자동 장착됨
    /// </summary>
    [CreateAssetMenu(fileName = "SkillItem", menuName = "GASPT/Items/Skill Item")]
    public class SkillItem : Item
    {
        // ====== 스킬 정보 ======

        [Header("스킬 정보")]
        [Tooltip("스킬 타입 (어떤 어빌리티를 부여할지)")]
        public AbilityType abilityType = AbilityType.None;

        [Tooltip("장착될 슬롯 인덱스 (0: 기본공격, 1-3: 스킬)")]
        [Range(0, 3)]
        public int targetSlotIndex = 1;

        [Tooltip("스킬 희귀도 (UI 색상 및 드롭률)")]
        public SkillRarity rarity = SkillRarity.Common;


        // ====== 스킬 설명 ======

        [Header("스킬 상세")]
        [Tooltip("스킬 쿨다운 (초)")]
        [Range(0f, 30f)]
        public float cooldown = 3f;

        [Tooltip("스킬 데미지 (데미지 스킬인 경우)")]
        [Range(0, 200)]
        public int damage = 0;

        [Tooltip("스킬 범위 (범위 스킬인 경우)")]
        [Range(0f, 10f)]
        public float range = 0f;


        // ====== 팩토리 메서드 ======

        /// <summary>
        /// IAbility 인스턴스 생성 팩토리 메서드
        /// AbilityType에 따라 적절한 IAbility 구현체를 생성
        /// </summary>
        /// <returns>생성된 IAbility 인스턴스 (없으면 null)</returns>
        public IAbility CreateAbilityInstance()
        {
            switch (abilityType)
            {
                case AbilityType.MagicMissile:
                    return new MagicMissileAbility();

                case AbilityType.Fireball:
                    return new FireballAbility();

                case AbilityType.Teleport:
                    return new TeleportAbility();

                case AbilityType.IceBlast:
                    return new IceBlastAbility();

                case AbilityType.LightningBolt:
                    return new LightningBoltAbility();

                case AbilityType.Shield:
                    return new ShieldAbility();

                case AbilityType.None:
                default:
                    Debug.LogWarning($"[SkillItem] {itemName}: 유효하지 않은 AbilityType ({abilityType})");
                    return null;
            }
        }


        // ====== 검증 ======

        /// <summary>
        /// 스킬 아이템 데이터 유효성 검증
        /// </summary>
        public override bool Validate()
        {
            if (!base.Validate())
                return false;

            if (abilityType == AbilityType.None)
            {
                Debug.LogError($"[SkillItem] {itemName}: AbilityType이 None입니다!");
                return false;
            }

            if (targetSlotIndex < 0 || targetSlotIndex > 3)
            {
                Debug.LogError($"[SkillItem] {itemName}: targetSlotIndex가 유효 범위를 벗어났습니다 ({targetSlotIndex})");
                return false;
            }

            return true;
        }


        // ====== 디버그 ======

        /// <summary>
        /// 스킬 아이템 정보 출력 (디버그용)
        /// </summary>
        [ContextMenu("Print Skill Item Info")]
        public void PrintSkillItemInfo()
        {
            Debug.Log("========== Skill Item Info ==========");
            Debug.Log($"Name: {itemName}");
            Debug.Log($"Description: {description}");
            Debug.Log($"Ability Type: {abilityType}");
            Debug.Log($"Target Slot: {targetSlotIndex}");
            Debug.Log($"Rarity: {rarity}");
            Debug.Log($"Cooldown: {cooldown}s");
            Debug.Log($"Damage: {damage}");
            Debug.Log($"Range: {range}");
            Debug.Log("=====================================");
        }

        /// <summary>
        /// 스킬 인스턴스 생성 테스트
        /// </summary>
        [ContextMenu("Test Create Ability Instance")]
        private void TestCreateAbilityInstance()
        {
            IAbility ability = CreateAbilityInstance();

            if (ability != null)
            {
                Debug.Log($"[SkillItem] 생성 성공: {ability.AbilityName} (쿨다운: {ability.Cooldown}s)");
            }
            else
            {
                Debug.LogError($"[SkillItem] 생성 실패: {abilityType}");
            }
        }
    }
}
