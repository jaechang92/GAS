using UnityEngine;
using System.Threading;
using Combat.Core;

namespace Enemy
{
    /// <summary>
    /// Enemy Attack 상태
    /// 플레이어에게 공격 수행 및 데미지 적용
    /// </summary>
    public class EnemyAttackState : EnemyBaseState
    {
        private bool attackExecuted = false;
        private float attackTime = 0f;
        private const float AttackDuration = 0.5f; // 공격 애니메이션 시간
        private const float HitboxSpawnDelay = 0.15f; // 공격 시작 후 히트박스 생성 딜레이

        public EnemyAttackState() : base(EnemyStateType.Attack) { }

        protected override async Awaitable EnterState(CancellationToken cancellationToken)
        {
            LogStateDebug("Attack 상태 진입");
            attackExecuted = false;
            attackTime = 0f;

            // 이동 정지
            StopMovement();

            // 타겟 방향으로 회전
            FaceTarget();
            enemy.UpdateSpriteDirection();

            // 공격 쿨다운 갱신
            enemy.LastAttackTime = Time.time;
            enemy.IsAttacking = true;

            // 히트박스 생성 (딜레이 후)
            SpawnHitbox(cancellationToken);

            await Awaitable.NextFrameAsync(cancellationToken);
        }

        protected override async Awaitable ExitState(CancellationToken cancellationToken)
        {
            LogStateDebug("Attack 상태 종료");
            enemy.IsAttacking = false;
            await Awaitable.NextFrameAsync(cancellationToken);
        }

        protected override void UpdateState(float deltaTime)
        {
            if (enemy == null) return;

            attackTime += deltaTime;

            // 공격 애니메이션 종료 후 다음 상태로 전환
            if (attackTime >= AttackDuration)
            {
                TransitionToNextState();
            }
        }

        /// <summary>
        /// 히트박스 생성 및 데미지 적용
        /// </summary>
        private async void SpawnHitbox(CancellationToken cancellationToken)
        {
            try
            {
                // 히트박스 생성 딜레이
                await Awaitable.WaitForSecondsAsync(HitboxSpawnDelay, cancellationToken);

                if (cancellationToken.IsCancellationRequested || enemy == null || enemy.Data == null) return;

                // 적 위치 및 방향
                Vector3 enemyPosition = enemy.transform.position;
                int facingDirection = enemy.FacingDirection;

                // 히트박스 중심 위치 계산
                Vector2 hitboxOffset = new Vector2(
                    enemy.Data.hitboxOffset.x * facingDirection,
                    enemy.Data.hitboxOffset.y
                );
                Vector3 hitboxCenter = enemyPosition + (Vector3)hitboxOffset;

                // 히트박스 크기
                Vector2 hitboxSize = enemy.Data.hitboxSize;

                // 데미지 데이터 생성
                var damageData = DamageData.CreateWithKnockback(
                    enemy.Data.attackDamage,
                    Core.Enums.DamageType.Physical,
                    enemy.gameObject,
                    enemy.Data.knockbackForce * facingDirection * Vector2.right
                );

                // 박스 범위 데미지 적용
                var hitTargets = DamageSystem.ApplyBoxDamage(
                    hitboxCenter,
                    hitboxSize,
                    0f, // 회전 없음
                    damageData,
                    LayerMask.GetMask("Default", "Player") // Player 레이어 타겟
                );

                LogStateDebug($"히트박스 생성: {hitTargets.Count}개 타격, 데미지: {enemy.Data.attackDamage}");

                // 히트박스 시각화 (디버그용)
                DrawHitboxDebug(hitboxCenter, hitboxSize, enemy.Data.hitboxDuration);

                attackExecuted = true;
            }
            catch (System.OperationCanceledException)
            {
                LogStateDebug("히트박스 생성 취소됨");
            }
        }

        /// <summary>
        /// 다음 상태로 전환
        /// </summary>
        private void TransitionToNextState()
        {
            if (enemy == null || enemy.Target == null) return;

            float distanceToTarget = enemy.DistanceToTarget;

            // 타겟이 여전히 공격 범위 내에 있고 쿨다운이 끝났으면 재공격
            if (distanceToTarget <= enemy.Data.attackRange && enemy.CanAttack())
            {
                LogStateDebug("재공격 시도");
                enemy.ChangeState(EnemyStateType.Attack);
                return;
            }

            // 추적 범위 내에 있으면 Chase
            if (distanceToTarget <= enemy.Data.chaseRange)
            {
                enemy.ChangeState(EnemyStateType.Chase);
            }
            else
            {
                // 범위 밖이면 Idle
                enemy.ChangeState(EnemyStateType.Idle);
            }
        }

        /// <summary>
        /// 히트박스 디버그 시각화
        /// </summary>
        private async void DrawHitboxDebug(Vector3 center, Vector2 size, float duration)
        {
            var go = new GameObject("Enemy_Hitbox_Debug");
            go.transform.position = center;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.color = new Color(1f, 0.5f, 0f, 0.4f); // 반투명 주황색

            // Static 리소스 재사용
            if (debugTexture == null)
            {
                debugTexture = new Texture2D(1, 1);
                debugTexture.SetPixel(0, 0, Color.white);
                debugTexture.Apply();
            }

            if (debugSprite == null)
            {
                debugSprite = Sprite.Create(debugTexture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
            }

            sr.sprite = debugSprite;
            go.transform.localScale = new Vector3(size.x, size.y, 1f);

            await Awaitable.WaitForSecondsAsync(duration);
            if (go != null) Object.Destroy(go);
        }

        // Static 리소스 (메모리 누수 방지)
        private static Texture2D debugTexture;
        private static Sprite debugSprite;
    }
}
