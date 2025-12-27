using UnityEngine;
using GASPT.Core.Enums;

namespace GASPT.Gameplay.Boss
{
    /// <summary>
    /// 얼음 정령 보스 (B002)
    /// 스테이지 2 미니보스
    ///
    /// 페이즈 1: 얼음 창, 빙결 지대
    /// 페이즈 2: + 얼음 소환수
    /// </summary>
    public class FrostSpiritBoss : BaseBoss
    {
        // ====== 패턴 인스턴스 ======

        private RangedPattern iceLancePattern;
        private AreaPattern frozenGroundPattern;
        private SummonPattern iceMinionPattern;


        // ====== 설정 ======

        [Header("얼음 정령 설정")]
        [SerializeField]
        private float iceLanceDamage = 15f;

        [SerializeField]
        private float frozenGroundDamage = 10f;

        [SerializeField]
        private float frozenGroundRadius = 5f;

        [SerializeField]
        private float frozenGroundDuration = 3f;

        [Header("소환 설정")]
        [SerializeField]
        private GameObject iceMinionPrefab;

        [SerializeField]
        private Data.EnemyData iceMinionData;


        // ====== 초기화 ======

        protected override void InitializePatternSelector()
        {
            patternSelector = new PatternSelector();

            // 얼음 창 (원거리)
            iceLancePattern = new RangedPattern
            {
                patternName = "얼음 창",
                patternType = PatternType.Ranged,
                damage = Mathf.RoundToInt(iceLanceDamage),
                cooldown = 3f,
                telegraphDuration = 0.6f,
                weight = 25,
                minPhase = 1,
                projectileSpeed = 12f,
                projectileCount = 3,
                fireInterval = 0.15f,
                spreadAngle = 10f,
                trackTarget = true
            };
            patternSelector.AddPattern(iceLancePattern);

            // 빙결 지대 (범위)
            frozenGroundPattern = new AreaPattern
            {
                patternName = "빙결 지대",
                patternType = PatternType.Area,
                damage = Mathf.RoundToInt(frozenGroundDamage),
                cooldown = 6f,
                telegraphDuration = 1f,
                weight = 15,
                minPhase = 1,
                radius = frozenGroundRadius,
                duration = frozenGroundDuration,
                tickInterval = 0.5f,
                targetPlayerPosition = true,
                delayedExplosion = false
            };
            patternSelector.AddPattern(frozenGroundPattern);

            // 얼음 소환수 (소환) - Phase 2부터
            iceMinionPattern = new SummonPattern
            {
                patternName = "얼음 소환수",
                patternType = PatternType.Summon,
                cooldown = 12f,
                telegraphDuration = 1f,
                weight = 8,
                minPhase = 2,
                summonCount = 2,
                summonRadius = 4f,
                maxSimultaneousMinions = 4,
                minionPrefab = iceMinionPrefab,
                minionData = iceMinionData
            };
            patternSelector.AddPattern(iceMinionPattern);

            Debug.Log("[FrostSpiritBoss] 패턴 초기화 완료 (3개)");
        }


        // ====== 페이즈 전환 오버라이드 ======

        protected override void HandlePhaseChanged(int newPhaseIndex)
        {
            base.HandlePhaseChanged(newPhaseIndex);

            if (newPhaseIndex >= 1) // Phase 2
            {
                Debug.Log("[FrostSpiritBoss] Phase 2 진입! 소환 패턴 활성화!");

                // Phase 2 진입 시 빙결 연출
                PlayFreezeEffect();
            }
        }


        // ====== 연출 ======

        private async void PlayFreezeEffect()
        {
            Debug.Log("[FrostSpiritBoss] 빙결 연출!");

            // 전체 화면 빙결 효과
            TelegraphController.Instance.ShowFullScreen(1f, new Color(0f, 0.5f, 1f, 0.3f));

            await Awaitable.WaitForSecondsAsync(0.5f);
        }


        // ====== 사망 오버라이드 ======

        protected override async void PlayDeathSequence()
        {
            Debug.Log("[FrostSpiritBoss] 얼음 정령 처치!");

            // 산산조각 연출
            TelegraphController.Instance.ShowCircle(
                transform.position,
                4f,
                1f,
                new Color(0f, 0.8f, 1f, 0.5f)
            );

            // 소환된 미니언 정리
            if (iceMinionPattern != null)
            {
                iceMinionPattern.ResetMinionCount();
            }

            await Awaitable.WaitForSecondsAsync(1.5f);

            Destroy(gameObject);
        }


        // ====== 기즈모 ======

        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();

            // 빙결 지대 범위 표시
            Gizmos.color = new Color(0f, 0.5f, 1f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, frozenGroundRadius);
        }
    }
}
