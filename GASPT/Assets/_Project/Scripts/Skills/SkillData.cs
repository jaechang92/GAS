using UnityEngine;
using GASPT.Data;

namespace GASPT.Skills
{
    /// <summary>
    /// 스킬 데이터 ScriptableObject
    /// 스킬의 모든 설정 및 효과 정의
    /// </summary>
    [CreateAssetMenu(fileName = "SkillData", menuName = "GASPT/Skills/Skill")]
    public class SkillData : ScriptableObject
    {
        // ====== 타입 ======

        [Header("타입")]
        [Tooltip("스킬 타입 (Damage/Heal/Buff/Utility)")]
        public SkillType skillType;

        [Tooltip("타겟 타입 (Self/Enemy/Area/Ally)")]
        public TargetType targetType;


        // ====== 기본 정보 ======

        [Header("기본 정보")]
        [Tooltip("스킬 표시 이름")]
        public string skillName;

        [Tooltip("스킬 설명")]
        [TextArea(2, 4)]
        public string description;

        [Tooltip("스킬 아이콘 (UI 표시용)")]
        public Sprite icon;


        // ====== 비용 및 쿨다운 ======

        [Header("비용 및 쿨다운")]
        [Tooltip("마나 소비량")]
        [Range(0, 100)]
        public int manaCost = 10;

        [Tooltip("쿨다운 시간 (초)")]
        [Range(0f, 60f)]
        public float cooldown = 3f;

        [Tooltip("캐스팅 시간 (초, 0이면 즉시 발동)")]
        [Range(0f, 5f)]
        public float castTime = 0f;


        // ====== 효과 수치 ======

        [Header("효과 수치")]
        [Tooltip("데미지 값 (Damage 타입일 때)")]
        [Range(0, 1000)]
        public int damageAmount = 0;

        [Tooltip("회복량 (Heal 타입일 때)")]
        [Range(0, 1000)]
        public int healAmount = 0;

        [Tooltip("적용할 상태 이상 효과 (Buff 타입일 때, 선택사항)")]
        public StatusEffectData statusEffect;


        // ====== 범위 및 타겟팅 ======

        [Header("범위 및 타겟팅")]
        [Tooltip("스킬 사거리 (0이면 무제한)")]
        [Range(0f, 50f)]
        public float skillRange = 10f;

        [Tooltip("범위 반경 (Area 타입일 때)")]
        [Range(0f, 20f)]
        public float areaRadius = 0f;

        [Tooltip("최대 타겟 수 (Area 타입일 때)")]
        [Range(1, 20)]
        public int maxTargets = 5;


        // ====== 애니메이션 ======

        [Header("애니메이션")]
        [Tooltip("애니메이터 트리거 이름 (선택사항)")]
        public string animationTrigger;


        // ====== 시각/사운드 효과 ======

        [Header("시각/사운드 효과")]
        [Tooltip("파티클 이펙트 (선택사항)")]
        public GameObject particleEffect;

        [Tooltip("사운드 클립 (선택사항)")]
        public AudioClip soundClip;

        [Tooltip("효과 색상 (UI 표시용)")]
        public Color effectColor = Color.cyan;


        // ====== 검증 ======

        private void OnValidate()
        {
            // skillName이 비어있으면 파일명으로 자동 설정
            if (string.IsNullOrEmpty(skillName))
            {
                skillName = name;
            }

            // Damage 타입인데 damageAmount가 0이면 경고
            if (skillType == SkillType.Damage && damageAmount == 0)
            {
                Debug.LogWarning($"[SkillData] {skillName}: Damage 타입인데 damageAmount가 0입니다.");
            }

            // Heal 타입인데 healAmount가 0이면 경고
            if (skillType == SkillType.Heal && healAmount == 0)
            {
                Debug.LogWarning($"[SkillData] {skillName}: Heal 타입인데 healAmount가 0입니다.");
            }

            // Buff 타입인데 statusEffect가 null이면 경고
            if (skillType == SkillType.Buff && statusEffect == null)
            {
                Debug.LogWarning($"[SkillData] {skillName}: Buff 타입인데 statusEffect가 없습니다.");
            }

            // Area 타입인데 areaRadius가 0이면 경고
            if (targetType == TargetType.Area && areaRadius == 0)
            {
                Debug.LogWarning($"[SkillData] {skillName}: Area 타입인데 areaRadius가 0입니다.");
            }
        }
    }
}
