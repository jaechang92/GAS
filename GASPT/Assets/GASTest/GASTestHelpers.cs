// ================================
// File: Assets/Scripts/GAS/Test/GASTestHelpers.cs
// 테스트를 위한 보조 컴포넌트들
// ================================
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GAS.Core;
using GAS.AttributeSystem;
using GAS.AbilitySystem;
using static GAS.Core.GASConstants;

namespace GAS.Test
{
    /// <summary>
    /// Simple character movement for testing
    /// </summary>
    public class SimpleMovement : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float rotateSpeed = 180f;

        private void Update()
        {
            // Get input
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            // Move
            Vector3 movement = new Vector3(horizontal, 0, vertical);
            movement = movement.normalized * moveSpeed * Time.deltaTime;
            transform.Translate(movement, Space.World);

            // Rotate to face movement direction
            if (movement.magnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(movement);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);
            }
        }
    }

    /// <summary>
    /// Simple health bar display
    /// </summary>
    public class SimpleHealthBar : MonoBehaviour
    {
        private Image fillImage;
        private AttributeSetComponent attributeComponent;
        private float maxHealth;

        private void Start()
        {
            attributeComponent = GetComponent<AttributeSetComponent>();
            if (attributeComponent != null)
            {
                maxHealth = attributeComponent.GetAttributeValue(AttributeType.HealthMax);
                attributeComponent.OnAttributeChanged += OnAttributeChanged;
            }
        }

        private void OnDestroy()
        {
            if (attributeComponent != null)
            {
                attributeComponent.OnAttributeChanged -= OnAttributeChanged;
            }
        }

        public void SetFillImage(Image image)
        {
            fillImage = image;
        }

        private void OnAttributeChanged(AttributeType type, float oldValue, float newValue)
        {
            if (type == AttributeType.Health && fillImage != null)
            {
                UpdateHealthBar(newValue);
            }
            else if (type == AttributeType.HealthMax)
            {
                maxHealth = newValue;
                float currentHealth = attributeComponent.GetAttributeValue(AttributeType.Health);
                UpdateHealthBar(currentHealth);
            }
        }

        private void UpdateHealthBar(float currentHealth)
        {
            if (fillImage == null || maxHealth <= 0) return;

            float healthPercent = currentHealth / maxHealth;
            fillImage.fillAmount = healthPercent;

            // Change color based on health
            if (healthPercent > 0.6f)
                fillImage.color = Color.green;
            else if (healthPercent > 0.3f)
                fillImage.color = Color.yellow;
            else
                fillImage.color = Color.red;
        }
    }

    /// <summary>
    /// UI Controller for test scene
    /// </summary>
    public class GASTestUIController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Text statusText;
        [SerializeField] private Text abilityText;
        [SerializeField] private Text attributeText;

        private AbilitySystemComponent playerAbilitySystem;
        private AttributeSetComponent playerAttributes;
        private GameObject player;

        private void Start()
        {
            // Find player
            player = GameObject.Find("Player");
            if (player != null)
            {
                playerAbilitySystem = player.GetComponent<AbilitySystemComponent>();
                playerAttributes = player.GetComponent<AttributeSetComponent>();

                // Subscribe to events
                if (playerAbilitySystem != null)
                {
                    playerAbilitySystem.AbilityActivated += OnAbilityActivated;
                    playerAbilitySystem.AbilityCooldownStarted += OnCooldownStarted;
                    playerAbilitySystem.AbilityCooldownEnded += OnCooldownEnded;
                }
            }

            // Create status display if not exists
            if (statusText == null)
            {
                CreateStatusDisplay();
            }
        }

        private void OnDestroy()
        {
            if (playerAbilitySystem != null)
            {
                playerAbilitySystem.AbilityActivated -= OnAbilityActivated;
                playerAbilitySystem.AbilityCooldownStarted -= OnCooldownStarted;
                playerAbilitySystem.AbilityCooldownEnded -= OnCooldownEnded;
            }
        }

        private void Update()
        {
            UpdateStatusDisplay();

            // Handle target selection
            if (Input.GetMouseButtonDown(0))
            {
                SelectTarget();
            }
        }

        private void CreateStatusDisplay()
        {
            // Create status text at bottom of screen
            GameObject statusObj = new GameObject("StatusText");
            statusObj.transform.SetParent(transform);
            statusText = statusObj.AddComponent<Text>();
            statusText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            statusText.fontSize = 16;
            statusText.color = Color.white;
            statusText.alignment = TextAnchor.LowerCenter;

            var rect = statusObj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0);
            rect.anchorMax = new Vector2(0.5f, 0);
            rect.pivot = new Vector2(0.5f, 0);
            rect.sizeDelta = new Vector2(600, 100);
            rect.anchoredPosition = new Vector2(0, 20);
        }

        private void UpdateStatusDisplay()
        {
            if (statusText == null || player == null) return;

            string status = "";

            // Show attributes
            if (playerAttributes != null)
            {
                float health = playerAttributes.GetAttributeValue(AttributeType.Health);
                float maxHealth = playerAttributes.GetAttributeValue(AttributeType.HealthMax);
                float mana = playerAttributes.GetAttributeValue(AttributeType.Mana);
                float maxMana = playerAttributes.GetAttributeValue(AttributeType.ManaMax);

                status += $"<b>Health:</b> {health:F0}/{maxHealth:F0} | ";
                status += $"<b>Mana:</b> {mana:F0}/{maxMana:F0}\n";
            }

            // Show abilities
            if (playerAbilitySystem != null)
            {
                var specs = playerAbilitySystem.GetAllAbilitySpecs();
                foreach (var spec in specs)
                {
                    string abilityStatus = spec.IsActive ? "<color=green>ACTIVE</color>" :
                                         spec.IsOnCooldown ? $"<color=red>CD: {spec.RemainingCooldown:F1}s</color>" :
                                         "<color=white>READY</color>";

                    status += $"[{spec.inputId}] {spec.ability.AbilityName}: {abilityStatus} | ";
                }
            }

            statusText.text = status;
        }

        private void SelectTarget()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                GameObject target = hit.collider.gameObject;

                // Check if it's a valid target (has attributes)
                if (target.GetComponent<AttributeSetComponent>() != null && target != player)
                {
                    // Try to activate ability with this target
                    if (playerAbilitySystem != null)
                    {
                        var specs = playerAbilitySystem.GetAllAbilitySpecs();
                        if (specs.Count > 0)
                        {
                            _ = playerAbilitySystem.TryActivateAbility(specs[0].ability, target);
                            Debug.Log($"Targeting: {target.name}");
                        }
                    }

                    // Visual feedback
                    HighlightTarget(target);
                }
            }
        }

        private void HighlightTarget(GameObject target)
        {
            var renderer = target.GetComponent<Renderer>();
            if (renderer != null)
            {
                // Flash the target
                Color originalColor = renderer.material.color;
                renderer.material.color = Color.white;

                // Reset color after delay
                StartCoroutine(ResetColorAfterDelay(renderer, originalColor, 0.2f));
            }
        }

        private System.Collections.IEnumerator ResetColorAfterDelay(Renderer renderer, Color color, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (renderer != null)
                renderer.material.color = color;
        }

        private void OnAbilityActivated(AbilitySpec spec)
        {
            Debug.Log($"[UI] Ability Activated: {spec.ability.AbilityName}");
            ShowFloatingText($"Activated: {spec.ability.AbilityName}", Color.green);
        }

        private void OnCooldownStarted(AbilitySpec spec)
        {
            Debug.Log($"[UI] Cooldown Started: {spec.ability.AbilityName}");
        }

        private void OnCooldownEnded(AbilitySpec spec)
        {
            Debug.Log($"[UI] Cooldown Ended: {spec.ability.AbilityName}");
            ShowFloatingText($"Ready: {spec.ability.AbilityName}", Color.cyan);
        }

        private void ShowFloatingText(string text, Color color)
        {
            if (player == null) return;

            // Create floating text
            GameObject floatingTextObj = new GameObject("FloatingText");
            floatingTextObj.transform.position = player.transform.position + Vector3.up * 2;

            // Add world canvas
            Canvas canvas = floatingTextObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = Camera.main;

            var rectTransform = floatingTextObj.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(2, 1);

            // Add text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(floatingTextObj.transform);
            var textComponent = textObj.AddComponent<Text>();
            textComponent.text = text;
            textComponent.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            textComponent.fontSize = 20;
            textComponent.color = color;
            textComponent.alignment = TextAnchor.MiddleCenter;

            var textRect = textObj.GetComponent<RectTransform>();
            textRect.sizeDelta = new Vector2(2, 1);
            textRect.anchoredPosition = Vector2.zero;

            // Animate and destroy
            floatingTextObj.AddComponent<FloatingTextAnimation>();
        }
    }

    /// <summary>
    /// Simple floating text animation
    /// </summary>
    public class FloatingTextAnimation : MonoBehaviour
    {
        private float lifetime = 2f;
        private float moveSpeed = 1f;
        private float fadeSpeed = 1f;
        private Text textComponent;

        private void Start()
        {
            textComponent = GetComponentInChildren<Text>();
            Destroy(gameObject, lifetime);
        }

        private void Update()
        {
            // Move up
            transform.position += Vector3.up * moveSpeed * Time.deltaTime;

            // Fade out
            if (textComponent != null)
            {
                Color color = textComponent.color;
                color.a -= fadeSpeed * Time.deltaTime;
                textComponent.color = color;
            }
        }
    }

    /// <summary>
    /// Target dummy for testing
    /// </summary>
    public class TargetDummy : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private bool autoHeal = true;
        [SerializeField] private float healAmount = 10f;
        [SerializeField] private float healInterval = 5f;

        private AttributeSetComponent attributes;
        private float lastHealTime;

        private void Start()
        {
            attributes = GetComponent<AttributeSetComponent>();
        }

        private void Update()
        {
            if (autoHeal && attributes != null)
            {
                if (Time.time - lastHealTime > healInterval)
                {
                    float currentHealth = attributes.GetAttributeValue(AttributeType.Health);
                    float maxHealth = attributes.GetAttributeValue(AttributeType.HealthMax);

                    if (currentHealth < maxHealth)
                    {
                        attributes.ModifyAttribute(AttributeType.Health, healAmount, ModifierOperation.Add);
                        lastHealTime = Time.time;
                        Debug.Log($"{gameObject.name} healed for {healAmount}");
                    }
                }
            }
        }
    }
}