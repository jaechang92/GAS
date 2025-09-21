using System.Collections.Generic;

namespace GAS.Core
{
    /// <summary>
    /// 어빌리티 데이터의 인터페이스
    /// </summary>
    public interface IAbilityData
    {
        // 기본 정보
        string AbilityId { get; }
        string AbilityName { get; }
        string Description { get; }
        AbilityType AbilityType { get; }

        // 실행 설정
        float CooldownDuration { get; }
        float CastTime { get; }
        float Duration { get; }
        bool CanBeCancelled { get; }

        // 리소스 비용
        Dictionary<string, float> ResourceCosts { get; }

        // 태그
        List<string> AbilityTags { get; }
        List<string> CancelTags { get; }
        List<string> BlockTags { get; }

        // 범위/타겟팅
        float Range { get; }
        TargetType TargetType { get; }
    }
}