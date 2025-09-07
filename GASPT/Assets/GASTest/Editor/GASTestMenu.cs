// ================================
// File: Assets/Scripts/GAS/Editor/GASTestMenu.cs
// Unity Editor 메뉴 추가
// ================================
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using GAS.AbilitySystem.Abilities;
using GAS.Test;

namespace GAS.Editor
{
    /// <summary>
    /// Editor menu items for GAS testing
    /// </summary>
    public static class GASTestMenu
    {
        [MenuItem("GAS/Test/Setup Test Scene", false, 100)]
        public static void SetupTestScene()
        {
            Debug.Log("Setting up GAS Test Scene...");

            // Check if we're in a scene
            if (EditorSceneManager.GetActiveScene().name == "")
            {
                EditorUtility.DisplayDialog("No Scene", "Please create or open a scene first.", "OK");
                return;
            }

            // Check if setup already exists
            var existingSetup = GameObject.FindFirstObjectByType<GASTestSceneSetup>();
            if (existingSetup != null)
            {
                EditorUtility.DisplayDialog("Setup Exists", "Test scene setup already exists. Use the component to reset.", "OK");
                return;
            }

            // Create setup GameObject
            GameObject setupObj = new GameObject("GAS_TestSceneSetup");
            var setup = setupObj.AddComponent<GASTestSceneSetup>();

            // Create test ability if it doesn't exist
            CreateTestAbility();

            // Create test attribute data if it doesn't exist
            CreateTestAttributeData();

            // Auto-setup
            setup.SetupTestScene();

            // Select the setup object
            Selection.activeGameObject = setupObj;

            EditorUtility.DisplayDialog("Setup Complete",
                "GAS Test Scene has been set up!\n\n" +
                "Press Play to test:\n" +
                " WASD to move\n" +
                " Press 1 to use ability\n" +
                " Click on enemy to target\n" +
                " Tab for debug info",
                "OK");
        }

        [MenuItem("GAS/Test/Create Test Ability", false, 101)]
        public static void CreateTestAbility()
        {
            // Check if test ability already exists
            string[] guids = AssetDatabase.FindAssets("t:SimpleDamageAbility");
            if (guids.Length > 0)
            {
                Debug.Log("Test ability already exists");
                return;
            }

            // Create folder if it doesn't exist
            if (!AssetDatabase.IsValidFolder("Assets/ScriptableObjects"))
            {
                AssetDatabase.CreateFolder("Assets", "ScriptableObjects");
            }
            if (!AssetDatabase.IsValidFolder("Assets/ScriptableObjects/Abilities"))
            {
                AssetDatabase.CreateFolder("Assets/ScriptableObjects", "Abilities");
            }

            // Create ability
            SimpleDamageAbility ability = ScriptableObject.CreateInstance<SimpleDamageAbility>();

            // Set default values via reflection (since fields are serialized)
            var type = ability.GetType();
            var baseType = type.BaseType;

            // Set base ability fields
            SetPrivateField(baseType, ability, "abilityId", "ABILITY_TEST_DAMAGE");
            SetPrivateField(baseType, ability, "abilityName", "Test Damage");
            SetPrivateField(baseType, ability, "description", "A simple damage ability for testing");
            SetPrivateField(baseType, ability, "maxLevel", 5);

            // Set cost
            var costData = new GAS.AbilitySystem.AbilityCostData(
                GAS.Core.GASConstants.AttributeType.Mana,
                10f
            );
            var costs = new System.Collections.Generic.List<GAS.AbilitySystem.AbilityCostData> { costData };
            SetPrivateField(baseType, ability, "costs", costs);

            // Set cooldown
            var cooldownData = new GAS.AbilitySystem.AbilityCooldownData(3f);
            SetPrivateField(baseType, ability, "cooldown", cooldownData);

            // Set damage ability specific fields
            SetPrivateField(type, ability, "baseDamage", 20f);
            SetPrivateField(type, ability, "damagePerLevel", 5f);
            SetPrivateField(type, ability, "range", 10f);
            SetPrivateField(type, ability, "debugMode", true);

            // Save asset
            string path = "Assets/ScriptableObjects/Abilities/TestDamageAbility.asset";
            AssetDatabase.CreateAsset(ability, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Select the created asset
            Selection.activeObject = ability;

            Debug.Log($"Created test ability at: {path}");
        }

        [MenuItem("GAS/Test/Create Test Attribute Data", false, 102)]
        public static void CreateTestAttributeData()
        {
            // Check if test attribute data already exists
            string[] guids = AssetDatabase.FindAssets("t:TestAttributeSetData");
            if (guids.Length > 0)
            {
                Debug.Log("Test attribute data already exists");
                return;
            }

            // Create folder if it doesn't exist
            if (!AssetDatabase.IsValidFolder("Assets/ScriptableObjects"))
            {
                AssetDatabase.CreateFolder("Assets", "ScriptableObjects");
            }
            if (!AssetDatabase.IsValidFolder("Assets/ScriptableObjects/Attributes"))
            {
                AssetDatabase.CreateFolder("Assets/ScriptableObjects", "Attributes");
            }

            // Create attribute data
            var attributeData = ScriptableObject.CreateInstance<TestAttributeSetData>();

            // Save asset
            string path = "Assets/ScriptableObjects/Attributes/TestAttributeSet.asset";
            AssetDatabase.CreateAsset(attributeData, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Select the created asset
            Selection.activeObject = attributeData;

            Debug.Log($"Created test attribute data at: {path}");
        }

        [MenuItem("GAS/Test/Clean Test Scene", false, 103)]
        public static void CleanTestScene()
        {
            var setup = GameObject.FindObjectOfType<GASTestSceneSetup>();
            if (setup != null)
            {
                setup.CleanUpTestScene();
                GameObject.DestroyImmediate(setup.gameObject);
            }

            // Clean up any remaining test objects
            var testObjects = new string[] { "Player", "Enemy", "TestUI_Canvas", "Ground" };
            foreach (var objName in testObjects)
            {
                var obj = GameObject.Find(objName);
                if (obj != null)
                {
                    GameObject.DestroyImmediate(obj);
                }
            }

            Debug.Log("Test scene cleaned");
        }

        [MenuItem("GAS/Documentation", false, 200)]
        public static void OpenDocumentation()
        {
            Application.OpenURL("https://github.com/yourusername/gas-unity-docs");
        }

        // Helper method to set private fields via reflection
        private static void SetPrivateField(System.Type type, object obj, string fieldName, object value)
        {
            var field = type.GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(obj, value);
            }
            else
            {
                Debug.LogWarning($"Could not find field: {fieldName}");
            }
        }
    }
}
#endif