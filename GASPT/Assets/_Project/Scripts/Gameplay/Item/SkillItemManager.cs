using System;
using System.Collections.Generic;
using UnityEngine;
using GASPT.Data;
using GASPT.Core.Enums;
using GASPT.Gameplay.Form;
using GASPT.Loot;

namespace GASPT.Gameplay.Item
{
    /// <summary>
    /// 스킬 아이템 관리 싱글톤
    /// V1(SkillItem) 및 V2(SkillItemData) 모두 지원
    /// ItemDropManager와 연동하여 아이템 획득 시 자동 장착
    /// </summary>
    public class SkillItemManager : SingletonManager<SkillItemManager>
    {
        // ====== 참조 ======

        [Header("현재 Form")]
        [SerializeField] private GameObject currentFormObject;
        private IFormController currentForm;


        // ====== 장착된 스킬 (V2 우선) ======

        private Dictionary<int, SkillItemData> equippedSkillsV2 = new Dictionary<int, SkillItemData>();
        private Dictionary<int, SkillItem> equippedSkillsV1 = new Dictionary<int, SkillItem>();


        // ====== 설정 ======

        [Header("설정")]
        [Tooltip("디버그 로그 출력 여부")]
        [SerializeField] private bool debugLog = true;


        // ====== 이벤트 (V2) ======

        /// <summary>
        /// 스킬 장착 시 발생하는 이벤트 (V2)
        /// 매개변수: (int slotIndex, SkillItemData skillItemData)
        /// </summary>
        public event Action<int, SkillItemData> OnSkillEquippedV2;

        /// <summary>
        /// 스킬 해제 시 발생하는 이벤트 (V2)
        /// 매개변수: (int slotIndex)
        /// </summary>
        public event Action<int> OnSkillUnequippedV2;


        // ====== 이벤트 (V1 레거시) ======

        /// <summary>
        /// 스킬 장착 시 발생하는 이벤트 (V1 레거시)
        /// 매개변수: (int slotIndex, SkillItem skillItem)
        /// </summary>
        [Obsolete("Use OnSkillEquippedV2 instead")]
        public event Action<int, SkillItem> OnSkillEquipped;

        /// <summary>
        /// 스킬 해제 시 발생하는 이벤트 (V1 레거시)
        /// 매개변수: (int slotIndex)
        /// </summary>
        [Obsolete("Use OnSkillUnequippedV2 instead")]
        public event Action<int> OnSkillUnequipped;


        // ====== 초기화 ======

