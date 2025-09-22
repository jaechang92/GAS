using System.Threading;
using UnityEngine;

namespace Player
{
    /// <summary>
    /// 플레이어 죽음 상태
    /// 플레이어가 죽었을 때의 상태
    /// </summary>
    public class PlayerDeadState : PlayerBaseState
    {
        private float deathDuration = 2f;
        private float deathTime = 0f;
        private bool respawnTriggered = false;

        public PlayerDeadState() : base(PlayerStateType.Dead) { }

        protected override async Awaitable EnterState(CancellationToken cancellationToken)
        {
            LogStateDebug("죽음 상태 진입");

            // 죽음 초기화
            deathTime = 0f;
            respawnTriggered = false;

            // 물리 정지
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.gravityScale = 0f;
            }

            // 죽음 이펙트
            ShowDeathEffect();

            // 게임 매니저에 죽음 알림
            NotifyGameManager();

            await Awaitable.NextFrameAsync();
        }

        protected override async Awaitable ExitState(CancellationToken cancellationToken)
        {
            LogStateDebug("죽음 상태 종료");

            // 물리 복구
            if (rb != null)
            {
                rb.gravityScale = 3f;
            }

            // 죽음 이펙트 제거
            HideDeathEffect();

            await Awaitable.NextFrameAsync();
        }

        protected override void UpdateState(float deltaTime)
        {
            deathTime += deltaTime;

            // 자동 리스폰 처리
            if (deathTime >= deathDuration && !respawnTriggered)
            {
                TriggerRespawn();
            }
        }

        private void ShowDeathEffect()
        {
            // 플레이어 스프라이트 회전 (죽음 표현)
            if (playerController != null)
            {
                playerController.transform.rotation = Quaternion.Euler(0, 0, 90f);
            }

            // 스프라이트 색상 변경
            SpriteRenderer spriteRenderer = playerController.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.gray;
            }

            // TODO: 죽음 파티클 이펙트 추가
            LogStateDebug("죽음 이펙트 표시");
        }

        private void HideDeathEffect()
        {
            // 플레이어 스프라이트 회전 복구
            if (playerController != null)
            {
                playerController.transform.rotation = Quaternion.identity;
            }

            // 스프라이트 색상 복구
            SpriteRenderer spriteRenderer = playerController.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.white;
            }

            LogStateDebug("죽음 이펙트 제거");
        }

        private void NotifyGameManager()
        {
            // GameManager에 플레이어 죽음 알림
            if (Managers.GameManager.TryGetInstance(out var gameManager))
            {
                gameManager.LoseLife();
                LogStateDebug("GameManager에 생명 감소 알림");
            }
        }

        private void TriggerRespawn()
        {
            if (respawnTriggered) return;

            respawnTriggered = true;

            // GameManager에서 게임 오버 확인
            if (Managers.GameManager.TryGetInstance(out var gameManager))
            {
                if (gameManager.IsGameOver)
                {
                    // 게임 오버 처리
                    LogStateDebug("게임 오버 - GameOver 씬으로 전환");
                    // TODO: GameOver 씬으로 전환
                    return;
                }
            }

            // 리스폰 처리
            RespawnPlayer();
        }

        private void RespawnPlayer()
        {
            LogStateDebug("플레이어 리스폰");

            // 체력 회복
            // TODO: PlayerStats에서 체력 회복 처리

            // 스폰 위치로 이동
            Vector3 spawnPosition = GetSpawnPosition();
            if (playerController != null)
            {
                playerController.transform.position = spawnPosition;
            }

            // Idle 상태로 전환
            StateMachine?.ForceTransitionTo(PlayerStateType.Idle.ToString());
        }

        private Vector3 GetSpawnPosition()
        {
            // 기본 스폰 위치 (나중에 체크포인트 시스템으로 확장)
            return Vector3.zero;

            // TODO: 체크포인트 시스템 구현 후 마지막 체크포인트 위치 반환
        }

        // 외부에서 즉시 리스폰을 트리거할 때 사용
        public void ForceRespawn()
        {
            if (!respawnTriggered)
            {
                TriggerRespawn();
            }
        }
    }
}