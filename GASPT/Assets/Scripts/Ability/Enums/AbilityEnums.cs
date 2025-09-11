// ===================================
// ����: Assets/Scripts/Ability/Enums/AbilityEnums.cs
// ===================================
using System;

namespace AbilitySystem
{
    /// <summary>
    /// �����Ƽ Ÿ�� ����
    /// </summary>
    public enum AbilityType
    {
        Instant,    // �����
        Channeling, // ä�θ� (���� ����)
        Toggle,     // �����
        Passive     // �нú�
    }

    /// <summary>
    /// �����Ƽ Ÿ�� Ÿ��
    /// </summary>
    public enum TargetType
    {
        Self,       // �ڱ� �ڽ�
        Single,     // ���� ���
        Area,       // ����
        Direction   // ����
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