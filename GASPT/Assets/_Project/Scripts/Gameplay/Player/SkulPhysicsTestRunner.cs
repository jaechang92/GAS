using UnityEngine;
using Player.Physics;
using System.Collections;

namespace Player
{
    /// <summary>
    /// Skul 스타일 물리 시스템 테스트 러너
    /// 다양한 시나리오로 물리 시스템을 검증
    /// </summary>
    public class SkulPhysicsTestRunner : MonoBehaviour
    {
        [Header("테스트 설정")]
        [SerializeField] private bool runTestsOnStart = false;
        [SerializeField] private bool enableDebugLogs = true;
        [SerializeField] private float testInterval = 2f;

        [Header("테스트 대상")]
        [SerializeField] private CharacterPhysics characterPhysics;
        [SerializeField] private PlayerController playerController;

        private bool isTestingRunning = false;

        private void Start()
        {
            if (runTestsOnStart)
            {
                StartCoroutine(RunAllTests());
            }
        }

        private void Update()
        {
            // 키보드 입력으로 개별 테스트 실행
            if (Input.GetKeyDown(KeyCode.Alpha1)) TestBasicMovement();
            if (Input.GetKeyDown(KeyCode.Alpha2)) TestJumpMechanics();
            if (Input.GetKeyDown(KeyCode.Alpha3)) TestDashSystem();
            if (Input.GetKeyDown(KeyCode.Alpha4)) TestWallInteraction();
            if (Input.GetKeyDown(KeyCode.Alpha5)) TestGravityAndFalling();
            if (Input.GetKeyDown(KeyCode.Alpha6)) StartCoroutine(TestCoyoteTimeAndJumpBuffer());
            if (Input.GetKeyDown(KeyCode.Alpha0)) StartCoroutine(RunAllTests());
        }

        /// <summary>
        /// 모든 테스트 실행
        /// </summary>
        public IEnumerator RunAllTests()
        {
            if (isTestingRunning) yield break;
            isTestingRunning = true;

            LogTest("=== Skul 물리 시스템 전체 테스트 시작 ===");

            yield return StartCoroutine(TestBasicMovementCoroutine());
            yield return new WaitForSeconds(testInterval);

            yield return StartCoroutine(TestJumpMechanicsCoroutine());
            yield return new WaitForSeconds(testInterval);

            yield return StartCoroutine(TestDashSystemCoroutine());
            yield return new WaitForSeconds(testInterval);

            yield return StartCoroutine(TestWallInteractionCoroutine());
            yield return new WaitForSeconds(testInterval);

            yield return StartCoroutine(TestGravityAndFallingCoroutine());
            yield return new WaitForSeconds(testInterval);

            yield return StartCoroutine(TestCoyoteTimeAndJumpBuffer());
            yield return new WaitForSeconds(testInterval);

            LogTest("=== 모든 테스트 완료 ===");
            isTestingRunning = false;
        }

        #region 개별 테스트 메서드

        /// <summary>
        /// 기본 이동 테스트
        /// </summary>
        public void TestBasicMovement()
        {
            StartCoroutine(TestBasicMovementCoroutine());
        }

        private IEnumerator TestBasicMovementCoroutine()
        {
            LogTest("--- 기본 이동 테스트 시작 ---");

            // 오른쪽 이동 테스트
            LogTest("오른쪽 이동 테스트");
            characterPhysics.SetHorizontalInput(1f);
            yield return new WaitForSeconds(1f);

            float rightSpeed = characterPhysics.Velocity.x;
            LogTest($"오른쪽 이동 속도: {rightSpeed:F2} (목표: {GetConfig()?.moveSpeed ?? 12f})");

            // 정지 테스트
            LogTest("정지 테스트");
            characterPhysics.SetHorizontalInput(0f);
            yield return new WaitForSeconds(0.5f);

            float stopSpeed = Mathf.Abs(characterPhysics.Velocity.x);
            LogTest($"정지 후 속도: {stopSpeed:F2} (목표: 거의 0)");

            // 왼쪽 이동 테스트
            LogTest("왼쪽 이동 테스트");
            characterPhysics.SetHorizontalInput(-1f);
            yield return new WaitForSeconds(1f);

            float leftSpeed = characterPhysics.Velocity.x;
            LogTest($"왼쪽 이동 속도: {leftSpeed:F2} (목표: -{GetConfig()?.moveSpeed ?? 12f})");

            // 입력 리셋
            characterPhysics.SetHorizontalInput(0f);
            LogTest("--- 기본 이동 테스트 완료 ---");
        }

        /// <summary>
        /// 점프 메커니즘 테스트
        /// </summary>
        public void TestJumpMechanics()
        {
            StartCoroutine(TestJumpMechanicsCoroutine());
        }

