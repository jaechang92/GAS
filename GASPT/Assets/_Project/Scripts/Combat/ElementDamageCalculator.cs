using UnityEngine;
using GASPT.Core.Enums;

namespace GASPT.Combat
{
    /// <summary>
    /// 속성 상성 데미지 계산기
    /// 공격 속성과 방어 속성에 따른 데미지 배율을 계산
    /// </summary>
    public static class ElementDamageCalculator
    {
        // 상성 배율 상수
        public const float STRONG_MULTIPLIER = 1.5f;  // 강함 (상성 우위)
        public const float WEAK_MULTIPLIER = 0.5f;   // 약함 (상성 열세)
        public const float NORMAL_MULTIPLIER = 1.0f; // 보통 (상성 없음)

        /// <summary>
        /// 속성 상성에 따른 데미지 배율 계산
        /// </summary>
        /// <param name="attackElement">공격 속성</param>
        /// <param name="defenseElement">방어 속성</param>
        /// <returns>데미지 배율 (0.5 ~ 1.5)</returns>
        public static float GetDamageMultiplier(ElementType attackElement, ElementType defenseElement)
        {
            // 무속성 공격은 항상 기본 배율
            if (attackElement == ElementType.None)
            {
                return NORMAL_MULTIPLIER;
            }

            // 무속성 방어는 항상 기본 배율
            if (defenseElement == ElementType.None)
            {
                return NORMAL_MULTIPLIER;
            }

            // 동일 속성은 약함 (저항)
            if (attackElement == defenseElement)
            {
                return WEAK_MULTIPLIER;
            }

            // 상성표에 따른 배율
            return GetElementMatchup(attackElement, defenseElement);
        }

        /// <summary>
        /// 속성 상성표에 따른 배율 반환
        /// </summary>
        private static float GetElementMatchup(ElementType attack, ElementType defense)
        {
            return attack switch
            {
                ElementType.Fire => defense switch
                {
                    ElementType.Ice => STRONG_MULTIPLIER,      // Fire > Ice
                    ElementType.Poison => STRONG_MULTIPLIER,   // Fire > Poison
                    _ => NORMAL_MULTIPLIER
                },

                ElementType.Ice => defense switch
                {
                    ElementType.Thunder => STRONG_MULTIPLIER,  // Ice > Thunder
                    ElementType.Earth => STRONG_MULTIPLIER,    // Ice > Earth
                    ElementType.Fire => WEAK_MULTIPLIER,       // Ice < Fire
                    _ => NORMAL_MULTIPLIER
                },

                ElementType.Thunder => defense switch
                {
                    ElementType.Ice => STRONG_MULTIPLIER,      // Thunder > Ice (빙결된 적에게 강함)
                    ElementType.Earth => WEAK_MULTIPLIER,      // Thunder < Earth (접지)
                    _ => NORMAL_MULTIPLIER
                },

                ElementType.Dark => defense switch
                {
                    ElementType.Light => STRONG_MULTIPLIER,    // Dark > Light
                    _ => NORMAL_MULTIPLIER
                },

                ElementType.Light => defense switch
                {
                    ElementType.Dark => STRONG_MULTIPLIER,     // Light > Dark
                    _ => NORMAL_MULTIPLIER
                },

                ElementType.Poison => defense switch
                {
                    ElementType.Earth => STRONG_MULTIPLIER,    // Poison > Earth
                    ElementType.Fire => WEAK_MULTIPLIER,       // Poison < Fire (소각)
                    _ => NORMAL_MULTIPLIER
                },

                ElementType.Earth => defense switch
                {
                    ElementType.Thunder => STRONG_MULTIPLIER,  // Earth > Thunder (접지)
                    ElementType.Fire => STRONG_MULTIPLIER,     // Earth > Fire (질식)
                    ElementType.Ice => WEAK_MULTIPLIER,        // Earth < Ice (동결)
                    _ => NORMAL_MULTIPLIER
                },

                _ => NORMAL_MULTIPLIER
            };
        }

        /// <summary>
        /// 최종 데미지 계산 (속성 상성 적용)
        /// </summary>
        /// <param name="baseDamage">기본 데미지</param>
        /// <param name="attackElement">공격 속성</param>
        /// <param name="defenseElement">방어 속성</param>
        /// <returns>최종 데미지</returns>
        public static int CalculateDamage(int baseDamage, ElementType attackElement, ElementType defenseElement)
        {
            float multiplier = GetDamageMultiplier(attackElement, defenseElement);
            int finalDamage = Mathf.RoundToInt(baseDamage * multiplier);

            // 최소 데미지 1 보장
            return Mathf.Max(1, finalDamage);
        }

        /// <summary>
        /// 상성 관계 문자열 반환 (디버그/UI용)
        /// </summary>
        public static string GetMatchupDescription(ElementType attackElement, ElementType defenseElement)
        {
            float multiplier = GetDamageMultiplier(attackElement, defenseElement);

            if (multiplier > NORMAL_MULTIPLIER)
            {
                return $"{attackElement} > {defenseElement} (x{multiplier})";
            }
            else if (multiplier < NORMAL_MULTIPLIER)
            {
                return $"{attackElement} < {defenseElement} (x{multiplier})";
            }
            else
            {
                return $"{attackElement} = {defenseElement} (x{multiplier})";
            }
        }

        /// <summary>
        /// 특정 속성에 강한 속성 목록 반환
        /// </summary>
        public static ElementType[] GetStrongAgainst(ElementType element)
        {
            return element switch
            {
                ElementType.Fire => new[] { ElementType.Ice, ElementType.Poison },
                ElementType.Ice => new[] { ElementType.Thunder, ElementType.Earth },
                ElementType.Thunder => new[] { ElementType.Ice },
                ElementType.Dark => new[] { ElementType.Light },
                ElementType.Light => new[] { ElementType.Dark },
                ElementType.Poison => new[] { ElementType.Earth },
                ElementType.Earth => new[] { ElementType.Thunder, ElementType.Fire },
                _ => new ElementType[0]
            };
        }

        /// <summary>
        /// 특정 속성에 약한 속성 목록 반환
        /// </summary>
        public static ElementType[] GetWeakAgainst(ElementType element)
        {
            return element switch
            {
                ElementType.Fire => new ElementType[0], // 자기자신만 약함
                ElementType.Ice => new[] { ElementType.Fire },
                ElementType.Thunder => new[] { ElementType.Earth },
                ElementType.Dark => new ElementType[0],
                ElementType.Light => new ElementType[0],
                ElementType.Poison => new[] { ElementType.Fire },
                ElementType.Earth => new[] { ElementType.Ice },
                _ => new ElementType[0]
            };
        }
    }
}
