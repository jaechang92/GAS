// ===================================
// 파일: Assets/Scripts/Ability/Enums/AbilityEnums.cs
// ===================================
using System;

namespace AbilitySystem
{
    /// <summary>
    /// 어빌리티 타입 정의
    /// </summary>
    public enum AbilityType
    {
        Instant,    // 즉발형
        Channeling, // 채널링 (시전 유지)
        Toggle,     // 토글형
        Passive     // 패시브
    }

    /// <summary>
    /// 어빌리티 타겟 타입
    /// </summary>
    public enum TargetType
    {
        Self,       // 자기 자신
        Single,     // 단일 대상
        Area,       // 범위
        Direction   // 방향
    }

    /// <summary>
    /// 어빌리티 상태
    /// </summary>
    public enum AbilityState
    {
        Ready,      // 사용 가능
        Active,     // 실행 중
        Cooldown,   // 쿨다운 중
        Disabled    // 비활성화
    }
}