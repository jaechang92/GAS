// 파일 위치: Assets/Scripts/Ability/Core/GroundChecker.cs
using UnityEngine;
using System;

namespace AbilitySystem.Core
{
    /// <summary>
    /// 정확한 바닥 감지와 플랫포머 특수 기능을 제공하는 컴포넌트
    /// </summary>
    public class GroundChecker : MonoBehaviour
    {
        [Header("Ground Check Settings")]
        [SerializeField] private LayerMask groundLayer = 1 << 3; // Default to layer 3
        [SerializeField] private float checkDistance = 0.1f;
        [SerializeField] private Vector2 boxSize = new Vector2(0.9f, 0.05f);
        [SerializeField] private Vector2 boxOffset = Vector2.zero;

        [Header("Slope Detection")]
        [SerializeField] private bool detectSlopes = true;
        [SerializeField] private float maxSlopeAngle = 45f;

        [Header("Coyote Time")]
        [SerializeField] private bool useCoyoteTime = true;
        [SerializeField] private float coyoteTimeDuration = 0.1f;

        [Header("Jump Buffer")]
        [SerializeField] private bool useJumpBuffer = true;
        [SerializeField] private float jumpBufferDuration = 0.15f;

        [Header("Debug")]
        [SerializeField] private bool showDebugGizmos = true;
        [SerializeField] private Color gizmoColor = Color.green;

        // Properties
        public bool IsGrounded { get; private set; }
        public bool WasGroundedLastFrame { get; private set; }
        public bool CanUseCoyoteTime { get; private set; }
        public bool HasJumpBuffered { get; private set; }
        public float CurrentSlopeAngle { get; private set; }
        public Vector2 GroundNormal { get; private set; }
        public GameObject CurrentPlatform { get; private set; }
        public LayerMask GroundLayerMask => groundLayer; // Getter for groundLayer

        // Events
        public event Action<bool> OnGroundedChanged;
        public event Action OnLanded;
        public event Action OnLeftGround;
        public event Action<GameObject> OnPlatformChanged;

        // Private fields
        private float coyoteTimeCounter;
        private float jumpBufferCounter;
        private BoxCollider2D boxCollider;
        private ContactFilter2D contactFilter;
        private RaycastHit2D[] hitBuffer = new RaycastHit2D[8];
        private GameObject lastPlatform;

        private void Awake()
        {
            boxCollider = GetComponent<BoxCollider2D>();
            SetupContactFilter();
        }

        private void SetupContactFilter()
        {
            contactFilter = new ContactFilter2D();
            contactFilter.SetLayerMask(groundLayer);
            contactFilter.useLayerMask = true;
            contactFilter.useTriggers = false;
        }

        private void FixedUpdate()
        {
            WasGroundedLastFrame = IsGrounded;
            PerformGroundCheck();
            UpdateCoyoteTime();
            HandleGroundStateChanges();
            DetectPlatformChange();
        }

        private void Update()
        {
            UpdateJumpBuffer();
        }

        private void PerformGroundCheck()
        {
            Vector2 boxCenter = (Vector2)transform.position + boxOffset;
            boxCenter.y -= (boxCollider.size.y * 0.5f);

            // BoxCast로 바닥 감지
            int hitCount = Physics2D.BoxCast(
                boxCenter,
                boxSize,
                0f,
                Vector2.down,
                contactFilter,
                hitBuffer,
                checkDistance
            );

            IsGrounded = false;
            CurrentSlopeAngle = 0f;
            GroundNormal = Vector2.up;
            CurrentPlatform = null;

            for (int i = 0; i < hitCount; i++)
            {
                RaycastHit2D hit = hitBuffer[i];

                if (hit.collider != null && hit.collider.gameObject != gameObject)
                {
                    IsGrounded = true;
                    CurrentPlatform = hit.collider.gameObject;

                    if (detectSlopes)
                    {
                        GroundNormal = hit.normal;
                        CurrentSlopeAngle = Vector2.Angle(Vector2.up, hit.normal);

                        // 최대 경사각 체크
                        if (CurrentSlopeAngle > maxSlopeAngle)
                        {
                            IsGrounded = false;
                            CurrentPlatform = null;
                        }
                    }

                    break;
                }
            }
        }

        private void UpdateCoyoteTime()
        {
            if (!useCoyoteTime) return;

            if (IsGrounded)
            {
                coyoteTimeCounter = coyoteTimeDuration;
                CanUseCoyoteTime = true;
            }
            else if (coyoteTimeCounter > 0)
            {
                coyoteTimeCounter -= Time.fixedDeltaTime;
                CanUseCoyoteTime = true;
            }
            else
            {
                CanUseCoyoteTime = false;
            }
        }

        private void UpdateJumpBuffer()
        {
            if (!useJumpBuffer) return;

            if (jumpBufferCounter > 0)
            {
                jumpBufferCounter -= Time.deltaTime;
                HasJumpBuffered = jumpBufferCounter > 0;
            }
            else
            {
                HasJumpBuffered = false;
            }
        }

        public void BufferJumpInput()
        {
            if (useJumpBuffer)
            {
                jumpBufferCounter = jumpBufferDuration;
                HasJumpBuffered = true;
            }
        }

        public void ConsumeJumpBuffer()
        {
            jumpBufferCounter = 0;
            HasJumpBuffered = false;
        }

        public void ConsumeCoyoteTime()
        {
            coyoteTimeCounter = 0;
            CanUseCoyoteTime = false;
        }

        private void HandleGroundStateChanges()
        {
            if (IsGrounded != WasGroundedLastFrame)
            {
                OnGroundedChanged?.Invoke(IsGrounded);

                if (IsGrounded && !WasGroundedLastFrame)
                {
                    OnLanded?.Invoke();
                }
                else if (!IsGrounded && WasGroundedLastFrame)
                {
                    OnLeftGround?.Invoke();
                }
            }
        }

        private void DetectPlatformChange()
        {
            if (CurrentPlatform != lastPlatform)
            {
                OnPlatformChanged?.Invoke(CurrentPlatform);
                lastPlatform = CurrentPlatform;
            }
        }

        public bool CanJump()
        {
            return IsGrounded || CanUseCoyoteTime;
        }

        public Vector2 GetMovementDirectionOnSlope(Vector2 inputDirection)
        {
            if (!detectSlopes || CurrentSlopeAngle == 0f)
            {
                return inputDirection;
            }

            // 경사면에서의 이동 방향 계산
            Vector3 slopeDirection = Vector3.Cross(Vector3.Cross(Vector3.up, GroundNormal), GroundNormal);
            return new Vector2(slopeDirection.x * inputDirection.x, slopeDirection.y).normalized;
        }

        private void OnDrawGizmos()
        {
            if (!showDebugGizmos) return;

            Vector2 boxCenter = Application.isPlaying ?
                (Vector2)transform.position + boxOffset :
                (Vector2)transform.position;

            if (boxCollider != null)
            {
                boxCenter.y -= (boxCollider.size.y * 0.5f);
            }

            Gizmos.color = IsGrounded ? gizmoColor : Color.red;

            // Ground check box 시각화
            Vector3 boxPos = boxCenter + Vector2.down * checkDistance * 0.5f;
            Gizmos.DrawWireCube(boxPos, new Vector3(boxSize.x, boxSize.y + checkDistance, 0));

            // 경사면 normal 시각화
            if (IsGrounded && detectSlopes)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawRay(transform.position, GroundNormal);
            }
        }
    }
}