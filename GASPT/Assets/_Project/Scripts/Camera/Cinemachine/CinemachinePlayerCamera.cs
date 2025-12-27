using UnityEngine;
using Unity.Cinemachine;
using GASPT.Core;
using GASPT.Core.SceneManagement;

namespace GASPT.CameraSystem
{
    /// <summary>
    /// 플레이어 추적용 Virtual Camera 설정
    /// 메트로바니아 스타일에 최적화된 기본값 제공
    /// Cinemachine 3.x API 사용
    /// ISceneValidator 구현으로 Scene/Room 전환 시 자동 재할당
    /// </summary>
    [RequireComponent(typeof(CinemachineCamera))]
    public class CinemachinePlayerCamera : MonoBehaviour, ISceneValidator
    {
        // ====== 프리셋 ======

        public enum CameraPreset
        {
            Metroidvania,     // 탐험 중심: 약간의 Dead Zone, 적당한 Look Ahead
            Action,           // 액션 중심: 작은 Dead Zone, 빠른 반응
            Exploration,      // 탐험 중심: 넓은 Dead Zone, 느린 추적
            BossFight,        // 보스전: 넓은 시야, 느린 추적
            Custom            // 커스텀 설정 사용
        }


        // ====== 설정 ======

        [Header("프리셋")]
        [SerializeField] private CameraPreset preset = CameraPreset.Metroidvania;

        [Header("추적 설정 (Lookahead)")]
        [SerializeField] private float lookAheadTime = 0.3f;
        [SerializeField] private float lookAheadSmoothing = 5f;
        [SerializeField] private bool lookAheadIgnoreY = true;

        [Header("Damping")]
        [SerializeField] private float dampingX = 0.5f;
        [SerializeField] private float dampingY = 0.7f;

        [Header("Dead Zone (Composition.DeadZone)")]
        [Tooltip("카메라가 움직이지 않는 중앙 영역")]
        [SerializeField] private float deadZoneWidth = 0.1f;
        [SerializeField] private float deadZoneHeight = 0.05f;

        [Header("Hard Limits (Composition.HardLimits)")]
        [Tooltip("타겟이 벗어날 수 없는 최대 영역 (3.x에서 SoftZone 대체)")]
        [SerializeField] private float hardLimitWidth = 0.8f;
        [SerializeField] private float hardLimitHeight = 0.6f;

        [Header("화면 위치 (Composition.ScreenPosition)")]
        [Tooltip("Cinemachine 3.x: 0 = 중앙, -1 = 좌측/하단, +1 = 우측/상단")]
        [SerializeField] private float screenX = 0f;
        [SerializeField] private float screenY = 0f;

        [Header("Lens")]
        [SerializeField] private float orthoSize = 5f;

        [Header("자동 설정")]
        [Tooltip("시작 시 Player 태그로 Follow 타겟 자동 탐색")]
        [SerializeField] private bool autoFindPlayer = true;

        [Tooltip("시작 시 Background 태그로 Confiner 경계 자동 설정")]
        [SerializeField] private bool autoFindBounds = true;

        [Tooltip("자동 탐색 최대 시도 횟수")]
        [SerializeField] private int maxFindAttempts = 50;

        [Tooltip("GameFlowStateMachine의 상태 변경을 통해 타이밍 관리")]
        [SerializeField] private bool useGameFlowEvents = true;

        [Header("카메라 설정")]
        [Tooltip("카메라 Z 오프셋 (2D용, 음수값 사용)")]
        [SerializeField] private float cameraDistance = -10f;

        [Tooltip("활성화 시 타겟을 화면 중앙에 배치")]
        [SerializeField] private bool centerOnActivate = true;

        [Header("타겟 오프셋")]
        [Tooltip("타겟 로컬 좌표 기준 오프셋 (캐릭터 발 → 몸통 중심 등)")]
        [SerializeField] private Vector3 targetOffset = Vector3.zero;


        // ====== 컴포넌트 ======

        private CinemachineCamera virtualCamera;
        private CinemachinePositionComposer positionComposer;
        private CinemachineConfiner2D confiner;

        // ====== 상태 ======
        private bool isSearching = false;
        private bool isInitialized = false;
        private bool isReady = false;  // 카메라 준비 완료 플래그

        // ====== 이벤트 ======

