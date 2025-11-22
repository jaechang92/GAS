using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using FSM.Core;
using GASPT.Gameplay.Level;
using GASPT.UI;

namespace GASPT.Core.GameFlow
{
    /// <summary>
    /// 던전 씬 로딩 상태
    /// - GameplayScene 로드
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

            // GameplayScene 로드 (Single 모드 - 기존 씬 완전 교체)
            Debug.Log("[LoadingDungeonState] GameplayScene 로딩 시작...");

            AsyncOperation loadOperation = SceneManager.LoadSceneAsync("GameplayScene", LoadSceneMode.Single);
            if (loadOperation != null)
            {
                while (!loadOperation.isDone)
                {
                    // 로딩 진행도 표시 (TODO: LoadingUI 업데이트)
                    // LoadingUI.SetProgress(loadOperation.progress);
                    await Awaitable.NextFrameAsync(cancellationToken);
                }

                Debug.Log("[LoadingDungeonState] GameplayScene 로딩 완료!");
            }
            else
            {
                Debug.LogError("[LoadingDungeonState] GameplayScene 로드 실패! Build Settings에 씬이 추가되었는지 확인하세요.");
                return;
            }

            // Player 초기화 대기 (중요: 다른 시스템보다 먼저 대기)
            await WaitForPlayerReady(cancellationToken);

            // RoomManager 초기화 대기
            await WaitForRoomManagerReady(cancellationToken);

            // 던전 시작
            var roomManager = RoomManager.Instance;
            if (roomManager != null)
            {
                await roomManager.StartDungeonAsync();
                Debug.Log("[LoadingDungeonState] 던전 시작 완료");
            }
            else
            {
                Debug.LogError("[LoadingDungeonState] RoomManager를 찾을 수 없습니다!");
            }

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
    }
}
