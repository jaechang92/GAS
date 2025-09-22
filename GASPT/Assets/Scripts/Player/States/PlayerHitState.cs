using System.Threading;
using UnityEngine;

namespace Player
{
    /// <summary>
    /// 플레이어 피격 상태
    /// 플레이어가 데미지를 받았을 때의 상태
    /// </summary>
    public class PlayerHitState : PlayerBaseState
    {
        private float hitStunDuration = 0.5f;
        private float hitStunTime = 0f;
        private Vector2 knockbackForce = new Vector2(5f, 3f);
        private bool invulnerable = true;

        public PlayerHitState() : base(PlayerStateType.Hit) { }

        protected override async Awaitable EnterState(CancellationToken cancellationToken)
        {
            LogStateDebug("피격 상태 진입");

            // 피격 초기화
            hitStunTime = 0f;
            invulnerable = true;

            // TODO: PlayerStats에서 피격 설정 가져오기
            hitStunDuration = 0.5f;

            // 넉백 적용
            ApplyKnockback();

            // 피격 이펙트
            ShowHitEffect();

            await Awaitable.NextFrameAsync();
        }

        protected override async Awaitable ExitState(CancellationToken cancellationToken)
        {
            LogStateDebug("피격 상태 종료");

            // 무적 상태 해제 (일정 시간 후)
            await Awaitable.WaitForSecondsAsync(0.5f);
            invulnerable = false;

            // 피격 이펙트 제거
            HideHitEffect();

            await Awaitable.NextFrameAsync();
        }

        protected override void UpdateState(float deltaTime)
        {
            hitStunTime += deltaTime;

            // 넉백 감소
            HandleKnockbackDecay();

            // 중력 적용
            ApplyGravity();

            // 상태 전환 체크
            CheckForStateTransitions();
        }

        private void ApplyKnockback()
        {
            if (rb == null) return;

            // 넉백 방향 결정 (피격 방향의 반대)
            Vector2 velocity = rb.linearVelocity;
            velocity.x = -playerController.FacingDirection * knockbackForce.x;
            velocity.y = knockbackForce.y;

            rb.linearVelocity = velocity;
        }

        private void HandleKnockbackDecay()
        {
            if (rb == null) return;

            // 넉백 속도 점진적 감소
            Vector2 velocity = rb.linearVelocity;
            velocity.x = Mathf.MoveTowards(velocity.x, 0, 10f * Time.fixedDeltaTime);
            rb.linearVelocity = velocity;
        }

        private void ShowHitEffect()
        {
            // 플레이어 색상 변경 (빨간색으로 깜빡임)
            SpriteRenderer spriteRenderer = playerController.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.red;
            }

            // TODO: 피격 파티클 이펙트 추가
            LogStateDebug("피격 이펙트 표시");
        }

        private void HideHitEffect()
        {
            // 플레이어 색상 복구
            SpriteRenderer spriteRenderer = playerController.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.white;
            }

            LogStateDebug("피격 이펙트 제거");
        }

        private void CheckForStateTransitions()
        {
            if (playerController == null) return;

            // 히트 스턴 시간 완료
            if (hitStunTime >= hitStunDuration)
            {
                RecoverFromHit();
                return;
            }
        }

        private void RecoverFromHit()
        {
            // 피격 회복 후 상태 결정
            if (playerController.IsGrounded)
            {
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
                StateMachine?.ForceTransitionTo(PlayerStateType.Fall.ToString());
            }
        }

        // 무적 상태 확인 (외부에서 호출)
        public bool IsInvulnerable()
        {
            return invulnerable;
        }
    }
}