using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GASPT.UI.MVP.ViewModels;
using GASPT.UI.MVP.Presenters;

namespace GASPT.UI.MVP.Views
{
    /// <summary>
    /// 런 결과 화면 View 컴포넌트
    /// MVP 패턴 - 순수 UI 렌더링만 담당
    /// </summary>
    public class RunResultView : MonoBehaviour, IRunResultView
    {
        // ====== UI 요소 - 헤더 ======

        [Header("헤더")]
        [SerializeField] private GameObject rootPanel;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI subtitleText;
        [SerializeField] private Image resultIcon;
        [SerializeField] private Sprite clearIcon;
        [SerializeField] private Sprite deathIcon;


        // ====== UI 요소 - 통계 ======

        [Header("런 통계")]
        [SerializeField] private TextMeshProUGUI stageText;
        [SerializeField] private TextMeshProUGUI roomsText;
        [SerializeField] private TextMeshProUGUI enemiesText;
        [SerializeField] private TextMeshProUGUI playTimeText;


        // ====== UI 요소 - 재화 ======

        [Header("획득 재화")]
        [SerializeField] private TextMeshProUGUI goldText;
        [SerializeField] private TextMeshProUGUI boneText;
        [SerializeField] private TextMeshProUGUI soulText;
        [SerializeField] private GameObject boneContainer;
        [SerializeField] private GameObject soulContainer;


        // ====== UI 요소 - 누적 정보 ======

        [Header("누적 정보 (선택적)")]
        [SerializeField] private TextMeshProUGUI totalBoneText;
        [SerializeField] private TextMeshProUGUI totalSoulText;
        [SerializeField] private TextMeshProUGUI highestStageText;
        [SerializeField] private GameObject newRecordBadge;


        // ====== UI 요소 - 버튼 ======

        [Header("버튼")]
        [SerializeField] private Button returnButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private TextMeshProUGUI returnButtonText;
        [SerializeField] private TextMeshProUGUI restartButtonText;


        // ====== UI 요소 - 배경/이펙트 ======

        [Header("배경/이펙트")]
        [SerializeField] private Image backgroundOverlay;
        [SerializeField] private Color clearBackgroundColor = new Color(0.1f, 0.3f, 0.1f, 0.9f);
        [SerializeField] private Color deathBackgroundColor = new Color(0.3f, 0.1f, 0.1f, 0.9f);
        [SerializeField] private CanvasGroup canvasGroup;


        // ====== 이벤트 (IRunResultView) ======

        public event Action OnReturnToLobbyRequested;
        public event Action OnRestartRequested;
        public event Action OnViewOpened;
        public event Action OnViewClosed;


        // ====== 속성 ======

        public bool IsVisible => rootPanel != null && rootPanel.activeSelf;

        private RunResultPresenter presenter;


        // ====== Unity 생명주기 ======

        private void Awake()
        {
            // Presenter 생성
            presenter = new RunResultPresenter(this);

            // 버튼 이벤트 연결
            if (returnButton != null)
            {
                returnButton.onClick.AddListener(HandleReturnButtonClick);
            }

            if (restartButton != null)
            {
                restartButton.onClick.AddListener(HandleRestartButtonClick);
            }

            // 초기 상태: 숨김
            if (rootPanel != null)
            {
                rootPanel.SetActive(false);
            }
        }

        private void Start()
        {
            // Presenter 초기화
            presenter?.Initialize();
        }

        private void OnDestroy()
        {
            // Presenter 정리
            presenter?.Dispose();
            presenter = null;

            // 버튼 이벤트 해제
            if (returnButton != null)
            {
                returnButton.onClick.RemoveListener(HandleReturnButtonClick);
            }

            if (restartButton != null)
            {
                restartButton.onClick.RemoveListener(HandleRestartButtonClick);
            }
        }


        // ====== IRunResultView 구현 ======

