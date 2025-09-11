// ===================================
// ����: Assets/Scripts/Ability/Sample/SampleAbilitySetup.cs
// ===================================
using UnityEngine;
using System.Collections.Generic;

namespace AbilitySystem
{
    /// <summary>
    /// �׽�Ʈ�� �����Ƽ �¾� ����
    /// </summary>
    public class SampleAbilitySetup : MonoBehaviour
    {
        [Header("�׽�Ʈ ����")]
        [SerializeField] private AbilitySystem playerAbilitySystem;
        [SerializeField] private AbilityController abilityController;
        [SerializeField] private List<AbilityData> testAbilities;

        [Header("�׽�Ʈ ��Ʈ��")]
        [SerializeField] private bool autoSetupOnStart = true;
        [SerializeField] private bool enableDebugKeys = true;

        /// <summary>
        /// �ڵ� �¾�
        /// </summary>
        private void Start()
        {
            // �׽�Ʈ �����Ƽ �ڵ� ���
        }

        /// <summary>
        /// �׽�Ʈ �����Ƽ ���
        /// </summary>
        public void SetupTestAbilities()
        {
            // �̸� ���ǵ� �����Ƽ ���
        }

        /// <summary>
        /// �⺻ ���̾ ����
        /// </summary>
        private AbilityData CreateFireballAbility()
        {
            // ���̾ �����Ƽ ������ ����
            return null;
        }

        /// <summary>
        /// �⺻ �� ����
        /// </summary>
        private AbilityData CreateHealAbility()
        {
            // �� �����Ƽ ������ ����
            return null;
        }

        /// <summary>
        /// ����� Ű ó��
        /// </summary>
        private void Update()
        {
            // �׽�Ʈ�� ����Ű ó��
        }

        /// <summary>
        /// ��� ��ٿ� ���� (�׽�Ʈ��)
        /// </summary>
        [ContextMenu("Reset All Cooldowns")]
        public void ResetAllCooldowns()
        {
            // ����׿� ��ٿ� �ʱ�ȭ
        }

        /// <summary>
        /// ���� ���ҽ� ��� (�׽�Ʈ��)
        /// </summary>
        [ContextMenu("Toggle Infinite Resources")]
        public void ToggleInfiniteResources()
        {
            // ����׿� ���� ����/���¹̳�
        }

        /// <summary>
        /// ���� �׽�Ʈ
        /// </summary>
        [ContextMenu("Run Performance Test")]
        public void RunPerformanceTest()
        {
            // �ټ� �����Ƽ ���� ���� �׽�Ʈ
        }
    }
}