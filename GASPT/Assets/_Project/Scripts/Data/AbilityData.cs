using UnityEngine;
using GASPT.Gameplay.Form;

namespace GASPT.Data
{
    /// <summary>
    /// 어빌리티 데이터 ScriptableObject
    /// 스킬의 기본 정보 및 스탯을 데이터로 관리
    /// </summary>
    [CreateAssetMenu(fileName = "AbilityData", menuName = "GASPT/Abilities/AbilityData")]
    public class AbilityData : ScriptableObject
    {
        // ====== 기본 정보 ======

        [Header("기본 정보")]
        [Tooltip("어빌리티 ID (고유 식별자)")]
        public string abilityId;

        [Tooltip("어빌리티 이름 (UI 표시용)")]
        public string abilityName;

        [TextArea(2, 4)]
        [Tooltip("어빌리티 설명")]
        public string description;

        [Tooltip("어빌리티 아이콘")]
        public Sprite icon;

        [Tooltip("어빌리티 타입")]
        public AbilityType abilityType;


        // ====== 스탯 ======

        [Header("기본 스탯")]
        [Tooltip("쿨다운 (초)")]
        [Range(0f, 60f)]
        public float cooldown = 1f;

        [Tooltip("마나 소모량")]
        [Range(0, 100)]
        public int manaCost = 0;

        [Tooltip("기본 데미지")]
        [Range(0, 500)]
        public int baseDamage = 0;

        [Tooltip("기본 범위")]
        [Range(0f, 20f)]
        public float baseRange = 0f;


        // ====== 투사체 (투사체 스킬인 경우) ======

        [Header("투사체 설정")]
        [Tooltip("투사체 스킬 여부")]
        public bool isProjectile = false;

        [Tooltip("투사체 속도")]
        [Range(1f, 50f)]
        public float projectileSpeed = 10f;

        [Tooltip("투사체 수명 (초)")]
        [Range(0.1f, 10f)]
        public float projectileLifetime = 3f;

        [Tooltip("투사체 프리팹")]
        public GameObject projectilePrefab;


        // ====== 이펙트 ======

        [Header("이펙트")]
        [Tooltip("시전 이펙트 프리팹")]
        public GameObject castEffectPrefab;

        [Tooltip("적중 이펙트 프리팹")]
        public GameObject hitEffectPrefab;

        [Tooltip("시전 사운드")]
        public AudioClip castSound;

        [Tooltip("적중 사운드")]
        public AudioClip hitSound;


        // ====== 버프/디버프 (버프 스킬인 경우) ======

        [Header("버프/디버프")]
        [Tooltip("지속시간 (버프/디버프인 경우)")]
        [Range(0f, 60f)]
        public float duration = 0f;

        [Tooltip("효과량 (버프/디버프 값)")]
        public float effectValue = 0f;


        // ====== AOE (범위 스킬인 경우) ======

        [Header("AOE 설정")]
        [Tooltip("AOE 스킬 여부")]
        public bool isAOE = false;

        [Tooltip("AOE 반경")]
        [Range(0f, 10f)]
        public float aoeRadius = 0f;

        [Tooltip("AOE 이펙트 프리팹")]
        public GameObject aoeEffectPrefab;


        // ====== Unity 콜백 ======

        private void OnValidate()
        {
            // ID 자동 생성
            if (string.IsNullOrEmpty(abilityId))
            {
                abilityId = $"ABL_{name}";
            }

            // 음수 값 보정
            if (cooldown < 0f) cooldown = 0f;
            if (manaCost < 0) manaCost = 0;
            if (baseDamage < 0) baseDamage = 0;
            if (baseRange < 0f) baseRange = 0f;
            if (duration < 0f) duration = 0f;
            if (aoeRadius < 0f) aoeRadius = 0f;
        }


        // ====== 검증 ======

        /// <summary>
        /// 어빌리티 데이터 유효성 검증
        /// </summary>
        public bool Validate()
        {
            bool isValid = true;

            if (string.IsNullOrEmpty(abilityName))
            {
                Debug.LogError($"[AbilityData] abilityName이 비어있습니다: {name}");
                isValid = false;
            }

            if (abilityType == AbilityType.None)
            {
                Debug.LogWarning($"[AbilityData] {abilityName}: abilityType이 None입니다.");
            }

            if (isProjectile && projectilePrefab == null)
            {
                Debug.LogWarning($"[AbilityData] {abilityName}: 투사체 스킬이지만 projectilePrefab이 없습니다.");
            }

            if (isAOE && aoeRadius <= 0f)
            {
                Debug.LogWarning($"[AbilityData] {abilityName}: AOE 스킬이지만 aoeRadius가 0입니다.");
            }

            return isValid;
        }


        // ====== 디버그 ======

        [ContextMenu("Print Ability Info")]
        public void PrintAbilityInfo()
        {
            Debug.Log("========== Ability Data Info ==========");
            Debug.Log($"ID: {abilityId}");
            Debug.Log($"Name: {abilityName}");
            Debug.Log($"Type: {abilityType}");
            Debug.Log($"Description: {description}");
            Debug.Log($"--- Stats ---");
            Debug.Log($"Cooldown: {cooldown}s");
            Debug.Log($"Mana Cost: {manaCost}");
            Debug.Log($"Base Damage: {baseDamage}");
            Debug.Log($"Base Range: {baseRange}");
            if (isProjectile)
            {
                Debug.Log($"--- Projectile ---");
                Debug.Log($"Speed: {projectileSpeed}");
                Debug.Log($"Lifetime: {projectileLifetime}s");
                Debug.Log($"Prefab: {(projectilePrefab != null ? projectilePrefab.name : "None")}");
            }
            if (isAOE)
            {
                Debug.Log($"--- AOE ---");
                Debug.Log($"Radius: {aoeRadius}");
            }
            if (duration > 0f)
            {
                Debug.Log($"--- Buff/Debuff ---");
                Debug.Log($"Duration: {duration}s");
                Debug.Log($"Effect Value: {effectValue}");
            }
            Debug.Log("========================================");
        }
    }
}
