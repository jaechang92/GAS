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

            // InputHandler에서 요청된 어빌리티 ID 가져오기
            string abilityId = playerController.PlayerInput?.PendingAbilityId;

            // 체이닝 중이 아닐 때만 입력된 어빌리티 사용
            // 체이닝 중이면 AbilitySystem이 자동으로 다음 체인 실행
            if (string.IsNullOrEmpty(abilityId))
            {
                // 기본값: PlayerAttack_1
                abilityId = "PlayerAttack_1";
            }

            // 공격 입력 리셋
            playerController.PlayerInput?.ResetAttack();
            playerController.PlayerInput?.ResetPendingAbility();

            // GAS를 통해 어빌리티 실행 (체이닝은 AbilitySystem이 자동 처리)
            playerController.ActivateAbility(abilityId);
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
