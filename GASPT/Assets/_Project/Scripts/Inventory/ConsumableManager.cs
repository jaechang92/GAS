using System;
using UnityEngine;
using GASPT.Data;
using GASPT.Core;
using GASPT.Core.Enums;
using GASPT.Core.Utilities;
using GASPT.Stats;
using GASPT.StatusEffects;

namespace GASPT.Inventory
{
    /// <summary>
    /// 소비 아이템 매니저
    /// 아이템 사용, 쿨다운 관리
    /// </summary>
    public class ConsumableManager : SingletonManager<ConsumableManager>
    {
        // ====== 쿨다운 데이터 ======

        /// <summary>
        /// 아이템별 쿨다운 트래커
        /// </summary>
        private readonly CooldownTracker<string> cooldownTracker = new CooldownTracker<string>();


        // ====== 이벤트 ======

        /// <summary>
        /// 아이템 사용 이벤트
        /// 매개변수: (아이템 인스턴스, 사용 결과)
        /// </summary>
        public event Action<ItemInstance, UseResult> OnItemUsed;

        /// <summary>
        /// 쿨다운 시작 이벤트 (CooldownTracker에서 전달)
        /// 매개변수: (아이템 ID, 쿨다운 시간)
        /// </summary>
        public event Action<string, float> OnCooldownStarted;

        /// <summary>
        /// 쿨다운 종료 이벤트 (CooldownTracker에서 전달)
        /// 매개변수: (아이템 ID)
        /// </summary>
        public event Action<string> OnCooldownEnded;


        // ====== Unity 생명주기 ======

        protected override void OnAwake()
        {
            // CooldownTracker 이벤트 연결
            cooldownTracker.OnCooldownStarted += (id, duration) => OnCooldownStarted?.Invoke(id, duration);
            cooldownTracker.OnCooldownEnded += (id) => OnCooldownEnded?.Invoke(id);

            Debug.Log("[ConsumableManager] 초기화 완료");
        }

        private void Update()
        {
            // 쿨다운 만료 체크
            cooldownTracker.Tick();
        }


        // ====== 아이템 사용 ======

        /// <summary>
        /// 아이템 사용
        /// </summary>
        /// <param name="itemInstance">사용할 아이템</param>
        /// <param name="target">효과 대상 (PlayerStats)</param>
        /// <returns>사용 결과</returns>
        public UseResult UseItem(ItemInstance itemInstance, PlayerStats target)
        {
            if (itemInstance == null || !itemInstance.IsValid)
            {
                return UseResult.InvalidItem;
            }

            ConsumableData consumableData = itemInstance.ConsumableData;

            if (consumableData == null)
            {
                return UseResult.InvalidItem;
            }

            if (target == null)
            {
                return UseResult.NoTarget;
            }

            // 쿨다운 체크
            if (IsOnCooldown(consumableData.itemId))
            {
                return UseResult.OnCooldown;
            }

            // 효과 적용
            UseResult result = ApplyEffect(consumableData, target);

            if (result == UseResult.Success)
            {
                // 쿨다운 시작
                if (consumableData.HasCooldown)
                {
                    StartCooldown(consumableData.itemId, consumableData.cooldown);
                }

                // 인벤토리에서 아이템 소비
                if (InventoryManager.HasInstance)
                {
                    InventoryManager.Instance.RemoveItemInstance(itemInstance, 1);
                }

                Debug.Log($"[ConsumableManager] 아이템 사용: {consumableData.itemName}");
            }

            OnItemUsed?.Invoke(itemInstance, result);

            return result;
        }

        /// <summary>
        /// 슬롯 인덱스로 아이템 사용
        /// </summary>
        /// <param name="slotIndex">인벤토리 슬롯 인덱스</param>
        /// <param name="target">효과 대상</param>
        /// <returns>사용 결과</returns>
        public UseResult UseItemFromSlot(int slotIndex, PlayerStats target)
        {
            if (!InventoryManager.HasInstance)
            {
                return UseResult.InvalidItem;
            }

            InventorySlot slot = InventoryManager.Instance.GetSlot(slotIndex);

            if (slot == null || slot.IsEmpty)
            {
                return UseResult.InvalidItem;
            }

            return UseItem(slot.Item, target);
        }


        // ====== 효과 적용 ======

        /// <summary>
        /// 소비 아이템 효과 적용
        /// </summary>
        private UseResult ApplyEffect(ConsumableData consumableData, PlayerStats target)
        {
            switch (consumableData.consumeType)
            {
                case ConsumeType.Heal:
                    return ApplyHeal(consumableData, target);

                case ConsumeType.HealOverTime:
                    return ApplyHealOverTime(consumableData, target);

                case ConsumeType.RestoreMana:
                    return ApplyManaRestore(consumableData, target);

                case ConsumeType.Buff:
                    return ApplyBuff(consumableData, target);

                case ConsumeType.Cleanse:
                    return ApplyCleanse(consumableData, target);

                case ConsumeType.Revive:
                    return ApplyRevive(consumableData, target);

                case ConsumeType.Teleport:
                    // 텔레포트는 별도 시스템 필요
                    return UseResult.Success;

                default:
                    return UseResult.InvalidItem;
            }
        }

