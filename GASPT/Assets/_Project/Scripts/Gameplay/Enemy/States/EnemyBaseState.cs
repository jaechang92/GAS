using UnityEngine;
using FSM.Core;
using System.Threading;

namespace Enemy
{
    /// <summary>
    /// 적 상태의 기본 클래스
    /// 모든 적 상태가 상속받는 베이스 클래스
    /// </summary>
    public abstract class EnemyBaseState : IState
    {
        protected EnemyController enemy;
        protected EnemyStateType stateType;

        public string Id { get; private set; }
        public string Name { get; private set; }
        public bool IsActive { get; private set; }
        public GameObject Owner { get; private set; }
        public IStateMachine StateMachine { get; private set; }

        // 이벤트
        public event System.Action<IState> OnEntered;
        public event System.Action<IState> OnExited;

        protected EnemyBaseState(EnemyStateType type)
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

            enemy = owner.GetComponent<EnemyController>();

            if (enemy == null)
            {
                Debug.LogError($"[{stateType}State] EnemyController를 찾을 수 없습니다!");
            }
        }

        // === 동기 메서드 (Combat용 - 기본 구현) ===
        public virtual void OnEnterSync()
        {
            Debug.Log($"[EnemyState] {stateType} 상태 진입(동기)");
            IsActive = true;

            EnterStateSync();
            OnEntered?.Invoke(this);
        }

        public virtual void OnExitSync()
        {
            Debug.Log($"[EnemyState] {stateType} 상태 종료(동기)");
            IsActive = false;

            ExitStateSync();
            OnExited?.Invoke(this);
        }

        // === 비동기 메서드 (GameFlow용 - 동기 호출) ===
        public virtual async Awaitable OnEnter(CancellationToken cancellationToken = default)
        {
            // 동기 메서드 호출
            OnEnterSync();
            await Awaitable.NextFrameAsync(cancellationToken);
        }

        public virtual async Awaitable OnExit(CancellationToken cancellationToken = default)
        {
            // 동기 메서드 호출
            OnExitSync();
            await Awaitable.NextFrameAsync(cancellationToken);
        }

        public virtual void OnUpdate(float deltaTime)
        {
            UpdateState(deltaTime);
        }

        // === 하위 클래스에서 구현할 동기 메서드들 ===
        protected abstract void EnterStateSync();
        protected abstract void ExitStateSync();
        protected abstract void UpdateState(float deltaTime);

        // Enemy용 유틸리티 메서드들
        protected void StopMovement()
        {
            if (enemy == null || enemy.Rigidbody == null) return;

            enemy.Rigidbody.linearVelocity = Vector2.zero;
        }

        protected void MoveToTarget(float speed)
        {
            if (enemy == null || enemy.Target == null || enemy.Data == null) return;

            float distanceToTarget = enemy.DistanceToTarget;

            // 공격 범위 내에 있으면 정지
            if (distanceToTarget <= enemy.Data.attackRange)
            {
                StopMovement();
                FaceTarget();
                return;
            }

            // 공격 범위 밖이면 이동
            Vector2 direction = (enemy.Target.position - enemy.transform.position).normalized;
            enemy.Rigidbody.linearVelocity = new Vector2(direction.x * speed, enemy.Rigidbody.linearVelocity.y);

            // 타겟 방향으로 회전
            FaceTarget();
        }

        protected void FaceTarget()
        {
            if (enemy == null || enemy.Target == null) return;

            float direction = enemy.Target.position.x - enemy.transform.position.x;
            if (Mathf.Abs(direction) > 0.1f)
            {
                enemy.FacingDirection = direction > 0 ? 1 : -1;
            }
        }

        protected void LogStateDebug(string message)
        {
            Debug.Log($"[{stateType}State] {message}");
        }
    }
}
