using UnityEngine;
using Skull.Data;
using Skull.Core;
using Player;
using System.Threading;

namespace Skull.Core
{
    /// <summary>
    /// 스컬 교체 메인 시스템
    /// 플레이어 컨트롤러와 연동하여 스컬 시스템을 통합 관리
    /// </summary>
    [RequireComponent(typeof(SkullManager))]
    public class SkullSystem : MonoBehaviour
    {
        [Header("시스템 연동")]
        [SerializeField] private PlayerController playerController;
        [SerializeField] private bool enableInputHandling = true;
        [SerializeField] private bool enableDebugLogs = true;

        [Header("스컬 교체 설정")]
        [SerializeField] private float switchAnimationDuration = 0.3f;
        [SerializeField] private bool pauseGameplayDuringSwitch = true;

        // 컴포넌트 참조
        private SkullManager skullManager;
        private AnimationController animationController;
        private InputHandler inputHandler;

        // 상태 관리
        private bool isSystemActive = false;
        private bool isInitialized = false;

        #region Unity 생명주기

        private void Awake()
        {
            InitializeComponents();
        }

        private void Start()
        {
            InitializeSystem();
        }

        private void Update()
        {
            if (isSystemActive)
            {
                HandleSkullInput();
                UpdateSkullSystem();
            }
        }

        private void OnDestroy()
        {
            CleanupSystem();
        }

        #endregion

        #region 초기화

        /// <summary>
        /// 컴포넌트 참조 초기화
        /// </summary>
        private void InitializeComponents()
        {
            skullManager = GetComponent<SkullManager>();

            if (playerController == null)
                playerController = GetComponent<PlayerController>();

            if (playerController != null)
            {
                animationController = playerController.GetComponent<AnimationController>();
                inputHandler = playerController.GetComponent<InputHandler>();
            }

            LogDebug("컴포넌트 참조 초기화 완료");
        }

        /// <summary>
        /// 시스템 초기화
        /// </summary>
        private void InitializeSystem()
        {
            if (skullManager == null)
            {
                LogError("SkullManager가 없습니다.");
                return;
            }

            // 이벤트 구독
            SubscribeToEvents();

            // 플레이어 컨트롤러와 연동
            ConnectToPlayerController();

            isInitialized = true;
            isSystemActive = true;

            LogDebug("스컬 시스템 초기화 완료");
        }

        /// <summary>
        /// 이벤트 구독
        /// </summary>
        private void SubscribeToEvents()
        {
            if (skullManager != null)
            {
                skullManager.OnSkullChanged += OnSkullChanged;
                skullManager.OnSkullEquipped += OnSkullEquipped;
                skullManager.OnSkullUnequipped += OnSkullUnequipped;
            }
        }

        /// <summary>
        /// 플레이어 컨트롤러와 연동
        /// </summary>
        private void ConnectToPlayerController()
        {
            if (playerController == null) return;

            // 플레이어 컨트롤러에 스컬 시스템 참조 전달
            // (플레이어 컨트롤러에서 공격 입력 시 현재 스컬의 어빌리티 호출)

            LogDebug("플레이어 컨트롤러 연동 완료");
        }

        #endregion

        #region 입력 처리

        /// <summary>
        /// 스컬 관련 입력 처리
        /// </summary>
        private void HandleSkullInput()
        {
            if (!enableInputHandling || !isSystemActive) return;

            // 스컬 어빌리티 입력
            HandleAbilityInput();

            // 스컬 던지기 입력
            HandleSkullThrowInput();
        }

        /// <summary>
        /// 어빌리티 입력 처리
        /// </summary>
        private void HandleAbilityInput()
        {
            var currentSkull = skullManager.CurrentSkull;
            if (currentSkull == null || !currentSkull.IsActive) return;

            // 기본 공격 (좌클릭)
            if (Input.GetMouseButtonDown(0))
            {
                _ = currentSkull.PerformPrimaryAttack();
            }
            // 보조 공격 (우클릭)
            else if (Input.GetMouseButtonDown(1))
            {
                _ = currentSkull.PerformSecondaryAttack();
            }
            // 궁극기 (R키)
            else if (Input.GetKeyDown(KeyCode.R))
            {
                _ = currentSkull.PerformUltimate();
            }
        }

