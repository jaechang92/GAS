using System;
using UnityEngine;

namespace GASPT.Forms
{
    /// <summary>
    /// 던전에서 드롭되는 폼 픽업 아이템
    /// 플레이어가 상호작용하여 폼을 획득
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class FormPickup : MonoBehaviour
    {
        // ====== 이벤트 ======

        /// <summary>픽업 시도 이벤트 (성공 여부)</summary>
        public event Action<bool> OnPickupAttempted;

        /// <summary>픽업 성공 이벤트</summary>
        public event Action<FormData> OnPickedUp;

        /// <summary>선택 필요 이벤트 (슬롯 가득 참)</summary>
        public event Action<FormData> OnSelectionRequired;


        // ====== 설정 ======

        [Header("폼 데이터")]
        [SerializeField] private FormData formData;

        [Header("상호작용")]
        [SerializeField] private KeyCode interactKey = KeyCode.F;
        [SerializeField] private float interactRange = 1.5f;
        [SerializeField] private bool autoPickup = false;

        [Header("시각 효과")]
        [SerializeField] private SpriteRenderer iconRenderer;
        [SerializeField] private GameObject highlightEffect;
        [SerializeField] private GameObject floatingEffect;

        [Header("사운드")]
        [SerializeField] private AudioClip pickupSound;
        [SerializeField] private AudioClip appearSound;

        [Header("애니메이션")]
        [SerializeField] private float bobSpeed = 2f;
        [SerializeField] private float bobHeight = 0.2f;
        [SerializeField] private float rotateSpeed = 30f;


        // ====== 상태 ======

        private bool isPlayerInRange;
        private Transform playerTransform;
        private Vector3 initialPosition;
        private bool isPickedUp;
        private FormManager formManager;


        // ====== 프로퍼티 ======

        /// <summary>이 픽업의 폼 데이터</summary>
        public FormData Data => formData;

        /// <summary>플레이어가 범위 내에 있는지</summary>
        public bool IsPlayerInRange => isPlayerInRange;

        /// <summary>이미 획득되었는지</summary>
        public bool IsPickedUp => isPickedUp;


        // ====== Unity 생명주기 ======

        private void Awake()
        {
            initialPosition = transform.position;
            SetupCollider();
        }

        private void Start()
        {
            UpdateVisuals();
            PlayAppearEffect();
        }

        private void Update()
        {
            if (isPickedUp) return;

            AnimatePickup();
            CheckPlayerInput();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;

            isPlayerInRange = true;
            playerTransform = other.transform;
            formManager = other.GetComponentInParent<FormManager>();

            ShowHighlight(true);

            if (autoPickup)
            {
                TryPickup();
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;

            isPlayerInRange = false;
            playerTransform = null;

            ShowHighlight(false);
        }


        // ====== 초기화 ======

        private void SetupCollider()
        {
            var collider = GetComponent<Collider2D>();
            if (collider != null)
            {
                collider.isTrigger = true;
            }
        }

        /// <summary>
        /// 폼 데이터 설정 (동적 생성 시)
        /// </summary>
        public void Initialize(FormData data)
        {
            formData = data;
            UpdateVisuals();
        }


        // ====== 상호작용 ======

        private void CheckPlayerInput()
        {
            if (!isPlayerInRange) return;
            if (autoPickup) return;

            if (Input.GetKeyDown(interactKey))
            {
                TryPickup();
            }
        }

        /// <summary>
        /// 픽업 시도
        /// </summary>
        public void TryPickup()
        {
            if (isPickedUp || formData == null) return;

            if (formManager == null)
            {
                Debug.LogWarning("[FormPickup] FormManager를 찾을 수 없습니다.");
                OnPickupAttempted?.Invoke(false);
                return;
            }

            var result = formManager.TryAcquireForm(formData);

            switch (result)
            {
                case AcquireResult.AcquiredPrimary:
                case AcquireResult.AcquiredSecondary:
                case AcquireResult.Awakened:
                    CompletePickup();
                    break;

                case AcquireResult.SlotFull:
                    // 슬롯이 가득 참 - 선택 UI 필요
                    OnSelectionRequired?.Invoke(formData);
                    Debug.Log($"[FormPickup] 슬롯이 가득 참. 교체할 폼을 선택하세요: {formData.formName}");
                    break;

                case AcquireResult.MaxAwakening:
                    Debug.Log($"[FormPickup] {formData.formName}은 이미 최대 각성 상태입니다.");
                    OnPickupAttempted?.Invoke(false);
                    break;

                default:
                    OnPickupAttempted?.Invoke(false);
                    break;
            }
        }

        /// <summary>
        /// 슬롯 선택 후 교체 완료 (UI에서 호출)
        /// </summary>
        public void CompletePickupWithReplacement(int slotIndex)
        {
            if (isPickedUp || formData == null || formManager == null) return;

            var droppedForm = formManager.ReplaceForm(slotIndex, formData);

            // 버려진 폼을 새 픽업으로 드롭
            if (droppedForm != null)
            {
                SpawnDroppedForm(droppedForm);
            }

            CompletePickup();
        }

        private void CompletePickup()
        {
            isPickedUp = true;

            PlayPickupSound();
            PlayPickupEffect();

            OnPickedUp?.Invoke(formData);
            OnPickupAttempted?.Invoke(true);

            Debug.Log($"[FormPickup] 폼 획득: {formData.formName}");

            // 오브젝트 제거
            Destroy(gameObject, 0.1f);
        }


        // ====== 드롭 ======

        /// <summary>
        /// 버려진 폼을 새 픽업으로 생성
        /// </summary>
        private void SpawnDroppedForm(FormInstance droppedForm)
        {
            if (droppedForm?.Data == null) return;

            // 약간 옆에 드롭
            Vector3 dropPosition = transform.position + new Vector3(
                UnityEngine.Random.Range(-1f, 1f),
                0.5f,
                0
            );

            var pickup = CreateFormPickup(droppedForm.Data, dropPosition);
            if (pickup != null)
            {
                Debug.Log($"[FormPickup] 폼 드롭: {droppedForm.FormName}");
            }
        }

        /// <summary>
        /// 폼 픽업 오브젝트 생성 (정적 메서드)
        /// </summary>
        public static FormPickup CreateFormPickup(FormData data, Vector3 position)
        {
            if (data == null) return null;

            // 프리팹 로드 시도
            var prefab = Resources.Load<GameObject>("Prefabs/Forms/FormPickup");

            GameObject pickupObj;
            if (prefab != null)
            {
                pickupObj = Instantiate(prefab, position, Quaternion.identity);
            }
            else
            {
                // 프리팹 없으면 기본 오브젝트 생성
                pickupObj = new GameObject($"FormPickup_{data.formName}");
                pickupObj.transform.position = position;

                // 기본 컴포넌트 추가
                var sr = pickupObj.AddComponent<SpriteRenderer>();
                sr.sprite = data.icon;
                sr.sortingOrder = 10;

                var collider = pickupObj.AddComponent<CircleCollider2D>();
                collider.radius = 0.5f;
                collider.isTrigger = true;

                pickupObj.AddComponent<FormPickup>();
            }

            var pickup = pickupObj.GetComponent<FormPickup>();
            pickup.Initialize(data);

            return pickup;
        }


        // ====== 시각 효과 ======

        private void UpdateVisuals()
        {
            if (formData == null) return;

            // 아이콘 설정
            if (iconRenderer != null && formData.icon != null)
            {
                iconRenderer.sprite = formData.icon;
            }

            // SpriteRenderer가 없으면 직접 찾기
            if (iconRenderer == null)
            {
                iconRenderer = GetComponent<SpriteRenderer>();
                if (iconRenderer != null && formData.icon != null)
                {
                    iconRenderer.sprite = formData.icon;
                }
            }

            // 등급별 색상 적용
            if (iconRenderer != null)
            {
                iconRenderer.color = GetRarityColor(formData.baseRarity);
            }
        }

        private void AnimatePickup()
        {
            // 위아래 흔들림
            float newY = initialPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);

            // 회전
            if (rotateSpeed > 0)
            {
                transform.Rotate(Vector3.forward, rotateSpeed * Time.deltaTime);
            }
        }

        private void ShowHighlight(bool show)
        {
            if (highlightEffect != null)
            {
                highlightEffect.SetActive(show);
            }
        }

        private void PlayAppearEffect()
        {
            if (floatingEffect != null)
            {
                floatingEffect.SetActive(true);
            }

            if (appearSound != null)
            {
                AudioSource.PlayClipAtPoint(appearSound, transform.position);
            }
        }

        private void PlayPickupEffect()
        {
            // 픽업 파티클 등
        }

        private void PlayPickupSound()
        {
            if (pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);
            }
        }

        private Color GetRarityColor(FormRarity rarity)
        {
            return rarity switch
            {
                FormRarity.Common => Color.white,
                FormRarity.Rare => new Color(0.3f, 0.5f, 1f),      // 파랑
                FormRarity.Unique => new Color(0.7f, 0.3f, 0.9f),  // 보라
                FormRarity.Legendary => new Color(1f, 0.8f, 0.2f), // 금색
                _ => Color.white
            };
        }


        // ====== 에디터 ======

        private void OnDrawGizmosSelected()
        {
            // 상호작용 범위 표시
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactRange);
        }

        private void OnValidate()
        {
            if (formData != null)
            {
                gameObject.name = $"FormPickup_{formData.formName}";
            }
        }
    }
}
