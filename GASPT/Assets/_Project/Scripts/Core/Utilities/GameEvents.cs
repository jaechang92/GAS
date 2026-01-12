using System;
using UnityEngine;

namespace GASPT.Core.Utilities
{
    /// <summary>
    /// 중앙화된 게임 이벤트 시스템
    /// 글로벌 게임 이벤트만 정의 (컴포넌트 레벨 이벤트는 인터페이스 사용)
    /// </summary>
    public static class GameEvents
    {
        #region Player Events

        /// <summary>
        /// 플레이어 체력 변경 (현재, 최대)
        /// </summary>
        public static event Action<float, float> OnPlayerHealthChanged;

        /// <summary>
        /// 플레이어 위치 이동
        /// </summary>
        public static event Action<Vector3> OnPlayerMoved;

        /// <summary>
        /// 플레이어 사망
        /// </summary>
        public static event Action OnPlayerDeath;

        /// <summary>
        /// 플레이어 부활
        /// </summary>
        public static event Action OnPlayerRespawn;

        #endregion

        #region Combat Events

        /// <summary>
        /// 콤보 변경 (현재 콤보 수)
        /// </summary>
        public static event Action<int> OnComboChanged;

        /// <summary>
        /// 데미지 발생 (데미지 량, 대상 ID)
        /// </summary>
        public static event Action<int, int> OnDamageDealt;

        /// <summary>
        /// 적 처치 (처치된 적 ID)
        /// </summary>
        public static event Action<int> OnEnemyKilled;

        #endregion

        #region Game Events

        /// <summary>
        /// 점수 변경 (새로운 점수)
        /// </summary>
        public static event Action<int> OnScoreChanged;

        /// <summary>
        /// 적 수 변경 (현재 남은 적 수)
        /// </summary>
        public static event Action<int> OnEnemyCountChanged;

        /// <summary>
        /// 웨이브 시작 (웨이브 번호)
        /// </summary>
        public static event Action<int> OnWaveStarted;

        /// <summary>
        /// 웨이브 완료 (웨이브 번호)
        /// </summary>
        public static event Action<int> OnWaveCompleted;

        /// <summary>
        /// 게임 오버
        /// </summary>
        public static event Action OnGameOver;

        /// <summary>
        /// 게임 승리
        /// </summary>
        public static event Action OnGameWin;

        #endregion

        #region Invoke Methods

        // Player
        public static void InvokePlayerHealthChanged(float current, float max)
        {
            OnPlayerHealthChanged?.Invoke(current, max);
        }

        public static void InvokePlayerMoved(Vector3 position)
        {
            OnPlayerMoved?.Invoke(position);
        }

        public static void InvokePlayerDeath()
        {
            OnPlayerDeath?.Invoke();
        }

        public static void InvokePlayerRespawn()
        {
            OnPlayerRespawn?.Invoke();
        }

        // Combat
        public static void InvokeComboChanged(int comboCount)
        {
            OnComboChanged?.Invoke(comboCount);
        }

        public static void InvokeDamageDealt(int damage, int targetId)
        {
            OnDamageDealt?.Invoke(damage, targetId);
        }

        public static void InvokeEnemyKilled(int enemyId)
        {
            OnEnemyKilled?.Invoke(enemyId);
        }

        // Game
        public static void InvokeScoreChanged(int score)
        {
            OnScoreChanged?.Invoke(score);
        }

        public static void InvokeEnemyCountChanged(int count)
        {
            OnEnemyCountChanged?.Invoke(count);
        }

        public static void InvokeWaveStarted(int waveNumber)
        {
            OnWaveStarted?.Invoke(waveNumber);
        }

        public static void InvokeWaveCompleted(int waveNumber)
        {
            OnWaveCompleted?.Invoke(waveNumber);
        }

        public static void InvokeGameOver()
        {
            OnGameOver?.Invoke();
        }

        public static void InvokeGameWin()
        {
            OnGameWin?.Invoke();
        }

        #endregion

        #region Utility

        /// <summary>
        /// 모든 이벤트 구독 해제 (씬 전환 시 호출)
        /// </summary>
        public static void ClearAllEvents()
        {
            // Player
            OnPlayerHealthChanged = null;
            OnPlayerMoved = null;
            OnPlayerDeath = null;
            OnPlayerRespawn = null;

            // Combat
            OnComboChanged = null;
            OnDamageDealt = null;
            OnEnemyKilled = null;

            // Game
            OnScoreChanged = null;
            OnEnemyCountChanged = null;
            OnWaveStarted = null;
            OnWaveCompleted = null;
            OnGameOver = null;
            OnGameWin = null;

            Debug.Log("[GameEvents] 모든 이벤트 구독 해제 완료");
        }

        /// <summary>
        /// 현재 구독자 수 출력 (디버그용)
        /// </summary>
        public static void LogSubscriberCounts()
        {
            Debug.Log("=== GameEvents Subscriber Counts ===");
            Debug.Log($"OnPlayerHealthChanged: {OnPlayerHealthChanged?.GetInvocationList().Length ?? 0}");
            Debug.Log($"OnComboChanged: {OnComboChanged?.GetInvocationList().Length ?? 0}");
            Debug.Log($"OnScoreChanged: {OnScoreChanged?.GetInvocationList().Length ?? 0}");
            Debug.Log($"OnEnemyCountChanged: {OnEnemyCountChanged?.GetInvocationList().Length ?? 0}");
            Debug.Log("====================================");
        }

        #endregion
    }
}