        /// <summary>
        /// 카메라 준비 완료 이벤트 (Fade In 전에 안전하게 사용 가능)
        /// </summary>
        public event System.Action OnCameraReady;

        // ====== 프로퍼티 ======

        /// <summary>
        /// 카메라가 완전히 준비되었는지 여부
        /// </summary>
        public bool IsReady => isReady;


        // ====== Unity 생명주기 ======

        private void Awake()
        {
            virtualCamera = GetComponent<CinemachineCamera>();
            positionComposer = virtualCamera.GetComponent<CinemachinePositionComposer>();
            confiner = virtualCamera.GetComponent<CinemachineConfiner2D>();

            if (positionComposer == null)
            {
                Debug.LogWarning("[CinemachinePlayerCamera] CinemachinePositionComposer를 찾을 수 없습니다. Inspector에서 Body에 Position Composer를 추가하세요.");
            }
        }

        private void OnEnable()
        {
            // ★ SceneValidationManager에 검증기 등록 (Instance 직접 사용으로 자동 생성 보장)
            var validationManager = SceneValidationManager.Instance;
            if (validationManager != null)
            {
                validationManager.RegisterValidator(this);
                Debug.Log("[CinemachinePlayerCamera] SceneValidationManager에 등록 완료");
            }
            else
            {
                Debug.LogWarning("[CinemachinePlayerCamera] SceneValidationManager를 찾을 수 없음 - 등록 실패");
            }

            // GameFlowStateMachine 이벤트 구독 (Validation 시스템의 백업용)
            if (useGameFlowEvents)
            {
                var gameFlow = GameFlowStateMachine.Instance;
                if (gameFlow != null)
                {
                    gameFlow.OnGameStateChanged += OnGameStateChanged;
                    Debug.Log("[CinemachinePlayerCamera] GameFlowStateMachine 이벤트 구독 완료");
                }
            }
        }

        private void OnDisable()
        {
            // SceneValidationManager에서 검증기 해제 (HasInstance 사용 - 종료 시 생성 방지)
            if (SceneValidationManager.HasInstance)
            {
                SceneValidationManager.Instance.UnregisterValidator(this);
            }

            // GameFlowStateMachine 이벤트 해제 (HasInstance 사용 - 종료 시 생성 방지)
            if (GameFlowStateMachine.HasInstance)
            {
                GameFlowStateMachine.Instance.OnGameStateChanged -= OnGameStateChanged;
            }
        }

        private void Start()
        {
            if (preset != CameraPreset.Custom)
            {
                ApplyPreset(preset);
            }
            else
            {
                ApplyCurrentSettings();
            }

            // Position Composer의 CameraDistance 설정
            ApplyCameraDistance();

            // GameFlow 이벤트를 사용하지 않으면 즉시 탐색 시작
            if (!useGameFlowEvents)
            {
                StartAutoSearch();
            }
            // useGameFlowEvents가 true면 OnGameStateChanged에서 탐색 시작

            // 이미 StartRoom 또는 DungeonCombat 상태라면 즉시 탐색
            if (useGameFlowEvents && GameFlowStateMachine.HasInstance)
            {
                string currentState = GameFlowStateMachine.Instance.CurrentStateId;
                if (currentState == "StartRoom" || currentState == "DungeonCombat")
                {
                    Debug.Log($"[CinemachinePlayerCamera] 이미 {currentState} 상태 - 즉시 탐색 시작");
                    StartAutoSearch();
                }
            }
        }

        /// <summary>
        /// GameFlowStateMachine 상태 변경 시 호출
        /// </summary>
        private void OnGameStateChanged(string fromState, string toState)
        {
            Debug.Log($"[CinemachinePlayerCamera] GameFlow 상태 변경: {fromState} → {toState}");

            // 상태 변경 시 Ready 플래그 리셋
            isReady = false;

            // StartRoom 또는 DungeonCombat 진입 시 카메라 타겟 탐색
            if (toState == "StartRoom" || toState == "DungeonCombat")
            {
                // 새로운 Scene이므로 탐색 상태 리셋
                isSearching = false;
                StartAutoSearch();
            }
        }