        /// <summary>
        /// 즉시 회복
        /// </summary>
        private UseResult ApplyHeal(ConsumableData data, PlayerStats target)
        {
            if (target.CurrentHP >= target.MaxHP)
            {
                return UseResult.AlreadyFull;
            }

            int healAmount = Mathf.RoundToInt(data.effectValue);
            target.Heal(healAmount);

            return UseResult.Success;
        }

        /// <summary>
        /// 지속 회복 (HoT)
        /// </summary>
        private UseResult ApplyHealOverTime(ConsumableData data, PlayerStats target)
        {
            if (!StatusEffectManager.HasInstance)
            {
                // StatusEffectManager 없으면 즉시 회복으로 대체
                return ApplyHeal(data, target);
            }

            // Regeneration 효과 생성
            StatusEffectData effectData = ScriptableObject.CreateInstance<StatusEffectData>();
            effectData.effectType = StatusEffectType.Regeneration;
            effectData.displayName = $"{data.itemName} 효과";
            effectData.description = $"초당 {data.effectValue / data.effectDuration} HP 회복";
            effectData.value = data.effectValue / data.effectDuration;
            effectData.duration = data.effectDuration;
            effectData.tickInterval = 1f;
            effectData.maxStack = 1;
            effectData.isBuff = true;

            StatusEffectManager.Instance.ApplyEffect(target.gameObject, effectData);

            return UseResult.Success;
        }

        /// <summary>
        /// 마나 회복
        /// </summary>
        private UseResult ApplyManaRestore(ConsumableData data, PlayerStats target)
        {
            if (target.CurrentMana >= target.MaxMana)
            {
                return UseResult.AlreadyFull;
            }

            int manaAmount = Mathf.RoundToInt(data.effectValue);
            target.RegenerateMana(manaAmount);

            return UseResult.Success;
        }

        /// <summary>
        /// 버프 적용
        /// </summary>
        private UseResult ApplyBuff(ConsumableData data, PlayerStats target)
        {
            if (data.buffEffect == null)
            {
                Debug.LogWarning($"[ConsumableManager] {data.itemName}: buffEffect가 설정되지 않음");
                return UseResult.InvalidItem;
            }

            if (!StatusEffectManager.HasInstance)
            {
                return UseResult.NoTarget;
            }

            StatusEffectManager.Instance.ApplyEffect(target.gameObject, data.buffEffect);

            return UseResult.Success;
        }

        /// <summary>
        /// 상태이상 해제
        /// </summary>
        private UseResult ApplyCleanse(ConsumableData data, PlayerStats target)
        {
            if (!StatusEffectManager.HasInstance)
            {
                return UseResult.NoTarget;
            }

            // 모든 디버프 제거
            StatusEffectManager.Instance.RemoveAllDebuffs(target.gameObject);

            return UseResult.Success;
        }

        /// <summary>
        /// 부활
        /// </summary>
        private UseResult ApplyRevive(ConsumableData data, PlayerStats target)
        {
            if (!target.IsDead)
            {
                return UseResult.AlreadyFull;
            }

            target.Revive();

            // 추가 회복이 있으면 적용
            if (data.effectValue > 0)
            {
                int healAmount = Mathf.RoundToInt(data.effectValue);
                target.Heal(healAmount);
            }

            return UseResult.Success;
        }


        // ====== 쿨다운 시스템 (CooldownTracker 위임) ======

        /// <summary>
        /// 쿨다운 시작
        /// </summary>
        private void StartCooldown(string itemId, float duration)
        {
            cooldownTracker.Start(itemId, duration);
            Debug.Log($"[ConsumableManager] 쿨다운 시작: {itemId} ({duration}초)");
        }

        /// <summary>
        /// 쿨다운 중인지 확인
        /// </summary>
        public bool IsOnCooldown(string itemId)
        {
            return cooldownTracker.IsOnCooldown(itemId);
        }

        /// <summary>
        /// 남은 쿨다운 시간
        /// </summary>
        public float GetRemainingCooldown(string itemId)
        {
            return cooldownTracker.GetRemainingTime(itemId);
        }

        /// <summary>
        /// 쿨다운 강제 해제
        /// </summary>
        public void ClearCooldown(string itemId)
        {
            cooldownTracker.Reset(itemId);
        }

        /// <summary>
        /// 모든 쿨다운 해제
        /// </summary>
        public void ClearAllCooldowns()
        {
            cooldownTracker.ResetAll();
        }


        // ====== 디버그 ======

        /// <summary>
        /// 쿨다운 상태 출력
        /// </summary>
        [ContextMenu("Print Cooldowns")]
        public void DebugPrintCooldowns()
        {
            Debug.Log($"[ConsumableManager] ========== 쿨다운 ({cooldownTracker.ActiveCount}개) ==========");

            foreach (var kvp in cooldownTracker.GetAllCooldowns())
            {
                Debug.Log($"[ConsumableManager] {kvp.Key}: {kvp.Value:F1}초 남음");
            }

            Debug.Log("[ConsumableManager] ============================");
        }
    }
}
