using System;
using UnityEngine;
using GASPT.Stats;

namespace GASPT.Gameplay.Reward
{
    /// <summary>
    /// 체력 회복 물약 픽업
    /// 플레이어와 접촉 시 자동 획득
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class HealthPotion : MonoBehaviour
    {
        // ====== 설정 ======

        [Header("회복량")]
        [SerializeField] private int healAmount = 30;
        [SerializeField] private bool healPercentage = false;
        [SerializeField] [Range(0f, 1f)] private float healPercent = 0.3f;

        [Header("애니메이션")]
        [SerializeField] private float bobSpeed = 2.5f;
        [SerializeField] private float bobHeight = 0.1f;

        [Header("사운드")]
        [SerializeField] private AudioClip pickupSound;


        // ====== 상태 ======

        private Vector3 initialPosition;
        private bool isPickedUp;


        // ====== 이벤트 ======

        public event Action<int> OnPickedUp;


        // ====== 프로퍼티 ======

        public int HealAmount => healAmount;


        // ====== Unity 생명주기 ======

        private void Awake()
        {
            initialPosition = transform.position;

            var collider = GetComponent<Collider2D>();
            if (collider != null)
            {
                collider.isTrigger = true;
            }
        }

        private void Update()
        {
            if (isPickedUp) return;

            // 위아래 흔들림
            float newY = initialPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (isPickedUp) return;
            if (!other.CompareTag("Player")) return;

            PickUp(other.gameObject);
        }


        // ====== 초기화 ======

        /// <summary>
        /// 회복량 설정
        /// </summary>
        public void Initialize(int amount)
        {
            healAmount = Mathf.Max(1, amount);
            healPercentage = false;
        }

        /// <summary>
        /// 퍼센트 회복량 설정
        /// </summary>
        public void InitializePercent(float percent)
        {
            healPercent = Mathf.Clamp01(percent);
            healPercentage = true;
        }


        // ====== 픽업 ======

        private void PickUp(GameObject player)
        {
            if (isPickedUp) return;
            isPickedUp = true;

            // PlayerStats 찾기
            var playerStats = player.GetComponent<PlayerStats>();
            if (playerStats == null)
            {
                playerStats = player.GetComponentInParent<PlayerStats>();
            }

            if (playerStats != null)
            {
                int actualHeal = healAmount;

                // 퍼센트 회복인 경우
                if (healPercentage)
                {
                    actualHeal = Mathf.RoundToInt(playerStats.MaxHP * healPercent);
                }

                // 회복
                playerStats.Heal(actualHeal);

                Debug.Log($"[HealthPotion] 체력 회복: {actualHeal} HP");

                OnPickedUp?.Invoke(actualHeal);
            }
            else
            {
                Debug.LogWarning("[HealthPotion] PlayerStats를 찾을 수 없습니다.");
            }

            // 사운드 재생
            if (pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);
            }

            // 오브젝트 제거
            Destroy(gameObject);
        }
    }
}
