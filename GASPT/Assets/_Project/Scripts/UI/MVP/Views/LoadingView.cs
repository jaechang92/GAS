using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Core;

namespace GASPT.UI
{
    /// <summary>
    /// 로딩 화면 View (MVP 패턴)
    /// - 진행 바: 로딩 진행률 표시
    /// - 팁 텍스트: 랜덤 게임 팁 표시
    /// - 회전 아이콘: 로딩 중 애니메이션
    /// </summary>
    public class LoadingView : MonoBehaviour
    {
        // ====== UI 참조 ======

        [Header("진행 바")]
        [SerializeField] [Tooltip("진행률 슬라이더")]
        private Slider progressSlider;

        [SerializeField] [Tooltip("진행률 텍스트 (예: 75%)")]
        private TextMeshProUGUI progressText;

        [Header("로딩 텍스트")]
        [SerializeField] [Tooltip("로딩 상태 텍스트")]
        private TextMeshProUGUI loadingText;

        [SerializeField] [Tooltip("기본 로딩 텍스트")]
        private string defaultLoadingText = "로딩 중...";

        [Header("팁 시스템")]
        [SerializeField] [Tooltip("팁 텍스트")]
        private TextMeshProUGUI tipText;

        [SerializeField] [Tooltip("게임 팁 목록")]
        private List<string> tips = new List<string>
        {
            "폼을 교체하면 잠시 무적 상태가 됩니다.",
            "보스는 각 페이즈마다 다른 패턴을 사용합니다.",
            "상점에서 아이템을 구매하여 능력을 강화하세요.",
            "스킬 쿨다운을 잘 관리하면 전투가 수월해집니다.",
            "던전의 분기점에서 다양한 보상을 선택할 수 있습니다.",
            "엘리트 적은 일반 적보다 강하지만 더 좋은 보상을 줍니다.",
            "체력이 낮을 때는 방어적으로 플레이하세요.",
            "각 폼은 고유한 능력과 스탯 보너스를 가지고 있습니다."
        };

        [Header("회전 아이콘")]
        [SerializeField] [Tooltip("회전할 로딩 아이콘")]
        private RectTransform spinnerIcon;

        [SerializeField] [Tooltip("회전 속도 (도/초)")]
        private float spinSpeed = 180f;

        [Header("컨테이너")]
        [SerializeField] [Tooltip("로딩 UI 컨테이너 (전체 패널)")]
        private GameObject container;


        // ====== 이벤트 ======

        /// <summary>로딩 완료 이벤트</summary>
        public event Action OnLoadingComplete;


        // ====== 상태 ======

        private bool isVisible;
        private float targetProgress;
        private float currentProgress;
        private float progressLerpSpeed = 3f;


        // ====== 프로퍼티 ======

        /// <summary>표시 중 여부</summary>
        public bool IsVisible => isVisible;

        /// <summary>현재 진행률 (0~1)</summary>
        public float Progress => currentProgress;


        // ====== Unity 생명주기 ======

        private void Awake()
        {
            ValidateReferences();

            // 시작 시 숨김
            if (container != null)
            {
                container.SetActive(false);
            }
        }

        private void Update()
        {
            if (!isVisible) return;

            // 진행 바 부드러운 업데이트
            UpdateProgressSmooth();

            // 스피너 회전
            UpdateSpinner();
        }


        // ====== 공개 메서드 ======

        /// <summary>
        /// 로딩 화면 표시
        /// </summary>
        public void Show()
        {
            if (container != null)
            {
                container.SetActive(true);
            }

            isVisible = true;
            currentProgress = 0f;
            targetProgress = 0f;

            // 진행 바 초기화
            SetProgressImmediate(0f);

            // 로딩 텍스트 초기화
            SetLoadingText(defaultLoadingText);

            // 랜덤 팁 표시
            ShowRandomTip();

            Debug.Log("[LoadingView] 로딩 화면 표시");
        }

