using UnityEngine;
using Core.Enums;

namespace Player
{
    /// <summary>
    /// 플레이어 공격 상태 (GAS 기반 Combat 시스템)
    /// ComboSystem과 GAS를 연동하여 콤보 공격 처리
    /// </summary>
    public class PlayerAttackState : PlayerBaseState
    {
        private bool attackTriggered = false;
        private bool comboWindowActive = false;
        private float attackAnimationTime = 0f;

        public PlayerAttackState(PlayerController controller) : base(PlayerStateType.Attack)
        {
            playerController = controller;
        }

        protected override void EnterStateSync()
        {
            LogStateDebug("공격 상태 진입(동기)");
            attackTriggered = false;
            attackAnimationTime = 0f;

            // 공격 입력 즉시 리셋 (중복 전환 방지)
            playerController.PlayerInput?.ResetAttack();

            // GAS가 체이닝을 자동으로 처리
            playerController.ActivateAbility("PlayerAttack_1");
            attackTriggered = true;
        }

        protected override void ExitStateSync()
        {
            LogStateDebug("공격 상태 종료(동기)");
            attackTriggered = false;
            comboWindowActive = false;
        }

        protected override void UpdateState(float deltaTime)
        {
            if (!attackTriggered) return;

            attackAnimationTime += deltaTime;

            // GAS가 히트박스/VFX/사운드를 자동으로 처리
            // FSM의 Attack→Attack 자동 전환에 의존하여 콤보 처리

            CheckForStateTransitions();
        }

        private void CheckForStateTransitions()
        {
            if (playerController == null) return;

            // AttackAnimationHandler가 있으면 애니메이션 상태 확인
            if (playerController.AttackAnimationHandler != null)
            {
                // 공격 중이 아니면 다음 상태로 전환
                if (!playerController.IsAttacking)
                {
                    TransitionToNextState();
                }
            }
            else
            {
                // AttackAnimationHandler 없으면 0.3초 타이머 사용
                if (attackAnimationTime >= 0.3f)
                {
                    TransitionToNextState();
                }
            }
        }

        private void TransitionToNextState()
        {
            if (playerController.IsGrounded)
            {
                if (Mathf.Abs(playerController.Velocity.x) > 0.1f)
                    playerController.ChangeState(PlayerStateType.Move);
                else
                    playerController.ChangeState(PlayerStateType.Idle);
            }
            else
            {
                playerController.ChangeState(PlayerStateType.Fall);
            }
        }
    }
}
