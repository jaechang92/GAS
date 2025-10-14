using UnityEngine;
using Core;

namespace Core.Managers
{
    /// <summary>
    /// 게임 전체 상태를 관리하는 매니저
    /// </summary>
    public class GameManager : SingletonManager<GameManager>
    {
        [Header("게임 설정")]
        [SerializeField] private int initialLives = 3;
        [SerializeField] private int currentLives;
        [SerializeField] private int currentScore;

        [Header("리소스 설정")]
        [SerializeField] private int currentGold = 0;
        [SerializeField] private int currentDiamond = 0;

        [Header("디버그")]
        [SerializeField] private bool showDebugLog = false;

        // 기본 프로퍼티
        public int CurrentLives => currentLives;
        public int CurrentScore => currentScore;
        public bool IsGameOver { get; private set; }

        // 리소스 프로퍼티
        public int CurrentGold => currentGold;
        public int CurrentDiamond => currentDiamond;

        // 기본 이벤트
        public event System.Action<int> OnScoreChanged;
        public event System.Action<int> OnLivesChanged;
        public event System.Action OnGameOver;
        public event System.Action OnGameStart;

        // 리소스 이벤트
        public event System.Action<int> OnGoldChanged;
        public event System.Action<int> OnDiamondChanged;

        protected override void OnSingletonAwake()
        {
            Log("게임 매니저 초기화");
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
            OnGoldChanged?.Invoke(currentGold);
            OnDiamondChanged?.Invoke(currentDiamond);

            Log($"게임 초기화 완료 - 생명: {currentLives}, 점수: {currentScore}");
        }

        /// <summary>
        /// 게임 시작
        /// </summary>
        public void StartGame()
        {
            if (!IsGameOver)
            {
                OnGameStart?.Invoke();
                Log("게임 시작");
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
            Log($"점수 추가: +{points}, 총점: {currentScore}");
        }

        /// <summary>
        /// 생명 감소
        /// </summary>
        public void LoseLife()
        {
            if (IsGameOver) return;

            currentLives = Mathf.Max(0, currentLives - 1);
            OnLivesChanged?.Invoke(currentLives);
            Log($"생명 감소, 남은 생명: {currentLives}");

            if (currentLives <= 0) TriggerGameOver();
        }

        /// <summary>
        /// 게임오버 처리
        /// </summary>
        private void TriggerGameOver()
        {
            IsGameOver = true;
            OnGameOver?.Invoke();
            Log("게임 오버!");
        }

        /// <summary>
        /// 게임 재시작
        /// </summary>
        public void RestartGame()
        {
            InitializeGame();
            Log("게임 재시작");
        }

        #region 리소스 관리

        /// <summary>
        /// 골드 추가
        /// </summary>
        public void AddGold(int amount)
        {
            if (amount <= 0) return;

            currentGold += amount;
            OnGoldChanged?.Invoke(currentGold);
        }

        /// <summary>
        /// 골드 사용 (차감)
        /// </summary>
        public bool SpendGold(int amount)
        {
            if (amount <= 0 || currentGold < amount) return false;

            currentGold -= amount;
            OnGoldChanged?.Invoke(currentGold);
            return true;
        }

        /// <summary>
        /// 골드 직접 설정
        /// </summary>
        public void SetGold(int amount)
        {
            currentGold = Mathf.Max(0, amount);
            OnGoldChanged?.Invoke(currentGold);
        }

        /// <summary>
        /// 다이아 추가
        /// </summary>
        public void AddDiamond(int amount)
        {
            if (amount <= 0) return;

            currentDiamond += amount;
            OnDiamondChanged?.Invoke(currentDiamond);
        }

        /// <summary>
        /// 다이아 사용 (차감)
        /// </summary>
        public bool SpendDiamond(int amount)
        {
            if (amount <= 0 || currentDiamond < amount) return false;

            currentDiamond -= amount;
            OnDiamondChanged?.Invoke(currentDiamond);
            return true;
        }

        /// <summary>
        /// 다이아 직접 설정
        /// </summary>
        public void SetDiamond(int amount)
        {
            currentDiamond = Mathf.Max(0, amount);
            OnDiamondChanged?.Invoke(currentDiamond);
        }

        /// <summary>
        /// 골드가 충분한지 확인
        /// </summary>
        public bool HasEnoughGold(int amount)
        {
            return currentGold >= amount;
        }

        /// <summary>
        /// 다이아가 충분한지 확인
        /// </summary>
        public bool HasEnoughDiamond(int amount)
        {
            return currentDiamond >= amount;
        }

        #endregion

        private void Log(string message)
        {
            if (showDebugLog) Debug.Log($"[GameManager] {message}");
        }
    }
}