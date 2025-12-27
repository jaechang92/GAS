using GASPT.Core.Enums;

namespace GASPT.Core
{
    /// <summary>
    /// 대미지를 받을 수 있는 객체 인터페이스
    /// 적, 플레이어, 파괴 가능한 오브젝트 등에서 구현
    /// </summary>
    public interface IDamageable
    {
        /// <summary>
        /// 현재 체력
        /// </summary>
        float CurrentHealth { get; }

        /// <summary>
        /// 최대 체력
        /// </summary>
        float MaxHealth { get; }

        /// <summary>
        /// 생존 여부
        /// </summary>
        bool IsAlive { get; }

        /// <summary>
        /// 대미지 적용
        /// </summary>
        /// <param name="damage">대미지 양</param>
        /// <param name="damageType">대미지 타입</param>
        void TakeDamage(float damage, DamageType damageType = DamageType.Normal);

        /// <summary>
        /// 회복
        /// </summary>
        /// <param name="amount">회복량</param>
        void Heal(float amount);

        /// <summary>
        /// 무적 상태 설정
        /// </summary>
        /// <param name="invincible">무적 여부</param>
        void SetInvincible(bool invincible);

        /// <summary>
        /// 무적 상태 여부
        /// </summary>
        bool IsInvincible { get; }
    }
}
