using System.Threading;
using UnityEngine;
using TMPro;

namespace GASPT.UI
{
    /// <summary>
    /// 데미지 숫자 표시 UI (World Space)
    /// 공용 Canvas를 사용하여 최적화
    /// </summary>
    public class DamageNumber : MonoBehaviour
    {
        // ====== UI 요소 ======

        [Header("UI 요소")]
        [SerializeField] [Tooltip("데미지 텍스트")]
        private TextMeshProUGUI damageText;


        // ====== 애니메이션 설정 ======

        [Header("애니메이션 설정")]
        [SerializeField] [Tooltip("부유 속도 (위로 올라가는 속도)")]
        private float floatSpeed = 2f;

        [SerializeField] [Tooltip("페이드 속도")]
        private float fadeSpeed = 1f;

        [SerializeField] [Tooltip("애니메이션 지속 시간")]
        private float duration = 1.5f;

        [SerializeField] [Tooltip("랜덤 오프셋 범위 (X축)")]
        private float randomOffsetX = 0.5f;


        // ====== 색상 설정 ======

        [Header("색상 설정")]
        [SerializeField] [Tooltip("일반 데미지 색상")]
        private Color normalDamageColor = Color.white;

        [SerializeField] [Tooltip("크리티컬 데미지 색상")]
        private Color criticalDamageColor = Color.yellow;

        [SerializeField] [Tooltip("회복 색상")]
        private Color healColor = Color.green;

        [SerializeField] [Tooltip("경험치 색상")]
        private Color expColor = new Color(0.5f, 0.8f, 1f); // 밝은 파란색


        // ====== 내부 상태 ======

        private Vector3 startPosition;
        private CancellationTokenSource animationCts;


        // ====== Unity 생명주기 ======

        private void Awake()
        {
            ValidateReferences();
        }

        private void OnDisable()
        {
            CancelAnimation();
        }

        private void OnDestroy()
        {
            CancelAnimation();
        }

        private void CancelAnimation()
        {
            if (animationCts != null)
            {
                animationCts.Cancel();
                animationCts.Dispose();
                animationCts = null;
            }
        }


        // ====== 초기화 ======

        /// <summary>
        /// UI 참조 유효성 검증
        /// </summary>
        private void ValidateReferences()
        {
            if (damageText == null)
            {
                Debug.LogError("[DamageNumber] damageText가 설정되지 않았습니다.");
            }
        }


        // ====== Public 메서드 ======

        /// <summary>
        /// 데미지 숫자 표시
        /// </summary>
        /// <param name="damage">데미지 값</param>
        /// <param name="position">표시 위치 (World Space)</param>
        /// <param name="isCritical">크리티컬 여부</param>
        public void ShowDamage(int damage, Vector3 position, bool isCritical = false)
        {
            Show(damage.ToString(), position, isCritical ? criticalDamageColor : normalDamageColor, isCritical);
        }

        /// <summary>
        /// 회복 숫자 표시
        /// </summary>
        /// <param name="healAmount">회복량</param>
        /// <param name="position">표시 위치 (World Space)</param>
        public void ShowHeal(int healAmount, Vector3 position)
        {
            Show($"+{healAmount}", position, healColor, false);
        }

        /// <summary>
        /// 경험치 획득 표시
        /// </summary>
        /// <param name="exp">경험치 값</param>
        /// <param name="position">표시 위치 (World Space)</param>
        public void ShowExp(int exp, Vector3 position)
        {
            Show($"+{exp} EXP", position, expColor, false);
        }

        /// <summary>
        /// 커스텀 텍스트 표시
        /// </summary>
        /// <param name="text">표시할 텍스트</param>
        /// <param name="position">표시 위치 (World Space)</param>
        /// <param name="color">텍스트 색상</param>
        public void ShowText(string text, Vector3 position, Color color)
        {
            Show(text, position, color, false);
        }


        // ====== 내부 메서드 ======

        /// <summary>
        /// 텍스트 표시 및 애니메이션 시작
        /// </summary>
        private void Show(string text, Vector3 position, Color color, bool isCritical)
        {
            // 텍스트 설정
            if (damageText != null)
            {
                damageText.text = text;
                damageText.color = color;

                // 크리티컬이면 크기 증가
                damageText.fontSize = isCritical ? 48 : 36;
            }

            // 위치 설정 (랜덤 오프셋 추가)
            float randomX = Random.Range(-randomOffsetX, randomOffsetX);
            startPosition = position + new Vector3(randomX, 0, 0);
            transform.position = startPosition;

            // 애니메이션 시작
            CancelAnimation();
            animationCts = new CancellationTokenSource();
            _ = FloatAndFadeAsync(animationCts.Token);
        }

        /// <summary>
        /// 부유 및 페이드 애니메이션 (Awaitable)
        /// </summary>
        private async Awaitable FloatAndFadeAsync(CancellationToken cancellationToken)
        {
            float elapsed = 0f;
            Color startColor = damageText.color;

            while (elapsed < duration)
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                // 위로 부유
                transform.position = startPosition + Vector3.up * (floatSpeed * elapsed);

                // 페이드 아웃
                if (damageText != null)
                {
                    Color newColor = startColor;
                    newColor.a = Mathf.Lerp(1f, 0f, t * fadeSpeed);
                    damageText.color = newColor;
                }

                await Awaitable.NextFrameAsync(cancellationToken);
            }

            // 애니메이션 종료 후 비활성화 (풀로 반환)
            if (!cancellationToken.IsCancellationRequested)
            {
                gameObject.SetActive(false);
            }
        }


        // ====== Context Menu (테스트용) ======

        [ContextMenu("Test Show Damage")]
        private void TestShowDamage()
        {
            ShowDamage(100, transform.position, false);
        }

        [ContextMenu("Test Show Critical Damage")]
        private void TestShowCriticalDamage()
        {
            ShowDamage(250, transform.position, true);
        }

        [ContextMenu("Test Show Heal")]
        private void TestShowHeal()
        {
            ShowHeal(50, transform.position);
        }

        [ContextMenu("Test Show EXP")]
        private void TestShowExp()
        {
            ShowExp(100, transform.position);
        }
    }
}
