using System.Threading;
using UnityEngine;
using Combat.Attack;
using Combat.Core;
using Core.Enums;
using System.Collections;

namespace Player
{
    /// <summary>
    /// 플레이어 공격 상태 (Combat 시스템 통합)
    /// ComboSystem과 연동하여 콤보 공격 처리
    /// </summary>
    public class PlayerAttackState : PlayerBaseState
    {
        private bool attackTriggered = false;
        private bool comboWindowActive = false;
        private float attackAnimationTime = 0f;

        // 히트박스 설정
        private const float HitboxSpawnDelay = 0.1f; // 공격 시작 후 히트박스 생성 딜레이
        private LayerMask enemyLayer = 1 << LayerMask.NameToLayer("Enemy"); // Enemy 레이어

        // 히트박스 디버그용 Static 리소스 (메모리 누수 방지)
        private static Texture2D debugTexture;
        private static Sprite debugSprite;

        public PlayerAttackState(PlayerController controller) : base(PlayerStateType.Attack)
        {
            playerController = controller;
        }
        
        protected override async Awaitable EnterState(CancellationToken cancellationToken)
        {
            LogStateDebug("공격 상태 진입");
            attackTriggered = false;
            attackAnimationTime = 0f;

            // 공격 입력 즉시 리셋 (중복 전환 방지)
            playerController.PlayerInput?.ResetAttack();

            // ComboSystem에 타격 등록
            if (playerController.ComboSystem != null)
            {
                var comboSystem = playerController.ComboSystem;
                int currentComboIndex = comboSystem.CurrentComboIndex;

                // RegisterHit 호출 전에 현재 콤보 데이터를 먼저 가져옴
                // (RegisterHit에서 AdvanceCombo가 호출되어 인덱스가 변경되기 전)
                var comboData = comboSystem.CurrentComboData;

                bool registered = comboSystem.RegisterHit(currentComboIndex);

                if (registered)
                {
                    LogStateDebug($"콤보 등록 성공: index {currentComboIndex}");

                    if (comboData != null)
                    {
                        // AttackAnimationHandler를 통해 애니메이션 재생
                        if (playerController.AttackAnimationHandler != null)
                        {
                            playerController.AttackAnimationHandler.TriggerAttackAnimation(
                                comboData.comboIndex,
                                comboData.animationSpeed
                            );
                        }

                        // 히트박스 생성 (비동기)
                        SpawnHitbox(comboData, cancellationToken);

                        attackTriggered = true;
                    }
                    else
                    {
                        LogStateDebug("콤보 데이터 없음 - 기본 공격 실행");
                        // 콤보 데이터 없으면 기본 공격
                        ExecuteBasicAttack();
                    }
                }
                else
                {
                    LogStateDebug("콤보 등록 실패 - 기본 공격 실행");
                    ExecuteBasicAttack();
                }
            }
            else
            {
                LogStateDebug("ComboSystem 없음 - 기본 공격 실행");
                ExecuteBasicAttack();
            }

            await Awaitable.NextFrameAsync();
        }

        protected override async Awaitable ExitState(CancellationToken cancellationToken)
        {
            LogStateDebug("공격 상태 종료");
            attackTriggered = false;
            comboWindowActive = false;

            await Awaitable.NextFrameAsync();
        }

        protected override void UpdateState(float deltaTime)
        {
            if (!attackTriggered) return;

            attackAnimationTime += deltaTime;

            // FSM의 Attack→Attack 자동 전환에 의존하여 콤보 처리
            // CheckComboInput() 제거 (중복 전환 방지)

            CheckForStateTransitions();
        }

        /// <summary>
        /// 기본 공격 실행 (ComboSystem 없을 때)
        /// </summary>
        private void ExecuteBasicAttack()
        {
            // GAS 시스템을 통한 기본 공격
            playerController.ActivateAbility("BasicAttack");

            // AttackAnimationHandler를 통해 애니메이션 재생
            if (playerController.AttackAnimationHandler != null)
            {
                playerController.AttackAnimationHandler.TriggerAttackAnimation(0, 1f);
            }

            attackTriggered = true;
        }

