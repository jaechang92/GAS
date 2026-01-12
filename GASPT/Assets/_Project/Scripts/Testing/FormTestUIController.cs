using GASPT.UI.Forms;
using UnityEngine;
using UnityEngine.UI;

namespace GASPT.Gameplay.Form
{
    /// <summary>
    /// 폼 시스템 테스트용 UI 컨트롤러
    /// FormManager의 상태를 화면에 표시
    /// </summary>
    public class FormTestUIController : MonoBehaviour
    {
        [Header("UI 참조")]
        [SerializeField] private Text statusText;
        [SerializeField] private Image cooldownFill;
        [SerializeField] private Text cooldownText;

        private FormManager formManager;
        private FormUIManager formUIManager;

        private void Start()
        {
            // FormManager 찾기
            formManager = FindAnyObjectByType<FormManager>();

            if (formManager != null)
            {
                // 이벤트 구독
                formManager.OnFormSwapped += HandleFormSwapped;
                formManager.OnFormAcquired += HandleFormAcquired;
                formManager.OnSwapCooldownStarted += HandleCooldownStarted;
                formManager.OnSwapCooldownEnded += HandleCooldownEnded;
            }

            // UI 참조 자동 찾기
            AutoFindUIReferences();

            UpdateStatusDisplay();
        }

        private void OnDestroy()
        {
            if (formManager != null)
            {
                formManager.OnFormSwapped -= HandleFormSwapped;
                formManager.OnFormAcquired -= HandleFormAcquired;
                formManager.OnSwapCooldownStarted -= HandleCooldownStarted;
                formManager.OnSwapCooldownEnded -= HandleCooldownEnded;
            }
        }

        private void Update()
        {
            UpdateCooldownDisplay();
            UpdateStatusDisplay();
        }

        private void AutoFindUIReferences()
        {
            if (statusText == null)
            {
                var statusObj = transform.Find("StatusText");
                if (statusObj != null)
                    statusText = statusObj.GetComponent<Text>();
            }

            if (cooldownFill == null)
            {
                var cooldownUI = transform.Find("CooldownUI");
                if (cooldownUI != null)
                {
                    var fillObj = cooldownUI.Find("Fill");
                    if (fillObj != null)
                        cooldownFill = fillObj.GetComponent<Image>();

                    var textObj = cooldownUI.Find("Text");
                    if (textObj != null)
                        cooldownText = textObj.GetComponent<Text>();
                }
            }
        }

        private void HandleFormSwapped(FormInstance previous, FormInstance current)
        {
            Debug.Log($"[UI] 폼 교체: {previous?.FormName ?? "없음"} → {current?.FormName ?? "없음"}");
        }

        private void HandleFormAcquired(FormInstance form)
        {
            Debug.Log($"[UI] 폼 획득: {form.FormName} (등급: {form.CurrentRarity})");
        }

        private void HandleCooldownStarted(float duration)
        {
            Debug.Log($"[UI] 쿨다운 시작: {duration}초");
        }

        private void HandleCooldownEnded()
        {
            Debug.Log("[UI] 쿨다운 완료!");
        }

        private void UpdateCooldownDisplay()
        {
            if (formManager == null) return;

            if (cooldownFill != null)
            {
                cooldownFill.fillAmount = formManager.CooldownProgress;

                // 색상 변경
                if (formManager.IsOnCooldown)
                {
                    cooldownFill.color = new Color(0.5f, 0.5f, 0.5f, 1f);
                }
                else
                {
                    cooldownFill.color = new Color(0.3f, 0.7f, 1f, 1f);
                }
            }

            if (cooldownText != null)
            {
                if (formManager.IsOnCooldown)
                {
                    cooldownText.text = $"쿨다운: {formManager.RemainingCooldown:F1}s";
                }
                else if (formManager.CanSwap)
                {
                    cooldownText.text = "교체 준비 [Q]";
                }
                else
                {
                    cooldownText.text = "대기 폼 없음";
                }
            }
        }

        private void UpdateStatusDisplay()
        {
            if (statusText == null || formManager == null) return;

            var primary = formManager.CurrentForm;
            var secondary = formManager.ReserveForm;

            string status = "=== 폼 시스템 테스트 ===\n\n";

            status += $"[슬롯 1] {FormatFormInfo(primary)}\n";
            status += $"[슬롯 2] {FormatFormInfo(secondary)}\n\n";

            status += "--- 조작법 ---\n";
            status += "[Q] 폼 교체\n";
            status += "[F] 픽업 상호작용\n";
            status += "[방향키/WASD] 이동\n";
            status += "[Space] 점프\n\n";

            status += $"교체 가능: {(formManager.CanSwap ? "O" : "X")}\n";

            if (formManager.IsOnCooldown)
            {
                status += $"쿨다운: {formManager.RemainingCooldown:F1}s";
            }

            statusText.text = status;
        }

        private string FormatFormInfo(FormInstance form)
        {
            if (form == null)
                return "(비어있음)";

            return $"{form.FormName} [{form.FormType}] 각성Lv.{form.AwakeningLevel}";
        }
    }
}
