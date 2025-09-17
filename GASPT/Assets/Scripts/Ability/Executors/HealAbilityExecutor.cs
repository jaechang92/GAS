// ===================================
// 파일: Assets/Scripts/Ability/Executors/HealAbilityExecutor.cs
// ===================================
using AbilitySystem.Platformer;
using System.Collections.Generic;
using UnityEngine;

namespace AbilitySystem
{
    /// <summary>
    /// 힐을 시전하는 어빌리티 실행자
    /// </summary>
    [CreateAssetMenu(fileName = "HealExecutor", menuName = "Ability/Executors/Heal")]
    public class HealAbilityExecutor : AbilityExecutor
    {
        [Header("힐 설정")]
        [SerializeField] private float healMultiplier = 1.0f;
        [SerializeField] private bool canOverheal = false;
        [SerializeField] private float overhealShieldPercent = 0.5f;

        [Header("힐 타입")]
        [SerializeField] private bool isInstant = true;
        [SerializeField] private float healOverTime = 0f;
        [SerializeField] private int tickCount = 1;

        /// <summary>
        /// 힐 어빌리티 실행
        /// </summary>
        public override async Awaitable ExecuteAsync(GameObject caster, PlatformerAbilityData data, List<IAbilityTarget> targets)
        {
            // 타겟에게 힐 적용
            await AwaitableHelper.CompletedTask;
        }

        /// <summary>
        /// 힐량 계산
        /// </summary>
        private float CalculateHealAmount(GameObject caster, PlatformerAbilityData data, IAbilityTarget target)
        {
            // 최종 힐량 계산
            return 0f;
        }

        /// <summary>
        /// 즉시 힐 적용
        /// </summary>
        private void ApplyInstantHeal(GameObject caster, IAbilityTarget target, float amount)
        {
            // 즉시 체력 회복
        }

        /// <summary>
        /// 도트힐 적용
        /// </summary>
        private async Awaitable ApplyHealOverTime(GameObject caster, IAbilityTarget target, float totalAmount)
        {
            // 시간에 걸쳐 체력 회복
            await AwaitableHelper.CompletedTask;
        }

        /// <summary>
        /// 오버힐 처리
        /// </summary>
        private void HandleOverheal(IAbilityTarget target, float overhealAmount)
        {
            // 초과 힐을 보호막으로 변환
        }

        /// <summary>
        /// 힐 이펙트 표시
        /// </summary>
        private void ShowHealEffect(Vector3 position, float amount)
        {
            // 힐 시각 효과 및 숫자 표시
        }
    }
}