using UnityEngine;
using System.Threading;

namespace GASPT.Form
{
    /// <summary>
    /// 폼(Form) 입력 처리기
    /// 플레이어의 마우스/키보드 입력을 받아 폼의 스킬을 실행
    /// </summary>
    public class FormInputHandler : MonoBehaviour
    {
        // ====== 설정 ======

        [Header("Form Reference")]
        [Tooltip("연결할 폼 (MageForm 등) - null이면 자동 탐색")]
        [SerializeField] private BaseForm targetForm;


        [Header("키 바인딩")]
        [Tooltip("점프")]
        [SerializeField] private KeyCode jumpKey = KeyCode.Space;

        [Tooltip("기본 공격 (Magic Missile)")]
        [SerializeField] private KeyCode basicAttackKey = KeyCode.Mouse0;

        [Tooltip("스킬 1 (Teleport)")]
        [SerializeField] private KeyCode skill1Key = KeyCode.Q;

        [Tooltip("스킬 2 (Fireball)")]
        [SerializeField] private KeyCode skill2Key = KeyCode.E;


        [Header("디버그")]
        [SerializeField] private bool showDebugLogs = false;


        // ====== 상태 ======

        private CancellationTokenSource cts;


        // ====== Unity 생명주기 ======

        private void Awake()
        {
            // BaseForm 자동 탐색
            if (targetForm == null)
            {
                targetForm = GetComponent<BaseForm>();
            }

            if (targetForm == null)
            {
                Debug.LogError("[FormInputHandler] BaseForm 컴포넌트를 찾을 수 없습니다!");
            }

            // CancellationToken 생성
            cts = new CancellationTokenSource();
        }

        private void Update()
        {
            if (targetForm == null) return;

            HandleAbilityInput();
        }

        private void OnDestroy()
        {
            // CancellationToken 정리
            cts?.Cancel();
            cts?.Dispose();
        }


        // ====== 입력 처리 ======

        /// <summary>
        /// 스킬 입력 감지 및 실행
        /// </summary>
        private void HandleAbilityInput()
        {
            // Space키 - 점프
            if (Input.GetKeyDown(jumpKey))
            {
                ExecuteJumpAbility();
            }

            // 마우스 왼쪽 클릭 - 기본 공격 (슬롯 0)
            if (Input.GetMouseButtonDown(0))
            {
                ExecuteAbility(0, "기본 공격");
            }

            // Q키 - 스킬 1 (슬롯 1)
            if (Input.GetKeyDown(skill1Key))
            {
                ExecuteAbility(1, "스킬 1");
            }

            // E키 - 스킬 2 (슬롯 2)
            if (Input.GetKeyDown(skill2Key))
            {
                ExecuteAbility(2, "스킬 2");
            }
        }

        /// <summary>
        /// 점프 실행 (비동기)
        /// </summary>
        private async void ExecuteJumpAbility()
        {
            IAbility jumpAbility = targetForm.GetJumpAbility();

            if (jumpAbility == null)
            {
                if (showDebugLogs)
                    Debug.LogWarning("[FormInputHandler] 점프 어빌리티가 설정되지 않았습니다!");
                return;
            }

            try
            {
                await jumpAbility.ExecuteAsync(gameObject, cts.Token);
            }
            catch (System.OperationCanceledException)
            {
                // CancellationToken에 의한 취소 (정상 종료)
                if (showDebugLogs)
                    Debug.Log("[FormInputHandler] 점프 취소됨");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[FormInputHandler] 점프 실행 중 오류: {ex.Message}\n{ex.StackTrace}");
            }
        }

        /// <summary>
        /// 스킬 실행 (비동기)
        /// </summary>
        private async void ExecuteAbility(int slotIndex, string slotName)
        {
            IAbility ability = targetForm.GetAbility(slotIndex);

            if (ability == null)
            {
                if (showDebugLogs)
                    Debug.LogWarning($"[FormInputHandler] {slotName}에 스킬이 없습니다!");
                return;
            }

            if (showDebugLogs)
                Debug.Log($"[FormInputHandler] {ability.AbilityName} 실행 요청");

            try
            {
                await ability.ExecuteAsync(gameObject, cts.Token);
            }
            catch (System.OperationCanceledException)
            {
                // CancellationToken에 의한 취소 (정상 종료)
                if (showDebugLogs)
                    Debug.Log($"[FormInputHandler] {ability.AbilityName} 취소됨");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[FormInputHandler] {ability.AbilityName} 실행 중 오류: {ex.Message}\n{ex.StackTrace}");
            }
        }


        // ====== 디버그 ======

        [ContextMenu("Print Current Form")]
        private void PrintCurrentForm()
        {
            if (targetForm == null)
            {
                Debug.Log("[FormInputHandler] BaseForm이 할당되지 않았습니다!");
                return;
            }

            Debug.Log($"=== FormInputHandler ===\n" +
                     $"Form: {targetForm.FormName}\n" +
                     $"Type: {targetForm.FormType}\n" +
                     $"Abilities:\n" +
                     $"  [0] {targetForm.GetAbility(0)?.AbilityName ?? "Empty"}\n" +
                     $"  [1] {targetForm.GetAbility(1)?.AbilityName ?? "Empty"}\n" +
                     $"  [2] {targetForm.GetAbility(2)?.AbilityName ?? "Empty"}\n" +
                     $"Key Bindings:\n" +
                     $"  Basic Attack: {basicAttackKey}\n" +
                     $"  Skill 1: {skill1Key}\n" +
                     $"  Skill 2: {skill2Key}");
        }

        [ContextMenu("Test Basic Attack (Slot 0)")]
        private void TestBasicAttack()
        {
            if (Application.isPlaying)
            {
                ExecuteAbility(0, "기본 공격 테스트");
            }
            else
            {
                Debug.LogWarning("[FormInputHandler] Play 모드에서만 실행 가능합니다.");
            }
        }

        [ContextMenu("Test Skill 1 (Slot 1)")]
        private void TestSkill1()
        {
            if (Application.isPlaying)
            {
                ExecuteAbility(1, "스킬 1 테스트");
            }
            else
            {
                Debug.LogWarning("[FormInputHandler] Play 모드에서만 실행 가능합니다.");
            }
        }

        [ContextMenu("Test Skill 2 (Slot 2)")]
        private void TestSkill2()
        {
            if (Application.isPlaying)
            {
                ExecuteAbility(2, "스킬 2 테스트");
            }
            else
            {
                Debug.LogWarning("[FormInputHandler] Play 모드에서만 실행 가능합니다.");
            }
        }
    }
}
