using UnityEngine;
using GASPT.Skills;

namespace GASPT.UI
{
    /// <summary>
    /// 스킬 UI 패널 (4개 슬롯 관리)
    /// SkillSystem 이벤트 구독 및 실시간 업데이트
    /// </summary>
    public class SkillUIPanel : MonoBehaviour
    {
        [Header("스킬 슬롯")]
        [SerializeField] [Tooltip("4개의 SkillSlotUI")]
        private SkillSlotUI[] skillSlots = new SkillSlotUI[4];


        // ====== Unity 생명주기 ======

        private void Start()
        {
            // 슬롯 인덱스 자동 설정
            for (int i = 0; i < skillSlots.Length; i++)
            {
                if (skillSlots[i] != null)
                {
                    skillSlots[i].SetSlotIndex(i);
                }
            }

            // SkillSystem 이벤트 구독
            SubscribeToSkillSystemEvents();

            // 초기 스킬 로드 (이미 등록된 스킬이 있으면)
            LoadExistingSkills();
        }

        private void OnDestroy()
        {
            // 이벤트 구독 해제
            UnsubscribeFromSkillSystemEvents();
        }


        // ====== SkillSystem 이벤트 구독 ======

        private void SubscribeToSkillSystemEvents()
        {
            if (!SkillSystem.HasInstance)
            {
                Debug.LogWarning("[SkillUIPanel] SkillSystem이 초기화되지 않았습니다.");
                return;
            }

            SkillSystem skillSystem = SkillSystem.Instance;

            // 중복 구독 방지
            skillSystem.OnSkillRegistered -= OnSkillRegistered;
            skillSystem.OnSkillUsed -= OnSkillUsed;
            skillSystem.OnCooldownChanged -= OnCooldownChanged;

            // 구독
            skillSystem.OnSkillRegistered += OnSkillRegistered;
            skillSystem.OnSkillUsed += OnSkillUsed;
            skillSystem.OnCooldownChanged += OnCooldownChanged;

            Debug.Log("[SkillUIPanel] SkillSystem 이벤트 구독 완료");
        }

        private void UnsubscribeFromSkillSystemEvents()
        {
            if (SkillSystem.HasInstance)
            {
                SkillSystem skillSystem = SkillSystem.Instance;

                skillSystem.OnSkillRegistered -= OnSkillRegistered;
                skillSystem.OnSkillUsed -= OnSkillUsed;
                skillSystem.OnCooldownChanged -= OnCooldownChanged;

                Debug.Log("[SkillUIPanel] SkillSystem 이벤트 구독 해제");
            }
        }


        // ====== SkillSystem 이벤트 핸들러 ======

        /// <summary>
        /// 스킬 등록 시 호출
        /// </summary>
        private void OnSkillRegistered(int slotIndex, Skill skill)
        {
            Debug.Log($"[SkillUIPanel] 슬롯 {slotIndex}에 스킬 등록: {skill.Data.skillName}");

            if (!IsValidSlotIndex(slotIndex))
            {
                Debug.LogError($"[SkillUIPanel] 유효하지 않은 슬롯 인덱스: {slotIndex}");
                return;
            }

            // UI 업데이트
            if (skillSlots[slotIndex] != null)
            {
                skillSlots[slotIndex].RegisterSkill(skill);
            }
        }

        /// <summary>
        /// 스킬 사용 시 호출
        /// </summary>
        private void OnSkillUsed(int slotIndex, Skill skill)
        {
            Debug.Log($"[SkillUIPanel] 슬롯 {slotIndex} 스킬 사용: {skill.Data.skillName}");

            // UI는 자동으로 쿨다운 표시 (Update에서 처리)
        }

        /// <summary>
        /// 쿨다운 변경 시 호출
        /// </summary>
        private void OnCooldownChanged(int slotIndex, Skill skill)
        {
            // UI는 자동으로 쿨다운 표시 (Update에서 처리)
        }


        // ====== 초기 스킬 로드 ======

        /// <summary>
        /// 이미 등록된 스킬 로드 (Start 시 호출)
        /// </summary>
        private void LoadExistingSkills()
        {
            if (!SkillSystem.HasInstance)
            {
                return;
            }

            SkillSystem skillSystem = SkillSystem.Instance;

            for (int i = 0; i < skillSlots.Length; i++)
            {
                Skill skill = skillSystem.GetSkill(i);
                if (skill != null && skillSlots[i] != null)
                {
                    skillSlots[i].RegisterSkill(skill);
                    Debug.Log($"[SkillUIPanel] 기존 스킬 로드: 슬롯 {i} - {skill.Data.skillName}");
                }
            }
        }


        // ====== 유틸리티 ======

        private bool IsValidSlotIndex(int index)
        {
            return index >= 0 && index < skillSlots.Length;
        }

        /// <summary>
        /// 특정 슬롯 UI 가져오기
        /// </summary>
        public SkillSlotUI GetSlotUI(int slotIndex)
        {
            if (IsValidSlotIndex(slotIndex))
            {
                return skillSlots[slotIndex];
            }
            return null;
        }


        // ====== Context Menu (테스트용) ======

        [ContextMenu("Print Slot Status")]
        private void PrintSlotStatus()
        {
            Debug.Log("========== SkillSlotUI 상태 ==========");
            for (int i = 0; i < skillSlots.Length; i++)
            {
                if (skillSlots[i] != null)
                {
                    bool isRegistered = skillSlots[i].IsRegistered;
                    string skillName = isRegistered ? skillSlots[i].CurrentSkill.Data.skillName : "비어있음";
                    Debug.Log($"슬롯 {i}: {skillName} (등록: {isRegistered})");
                }
                else
                {
                    Debug.Log($"슬롯 {i}: SkillSlotUI 컴포넌트 없음");
                }
            }
            Debug.Log("=====================================");
        }

        [ContextMenu("Reload All Skills")]
        private void ReloadAllSkills()
        {
            Debug.Log("[SkillUIPanel] 모든 스킬 재로드");
            LoadExistingSkills();
        }
    }
}
