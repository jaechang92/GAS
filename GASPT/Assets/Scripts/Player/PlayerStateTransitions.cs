using FSM.Core;

namespace Player
{
    /// <summary>
    /// 플레이어 상태 전환 규칙 정의
    /// FSM의 상태 전환 로직을 설정
    /// </summary>
    public static class PlayerStateTransitions
    {
        /// <summary>
        /// 플레이어 FSM의 상태 전환 규칙 설정
        /// </summary>
        public static void SetupTransitions(StateMachine stateMachine, PlayerController playerController)
        {
            // Idle 상태에서의 전환
            SetupIdleTransitions(stateMachine);

            // Move 상태에서의 전환
            SetupMoveTransitions(stateMachine);

            // Jump 상태에서의 전환
            SetupJumpTransitions(stateMachine);

            // Fall 상태에서의 전환
            SetupFallTransitions(stateMachine);

            // Dash 상태에서의 전환
            SetupDashTransitions(stateMachine);

            // Attack 상태에서의 전환
            SetupAttackTransitions(stateMachine);

            // Hit 상태에서의 전환
            SetupHitTransitions(stateMachine);

            // Dead 상태에서의 전환
            SetupDeadTransitions(stateMachine);

            // WallGrab 상태에서의 전환
            SetupWallGrabTransitions(stateMachine);

            // WallJump 상태에서의 전환
            SetupWallJumpTransitions(stateMachine);

            // Slide 상태에서의 전환
            SetupSlideTransitions(stateMachine);

            // 공통 전환 (모든 상태에서 가능한 전환)
            SetupCommonTransitions(stateMachine);
        }

        private static void SetupIdleTransitions(StateMachine stateMachine)
        {
            // Idle → Move: 이동 입력
            stateMachine.AddTransition(
                PlayerStateType.Idle.ToString(),
                PlayerStateType.Move.ToString(),
                PlayerEventType.StartMove.ToString()
            );

            // Idle → Jump: 점프 입력
            stateMachine.AddTransition(
                PlayerStateType.Idle.ToString(),
                PlayerStateType.Jump.ToString(),
                PlayerEventType.JumpPressed.ToString()
            );

            // Idle → Fall: 땅에서 떨어짐
            stateMachine.AddTransition(
                PlayerStateType.Idle.ToString(),
                PlayerStateType.Fall.ToString(),
                PlayerEventType.LeaveGround.ToString()
            );

            // Idle → Dash: 대시 입력
            stateMachine.AddTransition(
                PlayerStateType.Idle.ToString(),
                PlayerStateType.Dash.ToString(),
                PlayerEventType.DashPressed.ToString()
            );

            // Idle → Attack: 공격 입력
            stateMachine.AddTransition(
                PlayerStateType.Idle.ToString(),
                PlayerStateType.Attack.ToString(),
                PlayerEventType.AttackPressed.ToString()
            );
        }

        private static void SetupMoveTransitions(StateMachine stateMachine)
        {
            // Move → Idle: 이동 정지
            stateMachine.AddTransition(
                PlayerStateType.Move.ToString(),
                PlayerStateType.Idle.ToString(),
                PlayerEventType.StopMove.ToString()
            );

            // Move → Jump: 점프 입력
            stateMachine.AddTransition(
                PlayerStateType.Move.ToString(),
                PlayerStateType.Jump.ToString(),
                PlayerEventType.JumpPressed.ToString()
            );

            // Move → Fall: 땅에서 떨어짐
            stateMachine.AddTransition(
                PlayerStateType.Move.ToString(),
                PlayerStateType.Fall.ToString(),
                PlayerEventType.LeaveGround.ToString()
            );

            // Move → Dash: 대시 입력
            stateMachine.AddTransition(
                PlayerStateType.Move.ToString(),
                PlayerStateType.Dash.ToString(),
                PlayerEventType.DashPressed.ToString()
            );

            // Move → Attack: 공격 입력
            stateMachine.AddTransition(
                PlayerStateType.Move.ToString(),
                PlayerStateType.Attack.ToString(),
                PlayerEventType.AttackPressed.ToString()
            );

            // Move → Slide: 슬라이딩 입력
            stateMachine.AddTransition(
                PlayerStateType.Move.ToString(),
                PlayerStateType.Slide.ToString(),
                PlayerEventType.SlidePressed.ToString()
            );
        }

