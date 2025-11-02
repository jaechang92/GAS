using UnityEngine;

namespace GASPT.Combat
{
    /// <summary>
    /// 데미지 계산 유틸리티 클래스
    /// 공격/방어 계산 로직을 중앙화하여 관리
    /// </summary>
    public static class DamageCalculator
    {
        // ====== 데미지 계산 상수 ======

        /// <summary>
        /// 공격 데미지 최소 배율 (100%)
        /// </summary>
        private const float MinDamageMultiplier = 1.0f;

        /// <summary>
        /// 공격 데미지 최대 배율 (120%)
        /// </summary>
        private const float MaxDamageMultiplier = 1.2f;

        /// <summary>
        /// 방어력 계수 (방어력 1당 받는 데미지 감소량)
        /// </summary>
        private const float DefenseMultiplier = 0.5f;

        /// <summary>
        /// 최소 데미지 (완전 무효화 방지)
        /// </summary>
        private const int MinDamage = 1;


        // ====== 공격 데미지 계산 ======

        /// <summary>
        /// 공격자가 입힐 데미지를 계산합니다
        /// 공식: attackStat × 랜덤 배율 (1.0 ~ 1.2)
        /// </summary>
        /// <param name="attackStat">공격자의 공격력 스탯</param>
        /// <returns>계산된 데미지 (정수)</returns>
        public static int CalculateDamageDealt(int attackStat)
        {
            if (attackStat <= 0)
            {
                Debug.LogWarning($"[DamageCalculator] 공격력이 0 이하입니다: {attackStat}. 최소 데미지 {MinDamage} 반환.");
                return MinDamage;
            }

            // 랜덤 배율 적용 (1.0 ~ 1.2)
            float randomMultiplier = Random.Range(MinDamageMultiplier, MaxDamageMultiplier);

            // 최종 데미지 계산 (소수점 반올림)
            int finalDamage = Mathf.RoundToInt(attackStat * randomMultiplier);

            // 최소 데미지 보장
            finalDamage = Mathf.Max(finalDamage, MinDamage);

            return finalDamage;
        }


        // ====== 방어 데미지 계산 ======

        /// <summary>
        /// 방어력을 고려하여 실제로 받을 데미지를 계산합니다
        /// 공식: max(1, incomingDamage - defenseStat × 0.5)
        /// </summary>
        /// <param name="incomingDamage">들어오는 데미지</param>
        /// <param name="defenseStat">방어자의 방어력 스탯</param>
        /// <returns>방어력 적용 후 실제 받을 데미지 (정수)</returns>
        public static int CalculateDamageReceived(int incomingDamage, int defenseStat)
        {
            if (incomingDamage <= 0)
            {
                Debug.LogWarning($"[DamageCalculator] 들어오는 데미지가 0 이하입니다: {incomingDamage}. 데미지 0 반환.");
                return 0;
            }

            if (defenseStat < 0)
            {
                Debug.LogWarning($"[DamageCalculator] 방어력이 음수입니다: {defenseStat}. 방어력 0으로 처리.");
                defenseStat = 0;
            }

            // 방어력에 의한 데미지 감소 계산
            float damageReduction = defenseStat * DefenseMultiplier;

            // 최종 받는 데미지 계산
            int finalDamage = Mathf.RoundToInt(incomingDamage - damageReduction);

            // 최소 데미지 보장 (완전 무효화 방지)
            finalDamage = Mathf.Max(finalDamage, MinDamage);

            return finalDamage;
        }


        // ====== 전투 시뮬레이션 (테스트용) ======

        /// <summary>
        /// 전투 시뮬레이션: 공격자 → 방어자
        /// 공격 데미지 계산 + 방어력 적용을 한 번에 처리
        /// </summary>
        /// <param name="attackerAttackStat">공격자의 공격력</param>
        /// <param name="defenderDefenseStat">방어자의 방어력</param>
        /// <returns>방어자가 실제로 받을 데미지</returns>
        public static int SimulateCombat(int attackerAttackStat, int defenderDefenseStat)
        {
            // 1단계: 공격 데미지 계산
            int damageDealt = CalculateDamageDealt(attackerAttackStat);

            // 2단계: 방어력 적용
            int damageReceived = CalculateDamageReceived(damageDealt, defenderDefenseStat);

            Debug.Log($"[DamageCalculator] 전투 시뮬레이션: " +
                      $"공격력 {attackerAttackStat} → 데미지 {damageDealt} → " +
                      $"방어력 {defenderDefenseStat} 적용 → 최종 데미지 {damageReceived}");

            return damageReceived;
        }


        // ====== 유틸리티 메서드 ======

        /// <summary>
        /// 데미지 계산 공식 정보를 반환합니다 (디버깅/UI 표시용)
        /// </summary>
        public static string GetFormulaInfo()
        {
            return $"공격 데미지: AttackStat × ({MinDamageMultiplier}~{MaxDamageMultiplier})\n" +
                   $"받는 데미지: max({MinDamage}, IncomingDamage - Defense × {DefenseMultiplier})";
        }
    }
}
