using UnityEngine;

namespace GASPT.Gameplay.Enemy
{
    /// <summary>
    /// 비행 적
    /// FSM: Idle → Fly → PositionAbove → DiveAttack → ReturnToAir → Dead
    /// 공중에서 이동하며 플레이어 머리 위에서 급강하 공격
    ///
    /// ✅ PlatformerEnemy 상속으로 중복 코드 제거:
    /// - 컴포넌트 필드 (rb, col, spriteRenderer)
    /// - 플레이어 참조 (playerTransform, playerStats)
    /// - 공통 메서드 (FindPlayer, Stop, IsPlayerInDetectionRange 등)
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class FlyingEnemy : PlatformerEnemy
    {
        // ====== 비행 전용 상태 ======

        /// <summary>
        /// 비행 적 AI 상태
        /// </summary>
        public enum FlyingState
        {
            Idle,           // 대기
            Fly,            // 자유 비행
            PositionAbove,  // 플레이어 위로 이동
            DiveAttack,     // 급강하 공격
            ReturnToAir,    // 공중 복귀
            Dead            // 사망
        }

        private FlyingState flyingState = FlyingState.Idle;
        private FlyingState previousFlyingState = FlyingState.Idle;


        // ====== 비행 설정 ======

        [Header("비행 설정")]
        [SerializeField] private float targetFlyHeight = 6f;
        [SerializeField] private LayerMask groundLayer = 1 << 8; // Ground Layer


        // ====== 비행 전용 상태 변수 ======

        private Vector3 diveStartPosition;
        private bool isDiving;


        // ====== Unity 생명주기 (Override) ======

        protected override void Start()
        {
            base.Start(); // PlatformerEnemy.Start() 호출

            // 비행 높이 초기화
            targetFlyHeight = transform.position.y + (Data != null ? Data.flyHeight : 6f);

            ChangeFlyingState(FlyingState.Idle);
        }

        protected override void Update()
        {
            if (IsDead)
            {
                if (flyingState != FlyingState.Dead)
                {
                    ChangeFlyingState(FlyingState.Dead);
                }
                return;
            }

            UpdateFlyingState();
        }

        protected override void FixedUpdate()
        {
            if (IsDead) return;

            PhysicsUpdateFlying();
        }


        // ====== 초기화 (Override) ======

        /// <summary>
        /// 컴포넌트 초기화 (비행 적 전용)
        /// </summary>
        protected override void InitializeComponents()
        {
            base.InitializeComponents(); // PlatformerEnemy 초기화

            // 비행 특성: 중력 비활성화
            if (rb != null)
            {
                rb.gravityScale = 0f;
            }

            if (showDebugLogs)
                Debug.Log($"[FlyingEnemy] {Data?.enemyName} 컴포넌트 초기화 완료 (중력 무시)");
        }


        // ====== FSM 상태 관리 (비행 전용) ======

        /// <summary>
        /// 비행 상태 변경
        /// </summary>
        private void ChangeFlyingState(FlyingState newState)
        {
            if (flyingState == newState) return;

            previousFlyingState = flyingState;
            flyingState = newState;

            if (showDebugLogs)
                Debug.Log($"[FlyingEnemy] {Data?.enemyName} 상태 변경: {previousFlyingState} → {flyingState}");

            OnFlyingStateExit(previousFlyingState);
            OnFlyingStateEnter(flyingState);
        }

        /// <summary>
        /// 비행 상태 진입 시 호출
        /// </summary>
        private void OnFlyingStateEnter(FlyingState state)
        {
            switch (state)
            {
                case FlyingState.Idle:
                    Stop();
                    break;

                case FlyingState.Fly:
                    // 자유 비행 시작
                    break;

                case FlyingState.PositionAbove:
                    if (showDebugLogs)
                        Debug.Log($"[FlyingEnemy] {Data?.enemyName} 플레이어 위로 이동 시작!");
                    break;

                case FlyingState.DiveAttack:
                    diveStartPosition = transform.position;
                    isDiving = true;
                    if (showDebugLogs)
                        Debug.Log($"[FlyingEnemy] {Data?.enemyName} 급강하 공격!");
                    break;

                case FlyingState.ReturnToAir:
                    isDiving = false;
                    if (showDebugLogs)
                        Debug.Log($"[FlyingEnemy] {Data?.enemyName} 공중 복귀!");
                    break;

                case FlyingState.Dead:
                    Stop();
                    if (col != null)
                        col.enabled = false;
                    // 중력 적용 (떨어짐)
                    if (rb != null)
                        rb.gravityScale = 2f;
                    break;
            }
        }

        /// <summary>
        /// 비행 상태 퇴장 시 호출
        /// </summary>
        private void OnFlyingStateExit(FlyingState state)
        {
            // 필요 시 구현
        }

        /// <summary>
        /// 비행 상태 업데이트 (매 프레임)
        /// </summary>
        private void UpdateFlyingState()
        {
            switch (flyingState)
            {
                case FlyingState.Idle:
                    UpdateIdle();
                    break;

                case FlyingState.Fly:
                    UpdateFly();
                    break;

                case FlyingState.PositionAbove:
                    UpdatePositionAbove();
                    break;

                case FlyingState.DiveAttack:
                    UpdateDiveAttack();
                    break;

                case FlyingState.ReturnToAir:
                    UpdateReturnToAir();
                    break;

                case FlyingState.Dead:
                    // 아무것도 안 함
                    break;
            }
        }

        /// <summary>
        /// 비행 물리 업데이트 (FixedUpdate)
        /// </summary>
        private void PhysicsUpdateFlying()
        {
            if (Data == null) return;

            switch (flyingState)
            {
                case FlyingState.Fly:
                    PhysicsFly();
                    break;

                case FlyingState.PositionAbove:
                    PhysicsPositionAbove();
                    break;

                case FlyingState.DiveAttack:
                    PhysicsDiveAttack();
                    break;

                case FlyingState.ReturnToAir:
                    PhysicsReturnToAir();
                    break;
            }
        }


        // ====== Idle 상태 ======

        private void UpdateIdle()
        {
            // 일정 시간 후 Fly 상태로 전환
            ChangeFlyingState(FlyingState.Fly);
        }


        // ====== Fly 상태 ======

        private void UpdateFly()
        {
            // 플레이어 감지 체크 (PlatformerEnemy의 메서드 사용)
            if (IsPlayerInDetectionRange())
            {
                ChangeFlyingState(FlyingState.PositionAbove);
                return;
            }

            // 자유 비행 (PhysicsUpdate에서 처리)
        }

        private void PhysicsFly()
        {
            // 목표 높이 유지하면서 좌우로 이동
            float currentHeight = transform.position.y;
            Vector2 velocity = rb.linearVelocity;

            // 높이 조정
            if (Mathf.Abs(currentHeight - targetFlyHeight) > 0.5f)
            {
                float verticalDirection = currentHeight < targetFlyHeight ? 1f : -1f;
                velocity.y = verticalDirection * Data.flySpeed;
            }
            else
            {
                velocity.y = 0f;
            }

            // 좌우 이동 (랜덤)
            if (Random.value < 0.02f)
            {
                isFacingRight = !isFacingRight;
            }

            velocity.x = (isFacingRight ? 1f : -1f) * Data.flySpeed * 0.5f;

            rb.linearVelocity = velocity;

            // 스프라이트 반전 (PlatformerEnemy의 Flip 로직 활용)
            UpdateFlyingDirection();
        }


        // ====== PositionAbove 상태 ======

        private void UpdatePositionAbove()
        {
            // 목표 위치 도달 체크 (PhysicsUpdate에서 이동)
            if (IsAbovePlayer())
            {
                ChangeFlyingState(FlyingState.DiveAttack);
            }
        }

        private void PhysicsPositionAbove()
        {
            if (playerTransform == null) return;

            // 플레이어 위 목표 위치 계산
            Vector3 targetPosition = new Vector3(
                playerTransform.position.x,
                playerTransform.position.y + Data.flyHeight,
                transform.position.z
            );

            // 목표 위치로 이동
            Vector2 direction = (targetPosition - transform.position).normalized;
            rb.linearVelocity = direction * Data.flySpeed;

            // 스프라이트 반전
            if (direction.x > 0.01f)
                isFacingRight = true;
            else if (direction.x < -0.01f)
                isFacingRight = false;

            UpdateFlyingDirection();
        }


        // ====== DiveAttack 상태 ======

        private void UpdateDiveAttack()
        {
            // 지면 또는 플레이어 충돌 체크 (PhysicsUpdate에서 이동)
            if (IsGroundBelow() || HasDivedEnough())
            {
                ChangeFlyingState(FlyingState.ReturnToAir);
            }
        }

        private void PhysicsDiveAttack()
        {
            if (playerTransform == null) return;

            // 플레이어 방향으로 급강하
            Vector2 diveDirection = (playerTransform.position - transform.position).normalized;
            rb.linearVelocity = diveDirection * Data.diveSpeed;

            // 스프라이트 반전
            if (diveDirection.x > 0.01f)
                isFacingRight = true;
            else if (diveDirection.x < -0.01f)
                isFacingRight = false;

            UpdateFlyingDirection();
        }


        // ====== ReturnToAir 상태 ======

        private void UpdateReturnToAir()
        {
            // 목표 높이 도달 체크
            if (Mathf.Abs(transform.position.y - targetFlyHeight) < 0.5f)
            {
                ChangeFlyingState(FlyingState.Fly);
                lastAttackTime = Time.time;
            }
        }

        private void PhysicsReturnToAir()
        {
            // 위로 상승
            rb.linearVelocity = new Vector2(0f, Data.flySpeed);
        }


        // ====== 비행 전용 유틸리티 ======

        /// <summary>
        /// 플레이어 위에 있는지 확인
        /// </summary>
        private bool IsAbovePlayer()
        {
            if (playerTransform == null) return false;

            Vector3 targetPosition = new Vector3(
                playerTransform.position.x,
                playerTransform.position.y + Data.flyHeight,
                transform.position.z
            );

            return Vector2.Distance(transform.position, targetPosition) < 1f;
        }

        /// <summary>
        /// 충분히 급강하했는지 확인
        /// </summary>
        private bool HasDivedEnough()
        {
            float divedDistance = diveStartPosition.y - transform.position.y;
            return divedDistance > Data.flyHeight + 2f;
        }

        /// <summary>
        /// 지면이 아래에 있는지 확인
        /// </summary>
        private bool IsGroundBelow()
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 1f, groundLayer);
            return hit.collider != null;
        }

        /// <summary>
        /// 비행 방향에 따라 스프라이트 업데이트
        /// </summary>
        private void UpdateFlyingDirection()
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = !isFacingRight;
            }
        }


        // ====== 충돌 ======

        private void OnTriggerEnter2D(Collider2D collision)
        {
            // 급강하 공격 중 플레이어와 충돌
            if (isDiving && collision.CompareTag("Player"))
            {
                var player = collision.GetComponent<GASPT.Stats.PlayerStats>();
                if (player != null && !player.IsDead)
                {
                    DealDamageTo(player); // PlatformerEnemy의 메서드 사용
                    ChangeFlyingState(FlyingState.ReturnToAir);

                    if (showDebugLogs)
                        Debug.Log($"[FlyingEnemy] {Data?.enemyName} 플레이어 급강하 공격 성공!");
                }
            }
        }


        // ====== Gizmos (Override) ======

        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos(); // PlatformerEnemy Gizmos

            if (!showGizmos || Data == null) return;

            // 목표 비행 높이 (시안색)
            Gizmos.color = Color.cyan;
            Vector3 heightLine = transform.position;
            heightLine.y = targetFlyHeight;
            Gizmos.DrawLine(heightLine - Vector3.right * 2f, heightLine + Vector3.right * 2f);
        }

        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected(); // PlatformerEnemy Gizmos

            if (Data == null) return;

            // 플레이어 위 목표 위치
            if (playerTransform != null)
            {
                Vector3 targetPosition = new Vector3(
                    playerTransform.position.x,
                    playerTransform.position.y + Data.flyHeight,
                    transform.position.z
                );
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(targetPosition, 0.5f);
            }

            // 현재 비행 상태 표시
            UnityEditor.Handles.Label(transform.position + Vector3.up * 3f,
                $"FlyingState: {flyingState}\nHP: {CurrentHp}/{MaxHp}");
        }


        // ====== 디버그 ======

        [ContextMenu("Print FlyingEnemy Info")]
        private void DebugPrintInfo()
        {
            Debug.Log($"=== {Data?.enemyName} FlyingEnemy Info ===\n" +
                     $"Flying State: {flyingState}\n" +
                     $"Position: {transform.position}\n" +
                     $"Target Fly Height: {targetFlyHeight}\n" +
                     $"Is Diving: {isDiving}\n" +
                     $"In Detection Range: {IsPlayerInDetectionRange()}\n" +
                     $"Is Above Player: {IsAbovePlayer()}\n" +
                     $"Facing Right: {isFacingRight}\n" +
                     $"==========================================");
        }

        [ContextMenu("Force State: Idle")]
        private void DebugForceIdle() => ChangeFlyingState(FlyingState.Idle);

        [ContextMenu("Force State: Fly")]
        private void DebugForceFly() => ChangeFlyingState(FlyingState.Fly);

        [ContextMenu("Force State: PositionAbove")]
        private void DebugForcePositionAbove() => ChangeFlyingState(FlyingState.PositionAbove);

        [ContextMenu("Force State: DiveAttack")]
        private void DebugForceDiveAttack() => ChangeFlyingState(FlyingState.DiveAttack);
    }
}
