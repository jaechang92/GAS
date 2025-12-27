using UnityEngine;
using GASPT.Core.Enums;

namespace GASPT.Gameplay.Boss
{
    /// <summary>
    /// 어둠의 군주 보스 (B004)
    /// 스테이지 5 최종보스 (4 페이즈)
    ///
    /// 페이즈 1: 암흑 베기, 암흑구, 생명력 흡수
    /// 페이즈 2: + 분신 소환
    /// 페이즈 3: + 암흑 폭풍
    /// 페이즈 4: + 멸망의 일격
    /// </summary>
    public class DarkLordBoss : BaseBoss
    {
        // ====== 패턴 인스턴스 ======

        private MeleePattern darkSlashPattern;
        private RangedPattern darkOrbPattern;
        private AreaPattern lifeDrainPattern;
        private SummonPattern shadowClonePattern;
        private AreaPattern darkStormPattern;
        private AreaPattern annihilationPattern;


        // ====== 설정 ======

        [Header("어둠의 군주 설정")]
        [SerializeField]
        private float darkSlashDamage = 35f;

        [SerializeField]
        private float darkOrbDamage = 30f;

        [SerializeField]
        private float lifeDrainDamage = 40f;

        [SerializeField]
        private float darkStormDamage = 60f;

        [SerializeField]
        private float annihilationDamage = 100f;

        [Header("소환 설정")]
        [SerializeField]
        private GameObject shadowClonePrefab;

        [SerializeField]
        private Data.EnemyData shadowCloneData;


        // ====== 초기화 ======

        protected override void InitializePatternSelector()
        {
            patternSelector = new PatternSelector();

            // 암흑 베기 (근접)
            darkSlashPattern = new MeleePattern
            {
                patternName = "암흑 베기",
                patternType = PatternType.Melee,
                damage = Mathf.RoundToInt(darkSlashDamage),
                cooldown = 3f,
                telegraphDuration = 0.6f,
                weight = 20,
                minPhase = 1,
                chargeSpeed = 15f,
                chargeDistance = 5f,
                attackRadius = 3f
            };
            patternSelector.AddPattern(darkSlashPattern);

            // 암흑구 (원거리)
            darkOrbPattern = new RangedPattern
            {
                patternName = "암흑구",
                patternType = PatternType.Ranged,
                damage = Mathf.RoundToInt(darkOrbDamage),
                cooldown = 4f,
                telegraphDuration = 0.7f,
                weight = 18,
                minPhase = 1,
                projectileSpeed = 8f,
                projectileCount = 3,
                fireInterval = 0.2f,
                spreadAngle = 15f,
                trackTarget = true
            };
            patternSelector.AddPattern(darkOrbPattern);

            // 생명력 흡수 (범위)
            lifeDrainPattern = new AreaPattern
            {
                patternName = "생명력 흡수",
                patternType = PatternType.Area,
                damage = Mathf.RoundToInt(lifeDrainDamage),
                cooldown = 10f,
                telegraphDuration = 1.5f,
                weight = 12,
                minPhase = 1,
                radius = 5f,
                duration = 0f,
                targetPlayerPosition = false,
                delayedExplosion = true,
                explosionDelay = 1f
            };
            patternSelector.AddPattern(lifeDrainPattern);

            // 분신 소환 (소환) - Phase 2부터
            shadowClonePattern = new SummonPattern
            {
                patternName = "분신 소환",
                patternType = PatternType.Summon,
                cooldown = 20f,
                telegraphDuration = 1.5f,
                weight = 8,
                minPhase = 2,
                summonCount = 2,
                summonRadius = 5f,
                maxSimultaneousMinions = 3,
                minionPrefab = shadowClonePrefab,
                minionData = shadowCloneData
            };
            patternSelector.AddPattern(shadowClonePattern);

            // 암흑 폭풍 (특수) - Phase 3부터
            darkStormPattern = new AreaPattern
            {
                patternName = "암흑 폭풍",
                patternType = PatternType.Special,
                damage = Mathf.RoundToInt(darkStormDamage),
                cooldown = 25f,
                telegraphDuration = 2.5f,
                weight = 10,
                minPhase = 3,
                radius = 10f,
                duration = 3f,
                tickInterval = 0.5f,
                targetPlayerPosition = false,
                delayedExplosion = false
            };
            patternSelector.AddPattern(darkStormPattern);

            // 멸망의 일격 (특수) - Phase 4 전용
            annihilationPattern = new AreaPattern
            {
                patternName = "멸망의 일격",
                patternType = PatternType.Special,
                damage = Mathf.RoundToInt(annihilationDamage),
                cooldown = 30f,
                telegraphDuration = 3f,
                weight = 15,
                minPhase = 4,
                radius = 15f,
                duration = 0f,
                targetPlayerPosition = false,
                delayedExplosion = true,
                explosionDelay = 2f
            };
            patternSelector.AddPattern(annihilationPattern);

            Debug.Log("[DarkLordBoss] 패턴 초기화 완료 (6개)");
        }


