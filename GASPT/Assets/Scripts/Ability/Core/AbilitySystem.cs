// ===================================
// ����: Assets/Scripts/Ability/Core/AbilitySystem.cs
// ===================================
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace AbilitySystem
{
    /// <summary>
    /// ĳ������ �����Ƽ�� �����ϴ� �ý���
    /// </summary>
    public class AbilitySystem : MonoBehaviour
    {
        [Header("�����Ƽ ����")]
        [SerializeField] private List<AbilityData> initialAbilities = new List<AbilityData>();

        // ��ϵ� �����Ƽ ���
        private Dictionary<string, Ability> abilities = new Dictionary<string, Ability>();

        // ĳ���� ���� ���� (�ӽ�)
        [Header("ĳ���� ����")]
        [SerializeField] private int maxMana = 100;
        [SerializeField] private int currentMana = 100;
        [SerializeField] private int maxStamina = 100;
        [SerializeField] private int currentStamina = 100;

        // ������Ƽ
        public int CurrentMana => currentMana;
        public int CurrentStamina => currentStamina;
        public IReadOnlyDictionary<string, Ability> Abilities => abilities;

        /// <summary>
        /// �ý��� �ʱ�ȭ
        /// </summary>
        private void Awake()
        {
            // �ʱ� �����Ƽ �ε�
        }

        /// <summary>
        /// �� ������ ��ٿ� ������Ʈ
        /// </summary>
        private void Update()
        {
            // ��� �����Ƽ�� ��ٿ� ������Ʈ
        }

        /// <summary>
        /// �����Ƽ ���
        /// </summary>
        public bool RegisterAbility(AbilityData abilityData)
        {
            // ���ο� �����Ƽ�� �ý��ۿ� ���
            return false;
        }

        /// <summary>
        /// �����Ƽ ����
        /// </summary>
        public bool UnregisterAbility(string abilityId)
        {
            // ��ϵ� �����Ƽ ����
            return false;
        }

        /// <summary>
        /// �����Ƽ ��� �õ�
        /// </summary>
        public async Awaitable TryUseAbility(string abilityId)
        {
            // �����Ƽ ��� ���� ���� üũ �� ����
            await Awaitable.NextFrameAsync();
        }

        /// <summary>
        /// Ư�� �����Ƽ ��������
        /// </summary>
        public Ability GetAbility(string abilityId)
        {
            // ID�� �����Ƽ �˻�
            return null;
        }

        /// <summary>
        /// ��� �����Ƽ ��������
        /// </summary>
        public List<Ability> GetAllAbilities()
        {
            // ��ϵ� ��� �����Ƽ ��ȯ
            return abilities.Values.ToList();
        }

        /// <summary>
        /// ��� ������ �����Ƽ ��� ��������
        /// </summary>
        public List<Ability> GetUsableAbilities()
        {
            // ���� ��� ������ �����Ƽ�� ���͸�
            return null;
        }

        /// <summary>
        /// �ڽ�Ʈ �Һ� ó��
        /// </summary>
        private bool ConsumeResources(AbilityData abilityData)
        {
            // ����, ���¹̳� �� �Һ�
            return false;
        }

        /// <summary>
        /// ��� �����Ƽ ��ٿ� ����
        /// </summary>
        public void ResetAllCooldowns()
        {
            // ��� �����Ƽ�� ��ٿ� �ʱ�ȭ
        }

        /// <summary>
        /// �ý��� ����
        /// </summary>
        private void OnDestroy()
        {
            // ���ҽ� ����
        }
    }
}