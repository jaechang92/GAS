using System;
using UnityEngine;
using GASPT.Economy;

namespace GASPT.Gameplay.Reward
{
    /// <summary>
    /// 골드 픽업 아이템
    /// 플레이어와 접촉 시 자동 획득
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class GoldPickup : MonoBehaviour
    {
        // ====== 설정 ======

        [Header("골드")]
        [SerializeField] private int goldAmount = 10;

        [Header("애니메이션")]
        [SerializeField] private float bobSpeed = 3f;
        [SerializeField] private float bobHeight = 0.15f;
        [SerializeField] private float magnetRange = 2f;
        [SerializeField] private float magnetSpeed = 8f;

        [Header("사운드")]
        [SerializeField] private AudioClip pickupSound;


        // ====== 상태 ======

        private Vector3 initialPosition;
        private bool isPickedUp;
        private Transform playerTransform;
        private bool isMagneting;


        // ====== 이벤트 ======

        public event Action<int> OnPickedUp;


        // ====== 프로퍼티 ======

        public int GoldAmount => goldAmount;


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
            if (!isMagneting)
            {
                float newY = initialPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
                transform.position = new Vector3(transform.position.x, newY, transform.position.z);
            }

            // 자석 효과 (플레이어에게 끌려감)
            if (isMagneting && playerTransform != null)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    playerTransform.position,
                    magnetSpeed * Time.deltaTime
                );
            }

            // 플레이어 근처에서 자석 효과 시작
            CheckMagnetRange();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (isPickedUp) return;
            if (!other.CompareTag("Player")) return;

            PickUp();
        }


        // ====== 초기화 ======

        /// <summary>
        /// 골드 양 설정
        /// </summary>
        public void Initialize(int amount)
        {
            goldAmount = Mathf.Max(1, amount);
            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            // 골드 양에 따라 크기 조절
            float scale = 0.3f + Mathf.Min(goldAmount / 100f, 0.5f);
            transform.localScale = Vector3.one * scale;
        }


        // ====== 자석 효과 ======

        private void CheckMagnetRange()
        {
            if (isMagneting) return;

            var player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return;

            float distance = Vector3.Distance(transform.position, player.transform.position);
            if (distance <= magnetRange)
            {
                isMagneting = true;
                playerTransform = player.transform;
            }
        }


        // ====== 픽업 ======

        private void PickUp()
        {
            if (isPickedUp) return;
            isPickedUp = true;

            // 골드 추가
            if (CurrencySystem.HasInstance)
            {
                CurrencySystem.Instance.AddGold(goldAmount);
            }

            // 사운드 재생
            if (pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);
            }

            Debug.Log($"[GoldPickup] 골드 획득: {goldAmount}G");

            OnPickedUp?.Invoke(goldAmount);

            // 오브젝트 제거
            Destroy(gameObject);
        }


        // ====== 에디터 ======

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, magnetRange);
        }
    }
}
