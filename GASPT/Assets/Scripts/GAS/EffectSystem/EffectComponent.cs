using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GAS.Core;
using GAS.AttributeSystem;
using GAS.TagSystem;

namespace GAS.EffectSystem
{
    /// <summary>
    /// GameObject�� Effect�� �����ϰ� �����ϴ� ������Ʈ
    /// IEffectReceiver �������̽��� �����Ͽ� Effect �ý��۰� ����
    /// </summary>
    [RequireComponent(typeof(TagComponent))]
    public class EffectComponent : MonoBehaviour, IEffectReceiver
    {
        #region Fields

        [Header("=== Effect Settings ===")]
        [SerializeField] private bool acceptEffects = true;
        [SerializeField] private int maxActiveEffects = 50;
        [SerializeField] private bool debugMode = false;

        [Header("=== Immunity Settings ===")]
        [SerializeField] private List<GameplayTag> immunityTags = new List<GameplayTag>();
        [SerializeField] private float immunityDuration = 0f;

        [Header("=== Effect Filters ===")]
        [SerializeField] private TagRequirement effectApplicationFilter;
        [SerializeField] private List<string> blacklistedEffectIds = new List<string>();

        [Header("=== Runtime Info (Read Only) ===")]
        [SerializeField] private List<EffectInstance> activeEffects = new List<EffectInstance>();
        [SerializeField] private int currentEffectCount = 0;

        // ������Ʈ ĳ��
        private TagComponent tagComponent;
        private AttributeSetComponent attributeComponent;

        // Immunity ����
        private Dictionary<string, float> immunityTimers = new Dictionary<string, float>();

        // Effect �׷� ���� (���� ID�� Effect��)
        private Dictionary<string, List<EffectInstance>> effectGroups = new Dictionary<string, List<EffectInstance>>();

        #endregion

        #region Properties

        /// <summary>
        /// Ȱ��ȭ�� Effect �ν��Ͻ� ��� (�б� ����)
        /// </summary>
        public IReadOnlyList<EffectInstance> ActiveEffects => activeEffects.AsReadOnly();

        /// <summary>
        /// Effect ���� ���� ����
        /// </summary>
        public bool AcceptEffects
        {
            get => acceptEffects;
            set => acceptEffects = value;
        }

        /// <summary>
        /// ���� Ȱ�� Effect ����
        /// </summary>
        public int ActiveEffectCount => activeEffects.Count(e => !e.IsExpired);

        #endregion

        #region Events

        public event Action<EffectInstance> OnEffectApplied;
        public event Action<EffectInstance> OnEffectRemoved;
        public event Action<EffectInstance, int> OnEffectStackChanged;
        public event Action<EffectInstance> OnEffectExpired;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            // ������Ʈ ĳ��
            tagComponent = GetComponent<TagComponent>();
            if (tagComponent == null)
            {
                tagComponent = gameObject.AddComponent<TagComponent>();
            }

            attributeComponent = GetComponent<AttributeSetComponent>();

            // �ʱ�ȭ
            activeEffects = new List<EffectInstance>();
            effectGroups = new Dictionary<string, List<EffectInstance>>();
            immunityTimers = new Dictionary<string, float>();
        }

        private void Start()
        {
            // GAS Manager�� ���
            if (GASManager.Instance != null)
            {
                Debug.Log($"[EffectComponent] Registered on {gameObject.name}");
            }
        }

        private void Update()
        {
            // Immunity Ÿ�̸� ������Ʈ
            UpdateImmunityTimers();

            // Debug ǥ��
            if (debugMode)
            {
                currentEffectCount = ActiveEffectCount;
            }
        }

        private void OnDestroy()
        {
            // ��� Effect ����
            RemoveAllEffects();
        }

        #endregion

        #region IEffectReceiver Implementation

