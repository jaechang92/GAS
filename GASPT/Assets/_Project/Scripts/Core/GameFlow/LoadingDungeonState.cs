using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using FSM.Core;
using GASPT.Gameplay.Level;
using GASPT.Gameplay.Level.Graph;
using GASPT.Gameplay.Level.Generation;
using GASPT.UI;
using GASPT.UI.Minimap;
using GASPT.Core.SceneManagement;
using GASPT.CameraSystem;

namespace GASPT.Core.GameFlow
{
    /// <summary>
    /// 던전 씬 로딩 상태
    /// - GameplayScene 로드 (Additive)
    /// - RoomManager 초기화 대기
    /// - 던전 시작
    /// </summary>
    public class LoadingDungeonState : State
    {
        public override string Name => "LoadingDungeon";

        protected override async Awaitable OnEnterState(CancellationToken cancellationToken)
        {
            Debug.Log("[LoadingDungeonState] 던전 로딩 시작");

            // Fade Out (화면을 검게)
            var fadeController = FadeController.Instance;
            if (fadeController != null)
            {
                await fadeController.FadeOut(0.5f);
                Debug.Log("[LoadingDungeonState] Fade Out 완료");
            }

            // TODO: 로딩 UI 표시
            // LoadingUI.Show();

            // AdditiveSceneLoader를 통한 씬 전환
            var sceneLoader = AdditiveSceneLoader.Instance;
            if (sceneLoader != null)
            {
                Debug.Log("[LoadingDungeonState] GameplayScene 로딩 시작 (Additive)...");
                await sceneLoader.SwitchContentSceneAsync("GameplayScene", cancellationToken);
                Debug.Log("[LoadingDungeonState] GameplayScene 로딩 완료!");
            }
            else
            {
                // Fallback: 기존 Single 모드 로딩
                Debug.LogWarning("[LoadingDungeonState] AdditiveSceneLoader 없음 - Single 모드로 로딩");
                AsyncOperation loadOperation = SceneManager.LoadSceneAsync("GameplayScene", LoadSceneMode.Single);
                if (loadOperation != null)
                {
                    while (!loadOperation.isDone)
                    {
                        await Awaitable.NextFrameAsync(cancellationToken);
                    }
                }
                else
                {
                    Debug.LogError("[LoadingDungeonState] GameplayScene 로드 실패!");
                    return;
                }
            }

            // Player 초기화 대기 (중요: 다른 시스템보다 먼저 대기)
            await WaitForPlayerReady(cancellationToken);

            // RoomManager 초기화 대기
            await WaitForRoomManagerReady(cancellationToken);

            // 던전 시작 (StageManager 또는 RoomManager 사용)
            await StartDungeonWithStageSystem(cancellationToken);

            // ★ Scene 검증 및 재할당 (카메라, UI 등)
            await ValidateSceneReferences(cancellationToken);

            // TODO: 로딩 UI 숨기기
            // LoadingUI.Hide();

            // Fade In (화면을 밝게)
            if (fadeController != null)
            {
                await fadeController.FadeIn(1.0f);
                Debug.Log("[LoadingDungeonState] Fade In 완료");
            }

            // 던전 로딩 완료 알림
            var gameFlowFSM = GameFlowStateMachine.Instance;
            if (gameFlowFSM != null)
            {
                gameFlowFSM.NotifyDungeonLoaded();
            }
        }

        /// <summary>
        /// Scene 참조 검증 및 재할당
        /// SceneValidationManager를 통해 모든 등록된 검증기 실행
        /// </summary>
        private async Awaitable ValidateSceneReferences(CancellationToken cancellationToken)
        {
            Debug.Log("[LoadingDungeonState] Scene 검증 시작...");

            if (SceneValidationManager.HasInstance)
            {
                bool success = await SceneValidationManager.Instance.ValidateAllAsync();
                if (success)
                {
                    Debug.Log("[LoadingDungeonState] Scene 검증 완료 - 모든 참조 유효");
                }
                else
                {
                    Debug.LogWarning("[LoadingDungeonState] Scene 검증 완료 - 일부 참조 실패 (게임 계속 진행)");
                }
            }
            else
            {
                Debug.LogWarning("[LoadingDungeonState] SceneValidationManager 없음 - 검증 스킵");
            }
        }

        protected override async Awaitable OnExitState(CancellationToken cancellationToken)
        {
            Debug.Log("[LoadingDungeonState] 로딩 완료 - 전투 시작");
            await Awaitable.NextFrameAsync(cancellationToken);
        }

        /// <summary>
        /// Player 초기화 대기 (GameManager.PlayerStats가 등록될 때까지)
        /// </summary>
        private async Awaitable WaitForPlayerReady(CancellationToken cancellationToken)
        {
            int maxAttempts = 100; // 최대 10초 대기 (100 * 0.1초)
            int attempts = 0;

            while (attempts < maxAttempts)
            {
                // GameManager.PlayerStats 확인
                if (GASPT.Core.GameManager.HasInstance && GASPT.Core.GameManager.Instance.PlayerStats != null)
                {
                    Debug.Log("[LoadingDungeonState] Player 초기화 완료");
                    return;
                }

                await Awaitable.WaitForSecondsAsync(0.1f, cancellationToken);
                attempts++;
            }

            Debug.LogError("[LoadingDungeonState] Player 초기화 실패 - 타임아웃");
        }

