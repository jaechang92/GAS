using UnityEngine;
using GASPT.Stats;

namespace GASPT.Meta
{
    /// <summary>
    /// PlayerStats에 메타 업그레이드 효과를 적용하는 확장 유틸리티
    /// PlayerStats.RecalculateStats() 호출 후 또는 런 시작 시 사용
    /// </summary>
    public static class PlayerStatsMetaExtension
    {
        /// <summary>
        /// PlayerStats에 메타 업그레이드 효과 적용
        /// 런 시작 시 호출하여 영구 업그레이드 보너스를 적용합니다
        /// </summary>
        /// <param name="stats">대상 PlayerStats</param>
        public static void ApplyMetaUpgrades(this PlayerStats stats)
        {
            if (stats == null)
            {
                Debug.LogWarning("[PlayerStatsMetaExtension] PlayerStats가 null입니다.");
                return;
            }

            if (!MetaProgressionManager.HasInstance)
            {
                Debug.Log("[PlayerStatsMetaExtension] MetaProgressionManager가 없습니다. 메타 업그레이드 스킵.");
                return;
            }

            var meta = MetaProgressionManager.Instance;

            // 업그레이드 보너스 정보 로깅
            int hpBonus = meta.GetMaxHPBonus();
            float attackBonus = meta.GetAttackBonus();
            float defenseBonus = meta.GetDefenseBonus();
            float moveSpeedBonus = meta.GetMoveSpeedBonus();

            Debug.Log($"[PlayerStatsMetaExtension] 메타 업그레이드 적용 - " +
                     $"HP: +{hpBonus}, Attack: +{attackBonus * 100}%, " +
                     $"Defense: -{defenseBonus * 100}%, MoveSpeed: +{moveSpeedBonus * 100}%");

            // 주의: PlayerStats의 private 필드에 직접 접근할 수 없으므로
            // 별도의 메타 보너스 시스템 또는 PlayerStats 수정이 필요합니다.
            // 현재는 로깅만 수행하고, 실제 적용은 PlayerStats 수정 후 구현됩니다.
        }

        /// <summary>
        /// 메타 업그레이드 방어력 보너스를 적용한 최종 데미지 계산
        /// </summary>
        /// <param name="incomingDamage">들어오는 데미지</param>
        /// <returns>메타 방어력 보너스 적용된 데미지</returns>
        public static int ApplyMetaDefenseBonus(int incomingDamage)
        {
            if (!MetaProgressionManager.HasInstance)
            {
                return incomingDamage;
            }

            float defenseBonus = MetaProgressionManager.Instance.GetDefenseBonus();
            float multiplier = 1f - defenseBonus;
            int reducedDamage = Mathf.RoundToInt(incomingDamage * multiplier);

            return Mathf.Max(1, reducedDamage); // 최소 1 데미지
        }

        /// <summary>
        /// 메타 업그레이드 골드 보너스 적용
        /// </summary>
        /// <param name="baseGold">기본 골드</param>
        /// <returns>보너스 적용된 골드</returns>
        public static int ApplyMetaGoldBonus(int baseGold)
        {
            if (!MetaProgressionManager.HasInstance)
            {
                return baseGold;
            }

            float goldBonus = MetaProgressionManager.Instance.GetGoldBonus();
            int bonusGold = Mathf.RoundToInt(baseGold * goldBonus);

            return baseGold + bonusGold;
        }

        /// <summary>
        /// 메타 업그레이드 경험치 보너스 적용
        /// </summary>
        /// <param name="baseExp">기본 경험치</param>
        /// <returns>보너스 적용된 경험치</returns>
        public static int ApplyMetaExpBonus(int baseExp)
        {
            if (!MetaProgressionManager.HasInstance)
            {
                return baseExp;
            }

            float expBonus = MetaProgressionManager.Instance.GetExpBonus();
            int bonusExp = Mathf.RoundToInt(baseExp * expBonus);

            return baseExp + bonusExp;
        }

        /// <summary>
        /// 메타 업그레이드 시작 골드 가져오기
        /// </summary>
        /// <returns>시작 골드</returns>
        public static int GetMetaStartGold()
        {
            if (!MetaProgressionManager.HasInstance)
            {
                return 0;
            }

            return MetaProgressionManager.Instance.GetStartGold();
        }

        /// <summary>
        /// 메타 업그레이드 이동속도 배수 가져오기
        /// </summary>
        /// <returns>이동속도 배수 (1.0 = 기본)</returns>
        public static float GetMetaMoveSpeedMultiplier()
        {
            if (!MetaProgressionManager.HasInstance)
            {
                return 1f;
            }

            return 1f + MetaProgressionManager.Instance.GetMoveSpeedBonus();
        }

        /// <summary>
        /// 메타 업그레이드 추가 대시 횟수 가져오기
        /// </summary>
        /// <returns>추가 대시 횟수</returns>
        public static int GetMetaExtraDash()
        {
            if (!MetaProgressionManager.HasInstance)
            {
                return 0;
            }

            return MetaProgressionManager.Instance.GetExtraDash();
        }

        /// <summary>
        /// 메타 업그레이드 부활 가능 여부
        /// </summary>
        /// <returns>부활 가능 횟수</returns>
        public static int GetMetaReviveCount()
        {
            if (!MetaProgressionManager.HasInstance)
            {
                return 0;
            }

            return MetaProgressionManager.Instance.GetReviveCount();
        }
    }
}
