using UnityEngine;
using FSM.Core;
using System.Threading;

namespace Player
{
    /// <summary>
    /// 플레이어 상태의 기본 클래스
    /// 모든 플레이어 상태가 상속받는 베이스 클래스
    /// </summary>
    public abstract class PlayerBaseState : IState
    {
        protected PlayerController playerController;
        protected Rigidbody2D rb;
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
            rb = owner.GetComponent<Rigidbody2D>();

            if (playerController == null)
            {
                Debug.LogError($"[{stateType}State] PlayerController를 찾을 수 없습니다!");
            }

            if (rb == null)
            {
                Debug.LogError($"[{stateType}State] Rigidbody2D를 찾을 수 없습니다!");
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

        // 공통 유틸리티 메서드들
        protected void ApplyMovement(float speed)
        {
            if (rb == null || playerController == null) return;

            Vector2 inputVector = playerController.GetInputVector();
            Vector2 velocity = rb.linearVelocity;
            velocity.x = inputVector.x * speed;
            rb.linearVelocity = velocity;
        }

        protected void ApplyJump(float force)
        {
            if (rb == null) return;

            Vector2 velocity = rb.linearVelocity;
            velocity.y = force;
            rb.linearVelocity = velocity;
        }

        protected void ApplyDash(float speed, int direction)
        {
            if (rb == null) return;

            Vector2 velocity = rb.linearVelocity;
            velocity.x = speed * direction;
            velocity.y = 0; // 대시 중에는 중력 무시
            rb.linearVelocity = velocity;
        }

        protected void StopHorizontalMovement()
        {
            if (rb == null) return;

            Vector2 velocity = rb.linearVelocity;
            velocity.x = 0;
            rb.linearVelocity = velocity;
        }

        protected void StopVerticalMovement()
        {
            if (rb == null) return;

            Vector2 velocity = rb.linearVelocity;
            velocity.y = 0;
            rb.linearVelocity = velocity;
        }

        protected void ApplyGravity(float multiplier = 1f)
        {
            if (rb == null) return;

            if (rb.linearVelocity.y < 0)
            {
                rb.gravityScale = 3f * multiplier; // 떨어질 때 중력 증가
            }
            else
            {
                rb.gravityScale = 3f;
            }
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
                    return !playerController.IsGrounded && rb.linearVelocity.y < 0;

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