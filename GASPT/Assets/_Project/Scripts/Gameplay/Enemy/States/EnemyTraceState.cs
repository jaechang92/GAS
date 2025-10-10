using UnityEngine;
using System.Threading;

namespace Enemy
{
    /// <summary>
    /// Enemy Trace 상태 (구 Chase)
    /// 플레이어 추적 전용 - 감지는 EnemyController가 처리
    /// </summary>
    public class EnemyTraceState : EnemyBaseState
    {
        public EnemyTraceState() : base(EnemyStateType.Trace) { }

        protected override void EnterStateSync()
        {
            LogStateDebug("Trace 상태 진입(동기) - 플레이어 추적 시작");
        }

        protected override void ExitStateSync()
        {
            LogStateDebug("Trace 상태 종료(동기)");
            StopMovement();
        }

        protected override void UpdateState(float deltaTime)
        {
            if (enemy == null || enemy.Data == null || enemy.Target == null) return;

            // 타겟 추적 (상태 전환은 EnemyController가 처리)
            MoveToTarget(enemy.Data.moveSpeed);
            enemy.UpdateSpriteDirection();
        }
    }
}
