using System.Threading;
using UnityEngine;
using GASPT.Data;

namespace GASPT.Loot
{
    /// <summary>
    /// 월드에 드롭된 아이템 V2 (ItemInstance 기반)
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(CircleCollider2D))]
    public class DroppedItemV2 : MonoBehaviour
    {
        // ====== 아이템 데이터 ======

        private ItemInstance itemInstance;
        private int quantity = 1;


        // ====== 컴포넌트 참조 ======

        private SpriteRenderer spriteRenderer;
        private CircleCollider2D circleCollider;


        // ====== 설정 ======

        [Header("애니메이션 설정")]
        [Tooltip("부유 애니메이션 속도")]
        [SerializeField] private float floatSpeed = 1f;

        [Tooltip("부유 애니메이션 높이")]
        [SerializeField] private float floatHeight = 0.3f;


        // ====== 내부 상태 ======

        private Vector3 startPosition;
        private CancellationTokenSource floatCts;
        private CancellationTokenSource lifetimeCts;
        private bool isPickedUp = false;
        private float lifetime = 300f;


        // ====== 프로퍼티 ======

        /// <summary>
        /// 아이템 인스턴스
        /// </summary>
        public ItemInstance ItemInstance => itemInstance;

        /// <summary>
        /// 아이템 수량
        /// </summary>
        public int Quantity => quantity;

        /// <summary>
        /// 픽업 여부
        /// </summary>
        public bool IsPickedUp => isPickedUp;


        // ====== 초기화 ======

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            circleCollider = GetComponent<CircleCollider2D>();

            if (circleCollider != null)
            {
                circleCollider.isTrigger = true;
            }
        }

        private void OnDestroy()
        {
            StopAllAnimations();
        }


        // ====== Public 메서드 ======

        /// <summary>
        /// DroppedItem 초기화
        /// </summary>
        /// <param name="instance">아이템 인스턴스</param>
        /// <param name="qty">수량</param>
        /// <param name="itemLifetime">수명 (초)</param>
        public void Initialize(ItemInstance instance, int qty = 1, float itemLifetime = 300f)
        {
            if (instance == null || !instance.IsValid)
            {
                Debug.LogError("[DroppedItemV2] Initialize: 유효하지 않은 인스턴스입니다.");
                Destroy(gameObject);
                return;
            }

            itemInstance = instance;
            quantity = qty;
            lifetime = itemLifetime;
            startPosition = transform.position;

            // 아이콘 설정
            if (spriteRenderer != null && instance.cachedItemData?.icon != null)
            {
                spriteRenderer.sprite = instance.cachedItemData.icon;

                // 등급 색상 적용 (틴트)
                spriteRenderer.color = instance.cachedItemData.RarityColor;
            }

            // 애니메이션 시작
            StartFloatAnimation();

            // 수명 타이머 시작
            StartLifetimeTimer();

            Debug.Log($"[DroppedItemV2] {instance.cachedItemData?.itemName} x{quantity} 생성 (수명: {lifetime}초)");
        }

        /// <summary>
        /// 픽업 처리 (ItemDropManager에서 호출)
        /// </summary>
        public void OnPickedUp()
        {
            if (isPickedUp)
                return;

            isPickedUp = true;

            // 오브젝트 파괴
            Destroy(gameObject);
        }


        // ====== 충돌 감지 ======

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (isPickedUp)
                return;

            // Player 태그 확인
            if (other.CompareTag("Player"))
            {
                // ItemDropManager를 통해 픽업
                if (ItemDropManager.HasInstance)
                {
                    ItemDropManager.Instance.PickupItem(this);
                }
            }
        }


        // ====== 애니메이션 ======

        private void StartFloatAnimation()
        {
            StopFloatAnimation();

            floatCts = new CancellationTokenSource();
            FloatAnimationAsync(floatCts.Token);
        }

        private async void FloatAnimationAsync(CancellationToken ct)
        {
            try
            {
                float elapsedTime = 0f;

                while (!ct.IsCancellationRequested)
                {
                    await Awaitable.NextFrameAsync(ct);

                    elapsedTime += Time.deltaTime * floatSpeed;

                    float yOffset = Mathf.Sin(elapsedTime) * floatHeight;
                    transform.position = startPosition + new Vector3(0f, yOffset, 0f);
                }
            }
            catch (System.OperationCanceledException)
            {
                // 정상 취소
            }
        }

        private void StopFloatAnimation()
        {
            if (floatCts != null)
            {
                floatCts.Cancel();
                floatCts.Dispose();
                floatCts = null;
            }
        }


        // ====== 수명 타이머 ======

        private void StartLifetimeTimer()
        {
            lifetimeCts = new CancellationTokenSource();
            LifetimeTimerAsync(lifetimeCts.Token);
        }

        private async void LifetimeTimerAsync(CancellationToken ct)
        {
            try
            {
                await Awaitable.WaitForSecondsAsync(lifetime, ct);

                if (!isPickedUp)
                {
                    Debug.Log($"[DroppedItemV2] {itemInstance?.cachedItemData?.itemName} 수명 만료");
                    Destroy(gameObject);
                }
            }
            catch (System.OperationCanceledException)
            {
                // 정상 취소
            }
        }

        private void StopAllAnimations()
        {
            StopFloatAnimation();

            if (lifetimeCts != null)
            {
                lifetimeCts.Cancel();
                lifetimeCts.Dispose();
                lifetimeCts = null;
            }
        }


        // ====== 디버그 ======

        [ContextMenu("Print Info")]
        private void PrintInfo()
        {
            Debug.Log("========== DroppedItemV2 Info ==========");
            Debug.Log($"Item: {itemInstance?.cachedItemData?.itemName ?? "NULL"}");
            Debug.Log($"Quantity: {quantity}");
            Debug.Log($"Rarity: {itemInstance?.cachedItemData?.rarity}");
            Debug.Log($"Position: {transform.position}");
            Debug.Log($"Is Picked Up: {isPickedUp}");
            Debug.Log("=========================================");
        }
    }
}
