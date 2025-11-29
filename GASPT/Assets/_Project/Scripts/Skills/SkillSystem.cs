using System;
using System.Collections.Generic;
using UnityEngine;

namespace GASPT.Skills
{
    /// <summary>
    /// 스킬 시스템 싱글톤
    /// 플레이어의 스킬 슬롯 관리 (등록, 사용, 쿨다운 추적)
    /// </summary>
    public class SkillSystem : SingletonManager<SkillSystem>
    {
        // ====== 스킬 슬롯 ======

        [Header("스킬 슬롯 설정")]
        [SerializeField] [Tooltip("최대 스킬 슬롯 수")]
        private int maxSlots = 4;

        /// <summary>
        /// 스킬 슬롯 (슬롯 인덱스 → Skill)
        /// </summary>
        private Dictionary<int, Skill> skillSlots = new Dictionary<int, Skill>();


        // ====== 플레이어 참조 ======

        /// <summary>
        /// 플레이어 GameObject (스킬 시전자)
        /// </summary>
        private GameObject player;


        // ====== 이벤트 ======

        /// <summary>
        /// 스킬 등록 시 발생
        /// 매개변수: (슬롯 인덱스, Skill)
        /// </summary>
        public event Action<int, Skill> OnSkillRegistered;

        /// <summary>
        /// 스킬 사용 시 발생
        /// 매개변수: (슬롯 인덱스, Skill)
        /// </summary>
        public event Action<int, Skill> OnSkillUsed;

        /// <summary>
        /// 스킬 사용 실패 시 발생
        /// 매개변수: (슬롯 인덱스, Skill, 이유)
        /// </summary>
        public event Action<int, Skill, string> OnSkillFailed;

        /// <summary>
        /// 쿨다운 변경 시 발생 (UI 업데이트용)
        /// 매개변수: (슬롯 인덱스, Skill)
        /// </summary>
        public event Action<int, Skill> OnCooldownChanged;


        // ====== Unity 생명주기 ======

        protected override void OnAwake()
        {
            skillSlots = new Dictionary<int, Skill>();

            // Player는 Content Scene에 있으므로 지연 참조
            // Awake 시점에는 Player가 없을 수 있음

            Debug.Log($"[SkillSystem] 초기화 완료 - 최대 슬롯 수: {maxSlots}");
        }

        /// <summary>
        /// Player GameObject 지연 획득
        /// </summary>
        private GameObject GetPlayer()
        {
            if (player == null)
            {
                player = GameObject.FindWithTag("Player");
            }
            return player;
        }


        // ====== 스킬 등록 ======

        /// <summary>
        /// 스킬 슬롯에 스킬 등록
        /// </summary>
        /// <param name="slotIndex">슬롯 인덱스 (0부터 시작)</param>
        /// <param name="skillData">등록할 스킬 데이터</param>
        /// <returns>true: 등록 성공, false: 등록 실패</returns>
        public bool RegisterSkill(int slotIndex, SkillData skillData)
        {
            // 슬롯 인덱스 유효성 검사
            if (!IsValidSlotIndex(slotIndex))
            {
                Debug.LogError($"[SkillSystem] RegisterSkill(): 유효하지 않은 슬롯 인덱스입니다: {slotIndex} (최대: {maxSlots - 1})");
                return false;
            }

            if (skillData == null)
            {
                Debug.LogError($"[SkillSystem] RegisterSkill(): skillData가 null입니다.");
                return false;
            }

            // 기존 스킬 제거
            if (skillSlots.ContainsKey(slotIndex))
            {
                Skill oldSkill = skillSlots[slotIndex];
                Debug.Log($"[SkillSystem] 슬롯 {slotIndex}의 {oldSkill.Data.skillName}을(를) {skillData.skillName}(으)로 교체합니다.");

                // 이벤트 구독 해제
                UnsubscribeFromSkillEvents(slotIndex, oldSkill);
            }

            // 새 스킬 생성 및 등록
            Skill newSkill = new Skill(skillData);
            skillSlots[slotIndex] = newSkill;

            // 이벤트 구독
            SubscribeToSkillEvents(slotIndex, newSkill);

            // 이벤트 발생
            OnSkillRegistered?.Invoke(slotIndex, newSkill);

            Debug.Log($"[SkillSystem] 슬롯 {slotIndex}에 {skillData.skillName} 등록 완료!");
            return true;
        }

        /// <summary>
        /// 스킬 슬롯 해제
        /// </summary>
        /// <param name="slotIndex">해제할 슬롯 인덱스</param>
        /// <returns>true: 해제 성공, false: 해제 실패</returns>
        public bool UnregisterSkill(int slotIndex)
        {
            if (!skillSlots.ContainsKey(slotIndex))
            {
                Debug.LogWarning($"[SkillSystem] UnregisterSkill(): 슬롯 {slotIndex}에 스킬이 없습니다.");
                return false;
            }

            Skill skill = skillSlots[slotIndex];
            UnsubscribeFromSkillEvents(slotIndex, skill);
            skillSlots.Remove(slotIndex);

            Debug.Log($"[SkillSystem] 슬롯 {slotIndex}의 {skill.Data.skillName} 해제 완료");
            return true;
        }


        // ====== 스킬 사용 ======

