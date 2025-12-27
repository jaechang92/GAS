using UnityEngine;
using GASPT.Core.Enums;

namespace GASPT.Gameplay.Boss
{
    /// <summary>
    /// 불꽃 골렘 보스 (B001)
    /// 스테이지 1 미니보스
    ///
    /// 페이즈 1: 화염 내려찍기, 화염구 발사
    /// 페이즈 2: + 폭발 추가
    /// </summary>
    public class FlameGolemBoss : BaseBoss
    {
        // ====== 패턴 인스턴스 ======

        private MeleePattern flameSmashPattern;
        private RangedPattern fireballPattern;
        private AreaPattern explosionPattern;


        // ====== 설정 ======

        [Header("불꽃 골렘 설정")]
        [SerializeField]
        private float smashDamage = 20f;

        [SerializeField]
        private float fireballDamage = 15f;

        [SerializeField]
        private float explosionDamage = 25f;

        [SerializeField]
        private float explosionRadius = 4f;


        // ====== 초기화 ======

        protected override void InitializePatternSelector()
        {
            patternSelector = new PatternSelector();

            // 화염 내려찍기 (근접)
            flameSmashPattern = new MeleePattern
            {
                patternName = "화염 내려찍기",
                patternType = PatternType.Melee,
                damage = Mathf.RoundToInt(smashDamage),
                cooldown = 4f,
                telegraphDuration = 0.8f,
                weight = 15,
                minPhase = 1,
                chargeSpeed = 12f,
                chargeDistance = 6f,
                attackRadius = 2.5f
            };
            patternSelector.AddPattern(flameSmashPattern);

            // 화염구 발사 (원거리)
            fireballPattern = new RangedPattern
            {
                patternName = "화염구 발사",
                patternType = PatternType.Ranged,
                damage = Mathf.RoundToInt(fireballDamage),
                cooldown = 3f,
                telegraphDuration = 0.5f,
                weight = 20,
                minPhase = 1,
                projectileSpeed = 10f,
                projectileCount = 1,
                trackTarget = true
            };
            patternSelector.AddPattern(fireballPattern);

            // 폭발 (범위) - Phase 2부터
            explosionPattern = new AreaPattern
            {
                patternName = "폭발",
                patternType = PatternType.Area,
                damage = Mathf.RoundToInt(explosionDamage),
                cooldown = 8f,
                telegraphDuration = 1.5f,
                weight = 10,
                minPhase = 2,
                radius = explosionRadius,
                duration = 0f,
                targetPlayerPosition = false,
                delayedExplosion = false
            };
            patternSelector.AddPattern(explosionPattern);

            Debug.Log("[FlameGolemBoss] 패턴 초기화 완료 (3개)");
        }


        // ====== 페이즈 전환 오버라이드 ======

        protected override void HandlePhaseChanged(int newPhaseIndex)
        {
            base.HandlePhaseChanged(newPhaseIndex);

            if (newPhaseIndex >= 1) // Phase 2
            {
                Debug.Log("[FlameGolemBoss] Phase 2 진입! 폭발 패턴 활성화!");

                // Phase 2 진입 시 폭발 연출
                PlayEnrageEffect();
            }
        }


        // ====== 연출 ======

        private async void PlayEnrageEffect()
        {
            // 화면 흔들림 (카메라 시스템이 있다면 연동)
            Debug.Log("[FlameGolemBoss] 광폭화 연출!");

            // 잠시 멈춤
            await Awaitable.WaitForSecondsAsync(0.5f);

            // 전체 화면 텔레그래프
            TelegraphController.Instance.ShowFullScreen(1f, new Color(1f, 0.3f, 0f, 0.3f));
        }


        // ====== 사망 오버라이드 ======

        protected override async void PlayDeathSequence()
        {
            Debug.Log("[FlameGolemBoss] 불꽃 골렘 처치!");

            // 폭발 연출
            TelegraphController.Instance.ShowCircle(
                transform.position,
                5f,
                1f,
                new Color(1f, 0.5f, 0f, 0.5f)
            );

            await Awaitable.WaitForSecondsAsync(1.5f);

            Destroy(gameObject);
        }


        // ====== 기즈모 ======

        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();

            // 폭발 범위 표시
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, explosionRadius);
        }
    }
}