        /// <summary>
        /// Player 및 Bounds 자동 탐색 시작
        /// </summary>
        private void StartAutoSearch()
        {
            if (isSearching) return;
            isSearching = true;

            // Player 자동 탐색
            if (autoFindPlayer && virtualCamera.Follow == null)
            {
                _ = FindPlayerAsync();
            }

            // Bounds 자동 탐색
            if (autoFindBounds && confiner != null && confiner.BoundingShape2D == null)
            {
                _ = FindBoundsAsync();
            }
        }

        /// <summary>
        /// Player 태그로 Follow 타겟 자동 탐색 (비동기)
        /// </summary>
        private async Awaitable FindPlayerAsync()
        {
            int attempts = 0;

            while (virtualCamera.Follow == null && attempts < maxFindAttempts)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                {
                    SetFollowTargetImmediate(playerObj.transform);
                    Debug.Log($"[CinemachinePlayerCamera] Player 자동 탐색 완료: {playerObj.name}");
                    return;
                }

                await Awaitable.WaitForSecondsAsync(0.1f);
                attempts++;
            }

            if (virtualCamera.Follow == null)
            {
                Debug.LogWarning("[CinemachinePlayerCamera] Player를 찾을 수 없습니다! (타임아웃)");
            }
        }

        /// <summary>
        /// (ContextMenu) 수동으로 Bounds 탐색
        /// </summary>
        [ContextMenu("Find Bounds Now")]
        public void ContextFindBoundsAsync()
        {
            _ = FindBoundsAsync();
        }

        /// <summary>
        /// (ContextMenu) 수동으로 Player 탐색
        /// </summary>
        [ContextMenu("Find Player Now")]
        public void ContextFindPlayerAsync()
        {
            _ = FindPlayerAsync();
        }

        /// <summary>
        /// (ContextMenu) Player 위치로 카메라 즉시 스냅
        /// </summary>
        [ContextMenu("Snap To Player")]
        public void ContextSnapToPlayer()
        {
            SnapToTarget();
        }

        /// <summary>
        /// 외부에서 호출 가능한 탐색 재시작
        /// (Scene 전환 후 수동 호출용)
        /// </summary>
        public void RefreshTargets()
        {
            isSearching = false;
            StartAutoSearch();
        }
        /// <summary>
        /// Background 또는 CameraBounds 태그로 Confiner 경계 자동 탐색 (비동기)
        /// </summary>
        private async Awaitable FindBoundsAsync()
        {
            if (confiner == null) return;

            int attempts = 0;

            while (confiner.BoundingShape2D == null && attempts < maxFindAttempts)
            {
                // 1. CameraBounds 태그로 찾기
                GameObject boundsObj = GameObject.FindGameObjectWithTag("CameraBounds");

                // 2. Background 태그로 찾기
                if (boundsObj == null)
                {
                    boundsObj = GameObject.FindGameObjectWithTag("Background");
                }

                // 3. 이름으로 찾기
                if (boundsObj == null)
                {
                    boundsObj = GameObject.Find("CameraBounds");
                }
                if (boundsObj == null)
                {
                    boundsObj = GameObject.Find("RoomBounds");
                }

                if (boundsObj != null)
                {
                    Collider2D boundsCollider = boundsObj.GetComponent<Collider2D>();
                    if (boundsCollider != null)
                    {
                        confiner.BoundingShape2D = boundsCollider;
                        confiner.InvalidateBoundingShapeCache();
                        Debug.Log($"[CinemachinePlayerCamera] Bounds 자동 탐색 완료: {boundsObj.name}");
                        return;
                    }
                }

                await Awaitable.WaitForSecondsAsync(0.1f);
                attempts++;
            }

            if (confiner.BoundingShape2D == null)
            {
                Debug.LogWarning("[CinemachinePlayerCamera] Camera Bounds를 찾을 수 없습니다! Room에 CameraBounds 태그가 있는 Collider2D를 추가하세요.");
            }
        }

        /// <summary>
        /// 수동으로 Confiner 경계 설정
        /// </summary>
        public void SetBounds(Collider2D boundsCollider)
        {
            if (confiner != null)
            {
                confiner.BoundingShape2D = boundsCollider;
                confiner.InvalidateBoundingShapeCache();
                Debug.Log($"[CinemachinePlayerCamera] Bounds 수동 설정: {boundsCollider?.name ?? "null"}");
            }
        }

