using System;
using System.Collections.Generic;
using UnityEngine;
using GASPT.Data;
using GASPT.Gameplay.Form;
using GASPT.Loot;

namespace GASPT.Gameplay.Item
{
    /// <summary>
    /// 스킬 아이템 관리 싱글톤
    /// 스킬 아이템 획득 및 장착/해제 관리
    /// LootSystem과 연동하여 아이템 획득 시 자동 장착
    /// </summary>
    public class SkillItemManager : SingletonManager<SkillItemManager>
    {
        // ====== 참조 ======

        [Header("현재 Form")]
        [SerializeField] private GameObject currentFormObject;
        private IFormController currentForm;


        // ====== 장착된 스킬 ======

        private Dictionary<int, SkillItem> equippedSkills = new Dictionary<int, SkillItem>();


        // ====== 설정 ======

        [Header("설정")]
        [Tooltip("디버그 로그 출력 여부")]
        [SerializeField] private bool debugLog = true;


        // ====== 이벤트 ======

        /// <summary>
        /// 스킬 장착 시 발생하는 이벤트
        /// 매개변수: (int slotIndex, SkillItem skillItem)
        /// </summary>
        public event Action<int, SkillItem> OnSkillEquipped;

        /// <summary>
        /// 스킬 해제 시 발생하는 이벤트
        /// 매개변수: (int slotIndex)
        /// </summary>
        public event Action<int> OnSkillUnequipped;


        // ====== 초기화 ======

        protected override void OnAwake()
        {
            // LootSystem 이벤트 구독
            if (LootSystem.HasInstance)
            {
                LootSystem.Instance.OnItemPickedUp += OnItemPickedUpHandler;
                Debug.Log("[SkillItemManager] LootSystem 이벤트 구독 완료");
            }
            else
            {
                Debug.LogWarning("[SkillItemManager] LootSystem이 존재하지 않습니다.");
            }

            Debug.Log("[SkillItemManager] 초기화 완료");
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            // 이벤트 구독 해제
            if (LootSystem.HasInstance)
            {
                LootSystem.Instance.OnItemPickedUp -= OnItemPickedUpHandler;
            }
        }


        // ====== Form 설정 ======

        /// <summary>
        /// 현재 Form 설정
        /// </summary>
        /// <param name="form">IFormController 구현체</param>
        public void SetCurrentForm(IFormController form)
        {
            currentForm = form;

            if (form != null && debugLog)
                Debug.Log($"[SkillItemManager] 현재 Form 설정: {form.FormName}");
        }

        /// <summary>
        /// 현재 Form 설정 (GameObject에서 찾기)
        /// </summary>
        /// <param name="formObject">Form MonoBehaviour가 있는 GameObject</param>
        public void SetCurrentFormObject(GameObject formObject)
        {
            if (formObject == null)
            {
                Debug.LogWarning("[SkillItemManager] formObject가 null입니다.");
                return;
            }

            currentFormObject = formObject;

            // IFormController 찾기
            IFormController form = formObject.GetComponent<IFormController>();

            if (form != null)
            {
                SetCurrentForm(form);
            }
            else
            {
                Debug.LogError($"[SkillItemManager] {formObject.name}에서 IFormController를 찾을 수 없습니다!");
            }
        }


        // ====== 스킬 장착/해제 ======

        /// <summary>
        /// 스킬 아이템 장착
        /// </summary>
        /// <param name="skillItem">장착할 스킬 아이템</param>
        /// <returns>장착 성공 여부</returns>
        public bool EquipSkillItem(SkillItem skillItem)
        {
            if (skillItem == null)
            {
                Debug.LogWarning("[SkillItemManager] skillItem이 null입니다.");
                return false;
            }

            if (currentForm == null)
            {
                Debug.LogWarning("[SkillItemManager] 현재 Form이 설정되지 않았습니다. SetCurrentForm()을 먼저 호출하세요.");
                return false;
            }

            // 유효성 검증
            if (!skillItem.Validate())
            {
                Debug.LogError($"[SkillItemManager] {skillItem.itemName}: 유효하지 않은 스킬 아이템입니다.");
                return false;
            }

            int slotIndex = skillItem.targetSlotIndex;

            // IAbility 인스턴스 생성
            IAbility ability = skillItem.CreateAbilityInstance();

            if (ability == null)
            {
                Debug.LogError($"[SkillItemManager] {skillItem.itemName}: Ability 인스턴스를 생성할 수 없습니다.");
                return false;
            }

            // Form에 스킬 설정
            currentForm.SetAbility(slotIndex, ability);

            // Dictionary에 저장
            equippedSkills[slotIndex] = skillItem;

            // 이벤트 발생
            OnSkillEquipped?.Invoke(slotIndex, skillItem);

            if (debugLog)
                Debug.Log($"[SkillItemManager] 스킬 장착 완료: {skillItem.itemName} → 슬롯 {slotIndex}");

            return true;
        }

