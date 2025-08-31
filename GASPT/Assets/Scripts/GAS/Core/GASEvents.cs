// ���� ��ġ: Assets/Scripts/GAS/Core/GASEvents.cs
using GAS.AttributeSystem;
using GAS.EffectSystem;
using System;
using UnityEngine;
using static GAS.Core.GASConstants;

namespace GAS.Core
{
    /// <summary>
    /// GAS �ý��� ��ü���� ���Ǵ� �߾� �̺�Ʈ �ý���
    /// </summary>
    public static class GASEvents
    {
        #region Tag Events
        /// <summary>
        /// �±װ� �߰��Ǿ��� �� �߻�
        /// </summary>
        public static event Action<GameObject, string> OnTagAdded;

        /// <summary>
        /// �±װ� ���ŵǾ��� �� �߻�
        /// </summary>
        public static event Action<GameObject, string> OnTagRemoved;

        /// <summary>
        /// �±� �����̳ʰ� ����Ǿ��� �� �߻�
        /// </summary>
        public static event Action<GameObject> OnTagsChanged;
        #endregion

        #region Attribute Events
        /// <summary>
        /// �Ӽ����� ����Ǿ��� �� �߻�
        /// </summary>
        public static event Action<GameObject, string, float, float> OnAttributeChanged;

        /// <summary>
        /// �Ӽ��� �ִ밪�� �������� �� �߻�
        /// </summary>
        public static event Action<GameObject, string> OnAttributeMaxed;

        /// <summary>
        /// �Ӽ��� 0�� �������� �� �߻�
        /// </summary>
        public static event Action<GameObject, string> OnAttributeZero;

        public static event Action<GameObject, AttributeType> OnGlobalAttributeQuery;
        #endregion

        #region Effect Events
        /// <summary>
        /// ȿ���� ����Ǿ��� �� �߻�
        /// </summary>
        public static event Action<GameObject, string> OnEffectApplied;

        /// <summary>
        /// ȿ���� ���ŵǾ��� �� �߻�
        /// </summary>
        public static event Action<GameObject, string> OnEffectRemoved;

        /// <summary>
        /// ȿ���� ���õǾ��� �� �߻�
        /// </summary>
        public static event Action<GameObject, string, int> OnEffectStacked;

        /// <summary>
        /// ȿ���� ����Ǿ��� �� �߻�
        /// </summary>
        public static event Action<GameObject, string> OnEffectExpired;

        #endregion

        #region Ability Events
        /// <summary>
        /// �ɷ��� Ȱ��ȭ�Ǿ��� �� �߻�
        /// </summary>
        public static event Action<GameObject, string> OnAbilityActivated;

        /// <summary>
        /// �ɷ��� ����Ǿ��� �� �߻�
        /// </summary>
        public static event Action<GameObject, string> OnAbilityEnded;

        /// <summary>
        /// �ɷ��� ��ҵǾ��� �� �߻�
        /// </summary>
        public static event Action<GameObject, string> OnAbilityCancelled;

        /// <summary>
        /// �ɷ��� ��ٿ ���� �� �߻�
        /// </summary>
        public static event Action<GameObject, string, float> OnAbilityCooldown;

        /// <summary>
        /// �ɷ� ��ٿ��� ������ �� �߻�
        /// </summary>
        public static event Action<GameObject, string> OnAbilityReady;
        #endregion

        #region Trigger Methods - Tags
        public static void TriggerTagAdded(GameObject target, string tagName)
        {
            OnTagAdded?.Invoke(target, tagName);
            Debug.Log($"[GAS] Tag added: {tagName} to {target.name}");
        }

        public static void TriggerTagRemoved(GameObject target, string tagName)
        {
            OnTagRemoved?.Invoke(target, tagName);
            Debug.Log($"[GAS] Tag removed: {tagName} from {target.name}");
        }

        public static void TriggerTagsChanged(GameObject target)
        {
            OnTagsChanged?.Invoke(target);
        }
        #endregion

