using UnityEngine;
using GAS.Core;

namespace Skull.Data
{
    /// <summary>
    /// 스컬 기본 데이터 구조 (ScriptableObject)
    /// </summary>
    [CreateAssetMenu(fileName = "New Skull Data", menuName = "Skull System/Skull Data")]
    public class SkullData : ScriptableObject
    {
        [Header("기본 정보")]
        [SerializeField] private string skullName = "Unknown Skull";
        [SerializeField] private SkullType skullType = SkullType.Default;
        [SerializeField] private string description = "";
        [SerializeField] private Sprite icon;

        [Header("비주얼")]
        [SerializeField] private GameObject skullPrefab;
        [SerializeField] private Sprite playerSprite;
        [SerializeField] private RuntimeAnimatorController animatorController;
        [SerializeField] private Material playerMaterial;

        [Header("능력치")]
        [SerializeField] private SkullStats baseStats = new SkullStats();

        [Header("어빌리티")]
        [SerializeField] private Ability[] skullAbilities;
        [SerializeField] private Ability primaryAbility;
        [SerializeField] private Ability secondaryAbility;
        [SerializeField] private Ability ultimateAbility;

        [Header("사운드")]
        [SerializeField] private AudioClip equipSound;
        [SerializeField] private AudioClip[] attackSounds;
        [SerializeField] private AudioClip[] movementSounds;

        // Properties for external access
        public string SkullName => skullName;
        public SkullType Type => skullType;
        public string Description => description;
        public Sprite Icon => icon;
        public GameObject SkullPrefab => skullPrefab;
        public Sprite PlayerSprite => playerSprite;
        public RuntimeAnimatorController AnimatorController => animatorController;
        public Material PlayerMaterial => playerMaterial;
        public SkullStats BaseStats => baseStats;
        public Ability[] SkullAbilities => skullAbilities;
        public Ability PrimaryAbility => primaryAbility;
        public Ability SecondaryAbility => secondaryAbility;
        public Ability UltimateAbility => ultimateAbility;
        public AudioClip EquipSound => equipSound;
        public AudioClip[] AttackSounds => attackSounds;
        public AudioClip[] MovementSounds => movementSounds;

        /// <summary>
        /// 스컬 데이터 유효성 검증
        /// </summary>
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(skullName) &&
                   skullType != SkullType.None &&
                   baseStats != null;
        }

        private void OnValidate()
        {
            // 에디터에서 값 변경 시 자동 검증
            if (string.IsNullOrEmpty(skullName))
            {
                skullName = $"{skullType} Skull";
            }
        }
    }
}