        private void CheckForStateTransitions()
        {
            if (playerController == null) return;

            // AttackAnimationHandler가 있으면 애니메이션 상태 확인
            if (playerController.AttackAnimationHandler != null)
            {
                // 공격 중이 아니면 다음 상태로 전환
                if (!playerController.IsAttacking)
                {
                    TransitionToNextState();
                }
            }
            else
            {
                // AttackAnimationHandler 없으면 0.3초 타이머 사용
                if (attackAnimationTime >= 0.3f)
                {
                    TransitionToNextState();
                }
            }
        }

        private void TransitionToNextState()
        {
            if (playerController.IsGrounded)
            {
                if (Mathf.Abs(playerController.Velocity.x) > 0.1f)
                    playerController.ChangeState(PlayerStateType.Move);
                else
                    playerController.ChangeState(PlayerStateType.Idle);
            }
            else
            {
                playerController.ChangeState(PlayerStateType.Fall);
            }
        }

        /// <summary>
        /// 히트박스 생성 및 데미지 적용
        /// </summary>
        private async void SpawnHitbox(ComboData comboData, CancellationToken cancellationToken)
        {
            try
            {
                // 히트박스 생성 딜레이
                await Awaitable.WaitForSecondsAsync(HitboxSpawnDelay, cancellationToken);

                if (cancellationToken.IsCancellationRequested) return;

                // 플레이어 위치 및 방향
                Vector3 playerPosition = playerController.transform.position;
                int facingDirection = playerController.FacingDirection;

                // 히트박스 중심 위치 계산 (플레이어 앞쪽)
                Vector2 hitboxOffset = new Vector2(
                    comboData.hitboxOffset.x * facingDirection,
                    comboData.hitboxOffset.y
                );
                Vector3 hitboxCenter = playerPosition + (Vector3)hitboxOffset;

                // 히트박스 크기
                Vector2 hitboxSize = comboData.hitboxSize;

                // 데미지 데이터 생성
                float baseDamage = 10f; // 기본 데미지
                float totalDamage = baseDamage * comboData.damageMultiplier;

                var damageData = DamageData.CreateWithKnockback(
                    totalDamage,
                    DamageType.Physical,
                    playerController.gameObject,
                    comboData.knockbackForce * facingDirection
                );

                // 박스 범위 데미지 적용
                var hitTargets = DamageSystem.ApplyBoxDamage(
                    hitboxCenter,
                    hitboxSize,
                    0f, // 회전 없음
                    damageData,
                    LayerMask.GetMask("Default") // Enemy 레이어 또는 Default 레이어 타겟
                );

                LogStateDebug($"히트박스 생성: {hitTargets.Count}개 타격, 데미지: {totalDamage}");

                // 히트박스 시각화 (디버그용)
                DrawHitboxDebug(hitboxCenter, hitboxSize, comboData.hitboxDuration);
            }
            catch (System.OperationCanceledException)
            {
                // 상태 전환으로 인한 취소는 정상
                LogStateDebug("히트박스 생성 취소됨");
            }
        }

        /// <summary>
        /// 히트박스 디버그 시각화
        /// </summary>
        private async void DrawHitboxDebug(Vector3 center, Vector2 size, float duration)
        {
            var go = new GameObject("Hitbox_Debug");
            go.transform.position = center;

            // SpriteRenderer로 시각화
            var sr = go.AddComponent<SpriteRenderer>();
            sr.color = new Color(1f, 0f, 0f, 0.3f); // 반투명 빨간색

            // Static 리소스 재사용 (메모리 누수 방지)
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

            // 일정 시간 후 파괴
            await Awaitable.WaitForSecondsAsync(duration);
            if (go != null) Object.Destroy(go);
        }
    }
}
