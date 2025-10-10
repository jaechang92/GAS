using UnityEngine;
using System.Threading;
using Combat.Core;

namespace Enemy
{
    /// <summary>
    /// Enemy Hit 상태
    /// 피격 시 경직, 넉백, 시각 효과 처리
    /// </summary>
    public class EnemyHitState : EnemyBaseState
    {
        private float hitStunTime = 0f;
        private float hitStunDuration = 0.3f;

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

        public EnemyHitState() : base(EnemyStateType.Hit) { }

        protected override void EnterStateSync()
        {
            LogStateDebug("Hit 상태 진입(동기)");

            // 타이머 초기화
            hitStunTime = 0f;
            blinkTimer = 0f;
            flashTimer = 0f;
            isVisible = true;
            isFlashing = true;
            needsGravityRestore = false;
            gravityRestoreTimer = 0f;

            // EnemyController로부터 DamageData 가져오기
            currentDamageData = enemy.GetPendingDamageData();

            // 스턴 시간 설정 (DamageData 우선, 없으면 EnemyData 사용)
            if (currentDamageData.stunDuration > 0)
            {
                hitStunDuration = currentDamageData.stunDuration;
            }
            else if (enemy?.Data != null)
            {
                hitStunDuration = enemy.Data.hitStunDuration;
            }

            hitStunDuration = Mathf.Clamp(hitStunDuration, 0.1f, 2f);

            LogStateDebug($"피격 데이터 설정: 데미지={currentDamageData.amount}, 스턴={hitStunDuration}초");

            // SpriteRenderer 가져오기
            spriteRenderer = enemy.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                originalColor = spriteRenderer.color;
            }

            // 이동 정지
            StopMovement();

            // 넉백 즉시 적용
            ApplyKnockback();
        }

        protected override void ExitStateSync()
        {
            LogStateDebug("Hit 상태 종료(동기)");

            // 스프라이트 원래대로 복원
            if (spriteRenderer != null)
            {
                spriteRenderer.color = originalColor;
            }

            // 중력 복원 (만약 아직 복원되지 않았다면)
            if (needsGravityRestore && enemy?.Rigidbody != null)
            {
                enemy.Rigidbody.gravityScale = originalGravity;
                LogStateDebug("중력 복원 (ExitState)");
            }
        }

        protected override void UpdateState(float deltaTime)
        {
            if (enemy == null || enemy.Data == null) return;

            hitStunTime += deltaTime;

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
                if (gravityRestoreTimer >= 0.1f && enemy?.Rigidbody != null)
                {
                    enemy.Rigidbody.gravityScale = originalGravity;
                    needsGravityRestore = false;
                    LogStateDebug("중력 복원");
                }
            }

            // 경직 시간 종료 후 다음 상태로 전환
            if (hitStunTime >= hitStunDuration)
            {
                TransitionToNextState();
            }
        }

        /// <summary>
        /// 피격 데이터 설정
        /// </summary>
        public void SetDamageData(DamageData damageData)
        {
            currentDamageData = damageData;

            // 스턴 시간 설정 (DamageData 우선, 없으면 EnemyData 사용)
            if (damageData.stunDuration > 0)
            {
                hitStunDuration = damageData.stunDuration;
            }
            else if (enemy?.Data != null)
            {
                hitStunDuration = enemy.Data.hitStunDuration;
            }

            hitStunDuration = Mathf.Clamp(hitStunDuration, 0.1f, 2f);

            LogStateDebug($"피격 데이터 설정: 데미지={damageData.amount}, 스턴={hitStunDuration}초");
        }

        /// <summary>
        /// 넉백 효과 적용
        /// </summary>
        private void ApplyKnockback()
        {
            if (enemy.Rigidbody == null) return;

            if (currentDamageData.knockback == Vector2.zero)
            {
                // 기본 넉백 (공격자 반대 방향)
                if (currentDamageData.source != null)
                {
                    Vector2 direction = (enemy.transform.position - currentDamageData.source.transform.position).normalized;
                    currentDamageData.knockback = new Vector2(direction.x * 12f, 5f); // 더 강한 넉백
                }
            }

            if (currentDamageData.knockback != Vector2.zero)
            {
                // 중력 저장 및 비활성화
                originalGravity = enemy.Rigidbody.gravityScale;
                enemy.Rigidbody.gravityScale = 0f; // 넉백 중 중력 무시
                enemy.Rigidbody.linearVelocity = currentDamageData.knockback;

                // 0.1초 후 중력 복원 플래그 설정
                needsGravityRestore = true;
                gravityRestoreTimer = 0f;

                LogStateDebug($"넉백 적용! velocity={enemy.Rigidbody.linearVelocity}, knockback={currentDamageData.knockback}");
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

        /// <summary>
        /// 다음 상태로 전환
        /// </summary>
        private void TransitionToNextState()
        {
            if (enemy == null || enemy.Target == null)
            {
                enemy.ChangeState(EnemyStateType.Idle);
                return;
            }

            float distanceToTarget = enemy.DistanceToTarget;

            // 타겟이 추적 범위 내에 있으면 Trace
            if (distanceToTarget <= enemy.Data.chaseRange)
            {
                enemy.ChangeState(EnemyStateType.Trace);
            }
            else
            {
                // 범위 밖이면 Idle
                enemy.ChangeState(EnemyStateType.Idle);
            }
        }
    }
}
