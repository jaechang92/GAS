using UnityEngine;
using GASPT.Core.Enums;
using GASPT.Data;

namespace GASPT.Combat
{
    /// <summary>
    /// 스테이지별 스탯 스케일링 계산기
    /// 스테이지가 올라갈수록 몬스터가 강해지는 수치를 계산
    /// </summary>
    public static class StageScalingCalculator
    {
        // 스케일링 상수 (spec 기준)
        public const float HP_SCALE_PER_STAGE = 0.15f;      // 스테이지당 HP 15% 증가
        public const float ATTACK_SCALE_PER_STAGE = 0.10f;  // 스테이지당 공격력 10% 증가
        public const float GOLD_SCALE_PER_STAGE = 0.20f;    // 스테이지당 골드 20% 증가
        public const float EXP_SCALE_PER_STAGE = 0.15f;     // 스테이지당 경험치 15% 증가

        // 난이도 배율
        public static class DifficultyMultiplier
        {
            public const float EASY_HP = 0.7f;
            public const float EASY_ATTACK = 0.7f;
            public const float EASY_REWARD = 0.8f;

            public const float NORMAL_HP = 1.0f;
            public const float NORMAL_ATTACK = 1.0f;
            public const float NORMAL_REWARD = 1.0f;

            public const float HARD_HP = 1.5f;
            public const float HARD_ATTACK = 1.3f;
            public const float HARD_REWARD = 1.2f;
        }

        /// <summary>
        /// 스테이지에 따른 스케일링 배율 계산
        /// </summary>
        /// <param name="baseValue">기본값</param>
        /// <param name="stage">현재 스테이지 (1-5)</param>
        /// <param name="scalePerStage">스테이지당 증가율</param>
        /// <returns>스케일링된 값</returns>
        public static float CalculateScaledValue(float baseValue, int stage, float scalePerStage)
        {
            // Stage 1 기준으로 계산 (Stage 1 = 1배, Stage 2 = 1 + 0.15, ...)
            float multiplier = 1f + (stage - 1) * scalePerStage;
            return baseValue * multiplier;
        }

        /// <summary>
        /// 스케일링된 HP 계산
        /// </summary>
        public static int CalculateScaledHP(int baseHP, int stage)
        {
            float scaled = CalculateScaledValue(baseHP, stage, HP_SCALE_PER_STAGE);
            return Mathf.RoundToInt(scaled);
        }

        /// <summary>
        /// 스케일링된 공격력 계산
        /// </summary>
        public static int CalculateScaledAttack(int baseAttack, int stage)
        {
            float scaled = CalculateScaledValue(baseAttack, stage, ATTACK_SCALE_PER_STAGE);
            return Mathf.RoundToInt(scaled);
        }

        /// <summary>
        /// 스케일링된 골드 계산
        /// </summary>
        public static int CalculateScaledGold(int baseGold, int stage)
        {
            float scaled = CalculateScaledValue(baseGold, stage, GOLD_SCALE_PER_STAGE);
            return Mathf.RoundToInt(scaled);
        }

        /// <summary>
        /// 스케일링된 경험치 계산
        /// </summary>
        public static int CalculateScaledExp(int baseExp, int stage)
        {
            float scaled = CalculateScaledValue(baseExp, stage, EXP_SCALE_PER_STAGE);
            return Mathf.RoundToInt(scaled);
        }

        /// <summary>
        /// EnemyData 기반 전체 스케일링 계산
        /// </summary>
        public static ScaledEnemyStats CalculateScaledStats(EnemyData data, int currentStage)
        {
            if (data == null)
            {
                Debug.LogError("[StageScalingCalculator] EnemyData가 null입니다.");
                return new ScaledEnemyStats();
            }

            return new ScaledEnemyStats
            {
                maxHp = CalculateScaledHP(data.maxHp, currentStage),
                attack = CalculateScaledAttack(data.attack, currentStage),
                minGold = CalculateScaledGold(data.minGoldDrop, currentStage),
                maxGold = CalculateScaledGold(data.maxGoldDrop, currentStage),
                exp = CalculateScaledExp(data.expReward, currentStage)
            };
        }

        /// <summary>
        /// 난이도 적용 스탯 계산
        /// </summary>
        /// <param name="stats">스케일링된 스탯</param>
        /// <param name="difficulty">난이도 (0=Easy, 1=Normal, 2=Hard)</param>
        public static ScaledEnemyStats ApplyDifficulty(ScaledEnemyStats stats, int difficulty)
        {
            float hpMult, attackMult, rewardMult;

            switch (difficulty)
            {
                case 0: // Easy
                    hpMult = DifficultyMultiplier.EASY_HP;
                    attackMult = DifficultyMultiplier.EASY_ATTACK;
                    rewardMult = DifficultyMultiplier.EASY_REWARD;
                    break;
                case 2: // Hard
                    hpMult = DifficultyMultiplier.HARD_HP;
                    attackMult = DifficultyMultiplier.HARD_ATTACK;
                    rewardMult = DifficultyMultiplier.HARD_REWARD;
                    break;
                default: // Normal
                    return stats;
            }

            return new ScaledEnemyStats
            {
                maxHp = Mathf.RoundToInt(stats.maxHp * hpMult),
                attack = Mathf.RoundToInt(stats.attack * attackMult),
                minGold = Mathf.RoundToInt(stats.minGold * rewardMult),
                maxGold = Mathf.RoundToInt(stats.maxGold * rewardMult),
                exp = Mathf.RoundToInt(stats.exp * rewardMult)
            };
        }

        /// <summary>
        /// 스테이지 스케일링 미리보기 (에디터/디버그용)
        /// </summary>
        public static void PrintScalingPreview(EnemyData data)
        {
            if (data == null)
            {
                Debug.LogError("[StageScalingCalculator] EnemyData가 null입니다.");
                return;
            }

            Debug.Log($"=== {data.enemyName} 스테이지별 스케일링 ===");
            Debug.Log($"Base: HP={data.maxHp}, Attack={data.attack}, Gold={data.minGoldDrop}-{data.maxGoldDrop}, EXP={data.expReward}");

            for (int stage = 1; stage <= 5; stage++)
            {
                var scaled = CalculateScaledStats(data, stage);
                Debug.Log($"Stage {stage}: HP={scaled.maxHp}, Attack={scaled.attack}, Gold={scaled.minGold}-{scaled.maxGold}, EXP={scaled.exp}");
            }
        }
    }

    /// <summary>
    /// 스케일링된 적 스탯 구조체
    /// </summary>
    public struct ScaledEnemyStats
    {
        public int maxHp;
        public int attack;
        public int minGold;
        public int maxGold;
        public int exp;

        /// <summary>
        /// 랜덤 골드 드랍
        /// </summary>
        public int GetRandomGold()
        {
            return Random.Range(minGold, maxGold + 1);
        }

        public override string ToString()
        {
            return $"HP={maxHp}, Attack={attack}, Gold={minGold}-{maxGold}, EXP={exp}";
        }
    }
}