        #region Trigger Methods - Attributes
        public static void TriggerAttributeChanged(GameObject target, string attributeName, float oldValue, float newValue)
        {
            OnAttributeChanged?.Invoke(target, attributeName, oldValue, newValue);
            Debug.Log($"[GAS] Attribute changed: {attributeName} {oldValue} -> {newValue} on {target.name}");
        }

        public static void TriggerAttributeMaxed(GameObject target, string attributeName)
        {
            OnAttributeMaxed?.Invoke(target, attributeName);
            Debug.Log($"[GAS] Attribute maxed: {attributeName} on {target.name}");
        }

        public static void TriggerAttributeZero(GameObject target, string attributeName)
        {
            OnAttributeZero?.Invoke(target, attributeName);
            Debug.Log($"[GAS] Attribute zero: {attributeName} on {target.name}");
        }
        #endregion

        #region Trigger Methods - Effects
        public static void TriggerEffectApplied(GameObject target, string effectName)
        {
            OnEffectApplied?.Invoke(target, effectName);
            Debug.Log($"[GAS] Effect applied: {effectName} to {target.name}");
        }

        public static void TriggerEffectRemoved(GameObject target, string effectName)
        {
            OnEffectRemoved?.Invoke(target, effectName);
            Debug.Log($"[GAS] Effect removed: {effectName} from {target.name}");
        }

        public static void TriggerEffectStacked(GameObject target, string effectName, int stackCount)
        {
            OnEffectStacked?.Invoke(target, effectName, stackCount);
            Debug.Log($"[GAS] Effect stacked: {effectName} x{stackCount} on {target.name}");
        }

        public static void TriggerEffectExpired(GameObject target, string effectName)
        {
            OnEffectExpired?.Invoke(target, effectName);
            Debug.Log($"[GAS] Effect expired: {effectName} on {target.name}");
        }
        #endregion

        #region Trigger Methods - Abilities
        public static void TriggerAbilityActivated(GameObject caster, string abilityName)
        {
            OnAbilityActivated?.Invoke(caster, abilityName);
            Debug.Log($"[GAS] Ability activated: {abilityName} by {caster.name}");
        }

        public static void TriggerAbilityEnded(GameObject caster, string abilityName)
        {
            OnAbilityEnded?.Invoke(caster, abilityName);
            Debug.Log($"[GAS] Ability ended: {abilityName} by {caster.name}");
        }

        public static void TriggerAbilityCancelled(GameObject caster, string abilityName)
        {
            OnAbilityCancelled?.Invoke(caster, abilityName);
            Debug.Log($"[GAS] Ability cancelled: {abilityName} by {caster.name}");
        }

        public static void TriggerAbilityCooldown(GameObject caster, string abilityName, float duration)
        {
            OnAbilityCooldown?.Invoke(caster, abilityName, duration);
            Debug.Log($"[GAS] Ability on cooldown: {abilityName} for {duration}s on {caster.name}");
        }

        public static void TriggerAbilityReady(GameObject caster, string abilityName)
        {
            OnAbilityReady?.Invoke(caster, abilityName);
            Debug.Log($"[GAS] Ability ready: {abilityName} on {caster.name}");
        }
        #endregion

        #region Cleanup
        /// <summary>
        /// ��� �̺�Ʈ ���� ���� (�� ��ȯ �� ȣ�� ����)
        /// </summary>
        public static void ClearAllListeners()
        {
            // Tag Events
            OnTagAdded = null;
            OnTagRemoved = null;
            OnTagsChanged = null;

            // Attribute Events
            OnAttributeChanged = null;
            OnAttributeMaxed = null;
            OnAttributeZero = null;
            OnGlobalAttributeQuery = null;

            // Effect Events
            OnEffectApplied = null;
            OnEffectRemoved = null;
            OnEffectStacked = null;
            OnEffectExpired = null;

            // Ability Events
            OnAbilityActivated = null;
            OnAbilityEnded = null;
            OnAbilityCancelled = null;
            OnAbilityCooldown = null;
            OnAbilityReady = null;

            Debug.Log("[GAS] All event listeners cleared");
        }
        #endregion
    }
}