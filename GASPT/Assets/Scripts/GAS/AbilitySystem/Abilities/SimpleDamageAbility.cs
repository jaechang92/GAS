// ================================
// File: Assets/Scripts/GAS/AbilitySystem/Abilities/SimpleDamageAbility.cs
// 테스트용 간단한 데미지 능력
// ================================
using System;
using UnityEngine;
using GAS.Core;
using GAS.TagSystem;
using GAS.AttributeSystem;
using GAS.EffectSystem;
using static GAS.Core.GASConstants;

namespace GAS.AbilitySystem.Abilities
{
    /// <summary>
    /// 테스트용 간단한 데미지 능력
    /// </summary>
    [CreateAssetMenu(fileName = "SimpleDamageAbility", menuName = "GAS/Abilities/Simple Damage")]
    public class SimpleDamageAbility : GameplayAbility
    {
        [Header("Damage Settings")]
        [SerializeField] private float baseDamage = 50f;
        [SerializeField] private float damagePerLevel = 10f;
        [SerializeField] private float range = 5f;

        [Header("Visual Settings")]
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private float projectileSpeed = 10f;
        [SerializeField] private GameObject impactEffectPrefab;

        [Header("Audio")]
        [SerializeField] private AudioClip castSound;
        [SerializeField] private AudioClip impactSound;

        [Header("Debug")]
        [SerializeField] private bool debugMode = true;

        /// <summary>
        /// Execute the damage ability
        /// </summary>
        protected override async Awaitable<AbilityExecutionResult> ExecuteAbility()
        {
            if (debugMode)
            {
                Debug.Log($"[SimpleDamageAbility] Executing {abilityName} at level {currentSpec.level}");
            }

            try
            {
                // Calculate damage
                float damage = CalculateDamage();

                // Log execution details
                LogExecutionDetails(damage);

                // Play cast sound
                if (castSound != null && activationInfo.source.GameObject != null)
                {
                    AudioSource.PlayClipAtPoint(castSound, activationInfo.source.Transform.position);
                }

                // Handle based on target type
                switch (targetType)
                {
                    case AbilityTargetType.Self:
                        await ExecuteSelfDamage(damage);
                        break;

                    case AbilityTargetType.Target:
                        await ExecuteTargetDamage(damage);
                        break;

                    case AbilityTargetType.Point:
                        await ExecutePointDamage(damage);
                        break;

                    case AbilityTargetType.Direction:
                        await ExecuteDirectionalDamage(damage);
                        break;

                    case AbilityTargetType.Area:
                        await ExecuteAreaDamage(damage);
                        break;

                    default:
                        return AbilityExecutionResult.Failure(AbilityFailureReason.InvalidTarget);
                }

                // Ability executed successfully
                return AbilityExecutionResult.Success();
            }
            catch (Exception e)
            {
                Debug.LogError($"[SimpleDamageAbility] Execution failed: {e.Message}");
                return AbilityExecutionResult.Failure(AbilityFailureReason.Custom, e.Message);
            }
        }

        /// <summary>
        /// Calculate damage based on level and attributes
        /// </summary>
        private float CalculateDamage()
        {
            float damage = baseDamage + (damagePerLevel * (currentSpec.level - 1));

            // Apply magnitude from base class
            float magnitude = CalculateEffectMagnitude();
            damage *= magnitude;

            if (debugMode)
            {
                Debug.Log($"[SimpleDamageAbility] Base: {baseDamage}, Level Bonus: {damagePerLevel * (currentSpec.level - 1)}, Magnitude: {magnitude}, Total: {damage}");
            }

            return damage;
        }

        /// <summary>
        /// Execute damage on self (for testing)
        /// </summary>
        private async Awaitable ExecuteSelfDamage(float damage)
        {
            if (debugMode)
                Debug.Log($"[SimpleDamageAbility] Self damage: {damage}");

            ApplyDamageToTarget(activationInfo.source.GameObject, damage);
            await Awaitable.WaitForSecondsAsync(0.1f);
        }

