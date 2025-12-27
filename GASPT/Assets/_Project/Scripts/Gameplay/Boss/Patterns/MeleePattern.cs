using UnityEngine;
using GASPT.Core.Enums;

namespace GASPT.Gameplay.Boss
{
    /// <summary>
    /// 근접 패턴
    /// 돌진, 내려찍기 등 근접 공격
    /// </summary>
    [System.Serializable]
    public class MeleePattern : BossPattern
    {
        // ====== 근접 패턴 설정 ======

        [Header("근접 패턴 설정")]
        [Tooltip("돌진 속도")]
        [Range(5f, 30f)]
        public float chargeSpeed = 15f;

        [Tooltip("돌진 거리")]
        [Range(3f, 20f)]
        public float chargeDistance = 8f;

        [Tooltip("공격 범위")]
        [Range(1f, 5f)]
        public float attackRadius = 2f;

        [Tooltip("돌진 후 대기 시간")]
        [Range(0f, 1f)]
        public float postChargeDelay = 0.3f;


        // ====== 생성자 ======

        public MeleePattern()
        {
            patternName = "근접 공격";
            patternType = PatternType.Melee;
            damage = 25;
            cooldown = 4f;
            telegraphDuration = 0.8f;
            weight = 15;
            minPhase = 1;
            minRange = 0f;
            maxRange = 10f;
        }


        // ====== 텔레그래프 ======

        public override void ShowTelegraph(BaseBoss boss, Vector3 targetPosition)
        {
            Vector3 direction = (targetPosition - boss.transform.position).normalized;
            Vector3 endPos = boss.transform.position + direction * chargeDistance;

            TelegraphController.Instance.ShowLine(
                boss.transform.position,
                endPos,
                attackRadius,
                telegraphDuration,
                new Color(1f, 0.5f, 0f, 0.4f) // 주황색
            );
        }


        // ====== 실행 ======

        public override async Awaitable Execute(BaseBoss boss, Transform target)
        {
            if (boss == null || target == null) return;

            BeginExecution();

            try
            {
                // 1. 텔레그래프 표시
                ShowTelegraph(boss, target.position);

                // 2. 텔레그래프 시간 대기
                await Awaitable.WaitForSecondsAsync(telegraphDuration);
                if (IsCancelled()) return;

                // 3. 돌진 방향 계산
                Vector2 direction = new Vector2(
                    target.position.x - boss.transform.position.x,
                    0f // Y축은 0으로 고정 (수평 돌진)
                ).normalized;

                Vector3 startPos = boss.transform.position;
                float traveled = 0f;

                // 4. 돌진 실행
                while (traveled < chargeDistance)
                {
                    if (IsCancelled()) return;

                    float step = chargeSpeed * Time.deltaTime;
                    boss.transform.position += new Vector3(direction.x * step, 0f, 0f);
                    traveled += step;

                    // 플레이어 충돌 체크
                    float distToPlayer = Vector2.Distance(boss.transform.position, target.position);
                    if (distToPlayer < attackRadius)
                    {
                        // 데미지 적용
                        ApplyDamageInRadius(boss.transform.position, attackRadius, damage);
                        break;
                    }

                    await Awaitable.NextFrameAsync();
                }

                // 5. 돌진 후 대기
                await Awaitable.WaitForSecondsAsync(postChargeDelay);

                Debug.Log($"[MeleePattern] {patternName} 완료");
            }
            finally
            {
                EndExecution();
            }
        }
    }
}
