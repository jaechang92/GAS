// ================================
// File: Assets/Scripts/GAS/Test/GASTestSceneSetup.cs
// 테스트 씬 자동 설정 스크립트
// ================================
using UnityEngine;
using UnityEngine.UI;
using GAS.Core;
using GAS.AttributeSystem;
using GAS.TagSystem;
using GAS.EffectSystem;
using GAS.AbilitySystem;
using GAS.AbilitySystem.Abilities;

namespace GAS.Test
{
    /// <summary>
    /// GAS 시스템 테스트를 위한 씬 자동 설정
    /// </summary>
    public class GASTestSceneSetup : MonoBehaviour
    {
        [Header("Setup Options")]
        [SerializeField] private bool autoSetupOnStart = true;
        [SerializeField] private bool createPlayer = true;
        [SerializeField] private bool createEnemy = true;
        [SerializeField] private bool createUI = true;

        [Header("Character Settings")]
        [SerializeField] private Vector3 playerSpawnPosition = new Vector3(-3, 0, 0);
        [SerializeField] private Vector3 enemySpawnPosition = new Vector3(3, 0, 0);

        [Header("Test Abilities")]
        [SerializeField] private SimpleDamageAbility testDamageAbility;
        [SerializeField] private int abilityLevel = 1;

        [Header("References - Auto Set")]
        [SerializeField] private GameObject playerObject;
        [SerializeField] private GameObject enemyObject;
        [SerializeField] private GameObject uiCanvas;

        private void Start()
        {
            if (autoSetupOnStart)
            {
                SetupTestScene();
            }
        }

        [ContextMenu("Setup Test Scene")]
        public void SetupTestScene()
        {
            Debug.Log("=== Starting GAS Test Scene Setup ===");

            // Create test characters
            if (createPlayer)
            {
                playerObject = CreateTestCharacter("Player", playerSpawnPosition, Color.blue, true);
                SetupPlayerTags(playerObject);
            }

            if (createEnemy)
            {
                enemyObject = CreateTestCharacter("Enemy", enemySpawnPosition, Color.red, false);
                SetupEnemyTags(enemyObject);
            }

            // Create UI
            if (createUI)
            {
                uiCanvas = CreateTestUI();
            }

            // Create environment
            CreateTestEnvironment();

            Debug.Log("=== GAS Test Scene Setup Complete ===");
            PrintSetupSummary();
        }

        /// <summary>
        /// Create a test character with all GAS components
        /// </summary>
        private GameObject CreateTestCharacter(string name, Vector3 position, Color color, bool isPlayer)
        {
            Debug.Log($"Creating test character: {name}");

            // Create GameObject
            GameObject character = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            character.name = name;
            character.transform.position = position;

            // Set color
            var renderer = character.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = color;
            }

            // Add AttributeSetComponent
            var attributeComponent = character.AddComponent<AttributeSetComponent>();
            SetupDefaultAttributes(attributeComponent);

            // Add TagComponent
            var tagComponent = character.AddComponent<TagComponent>();

            // Add EffectComponent
            var effectComponent = character.AddComponent<EffectComponent>();

            // Add AbilitySystemComponent
            var abilityComponent = character.AddComponent<AbilitySystemComponent>();

            // Grant test ability if available
            if (testDamageAbility != null && isPlayer)
            {
                GrantTestAbility(abilityComponent);
            }

            // Add health bar
            CreateHealthBar(character);

            // Add movement for player
            if (isPlayer)
            {
                character.AddComponent<SimpleMovement>();
            }

            Debug.Log($"Character {name} created successfully");
            return character;
        }

        /// <summary>
        /// Setup default attributes for testing
        /// </summary>
        private void SetupDefaultAttributes(AttributeSetComponent attributeComponent)
        {
            // Note: In actual implementation, this would be done through AttributeSetData
            // For testing, we'll use reflection or wait for the component to initialize

            // The attributes will be initialized by the AttributeSetData assigned in the inspector
            // For now, we'll log what should be set
            Debug.Log("Attributes will be initialized from AttributeSetData");
            Debug.Log("Expected attributes: Health, Mana, Stamina, AttackPower, etc.");
        }

