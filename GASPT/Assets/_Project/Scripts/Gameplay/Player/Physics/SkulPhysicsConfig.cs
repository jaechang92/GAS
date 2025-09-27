using UnityEngine;

namespace Player.Physics
{
    /// <summary>
    /// Skul: The Hero Slayer 스타일 물리 설정
    /// 즉시 반응성과 정밀한 조작감을 위한 파라미터
    /// </summary>
    [CreateAssetMenu(fileName = "SkulPhysicsConfig", menuName = "Skul/Physics Config", order = 1)]
    public class SkulPhysicsConfig : ScriptableObject
    {
        [Header("=== Skul 스타일 기본 이동 ===")]
        [SerializeField] public float moveSpeed = 12f;
        [SerializeField] public float airMoveSpeed = 10f;
        // 가속도와 마찰력 제거 - 즉시 반응 시스템 사용

        [Header("=== 정밀한 점프 시스템 ===")]
        [SerializeField] public float jumpVelocity = 16f;
        [SerializeField] public float minJumpVelocity = 8f;
        [SerializeField] public float maxJumpVelocity = 20f;
        [SerializeField] public float jumpCutMultiplier = 0.4f;
        [SerializeField] public float coyoteTime = 0.12f;
        [SerializeField] public float jumpBufferTime = 0.15f;
        [SerializeField] public float minJumpHoldTime = 0.05f;
        [SerializeField] public float maxJumpHoldTime = 0.3f;

        [Header("=== 반응성 좋은 중력 ===")]
        [SerializeField] public float gravity = 32f;
        [SerializeField] public float fallGravityMultiplier = 1.8f;
        [SerializeField] public float quickFallGravity = 45f;
        [SerializeField] public float maxFallSpeed = 22f;
        [SerializeField] public float lowJumpGravityMultiplier = 2.5f;

        [Header("=== 빠른 대시 시스템 ===")]
        [SerializeField] public float dashSpeed = 28f;
        [SerializeField] public float dashDuration = 0.15f;
        [SerializeField] public float dashCooldown = 0.8f;
        [SerializeField] public bool dashIgnoreGravity = true;
        [SerializeField] public bool canDashInAir = true;
        [SerializeField] public int maxAirDashes = 1;

        [Header("=== 벽 상호작용 ===")]
        [SerializeField] public float wallSlideSpeed = -3f;
        [SerializeField] public Vector2 wallJumpVelocity = new Vector2(12f, 16f);
        [SerializeField] public float wallStickTime = 0.08f;
        [SerializeField] public float wallJumpCooldown = 0.2f;
        [SerializeField] public float wallDetectionDistance = 0.6f;

        [Header("=== 충돌 감지 ===")]
        [SerializeField] public LayerMask groundLayerMask = 1;
        [SerializeField] public LayerMask wallLayerMask = 1;
        [SerializeField] public float groundCheckDistance = 0.1f;
        [SerializeField] public float groundCheckWidth = 0.8f;
        [SerializeField] public float wallCheckDistance = 0.1f;

        [Header("=== 디버그 ===")]
        [SerializeField] public bool enableDebugLogs = false;
        [SerializeField] public bool showPhysicsGizmos = true;

        /// <summary>
        /// Skul 게임 스타일의 완벽한 프리셋 적용
        /// </summary>
        [ContextMenu("Apply Perfect Skul Preset")]
        public void ApplyPerfectSkulPreset()
        {
            // 즉시 반응하는 이동 (가속도 없음)
            moveSpeed = 12f;
            airMoveSpeed = 10f;

            // 정밀한 점프 (Skul의 핵심)
            jumpVelocity = 16f;
            minJumpVelocity = 8f;
            maxJumpVelocity = 20f;
            jumpCutMultiplier = 0.4f;
            coyoteTime = 0.12f;
            jumpBufferTime = 0.15f;
            minJumpHoldTime = 0.05f;
            maxJumpHoldTime = 0.3f;

            // 빠른 반응의 중력 (Skul 스타일)
            gravity = 45f;
            fallGravityMultiplier = 2.2f;
            quickFallGravity = 65f;
            maxFallSpeed = 30f;
            lowJumpGravityMultiplier = 2.5f;

            // 빠르고 정확한 대시
            dashSpeed = 28f;
            dashDuration = 0.15f;
            dashCooldown = 0.8f;
            dashIgnoreGravity = true;
            canDashInAir = true;
            maxAirDashes = 1;

            // 부드러운 벽 상호작용
            wallSlideSpeed = -3f;
            wallJumpVelocity = new Vector2(12f, 16f);
            wallStickTime = 0.08f;
            wallJumpCooldown = 0.2f;
            wallDetectionDistance = 0.6f;

            // 정밀한 충돌 감지
            groundCheckDistance = 0.1f;
            groundCheckWidth = 0.8f;
            wallCheckDistance = 0.1f;

            Debug.Log("[SkulPhysicsConfig] Skul 완벽 프리셋이 적용되었습니다!");
        }