        // ====== 페이즈 전환 오버라이드 ======

        protected override void HandlePhaseChanged(int newPhaseIndex)
        {
            base.HandlePhaseChanged(newPhaseIndex);

            switch (newPhaseIndex)
            {
                case 1: // Phase 2
                    Debug.Log("[DarkLordBoss] Phase 2 진입! 분신 소환 활성화!");
                    PlayPhase2Effect();
                    break;

                case 2: // Phase 3
                    Debug.Log("[DarkLordBoss] Phase 3 진입! 암흑 폭풍 활성화!");
                    PlayPhase3Effect();
                    break;

                case 3: // Phase 4
                    Debug.Log("[DarkLordBoss] Phase 4 진입! 광폭화! 멸망의 일격 활성화!");
                    PlayPhase4Effect();
                    break;
            }
        }


        // ====== 연출 ======

        private async void PlayPhase2Effect()
        {
            Debug.Log("[DarkLordBoss] 암흑 분열 연출!");

            TelegraphController.Instance.ShowFullScreen(0.8f, new Color(0.2f, 0f, 0.3f, 0.5f));

            await Awaitable.WaitForSecondsAsync(0.5f);
        }

        private async void PlayPhase3Effect()
        {
            Debug.Log("[DarkLordBoss] 암흑 폭발 연출!");

            for (int i = 0; i < 2; i++)
            {
                TelegraphController.Instance.ShowFullScreen(0.3f, new Color(0.3f, 0f, 0.5f, 0.6f));
                await Awaitable.WaitForSecondsAsync(0.4f);
            }
        }

        private async void PlayPhase4Effect()
        {
            Debug.Log("[DarkLordBoss] 최종 광폭화 연출!");

            // 화면 전체 흔들림 + 암흑 효과
            for (int i = 0; i < 5; i++)
            {
                TelegraphController.Instance.ShowFullScreen(0.15f, new Color(0.5f, 0f, 0.5f, 0.7f));
                await Awaitable.WaitForSecondsAsync(0.2f);
            }
        }


        // ====== 사망 오버라이드 ======

        protected override async void PlayDeathSequence()
        {
            Debug.Log("[DarkLordBoss] 어둠의 군주 처치! 최종 보스 클리어!");

            // 대규모 소멸 연출
            TelegraphController.Instance.ShowCircle(
                transform.position,
                15f,
                2f,
                new Color(0.5f, 0f, 0.5f, 0.6f)
            );

            // 분신 정리
            if (shadowClonePattern != null)
            {
                shadowClonePattern.ResetMinionCount();
            }

            await Awaitable.WaitForSecondsAsync(3f);

            Destroy(gameObject);
        }


        // ====== 기즈모 ======

        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();

            // 암흑 폭풍 범위
            Gizmos.color = new Color(0.3f, 0f, 0.5f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, 10f);

            // 멸망의 일격 범위
            Gizmos.color = new Color(0.5f, 0f, 0.5f, 0.2f);
            Gizmos.DrawWireSphere(transform.position, 15f);
        }
    }
}