        protected override void OnAwake()
        {
            // V2: ItemDropManager 이벤트 구독
            if (ItemDropManager.HasInstance)
            {
                ItemDropManager.Instance.OnItemPickedUp += OnItemPickedUpHandlerV2;
                Debug.Log("[SkillItemManager] ItemDropManager(V2) 이벤트 구독 완료");
            }

            // V1 폴백: LootSystem 이벤트 구독
#pragma warning disable CS0618
            if (LootSystem.HasInstance)
            {
                LootSystem.Instance.OnItemPickedUp += OnItemPickedUpHandlerV1;
                Debug.Log("[SkillItemManager] LootSystem(V1) 이벤트 구독 완료");
            }
#pragma warning restore CS0618

            Debug.Log("[SkillItemManager] 초기화 완료");
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            // V2 이벤트 구독 해제
            if (ItemDropManager.HasInstance)
            {
                ItemDropManager.Instance.OnItemPickedUp -= OnItemPickedUpHandlerV2;
            }

            // V1 이벤트 구독 해제
#pragma warning disable CS0618
            if (LootSystem.HasInstance)
            {
                LootSystem.Instance.OnItemPickedUp -= OnItemPickedUpHandlerV1;
            }
#pragma warning restore CS0618
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


        // ====== 스킬 장착/해제 (V2) ======

        /// <summary>
        /// 스킬 아이템 장착 (V2)
        /// </summary>
        /// <param name="skillItemData">장착할 스킬 아이템 데이터</param>
        /// <returns>장착 성공 여부</returns>
        public bool EquipSkillItemV2(SkillItemData skillItemData)
        {
            if (skillItemData == null)
            {
                Debug.LogWarning("[SkillItemManager] skillItemData가 null입니다.");
                return false;
            }

            if (currentForm == null)
            {
                Debug.LogWarning("[SkillItemManager] 현재 Form이 설정되지 않았습니다. SetCurrentForm()을 먼저 호출하세요.");
                return false;
            }

            // 유효성 검증
            if (!skillItemData.Validate())
            {
                Debug.LogError($"[SkillItemManager] {skillItemData.itemName}: 유효하지 않은 스킬 아이템입니다.");
                return false;
            }

            int slotIndex = skillItemData.targetSlotIndex;

            // IAbility 인스턴스 생성
            IAbility ability = skillItemData.CreateAbilityInstance();

            if (ability == null)
            {
                Debug.LogError($"[SkillItemManager] {skillItemData.itemName}: Ability 인스턴스를 생성할 수 없습니다.");
                return false;
            }

            // Form에 스킬 설정
            currentForm.SetAbility(slotIndex, ability);

            // Dictionary에 저장
            equippedSkillsV2[slotIndex] = skillItemData;

            // 이벤트 발생
            OnSkillEquippedV2?.Invoke(slotIndex, skillItemData);

            if (debugLog)
                Debug.Log($"[SkillItemManager] 스킬 장착 완료 (V2): {skillItemData.itemName} → 슬롯 {slotIndex}");

            return true;
        }

        /// <summary>
        /// 스킬 아이템 해제 (V2)
        /// </summary>
        /// <param name="slotIndex">해제할 슬롯 인덱스</param>
        /// <returns>해제 성공 여부</returns>
        public bool UnequipSkillItemV2(int slotIndex)
        {
            if (currentForm == null)
            {
                Debug.LogWarning("[SkillItemManager] 현재 Form이 설정되지 않았습니다.");
                return false;
            }

            // V2 Dictionary에서 확인
            if (!equippedSkillsV2.ContainsKey(slotIndex) && !equippedSkillsV1.ContainsKey(slotIndex))
            {
                Debug.LogWarning($"[SkillItemManager] 슬롯 {slotIndex}에 장착된 스킬이 없습니다.");
                return false;
            }

            // Form에서 스킬 제거
            currentForm.SetAbility(slotIndex, null);

            // Dictionary에서 제거
            equippedSkillsV2.Remove(slotIndex);
            equippedSkillsV1.Remove(slotIndex);

            // 이벤트 발생
            OnSkillUnequippedV2?.Invoke(slotIndex);

            if (debugLog)
                Debug.Log($"[SkillItemManager] 스킬 해제 완료: 슬롯 {slotIndex}");

            return true;
        }


        // ====== 스킬 장착/해제 (V1 레거시) ======

        /// <summary>
        /// 스킬 아이템 장착 (V1 레거시)
        /// </summary>
        /// <param name="skillItem">장착할 스킬 아이템</param>
        /// <returns>장착 성공 여부</returns>
        [Obsolete("Use EquipSkillItemV2 instead")]
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
            equippedSkillsV1[slotIndex] = skillItem;

            // 이벤트 발생
#pragma warning disable CS0618
            OnSkillEquipped?.Invoke(slotIndex, skillItem);
#pragma warning restore CS0618

            if (debugLog)
                Debug.Log($"[SkillItemManager] 스킬 장착 완료 (V1): {skillItem.itemName} → 슬롯 {slotIndex}");

            return true;
        }

        /// <summary>
        /// 스킬 아이템 해제 (V1 레거시)
        /// </summary>
        /// <param name="slotIndex">해제할 슬롯 인덱스</param>
        /// <returns>해제 성공 여부</returns>
        [Obsolete("Use UnequipSkillItemV2 instead")]
        public bool UnequipSkillItem(int slotIndex)
        {
            return UnequipSkillItemV2(slotIndex);
        }


        // ====== 조회 메서드 (V2) ======

        /// <summary>
        /// 장착된 스킬 가져오기 (V2)
        /// </summary>
        /// <param name="slotIndex">슬롯 인덱스</param>
        /// <returns>장착된 SkillItemData (없으면 null)</returns>
        public SkillItemData GetEquippedSkillV2(int slotIndex)
        {
            if (equippedSkillsV2.TryGetValue(slotIndex, out SkillItemData data))
            {
                return data;
            }

            return null;
        }

