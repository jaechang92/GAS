using System;
using System.Collections.Generic;
using UnityEngine;
using Core;
using GASPT.UI;
using GASPT.Core;
using GASPT.Gameplay.Level.Graph;

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


        // ====== UI ======

        [Header("UI")]
        [Tooltip("포탈 상호작용 UI (PortalUI) - 자동으로 생성됨")]
        private PortalUI portalUI;


        // ====== 상태 ======

        private bool isActive = false;
        private Collider2D portalCollider;
        private Room parentRoom;
        private bool playerInRange = false;
        private GASPT.Stats.PlayerStats playerInPortal = null;


        // ====== 그래프 기반 연결 (신규) ======

        [Header("그래프 기반 연결")]
        [Tooltip("연결된 노드 목록 (BranchSelection 타입에서 사용)")]
        private List<DungeonNode> connectedNodes = new List<DungeonNode>();

        [Tooltip("단일 대상 노드 (NextRoom 타입에서 사용)")]
        private DungeonNode targetNode;

        /// <summary>
        /// 분기 선택 이벤트 (여러 노드 중 선택 필요 시)
        /// </summary>
        public event Action<List<DungeonNode>> OnBranchSelectionRequired;

        /// <summary>
        /// 노드 선택 완료 이벤트
        /// </summary>
        public event Action<DungeonNode> OnNodeSelected;


        // ====== Unity 생명주기 ======

        private void Awake()
        {
            portalCollider = GetComponent<Collider2D>();
            portalCollider.isTrigger = true;

            // 부모 Room 찾기 (DungeonEntrance 타입은 Room 없이도 동작 가능)
            parentRoom = GetComponentInParent<Room>();

            // 초기 상태 설정
            SetActive(startActive);

            // 디버그 로그 (DungeonEntrance는 Room 없어도 정상)
            if (parentRoom == null && portalType != PortalType.DungeonEntrance)
            {
                Debug.LogWarning($"[Portal] {name}: 부모 Room을 찾을 수 없습니다! Portal은 Room의 자식이어야 합니다.");
            }
            else if (parentRoom != null)
            {
                Debug.Log($"[Portal] {name}: 부모 Room 찾기 성공 - {parentRoom.name}");
            }
        }

        private void Start()
        {
            // Room 클리어 이벤트 구독 (DungeonEntrance 타입은 Room 클리어와 무관)
            if (autoActivateOnRoomClear && parentRoom != null)
            {
                parentRoom.OnRoomClear += OnRoomCleared;
                Debug.Log($"[Portal] {name}: Room 클리어 이벤트 구독 완료 (AutoActivate: {autoActivateOnRoomClear})");
            }
            else if (portalType == PortalType.DungeonEntrance)
            {
                // DungeonEntrance는 Room 클리어 이벤트 필요 없음 (항상 활성 상태)
                Debug.Log($"[Portal] {name}: DungeonEntrance 포탈 - Room 클리어 이벤트 구독 생략");
            }
            else if (autoActivateOnRoomClear && parentRoom == null)
            {
                Debug.LogWarning($"[Portal] {name}: Room 클리어 이벤트 구독 실패! (ParentRoom이 null)");
            }
        }

        private void Update()
        {
            // E키 입력 체크 (플레이어가 포탈 범위 안에 있고, 포탈이 활성화 상태일 때)
            if (playerInRange && isActive && playerInPortal != null)
            {
                if (Input.GetKeyDown(KeyCode.E))
                {
                    OnPlayerUsePortal();
                }
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
                playerInRange = true;
                playerInPortal = player;

                // PortalUI 생성 또는 찾기
                if (portalUI == null)
                {
                    portalUI = CreateOrFindPortalUI();
                }

                if (portalUI != null)
                {
                    // 포탈 타입에 따라 메시지 변경
                    string message = GetPortalMessage();
                    portalUI.SetMessage(message);
                    portalUI.Show();
                }

                Debug.Log($"[Portal] 플레이어가 포탈 범위 진입!");
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            // 플레이어 확인
            if (other.TryGetComponent<GASPT.Stats.PlayerStats>(out var player))
            {
                playerInRange = false;
                playerInPortal = null;

                // PortalUI 숨김
                if (portalUI != null)
                {
                    portalUI.Hide();
                }

                Debug.Log($"[Portal] 플레이어가 포탈 범위 벗어남!");
            }
        }


        // ====== 포탈 사용 ======

        /// <summary>
        /// 플레이어가 포탈 사용 (E키 입력 시)
        /// </summary>
        private void OnPlayerUsePortal()
        {
            Debug.Log($"[Portal] 플레이어가 포탈 사용!");

            // PortalUI 숨김
            if (portalUI != null)
            {
                portalUI.Hide();
            }

            // 플레이어 범위 초기화
            playerInRange = false;
            playerInPortal = null;

            // 다음 방으로 이동
            UsePortalAsync().Forget();
        }

        /// <summary>
        /// 포탈 사용 (비동기)
        /// </summary>
        private async Awaitable UsePortalAsync()
        {
            // 포탈 비활성화 (중복 사용 방지)
            SetActive(false);

            // 포탈 사용 연출 (페이드 아웃 등)
            await Awaitable.WaitForSecondsAsync(0.3f);

            // 방 이동
            switch (portalType)
            {
                case PortalType.DungeonEntrance:
                    // StartRoom → Dungeon 입장 (GameFlowStateMachine 사용)
                    var gameFlowFSM = GameFlowStateMachine.Instance;
                    if (gameFlowFSM != null)
                    {
                        gameFlowFSM.TriggerEnterDungeon();
                        Debug.Log($"[Portal] 던전 입장 이벤트 트리거!");
                    }
                    else
                    {
                        Debug.LogError("[Portal] GameFlowStateMachine을 찾을 수 없습니다!");
                    }
                    break;

                case PortalType.NextRoom:
                    // 던전 내 다음 방 이동 (GameFlowStateMachine 사용)
                    var gameFlow = GameFlowStateMachine.Instance;
                    if (gameFlow != null)
                    {
                        gameFlow.TriggerEnterNextRoom();
                        Debug.Log($"[Portal] 다음 방 이동 이벤트 트리거!");
                    }
                    else
                    {
                        Debug.LogError("[Portal] GameFlowStateMachine을 찾을 수 없습니다!");
                    }
                    break;

                case PortalType.SpecificRoom:
                    // 특정 방으로 직접 이동 (RoomManager 직접 사용)
                    if (RoomManager.Instance != null)
                    {
                        await RoomManager.Instance.MoveToRoomAsync(targetRoomIndex);
                        Debug.Log($"[Portal] 특정 방({targetRoomIndex})으로 이동 완료!");
                    }
                    else
                    {
                        Debug.LogError("[Portal] RoomManager를 찾을 수 없습니다!");
                    }
                    break;

                case PortalType.RandomRoom:
                    // 랜덤 방으로 직접 이동 (RoomManager 직접 사용)
                    if (RoomManager.Instance != null)
                    {
                        int randomIndex = UnityEngine.Random.Range(0, RoomManager.Instance.TotalRoomCount);
                        await RoomManager.Instance.MoveToRoomAsync(randomIndex);
                        Debug.Log($"[Portal] 랜덤 방({randomIndex})으로 이동 완료!");
                    }
                    else
                    {
                        Debug.LogError("[Portal] RoomManager를 찾을 수 없습니다!");
                    }
                    break;

                case PortalType.BranchSelection:
                    // 분기 선택 (그래프 기반)
                    // 포탈을 다시 활성화 (분기 선택 UI에서 선택 후 이동)
                    SetActive(true);
                    ShowBranchSelection();
                    return; // 여기서 리턴 (선택 후 이동은 SelectNode에서 처리)
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


        // ====== 유틸리티 ======

        /// <summary>
        /// PortalUI 생성 또는 찾기
        /// </summary>
        private PortalUI CreateOrFindPortalUI()
        {
            // 1. 먼저 씬에서 PortalUI 찾기
            PortalUI existingUI = FindAnyObjectByType<PortalUI>(FindObjectsInactive.Include);
            if (existingUI != null)
            {
                Debug.Log("[Portal] 기존 PortalUI 찾기 성공!");
                return existingUI;
            }

            // 2. 없으면 Resources.Load로 Prefab 로드
            GameObject uiPrefab = Resources.Load<GameObject>("Prefabs/UI/PortalUI");
            if (uiPrefab == null)
            {
                Debug.LogError("[Portal] PortalUI Prefab을 찾을 수 없습니다! 경로: Resources/Prefabs/UI/PortalUI.prefab");
                return null;
            }

            // 3. Canvas 찾기
            Canvas canvas = FindAnyObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[Portal] Canvas를 찾을 수 없습니다! PortalUI를 생성할 수 없습니다.");
                return null;
            }

            // 4. PortalUI Prefab 인스턴스화
            GameObject uiObj = Instantiate(uiPrefab, canvas.transform);
            uiObj.name = "PortalUI";

            PortalUI ui = uiObj.GetComponent<PortalUI>();
            if (ui == null)
            {
                Debug.LogError("[Portal] 생성한 PortalUI에 PortalUI 컴포넌트가 없습니다!");
                Destroy(uiObj);
                return null;
            }

            Debug.Log("[Portal] PortalUI Prefab을 Resources.Load로 생성 완료!");
            return ui;
        }

        /// <summary>
        /// 포탈 타입에 따른 메시지 반환
        /// </summary>
        private string GetPortalMessage()
        {
            switch (portalType)
            {
                case PortalType.DungeonEntrance:
                    return "E 키를 눌러 던전 입장";

                case PortalType.NextRoom:
                    return "E 키를 눌러 다음 방으로 이동";

                case PortalType.SpecificRoom:
                    return $"E 키를 눌러 {targetRoomIndex}번 방으로 이동";

                case PortalType.RandomRoom:
                    return "E 키를 눌러 랜덤 방으로 이동";

                default:
                    return "E 키를 눌러 이동";
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


        // ====== 그래프 기반 포탈 설정 (신규) ======

        /// <summary>
        /// 단일 대상 노드 설정
        /// </summary>
        public void SetTargetNode(DungeonNode node)
        {
            targetNode = node;
            connectedNodes.Clear();
            if (node != null)
            {
                connectedNodes.Add(node);
            }
            Debug.Log($"[Portal] {name}: 대상 노드 설정 - {node?.nodeId} ({node?.roomType})");
        }

        /// <summary>
        /// 여러 대상 노드 설정 (분기 선택용)
        /// </summary>
        public void SetConnectedNodes(List<DungeonNode> nodes)
        {
            connectedNodes = nodes ?? new List<DungeonNode>();
            targetNode = connectedNodes.Count == 1 ? connectedNodes[0] : null;

            // 다중 목적지면 BranchSelection 타입으로 변경
            if (connectedNodes.Count > 1)
            {
                portalType = PortalType.BranchSelection;
            }

            Debug.Log($"[Portal] {name}: {connectedNodes.Count}개 노드 연결됨");
        }

        /// <summary>
        /// 연결된 노드 목록 조회
        /// </summary>
        public List<DungeonNode> GetConnectedNodes()
        {
            return new List<DungeonNode>(connectedNodes);
        }

        /// <summary>
        /// 분기 선택 UI 표시 (여러 목적지 중 선택)
        /// </summary>
        private void ShowBranchSelection()
        {
            if (connectedNodes.Count <= 1)
            {
                // 단일 목적지면 바로 이동
                if (connectedNodes.Count == 1)
                {
                    SelectNode(connectedNodes[0]);
                }
                return;
            }

            Debug.Log($"[Portal] 분기 선택 UI 표시 - {connectedNodes.Count}개 선택지");

            // 분기 선택 이벤트 발생 (UI가 구독하여 처리)
            OnBranchSelectionRequired?.Invoke(connectedNodes);

            // Fallback: 이벤트 구독자가 없으면 직접 Presenter 찾아서 호출
            if (OnBranchSelectionRequired == null)
            {
                var presenter = FindAnyObjectByType<GASPT.UI.MVP.BranchSelectionPresenter>();
                if (presenter != null)
                {
                    presenter.ShowBranchSelection(connectedNodes, this);
                    Debug.Log("[Portal] Fallback: BranchSelectionPresenter 직접 호출");
                }
                else
                {
                    Debug.LogWarning("[Portal] BranchSelectionPresenter를 찾을 수 없습니다!");
                }
            }
        }

        /// <summary>
        /// 노드 선택 처리 (분기 선택 UI에서 호출)
        /// </summary>
        public void SelectNode(DungeonNode node)
        {
            if (node == null)
            {
                Debug.LogWarning("[Portal] 선택된 노드가 null입니다!");
                return;
            }

            Debug.Log($"[Portal] 노드 선택됨: {node.nodeId} ({node.roomType})");

            // 이벤트 발생
            OnNodeSelected?.Invoke(node);

            // RoomManager를 통해 이동
            MoveToNodeAsync(node).Forget();
        }

        /// <summary>
        /// 노드로 이동 (비동기)
        /// </summary>
        private async Awaitable MoveToNodeAsync(DungeonNode node)
        {
            // 포탈 비활성화
            SetActive(false);

            // 페이드 효과 대기
            await Awaitable.WaitForSecondsAsync(0.3f);

            // RoomManager를 통해 이동
            if (RoomManager.Instance != null)
            {
                await RoomManager.Instance.MoveToNodeAsync(node);
            }
            else
            {
                Debug.LogError("[Portal] RoomManager를 찾을 수 없습니다!");
            }
        }

        /// <summary>
        /// 분기 선택이 필요한지 확인
        /// </summary>
        public bool NeedsBranchSelection()
        {
            return portalType == PortalType.BranchSelection && connectedNodes.Count > 1;
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
        NextRoom,       // 던전 내 다음 방으로 (GameFlow 사용)
        SpecificRoom,   // 특정 방으로 (직접 이동)
        RandomRoom,     // 랜덤 방으로 (직접 이동)
        DungeonEntrance, // StartRoom → Dungeon 입장 (GameFlow 사용)
        BranchSelection  // 분기 선택 (그래프 기반 - 다중 목적지)
    }
}
