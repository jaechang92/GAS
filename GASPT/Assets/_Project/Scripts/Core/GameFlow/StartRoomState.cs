using System.Threading;
using UnityEngine;
using FSM.Core;
using GASPT.UI;

namespace GASPT.Core.GameFlow
{
    /// <summary>
    /// 준비실 상태
    /// - 정비, 상점, 메타 업그레이드
    /// - 던전 입장 포탈 활성화
    /// </summary>
    public class StartRoomState : State
    {
        public override string Name => "StartRoom";

        protected override async Awaitable OnEnterState(CancellationToken cancellationToken)
        {
            Debug.Log("[StartRoomState] 준비실 진입");

            // 현재 씬 확인
            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            Debug.Log($"[StartRoomState] 현재 씬: {currentScene}");

            if (currentScene != "StartRoom")
            {
                Debug.LogWarning($"[StartRoomState] StartRoom 씬이 아닙니다! 현재 씬: {currentScene}");
            }

            // Fade In (초기 진입 시 검은 화면에서 밝아짐)
            var fadeController = FadeController.Instance;
            if (fadeController != null && fadeController.IsFadedOut)
            {
                await fadeController.FadeIn(1.0f);
                Debug.Log("[StartRoomState] Fade In 완료");
            }

            // GameManager 통합
            var gameManager = GameManager.Instance;
            if (gameManager != null)
            {
                Debug.Log($"[StartRoomState] 메타 골드: {gameManager.Meta?.TotalGold ?? 0}");
                Debug.Log($"[StartRoomState] 언락 Form: {gameManager.Meta?.UnlockedFormCount ?? 0}개");
            }

            await Awaitable.NextFrameAsync(cancellationToken);
        }

        protected override async Awaitable OnExitState(CancellationToken cancellationToken)
        {
            Debug.Log("[StartRoomState] 준비실 퇴장");

            // TODO: 준비실 UI 숨기기
            // - 던전 입장 포탈 비활성화

            await Awaitable.NextFrameAsync(cancellationToken);
        }

        protected override void OnUpdateState(float deltaTime)
        {
            // 포탈 입장 감지는 Portal 스크립트에서 처리
            // Portal → GameFlowStateMachine.TriggerEnterDungeon() 호출
        }
    }
}