        /// <summary>
        /// RoomManager가 준비될 때까지 대기
        /// </summary>
        private async Awaitable WaitForRoomManagerReady(CancellationToken cancellationToken)
        {
            int maxAttempts = 100; // 최대 10초 대기 (100 * 0.1초)
            int attempts = 0;

            while (RoomManager.Instance == null && attempts < maxAttempts)
            {
                await Awaitable.WaitForSecondsAsync(0.1f, cancellationToken);
                attempts++;
            }

            if (RoomManager.Instance == null)
            {
                Debug.LogError("[LoadingDungeonState] RoomManager 초기화 실패 - 타임아웃");
            }
            else
            {
                Debug.Log("[LoadingDungeonState] RoomManager 준비 완료");
            }
        }

        /// <summary>
        /// 스테이지 시스템을 통한 던전 시작
        /// StageManager가 있으면 사용, 없으면 RoomManager 직접 사용
        /// </summary>
        private async Awaitable StartDungeonWithStageSystem(CancellationToken cancellationToken)
        {
            var stageManager = StageManager.Instance;
            var roomManager = RoomManager.Instance;

            // StageManager가 있고 스테이지 설정이 있는 경우
            if (stageManager != null && stageManager.CurrentStage != null)
            {
                Debug.Log($"[LoadingDungeonState] StageManager를 통한 던전 시작: {stageManager.CurrentStage.stageName}");
                await stageManager.LoadFloorAsync(stageManager.CurrentFloorIndex);
                Debug.Log("[LoadingDungeonState] 스테이지 시스템 던전 시작 완료");

                // 미니맵 초기화 (그래프 기반일 경우)
                InitializeMinimapIfGraphBased();
            }
            // 기존 방식: RoomManager 직접 사용
            else if (roomManager != null)
            {
                // 그래프 기반 던전 확인
                if (roomManager.IsUsingGraphDungeon)
                {
                    await roomManager.StartGraphDungeonAsync();
                    Debug.Log("[LoadingDungeonState] 그래프 기반 던전 시작 완료");

                    // 미니맵 초기화
                    InitializeMinimapIfGraphBased();
                }
                else
                {
                    // 방이 없으면 기본 던전 로드 시도
                    if (roomManager.TotalRoomCount == 0)
                    {
                        await LoadDefaultDungeon(roomManager);
                    }

                    // 던전 시작
                    if (roomManager.TotalRoomCount > 0)
                    {
                        await roomManager.StartDungeonAsync();
                        Debug.Log("[LoadingDungeonState] 선형 던전 시작 완료");
                    }
                    else
                    {
                        Debug.LogError("[LoadingDungeonState] 던전 로드 실패 - 방이 없습니다!");
                    }
                }
            }
            else
            {
                Debug.LogError("[LoadingDungeonState] RoomManager를 찾을 수 없습니다!");
            }
        }

        /// <summary>
        /// 기본 던전 로드 (Resources에서 TestDungeon 로드)
        /// </summary>
        private async Awaitable LoadDefaultDungeon(RoomManager roomManager)
        {
            Debug.Log("[LoadingDungeonState] 방이 없음 - 기본 던전 로드 시도...");

            // 1. StageManager에서 기본 스테이지 시작 시도
            var stageManager = StageManager.Instance;
            if (stageManager != null)
            {
                // Resources에서 기본 StageConfig 로드
                var defaultStage = Resources.Load<StageConfig>("Data/Stages/Stage1_Forest");
                if (defaultStage != null && defaultStage.floors != null && defaultStage.floors.Length > 0)
                {
                    Debug.Log($"[LoadingDungeonState] 기본 스테이지 로드: {defaultStage.stageName}");
                    await stageManager.StartStageAsync(defaultStage);
                    return;
                }
            }

            // 2. Resources에서 기본 DungeonConfig 로드
            var defaultDungeon = Resources.Load<DungeonConfig>("Data/Dungeons/TestDungeon");
            if (defaultDungeon != null)
            {
                Debug.Log($"[LoadingDungeonState] 기본 던전 로드: {defaultDungeon.dungeonName}");

                // 그래프 기반 던전 생성
                var generator = new DungeonGenerator();
                var graph = await generator.GenerateDungeonAsync(defaultDungeon);

                if (graph != null && defaultDungeon.roomDataPool != null)
                {
                    var placer = new RoomPlacer(defaultDungeon.roomDataPool, defaultDungeon.roomTemplatePrefab);
                    var roomMap = placer.PlaceRooms(graph, roomManager.transform);
                    roomManager.LoadDungeonGraph(graph, roomMap);
                    await roomManager.StartGraphDungeonAsync();
                    InitializeMinimapIfGraphBased();
                }
            }
            else
            {
                Debug.LogError("[LoadingDungeonState] 기본 던전 설정을 찾을 수 없습니다! (Resources/Data/Dungeons/TestDungeon)");
            }
        }

        /// <summary>
        /// 그래프 기반 던전일 경우 미니맵 초기화
        /// </summary>
        private void InitializeMinimapIfGraphBased()
        {
            var roomManager = RoomManager.Instance;
            if (roomManager == null || !roomManager.IsUsingGraphDungeon) return;

            var graph = roomManager.DungeonGraph;
            if (graph == null) return;

            // MinimapPresenter 찾기
            var minimapPresenter = Object.FindAnyObjectByType<MinimapPresenter>();
            if (minimapPresenter != null)
            {
                minimapPresenter.InitializeWithGraph(graph);
                minimapPresenter.SetCurrentNode(graph.entryNodeId);
                minimapPresenter.ShowMinimap();
                Debug.Log("[LoadingDungeonState] 미니맵 초기화 완료");
            }
            else
            {
                Debug.LogWarning("[LoadingDungeonState] MinimapPresenter를 찾을 수 없습니다.");
            }
        }
    }
}
