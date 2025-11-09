using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Threading;
using GASPT.Data;
using GASPT.Loot;

namespace GASPT.UI
{
    /// <summary>
    /// 아이템 획득 알림 UI
    /// 획득한 아이템을 화면 상단에 팝업으로 표시
    /// </summary>
    public class ItemPickupUI : MonoBehaviour
    {
        // ====== UI 참조 ======

        [Header("UI References")]
        [SerializeField] private Transform slotContainer;
        [SerializeField] private GameObject pickupSlotPrefab;


        // ====== 설정 ======

        [Header("설정")]
        [Tooltip("동시에 표시할 최대 슬롯 수")]
        [SerializeField] private int maxSlots = 5;

        [Tooltip("팝업 표시 시간 (초)")]
        [SerializeField] private float displayDuration = 3f;

        [Tooltip("페이드 애니메이션 시간 (초)")]
        [SerializeField] private float fadeDuration = 0.5f;


        // ====== 슬롯 풀링 ======

        private List<ItemPickupSlot> slotPool = new List<ItemPickupSlot>();
        private Queue<ItemPickupSlot> availableSlots = new Queue<ItemPickupSlot>();


        // ====== 초기화 ======

        private void Start()
        {
            InitializeSlotPool();
            SubscribeToEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }


        // ====== 슬롯 풀 초기화 ======

        private void InitializeSlotPool()
        {
            if (pickupSlotPrefab == null)
            {
                Debug.LogError("[ItemPickupUI] pickupSlotPrefab이 할당되지 않았습니다!");
                return;
            }

            if (slotContainer == null)
            {
                slotContainer = transform;
            }

            // 슬롯 풀 생성
            for (int i = 0; i < maxSlots; i++)
            {
                GameObject slotObj = Instantiate(pickupSlotPrefab, slotContainer);
                ItemPickupSlot slot = slotObj.GetComponent<ItemPickupSlot>();

                if (slot != null)
                {
                    slot.gameObject.SetActive(false);
                    slotPool.Add(slot);
                    availableSlots.Enqueue(slot);
                }
                else
                {
                    Debug.LogError("[ItemPickupUI] ItemPickupSlot 컴포넌트를 찾을 수 없습니다!");
                }
            }

            Debug.Log($"[ItemPickupUI] 슬롯 풀 초기화 완료: {slotPool.Count}개");
        }


        // ====== 이벤트 구독 ======

        private void SubscribeToEvents()
        {
            if (LootSystem.HasInstance)
            {
                LootSystem.Instance.OnItemPickedUp += OnItemPickedUp;
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (LootSystem.HasInstance)
            {
                LootSystem.Instance.OnItemPickedUp -= OnItemPickedUp;
            }
        }


        // ====== 이벤트 핸들러 ======

        private void OnItemPickedUp(Item item)
        {
            ShowPickupNotification(item);
        }


        // ====== 알림 표시 ======

        /// <summary>
        /// 아이템 획득 알림 표시
        /// </summary>
        public void ShowPickupNotification(Item item)
        {
            if (item == null)
            {
                Debug.LogWarning("[ItemPickupUI] ShowPickupNotification(): item이 null입니다.");
                return;
            }

            // 사용 가능한 슬롯 가져오기
            ItemPickupSlot slot = GetAvailableSlot();
            if (slot == null)
            {
                Debug.LogWarning("[ItemPickupUI] 사용 가능한 슬롯이 없습니다!");
                return;
            }

            // 슬롯 표시
            slot.Show(item, displayDuration, fadeDuration, () =>
            {
                // 애니메이션 완료 후 슬롯 반환
                ReturnSlotToPool(slot);
            });
        }


        // ====== 슬롯 관리 ======

        private ItemPickupSlot GetAvailableSlot()
        {
            if (availableSlots.Count > 0)
            {
                return availableSlots.Dequeue();
            }
            return null;
        }

        private void ReturnSlotToPool(ItemPickupSlot slot)
        {
            if (slot != null)
            {
                slot.gameObject.SetActive(false);
                availableSlots.Enqueue(slot);
            }
        }


        // ====== Context Menu (테스트용) ======

        [ContextMenu("Test: Show Random Pickup")]
        private void TestShowRandomPickup()
        {
            Debug.LogWarning("[ItemPickupUI] TestShowRandomPickup(): 테스트용 메서드입니다. 실제 아이템이 필요합니다.");
        }

        [ContextMenu("Print Pool Status")]
        private void PrintPoolStatus()
        {
            Debug.Log($"[ItemPickupUI] 총 슬롯: {slotPool.Count}, 사용 가능: {availableSlots.Count}");
        }
    }


    /// <summary>
    /// 개별 아이템 획득 슬롯 UI
    /// </summary>
    public class ItemPickupSlot : MonoBehaviour
    {
        // ====== UI 참조 ======

        [Header("UI References")]
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private CanvasGroup canvasGroup;


        // ====== 내부 상태 ======

        private CancellationTokenSource animationCts;
        private System.Action onComplete;


        // ====== 초기화 ======

        private void OnDestroy()
        {
            StopAnimation();
        }


        // ====== Public 메서드 ======

        /// <summary>
        /// 슬롯 표시
        /// </summary>
        public void Show(Item item, float displayDuration, float fadeDuration, System.Action onCompleteCallback)
        {
            if (item == null)
                return;

            onComplete = onCompleteCallback;

            // UI 설정
            if (iconImage != null && item.icon != null)
            {
                iconImage.sprite = item.icon;
            }

            if (nameText != null)
            {
                nameText.text = $"{item.itemName} 획득!";
            }

            // 애니메이션 시작
            StopAnimation();
            gameObject.SetActive(true);

            animationCts = new CancellationTokenSource();
            PlayAnimationAsync(displayDuration, fadeDuration, animationCts.Token);
        }


        // ====== 애니메이션 ======

        private async void PlayAnimationAsync(float displayDuration, float fadeDuration, CancellationToken ct)
        {
            try
            {
                // 페이드 인
                await FadeAsync(0f, 1f, fadeDuration, ct);

                // 표시 시간 대기
                await Awaitable.WaitForSecondsAsync(displayDuration, ct);

                // 페이드 아웃
                await FadeAsync(1f, 0f, fadeDuration, ct);

                // 완료 콜백 호출
                onComplete?.Invoke();
            }
            catch (System.OperationCanceledException)
            {
                // 취소는 정상 동작
            }
        }

        private async Awaitable FadeAsync(float startAlpha, float endAlpha, float duration, CancellationToken ct)
        {
            if (canvasGroup == null)
                return;

            float elapsedTime = 0f;

            while (elapsedTime < duration && !ct.IsCancellationRequested)
            {
                await Awaitable.NextFrameAsync(ct);

                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / duration);
                canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, t);
            }

            // 최종 값 설정
            if (!ct.IsCancellationRequested)
            {
                canvasGroup.alpha = endAlpha;
            }
        }

        private void StopAnimation()
        {
            if (animationCts != null)
            {
                animationCts.Cancel();
                animationCts.Dispose();
                animationCts = null;
            }
        }
    }
}
