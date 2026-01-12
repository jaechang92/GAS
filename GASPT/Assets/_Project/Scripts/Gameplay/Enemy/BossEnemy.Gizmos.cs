using UnityEngine;

namespace GASPT.Gameplay.Enemies
{
    /// <summary>
    /// BossEnemy 디버깅 시각화
    /// - OnDrawGizmos
    /// - Capsule/Circle 그리기 헬퍼
    /// </summary>
    public partial class BossEnemy
    {
        // ====== Gizmos ======

        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();

            if (!showGizmos || Data == null) return;

            // Phase 2 돌진 경로 시각화 (Phase 2 = 인덱스 1)
            if (phaseController != null && phaseController.CurrentPhaseIndex == 1)
            {
                DrawChargeGizmos();
            }

            // Phase 3 범위 공격 범위 (빨간색 원) (Phase 3 = 인덱스 2)
            if (phaseController != null && phaseController.CurrentPhaseIndex == 2)
            {
                Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
                Gizmos.DrawWireSphere(transform.position, Data.bossAreaRadius);
            }
        }

        /// <summary>
        /// 돌진 공격 Gizmos 그리기
        /// </summary>
        private void DrawChargeGizmos()
        {
            if (playerTransform == null || col == null) return;

            CapsuleCollider2D capsule = col as CapsuleCollider2D;
            if (capsule == null) return;

            // Collider 크기 (스케일 적용)
            Vector2 capsuleSize = capsule.size;
            Vector2 scaledSize = new Vector2(
                capsuleSize.x * Mathf.Abs(transform.localScale.x),
                capsuleSize.y * Mathf.Abs(transform.localScale.y)
            );

            // 돌진 가능 거리
            Vector2 horizontalDir = new Vector2(
                playerTransform.position.x - transform.position.x,
                0f
            ).normalized;

            Vector3 chargeEnd = transform.position + new Vector3(
                horizontalDir.x * Data.bossChargeDistance,
                0f,
                0f
            );

            // CapsuleCast 영역 시각화 (초록색 캡슐)
            Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
            DrawCapsuleGizmo(transform.position, scaledSize);
            DrawCapsuleGizmo(chargeEnd, scaledSize);

            // 돌진 경로 선 (초록색)
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, chargeEnd);

            // 돌진 중일 때 (빨간색)
            if (isCharging)
            {
                Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
                DrawCapsuleGizmo(transform.position, scaledSize);
                DrawCapsuleGizmo(chargeTargetPos, scaledSize);

                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, chargeTargetPos);
                Gizmos.DrawWireSphere(chargeTargetPos, 0.5f);
            }
        }

        /// <summary>
        /// Capsule Gizmo 그리기 (디버깅용)
        /// </summary>
        private void DrawCapsuleGizmo(Vector3 position, Vector2 size)
        {
            float radius = size.x * 0.5f;
            float height = size.y;

            // 상단 반원
            Vector3 topCenter = position + Vector3.up * (height * 0.5f - radius);
            DrawHalfCircle(topCenter, radius, true);

            // 하단 반원
            Vector3 bottomCenter = position + Vector3.down * (height * 0.5f - radius);
            DrawHalfCircle(bottomCenter, radius, false);

            // 좌우 세로선
            Gizmos.DrawLine(
                topCenter + Vector3.left * radius,
                bottomCenter + Vector3.left * radius
            );
            Gizmos.DrawLine(
                topCenter + Vector3.right * radius,
                bottomCenter + Vector3.right * radius
            );
        }

        /// <summary>
        /// 반원 그리기 (디버깅용)
        /// </summary>
        private void DrawHalfCircle(Vector3 center, float radius, bool top)
        {
            int segments = 16;
            float angleStart = top ? 0f : 180f;
            float angleEnd = top ? 180f : 360f;

            Vector3 prevPoint = center + new Vector3(
                Mathf.Cos(angleStart * Mathf.Deg2Rad) * radius,
                Mathf.Sin(angleStart * Mathf.Deg2Rad) * radius,
                0f
            );

            for (int i = 1; i <= segments; i++)
            {
                float angle = Mathf.Lerp(angleStart, angleEnd, i / (float)segments);
                Vector3 newPoint = center + new Vector3(
                    Mathf.Cos(angle * Mathf.Deg2Rad) * radius,
                    Mathf.Sin(angle * Mathf.Deg2Rad) * radius,
                    0f
                );

                Gizmos.DrawLine(prevPoint, newPoint);
                prevPoint = newPoint;
            }
        }
    }
}
