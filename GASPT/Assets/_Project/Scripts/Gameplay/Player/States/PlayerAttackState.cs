using System.Threading;
using UnityEngine;
using Combat.Attack;
using Combat.Core;
using Core.Enums;

namespace Player
{
    /// <summary>
    /// 플레이어 공격 상태 (Combat 시스템 통합)
    /// ComboSystem과 연동하여 콤보 공격 처리
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

        protected override async Awaitable EnterState(CancellationToken cancellationToken)
        {
            LogStateDebug("공격 상태 진입");
            attackTriggered = false;
            attackAnimationTime = 0f;

            // ComboSystem에 타격 등록
            if (playerController.ComboSystem != null)
            {
                var comboSystem = playerController.ComboSystem;
                int currentComboIndex = comboSystem.CurrentComboIndex;

                bool registered = comboSystem.RegisterHit(currentComboIndex);

                if (registered)
                {
                    LogStateDebug($"콤보 등록 성공: index {currentComboIndex}");

                    // 현재 콤보 데이터 가져오기
                    var comboData = comboSystem.CurrentComboData;

                    if (comboData != null)
                    {
                        // AttackAnimationHandler를 통해 애니메이션 재생
                        if (playerController.AttackAnimationHandler != null)
                        {
                            playerController.AttackAnimationHandler.TriggerAttackAnimation(
                                comboData.comboIndex,
                                comboData.animationSpeed
                            );
                        }

                        attackTriggered = true;
                    }
                    else
                    {
                        LogStateDebug("콤보 데이터 없음 - 기본 공격 실행");
                        // 콤보 데이터 없으면 기본 공격
                        ExecuteBasicAttack();
                    }
                }
                else
                {
                    LogStateDebug("콤보 등록 실패 - 기본 공격 실행");
                    ExecuteBasicAttack();
                }
            }
            else
            {
                LogStateDebug("ComboSystem 없음 - 기본 공격 실행");
                ExecuteBasicAttack();
            }

            await Awaitable.NextFrameAsync();
        }

        protected override async Awaitable ExitState(CancellationToken cancellationToken)
        {
            LogStateDebug("공격 상태 종료");
            attackTriggered = false;
            comboWindowActive = false;

            await Awaitable.NextFrameAsync();
        }

        protected override void UpdateState(float deltaTime)
        {
            if (!attackTriggered) return;

            attackAnimationTime += deltaTime;
            CheckForStateTransitions();
        }

        /// <summary>
        /// 기본 공격 실행 (ComboSystem 없을 때)
        /// </summary>
        private void ExecuteBasicAttack()
        {
            // GAS 시스템을 통한 기본 공격
            playerController.ActivateAbility("BasicAttack");

            // AttackAnimationHandler를 통해 애니메이션 재생
            if (playerController.AttackAnimationHandler != null)
            {
                playerController.AttackAnimationHandler.TriggerAttackAnimation(0, 1f);
            }

            attackTriggered = true;
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
