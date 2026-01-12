using System;
using System.Threading;
using UnityEngine;
using GASPT.Core.Extensions;

namespace GASPT.UI
{
    /// <summary>
    /// 로딩 화면 Presenter (MVP 패턴)
    /// LoadingView를 제어하고 로딩 진행 상태를 관리
    /// </summary>
    public class LoadingPresenter : MonoBehaviour
    {
        // ====== 참조 ======

        [Header("뷰")]
        [SerializeField] private LoadingView view;

        [Header("설정")]
        [SerializeField] private bool autoFindView = true;

        [Header("진행 단계 설정")]
        [SerializeField] [Tooltip("씬 로딩 완료 시 진행률")]
        private float sceneLoadedProgress = 0.5f;

        [SerializeField] [Tooltip("초기화 완료 시 진행률")]
        private float initCompleteProgress = 0.8f;

        [SerializeField] [Tooltip("최종 완료 진행률")]
        private float finalProgress = 1.0f;


        // ====== 이벤트 ======

        /// <summary>로딩 시작 이벤트</summary>
        public event Action OnLoadingStarted;

        /// <summary>로딩 완료 이벤트</summary>
        public event Action OnLoadingFinished;


        // ====== 상태 ======

        private bool isLoading;
        private CancellationTokenSource loadingCts;


        // ====== 프로퍼티 ======

        /// <summary>로딩 중 여부</summary>
        public bool IsLoading => isLoading;


        // ====== Unity 생명주기 ======

        private void Awake()
        {
            if (autoFindView && view == null)
            {
                view = GetComponentInChildren<LoadingView>();
                if (view == null)
                {
                    view = FindAnyObjectByType<LoadingView>();
                }
            }
        }

        private void OnDestroy()
        {
            loadingCts?.Cancel();
            loadingCts?.Dispose();
        }


        // ====== 공개 메서드 ======

        /// <summary>
        /// 로딩 시작
        /// </summary>
        /// <param name="loadingText">표시할 로딩 텍스트</param>
        public void StartLoading(string loadingText = null)
        {
            if (view == null)
            {
                Debug.LogWarning("[LoadingPresenter] View가 없습니다.");
                return;
            }

            isLoading = true;
            loadingCts?.Cancel();
            loadingCts = new CancellationTokenSource();

            view.Show();

            if (!string.IsNullOrEmpty(loadingText))
            {
                view.SetLoadingText(loadingText);
            }

            OnLoadingStarted?.Invoke();

            Debug.Log($"[LoadingPresenter] 로딩 시작: {loadingText}");
        }

        /// <summary>
        /// 로딩 종료
        /// </summary>
        public void FinishLoading()
        {
            if (view == null) return;

            view.Complete();

            // 잠시 후 숨기기 (완료 표시를 보여주기 위해)
            HideAfterDelayAsync().Forget();
        }

        /// <summary>
        /// 로딩 즉시 숨기기
        /// </summary>
        public void HideImmediately()
        {
            if (view == null) return;

            isLoading = false;
            loadingCts?.Cancel();
            view.Hide();

            OnLoadingFinished?.Invoke();

            Debug.Log("[LoadingPresenter] 로딩 화면 즉시 숨김");
        }

        /// <summary>
        /// 진행률 설정 (0~1)
        /// </summary>
        public void SetProgress(float progress)
        {
            if (view != null)
            {
                view.SetProgress(progress);
            }
        }

        /// <summary>
        /// 로딩 텍스트 변경
        /// </summary>
        public void SetLoadingText(string text)
        {
            if (view != null)
            {
                view.SetLoadingText(text);
            }
        }

        /// <summary>
        /// 씬 로딩 완료 알림
        /// </summary>
        public void NotifySceneLoaded()
        {
            SetProgress(sceneLoadedProgress);
            SetLoadingText("초기화 중...");
            Debug.Log("[LoadingPresenter] 씬 로딩 완료");
        }

        /// <summary>
        /// 초기화 완료 알림
        /// </summary>
        public void NotifyInitComplete()
        {
            SetProgress(initCompleteProgress);
            SetLoadingText("준비 완료!");
            Debug.Log("[LoadingPresenter] 초기화 완료");
        }

        /// <summary>
        /// 최종 완료 알림
        /// </summary>
        public void NotifyFinalComplete()
        {
            SetProgress(finalProgress);
            FinishLoading();
        }


        // ====== 단계별 로딩 메서드 ======

        /// <summary>
        /// 던전 로딩 시작
        /// </summary>
        public void StartDungeonLoading()
        {
            StartLoading("던전 입장 중...");
        }

        /// <summary>
        /// 준비실 로딩 시작
        /// </summary>
        public void StartStartRoomLoading()
        {
            StartLoading("준비실로 이동 중...");
        }

        /// <summary>
        /// 게임 시작 로딩
        /// </summary>
        public void StartGameLoading()
        {
            StartLoading("게임 로딩 중...");
        }


        // ====== 내부 메서드 ======

        /// <summary>
        /// 지연 후 숨기기
        /// </summary>
        private async Awaitable HideAfterDelayAsync()
        {
            try
            {
                await Awaitable.WaitForSecondsAsync(0.5f, loadingCts.Token);

                isLoading = false;
                view?.Hide();
                OnLoadingFinished?.Invoke();

                Debug.Log("[LoadingPresenter] 로딩 완료 - 화면 숨김");
            }
            catch (OperationCanceledException)
            {
                // 취소됨 - 무시
            }
        }


        // ====== 싱글톤 접근 ======

        private static LoadingPresenter instance;

        /// <summary>싱글톤 인스턴스</summary>
        public static LoadingPresenter Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindAnyObjectByType<LoadingPresenter>();
                }
                return instance;
            }
        }

        /// <summary>인스턴스 존재 여부</summary>
        public static bool HasInstance => instance != null || FindAnyObjectByType<LoadingPresenter>() != null;

        private void OnEnable()
        {
            if (instance == null)
            {
                instance = this;
            }
        }

        private void OnDisable()
        {
            if (instance == this)
            {
                instance = null;
            }
        }
    }
}
