// ================================
// File: Assets/Scripts/GAS/AbilitySystem/Abilities/DashAbility.cs
// ��� �̵� �����Ƽ ����
// ================================
using System.Threading.Tasks;
using UnityEngine;
using GAS.Core;
using GAS.TagSystem;
using static GAS.Core.GASConstants;

namespace GAS.AbilitySystem.Abilities
{
    /// <summary>
    /// ��� �̵� �����Ƽ
    /// </summary>
    [CreateAssetMenu(fileName = "DashAbility", menuName = "GAS/Abilities/Dash")]
    public class DashAbility : GameplayAbility
    {
        [Header("Dash Settings")]
        [SerializeField] private float dashDistance = 5f;
        [SerializeField] private float dashDuration = 0.2f;
        [SerializeField] private AnimationCurve dashCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Dash Options")]
        [SerializeField] private bool canDashThroughEnemies = true;
        [SerializeField] private bool invulnerableDuringDash = true;
        [SerializeField] private bool canDashInAir = true;
        [SerializeField] private int maxAirDashes = 1;

        [Header("Effects")]
        [SerializeField] private GameObject dashTrailPrefab;
        [SerializeField] private GameObject dashStartEffectPrefab;
        [SerializeField] private GameObject dashEndEffectPrefab;
        [SerializeField] private AudioClip dashSound;

        [Header("Collision")]
        [SerializeField] private LayerMask obstacleLayer = -1;
        [SerializeField] private float collisionCheckRadius = 0.5f;

        // Runtime
        private int currentAirDashCount = 0;

        public override void InitializeAbility()
        {
            base.InitializeAbility();

            if (string.IsNullOrEmpty(abilityName))
                abilityName = "Dash";

            if (string.IsNullOrEmpty(description))
                description = "Quickly dash in a direction";

            if (cooldownTime <= 0)
                cooldownTime = 1f;
        }

        public override bool CanActivateAbility(AbilitySystemComponent source)
        {
            if (!base.CanActivateAbility(source))
                return false;

            // ���� ��� üũ
            if (!canDashInAir && !IsGrounded(source))
            {
                if (debugMode)
                    Debug.Log("[Dash] Cannot dash in air");
                return false;
            }

            // ���� ��� Ƚ�� üũ
            if (!IsGrounded(source) && currentAirDashCount >= maxAirDashes)
            {
                if (debugMode)
                    Debug.Log($"[Dash] Air dash limit reached: {currentAirDashCount}/{maxAirDashes}");
                return false;
            }

            // ���¹̳� üũ
            var attributes = source.GetComponent<AttributeSystem.AttributeSetComponent>();
            if (attributes != null)
            {
                float stamina = attributes.GetAttributeValue(AttributeType.Stamina);
                if (stamina < GetCostValue())
                {
                    if (debugMode)
                        Debug.Log($"[Dash] Not enough stamina: {stamina}/{GetCostValue()}");
                    return false;
                }
            }

            return true;
        }

        public override async void ActivateAbility(AbilitySystemComponent source, AbilityActivationInfo activationInfo)
        {
            base.ActivateAbility(source, activationInfo);

            if (debugMode)
                Debug.Log($"[Dash] Activated by {source.name}");

            // ��� ���� ����
            Vector3 dashDirection = GetDashDirection(source, activationInfo);

            // ��� �Ÿ� ��� (��ֹ� üũ)
            float finalDashDistance = CalculateDashDistance(source, dashDirection);

            // ���� ��� ī��Ʈ ����
            if (!IsGrounded(source))
            {
                currentAirDashCount++;
            }

            // ���� ���� ����
            if (invulnerableDuringDash)
            {
                ApplyInvulnerability(source, true);
            }

            // ����Ʈ ����
            CreateDashEffects(source, dashDirection);

            // ���� ���
            if (dashSound != null)
            {
                AudioSource.PlayClipAtPoint(dashSound, source.transform.position);
            }

            // ��� ����
            await PerformDash(source, dashDirection, finalDashDistance);

            // ���� ���� ����
            if (invulnerableDuringDash)
            {
                ApplyInvulnerability(source, false);
            }

            // �����Ƽ ����
            EndAbility(source);
        }

        private async Task PerformDash(AbilitySystemComponent source, Vector3 direction, float distance)
        {
            var rb = source.GetComponent<Rigidbody2D>();
            var transform = source.transform;

            Vector3 startPosition = transform.position;
            Vector3 targetPosition = startPosition + direction * distance;

            float elapsedTime = 0f;

            // ���� ��� �̵��� �Ͻ� ����
            bool wasKinematic = false;
            if (rb != null)
            {
                wasKinematic = rb.isKinematic;
                rb.isKinematic = true;
                rb.velocity = Vector2.zero;
            }

            // ��� �ִϸ��̼�
            while (elapsedTime < dashDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / dashDuration;
                float curveValue = dashCurve.Evaluate(t);

                Vector3 newPosition = Vector3.Lerp(startPosition, targetPosition, curveValue);

                // �浹 üũ
                if (!canDashThroughEnemies)
                {
                    RaycastHit2D hit = Physics2D.CircleCast(
                        transform.position,
                        collisionCheckRadius,
                        direction,
                        (newPosition - transform.position).magnitude,
                        obstacleLayer
                    );

                    if (hit.collider != null)
                    {
                        newPosition = hit.point - (Vector2)(direction * collisionCheckRadius);
                        targetPosition = newPosition;
                    }
                }

                transform.position = newPosition;

                await Task.Yield();
            }

            // ���� ��ġ ����
            transform.position = targetPosition;

            // ���� ����
            if (rb != null)
            {
                rb.isKinematic = wasKinematic;
            }
        }