        public void DisplayResults(RunResultViewModel viewModel)
        {
            if (viewModel == null) return;

            // 헤더
            SetTextSafe(titleText, viewModel.ResultTitle);
            SetTextSafe(subtitleText, viewModel.ResultSubtitle);

            // 아이콘 & 배경색
            if (resultIcon != null)
            {
                resultIcon.sprite = viewModel.IsCleared ? clearIcon : deathIcon;
            }

            if (backgroundOverlay != null)
            {
                backgroundOverlay.color = viewModel.IsCleared ? clearBackgroundColor : deathBackgroundColor;
            }

            // 런 통계
            SetTextSafe(stageText, $"스테이지 {viewModel.StageReached}");
            SetTextSafe(roomsText, $"{viewModel.RoomsCleared}개 방 클리어");
            SetTextSafe(enemiesText, $"{viewModel.EnemiesKilled}마리 처치");
            SetTextSafe(playTimeText, viewModel.PlayTimeFormatted);

            // 획득 재화
            SetTextSafe(goldText, $"+{viewModel.GoldEarned:N0}");
            SetTextSafe(boneText, $"+{viewModel.BoneEarned:N0}");
            SetTextSafe(soulText, $"+{viewModel.SoulEarned:N0}");

            // Soul 컨테이너는 Soul 획득 시에만 표시
            if (soulContainer != null)
            {
                soulContainer.SetActive(viewModel.SoulEarned > 0);
            }

            // Bone 컨테이너는 항상 표시 (0이어도)
            if (boneContainer != null)
            {
                boneContainer.SetActive(true);
            }

            // 누적 정보
            SetTextSafe(totalBoneText, $"{viewModel.TotalBone:N0}");
            SetTextSafe(totalSoulText, $"{viewModel.TotalSoul:N0}");
            SetTextSafe(highestStageText, $"최고 기록: 스테이지 {viewModel.HighestStage}");

            // 신기록 배지
            if (newRecordBadge != null)
            {
                newRecordBadge.SetActive(viewModel.IsNewRecord);
            }

            // 버튼 텍스트
            SetTextSafe(returnButtonText, "로비로 돌아가기");
            SetTextSafe(restartButtonText, "다시 도전");
        }

        public void ShowUI()
        {
            if (rootPanel != null)
            {
                rootPanel.SetActive(true);
            }

            // 페이드인 효과 (CanvasGroup이 있다면)
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }

            OnViewOpened?.Invoke();
        }

        public void HideUI()
        {
            if (rootPanel != null)
            {
                rootPanel.SetActive(false);
            }

            if (canvasGroup != null)
            {
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }

            OnViewClosed?.Invoke();
        }

        public void PlayCurrencyAnimation(int bone, int soul)
        {
            // 재화 카운트업 애니메이션 (선택적 구현)
            // TODO: DOTween 또는 코루틴으로 숫자 증가 애니메이션
        }

        public void PlayNewRecordEffect()
        {
            // 신기록 이펙트 (선택적 구현)
            if (newRecordBadge != null)
            {
                newRecordBadge.SetActive(true);
                // TODO: 반짝이는 애니메이션 추가
            }
        }

        public void SetButtonsEnabled(bool enabled)
        {
            if (returnButton != null)
            {
                returnButton.interactable = enabled;
            }

            if (restartButton != null)
            {
                restartButton.interactable = enabled;
            }
        }


        // ====== 외부 호출용 (상태 머신에서 직접 호출) ======

        /// <summary>
        /// 런 결과 표시 (외부에서 직접 호출 가능)
        /// </summary>
        public void ShowRunResult(bool cleared, int stage, int rooms, int enemies, float time, int gold, int bone, int soul)
        {
            presenter?.ShowResult(cleared, stage, rooms, enemies, time, gold, bone, soul);
        }

        /// <summary>
        /// 로비로 돌아가기 (외부 콜백 설정)
        /// </summary>
        public void SetReturnCallback(Action callback)
        {
            presenter?.SetReturnCallback(callback);
        }

        /// <summary>
        /// 재시작 콜백 설정
        /// </summary>
        public void SetRestartCallback(Action callback)
        {
            presenter?.SetRestartCallback(callback);
        }


        // ====== 버튼 핸들러 ======

        private void HandleReturnButtonClick()
        {
            OnReturnToLobbyRequested?.Invoke();
        }

        private void HandleRestartButtonClick()
        {
            OnRestartRequested?.Invoke();
        }


        // ====== 유틸리티 ======

        private void SetTextSafe(TextMeshProUGUI textComponent, string value)
        {
            if (textComponent != null)
            {
                textComponent.text = value;
            }
        }


        // ====== 에디터 테스트 ======

#if UNITY_EDITOR
        [ContextMenu("Test Clear Result")]
        private void TestClearResult()
        {
            ShowRunResult(true, 3, 15, 47, 325.5f, 1250, 85, 2);
        }

        [ContextMenu("Test Death Result")]
        private void TestDeathResult()
        {
            ShowRunResult(false, 2, 8, 23, 180.3f, 650, 35, 0);
        }
#endif
    }
}
