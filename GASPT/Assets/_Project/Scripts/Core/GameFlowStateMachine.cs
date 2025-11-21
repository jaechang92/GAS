using UnityEngine;
using FSM.Core;
using GASPT.Core.GameFlow;

namespace GASPT.Core
{
    /// <summary>
    /// 게임 전체 Flow를 관리하는 FSM
    /// StartRoom → Dungeon → Reward → NextStage → ... → DungeonCleared → StartRoom
    /// </summary>
    public class GameFlowStateMachine : SingletonManager<GameFlowStateMachine>
    {
        [Header("디버그")]
        [SerializeField] private bool showDebugLogs = true;

        [Header("현재 상태 (읽기 전용)")]
        [SerializeField] private string currentStateDisplay = "None";

        // FSM 컴포넌트
        private StateMachine fsm;

        // 상태들
        private InitializingState initializingState;
        private StartRoomState startRoomState;
        private LoadingDungeonState loadingDungeonState;
        private DungeonCombatState dungeonCombatState;
        private DungeonRewardState dungeonRewardState;
        private DungeonTransitionState dungeonTransitionState;
        private DungeonClearedState dungeonClearedState;
        private LoadingStartRoomState loadingStartRoomState;
        private GameOverState gameOverState;


        // ====== 프로퍼티 ======

        /// <summary>
        /// 현재 게임 상태 ID
        /// </summary>
        public string CurrentStateId => fsm?.CurrentStateId ?? "None";

        /// <summary>
        /// FSM이 실행 중인지 여부
        /// </summary>
        public bool IsRunning => fsm?.IsRunning ?? false;


        // ====== 초기화 ======

        protected override void OnAwake()
        {
            SetupStateMachine();
        }

        private void SetupStateMachine()
        {
            // FSM 컴포넌트 추가
            fsm = gameObject.AddComponent<StateMachine>();

            // 상태 생성 및 추가
            CreateStates();

            // 전환 설정
            SetupTransitions();

            // 이벤트 구독
            fsm.OnStateChanged += OnStateChanged;

            LogMessage("[GameFlowFSM] 초기화 완료");
        }

        private void Update()
        {
            // Inspector 디버그 표시 업데이트
            #if UNITY_EDITOR
            currentStateDisplay = CurrentStateId;
            #endif
        }


        // ====== 상태 생성 ======

        private void CreateStates()
        {
            // 0. Initializing - 게임 최초 시작 (싱글톤 초기화)
            initializingState = new InitializingState();
            fsm.AddState(initializingState);
            LogMessage($"[GameFlowFSM] State 추가: {initializingState.Name}");

            // 1. StartRoom - 준비실 (정비, 상점, 업그레이드)
            startRoomState = new StartRoomState();
            fsm.AddState(startRoomState);
            LogMessage($"[GameFlowFSM] State 추가: {startRoomState.Name}");

            // 2. LoadingDungeon - 던전 씬 로딩
            loadingDungeonState = new LoadingDungeonState();
            fsm.AddState(loadingDungeonState);
            LogMessage($"[GameFlowFSM] State 추가: {loadingDungeonState.Name}");

            // 3. DungeonCombat - 전투 진행
            dungeonCombatState = new DungeonCombatState();
            fsm.AddState(dungeonCombatState);
            LogMessage($"[GameFlowFSM] State 추가: {dungeonCombatState.Name}");

            // 4. DungeonReward - 보상 선택
            dungeonRewardState = new DungeonRewardState();
            fsm.AddState(dungeonRewardState);
            LogMessage($"[GameFlowFSM] State 추가: {dungeonRewardState.Name}");

            // 5. DungeonTransition - 다음 방으로 전환
            dungeonTransitionState = new DungeonTransitionState();
            fsm.AddState(dungeonTransitionState);
            LogMessage($"[GameFlowFSM] State 추가: {dungeonTransitionState.Name}");

            // 6. DungeonCleared - 던전 클리어 (결산)
            dungeonClearedState = new DungeonClearedState();
            fsm.AddState(dungeonClearedState);
            LogMessage($"[GameFlowFSM] State 추가: {dungeonClearedState.Name}");

            // 7. LoadingStartRoom - 준비실 복귀 로딩
            loadingStartRoomState = new LoadingStartRoomState();
            fsm.AddState(loadingStartRoomState);
            LogMessage($"[GameFlowFSM] State 추가: {loadingStartRoomState.Name}");

            // 8. GameOver - 게임 오버
            gameOverState = new GameOverState();
            fsm.AddState(gameOverState);
            LogMessage($"[GameFlowFSM] State 추가: {gameOverState.Name}");

            LogMessage("[GameFlowFSM] 총 9개 상태 생성 완료");
        }