        private Vector3 GetDashDirection(AbilitySystemComponent source, AbilityActivationInfo activationInfo)
        {
            // �Է� ���� �켱
            if (activationInfo.activationDirection != Vector3.zero)
            {
                return activationInfo.activationDirection.normalized;
            }

            // �̵� �Է� üũ
            var input = source.GetComponent<UnityEngine.InputSystem.PlayerInput>();
            if (input != null)
            {
                var moveAction = input.currentActionMap?.FindAction("Move");
                if (moveAction != null)
                {
                    Vector2 moveInput = moveAction.ReadValue<Vector2>();
                    if (moveInput != Vector2.zero)
                    {
                        return new Vector3(moveInput.x, moveInput.y, 0).normalized;
                    }
                }
            }

            // ĳ���Ͱ� �ٶ󺸴� ����
            var spriteRenderer = source.GetComponentInChildren<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                return spriteRenderer.flipX ? Vector3.left : Vector3.right;
            }

            return source.transform.right;
        }

        private float CalculateDashDistance(AbilitySystemComponent source, Vector3 direction)
        {
            // ��ֹ������� �Ÿ� üũ
            RaycastHit2D hit = Physics2D.CircleCast(
                source.transform.position,
                collisionCheckRadius,
                direction,
                dashDistance,
                obstacleLayer
            );

            if (hit.collider != null)
            {
                float distanceToObstacle = hit.distance - collisionCheckRadius;
                return Mathf.Max(0, distanceToObstacle);
            }

            return dashDistance;
        }

        private void CreateDashEffects(AbilitySystemComponent source, Vector3 direction)
        {
            // ���� ����Ʈ
            if (dashStartEffectPrefab != null)
            {
                var startEffect = GameObject.Instantiate(
                    dashStartEffectPrefab,
                    source.transform.position,
                    Quaternion.LookRotation(Vector3.forward, direction)
                );
                GameObject.Destroy(startEffect, 2f);
            }

            // Ʈ���� ����Ʈ
            if (dashTrailPrefab != null)
            {
                var trail = GameObject.Instantiate(
                    dashTrailPrefab,
                    source.transform.position,
                    source.transform.rotation
                );
                trail.transform.SetParent(source.transform);
                GameObject.Destroy(trail, dashDuration + 1f);
            }
        }

        private void ApplyInvulnerability(AbilitySystemComponent source, bool enable)
        {
            var tagComponent = source.GetComponent<TagComponent>();
            if (tagComponent == null) return;

            if (enable)
            {
                // ���� �±� �߰�
                var invulnerableTag = Resources.Load<GameplayTag>("Tags/State_Invulnerable");
                if (invulnerableTag != null)
                {
                    tagComponent.AddTag(invulnerableTag, this);
                }
            }
            else
            {
                // ���� �±� ����
                tagComponent.RemoveAllTagsFromSource(this);
            }
        }

        private bool IsGrounded(AbilitySystemComponent source)
        {
            // Platform2DController�� ������ �װ��� ���
            var platformController = source.GetComponent<TestScene.Platform2DController>();
            if (platformController != null)
            {
                // Reflection�� ���� isGrounded üũ (�Ǵ� public property�� ����)
                var field = platformController.GetType().GetField("isGrounded",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field != null)
                {
                    return (bool)field.GetValue(platformController);
                }
            }

            // �⺻ ground üũ
            RaycastHit2D hit = Physics2D.Raycast(
                source.transform.position,
                Vector2.down,
                1.1f,
                1 << 10 // Ground layer
            );

            return hit.collider != null;
        }

        public override void EndAbility(AbilitySystemComponent source)
        {
            base.EndAbility(source);

            // ���鿡 �����ϸ� ���� ��� ī��Ʈ ����
            if (IsGrounded(source))
            {
                currentAirDashCount = 0;
            }

            // ���� ����Ʈ
            if (dashEndEffectPrefab != null)
            {
                var endEffect = GameObject.Instantiate(
                    dashEndEffectPrefab,
                    source.transform.position,
                    Quaternion.identity
                );
                GameObject.Destroy(endEffect, 2f);
            }
        }

        public override float GetCostValue()
        {
            return 10f; // ���¹̳� �Һ�
        }
    }
}