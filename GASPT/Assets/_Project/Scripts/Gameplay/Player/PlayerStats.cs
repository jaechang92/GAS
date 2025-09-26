using UnityEngine;

namespace Player
{
    /// <summary>
    /// 플레이어 스탯 정의 ScriptableObject
    /// 플레이어의 기본 능력치와 설정값들을 관리
    /// </summary>
    [CreateAssetMenu(fileName = "New Player Stats", menuName = "GASPT/Player/Player Stats")]
    public class PlayerStats : ScriptableObject
    {
        [Header("기본 스탯")]
        [SerializeField] public int maxHealth = 100;
        [SerializeField] public int currentHealth = 100;
        [SerializeField] public int lives = 3;

        [Header("이동 관련")]
        [SerializeField] public float moveSpeed = 8f;
        [SerializeField] public float acceleration = 50f;
        [SerializeField] public float deceleration = 50f;

        [Header("점프 관련")]
        [SerializeField] public float jumpForce = 15f;
        [SerializeField] public float jumpBufferTime = 0.1f;
        [SerializeField] public float coyoteTime = 0.1f;
        [SerializeField] public int maxJumpCount = 2; // 더블 점프
        [SerializeField] public float jumpCutMultiplier = 0.5f; // 점프 버튼을 뗄 때 속도 감소

        [Header("대시 관련")]
        [SerializeField] public float dashSpeed = 20f;
        [SerializeField] public float dashDuration = 0.2f;
        [SerializeField] public float dashCooldown = 1f;
        [SerializeField] public bool canDashInAir = true;

        [Header("벽 관련")]
        [SerializeField] public float wallSlideSpeed = 2f;
        [SerializeField] public float wallJumpForce = 12f;
        [SerializeField] public float wallJumpHorizontalForce = 8f;
        [SerializeField] public float wallGrabDuration = 1f;

        [Header("공격 관련")]
        [SerializeField] public int attackDamage = 25;
        [SerializeField] public float attackRange = 1.5f;
        [SerializeField] public float attackCooldown = 0.5f;
        [SerializeField] public float comboWindow = 1f;

        [Header("물리 관련")]
        [SerializeField] public float gravityScale = 3f;
        [SerializeField] public float fallMultiplier = 2.5f;
        [SerializeField] public float lowJumpMultiplier = 2f;
        [SerializeField] public float maxFallSpeed = 20f;

        [Header("슬라이딩 관련")]
        [SerializeField] public float slideSpeed = 12f;
        [SerializeField] public float slideDuration = 0.8f;
        [SerializeField] public float slideColliderHeight = 0.5f;

        /// <summary>
        /// 스탯을 기본값으로 리셋
        /// </summary>
        public void ResetToDefaults()
        {
            currentHealth = maxHealth;
            // 다른 런타임 값들도 여기서 리셋
        }

        /// <summary>
        /// 체력 회복
        /// </summary>
        public void Heal(int amount)
        {
            currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        }

        /// <summary>
        /// 데미지 받기
        /// </summary>
        public void TakeDamage(int damage)
        {
            currentHealth = Mathf.Max(currentHealth - damage, 0);
        }

        /// <summary>
        /// 죽었는지 확인
        /// </summary>
        public bool IsDead()
        {
            return currentHealth <= 0;
        }

        /// <summary>
        /// 생명 감소
        /// </summary>
        public void LoseLife()
        {
            lives = Mathf.Max(lives - 1, 0);
        }

        /// <summary>
        /// 게임 오버 확인
        /// </summary>
        public bool IsGameOver()
        {
            return lives <= 0;
        }

        private void OnValidate()
        {
            // 에디터에서 값 변경 시 검증
            maxHealth = Mathf.Max(1, maxHealth);
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
            lives = Mathf.Max(0, lives);

            moveSpeed = Mathf.Max(0, moveSpeed);
            jumpForce = Mathf.Max(0, jumpForce);
            dashSpeed = Mathf.Max(0, dashSpeed);

            dashDuration = Mathf.Max(0.1f, dashDuration);
            dashCooldown = Mathf.Max(0, dashCooldown);

            attackDamage = Mathf.Max(1, attackDamage);
            attackRange = Mathf.Max(0.1f, attackRange);

            gravityScale = Mathf.Max(0.1f, gravityScale);
            fallMultiplier = Mathf.Max(1f, fallMultiplier);
            lowJumpMultiplier = Mathf.Max(1f, lowJumpMultiplier);
            maxFallSpeed = Mathf.Max(1f, maxFallSpeed);
        }
    }
}