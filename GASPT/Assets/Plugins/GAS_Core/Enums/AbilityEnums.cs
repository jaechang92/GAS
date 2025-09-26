using System;

namespace GAS.Core
{
    /// <summary>
    /// 범용 어빌리티 타입
    /// </summary>
    public enum AbilityType
    {
        Active,         // 액티브 스킬
        Passive,        // 패시브 스킬
        Toggle,         // 토글 스킬
        Channeled,      // 채널링 스킬
        Instant,        // 즉시 발동
        Buff,           // 버프
        Debuff,         // 디버프
        Ultimate        // 궁극기
    }

    /// <summary>
    /// 어빌리티 상태
    /// </summary>
    public enum AbilityState
    {
        Ready,          // 사용 가능
        Casting,        // 시전 중
        Active,         // 활성 중
        Cooldown,       // 쿨다운 중
        Disabled,       // 비활성화
        Cancelled       // 취소됨
    }

    /// <summary>
    /// 타겟팅 타입
    /// </summary>
    public enum TargetType
    {
        None,           // 타겟 없음
        Self,           // 자신
        SingleTarget,   // 단일 대상
        PointTarget,    // 지점 대상
        Directional,    // 방향성
        Area,           // 범위
        AllEnemies,     // 모든 적
        AllAllies,      // 모든 아군
        Cone,           // 원뿔 범위
        Line            // 직선 범위
    }

    /// <summary>
    /// 데미지 타입
    /// </summary>
    public enum DamageType
    {
        Physical,       // 물리 데미지
        Magical,        // 마법 데미지
        True,           // 고정 데미지
        Healing,        // 힐링
        Shield,         // 실드
        Custom          // 커스텀
    }

    /// <summary>
    /// 효과 지속 타입
    /// </summary>
    public enum EffectDurationType
    {
        Instant,        // 즉시
        Duration,       // 지속 시간
        Permanent,      // 영구
        UntilRemoved,   // 제거될 때까지
        Stacks          // 스택
    }

    /// <summary>
    /// 리소스 타입 (확장 가능)
    /// </summary>
    public enum ResourceType
    {
        Health,         // 체력
        Mana,           // 마나
        Stamina,        // 스태미나
        Energy,         // 에너지
        Rage,           // 분노
        Focus,          // 집중력
        Custom          // 커스텀
    }

    /// <summary>
    /// 실행 결과
    /// </summary>
    public enum AbilityExecutionResult
    {
        Success,        // 성공
        Failed,         // 실패
        Cancelled,      // 취소
        NotEnoughResources, // 리소스 부족
        OnCooldown,     // 쿨다운 중
        InvalidTarget,  // 유효하지 않은 타겟
        OutOfRange,     // 범위 밖
        Blocked         // 차단됨
    }
}