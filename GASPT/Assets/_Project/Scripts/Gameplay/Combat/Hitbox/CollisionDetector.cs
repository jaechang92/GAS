using UnityEngine;
using System.Collections.Generic;

namespace Combat.Hitbox
{
    /// <summary>
    /// 충돌 감지 유틸리티
    /// 수동으로 충돌을 체크하고 처리하는 헬퍼 클래스
    /// </summary>
    public static class CollisionDetector
    {
        /// <summary>
        /// 원형 범위 내 허트박스 감지
        /// </summary>
        public static List<HurtboxController> DetectHurtboxesInRadius(Vector3 center, float radius, LayerMask targetLayers)
        {
            var hurtboxes = new List<HurtboxController>();
            var colliders = Physics2D.OverlapCircleAll(center, radius, targetLayers);

            foreach (var col in colliders)
            {
                var hurtbox = col.GetComponent<HurtboxController>();
                if (hurtbox != null && hurtbox.CanBeHit)
                {
                    hurtboxes.Add(hurtbox);
                }
            }

            return hurtboxes;
        }

        /// <summary>
        /// 박스 범위 내 허트박스 감지
        /// </summary>
        public static List<HurtboxController> DetectHurtboxesInBox(Vector3 center, Vector2 size, float angle, LayerMask targetLayers)
        {
            var hurtboxes = new List<HurtboxController>();
            var colliders = Physics2D.OverlapBoxAll(center, size, angle, targetLayers);

            foreach (var col in colliders)
            {
                var hurtbox = col.GetComponent<HurtboxController>();
                if (hurtbox != null && hurtbox.CanBeHit)
                {
                    hurtboxes.Add(hurtbox);
                }
            }

            return hurtboxes;
        }

        /// <summary>
        /// 캡슐 범위 내 허트박스 감지
        /// </summary>
        public static List<HurtboxController> DetectHurtboxesInCapsule(Vector2 point1, Vector2 point2, float radius, LayerMask targetLayers)
        {
            var hurtboxes = new List<HurtboxController>();
            var colliders = Physics2D.OverlapCapsuleAll(point1, new Vector2(Vector2.Distance(point1, point2), radius * 2), CapsuleDirection2D.Horizontal, 0f, targetLayers);

            foreach (var col in colliders)
            {
                var hurtbox = col.GetComponent<HurtboxController>();
                if (hurtbox != null && hurtbox.CanBeHit)
                {
                    hurtboxes.Add(hurtbox);
                }
            }

            return hurtboxes;
        }

        /// <summary>
        /// 레이캐스트로 허트박스 감지
        /// </summary>
        public static HurtboxController RaycastHurtbox(Vector2 origin, Vector2 direction, float distance, LayerMask targetLayers)
        {
            var hit = Physics2D.Raycast(origin, direction, distance, targetLayers);

            if (hit.collider != null)
            {
                var hurtbox = hit.collider.GetComponent<HurtboxController>();
                if (hurtbox != null && hurtbox.CanBeHit)
                {
                    return hurtbox;
                }
            }

            return null;
        }

        /// <summary>
        /// 레이캐스트로 모든 허트박스 감지
        /// </summary>
        public static List<HurtboxController> RaycastAllHurtboxes(Vector2 origin, Vector2 direction, float distance, LayerMask targetLayers)
        {
            var hurtboxes = new List<HurtboxController>();
            var hits = Physics2D.RaycastAll(origin, direction, distance, targetLayers);

            foreach (var hit in hits)
            {
                var hurtbox = hit.collider.GetComponent<HurtboxController>();
                if (hurtbox != null && hurtbox.CanBeHit)
                {
                    hurtboxes.Add(hurtbox);
                }
            }

            return hurtboxes;
        }

        /// <summary>
        /// 가장 가까운 허트박스 찾기
        /// </summary>
        public static HurtboxController FindNearestHurtbox(Vector3 position, float searchRadius, LayerMask targetLayers)
        {
            var hurtboxes = DetectHurtboxesInRadius(position, searchRadius, targetLayers);

            if (hurtboxes.Count == 0)
                return null;

            HurtboxController nearest = null;
            float nearestDistance = float.MaxValue;

            foreach (var hurtbox in hurtboxes)
            {
                float distance = Vector3.Distance(position, hurtbox.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = hurtbox;
                }
            }

            return nearest;
        }

        /// <summary>
        /// 특정 GameObject에 속한 허트박스 필터링
        /// </summary>
        public static List<HurtboxController> FilterByOwner(List<HurtboxController> hurtboxes, GameObject owner, bool exclude = true)
        {
            var filtered = new List<HurtboxController>();

            foreach (var hurtbox in hurtboxes)
            {
                bool isOwner = hurtbox.transform.IsChildOf(owner.transform) || hurtbox.gameObject == owner;

                if (exclude && !isOwner)
                {
                    filtered.Add(hurtbox);
                }
                else if (!exclude && isOwner)
                {
                    filtered.Add(hurtbox);
                }
            }

            return filtered;
        }

        /// <summary>
        /// 디버그 시각화 - 원형
        /// </summary>
        public static void DrawCircleGizmo(Vector3 center, float radius, Color color, float duration = 0f)
        {
            if (duration > 0f)
            {
                Debug.DrawLine(center + Vector3.up * radius, center + Vector3.right * radius, color, duration);
                Debug.DrawLine(center + Vector3.right * radius, center + Vector3.down * radius, color, duration);
                Debug.DrawLine(center + Vector3.down * radius, center + Vector3.left * radius, color, duration);
                Debug.DrawLine(center + Vector3.left * radius, center + Vector3.up * radius, color, duration);
            }
        }

        /// <summary>
        /// 디버그 시각화 - 박스
        /// </summary>
        public static void DrawBoxGizmo(Vector3 center, Vector2 size, Color color, float duration = 0f)
        {
            Vector3 halfSize = size * 0.5f;

            Vector3 topLeft = center + new Vector3(-halfSize.x, halfSize.y, 0);
            Vector3 topRight = center + new Vector3(halfSize.x, halfSize.y, 0);
            Vector3 bottomRight = center + new Vector3(halfSize.x, -halfSize.y, 0);
            Vector3 bottomLeft = center + new Vector3(-halfSize.x, -halfSize.y, 0);

            if (duration > 0f)
            {
                Debug.DrawLine(topLeft, topRight, color, duration);
                Debug.DrawLine(topRight, bottomRight, color, duration);
                Debug.DrawLine(bottomRight, bottomLeft, color, duration);
                Debug.DrawLine(bottomLeft, topLeft, color, duration);
            }
        }
    }
}