        private static void SetupJumpTransitions(StateMachine stateMachine)
        {
            // Jump → Fall: 하강 시작 (자동 전환)
            // 이는 상태 내부에서 처리됨

            // Jump → Idle: 착지 + 정지
            stateMachine.AddTransition(
                PlayerStateType.Jump.ToString(),
                PlayerStateType.Idle.ToString(),
                PlayerEventType.TouchGround.ToString()
            );

            // Jump → Move: 착지 + 이동
            // 조건부 전환은 상태 내부에서 처리

            // Jump → Dash: 공중 대시
            stateMachine.AddTransition(
                PlayerStateType.Jump.ToString(),
                PlayerStateType.Dash.ToString(),
                PlayerEventType.DashPressed.ToString()
            );

            // Jump → Attack: 공중 공격
            stateMachine.AddTransition(
                PlayerStateType.Jump.ToString(),
                PlayerStateType.Attack.ToString(),
                PlayerEventType.AttackPressed.ToString()
            );

            // Jump → WallGrab: 벽 접촉
            stateMachine.AddTransition(
                PlayerStateType.Jump.ToString(),
                PlayerStateType.WallGrab.ToString(),
                PlayerEventType.TouchWall.ToString()
            );
        }

        private static void SetupFallTransitions(StateMachine stateMachine)
        {
            // Fall → Idle: 착지 + 정지
            stateMachine.AddTransition(
                PlayerStateType.Fall.ToString(),
                PlayerStateType.Idle.ToString(),
                PlayerEventType.TouchGround.ToString()
            );

            // Fall → Move: 착지 + 이동
            // 조건부 전환은 상태 내부에서 처리

            // Fall → Jump: 코요테 타임 점프
            // 조건부 전환은 상태 내부에서 처리

            // Fall → Dash: 공중 대시
            stateMachine.AddTransition(
                PlayerStateType.Fall.ToString(),
                PlayerStateType.Dash.ToString(),
                PlayerEventType.DashPressed.ToString()
            );

            // Fall → Attack: 공중 공격
            stateMachine.AddTransition(
                PlayerStateType.Fall.ToString(),
                PlayerStateType.Attack.ToString(),
                PlayerEventType.AttackPressed.ToString()
            );

            // Fall → WallGrab: 벽 접촉
            stateMachine.AddTransition(
                PlayerStateType.Fall.ToString(),
                PlayerStateType.WallGrab.ToString(),
                PlayerEventType.TouchWall.ToString()
            );
        }

        private static void SetupDashTransitions(StateMachine stateMachine)
        {
            // Dash → Idle: 대시 완료 + 정지
            stateMachine.AddTransition(
                PlayerStateType.Dash.ToString(),
                PlayerStateType.Idle.ToString(),
                PlayerEventType.DashCompleted.ToString()
            );

            // Dash → Move: 대시 완료 + 이동
            // 조건부 전환은 상태 내부에서 처리

            // Dash → Fall: 대시 완료 + 공중
            // 조건부 전환은 상태 내부에서 처리
        }

        private static void SetupAttackTransitions(StateMachine stateMachine)
        {
            // Attack → Idle: 공격 완료 + 정지
            stateMachine.AddTransition(
                PlayerStateType.Attack.ToString(),
                PlayerStateType.Idle.ToString(),
                PlayerEventType.AttackCompleted.ToString()
            );

            // Attack → Move: 공격 완료 + 이동
            // 조건부 전환은 상태 내부에서 처리

            // Attack → Fall: 공격 완료 + 공중
            // 조건부 전환은 상태 내부에서 처리
        }

        private static void SetupHitTransitions(StateMachine stateMachine)
        {
            // Hit → Idle: 피격 회복 + 정지
            stateMachine.AddTransition(
                PlayerStateType.Hit.ToString(),
                PlayerStateType.Idle.ToString(),
                PlayerEventType.RecoverFromHit.ToString()
            );

            // Hit → Move: 피격 회복 + 이동
            // 조건부 전환은 상태 내부에서 처리

            // Hit → Fall: 피격 회복 + 공중
            // 조건부 전환은 상태 내부에서 처리

            // Hit → Dead: 체력 0
            stateMachine.AddTransition(
                PlayerStateType.Hit.ToString(),
                PlayerStateType.Dead.ToString(),
                PlayerEventType.Die.ToString()
            );
        }