        private IEnumerator TestJumpMechanicsCoroutine()
        {
            LogTest("--- 점프 메커니즘 테스트 시작 ---");

            // 기본 점프 테스트
            LogTest("기본 점프 테스트");
            if (characterPhysics.IsGrounded)
            {
                characterPhysics.SetJumpInput(true, true);
                yield return new WaitForFixedUpdate();

                float jumpVelocity = characterPhysics.Velocity.y;
                LogTest($"점프 속도: {jumpVelocity:F2} (목표: {GetConfig()?.jumpVelocity ?? 16f})");

                characterPhysics.SetJumpInput(false, false);
            }
            else
            {
                LogTest("캐릭터가 접지 상태가 아닙니다.");
            }

            yield return new WaitForSeconds(1f);

            // 짧은 점프 테스트 (착지 후)
            yield return new WaitUntil(() => characterPhysics.IsGrounded);
            LogTest("짧은 점프 테스트");

            characterPhysics.SetJumpInput(true, true);
            yield return new WaitForFixedUpdate();
            yield return new WaitForSeconds(0.1f); // 짧게 누르기
            characterPhysics.SetJumpInput(false, false);

            float shortJumpPeak = 0f;
            while (characterPhysics.Velocity.y > 0)
            {
                shortJumpPeak = Mathf.Max(shortJumpPeak, characterPhysics.Velocity.y);
                yield return new WaitForFixedUpdate();
            }
            LogTest($"짧은 점프 최대 속도: {shortJumpPeak:F2}");

            LogTest("--- 점프 메커니즘 테스트 완료 ---");
        }

        /// <summary>
        /// 대시 시스템 테스트
        /// </summary>
        public void TestDashSystem()
        {
            StartCoroutine(TestDashSystemCoroutine());
        }

        private IEnumerator TestDashSystemCoroutine()
        {
            LogTest("--- 대시 시스템 테스트 시작 ---");

            // 오른쪽 대시 테스트
            LogTest("오른쪽 대시 테스트");
            if (characterPhysics.CanDash)
            {
                characterPhysics.PerformDash(Vector2.right);
                yield return new WaitForFixedUpdate();

                float dashSpeed = characterPhysics.Velocity.x;
                LogTest($"대시 속도: {dashSpeed:F2} (목표: {GetConfig()?.dashSpeed ?? 28f})");
                LogTest($"대시 상태: {characterPhysics.IsDashing}");

                // 대시 지속 시간 테스트
                float dashDuration = 0f;
                while (characterPhysics.IsDashing)
                {
                    dashDuration += Time.fixedDeltaTime;
                    yield return new WaitForFixedUpdate();
                }
                LogTest($"대시 지속 시간: {dashDuration:F2}초 (목표: {GetConfig()?.dashDuration ?? 0.15f}초)");
            }
            else
            {
                LogTest("대시를 사용할 수 없습니다 (쿨다운 중)");
            }

            yield return new WaitForSeconds(1f);
            LogTest("--- 대시 시스템 테스트 완료 ---");
        }

        /// <summary>
        /// 벽 상호작용 테스트
        /// </summary>
        public void TestWallInteraction()
        {
            StartCoroutine(TestWallInteractionCoroutine());
        }

        private IEnumerator TestWallInteractionCoroutine()
        {
            LogTest("--- 벽 상호작용 테스트 시작 ---");

            LogTest($"현재 벽 터치 상태: {characterPhysics.IsTouchingWall}");
            LogTest($"벽 방향: {characterPhysics.WallDirection}");

            if (characterPhysics.IsTouchingWall)
            {
                LogTest("벽 슬라이딩 테스트");
                // 벽 방향으로 이동 시도
                characterPhysics.SetHorizontalInput(characterPhysics.WallDirection);
                yield return new WaitForSeconds(0.5f);

                float slideSpeed = characterPhysics.Velocity.y;
                LogTest($"벽 슬라이딩 속도: {slideSpeed:F2} (목표: {GetConfig()?.wallSlideSpeed ?? -3f})");

                // 벽점프 테스트
                LogTest("벽점프 테스트");
                characterPhysics.SetJumpInput(true, true);
                yield return new WaitForFixedUpdate();
                characterPhysics.SetJumpInput(false, false);

                Vector2 wallJumpVel = characterPhysics.Velocity;
                LogTest($"벽점프 속도: {wallJumpVel} (목표 방향: 벽 반대편)");
            }
            else
            {
                LogTest("벽이 감지되지 않습니다. 벽 근처로 이동해주세요.");
            }

            characterPhysics.SetHorizontalInput(0f);
            LogTest("--- 벽 상호작용 테스트 완료 ---");
        }

        /// <summary>
        /// 중력과 낙하 테스트
        /// </summary>
        public void TestGravityAndFalling()
        {
            StartCoroutine(TestGravityAndFallingCoroutine());
        }

        private IEnumerator TestGravityAndFallingCoroutine()
        {
            LogTest("--- 중력과 낙하 테스트 시작 ---");

            // 공중으로 이동 (점프나 텔레포트)
            if (characterPhysics.IsGrounded)
            {
                characterPhysics.SetVelocity(new Vector2(0, 5f)); // 공중으로 띄우기
                yield return new WaitForSeconds(0.2f);
            }

            LogTest("낙하 중력 테스트");
            float fallStartY = transform.position.y;
            float maxFallSpeed = 0f;

            while (!characterPhysics.IsGrounded && transform.position.y > fallStartY - 5f)
            {
                float currentFallSpeed = Mathf.Abs(characterPhysics.Velocity.y);
                maxFallSpeed = Mathf.Max(maxFallSpeed, currentFallSpeed);
                yield return new WaitForFixedUpdate();
            }

            LogTest($"최대 낙하 속도: {maxFallSpeed:F2} (목표 제한: {GetConfig()?.maxFallSpeed ?? 22f})");
            LogTest("--- 중력과 낙하 테스트 완료 ---");
        }

