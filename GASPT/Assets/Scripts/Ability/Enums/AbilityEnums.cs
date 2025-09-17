// ===================================
// ����: Assets/Scripts/Ability/Enums/AbilityEnums.cs
// ===================================
using System;

namespace AbilitySystem
{
    /// <summary>
    /// �÷����ӿ� �����Ƽ Ÿ��
    /// </summary>
    public enum PlatformerAbilityType
    {
        BasicAttack,    // �⺻ ����
        Skill,          // ��ų
        Ultimate,       // �ñر�
        Movement,       // �̵� ��ų (��� ��)
        Passive         // �нú�
    }

    /// <summary>
    /// ���� ����
    /// </summary>
    public enum AttackDirection
    {
        Forward,        // ����
        Up,            // ��
        Down,          // �Ʒ�
        Air            // ����
    }

    /// <summary>
    /// ĳ���� ����
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
    /// ���� Ÿ�� (ĳ���� Ŭ����)
    /// </summary>
    public enum SkulType
    {
        Balance,        // �뷱����
        Power,          // �Ŀ���
        Speed,          // ���ǵ���
        Range           // ���Ÿ���
    }

    /// <summary>
    /// �����Ƽ ����
    /// </summary>
    public enum AbilityState
    {
        Ready,      // ��� ����
        Active,     // ���� ��
        Cooldown,   // ��ٿ� ��
        Disabled    // ��Ȱ��ȭ
    }
}