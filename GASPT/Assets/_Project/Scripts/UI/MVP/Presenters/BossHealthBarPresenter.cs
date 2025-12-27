using UnityEngine;
using GASPT.Gameplay.Boss;
using GASPT.Data;

namespace GASPT.UI.MVP
{
    /// <summary>
    /// 보스 체력바 Presenter
    /// BaseBoss와 IBossHealthBarView를 연결
    /// </summary>
    public class BossHealthBarPresenter : MonoBehaviour
    {
        // ====== View 참조 ======

        [Header("View")]
        [SerializeField]
        private BossHealthBarView view;


        // ====== 설정 ======

        [Header("설정")]
        [SerializeField]
        [Tooltip("체력 변경 애니메이션 시간")]
        private float healthAnimationDuration = 0.3f;

        [SerializeField]
        [Tooltip("시간 제한 표시 여부")]
        private bool showTimeLimit = true;


        // ====== 상태 ======

        private BaseBoss currentBoss;
        private float combatStartTime;
        private float timeLimit;


        // ====== 싱글톤 ======

        private static BossHealthBarPresenter instance;
        public static BossHealthBarPresenter Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindAnyObjectByType<BossHealthBarPresenter>();
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

            if (view == null)
                view = GetComponent<BossHealthBarView>();
        }

        private void OnDestroy()
        {
            if (instance == this)
                instance = null;

            UnsubscribeFromBoss();
        }

        private void Update()
        {
            UpdateTimeLimit();
        }


        // ====== 보스 바인딩 ======

        /// <summary>
        /// 보스 바인딩 및 UI 표시
        /// </summary>
        public void BindBoss(BaseBoss boss)
        {
            if (boss == null)
            {
                Debug.LogWarning("[BossHealthBarPresenter] boss가 null입니다.");
                return;
            }

            // 이전 보스 구독 해제
            UnsubscribeFromBoss();

            currentBoss = boss;

            // 이벤트 구독
            SubscribeToBoss();

            // View 초기화
            if (view != null && boss.Data != null)
            {
                view.Show(boss.Data.bossName, boss.TotalPhases);

                // 시간 제한 설정
                timeLimit = boss.Data.timeLimit;
                combatStartTime = Time.time;
            }

            Debug.Log($"[BossHealthBarPresenter] 보스 바인딩: {boss.Data?.bossName}");
        }

        /// <summary>
        /// 보스 바인딩 해제
        /// </summary>
        public void UnbindBoss()
        {
            UnsubscribeFromBoss();
            currentBoss = null;

            if (view != null)
                view.Hide();
        }


        // ====== 이벤트 구독 ======

        private void SubscribeToBoss()
        {
            if (currentBoss == null) return;

            currentBoss.OnHpChanged += HandleHpChanged;
            currentBoss.OnPhaseChanged += HandlePhaseChanged;
            currentBoss.OnBossDefeated += HandleBossDefeated;
            currentBoss.OnInvulnerableChanged += HandleInvulnerableChanged;
        }

        private void UnsubscribeFromBoss()
        {
            if (currentBoss == null) return;

            currentBoss.OnHpChanged -= HandleHpChanged;
            currentBoss.OnPhaseChanged -= HandlePhaseChanged;
            currentBoss.OnBossDefeated -= HandleBossDefeated;
            currentBoss.OnInvulnerableChanged -= HandleInvulnerableChanged;
        }


        // ====== 이벤트 핸들러 ======

        private void HandleHpChanged(int currentHp, int maxHp)
        {
            if (view == null || maxHp <= 0) return;

            float ratio = (float)currentHp / maxHp;
            view.UpdateHealthAnimated(ratio, healthAnimationDuration);
        }

        private void HandlePhaseChanged(int currentPhase, int totalPhases)
        {
            if (view == null) return;

            view.UpdatePhase(currentPhase, totalPhases);
            view.PlayPhaseTransitionEffect(currentPhase);
        }

        private void HandleBossDefeated(BaseBoss boss)
        {
            // 보스 처치 시 UI 숨기기 (딜레이 후)
            HideWithDelay(2f);
        }

        private void HandleInvulnerableChanged(bool invulnerable)
        {
            if (view == null) return;

            view.SetInvulnerable(invulnerable);
        }


        // ====== 시간 제한 ======

        private void UpdateTimeLimit()
        {
            if (!showTimeLimit || currentBoss == null || timeLimit <= 0)
                return;

            if (view == null) return;

            float elapsed = Time.time - combatStartTime;
            float remaining = Mathf.Max(0f, timeLimit - elapsed);

            view.UpdateTimeLimit(remaining, remaining > 0);
        }


        // ====== 유틸리티 ======

        private async void HideWithDelay(float delay)
        {
            await Awaitable.WaitForSecondsAsync(delay);

            if (view != null)
                view.Hide();
        }


        // ====== 외부 API ======

        /// <summary>
        /// 현재 바인딩된 보스
        /// </summary>
        public BaseBoss CurrentBoss => currentBoss;

        /// <summary>
        /// UI 표시 중 여부
        /// </summary>
        public bool IsShowing => currentBoss != null;

        /// <summary>
        /// 수동 체력 업데이트 (테스트용)
        /// </summary>
        public void ForceUpdateHealth(float ratio)
        {
            if (view != null)
                view.UpdateHealth(ratio);
        }

        /// <summary>
        /// 수동 페이즈 업데이트 (테스트용)
        /// </summary>
        public void ForceUpdatePhase(int current, int total)
        {
            if (view != null)
                view.UpdatePhase(current, total);
        }
    }
}
