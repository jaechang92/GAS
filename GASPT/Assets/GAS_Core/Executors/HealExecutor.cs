using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace GAS.Core
{
    /// <summary>
    /// 힐링 어빌리티 실행기
    /// </summary>
    [CreateAssetMenu(fileName = "NewHealExecutor", menuName = "GAS/Executors/HealExecutor")]
    public class HealExecutor : AbilityExecutor
    {
        [Header("힐링 설정")]
        [SerializeField] private float baseHealAmount = 20f;
        [SerializeField] private bool scalingWithCaster = true;
        [SerializeField] private bool canOverheal = false;

        [Header("타겟팅")]
        [SerializeField] private TargetType targetType = TargetType.Self;
        [SerializeField] private LayerMask targetLayer = -1;

        public override async Awaitable<bool> ExecuteAsync(GameObject caster, IAbilityData data, List<IAbilityTarget> targets)
        {
            try
            {
                // 실행 전 처리
                await OnPreExecute(caster, data);

                // 타겟이 없으면 자동으로 찾기
                if (targets == null || targets.Count == 0)
                {
                    targets = FindTargets(caster, data);
                }

                // 각 타겟에게 힐링 적용
                foreach (var target in targets)
                {
                    if (IsValidTarget(caster, target, data))
                    {
                        ApplyHealing(caster, target, data);
                    }
                }

                // 실행 후 처리
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
        /// 타겟 찾기
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
        /// 아군 타겟 찾기
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
        /// 힐링 적용
        /// </summary>
        private void ApplyHealing(GameObject caster, IAbilityTarget target, IAbilityData data)
        {
            float healAmount = CalculateHealAmount(caster, target, data);

            // 오버힐 체크
            if (!canOverheal)
            {
                float missingHealth = target.MaxHealth - target.CurrentHealth;
                healAmount = Mathf.Min(healAmount, missingHealth);
            }

            // 힐링 적용
            target.Heal(healAmount, caster.GetComponent<IAbilityTarget>());

            Debug.Log($"Healed {target.GameObject.name} for {healAmount:F1} health");
        }

        /// <summary>
        /// 힐링량 계산
        /// </summary>
        private float CalculateHealAmount(GameObject caster, IAbilityTarget target, IAbilityData data)
        {
            float healAmount = baseHealAmount;

            // AbilityData에서 추가 힐링 정보 가져오기
            if (data is AbilityData concreteData && concreteData.HealValue > 0)
            {
                healAmount = concreteData.HealValue;
            }

            // 캐스터 스케일링
            if (scalingWithCaster)
            {
                // 여기서 캐스터의 스탯을 기반으로 힐링 스케일링 가능
                // 예시: 지능 스탯이 있다면
                // var intelligence = caster.GetComponent<StatsComponent>()?.Intelligence ?? 1;
                // healAmount *= (1f + intelligence * 0.05f);
            }

            return healAmount;
        }

        protected override bool IsValidTarget(GameObject caster, IAbilityTarget target, IAbilityData data)
        {
            if (!base.IsValidTarget(caster, target, data))
                return false;

            // 힐링은 아군에게만 적용
            var casterTarget = caster.GetComponent<IAbilityTarget>();
            if (casterTarget != null && target.IsHostileTo(casterTarget))
                return false;

            // 이미 최대 체력이고 오버힐이 불가능한 경우
            if (!canOverheal && target.CurrentHealth >= target.MaxHealth)
                return false;

            return true;
        }
    }
}