        // ====== 전환 설정 ======

        private void SetupTransitions()
        {
            // Initializing → LoadingStartRoom (초기화 완료)
            fsm.AddTransition("Initializing", "LoadingStartRoom", "InitializationComplete", priority: 10);

            // StartRoom → LoadingDungeon (던전 입장 포탈)
            fsm.AddTransition("StartRoom", "LoadingDungeon", "EnterDungeon", priority: 10);

            // LoadingDungeon → DungeonCombat (로딩 완료)
            fsm.AddTransition("LoadingDungeon", "DungeonCombat", "DungeonLoaded", priority: 10);

            // DungeonCombat → DungeonReward (적 전멸)
            fsm.AddTransition("DungeonCombat", "DungeonReward", "EnemiesCleared", priority: 10);

            // DungeonReward → DungeonTransition (보상 선택 완료 또는 포탈 입장)
            fsm.AddTransition("DungeonReward", "DungeonTransition", "EnterNextRoom", priority: 10);

            // DungeonTransition → DungeonCombat (다음 방 준비 완료)
            fsm.AddTransition("DungeonTransition", "DungeonCombat", "NextRoomReady", priority: 10);

            // DungeonTransition → DungeonCleared (마지막 방 클리어)
            fsm.AddTransition("DungeonTransition", "DungeonCleared", "DungeonComplete", priority: 20);

            // DungeonCleared → LoadingStartRoom (준비실 복귀)
            fsm.AddTransition("DungeonCleared", "LoadingStartRoom", "ReturnToStart", priority: 10);

            // LoadingStartRoom → StartRoom (로딩 완료)
            fsm.AddTransition("LoadingStartRoom", "StartRoom", "StartRoomLoaded", priority: 10);

            // Any State → GameOver (플레이어 사망)
            fsm.AddTransition("DungeonCombat", "GameOver", "PlayerDied", priority: 100);
            fsm.AddTransition("DungeonReward", "GameOver", "PlayerDied", priority: 100);
            fsm.AddTransition("DungeonTransition", "GameOver", "PlayerDied", priority: 100);

            // GameOver → StartRoom (재시작)
            fsm.AddTransition("GameOver", "StartRoom", "RestartGame", priority: 10);

            LogMessage("[GameFlowFSM] 전환 설정 완료");
        }


        // ====== 게임 시작 ======

        /// <summary>
        /// 게임 시작 (Initializing 상태로)
        /// </summary>
        public void StartGame()
        {
            if (IsRunning)
            {
                LogWarning("[GameFlowFSM] 이미 실행 중입니다.");
                return;
            }

            fsm.StartStateMachine("Initializing");
            LogMessage("[GameFlowFSM] 게임 시작 - Initializing 상태로 진입");
        }

        /// <summary>
        /// 게임 중지
        /// </summary>
        public void StopGame()
        {
            if (!IsRunning)
            {
                LogWarning("[GameFlowFSM] 실행 중이 아닙니다.");
                return;
            }

            fsm.Stop();
            LogMessage("[GameFlowFSM] 게임 중지");
        }


        // ====== 이벤트 트리거 (외부에서 호출) ======

        /// <summary>
        /// 초기화 완료 이벤트 (InitializingState에서 호출)
        /// </summary>
        public void TriggerInitializationComplete()
        {
            fsm.TriggerEvent("InitializationComplete");
            LogMessage("[GameFlowFSM] 초기화 완료 이벤트 발생");
        }

        /// <summary>
        /// 던전 입장 이벤트 (StartRoom 포탈에서 호출)
        /// </summary>
        public void TriggerEnterDungeon()
        {
            fsm.TriggerEvent("EnterDungeon");
            LogMessage("[GameFlowFSM] 던전 입장 이벤트 발생");
        }

        /// <summary>
        /// 적 전멸 이벤트 (RoomManager에서 호출)
        /// </summary>
        public void TriggerEnemiesCleared()
        {
            fsm.TriggerEvent("EnemiesCleared");
            LogMessage("[GameFlowFSM] 적 전멸 이벤트 발생");
        }

