using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using GASPT.Meta;

namespace GASPT.UI.Meta
{
    /// <summary>
    /// 런 결과 화면 UI
    /// 런 종료 시 획득한 재화와 통계를 표시
    /// </summary>
    public class RunResultView : BaseUI
    {
        // ====== UI 요소 ======

        [Header("Run Result UI 요소")]

        [Tooltip("타이틀 텍스트")]
        [SerializeField] private Text titleText;

        [Tooltip("결과 상태 텍스트 (클리어/사망)")]
        [SerializeField] private Text resultStatusText;

        [Tooltip("Bone 획득량 텍스트")]
        [SerializeField] private Text boneEarnedText;

        [Tooltip("Soul 획득량 텍스트")]
        [SerializeField] private Text soulEarnedText;

        [Tooltip("총 Bone 보유량 텍스트")]
        [SerializeField] private Text totalBoneText;

        [Tooltip("총 Soul 보유량 텍스트")]
        [SerializeField] private Text totalSoulText;

        [Tooltip("런 통계 텍스트 (스테이지, 처치 수 등)")]
        [SerializeField] private Text statisticsText;

        [Tooltip("계속하기 버튼")]
        [SerializeField] private Button continueButton;

        [Tooltip("업그레이드 버튼")]
        [SerializeField] private Button upgradeButton;


        // ====== 설정 ======

        [Header("설정")]
        [Tooltip("클리어 시 타이틀")]
        [SerializeField] private string clearTitle = "던전 클리어!";

        [Tooltip("사망 시 타이틀")]
        [SerializeField] private string deathTitle = "사망...";

        [Tooltip("표시 시 시간 정지")]
        [SerializeField] private bool pauseGameOnShow = true;


        // ====== 이벤트 ======

        /// <summary>
        /// 계속하기 버튼 클릭 이벤트
        /// </summary>
        public event Action OnContinueClicked;

        /// <summary>
        /// 업그레이드 버튼 클릭 이벤트
        /// </summary>
        public event Action OnUpgradeClicked;


        // ====== 런타임 데이터 ======

        private int earnedBone;
        private int earnedSoul;


        // ====== 초기화 ======

        protected override void Initialize()
        {
            base.Initialize();

            // 버튼 이벤트 연결
            if (continueButton != null)
            {
                continueButton.onClick.AddListener(OnContinueButtonClick);
            }

            if (upgradeButton != null)
            {
                upgradeButton.onClick.AddListener(OnUpgradeButtonClick);
            }
        }


        // ====== UI 표시 ======

        /// <summary>
        /// 런 결과 표시
        /// </summary>
        /// <param name="cleared">클리어 여부</param>
        /// <param name="boneEarned">획득한 Bone</param>
        /// <param name="soulEarned">획득한 Soul</param>
        /// <param name="stageReached">도달한 스테이지</param>
        /// <param name="enemiesKilled">처치한 적 수</param>
        public void Show(bool cleared, int boneEarned, int soulEarned, int stageReached, int enemiesKilled)
        {
            // 데이터 저장
            earnedBone = boneEarned;
            earnedSoul = soulEarned;

            // BaseUI 표시
            base.Show();

            // 타이틀 업데이트
            UpdateTitle(cleared);

            // 재화 정보 업데이트
            UpdateCurrencyInfo(boneEarned, soulEarned);

            // 통계 업데이트
            UpdateStatistics(cleared, stageReached, enemiesKilled);

            // 시간 정지
            if (pauseGameOnShow)
            {
                Time.timeScale = 0f;
            }

            Debug.Log($"[RunResultView] 표시 - 클리어: {cleared}, Bone: +{boneEarned}, Soul: +{soulEarned}");
        }

        /// <summary>
        /// UI 숨김
        /// </summary>
        public override void Hide()
        {
            base.Hide();

            // 시간 재개
            if (pauseGameOnShow)
            {
                Time.timeScale = 1f;
            }
        }


        // ====== UI 업데이트 ======

        /// <summary>
        /// 타이틀 업데이트
        /// </summary>
        private void UpdateTitle(bool cleared)
        {
            if (titleText != null)
            {
                titleText.text = cleared ? clearTitle : deathTitle;
            }

            if (resultStatusText != null)
            {
                resultStatusText.text = cleared ? "축하합니다!" : "다시 도전하세요!";
                resultStatusText.color = cleared ? Color.green : Color.red;
            }
        }

        /// <summary>
        /// 재화 정보 업데이트
        /// </summary>
        private void UpdateCurrencyInfo(int boneEarned, int soulEarned)
        {
            if (boneEarnedText != null)
            {
                boneEarnedText.text = $"+{boneEarned}";
            }

            if (soulEarnedText != null)
            {
                soulEarnedText.text = $"+{soulEarned}";
            }

            // 총 보유량 표시 (MetaProgressionManager에서 가져오기)
            if (MetaProgressionManager.HasInstance)
            {
                var currency = MetaProgressionManager.Instance.Currency;

                if (totalBoneText != null)
                {
                    totalBoneText.text = $"총 Bone: {currency.Bone}";
                }

                if (totalSoulText != null)
                {
                    totalSoulText.text = $"총 Soul: {currency.Soul}";
                }
            }
        }

        /// <summary>
        /// 통계 업데이트
        /// </summary>
        private void UpdateStatistics(bool cleared, int stageReached, int enemiesKilled)
        {
            if (statisticsText != null)
            {
                string stats = $"도달 스테이지: {stageReached}\n" +
                              $"처치한 적: {enemiesKilled}";

                if (MetaProgressionManager.HasInstance)
                {
                    var progress = MetaProgressionManager.Instance.Progress;
                    stats += $"\n\n--- 누적 기록 ---\n" +
                            $"총 런 횟수: {progress.totalRuns}\n" +
                            $"클리어 횟수: {progress.totalClears}\n" +
                            $"최고 스테이지: {progress.highestStage}";
                }

                statisticsText.text = stats;
            }
        }


        // ====== 버튼 이벤트 ======

        /// <summary>
        /// 계속하기 버튼 클릭
        /// </summary>
        private void OnContinueButtonClick()
        {
            Debug.Log("[RunResultView] 계속하기 버튼 클릭");

            Hide();
            OnContinueClicked?.Invoke();

            // 기본 동작: 현재 씬 재시작
            if (OnContinueClicked == null)
            {
                RestartCurrentScene();
            }
        }

        /// <summary>
        /// 업그레이드 버튼 클릭
        /// </summary>
        private void OnUpgradeButtonClick()
        {
            Debug.Log("[RunResultView] 업그레이드 버튼 클릭");

            OnUpgradeClicked?.Invoke();

            // 기본 동작: 업그레이드 UI가 없으면 로그만 출력
            if (OnUpgradeClicked == null)
            {
                Debug.Log("[RunResultView] 업그레이드 UI 연동 필요");
            }
        }

        /// <summary>
        /// 현재 씬 재시작
        /// </summary>
        private void RestartCurrentScene()
        {
            Time.timeScale = 1f;
            string currentSceneName = SceneManager.GetActiveScene().name;
            SceneManager.LoadScene(currentSceneName);
        }


        // ====== 디버그 ======

        [ContextMenu("Show UI (Test - Clear)")]
        private void DebugShowClear()
        {
            Show(true, 150, 25, 5, 42);
        }

        [ContextMenu("Show UI (Test - Death)")]
        private void DebugShowDeath()
        {
            Show(false, 80, 0, 3, 28);
        }
    }
}
