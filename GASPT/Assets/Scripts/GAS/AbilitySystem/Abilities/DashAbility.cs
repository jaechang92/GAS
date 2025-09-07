// ================================
// File: Assets/Scripts/GAS/AbilitySystem/Abilities/DashAbility.cs
// 대시 이동 어빌리티 구현
// ================================
using System.Threading.Tasks;
using UnityEngine;
using GAS.Core;
using GAS.TagSystem;
using static GAS.Core.GASConstants;

namespace GAS.AbilitySystem.Abilities
{
    /// <summary>
    /// 대시 이동 어빌리티
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

            // 공중 대시 체크
            if (!canDashInAir && !IsGrounded(source))
            {
                if (debugMode)
                    Debug.Log("[Dash] Cannot dash in air");
                return false;
            }

            // 공중 대시 횟수 체크
            if (!IsGrounded(source) && currentAirDashCount >= maxAirDashes)
            {
                if (debugMode)
                    Debug.Log($"[Dash] Air dash limit reached: {currentAirDashCount}/{maxAirDashes}");
                return false;
            }

            // 스태미나 체크
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

            // 대시 방향 결정
            Vector3 dashDirection = GetDashDirection(source, activationInfo);

            // 대시 거리 계산 (장애물 체크)
            float finalDashDistance = CalculateDashDistance(source, dashDirection);

            // 공중 대시 카운트 증가
            if (!IsGrounded(source))
            {
                currentAirDashCount++;
            }

            // 무적 상태 적용
            if (invulnerableDuringDash)
            {
                ApplyInvulnerability(source, true);
            }

            // 이펙트 생성
            CreateDashEffects(source, dashDirection);

            // 사운드 재생
            if (dashSound != null)
            {
                AudioSource.PlayClipAtPoint(dashSound, source.transform.position);
            }

            // 대시 실행
            await PerformDash(source, dashDirection, finalDashDistance);

            // 무적 상태 해제
            if (invulnerableDuringDash)
            {
                ApplyInvulnerability(source, false);
            }

            // 어빌리티 종료
            EndAbility(source);
        }

        private async Task PerformDash(AbilitySystemComponent source, Vector3 direction, float distance)
        {
            var rb = source.GetComponent<Rigidbody2D>();
            var transform = source.transform;

            Vector3 startPosition = transform.position;
            Vector3 targetPosition = startPosition + direction * distance;

            float elapsedTime = 0f;

            // 물리 기반 이동을 일시 정지
            bool wasKinematic = false;
            if (rb != null)
            {
                wasKinematic = rb.isKinematic;
                rb.isKinematic = true;
                rb.velocity = Vector2.zero;
            }

            // 대시 애니메이션
            while (elapsedTime < dashDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / dashDuration;
                float curveValue = dashCurve.Evaluate(t);

                Vector3 newPosition = Vector3.Lerp(startPosition, targetPosition, curveValue);

                // 충돌 체크
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

            // 최종 위치 설정
            transform.position = targetPosition;

            // 물리 복원
            if (rb != null)
            {
                rb.isKinematic = wasKinematic;
            }
        }

        private Vector3 GetDashDirection(AbilitySystemComponent source, AbilityActivationInfo activationInfo)
        {
            // 입력 방향 우선
            if (activationInfo.activationDirection != Vector3.zero)
            {
                return activationInfo.activationDirection.normalized;
            }

            // 이동 입력 체크
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

            // 캐릭터가 바라보는 방향
            var spriteRenderer = source.GetComponentInChildren<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                return spriteRenderer.flipX ? Vector3.left : Vector3.right;
            }

            return source.transform.right;
        }

        private float CalculateDashDistance(AbilitySystemComponent source, Vector3 direction)
        {
            // 장애물까지의 거리 체크
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
            // 시작 이펙트
            if (dashStartEffectPrefab != null)
            {
                var startEffect = GameObject.Instantiate(
                    dashStartEffectPrefab,
                    source.transform.position,
                    Quaternion.LookRotation(Vector3.forward, direction)
                );
                GameObject.Destroy(startEffect, 2f);
            }

            // 트레일 이펙트
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
                // 무적 태그 추가
                var invulnerableTag = Resources.Load<GameplayTag>("Tags/State_Invulnerable");
                if (invulnerableTag != null)
                {
                    tagComponent.AddTag(invulnerableTag, this);
                }
            }
            else
            {
                // 무적 태그 제거
                tagComponent.RemoveAllTagsFromSource(this);
            }
        }

        private bool IsGrounded(AbilitySystemComponent source)
        {
            // Platform2DController가 있으면 그것을 사용
            var platformController = source.GetComponent<TestScene.Platform2DController>();
            if (platformController != null)
            {
                // Reflection을 통해 isGrounded 체크 (또는 public property로 변경)
                var field = platformController.GetType().GetField("isGrounded",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field != null)
                {
                    return (bool)field.GetValue(platformController);
                }
            }

            // 기본 ground 체크
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

            // 지면에 착지하면 공중 대시 카운트 리셋
            if (IsGrounded(source))
            {
                currentAirDashCount = 0;
            }

            // 종료 이펙트
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
            return 10f; // 스태미나 소비량
        }
    }
}