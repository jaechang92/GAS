using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using GASPT.Core.Enums;
using GASPT.Core;

namespace GASPT.Gameplay.Form
{
    /// <summary>
    /// 함성 - 전사 스킬 3
    /// 자신에게 공격력 버프 적용
    ///
    /// SOLID 원칙 적용:
    /// - DIP: IStatusEffectTarget, IBuffable 인터페이스에 의존
    /// - ISP: 버프 기능만 사용하는 인터페이스 분리
    /// </summary>
    public class WarCryAbility : BaseAbility
    {
        // ====== Ability 정보 ======

        public override string AbilityName => "WarCry";
        public override float Cooldown => 15f;  // 15초 쿨다운


        // ====== 스킬 설정 ======

        private const float BuffDuration = 8f;       // 버프 지속시간
        private const float AttackBonus = 30f;       // 공격력 증가량 (%)


        // ====== 실행 ======

        public override async Task ExecuteAsync(GameObject caster, CancellationToken token)
        {
            // 쿨다운 체크
            if (!CheckCooldown())
            {
                return;
            }

            // 함성 실행
            PerformWarCry(caster);

            // 쿨다운 시작
            StartCooldown();

            // 함성 애니메이션 대기
            await Awaitable.WaitForSecondsAsync(0.5f, token);
        }

        /// <summary>
        /// 함성 실행
        /// </summary>
        private void PerformWarCry(GameObject caster)
        {
            // 자신에게 버프 적용
            ApplyAttackBuff(caster);

            // 디버그 시각화 (원형 이펙트)
            Debug.Log($"[WarCry] 전투 함성! 공격력 +{AttackBonus}% ({BuffDuration}초)");

            // TODO: 파티클 이펙트, 사운드 재생
        }

        /// <summary>
        /// 공격력 버프 적용
        /// DIP (Dependency Inversion Principle): 인터페이스에 의존
        /// </summary>
        private void ApplyAttackBuff(GameObject target)
        {
            // 1순위: IStatusEffectTarget (상태 효과 시스템)
            var statusTarget = target.GetComponent<IStatusEffectTarget>();
            if (statusTarget != null)
            {
                statusTarget.ApplyStatusEffect(StatusEffectType.AttackUp, BuffDuration, AttackBonus);
                Debug.Log($"[WarCry] {target.name}에게 AttackUp 버프 적용! (StatusEffect)");
                return;
            }

            // 2순위: IBuffable (직접 버프 시스템)
            var buffable = target.GetComponent<IBuffable>();
            if (buffable != null)
            {
                buffable.ApplyTemporaryBuff(StatType.Attack, AttackBonus, BuffDuration, isPercentage: true);
                Debug.Log($"[WarCry] {target.name}에게 AttackUp 버프 적용! (IBuffable)");
                return;
            }

            // 폴백: 버프 시스템 없음
            Debug.LogWarning($"[WarCry] {target.name}에 버프 시스템 없음 (IStatusEffectTarget/IBuffable)");
        }
    }
}
