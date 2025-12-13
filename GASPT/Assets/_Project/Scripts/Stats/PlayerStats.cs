using System;
using System.Collections.Generic;
using System.Linq;
using GASPT.Core.Enums;
using GASPT.Data;
using GASPT.Combat;
using GASPT.Save;
using GASPT.UI;
using GASPT.StatusEffects;
using GASPT.Core;
using GASPT.DTOs;
using UnityEngine;
using GASPT.Gameplay.Enemies;

namespace GASPT.Stats
{
    /// <summary>
    /// 플레이어 스탯 관리 MonoBehaviour
    /// 기본 스탯 + 장비 보너스 = 최종 스탯
    /// Dirty flag 최적화로 변경 시에만 재계산 (<50ms)
    /// ISaveable 인터페이스 구현으로 SaveManager 지원
    /// </summary>
    public class PlayerStats : MonoBehaviour, ISaveable
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

            Debug.Log($"[PlayerStats] 초기화 완료 - MaxHP: {MaxHP}, CurrentHP: {CurrentHP}, MaxMana: {MaxMana}, CurrentMana: {CurrentMana}, Attack: {Attack}, Defense: {Defense}");
        }

        private void Start()
        {
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

            isDirty = false;

            // 변경된 스탯에 대해 이벤트 발생
            NotifyStatChangedIfDifferent(StatType.HP, oldHP, finalHP);
            NotifyStatChangedIfDifferent(StatType.Attack, oldAttack, finalAttack);
            NotifyStatChangedIfDifferent(StatType.Defense, oldDefense, finalDefense);
            NotifyStatChangedIfDifferent(StatType.Mana, oldMana, finalMana);

            Debug.Log($"[PlayerStats] 스탯 재계산 완료 - HP: {finalHP}, Attack: {finalAttack}, Defense: {finalDefense}, Mana: {finalMana}");
        }

        /// <summary>
        /// 값이 변경된 경우에만 이벤트 발생
        /// </summary>
        private void NotifyStatChangedIfDifferent(StatType statType, int oldValue, int newValue)
        {
            if (oldValue != newValue)
            {
                OnStatsChanged?.Invoke(statType, oldValue, newValue);
                Debug.Log($"[PlayerStats] {statType} 변경: {oldValue} → {newValue}");
            }
        }


        // ====== 장비 착용/해제 ======
        [ContextMenu("Equip Test Item")]

        public void EquipTestItem()
        {
            if (testItemToEquip != null)
            {
                EquipItem(testItemToEquip);
            }
            else
            {
                Debug.LogWarning("[PlayerStats] EquipTestItem(): testItemToEquip이 설정되지 않았습니다.");
            }
        }

        /// <summary>
        /// 아이템 장착
        /// </summary>
        /// <param name="item">장착할 아이템</param>
        /// <returns>true: 장착 성공, false: 장착 실패</returns>
        public bool EquipItem(Item item)
        {
            if (item == null)
            {
                Debug.LogWarning("[PlayerStats] EquipItem(): item이 null입니다.");
                return false;
            }

            // 유효성 검증
            if (!item.Validate())
            {
                Debug.LogError($"[PlayerStats] EquipItem(): 유효하지 않은 아이템입니다: {item.name}");
                return false;
            }

            // 해당 슬롯에 이미 아이템이 있으면 자동으로 교체
            if (equippedItems.ContainsKey(item.slot))
            {
                Item oldItem = equippedItems[item.slot];
                Debug.Log($"[PlayerStats] {item.slot} 슬롯의 {oldItem.itemName}을(를) {item.itemName}(으)로 교체합니다.");
            }

            // 아이템 장착
            equippedItems[item.slot] = item;
            isDirty = true;

            // 스탯 재계산 (프로퍼티 접근 시 자동 수행됨)
            RecalculateIfDirty();

            Debug.Log($"[PlayerStats] {item.itemName} 장착 완료 ({item.slot})");
            return true;
        }

        /// <summary>
        /// 아이템 장착 해제
        /// </summary>
        /// <param name="slot">해제할 슬롯</param>
        /// <returns>true: 해제 성공, false: 해제 실패 (슬롯에 아이템 없음)</returns>
        public bool UnequipItem(EquipmentSlot slot)
        {
            if (!equippedItems.ContainsKey(slot))
            {
                Debug.LogWarning($"[PlayerStats] UnequipItem(): {slot} 슬롯에 장착된 아이템이 없습니다.");
                return false;
            }

            Item removedItem = equippedItems[slot];
            equippedItems.Remove(slot);
            isDirty = true;

            // 스탯 재계산
            RecalculateIfDirty();

            Debug.Log($"[PlayerStats] {removedItem.itemName} 장착 해제 완료 ({slot})");
            return true;
        }

        /// <summary>
        /// 특정 슬롯에 장착된 아이템 가져오기
        /// </summary>
        /// <param name="slot">확인할 슬롯</param>
        /// <returns>장착된 아이템 (없으면 null)</returns>
        public Item GetEquippedItem(EquipmentSlot slot)
        {
            equippedItems.TryGetValue(slot, out Item item);
            return item;
        }

        /// <summary>
        /// 모든 장착된 아이템 가져오기 (읽기 전용 복사본)
        /// </summary>
        /// <returns>장착된 아이템 딕셔너리 (읽기 전용)</returns>
        public Dictionary<EquipmentSlot, Item> GetAllEquippedItems()
        {
            return new Dictionary<EquipmentSlot, Item>(equippedItems);
        }

        /// <summary>
        /// 장착된 아이템 개수
        /// </summary>
        public int EquippedItemCount => equippedItems.Count;

        /// <summary>
        /// 모든 아이템 장착 해제
        /// </summary>
        public void UnequipAll()
        {
            equippedItems.Clear();
            isDirty = true;
            RecalculateIfDirty();

            Debug.Log("[PlayerStats] 모든 아이템 장착 해제 완료");
        }


        // ====== 디버그 ======

        /// <summary>
        /// 현재 스탯 정보 출력
        /// </summary>
        public void DebugPrintStats()
        {
            Debug.Log("========== PlayerStats ==========");
            Debug.Log($"기본 스탯: HP {baseHP}, Attack {baseAttack}, Defense {baseDefense}");
            Debug.Log($"최종 스탯: MaxHP {MaxHP}, Attack {Attack}, Defense {Defense}");
            Debug.Log($"현재 상태: CurrentHP {CurrentHP}/{MaxHP}, IsDead {IsDead}");
            Debug.Log($"장착 아이템 수: {equippedItems.Count}");

            foreach (var kvp in equippedItems)
            {
                Debug.Log($"  - {kvp.Key}: {kvp.Value.itemName}");
            }

            Debug.Log("=================================");
        }


        // ====== Combat 메서드 ======

        /// <summary>
        /// 데미지를 받습니다 (방어력 적용)
        /// </summary>
        /// <param name="incomingDamage">들어오는 데미지</param>
        public void TakeDamage(int incomingDamage)
        {
            if (isDead)
            {
                Debug.LogWarning("[PlayerStats] TakeDamage(): 이미 사망한 상태입니다.");
                return;
            }

            if (incomingDamage <= 0)
            {
                Debug.LogWarning($"[PlayerStats] TakeDamage(): 유효하지 않은 데미지입니다: {incomingDamage}");
                return;
            }

            // DamageCalculator를 사용하여 방어력 적용
            int actualDamage = DamageCalculator.CalculateDamageReceived(incomingDamage, Defense);

            // HP 감소
            int previousHP = currentHP;
            currentHP -= actualDamage;
            currentHP = Mathf.Max(currentHP, 0);

            Debug.Log($"[PlayerStats] 데미지 받음: {incomingDamage} → 방어력 {Defense} 적용 → 실제 데미지 {actualDamage} → HP {previousHP} → {currentHP}");

            // DamageNumber 표시
            if (DamageNumberPool.Instance != null)
            {
                Vector3 damagePosition = transform.position + Vector3.up * 1.5f;
                DamageNumberPool.Instance.ShowDamage(actualDamage, damagePosition, false);
            }

            // 이벤트 발생
            OnDamaged?.Invoke(actualDamage, currentHP, MaxHP);

            // 사망 체크
            if (currentHP <= 0)
            {
                Die();
            }
        }

        /// <summary>
        /// 체력을 회복합니다
        /// </summary>
        /// <param name="healAmount">회복량</param>
        public void Heal(int healAmount)
        {
            if (isDead)
            {
                Debug.LogWarning("[PlayerStats] Heal(): 사망한 상태에서는 회복할 수 없습니다.");
                return;
            }

            if (healAmount <= 0)
            {
                Debug.LogWarning($"[PlayerStats] Heal(): 유효하지 않은 회복량입니다: {healAmount}");
                return;
            }

            int previousHP = currentHP;
            currentHP += healAmount;
            currentHP = Mathf.Min(currentHP, MaxHP);

            int actualHealed = currentHP - previousHP;

            Debug.Log($"[PlayerStats] 체력 회복: {actualHealed} (HP {previousHP} → {currentHP})");

            // DamageNumber 표시 (회복)
            if (DamageNumberPool.Instance != null && actualHealed > 0)
            {
                Vector3 healPosition = transform.position + Vector3.up * 1.5f;
                DamageNumberPool.Instance.ShowHeal(actualHealed, healPosition);
            }

            // 이벤트 발생
            OnHealed?.Invoke(actualHealed, currentHP, MaxHP);
        }

        /// <summary>
        /// 적에게 데미지를 입힙니다
        /// </summary>
        /// <param name="target">공격할 적</param>
        public void DealDamageTo(Enemy target)
        {
            if (target == null)
            {
                Debug.LogWarning("[PlayerStats] DealDamageTo(): target이 null입니다.");
                return;
            }

            if (isDead)
            {
                Debug.LogWarning("[PlayerStats] DealDamageTo(): 사망한 상태에서는 공격할 수 없습니다.");
                return;
            }

            if (target.IsDead)
            {
                Debug.LogWarning($"[PlayerStats] DealDamageTo(): {target.name}은(는) 이미 사망했습니다.");
                return;
            }

            // DamageCalculator를 사용하여 데미지 계산
            int damage = DamageCalculator.CalculateDamageDealt(Attack);

            Debug.Log($"[PlayerStats] {target.name}을(를) 공격! 공격력 {Attack} → 데미지 {damage}");

            // 적에게 데미지 적용
            target.TakeDamage(damage);
        }

        /// <summary>
        /// 플레이어 사망 처리
        /// </summary>
        private void Die()
        {
            if (isDead)
            {
                return;
            }

            isDead = true;
            currentHP = 0;

            Debug.Log("[PlayerStats] 플레이어 사망!");

            // 이벤트 발생
            OnDeath?.Invoke();

            // GameFlow 상태 전환 (GameOver로)
            if (GameFlowStateMachine.HasInstance)
            {
                GameFlowStateMachine.Instance.TriggerPlayerDied();
            }
        }

        /// <summary>
        /// 플레이어를 부활시킵니다 (테스트용)
        /// </summary>
        public void Revive()
        {
            isDead = false;
            int oldHP = currentHP;
            currentHP = MaxHP;

            Debug.Log($"[PlayerStats] 부활! HP {currentHP}/{MaxHP}");

            // 회복 이벤트 발생 (UI 업데이트용)
            int healAmount = currentHP - oldHP;
            OnHealed?.Invoke(healAmount, currentHP, MaxHP);
        }

        /// <summary>
        /// RunData로부터 플레이어 초기화 (씬 로드 후)
        /// RunManager.SyncToPlayer()에서 호출됨
        /// </summary>
        public void InitializeFromRunData(PlayerRunData data)
        {
            if (data == null)
            {
                Debug.LogWarning("[PlayerStats] InitializeFromRunData: data가 null입니다.");
                return;
            }

            // 이전 값 저장
            int oldHP = currentHP;
            int oldMana = currentMana;

            // 기본 스탯 설정
            baseHP = data.maxHP;
            baseAttack = data.baseAttack;
            baseDefense = data.baseDefense;
            baseMana = data.maxMana;

            // 현재 상태 복원
            currentHP = data.currentHP;
            currentMana = data.currentMana;
            isDead = currentHP <= 0;

            // 스탯 재계산
            isDirty = true;
            RecalculateIfDirty();

            Debug.Log($"[PlayerStats] RunData로 초기화 완료 - HP: {currentHP}/{MaxHP}, Mana: {currentMana}/{MaxMana}, Attack: {Attack}, Defense: {Defense}");

            // 이벤트 발생
            OnStatsChanged?.Invoke(StatType.HP, oldHP, currentHP);
            OnStatsChanged?.Invoke(StatType.Mana, oldMana, currentMana);
        }

        /// <summary>
        /// 현재 상태를 RunData로 변환 (씬 전환 전)
        /// RunManager.SyncFromPlayer()에서 호출됨
        /// </summary>
        public PlayerRunData ToRunData()
        {
            return new PlayerRunData
            {
                maxHP = baseHP,
                currentHP = currentHP,
                maxMana = baseMana,
                currentMana = currentMana,
                baseAttack = baseAttack,
                baseDefense = baseDefense,
                level = 1, // TODO: PlayerLevel 연동
                currentExp = 0
            };
        }

        /// <summary>
        /// 런 시작 시 플레이어를 초기 상태로 리셋
        /// GameManager.StartNewRun()에서 호출됨
        /// </summary>
        public void ResetToBaseStats()
        {
            // 모든 장비 해제
            UnequipAll();

            // 이전 값 저장
            int oldHP = currentHP;
            int oldMana = currentMana;

            // HP/마나 최대로 회복
            isDead = false;
            currentHP = MaxHP;
            currentMana = MaxMana;

            // 상태 효과 클리어 (있다면)
            // ClearAllStatusEffects();

            Debug.Log($"[PlayerStats] 기본 스탯으로 리셋 - HP: {currentHP}/{MaxHP}, Mana: {currentMana}/{MaxMana}");

            // OnStatsChanged로 통일된 이벤트 발생
            OnStatsChanged?.Invoke(StatType.HP, oldHP, currentHP);
            OnStatsChanged?.Invoke(StatType.Mana, oldMana, currentMana);
        }


        // ====== Mana 관리 ======

        /// <summary>
        /// 마나를 소비합니다
        /// </summary>
        /// <param name="amount">소비할 마나량</param>
        /// <returns>true: 소비 성공, false: 마나 부족</returns>
        public bool TrySpendMana(int amount)
        {
            if (amount < 0)
            {
                Debug.LogWarning($"[PlayerStats] TrySpendMana(): 유효하지 않은 값입니다: {amount}");
                return false;
            }

            if (amount == 0)
            {
                return true; // 0 마나는 항상 성공
            }

            if (currentMana < amount)
            {
                Debug.LogWarning($"[PlayerStats] TrySpendMana(): 마나 부족 (필요: {amount}, 현재: {currentMana})");
                return false;
            }

            int previousMana = currentMana;
            currentMana -= amount;

            Debug.Log($"[PlayerStats] 마나 소비: {amount} (Mana {previousMana} → {currentMana})");

            // 이벤트 발생
            OnStatsChanged?.Invoke(StatType.Mana, previousMana, currentMana);

            return true;
        }

        /// <summary>
        /// 마나를 회복합니다
        /// </summary>
        /// <param name="amount">회복할 마나량</param>
        public void RegenerateMana(int amount)
        {
            if (amount <= 0)
            {
                Debug.LogWarning($"[PlayerStats] RegenerateMana(): 유효하지 않은 회복량입니다: {amount}");
                return;
            }

            int previousMana = currentMana;
            currentMana += amount;
            currentMana = Mathf.Min(currentMana, MaxMana);

            int actualRegenerated = currentMana - previousMana;

            Debug.Log($"[PlayerStats] 마나 회복: {actualRegenerated} (Mana {previousMana} → {currentMana})");

            // 이벤트 발생
            OnStatsChanged?.Invoke(StatType.Mana, previousMana, currentMana);
        }


        // ====== Context Menu (테스트용) ======

        [ContextMenu("Take 10 Damage (Test)")]
        private void TestTakeDamage()
        {
            TakeDamage(10);
        }

        [ContextMenu("Heal 20 HP (Test)")]
        private void TestHeal()
        {
            Heal(20);
        }

        [ContextMenu("Revive (Test)")]
        private void TestRevive()
        {
            Revive();
        }

        [ContextMenu("Print Combat Info")]
        private void PrintCombatInfo()
        {
            Debug.Log("========== Combat Info ==========");
            Debug.Log($"CurrentHP: {CurrentHP}/{MaxHP}");
            Debug.Log($"Attack: {Attack}");
            Debug.Log($"Defense: {Defense}");
            Debug.Log($"IsDead: {IsDead}");
            Debug.Log($"\n{DamageCalculator.GetFormulaInfo()}");
            Debug.Log("=================================");
        }

        [ContextMenu("Print Stats Info")]
        private void PrintStatsInfo()
        {
            DebugPrintStats();
        }

        [ContextMenu("Spend 20 Mana (Test)")]
        private void TestSpendMana()
        {
            TrySpendMana(20);
        }

        [ContextMenu("Regenerate 30 Mana (Test)")]
        private void TestRegenerateMana()
        {
            RegenerateMana(30);
        }

        [ContextMenu("Print Mana Info")]
        private void PrintManaInfo()
        {
            Debug.Log("========== Mana Info ==========");
            Debug.Log($"CurrentMana: {CurrentMana}/{MaxMana}");
            Debug.Log($"===============================");
        }

        // ====== BuffIcon 테스트용 ======

        [Header("BuffIcon Test")]
        [SerializeField] [Tooltip("테스트용 StatusEffectData 배열")]
        private StatusEffectData[] testEffects;

        [ContextMenu("Test: Apply Attack Buff (10s)")]
        private void TestApplyAttackBuff()
        {
            if (!StatusEffectManager.HasInstance)
            {
                Debug.LogError("[PlayerStats] StatusEffectManager가 없습니다!");
                return;
            }

            // StatusEffectData를 코드로 생성 (ScriptableObject.CreateInstance 사용)
            var effectData = ScriptableObject.CreateInstance<StatusEffectData>();
            effectData.effectType = StatusEffectType.AttackUp;
            effectData.displayName = "공격력 증가";
            effectData.description = "공격력이 10 증가합니다";
            effectData.value = 10f;
            effectData.duration = 10f;
            effectData.tickInterval = 0f;
            effectData.maxStack = 3;
            effectData.isBuff = true;

            StatusEffectManager.Instance.ApplyEffect(gameObject, effectData);
            Debug.Log("[PlayerStats] 공격력 버프 적용! (10초간 유지)");
        }

        [ContextMenu("Test: Apply Defense Buff (15s)")]
        private void TestApplyDefenseBuff()
        {
            if (!StatusEffectManager.HasInstance)
            {
                Debug.LogError("[PlayerStats] StatusEffectManager가 없습니다!");
                return;
            }

            var effectData = ScriptableObject.CreateInstance<StatusEffectData>();
            effectData.effectType = StatusEffectType.DefenseUp;
            effectData.displayName = "방어력 증가";
            effectData.description = "방어력이 5 증가합니다";
            effectData.value = 5f;
            effectData.duration = 15f;
            effectData.tickInterval = 0f;
            effectData.maxStack = 2;
            effectData.isBuff = true;

            StatusEffectManager.Instance.ApplyEffect(gameObject, effectData);
            Debug.Log("[PlayerStats] 방어력 버프 적용! (15초간 유지)");
        }

        [ContextMenu("Test: Apply Speed Buff (20s)")]
        private void TestApplySpeedBuff()
        {
            if (!StatusEffectManager.HasInstance)
            {
                Debug.LogError("[PlayerStats] StatusEffectManager가 없습니다!");
                return;
            }

            var effectData = ScriptableObject.CreateInstance<StatusEffectData>();
            effectData.effectType = StatusEffectType.SpeedUp;
            effectData.displayName = "이동속도 증가";
            effectData.description = "이동속도가 50% 증가합니다";
            effectData.value = 0.5f;
            effectData.duration = 20f;
            effectData.tickInterval = 0f;
            effectData.maxStack = 1;
            effectData.isBuff = true;

            StatusEffectManager.Instance.ApplyEffect(gameObject, effectData);
            Debug.Log("[PlayerStats] 이동속도 버프 적용! (20초간 유지)");
        }

        [ContextMenu("Test: Apply Poison Debuff (DoT)")]
        private void TestApplyPoisonDebuff()
        {
            if (!StatusEffectManager.HasInstance)
            {
                Debug.LogError("[PlayerStats] StatusEffectManager가 없습니다!");
                return;
            }

            var effectData = ScriptableObject.CreateInstance<StatusEffectData>();
            effectData.effectType = StatusEffectType.Poison;
            effectData.displayName = "독";
            effectData.description = "1초마다 5 데미지";
            effectData.value = 5f;
            effectData.duration = 10f;
            effectData.tickInterval = 1f;  // 1초마다 틱
            effectData.maxStack = 1;
            effectData.isBuff = false;  // 디버프

            StatusEffectManager.Instance.ApplyEffect(gameObject, effectData);
            Debug.Log("[PlayerStats] 독 디버프 적용! (10초간, 1초마다 5 데미지)");
        }

        [ContextMenu("Test: Stack Attack Buff x3")]
        private void TestStackAttackBuff()
        {
            if (!StatusEffectManager.HasInstance)
            {
                Debug.LogError("[PlayerStats] StatusEffectManager가 없습니다!");
                return;
            }

            // 3번 적용하여 스택 테스트
            for (int i = 0; i < 3; i++)
            {
                var effectData = ScriptableObject.CreateInstance<StatusEffectData>();
                effectData.effectType = StatusEffectType.AttackUp;
                effectData.displayName = "공격력 증가";
                effectData.description = "공격력이 10 증가합니다";
                effectData.value = 10f;
                effectData.duration = 10f;
                effectData.tickInterval = 0f;
                effectData.maxStack = 3;
                effectData.isBuff = true;

                StatusEffectManager.Instance.ApplyEffect(gameObject, effectData);
            }

            Debug.Log("[PlayerStats] 공격력 버프 3스택 적용!");
        }

        [ContextMenu("Test: Apply From Inspector Array")]
        private void TestApplyFromInspectorArray()
        {
            if (!StatusEffectManager.HasInstance)
            {
                Debug.LogError("[PlayerStats] StatusEffectManager가 없습니다!");
                return;
            }

            if (testEffects == null || testEffects.Length == 0)
            {
                Debug.LogWarning("[PlayerStats] testEffects 배열이 비어있습니다! Inspector에서 StatusEffectData를 할당하세요.");
                return;
            }

            foreach (var effectData in testEffects)
            {
                if (effectData != null)
                {
                    StatusEffectManager.Instance.ApplyEffect(gameObject, effectData);
                    Debug.Log($"[PlayerStats] {effectData.displayName} 적용!");
                }
            }
        }

        [ContextMenu("Test: Clear All Buffs")]
        private void TestClearAllBuffs()
        {
            if (!StatusEffectManager.HasInstance)
            {
                Debug.LogError("[PlayerStats] StatusEffectManager가 없습니다!");
                return;
            }

            StatusEffectManager.Instance.RemoveAllEffects(gameObject);
            Debug.Log("[PlayerStats] 모든 버프/디버프 제거!");
        }


        // ====== StatusEffect 통합 ======

        /// <summary>
        /// StatusEffect 이벤트 구독
        /// </summary>
        private void SubscribeToStatusEffectEvents()
        {
            // Instance 호출로 StatusEffectManager가 없으면 자동 생성
            StatusEffectManager manager = StatusEffectManager.Instance;

            if (manager != null)
            {
                // 중복 구독 방지를 위해 먼저 구독 해제
                manager.OnEffectApplied -= OnEffectApplied;
                manager.OnEffectRemoved -= OnEffectRemoved;

                // 구독
                manager.OnEffectApplied += OnEffectApplied;
                manager.OnEffectRemoved += OnEffectRemoved;

                Debug.Log("[PlayerStats] StatusEffectManager 이벤트 구독 완료");
            }
            else
            {
                Debug.LogError("[PlayerStats] StatusEffectManager를 찾을 수 없습니다.");
            }
        }

        /// <summary>
        /// StatusEffect 이벤트 구독 해제
        /// </summary>
        private void UnsubscribeFromStatusEffectEvents()
        {
            if (StatusEffectManager.HasInstance)
            {
                StatusEffectManager.Instance.OnEffectApplied -= OnEffectApplied;
                StatusEffectManager.Instance.OnEffectRemoved -= OnEffectRemoved;

                Debug.Log("[PlayerStats] StatusEffectManager 이벤트 구독 해제");
            }
        }

        /// <summary>
        /// 효과 적용 시 호출
        /// </summary>
        private void OnEffectApplied(GameObject target, StatusEffect effect)
        {
            if (target != gameObject) return;

            // DoT 효과 처리
            if (effect.TickInterval > 0f)
            {
                effect.OnTick += OnStatusEffectTick;
            }

            Debug.Log($"[PlayerStats] StatusEffect 적용: {effect.DisplayName}");

            // 공격력/방어력 관련 효과면 OnStatChanged 이벤트 발생
            TriggerStatChangedForEffect(effect);
        }

        /// <summary>
        /// 효과 제거 시 호출
        /// </summary>
        private void OnEffectRemoved(GameObject target, StatusEffect effect)
        {
            if (target != gameObject) return;

            // DoT 이벤트 구독 해제
            if (effect.TickInterval > 0f)
            {
                effect.OnTick -= OnStatusEffectTick;
            }

            Debug.Log($"[PlayerStats] StatusEffect 제거: {effect.DisplayName}");

            // 공격력/방어력 관련 효과면 OnStatChanged 이벤트 발생
            TriggerStatChangedForEffect(effect);
        }

        /// <summary>
        /// StatusEffect에 따라 OnStatChanged 이벤트 발생
        /// </summary>
        private void TriggerStatChangedForEffect(StatusEffect effect)
        {
            if (effect.EffectType == StatusEffectType.AttackUp ||
                effect.EffectType == StatusEffectType.AttackDown)
            {
                // 공격력 변경 이벤트 발생
                int currentAttack = Attack;
                OnStatsChanged?.Invoke(StatType.Attack, currentAttack, currentAttack);
            }
            else if (effect.EffectType == StatusEffectType.DefenseUp ||
                     effect.EffectType == StatusEffectType.DefenseDown)
            {
                // 방어력 변경 이벤트 발생
                int currentDefense = Defense;
                OnStatsChanged?.Invoke(StatType.Defense, currentDefense, currentDefense);
            }
        }

        /// <summary>
        /// StatusEffect 틱 발생 시 호출 (DoT/Regeneration)
        /// </summary>
        private void OnStatusEffectTick(StatusEffect effect, float tickValue)
        {
            if (effect.Target != gameObject) return;

            // Regeneration (회복)
            if (effect.EffectType == StatusEffectType.Regeneration)
            {
                Heal(Mathf.RoundToInt(tickValue));
            }
            // Poison, Burn, Bleed (지속 데미지)
            else if (effect.EffectType == StatusEffectType.Poison ||
                     effect.EffectType == StatusEffectType.Burn ||
                     effect.EffectType == StatusEffectType.Bleed)
            {
                // DoT는 방어력 무시
                int damage = Mathf.RoundToInt(Mathf.Abs(tickValue));
                int previousHP = currentHP;
                currentHP -= damage;
                currentHP = Mathf.Max(currentHP, 0);

                Debug.Log($"[PlayerStats] {effect.DisplayName} 틱 데미지: {damage} (HP {previousHP} → {currentHP})");

                // DamageNumber 표시
                if (DamageNumberPool.Instance != null)
                {
                    Vector3 damagePosition = transform.position + Vector3.up * 1.5f;
                    DamageNumberPool.Instance.ShowDamage(damage, damagePosition, false);
                }

                // 이벤트 발생
                OnDamaged?.Invoke(damage, currentHP, MaxHP);

                // 사망 체크
                if (currentHP <= 0)
                {
                    Die();
                }
            }
        }

        /// <summary>
        /// StatusEffect 버프/디버프 적용
        /// </summary>
        /// <param name="baseStat">기본 스탯 값</param>
        /// <param name="buffType">버프 타입</param>
        /// <param name="debuffType">디버프 타입</param>
        /// <returns>버프/디버프 적용된 최종 값</returns>
        private int ApplyStatusEffects(int baseStat, StatusEffectType buffType, StatusEffectType debuffType)
        {
            if (!StatusEffectManager.HasInstance)
            {
                return baseStat;
            }

            float modifier = 0f;

            // 버프 합산
            StatusEffect buffEffect = StatusEffectManager.Instance.GetEffect(gameObject, buffType);
            if (buffEffect != null && buffEffect.IsActive)
            {
                modifier += buffEffect.Value * buffEffect.StackCount;
            }

            // 디버프 합산
            StatusEffect debuffEffect = StatusEffectManager.Instance.GetEffect(gameObject, debuffType);
            if (debuffEffect != null && debuffEffect.IsActive)
            {
                modifier -= debuffEffect.Value * debuffEffect.StackCount;
            }

            int finalValue = baseStat + Mathf.RoundToInt(modifier);
            return Mathf.Max(finalValue, 1); // 최소 1 보장
        }


        // ====== ISaveable 인터페이스 구현 ======

        /// <summary>
        /// ISaveable 인터페이스: 저장 가능 객체 고유 ID
        /// </summary>
        public string SaveID => "PlayerStats";

        /// <summary>
        /// ISaveable.GetSaveData() 명시적 구현
        /// 내부적으로 구체적 타입의 GetSaveData()를 호출합니다
        /// </summary>
        object ISaveable.GetSaveData()
        {
            return GetSaveData();
        }

        /// <summary>
        /// ISaveable.LoadFromSaveData(object) 명시적 구현
        /// 타입 검증 후 구체적 타입의 LoadFromSaveData()를 호출합니다
        /// </summary>
        void ISaveable.LoadFromSaveData(object data)
        {
            if (data is PlayerStatsData statsData)
            {
                LoadFromSaveData(statsData);
            }
            else
            {
                Debug.LogError($"[PlayerStats] ISaveable.LoadFromSaveData(): 잘못된 데이터 타입입니다. Expected: PlayerStatsData, Got: {data?.GetType().Name}");
            }
        }


        // ====== Save/Load (기존 방식) ======

        /// <summary>
        /// 현재 플레이어 스탯 데이터를 저장용 구조로 반환합니다
        /// </summary>
        public PlayerStatsData GetSaveData()
        {
            PlayerStatsData data = new PlayerStatsData();

            // 현재 HP 저장
            data.currentHP = currentHP;

            // 장착된 아이템 저장
            data.equippedItems = new List<EquippedItemEntry>();

            foreach (var kvp in equippedItems)
            {
                if (kvp.Value != null)
                {
                    // ScriptableObject의 에셋 경로 저장
#if UNITY_EDITOR
                    string assetPath = UnityEditor.AssetDatabase.GetAssetPath(kvp.Value);
                    data.equippedItems.Add(new EquippedItemEntry(kvp.Key, assetPath));
#else
                    Debug.LogWarning($"[PlayerStats] GetSaveData(): 빌드 환경에서는 아이템 저장이 지원되지 않습니다. 슬롯: {kvp.Key}");
#endif
                }
            }

            Debug.Log($"[PlayerStats] GetSaveData(): CurrentHP={data.currentHP}, EquippedItems={data.equippedItems.Count}");

            return data;
        }

        /// <summary>
        /// 저장된 데이터로부터 플레이어 스탯을 복원합니다
        /// </summary>
        public void LoadFromSaveData(PlayerStatsData data)
        {
            if (data == null)
            {
                Debug.LogError("[PlayerStats] LoadFromSaveData(): data가 null입니다.");
                return;
            }

            // 현재 HP 복원
            currentHP = data.currentHP;
            currentHP = Mathf.Clamp(currentHP, 0, MaxHP);

            Debug.Log($"[PlayerStats] LoadFromSaveData(): CurrentHP={currentHP}");

            // 모든 아이템 장착 해제
            equippedItems.Clear();
            isDirty = true;

            // 장착된 아이템 복원
            if (data.equippedItems != null)
            {
                foreach (var entry in data.equippedItems)
                {
                    if (string.IsNullOrEmpty(entry.itemPath))
                    {
                        Debug.LogWarning($"[PlayerStats] LoadFromSaveData(): 빈 아이템 경로입니다. 슬롯: {entry.slot}");
                        continue;
                    }

#if UNITY_EDITOR
                    // 에셋 경로로부터 아이템 로드
                    Item item = UnityEditor.AssetDatabase.LoadAssetAtPath<Item>(entry.itemPath);

                    if (item != null)
                    {
                        equippedItems[entry.slot] = item;
                        Debug.Log($"[PlayerStats] LoadFromSaveData(): 아이템 복원 - 슬롯: {entry.slot}, 아이템: {item.itemName}");
                    }
                    else
                    {
                        Debug.LogWarning($"[PlayerStats] LoadFromSaveData(): 아이템을 찾을 수 없습니다. 경로: {entry.itemPath}");
                    }
#else
                    Debug.LogWarning($"[PlayerStats] LoadFromSaveData(): 빌드 환경에서는 아이템 불러오기가 지원되지 않습니다. 슬롯: {entry.slot}");
#endif
                }
            }

            // 스탯 재계산
            isDirty = true;
            RecalculateIfDirty();

            Debug.Log($"[PlayerStats] LoadFromSaveData() 완료: HP={currentHP}/{MaxHP}, Attack={Attack}, Defense={Defense}");
        }
    }
}
