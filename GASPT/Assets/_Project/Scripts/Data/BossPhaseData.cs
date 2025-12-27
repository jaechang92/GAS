using System;
using UnityEngine;

namespace GASPT.Data
{
    /// <summary>
    /// 보스 페이즈 데이터 구조체
    /// 페이즈별 체력 임계값, 스탯 배율, 사용 가능 패턴 정의
    /// </summary>
    [Serializable]
    public struct BossPhaseData
    {
        [Header("페이즈 기본 정보")]
        [Tooltip("페이즈 번호 (1, 2, 3...)")]
        public int phaseIndex;

        [Tooltip("페이즈 전환 체력 비율 (0.7 = 70% 이하일 때 전환)")]
        [Range(0f, 1f)]
        public float healthThreshold;

        [Header("스탯 배율")]
        [Tooltip("공격력 배율 (1.0 = 100%)")]
        [Range(0.5f, 3f)]
        public float attackMultiplier;

        [Tooltip("이동속도 배율 (1.0 = 100%)")]
        [Range(0.5f, 2f)]
        public float speedMultiplier;

        [Tooltip("공격 속도 배율 (1.0 = 100%, 높을수록 빠름)")]
        [Range(0.5f, 2f)]
        public float attackSpeedMultiplier;

        [Header("페이즈 전환 연출")]
        [Tooltip("페이즈 전환 시 무적 시간 (초)")]
        [Range(0f, 3f)]
        public float invulnerabilityDuration;

        [Tooltip("페이즈 전환 시 카메라 흔들림 강도")]
        [Range(0f, 1f)]
        public float cameraShakeIntensity;

        [Header("패턴 설정")]
        [Tooltip("이 페이즈에서 사용 가능한 패턴 인덱스 목록")]
        public int[] availablePatternIndices;

        /// <summary>
        /// 기본값으로 초기화된 BossPhaseData 생성
        /// </summary>
        public static BossPhaseData CreateDefault(int phaseIndex, float healthThreshold)
        {
            return new BossPhaseData
            {
                phaseIndex = phaseIndex,
                healthThreshold = healthThreshold,
                attackMultiplier = 1f + (phaseIndex - 1) * 0.2f,
                speedMultiplier = 1f + (phaseIndex - 1) * 0.1f,
                attackSpeedMultiplier = 1f + (phaseIndex - 1) * 0.15f,
                invulnerabilityDuration = 1.5f,
                cameraShakeIntensity = 0.3f * phaseIndex,
                availablePatternIndices = new int[0]
            };
        }

        /// <summary>
        /// 미니보스용 기본 페이즈 (2 페이즈)
        /// </summary>
        public static BossPhaseData[] CreateMiniBossPhases()
        {
            return new BossPhaseData[]
            {
                new BossPhaseData
                {
                    phaseIndex = 1,
                    healthThreshold = 1f,
                    attackMultiplier = 1f,
                    speedMultiplier = 1f,
                    attackSpeedMultiplier = 1f,
                    invulnerabilityDuration = 1f,
                    cameraShakeIntensity = 0.2f,
                    availablePatternIndices = new int[] { 0, 1 }
                },
                new BossPhaseData
                {
                    phaseIndex = 2,
                    healthThreshold = 0.5f,
                    attackMultiplier = 1.3f,
                    speedMultiplier = 1.2f,
                    attackSpeedMultiplier = 1.3f,
                    invulnerabilityDuration = 1.5f,
                    cameraShakeIntensity = 0.4f,
                    availablePatternIndices = new int[] { 0, 1, 2 }
                }
            };
        }

        /// <summary>
        /// 중간보스용 기본 페이즈 (3 페이즈)
        /// </summary>
        public static BossPhaseData[] CreateMidBossPhases()
        {
            return new BossPhaseData[]
            {
                new BossPhaseData
                {
                    phaseIndex = 1,
                    healthThreshold = 1f,
                    attackMultiplier = 1f,
                    speedMultiplier = 1f,
                    attackSpeedMultiplier = 1f,
                    invulnerabilityDuration = 1f,
                    cameraShakeIntensity = 0.2f,
                    availablePatternIndices = new int[] { 0, 1 }
                },
                new BossPhaseData
                {
                    phaseIndex = 2,
                    healthThreshold = 0.7f,
                    attackMultiplier = 1.2f,
                    speedMultiplier = 1.1f,
                    attackSpeedMultiplier = 1.2f,
                    invulnerabilityDuration = 1.5f,
                    cameraShakeIntensity = 0.3f,
                    availablePatternIndices = new int[] { 0, 1, 2 }
                },
                new BossPhaseData
                {
                    phaseIndex = 3,
                    healthThreshold = 0.4f,
                    attackMultiplier = 1.5f,
                    speedMultiplier = 1.2f,
                    attackSpeedMultiplier = 1.4f,
                    invulnerabilityDuration = 2f,
                    cameraShakeIntensity = 0.5f,
                    availablePatternIndices = new int[] { 0, 1, 2, 3 }
                }
            };
        }

        /// <summary>
        /// 최종보스용 기본 페이즈 (4 페이즈)
        /// </summary>
        public static BossPhaseData[] CreateFinalBossPhases()
        {
            return new BossPhaseData[]
            {
                new BossPhaseData
                {
                    phaseIndex = 1,
                    healthThreshold = 1f,
                    attackMultiplier = 1f,
                    speedMultiplier = 1f,
                    attackSpeedMultiplier = 1f,
                    invulnerabilityDuration = 1f,
                    cameraShakeIntensity = 0.2f,
                    availablePatternIndices = new int[] { 0, 1 }
                },
                new BossPhaseData
                {
                    phaseIndex = 2,
                    healthThreshold = 0.7f,
                    attackMultiplier = 1.2f,
                    speedMultiplier = 1.1f,
                    attackSpeedMultiplier = 1.2f,
                    invulnerabilityDuration = 1.5f,
                    cameraShakeIntensity = 0.3f,
                    availablePatternIndices = new int[] { 0, 1, 2 }
                },
                new BossPhaseData
                {
                    phaseIndex = 3,
                    healthThreshold = 0.4f,
                    attackMultiplier = 1.4f,
                    speedMultiplier = 1.2f,
                    attackSpeedMultiplier = 1.3f,
                    invulnerabilityDuration = 2f,
                    cameraShakeIntensity = 0.5f,
                    availablePatternIndices = new int[] { 0, 1, 2, 3 }
                },
                new BossPhaseData
                {
                    phaseIndex = 4,
                    healthThreshold = 0.15f,
                    attackMultiplier = 1.7f,
                    speedMultiplier = 1.3f,
                    attackSpeedMultiplier = 1.5f,
                    invulnerabilityDuration = 2.5f,
                    cameraShakeIntensity = 0.7f,
                    availablePatternIndices = new int[] { 0, 1, 2, 3, 4 }
                }
            };
        }
    }
}