        /// <summary>
        /// 스컬 던지기 입력 처리
        /// </summary>
        private void HandleSkullThrowInput()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Vector2 throwDirection = GetThrowDirection();
                PerformSkullThrow(throwDirection);
            }
        }

        /// <summary>
        /// 던지기 방향 계산
        /// </summary>
        private Vector2 GetThrowDirection()
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0f;

            Vector2 direction = (mousePos - transform.position).normalized;
            return direction.magnitude > 0.1f ? direction : Vector2.right;
        }

        #endregion

        #region 공개 API

        /// <summary>
        /// 스컬 던지기 실행
        /// </summary>
        public async Awaitable PerformSkullThrow(Vector2 direction)
        {
            var currentSkull = skullManager.CurrentSkull;
            if (currentSkull == null || !currentSkull.IsActive)
            {
                LogWarning("활성화된 스컬이 없어 던지기를 실행할 수 없습니다.");
                return;
            }

            if (skullManager.IsSwitching)
            {
                LogDebug("스컬 교체 중에는 던지기를 실행할 수 없습니다.");
                return;
            }

            LogDebug($"스컬 던지기 실행: {direction}");

            try
            {
                // 게임플레이 일시정지 (옵션)
                if (pauseGameplayDuringSwitch)
                {
                    SetPlayerControlEnabled(false);
                }

                // 스컬 던지기 실행
                await currentSkull.PerformSkullThrow(direction);
            }
            finally
            {
                // 게임플레이 재개
                if (pauseGameplayDuringSwitch)
                {
                    SetPlayerControlEnabled(true);
                }
            }
        }

        /// <summary>
        /// 특정 스컬로 교체
        /// </summary>
        public async Awaitable SwitchToSkull(SkullType skullType)
        {
            if (skullManager == null) return;

            await skullManager.SwitchToSkullType(skullType);
        }

        /// <summary>
        /// 다음 스컬로 교체
        /// </summary>
        public async Awaitable SwitchToNextSkull()
        {
            if (skullManager == null) return;

            await skullManager.SwitchToNextSlot();
        }

        /// <summary>
        /// 이전 스컬로 교체
        /// </summary>
        public async Awaitable SwitchToPreviousSkull()
        {
            if (skullManager == null) return;

            await skullManager.SwitchToPreviousSlot();
        }

        /// <summary>
        /// 현재 스컬 정보 가져오기
        /// </summary>
        public ISkullController GetCurrentSkull()
        {
            return skullManager?.CurrentSkull;
        }

        /// <summary>
        /// 현재 스컬 상태 가져오기
        /// </summary>
        public SkullStatus GetCurrentSkullStatus()
        {
            var skull = GetCurrentSkull();
            return skull?.GetStatus() ?? SkullStatus.NotReady();
        }

        /// <summary>
        /// 시스템 활성화/비활성화
        /// </summary>
        public void SetSystemActive(bool active)
        {
            isSystemActive = active;
            LogDebug($"스컬 시스템 활성화: {active}");
        }

        #endregion

        #region 시스템 업데이트

        /// <summary>
        /// 스컬 시스템 업데이트
        /// </summary>
        private void UpdateSkullSystem()
        {
            // 현재 스컬의 상태 업데이트는 SkullManager에서 처리
            // 여기서는 추가적인 시스템 레벨 업데이트 수행

            UpdatePlayerStats();
        }

        /// <summary>
        /// 플레이어 능력치 업데이트
        /// </summary>
        private void UpdatePlayerStats()
        {
            var currentSkull = GetCurrentSkull();
            if (currentSkull?.SkullData?.BaseStats == null) return;

            // 플레이어 컨트롤러의 이동 속도 등을 스컬 능력치에 맞게 조정
            // (향후 PlayerController와 연동)
        }

        #endregion

        #region 이벤트 핸들러

        /// <summary>
        /// 스컬 변경 이벤트 핸들러
        /// </summary>
        private void OnSkullChanged(ISkullController previousSkull, ISkullController newSkull)
        {
            LogDebug($"스컬 변경: {previousSkull?.SkullData?.SkullName} → {newSkull?.SkullData?.SkullName}");

            // 스컬 변경 애니메이션
            PlaySkullSwitchAnimation();

            // UI 업데이트 (향후 구현)
            UpdateSkullUI(newSkull);

            // 플레이어 능력치 동기화
            SynchronizePlayerStats(newSkull);
        }

        /// <summary>
        /// 스컬 장착 이벤트 핸들러
        /// </summary>
        private void OnSkullEquipped(ISkullController skull)
        {
            LogDebug($"스컬 장착: {skull?.SkullData?.SkullName}");

            // 장착 사운드 재생 (향후 오디오 시스템과 연동)
            PlayEquipSound(skull);
        }

        /// <summary>
        /// 스컬 해제 이벤트 핸들러
        /// </summary>
        private void OnSkullUnequipped(ISkullController skull)
        {
            LogDebug($"스컬 해제: {skull?.SkullData?.SkullName}");
        }

        #endregion

        #region 애니메이션 및 피드백

        /// <summary>
        /// 스컬 교체 애니메이션 재생
        /// </summary>
        private void PlaySkullSwitchAnimation()
        {
            if (animationController != null)
            {
                // 교체 애니메이션 트리거
                // animationController.PlayAnimation("SkullSwitch");
            }
        }

        /// <summary>
        /// 장착 사운드 재생
        /// </summary>
        private void PlayEquipSound(ISkullController skull)
        {
            // 향후 AudioManager와 연동
            LogDebug($"장착 사운드 재생: {skull?.SkullData?.SkullName}");
        }

        /// <summary>
        /// 스컬 UI 업데이트
        /// </summary>
        private void UpdateSkullUI(ISkullController skull)
        {
            // 향후 UI 시스템과 연동
            LogDebug($"스컬 UI 업데이트: {skull?.SkullData?.SkullName}");
        }

        #endregion

        #region 플레이어 컨트롤러 연동

        /// <summary>
        /// 플레이어 컨트롤 활성화/비활성화
        /// </summary>
        private void SetPlayerControlEnabled(bool enabled)
        {
            if (inputHandler != null)
            {
                inputHandler.enabled = enabled;
            }
        }

        /// <summary>
        /// 플레이어 능력치 동기화
        /// </summary>
        private void SynchronizePlayerStats(ISkullController skull)
        {
            if (skull?.SkullData?.BaseStats == null || playerController == null) return;

            var stats = skull.SkullData.BaseStats;

            // 플레이어 컨트롤러의 이동 관련 능력치 동기화
            // playerController.SetMoveSpeed(stats.MoveSpeed);
            // playerController.SetJumpPower(stats.JumpPower);

            LogDebug($"플레이어 능력치 동기화: 이동속도={stats.MoveSpeed}, 점프력={stats.JumpPower}");
        }

        #endregion

        #region 정리

        /// <summary>
        /// 시스템 정리
        /// </summary>
        private void CleanupSystem()
        {
            // 이벤트 구독 해제
            if (skullManager != null)
            {
                skullManager.OnSkullChanged -= OnSkullChanged;
                skullManager.OnSkullEquipped -= OnSkullEquipped;
                skullManager.OnSkullUnequipped -= OnSkullUnequipped;
            }

            LogDebug("스컬 시스템 정리 완료");
        }

        #endregion

        #region 디버그 및 로깅

        private void LogDebug(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[SkullSystem] {message}");
            }
        }

        private void LogWarning(string message)
        {
            Debug.LogWarning($"[SkullSystem] {message}");
        }

        private void LogError(string message)
        {
            Debug.LogError($"[SkullSystem] {message}");
        }

        #endregion

        #region 에디터 전용

        [ContextMenu("시스템 정보 출력")]
        private void PrintSystemInfo()
        {
            var currentSkull = GetCurrentSkull();
            var status = GetCurrentSkullStatus();

            Debug.Log($"=== 스컬 시스템 정보 ===\n" +
                     $"시스템 활성화: {isSystemActive}\n" +
                     $"초기화 상태: {isInitialized}\n" +
                     $"현재 스컬: {currentSkull?.SkullData?.SkullName ?? "없음"}\n" +
                     $"스컬 상태: 준비={status.isReady}, 쿨다운={status.cooldownRemaining:F1}초\n" +
                     $"입력 처리: {enableInputHandling}\n" +
                     $"플레이어 컨트롤러: {(playerController != null ? "연결됨" : "없음")}");
        }

        [ContextMenu("다음 스컬로 교체")]
        private void EditorSwitchNext()
        {
            if (Application.isPlaying)
            {
                _ = SwitchToNextSkull();
            }
        }

        [ContextMenu("이전 스컬로 교체")]
        private void EditorSwitchPrevious()
        {
            if (Application.isPlaying)
            {
                _ = SwitchToPreviousSkull();
            }
        }

        #endregion

        #region GUI 디버그

        private void OnGUI()
        {
            if (!enableDebugLogs || !isSystemActive) return;

            GUILayout.BeginArea(new Rect(10, Screen.height - 150, 400, 140));
            GUILayout.Label("=== 스컬 시스템 상태 ===", new GUIStyle(GUI.skin.label) { fontSize = 14, fontStyle = FontStyle.Bold });

            var currentSkull = GetCurrentSkull();
            var status = GetCurrentSkullStatus();

            if (currentSkull != null)
            {
                GUILayout.Label($"현재 스컬: {currentSkull.SkullData?.SkullName}");
                GUILayout.Label($"스컬 타입: {currentSkull.SkullType}");
                GUILayout.Label($"준비 상태: {(status.isReady ? "준비" : "대기")}");
                GUILayout.Label($"쿨다운: {status.cooldownRemaining:F1}초");
                GUILayout.Label($"마나: {status.manaRemaining:F0}");
            }
            else
            {
                GUILayout.Label("활성화된 스컬이 없습니다.");
            }

            GUILayout.Space(5);
            GUILayout.Label("=== 조작법 ===");
            GUILayout.Label("좌클릭: 기본공격, 우클릭: 보조공격, R: 궁극기, 스페이스: 스컬던지기");

            GUILayout.EndArea();
        }

        #endregion
    }
}