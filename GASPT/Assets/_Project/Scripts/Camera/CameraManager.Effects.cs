using UnityEngine;

namespace GASPT.CameraSystem
{
    /// <summary>
    /// CameraManager - 효과 시스템
    /// Follow, Shake, Zoom 관련 기능
    /// </summary>
    public partial class CameraManager
    {
        // ====== Follow 시스템 ======

        /// <summary>
        /// 플레이어 타겟 자동 탐색 (비동기)
        /// </summary>
        private async Awaitable FindPlayerTargetAsync()
        {
            int maxAttempts = 50;
            int attempts = 0;

            while (followTarget == null && attempts < maxAttempts)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                {
                    followTarget = playerObj.transform;
                    if (showDebugLogs)
                        Debug.Log($"[CameraManager] 플레이어 타겟 탐색 완료: {followTarget.name}");

                    // 즉시 타겟 위치로 이동
                    SnapToTarget();
                    break;
                }

                await Awaitable.WaitForSecondsAsync(0.1f);
                attempts++;
            }

            if (followTarget == null)
            {
                Debug.LogWarning("[CameraManager] 플레이어를 찾을 수 없습니다! (타임아웃)");
            }
        }

        /// <summary>
        /// Follow 업데이트
        /// </summary>
        private void UpdateFollow()
        {
            if (followTarget == null) return;

            Vector3 targetPosition = followTarget.position + offset;

            // 선택적 축 추적
            Vector3 currentPos = mainCamera.transform.position;
            if (!followX) targetPosition.x = currentPos.x;
            if (!followY) targetPosition.y = currentPos.y;

            // Z축 고정
            targetPosition.z = offset.z;
        }

        /// <summary>
        /// 최종 카메라 위치 적용
        /// </summary>
        private void ApplyCameraPosition()
        {
            if (followTarget == null) return;

            Vector3 targetPosition = followTarget.position + offset;

            // 선택적 축 추적
            Vector3 currentPos = mainCamera.transform.position;
            if (!followX) targetPosition.x = currentPos.x;
            if (!followY) targetPosition.y = currentPos.y;

            // SmoothDamp 이동
            Vector3 smoothedPosition;
            if (smoothTime > 0f)
            {
                smoothedPosition = Vector3.SmoothDamp(currentPos, targetPosition, ref velocity, smoothTime);
            }
            else
            {
                smoothedPosition = targetPosition;
            }

            // 경계 제한 적용
            if (useBounds && currentBounds.IsValid)
            {
                Vector2 clamped = currentBounds.ClampCamera(
                    new Vector2(smoothedPosition.x, smoothedPosition.y),
                    mainCamera.orthographicSize,
                    mainCamera.aspect
                );
                smoothedPosition.x = clamped.x;
                smoothedPosition.y = clamped.y;
            }

            // Z축 고정
            smoothedPosition.z = offset.z;

            // Shake 오프셋 적용
            smoothedPosition += shakeOffset;

            // 최종 위치 적용
            mainCamera.transform.position = smoothedPosition;
        }


        // ====== Shake 시스템 ======

        /// <summary>
        /// Shake 업데이트
        /// </summary>
        private void UpdateShake()
        {
            if (shakeDuration <= 0f)
            {
                shakeOffset = Vector3.zero;
                return;
            }

            shakeTimer += Time.deltaTime;
            shakeDuration -= Time.deltaTime;

            // Perlin Noise 기반 Shake
            float offsetX = (Mathf.PerlinNoise(shakeTimer * shakeFrequency, 0f) - 0.5f) * 2f;
            float offsetY = (Mathf.PerlinNoise(0f, shakeTimer * shakeFrequency) - 0.5f) * 2f;

            // 페이드 아웃
            float fadeOut = shakeDuration / (shakeDuration + 0.1f);

            shakeOffset = new Vector3(offsetX, offsetY, 0f) * shakeIntensity * shakeMultiplier * fadeOut;
        }

        /// <summary>
        /// 카메라 Shake 실행
        /// </summary>
        /// <param name="intensity">강도 (0.1 ~ 1.0 권장)</param>
        /// <param name="duration">지속 시간</param>
        /// <param name="frequency">진동 빈도 (10 ~ 50 권장)</param>
        public void Shake(float intensity = 0.3f, float duration = 0.3f, float frequency = 25f)
        {
            // 더 강한 Shake가 이미 진행 중이면 무시
            if (shakeDuration > 0f && shakeIntensity > intensity)
                return;

            shakeIntensity = intensity;
            shakeDuration = duration;
            shakeFrequency = frequency;
            shakeTimer = Random.Range(0f, 100f); // 랜덤 시작점

            if (showDebugLogs)
                Debug.Log($"[CameraManager] Shake 시작: intensity={intensity}, duration={duration}");
        }

        /// <summary>
        /// Shake 즉시 중지
        /// </summary>
        public void StopShake()
        {
            shakeDuration = 0f;
            shakeOffset = Vector3.zero;
        }


        // ====== Zoom 시스템 ======

        /// <summary>
        /// Zoom 업데이트
        /// </summary>
        private void UpdateZoom()
        {
            if (mainCamera == null) return;

            float currentSize = mainCamera.orthographicSize;

            if (Mathf.Abs(currentSize - targetOrthographicSize) > 0.01f)
            {
                mainCamera.orthographicSize = Mathf.SmoothDamp(
                    currentSize,
                    targetOrthographicSize,
                    ref currentZoomVelocity,
                    zoomSmoothTime
                );
            }
        }

        /// <summary>
        /// Zoom 설정 (절대값)
        /// </summary>
        /// <param name="orthographicSize">Orthographic Size</param>
        /// <param name="instant">즉시 적용 여부</param>
        public void SetZoom(float orthographicSize, bool instant = false)
        {
            targetOrthographicSize = Mathf.Clamp(orthographicSize, minOrthographicSize, maxOrthographicSize);

            if (instant && mainCamera != null)
            {
                mainCamera.orthographicSize = targetOrthographicSize;
                currentZoomVelocity = 0f;
            }

            if (showDebugLogs)
                Debug.Log($"[CameraManager] Zoom 설정: {targetOrthographicSize}");
        }

        /// <summary>
        /// Zoom 배율 설정 (기본 크기 기준)
        /// </summary>
        /// <param name="multiplier">배율 (1.0 = 기본, 0.5 = 확대, 2.0 = 축소)</param>
        /// <param name="instant">즉시 적용</param>
        public void SetZoomMultiplier(float multiplier, bool instant = false)
        {
            SetZoom(defaultOrthographicSize * multiplier, instant);
        }

        /// <summary>
        /// 기본 Zoom으로 복귀
        /// </summary>
        public void ResetZoom(bool instant = false)
        {
            SetZoom(defaultOrthographicSize, instant);
        }
    }
}
