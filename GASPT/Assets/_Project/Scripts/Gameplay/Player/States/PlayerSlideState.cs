using System.Threading;
using UnityEngine;

namespace Player
{
    /// <summary>
    /// 플레이어 슬라이딩 상태
    /// 플레이어가 슬라이딩할 때의 상태
    /// </summary>
    public class PlayerSlideState : PlayerBaseState
    {
        private float slideSpeed = 12f;
        private float slideDuration = 0.8f;
        private float slideTime = 0f;
        private float originalColliderHeight;
        private float slideColliderHeight = 0.5f;
        private Vector2 originalColliderOffset;

        public PlayerSlideState() : base(PlayerStateType.Slide) { }

        protected override async Awaitable EnterState(CancellationToken cancellationToken)
        {
            LogStateDebug("슬라이딩 상태 진입");

            // 슬라이딩 초기화
            slideTime = 0f;

            // TODO: PlayerStats에서 슬라이딩 설정 가져오기
            slideSpeed = 12f;
            slideDuration = 0.8f;
            slideColliderHeight = 0.5f;

            // 콜라이더 크기 변경 (슬라이딩 자세)
            ModifyColliderForSlide();

            // 슬라이딩 시작
            if (playerController != null)
            {
                Vector2 velocity = playerController.Velocity;
                velocity.x = slideSpeed * playerController.FacingDirection;
                playerController.SetVelocity(velocity);
            }

            // 슬라이딩 입력 리셋
            playerController.ResetSlide();

            await Awaitable.NextFrameAsync();
        }

        protected override async Awaitable ExitState(CancellationToken cancellationToken)
        {
            LogStateDebug("슬라이딩 상태 종료");

            // 콜라이더 원래 크기로 복구
            RestoreCollider();

            await Awaitable.NextFrameAsync();
        }

        protected override void UpdateState(float deltaTime)
        {
            slideTime += deltaTime;

            // 슬라이딩 속도 감소
            HandleSlideMovement();

            // 중력 적용
            ApplyGravity();

            // 상태 전환 체크
            CheckForStateTransitions();
        }

        private void ModifyColliderForSlide()
        {
            if (playerController == null) return;

            // 현재 콜라이더 정보 저장
            Collider2D collider = playerController.GetComponent<Collider2D>();

            if (collider is BoxCollider2D boxCollider)
            {
                originalColliderHeight = boxCollider.size.y;
                originalColliderOffset = boxCollider.offset;

                // 슬라이딩용으로 콜라이더 높이 줄이기
                Vector2 newSize = boxCollider.size;
                newSize.y = slideColliderHeight;
                boxCollider.size = newSize;

                // 콜라이더 위치 조정 (바닥에 맞춤)
                Vector2 newOffset = boxCollider.offset;
                newOffset.y = originalColliderOffset.y - (originalColliderHeight - slideColliderHeight) * 0.5f;
                boxCollider.offset = newOffset;
            }
            else if (collider is CapsuleCollider2D capsuleCollider)
            {
                originalColliderHeight = capsuleCollider.size.y;
                originalColliderOffset = capsuleCollider.offset;

                // 슬라이딩용으로 콜라이더 높이 줄이기
                Vector2 newSize = capsuleCollider.size;
                newSize.y = slideColliderHeight;
                capsuleCollider.size = newSize;

                // 콜라이더 위치 조정
                Vector2 newOffset = capsuleCollider.offset;
                newOffset.y = originalColliderOffset.y - (originalColliderHeight - slideColliderHeight) * 0.5f;
                capsuleCollider.offset = newOffset;
            }
        }

        private void RestoreCollider()
        {
            if (playerController == null) return;

            Collider2D collider = playerController.GetComponent<Collider2D>();

            if (collider is BoxCollider2D boxCollider)
            {
                Vector2 newSize = boxCollider.size;
                newSize.y = originalColliderHeight;
                boxCollider.size = newSize;
                boxCollider.offset = originalColliderOffset;
            }
            else if (collider is CapsuleCollider2D capsuleCollider)
            {
                Vector2 newSize = capsuleCollider.size;
                newSize.y = originalColliderHeight;
                capsuleCollider.size = newSize;
                capsuleCollider.offset = originalColliderOffset;
            }
        }

        private void HandleSlideMovement()
        {
            if (playerController == null || playerController == null) return;

            // 슬라이딩 속도 점진적 감소
            Vector2 velocity = playerController.Velocity;

            // 시간에 따른 속도 감소 (처음에는 빠르게, 나중에는 천천히)
            float speedDecay = Mathf.Lerp(1f, 0.3f, slideTime / slideDuration);
            float currentSlideSpeed = slideSpeed * speedDecay;

            velocity.x = currentSlideSpeed * playerController.FacingDirection;
            playerController.SetVelocity(velocity);
        }

        private void CheckForStateTransitions()
        {
            if (playerController == null || playerController == null) return;

            // 슬라이딩 지속시간 완료
            if (slideTime >= slideDuration)
            {
                CompleteSlide();
                return;
            }

            // 땅에서 떨어지면 Fall 상태로
            if (!playerController.IsGrounded)
            {
                StateMachine?.ForceTransitionTo(PlayerStateType.Fall.ToString());
                return;
            }

            // 벽에 부딪히면 슬라이딩 종료
            if (playerController.IsTouchingWall)
            {
                CompleteSlide();
                return;
            }

            // 점프 입력이 있으면 슬라이딩에서 점프
            if (playerController.IsJumpPressed())
            {
                StateMachine?.ForceTransitionTo(PlayerStateType.Jump.ToString());
                return;
            }

            // 슬라이딩 속도가 너무 느려지면 종료
            if (Mathf.Abs(playerController.Velocity.x) < 2f)
            {
                CompleteSlide();
                return;
            }
        }

        private void CompleteSlide()
        {
            // 슬라이딩 완료 후 상태 결정
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
    }
}
