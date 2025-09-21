using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace GAS.Core
{
    /// <summary>
    /// ������ �����Ƽ �����
    /// </summary>
    [CreateAssetMenu(fileName = "NewDamageExecutor", menuName = "GAS/Executors/DamageExecutor")]
    public class DamageExecutor : AbilityExecutor
    {
        [Header("������ ����")]
        [SerializeField] private float baseDamage = 10f;
        [SerializeField] private DamageType damageType = DamageType.Physical;
        [SerializeField] private bool scalingWithCaster = true;

        [Header("Ÿ����")]
        [SerializeField] private TargetType targetType = TargetType.SingleTarget;
        [SerializeField] private LayerMask targetLayer = -1;

        public override async Awaitable<bool> ExecuteAsync(GameObject caster, IAbilityData data, List<IAbilityTarget> targets)
        {
            try
            {
                // ���� �� ó��
                await OnPreExecute(caster, data);

                // Ÿ���� ������ �ڵ����� ã��
                if (targets == null || targets.Count == 0)
                {
                    targets = FindTargets(caster, data);
                }

                // �� Ÿ�ٿ��� ������ ����
                foreach (var target in targets)
                {
                    if (IsValidTarget(caster, target, data))
                    {
                        ApplyDamage(caster, target, data);
                    }
                }

                // ���� �� ó��
                await OnPostExecute(caster, data);

                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"DamageExecutor execution failed: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Ÿ�� ã��
        /// </summary>
        private List<IAbilityTarget> FindTargets(GameObject caster, IAbilityData data)
        {
            switch (targetType)
            {
                case TargetType.Self:
                    var selfTarget = caster.GetComponent<IAbilityTarget>();
                    return selfTarget != null ? new List<IAbilityTarget> { selfTarget } : new List<IAbilityTarget>();

                case TargetType.Area:
                    return FindTargetsInRange(caster.transform.position, data.Range, targetLayer);

                case TargetType.Cone:
                    return FindTargetsInDirection(
                        caster.transform.position,
                        caster.transform.forward,
                        data.Range,
                        90f, // �⺻ ����
                        targetLayer);

                case TargetType.Line:
                    Vector3 endPoint = caster.transform.position + caster.transform.forward * data.Range;
                    return FindTargetsInLine(caster.transform.position, endPoint, 2f, targetLayer);

                default:
                    return new List<IAbilityTarget>();
            }
        }

        /// <summary>
        /// ������ ����
        /// </summary>
        private void ApplyDamage(GameObject caster, IAbilityTarget target, IAbilityData data)
        {
            float finalDamage = CalculateDamage(caster, target, data);

            // ������ ����
            target.TakeDamage(finalDamage, caster.GetComponent<IAbilityTarget>());

            Debug.Log($"Applied {finalDamage:F1} {damageType} damage to {target.GameObject.name}");
        }

        /// <summary>
        /// ���� ������ ���
        /// </summary>
        private float CalculateDamage(GameObject caster, IAbilityTarget target, IAbilityData data)
        {
            float damage = baseDamage;

            // AbilityData���� �߰� ������ ���� ��������
            if (data is AbilityData concreteData && concreteData.DamageValue > 0)
            {
                damage = concreteData.DamageValue;
            }

            // ĳ���� �����ϸ� (��: ����, ���� ��)
            if (scalingWithCaster)
            {
                // ���⼭ ĳ������ ������ ������� ������ �����ϸ� ����
                // ����: ���� �ý����� �ִٸ�
                // var level = caster.GetComponent<LevelComponent>()?.Level ?? 1;
                // damage *= (1f + level * 0.1f);
            }

            return damage;
        }

        protected override bool IsValidTarget(GameObject caster, IAbilityTarget target, IAbilityData data)
        {
            if (!base.IsValidTarget(caster, target, data))
                return false;

            // �ڱ� �ڽ� ���� (Self Ÿ���� �ƴ� ���)
            if (targetType != TargetType.Self && target.GameObject == caster)
                return false;

            // �� üũ (�������� ���)
            var casterTarget = caster.GetComponent<IAbilityTarget>();
            if (casterTarget != null && target.IsFriendlyTo(casterTarget))
                return false;

            return true;
        }
    }
}