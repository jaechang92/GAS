using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace GAS.Core
{
    /// <summary>
    /// ���� �����Ƽ �����
    /// </summary>
    [CreateAssetMenu(fileName = "NewHealExecutor", menuName = "GAS/Executors/HealExecutor")]
    public class HealExecutor : AbilityExecutor
    {
        [Header("���� ����")]
        [SerializeField] private float baseHealAmount = 20f;
        [SerializeField] private bool scalingWithCaster = true;
        [SerializeField] private bool canOverheal = false;

        [Header("Ÿ����")]
        [SerializeField] private TargetType targetType = TargetType.Self;
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

                // �� Ÿ�ٿ��� ���� ����
                foreach (var target in targets)
                {
                    if (IsValidTarget(caster, target, data))
                    {
                        ApplyHealing(caster, target, data);
                    }
                }

                // ���� �� ó��
                await OnPostExecute(caster, data);

                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"HealExecutor execution failed: {e.Message}");
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

                case TargetType.AllAllies:
                    return FindAlliedTargets(caster, data.Range);

                case TargetType.Area:
                    return FindTargetsInRange(caster.transform.position, data.Range, targetLayer);

                default:
                    return new List<IAbilityTarget>();
            }
        }

        /// <summary>
        /// �Ʊ� Ÿ�� ã��
        /// </summary>
        private List<IAbilityTarget> FindAlliedTargets(GameObject caster, float range)
        {
            var targets = new List<IAbilityTarget>();
            var casterTarget = caster.GetComponent<IAbilityTarget>();

            if (casterTarget == null) return targets;

            var allTargets = FindTargetsInRange(caster.transform.position, range, targetLayer);

            foreach (var target in allTargets)
            {
                if (target.IsFriendlyTo(casterTarget))
                {
                    targets.Add(target);
                }
            }

            return targets;
        }

        /// <summary>
        /// ���� ����
        /// </summary>
        private void ApplyHealing(GameObject caster, IAbilityTarget target, IAbilityData data)
        {
            float healAmount = CalculateHealAmount(caster, target, data);

            // ������ üũ
            if (!canOverheal)
            {
                float missingHealth = target.MaxHealth - target.CurrentHealth;
                healAmount = Mathf.Min(healAmount, missingHealth);
            }

            // ���� ����
            target.Heal(healAmount, caster.GetComponent<IAbilityTarget>());

            Debug.Log($"Healed {target.GameObject.name} for {healAmount:F1} health");
        }

        /// <summary>
        /// ������ ���
        /// </summary>
        private float CalculateHealAmount(GameObject caster, IAbilityTarget target, IAbilityData data)
        {
            float healAmount = baseHealAmount;

            // AbilityData���� �߰� ���� ���� ��������
            if (data is AbilityData concreteData && concreteData.HealValue > 0)
            {
                healAmount = concreteData.HealValue;
            }

            // ĳ���� �����ϸ�
            if (scalingWithCaster)
            {
                // ���⼭ ĳ������ ������ ������� ���� �����ϸ� ����
                // ����: ���� ������ �ִٸ�
                // var intelligence = caster.GetComponent<StatsComponent>()?.Intelligence ?? 1;
                // healAmount *= (1f + intelligence * 0.05f);
            }

            return healAmount;
        }

        protected override bool IsValidTarget(GameObject caster, IAbilityTarget target, IAbilityData data)
        {
            if (!base.IsValidTarget(caster, target, data))
                return false;

            // ������ �Ʊ����Ը� ����
            var casterTarget = caster.GetComponent<IAbilityTarget>();
            if (casterTarget != null && target.IsHostileTo(casterTarget))
                return false;

            // �̹� �ִ� ü���̰� �������� �Ұ����� ���
            if (!canOverheal && target.CurrentHealth >= target.MaxHealth)
                return false;

            return true;
        }
    }
}