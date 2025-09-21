// ===================================
// ����: Assets/Scripts/Ability/Data/SkulData.cs
// ===================================
using System.Collections.Generic;
using UnityEngine;

namespace AbilitySystem.Platformer
{
    /// <summary>
    /// ����(ĳ����) ������
    /// </summary>
    [CreateAssetMenu(fileName = "NewSkul", menuName = "Ability/Platformer/SkulData")]
    public class SkulData : ScriptableObject
    {
        [Header("���� ����")]
        public string skulId;
        public string skulName;
        [TextArea(3, 5)]
        public string description;
        public Sprite icon;
        public SkulType skulType;

        [Header("�⺻ ����")]
        public float attackPower = 10f;
        public float attackSpeed = 1f;
        public float moveSpeed = 5f;
        public float jumpPower = 10f;

        [Header("�����Ƽ")]
        public PlatformerAbilityData basicAttack;      // �⺻ ����
        public PlatformerAbilityData skill1;           // ��ų 1
        public PlatformerAbilityData skill2;           // ��ų 2
        public PlatformerAbilityData dashAbility;      // ���
        public List<PlatformerAbilityData> passives;   // �нú� ���

        [Header("�޺� ����")]
        public int maxComboCount = 3;
        public float comboResetTime = 1f;
        public float[] comboDamageMultipliers = { 1f, 1.2f, 1.5f };
    }
}