        private static void SetupDeadTransitions(StateMachine stateMachine)
        {
            // Dead → Idle: 리스폰
            stateMachine.AddTransition(
                PlayerStateType.Dead.ToString(),
                PlayerStateType.Idle.ToString(),
                PlayerEventType.Respawn.ToString()
            );
        }

        private static void SetupWallGrabTransitions(StateMachine stateMachine)
        {
            // WallGrab → Fall: 벽에서 떨어짐
            stateMachine.AddTransition(
                PlayerStateType.WallGrab.ToString(),
                PlayerStateType.Fall.ToString(),
                PlayerEventType.LeaveWall.ToString()
            );

            // WallGrab → WallJump: 벽 점프
            stateMachine.AddTransition(
                PlayerStateType.WallGrab.ToString(),
                PlayerStateType.WallJump.ToString(),
                PlayerEventType.WallJumpPressed.ToString()
            );

            // WallGrab → Idle: 땅에 착지
            stateMachine.AddTransition(
                PlayerStateType.WallGrab.ToString(),
                PlayerStateType.Idle.ToString(),
                PlayerEventType.TouchGround.ToString()
            );

            // WallGrab → Dash: 벽에서 대시
            stateMachine.AddTransition(
                PlayerStateType.WallGrab.ToString(),
                PlayerStateType.Dash.ToString(),
                PlayerEventType.DashPressed.ToString()
            );
        }

        private static void SetupWallJumpTransitions(StateMachine stateMachine)
        {
            // WallJump → Fall: 하강 시작
            // 조건부 전환은 상태 내부에서 처리

            // WallJump → Idle: 착지 + 정지
            stateMachine.AddTransition(
                PlayerStateType.WallJump.ToString(),
                PlayerStateType.Idle.ToString(),
                PlayerEventType.TouchGround.ToString()
            );

            // WallJump → Move: 착지 + 이동
            // 조건부 전환은 상태 내부에서 처리

            // WallJump → WallGrab: 다른 벽 접촉
            stateMachine.AddTransition(
                PlayerStateType.WallJump.ToString(),
                PlayerStateType.WallGrab.ToString(),
                PlayerEventType.TouchWall.ToString()
            );

            // WallJump → Dash: 공중 대시
            stateMachine.AddTransition(
                PlayerStateType.WallJump.ToString(),
                PlayerStateType.Dash.ToString(),
                PlayerEventType.DashPressed.ToString()
            );
        }

        private static void SetupSlideTransitions(StateMachine stateMachine)
        {
            // Slide → Idle: 슬라이딩 완료 + 정지
            stateMachine.AddTransition(
                PlayerStateType.Slide.ToString(),
                PlayerStateType.Idle.ToString(),
                PlayerEventType.SlideCompleted.ToString()
            );

            // Slide → Move: 슬라이딩 완료 + 이동
            // 조건부 전환은 상태 내부에서 처리

            // Slide → Jump: 슬라이딩 중 점프
            stateMachine.AddTransition(
                PlayerStateType.Slide.ToString(),
                PlayerStateType.Jump.ToString(),
                PlayerEventType.JumpPressed.ToString()
            );

            // Slide → Fall: 플랫폼에서 떨어짐
            stateMachine.AddTransition(
                PlayerStateType.Slide.ToString(),
                PlayerStateType.Fall.ToString(),
                PlayerEventType.LeaveGround.ToString()
            );
        }

        private static void SetupCommonTransitions(StateMachine stateMachine)
        {
            // 모든 상태에서 Hit 상태로 전환 가능 (데미지 받음)
            var allStates = new[]
            {
                PlayerStateType.Idle, PlayerStateType.Move, PlayerStateType.Jump,
                PlayerStateType.Fall, PlayerStateType.Dash, PlayerStateType.Attack,
                PlayerStateType.WallGrab, PlayerStateType.WallJump, PlayerStateType.Slide
            };

            foreach (var state in allStates)
            {
                stateMachine.AddTransition(
                    state.ToString(),
                    PlayerStateType.Hit.ToString(),
                    PlayerEventType.TakeDamage.ToString()
                );
            }

            // Hit 상태에서 Dead 상태로 전환 (체력 0)
            stateMachine.AddTransition(
                PlayerStateType.Hit.ToString(),
                PlayerStateType.Dead.ToString(),
                PlayerEventType.Die.ToString()
            );
        }
    }
}