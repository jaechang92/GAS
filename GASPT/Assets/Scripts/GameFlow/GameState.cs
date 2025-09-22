using System.Threading;
using UnityEngine;
using FSM.Core;

namespace GameFlow
{
    /// <summary>
    /// 게임 상태 기본 클래스
    /// </summary>
    public abstract class GameState : IState
    {
        protected GameStateType stateType;
        protected GameFlowManager gameFlowManager;

        public string Id { get; private set; }
        public string Name { get; private set; }
        public bool IsActive { get; private set; }
        public GameObject Owner { get; private set; }
        public IStateMachine StateMachine { get; private set; }

        // 이벤트
        public event System.Action<IState> OnEntered;
        public event System.Action<IState> OnExited;

        protected GameState(GameStateType type)
        {
            stateType = type;
            Id = type.ToString();
        }

        public virtual void Initialize(string id, GameObject owner, IStateMachine stateMachine)
        {
            Id = id;
            Name = stateType.ToString();
            Owner = owner;
            StateMachine = stateMachine;
            gameFlowManager = owner.GetComponent<GameFlowManager>();
        }

        public virtual async Awaitable OnEnter(CancellationToken cancellationToken = default)
        {
            Debug.Log($"[GameFlow] Entering {stateType} state");
            IsActive = true;
            await EnterState(cancellationToken);
            OnEntered?.Invoke(this);
        }

        public virtual async Awaitable OnExit(CancellationToken cancellationToken = default)
        {
            Debug.Log($"[GameFlow] Exiting {stateType} state");
            IsActive = false;
            await ExitState(cancellationToken);
            OnExited?.Invoke(this);
        }

        public virtual void OnUpdate(float deltaTime)
        {
            UpdateState(deltaTime);
        }

        // 하위 클래스에서 구현할 추상 메서드들
        protected abstract Awaitable EnterState(CancellationToken cancellationToken);
        protected abstract Awaitable ExitState(CancellationToken cancellationToken);
        protected abstract void UpdateState(float deltaTime);
    }

    /// <summary>
    /// 메인 메뉴 상태
    /// </summary>
    public class MainState : GameState
    {
        public MainState() : base(GameStateType.Main) { }

        protected override async Awaitable EnterState(CancellationToken cancellationToken)
        {
            // 메인 메뉴 UI 활성화
            gameFlowManager?.ShowMainMenu();
            await Awaitable.NextFrameAsync();
        }

        protected override async Awaitable ExitState(CancellationToken cancellationToken)
        {
            // 메인 메뉴 UI 비활성화
            gameFlowManager?.HideMainMenu();
            await Awaitable.NextFrameAsync();
        }

        protected override void UpdateState(float deltaTime)
        {
            // 메인 메뉴에서의 업데이트 로직
        }
    }

    /// <summary>
    /// 로딩 상태
    /// </summary>
    public class LoadingState : GameState
    {
        private float loadingProgress = 0f;
        private bool isLoadingComplete = false;

        public LoadingState() : base(GameStateType.Loading) { }

        protected override async Awaitable EnterState(CancellationToken cancellationToken)
        {
            // 로딩 화면 활성화
            gameFlowManager?.ShowLoadingScreen();
            loadingProgress = 0f;
            isLoadingComplete = false;

            // 가상 로딩 시작
            await StartLoading(cancellationToken);
        }

        protected override async Awaitable ExitState(CancellationToken cancellationToken)
        {
            // 로딩 화면 비활성화
            gameFlowManager?.HideLoadingScreen();
            await Awaitable.NextFrameAsync();
        }

        protected override void UpdateState(float deltaTime)
        {
            // 로딩 진행률 업데이트
            if (!isLoadingComplete)
            {
                gameFlowManager?.UpdateLoadingProgress(loadingProgress);
            }
        }

        private async Awaitable StartLoading(CancellationToken cancellationToken)
        {
            // 가상의 로딩 프로세스
            for (int i = 0; i <= 100; i++)
            {
                if (cancellationToken.IsCancellationRequested) return;

                loadingProgress = i / 100f;
                await Awaitable.WaitForSecondsAsync(0.05f);
            }

            isLoadingComplete = true;

            // 로딩 완료 후 자동으로 인게임으로 전환
            await Awaitable.WaitForSecondsAsync(0.5f);
            gameFlowManager?.TriggerEvent(GameEventType.LoadComplete);
        }
    }

    /// <summary>
    /// 인게임 상태
    /// </summary>
    public class IngameState : GameState
    {
        public IngameState() : base(GameStateType.Ingame) { }

        protected override async Awaitable EnterState(CancellationToken cancellationToken)
        {
            // 인게임 UI 활성화
            gameFlowManager?.ShowIngameUI();

            // 게임 시간 복구
            Time.timeScale = 1f;

            await Awaitable.NextFrameAsync();
        }

        protected override async Awaitable ExitState(CancellationToken cancellationToken)
        {
            // 인게임 UI 처리
            await Awaitable.NextFrameAsync();
        }

        protected override void UpdateState(float deltaTime)
        {
            // 인게임 업데이트 로직
            // ESC 키로 일시정지
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                gameFlowManager?.TriggerEvent(GameEventType.PauseGame);
            }
        }
    }

    /// <summary>
    /// 일시정지 상태
    /// </summary>
    public class PauseState : GameState
    {
        public PauseState() : base(GameStateType.Pause) { }

        protected override async Awaitable EnterState(CancellationToken cancellationToken)
        {
            // 게임 시간 정지
            Time.timeScale = 0f;

            // 일시정지 UI 활성화
            gameFlowManager?.ShowPauseMenu();

            await Awaitable.NextFrameAsync();
        }

        protected override async Awaitable ExitState(CancellationToken cancellationToken)
        {
            // 일시정지 UI 비활성화
            gameFlowManager?.HidePauseMenu();

            // 게임 시간 복구
            Time.timeScale = 1f;

            await Awaitable.NextFrameAsync();
        }

        protected override void UpdateState(float deltaTime)
        {
            // ESC 키로 게임 복구
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                gameFlowManager?.TriggerEvent(GameEventType.ResumeGame);
            }
        }
    }

    /// <summary>
    /// 메뉴 상태
    /// </summary>
    public class MenuState : GameState
    {
        public MenuState() : base(GameStateType.Menu) { }

        protected override async Awaitable EnterState(CancellationToken cancellationToken)
        {
            // 메뉴 UI 활성화
            gameFlowManager?.ShowInGameMenu();
            await Awaitable.NextFrameAsync();
        }

        protected override async Awaitable ExitState(CancellationToken cancellationToken)
        {
            // 메뉴 UI 비활성화
            gameFlowManager?.HideInGameMenu();
            await Awaitable.NextFrameAsync();
        }

        protected override void UpdateState(float deltaTime)
        {
            // 메뉴 업데이트 로직
        }
    }

    /// <summary>
    /// 로비 상태
    /// </summary>
    public class LobbyState : GameState
    {
        public LobbyState() : base(GameStateType.Lobby) { }

        protected override async Awaitable EnterState(CancellationToken cancellationToken)
        {
            // 로비 UI 활성화
            gameFlowManager?.ShowLobby();
            await Awaitable.NextFrameAsync();
        }

        protected override async Awaitable ExitState(CancellationToken cancellationToken)
        {
            // 로비 UI 비활성화
            gameFlowManager?.HideLobby();
            await Awaitable.NextFrameAsync();
        }

        protected override void UpdateState(float deltaTime)
        {
            // 로비 업데이트 로직
        }
    }
}
