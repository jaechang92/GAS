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
            // ���� �޴� UI Ȱ��ȭ
            gameFlowManager?.ShowMainMenu();
            await Awaitable.NextFrameAsync();
        }

        protected override async Awaitable ExitState(CancellationToken cancellationToken)
        {
            // ���� �޴� UI ��Ȱ��ȭ
            gameFlowManager?.HideMainMenu();
            await Awaitable.NextFrameAsync();
        }

        protected override void UpdateState(float deltaTime)
        {
            // ���� �޴������� ������Ʈ ����
        }
    }

    /// <summary>
    /// �ε� ����
    /// </summary>
    public class LoadingState : GameState
    {
        private float loadingProgress = 0f;
        private bool isLoadingComplete = false;

        public LoadingState() : base(GameStateType.Loading) { }

        protected override async Awaitable EnterState(CancellationToken cancellationToken)
        {
            // �ε� ȭ�� Ȱ��ȭ
            gameFlowManager?.ShowLoadingScreen();
            loadingProgress = 0f;
            isLoadingComplete = false;

            // ���� �ε� ����
            await StartLoading(cancellationToken);
        }

        protected override async Awaitable ExitState(CancellationToken cancellationToken)
        {
            // �ε� ȭ�� ��Ȱ��ȭ
            gameFlowManager?.HideLoadingScreen();
            await Awaitable.NextFrameAsync();
        }

        protected override void UpdateState(float deltaTime)
        {
            // �ε� ����� ������Ʈ
            if (!isLoadingComplete)
            {
                gameFlowManager?.UpdateLoadingProgress(loadingProgress);
            }
        }

        private async Awaitable StartLoading(CancellationToken cancellationToken)
        {
            // ������ �ε� ���μ���
            for (int i = 0; i <= 100; i++)
            {
                if (cancellationToken.IsCancellationRequested) return;

                loadingProgress = i / 100f;
                await Awaitable.WaitForSecondsAsync(0.05f);
            }

            isLoadingComplete = true;

            // �ε� �Ϸ� �� �ڵ����� �ΰ������� ��ȯ
            await Awaitable.WaitForSecondsAsync(0.5f);
            gameFlowManager?.TriggerEvent(GameEventType.LoadComplete);
        }
    }

    /// <summary>
    /// �ΰ��� ����
    /// </summary>
    public class IngameState : GameState
    {
        public IngameState() : base(GameStateType.Ingame) { }

        protected override async Awaitable EnterState(CancellationToken cancellationToken)
        {
            // �ΰ��� UI Ȱ��ȭ
            gameFlowManager?.ShowIngameUI();

            // ���� �ð� �簳
            Time.timeScale = 1f;

            await Awaitable.NextFrameAsync();
        }

        protected override async Awaitable ExitState(CancellationToken cancellationToken)
        {
            // �ΰ��� UI �����
            await Awaitable.NextFrameAsync();
        }

        protected override void UpdateState(float deltaTime)
        {
            // �ΰ��� ������Ʈ ����
            // ESC Ű�� �Ͻ�����
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                gameFlowManager?.TriggerEvent(GameEventType.PauseGame);
            }
        }
    }

    /// <summary>
    /// �Ͻ����� ����
    /// </summary>
    public class PauseState : GameState
    {
        public PauseState() : base(GameStateType.Pause) { }

        protected override async Awaitable EnterState(CancellationToken cancellationToken)
        {
            // ���� �ð� ����
            Time.timeScale = 0f;

            // �Ͻ����� UI Ȱ��ȭ
            gameFlowManager?.ShowPauseMenu();

            await Awaitable.NextFrameAsync();
        }

        protected override async Awaitable ExitState(CancellationToken cancellationToken)
        {
            // �Ͻ����� UI ��Ȱ��ȭ
            gameFlowManager?.HidePauseMenu();

            // ���� �ð� �簳
            Time.timeScale = 1f;

            await Awaitable.NextFrameAsync();
        }

        protected override void UpdateState(float deltaTime)
        {
            // ESC Ű�� ���� �簳
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                gameFlowManager?.TriggerEvent(GameEventType.ResumeGame);
            }
        }
    }

    /// <summary>
    /// �޴� ����
    /// </summary>
    public class MenuState : GameState
    {
        public MenuState() : base(GameStateType.Menu) { }

        protected override async Awaitable EnterState(CancellationToken cancellationToken)
        {
            // �޴� UI Ȱ��ȭ
            gameFlowManager?.ShowInGameMenu();
            await Awaitable.NextFrameAsync();
        }

        protected override async Awaitable ExitState(CancellationToken cancellationToken)
        {
            // �޴� UI ��Ȱ��ȭ
            gameFlowManager?.HideInGameMenu();
            await Awaitable.NextFrameAsync();
        }

        protected override void UpdateState(float deltaTime)
        {
            // �޴� ������Ʈ ����
        }
    }

    /// <summary>
    /// �κ� ����
    /// </summary>
    public class LobbyState : GameState
    {
        public LobbyState() : base(GameStateType.Lobby) { }

        protected override async Awaitable EnterState(CancellationToken cancellationToken)
        {
            // �κ� UI Ȱ��ȭ
            gameFlowManager?.ShowLobby();
            await Awaitable.NextFrameAsync();
        }

        protected override async Awaitable ExitState(CancellationToken cancellationToken)
        {
            // �κ� UI ��Ȱ��ȭ
            gameFlowManager?.HideLobby();
            await Awaitable.NextFrameAsync();
        }

        protected override void UpdateState(float deltaTime)
        {
            // �κ� ������Ʈ ����
        }
    }
}