        /// <summary>
        /// 모든 장착된 스킬 가져오기 (V2)
        /// </summary>
        /// <returns>장착된 스킬 Dictionary</returns>
        public Dictionary<int, SkillItemData> GetAllEquippedSkillsV2()
        {
            return new Dictionary<int, SkillItemData>(equippedSkillsV2);
        }


        // ====== 조회 메서드 (V1 레거시) ======

        /// <summary>
        /// 장착된 스킬 가져오기 (V1 레거시)
        /// </summary>
        /// <param name="slotIndex">슬롯 인덱스</param>
        /// <returns>장착된 SkillItem (없으면 null)</returns>
        [Obsolete("Use GetEquippedSkillV2 instead")]
        public SkillItem GetEquippedSkill(int slotIndex)
        {
            if (equippedSkillsV1.TryGetValue(slotIndex, out SkillItem item))
            {
                return item;
            }

            return null;
        }

        /// <summary>
        /// 모든 장착된 스킬 가져오기 (V1 레거시)
        /// </summary>
        /// <returns>장착된 스킬 Dictionary</returns>
        [Obsolete("Use GetAllEquippedSkillsV2 instead")]
        public Dictionary<int, SkillItem> GetAllEquippedSkills()
        {
            return new Dictionary<int, SkillItem>(equippedSkillsV1);
        }


        // ====== ItemDropManager(V2) 연동 ======

        /// <summary>
        /// 아이템 획득 이벤트 핸들러 (V2)
        /// SkillItemData인 경우 자동으로 장착
        /// </summary>
        private void OnItemPickedUpHandlerV2(ItemInstance itemInstance, PickupResult result)
        {
            if (result != PickupResult.Success)
                return;

            if (itemInstance == null || !itemInstance.IsValid)
                return;

            // SkillItemData인지 확인
            SkillItemData skillItemData = itemInstance.cachedItemData as SkillItemData;

            if (skillItemData != null)
            {
                if (debugLog)
                    Debug.Log($"[SkillItemManager] 스킬 아이템 획득 (V2): {skillItemData.itemName}");

                // 자동 장착
                EquipSkillItemV2(skillItemData);
            }
        }


        // ====== LootSystem(V1) 연동 ======

        /// <summary>
        /// 아이템 획득 이벤트 핸들러 (V1)
        /// SkillItem인 경우 자동으로 장착
        /// </summary>
        private void OnItemPickedUpHandlerV1(GASPT.Data.Item item)
        {
            // SkillItem인지 확인
            SkillItem skillItem = item as SkillItem;

            if (skillItem != null)
            {
                if (debugLog)
                    Debug.Log($"[SkillItemManager] 스킬 아이템 획득 (V1): {skillItem.itemName}");

                // 자동 장착
#pragma warning disable CS0618
                EquipSkillItem(skillItem);
#pragma warning restore CS0618
            }
        }


        // ====== Context Menu (테스트용) ======

        [ContextMenu("Print Equipped Skills")]
        private void PrintEquippedSkills()
        {
            Debug.Log("========== Equipped Skills ==========");
            Debug.Log($"Current Form: {currentForm?.FormName ?? "None"}");

            Debug.Log("--- V2 Skills ---");
            if (equippedSkillsV2.Count == 0)
            {
                Debug.Log("  (없음)");
            }
            else
            {
                foreach (var kvp in equippedSkillsV2)
                {
                    Debug.Log($"  Slot {kvp.Key}: {kvp.Value.itemName} ({kvp.Value.abilityType})");
                }
            }

            Debug.Log("--- V1 Skills (Legacy) ---");
            if (equippedSkillsV1.Count == 0)
            {
                Debug.Log("  (없음)");
            }
            else
            {
                foreach (var kvp in equippedSkillsV1)
                {
                    Debug.Log($"  Slot {kvp.Key}: {kvp.Value.itemName} ({kvp.Value.abilityType})");
                }
            }

            Debug.Log("====================================");
        }

        [ContextMenu("Print System Info")]
        private void PrintSystemInfo()
        {
            Debug.Log("========== SkillItemManager Info ==========");
            Debug.Log($"Current Form: {currentForm?.FormName ?? "None"}");
            Debug.Log($"Equipped Skills V2 Count: {equippedSkillsV2.Count}");
            Debug.Log($"Equipped Skills V1 Count: {equippedSkillsV1.Count}");
            Debug.Log($"Debug Log: {debugLog}");
            Debug.Log("==========================================");
        }
    }
}