        /// <summary>
        /// 스킬 아이템 해제
        /// </summary>
        /// <param name="slotIndex">해제할 슬롯 인덱스</param>
        /// <returns>해제 성공 여부</returns>
        public bool UnequipSkillItem(int slotIndex)
        {
            if (currentForm == null)
            {
                Debug.LogWarning("[SkillItemManager] 현재 Form이 설정되지 않았습니다.");
                return false;
            }

            if (!equippedSkills.ContainsKey(slotIndex))
            {
                Debug.LogWarning($"[SkillItemManager] 슬롯 {slotIndex}에 장착된 스킬이 없습니다.");
                return false;
            }

            // Form에서 스킬 제거
            currentForm.SetAbility(slotIndex, null);

            // Dictionary에서 제거
            equippedSkills.Remove(slotIndex);

            // 이벤트 발생
            OnSkillUnequipped?.Invoke(slotIndex);

            if (debugLog)
                Debug.Log($"[SkillItemManager] 스킬 해제 완료: 슬롯 {slotIndex}");

            return true;
        }


        // ====== 조회 메서드 ======

        /// <summary>
        /// 장착된 스킬 가져오기
        /// </summary>
        /// <param name="slotIndex">슬롯 인덱스</param>
        /// <returns>장착된 SkillItem (없으면 null)</returns>
        public SkillItem GetEquippedSkill(int slotIndex)
        {
            if (equippedSkills.ContainsKey(slotIndex))
            {
                return equippedSkills[slotIndex];
            }

            return null;
        }

        /// <summary>
        /// 모든 장착된 스킬 가져오기
        /// </summary>
        /// <returns>장착된 스킬 Dictionary</returns>
        public Dictionary<int, SkillItem> GetAllEquippedSkills()
        {
            return new Dictionary<int, SkillItem>(equippedSkills);
        }


        // ====== LootSystem 연동 ======

        /// <summary>
        /// 아이템 획득 이벤트 핸들러
        /// SkillItem인 경우 자동으로 장착
        /// </summary>
        private void OnItemPickedUpHandler(GASPT.Data.Item item)
        {
            // SkillItem인지 확인
            SkillItem skillItem = item as SkillItem;

            if (skillItem != null)
            {
                if (debugLog)
                    Debug.Log($"[SkillItemManager] 스킬 아이템 획득: {skillItem.itemName}");

                // 자동 장착
                EquipSkillItem(skillItem);
            }
        }


        // ====== Context Menu (테스트용) ======

        [ContextMenu("Print Equipped Skills")]
        private void PrintEquippedSkills()
        {
            Debug.Log("========== Equipped Skills ==========");
            Debug.Log($"Current Form: {currentForm?.FormName ?? "None"}");

            if (equippedSkills.Count == 0)
            {
                Debug.Log("장착된 스킬 없음");
            }
            else
            {
                foreach (var kvp in equippedSkills)
                {
                    Debug.Log($"Slot {kvp.Key}: {kvp.Value.itemName} ({kvp.Value.abilityType})");
                }
            }

            Debug.Log("====================================");
        }

        [ContextMenu("Print System Info")]
        private void PrintSystemInfo()
        {
            Debug.Log("========== SkillItemManager Info ==========");
            Debug.Log($"Current Form: {currentForm?.FormName ?? "None"}");
            Debug.Log($"Equipped Skills Count: {equippedSkills.Count}");
            Debug.Log($"Debug Log: {debugLog}");
            Debug.Log("==========================================");
        }
    }
}
