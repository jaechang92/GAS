using UnityEngine;
using FSM.Core;
using System.Threading;

namespace Player
{
    /// <summary>
    /// 플레이어 상태의 기본 클래스
    /// 모든 플레이어 상태가 상속받는 베이스 클래스
    /// 커스텀 물리 시스템 사용
    /// </summary>
    public abstract class PlayerBaseState : IState
    {
        protected PlayerController playerController;
        protected PlayerStateType stateType;

        public string Id { get; private set; }
        public string Name { get; private set; }
        public bool IsActive { get; private set; }
        public GameObject Owner { get; private set; }
        public IStateMachine StateMachine { get; private set; }

        // 이벤트
        public event System.Action<IState> OnEntered;
        public event System.Action<IState> OnExited;

        protected PlayerBaseState(PlayerStateType type)
        {
            stateType = type;
            Id = type.ToString();
            Name = type.ToString();
        }

        public virtual void Initialize(string id, GameObject owner, IStateMachine stateMachine)
        {
            Id = id;
            Owner = owner;
            StateMachine = stateMachine;

            playerController = owner.GetComponent<PlayerController>();

            if (playerController == null)
            {
                Debug.LogError($"[{stateType}State] PlayerController를 찾을 수 없습니다!");
            }
        }

        public virtual async Awaitable OnEnter(CancellationToken cancellationToken = default)
        {
            Debug.Log($"[PlayerState] {stateType} 상태 진입");
            IsActive = true;

            await EnterState(cancellationToken);
            OnEntered?.Invoke(this);
        }

        public virtual async Awaitable OnExit(CancellationToken cancellationToken = default)
        {
            Debug.Log($"[PlayerState] {stateType} 상태 종료");
            IsActive = false;

            await ExitState(cancellationToken);
            OnExited?.Invoke(this);
        }

        public virtual void OnUpdate(float deltaTime)
        {
            UpdateState(deltaTime);
        }

        // 하위 클래스에서 구현할 추상 메서드들
        protected abstract Awaitable EnterState(CancellationToken cancellationToken);
        protected abstract Awaitable ExitState(CancellationToken cancellationToken);
        protected abstract void UpdateState(float deltaTime);

        // 커스텀 물리 시스템용 공통 유틸리티 메서드들 (간소화됨)
        // 대부분의 물리 연산은 PhysicsEngine에서 자동으로 처리됨

        protected void StopHorizontalMovement()
        {
            if (playerController == null) return;

            Vector3 velocity = playerController.Velocity;
            velocity.x = 0;
            playerController.SetVelocity(velocity);
        }

        protected void StopVerticalMovement()
        {
            if (playerController == null) return;

            Vector3 velocity = playerController.Velocity;
            velocity.y = 0;
            playerController.SetVelocity(velocity);
        }

        protected bool CanTransitionTo(PlayerStateType targetState)
        {
            // 기본적인 상태 전환 검증 로직
            if (playerController == null) return false;

            switch (targetState)
            {
                case PlayerStateType.Jump:
                    return playerController.IsGrounded || playerController.IsTouchingWall;

                case PlayerStateType.Dash:
                    return playerController.CanDash;

                case PlayerStateType.WallGrab:
                    return playerController.IsTouchingWall && !playerController.IsGrounded;

                case PlayerStateType.Fall:
                    return !playerController.IsGrounded && playerController.Velocity.y < 0;

                default:
                    return true;
            }
        }

        protected void LogStateDebug(string message)
        {
            Debug.Log($"[{stateType}State] {message}");
        }
    }
}