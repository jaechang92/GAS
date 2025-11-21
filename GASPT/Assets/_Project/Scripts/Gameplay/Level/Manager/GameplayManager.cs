using UnityEngine;
using GASPT.Core;
using GASPT.ResourceManagement;

namespace GASPT.Gameplay.Level
{
    /// <summary>
    /// GameplayScene 전용 매니저
    /// - GameplayScene 로드 시 UI, Player, RoomManager 초기화
    /// - DungeonConfig 기반으로 던전 구성
    /// </summary>
    public class GameplayManager : SingletonManager<GameplayManager>
    {
        // ====== 설정 ======

        [Header("던전 설정")]
        [Tooltip("현재 던전 설정 (Inspector에서 할당 또는 코드로 설정)")]
        [SerializeField] private DungeonConfig currentDungeon;

        [Header("초기화 옵션")]
        [Tooltip("씬 로드 시 자동으로 초기화")]
        [SerializeField] private bool autoInitialize = true;


        // ====== 상태 ======

        private bool isInitialized = false;
        private GameObject playerInstance;
        private GameObject gameplayUIInstance;


        // ====== 프로퍼티 ======

        public DungeonConfig CurrentDungeon => currentDungeon;
        public bool IsInitialized => isInitialized;
        public GameObject PlayerInstance => playerInstance;


        // ====== 초기화 ======

        protected override void Awake()
        {
            base.Awake();

            // 자동 초기화가 꺼져있으면 Start에서 초기화하지 않음
            // (코드로 SetDungeon() 호출 후 수동 Initialize() 호출 필요)
        }

        private void Start()
        {
            if (autoInitialize)
            {
                Initialize();
            }
        }

        /// <summary>
        /// GameplayScene 초기화
        /// </summary>
        public void Initialize()
        {
            if (isInitialized)
            {
                Debug.LogWarning("[GameplayManager] 이미 초기화되었습니다.");
                return;
            }

            Debug.Log("[GameplayManager] GameplayScene 초기화 시작...");

            // 1. 던전 설정 확인
            if (currentDungeon == null)
            {
                Debug.LogError("[GameplayManager] DungeonConfig가 없습니다! 초기화를 중단합니다.");
                return;
            }

            // 2. Player 먼저 생성 (UI가 PlayerStats를 참조하기 때문)
            CreatePlayer();

            // 3. UI 생성 (Player 생성 후)
            CreateGameplayUI();

            // 4. RoomManager 초기화 (던전 로드)
            InitializeRoomManager();

            isInitialized = true;
            Debug.Log("[GameplayManager] GameplayScene 초기화 완료!");
        }


        // ====== UI 생성 ======

        /// <summary>
        /// GameplayUI 생성
        /// </summary>
        private void CreateGameplayUI()
        {
            Debug.Log("[GameplayManager] GameplayUI 생성 중...");

            // Resources.Load로 UI Prefab 로드
            GameObject uiPrefab = Resources.Load<GameObject>("Prefabs/UI/GameplayUI");

            if (uiPrefab == null)
            {
                Debug.LogWarning("[GameplayManager] GameplayUI Prefab을 찾을 수 없습니다. (경로: Resources/Prefabs/UI/GameplayUI.prefab)");
                return;
            }

            // UI 인스턴스화
            gameplayUIInstance = Instantiate(uiPrefab);
            gameplayUIInstance.name = "GameplayUI";

            // Canvas가 있는지 확인
            Canvas canvas = gameplayUIInstance.GetComponentInChildren<Canvas>();
            if (canvas == null)
            {
                Debug.LogWarning("[GameplayManager] GameplayUI에 Canvas가 없습니다!");
            }

            Debug.Log("[GameplayManager] GameplayUI 생성 완료!");
        }


        // ====== Player 생성 ======

        /// <summary>
        /// Player 생성
        /// </summary>
        private void CreatePlayer()
        {
            Debug.Log("[GameplayManager] Player 생성 중...");

            // 이미 씬에 Player가 있는지 확인
            var existingPlayer = FindAnyObjectByType<GASPT.Stats.PlayerStats>();
            if (existingPlayer != null)
            {
                Debug.Log("[GameplayManager] 씬에 이미 Player가 존재합니다. 생성을 건너뜁니다.");
                playerInstance = existingPlayer.gameObject;
                return;
            }

            // MageForm Prefab 로드
            GameObject playerPrefab = Resources.Load<GameObject>(ResourcePaths.Prefabs.Player.MageForm);

            if (playerPrefab == null)
            {
                Debug.LogError("[GameplayManager] MageForm Prefab을 찾을 수 없습니다!");
                return;
            }

            // Player 인스턴스화
            playerInstance = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
            playerInstance.name = "Player";
            playerInstance.tag = "Player";

            Debug.Log("[GameplayManager] Player 생성 완료!");
        }


        // ====== RoomManager 초기화 ======

        /// <summary>
        /// RoomManager 초기화 (던전 로드)
        /// </summary>
        private void InitializeRoomManager()
        {
            Debug.Log("[GameplayManager] RoomManager 초기화 중...");

            var roomManager = RoomManager.Instance;

            if (roomManager == null)
            {
                Debug.LogError("[GameplayManager] RoomManager를 찾을 수 없습니다!");
                return;
            }

            // RoomManager에 현재 던전 설정 전달
            roomManager.LoadDungeon(currentDungeon);

            Debug.Log($"[GameplayManager] RoomManager 초기화 완료! (던전: {currentDungeon.dungeonName})");
        }


        // ====== 던전 설정 ======

        /// <summary>
        /// 던전 설정 (코드로 설정 시 사용)
        /// </summary>
        public void SetDungeon(DungeonConfig config)
        {
            if (isInitialized)
            {
                Debug.LogWarning("[GameplayManager] 이미 초기화된 상태에서는 던전을 변경할 수 없습니다.");
                return;
            }

            currentDungeon = config;
            Debug.Log($"[GameplayManager] 던전 설정: {config.dungeonName}");
        }


        // ====== 정리 ======

        /// <summary>
        /// GameplayScene 정리 (씬 전환 전 호출)
        /// </summary>
        public void Cleanup()
        {
            Debug.Log("[GameplayManager] GameplayScene 정리 중...");

            // UI 제거
            if (gameplayUIInstance != null)
            {
                Destroy(gameplayUIInstance);
                gameplayUIInstance = null;
            }

            // Player 제거 (씬 전환 시 자동 제거되지만 명시적으로 처리)
            if (playerInstance != null)
            {
                // Player는 DontDestroyOnLoad가 아니므로 씬 전환 시 자동 제거됨
                playerInstance = null;
            }

            isInitialized = false;

            Debug.Log("[GameplayManager] GameplayScene 정리 완료!");
        }


        // ====== 디버그 ======

        [ContextMenu("강제 초기화")]
        private void ForceInitialize()
        {
            isInitialized = false;
            Initialize();
        }

        [ContextMenu("던전 정보 출력")]
        private void PrintDungeonInfo()
        {
            if (currentDungeon == null)
            {
                Debug.Log("[GameplayManager] DungeonConfig가 없습니다.");
                return;
            }

            Debug.Log($"========== Dungeon Info ==========");
            Debug.Log($"Dungeon: {currentDungeon.dungeonName}");
            Debug.Log($"Type: {currentDungeon.generationType}");
            Debug.Log($"Level: {currentDungeon.recommendedLevel}");
            Debug.Log($"Initialized: {isInitialized}");
            Debug.Log($"==================================");
        }
    }
}