        /// <summary>
        /// 스킬 사용 시도
        /// </summary>
        /// <param name="slotIndex">슬롯 인덱스</param>
        /// <param name="target">타겟 GameObject (Enemy 타입인 경우)</param>
        /// <returns>true: 사용 성공, false: 사용 실패</returns>
        public bool TryUseSkill(int slotIndex, GameObject target = null)
        {
            // 슬롯 확인
            if (!skillSlots.ContainsKey(slotIndex))
            {
                Debug.LogWarning($"[SkillSystem] TryUseSkill(): 슬롯 {slotIndex}에 스킬이 등록되어 있지 않습니다.");
                return false;
            }

            // Player 지연 획득
            GameObject currentPlayer = GetPlayer();
            if (currentPlayer == null)
            {
                Debug.LogError($"[SkillSystem] TryUseSkill(): Player를 찾을 수 없습니다.");
                return false;
            }

            Skill skill = skillSlots[slotIndex];

            // 스킬 실행
            bool success = skill.TryExecute(currentPlayer, target);

            if (success)
            {
                // 성공 이벤트는 Skill 이벤트 구독에서 처리됨
            }

            return success;
        }


        // ====== 스킬 정보 조회 ======

        /// <summary>
        /// 특정 슬롯의 스킬 가져오기
        /// </summary>
        /// <param name="slotIndex">슬롯 인덱스</param>
        /// <returns>Skill 인스턴스 (없으면 null)</returns>
        public Skill GetSkill(int slotIndex)
        {
            skillSlots.TryGetValue(slotIndex, out Skill skill);
            return skill;
        }

        /// <summary>
        /// 특정 슬롯의 쿨다운 비율 (0.0 ~ 1.0)
        /// </summary>
        /// <param name="slotIndex">슬롯 인덱스</param>
        /// <returns>쿨다운 비율 (0 = 사용 가능, 1 = 쿨다운 시작)</returns>
        public float GetCooldownRatio(int slotIndex)
        {
            Skill skill = GetSkill(slotIndex);
            return skill?.GetCooldownRatio() ?? 0f;
        }

        /// <summary>
        /// 특정 슬롯의 쿨다운 중 여부
        /// </summary>
        public bool IsOnCooldown(int slotIndex)
        {
            Skill skill = GetSkill(slotIndex);
            return skill?.IsOnCooldown ?? false;
        }

        /// <summary>
        /// 모든 스킬 슬롯 정보 가져오기
        /// </summary>
        public Dictionary<int, Skill> GetAllSkills()
        {
            return new Dictionary<int, Skill>(skillSlots);
        }


        // ====== 이벤트 구독 ======

        /// <summary>
        /// Skill 이벤트 구독
        /// </summary>
        private void SubscribeToSkillEvents(int slotIndex, Skill skill)
        {
            skill.OnSkillUsed += (s) => OnSkillUsedHandler(slotIndex, s);
            skill.OnSkillFailed += (s, reason) => OnSkillFailedHandler(slotIndex, s, reason);
            skill.OnCooldownComplete += (s) => OnCooldownCompleteHandler(slotIndex, s);
        }

        /// <summary>
        /// Skill 이벤트 구독 해제
        /// </summary>
        private void UnsubscribeFromSkillEvents(int slotIndex, Skill skill)
        {
            skill.OnSkillUsed -= (s) => OnSkillUsedHandler(slotIndex, s);
            skill.OnSkillFailed -= (s, reason) => OnSkillFailedHandler(slotIndex, s, reason);
            skill.OnCooldownComplete -= (s) => OnCooldownCompleteHandler(slotIndex, s);
        }

        /// <summary>
        /// 스킬 사용 이벤트 핸들러
        /// </summary>
        private void OnSkillUsedHandler(int slotIndex, Skill skill)
        {
            Debug.Log($"[SkillSystem] 슬롯 {slotIndex} 스킬 사용: {skill.Data.skillName}");
            OnSkillUsed?.Invoke(slotIndex, skill);
            OnCooldownChanged?.Invoke(slotIndex, skill);
        }

        /// <summary>
        /// 스킬 사용 실패 이벤트 핸들러
        /// </summary>
        private void OnSkillFailedHandler(int slotIndex, Skill skill, string reason)
        {
            Debug.LogWarning($"[SkillSystem] 슬롯 {slotIndex} 스킬 사용 실패: {skill.Data.skillName} - {reason}");
            OnSkillFailed?.Invoke(slotIndex, skill, reason);
        }

        /// <summary>
        /// 쿨다운 완료 이벤트 핸들러
        /// </summary>
        private void OnCooldownCompleteHandler(int slotIndex, Skill skill)
        {
            Debug.Log($"[SkillSystem] 슬롯 {slotIndex} 쿨다운 완료: {skill.Data.skillName}");
            OnCooldownChanged?.Invoke(slotIndex, skill);
        }


        // ====== 유틸리티 ======

        /// <summary>
        /// 슬롯 인덱스 유효성 검사
        /// </summary>
        private bool IsValidSlotIndex(int slotIndex)
        {
            return slotIndex >= 0 && slotIndex < maxSlots;
        }

        /// <summary>
        /// 플레이어 GameObject 설정 (수동 설정용)
        /// </summary>
        public void SetPlayer(GameObject playerObject)
        {
            player = playerObject;
            Debug.Log($"[SkillSystem] 플레이어 설정: {playerObject.name}");
        }


        // ====== Context Menu (테스트용) ======

        [ContextMenu("Print Skill Slots")]
        private void PrintSkillSlots()
        {
            DebugPrintSkillSlots();
        }

        /// <summary>
        /// 스킬 슬롯 상태 출력 (public, 외부에서 호출 가능)
        /// </summary>
        public void DebugPrintSkillSlots()
        {
            Debug.Log("========== Skill Slots ==========");
            Debug.Log($"최대 슬롯 수: {maxSlots}");
            Debug.Log($"등록된 스킬 수: {skillSlots.Count}");

            foreach (var kvp in skillSlots)
            {
                Skill skill = kvp.Value;
                Debug.Log($"  - 슬롯 {kvp.Key}: {skill.Data.skillName} (쿨다운: {skill.IsOnCooldown})");
            }

            Debug.Log("=================================");
        }
    }
}