        /// <summary>
        /// 다음 방 입장 이벤트 (RoomPortal에서 호출)
        /// </summary>
        public void TriggerEnterNextRoom()
        {
            fsm.TriggerEvent("EnterNextRoom");
            LogMessage("[GameFlowFSM] 다음 방 입장 이벤트 발생");
        }

        /// <summary>
        /// 플레이어 사망 이벤트 (PlayerStats에서 호출)
        /// </summary>
        public void TriggerPlayerDied()
        {
            fsm.TriggerEvent("PlayerDied");
            LogMessage("[GameFlowFSM] 플레이어 사망 이벤트 발생");
        }

        /// <summary>
        /// 준비실 복귀 이벤트 (DungeonClearedState에서 호출)
        /// </summary>
        public void TriggerReturnToStart()
        {
            fsm.TriggerEvent("ReturnToStart");
            LogMessage("[GameFlowFSM] 준비실 복귀 이벤트 발생");
        }

        /// <summary>
        /// 게임 재시작 이벤트 (GameOverState에서 호출)
        /// </summary>
        public void TriggerRestartGame()
        {
            fsm.TriggerEvent("RestartGame");
            LogMessage("[GameFlowFSM] 게임 재시작 이벤트 발생");
        }


        // ====== 내부 이벤트 (State에서 자동 호출) ======

        /// <summary>
        /// 던전 로딩 완료 (LoadingDungeonState에서 호출)
        /// </summary>
        public void NotifyDungeonLoaded()
        {
            fsm.TriggerEvent("DungeonLoaded");
            LogMessage("[GameFlowFSM] 던전 로딩 완료");
        }

        /// <summary>
        /// 다음 방 준비 완료 (DungeonTransitionState에서 호출)
        /// </summary>
        public void NotifyNextRoomReady()
        {
            fsm.TriggerEvent("NextRoomReady");
            LogMessage("[GameFlowFSM] 다음 방 준비 완료");
        }

        /// <summary>
        /// 던전 완전 클리어 (DungeonTransitionState에서 호출)
        /// </summary>
        public void NotifyDungeonComplete()
        {
            fsm.TriggerEvent("DungeonComplete");
            LogMessage("[GameFlowFSM] 던전 완전 클리어");
        }

        /// <summary>
        /// 준비실 로딩 완료 (LoadingStartRoomState에서 호출)
        /// </summary>
        public void NotifyStartRoomLoaded()
        {
            fsm.TriggerEvent("StartRoomLoaded");
            LogMessage("[GameFlowFSM] 준비실 로딩 완료");
        }


        // ====== 이벤트 핸들러 ======

        private void OnStateChanged(string fromState, string toState)
        {
            LogMessage($"[GameFlowFSM] 상태 전환: {fromState} → {toState}");
        }


        // ====== 로깅 ======

        private void LogMessage(string message)
        {
            if (showDebugLogs)
            {
                Debug.Log(message);
            }
        }

        private void LogWarning(string message)
        {
            if (showDebugLogs)
            {
                Debug.LogWarning(message);
            }
        }


        // ====== Context Menu (테스트용) ======

        [ContextMenu("게임 시작")]
        private void TestStartGame()
        {
            StartGame();
        }

        [ContextMenu("게임 중지")]
        private void TestStopGame()
        {
            StopGame();
        }

        [ContextMenu("던전 입장")]
        private void TestEnterDungeon()
        {
            TriggerEnterDungeon();
        }

        [ContextMenu("적 전멸")]
        private void TestEnemiesCleared()
        {
            TriggerEnemiesCleared();
        }

        [ContextMenu("다음 방 입장")]
        private void TestEnterNextRoom()
        {
            TriggerEnterNextRoom();
        }

        [ContextMenu("플레이어 사망")]
        private void TestPlayerDied()
        {
            TriggerPlayerDied();
        }

        [ContextMenu("현재 상태 출력")]
        private void TestPrintCurrentState()
        {
            Debug.Log($"========== GameFlow State ==========");
            Debug.Log($"Current State: {CurrentStateId}");
            Debug.Log($"Is Running: {IsRunning}");
            Debug.Log($"====================================");
        }
    }
}
