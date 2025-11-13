using UnityEngine;

namespace GASPT.Form
{
    /// <summary>
    /// 모든 폼의 기본 구현을 제공하는 추상 클래스
    /// 상속받아 구체적인 폼(MageForm, WarriorForm 등)을 구현
    /// </summary>
    public abstract class BaseForm : MonoBehaviour, IFormController
    {
        [Header("Form Settings")]
        [SerializeField] protected FormData formData;

        [Header("Abilities")]
        protected IAbility[] abilities = new IAbility[4];  // 0: 기본공격, 1-3: 스킬
        protected IAbility jumpAbility;  // 점프 (별도 관리)

        [Header("Debug")]
        [SerializeField] protected bool showDebugLogs = true;

        // IFormController 구현
        public abstract string FormName { get; }
        public abstract FormType FormType { get; }

        public virtual float MaxHealth => formData != null ? formData.maxHealth : 100f;
        public virtual float MoveSpeed => formData != null ? formData.moveSpeed : 5f;
        public virtual float JumpPower => formData != null ? formData.jumpPower : 10f;

        /// <summary>
        /// 폼 활성화 (폼 전환 시 호출)
        /// </summary>
        public virtual void Activate()
        {
            gameObject.SetActive(true);

            if (showDebugLogs)
                Debug.Log($"[BaseForm] {FormName} 활성화됨");

            OnFormActivated();
        }

        /// <summary>
        /// 폼 비활성화 (폼 전환 시 호출)
        /// </summary>
        public virtual void Deactivate()
        {
            OnFormDeactivated();

            if (showDebugLogs)
                Debug.Log($"[BaseForm] {FormName} 비활성화됨");

            gameObject.SetActive(false);
        }

        /// <summary>
        /// 스킬 슬롯에 어빌리티 설정
        /// </summary>
        public void SetAbility(int slotIndex, IAbility ability)
        {
            if (slotIndex < 0 || slotIndex >= abilities.Length)
            {
                Debug.LogWarning($"[BaseForm] 잘못된 슬롯 인덱스: {slotIndex}");
                return;
            }

            abilities[slotIndex] = ability;

            if (showDebugLogs)
                Debug.Log($"[BaseForm] {FormName} - 슬롯 {slotIndex}에 {ability?.AbilityName ?? "null"} 설정됨");
        }

        /// <summary>
        /// 스킬 슬롯에서 어빌리티 가져오기
        /// </summary>
        public IAbility GetAbility(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= abilities.Length)
            {
                Debug.LogWarning($"[BaseForm] 잘못된 슬롯 인덱스: {slotIndex}");
                return null;
            }

            return abilities[slotIndex];
        }

        /// <summary>
        /// 점프 어빌리티 가져오기
        /// </summary>
        public IAbility GetJumpAbility()
        {
            return jumpAbility;
        }

        /// <summary>
        /// 점프 어빌리티 설정
        /// </summary>
        public void SetJumpAbility(IAbility ability)
        {
            jumpAbility = ability;

            if (showDebugLogs)
                Debug.Log($"[BaseForm] {FormName} - 점프 어빌리티: {ability?.AbilityName ?? "null"}");
        }

        /// <summary>
        /// 폼 활성화 시 호출되는 가상 메서드 (오버라이드 가능)
        /// </summary>
        protected virtual void OnFormActivated()
        {
            // 하위 클래스에서 구현
        }

        /// <summary>
        /// 폼 비활성화 시 호출되는 가상 메서드 (오버라이드 가능)
        /// </summary>
        protected virtual void OnFormDeactivated()
        {
            // 하위 클래스에서 구현
        }

        /// <summary>
        /// 디버그용 - 현재 폼 정보 출력
        /// </summary>
        [ContextMenu("Print Form Info")]
        protected void PrintFormInfo()
        {
            Debug.Log($"=== {FormName} Info ===\n" +
                     $"Type: {FormType}\n" +
                     $"Stats: HP={MaxHealth}, Speed={MoveSpeed}, Jump={JumpPower}\n" +
                     $"Abilities:\n" +
                     $"  [0] {abilities[0]?.AbilityName ?? "Empty"}\n" +
                     $"  [1] {abilities[1]?.AbilityName ?? "Empty"}\n" +
                     $"  [2] {abilities[2]?.AbilityName ?? "Empty"}\n" +
                     $"  [3] {abilities[3]?.AbilityName ?? "Empty"}");
        }

        /// <summary>
        /// Unity 에디터 검증
        /// </summary>
        protected virtual void OnValidate()
        {
            if (formData == null)
            {
                Debug.LogWarning($"[BaseForm] {name}: FormData가 할당되지 않았습니다!");
            }
        }
    }
}
