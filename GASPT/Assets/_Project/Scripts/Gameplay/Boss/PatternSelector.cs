using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GASPT.Gameplay.Boss
{
    /// <summary>
    /// 패턴 선택기
    /// 가중치 기반 패턴 선택 및 관리
    /// </summary>
    public class PatternSelector
    {
        // ====== 패턴 목록 ======

        private readonly List<BossPattern> patterns = new List<BossPattern>();


        // ====== 설정 ======

        private float patternCooldownMultiplier = 1f;
        private int consecutivePatternLimit = 2;


        // ====== 상태 ======

        private BossPattern lastUsedPattern;
        private int consecutiveUseCount = 0;


        // ====== 생성자 ======

        public PatternSelector()
        {
        }

        public PatternSelector(IEnumerable<BossPattern> patternList)
        {
            if (patternList != null)
            {
                patterns.AddRange(patternList);
            }
        }


        // ====== 패턴 등록 ======

        /// <summary>
        /// 패턴 추가
        /// </summary>
        public void AddPattern(BossPattern pattern)
        {
            if (pattern != null && !patterns.Contains(pattern))
            {
                patterns.Add(pattern);
            }
        }

        /// <summary>
        /// 패턴 제거
        /// </summary>
        public void RemovePattern(BossPattern pattern)
        {
            patterns.Remove(pattern);
        }

        /// <summary>
        /// 모든 패턴 제거
        /// </summary>
        public void ClearPatterns()
        {
            patterns.Clear();
        }


        // ====== 패턴 선택 ======

        /// <summary>
        /// 가중치 기반 패턴 선택
        /// </summary>
        /// <param name="currentPhaseIndex">현재 페이즈 인덱스 (0부터 시작)</param>
        /// <param name="distanceToTarget">대상까지의 거리 (선택적)</param>
        public BossPattern SelectPattern(int currentPhaseIndex, float distanceToTarget = float.MaxValue)
        {
            int currentPhase = currentPhaseIndex + 1; // 1부터 시작하는 페이즈 번호

            // 사용 가능한 패턴 필터링
            var availablePatterns = GetAvailablePatterns(currentPhase, distanceToTarget);

            if (availablePatterns.Count == 0)
            {
                return null;
            }

            // 연속 사용 제한 적용
            availablePatterns = ApplyConsecutiveLimit(availablePatterns);

            if (availablePatterns.Count == 0)
            {
                return null;
            }

            // 가중치 기반 랜덤 선택
            BossPattern selected = SelectByWeight(availablePatterns);

            // 연속 사용 카운트 업데이트
            UpdateConsecutiveCount(selected);

            return selected;
        }

        /// <summary>
        /// 특정 타입의 패턴 선택
        /// </summary>
        public BossPattern SelectPatternOfType(Core.Enums.PatternType type, int currentPhaseIndex, float distanceToTarget = float.MaxValue)
        {
            int currentPhase = currentPhaseIndex + 1;

            var availablePatterns = GetAvailablePatterns(currentPhase, distanceToTarget)
                .Where(p => p.patternType == type)
                .ToList();

            if (availablePatterns.Count == 0)
            {
                return null;
            }

            return SelectByWeight(availablePatterns);
        }


        // ====== 필터링 ======

        /// <summary>
        /// 사용 가능한 패턴 목록 반환
        /// </summary>
        private List<BossPattern> GetAvailablePatterns(int currentPhase, float distanceToTarget)
        {
            return patterns
                .Where(p => p.CanUse(currentPhase, distanceToTarget))
                .ToList();
        }

        /// <summary>
        /// 연속 사용 제한 적용
        /// </summary>
        private List<BossPattern> ApplyConsecutiveLimit(List<BossPattern> availablePatterns)
        {
            if (lastUsedPattern == null || consecutiveUseCount < consecutivePatternLimit)
            {
                return availablePatterns;
            }

            // 마지막 패턴을 제외
            var filtered = availablePatterns.Where(p => p != lastUsedPattern).ToList();

            // 필터링 후 비어있으면 원본 반환
            return filtered.Count > 0 ? filtered : availablePatterns;
        }


        // ====== 가중치 선택 ======

        /// <summary>
        /// 가중치 기반 랜덤 선택
        /// </summary>
        private BossPattern SelectByWeight(List<BossPattern> availablePatterns)
        {
            if (availablePatterns.Count == 0)
            {
                return null;
            }

            if (availablePatterns.Count == 1)
            {
                return availablePatterns[0];
            }

            // 총 가중치 계산
            int totalWeight = availablePatterns.Sum(p => p.weight);

            if (totalWeight <= 0)
            {
                return availablePatterns[Random.Range(0, availablePatterns.Count)];
            }

            // 랜덤 값 선택
            int roll = Random.Range(0, totalWeight);
            int cumulative = 0;

            foreach (var pattern in availablePatterns)
            {
                cumulative += pattern.weight;
                if (roll < cumulative)
                {
                    return pattern;
                }
            }

            // 폴백
            return availablePatterns[^1];
        }


        // ====== 연속 사용 추적 ======

        private void UpdateConsecutiveCount(BossPattern selected)
        {
            if (selected == lastUsedPattern)
            {
                consecutiveUseCount++;
            }
            else
            {
                consecutiveUseCount = 1;
                lastUsedPattern = selected;
            }
        }


        // ====== 설정 ======

        /// <summary>
        /// 쿨다운 배율 설정
        /// </summary>
        public void SetCooldownMultiplier(float multiplier)
        {
            patternCooldownMultiplier = Mathf.Max(0.1f, multiplier);
        }

        /// <summary>
        /// 연속 사용 제한 설정
        /// </summary>
        public void SetConsecutiveLimit(int limit)
        {
            consecutivePatternLimit = Mathf.Max(1, limit);
        }


        // ====== 쿨다운 관리 ======

        /// <summary>
        /// 모든 패턴 쿨다운 리셋
        /// </summary>
        public void ResetAllCooldowns()
        {
            foreach (var pattern in patterns)
            {
                pattern.ForceCooldownComplete();
            }
        }

        /// <summary>
        /// 특정 타입 패턴 쿨다운 리셋
        /// </summary>
        public void ResetCooldownsByType(Core.Enums.PatternType type)
        {
            foreach (var pattern in patterns.Where(p => p.patternType == type))
            {
                pattern.ForceCooldownComplete();
            }
        }


        // ====== 유틸리티 ======

        /// <summary>
        /// 등록된 패턴 수
        /// </summary>
        public int PatternCount => patterns.Count;

        /// <summary>
        /// 사용 가능한 패턴 수
        /// </summary>
        public int GetAvailablePatternCount(int currentPhase, float distanceToTarget = float.MaxValue)
        {
            return GetAvailablePatterns(currentPhase, distanceToTarget).Count;
        }

        /// <summary>
        /// 패턴 목록 반환 (읽기 전용)
        /// </summary>
        public IReadOnlyList<BossPattern> GetPatterns()
        {
            return patterns.AsReadOnly();
        }


        // ====== 디버그 ======

        /// <summary>
        /// 디버그 정보 출력
        /// </summary>
        public string GetDebugInfo(int currentPhase, float distanceToTarget = float.MaxValue)
        {
            var available = GetAvailablePatterns(currentPhase, distanceToTarget);
            int totalWeight = available.Sum(p => p.weight);

            string info = $"[PatternSelector] Phase {currentPhase}, Distance {distanceToTarget:F1}\n";
            info += $"Available: {available.Count}/{patterns.Count}, TotalWeight: {totalWeight}\n";

            foreach (var p in available)
            {
                float chance = totalWeight > 0 ? (float)p.weight / totalWeight * 100f : 0f;
                info += $"  - {p.patternName}: Weight={p.weight} ({chance:F1}%), CD={p.RemainingCooldown:F1}s\n";
            }

            return info;
        }
    }
}
