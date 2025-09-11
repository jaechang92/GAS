// ===================================
// 파일: Assets/Scripts/Ability/Executors/DamageAbilityExecutor.cs
// ===================================
using System.Collections.Generic;
using UnityEngine;

namespace AbilitySystem
{
    /// <summary>
    /// 데미지를 입히는 어빌리티 실행자
    /// </summary>
    [CreateAssetMenu(fileName = "DamageExecutor", menuName = "Ability/Executors/Damage")]
    public class DamageAbilityExecutor : AbilityExecutor
    {
        [Header("데미지 설정")]
        [SerializeField] private float damageMultiplier = 1.0f;
        [SerializeField] private bool canCritical = true;
        [SerializeField] private float criticalChance = 0.1f;
        [SerializeField] private float criticalMultiplier = 2.0f;

        [Header("타격 설정")]
        [SerializeField] private bool isPiercing = false;  // 관통 여부
        [SerializeField] private int maxTargets = 1;       // 최대 타겟 수
        [SerializeField] private float knockbackForce = 0f;

        /// <summary>
        /// 데미지 어빌리티 실행
        /// </summary>
        public override async Awaitable ExecuteAsync(GameObject caster, AbilityData data, List<IAbilityTarget> targets)
        {
            // 타겟에게 데미지 적용
            await AwaitableHelper.CompletedTask;
        }

        /// <summary>
        /// 데미지 계산
        /// </summary>
        private float CalculateDamage(GameObject caster, AbilityData data, IAbilityTarget target)
        {
            // 최종 데미지 계산 (기본 데미지 * 배율 * 크리티컬 등)
            return 0f;
        }

        /// <summary>
        /// 크리티컬 판정
        /// </summary>
        private bool RollCritical()
        {
            // 크리티컬 확률 계산
            return false;
        }

        /// <summary>
        /// 넉백 적용
        /// </summary>
        private void ApplyKnockback(GameObject caster, IAbilityTarget target)
        {
            // 타겟에게 넉백 효과 적용
        }

        /// <summary>
        /// 데미지 숫자 표시
        /// </summary>
        private void ShowDamageNumber(Vector3 position, float damage, bool isCritical)
        {
            // UI로 데미지 숫자 표시
        }
    }
}