// 파일 위치: Assets/GASTest/TestCode/DummyEnemy.cs
using AbilitySystem;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AbilitySystem.Platformer.Test
{
    /// <summary>
    /// 테스트용 더미 적 - IAbilityTarget 구현
    /// </summary>
    public class DummyEnemy : AbilityTargetBase
    {
        [Header("Enemy Settings")]
        [SerializeField] private bool showDamageNumbers = true;
        [SerializeField] private bool respawnOnDeath = true;
        [SerializeField] private float respawnDelay = 3f;

        [Header("UI")]
        [SerializeField] private Slider healthBar;
        [SerializeField] private GameObject damageTextPrefab;
        [SerializeField] private Transform damageTextSpawnPoint;

        [Header("Effects")]
        [SerializeField] private Color damageColor = Color.red;
        [SerializeField] private float damageFeedbackDuration = 0.1f;

        // Components
        private SpriteRenderer spriteRenderer;
        private Rigidbody2D rb;
        private Collider2D col;
        private Vector3 initialPosition;
        private Color originalColor;

        // Buffs and Effects
        private Dictionary<BuffType, float> activeBuffs = new Dictionary<BuffType, float>();
        private Dictionary<StatusEffectType, float> activeStatusEffects = new Dictionary<StatusEffectType, float>();

        protected override void Start()
        {
            base.Start();
            InitializeComponents();
            UpdateHealthBar();
        }

        private void InitializeComponents()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            rb = GetComponent<Rigidbody2D>();
            col = GetComponent<Collider2D>();

            if (spriteRenderer != null)
            {
                originalColor = spriteRenderer.color;
            }

            initialPosition = transform.position;

            // Find health bar in children
            if (healthBar == null)
            {
                healthBar = GetComponentInChildren<Slider>();
            }
        }

        #region IAbilityTarget Implementation

        public override void TakeDamage(float damage)
        {
            TakeDamage(damage, null, DamageType.Physical);
        }

        public override void TakeDamage(float damage, GameObject attacker)
        {
            TakeDamage(damage, attacker, DamageType.Physical);
        }

        public override void TakeDamage(float damage, GameObject attacker, DamageType damageType)
        {
            if (!IsAlive) return;

            // Apply damage reduction buffs
            if (activeBuffs.ContainsKey(BuffType.DamageReduction))
            {
                damage *= (1f - activeBuffs[BuffType.DamageReduction]);
            }

            currentHealth = Mathf.Max(0, currentHealth - damage);

            // Visual feedback
            ShowDamageFeedback(damage);
            UpdateHealthBar();

            Debug.Log($"{gameObject.name} took {damage:F1} {damageType} damage from {attacker?.name ?? "unknown"}. Health: {currentHealth}/{maxHealth}");

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        public override void Heal(float amount)
        {
            Heal(amount, null);
        }

        public override void Heal(float amount, GameObject healer)
        {
            if (!IsAlive) return;

            float oldHealth = currentHealth;
            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
            float actualHeal = currentHealth - oldHealth;

            if (actualHeal > 0)
            {
                ShowHealFeedback(actualHeal);
                UpdateHealthBar();
                Debug.Log($"{gameObject.name} healed for {actualHeal:F1} by {healer?.name ?? "unknown"}. Health: {currentHealth}/{maxHealth}");
            }
        }

        public override void ApplyBuff(BuffType buffType, float duration, float value)
        {
            activeBuffs[buffType] = value;

            // Start coroutine to remove buff after duration
            StartCoroutine(RemoveBuffAfterDuration(buffType, duration));

            Debug.Log($"{gameObject.name} received {buffType} buff: {value} for {duration}s");
        }

        public override void RemoveBuff(BuffType buffType)
        {
            if (activeBuffs.ContainsKey(buffType))
            {
                activeBuffs.Remove(buffType);
                Debug.Log($"{gameObject.name} removed {buffType} buff");
            }
        }

        public override void ApplyStatusEffect(StatusEffectType effectType, float duration)
        {
            activeStatusEffects[effectType] = Time.time + duration;

            // Apply effect immediately
            switch (effectType)
            {
                case StatusEffectType.Stun:
                    // Disable movement
                    if (rb != null) rb.constraints = RigidbodyConstraints2D.FreezeAll;
                    break;
                case StatusEffectType.Slow:
                    // Reduce speed (would affect movement component)
                    break;
                case StatusEffectType.Freeze:
                    if (rb != null) rb.constraints = RigidbodyConstraints2D.FreezeAll;
                    if (spriteRenderer != null) spriteRenderer.color = Color.cyan;
                    break;
            }

            StartCoroutine(RemoveStatusEffectAfterDuration(effectType, duration));
            Debug.Log($"{gameObject.name} affected by {effectType} for {duration}s");
        }

        public override void RemoveStatusEffect(StatusEffectType effectType)
        {
            if (!activeStatusEffects.ContainsKey(effectType)) return;

            activeStatusEffects.Remove(effectType);

            // Remove effect
            switch (effectType)
            {
                case StatusEffectType.Stun:
                case StatusEffectType.Freeze:
                    if (rb != null) rb.constraints = RigidbodyConstraints2D.FreezeRotation;
                    if (spriteRenderer != null) spriteRenderer.color = originalColor;
                    break;
            }

            Debug.Log($"{gameObject.name} removed {effectType} effect");
        }

        public override void ApplyKnockback(Vector2 force)
        {
            if (rb != null && !rb.isKinematic)
            {
                rb.AddForce(force, ForceMode2D.Impulse);
            }
        }

        public override void ApplyKnockback(Vector2 direction, float power)
        {
            ApplyKnockback(direction.normalized * power);
        }

        #endregion

        #region Visual Feedback

        private void ShowDamageFeedback(float damage)
        {
            // Flash red
            if (spriteRenderer != null)
            {
                StartCoroutine(FlashColor(damageColor, damageFeedbackDuration));
            }

            // Show damage number
            if (showDamageNumbers && damageTextPrefab != null)
            {
                Vector3 spawnPos = damageTextSpawnPoint != null ? damageTextSpawnPoint.position : transform.position + Vector3.up;
                GameObject damageText = Instantiate(damageTextPrefab, spawnPos, Quaternion.identity);

                // Setup damage text (assumes it has a Text or TextMeshPro component)
                TextMeshPro textComponent = damageText.GetComponentInChildren<TMPro.TextMeshPro>();

                if (textComponent != null)
                {
                    textComponent.text = damage.ToString("F0");
                }

                Destroy(damageText, 1f);
            }
        }

        private void ShowHealFeedback(float amount)
        {
            // Flash green
            if (spriteRenderer != null)
            {
                StartCoroutine(FlashColor(Color.green, damageFeedbackDuration));
            }

            // Show heal number
            if (showDamageNumbers && damageTextPrefab != null)
            {
                Vector3 spawnPos = damageTextSpawnPoint != null ? damageTextSpawnPoint.position : transform.position + Vector3.up;
                GameObject healText = Instantiate(damageTextPrefab, spawnPos, Quaternion.identity);

                var textComponent = healText.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                if (textComponent != null)
                {
                    textComponent.text = "+" + amount.ToString("F0");
                    textComponent.color = Color.green;
                }

                Destroy(healText, 1f);
            }
        }

        private IEnumerator FlashColor(Color flashColor, float duration)
        {
            if (spriteRenderer == null) yield break;

            spriteRenderer.color = flashColor;
            yield return new WaitForSeconds(duration);
            spriteRenderer.color = originalColor;
        }

        private void UpdateHealthBar()
        {
            if (healthBar != null)
            {
                healthBar.value = currentHealth / maxHealth;
            }
        }

        #endregion

        #region Death and Respawn

        private void Die()
        {
            Debug.Log($"{gameObject.name} died!");

            // Disable components
            if (col != null) col.enabled = false;
            if (rb != null) rb.simulated = false;

            // Death animation
            if (spriteRenderer != null)
            {
                StartCoroutine(DeathAnimation());
            }

            // Respawn if enabled
            if (respawnOnDeath)
            {
                StartCoroutine(RespawnAfterDelay());
            }
        }

        private IEnumerator DeathAnimation()
        {
            float duration = 0.5f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);

                if (spriteRenderer != null)
                {
                    Color c = spriteRenderer.color;
                    c.a = alpha;
                    spriteRenderer.color = c;
                }

                yield return null;
            }
        }

        private IEnumerator RespawnAfterDelay()
        {
            yield return new WaitForSeconds(respawnDelay);

            Respawn();
        }

        private void Respawn()
        {
            // Reset position
            transform.position = initialPosition;

            // Reset health
            currentHealth = maxHealth;
            UpdateHealthBar();

            // Re-enable components
            if (col != null) col.enabled = true;
            if (rb != null) rb.simulated = true;

            // Reset visuals
            if (spriteRenderer != null)
            {
                Color c = originalColor;
                c.a = 1f;
                spriteRenderer.color = c;
            }

            // Clear effects
            activeBuffs.Clear();
            activeStatusEffects.Clear();

            Debug.Log($"{gameObject.name} respawned!");
        }

        #endregion

        #region Coroutines

        private IEnumerator RemoveBuffAfterDuration(BuffType buffType, float duration)
        {
            yield return new WaitForSeconds(duration);
            RemoveBuff(buffType);
        }

        private IEnumerator RemoveStatusEffectAfterDuration(StatusEffectType effectType, float duration)
        {
            yield return new WaitForSeconds(duration);
            RemoveStatusEffect(effectType);
        }

        #endregion

        #region Debug

        [ContextMenu("Take 10 Damage")]
        private void Debug_Take10Damage()
        {
            TakeDamage(10f);
        }

        [ContextMenu("Heal 20 HP")]
        private void Debug_Heal20HP()
        {
            Heal(20f);
        }

        [ContextMenu("Apply Stun")]
        private void Debug_ApplyStun()
        {
            ApplyStatusEffect(StatusEffectType.Stun, 2f);
        }

        #endregion
    }
}