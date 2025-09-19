// ===================================
// ����: Assets/Scripts/Ability/Executors/AbilityExecutor.cs
// ===================================
using AbilitySystem.Platformer;
using Helper;
using System.Collections.Generic;
using UnityEngine;

namespace AbilitySystem
{
    /// <summary>
    /// �����Ƽ ���� ������ �߻� Ŭ����
    /// </summary>
    public abstract class AbilityExecutor : ScriptableObject
    {
        [Header("������ ����")]
        [SerializeField] protected string executorId;
        [SerializeField] protected string executorName;

        [Header("ȿ�� ����")]
        [SerializeField] protected GameObject effectPrefab;
        [SerializeField] protected AudioClip soundEffect;
        [SerializeField] protected float executionDelay = 0f;

        // ������Ƽ
        public string Id => executorId;
        public string Name => executorName;

        /// <summary>
        /// �����Ƽ ���� �� ����
        /// </summary>
        public virtual bool Validate(GameObject caster, PlatformerAbilityData data, List<IAbilityTarget> targets)
        {
            // ���� ���� ���� ����
            return true;
        }

        /// <summary>
        /// �����Ƽ ���� ���� ���� (�񵿱�)
        /// </summary>
        public abstract Awaitable ExecuteAsync(GameObject caster, PlatformerAbilityData data, List<IAbilityTarget> targets);

        /// <summary>
        /// Ÿ�� ��ȿ�� �˻�
        /// </summary>
        protected virtual bool IsValidTarget(GameObject caster, IAbilityTarget target, PlatformerAbilityData data)
        {
            // Ÿ���� ��ȿ���� �˻�
            return false;
        }

        /// <summary>
        /// ����Ʈ ����
        /// </summary>
        protected virtual void SpawnEffect(Vector3 position, Quaternion rotation)
        {
            // �ð� ȿ�� ����
        }

        /// <summary>
        /// ���� ���
        /// </summary>
        protected virtual void PlaySound(Vector3 position)
        {
            // ���� ȿ�� ���
        }

        /// <summary>
        /// ���� �� ó��
        /// </summary>
        protected virtual Awaitable OnPreExecute(GameObject caster, SkulData data)
        {
            // ���� �� �غ� �۾�
            return AwaitableHelper.CompletedTask;
        }

        /// <summary>
        /// ���� �� ó��
        /// </summary>
        protected virtual Awaitable OnPostExecute(GameObject caster, SkulData data)
        {
            // ���� �� ���� �۾�
            return AwaitableHelper.CompletedTask;
        }

        /// <summary>
        /// ���� �� Ÿ�� ã��
        /// </summary>
        protected List<IAbilityTarget> FindTargetsInRange(Vector3 center, float range, LayerMask targetLayer)
        {
            // ���� �� ��� Ÿ�� �˻�
            return new List<IAbilityTarget>();
        }
    }
}