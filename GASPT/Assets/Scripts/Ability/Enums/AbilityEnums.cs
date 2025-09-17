// ===================================
// 파일: Assets/Scripts/Ability/Enums/AbilityEnums.cs
// ===================================
using System;

namespace AbilitySystem
{
    /// <summary>
    /// 플랫포머용 어빌리티 타입
    /// </summary>
    public enum PlatformerAbilityType
    {
        BasicAttack,    // 기본 공격
        Skill,          // 스킬
        Ultimate,       // 궁극기
        Movement,       // 이동 스킬 (대시 등)
        Passive         // 패시브
    }

    /// <summary>
    /// 공격 방향
    /// </summary>
    public enum AttackDirection
    {
        Forward,        // 전방
        Up,            // 위
        Down,          // 아래
        Air            // 공중
    }

    /// <summary>
    /// 캐릭터 상태
    /// </summary>
    public enum CharacterState
    {
        Idle,
        Moving,
        Jumping,
        Falling,
        Attacking,
        Dashing,
        Hit,
        Dead
    }

    /// <summary>
    /// 스컬 타입 (캐릭터 클래스)
    /// </summary>
    public enum SkulType
    {
        Balance,        // 밸런스형
        Power,          // 파워형
        Speed,          // 스피드형
        Range           // 원거리형
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