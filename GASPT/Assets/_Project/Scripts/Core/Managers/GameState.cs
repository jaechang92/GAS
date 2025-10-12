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

        // === 동기 메서드 (기본 구현) ===
        public virtual void OnEnterSync()
        {
            Debug.Log($"[GameFlow] Entering {stateType} state (sync)");
            IsActive = true;
            EnterStateSync();
            OnEntered?.Invoke(this);
        }

        public virtual void OnExitSync()
        {
            Debug.Log($"[GameFlow] Exiting {stateType} state (sync)");
            IsActive = false;
            ExitStateSync();
            OnExited?.Invoke(this);
        }

        // === 비동기 메서드 (GameFlow 주 사용) ===
        public virtual async Awaitable OnEnter(CancellationToken cancellationToken = default)
        {
            Debug.Log($"[GameFlow] Entering {stateType} state (async)");
            IsActive = true;
            await EnterState(cancellationToken);
            OnEntered?.Invoke(this);
        }

        public virtual async Awaitable OnExit(CancellationToken cancellationToken = default)
        {
            Debug.Log($"[GameFlow] Exiting {stateType} state (async)");
            IsActive = false;
            await ExitState(cancellationToken);
            OnExited?.Invoke(this);
        }

        public virtual void OnUpdate(float deltaTime)
        {
            UpdateState(deltaTime);
        }

        // === 하위 클래스에서 구현할 메서드들 ===
        // 동기 메서드 (기본 구현 제공)
        protected virtual void EnterStateSync() { }
        protected virtual void ExitStateSync() { }

        // 비동기 메서드 (추상 - 하위 클래스에서 필수 구현)
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
    /// 게임플레이 리소스 로딩 상태
    /// Gameplay 카테고리 리소스를 로드
    /// </summary>
    public class LoadingState : GameState
    {
        public LoadingState() : base(GameStateType.Loading) { }

        protected override async Awaitable EnterState(CancellationToken cancellationToken)
        {
            Debug.Log("[LoadingState] 게임플레이 리소스 로딩 시작...");

            // 로딩 화면 활성화
            gameFlowManager?.ShowLoadingScreen();

            bool success = true;
            bool hasResourceManager = false;

            // GameResourceManager가 존재하는 경우에만 리소스 로드 시도
            try
            {
                var resourceManager = Core.Managers.GameResourceManager.Instance;

                if (resourceManager != null)
                {
                    hasResourceManager = true;
                    // 로드 진행률 이벤트 구독
                    resourceManager.OnLoadProgress += OnResourceLoadProgress;

                    // Gameplay 카테고리 로드
                    success = await resourceManager.LoadCategoryAsync(
                        Core.Enums.ResourceCategory.Gameplay,
                        cancellationToken
                    );

                    // 이벤트 구독 해제
                    resourceManager.OnLoadProgress -= OnResourceLoadProgress;
                }
                else
                {
                    Debug.LogWarning("[LoadingState] GameResourceManager가 없습니다. 리소스 로딩을 건너뜁니다.");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[LoadingState] 리소스 로딩 중 예외 발생: {ex.Message}. 계속 진행합니다.");
                success = true; // 예외 발생 시에도 계속 진행
            }

            // 리소스 매니저가 없으면 수동으로 진행률 업데이트 (시뮬레이션)
            if (!hasResourceManager)
            {
                Debug.Log("[LoadingState] 리소스 로딩 시뮬레이션 시작...");

                // 진행률을 수동으로 업데이트 (0% → 100%)
                for (float progress = 0f; progress <= 1f; progress += 0.25f)
                {
                    gameFlowManager?.UpdateLoadingProgress(progress);
                    Debug.Log($"[LoadingState] 로딩 진행률: {progress * 100:F0}%");
                    await Awaitable.WaitForSecondsAsync(0.3f, cancellationToken);
                }

                gameFlowManager?.UpdateLoadingProgress(1f);
                Debug.Log("[LoadingState] 로딩 시뮬레이션 완료!");
            }

            // 성공 여부와 관계없이 Ingame으로 전환 (데모/테스트 환경 고려)
            if (success)
            {
                Debug.Log("[LoadingState] 게임플레이 리소스 로딩 완료!");
            }
            else
            {
                Debug.LogWarning("[LoadingState] 리소스 로딩 실패했지만 계속 진행합니다.");
            }

            // 잠시 대기 후 인게임으로 전환
            await Awaitable.WaitForSecondsAsync(0.5f, cancellationToken);

            Debug.Log("[LoadingState] Ingame으로 전환 시도...");
            // 비동기 전환 사용 (TriggerEvent는 동기 방식이므로 StateMachine에 직접 접근)
            await StateMachine.ForceTransitionToAsync(GameStateType.Ingame.ToString());
        }

        protected override async Awaitable ExitState(CancellationToken cancellationToken)
        {
            // 로딩 화면 비활성화
            gameFlowManager?.HideLoadingScreen();
            await Awaitable.NextFrameAsync();
        }

        protected override void UpdateState(float deltaTime)
        {
            // 로딩 중에는 특별한 업데이트 로직 없음
        }

        private void OnResourceLoadProgress(Core.Enums.ResourceCategory category, float progress, string resourceName)
        {
            // UI 업데이트
            gameFlowManager?.UpdateLoadingProgress(progress);
            Debug.Log($"[LoadingState] {category} 로딩 중... {progress * 100:F0}% - {resourceName}");
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

    /// <summary>
    /// 초기 리소스 로딩 상태 (Preload)
    /// Essential + MainMenu 카테고리 리소스를 로드
    /// </summary>
    public class PreloadState : GameState
    {
        public bool IsCompleted { get; private set; } = false;

        public PreloadState() : base(GameStateType.Preload) { }

        protected override async Awaitable EnterState(CancellationToken cancellationToken)
        {
            Debug.Log("[PreloadState] 초기 리소스 로딩 시작...");

            // 최소 2초 보장을 위한 시작 시간 기록
            float startTime = Time.time;
            float minimumDuration = 2f;

            // 로딩 화면 활성화
            gameFlowManager?.ShowLoadingScreen();

            bool success = true;

            // GameResourceManager가 존재하는 경우에만 리소스 로드 시도
            try
            {
                var resourceManager = Core.Managers.GameResourceManager.Instance;

                if (resourceManager != null)
                {
                    // 로드 진행률 이벤트 구독
                    resourceManager.OnLoadProgress += OnResourceLoadProgress;

                    // Essential + MainMenu 카테고리 로드
                    success = await resourceManager.LoadCategoriesAsync(
                        new Core.Enums.ResourceCategory[]
                        {
                            Core.Enums.ResourceCategory.Essential,
                            Core.Enums.ResourceCategory.MainMenu
                        },
                        cancellationToken
                    );

                    // 이벤트 구독 해제
                    resourceManager.OnLoadProgress -= OnResourceLoadProgress;
                }
                else
                {
                    Debug.LogWarning("[PreloadState] GameResourceManager가 없습니다. 리소스 로딩을 건너뜁니다.");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[PreloadState] 리소스 로딩 중 예외 발생: {ex.Message}. 계속 진행합니다.");
                success = true; // 예외 발생 시에도 계속 진행
            }

            // 성공 여부와 관계없이 계속 진행 (데모/테스트 환경 고려)
            if (success)
            {
                Debug.Log("[PreloadState] 초기 리소스 로딩 완료!");
            }
            else
            {
                Debug.LogWarning("[PreloadState] 리소스 로딩 실패했지만 계속 진행합니다.");
            }

            // 최소 2초 보장 - 남은 시간만큼 대기
            float elapsed = Time.time - startTime;
            float remainingTime = Mathf.Max(0f, minimumDuration - elapsed);

            if (remainingTime > 0f)
            {
                Debug.Log($"[PreloadState] 최소 시간 보장을 위해 {remainingTime:F1}초 대기 중...");
                await Awaitable.WaitForSecondsAsync(remainingTime, cancellationToken);
            }

            // 작업 완료 - Transition이 자동으로 Main으로 전환
            IsCompleted = true;
            Debug.Log("[PreloadState] 리소스 로딩 완료 - Main으로 전환 대기 중...");
        }

        protected override async Awaitable ExitState(CancellationToken cancellationToken)
        {
            // 로딩 화면 비활성화
            gameFlowManager?.HideLoadingScreen();

            // 완료 플래그 리셋
            IsCompleted = false;

            await Awaitable.NextFrameAsync();
        }

        protected override void UpdateState(float deltaTime)
        {
            // 로딩 중에는 특별한 업데이트 로직 없음
        }

        private void OnResourceLoadProgress(Core.Enums.ResourceCategory category, float progress, string resourceName)
        {
            // UI 업데이트
            gameFlowManager?.UpdateLoadingProgress(progress);
            Debug.Log($"[PreloadState] {category} 로딩 중... {progress * 100:F0}% - {resourceName}");
        }
    }
}
