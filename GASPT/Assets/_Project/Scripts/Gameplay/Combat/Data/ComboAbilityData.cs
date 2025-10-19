using UnityEngine;
using GAS.Core;
using Core.Enums;

namespace Combat.Data
{
    /// <summary>
    /// 콤보 공격 어빌리티 데이터 (ScriptableObject)
    /// GAS AbilityData를 상속하여 VFX/사운드/애니메이션 자동 지원
    /// </summary>
    [CreateAssetMenu(fileName = "ComboAttack", menuName = "GASPT/Abilities/ComboAttack")]
    public class ComboAbilityData : AbilityData
    {
        [Header("=== Combo 설정 ===")]
        [Tooltip("콤보 단계 인덱스 (0:1단, 1:2단, 2:3단)")]
        public int comboIndex = 0;

        [Tooltip("데미지 배율 (기본 데미지 * 배율)")]
        [Range(0.5f, 3f)]
        public float damageMultiplier = 1.0f;

        [Header("=== Hitbox 설정 ===")]
        [Tooltip("히트박스 크기 (Width x Height)")]
        public Vector2 hitboxSize = new Vector2(1.5f, 1f);

        [Tooltip("히트박스 오프셋 (플레이어 기준 상대 위치)")]
        public Vector2 hitboxOffset = new Vector2(0.5f, 0f);

        [Tooltip("히트박스 지속 시간 (초)")]
        [Range(0.1f, 1f)]
        public float hitboxDuration = 0.2f;

        [Tooltip("히트박스 생성 딜레이 (애니메이션 싱크)")]
        [Range(0f, 0.5f)]
        public float hitboxSpawnDelay = 0.1f;

        [Header("=== Knockback 설정 ===")]
        [Tooltip("넉백 세기 (수평 방향)")]
        [Range(0f, 20f)]
        public float knockbackForce = 5f;

        [Tooltip("스턴 지속 시간 (초)")]
        [Range(0f, 2f)]
        public float stunDuration = 0.3f;

        [Header("=== 타겟 설정 ===")]
        [Tooltip("타겟 레이어")]
        public LayerMask targetLayers = ~0;

        [Tooltip("다중 타겟 허용")]
        public bool hitMultipleTargets = true;

        [Tooltip("최대 타겟 수 (다중 타겟 시)")]
        [Range(1, 10)]
        public int maxTargets = 5;

        [Header("=== VFX/사운드/애니메이션 (AbilityData 상속) ===")]
        // EffectPrefab (부모 클래스)
        // SoundEffect (부모 클래스)
        // AnimationTrigger (부모 클래스)

        [Header("=== 디버그 ===")]
        [Tooltip("히트박스 Gizmo 표시")]
        public bool showGizmos = true;

        [Tooltip("디버그 로그 출력")]
        public bool debugLog = false;

        /// <summary>
        /// 최종 데미지 계산 (부모 클래스의 DamageValue 사용)
        /// </summary>
        public float GetFinalDamage()
        {
            return DamageValue * damageMultiplier;
        }

        /// <summary>
        /// 콤보 단계별 스턴 시간 계산
        /// </summary>
        public float GetStunDuration()
        {
            // 콤보 단계가 높을수록 스턴 시간 증가
            return stunDuration + (comboIndex * 0.1f);
        }

        /// <summary>
        /// 에디터 검증
        /// </summary>
        private void OnValidate()
        {
            // 콤보 인덱스 범위 제한
            if (comboIndex < 0) comboIndex = 0;
            if (comboIndex > 2) comboIndex = 2;

            // AbilityId 자동 설정 (없으면)
            if (string.IsNullOrEmpty(AbilityId))
            {
                AbilityId = $"Combo_{comboIndex}";
            }

            // AbilityName 자동 설정 (없으면)
            if (string.IsNullOrEmpty(AbilityName))
            {
                string[] comboNames = { "1단 공격", "2단 공격", "3단 공격" };
                if (comboIndex >= 0 && comboIndex < comboNames.Length)
                {
                    AbilityName = comboNames[comboIndex];
                }
            }
        }
    }
}
