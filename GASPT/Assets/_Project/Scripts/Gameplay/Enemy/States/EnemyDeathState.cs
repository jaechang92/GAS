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

        public EnemyDeathState() : base(EnemyStateType.Death) { }

        protected override async Awaitable EnterState(CancellationToken cancellationToken)
        {
            LogStateDebug("Death 상태 진입");
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

            // 색상 변경 (페이드 효과)
            var spriteRenderer = enemy.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                FadeOut(spriteRenderer, DeathDuration, cancellationToken);
            }

            await Awaitable.NextFrameAsync(cancellationToken);
        }

        protected override async Awaitable ExitState(CancellationToken cancellationToken)
        {
            LogStateDebug("Death 상태 종료");
            await Awaitable.NextFrameAsync(cancellationToken);
        }

        protected override void UpdateState(float deltaTime)
        {
            if (enemy == null) return;

            deathTime += deltaTime;

            // 사망 애니메이션 종료 후 GameObject 파괴
            if (deathTime >= DeathDuration)
            {
                LogStateDebug("Enemy GameObject 파괴");
                Object.Destroy(enemy.gameObject);
            }
        }

        /// <summary>
        /// 페이드 아웃 효과
        /// </summary>
        private async void FadeOut(SpriteRenderer spriteRenderer, float duration, CancellationToken cancellationToken)
        {
            if (spriteRenderer == null) return;

            Color startColor = spriteRenderer.color;
            Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);

            float elapsed = 0f;

            try
            {
                while (elapsed < duration)
                {
                    if (cancellationToken.IsCancellationRequested || spriteRenderer == null) return;

                    elapsed += Time.deltaTime;
                    float t = elapsed / duration;
                    spriteRenderer.color = Color.Lerp(startColor, endColor, t);

                    await Awaitable.NextFrameAsync(cancellationToken);
                }
            }
            catch (System.OperationCanceledException)
            {
                // 취소되면 종료
            }
        }
    }
}
