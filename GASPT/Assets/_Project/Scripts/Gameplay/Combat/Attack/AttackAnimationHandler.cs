using UnityEngine;

namespace Combat.Attack
{
    /// <summary>
    /// 공격 애니메이션 핸들러
    /// 공격 애니메이션과 게임플레이 로직 연동
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class AttackAnimationHandler : MonoBehaviour
    {
        [Header("참조")]
        [SerializeField] private Animator animator;
        [SerializeField] private ComboSystem comboSystem;

        [Header("애니메이션 파라미터")]
        [SerializeField] private string attackTriggerParameter = "Attack";
        [SerializeField] private string comboIndexParameter = "ComboIndex";
        [SerializeField] private string attackSpeedParameter = "AttackSpeed";
        [SerializeField] private string isAttackingParameter = "IsAttacking";

        [Header("타이밍 설정")]
        [SerializeField] private float defaultAttackSpeed = 1f;
        [SerializeField] private bool useAnimationEvents = true;

        [Header("디버그")]
        [SerializeField] private bool enableDebugLogs = false;

        // 현재 상태
        private bool isAttacking = false;
        private int currentAnimationComboIndex = 0;

        // 이벤트
        public event System.Action OnAttackAnimationStart;
        public event System.Action OnAttackAnimationHit;
        public event System.Action OnAttackAnimationEnd;

        #region 프로퍼티

        /// <summary>
        /// 공격 중 여부
        /// </summary>
        public bool IsAttacking => isAttacking;

        /// <summary>
        /// 현재 애니메이션 콤보 인덱스
        /// </summary>
        public int CurrentAnimationComboIndex => currentAnimationComboIndex;

        #endregion

        #region Unity 생명주기

        private void Awake()
        {
            // 자동 참조
            if (animator == null)
            {
                animator = GetComponent<Animator>();
            }

            if (comboSystem == null)
            {
                comboSystem = GetComponent<ComboSystem>();
            }
        }

        private void OnEnable()
        {
            // 콤보 시스템 이벤트 구독
            if (comboSystem != null)
            {
                comboSystem.OnComboStarted += OnComboStarted;
                comboSystem.OnComboAdvanced += OnComboAdvanced;
                comboSystem.OnComboReset += OnComboReset;
            }
        }

        private void OnDisable()
        {
            // 콤보 시스템 이벤트 구독 해제
            if (comboSystem != null)
            {
                comboSystem.OnComboStarted -= OnComboStarted;
                comboSystem.OnComboAdvanced -= OnComboAdvanced;
                comboSystem.OnComboReset -= OnComboReset;
            }
        }

        #endregion

        #region 애니메이션 제어

        /// <summary>
        /// 공격 애니메이션 트리거
        /// </summary>
        public void TriggerAttackAnimation(int comboIndex = 0, float attackSpeed = 1f)
        {
            if (animator == null) return;

            // 콤보 인덱스 설정
            currentAnimationComboIndex = comboIndex;
            animator.SetInteger(comboIndexParameter, comboIndex);

            // 공격 속도 설정
            animator.SetFloat(attackSpeedParameter, attackSpeed);

            // 공격 상태 설정
            isAttacking = true;
            animator.SetBool(isAttackingParameter, true);

            // 공격 트리거
            animator.SetTrigger(attackTriggerParameter);

            LogDebug($"Triggered attack animation: combo {comboIndex}, speed {attackSpeed}");
        }

        /// <summary>
        /// 공격 애니메이션 중단
        /// </summary>
        public void StopAttackAnimation()
        {
            if (animator == null) return;

            isAttacking = false;
            animator.SetBool(isAttackingParameter, false);
            animator.ResetTrigger(attackTriggerParameter);

            LogDebug("Stopped attack animation");
        }

        /// <summary>
        /// 애니메이션 속도 설정
        /// </summary>
        public void SetAttackSpeed(float speed)
        {
            if (animator == null) return;

            animator.SetFloat(attackSpeedParameter, speed);
        }

        #endregion

        #region 콤보 시스템 이벤트 처리

        /// <summary>
        /// 콤보 시작 이벤트
        /// </summary>
        private void OnComboStarted(int comboIndex)
        {
            TriggerAttackAnimation(comboIndex, defaultAttackSpeed);
        }

        /// <summary>
        /// 콤보 진행 이벤트
        /// </summary>
        private void OnComboAdvanced(int comboIndex)
        {
            TriggerAttackAnimation(comboIndex, defaultAttackSpeed);
        }

        /// <summary>
        /// 콤보 리셋 이벤트
        /// </summary>
        private void OnComboReset()
        {
            StopAttackAnimation();
        }

        #endregion

        #region Animation Events (애니메이션 이벤트에서 호출)

        /// <summary>
        /// 공격 애니메이션 시작 (Animation Event)
        /// </summary>
        public void OnAttackStart()
        {
            isAttacking = true;
            OnAttackAnimationStart?.Invoke();
            LogDebug("Attack animation started");
        }

        /// <summary>
        /// 공격 타격 시점 (Animation Event)
        /// </summary>
        public void OnAttackHit()
        {
            OnAttackAnimationHit?.Invoke();
            LogDebug("Attack hit point reached");
        }

        /// <summary>
        /// 공격 애니메이션 종료 (Animation Event)
        /// </summary>
        public void OnAttackEnd()
        {
            isAttacking = false;
            animator?.SetBool(isAttackingParameter, false);
            OnAttackAnimationEnd?.Invoke();
            LogDebug("Attack animation ended");
        }

        #endregion

        #region 유틸리티

        /// <summary>
        /// 현재 애니메이션 상태 확인
        /// </summary>
        public bool IsInState(string stateName)
        {
            if (animator == null) return false;

            var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            return stateInfo.IsName(stateName);
        }

        /// <summary>
        /// 애니메이션 정규화 시간 가져오기
        /// </summary>
        public float GetNormalizedTime()
        {
            if (animator == null) return 0f;

            var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            return stateInfo.normalizedTime;
        }

        /// <summary>
        /// 특정 파라미터 값 설정
        /// </summary>
        public void SetAnimatorParameter(string parameterName, object value)
        {
            if (animator == null) return;

            if (value is int intValue)
            {
                animator.SetInteger(parameterName, intValue);
            }
            else if (value is float floatValue)
            {
                animator.SetFloat(parameterName, floatValue);
            }
            else if (value is bool boolValue)
            {
                animator.SetBool(parameterName, boolValue);
            }
            else if (value is string)
            {
                animator.SetTrigger(parameterName);
            }
        }

        #endregion

        #region 설정

        /// <summary>
        /// Animator 설정
        /// </summary>
        public void SetAnimator(Animator anim)
        {
            animator = anim;
        }

        /// <summary>
        /// ComboSystem 설정
        /// </summary>
        public void SetComboSystem(ComboSystem combo)
        {
            // 기존 이벤트 구독 해제
            if (comboSystem != null)
            {
                comboSystem.OnComboStarted -= OnComboStarted;
                comboSystem.OnComboAdvanced -= OnComboAdvanced;
                comboSystem.OnComboReset -= OnComboReset;
            }

            comboSystem = combo;

            // 새 이벤트 구독
            if (comboSystem != null)
            {
                comboSystem.OnComboStarted += OnComboStarted;
                comboSystem.OnComboAdvanced += OnComboAdvanced;
                comboSystem.OnComboReset += OnComboReset;
            }
        }

        /// <summary>
        /// 기본 공격 속도 설정
        /// </summary>
        public void SetDefaultAttackSpeed(float speed)
        {
            defaultAttackSpeed = Mathf.Max(0.1f, speed);
        }

        #endregion

        #region 디버그

        private void LogDebug(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[AttackAnimationHandler - {gameObject.name}] {message}");
            }
        }

        #endregion
    }
}
