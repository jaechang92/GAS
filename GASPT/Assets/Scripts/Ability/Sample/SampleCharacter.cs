// ===================================
// ����: Assets/Scripts/Ability/Sample/SampleCharacter.cs
// ===================================
using UnityEngine;
using System.Collections.Generic;

namespace AbilitySystem
{
    /// <summary>
    /// �׽�Ʈ�� ĳ���� ����ü
    /// </summary>
    public class SampleCharacter : MonoBehaviour, IAbilityTarget
    {
        [Header("ĳ���� ����")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float currentHealth = 100f;
        [SerializeField] private bool isInvulnerable = false;

        [Header("����Ʈ")]
        [SerializeField] private GameObject damageEffectPrefab;
        [SerializeField] private GameObject healEffectPrefab;

        // IAbilityTarget ����
        public GameObject GameObject => gameObject;
        public Transform Transform => transform;
        public bool IsAlive => currentHealth > 0;
        public bool IsTargetable => IsAlive && !isInvulnerable;

        /// <summary>
        /// ĳ���� �ʱ�ȭ
        /// </summary>
        private void Start()
        {
            // ü�� �� ������Ʈ �ʱ�ȭ
        }

        /// <summary>
        /// ������ �ޱ� (IAbilityTarget ����)
        /// </summary>
        public void TakeDamage(float damage, GameObject source)
        {
            // ������ ó�� �� ����Ʈ
        }

        /// <summary>
        /// �� �ޱ� (IAbilityTarget ����)
        /// </summary>
        public void Heal(float amount, GameObject source)
        {
            // ü�� ȸ�� ó��
        }

        /// <summary>
        /// ����/����� ���� (IAbilityTarget ����)
        /// </summary>
        public void ApplyEffect(string effectId, float duration, GameObject source)
        {
            // ���� ȿ�� ����
        }

        /// <summary>
        /// ĳ���� ��� ó��
        /// </summary>
        private void Die()
        {
            // ��� ó�� ����
        }

        /// <summary>
        /// ü�� �� ������Ʈ
        /// </summary>
        private void UpdateHealthBar()
        {
            // UI ü�¹� ����
        }

        /// <summary>
        /// ���� ���� ����
        /// </summary>
        public void SetInvulnerable(float duration)
        {
            // �Ͻ��� ���� ����
        }
    }
}