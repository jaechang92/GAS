using System;
using UnityEngine;

namespace GASPT.Gameplay.Enemy
{
    /// <summary>
    /// 보스 Phase 상태
    /// HP 비율에 따라 자동 전환
    /// </summary>
    public enum BossPhase
    {
        /// <summary>
        /// Phase 1 (HP 100% ~ 70%)
        /// 기본 근접 공격 + 주기적인 원거리 공격
        /// </summary>
        Phase1,

        /// <summary>
        /// Phase 2 (HP 70% ~ 30%)
        /// 공격 속도 증가 + 돌진 공격 + 소환
        /// </summary>
        Phase2,

        /// <summary>
        /// Phase 3 (HP 30% ~ 0%)
        /// 광폭화 + 범위 공격
        /// </summary>
        Phase3
    }


    /// <summary>
    /// 보스 Phase 전환 컨트롤러
    /// HP 비율 기반 Phase 관리 및 스탯 배율 제공
    /// </summary>
    public class BossPhaseController
    {
        // ====== Phase 전환 임계값 ======

        private const float Phase2Threshold = 0.7f;  // 70%
        private const float Phase3Threshold = 0.3f;  // 30%


        // ====== Phase별 스탯 배율 ======

        private const float Phase1AttackMultiplier = 1.0f;
        private const float Phase1SpeedMultiplier = 1.0f;

        private const float Phase2AttackMultiplier = 1.2f;  // +20% 공격력
        private const float Phase2SpeedMultiplier = 1.3f;   // +30% 이동속도

        private const float Phase3AttackMultiplier = 1.5f;  // +50% 공격력
        private const float Phase3SpeedMultiplier = 1.3f;   // +30% 이동속도


        // ====== 상태 ======

        private BossPhase currentPhase = BossPhase.Phase1;
        private BossPhase previousPhase = BossPhase.Phase1;


        // ====== 이벤트 ======

        /// <summary>
        /// Phase 전환 이벤트 (newPhase)
        /// </summary>
        public event Action<BossPhase> OnPhaseChanged;


        // ====== 프로퍼티 ======

        public BossPhase CurrentPhase => currentPhase;
        public BossPhase PreviousPhase => previousPhase;


        // ====== 생성자 ======

        public BossPhaseController()
        {
            currentPhase = BossPhase.Phase1;
            previousPhase = BossPhase.Phase1;
        }


        // ====== Phase 업데이트 ======

        /// <summary>
        /// HP 비율 기반 Phase 업데이트
        /// </summary>
        /// <param name="currentHp">현재 HP</param>
        /// <param name="maxHp">최대 HP</param>
        public void UpdatePhase(int currentHp, int maxHp)
        {
            if (maxHp <= 0)
            {
                Debug.LogWarning("[BossPhaseController] maxHp가 0 이하입니다.");
                return;
            }

            float hpRatio = (float)currentHp / maxHp;

            BossPhase newPhase = DeterminePhase(hpRatio);

            if (newPhase != currentPhase)
            {
                ChangePhase(newPhase);
            }
        }

        /// <summary>
        /// HP 비율로 Phase 결정
        /// </summary>
        private BossPhase DeterminePhase(float hpRatio)
        {
            if (hpRatio > Phase2Threshold)
            {
                return BossPhase.Phase1;
            }
            else if (hpRatio > Phase3Threshold)
            {
                return BossPhase.Phase2;
            }
            else
            {
                return BossPhase.Phase3;
            }
        }

        /// <summary>
        /// Phase 전환
        /// </summary>
        private void ChangePhase(BossPhase newPhase)
        {
            previousPhase = currentPhase;
            currentPhase = newPhase;

            Debug.Log($"[BossPhaseController] Phase 전환: {previousPhase} → {currentPhase}");

            // 이벤트 발생
            OnPhaseChanged?.Invoke(currentPhase);
        }


        // ====== Phase별 스탯 배율 ======

        /// <summary>
        /// 현재 Phase의 공격력 배율 반환
        /// </summary>
        public float GetAttackMultiplier()
        {
            return currentPhase switch
            {
                BossPhase.Phase1 => Phase1AttackMultiplier,
                BossPhase.Phase2 => Phase2AttackMultiplier,
                BossPhase.Phase3 => Phase3AttackMultiplier,
                _ => 1.0f
            };
        }

        /// <summary>
        /// 현재 Phase의 이동속도 배율 반환
        /// </summary>
        public float GetSpeedMultiplier()
        {
            return currentPhase switch
            {
                BossPhase.Phase1 => Phase1SpeedMultiplier,
                BossPhase.Phase2 => Phase2SpeedMultiplier,
                BossPhase.Phase3 => Phase3SpeedMultiplier,
                _ => 1.0f
            };
        }


        // ====== 디버그 ======

        /// <summary>
        /// 현재 Phase 정보 출력
        /// </summary>
        public string GetDebugInfo()
        {
            return $"Phase: {currentPhase}, Attack: x{GetAttackMultiplier():F1}, Speed: x{GetSpeedMultiplier():F1}";
        }

        /// <summary>
        /// 강제 Phase 전환 (디버그/테스트용)
        /// </summary>
        public void ForcePhase(BossPhase phase)
        {
            if (phase != currentPhase)
            {
                ChangePhase(phase);
            }
        }
    }
}
