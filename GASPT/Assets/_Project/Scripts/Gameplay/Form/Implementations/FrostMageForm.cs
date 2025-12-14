using UnityEngine;

namespace GASPT.Gameplay.Form
{
    /// <summary>
    /// 얼음 마법사 폼
    /// 특징: CC 특화, 슬로우/빙결, 안전한 원거리 딜링
    /// </summary>
    public class FrostMageForm : BaseForm
    {
        public override string FormName => "Frost Mage";
        public override FormType FormType => FormType.Frost;

        [Header("Frost Mage Specific")]
        [SerializeField] private ParticleSystem frostAuraEffect;
        [SerializeField] private Color frostColor = new Color(0.6f, 0.9f, 1f);  // 밝은 얼음색

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

            // 기본 공격: 아이스 블라스트 (기존 IceBlastAbility 재사용)
            SetAbility(0, new IceBlastAbility());

            // 스킬 1: 아이스 랜스 (빠른 투사체)
            SetAbility(1, new IceLanceAbility());

            // 스킬 2: 빙결 지대 (궁극기)
            SetAbility(2, new FrozenGroundAbility());

            if (showDebugLogs)
                Debug.Log("[FrostMageForm] 기본 스킬 초기화 완료");
        }

        protected override void OnFormActivated()
        {
            base.OnFormActivated();

            ApplyFrostStats();
            PlayFrostAuraEffect();

            if (showDebugLogs)
                Debug.Log("[FrostMageForm] 얼음 마법사 폼 활성화: CC 및 슬로우 특화");
        }

        protected override void OnFormDeactivated()
        {
            base.OnFormDeactivated();

            StopFrostAuraEffect();
        }

        /// <summary>
        /// 얼음 마법사 스탯 적용
        /// </summary>
        private void ApplyFrostStats()
        {
            // 얼음 마법사: 안정적인 딜링, 높은 CC 능력
            if (showDebugLogs)
                Debug.Log($"[FrostMageForm] 스탯 적용 - Speed: {MoveSpeed}, Jump: {JumpPower}");
        }

        /// <summary>
        /// 서리 오라 이펙트 재생
        /// </summary>
        private void PlayFrostAuraEffect()
        {
            if (frostAuraEffect != null)
            {
                frostAuraEffect.Play();
            }
        }

        /// <summary>
        /// 서리 오라 이펙트 중지
        /// </summary>
        private void StopFrostAuraEffect()
        {
            if (frostAuraEffect != null)
            {
                frostAuraEffect.Stop();
            }
        }


        // ====== 디버그 ======

        [ContextMenu("Test Ice Blast")]
        private void TestIceBlast()
        {
            var ability = GetAbility(0);
            if (ability != null)
            {
                Debug.Log($"[FrostMageForm] 테스트 - {ability.AbilityName}");
            }
        }

        [ContextMenu("Test Ice Lance")]
        private void TestIceLance()
        {
            var ability = GetAbility(1);
            if (ability != null)
            {
                Debug.Log($"[FrostMageForm] 테스트 - {ability.AbilityName}");
            }
        }

        [ContextMenu("Test Frozen Ground")]
        private void TestFrozenGround()
        {
            var ability = GetAbility(2);
            if (ability != null)
            {
                Debug.Log($"[FrostMageForm] 테스트 - {ability.AbilityName}");
            }
        }
    }
}