        private void OnValidate()
        {
            if (!Application.isPlaying) return;

            if (preset != CameraPreset.Custom)
            {
                ApplyPreset(preset);
            }
            else
            {
                ApplyCurrentSettings();
            }
        }


        // ====== 프리셋 적용 ======

        /// <summary>
        /// 프리셋 적용
        /// </summary>
        public void ApplyPreset(CameraPreset newPreset)
        {
            preset = newPreset;

            switch (newPreset)
            {
                case CameraPreset.Metroidvania:
                    SetMetroidvaniaPreset();
                    break;
                case CameraPreset.Action:
                    SetActionPreset();
                    break;
                case CameraPreset.Exploration:
                    SetExplorationPreset();
                    break;
                case CameraPreset.BossFight:
                    SetBossFightPreset();
                    break;
                case CameraPreset.Custom:
                    ApplyCurrentSettings();
                    break;
            }
        }

        private void SetMetroidvaniaPreset()
        {
            lookAheadTime = 0.3f;
            lookAheadSmoothing = 5f;
            lookAheadIgnoreY = true;
            dampingX = 0.5f;
            dampingY = 0.7f;
            deadZoneWidth = 0.1f;
            deadZoneHeight = 0.05f;
            hardLimitWidth = 0.8f;
            hardLimitHeight = 0.6f;
            screenX = 0f;  // Cinemachine 3.x: 0 = 중앙
            screenY = 0.25f;
            orthoSize = 5f;

            ApplyCurrentSettings();
            Debug.Log("[CinemachinePlayerCamera] Metroidvania 프리셋 적용");
        }

        private void SetActionPreset()
        {
            lookAheadTime = 0.15f;
            lookAheadSmoothing = 3f;
            lookAheadIgnoreY = true;
            dampingX = 0.2f;
            dampingY = 0.3f;
            deadZoneWidth = 0.05f;
            deadZoneHeight = 0.02f;
            hardLimitWidth = 0.7f;
            hardLimitHeight = 0.5f;
            screenX = 0f;  // Cinemachine 3.x: 0 = 중앙
            screenY = 0.25f;
            orthoSize = 5f;

            ApplyCurrentSettings();
            Debug.Log("[CinemachinePlayerCamera] Action 프리셋 적용");
        }

        private void SetExplorationPreset()
        {
            lookAheadTime = 0.5f;
            lookAheadSmoothing = 8f;
            lookAheadIgnoreY = false;
            dampingX = 1f;
            dampingY = 1.2f;
            deadZoneWidth = 0.25f;
            deadZoneHeight = 0.15f;
            hardLimitWidth = 0.9f;
            hardLimitHeight = 0.8f;
            screenX = 0f;  // Cinemachine 3.x: 0 = 중앙
            screenY = 0.25f;
            orthoSize = 6f;

            ApplyCurrentSettings();
            Debug.Log("[CinemachinePlayerCamera] Exploration 프리셋 적용");
        }

        private void SetBossFightPreset()
        {
            lookAheadTime = 0f;
            lookAheadSmoothing = 0f;
            lookAheadIgnoreY = true;
            dampingX = 0.8f;
            dampingY = 0.8f;
            deadZoneWidth = 0.3f;
            deadZoneHeight = 0.2f;
            hardLimitWidth = 0.9f;
            hardLimitHeight = 0.8f;
            screenX = 0f;  // Cinemachine 3.x: 0 = 중앙
            screenY = 0.25f;
            orthoSize = 7f;

            ApplyCurrentSettings();
            Debug.Log("[CinemachinePlayerCamera] BossFight 프리셋 적용");
        }


        // ====== 설정 적용 ======

