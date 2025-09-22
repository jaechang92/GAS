using UnityEngine;

namespace Player
{
    /// <summary>
    /// 플레이어 컨트롤러 테스트 및 디버깅 도구
    /// </summary>
    public class PlayerControllerTest : MonoBehaviour
    {
        [Header("테스트 대상")]
        [SerializeField] private PlayerController playerController;

        [Header("테스트 설정")]
        [SerializeField] private bool enableDebugGUI = true;
        [SerializeField] private bool enableStateLogging = true;
        [SerializeField] private bool enableInputLogging = false;

        [Header("강제 상태 테스트")]
        [SerializeField] private PlayerStateType forceState = PlayerStateType.Idle;

        private void Start()
        {
            // PlayerController 자동 찾기
            if (playerController == null)
            {
                playerController = Object.FindAnyObjectByType<PlayerController>();
            }

            if (playerController != null)
            {
                // 이벤트 구독
                playerController.OnStateChanged += OnPlayerStateChanged;
                playerController.OnPlayerEvent += OnPlayerEvent;
            }
            else
            {
                Debug.LogError("[PlayerControllerTest] PlayerController를 찾을 수 없습니다!");
            }
        }

        private void Update()
        {
            HandleTestInputs();
        }

        private void HandleTestInputs()
        {
            // F 키로 강제 상태 전환 테스트
            if (Input.GetKeyDown(KeyCode.F))
            {
                ForceStateTransition(forceState);
            }

            // G 키로 데미지 테스트
            if (Input.GetKeyDown(KeyCode.G))
            {
                TestTakeDamage();
            }

            // H 키로 죽음 테스트
            if (Input.GetKeyDown(KeyCode.H))
            {
                TestDeath();
            }

            // J 키로 리스폰 테스트
            if (Input.GetKeyDown(KeyCode.J))
            {
                TestRespawn();
            }

            // K 키로 현재 상태 정보 출력
            if (Input.GetKeyDown(KeyCode.K))
            {
                PrintCurrentStateInfo();
            }

            // 입력 로깅
            if (enableInputLogging)
            {
                LogInputs();
            }
        }

        private void OnPlayerStateChanged(PlayerStateType fromState, PlayerStateType toState)
        {
            if (enableStateLogging)
            {
                Debug.Log($"[PlayerTest] 상태 변경: {fromState} → {toState}");
            }
        }

        private void OnPlayerEvent(PlayerEventType eventType)
        {
            if (enableStateLogging)
            {
                Debug.Log($"[PlayerTest] 이벤트: {eventType}");
            }
        }

        private void LogInputs()
        {
            if (playerController == null) return;

            Vector2 input = playerController.GetInputVector();
            if (input != Vector2.zero)
            {
                Debug.Log($"[PlayerTest] 입력: {input}");
            }
        }

        private void ForceStateTransition(PlayerStateType targetState)
        {
            if (playerController == null) return;

            Debug.Log($"[PlayerTest] 강제 상태 전환: {targetState}");
            // 직접 상태머신에 접근하여 강제 전환
            // 실제 게임에서는 이런 방식보다 이벤트를 통한 전환을 권장
        }

        private void TestTakeDamage()
        {
            if (playerController == null) return;

            Debug.Log("[PlayerTest] 데미지 테스트");
            playerController.TriggerEvent(PlayerEventType.TakeDamage);
        }

        private void TestDeath()
        {
            if (playerController == null) return;

            Debug.Log("[PlayerTest] 죽음 테스트");
            playerController.TriggerEvent(PlayerEventType.Die);
        }

        private void TestRespawn()
        {
            if (playerController == null) return;

            Debug.Log("[PlayerTest] 리스폰 테스트");
            playerController.TriggerEvent(PlayerEventType.Respawn);
        }

        private void PrintCurrentStateInfo()
        {
            if (playerController == null) return;

            Debug.Log("=== 플레이어 상태 정보 ===");
            Debug.Log($"현재 상태: {playerController.CurrentState}");
            Debug.Log($"접지 상태: {playerController.IsGrounded}");
            Debug.Log($"벽 접촉: {playerController.IsTouchingWall}");
            Debug.Log($"대시 가능: {playerController.CanDash}");
            Debug.Log($"방향: {(playerController.FacingDirection > 0 ? "오른쪽" : "왼쪽")}");
            Debug.Log($"속도: {playerController.Velocity}");
        }

        // 컨텍스트 메뉴 테스트들
        [ContextMenu("Idle로 전환")]
        private void TestIdleState()
        {
            ForceStateTransition(PlayerStateType.Idle);
        }

        [ContextMenu("Jump로 전환")]
        private void TestJumpState()
        {
            ForceStateTransition(PlayerStateType.Jump);
        }

        [ContextMenu("Dash로 전환")]
        private void TestDashState()
        {
            ForceStateTransition(PlayerStateType.Dash);
        }

        [ContextMenu("Attack로 전환")]
        private void TestAttackState()
        {
            ForceStateTransition(PlayerStateType.Attack);
        }

        [ContextMenu("데미지 받기")]
        private void ContextTestTakeDamage()
        {
            TestTakeDamage();
        }

        [ContextMenu("죽음")]
        private void ContextTestDeath()
        {
            TestDeath();
        }

        [ContextMenu("현재 상태 정보")]
        private void ContextPrintStateInfo()
        {
            PrintCurrentStateInfo();
        }

        // GUI 디버그 정보
        private void OnGUI()
        {
            if (!enableDebugGUI || playerController == null) return;

            GUILayout.BeginArea(new Rect(10, 10, 300, 400));
            GUILayout.Label("=== Player Controller Debug ===");

            GUILayout.Label($"State: {playerController.CurrentState}");
            GUILayout.Label($"Grounded: {playerController.IsGrounded}");
            GUILayout.Label($"Touching Wall: {playerController.IsTouchingWall}");
            GUILayout.Label($"Can Dash: {playerController.CanDash}");
            GUILayout.Label($"Facing: {(playerController.FacingDirection > 0 ? "Right" : "Left")}");
            GUILayout.Label($"Velocity: {playerController.Velocity}");

            GUILayout.Space(10);
            GUILayout.Label("=== Test Controls ===");
            GUILayout.Label("F: Force State Transition");
            GUILayout.Label("G: Take Damage");
            GUILayout.Label("H: Death");
            GUILayout.Label("J: Respawn");
            GUILayout.Label("K: Print State Info");

            GUILayout.Space(10);
            GUILayout.Label("=== Movement Controls ===");
            GUILayout.Label("Arrow Keys: Move");
            GUILayout.Label("Space: Jump");
            GUILayout.Label("Shift: Dash");
            GUILayout.Label("X / Mouse: Attack");
            GUILayout.Label("S + Move: Slide");

            GUILayout.EndArea();
        }

        private void OnDestroy()
        {
            if (playerController != null)
            {
                playerController.OnStateChanged -= OnPlayerStateChanged;
                playerController.OnPlayerEvent -= OnPlayerEvent;
            }
        }
    }
}