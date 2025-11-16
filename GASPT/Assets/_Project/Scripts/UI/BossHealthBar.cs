using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GASPT.Gameplay.Enemy;

namespace GASPT.UI
{
    /// <summary>
    /// 보스 체력바 UI (Boss 적 전용)
    /// 화면 상단에 보스 이름과 체력바 표시
    /// </summary>
    public class BossHealthBar : MonoBehaviour
    {
        // ====== UI 요소 ======

        [Header("UI 요소")]
        [SerializeField] [Tooltip("보스 이름 텍스트")]
        private TextMeshProUGUI bossNameText;

        [SerializeField] [Tooltip("체력바 이미지 (Fill)")]
        private Image healthBarFill;

        [SerializeField] [Tooltip("체력 텍스트 (현재 HP / 최대 HP)")]
        private TextMeshProUGUI healthText;


        // ====== 설정 ======

        [Header("설정")]
        [SerializeField] [Tooltip("체력바 색상 (건강할 때)")]
        private Color healthyColor = Color.green;

        [SerializeField] [Tooltip("체력바 색상 (위험할 때, HP < 30%)")]
        private Color dangerColor = Color.red;

        [SerializeField] [Tooltip("체력바 애니메이션 속도")]
        private float fillSpeed = 5f;


        // ====== Enemy 참조 ======

        private Enemy targetEnemy;
        private float targetFillAmount = 1f;


        // ====== Unity 생명주기 ======

        private void Awake()
        {
            ValidateReferences();

            // 초기에는 비활성화
            gameObject.SetActive(false);
        }

        private void Update()
        {
            // 체력바 부드럽게 애니메이션
            if (healthBarFill != null)
            {
                healthBarFill.fillAmount = Mathf.Lerp(healthBarFill.fillAmount, targetFillAmount, Time.deltaTime * fillSpeed);
            }
        }


        // ====== 초기화 ======

        /// <summary>
        /// 보스 체력바 초기화
        /// </summary>
        /// <param name="enemy">대상 보스</param>
        public void Initialize(Enemy enemy)
        {
            if (enemy == null)
            {
                Debug.LogError("[BossHealthBar] Initialize(): enemy가 null입니다.");
                return;
            }

            targetEnemy = enemy;

            // 보스 이름 설정
            if (bossNameText != null && enemy.Data != null)
            {
                bossNameText.text = enemy.Data.enemyName;
            }

            // 체력 초기화
            UpdateHealthBar(enemy.CurrentHp, enemy.MaxHp);

            // HP 변경 이벤트 구독
            targetEnemy.OnHpChanged += OnBossHpChanged;

            // 사망 이벤트 구독
            targetEnemy.OnDeath += OnBossDeath;

            // UI 활성화
            gameObject.SetActive(true);

            Debug.Log($"[BossHealthBar] {enemy.Data?.enemyName} 체력바 초기화 완료");
        }


        // ====== 이벤트 핸들러 ======

        /// <summary>
        /// 보스 HP 변경 시 체력바 업데이트
        /// </summary>
        private void OnBossHpChanged(int currentHp, int maxHp)
        {
            UpdateHealthBar(currentHp, maxHp);
        }

        /// <summary>
        /// 보스 사망 시 체력바 제거
        /// </summary>
        private void OnBossDeath(Enemy enemy)
        {
            Debug.Log($"[BossHealthBar] {enemy.Data?.enemyName} 사망으로 체력바 제거");

            // 체력바 숨김
            gameObject.SetActive(false);

            // GameObject 파괴 (1초 후)
            Destroy(gameObject, 1f);
        }


        // ====== UI 업데이트 ======

        /// <summary>
        /// 체력바 업데이트
        /// </summary>
        /// <param name="currentHp">현재 HP</param>
        /// <param name="maxHp">최대 HP</param>
        private void UpdateHealthBar(int currentHp, int maxHp)
        {
            if (maxHp <= 0)
            {
                Debug.LogWarning("[BossHealthBar] maxHp가 0 이하입니다.");
                return;
            }

            // Fill Amount 계산
            targetFillAmount = (float)currentHp / maxHp;

            // 체력 텍스트 업데이트
            if (healthText != null)
            {
                healthText.text = $"{currentHp} / {maxHp}";
            }

            // 체력바 색상 변경 (HP < 30%일 때 빨간색)
            if (healthBarFill != null)
            {
                if (targetFillAmount < 0.3f)
                {
                    healthBarFill.color = dangerColor;
                }
                else
                {
                    healthBarFill.color = healthyColor;
                }
            }
        }


        // ====== 유효성 검증 ======

        /// <summary>
        /// 필수 참조 유효성 검증
        /// </summary>
        private void ValidateReferences()
        {
            if (bossNameText == null)
            {
                Debug.LogError("[BossHealthBar] bossNameText가 null입니다. Inspector에서 설정하세요.");
            }

            if (healthBarFill == null)
            {
                Debug.LogError("[BossHealthBar] healthBarFill이 null입니다. Inspector에서 설정하세요.");
            }

            if (healthText == null)
            {
                Debug.LogWarning("[BossHealthBar] healthText가 null입니다. (선택 사항)");
            }
        }


        // ====== 정리 ======

        private void OnDestroy()
        {
            // 이벤트 구독 해제
            if (targetEnemy != null)
            {
                targetEnemy.OnHpChanged -= OnBossHpChanged;
                targetEnemy.OnDeath -= OnBossDeath;
            }
        }
    }
}
