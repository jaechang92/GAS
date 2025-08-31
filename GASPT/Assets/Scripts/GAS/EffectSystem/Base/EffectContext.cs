using System;
using System.Collections.Generic;
using UnityEngine;
using GAS.Core;
using GAS.TagSystem;

namespace GAS.EffectSystem
{
    /// <summary>
    /// Effect ���� �� �ʿ��� ���ؽ�Ʈ ������ ��� Ŭ����
    /// Instigator(������), Target(���), �߰� �Ķ���� ���� ����
    /// </summary>
    [Serializable]
    public class EffectContext
    {
        #region Fields

        private GameObject instigator;
        private GameObject target;
        private GameplayEffect sourceEffect;
        private object sourceAbility;
        private float magnitude;
        private int stackCount;
        private float elapsedTime;
        private Dictionary<string, object> additionalData;
        private Vector3 hitPoint;
        private Vector3 hitNormal;
        private TagContainer contextTags;

        #endregion

        #region Properties

        /// <summary>
        /// Effect ������
        /// </summary>
        public GameObject Instigator
        {
            get => instigator;
            set => instigator = value;
        }

        /// <summary>
        /// Effect ���
        /// </summary>
        public GameObject Target
        {
            get => target;
            set => target = value;
        }

        /// <summary>
        /// ���� Effect
        /// </summary>
        public GameplayEffect SourceEffect
        {
            get => sourceEffect;
            set => sourceEffect = value;
        }

        /// <summary>
        /// Effect�� �߻���Ų Ability (Phase 3���� ���)
        /// </summary>
        public object SourceAbility
        {
            get => sourceAbility;
            set => sourceAbility = value;
        }

        /// <summary>
        /// Effect ���� ���
        /// </summary>
        public float Magnitude
        {
            get => magnitude;
            set => magnitude = Mathf.Max(0f, value);
        }

        /// <summary>
        /// ���� ���� ��
        /// </summary>
        public int StackCount
        {
            get => stackCount;
            set => stackCount = Mathf.Max(0, value);
        }

        /// <summary>
        /// Effect ��� �ð�
        /// </summary>
        public float ElapsedTime
        {
            get => elapsedTime;
            set => elapsedTime = Mathf.Max(0f, value);
        }

        /// <summary>
        /// �浹 ���� (����ü ��� ���)
        /// </summary>
        public Vector3 HitPoint
        {
            get => hitPoint;
            set => hitPoint = value;
        }

        /// <summary>
        /// �浹 ǥ�� ����
        /// </summary>
        public Vector3 HitNormal
        {
            get => hitNormal;
            set => hitNormal = value;
        }

        /// <summary>
        /// ���ؽ�Ʈ �±�
        /// </summary>
        public TagContainer ContextTags
        {
            get => contextTags;
            set => contextTags = value;
        }

        /// <summary>
        /// �߰� ������ ��ųʸ�
        /// </summary>
        public Dictionary<string, object> AdditionalData
        {
            get => additionalData ??= new Dictionary<string, object>();
        }

        #endregion

        #region Constructors

        /// <summary>
        /// �⺻ ������
        /// </summary>
        public EffectContext()
        {
            magnitude = 1f;
            stackCount = 1;
            elapsedTime = 0f;
            additionalData = new Dictionary<string, object>();
            contextTags = new TagContainer();
        }

        /// <summary>
        /// �ʼ� �Ķ���͸� �޴� ������
        /// </summary>
        /// <param name="instigator">������</param>
        /// <param name="target">���</param>
        /// <param name="sourceEffect">���� Effect</param>
        public EffectContext(GameObject instigator, GameObject target, GameplayEffect sourceEffect) : this()
        {
            this.instigator = instigator;
            this.target = target;
            this.sourceEffect = sourceEffect;
        }

