namespace Player
{
    /// <summary>
    /// 플레이어의 상태 타입 정의
    /// FSM에서 사용할 플레이어 행동 상태들
    /// </summary>
    public enum PlayerStateType
    {
        /// <summary>기본 대기 상태</summary>
        Idle,

        /// <summary>걷기/달리기 상태</summary>
        Move,

        /// <summary>점프 상태</summary>
        Jump,

        /// <summary>떨어지는 상태</summary>
        Fall,

        /// <summary>대시 상태</summary>
        Dash,

        /// <summary>공격 상태</summary>
        Attack,

        /// <summary>피격 상태</summary>
        Hit,

        /// <summary>죽음 상태</summary>
        Dead,

        /// <summary>벽에 매달린 상태</summary>
        WallGrab,

        /// <summary>벽 점프 상태</summary>
        WallJump,

        /// <summary>슬라이딩 상태</summary>
        Slide
    }

    /// <summary>
    /// 플레이어 이벤트 타입 정의
    /// 상태 전환을 트리거하는 이벤트들
    /// </summary>
    public enum PlayerEventType
    {
        // 이동 관련
        StartMove,
        StopMove,

        // 점프 관련
        JumpPressed,
        JumpReleased,
        TouchGround,
        LeaveGround,

        // 대시 관련
        DashPressed,
        DashCompleted,

        // 공격 관련
        AttackPressed,
        AttackCompleted,

        // 피격 관련
        TakeDamage,
        RecoverFromHit,

        // 죽음 관련
        Die,
        Respawn,

        // 벽 관련
        TouchWall,
        LeaveWall,
        WallJumpPressed,

        // 슬라이딩 관련
        SlidePressed,
        SlideCompleted
    }
}