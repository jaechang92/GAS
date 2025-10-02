using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Enums;
using UnityEngine;

namespace GAS.Core
{
    /// <summary>
    /// 데미지 어빌리티 실행기
    /// </summary>
    [CreateAssetMenu(fileName = "NewDamageExecutor", menuName = "GAS/Executors/DamageExecutor")]
    public class DamageExecutor : AbilityExecutor
    {
        [Header("데미지 설정")]
        [SerializeField] private float baseDamage = 10f;
        [SerializeField] private DamageType damageType = DamageType.Physical;
        [SerializeField] private bool scalingWithCaster = true;

        [Header("타겟팅")]
        [SerializeField] private TargetType targetType = TargetType.SingleTarget;
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

                // 각 타겟에게 데미지 적용
                foreach (var target in targets)
                {
                    if (IsValidTarget(caster, target, data))
                    {
                        ApplyDamage(caster, target, data);
                    }
                }

                // 실행 후 처리
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
        /// 타겟 찾기
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
                        90f, // 기본 각도
                        targetLayer);

                case TargetType.Line:
                    Vector3 endPoint = caster.transform.position + caster.transform.forward * data.Range;
                    return FindTargetsInLine(caster.transform.position, endPoint, 2f, targetLayer);

                default:
                    return new List<IAbilityTarget>();
            }
        }

        /// <summary>
        /// 데미지 적용
        /// </summary>
        private void ApplyDamage(GameObject caster, IAbilityTarget target, IAbilityData data)
        {
            float finalDamage = CalculateDamage(caster, target, data);

            // 데미지 적용
            target.TakeDamage(finalDamage, caster.GetComponent<IAbilityTarget>());

            Debug.Log($"Applied {finalDamage:F1} {damageType} damage to {target.GameObject.name}");
        }

        /// <summary>
        /// 최종 데미지 계산
        /// </summary>
        private float CalculateDamage(GameObject caster, IAbilityTarget target, IAbilityData data)
        {
            float damage = baseDamage;

            // AbilityData에서 추가 데미지 정보 가져오기
            if (data is AbilityData concreteData && concreteData.DamageValue > 0)
            {
                damage = concreteData.DamageValue;
            }

            // 캐스터 스케일링 (예: 레벨, 스탯 등)
            if (scalingWithCaster)
            {
                // 여기서 캐스터의 스탯을 기반으로 데미지 스케일링 가능
                // 예시: 레벨 시스템이 있다면
                // var level = caster.GetComponent<LevelComponent>()?.Level ?? 1;
                // damage *= (1f + level * 0.1f);
            }

            return damage;
        }

        protected override bool IsValidTarget(GameObject caster, IAbilityTarget target, IAbilityData data)
        {
            if (!base.IsValidTarget(caster, target, data))
                return false;

            // 자기 자신 제외 (Self 타입이 아닌 경우)
            if (targetType != TargetType.Self && target.GameObject == caster)
                return false;

            // 팀 체크 (적대적인 대상만)
            var casterTarget = caster.GetComponent<IAbilityTarget>();
            if (casterTarget != null && target.IsFriendlyTo(casterTarget))
                return false;

            return true;
        }
    }
}
