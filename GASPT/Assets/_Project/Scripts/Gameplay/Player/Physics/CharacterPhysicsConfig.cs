using UnityEngine;

namespace Character.Physics
{
    /// <summary>
    /// 캐릭터 물리 설정 ScriptableObject
    /// Skul 스타일 플랫폼 액션 게임을 위한 물리 파라미터 정의
    /// </summary>
    [CreateAssetMenu(fileName = "CharacterPhysicsConfig", menuName = "Character/Physics Config", order = 1)]
    public class CharacterPhysicsConfig : ScriptableObject
    {
        [Header("Skul 프리셋")]
        [SerializeField] public bool useSkulPreset = true;

        [Space(10)]
        [Header("기본 이동")]
        [SerializeField] public float moveSpeed = 10f;
        [SerializeField] public float airMoveSpeed = 8f;
        [SerializeField] public float acceleration = 50f;
        [SerializeField] public float deceleration = 50f;

        [Header("점프 설정")]
        [SerializeField] public float jumpVelocity = 18f;
        [SerializeField] public float jumpCutMultiplier = 0.5f;
        [SerializeField] public float coyoteTime = 0.1f;
        [SerializeField] public float jumpBufferTime = 0.1f;

        [Header("중력과 낙하")]
        [SerializeField] public float gravity = 35f;
        [SerializeField] public float maxFallSpeed = -25f;
        [SerializeField] public float quickFallSpeed = -30f;
        [SerializeField] public float fallMultiplier = 2.5f;
        [SerializeField] public float lowJumpMultiplier = 2f;

        [Header("대시 설정")]
        [SerializeField] public float dashSpeed = 25f;
        [SerializeField] public float dashDuration = 0.2f;
        [SerializeField] public float dashCooldown = 1f;
        [SerializeField] public bool canDashInAir = true;

        [Header("벽 점프")]
        [SerializeField] public float wallJumpForce = 15f;
        [SerializeField] public float wallJumpAngle = 45f;
        [SerializeField] public float wallSlideSpeed = -5f;
        [SerializeField] public float wallStickTime = 0.1f;

        [Header("충돌 검사")]
        [SerializeField] public float skinWidth = 0.08f;
        [SerializeField] public int horizontalRayCount = 4;
        [SerializeField] public int verticalRayCount = 4;
        [SerializeField] public LayerMask collisionMask = 1;

        [Header("디버그")]
        [SerializeField] public bool enableDebugLogs = false;
        [SerializeField] public bool showRaycastGizmos = false;

        /// <summary>
        /// Skul 게임 스타일의 프리셋 적용
        /// </summary>
        [ContextMenu("Apply Skul Preset")]
        public void ApplySkulPreset()
        {
            useSkulPreset = true;

            // Skul 스타일 기본 이동
            moveSpeed = 10f;
            airMoveSpeed = 8f;
            acceleration = 50f;
            deceleration = 50f;

            // Skul 스타일 점프 (높고 정밀한 점프)
            jumpVelocity = 18f;
            jumpCutMultiplier = 0.5f;
            coyoteTime = 0.15f;
            jumpBufferTime = 0.15f;

            // Skul 스타일 중력 (빠른 낙하)
            gravity = 35f;
            maxFallSpeed = -25f;
            quickFallSpeed = -30f;
            fallMultiplier = 2.5f;
            lowJumpMultiplier = 2f;

            // Skul 스타일 대시 (빠르고 직선적)
            dashSpeed = 25f;
            dashDuration = 0.2f;
            dashCooldown = 1f;
            canDashInAir = true;

            // Skul 스타일 벽 점프
            wallJumpForce = 15f;
            wallJumpAngle = 45f;
            wallSlideSpeed = -5f;
            wallStickTime = 0.1f;

            // 정밀한 충돌 검사
            skinWidth = 0.08f;
            horizontalRayCount = 4;
            verticalRayCount = 4;

            Debug.Log("Skul 프리셋이 적용되었습니다!");
        }

        /// <summary>
        /// 설정 검증
        /// </summary>
        public void ValidateSettings()
        {
            // 음수 값들 검증
            moveSpeed = Mathf.Max(0f, moveSpeed);
            airMoveSpeed = Mathf.Max(0f, airMoveSpeed);
            acceleration = Mathf.Max(0f, acceleration);
            deceleration = Mathf.Max(0f, deceleration);

            jumpVelocity = Mathf.Max(0f, jumpVelocity);
            gravity = Mathf.Max(0f, gravity);

            dashSpeed = Mathf.Max(0f, dashSpeed);
            dashDuration = Mathf.Max(0f, dashDuration);
            dashCooldown = Mathf.Max(0f, dashCooldown);

            // 레이캐스트 개수 검증
            horizontalRayCount = Mathf.Max(2, horizontalRayCount);
            verticalRayCount = Mathf.Max(2, verticalRayCount);

            // 스킨 두께 검증
            skinWidth = Mathf.Max(0.01f, skinWidth);
        }

        private void OnValidate()
        {
            ValidateSettings();
        }
    }
}