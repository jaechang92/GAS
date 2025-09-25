using UnityEngine;

namespace Character.Physics
{
    [CreateAssetMenu(fileName = "CharacterPhysicsConfig", menuName = "Character/Physics Config")]
    public class CharacterPhysicsConfig : ScriptableObject
    {
        [Header("Movement")]
        [SerializeField] public float moveSpeed = 8f;
        [SerializeField] public float maxMoveSpeed = 12f;
        [SerializeField] public float groundAcceleration = 50f;
        [SerializeField] public float groundDeceleration = 40f;
        [SerializeField] public float airAcceleration = 30f;
        [SerializeField] public float airDeceleration = 20f;
        [SerializeField] public float turnAroundMultiplier = 1.2f;

        [Header("== SKUL STYLE JUMP (No Gravity) ==")]
        [SerializeField] public float jumpVelocity = 18f;          // 점프 초기 상승 속도
        [SerializeField] public float jumpStrongDuration = 0.2f;   // 강한 점프 지속 시간
        [SerializeField] public float jumpWeakDuration = 0.1f;     // 약한 점프 지속 시간
        [SerializeField] public float jumpApexDuration = 0.05f;    // 점프 정점 시간
        [SerializeField] public float jumpCancelMultiplier = 0.5f; // 점프 취소 시 속도 감소량
        [SerializeField] public float jumpBufferTime = 0.15f;      // 점프 버퍼
        [SerializeField] public float coyoteTime = 0.1f;           // 코요테 타임

        [Header("== SKUL STYLE FALL (No Gravity) ==")]
        [SerializeField] public float normalFallSpeed = -18f;      // 일반 낙하 속도
        [SerializeField] public float fastFallSpeed = -30f;        // 빠른 낙하 속도 (아래 입력)
        [SerializeField] public float maxFallSpeed = -25f;         // 최대 낙하 속도
        [SerializeField] public float fallAccelTime = 0.3f;        // 최대 속도 도달 시간
        [SerializeField] public float downJumpFallSpeed = -22f;    // DownJump 낙하 속도

        [Header("== SKUL STYLE AIR CONTROL ==")]
        [SerializeField] public float airControlMultiplier = 0.95f;     // 공중 제어력 (95%)
        [SerializeField] public float fastFallControlMultiplier = 0.7f; // Fast Fall 시 제어력
        [SerializeField] public float downJumpControlMultiplier = 0.6f; // DownJump 시 제어력
        [SerializeField] public float wallSlideControlMultiplier = 0.5f;// 벽 슬라이드 시 제어력

        [Header("Friction")]
        [SerializeField] public float groundFriction = 10f;
        [SerializeField] public float airResistance = 5f;
        [SerializeField] public float slopeFriction = 8f;

        [Header("Crouch")]
        [SerializeField] public float crouchMoveSpeed = 3f;
        [SerializeField] public float crouchTransitionSpeed = 10f;
        [SerializeField] public Vector2 crouchColliderSize = new Vector2(1f, 0.5f);
        [SerializeField] public Vector2 crouchColliderOffset = new Vector2(0f, -0.25f);

        [Header("Advanced Movement")]
        [SerializeField] public float dashSpeed = 25f;      // Skul식 빠른 대시
        [SerializeField] public float dashDuration = 0.15f; // 짧은 대시 시간
        [SerializeField] public float dashCooldown = 0.8f;  // 대시 쿨다운
        [SerializeField] public float wallSlideSpeed = -2f;
        [SerializeField] public float wallJumpVelocity = 12f;
        [SerializeField] public Vector2 wallJumpDirection = new Vector2(0.7f, 1f);

        [Header("Platform")]
        [SerializeField] public float downJumpDisableTime = 0.3f;
        [SerializeField] public LayerMask oneWayPlatformMask;
        [SerializeField] public LayerMask groundMask;

        [Header("Debug")]
        [SerializeField] public bool enableDebugLogs = false;
        [SerializeField] public bool showVelocityGizmo = true;
        [SerializeField] public Color velocityGizmoColor = Color.blue;

        [Header("== SKUL PRESET ==")]
        [SerializeField] public bool useSkulPreset = true;

        // Skul 프리셋 적용 메서드
        public void ApplySkulPreset()
        {
            if (!useSkulPreset) return;

            // Movement
            moveSpeed = 10f;
            maxMoveSpeed = 14f;
            groundAcceleration = 60f;
            groundDeceleration = 50f;
            airAcceleration = 40f;
            airDeceleration = 25f;
            turnAroundMultiplier = 1.5f;

            // Jump
            jumpVelocity = 18f;
            jumpStrongDuration = 0.2f;
            jumpWeakDuration = 0.1f;
            jumpApexDuration = 0.05f;
            jumpCancelMultiplier = 0.5f;
            jumpBufferTime = 0.15f;
            coyoteTime = 0.1f;

            // Fall
            normalFallSpeed = -18f;
            fastFallSpeed = -30f;
            maxFallSpeed = -25f;
            fallAccelTime = 0.3f;
            downJumpFallSpeed = -22f;

            // Air Control
            airControlMultiplier = 0.95f;
            fastFallControlMultiplier = 0.7f;
            downJumpControlMultiplier = 0.6f;

            // Dash
            dashSpeed = 25f;
            dashDuration = 0.15f;
            dashCooldown = 0.8f;

            Debug.Log("[Physics Config] Skul preset applied!");
        }

        // Helper Methods
        public float GetAcceleration(bool isGrounded, bool isTurningAround = false)
        {
            float acceleration = isGrounded ? groundAcceleration : airAcceleration;
            return isTurningAround ? acceleration * turnAroundMultiplier : acceleration;
        }

        public float GetDeceleration(bool isGrounded)
        {
            return isGrounded ? groundDeceleration : airDeceleration;
        }

        public float GetAirControl(bool isFastFalling = false, bool isDownJumping = false)
        {
            if (isDownJumping) return downJumpControlMultiplier;
            if (isFastFalling) return fastFallControlMultiplier;
            return airControlMultiplier;
        }

        // Skul 스타일에서는 중력 안 씀
        public float GetCurrentGravity(bool isFastFalling)
        {
            return 0f; // No gravity in Skul style
        }

        // Inspector에서 버튼으로 프리셋 적용
        [ContextMenu("Apply Skul Preset")]
        private void ApplySkulPresetFromMenu()
        {
            ApplySkulPreset();
        }
    }
}