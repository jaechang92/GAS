using UnityEngine;

namespace GASPT.Form
{
    /// <summary>
    /// 모든 폼이 구현해야 할 인터페이스
    /// 폼: 플레이어가 변신할 수 있는 다양한 형태 (마법사, 전사 등)
    /// </summary>
    public interface IFormController
    {
        // 기본 정보
        string FormName { get; }
        FormType FormType { get; }

        // 폼 활성화/비활성화
        void Activate();
        void Deactivate();

        // 스탯
        float MaxHealth { get; }
        float MoveSpeed { get; }
        float JumpPower { get; }

        // 스킬 관리
        void SetAbility(int slotIndex, IAbility ability);
        IAbility GetAbility(int slotIndex);
    }

    /// <summary>
    /// 폼 타입 정의
    /// </summary>
    public enum FormType
    {
        Mage,      // 마법사 - 원거리 마법 공격
        Warrior,   // 전사 - 근접 전투
        Assassin,  // 암살자 - 빠른 공격
        Tank       // 탱커 - 방어 중심
    }

    /// <summary>
    /// 어빌리티 인터페이스 (임시 - 나중에 GAS Core와 통합)
    /// </summary>
    public interface IAbility
    {
        string AbilityName { get; }
        float Cooldown { get; }
        System.Threading.Tasks.Task ExecuteAsync(GameObject caster, System.Threading.CancellationToken token);
    }
}
