using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GASPT.Data;
using GASPT.Gameplay.Boss;
using GASPT.UI.MVP;

namespace GASPT.UI.Boss
{
    /// <summary>
    /// 보스 등장 연출 컨트롤러
    /// 화면 어두워짐, 보스 이름 표시, 전투 시작 시퀀스
    /// </summary>
    public class BossIntroController : MonoBehaviour
    {
        // ====== UI 참조 ======

        [Header("연출 UI")]
        [SerializeField]
        private GameObject introPanel;

        [SerializeField]
        private CanvasGroup fadeOverlay;

        [SerializeField]
        private TextMeshProUGUI bossNameText;

        [SerializeField]
        private TextMeshProUGUI bossSubtitleText;

        [SerializeField]
        private Image bossIconImage;

        [SerializeField]
        private Animator introAnimator;


        // ====== 설정 ======

        [Header("연출 시간")]
        [SerializeField]
        private float fadeInDuration = 0.5f;

        [SerializeField]
        private float nameDisplayDuration = 1.5f;

        [SerializeField]
        private float fadeOutDuration = 0.5f;

        [SerializeField]
        private float playerLockDuration = 2f;


        // ====== 이벤트 ======

        /// <summary>
        /// 등장 연출 완료 이벤트
        /// </summary>
        public event Action OnIntroComplete;

        /// <summary>
        /// 전투 시작 이벤트
        /// </summary>
        public event Action<BaseBoss> OnBattleStart;


        // ====== 상태 ======

        private bool isPlaying = false;
        private BaseBoss currentBoss;


        // ====== 싱글톤 ======

