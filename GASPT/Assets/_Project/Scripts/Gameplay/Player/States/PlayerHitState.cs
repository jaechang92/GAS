using System.Threading;
using UnityEngine;
using Combat.Core;

namespace Player
{
    /// <summary>
    /// 플레이어 피격 상태
    /// 넉백, 스턴, 피격 시각 효과 처리
    /// </summary>
    public class PlayerHitState : PlayerBaseState
    {
        private float hitDuration = 0.5f;
        private float hitTime = 0f;

        // 피격 데이터
        private DamageData currentDamageData;

        // 깜빡임 효과
        private const float BlinkInterval = 0.1f;
        private float blinkTimer = 0f;
        private bool isVisible = true;
        private SpriteRenderer spriteRenderer;
        private Color originalColor;

        // 플래시 효과
        private const float FlashDuration = 0.15f;
        private float flashTimer = 0f;
        private bool isFlashing = true;

        // 중력 복원
        private float gravityRestoreTimer = 0f;
        private bool needsGravityRestore = false;
        private float originalGravity = 1f;

        public PlayerHitState(PlayerController controller) : base(PlayerStateType.Hit)
        {
            playerController = controller;
        }

        protected override void EnterStateSync()
        {
            LogStateDebug("피격 상태 진입(동기)");

            // 타이머 초기화
            hitTime = 0f;
            blinkTimer = 0f;
            flashTimer = 0f;
            isVisible = true;
            isFlashing = true;
            needsGravityRestore = false;
            gravityRestoreTimer = 0f;

            // PlayerController로부터 DamageData 가져오기
            currentDamageData = playerController.GetPendingDamageData();

            // 스턴 시간 설정 (최소 0.2초, 최대 2초)
            hitDuration = Mathf.Clamp(currentDamageData.stunDuration > 0 ? currentDamageData.stunDuration : 0.3f, 0.2f, 2f);

            LogStateDebug($"피격 데이터 설정: 데미지={currentDamageData.amount}, 스턴={hitDuration}초");

            // SpriteRenderer 가져오기
            spriteRenderer = playerController.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                originalColor = spriteRenderer.color;
            }

            // CharacterPhysics 물리 입력 강제 정지
            var characterPhysics = playerController.GetComponent<Player.Physics.CharacterPhysics>();
            if (characterPhysics != null)
            {
                characterPhysics.SetHorizontalInput(0f);
            }

            // 넉백 즉시 적용
            ApplyKnockback();
        }

        protected override void ExitStateSync()
        {
            LogStateDebug("피격 상태 종료(동기)");

            // 스프라이트 원래대로 복원
            if (spriteRenderer != null)
            {
                spriteRenderer.color = originalColor;
            }

            // 중력 복원 (만약 아직 복원되지 않았다면)
            var rb = playerController.GetComponent<Rigidbody2D>();
            if (needsGravityRestore && rb != null)
            {
                rb.gravityScale = originalGravity;
                LogStateDebug("중력 복원 (ExitState)");
            }
        }

        protected override void UpdateState(float deltaTime)
        {
            hitTime += deltaTime;

            // 플래시 효과 업데이트 (처음 0.15초)
            if (isFlashing)
            {
                UpdateFlashEffect(deltaTime);
            }
            else
            {
                // 플래시 종료 후 깜빡임 효과
                UpdateBlinkEffect(deltaTime);
            }

            // 중력 복원 타이머 (넉백 후 0.1초)
            if (needsGravityRestore)
            {
                gravityRestoreTimer += deltaTime;
                if (gravityRestoreTimer >= 0.1f)
                {
                    var rb = playerController.GetComponent<Rigidbody2D>();
                    if (rb != null)
                    {
                        rb.gravityScale = originalGravity;
                        needsGravityRestore = false;
                        LogStateDebug("중력 복원");
                    }
                }
            }

            // 피격 중에는 CharacterPhysics 입력 강제 차단
            var characterPhysics = playerController.GetComponent<Player.Physics.CharacterPhysics>();
            if (characterPhysics != null)
            {
                characterPhysics.SetHorizontalInput(0f);
            }

            // 스턴 시간이 끝나면 Idle로 전환
            if (hitTime >= hitDuration)
            {
                playerController.ChangeState(PlayerStateType.Idle);
            }
        }

        /// <summary>
        /// 피격 데이터 설정
        /// </summary>
        public void SetDamageData(DamageData damageData)
        {
            currentDamageData = damageData;

            // 스턴 시간 설정 (최소 0.2초, 최대 2초)
            hitDuration = Mathf.Clamp(damageData.stunDuration > 0 ? damageData.stunDuration : 0.3f, 0.2f, 2f);

            LogStateDebug($"피격 데이터 설정: 데미지={damageData.amount}, 스턴={hitDuration}초");
        }

        /// <summary>
        /// 넉백 효과 적용
        /// </summary>
        private void ApplyKnockback()
        {
            var rb = playerController.GetComponent<Rigidbody2D>();
            if (rb == null) return;

            if (currentDamageData.knockback == Vector2.zero)
            {
                // 기본 넉백 (공격자 반대 방향)
                if (currentDamageData.source != null)
                {
                    Vector2 direction = (playerController.transform.position - currentDamageData.source.transform.position).normalized;
                    currentDamageData.knockback = new Vector2(direction.x * 12f, 6f); // 더 강한 넉백
                }
            }

            if (currentDamageData.knockback != Vector2.zero)
            {
                // 중력 저장 및 비활성화
                originalGravity = rb.gravityScale;
                rb.gravityScale = 0f; // 넉백 중 중력 무시
                rb.linearVelocity = currentDamageData.knockback;

                // 0.1초 후 중력 복원 플래그 설정
                needsGravityRestore = true;
                gravityRestoreTimer = 0f;

                LogStateDebug($"넉백 적용! velocity={rb.linearVelocity}, knockback={currentDamageData.knockback}");
            }
        }

        /// <summary>
        /// 빨간색 플래시 효과 업데이트 (동기)
        /// </summary>
        private void UpdateFlashEffect(float deltaTime)
        {
            if (spriteRenderer == null)
            {
                isFlashing = false;
                return;
            }

            flashTimer += deltaTime;

            if (flashTimer >= FlashDuration)
            {
                // 플래시 종료
                spriteRenderer.color = originalColor;
                isFlashing = false;
            }
            else
            {
                // 빨간색에서 원래 색으로 보간
                Color flashColor = new Color(1f, 0.3f, 0.3f, 1f); // 빨간색
                float t = flashTimer / FlashDuration;
                spriteRenderer.color = Color.Lerp(flashColor, originalColor, t);
            }
        }

        /// <summary>
        /// 깜빡임 효과 업데이트
        /// </summary>
        private void UpdateBlinkEffect(float deltaTime)
        {
            if (spriteRenderer == null) return;

            blinkTimer += deltaTime;

            if (blinkTimer >= BlinkInterval)
            {
                blinkTimer = 0f;
                isVisible = !isVisible;

                // 알파값만 변경 (색상은 유지)
                Color color = spriteRenderer.color;
                color.a = isVisible ? 1f : 0.3f;
                spriteRenderer.color = color;
            }
        }
    }
}
