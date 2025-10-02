using UnityEngine;
using System;
using System.Collections.Generic;

namespace Combat.Attack
{
    /// <summary>
    /// 콤보 시스템
    /// 연속 공격 관리 및 콤보 체인 처리
    /// </summary>
    public class ComboSystem : MonoBehaviour
    {
        [Header("콤보 설정")]
        [SerializeField] private List<ComboData> combos = new List<ComboData>();
        [SerializeField] private float comboWindowTime = 0.5f;
        [SerializeField] private float comboResetTime = 1.0f;

        [Header("디버그")]
        [SerializeField] private bool enableDebugLogs = false;

        // 현재 콤보 상태
        private int currentComboIndex = 0;
        private float lastHitTime = 0f;
        private bool isComboActive = false;
        private float comboTimer = 0f;

        // 이벤트
        public event Action<int> OnComboStarted;
        public event Action<int> OnComboAdvanced;
        public event Action<int> OnComboCompleted;
        public event Action OnComboReset;

        #region 프로퍼티

        /// <summary>
        /// 현재 콤보 인덱스
        /// </summary>
        public int CurrentComboIndex => currentComboIndex;

        /// <summary>
        /// 콤보 활성화 여부
        /// </summary>
        public bool IsComboActive => isComboActive;

        /// <summary>
        /// 콤보 진행률 (0~1)
        /// </summary>
        public float ComboProgress
        {
            get
            {
                if (!isComboActive || combos.Count == 0) return 0f;
                return (float)currentComboIndex / combos.Count;
            }
        }

        /// <summary>
        /// 다음 콤보 입력 가능 여부
        /// </summary>
        public bool CanInputNextCombo
        {
            get
            {
                if (!isComboActive) return true;

                float timeSinceLastHit = Time.time - lastHitTime;
                return timeSinceLastHit <= comboWindowTime;
            }
        }

        /// <summary>
        /// 현재 콤보 데이터
        /// </summary>
        public ComboData CurrentComboData
        {
            get
            {
                if (currentComboIndex >= 0 && currentComboIndex < combos.Count)
                {
                    return combos[currentComboIndex];
                }
                return null;
            }
        }

        #endregion

        #region Unity 생명주기

        private void Update()
        {
            UpdateComboTimer();
        }

        #endregion

        #region 콤보 관리

        /// <summary>
        /// 타격 등록 (콤보 진행)
        /// </summary>
        public bool RegisterHit(int comboIndex = -1)
        {
            // 콤보 인덱스 검증
            if (comboIndex >= 0 && comboIndex != currentComboIndex)
            {
                LogDebug($"Invalid combo index: expected {currentComboIndex}, got {comboIndex}");
                return false;
            }

            // 다음 콤보 입력 가능 체크
            if (isComboActive && !CanInputNextCombo)
            {
                LogDebug("Combo window expired");
                ResetCombo();
                return false;
            }

            // 현재 콤보 인덱스 저장 (이벤트용)
            int executedComboIndex = currentComboIndex;

            // 콤보 시작 또는 진행
            if (!isComboActive)
            {
                StartCombo(executedComboIndex);
            }

            // 다음 콤보로 진행 (StartCombo 이후에도 실행)
            AdvanceCombo();

            lastHitTime = Time.time;
            return true;
        }

        /// <summary>
        /// 콤보 시작
        /// </summary>
        private void StartCombo(int startIndex)
        {
            isComboActive = true;
            comboTimer = comboResetTime;

            OnComboStarted?.Invoke(startIndex);
            LogDebug($"Combo started: index {startIndex}");
        }

        /// <summary>
        /// 콤보 진행
        /// </summary>
        private void AdvanceCombo()
        {
            currentComboIndex++;

            // 콤보 완료 체크
            if (currentComboIndex >= combos.Count)
            {
                CompleteCombo();
                return;
            }

            comboTimer = comboResetTime;
            OnComboAdvanced?.Invoke(currentComboIndex);
            LogDebug($"Combo advanced to: index {currentComboIndex}");
        }

