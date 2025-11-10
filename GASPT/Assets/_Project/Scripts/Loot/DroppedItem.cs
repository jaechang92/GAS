using UnityEngine;
using System.Threading;
using GASPT.Data;

namespace GASPT.Loot
{
    /// <summary>
    /// 월드에 드롭된 아이템 MonoBehaviour
    /// 플레이어가 획득할 수 있는 아이템 오브젝트
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(CircleCollider2D))]
    public class DroppedItem : MonoBehaviour
    {
        // ====== 아이템 데이터 ======

        private Item itemData;


        // ====== 컴포넌트 참조 ======

        private SpriteRenderer spriteRenderer;
        private CircleCollider2D circleCollider;


        // ====== 설정 ======

        [Header("애니메이션 설정")]
        [Tooltip("부유 애니메이션 속도")]
        [SerializeField] private float floatSpeed = 1f;

        [Tooltip("부유 애니메이션 높이")]
        [SerializeField] private float floatHeight = 0.3f;

        [Header("수명 설정")]
        [Tooltip("자동 소멸까지의 시간 (초)")]
        [SerializeField] private float lifetime = 30f;


        // ====== 내부 상태 ======

        private Vector3 startPosition;
        private CancellationTokenSource floatCts;
        private CancellationTokenSource lifetimeCts;
        private bool isPickedUp = false;


        // ====== 초기화 ======

        private void Awake()
        {
            // 컴포넌트 가져오기
            spriteRenderer = GetComponent<SpriteRenderer>();
            circleCollider = GetComponent<CircleCollider2D>();

            // Collider를 Trigger로 설정
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
        /// <param name="item">드롭된 아이템 데이터</param>
        public void Initialize(Item item)
        {
            if (item == null)
            {
                Debug.LogError("[DroppedItem] Initialize(): item이 null입니다.");
                Destroy(gameObject);
                return;
            }

            itemData = item;
            startPosition = transform.position;

            // 아이콘 설정
            if (spriteRenderer != null && item.icon != null)
            {
                spriteRenderer.sprite = item.icon;
            }

            // 애니메이션 시작
            StartFloatAnimation();

            // 수명 타이머 시작
            StartLifetimeTimer();

            Debug.Log($"[DroppedItem] {item.itemName} 생성 완료 (수명: {lifetime}초)");
        }

        /// <summary>
        /// 아이템 획득
        /// </summary>
        public void PickUp()
        {
            if (isPickedUp)
                return;

            isPickedUp = true;

            if (itemData == null)
            {
                Debug.LogError("[DroppedItem] PickUp(): itemData가 null입니다.");
                Destroy(gameObject);
                return;
            }

            // LootSystem에 획득 알림
            if (LootSystem.HasInstance)
            {
                LootSystem.Instance.PickUpItem(itemData);
            }

            // 오브젝트 파괴
            Destroy(gameObject);
        }


        // ====== 충돌 감지 ======

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Player 태그 확인
            if (other.CompareTag("Player"))
            {
                PickUp();
            }
        }


        // ====== 애니메이션 ======

        /// <summary>
        /// 부유 애니메이션 시작
        /// </summary>
        private void StartFloatAnimation()
        {
            StopFloatAnimation();

            floatCts = new CancellationTokenSource();
            FloatAnimationAsync(floatCts.Token);
        }

        /// <summary>
        /// 부유 애니메이션 (Awaitable 기반)
        /// </summary>
        private async void FloatAnimationAsync(CancellationToken ct)
        {
            try
            {
                float elapsedTime = 0f;

                while (!ct.IsCancellationRequested)
                {
                    await Awaitable.NextFrameAsync(ct);

                    elapsedTime += Time.deltaTime * floatSpeed;

                    // Sin 곡선으로 위아래 움직임
                    float yOffset = Mathf.Sin(elapsedTime) * floatHeight;
                    transform.position = startPosition + new Vector3(0f, yOffset, 0f);
                }
            }
            catch (System.OperationCanceledException)
            {
                // 취소는 정상 동작
            }
        }

        /// <summary>
        /// 부유 애니메이션 중지
        /// </summary>
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

        /// <summary>
        /// 수명 타이머 시작
        /// </summary>
        private void StartLifetimeTimer()
        {
            lifetimeCts = new CancellationTokenSource();
            LifetimeTimerAsync(lifetimeCts.Token);
        }

        /// <summary>
        /// 수명 타이머 (Awaitable 기반)
        /// </summary>
        private async void LifetimeTimerAsync(CancellationToken ct)
        {
            try
            {
                // lifetime 동안 대기
                await Awaitable.WaitForSecondsAsync(lifetime, ct);

                // 시간 경과 후 자동 소멸
                if (!isPickedUp)
                {
                    Debug.Log($"[DroppedItem] {itemData?.itemName} 수명 만료로 소멸");
                    Destroy(gameObject);
                }
            }
            catch (System.OperationCanceledException)
            {
                // 취소는 정상 동작
            }
        }

        /// <summary>
        /// 모든 애니메이션 중지
        /// </summary>
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


        // ====== Getters ======

        public Item ItemData => itemData;
        public bool IsPickedUp => isPickedUp;


        // ====== Context Menu (테스트용) ======

        [ContextMenu("Test: Pick Up")]
        private void TestPickUp()
        {
            PickUp();
        }

        [ContextMenu("Print Info")]
        private void PrintInfo()
        {
            Debug.Log("========== DroppedItem Info ==========");
            Debug.Log($"Item: {(itemData != null ? itemData.itemName : "NULL")}");
            Debug.Log($"Position: {transform.position}");
            Debug.Log($"Is Picked Up: {isPickedUp}");
            Debug.Log($"Float Speed: {floatSpeed}");
            Debug.Log($"Float Height: {floatHeight}");
            Debug.Log($"Lifetime: {lifetime}s");
            Debug.Log("=====================================");
        }
    }
}