        /// <summary>
        /// 현재 설정값을 Virtual Camera에 적용
        /// Cinemachine 3.x API 사용
        /// </summary>
        public void ApplyCurrentSettings()
        {
            if (virtualCamera == null)
            {
                virtualCamera = GetComponent<CinemachineCamera>();
            }

            if (positionComposer == null)
            {
                positionComposer = virtualCamera?.GetComponent<CinemachinePositionComposer>();
            }

            // Lens 설정
            if (virtualCamera != null)
            {
                var lens = virtualCamera.Lens;
                lens.OrthographicSize = orthoSize;
                virtualCamera.Lens = lens;
            }

            // Position Composer 설정 (Cinemachine 3.x API)
            if (positionComposer != null)
            {
                // ★ CenterOnActivate 설정 (활성화 시 타겟을 화면 중앙에)
                positionComposer.CenterOnActivate = centerOnActivate;

                // ★ TargetOffset 설정 (타겟 로컬 좌표 기준 오프셋)
                positionComposer.TargetOffset = targetOffset;

                // Lookahead 설정
                var lookahead = positionComposer.Lookahead;
                lookahead.Time = lookAheadTime;
                lookahead.Smoothing = lookAheadSmoothing;
                lookahead.IgnoreY = lookAheadIgnoreY;
                positionComposer.Lookahead = lookahead;

                // Damping 설정
                positionComposer.Damping = new Vector3(dampingX, dampingY, 0);

                // Composition 설정 (Cinemachine 3.x: DeadZone, HardLimits, ScreenPosition)
                var composition = positionComposer.Composition;

                // DeadZone 설정
                var deadZone = composition.DeadZone;
                deadZone.Size = new Vector2(deadZoneWidth, deadZoneHeight);
                composition.DeadZone = deadZone;

                // HardLimits 설정 (Cinemachine 3.x에서 SoftZone 대체)
                var hardLimits = composition.HardLimits;
                hardLimits.Size = new Vector2(hardLimitWidth, hardLimitHeight);
                composition.HardLimits = hardLimits;

                // Screen Position 설정 (Cinemachine 3.x: 0 = 중앙, ±1 = 가장자리)
                composition.ScreenPosition = new Vector2(screenX, screenY);

                positionComposer.Composition = composition;

                Debug.Log($"[CinemachinePlayerCamera] PositionComposer 설정 완료 - CenterOnActivate: {centerOnActivate}, TargetOffset: {targetOffset}");
            }
        }

        /// <summary>
        /// Position Composer의 CameraDistance 설정 (2D용 Z 오프셋)
        /// </summary>
        private void ApplyCameraDistance()
        {
            if (positionComposer != null)
            {
                positionComposer.CameraDistance = Mathf.Abs(cameraDistance);
                Debug.Log($"[CinemachinePlayerCamera] CameraDistance 설정: {positionComposer.CameraDistance}");
            }
        }


        // ====== Public API ======

        /// <summary>
        /// 추적 대상 설정 (Damping 적용, 부드러운 전환)
        /// </summary>
        public void SetFollowTarget(Transform target)
        {
            if (virtualCamera != null)
            {
                virtualCamera.Follow = target;
                Debug.Log($"[CinemachinePlayerCamera] Follow 대상 설정: {target?.name ?? "null"}");
            }
        }

        /// <summary>
        /// 추적 대상 설정 및 카메라 즉시 스냅 (타겟 위치로 즉시 이동)
        /// 씬 전환이나 초기 설정 시 사용
        /// </summary>
        public void SetFollowTargetImmediate(Transform target)
        {
            if (virtualCamera == null) return;

            virtualCamera.Follow = target;

            if (target != null)
            {
                // 카메라 위치를 타겟 위치로 즉시 스냅
                ForceCameraPosition(target.position);
            }

            Debug.Log($"[CinemachinePlayerCamera] Follow 대상 즉시 설정: {target?.name ?? "null"}");
        }

        /// <summary>
        /// 카메라 위치를 지정된 위치로 즉시 이동 (Damping 무시)
        /// 2D에서는 타겟의 XY + 카메라 Z 오프셋을 사용
        /// ★ Lookahead, Damping 상태도 리셋하여 즉시 중앙 정렬
        /// </summary>
        public void ForceCameraPosition(Vector3 targetPosition)
        {
            if (virtualCamera == null) return;

            // ★ 1단계: PositionComposer의 내부 추적 상태 리셋
            if (positionComposer != null)
            {
                // OnTargetObjectWarped: 타겟이 순간이동했음을 알려 내부 상태 리셋
                // 이를 통해 Lookahead, Damping 계산용 이전 위치 데이터가 초기화됨
                positionComposer.OnTargetObjectWarped(virtualCamera.Follow, targetPosition - (virtualCamera.Follow?.position ?? targetPosition));
                Debug.Log("[CinemachinePlayerCamera] PositionComposer 내부 상태 리셋 (OnTargetObjectWarped)");
            }

            // TargetOffset을 고려한 실제 카메라 위치 계산
            Vector3 offsetPosition = targetPosition + targetOffset;

            // 2D 카메라: 타겟의 XY 좌표 + Z 오프셋
            Vector3 cameraPosition = new Vector3(
                offsetPosition.x,
                offsetPosition.y,
                cameraDistance  // 음수값 (예: -10)
            );

            // ★ 2단계: Cinemachine 3.x ForceCameraPosition 사용
            virtualCamera.ForceCameraPosition(cameraPosition, Quaternion.identity);

            Debug.Log($"[CinemachinePlayerCamera] 카메라 위치 강제 설정: {cameraPosition} (타겟: {targetPosition}, 오프셋: {targetOffset})");
        }

