using UnityEngine;

namespace GASPT.Gameplay.Form
{
    /// <summary>
    /// 화염 마법사 폼
    /// 특징: 강력한 범위 공격, 화상 데미지, 높은 DPS
    /// </summary>
    public class FlameMageForm : BaseForm
    {
        public override string FormName => "Flame Mage";
        public override FormType FormType => FormType.Flame;

        [Header("Flame Mage Specific")]
        [SerializeField] private ParticleSystem flameAuraEffect;
        [SerializeField] private Color flameColor = new Color(1f, 0.5f, 0f);  // 주황색

        private void Awake()
        {
            InitializeDefaultAbilities();
        }

        /// <summary>
        /// 기본 스킬 초기화
        /// </summary>
        private void InitializeDefaultAbilities()
        {
            // 점프
            SetJumpAbility(new JumpAbility(jumpForce: JumpPower));

            // 기본 공격: 화염구 (기존 FireballAbility 재사용)
            SetAbility(0, new FireballAbility());

            // 스킬 1: 파이어스톰 (범위 지속 데미지)
            SetAbility(1, new FireStormAbility());

            // 스킬 2: 메테오 스트라이크 (궁극기)
            SetAbility(2, new MeteorStrikeAbility());

            if (showDebugLogs)
                Debug.Log("[FlameMageForm] 기본 스킬 초기화 완료");
        }

        protected override void OnFormActivated()
        {
            base.OnFormActivated();

            ApplyFlameStats();
            PlayFlameAuraEffect();

            if (showDebugLogs)
                Debug.Log("[FlameMageForm] 화염 마법사 폼 활성화: 범위 화염 공격 특화");
        }

        protected override void OnFormDeactivated()
        {
            base.OnFormDeactivated();

            StopFlameAuraEffect();
        }

        /// <summary>
        /// 화염 마법사 스탯 적용
        /// </summary>
        private void ApplyFlameStats()
        {
            // 화염 마법사: 공격력 높음, 기동성 보통
            if (showDebugLogs)
                Debug.Log($"[FlameMageForm] 스탯 적용 - Speed: {MoveSpeed}, Jump: {JumpPower}");
        }

        /// <summary>
        /// 화염 오라 이펙트 재생
        /// </summary>
        private void PlayFlameAuraEffect()
        {
            if (flameAuraEffect != null)
            {
                flameAuraEffect.Play();
            }
        }

        /// <summary>
        /// 화염 오라 이펙트 중지
        /// </summary>
        private void StopFlameAuraEffect()
        {
            if (flameAuraEffect != null)
            {
                flameAuraEffect.Stop();
            }
        }


        // ====== 디버그 ======

        [ContextMenu("Test Fireball")]
        private void TestFireball()
        {
            var ability = GetAbility(0);
            if (ability != null)
            {
                Debug.Log($"[FlameMageForm] 테스트 - {ability.AbilityName}");
            }
        }

        [ContextMenu("Test Fire Storm")]
        private void TestFireStorm()
        {
            var ability = GetAbility(1);
            if (ability != null)
            {
                Debug.Log($"[FlameMageForm] 테스트 - {ability.AbilityName}");
            }
        }

        [ContextMenu("Test Meteor Strike")]
        private void TestMeteorStrike()
        {
            var ability = GetAbility(2);
            if (ability != null)
            {
                Debug.Log($"[FlameMageForm] 테스트 - {ability.AbilityName}");
            }
        }
    }
}
