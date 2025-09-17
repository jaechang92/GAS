using UnityEngine;

namespace AbilitySystem.Platformer
{
    /// <summary>
    /// �÷����ӿ� �����Ƽ ������
    /// </summary>
    [CreateAssetMenu(fileName = "NewPlatformerAbility", menuName = "Ability/Platformer/AbilityData")]
    public class PlatformerAbilityData : ScriptableObject
    {
        [Header("�⺻ ����")]
        public string abilityId;
        public string abilityName;
        [TextArea(3, 5)]
        public string description;
        public Sprite icon;

        [Header("�����Ƽ Ÿ��")]
        public PlatformerAbilityType abilityType;
        public AttackDirection attackDirection;

        [Header("��ٿ� & �ڽ�Ʈ")]
        public float cooldownTime = 1f;
        public int manaCost = 0;

        [Header("���� ����")]
        public float damageMultiplier = 1f;
        public float range = 2f;
        public float knockbackPower = 0f;
        public bool canMoveWhileUsing = false;
        public bool cancelable = true;

        [Header("��Ʈ�ڽ�")]
        public Vector2 hitboxSize = Vector2.one;
        public Vector2 hitboxOffset = Vector2.zero;
        public float hitboxDuration = 0.2f;

        [Header("�ִϸ��̼�")]
        public string animationTrigger;
        public float animationSpeed = 1f;

        [Header("����Ʈ")]
        public GameObject effectPrefab;
        public AudioClip soundEffect;

        [Header("Ư�� ����")]
        public bool isChargeSkill = false;     // ��¡ ��ų ����
        public float maxChargeTime = 2f;       // �ִ� ��¡ �ð�
        public bool isMultiHit = false;        // �ٴ���Ʈ ����
        public int hitCount = 1;               // ��Ʈ Ƚ��

        /// <summary>
        /// �����Ƽ ��� ���� ���� üũ (�ڽ�Ʈ ����)
        /// </summary>
        public bool CanAfford(int currentMana)
        {
            // �ڽ�Ʈ�� ������ �� �ִ��� üũ
            return currentMana >= manaCost;
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