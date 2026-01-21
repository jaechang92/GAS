using UnityEngine;
using GASPT.Core.Enums;
using GASPT.Gameplay.Form;

namespace GASPT.Data
{
    /// <summary>
    /// 스킬 아이템 데이터 V2 (ItemData 상속)
    /// 스킬을 부여하는 아이템의 데이터 정의
    /// </summary>
    [CreateAssetMenu(fileName = "SkillItemData", menuName = "GASPT/Items/SkillItemData")]
    public class SkillItemData : ItemData
    {
        // ====== 스킬 정보 ======

        [Header("스킬 정보")]
        [Tooltip("스킬 타입 (어떤 어빌리티를 부여할지)")]
        public AbilityType abilityType = AbilityType.None;

        [Tooltip("장착될 슬롯 인덱스 (0: 기본공격, 1-3: 스킬)")]
        [Range(0, 3)]
        public int targetSlotIndex = 1;


        // ====== 스킬 스탯 ======

        [Header("스킬 스탯")]
        [Tooltip("스킬 쿨다운 (초)")]
        [Range(0f, 60f)]
        public float cooldown = 3f;

        [Tooltip("스킬 데미지 (데미지 스킬인 경우)")]
        [Range(0, 500)]
        public int damage = 0;

        [Tooltip("스킬 범위 (범위 스킬인 경우)")]
        [Range(0f, 20f)]
        public float range = 0f;

        [Tooltip("스킬 지속시간 (버프/디버프인 경우)")]
        [Range(0f, 30f)]
        public float duration = 0f;

        [Tooltip("마나 소모량")]
        [Range(0, 100)]
        public int manaCost = 0;


        // ====== 어빌리티 데이터 참조 (선택적) ======

        [Header("어빌리티 데이터 (선택적)")]
        [Tooltip("AbilityData ScriptableObject (상세 설정용, 없으면 위 스탯 사용)")]
        public AbilityData abilityData;


        // ====== 스킬 이펙트 ======

        [Header("스킬 이펙트")]
        [Tooltip("스킬 사용 시 이펙트 프리팹")]
        public GameObject effectPrefab;

        [Tooltip("스킬 투사체 프리팹 (투사체 스킬인 경우)")]
        public GameObject projectilePrefab;

        [Tooltip("스킬 사운드")]
        public AudioClip skillSound;


        // ====== 팩토리 메서드 ======

        /// <summary>
        /// IAbility 인스턴스 생성 팩토리 메서드
        /// AbilityType에 따라 적절한 IAbility 구현체를 생성
        /// AbilityData가 있으면 데이터 드리븐 방식, 없으면 하드코딩된 기본값 사용
        /// </summary>
        /// <returns>생성된 IAbility 인스턴스 (없으면 null)</returns>
        public IAbility CreateAbilityInstance()
        {
            BaseAbility ability = CreateAbilityByType();

            if (ability == null)
            {
                Debug.LogWarning($"[SkillItemData] {itemName}: 유효하지 않은 AbilityType ({abilityType})");
                return null;
            }

            // AbilityData가 있으면 설정
            if (abilityData != null)
            {
                ability.SetAbilityData(abilityData);
                Debug.Log($"[SkillItemData] {itemName}: AbilityData 적용됨 ({abilityData.abilityName})");
            }

            return ability;
        }

        /// <summary>
        /// AbilityType에 따른 BaseAbility 인스턴스 생성
        /// </summary>
        private BaseAbility CreateAbilityByType()
        {
            // AbilityData가 있으면 데이터 기반 생성자 사용
            if (abilityData != null)
            {
                return CreateAbilityWithData(abilityData);
            }

            // AbilityData가 없으면 기본 생성자 사용
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
                    return null;
            }
        }

        /// <summary>
        /// AbilityData를 사용하여 어빌리티 생성
        /// </summary>
        private BaseAbility CreateAbilityWithData(AbilityData data)
        {
            switch (abilityType)
            {
                case AbilityType.MagicMissile:
                    return new MagicMissileAbility(data);

                case AbilityType.Fireball:
                    return new FireballAbility(data);

                case AbilityType.Teleport:
                    return new TeleportAbility(data);

                case AbilityType.IceBlast:
                    return new IceBlastAbility(data);

                case AbilityType.LightningBolt:
                    return new LightningBoltAbility(data);

                case AbilityType.Shield:
                    return new ShieldAbility(data);

                case AbilityType.None:
                default:
                    return null;
            }
        }


        // ====== 검증 ======

        /// <summary>
        /// 스킬 아이템 데이터 유효성 검증
        /// </summary>
        public bool Validate()
        {
            bool isValid = true;

            if (string.IsNullOrEmpty(itemName))
            {
                Debug.LogError($"[SkillItemData] itemName이 비어있습니다: {name}");
                isValid = false;
            }

            if (abilityType == AbilityType.None)
            {
                Debug.LogError($"[SkillItemData] {itemName}: AbilityType이 None입니다!");
                isValid = false;
            }

            if (targetSlotIndex < 0 || targetSlotIndex > 3)
            {
                Debug.LogError($"[SkillItemData] {itemName}: targetSlotIndex가 유효 범위를 벗어났습니다 ({targetSlotIndex})");
                isValid = false;
            }

            if (cooldown < 0f)
            {
                Debug.LogWarning($"[SkillItemData] {itemName}: cooldown이 음수입니다. 0으로 보정합니다.");
                cooldown = 0f;
            }

            return isValid;
        }


        // ====== Unity 콜백 ======

        private new void OnValidate()
        {
            base.OnValidate();

            // 카테고리 자동 설정
            category = ItemCategory.Skill;

            // 스택 불가
            stackable = false;
            maxStack = 1;

            // 값 보정
            if (cooldown < 0f) cooldown = 0f;
            if (damage < 0) damage = 0;
            if (range < 0f) range = 0f;
            if (duration < 0f) duration = 0f;
            if (manaCost < 0) manaCost = 0;
        }


        // ====== 디버그 ======

        /// <summary>
        /// 스킬 아이템 정보 출력 (디버그용)
        /// </summary>
        [ContextMenu("Print Skill Item Info")]
        public void PrintSkillItemInfo()
        {
            Debug.Log("========== Skill Item Data Info ==========");
            Debug.Log($"Name: {itemName}");
            Debug.Log($"ID: {itemId}");
            Debug.Log($"Description: {description}");
            Debug.Log($"Rarity: {rarity}");
            Debug.Log($"--- Skill Info ---");
            Debug.Log($"Ability Type: {abilityType}");
            Debug.Log($"Target Slot: {targetSlotIndex}");
            Debug.Log($"Cooldown: {cooldown}s");
            Debug.Log($"Damage: {damage}");
            Debug.Log($"Range: {range}");
            Debug.Log($"Duration: {duration}s");
            Debug.Log($"Mana Cost: {manaCost}");
            Debug.Log("==========================================");
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
                Debug.Log($"[SkillItemData] 생성 성공: {ability.AbilityName} (쿨다운: {ability.Cooldown}s)");
            }
            else
            {
                Debug.LogError($"[SkillItemData] 생성 실패: {abilityType}");
            }
        }
    }
}