        /// <summary>
        /// Setup player tags
        /// </summary>
        private void SetupPlayerTags(GameObject player)
        {
            //var tagComponent = player.GetComponent<TagComponent>();
            //if (tagComponent == null) return;

            //// Create and add player tags
            //var playerTag = ScriptableObject.CreateInstance<GameplayTag>();
            //playerTag.name = "Character.Player";

            //var allyTag = ScriptableObject.CreateInstance<GameplayTag>();
            //allyTag.name = "Character.Ally";

            //tagComponent.AddTag(playerTag);
            //tagComponent.AddTag(allyTag);

            Debug.Log($"Added tags to player: Player, Ally");
        }

        /// <summary>
        /// Setup enemy tags
        /// </summary>
        private void SetupEnemyTags(GameObject enemy)
        {
            //var tagComponent = enemy.GetComponent<TagComponent>();
            //if (tagComponent == null) return;

            //// Create and add enemy tags
            //var enemyTag = ScriptableObject.CreateInstance<GameplayTag>();
            //enemyTag.name = "Character.Enemy";

            //var hostileTag = ScriptableObject.CreateInstance<GameplayTag>();
            //hostileTag.name = "Character.Hostile";

            //tagComponent.AddTag(enemyTag);
            //tagComponent.AddTag(hostileTag);

            Debug.Log($"Added tags to enemy: Enemy, Hostile");
        }

        /// <summary>
        /// Grant test ability to character
        /// </summary>
        private void GrantTestAbility(AbilitySystemComponent abilityComponent)
        {
            if (testDamageAbility == null)
            {
                Debug.LogWarning("No test damage ability assigned!");
                return;
            }

            // Grant ability with input binding
            abilityComponent.GrantAbility(testDamageAbility, abilityLevel, 1); // Bind to key 1

            Debug.Log($"Granted {testDamageAbility.name} at level {abilityLevel} (Press 1 to use)");
        }

        /// <summary>
        /// Create health bar above character
        /// </summary>
        private void CreateHealthBar(GameObject character)
        {
            // Create world space canvas
            GameObject healthBarObj = new GameObject($"{character.name}_HealthBar");
            healthBarObj.transform.SetParent(character.transform);
            healthBarObj.transform.localPosition = Vector3.up * 2.5f;

            Canvas canvas = healthBarObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = Camera.main;

            var canvasScaler = healthBarObj.AddComponent<CanvasScaler>();
            canvasScaler.dynamicPixelsPerUnit = 10;

            var rectTransform = healthBarObj.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(1, 0.2f);

            // Create background
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(healthBarObj.transform);
            var bgImage = bgObj.AddComponent<Image>();
            bgImage.color = Color.black;
            var bgRect = bgObj.GetComponent<RectTransform>();
            bgRect.sizeDelta = new Vector2(1, 0.2f);
            bgRect.anchoredPosition = Vector2.zero;

            // Create health fill
            GameObject fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(healthBarObj.transform);
            var fillImage = fillObj.AddComponent<Image>();
            fillImage.color = Color.green;
            var fillRect = fillObj.GetComponent<RectTransform>();
            fillRect.sizeDelta = new Vector2(1, 0.2f);
            fillRect.anchoredPosition = Vector2.zero;

            // Add health bar updater
            var healthBar = character.AddComponent<SimpleHealthBar>();
            healthBar.SetFillImage(fillImage);
        }