        /// <summary>
        /// Execute damage on target
        /// </summary>
        private async Awaitable ExecuteTargetDamage(float damage)
        {
            if (activationInfo.target == null)
            {
                if (debugMode)
                    Debug.LogWarning("[SimpleDamageAbility] No target specified");
                return;
            }

            if (debugMode)
                Debug.Log($"[SimpleDamageAbility] Target damage to {activationInfo.target.name}: {damage}");

            // Check range
            float distance = Vector3.Distance(
                activationInfo.source.Transform.position,
                activationInfo.target.transform.position
            );

            if (distance > range)
            {
                if (debugMode)
                    Debug.LogWarning($"[SimpleDamageAbility] Target out of range: {distance} > {range}");
                return;
            }

            // Create projectile if prefab exists
            if (projectilePrefab != null)
            {
                await LaunchProjectile(activationInfo.target, damage);
            }
            else
            {
                // Instant damage
                ApplyDamageToTarget(activationInfo.target, damage);
                await Awaitable.WaitForSecondsAsync(0.1f);
            }
        }

        /// <summary>
        /// Execute damage at point
        /// </summary>
        private async Awaitable ExecutePointDamage(float damage)
        {
            if (debugMode)
                Debug.Log($"[SimpleDamageAbility] Point damage at {activationInfo.activationPosition}: {damage}");

            // Find targets at point
            Collider[] colliders = Physics.OverlapSphere(activationInfo.activationPosition, 2f);

            foreach (var collider in colliders)
            {
                if (IsValidTarget(collider.gameObject))
                {
                    ApplyDamageToTarget(collider.gameObject, damage);
                }
            }

            // Spawn impact effect
            if (impactEffectPrefab != null)
            {
                var effect = GameObject.Instantiate(impactEffectPrefab, activationInfo.activationPosition, Quaternion.identity);
                GameObject.Destroy(effect, 3f);
            }

            await Awaitable.WaitForSecondsAsync(0.1f);
        }

        /// <summary>
        /// Execute damage in direction
        /// </summary>
        private async Awaitable ExecuteDirectionalDamage(float damage)
        {
            if (debugMode)
                Debug.Log($"[SimpleDamageAbility] Directional damage: {damage}");

            // Cast ray in direction
            RaycastHit hit;
            Vector3 origin = activationInfo.source.Transform.position;

            if (Physics.Raycast(origin, activationInfo.activationDirection, out hit, range))
            {
                if (IsValidTarget(hit.collider.gameObject))
                {
                    ApplyDamageToTarget(hit.collider.gameObject, damage);

                    // Spawn impact effect at hit point
                    if (impactEffectPrefab != null)
                    {
                        var effect = GameObject.Instantiate(impactEffectPrefab, hit.point, Quaternion.identity);
                        GameObject.Destroy(effect, 3f);
                    }
                }
            }

            await Awaitable.WaitForSecondsAsync(0.1f);
        }

        /// <summary>
        /// Execute area damage
        /// </summary>
        private async Awaitable ExecuteAreaDamage(float damage)
        {
            if (debugMode)
                Debug.Log($"[SimpleDamageAbility] Area damage: {damage}");

            Vector3 center = activationInfo.activationPosition != Vector3.zero
                ? activationInfo.activationPosition
                : activationInfo.source.Transform.position;

            // Find all targets in area
            Collider[] colliders = Physics.OverlapSphere(center, range);
            int hitCount = 0;

            foreach (var collider in colliders)
            {
                if (IsValidTarget(collider.gameObject))
                {
                    // Apply damage with falloff based on distance
                    float distance = Vector3.Distance(center, collider.transform.position);
                    float falloff = 1f - (distance / range);
                    float areaDamage = damage * falloff;

                    ApplyDamageToTarget(collider.gameObject, areaDamage);
                    hitCount++;
                }
            }

            if (debugMode)
                Debug.Log($"[SimpleDamageAbility] Hit {hitCount} targets in area");

            await Awaitable.WaitForSecondsAsync(0.1f);
        }