        private static BossIntroController instance;
        public static BossIntroController Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindAnyObjectByType<BossIntroController>();
                }
                return instance;
            }
        }


        // ====== Unity 생명주기 ======

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;

            HideImmediate();
        }

        private void OnDestroy()
        {
            if (instance == this)
                instance = null;
        }


        // ====== 공개 API ======

        /// <summary>
        /// 보스 등장 연출 재생
        /// </summary>
        public async Awaitable PlayIntro(BossData bossData, Transform spawnPoint, GameObject bossPrefab = null)
        {
            if (isPlaying)
            {
                Debug.LogWarning("[BossIntroController] 이미 연출 재생 중입니다.");
                return;
            }

            if (bossData == null)
            {
                Debug.LogError("[BossIntroController] bossData가 null입니다.");
                return;
            }

            isPlaying = true;

            try
            {
                Debug.Log($"[BossIntroController] 등장 연출 시작: {bossData.bossName}");

                // 1. 플레이어 조작 잠금
                LockPlayerControl(true);

                // 2. 화면 페이드 인 (어두워짐)
                await FadeIn();

                // 3. 보스 스폰
                if (bossPrefab != null && spawnPoint != null)
                {
                    currentBoss = SpawnBoss(bossData, bossPrefab, spawnPoint);
                }

                // 4. 보스 이름 표시
                await ShowBossName(bossData);

                // 5. 화면 페이드 아웃
                await FadeOut();

                // 6. 체력바 UI 표시
                if (currentBoss != null && BossHealthBarPresenter.Instance != null)
                {
                    BossHealthBarPresenter.Instance.BindBoss(currentBoss);
                }

                // 7. 플레이어 조작 잠금 해제
                LockPlayerControl(false);

                // 8. 전투 시작
                if (currentBoss != null)
                {
                    currentBoss.StartCombat();
                    OnBattleStart?.Invoke(currentBoss);
                }

                OnIntroComplete?.Invoke();

                Debug.Log("[BossIntroController] 등장 연출 완료, 전투 시작!");
            }
            finally
            {
                isPlaying = false;
                HideImmediate();
            }
        }

        /// <summary>
        /// 기존 보스로 등장 연출 재생
        /// </summary>
        public async Awaitable PlayIntro(BaseBoss existingBoss)
        {
            if (existingBoss == null || existingBoss.Data == null)
            {
                Debug.LogError("[BossIntroController] existingBoss 또는 Data가 null입니다.");
                return;
            }

            currentBoss = existingBoss;

            isPlaying = true;

            try
            {
                // 1. 플레이어 조작 잠금
                LockPlayerControl(true);

                // 2. 화면 페이드 인
                await FadeIn();

                // 3. 보스 이름 표시
                await ShowBossName(existingBoss.Data);

                // 4. 화면 페이드 아웃
                await FadeOut();

                // 5. 체력바 UI 표시
                if (BossHealthBarPresenter.Instance != null)
                {
                    BossHealthBarPresenter.Instance.BindBoss(currentBoss);
                }

                // 6. 플레이어 조작 잠금 해제
                LockPlayerControl(false);

                // 7. 전투 시작
                currentBoss.StartCombat();
                OnBattleStart?.Invoke(currentBoss);

                OnIntroComplete?.Invoke();
            }
            finally
            {
                isPlaying = false;
                HideImmediate();
            }
        }

        /// <summary>
        /// 연출 스킵
        /// </summary>
        public void SkipIntro()
        {
            if (!isPlaying) return;

            // 즉시 종료 처리
            isPlaying = false;
            HideImmediate();
            LockPlayerControl(false);

            if (currentBoss != null)
            {
                if (BossHealthBarPresenter.Instance != null)
                {
                    BossHealthBarPresenter.Instance.BindBoss(currentBoss);
                }

                currentBoss.StartCombat();
                OnBattleStart?.Invoke(currentBoss);
            }

            OnIntroComplete?.Invoke();

            Debug.Log("[BossIntroController] 연출 스킵됨");
        }


        // ====== 연출 단계 ======

        private async Awaitable FadeIn()
        {
            if (fadeOverlay == null) return;

            introPanel?.SetActive(true);
            fadeOverlay.alpha = 0f;

            float elapsed = 0f;

            while (elapsed < fadeInDuration)
            {
                elapsed += Time.deltaTime;
                fadeOverlay.alpha = Mathf.Lerp(0f, 0.8f, elapsed / fadeInDuration);
                await Awaitable.NextFrameAsync();
            }

            fadeOverlay.alpha = 0.8f;
        }

        private async Awaitable FadeOut()
        {
            if (fadeOverlay == null) return;

            float elapsed = 0f;

            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                fadeOverlay.alpha = Mathf.Lerp(0.8f, 0f, elapsed / fadeOutDuration);
                await Awaitable.NextFrameAsync();
            }

            fadeOverlay.alpha = 0f;
        }

        private async Awaitable ShowBossName(BossData bossData)
        {
            // 보스 이름 설정
            if (bossNameText != null)
            {
                bossNameText.text = bossData.bossName;
                bossNameText.gameObject.SetActive(true);
            }

            // 서브타이틀 설정
            if (bossSubtitleText != null)
            {
                string subtitle = bossData.bossGrade switch
                {
                    Core.Enums.BossGrade.MiniBoss => "미니보스",
                    Core.Enums.BossGrade.MidBoss => "중간보스",
                    Core.Enums.BossGrade.FinalBoss => "최종보스",
                    _ => ""
                };

                bossSubtitleText.text = subtitle;
                bossSubtitleText.gameObject.SetActive(true);
            }

            // 아이콘 설정
            if (bossIconImage != null && bossData.icon != null)
            {
                bossIconImage.sprite = bossData.icon;
                bossIconImage.gameObject.SetActive(true);
            }

            // 애니메이션 트리거
            if (introAnimator != null)
            {
                introAnimator.SetTrigger("ShowName");
            }

            // 표시 시간 대기
            await Awaitable.WaitForSecondsAsync(nameDisplayDuration);

            // 숨기기
            if (bossNameText != null)
                bossNameText.gameObject.SetActive(false);

            if (bossSubtitleText != null)
                bossSubtitleText.gameObject.SetActive(false);

            if (bossIconImage != null)
                bossIconImage.gameObject.SetActive(false);
        }


        // ====== 보스 스폰 ======

        private BaseBoss SpawnBoss(BossData bossData, GameObject prefab, Transform spawnPoint)
        {
            if (prefab == null || spawnPoint == null)
            {
                Debug.LogError("[BossIntroController] prefab 또는 spawnPoint가 null입니다.");
                return null;
            }

            GameObject bossObj = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
            BaseBoss boss = bossObj.GetComponent<BaseBoss>();

            if (boss != null)
            {
                boss.InitializeWithData(bossData);
            }
            else
            {
                Debug.LogError("[BossIntroController] 스폰된 프리팹에 BaseBoss가 없습니다.");
            }

            return boss;
        }


        // ====== 플레이어 조작 ======

        private void LockPlayerControl(bool locked)
        {
            // PlayerController 또는 InputManager와 연동
            // 현재는 로그만 출력
            Debug.Log($"[BossIntroController] 플레이어 조작 {(locked ? "잠금" : "해제")}");

            // TODO: 실제 플레이어 조작 잠금 구현
            // if (PlayerController.Instance != null)
            //     PlayerController.Instance.SetInputEnabled(!locked);
        }


        // ====== UI 제어 ======

        private void HideImmediate()
        {
            if (introPanel != null)
                introPanel.SetActive(false);

            if (fadeOverlay != null)
                fadeOverlay.alpha = 0f;

            if (bossNameText != null)
                bossNameText.gameObject.SetActive(false);

            if (bossSubtitleText != null)
                bossSubtitleText.gameObject.SetActive(false);

            if (bossIconImage != null)
                bossIconImage.gameObject.SetActive(false);
        }


        // ====== 프로퍼티 ======

        public bool IsPlaying => isPlaying;
        public BaseBoss CurrentBoss => currentBoss;
    }
}