        /// <summary>
        /// 코요테 타임과 점프 버퍼 테스트
        /// </summary>
        public IEnumerator TestCoyoteTimeAndJumpBuffer()
        {
            LogTest("--- 코요테 타임과 점프 버퍼 테스트 시작 ---");

            // 코요테 타임 테스트
            if (characterPhysics.IsGrounded)
            {
                LogTest("코요테 타임 테스트");

                // 바닥에서 살짝 떨어뜨리기
                characterPhysics.SetVelocity(new Vector2(1f, 1f));
                yield return new WaitForSeconds(0.05f); // 코요테 타임 내에서

                // 점프 시도
                characterPhysics.SetJumpInput(true, true);
                yield return new WaitForFixedUpdate();

                bool coyoteJumpSuccess = characterPhysics.Velocity.y > 0;
                LogTest($"코요테 타임 점프 성공: {coyoteJumpSuccess}");

                characterPhysics.SetJumpInput(false, false);
            }

            yield return new WaitUntil(() => characterPhysics.IsGrounded);

            // 점프 버퍼 테스트
            LogTest("점프 버퍼 테스트");

            // 공중에서 점프 입력
            characterPhysics.SetVelocity(new Vector2(0, 2f)); // 공중으로
            yield return new WaitForSeconds(0.1f);

            characterPhysics.SetJumpInput(true, true); // 공중에서 점프 입력
            yield return new WaitForSeconds(0.05f);
            characterPhysics.SetJumpInput(false, false);

            // 착지 후 자동 점프 확인
            yield return new WaitUntil(() => characterPhysics.IsGrounded);
            yield return new WaitForSeconds(0.1f);

            bool bufferJumpSuccess = characterPhysics.Velocity.y > 0;
            LogTest($"점프 버퍼 자동 실행: {bufferJumpSuccess}");

            LogTest("--- 코요테 타임과 점프 버퍼 테스트 완료 ---");
        }

        #endregion

        #region 유틸리티 메서드

        private SkulPhysicsConfig GetConfig()
        {
            // CharacterPhysics에서 설정 정보 가져오기 (reflection 사용 또는 public 프로퍼티 추가 필요)
            // 임시로 기본값 반환
            return null;
        }

        private void LogTest(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[SkulPhysicsTest] {message}");
            }
        }

        #endregion

        #region 에디터 전용 메서드

        [ContextMenu("기본 이동 테스트")]
        private void EditorTestBasicMovement() => TestBasicMovement();

        [ContextMenu("점프 테스트")]
        private void EditorTestJump() => TestJumpMechanics();

        [ContextMenu("대시 테스트")]
        private void EditorTestDash() => TestDashSystem();

        [ContextMenu("벽 상호작용 테스트")]
        private void EditorTestWall() => TestWallInteraction();

        [ContextMenu("중력 테스트")]
        private void EditorTestGravity() => TestGravityAndFalling();

        [ContextMenu("모든 테스트 실행")]
        private void EditorRunAllTests() => StartCoroutine(RunAllTests());

        #endregion

        #region 디버그 정보 표시

        private void OnGUI()
        {
            if (!enableDebugLogs) return;

            GUILayout.BeginArea(new Rect(10, 10, 300, 400));
            GUILayout.Label("=== Skul 물리 시스템 테스트 ===", new GUIStyle(GUI.skin.label) { fontSize = 16, fontStyle = FontStyle.Bold });

            if (characterPhysics != null)
            {
                GUILayout.Label($"접지 상태: {characterPhysics.IsGrounded}");
                GUILayout.Label($"속도: {characterPhysics.Velocity:F2}");
                GUILayout.Label($"벽 터치: {characterPhysics.IsTouchingWall}");
                GUILayout.Label($"벽 방향: {characterPhysics.WallDirection}");
                GUILayout.Label($"대시 가능: {characterPhysics.CanDash}");
                GUILayout.Label($"대시 중: {characterPhysics.IsDashing}");
                GUILayout.Label($"점프 가능: {characterPhysics.CanJump}");
            }

            GUILayout.Space(10);
            GUILayout.Label("=== 키보드 테스트 ===");
            GUILayout.Label("1: 기본 이동");
            GUILayout.Label("2: 점프 시스템");
            GUILayout.Label("3: 대시 시스템");
            GUILayout.Label("4: 벽 상호작용");
            GUILayout.Label("5: 중력/낙하");
            GUILayout.Label("6: 코요테/버퍼");
            GUILayout.Label("0: 전체 테스트");

            GUILayout.EndArea();
        }

        #endregion
    }
}