        /// <summary>
        /// Launch projectile to target
        /// </summary>
        private async Awaitable LaunchProjectile(GameObject target, float damage)
        {
            Vector3 startPos = activationInfo.source.Transform.position + Vector3.up;
            GameObject projectile = GameObject.Instantiate(projectilePrefab, startPos, Quaternion.identity);

            // Simple projectile movement
            float travelTime = 0f;
            float maxTravelTime = range / projectileSpeed;

            while (travelTime < maxTravelTime && projectile != null && target != null)
            {
                Vector3 direction = (target.transform.position - projectile.transform.position).normalized;
                projectile.transform.position += direction * projectileSpeed * Time.deltaTime;

                // Check if hit target
                if (Vector3.Distance(projectile.transform.position, target.transform.position) < 0.5f)
                {
                    ApplyDamageToTarget(target, damage);

                    // Spawn impact effect
                    if (impactEffectPrefab != null)
                    {
                        var effect = GameObject.Instantiate(impactEffectPrefab, projectile.transform.position, Quaternion.identity);
                        GameObject.Destroy(effect, 3f);
                    }

                    // Play impact sound
                    if (impactSound != null)
                    {
                        AudioSource.PlayClipAtPoint(impactSound, projectile.transform.position);
                    }

                    GameObject.Destroy(projectile);
                    break;
                }

                travelTime += Time.deltaTime;
                await Awaitable.NextFrameAsync();
            }

            // Destroy projectile if it didn't hit
            if (projectile != null)
            {
                GameObject.Destroy(projectile);
            }
        }

        /// <summary>
        /// Apply damage to target
        /// </summary>
        private void ApplyDamageToTarget(GameObject target, float damage)
        {
            // Get attribute component
            var attributes = target.GetComponent<AttributeSetComponent>();
            if (attributes != null)
            {
                // Apply damage to health
                attributes.ModifyAttribute(AttributeType.Health, -damage, ModifierOperation.Add);

                if (debugMode)
                {
                    float currentHealth = attributes.GetAttributeValue(AttributeType.Health);
                    Debug.Log($"[SimpleDamageAbility] Applied {damage} damage to {target.name}. Health: {currentHealth}");
                }
            }
            else if (debugMode)
            {
                Debug.LogWarning($"[SimpleDamageAbility] Target {target.name} has no AttributeSetComponent");
            }

            // Fire damage event
            GASEvents.Trigger(GASEventType.DamageDealt, new DamageEventData
            {
                source = activationInfo.source.GameObject,
                target = target,
                damage = damage,
                damageType = DamageType.Physical
            });
        }

        /// <summary>
        /// Check if target is valid
        /// </summary>
        private bool IsValidTarget(GameObject target)
        {
            // Don't damage self unless specifically targeting self
            if (targetType != AbilityTargetType.Self && target == activationInfo.source.GameObject)
                return false;

            // Check if has attribute component
            if (target.GetComponent<AttributeSetComponent>() == null)
                return false;

            // Check tags if needed
            var tagComponent = target.GetComponent<TagComponent>();
            if (tagComponent != null)
            {
                // Example: Don't damage allies
                if (tagComponent.HasTag("Ally"))
                    return false;

                // Example: Only damage enemies
                if (!tagComponent.HasTag("Enemy") && !tagComponent.HasTag("Neutral"))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Log execution details for debugging
        /// </summary>
        private void LogExecutionDetails(float damage)
        {
            if (!debugMode) return;

            Debug.Log($"=== SimpleDamageAbility Execution ===");
            Debug.Log($"Ability: {abilityName} (Level {currentSpec.level})");
            Debug.Log($"Source: {activationInfo.source.GameObject.name}");
            Debug.Log($"Target Type: {targetType}");
            Debug.Log($"Target: {(activationInfo.target != null ? activationInfo.target.name : "None")}");
            Debug.Log($"Position: {activationInfo.activationPosition}");
            Debug.Log($"Direction: {activationInfo.activationDirection}");
            Debug.Log($"Damage: {damage}");
            Debug.Log($"Costs: {string.Join(", ", CalculateCosts(currentSpec.level))}");
            Debug.Log($"Cooldown: {GetCooldownDuration(currentSpec.level)}s");
            Debug.Log($"===================================");
        }
    }
}