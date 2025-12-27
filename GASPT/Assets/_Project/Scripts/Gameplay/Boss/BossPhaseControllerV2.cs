using System;
using UnityEngine;
using GASPT.Data;

namespace GASPT.Gameplay.Boss
{
    /// <summary>
    /// 보스 페이즈 컨트롤러 V2
    /// BossData 기반 페이즈 전환 및 스탯 배율 관리
    /// </summary>
    public class BossPhaseControllerV2
    {
        // ====== 데이터 ======

        private readonly BossData bossData;
        private readonly BossPhaseData[] phases;


        // ====== 상태 ======

        private int currentPhaseIndex = 0;
        private int previousPhaseIndex = 0;


        // ====== 이벤트 ======

        /// <summary>
        /// 페이즈 변경 이벤트 (새 페이즈 인덱스)
        /// </summary>
        public event Action<int> OnPhaseChanged;


        // ====== 프로퍼티 ======

        /// <summary>
        /// 현재 페이즈 인덱스 (0부터 시작)
        /// </summary>
        public int CurrentPhaseIndex => currentPhaseIndex;

        /// <summary>
        /// 현재 페이즈 번호 (1부터 시작)
        /// </summary>
        public int CurrentPhase => currentPhaseIndex + 1;

        /// <summary>
        /// 총 페이즈 수
        /// </summary>
        public int TotalPhases => phases?.Length ?? 1;

        /// <summary>
        /// 현재 페이즈 데이터
        /// </summary>
        public BossPhaseData CurrentPhaseData =>
            phases != null && currentPhaseIndex < phases.Length
                ? phases[currentPhaseIndex]
                : default;


        // ====== 생성자 ======

        public BossPhaseControllerV2(BossData data)
        {
            bossData = data;
            phases = data?.phases;

            if (phases == null || phases.Length == 0)
            {
                Debug.LogWarning("[BossPhaseControllerV2] 페이즈 데이터가 없습니다. 기본 페이즈 사용.");
                phases = new BossPhaseData[] { BossPhaseData.CreateDefault(1, 1f) };
            }

            currentPhaseIndex = 0;
            previousPhaseIndex = 0;
        }


        // ====== 페이즈 업데이트 ======

        /// <summary>
        /// 체력 비율에 따라 페이즈 업데이트
        /// </summary>
        /// <param name="healthRatio">현재 체력 비율 (0~1)</param>
        public void UpdatePhase(float healthRatio)
        {
            int newPhaseIndex = DeterminePhaseIndex(healthRatio);

            if (newPhaseIndex != currentPhaseIndex)
            {
                ChangePhase(newPhaseIndex);
            }
        }

        /// <summary>
        /// 체력 비율로 페이즈 인덱스 결정
        /// </summary>
        private int DeterminePhaseIndex(float healthRatio)
        {
            if (phases == null) return 0;

            // 역순으로 탐색 (마지막 페이즈부터)
            for (int i = phases.Length - 1; i >= 0; i--)
            {
                if (healthRatio <= phases[i].healthThreshold)
                {
                    return i;
                }
            }

            return 0;
        }

        /// <summary>
        /// 페이즈 변경
        /// </summary>
        private void ChangePhase(int newPhaseIndex)
        {
            previousPhaseIndex = currentPhaseIndex;
            currentPhaseIndex = newPhaseIndex;

            Debug.Log($"[BossPhaseControllerV2] 페이즈 전환: Phase {previousPhaseIndex + 1} → Phase {currentPhaseIndex + 1}");

            OnPhaseChanged?.Invoke(currentPhaseIndex);
        }


        // ====== 스탯 배율 ======

        /// <summary>
        /// 현재 페이즈의 공격력 배율
        /// </summary>
        public float GetAttackMultiplier()
        {
            return CurrentPhaseData.attackMultiplier > 0
                ? CurrentPhaseData.attackMultiplier
                : 1f;
        }

        /// <summary>
        /// 현재 페이즈의 이동속도 배율
        /// </summary>
        public float GetSpeedMultiplier()
        {
            return CurrentPhaseData.speedMultiplier > 0
                ? CurrentPhaseData.speedMultiplier
                : 1f;
        }

        /// <summary>
        /// 현재 페이즈의 공격속도 배율
        /// </summary>
        public float GetAttackSpeedMultiplier()
        {
            return CurrentPhaseData.attackSpeedMultiplier > 0
                ? CurrentPhaseData.attackSpeedMultiplier
                : 1f;
        }

        /// <summary>
        /// 현재 페이즈에서 사용 가능한 패턴 인덱스 목록
        /// </summary>
        public int[] GetAvailablePatternIndices()
        {
            return CurrentPhaseData.availablePatternIndices ?? new int[0];
        }


        // ====== 유틸리티 ======

        /// <summary>
        /// 특정 패턴 인덱스가 현재 페이즈에서 사용 가능한지 확인
        /// </summary>
        public bool IsPatternAvailable(int patternIndex)
        {
            var available = GetAvailablePatternIndices();
            if (available == null || available.Length == 0)
                return true; // 설정이 없으면 모든 패턴 사용 가능

            foreach (var idx in available)
            {
                if (idx == patternIndex)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 강제 페이즈 전환 (디버그용)
        /// </summary>
        public void ForcePhase(int phaseIndex)
        {
            if (phaseIndex < 0 || phaseIndex >= phases.Length)
            {
                Debug.LogWarning($"[BossPhaseControllerV2] 유효하지 않은 페이즈 인덱스: {phaseIndex}");
                return;
            }

            if (phaseIndex != currentPhaseIndex)
            {
                ChangePhase(phaseIndex);
            }
        }

        /// <summary>
        /// 디버그 정보 문자열
        /// </summary>
        public string GetDebugInfo()
        {
            var data = CurrentPhaseData;
            return $"Phase: {CurrentPhase}/{TotalPhases}, " +
                   $"ATK: x{data.attackMultiplier:F1}, " +
                   $"SPD: x{data.speedMultiplier:F1}, " +
                   $"AtkSpd: x{data.attackSpeedMultiplier:F1}";
        }
    }
}
