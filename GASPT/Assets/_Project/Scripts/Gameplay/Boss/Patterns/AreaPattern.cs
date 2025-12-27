using UnityEngine;
using GASPT.Core.Enums;

namespace GASPT.Gameplay.Boss
{
    /// <summary>
    /// 범위 패턴
    /// 장판, 충격파, 폭발 등
    /// </summary>
    [System.Serializable]
    public class AreaPattern : BossPattern
    {
        // ====== 범위 패턴 설정 ======

        [Header("범위 패턴 설정")]
        [Tooltip("범위 반경")]
        [Range(2f, 15f)]
        public float radius = 5f;

        [Tooltip("지속 시간 (0 = 즉시 데미지)")]
        [Range(0f, 5f)]
        public float duration = 0f;

        [Tooltip("틱 간격 (지속 데미지용)")]
        [Range(0.1f, 1f)]
        public float tickInterval = 0.5f;

        [Tooltip("범위 유형 (true = 플레이어 위치, false = 보스 위치)")]
        public bool targetPlayerPosition = true;

        [Tooltip("딜레이 후 폭발 (true = 지연 폭발)")]
        public bool delayedExplosion = false;

        [Tooltip("폭발 딜레이")]
        [Range(0f, 3f)]
        public float explosionDelay = 1f;


        // ====== 생성자 ======

        public AreaPattern()
        {
            patternName = "범위 공격";
            patternType = PatternType.Area;
            damage = 30;
            cooldown = 8f;
            telegraphDuration = 1.5f;
            weight = 10;
            minPhase = 2;
            minRange = 0f;
            maxRange = 15f;
        }


        // ====== 텔레그래프 ======

        public override void ShowTelegraph(BaseBoss boss, Vector3 targetPosition)
        {
            Vector3 areaCenter = targetPlayerPosition ? targetPosition : boss.transform.position;

            TelegraphController.Instance.ShowCircle(
                areaCenter,
                radius,
                telegraphDuration + explosionDelay,
                new Color(1f, 0f, 0f, 0.4f)
            );
        }


        // ====== 실행 ======

        public override async Awaitable Execute(BaseBoss boss, Transform target)
        {
            if (boss == null || target == null) return;

            BeginExecution();

            try
            {
                // 범위 중심 결정
                Vector3 areaCenter = targetPlayerPosition ? target.position : boss.transform.position;

                // 1. 텔레그래프 표시
                ShowTelegraph(boss, areaCenter);

                // 2. 텔레그래프 시간 대기
                await Awaitable.WaitForSecondsAsync(telegraphDuration);
                if (IsCancelled()) return;

                // 3. 딜레이 폭발
                if (delayedExplosion)
                {
                    await Awaitable.WaitForSecondsAsync(explosionDelay);
                    if (IsCancelled()) return;
                }

                // 4. 데미지 적용
                if (duration <= 0)
                {
                    // 즉시 데미지
                    ApplyDamageInRadius(areaCenter, radius, damage);
                    Debug.Log($"[AreaPattern] {patternName} 폭발! (위치: {areaCenter}, 반경: {radius})");
                }
                else
                {
                    // 지속 데미지
                    float elapsed = 0f;
                    while (elapsed < duration)
                    {
                        if (IsCancelled()) return;

                        ApplyDamageInRadius(areaCenter, radius, Mathf.RoundToInt(damage * tickInterval));

                        await Awaitable.WaitForSecondsAsync(tickInterval);
                        elapsed += tickInterval;
                    }

                    Debug.Log($"[AreaPattern] {patternName} 지속 데미지 완료");
                }
            }
            finally
            {
                EndExecution();
            }
        }
    }
}
