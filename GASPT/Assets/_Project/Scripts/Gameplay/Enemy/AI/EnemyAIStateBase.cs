using UnityEngine;

namespace GASPT.Gameplay.Enemies.AI
{
    /// <summary>
    /// AI 상태 추상 베이스 클래스
    /// 공통 유틸리티 메서드 제공
    /// </summary>
    public abstract class EnemyAIStateBase : IEnemyAIState
    {
        // ====== IEnemyAIState 구현 ======

        /// <summary>
        /// 상태 이름 (하위 클래스에서 구현)
        /// </summary>
        public abstract string StateName { get; }

        /// <summary>
        /// 상태 진입 (기본 구현: 타이머 리셋)
        /// </summary>
        public virtual void Enter(EnemyAIContext context)
        {
            context.StateTimer = 0f;

            if (context.ShowDebugLogs)
            {
                Debug.Log($"[AI] {context.Data?.enemyName} → {StateName} 진입");
            }
        }

        /// <summary>
        /// 상태 퇴장 (기본 구현: 없음)
        /// </summary>
        public virtual void Exit(EnemyAIContext context)
        {
            if (context.ShowDebugLogs)
            {
                Debug.Log($"[AI] {context.Data?.enemyName} ← {StateName} 퇴장");
            }
        }

        /// <summary>
        /// 매 프레임 업데이트 (하위 클래스에서 구현)
        /// </summary>
        public abstract void Update(EnemyAIContext context);

        /// <summary>
        /// 물리 업데이트 (기본 구현: 없음)
        /// </summary>
        public virtual void PhysicsUpdate(EnemyAIContext context)
        {
            // 기본적으로 아무것도 하지 않음
            // 이동이 필요한 상태에서 오버라이드
        }

        /// <summary>
        /// 상태 전환 체크 (하위 클래스에서 구현)
        /// </summary>
        public abstract IEnemyAIState CheckTransitions(EnemyAIContext context);


        // ====== 공통 유틸리티 메서드 ======

        /// <summary>
        /// 이동 (수평)
        /// </summary>
        protected void Move(EnemyAIContext context, Vector2 direction, float speed)
        {
            if (context.Rigidbody == null) return;

            context.Rigidbody.linearVelocity = new Vector2(
                direction.x * speed,
                context.Rigidbody.linearVelocity.y
            );

            // 방향에 따라 스프라이트 반전
            if (direction.x > 0.01f && !context.IsFacingRight)
            {
                Flip(context);
            }
            else if (direction.x < -0.01f && context.IsFacingRight)
            {
                Flip(context);
            }
        }

        /// <summary>
        /// 정지
        /// </summary>
        protected void Stop(EnemyAIContext context)
        {
            if (context.Rigidbody == null) return;
            context.Rigidbody.linearVelocity = new Vector2(0, context.Rigidbody.linearVelocity.y);
        }

        /// <summary>
        /// 좌우 반전
        /// </summary>
        protected void Flip(EnemyAIContext context)
        {
            context.IsFacingRight = !context.IsFacingRight;

            if (context.SpriteRenderer != null)
            {
                context.SpriteRenderer.flipX = !context.IsFacingRight;
            }
            else
            {
                // SpriteRenderer가 없으면 scale로 반전
                Vector3 scale = context.Transform.localScale;
                scale.x *= -1;
                context.Transform.localScale = scale;
            }
        }

        /// <summary>
        /// 플레이어를 향해 바라보기
        /// </summary>
        protected void FacePlayer(EnemyAIContext context)
        {
            if (context.PlayerTransform == null) return;

            bool shouldFaceRight = context.PlayerTransform.position.x > context.Transform.position.x;

            if (shouldFaceRight != context.IsFacingRight)
            {
                Flip(context);
            }
        }

        /// <summary>
        /// 플레이어에게 데미지
        /// </summary>
        protected void DealDamageToPlayer(EnemyAIContext context)
        {
            if (context.PlayerStats == null) return;
            if (!context.CanAttack()) return;

            // Enemy의 DealDamageTo 메서드 호출 (존재하면)
            if (context.Enemy is PlatformerEnemy platformerEnemy)
            {
                // PlatformerEnemy의 protected 메서드에 접근 불가
                // 대신 공개 메서드나 이벤트 사용
                context.PlayerStats.TakeDamage(context.Data?.attack ?? 5);
            }

            context.LastAttackTime = Time.time;

            if (context.ShowDebugLogs)
            {
                Debug.Log($"[AI] {context.Data?.enemyName} 플레이어 공격! 데미지: {context.Data?.attack}");
            }
        }
    }
}
