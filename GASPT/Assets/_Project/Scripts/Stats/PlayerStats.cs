using System;
using System.Collections.Generic;
using GASPT.Core.Enums;
using GASPT.Data;
using GASPT.Save;
using GASPT.StatusEffects;
using GASPT.Core;
using GASPT.Gameplay.Form;
using UnityEngine;

namespace GASPT.Stats
{
    /// <summary>
    /// 플레이어 스탯 관리 MonoBehaviour
    /// 기본 스탯 + 장비 보너스 = 최종 스탯
    /// Dirty flag 최적화로 변경 시에만 재계산 (<50ms)
    /// ISaveable 인터페이스 구현으로 SaveManager 지원
    /// </summary>
    public partial class PlayerStats : MonoBehaviour, ISaveable
    {
        [Header("테스트 장착용 아이템 (런타임에만 사용됨)")]
        [SerializeField] [Tooltip("테스트용 아이템")]
        private Item testItemToEquip;

        // ====== 기본 스탯 ======

        [Header("기본 스탯")]
        [SerializeField] [Tooltip("기본 HP")]
        private int baseHP = 100;

        [SerializeField] [Tooltip("기본 공격력")]
        private int baseAttack = 10;

        [SerializeField] [Tooltip("기본 방어력")]
        private int baseDefense = 5;

        [SerializeField] [Tooltip("기본 마나")]
        private int baseMana = 100;


        // ====== 장비 슬롯 ======

        /// <summary>
        /// 장비된 아이템 (슬롯 → 아이템)
        /// </summary>
        private Dictionary<EquipmentSlot, Item> equippedItems = new Dictionary<EquipmentSlot, Item>();


        // ====== 폼 보너스 ======

        /// <summary>
        /// 현재 적용된 폼 보너스 (기본 스탯: Attack, Defense, HP, Mana)
        /// </summary>
        private FormStats formBonus = FormStats.Default;

        /// <summary>
        /// 폼 보너스 적용 여부
        /// </summary>
        private bool hasFormBonus = false;


        // ====== 최종 스탯 (캐시) ======

        private int finalHP;
        private int finalAttack;
        private int finalDefense;
        private int finalMana;

        /// <summary>
        /// Dirty flag: true면 재계산 필요
        /// </summary>
        private bool isDirty = true;


        // ====== 현재 상태 (Combat) ======

        /// <summary>
        /// 현재 HP (전투 중 변경됨)
        /// </summary>
        private int currentHP;

        /// <summary>
        /// 현재 마나
        /// </summary>
        private int currentMana;

        /// <summary>
        /// 사망 여부
        /// </summary>
        private bool isDead = false;


        // ====== 프로퍼티 (외부 접근) ======

        /// <summary>
        /// 최대 HP (기본 + 장비 보너스)
        /// </summary>
        public int MaxHP
        {
            get
            {
                RecalculateIfDirty();
                return finalHP;
            }
        }

        /// <summary>
        /// 현재 HP (전투 중 변경되는 체력)
        /// </summary>
        public int CurrentHP => currentHP;

        /// <summary>
        /// 최종 공격력 (기본 + 장비 보너스 + 버프/디버프)
        /// </summary>
        public int Attack
        {
            get
            {
                RecalculateIfDirty();
                return ApplyStatusEffects(finalAttack, StatusEffectType.AttackUp, StatusEffectType.AttackDown);
            }
        }

        /// <summary>
        /// 기본 공격력 (기본 + 장비 보너스, 버프/디버프 제외)
        /// </summary>
        public int BaseAttack
        {
            get
            {
                RecalculateIfDirty();
                return finalAttack;
            }
        }

        /// <summary>
        /// 최종 방어력 (기본 + 장비 보너스 + 버프/디버프)
        /// </summary>
        public int Defense
        {
            get
            {
                RecalculateIfDirty();
                return ApplyStatusEffects(finalDefense, StatusEffectType.DefenseUp, StatusEffectType.DefenseDown);
            }
        }

        /// <summary>
        /// 기본 방어력 (기본 + 장비 보너스, 버프/디버프 제외)
        /// </summary>
        public int BaseDefense
        {
            get
            {
                RecalculateIfDirty();
                return finalDefense;
            }
        }

        /// <summary>
        /// 최대 마나 (기본 + 장비 보너스)
        /// </summary>
        public int MaxMana
        {
            get
            {
                RecalculateIfDirty();
                return finalMana;
            }
        }

        /// <summary>
        /// 현재 마나
        /// </summary>
        public int CurrentMana => currentMana;

        /// <summary>
        /// 사망 여부
        /// </summary>
        public bool IsDead => isDead;


        // ====== 이벤트 ======

        /// <summary>
        /// 스탯 변경 시 발생하는 이벤트
        /// 매개변수: (StatType, 이전 값, 새 값)
        /// </summary>
        public event Action<StatType, int, int> OnStatsChanged;

        /// <summary>
        /// 데미지를 받았을 때 발생하는 이벤트
        /// 매개변수: (데미지량, 현재 HP, 최대 HP)
        /// </summary>
        public event Action<int, int, int> OnDamaged;

