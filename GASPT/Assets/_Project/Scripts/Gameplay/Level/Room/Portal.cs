using UnityEngine;
using Core;

namespace GASPT.Gameplay.Level
{
    /// <summary>
    /// 포탈 (다음 방으로 이동)
    /// 방 클리어 시 활성화되어 플레이어가 다음 방으로 이동할 수 있게 함
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Portal : MonoBehaviour
    {
        // ====== 설정 ======

        [Header("포탈 설정")]
        [Tooltip("포탈 타입")]
        [SerializeField] private PortalType portalType = PortalType.NextRoom;

        [Tooltip("특정 방 인덱스로 이동 (PortalType이 SpecificRoom일 때)")]
        [SerializeField] private int targetRoomIndex = 0;

        [Tooltip("방 클리어 시 자동 활성화")]
        [SerializeField] private bool autoActivateOnRoomClear = true;

        [Tooltip("초기 활성화 상태")]
        [SerializeField] private bool startActive = false;


        // ====== 비주얼 ======

        [Header("비주얼")]
        [Tooltip("포탈 스프라이트 (선택사항)")]
        [SerializeField] private SpriteRenderer portalSprite;

        [Tooltip("포탈 이펙트 (선택사항)")]
        [SerializeField] private ParticleSystem portalEffect;

        [Tooltip("비활성 색상")]
        [SerializeField] private Color inactiveColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

        [Tooltip("활성 색상")]
        [SerializeField] private Color activeColor = new Color(0, 1f, 1f, 1f); // 시안색


        // ====== 상태 ======

        private bool isActive = false;
        private Collider2D portalCollider;
        private Room parentRoom;


        // ====== Unity 생명주기 ======

        private void Awake()
        {
            portalCollider = GetComponent<Collider2D>();
            portalCollider.isTrigger = true;

            // 부모 Room 찾기
            parentRoom = GetComponentInParent<Room>();

            // 초기 상태 설정
            SetActive(startActive);
        }

        private void Start()
        {
            // Room 클리어 이벤트 구독
            if (autoActivateOnRoomClear && parentRoom != null)
            {
                parentRoom.OnRoomClear += OnRoomCleared;
            }
        }

        private void OnDestroy()
        {
            // 이벤트 구독 해제
            if (parentRoom != null)
            {
                parentRoom.OnRoomClear -= OnRoomCleared;
            }
        }


        // ====== 충돌 처리 ======

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!isActive) return;

            // 플레이어 확인 (PlayerStats 컴포넌트로 확인)
            if (other.TryGetComponent<GASPT.Stats.PlayerStats>(out var player))
            {
                OnPlayerEnter(player);
            }
        }


        // ====== 플레이어 진입 ======

        /// <summary>
        /// 플레이어가 포탈에 진입했을 때
        /// </summary>
        private void OnPlayerEnter(GASPT.Stats.PlayerStats player)
        {
            Debug.Log($"[Portal] 플레이어가 포탈에 진입!");

            // 다음 방으로 이동
            UsePortalAsync().Forget();
        }

        /// <summary>
        /// 포탈 사용 (비동기)
        /// </summary>
        private async Awaitable UsePortalAsync()
        {
            if (RoomManager.Instance == null)
            {
                Debug.LogError("[Portal] RoomManager를 찾을 수 없습니다!");
                return;
            }

            // 포탈 비활성화 (중복 사용 방지)
            SetActive(false);

            // 포탈 사용 연출 (페이드 아웃 등)
            await Awaitable.WaitForSecondsAsync(0.3f);

            // 방 이동
            switch (portalType)
            {
                case PortalType.NextRoom:
                    await RoomManager.Instance.MoveToNextRoomAsync();
                    break;

                case PortalType.SpecificRoom:
                    await RoomManager.Instance.MoveToRoomAsync(targetRoomIndex);
                    break;

                case PortalType.RandomRoom:
                    int randomIndex = Random.Range(0, RoomManager.Instance.TotalRoomCount);
                    await RoomManager.Instance.MoveToRoomAsync(randomIndex);
                    break;
            }

            Debug.Log($"[Portal] 포탈 사용 완료!");
        }


        // ====== Room 이벤트 처리 ======

        /// <summary>
        /// 방 클리어 시 호출
        /// </summary>
        private void OnRoomCleared(Room room)
        {
            if (autoActivateOnRoomClear)
            {
                SetActive(true);
                Debug.Log($"[Portal] 방 클리어 - 포탈 활성화!");
            }
        }


        // ====== 포탈 활성화/비활성화 ======

        /// <summary>
        /// 포탈 활성화/비활성화
        /// </summary>
        public void SetActive(bool active)
        {
            isActive = active;

            // Collider 활성화/비활성화
            if (portalCollider != null)
            {
                portalCollider.enabled = active;
            }

            // 비주얼 업데이트
            UpdateVisual();

            Debug.Log($"[Portal] {name} 포탈 {(active ? "활성화" : "비활성화")}");
        }

        /// <summary>
        /// 비주얼 업데이트
        /// </summary>
        private void UpdateVisual()
        {
            // SpriteRenderer 색상 변경
            if (portalSprite != null)
            {
                portalSprite.color = isActive ? activeColor : inactiveColor;
            }

            // ParticleSystem 재생/중지
            if (portalEffect != null)
            {
                if (isActive)
                {
                    if (!portalEffect.isPlaying)
                    {
                        portalEffect.Play();
                    }
                }
                else
                {
                    if (portalEffect.isPlaying)
                    {
                        portalEffect.Stop();
                    }
                }
            }
        }


        // ====== Gizmos ======

        private void OnDrawGizmos()
        {
            Gizmos.color = isActive ? Color.cyan : Color.gray;
            Gizmos.DrawWireSphere(transform.position, 0.5f);

            // 위쪽 화살표
            Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 1f);
        }

        private void OnDrawGizmosSelected()
        {
            // 선택 시 채워진 구
            Gizmos.color = new Color(0, 1f, 1f, 0.3f);
            Gizmos.DrawSphere(transform.position, 0.5f);

#if UNITY_EDITOR
            // 포탈 정보 표시
            string info = $"Portal: {portalType}\n";
            if (portalType == PortalType.SpecificRoom)
            {
                info += $"Target: Room {targetRoomIndex}";
            }
            info += $"\nActive: {isActive}";

            UnityEditor.Handles.Label(transform.position + Vector3.up * 1.5f, info);
#endif
        }


        // ====== 디버그 ======

        [ContextMenu("Activate Portal")]
        private void DebugActivate()
        {
            SetActive(true);
        }

        [ContextMenu("Deactivate Portal")]
        private void DebugDeactivate()
        {
            SetActive(false);
        }

        [ContextMenu("Print Portal Info")]
        private void PrintPortalInfo()
        {
            Debug.Log($"[Portal] {name}\n" +
                     $"Type: {portalType}\n" +
                     $"Active: {isActive}\n" +
                     $"Auto Activate On Clear: {autoActivateOnRoomClear}\n" +
                     $"Parent Room: {(parentRoom != null ? parentRoom.name : "None")}");
        }
    }


    // ====== 열거형 ======

    /// <summary>
    /// 포탈 타입
    /// </summary>
    public enum PortalType
    {
        NextRoom,       // 다음 방으로
        SpecificRoom,   // 특정 방으로
        RandomRoom      // 랜덤 방으로
    }
}
