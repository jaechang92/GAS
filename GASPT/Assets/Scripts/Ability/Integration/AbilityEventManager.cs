// ===================================
// ����: Assets/Scripts/Ability/Integration/AbilityEventManager.cs
// ===================================
using System;
using System.Collections.Generic;
using UnityEngine;

namespace AbilitySystem
{
    /// <summary>
    /// �����Ƽ �ý��� ���� �̺�Ʈ ������
    /// </summary>
    public class AbilityEventManager : MonoBehaviour
    {
        private static AbilityEventManager instance;
        public static AbilityEventManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<AbilityEventManager>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("AbilityEventManager");
                        instance = go.AddComponent<AbilityEventManager>();
                    }
                }
                return instance;
            }
        }

        // �̺�Ʈ ����
        public event Action<Ability> OnAbilityUsed;
        public event Action<Ability> OnAbilityCooldownStarted;
        public event Action<Ability> OnAbilityCooldownCompleted;
        public event Action<string, float> OnAbilityDamageDealt;
        public event Action<string, float> OnAbilityHealDealt;
        public event Action<string> OnAbilityBlocked;
        public event Action<string> OnAbilityInterrupted;

        /// <summary>
        /// �����Ƽ ��� �̺�Ʈ �߻�
        /// </summary>
        public void TriggerAbilityUsed(Ability ability)
        {
            // �����Ƽ ��� �˸�
        }

        /// <summary>
        /// ��ٿ� ���� �̺�Ʈ �߻�
        /// </summary>
        public void TriggerCooldownStarted(Ability ability)
        {
            // ��ٿ� ���� �˸�
        }

        /// <summary>
        /// ��ٿ� �Ϸ� �̺�Ʈ �߻�
        /// </summary>
        public void TriggerCooldownCompleted(Ability ability)
        {
            // ��ٿ� �Ϸ� �˸�
        }

        /// <summary>
        /// ������ �߻� �̺�Ʈ
        /// </summary>
        public void TriggerDamageDealt(string abilityId, float damage)
        {
            // ������ ó�� �˸�
        }

        /// <summary>
        /// �� �߻� �̺�Ʈ
        /// </summary>
        public void TriggerHealDealt(string abilityId, float healAmount)
        {
            // �� ó�� �˸�
        }

        /// <summary>
        /// ��� �̺�Ʈ ������ ����
        /// </summary>
        public void ClearAllListeners()
        {
            // �̺�Ʈ ����
        }
    }
}