        /// <summary>
        /// 체력을 회복했을 때 발생하는 이벤트
        /// 매개변수: (회복량, 현재 HP, 최대 HP)
        /// </summary>
        public event Action<int, int, int> OnHealed;

        /// <summary>
        /// 사망했을 때 발생하는 이벤트
        /// </summary>
        public event Action OnDeath;


        // ====== Unity 생명주기 ======

        private void Awake()
        {
            // 장비 슬롯 초기화
            equippedItems = new Dictionary<EquipmentSlot, Item>();

            // 초기 스탯 계산
            RecalculateStats();

            // 현재 HP를 최대 HP로 초기화
            currentHP = MaxHP;
            currentMana = MaxMana;
            isDead = false;

        }

        private void Start()
        {
            // V2 시스템 초기화
            InitializeV2System();

            // RunManager에 등록 (데이터 주입 포함)
            if (RunManager.HasInstance)
            {
                RunManager.Instance.RegisterPlayer(this);
            }
        }

        private void OnEnable()
        {
            // StatusEffect 이벤트 구독 (OnEnable에서 구독해야 StatusEffectManager 초기화 후 구독 가능)
            SubscribeToStatusEffectEvents();
        }

        private void OnDisable()
        {
            // StatusEffect 이벤트 구독 해제
            UnsubscribeFromStatusEffectEvents();
        }

        private void OnDestroy()
        {
            // V2 이벤트 구독 해제
            UnsubscribeFromV2Events();

            // RunManager에서 해제 (데이터 저장 포함)
            if (RunManager.HasInstance)
            {
                RunManager.Instance.UnregisterPlayer(this);
            }
        }


        // ====== 스탯 계산 (Dirty Flag 최적화) ======

        /// <summary>
        /// Dirty flag 확인 후 필요시 재계산
        /// </summary>
        private void RecalculateIfDirty()
        {
            if (isDirty)
            {
                RecalculateStats();
            }
        }

        /// <summary>
        /// 최종 스탯 재계산
        /// 기본 스탯 + 모든 장비 보너스 합산
        /// </summary>
        private void RecalculateStats()
        {
            // 이전 값 저장 (이벤트용)
            int oldHP = finalHP;
            int oldAttack = finalAttack;
            int oldDefense = finalDefense;
            int oldMana = finalMana;

            // 기본 스탯으로 초기화
            finalHP = baseHP;
            finalAttack = baseAttack;
            finalDefense = baseDefense;
            finalMana = baseMana;

            // 장비 보너스 합산
            foreach (var item in equippedItems.Values)
            {
                if (item != null)
                {
                    finalHP += item.hpBonus;
                    finalAttack += item.attackBonus;
                    finalDefense += item.defenseBonus;
                    // Mana 보너스는 현재 Item에 없으므로 생략 (나중에 추가 가능)
                }
            }

            // 폼 보너스 합산
            if (hasFormBonus)
            {
                finalHP += Mathf.RoundToInt(formBonus.maxHealthBonus);
                finalAttack += Mathf.RoundToInt(formBonus.attackPower);
                finalDefense += Mathf.RoundToInt(formBonus.defense);
                finalMana += Mathf.RoundToInt(formBonus.maxManaBonus);
            }

            // 메타 업그레이드 보너스 합산
            ApplyMetaUpgradeBonuses();

            // V2 시스템 보너스 적용 (장비 + 세트 효과)
            ApplyV2Bonuses();

            isDirty = false;

            // 변경된 스탯에 대해 이벤트 발생
            NotifyStatChangedIfDifferent(StatType.HP, oldHP, finalHP);
            NotifyStatChangedIfDifferent(StatType.Attack, oldAttack, finalAttack);
            NotifyStatChangedIfDifferent(StatType.Defense, oldDefense, finalDefense);
            NotifyStatChangedIfDifferent(StatType.Mana, oldMana, finalMana);

        }

        /// <summary>
        /// 값이 변경된 경우에만 이벤트 발생
        /// </summary>
        private void NotifyStatChangedIfDifferent(StatType statType, int oldValue, int newValue)
        {
            if (oldValue != newValue)
            {
                OnStatsChanged?.Invoke(statType, oldValue, newValue);
            }
        }

        /// <summary>
        /// 메타 업그레이드 보너스 적용
        /// MetaProgressionManager에서 영구 업그레이드 보너스를 가져와 적용
        /// </summary>
        private void ApplyMetaUpgradeBonuses()
        {
            if (!Meta.MetaProgressionManager.HasInstance) return;

            var meta = Meta.MetaProgressionManager.Instance;

            // HP 보너스 (고정값)
            int hpBonus = meta.GetMaxHPBonus();
            finalHP += hpBonus;

            // 공격력 보너스 (% 증가)
            float attackBonus = meta.GetAttackBonus();
            if (attackBonus > 0)
            {
                finalAttack += Mathf.RoundToInt(finalAttack * attackBonus);
            }

            // 방어력은 받는 피해 감소로 적용 (PlayerStatsMetaExtension.ApplyMetaDefenseBonus 사용)
            // finalDefense는 변경하지 않음 - 피해 계산 시 적용

        }
    }
}