        /// <summary>
        /// 현재 Follow 타겟 위치로 카메라 즉시 스냅
        /// </summary>
        public void SnapToTarget()
        {
            if (virtualCamera == null || virtualCamera.Follow == null) return;

            ForceCameraPosition(virtualCamera.Follow.position);
        }

        /// <summary>
        /// 줌 설정 (Ortho Size)
        /// </summary>
        public void SetOrthoSize(float size)
        {
            if (virtualCamera != null)
            {
                var lens = virtualCamera.Lens;
                lens.OrthographicSize = size;
                virtualCamera.Lens = lens;
            }
        }

        /// <summary>
        /// Priority 설정
        /// </summary>
        public void SetPriority(int priority)
        {
            if (virtualCamera != null)
            {
                virtualCamera.Priority = priority;
            }
        }

        /// <summary>
        /// Virtual Camera 참조 반환
        /// </summary>
        public CinemachineCamera GetVirtualCamera() => virtualCamera;


        // ====== 에디터 유틸리티 ======

#if UNITY_EDITOR
        [ContextMenu("Apply Metroidvania Preset")]
        private void EditorApplyMetroidvania() => ApplyPreset(CameraPreset.Metroidvania);

        [ContextMenu("Apply Action Preset")]
        private void EditorApplyAction() => ApplyPreset(CameraPreset.Action);

        [ContextMenu("Apply Exploration Preset")]
        private void EditorApplyExploration() => ApplyPreset(CameraPreset.Exploration);

        [ContextMenu("Apply BossFight Preset")]
        private void EditorApplyBossFight() => ApplyPreset(CameraPreset.BossFight);

        [ContextMenu("Print Current Settings")]
        private void PrintCurrentSettings()
        {
            Debug.Log($"[CinemachinePlayerCamera] 현재 설정:\n" +
                     $"  Preset: {preset}\n" +
                     $"  LookAhead: Time={lookAheadTime}, Smoothing={lookAheadSmoothing}, IgnoreY={lookAheadIgnoreY}\n" +
                     $"  Damping: X={dampingX}, Y={dampingY}\n" +
                     $"  DeadZone: {deadZoneWidth} x {deadZoneHeight}\n" +
                     $"  HardLimits: {hardLimitWidth} x {hardLimitHeight}\n" +
                     $"  Screen: {screenX}, {screenY}\n" +
                     $"  OrthoSize: {orthoSize}\n" +
                     $"  CenterOnActivate: {centerOnActivate}\n" +
                     $"  TargetOffset: {targetOffset}\n" +
                     $"  CameraDistance: {cameraDistance}");

            // PositionComposer 실제 값도 출력
            if (positionComposer != null)
            {
                Debug.Log($"[CinemachinePlayerCamera] PositionComposer 실제 값:\n" +
                         $"  CenterOnActivate: {positionComposer.CenterOnActivate}\n" +
                         $"  TargetOffset: {positionComposer.TargetOffset}\n" +
                         $"  CameraDistance: {positionComposer.CameraDistance}\n" +
                         $"  Composition.ScreenPosition: {positionComposer.Composition.ScreenPosition}");
            }
        }

        [ContextMenu("Force Center On Player")]
        private void EditorForceCenterOnPlayer()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("플레이 모드에서만 사용 가능합니다.");
                return;
            }

            if (virtualCamera != null && virtualCamera.Follow != null)
            {
                // 설정 재적용
                ApplyCurrentSettings();
                ApplyCameraDistance();

                // 강제 스냅
                ForceCameraPosition(virtualCamera.Follow.position);

                Debug.Log("[CinemachinePlayerCamera] Player 중심으로 강제 이동 완료");
            }
            else
            {
                Debug.LogWarning("[CinemachinePlayerCamera] Follow 타겟이 없습니다.");
            }
        }