        /// <summary>
        /// 로딩 화면 숨기기
        /// </summary>
        public void Hide()
        {
            if (container != null)
            {
                container.SetActive(false);
            }

            isVisible = false;

            Debug.Log("[LoadingView] 로딩 화면 숨김");
        }

        /// <summary>
        /// 진행률 설정 (0~1, 부드러운 전환)
        /// </summary>
        public void SetProgress(float progress)
        {
            targetProgress = Mathf.Clamp01(progress);
        }

        /// <summary>
        /// 진행률 즉시 설정 (0~1)
        /// </summary>
        public void SetProgressImmediate(float progress)
        {
            progress = Mathf.Clamp01(progress);
            targetProgress = progress;
            currentProgress = progress;

            if (progressSlider != null)
            {
                progressSlider.value = progress;
            }

            UpdateProgressText();
        }

        /// <summary>
        /// 로딩 텍스트 설정
        /// </summary>
        public void SetLoadingText(string text)
        {
            if (loadingText != null)
            {
                loadingText.text = text;
            }
        }

        /// <summary>
        /// 팁 텍스트 직접 설정
        /// </summary>
        public void SetTip(string tip)
        {
            if (tipText != null)
            {
                tipText.text = tip;
            }
        }

        /// <summary>
        /// 랜덤 팁 표시
        /// </summary>
        public void ShowRandomTip()
        {
            if (tipText != null && tips.Count > 0)
            {
                int randomIndex = UnityEngine.Random.Range(0, tips.Count);
                tipText.text = $"TIP: {tips[randomIndex]}";
            }
        }

        /// <summary>
        /// 팁 목록에 팁 추가
        /// </summary>
        public void AddTip(string tip)
        {
            if (!string.IsNullOrEmpty(tip) && !tips.Contains(tip))
            {
                tips.Add(tip);
            }
        }

        /// <summary>
        /// 로딩 완료 처리
        /// </summary>
        public void Complete()
        {
            SetProgressImmediate(1f);
            SetLoadingText("완료!");
            OnLoadingComplete?.Invoke();
        }


        // ====== 내부 메서드 ======

        /// <summary>
        /// UI 참조 검증
        /// </summary>
        private void ValidateReferences()
        {
            if (progressSlider == null)
            {
                Debug.LogWarning("[LoadingView] progressSlider가 설정되지 않았습니다.");
            }

            if (container == null)
            {
                // 컨테이너가 없으면 자기 자신을 컨테이너로 사용
                container = gameObject;
            }
        }

        /// <summary>
        /// 진행 바 부드러운 업데이트
        /// </summary>
        private void UpdateProgressSmooth()
        {
            if (Mathf.Abs(currentProgress - targetProgress) > 0.001f)
            {
                currentProgress = Mathf.Lerp(currentProgress, targetProgress, Time.deltaTime * progressLerpSpeed);

                if (progressSlider != null)
                {
                    progressSlider.value = currentProgress;
                }

                UpdateProgressText();
            }
        }

        /// <summary>
        /// 진행률 텍스트 업데이트
        /// </summary>
        private void UpdateProgressText()
        {
            if (progressText != null)
            {
                int percent = Mathf.RoundToInt(currentProgress * 100f);
                progressText.text = $"{percent}%";
            }
        }

        /// <summary>
        /// 스피너 회전 업데이트
        /// </summary>
        private void UpdateSpinner()
        {
            if (spinnerIcon != null)
            {
                spinnerIcon.Rotate(0f, 0f, -spinSpeed * Time.deltaTime);
            }
        }


        // ====== 싱글톤 접근 (선택적) ======

        private static LoadingView instance;

        /// <summary>싱글톤 인스턴스</summary>
        public static LoadingView Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindAnyObjectByType<LoadingView>();
                }
                return instance;
            }
        }

        /// <summary>인스턴스 존재 여부</summary>
        public static bool HasInstance => instance != null || FindAnyObjectByType<LoadingView>() != null;
    }
}
