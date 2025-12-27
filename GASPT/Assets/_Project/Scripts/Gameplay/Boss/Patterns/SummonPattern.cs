using UnityEngine;
using GASPT.Core.Enums;
using GASPT.Core.Pooling;
using GASPT.Data;
using GASPT.Gameplay.Enemies;

namespace GASPT.Gameplay.Boss
{
    /// <summary>
    /// 소환 패턴
    /// 졸개, 분신 등 소환
    /// </summary>
    [System.Serializable]
    public class SummonPattern : BossPattern
    {
        // ====== 소환 패턴 설정 ======

        [Header("소환 패턴 설정")]
        [Tooltip("소환할 적 프리팹")]
        public GameObject minionPrefab;

        [Tooltip("소환할 적 데이터")]
        public EnemyData minionData;

        [Tooltip("소환 수")]
        [Range(1, 5)]
        public int summonCount = 2;

        [Tooltip("소환 범위 (보스 주변)")]
        [Range(2f, 10f)]
        public float summonRadius = 4f;

        [Tooltip("소환 간격")]
        [Range(0f, 1f)]
        public float summonInterval = 0.3f;

        [Tooltip("최대 동시 소환 수")]
        [Range(1, 10)]
        public int maxSimultaneousMinions = 5;


        // ====== 상태 ======

        private int currentMinionCount = 0;


        // ====== 생성자 ======

        public SummonPattern()
        {
            patternName = "소환";
            patternType = PatternType.Summon;
            damage = 0; // 소환은 직접 데미지 없음
            cooldown = 12f;
            telegraphDuration = 1f;
            weight = 5;
            minPhase = 2;
            minRange = 0f;
            maxRange = 100f; // 거리 무관
        }


        // ====== 사용 가능 체크 오버라이드 ======

        public override bool CanUse(int currentPhase, float distanceToTarget)
        {
            // 기본 체크
            if (!base.CanUse(currentPhase, distanceToTarget))
                return false;

            // 최대 소환 수 체크
            if (currentMinionCount >= maxSimultaneousMinions)
                return false;

            return true;
        }


        // ====== 텔레그래프 ======

        public override void ShowTelegraph(BaseBoss boss, Vector3 targetPosition)
        {
            // 소환 위치 미리 표시
            for (int i = 0; i < summonCount; i++)
            {
                Vector3 summonPos = GetSummonPosition(boss.transform.position, i);

                TelegraphController.Instance.ShowCircle(
                    summonPos,
                    1f,
                    telegraphDuration,
                    new Color(0.5f, 0f, 1f, 0.5f) // 보라색
                );
            }
        }


        // ====== 실행 ======

        public override async Awaitable Execute(BaseBoss boss, Transform target)
        {
            if (boss == null) return;
            if (minionPrefab == null)
            {
                Debug.LogWarning("[SummonPattern] minionPrefab이 null입니다.");
                EndExecution();
                return;
            }

            BeginExecution();

            try
            {
                // 1. 텔레그래프 표시
                ShowTelegraph(boss, target?.position ?? boss.transform.position);

                // 2. 텔레그래프 시간 대기
                await Awaitable.WaitForSecondsAsync(telegraphDuration);
                if (IsCancelled()) return;

                // 3. 소환 실행
                int successCount = 0;

                for (int i = 0; i < summonCount; i++)
                {
                    if (IsCancelled()) return;

                    // 최대 소환 수 체크
                    if (currentMinionCount >= maxSimultaneousMinions)
                    {
                        Debug.Log("[SummonPattern] 최대 소환 수 도달");
                        break;
                    }

                    Vector3 summonPos = GetSummonPosition(boss.transform.position, i);

                    // 소환
                    if (SpawnMinion(summonPos))
                    {
                        successCount++;
                    }

                    // 소환 간격
                    if (i < summonCount - 1 && summonInterval > 0)
                    {
                        await Awaitable.WaitForSecondsAsync(summonInterval);
                    }
                }

                Debug.Log($"[SummonPattern] {patternName} 완료 (소환: {successCount}/{summonCount})");
            }
            finally
            {
                EndExecution();
            }
        }


        // ====== 소환 ======

        private bool SpawnMinion(Vector3 position)
        {
            GameObject minion = Object.Instantiate(minionPrefab, position, Quaternion.identity);

            if (minion == null)
            {
                return false;
            }

            // Enemy 컴포넌트 설정
            Enemy enemy = minion.GetComponent<Enemy>();
            if (enemy != null && minionData != null)
            {
                enemy.InitializeWithData(minionData);

                // 사망 시 카운트 감소
                currentMinionCount++;
                enemy.OnDeath += OnMinionDeath;
            }
            else
            {
                Debug.LogWarning("[SummonPattern] Enemy 컴포넌트 또는 minionData가 null입니다.");
            }

            return true;
        }

        private void OnMinionDeath(Enemy enemy)
        {
            currentMinionCount--;
            enemy.OnDeath -= OnMinionDeath;
        }


        // ====== 유틸리티 ======

        private Vector3 GetSummonPosition(Vector3 bossPosition, int index)
        {
            // 원형 배치
            float angle = (360f / summonCount) * index;
            float rad = angle * Mathf.Deg2Rad;

            return bossPosition + new Vector3(
                Mathf.Cos(rad) * summonRadius,
                0f,
                Mathf.Sin(rad) * summonRadius
            );
        }


        // ====== 정리 ======

        /// <summary>
        /// 현재 소환 수 리셋 (보스 사망/리셋 시 호출)
        /// </summary>
        public void ResetMinionCount()
        {
            currentMinionCount = 0;
        }
    }
}
