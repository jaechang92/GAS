using UnityEngine;

namespace GASPT.CameraSystem
{
    /// <summary>
    /// 카메라 경계 구조체
    /// 카메라가 이동할 수 있는 영역을 정의
    /// </summary>
    [System.Serializable]
    public struct CameraBounds
    {
        [Tooltip("최소 경계 (왼쪽 아래)")]
        public Vector2 min;

        [Tooltip("최대 경계 (오른쪽 위)")]
        public Vector2 max;

        // ====== 생성자 ======

        public CameraBounds(Vector2 min, Vector2 max)
        {
            this.min = min;
            this.max = max;
        }

        public CameraBounds(float minX, float minY, float maxX, float maxY)
        {
            this.min = new Vector2(minX, minY);
            this.max = new Vector2(maxX, maxY);
        }

        // ====== 프로퍼티 ======

        /// <summary>
        /// 경계 크기
        /// </summary>
        public Vector2 Size => max - min;

        /// <summary>
        /// 경계 중심점
        /// </summary>
        public Vector2 Center => (min + max) / 2f;

        /// <summary>
        /// 경계 너비
        /// </summary>
        public float Width => max.x - min.x;

        /// <summary>
        /// 경계 높이
        /// </summary>
        public float Height => max.y - min.y;

        // ====== 메서드 ======

        /// <summary>
        /// 점이 경계 안에 있는지 확인
        /// </summary>
        public bool Contains(Vector2 point)
        {
            return point.x >= min.x && point.x <= max.x &&
                   point.y >= min.y && point.y <= max.y;
        }

        /// <summary>
        /// 점을 경계 안으로 클램프
        /// </summary>
        public Vector2 Clamp(Vector2 point)
        {
            return new Vector2(
                Mathf.Clamp(point.x, min.x, max.x),
                Mathf.Clamp(point.y, min.y, max.y)
            );
        }

        /// <summary>
        /// 카메라 크기를 고려한 클램프 (카메라가 경계 밖을 비추지 않도록)
        /// </summary>
        /// <param name="position">카메라 위치</param>
        /// <param name="cameraHalfHeight">카메라 orthographicSize</param>
        /// <param name="cameraAspect">카메라 aspect ratio</param>
        public Vector2 ClampCamera(Vector2 position, float cameraHalfHeight, float cameraAspect)
        {
            float cameraHalfWidth = cameraHalfHeight * cameraAspect;

            // 카메라 뷰 영역이 경계보다 클 경우 중앙에 배치
            float clampedX, clampedY;

            if (Width <= cameraHalfWidth * 2f)
            {
                clampedX = Center.x;
            }
            else
            {
                clampedX = Mathf.Clamp(position.x, min.x + cameraHalfWidth, max.x - cameraHalfWidth);
            }

            if (Height <= cameraHalfHeight * 2f)
            {
                clampedY = Center.y;
            }
            else
            {
                clampedY = Mathf.Clamp(position.y, min.y + cameraHalfHeight, max.y - cameraHalfHeight);
            }

            return new Vector2(clampedX, clampedY);
        }

        /// <summary>
        /// Bounds를 확장
        /// </summary>
        public CameraBounds Expand(float amount)
        {
            return new CameraBounds(
                min - Vector2.one * amount,
                max + Vector2.one * amount
            );
        }

        /// <summary>
        /// 유효한 경계인지 확인
        /// </summary>
        public bool IsValid => min.x < max.x && min.y < max.y;

        // ====== 정적 메서드 ======

        /// <summary>
        /// 기본 경계 (매우 큰 범위)
        /// </summary>
        public static CameraBounds Default => new CameraBounds(-1000f, -1000f, 1000f, 1000f);

        /// <summary>
        /// 무한 경계 (제한 없음)
        /// </summary>
        public static CameraBounds Infinite => new CameraBounds(
            float.NegativeInfinity, float.NegativeInfinity,
            float.PositiveInfinity, float.PositiveInfinity
        );

        // ====== 연산자 ======

        public override string ToString()
        {
            return $"CameraBounds(Min: {min}, Max: {max})";
        }

        public override bool Equals(object obj)
        {
            if (obj is CameraBounds other)
            {
                return min == other.min && max == other.max;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return min.GetHashCode() ^ max.GetHashCode();
        }

        public static bool operator ==(CameraBounds a, CameraBounds b)
        {
            return a.min == b.min && a.max == b.max;
        }

        public static bool operator !=(CameraBounds a, CameraBounds b)
        {
            return !(a == b);
        }
    }
}