        /// <summary>
        /// ��ü �Ķ���͸� �޴� ������
        /// </summary>
        public EffectContext(
            GameObject instigator,
            GameObject target,
            GameplayEffect sourceEffect,
            float magnitude = 1f,
            int stackCount = 1) : this(instigator, target, sourceEffect)
        {
            this.magnitude = magnitude;
            this.stackCount = stackCount;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// �߰� ������ ����
        /// </summary>
        public void SetData(string key, object value)
        {
            if (string.IsNullOrEmpty(key)) return;

            AdditionalData[key] = value;
        }

        /// <summary>
        /// �߰� ������ ��������
        /// </summary>
        public T GetData<T>(string key, T defaultValue = default)
        {
            if (string.IsNullOrEmpty(key)) return defaultValue;

            if (AdditionalData.TryGetValue(key, out var value) && value is T typedValue)
            {
                return typedValue;
            }

            return defaultValue;
        }

        /// <summary>
        /// �߰� ������ ���� ���� Ȯ��
        /// </summary>
        public bool HasData(string key)
        {
            return !string.IsNullOrEmpty(key) && AdditionalData.ContainsKey(key);
        }

        /// <summary>
        /// �߰� ������ ����
        /// </summary>
        public bool RemoveData(string key)
        {
            if (string.IsNullOrEmpty(key)) return false;

            return AdditionalData.Remove(key);
        }

        /// <summary>
        /// ��� �߰� ������ ����
        /// </summary>
        public void ClearData()
        {
            AdditionalData.Clear();
        }

        /// <summary>
        /// ���ؽ�Ʈ �±� �߰�
        /// </summary>
        public void AddContextTag(GameplayTag tag)
        {
            contextTags?.AddTag(tag);
        }

        /// <summary>
        /// ���ؽ�Ʈ �±� ����
        /// </summary>
        public void RemoveContextTag(GameplayTag tag)
        {
            contextTags?.RemoveTag(tag);
        }

        /// <summary>
        /// ���ؽ�Ʈ �±� Ȯ��
        /// </summary>
        public bool HasContextTag(GameplayTag tag)
        {
            return contextTags?.HasTag(tag) ?? false;
        }

        /// <summary>
        /// ���ؽ�Ʈ ���� (Deep Copy)
        /// </summary>
        public EffectContext Clone()
        {
            var clone = new EffectContext
            {
                instigator = instigator,
                target = target,
                sourceEffect = sourceEffect,
                sourceAbility = sourceAbility,
                magnitude = magnitude,
                stackCount = stackCount,
                elapsedTime = elapsedTime,
                hitPoint = hitPoint,
                hitNormal = hitNormal,
                contextTags = contextTags?.Clone()
            };

            // �߰� ������ ����
            foreach (var kvp in AdditionalData)
            {
                clone.AdditionalData[kvp.Key] = kvp.Value;
            }

            return clone;
        }

        /// <summary>
        /// ���ؽ�Ʈ ����
        /// </summary>
        public void Reset()
        {
            instigator = null;
            target = null;
            sourceEffect = null;
            sourceAbility = null;
            magnitude = 1f;
            stackCount = 1;
            elapsedTime = 0f;
            hitPoint = Vector3.zero;
            hitNormal = Vector3.up;
            contextTags?.Clear();
            AdditionalData.Clear();
        }

        /// <summary>
        /// ��ȿ�� ����
        /// </summary>
        public bool IsValid()
        {
            // �ּ��� target�� �־�� ��
            return target != null && sourceEffect != null;
        }

        /// <summary>
        /// Instigator�� Target�� ������ Ȯ��
        /// </summary>
        public bool IsSelfTarget()
        {
            return instigator != null && target != null && instigator == target;
        }

        /// <summary>
        /// �Ÿ� ��� (Instigator�� Target ��)
        /// </summary>
        public float GetDistance()
        {
            if (instigator == null || target == null) return 0f;

            return Vector3.Distance(instigator.transform.position, target.transform.position);
        }

        /// <summary>
        /// ���� ���� ��� (Instigator���� Target����)
        /// </summary>
        public Vector3 GetDirection()
        {
            if (instigator == null || target == null) return Vector3.forward;

            return (target.transform.position - instigator.transform.position).normalized;
        }

        /// <summary>
        /// ���ؽ�Ʈ ���� ���ڿ� ��ȯ
        /// </summary>
        public override string ToString()
        {
            return $"EffectContext: {sourceEffect?.name ?? "Unknown"} " +
                   $"[{instigator?.name ?? "None"} -> {target?.name ?? "None"}] " +
                   $"Mag: {magnitude:F2}, Stack: {stackCount}";
        }

        #endregion

        #region Static Factory Methods

        /// <summary>
        /// Self-target ���ؽ�Ʈ ����
        /// </summary>
        public static EffectContext CreateSelfContext(GameObject self, GameplayEffect effect)
        {
            return new EffectContext(self, self, effect);
        }

        /// <summary>
        /// AOE ���ؽ�Ʈ ����
        /// </summary>
        public static EffectContext CreateAOEContext(GameObject instigator, GameObject target, GameplayEffect effect, Vector3 center)
        {
            var context = new EffectContext(instigator, target, effect);
            context.SetData("AOECenter", center);
            context.SetData("AOERadius", 5f); // �⺻��
            return context;
        }

        /// <summary>
        /// ����ü ���ؽ�Ʈ ����
        /// </summary>
        public static EffectContext CreateProjectileContext(
            GameObject instigator,
            GameObject target,
            GameplayEffect effect,
            Vector3 hitPoint,
            Vector3 hitNormal)
        {
            var context = new EffectContext(instigator, target, effect)
            {
                hitPoint = hitPoint,
                hitNormal = hitNormal
            };
            context.AddContextTag(new GameplayTag("Effect.Source.Projectile"));
            return context;
        }

        #endregion
    }

    /// <summary>
    /// EffectContext Pool�� ���� ���� Ŭ����
    /// �޸� �Ҵ� ����ȭ
    /// </summary>
    public static class EffectContextPool
    {
        private static readonly Stack<EffectContext> pool = new Stack<EffectContext>();
        private const int MaxPoolSize = 50;

        /// <summary>
        /// Pool���� Context ��������
        /// </summary>
        public static EffectContext Get()
        {
            if (pool.Count > 0)
            {
                var context = pool.Pop();
                context.Reset();
                return context;
            }

            return new EffectContext();
        }

        /// <summary>
        /// Pool�� Context ��ȯ
        /// </summary>
        public static void Return(EffectContext context)
        {
            if (context == null || pool.Count >= MaxPoolSize) return;

            context.Reset();
            pool.Push(context);
        }

        /// <summary>
        /// Pool �ʱ�ȭ
        /// </summary>
        public static void Clear()
        {
            pool.Clear();
        }

        /// <summary>
        /// Pool ���� �Ҵ�
        /// </summary>
        public static void Prewarm(int count)
        {
            for (int i = 0; i < count && pool.Count < MaxPoolSize; i++)
            {
                pool.Push(new EffectContext());
            }
        }
    }
}