        /// <summary>
        /// Effect ���� ���� ���� Ȯ��
        /// </summary>
        public bool CanReceiveEffect(GameplayEffect effect, EffectContext context)
        {
            if (!acceptEffects)
            {
                Debug.Log($"[EffectComponent] {gameObject.name} is not accepting effects");
                return false;
            }

            if (effect == null || context == null)
                return false;

            // �ִ� Effect �� üũ
            if (activeEffects.Count >= maxActiveEffects)
            {
                Debug.LogWarning($"[EffectComponent] Max effect limit reached on {gameObject.name}");
                return false;
            }

            // ������Ʈ üũ
            if (blacklistedEffectIds.Contains(effect.EffectId))
            {
                Debug.Log($"[EffectComponent] Effect {effect.EffectName} is blacklisted");
                return false;
            }

            // Immunity üũ
            if (IsImmuneToEffect(effect))
            {
                Debug.Log($"[EffectComponent] {gameObject.name} is immune to {effect.EffectName}");
                return false;
            }

            // Effect�� ���� ���� üũ
            if (!effect.CanApply(context, gameObject))
            {
                return false;
            }

            // ���� üũ
            if (effectApplicationFilter != null)
            {
                var effectTags = new TagContainer();
                if (effect.AssetTags != null)
                {
                    foreach (var tag in effect.AssetTags.Tags)
                    {
                        effectTags.AddTag(tag);
                    }
                }

                // Effect�� �±װ� ���� ������ �����ϴ��� Ȯ��
                bool satisfiesFilter = true;
                if (effectApplicationFilter.RequiredTags != null)
                {
                    foreach (var requiredTag in effectApplicationFilter.RequiredTags)
                    {
                        if (!effectTags.HasTag(requiredTag))
                        {
                            satisfiesFilter = false;
                            break;
                        }
                    }
                }

                if (!satisfiesFilter)
                {
                    Debug.Log($"[EffectComponent] Effect {effect.EffectName} doesn't satisfy filter requirements");
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Effect ����
        /// </summary>
        public EffectInstance ApplyEffect(GameplayEffect effect, EffectContext context)
        {
            if (!CanReceiveEffect(effect, context))
                return null;

            try
            {
                // Context�� Target�� ���� GameObject���� Ȯ��
                if (context.Target != gameObject)
                {
                    context.Target = gameObject;
                }

                // Effect ����
                bool success = effect.Apply(context, gameObject);
                if (!success)
                {
                    Debug.LogWarning($"[EffectComponent] Failed to apply {effect.EffectName} to {gameObject.name}");
                    return null;
                }

                // �� �ν��Ͻ� ���� (Effect Ÿ�Կ� ���� �ٸ��� ó��)
                var instance = new EffectInstance(effect, context);

                // �ν��Ͻ� ���
                RegisterInstance(instance);

                // �̺�Ʈ �߻�
                OnEffectApplied?.Invoke(instance);
                GASEvents.TriggerEffectApplied(gameObject, effect.EffectName);

                if (debugMode)
                {
                    Debug.Log($"[EffectComponent] Applied {effect.EffectName} to {gameObject.name}");
                }

                return instance;
            }
            catch (Exception e)
            {
                Debug.LogError($"[EffectComponent] Error applying effect: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Effect ����
        /// </summary>
        public bool RemoveEffect(EffectInstance instance)
        {
            if (instance == null || !activeEffects.Contains(instance))
                return false;

            try
            {
                // Effect ���� ȣ��
                instance.SourceEffect.Remove(instance.Context, gameObject);

                // �ν��Ͻ� ��� ����
                UnregisterInstance(instance);

                // �̺�Ʈ �߻�
                OnEffectRemoved?.Invoke(instance);
                GASEvents.TriggerEffectRemoved(gameObject, instance.SourceEffect.EffectName);

                if (debugMode)
                {
                    Debug.Log($"[EffectComponent] Removed {instance.SourceEffect.EffectName} from {gameObject.name}");
                }

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[EffectComponent] Error removing effect: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Effect ID�� ����
        /// </summary>
        public int RemoveEffectById(string effectId)
        {
            if (string.IsNullOrEmpty(effectId))
                return 0;

            var toRemove = activeEffects
                .Where(e => e.SourceEffect.EffectId == effectId)
                .ToList();

            int removedCount = 0;
            foreach (var instance in toRemove)
            {
                if (RemoveEffect(instance))
                    removedCount++;
            }

            return removedCount;
        }

        /// <summary>
        /// Ư�� Source�� ��� Effect ����
        /// </summary>
        public int RemoveEffectsFromSource(object source)
        {
            if (source == null)
                return 0;

            var toRemove = activeEffects
                .Where(e => e.Context.Instigator == source || e.Context.SourceAbility == source)
                .ToList();

            int removedCount = 0;
            foreach (var instance in toRemove)
            {
                if (RemoveEffect(instance))
                    removedCount++;
            }

            return removedCount;
        }

        /// <summary>
        /// ��� Effect ����
        /// </summary>
        public void RemoveAllEffects()
        {
            var toRemove = new List<EffectInstance>(activeEffects);

            foreach (var instance in toRemove)
            {
                RemoveEffect(instance);
            }

            activeEffects.Clear();
            effectGroups.Clear();

            Debug.Log($"[EffectComponent] All effects removed from {gameObject.name}");
        }

        /// <summary>
        /// Ư�� ID�� Ȱ�� Effect �ν��Ͻ� ��������
        /// </summary>
        public List<EffectInstance> GetActiveEffectsByID(string effectId)
        {
            if (string.IsNullOrEmpty(effectId))
                return new List<EffectInstance>();

            if (effectGroups.TryGetValue(effectId, out var instances))
            {
                return instances.Where(e => !e.IsExpired).ToList();
            }

            return new List<EffectInstance>();
        }

        /// <summary>
        /// Ư�� �±׸� ���� Effect �ν��Ͻ� ��������
        /// </summary>
        public List<EffectInstance> GetActiveEffectsByTag(GameplayTag tag)
        {
            if (tag == null)
                return new List<EffectInstance>();

            return activeEffects
                .Where(e => !e.IsExpired && e.SourceEffect.AssetTags != null &&
                       e.SourceEffect.AssetTags.HasTag(tag))
                .ToList();
        }

        /// <summary>
        /// Effect ���� �� ��������
        /// </summary>
        public int GetEffectStackCount(string effectId)
        {
            var effects = GetActiveEffectsByID(effectId);

            if (effects.Count == 0)
                return 0;

            // ���� ��å�� ���� �ٸ��� ���
            var firstEffect = effects.First();
            if (firstEffect.SourceEffect.StackingPolicy == EffectStackingPolicy.Stack)
            {
                return firstEffect.CurrentStack;
            }
            else
            {
                return effects.Count;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Ư�� Effect�� ���� �鿪 �ο�
        /// </summary>
        public void GrantImmunity(string effectId, float duration)
        {
            if (string.IsNullOrEmpty(effectId))
                return;

            immunityTimers[effectId] = Time.time + duration;

            Debug.Log($"[EffectComponent] Granted immunity to {effectId} for {duration}s");
        }

        /// <summary>
        /// ����� ���� (Purge)
        /// </summary>
        public int PurgeDebuffs(int purgeLevel = 1)
        {
            var debuffs = activeEffects
                .Where(e => !e.IsExpired &&
                       e.SourceEffect.AssetTags != null &&
                       e.SourceEffect.AssetTags.HasTag(new GameplayTag("Effect.Type.Debuff")))
                .ToList();

            int purgedCount = 0;
            foreach (var debuff in debuffs)
            {
                // Purge ���� ���� üũ
                if (debuff.SourceEffect is InfiniteEffect infinite && !infinite.CanBeDispelled)
                    continue;

                var context = debuff.Context.Clone();
                context.SetData("DispelPower", purgeLevel);

                if (RemoveEffect(debuff))
                    purgedCount++;
            }

            Debug.Log($"[EffectComponent] Purged {purgedCount} debuffs from {gameObject.name}");
            return purgedCount;
        }

        /// <summary>
        /// ���� ���� (Dispel)
        /// </summary>
        public int DispelBuffs(int dispelLevel = 1)
        {
            var buffs = activeEffects
                .Where(e => !e.IsExpired &&
                       e.SourceEffect.AssetTags != null &&
                       e.SourceEffect.AssetTags.HasTag(new GameplayTag("Effect.Type.Buff")))
                .ToList();

            int dispelledCount = 0;
            foreach (var buff in buffs)
            {
                // Dispel ���� ���� üũ
                if (buff.SourceEffect is InfiniteEffect infinite && !infinite.CanBeDispelled)
                    continue;

                var context = buff.Context.Clone();
                context.SetData("DispelPower", dispelLevel);

                if (RemoveEffect(buff))
                    dispelledCount++;
            }

            Debug.Log($"[EffectComponent] Dispelled {dispelledCount} buffs from {gameObject.name}");
            return dispelledCount;
        }

        /// <summary>
        /// Effect ���� ���� Ȯ��
        /// </summary>
        public bool HasEffect(string effectId)
        {
            return GetActiveEffectsByID(effectId).Count > 0;
        }

        /// <summary>
        /// Ư�� �±׸� ���� Effect ���� ����
        /// </summary>
        public bool HasEffectWithTag(GameplayTag tag)
        {
            return GetActiveEffectsByTag(tag).Count > 0;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// �ν��Ͻ� ���
        /// </summary>
        private void RegisterInstance(EffectInstance instance)
        {
            if (instance == null) return;

            activeEffects.Add(instance);

            // Effect �׷쿡 �߰�
            string effectId = instance.SourceEffect.EffectId;
            if (!effectGroups.ContainsKey(effectId))
            {
                effectGroups[effectId] = new List<EffectInstance>();
            }
            effectGroups[effectId].Add(instance);
        }

        /// <summary>
        /// �ν��Ͻ� ��� ����
        /// </summary>
        private void UnregisterInstance(EffectInstance instance)
        {
            if (instance == null) return;

            activeEffects.Remove(instance);

            // Effect �׷쿡�� ����
            string effectId = instance.SourceEffect.EffectId;
            if (effectGroups.TryGetValue(effectId, out var group))
            {
                group.Remove(instance);
                if (group.Count == 0)
                {
                    effectGroups.Remove(effectId);
                }
            }
        }

        /// <summary>
        /// Effect �鿪 Ȯ��
        /// </summary>
        private bool IsImmuneToEffect(GameplayEffect effect)
        {
            // Effect ID ��� �鿪
            if (immunityTimers.ContainsKey(effect.EffectId))
            {
                if (immunityTimers[effect.EffectId] > Time.time)
                    return true;
            }

            // �±� ��� �鿪
            if (effect.AssetTags != null)
            {
                foreach (var immunityTag in immunityTags)
                {
                    if (effect.AssetTags.HasTag(immunityTag))
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// �鿪 Ÿ�̸� ������Ʈ
        /// </summary>
        private void UpdateImmunityTimers()
        {
            var expiredImmunities = new List<string>();

            foreach (var kvp in immunityTimers)
            {
                if (kvp.Value <= Time.time)
                {
                    expiredImmunities.Add(kvp.Key);
                }
            }

            foreach (var key in expiredImmunities)
            {
                immunityTimers.Remove(key);
                Debug.Log($"[EffectComponent] Immunity to {key} expired");
            }
        }

        #endregion

        #region Editor

#if UNITY_EDITOR
        private void OnValidate()
        {
            maxActiveEffects = Mathf.Max(1, maxActiveEffects);
            immunityDuration = Mathf.Max(0f, immunityDuration);
        }

        private void OnDrawGizmosSelected()
        {
            // Active effects �ð�ȭ
            if (activeEffects != null && activeEffects.Count > 0)
            {
                Gizmos.color = Color.green;
                float offset = 0.5f;
                foreach (var effect in activeEffects)
                {
                    if (effect != null && !effect.IsExpired)
                    {
                        Gizmos.DrawWireCube(
                            transform.position + Vector3.up * offset,
                            Vector3.one * 0.1f
                        );
                        offset += 0.2f;
                    }
                }
            }
        }
#endif

        #endregion
    }
}

// ���� ��ġ: Assets/Scripts/GAS/EffectSystem/EffectComponent.cs