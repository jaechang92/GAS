using UnityEngine;
using System.Threading;

namespace Enemy
{
    /// <summary>
    /// Enemy Idle 상태
    /// 대기 전용 - 감지는 EnemyController가 처리
    /// </summary>
    public class EnemyIdleState : EnemyBaseState
    {
        public EnemyIdleState() : base(EnemyStateType.Idle) { }

        protected override void EnterStateSync()
        {
            LogStateDebug("Idle 상태 진입(동기) - 대기 중");

            // 이동 정지
            StopMovement();
        }

        protected override void ExitStateSync()
        {
            LogStateDebug("Idle 상태 종료(동기)");
        }

        protected override void UpdateState(float deltaTime)
        {
            // 대기 상태 - 상태 전환은 EnemyController.CheckPlayerDetection()에서 처리
            // UpdateState는 비워둠 (필요시 Idle 애니메이션 등 추가)
        }
    }
}
