using UnityEngine;
using System;
using System.Collections.Generic;

namespace Gameplay.Environment
{
    /// <summary>
    /// 일방향 낙하 플랫폼 컴포넌트
    /// 플레이어가 위에서만 착지하고, 아래 방향 + 점프 입력으로 통과 가능
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class OneWayPlatform : MonoBehaviour
    {
        [Header("플랫폼 설정")]
        [SerializeField] private PlatformType platformType = PlatformType.OneWay;
        [SerializeField] private float passthroughCooldown = 0.5f;
        [SerializeField] private bool enableDebugLogs = false;

        // 컴포넌트 참조
        private Collider2D platformCollider;

        // 충돌 무시 관리
        private readonly HashSet<Collider2D> ignoredColliders = new HashSet<Collider2D>();
        private readonly Dictionary<Collider2D, float> cooldownTimers = new Dictionary<Collider2D, float>();

        // 이벤트
        public event Action<Collider2D> OnPassthroughRequested;
        public event Action<Collider2D> OnPassthroughReset;

        // 프로퍼티
        public PlatformType Type => platformType;
        public float PassthroughCooldown => passthroughCooldown;

        #region Unity 생명주기

        private void Awake()
        {
            InitializeComponent();
        }

        private void FixedUpdate()
        {
            UpdateCooldowns(Time.fixedDeltaTime);
        }

        #endregion

        #region 초기화

        private void InitializeComponent()
        {
            platformCollider = GetComponent<Collider2D>();

            if (platformCollider == null)
            {
                Debug.LogError($"[OneWayPlatform] Collider2D를 찾을 수 없습니다! GameObject: {gameObject.name}");
            }

            LogDebug("일방향 플랫폼 초기화 완료");
        }

        #endregion

        #region 공개 API

        /// <summary>
        /// 플랫폼 통과 요청 (아래 방향 + 점프 입력)
        /// </summary>
        public void RequestPassthrough(Collider2D playerCollider)
        {
            if (playerCollider == null || platformCollider == null) return;

            // 이미 무시 중이면 스킵
            if (ignoredColliders.Contains(playerCollider))
            {
                LogDebug($"이미 통과 중: {playerCollider.gameObject.name}");
                return;
            }

            // 충돌 무시 시작
            Physics2D.IgnoreCollision(playerCollider, platformCollider, true);
            ignoredColliders.Add(playerCollider);
            cooldownTimers[playerCollider] = passthroughCooldown;

            OnPassthroughRequested?.Invoke(playerCollider);
            LogDebug($"플랫폼 통과 시작: {playerCollider.gameObject.name}");
        }

        /// <summary>
        /// 플랫폼 통과 리셋 (쿨다운 만료 시)
        /// </summary>
        public void ResetPassthrough(Collider2D playerCollider)
        {
            if (playerCollider == null || platformCollider == null) return;

            // 충돌 무시 해제
            Physics2D.IgnoreCollision(playerCollider, platformCollider, false);
            ignoredColliders.Remove(playerCollider);
            cooldownTimers.Remove(playerCollider);

            OnPassthroughReset?.Invoke(playerCollider);
            LogDebug($"플랫폼 통과 리셋: {playerCollider.gameObject.name}");
        }

        /// <summary>
        /// 플레이어가 플랫폼에 착지 가능한지 확인
        /// </summary>
        public bool CanLandOn(Collider2D playerCollider, Vector2 playerVelocity)
        {
            // 하강 중이고 충돌 무시 중이 아닐 때만 착지 가능
            return playerVelocity.y <= 0 && !IsIgnoringCollider(playerCollider);
        }

        /// <summary>
        /// 특정 Collider를 무시 중인지 확인
        /// </summary>
        public bool IsIgnoringCollider(Collider2D collider)
        {
            return ignoredColliders.Contains(collider);
        }

        #endregion

        #region 쿨다운 관리

        /// <summary>
        /// 쿨다운 타이머 업데이트 (FixedUpdate에서 호출)
        /// </summary>
        private void UpdateCooldowns(float deltaTime)
        {
            // 만료된 타이머 수집
            var expiredColliders = new List<Collider2D>();

            foreach (var kvp in cooldownTimers)
            {
                cooldownTimers[kvp.Key] -= deltaTime;

                if (cooldownTimers[kvp.Key] <= 0)
                {
                    expiredColliders.Add(kvp.Key);
                }
            }

            // 만료된 충돌 무시 해제
            foreach (var collider in expiredColliders)
            {
                ResetPassthrough(collider);
            }
        }

        #endregion

        #region 디버그

        private void LogDebug(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[OneWayPlatform] {message}");
            }
        }

        private void OnDrawGizmos()
        {
            if (platformCollider == null) return;

            // 플랫폼 영역 표시
            Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
            Gizmos.DrawCube(transform.position, platformCollider.bounds.size);

            // 위쪽 화살표 (착지 가능 방향)
            Gizmos.color = Color.green;
            Vector3 arrowStart = transform.position + Vector3.up * (platformCollider.bounds.extents.y + 0.2f);
            Vector3 arrowEnd = arrowStart + Vector3.down * 0.5f;
            Gizmos.DrawLine(arrowStart, arrowEnd);
            Gizmos.DrawLine(arrowEnd, arrowEnd + new Vector3(0.1f, 0.1f, 0f));
            Gizmos.DrawLine(arrowEnd, arrowEnd + new Vector3(-0.1f, 0.1f, 0f));
        }

        #endregion
    }
}
