using UnityEngine;
using GASPT.Core.Enums;

namespace GASPT.Gameplay.Boss
{
    /// <summary>
    /// 번개 드래곤 보스 (B003)
    /// 스테이지 3 중간보스 (3 페이즈)
    ///
    /// 페이즈 1: 낙뢰, 연쇄 번개
    /// 페이즈 2: + 돌진
    /// 페이즈 3: + 분노, 번개 폭풍
    /// </summary>
    public class ThunderDragonBoss : BaseBoss
    {
        // ====== 패턴 인스턴스 ======

        private RangedPattern lightningStrikePattern;
        private AreaPattern chainLightningPattern;
        private MeleePattern chargePattern;
        private BuffPattern ragePattern;
        private AreaPattern thunderStormPattern;


        // ====== 설정 ======

        [Header("번개 드래곤 설정")]
        [SerializeField]
        private float lightningDamage = 30f;

        [SerializeField]
        private float chainLightningDamage = 20f;

        [SerializeField]
        private float chargeDamage = 40f;

        [SerializeField]
        private float thunderStormDamage = 50f;

        [SerializeField]
        private float thunderStormRadius = 8f;


        // ====== 초기화 ======

        protected override void InitializePatternSelector()
        {
            patternSelector = new PatternSelector();

            // 낙뢰 (원거리)
            lightningStrikePattern = new RangedPattern
            {
                patternName = "낙뢰",
                patternType = PatternType.Ranged,
                damage = Mathf.RoundToInt(lightningDamage),
                cooldown = 4f,
                telegraphDuration = 0.8f,
                weight = 20,
                minPhase = 1,
                projectileSpeed = 20f,
                projectileCount = 1,
                trackTarget = true
            };
            patternSelector.AddPattern(lightningStrikePattern);

            // 연쇄 번개 (범위)
            chainLightningPattern = new AreaPattern
            {
                patternName = "연쇄 번개",
                patternType = PatternType.Area,
                damage = Mathf.RoundToInt(chainLightningDamage),
                cooldown = 8f,
                telegraphDuration = 1.2f,
                weight = 15,
                minPhase = 1,
                radius = 4f,
                duration = 0f,
                targetPlayerPosition = true,
                delayedExplosion = true,
                explosionDelay = 0.5f
            };
            patternSelector.AddPattern(chainLightningPattern);

            // 돌진 (근접) - Phase 2부터
            chargePattern = new MeleePattern
            {
                patternName = "번개 돌진",
                patternType = PatternType.Melee,
                damage = Mathf.RoundToInt(chargeDamage),
                cooldown = 10f,
                telegraphDuration = 1f,
                weight = 12,
                minPhase = 2,
                chargeSpeed = 20f,
                chargeDistance = 12f,
                attackRadius = 3f
            };
            patternSelector.AddPattern(chargePattern);

            // 분노 (버프) - Phase 3부터
            ragePattern = new BuffPattern
            {
                patternName = "분노",
                patternType = PatternType.Buff,
                cooldown = 20f,
                telegraphDuration = 1.5f,
                weight = 8,
                minPhase = 3,
                buffType = BuffPattern.BuffType.Rage,
                buffDuration = 10f,
                buffValue = 1.5f
            };
            patternSelector.AddPattern(ragePattern);

            // 번개 폭풍 (특수 범위) - Phase 3부터
            thunderStormPattern = new AreaPattern
            {
                patternName = "번개 폭풍",
                patternType = PatternType.Special,
                damage = Mathf.RoundToInt(thunderStormDamage),
                cooldown = 15f,
                telegraphDuration = 2f,
                weight = 10,
                minPhase = 3,
                radius = thunderStormRadius,
                duration = 2f,
                tickInterval = 0.5f,
                targetPlayerPosition = false,
                delayedExplosion = false
            };
            patternSelector.AddPattern(thunderStormPattern);

            Debug.Log("[ThunderDragonBoss] 패턴 초기화 완료 (5개)");
        }


        // ====== 페이즈 전환 오버라이드 ======

        protected override void HandlePhaseChanged(int newPhaseIndex)
        {
            base.HandlePhaseChanged(newPhaseIndex);

            switch (newPhaseIndex)
            {
                case 1: // Phase 2
                    Debug.Log("[ThunderDragonBoss] Phase 2 진입! 돌진 패턴 활성화!");
                    PlayPhase2Effect();
                    break;

                case 2: // Phase 3
                    Debug.Log("[ThunderDragonBoss] Phase 3 진입! 분노 & 번개 폭풍 활성화!");
                    PlayPhase3Effect();
                    break;
            }
        }


        // ====== 연출 ======

        private async void PlayPhase2Effect()
        {
            Debug.Log("[ThunderDragonBoss] 포효 연출!");

            // 번개 효과
            TelegraphController.Instance.ShowFullScreen(0.5f, new Color(1f, 1f, 0f, 0.4f));

            await Awaitable.WaitForSecondsAsync(0.3f);
        }

        private async void PlayPhase3Effect()
        {
            Debug.Log("[ThunderDragonBoss] 광폭화 연출!");

            // 강력한 번개 효과
            for (int i = 0; i < 3; i++)
            {
                TelegraphController.Instance.ShowFullScreen(0.2f, new Color(1f, 1f, 0f, 0.6f));
                await Awaitable.WaitForSecondsAsync(0.3f);
            }
        }


        // ====== 사망 오버라이드 ======

        protected override async void PlayDeathSequence()
        {
            Debug.Log("[ThunderDragonBoss] 번개 드래곤 처치!");

            // 대규모 번개 폭발 연출
            TelegraphController.Instance.ShowCircle(
                transform.position,
                thunderStormRadius,
                1.5f,
                new Color(1f, 1f, 0f, 0.5f)
            );

            await Awaitable.WaitForSecondsAsync(2f);

            Destroy(gameObject);
        }


        // ====== 기즈모 ======

        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();

            // 번개 폭풍 범위 표시
            Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, thunderStormRadius);
        }
    }
}
