using UnityEngine;

namespace GASPT.Gameplay.Form
{
    /// <summary>
    /// 암살자 폼 구현
    /// 특징: 빠른 이동, 높은 공격력, 낮은 체력, 기습/회피 스킬
    /// </summary>
    public class AssassinForm : BaseForm
    {
        public override string FormName => "Assassin";
        public override FormType FormType => FormType.Assassin;

        [Header("Assassin Specific")]
        [SerializeField] private ParticleSystem shadowAuraEffect;
        [SerializeField] private Color assassinColor = new Color(0.3f, 0.1f, 0.3f); // 어두운 보라색

        private void Awake()
        {
            InitializeDefaultAbilities();
        }

        /// <summary>
        /// 기본 스킬 초기화
        /// </summary>
        private void InitializeDefaultAbilities()
        {
            // 점프 (기본 동작) - 높은 점프력
            SetJumpAbility(new JumpAbility(jumpForce: JumpPower));

            // 기본 공격: 단검 연타
            SetAbility(0, new DaggerStrikeAbility());

            // 스킬 1: 그림자 일격
            SetAbility(1, new ShadowStrikeAbility());

            // 스킬 2: 연막
            SetAbility(2, new SmokeScreenAbility());

            if (showDebugLogs)
                Debug.Log("[AssassinForm] 기본 스킬 초기화 완료 (DaggerStrike, ShadowStrike, SmokeScreen)");
        }

        protected override void OnFormActivated()
        {
            base.OnFormActivated();

            // 암살자 폼 활성화 시 추가 효과
            ApplyAssassinStats();
            PlayShadowAuraEffect();

            if (showDebugLogs)
                Debug.Log("[AssassinForm] 암살자 폼 활성화: 빠른 이동, 높은 공격력, 낮은 체력");
        }

        protected override void OnFormDeactivated()
        {
            base.OnFormDeactivated();

            // 그림자 이펙트 중지
            StopShadowAuraEffect();
        }

        /// <summary>
        /// 암살자 스탯 적용
        /// </summary>
        private void ApplyAssassinStats()
        {
            if (showDebugLogs)
                Debug.Log($"[AssassinForm] 스탯 적용 - HP: {MaxHealth}, Speed: {MoveSpeed}, Jump: {JumpPower}");
        }

        /// <summary>
        /// 그림자 오라 이펙트 재생
        /// </summary>
        private void PlayShadowAuraEffect()
        {
            if (shadowAuraEffect != null)
            {
                shadowAuraEffect.Play();
            }
        }

        /// <summary>
        /// 그림자 오라 이펙트 중지
        /// </summary>
        private void StopShadowAuraEffect()
        {
            if (shadowAuraEffect != null)
            {
                shadowAuraEffect.Stop();
            }
        }

        /// <summary>
        /// 암살자 특수 능력 (예: 연속 타격 보너스)
        /// </summary>
        private void Update()
        {
            // 암살자 특수 능력 구현 (나중에)
            // 예: 연속 타격 시 대미지 증가, 후방 공격 크리티컬 등
        }

        // ===== 디버그 테스트 메서드 =====

        [ContextMenu("Test DaggerStrike")]
        private void TestDaggerStrike()
        {
            var ability = GetAbility(0);
            if (ability != null)
            {
                Debug.Log($"[AssassinForm] 테스트 - {ability.AbilityName} 실행");
            }
        }

        [ContextMenu("Test ShadowStrike")]
        private void TestShadowStrike()
        {
            var ability = GetAbility(1);
            if (ability != null)
            {
                Debug.Log($"[AssassinForm] 테스트 - {ability.AbilityName} 실행");
            }
        }

        [ContextMenu("Test SmokeScreen")]
        private void TestSmokeScreen()
        {
            var ability = GetAbility(2);
            if (ability != null)
            {
                Debug.Log($"[AssassinForm] 테스트 - {ability.AbilityName} 실행");
            }
        }
    }
}
