using GASPT.Core.Enums;
using UnityEngine;

namespace GASPT.Stats
{
    /// <summary>
    /// PlayerStats 마나 관련 partial class
    /// </summary>
    public partial class PlayerStats
    {
        // ====== Mana 관리 ======

        /// <summary>
        /// 마나를 소비합니다
        /// </summary>
        /// <param name="amount">소비할 마나량</param>
        /// <returns>true: 소비 성공, false: 마나 부족</returns>
        public bool TrySpendMana(int amount)
        {
            if (amount < 0)
            {
                Debug.LogWarning($"[PlayerStats] TrySpendMana(): 유효하지 않은 값입니다: {amount}");
                return false;
            }

            if (amount == 0)
            {
                return true; // 0 마나는 항상 성공
            }

            if (currentMana < amount)
            {
                Debug.LogWarning($"[PlayerStats] TrySpendMana(): 마나 부족 (필요: {amount}, 현재: {currentMana})");
                return false;
            }

            int previousMana = currentMana;
            currentMana -= amount;

            // 이벤트 발생
            OnStatsChanged?.Invoke(StatType.Mana, previousMana, currentMana);

            return true;
        }

        /// <summary>
        /// 마나를 회복합니다
        /// </summary>
        /// <param name="amount">회복할 마나량</param>
        public void RegenerateMana(int amount)
        {
            if (amount <= 0)
            {
                Debug.LogWarning($"[PlayerStats] RegenerateMana(): 유효하지 않은 회복량입니다: {amount}");
                return;
            }

            int previousMana = currentMana;
            currentMana += amount;
            currentMana = Mathf.Min(currentMana, MaxMana);

            int actualRegenerated = currentMana - previousMana;

            // 이벤트 발생
            OnStatsChanged?.Invoke(StatType.Mana, previousMana, currentMana);
        }
    }
}
