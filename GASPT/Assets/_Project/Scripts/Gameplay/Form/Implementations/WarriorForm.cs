using UnityEngine;

namespace GASPT.Gameplay.Form
{
    /// <summary>
    /// 전사 폼 구현
    /// 특징: 근접 전투 특화, 높은 체력, 강한 기본 공격, 스턴/버프 스킬
    /// </summary>
    public class WarriorForm : BaseForm
    {
        public override string FormName => "Warrior";
        public override FormType FormType => FormType.Warrior;

        [Header("Warrior Specific")]
        [SerializeField] private ParticleSystem battleAuraEffect;
        [SerializeField] private Color warriorColor = new Color(0.8f, 0.3f, 0.3f); // 붉은색

        private void Awake()
        {
            InitializeDefaultAbilities();
        }

        /// <summary>
        /// 기본 스킬 초기화
        /// </summary>
        private void InitializeDefaultAbilities()
        {
            // 점프 (기본 동작)
            SetJumpAbility(new JumpAbility(jumpForce: JumpPower));

            // 기본 공격: 검 베기
            SetAbility(0, new SwordSlashAbility());

            // 스킬 1: 돌진
            SetAbility(1, new ChargeAbility());

            // 스킬 2: 방패 강타
            SetAbility(2, new ShieldBashAbility());

            if (showDebugLogs)
                Debug.Log("[WarriorForm] 기본 스킬 초기화 완료 (SwordSlash, Charge, ShieldBash)");
        }

        protected override void OnFormActivated()
        {
            base.OnFormActivated();

            // 전사 폼 활성화 시 추가 효과
            ApplyWarriorStats();
            PlayBattleAuraEffect();

            if (showDebugLogs)
                Debug.Log("[WarriorForm] 전사 폼 활성화: 높은 체력, 근접 전투 특화");
        }

        protected override void OnFormDeactivated()
        {
            base.OnFormDeactivated();

            // 전투 이펙트 중지
            StopBattleAuraEffect();
        }

        /// <summary>
        /// 전사 스탯 적용
        /// </summary>
        private void ApplyWarriorStats()
        {
            if (showDebugLogs)
                Debug.Log($"[WarriorForm] 스탯 적용 - HP: {MaxHealth}, Speed: {MoveSpeed}, Jump: {JumpPower}");
        }

        /// <summary>
        /// 전투 오라 이펙트 재생
        /// </summary>
        private void PlayBattleAuraEffect()
        {
            if (battleAuraEffect != null)
            {
                battleAuraEffect.Play();
            }
        }

        /// <summary>
        /// 전투 오라 이펙트 중지
        /// </summary>
        private void StopBattleAuraEffect()
        {
            if (battleAuraEffect != null)
            {
                battleAuraEffect.Stop();
            }
        }

        /// <summary>
        /// 전사 특수 능력 (예: 피격 시 분노 게이지)
        /// </summary>
        private void Update()
        {
            // 전사 특수 능력 구현 (나중에)
            // 예: 피격 시 분노 게이지 증가, 분노 시 공격력 상승 등
        }

        // ===== 디버그 테스트 메서드 =====

        [ContextMenu("Test SwordSlash")]
        private void TestSwordSlash()
        {
            var ability = GetAbility(0);
            if (ability != null)
            {
                Debug.Log($"[WarriorForm] 테스트 - {ability.AbilityName} 실행");
            }
        }

        [ContextMenu("Test Charge")]
        private void TestCharge()
        {
            var ability = GetAbility(1);
            if (ability != null)
            {
                Debug.Log($"[WarriorForm] 테스트 - {ability.AbilityName} 실행");
            }
        }

        [ContextMenu("Test ShieldBash")]
        private void TestShieldBash()
        {
            var ability = GetAbility(2);
            if (ability != null)
            {
                Debug.Log($"[WarriorForm] 테스트 - {ability.AbilityName} 실행");
            }
        }
    }
}