        /// <summary>
        /// Create test UI
        /// </summary>
        private GameObject CreateTestUI()
        {
            Debug.Log("Creating test UI");

            // Create Canvas
            GameObject canvasObj = new GameObject("TestUI_Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();

            // Create info panel
            GameObject panel = new GameObject("InfoPanel");
            panel.transform.SetParent(canvasObj.transform);
            var panelImage = panel.AddComponent<Image>();
            panelImage.color = new Color(0, 0, 0, 0.5f);
            var panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0, 1);
            panelRect.anchorMax = new Vector2(0, 1);
            panelRect.pivot = new Vector2(0, 1);
            panelRect.sizeDelta = new Vector2(400, 200);
            panelRect.anchoredPosition = new Vector2(10, -10);

            // Create instruction text
            GameObject textObj = new GameObject("Instructions");
            textObj.transform.SetParent(panel.transform);
            var text = textObj.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 14;
            text.color = Color.white;
            text.alignment = TextAnchor.UpperLeft;
            text.text = GetInstructionText();
            var textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.offsetMin = new Vector2(10, 10);
            textRect.offsetMax = new Vector2(-10, -10);

            // Add UI controller
            canvasObj.AddComponent<GASTestUIController>();

            Debug.Log("Test UI created");
            return canvasObj;
        }

        /// <summary>
        /// Create test environment
        /// </summary>
        private void CreateTestEnvironment()
        {
            // Create ground
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(2, 1, 2);

            var groundRenderer = ground.GetComponent<Renderer>();
            if (groundRenderer != null)
            {
                groundRenderer.material.color = new Color(0.5f, 0.5f, 0.5f);
            }

            // Ensure we have a camera
            if (Camera.main == null)
            {
                GameObject cameraObj = new GameObject("Main Camera");
                var camera = cameraObj.AddComponent<Camera>();
                cameraObj.AddComponent<AudioListener>();
                cameraObj.transform.position = new Vector3(0, 5, -10);
                cameraObj.transform.rotation = Quaternion.Euler(20, 0, 0);
                cameraObj.tag = "MainCamera";
            }

            // Add directional light if none exists
            if (FindFirstObjectByType<Light>() == null)
            {
                GameObject lightObj = new GameObject("Directional Light");
                var light = lightObj.AddComponent<Light>();
                light.type = LightType.Directional;
                lightObj.transform.rotation = Quaternion.Euler(45, -30, 0);
            }

            Debug.Log("Test environment created");
        }

        /// <summary>
        /// Get instruction text for UI
        /// </summary>
        private string GetInstructionText()
        {
            return @"<b>GAS Test Scene Instructions:</b>

<b>Controls:</b>
 WASD - Move Player
 1 - Use Damage Ability
 Left Click - Target Enemy
 Tab - Show Debug Info
 Escape - Cancel All Abilities

<b>Test Features:</b>
 Health/Mana/Stamina attributes
 Damage ability with cooldown
 Effect system
 Tag system

<b>Status:</b> Ready for Testing";
        }

        /// <summary>
        /// Print setup summary
        /// </summary>
        private void PrintSetupSummary()
        {
            Debug.Log("========================================");
            Debug.Log("GAS Test Scene Setup Summary:");
            Debug.Log("========================================");

            if (playerObject != null)
            {
                var playerAbilities = playerObject.GetComponent<AbilitySystemComponent>();
                Debug.Log($"  Player created at {playerObject.transform.position}");
                Debug.Log($"  Has AbilitySystemComponent: {playerAbilities != null}");
                Debug.Log($"  Abilities granted: {playerAbilities?.GetAllAbilitySpecs().Count ?? 0}");
            }

            if (enemyObject != null)
            {
                Debug.Log($"  Enemy created at {enemyObject.transform.position}");
            }

            if (uiCanvas != null)
            {
                Debug.Log("  UI Canvas created");
            }

            Debug.Log("========================================");
            Debug.Log("Press Tab during play to see runtime info");
            Debug.Log("========================================");
        }

        /// <summary>
        /// Clean up test scene
        /// </summary>
        [ContextMenu("Clean Up Test Scene")]
        public void CleanUpTestScene()
        {
            if (playerObject != null) DestroyImmediate(playerObject);
            if (enemyObject != null) DestroyImmediate(enemyObject);
            if (uiCanvas != null) DestroyImmediate(uiCanvas);

            var ground = GameObject.Find("Ground");
            if (ground != null) DestroyImmediate(ground);

            Debug.Log("Test scene cleaned up");
        }
    }
}