        /// <summary>
        /// 콤보 완료
        /// </summary>
        private void CompleteCombo()
        {
            int completedIndex = currentComboIndex;
            OnComboCompleted?.Invoke(completedIndex);
            LogDebug($"Combo completed at index {completedIndex}");

            ResetCombo();
        }

        /// <summary>
        /// 콤보 리셋
        /// </summary>
        public void ResetCombo()
        {
            if (!isComboActive) return;

            currentComboIndex = 0;
            isComboActive = false;
            comboTimer = 0f;

            OnComboReset?.Invoke();
            LogDebug("Combo reset");
        }

        /// <summary>
        /// 강제 콤보 리셋 (외부 호출용)
        /// </summary>
        public void ForceReset()
        {
            ResetCombo();
        }

        #endregion

        #region 타이머 관리

        /// <summary>
        /// 콤보 타이머 업데이트
        /// </summary>
        private void UpdateComboTimer()
        {
            if (!isComboActive) return;

            float timeSinceLastHit = Time.time - lastHitTime;

            // 콤보 리셋 시간 초과
            if (timeSinceLastHit >= comboResetTime)
            {
                ResetCombo();
            }
        }

        #endregion

        #region 콤보 데이터 관리

        /// <summary>
        /// 콤보 추가
        /// </summary>
        public void AddCombo(ComboData comboData)
        {
            if (comboData != null)
            {
                combos.Add(comboData);
            }
        }

        /// <summary>
        /// 콤보 제거
        /// </summary>
        public void RemoveCombo(int index)
        {
            if (index >= 0 && index < combos.Count)
            {
                combos.RemoveAt(index);
            }
        }

        /// <summary>
        /// 모든 콤보 제거
        /// </summary>
        public void ClearCombos()
        {
            combos.Clear();
            ResetCombo();
        }

        /// <summary>
        /// 콤보 데이터 가져오기
        /// </summary>
        public ComboData GetComboData(int index)
        {
            if (index >= 0 && index < combos.Count)
            {
                return combos[index];
            }
            return null;
        }

        /// <summary>
        /// 전체 콤보 개수
        /// </summary>
        public int GetComboCount()
        {
            return combos.Count;
        }

        #endregion

        #region 설정

        /// <summary>
        /// 콤보 윈도우 시간 설정
        /// </summary>
        public void SetComboWindowTime(float time)
        {
            comboWindowTime = Mathf.Max(0f, time);
        }

        /// <summary>
        /// 콤보 리셋 시간 설정
        /// </summary>
        public void SetComboResetTime(float time)
        {
            comboResetTime = Mathf.Max(0f, time);
        }

        #endregion

        #region 디버그

        private void LogDebug(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[ComboSystem - {gameObject.name}] {message}");
            }
        }

        #endregion
    }

    /// <summary>
    /// 콤보 데이터
    /// </summary>
    [Serializable]
    public class ComboData
    {
        [Header("기본 정보")]
        public string comboName;
        public int comboIndex;

        [Header("타이밍")]
        public float inputWindowStart = 0f;
        public float inputWindowEnd = 0.5f;

        [Header("데미지")]
        public float damageMultiplier = 1f;

        [Header("애니메이션")]
        public string animationTrigger;
        public float animationSpeed = 1f;

        [Header("히트박스")]
        public Vector2 hitboxSize = Vector2.one;
        public Vector2 hitboxOffset = Vector2.zero;
        public float hitboxDuration = 0.2f;

        [Header("효과")]
        public GameObject effectPrefab;
        public AudioClip soundEffect;

        [Header("넉백")]
        public Vector2 knockbackForce = Vector2.zero;
        public bool useHitDirection = true;

        /// <summary>
        /// 입력 가능 시간 내 체크
        /// </summary>
        public bool IsInInputWindow(float timeSinceStart)
        {
            return timeSinceStart >= inputWindowStart && timeSinceStart <= inputWindowEnd;
        }
    }
}
