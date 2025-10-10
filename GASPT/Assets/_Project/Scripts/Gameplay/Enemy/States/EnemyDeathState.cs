using UnityEngine;
using System.Threading;

namespace Enemy
{
    /// <summary>
    /// Enemy Death 상태
    /// 사망 처리 및 GameObject 파괴
    /// </summary>
    public class EnemyDeathState : EnemyBaseState
    {
        private float deathTime = 0f;
        private const float DeathDuration = 1.0f; // 사망 애니메이션 시간
        private SpriteRenderer spriteRenderer;
        private Color startColor;

        public EnemyDeathState() : base(EnemyStateType.Death) { }

        protected override void EnterStateSync()
        {
            LogStateDebug("Death 상태 진입(동기)");
            deathTime = 0f;

            // 이동 정지
            StopMovement();

            // Collider 비활성화 (더 이상 충돌 안함)
            if (enemy.Collider != null)
            {
                enemy.Collider.enabled = false;
            }

            // Rigidbody 비활성화
            if (enemy.Rigidbody != null)
            {
                enemy.Rigidbody.simulated = false;
            }

            // 사망 애니메이션 트리거 (애니메이터 있으면)
            // TODO: 애니메이션 추가 시 구현

            // 페이드 효과 준비
            spriteRenderer = enemy.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                startColor = spriteRenderer.color;
            }
        }

        protected override void ExitStateSync()
        {
            LogStateDebug("Death 상태 종료(동기)");
        }

        protected override void UpdateState(float deltaTime)
        {
            if (enemy == null) return;

            deathTime += deltaTime;

            // 페이드 아웃 효과
            if (spriteRenderer != null && deathTime < DeathDuration)
            {
                float t = deathTime / DeathDuration;
                Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);
                spriteRenderer.color = Color.Lerp(startColor, endColor, t);
            }

            // 사망 애니메이션 종료 후 GameObject 파괴
            if (deathTime >= DeathDuration)
            {
                LogStateDebug("Enemy GameObject 파괴");
                Object.Destroy(enemy.gameObject);
            }
        }
    }
}
