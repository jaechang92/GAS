using GASPT.Combat;
using GASPT.Core.Enums;
using GASPT.Data;
using GASPT.StatusEffects;
using UnityEngine;

namespace GASPT.Stats
{
    /// <summary>
    /// PlayerStats 디버그 및 테스트 관련 partial class
    /// </summary>
    public partial class PlayerStats
    {
        // ====== BuffIcon 테스트용 ======

        [Header("BuffIcon Test")]
        [SerializeField] [Tooltip("테스트용 StatusEffectData 배열")]
        private StatusEffectData[] testEffects;


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
    }
}