#endif


        // ====== ISceneValidator 구현 ======

        /// <summary>
        /// 검증기 이름
        /// </summary>
        public string ValidatorName => "CinemachinePlayerCamera";

        /// <summary>
        /// 실행 우선순위 (카메라는 Player 다음으로 중요: 5)
        /// </summary>
        public int Priority => 5;

        /// <summary>
        /// 검증기 활성 상태
        /// </summary>
        public bool IsValidatorActive => enabled && gameObject.activeInHierarchy;

        /// <summary>
        /// Scene/Room 전환 후 카메라 참조 검증 및 재할당
        /// - Player Follow 타겟 확인/재할당
        /// - CameraBounds Confiner 확인/재할당
        /// - 카메라 위치 즉시 스냅
        /// - ★ Lookahead 일시 비활성화 후 복원 (카메라 중앙 정렬 보장)
        /// </summary>
        public async Awaitable<bool> ValidateAndReassignAsync()
        {
            Debug.Log("[CinemachinePlayerCamera] ===== 검증 및 재할당 시작 =====");

            // 검증 시작 시 Ready 플래그 리셋
            isReady = false;

            bool playerFound = false;
            bool boundsFound = false;

            // 1. Player Follow 타겟 검증 및 재할당
            if (virtualCamera != null)
            {
                // 기존 Follow가 null이거나 Missing인 경우
                if (virtualCamera.Follow == null || !IsValidReference(virtualCamera.Follow))
                {
                    Debug.Log("[CinemachinePlayerCamera] Follow 타겟이 없거나 Missing - 재탐색 시작");
                    playerFound = await FindAndAssignPlayerAsync();
                }
                else
                {
                    Debug.Log($"[CinemachinePlayerCamera] Follow 타겟 유효: {virtualCamera.Follow.name}");
                    playerFound = true;
                }
            }

            // 2. CameraBounds Confiner 검증 및 재할당
            if (confiner != null)
            {
                // 기존 BoundingShape2D가 null이거나 Missing인 경우
                if (confiner.BoundingShape2D == null || !IsValidReference(confiner.BoundingShape2D))
                {
                    Debug.Log("[CinemachinePlayerCamera] BoundingShape2D가 없거나 Missing - 재탐색 시작");
                    boundsFound = await FindAndAssignBoundsAsync();
                }
                else
                {
                    Debug.Log($"[CinemachinePlayerCamera] BoundingShape2D 유효: {confiner.BoundingShape2D.name}");
                    boundsFound = true;
                }
            }
            else
            {
                // Confiner가 없으면 Bounds는 필수가 아님
                boundsFound = true;
            }

            // 3. Position Composer 설정 재적용 (CenterOnActivate, TargetOffset 등)
            ApplyCurrentSettings();
            ApplyCameraDistance();
            Debug.Log("[CinemachinePlayerCamera] PositionComposer 설정 재적용 완료");

            // 4. Player가 있으면 카메라 위치 즉시 스냅
            if (playerFound && virtualCamera.Follow != null)
            {
                // ★ Lookahead 일시 비활성화 (이전 프레임 데이터 무효화)
                await ResetCameraWithLookaheadDisabled(virtualCamera.Follow.position);
            }

            // 5. 카메라 준비 완료
            isReady = true;
            OnCameraReady?.Invoke();
            Debug.Log("[CinemachinePlayerCamera] 카메라 준비 완료!");

            // Player를 못 찾아도 검증은 성공으로 처리 (Warning만 남김)
            // 이후 Player 생성 시 OnGameStateChanged에서 재탐색됨
            if (!playerFound)
            {
                Debug.LogWarning("[CinemachinePlayerCamera] Player를 찾지 못했지만 검증은 계속 진행");
            }

            Debug.Log($"[CinemachinePlayerCamera] ===== 검증 완료 (Player: {playerFound}, Bounds: {boundsFound}) =====");

            return true; // 항상 성공 반환 (카메라 자체는 준비됨)
        }

        /// <summary>
        /// Lookahead를 일시적으로 비활성화하고 카메라 위치를 강제 설정한 뒤 복원
        /// 이렇게 하면 Cinemachine의 Lookahead 버퍼가 비워지고 새로운 위치에서 시작
        /// </summary>
        private async Awaitable ResetCameraWithLookaheadDisabled(Vector3 targetPosition)
        {
            if (positionComposer == null)
            {
                // PositionComposer 없으면 일반 스냅
                ForceCameraPosition(targetPosition);
                return;
            }

            // 1. 현재 Lookahead 설정 백업
            var originalLookahead = positionComposer.Lookahead;
            var originalDamping = positionComposer.Damping;

            Debug.Log($"[CinemachinePlayerCamera] Lookahead 일시 비활성화 (원래값: Time={originalLookahead.Time})");

            // 2. Lookahead와 Damping 일시 비활성화
            var tempLookahead = originalLookahead;
            tempLookahead.Time = 0f;
            tempLookahead.Smoothing = 0f;
            positionComposer.Lookahead = tempLookahead;
            positionComposer.Damping = Vector3.zero;

            // 3. 2프레임 대기 (Cinemachine이 새 설정 적용)
            await Awaitable.NextFrameAsync();
            await Awaitable.NextFrameAsync();

            // 4. 카메라 위치 강제 설정
            ForceCameraPosition(targetPosition);

            // 5. 5프레임 대기 (카메라 완전 안정화 - Fade In 전에 완료 보장)
            for (int i = 0; i < 5; i++)
            {
                await Awaitable.NextFrameAsync();
            }

            // 6. Lookahead와 Damping 복원
            positionComposer.Lookahead = originalLookahead;
            positionComposer.Damping = originalDamping;

            Debug.Log($"[CinemachinePlayerCamera] Lookahead 복원 완료 (Time={originalLookahead.Time})");
        }

        /// <summary>
        /// Unity 오브젝트 참조가 유효한지 확인 (Missing 체크)
        /// </summary>
        private bool IsValidReference(Object obj)
        {
            // Unity의 == null 연산자는 destroyed 오브젝트도 체크
            return obj != null;
        }

        /// <summary>
        /// Player를 찾아서 Follow 타겟으로 할당 (동기식, 즉시 시도)
        /// </summary>
        private async Awaitable<bool> FindAndAssignPlayerAsync()
        {
            int attempts = 0;

            while (attempts < maxFindAttempts)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                {
                    virtualCamera.Follow = playerObj.transform;
                    Debug.Log($"[CinemachinePlayerCamera] Player 재할당 완료: {playerObj.name}");
                    return true;
                }

                await Awaitable.WaitForSecondsAsync(0.1f);
                attempts++;
            }

            Debug.LogWarning("[CinemachinePlayerCamera] Player 재할당 실패 - 타임아웃");
            return false;
        }

        /// <summary>
        /// CameraBounds를 찾아서 Confiner에 할당 (동기식, 즉시 시도)
        /// </summary>
        private async Awaitable<bool> FindAndAssignBoundsAsync()
        {
            if (confiner == null) return false;

            int attempts = 0;

            while (attempts < maxFindAttempts)
            {
                // 1. CameraBounds 태그로 찾기
                GameObject boundsObj = GameObject.FindGameObjectWithTag("CameraBounds");

                // 2. Background 태그로 찾기
                if (boundsObj == null)
                {
                    boundsObj = GameObject.FindGameObjectWithTag("Background");
                }

                // 3. 이름으로 찾기
                if (boundsObj == null)
                {
                    boundsObj = GameObject.Find("CameraBounds");
                }
                if (boundsObj == null)
                {
                    boundsObj = GameObject.Find("RoomBounds");
                }

                if (boundsObj != null)
                {
                    Collider2D boundsCollider = boundsObj.GetComponent<Collider2D>();
                    if (boundsCollider != null)
                    {
                        confiner.BoundingShape2D = boundsCollider;
                        confiner.InvalidateBoundingShapeCache();
                        Debug.Log($"[CinemachinePlayerCamera] Bounds 재할당 완료: {boundsObj.name}");
                        return true;
                    }
                }

                await Awaitable.WaitForSecondsAsync(0.1f);
                attempts++;
            }

            Debug.LogWarning("[CinemachinePlayerCamera] Bounds 재할당 실패 - 타임아웃 (선택적 기능이므로 계속 진행)");
            return false;
        }
    }
}
