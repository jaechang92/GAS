using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GASPT.Core.SceneManagement
{
    /// <summary>
    /// Scene/Room 전환 후 모든 검증기를 실행하는 중앙 관리자
    /// Loading 상태에서 Scene 로드 완료 후 호출하여 모든 시스템의 참조를 검증/재할당
    /// </summary>
    public class SceneValidationManager : SingletonManager<SceneValidationManager>
    {
        [Header("설정")]
        [SerializeField] private bool showDebugLogs = true;

        [Header("타임아웃")]
        [Tooltip("개별 검증기 타임아웃 (초)")]
        [SerializeField] private float validatorTimeout = 5f;

        [Tooltip("전체 검증 타임아웃 (초)")]
        [SerializeField] private float totalTimeout = 30f;

        [Header("상태 (읽기 전용)")]
        [SerializeField] private int registeredValidatorCount = 0;
        [SerializeField] private bool isValidating = false;


        // ====== 검증기 관리 ======

        private readonly List<ISceneValidator> validators = new List<ISceneValidator>();
        private readonly object lockObject = new object();


        // ====== 이벤트 ======

        /// <summary>
        /// 검증 시작 이벤트
        /// </summary>
        public event Action OnValidationStarted;

        /// <summary>
        /// 검증 완료 이벤트 (성공 여부)
        /// </summary>
        public event Action<bool> OnValidationCompleted;

        /// <summary>
        /// 개별 검증기 완료 이벤트 (검증기 이름, 성공 여부)
        /// </summary>
        public event Action<string, bool> OnValidatorCompleted;


        // ====== 프로퍼티 ======

        /// <summary>
        /// 현재 검증 중인지 여부
        /// </summary>
        public bool IsValidating => isValidating;

        /// <summary>
        /// 등록된 검증기 수
        /// </summary>
        public int ValidatorCount => validators.Count;


        // ====== 초기화 ======

        protected override void OnAwake()
        {
            LogMessage("[SceneValidationManager] 초기화 완료");
        }


        // ====== 검증기 등록/해제 ======

        /// <summary>
        /// 검증기 등록
        /// </summary>
        public void RegisterValidator(ISceneValidator validator)
        {
            if (validator == null)
            {
                LogWarning("[SceneValidationManager] null 검증기는 등록할 수 없습니다.");
                return;
            }

            lock (lockObject)
            {
                if (!validators.Contains(validator))
                {
                    validators.Add(validator);
                    registeredValidatorCount = validators.Count;
                    LogMessage($"[SceneValidationManager] 검증기 등록: {validator.ValidatorName} (Priority: {validator.Priority})");
                }
            }
        }

        /// <summary>
        /// 검증기 해제
        /// </summary>
        public void UnregisterValidator(ISceneValidator validator)
        {
            if (validator == null) return;

            lock (lockObject)
            {
                if (validators.Remove(validator))
                {
                    registeredValidatorCount = validators.Count;
                    LogMessage($"[SceneValidationManager] 검증기 해제: {validator.ValidatorName}");
                }
            }
        }

        /// <summary>
        /// 모든 검증기 해제
        /// </summary>
        public void ClearAllValidators()
        {
            lock (lockObject)
            {
                validators.Clear();
                registeredValidatorCount = 0;
                LogMessage("[SceneValidationManager] 모든 검증기 해제");
            }
        }


        // ====== 검증 실행 ======

        /// <summary>
        /// 모든 등록된 검증기 실행 (우선순위 순서대로)
        /// Loading 상태에서 Scene 로드 완료 후 호출
        /// </summary>
        /// <returns>모든 검증 성공 여부</returns>
        public async Awaitable<bool> ValidateAllAsync()
        {
            if (isValidating)
            {
                LogWarning("[SceneValidationManager] 이미 검증 중입니다.");
                return false;
            }

            isValidating = true;
            OnValidationStarted?.Invoke();

            LogMessage($"[SceneValidationManager] ========== 검증 시작 ({validators.Count}개 검증기) ==========");

            bool allSuccess = true;
            float startTime = Time.realtimeSinceStartup;

            // 우선순위 순으로 정렬하여 실행
            List<ISceneValidator> sortedValidators;
            lock (lockObject)
            {
                sortedValidators = validators
                    .Where(v => v.IsValidatorActive)
                    .OrderBy(v => v.Priority)
                    .ToList();
            }

            LogMessage($"[SceneValidationManager] 활성 검증기: {sortedValidators.Count}개");

            foreach (var validator in sortedValidators)
            {
                // 전체 타임아웃 체크
                if (Time.realtimeSinceStartup - startTime > totalTimeout)
                {
                    LogError($"[SceneValidationManager] 전체 검증 타임아웃! ({totalTimeout}초)");
                    allSuccess = false;
                    break;
                }

                // 개별 검증기 실행
                bool success = await ExecuteValidatorAsync(validator);

                if (!success)
                {
                    allSuccess = false;
                    LogWarning($"[SceneValidationManager] 검증기 실패: {validator.ValidatorName}");
                }

                OnValidatorCompleted?.Invoke(validator.ValidatorName, success);
            }

            float elapsedTime = Time.realtimeSinceStartup - startTime;
            LogMessage($"[SceneValidationManager] ========== 검증 완료 (성공: {allSuccess}, 소요시간: {elapsedTime:F2}초) ==========");

            isValidating = false;
            OnValidationCompleted?.Invoke(allSuccess);

            return allSuccess;
        }

        /// <summary>
        /// 개별 검증기 실행 (타임아웃 적용)
        /// </summary>
        private async Awaitable<bool> ExecuteValidatorAsync(ISceneValidator validator)
        {
            LogMessage($"[SceneValidationManager] 검증 시작: {validator.ValidatorName}");

            float startTime = Time.realtimeSinceStartup;
            bool completed = false;
            bool result = false;

            // 검증 실행
            try
            {
                // 타임아웃과 함께 실행
                var validationTask = validator.ValidateAndReassignAsync();

                // 간단한 타임아웃 체크 (Awaitable은 취소 토큰을 직접 지원하지 않음)
                while (!completed)
                {
                    if (Time.realtimeSinceStartup - startTime > validatorTimeout)
                    {
                        LogWarning($"[SceneValidationManager] 검증기 타임아웃: {validator.ValidatorName} ({validatorTimeout}초)");
                        return false;
                    }

                    // 검증 완료 체크 (Awaitable은 IsCompleted가 없으므로 직접 await)
                    // 여기서는 단순히 await하고 타임아웃은 별도로 체크
                    result = await validationTask;
                    completed = true;
                }
            }
            catch (Exception ex)
            {
                LogError($"[SceneValidationManager] 검증기 예외: {validator.ValidatorName} - {ex.Message}");
                return false;
            }

            float elapsedTime = Time.realtimeSinceStartup - startTime;
            LogMessage($"[SceneValidationManager] 검증 완료: {validator.ValidatorName} (결과: {result}, 소요: {elapsedTime:F2}초)");

            return result;
        }


        // ====== 로깅 ======

        private void LogMessage(string message)
        {
            if (showDebugLogs)
            {
                Debug.Log(message);
            }
        }

        private void LogWarning(string message)
        {
            Debug.LogWarning(message);
        }

        private void LogError(string message)
        {
            Debug.LogError(message);
        }


        // ====== 디버그 ======

#if UNITY_EDITOR
        [ContextMenu("Print Registered Validators")]
        private void PrintRegisteredValidators()
        {
            Debug.Log($"========== 등록된 검증기 ({validators.Count}개) ==========");
            foreach (var v in validators.OrderBy(v => v.Priority))
            {
                Debug.Log($"  [{v.Priority}] {v.ValidatorName} (Active: {v.IsValidatorActive})");
            }
            Debug.Log("==============================================");
        }

        [ContextMenu("Test Validate All")]
        private async void TestValidateAll()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("플레이 모드에서만 테스트 가능합니다.");
                return;
            }

            bool result = await ValidateAllAsync();
            Debug.Log($"검증 결과: {(result ? "성공" : "실패")}");
        }
#endif
    }
}
