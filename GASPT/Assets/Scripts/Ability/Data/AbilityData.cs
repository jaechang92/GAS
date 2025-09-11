// ===================================
// ����: Assets/Scripts/Ability/Data/AbilityData.cs
// ===================================
using UnityEngine;
using AbilitySystem;

namespace AbilitySystem
{
    /// <summary>
    /// �����Ƽ ���� �����͸� ��� ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "NewAbility", menuName = "Ability/AbilityData")]
    public class AbilityData : ScriptableObject
    {
        [Header("�⺻ ����")]
        public string abilityId;
        public string abilityName;
        [TextArea(3, 5)]
        public string description;
        public Sprite icon;

        [Header("�����Ƽ ����")]
        public AbilityType abilityType = AbilityType.Instant;
        public TargetType targetType = TargetType.Self;
        public float cooldownTime = 1.0f;
        public float castTime = 0f;

        [Header("�ڽ�Ʈ")]
        public int manaCost = 0;
        public int staminaCost = 0;
        public int healthCost = 0;

        [Header("ȿ��")]
        public float range = 5.0f;
        public float effectValue = 10.0f;  // ������, ���� ���� �⺻��
        public float duration = 0f;  // ����/����� ���ӽð�

        /// <summary>
        /// �����Ƽ ��� ���� ���� üũ (�ڽ�Ʈ ����)
        /// </summary>
        public bool CanAfford(int currentMana, int currentStamina, int currentHealth)
        {
            // �ڽ�Ʈ�� ������ �� �ִ��� üũ
            return currentMana >= manaCost &&
                   currentStamina >= staminaCost &&
                   currentHealth > healthCost;
        }

        /// <summary>
        /// �����Ƽ ������ ��ȿ�� ����
        /// </summary>
        public bool ValidateData()
        {
            // ������ ��ȿ�� ���� ����
            return !string.IsNullOrEmpty(abilityId) &&
                   !string.IsNullOrEmpty(abilityName) &&
                   cooldownTime >= 0;
        }
    }
}