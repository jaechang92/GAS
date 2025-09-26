using UnityEngine;

namespace Character.Physics
{
    /// <summary>
    /// 충돌 정보 인터페이스
    /// </summary>
    public interface ICollisionInfo
    {
        bool Above { get; }
        bool Below { get; }
        bool Left { get; }
        bool Right { get; }
        bool ClimbingSlope { get; }
        bool DescendingSlope { get; }
        float SlopeAngle { get; }
        Vector3 VelocityOld { get; }
        int FaceDirection { get; }
        bool FallingThroughPlatform { get; }
    }

    /// <summary>
    /// 이동 상태 인터페이스
    /// </summary>
    public interface IMovementState
    {
        Vector3 Velocity { get; }
        Vector3 PreviousVelocity { get; }
        float HorizontalInput { get; }
        bool IsMoving { get; }
        float MovementSpeed { get; }
    }

    /// <summary>
    /// 접지 상태 인터페이스
    /// </summary>
    public interface IGroundState
    {
        bool IsGrounded { get; }
        bool WasGrounded { get; }
        float TimeOnGround { get; }
        float TimeInAir { get; }
        float CoyoteTime { get; }
        bool CanCoyoteJump { get; }
    }

    /// <summary>
    /// 벽 상태 인터페이스
    /// </summary>
    public interface IWallState
    {
        bool IsTouchingWall { get; }
        bool WasTouchingWall { get; }
        int WallDirection { get; }
        float TimeOnWall { get; }
        bool CanWallJump { get; }
    }

    /// <summary>
    /// 점프 상태 인터페이스
    /// </summary>
    public interface IJumpState
    {
        bool IsJumping { get; }
        bool JumpPressed { get; }
        bool JumpHeld { get; }
        float JumpBufferTime { get; }
        bool HasDoubleJump { get; }
        int JumpCount { get; }
        float JumpTime { get; }
    }

    /// <summary>
    /// 대시 상태 인터페이스
    /// </summary>
    public interface IDashState
    {
        bool IsDashing { get; }
        bool CanDash { get; }
        Vector2 DashDirection { get; }
        float DashTime { get; }
        float DashCooldown { get; }
        int DashCount { get; }
    }

    /// <summary>
    /// 물리 상태 통합 인터페이스
    /// </summary>
    public interface IPhysicsState : IMovementState, IGroundState, IWallState
    {
        void SetVelocity(Vector3 velocity);
        void AddVelocity(Vector3 velocity);
        void SetHorizontalInput(float input);
        void ForceGroundState(bool grounded);
        void ForceWallState(bool touching, int direction = 0);
        void UpdateState(ICollisionInfo collisionInfo, float deltaTime);
    }

    /// <summary>
    /// 충돌 검사 인터페이스
    /// </summary>
    public interface ICollisionDetector
    {
        ICollisionInfo CollisionInfo { get; }
        void UpdateCollisions(Vector3 velocity);
        bool CheckGround(Vector3 position, float distance = 0.1f);
        bool CheckWall(Vector3 position, Vector3 direction, float distance = 0.1f);
        bool CheckCeiling(Vector3 position, float distance = 0.1f);
    }

    /// <summary>
    /// 이동 처리 인터페이스
    /// </summary>
    public interface IMovementProcessor
    {
        bool CanDash { get; }
        bool IsDashing { get; }
        IJumpState JumpState { get; }
        IDashState DashState { get; }

        void SetHorizontalInput(float input);
        void SetJumpInput(bool pressed, bool held);
        void PerformDash(Vector2 direction);
        void ProcessMovement(IPhysicsState physicsState, float deltaTime);
        Vector3 GetFinalVelocity();
    }
}
