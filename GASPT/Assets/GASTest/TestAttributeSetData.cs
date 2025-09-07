// ================================
// File: Assets/Scripts/GAS/Test/TestAttributeSetData.cs
// 테스트용 속성 데이터 설정
// ================================
using System.Collections.Generic;
using UnityEngine;
using GAS.AttributeSystem;
using static GAS.Core.GASConstants;

namespace GAS.Test
{
    /// <summary>
    /// Test attribute set data for characters
    /// </summary>
    [CreateAssetMenu(fileName = "TestAttributeSet", menuName = "GAS/Test/Test Attribute Set")]
    public class TestAttributeSetData : AttributeSetData
    {
        [Header("Test Character Attributes")]
        [SerializeField] private float testHealth = 100f;
        [SerializeField] private float testMana = 50f;
        [SerializeField] private float testStamina = 100f;
        [SerializeField] private float testAttackPower = 10f;
        [SerializeField] private float testMagicPower = 10f;
        [SerializeField] private float testDefense = 5f;

        public override void InitializeAttributes(AttributeSetComponent component)
        {
            // Health attributes
            var health = new BaseAttribute
            {
                AttributeType = AttributeType.Health,
                BaseValue = testHealth,
                CurrentValue = testHealth,
                MinValue = 0f,
                MaxValue = testHealth
            };
            component.RegisterAttribute(AttributeType.Health, health);

            var healthMax = new BaseAttribute
            {
                AttributeType = AttributeType.HealthMax,
                BaseValue = testHealth,
                CurrentValue = testHealth,
                MinValue = 1f,
                MaxValue = 9999f
            };
            component.RegisterAttribute(AttributeType.HealthMax, healthMax);

            // Mana attributes
            var mana = new BaseAttribute
            {
                AttributeType = AttributeType.Mana,
                BaseValue = testMana,
                CurrentValue = testMana,
                MinValue = 0f,
                MaxValue = testMana
            };
            component.RegisterAttribute(AttributeType.Mana, mana);

            var manaMax = new BaseAttribute
            {
                AttributeType = AttributeType.ManaMax,
                BaseValue = testMana,
                CurrentValue = testMana,
                MinValue = 0f,
                MaxValue = 9999f
            };
            component.RegisterAttribute(AttributeType.ManaMax, manaMax);

            // Stamina attributes
            var stamina = new BaseAttribute
            {
                AttributeType = AttributeType.Stamina,
                BaseValue = testStamina,
                CurrentValue = testStamina,
                MinValue = 0f,
                MaxValue = testStamina
            };
            component.RegisterAttribute(AttributeType.Stamina, stamina);

            var staminaMax = new BaseAttribute
            {
                AttributeType = AttributeType.StaminaMax,
                BaseValue = testStamina,
                CurrentValue = testStamina,
                MinValue = 0f,
                MaxValue = 9999f
            };
            component.RegisterAttribute(AttributeType.StaminaMax, staminaMax);

            // Combat attributes
            var attackPower = new BaseAttribute
            {
                AttributeType = AttributeType.AttackPower,
                BaseValue = testAttackPower,
                CurrentValue = testAttackPower,
                MinValue = 0f,
                MaxValue = 9999f
            };
            component.RegisterAttribute(AttributeType.AttackPower, attackPower);

            var magicPower = new BaseAttribute
            {
                AttributeType = AttributeType.MagicPower,
                BaseValue = testMagicPower,
                CurrentValue = testMagicPower,
                MinValue = 0f,
                MaxValue = 9999f
            };
            component.RegisterAttribute(AttributeType.MagicPower, magicPower);

            var defensePower = new BaseAttribute
            {
                AttributeType = AttributeType.DefensePower,
                BaseValue = testDefense,
                CurrentValue = testDefense,
                MinValue = 0f,
                MaxValue = 9999f
            };
            component.RegisterAttribute(AttributeType.DefensePower, defensePower);

            // Call base to register derived attributes
            base.InitializeAttributes(component);

            Debug.Log($"[TestAttributeSetData] Initialized attributes for {component.gameObject.name}");
            LogAttributeValues(component);
        }

        private void LogAttributeValues(AttributeSetComponent component)
        {
            Debug.Log($"  Health: {component.GetAttributeValue(AttributeType.Health)}/{component.GetAttributeValue(AttributeType.HealthMax)}");
            Debug.Log($"  Mana: {component.GetAttributeValue(AttributeType.Mana)}/{component.GetAttributeValue(AttributeType.ManaMax)}");
            Debug.Log($"  Stamina: {component.GetAttributeValue(AttributeType.Stamina)}/{component.GetAttributeValue(AttributeType.StaminaMax)}");
            Debug.Log($"  Attack Power: {component.GetAttributeValue(AttributeType.AttackPower)}");
            Debug.Log($"  Spell Power: {component.GetAttributeValue(AttributeType.MagicPower)}");
            Debug.Log($"  Defense: {component.GetAttributeValue(AttributeType.DefensePower)}");
        }
    }
}