        /// <summary>
        /// 빠른 개발용 프리셋 (디버깅 편의성)
        /// </summary>
        [ContextMenu("Apply Debug Preset")]
        public void ApplyDebugPreset()
        {
            ApplyPerfectSkulPreset();

            // 디버그 설정
            enableDebugLogs = true;
            showPhysicsGizmos = true;

            // 테스트하기 쉬운 값들
            dashCooldown = 0.3f; // 빠른 대시 테스트
            coyoteTime = 0.2f;   // 관대한 점프 타이밍
            jumpBufferTime = 0.2f;

            Debug.Log("[SkulPhysicsConfig] 디버그 프리셋이 적용되었습니다!");
        }

        /// <summary>
        /// 설정값 검증 및 자동 보정
        /// </summary>
        public void ValidateSettings()
        {
            // 이동 관련 값들 검증 (즉시 반응 시스템)
            moveSpeed = Mathf.Max(0f, moveSpeed);
            airMoveSpeed = Mathf.Max(0f, airMoveSpeed);

            // 점프 관련 값들 검증
            jumpVelocity = Mathf.Max(0f, jumpVelocity);
            minJumpVelocity = Mathf.Max(0f, minJumpVelocity);
            maxJumpVelocity = Mathf.Max(minJumpVelocity, maxJumpVelocity);
            jumpCutMultiplier = Mathf.Clamp01(jumpCutMultiplier);
            coyoteTime = Mathf.Max(0f, coyoteTime);
            jumpBufferTime = Mathf.Max(0f, jumpBufferTime);
            minJumpHoldTime = Mathf.Max(0f, minJumpHoldTime);
            maxJumpHoldTime = Mathf.Max(minJumpHoldTime, maxJumpHoldTime);

            // 중력 관련 값들 검증
            gravity = Mathf.Max(0f, gravity);
            fallGravityMultiplier = Mathf.Max(1f, fallGravityMultiplier);
            quickFallGravity = Mathf.Max(gravity, quickFallGravity);
            maxFallSpeed = Mathf.Max(0f, maxFallSpeed);
            lowJumpGravityMultiplier = Mathf.Max(1f, lowJumpGravityMultiplier);

            // 대시 관련 값들 검증
            dashSpeed = Mathf.Max(0f, dashSpeed);
            dashDuration = Mathf.Max(0.01f, dashDuration);
            dashCooldown = Mathf.Max(0f, dashCooldown);
            maxAirDashes = Mathf.Max(0, maxAirDashes);

            // 벽 관련 값들 검증
            wallSlideSpeed = Mathf.Min(0f, wallSlideSpeed);
            wallJumpVelocity = new Vector2(Mathf.Max(0f, wallJumpVelocity.x), Mathf.Max(0f, wallJumpVelocity.y));
            wallStickTime = Mathf.Max(0f, wallStickTime);
            wallJumpCooldown = Mathf.Max(0f, wallJumpCooldown);
            wallDetectionDistance = Mathf.Max(0.1f, wallDetectionDistance);

            // 충돌 감지 값들 검증
            groundCheckDistance = Mathf.Max(0.01f, groundCheckDistance);
            groundCheckWidth = Mathf.Max(0.1f, groundCheckWidth);
            wallCheckDistance = Mathf.Max(0.01f, wallCheckDistance);
        }

        private void OnValidate()
        {
            ValidateSettings();
        }
    }
}