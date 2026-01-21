using UnityEngine;

namespace GASPT.Gameplay.Form
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

    // FormType은 FormEnums.cs에 정의됨

    /// <summary>
    /// 어빌리티 인터페이스
    /// </summary>
    public interface IAbility
    {
        string AbilityName { get; }
        float Cooldown { get; }
        System.Threading.Tasks.Task ExecuteAsync(GameObject caster, System.Threading.CancellationToken token);

        // 확장 속성 (선택적)
        int BaseDamage { get; }
        float BaseRange { get; }
        int ManaCost { get; }
        bool IsReady { get; }
        float RemainingCooldown { get; }
        float CooldownProgress { get; }
    }
}
