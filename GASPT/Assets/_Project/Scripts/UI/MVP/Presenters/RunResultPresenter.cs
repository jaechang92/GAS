using System;
using UnityEngine;
using GASPT.UI.MVP.Views;
using GASPT.UI.MVP.ViewModels;
using GASPT.Meta.System;
using GASPT.Core;

namespace GASPT.UI.MVP.Presenters
{
    /// <summary>
    /// 런 결과 화면 Presenter
    /// MVP 패턴 - View와 Model 중재, 비즈니스 로직 처리
    /// </summary>
    public class RunResultPresenter : IDisposable
    {
        // ====== 참조 ======

        private readonly IRunResultView view;
        private RunResultViewModel currentViewModel;

        // 외부 콜백
        private Action returnCallback;
        private Action restartCallback;

        // 상태
        private bool isInitialized;
        private bool isDisposed;


        // ====== 생성자 ======

        public RunResultPresenter(IRunResultView view)
        {
            this.view = view ?? throw new ArgumentNullException(nameof(view));
        }


        // ====== 초기화/정리 ======

        public void Initialize()
        {
            if (isInitialized) return;

            // View 이벤트 구독
            view.OnReturnToLobbyRequested += HandleReturnToLobby;
            view.OnRestartRequested += HandleRestart;
            view.OnViewOpened += HandleViewOpened;
            view.OnViewClosed += HandleViewClosed;

            // MetaProgressionManager 이벤트 구독 (런 종료 시 자동 표시)
            if (MetaProgressionManager.HasInstance)
            {
                MetaProgressionManager.Instance.OnRunEnded += HandleRunEnded;
            }

            isInitialized = true;
        }

        public void Dispose()
        {
            if (isDisposed) return;

            // View 이벤트 해제
            if (view != null)
            {
                view.OnReturnToLobbyRequested -= HandleReturnToLobby;
                view.OnRestartRequested -= HandleRestart;
                view.OnViewOpened -= HandleViewOpened;
                view.OnViewClosed -= HandleViewClosed;
            }

            // MetaProgressionManager 이벤트 해제
            if (MetaProgressionManager.HasInstance)
            {
                MetaProgressionManager.Instance.OnRunEnded -= HandleRunEnded;
            }

            currentViewModel = null;
            returnCallback = null;
            restartCallback = null;
            isDisposed = true;
        }


        // ====== 외부 호출 API ======

        /// <summary>
        /// 런 결과 표시 (외부에서 직접 호출)
        /// </summary>
        public void ShowResult(bool cleared, int stage, int rooms, int enemies, float time, int gold, int bone, int soul)
        {
            // ViewModel 생성
            if (cleared)
            {
                currentViewModel = RunResultViewModel.CreateClearResult(stage, rooms, enemies, time, gold, bone, soul);
            }
            else
            {
                currentViewModel = RunResultViewModel.CreateDeathResult(stage, rooms, enemies, time, gold, bone, soul);
            }

            // 누적 정보 추가
            PopulateTotalStats();

            // View 업데이트
            view.DisplayResults(currentViewModel);
            view.ShowUI();

            // 게임 일시정지
            NotifyUIOpened();
        }

        /// <summary>
        /// 로비 복귀 콜백 설정
        /// </summary>
        public void SetReturnCallback(Action callback)
        {
            returnCallback = callback;
        }

        /// <summary>
        /// 재시작 콜백 설정
        /// </summary>
        public void SetRestartCallback(Action callback)
        {
            restartCallback = callback;
        }

        /// <summary>
        /// UI 숨기기
        /// </summary>
        public void Hide()
        {
            view?.HideUI();
            NotifyUIClosed();
        }


        // ====== View 이벤트 핸들러 ======

        private void HandleReturnToLobby()
        {
            Hide();

            // 콜백 실행 (GameOverState에서 설정한 로비 복귀 로직)
            returnCallback?.Invoke();
        }

        private void HandleRestart()
        {
            Hide();

            // 콜백 실행 (빠른 재시작 로직)
            restartCallback?.Invoke();
        }

        private void HandleViewOpened()
        {
            // View가 열렸을 때 추가 처리 (애니메이션 등)
            view?.SetButtonsEnabled(true);
        }

        private void HandleViewClosed()
        {
            // View가 닫혔을 때 정리
            currentViewModel = null;
        }


        // ====== MetaProgressionManager 이벤트 핸들러 ======

        private void HandleRunEnded(bool cleared)
        {
            // 런이 종료되면 자동으로 결과 화면 데이터 준비
            // 실제 표시는 GameOverState/DungeonClearedState에서 ShowResult() 호출
        }


        // ====== 데이터 수집 ======

        /// <summary>
        /// 누적 통계 정보 채우기
        /// </summary>
        private void PopulateTotalStats()
        {
            if (currentViewModel == null) return;

            // MetaProgressionManager에서 데이터 가져오기
            if (MetaProgressionManager.HasInstance)
            {
                var meta = MetaProgressionManager.Instance;

                currentViewModel.TotalBone = meta.Currency.Bone;
                currentViewModel.TotalSoul = meta.Currency.Soul;
                currentViewModel.HighestStage = meta.Progress.highestStage;
                currentViewModel.TotalClears = meta.Progress.totalClears;

                // 신기록 확인 (현재 스테이지 > 이전 최고 스테이지)
                // EndRun()에서 이미 highestStage를 업데이트했으므로,
                // 현재 스테이지와 같으면 이번 런이 신기록
                currentViewModel.IsNewRecord = currentViewModel.StageReached >= currentViewModel.HighestStage
                                               && currentViewModel.IsCleared;
            }
        }


        // ====== UIManager 연동 ======

        private void NotifyUIOpened()
        {
            // FullScreen UI 오픈 알림 (게임 Pause)
            if (UIManager.HasInstance)
            {
                UIManager.Instance.NotifyFullScreenUIOpened();
            }
            else
            {
                // UIManager가 없으면 직접 Pause
                Time.timeScale = 0f;
            }
        }

        private void NotifyUIClosed()
        {
            // FullScreen UI 클로즈 알림 (게임 Resume)
            if (UIManager.HasInstance)
            {
                UIManager.Instance.NotifyFullScreenUIClosed();
            }
            else
            {
                Time.timeScale = 1f;
            }
        }
    }
}
