using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace GASPT.Gameplay.Form
{
    /// <summary>
    /// 점프 어빌리티 - 모든 Form의 기본 동작
    /// Form별로 상속하여 다른 점프 구현 가능
    /// PlayerController의 지면 체크를 사용
    /// </summary>
    public class JumpAbility : IAbility
    {
        // ====== IAbility 프로퍼티 (필수) ======

        public string AbilityName => "Jump";

        public float Cooldown => 0f;  // 쿨다운 없음 (지면 체크로 제한)


        // ====== IAbility 프로퍼티 (선택적 확장) ======

        public int BaseDamage => 0;

        public float BaseRange => 0f;

        public int ManaCost => 0;

        public bool IsReady => !isOnCooldown;

        public float RemainingCooldown => remainingCooldown;

        public float CooldownProgress => Cooldown > 0 ? 1f - (remainingCooldown / Cooldown) : 1f;


        // ====== 내부 필드 ======

        private readonly float jumpForce;
        private bool isOnCooldown = false;
        private float remainingCooldown = 0f;


        // ====== 생성자 ======

        /// <summary>
        /// 생성자
        /// </summary>
        /// <param name="jumpForce">점프 힘</param>
        public JumpAbility(float jumpForce = 10f)
        {
            this.jumpForce = jumpForce;
        }


        // ====== IAbility 메서드 ======

        public async Task ExecuteAsync(GameObject caster, CancellationToken token)
        {
            // Rigidbody2D 확인
            Rigidbody2D rb = caster.GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                Debug.LogWarning("[JumpAbility] Rigidbody2D가 없어 점프할 수 없습니다!");
                return;
            }

            // PlayerController의 지면 체크 사용 (중복 체크 방지)
            var controller = caster.GetComponent<Player.PlayerController>();
            if (controller == null)
            {
                Debug.LogWarning("[JumpAbility] PlayerController가 없어 점프할 수 없습니다!");
                return;
            }

            if (!controller.IsGrounded)
            {
                // 공중에서는 점프할 수 없음
                return;
            }

            // 점프 실행
            PerformJump(rb);

            // 비동기 완료 (즉시 반환)
            await Task.CompletedTask;
        }


        // ====== 점프 로직 ======

        /// <summary>
        /// 점프 실행
        /// </summary>
        protected virtual void PerformJump(Rigidbody2D rb)
        {
            // Y축 속도를 0으로 리셋 후 점프 (이전 하강 속도 무시)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);

            // 점프 힘 적용
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);

            Debug.Log($"[JumpAbility] 점프! 힘: {jumpForce}");
        }
    }
}
