using UnityEngine;
using Core;

namespace Managers
{
    /// <summary>
    /// 게임 전체 상태를 관리하는 매니저
    /// 최소 기능: 점수, 생명, 게임오버 상태 관리
    /// </summary>
    public class GameManager : SingletonManager<GameManager>
    {
        [Header("게임 설정")]
        [SerializeField] private int initialLives = 3;
        [SerializeField] private int currentLives;
        [SerializeField] private int currentScore;

        // 기본 프로퍼티
        public int CurrentLives => currentLives;
        public int CurrentScore => currentScore;
        public bool IsGameOver { get; private set; }

        // 기본 이벤트
        public event System.Action<int> OnScoreChanged;
        public event System.Action<int> OnLivesChanged;
        public event System.Action OnGameOver;
        public event System.Action OnGameStart;

        protected override void OnSingletonAwake()
        {
            Debug.Log("[GameManager] 게임 매니저 초기화");
            InitializeGame();
        }

        /// <summary>
        /// 게임 초기화
        /// </summary>
        public void InitializeGame()
        {
            currentLives = initialLives;
            currentScore = 0;
            IsGameOver = false;

            OnLivesChanged?.Invoke(currentLives);
            OnScoreChanged?.Invoke(currentScore);

            Debug.Log($"[GameManager] 게임 초기화 완료 - 생명: {currentLives}, 점수: {currentScore}");
        }

        /// <summary>
        /// 게임 시작
        /// </summary>
        public void StartGame()
        {
            if (!IsGameOver)
            {
                OnGameStart?.Invoke();
                Debug.Log("[GameManager] 게임 시작");
            }
        }

        /// <summary>
        /// 점수 추가
        /// </summary>
        public void AddScore(int points)
        {
            if (IsGameOver) return;

            currentScore += Mathf.Max(0, points);
            OnScoreChanged?.Invoke(currentScore);
            Debug.Log($"[GameManager] 점수 추가: +{points}, 총점: {currentScore}");
        }

        /// <summary>
        /// 생명 감소
        /// </summary>
        public void LoseLife()
        {
            if (IsGameOver) return;

            currentLives = Mathf.Max(0, currentLives - 1);
            OnLivesChanged?.Invoke(currentLives);
            Debug.Log($"[GameManager] 생명 감소, 남은 생명: {currentLives}");

            if (currentLives <= 0)
            {
                TriggerGameOver();
            }
        }

        /// <summary>
        /// 게임오버 처리
        /// </summary>
        private void TriggerGameOver()
        {
            IsGameOver = true;
            OnGameOver?.Invoke();
            Debug.Log("[GameManager] 게임 오버!");
        }

        /// <summary>
        /// 게임 재시작
        /// </summary>
        public void RestartGame()
        {
            InitializeGame();
            Debug.Log("[GameManager] 게임 재시작");
        }

        // TODO: 차후 구현 예정
        // - 플레이어 데이터 저장/로드
        // - 업적 시스템
        // - 리더보드 연동
        // - 게임 설정 관리
        // - 치트 코드 시스템
    }
}