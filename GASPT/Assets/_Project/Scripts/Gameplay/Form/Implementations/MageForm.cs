using UnityEngine;

namespace GASPT.Gameplay.Form
{
    /// <summary>
    /// 마법사 폼 구현
    /// 특징: 원거리 마법 공격, 빠른 이동 속도, 마법 스킬
    /// </summary>
    public class MageForm : BaseForm
    {
        public override string FormName => "Mage";
        public override FormType FormType => FormType.Mage;

        [Header("Mage Specific")]
        [SerializeField] private ParticleSystem magicAuraEffect;
        [SerializeField] private Color magicColor = new Color(0.5f, 0.5f, 1f); // 파란색

        private void Awake()
        {
            InitializeDefaultAbilities();
        }

        /// <summary>
        /// 기본 스킬 초기화
        /// </summary>
        private void InitializeDefaultAbilities()
        {
            // 점프 (기본 동작) - PlayerController의 지면 체크 사용
            SetJumpAbility(new JumpAbility(jumpForce: JumpPower));

            // 기본 공격: 마법 미사일
            SetAbility(0, new MagicMissileAbility());

            // 스킬 1: 순간이동
            SetAbility(1, new TeleportAbility());

            // 스킬 2: 화염구
            SetAbility(2, new FireballAbility());

            if (showDebugLogs)
                Debug.Log("[MageForm] 기본 스킬 초기화 완료 (Jump 포함)");
        }

        protected override void OnFormActivated()
        {
            base.OnFormActivated();

            // 마법사 폼 활성화 시 추가 효과
            ApplyMageStats();
            PlayMagicAuraEffect();

            if (showDebugLogs)
                Debug.Log("[MageForm] 마법사 폼 활성화: 빠른 이동, 마법 공격 특화");
        }

        protected override void OnFormDeactivated()
        {
            base.OnFormDeactivated();

            // 마법 이펙트 중지
            StopMagicAuraEffect();
        }

        /// <summary>
        /// 마법사 스탯 적용
        /// </summary>
        private void ApplyMageStats()
        {
            // CharacterPhysics와 연동 (나중에 구현)
            // physics.moveSpeed = MoveSpeed;
            // physics.jumpForce = JumpPower;

            if (showDebugLogs)
                Debug.Log($"[MageForm] 스탯 적용 - Speed: {MoveSpeed}, Jump: {JumpPower}");
        }

        /// <summary>
        /// 마법 오라 이펙트 재생
        /// </summary>
        private void PlayMagicAuraEffect()
        {
            if (magicAuraEffect != null)
            {
                magicAuraEffect.Play();
            }
        }

        /// <summary>
        /// 마법 오라 이펙트 중지
        /// </summary>
        private void StopMagicAuraEffect()
        {
            if (magicAuraEffect != null)
            {
                magicAuraEffect.Stop();
            }
        }

        /// <summary>
        /// 마법사 특수 능력 (예: 마나 재생)
        /// </summary>
        private void Update()
        {
            // 마법사 특수 능력 구현 (나중에)
            // 예: 마나 자동 재생, 마법 강화 등
        }

        /// <summary>
        /// 디버그용 - 마법사 스킬 테스트
        /// </summary>
        [ContextMenu("Test Magic Missile")]
        private void TestMagicMissile()
        {
            var ability = GetAbility(0);
            if (ability != null)
            {
                Debug.Log($"[MageForm] 테스트 - {ability.AbilityName} 실행");
                // ability.ExecuteAsync(gameObject, default);
            }
        }

        [ContextMenu("Test Teleport")]
        private void TestTeleport()
        {
            var ability = GetAbility(1);
            if (ability != null)
            {
                Debug.Log($"[MageForm] 테스트 - {ability.AbilityName} 실행");
            }
        }

        [ContextMenu("Test Fireball")]
        private void TestFireball()
        {
            var ability = GetAbility(2);
            if (ability != null)
            {
                Debug.Log($"[MageForm] 테스트 - {ability.AbilityName} 실행");
            }
        }
    }
}
