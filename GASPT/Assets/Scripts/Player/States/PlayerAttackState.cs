using System.Threading;
using UnityEngine;

namespace Player
{
    /// <summary>
    /// 플레이어 공격 상태
    /// 플레이어가 공격할 때의 상태
    /// </summary>
    public class PlayerAttackState : PlayerBaseState
    {
        private float attackDuration = 0.4f;
        private float attackTime = 0f;
        private bool attackHitboxActive = false;
        private float attackRange = 1.5f;
        private int attackDamage = 25;

        public PlayerAttackState() : base(PlayerStateType.Attack) { }

        protected override async Awaitable EnterState(CancellationToken cancellationToken)
        {
            LogStateDebug("공격 상태 진입");

            // 공격 초기화
            attackTime = 0f;
            attackHitboxActive = false;

            // TODO: PlayerStats에서 공격 설정 가져오기
            attackDuration = 0.4f;
            attackRange = 1.5f;
            attackDamage = 25;

            // 공격 중 이동 제한
            if (playerController != null)
            {
                Vector2 velocity = playerController.Velocity;
                velocity.x *= 0.5f; // 공격 중에는 이동 속도 감소
                playerController.SetVelocity(velocity);
            }

            // 공격 입력 리셋
            playerController.ResetAttack();

            await Awaitable.NextFrameAsync();
        }

        protected override async Awaitable ExitState(CancellationToken cancellationToken)
        {
            LogStateDebug("공격 상태 종료");

            // 공격 히트박스 비활성화
            attackHitboxActive = false;

            await Awaitable.NextFrameAsync();
        }

        protected override void UpdateState(float deltaTime)
        {
            attackTime += deltaTime;

            // 공격 히트박스 활성화 타이밍 (공격 시작 후 0.1초~0.3초)
            if (attackTime >= 0.1f && attackTime <= 0.3f && !attackHitboxActive)
            {
                ActivateAttackHitbox();
            }
            else if (attackTime > 0.3f && attackHitboxActive)
            {
                DeactivateAttackHitbox();
            }

            // 공격 완료 확인
            if (attackTime >= attackDuration)
            {
                CompleteAttack();
                return;
            }

            // 공격 중에도 중력은 적용
            if (!playerController.IsGrounded)
            {
                ApplyGravity();
            }

            // 제한된 이동 허용 (공격 중에도 약간의 조작 가능)
            HandleLimitedMovement();
        }

        private void ActivateAttackHitbox()
        {
            if (attackHitboxActive) return;

            attackHitboxActive = true;
            LogStateDebug("공격 히트박스 활성화");

            // 공격 범위 내의 적들 검사
            PerformAttack();
        }

        private void DeactivateAttackHitbox()
        {
            if (!attackHitboxActive) return;

            attackHitboxActive = false;
            LogStateDebug("공격 히트박스 비활성화");
        }

        private void PerformAttack()
        {
            if (playerController == null) return;

            // 공격 위치 계산
            Vector3 attackPosition = playerController.transform.position;
            attackPosition.x += attackRange * 0.5f * playerController.FacingDirection;

            // 공격 범위 내의 적들 찾기
            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(
                attackPosition,
                attackRange,
                LayerMask.GetMask("Enemy") // Enemy 레이어의 오브젝트들
            );

            foreach (var enemy in hitEnemies)
            {
                // TODO: 적에게 데미지 적용
                LogStateDebug($"적 공격: {enemy.name}");

                // 적에게 넉백 효과 적용
                ApplyKnockback(enemy);
            }

            // 공격 이펙트 생성
            CreateAttackEffect(attackPosition);
        }

        private void ApplyKnockback(Collider2D enemy)
        {
            Rigidbody2D enemyRb = enemy.GetComponent<Rigidbody2D>();
            if (enemyRb != null)
            {
                Vector2 knockbackDirection = (enemy.transform.position - playerController.transform.position).normalized;
                knockbackDirection.y = 0.3f; // 약간 위로 띄우기
                enemyRb.AddForce(knockbackDirection * 10f, ForceMode2D.Impulse);
            }
        }

        private void CreateAttackEffect(Vector3 position)
        {
            // TODO: 공격 이펙트 파티클이나 애니메이션 생성
            LogStateDebug($"공격 이펙트 생성: {position}");
        }

        private void HandleLimitedMovement()
        {
            if (playerController == null || playerController == null) return;

            // 공격 중에도 약간의 이동 허용 (50% 속도)
            Vector2 input = playerController.GetInputVector();
            Vector2 velocity = playerController.Velocity;

            float limitedMoveSpeed = 4f; // 공격 중 이동 속도
            float targetVelocityX = input.x * limitedMoveSpeed;

            velocity.x = Mathf.MoveTowards(velocity.x, targetVelocityX, 20f * Time.fixedDeltaTime);
            playerController.SetVelocity(velocity);
        }

        private void CompleteAttack()
        {
            // 공격 완료 후 상태 결정
            if (playerController.IsGrounded)
            {
                // 땅에 있으면 이동 입력에 따라 상태 결정
                Vector2 input = playerController.GetInputVector();
                if (Mathf.Abs(input.x) > 0.1f)
                {
                    StateMachine?.ForceTransitionTo(PlayerStateType.Move.ToString());
                }
                else
                {
                    StateMachine?.ForceTransitionTo(PlayerStateType.Idle.ToString());
                }
            }
            else
            {
                // 공중에 있으면 낙하 상태로
                StateMachine?.ForceTransitionTo(PlayerStateType.Fall.ToString());
            }
        }

        // 디버그용 공격 범위 그리기
        private void OnDrawGizmosSelected()
        {
            if (playerController == null || !attackHitboxActive) return;

            Vector3 attackPosition = playerController.transform.position;
            attackPosition.x += attackRange * 0.5f * playerController.FacingDirection;

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPosition, attackRange);
        }
    }
}
