using System;
using System.Collections.Generic;
using Core.Enums;
using GASPT.Data;
using GASPT.Combat;
using GASPT.Save;
using UnityEngine;
using GASPT.Enemies;

namespace GASPT.Stats
{
    /// <summary>
    /// 플레이어 스탯 관리 MonoBehaviour
    /// 기본 스탯 + 장비 보너스 = 최종 스탯
    /// Dirty flag 최적화로 변경 시에만 재계산 (<50ms)
    /// </summary>
    public class PlayerStats : MonoBehaviour
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


        // ====== 장비 슬롯 ======

        /// <summary>
        /// 장비된 아이템 (슬롯 → 아이템)
        /// </summary>
        private Dictionary<EquipmentSlot, Item> equippedItems = new Dictionary<EquipmentSlot, Item>();


        // ====== 최종 스탯 (캐시) ======

        private int finalHP;
        private int finalAttack;
        private int finalDefense;

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
        /// 최종 공격력 (기본 + 장비 보너스)
        /// </summary>
        public int Attack
        {
            get
            {
                RecalculateIfDirty();
                return finalAttack;
            }
        }

        /// <summary>
        /// 최종 방어력 (기본 + 장비 보너스)
        /// </summary>
        public int Defense
        {
            get
            {
                RecalculateIfDirty();
                return finalDefense;
            }
        }

        /// <summary>
        /// 사망 여부
        /// </summary>
        public bool IsDead => isDead;


        // ====== 이벤트 ======

        /// <summary>
        /// 스탯 변경 시 발생하는 이벤트
        /// 매개변수: (StatType, 이전 값, 새 값)
        /// </summary>
        public event Action<StatType, int, int> OnStatChanged;

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
            isDead = false;

            Debug.Log($"[PlayerStats] 초기화 완료 - MaxHP: {MaxHP}, CurrentHP: {CurrentHP}, Attack: {Attack}, Defense: {Defense}");
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

            // 기본 스탯으로 초기화
            finalHP = baseHP;
            finalAttack = baseAttack;
            finalDefense = baseDefense;

            // 장비 보너스 합산
            foreach (var item in equippedItems.Values)
            {
                if (item != null)
                {
                    finalHP += item.hpBonus;
                    finalAttack += item.attackBonus;
                    finalDefense += item.defenseBonus;
                }
            }

            isDirty = false;

            // 변경된 스탯에 대해 이벤트 발생
            NotifyStatChangedIfDifferent(StatType.HP, oldHP, finalHP);
            NotifyStatChangedIfDifferent(StatType.Attack, oldAttack, finalAttack);
            NotifyStatChangedIfDifferent(StatType.Defense, oldDefense, finalDefense);

            Debug.Log($"[PlayerStats] 스탯 재계산 완료 - HP: {finalHP}, Attack: {finalAttack}, Defense: {finalDefense}");
        }

        /// <summary>
        /// 값이 변경된 경우에만 이벤트 발생
        /// </summary>
        private void NotifyStatChangedIfDifferent(StatType statType, int oldValue, int newValue)
        {
            if (oldValue != newValue)
            {
                OnStatChanged?.Invoke(statType, oldValue, newValue);
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


        // ====== Save/Load ======

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
