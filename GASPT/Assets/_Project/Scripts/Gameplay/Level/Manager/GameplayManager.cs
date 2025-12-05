using UnityEngine;
using GASPT.Core;
using GASPT.ResourceManagement;
using GASPT.UI.MVP;
using GASPT.UI.Minimap;

namespace GASPT.Gameplay.Level
{
    public class GameplayManager : SingletonManager<GameplayManager>
    {
        [Header("던전 설정")]
        [SerializeField] private DungeonConfig currentDungeon;
        [Header("초기화 옵션")]
        [SerializeField] private bool autoInitialize = true;

        private bool isInitialized = false;
        private GameObject playerInstance;
        private GameObject gameplayUIInstance;
        private GameObject minimapUIInstance;
        private GameObject branchSelectionUIInstance;

        public DungeonConfig CurrentDungeon => currentDungeon;
        public bool IsInitialized => isInitialized;
        public GameObject PlayerInstance => playerInstance;

        protected override void Awake() { base.Awake(); }
        private void Start() { if (autoInitialize) Initialize(); }

        public void Initialize()
        {
            if (isInitialized) return;
            if (currentDungeon == null) return;
            CreatePlayer();
            CreateGameplayUI();
            InitializeRoomManager();
            InitializeMinimapAndBranchSelection();
            isInitialized = true;
        }

        private void CreateGameplayUI()
        {
            var uiPrefab = Resources.Load<GameObject>("Prefabs/UI/GameplayUI");
            if (uiPrefab == null) return;
            gameplayUIInstance = Instantiate(uiPrefab);
            gameplayUIInstance.name = "GameplayUI";
        }

        private void CreatePlayer()
        {
            var existingPlayer = FindAnyObjectByType<GASPT.Stats.PlayerStats>();
            if (existingPlayer != null) { playerInstance = existingPlayer.gameObject; return; }
            var playerPrefab = Resources.Load<GameObject>(ResourcePaths.Prefabs.Player.MageForm);
            if (playerPrefab == null) return;
            playerInstance = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
            playerInstance.name = "Player";
            playerInstance.tag = "Player";
        }

        private void InitializeRoomManager()
        {
            var roomManager = RoomManager.Instance;
            if (roomManager == null) return;
            roomManager.LoadDungeon(currentDungeon);
        }

        private void InitializeMinimapAndBranchSelection()
        {
            CreateMinimapUI();
            CreateBranchSelectionUI();
            InitializeMinimapPresenter();
            InitializeBranchSelectionPresenter();
        }

        private void CreateMinimapUI()
        {
            var prefab = Resources.Load<GameObject>("Prefabs/UI/MinimapUI");
            if (prefab == null) return;
            minimapUIInstance = Instantiate(prefab);
            minimapUIInstance.name = "MinimapUI";
        }

        private void CreateBranchSelectionUI()
        {
            var prefab = Resources.Load<GameObject>("Prefabs/UI/BranchSelectionUI");
            if (prefab == null) return;
            branchSelectionUIInstance = Instantiate(prefab);
            branchSelectionUIInstance.name = "BranchSelectionUI";
        }

        private void InitializeMinimapPresenter()
        {
            var presenter = FindAnyObjectByType<MinimapPresenter>();
            if (presenter == null) return;
            var rm = RoomManager.Instance;
            if (rm != null && rm.DungeonGraph != null) presenter.InitializeWithGraph(rm.DungeonGraph);
        }

        private void InitializeBranchSelectionPresenter()
        {
            var presenter = FindAnyObjectByType<BranchSelectionPresenter>();
            if (presenter == null) return;
            presenter.SubscribeToAllPortals();
        }

        public void SetDungeon(DungeonConfig config)
        {
            if (isInitialized) return;
            currentDungeon = config;
        }

        public void Cleanup()
        {
            if (gameplayUIInstance != null) { Destroy(gameplayUIInstance); gameplayUIInstance = null; }
            if (minimapUIInstance != null) { Destroy(minimapUIInstance); minimapUIInstance = null; }
            if (branchSelectionUIInstance != null) { Destroy(branchSelectionUIInstance); branchSelectionUIInstance = null; }
            playerInstance = null;
            isInitialized = false;
        }
    }
}
