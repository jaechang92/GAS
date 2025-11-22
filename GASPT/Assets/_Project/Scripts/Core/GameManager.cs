using System;
using UnityEngine;
using GASPT.Stats;
using GASPT.Economy;
using GASPT.Save;
using GASPT.Gameplay.Level;
using GASPT.Inventory;
using Random = UnityEngine.Random;

namespace GASPT.Core
{
    /// <summary>
    /// 게임 전체 생명주기 및 참조 허브 관리
    /// 다른 시스템들에 쉽게 접근할 수 있는 중앙 관리자
    ///
    /// 사용 예시:
    /// - GameManager.Instance.Player.CurrentHP
    /// - GameManager.Instance.Currency.AddGold(100)
    /// - GameManager.Instance.Room.CurrentRoom
    /// - GameManager.Instance.GameFlow.TriggerEnterDungeon()
    /// </summary>
    public class GameManager : SingletonManager<GameManager>
    {
        // ====== 참조 허브 ======

        /// <summary>
        /// 플레이어 Stats (FindObject 한 번만 실행)
        /// </summary>
        public PlayerStats PlayerStats { get; private set; }

        /// <summary>
        /// 골드 시스템 (싱글톤 참조)
        /// </summary>
        public CurrencySystem Currency => CurrencySystem.Instance;

        /// <summary>
        /// 인벤토리 시스템 (싱글톤 참조)
        /// </summary>
        public InventorySystem Inventory => InventorySystem.Instance;

        /// <summary>
        /// 방 관리자 (싱글톤 참조)
        /// </summary>
        public RoomManager Room => RoomManager.Instance;

        /// <summary>
        /// 저장 시스템 (싱글톤 참조)
        /// </summary>
        public SaveSystem Save => SaveSystem.Instance;

        /// <summary>
        /// 메타 진행도 관리 (GameManager 하위 컴포넌트)
        /// </summary>
        public MetaProgressionManager Meta { get; private set; }

        /// <summary>
        /// 게임 Flow FSM (싱글톤 참조)
        /// </summary>
        public GameFlowStateMachine GameFlow => GameFlowStateMachine.Instance;


        // ====== 게임 상태 (읽기 전용 프로퍼티) ======

        /// <summary>
        /// 현재 런이 진행 중인지 여부
        /// </summary>
        public bool IsInRun => Room?.CurrentRoom != null;

        /// <summary>
        /// 현재 스테이지 (RoomManager 기반)
        /// </summary>
        public int CurrentStage => Room != null ? Room.CurrentRoomIndex + 1 : 0;

        /// <summary>
        /// 현재 런 골드 (CurrencySystem 기반)
        /// </summary>
        public int CurrentGold => Currency?.Gold ?? 0;

        /// <summary>
        /// 게임이 일시정지 상태인지 여부
        /// </summary>
        public bool IsPaused { get; private set; }


        // ====== 이벤트 ======

        /// <summary>
        /// Player 등록 시 발생 (씬 전환 후 Player 재생성 시)
        /// </summary>
        public event Action<PlayerStats> OnPlayerRegistered;

        /// <summary>
        /// Player 해제 시 발생 (씬 전환 전 Player 파괴 시)
        /// </summary>
        public event Action OnPlayerUnregistered;

        /// <summary>
        /// 게임 일시정지 시 발생
        /// </summary>
        public event Action OnPaused;

        /// <summary>
        /// 게임 재개 시 발생
        /// </summary>
        public event Action OnResumed;


        // ====== 초기화 ======

        protected override void OnAwake()
        {
            // MetaProgressionManager 컴포넌트 추가
            Meta = gameObject.AddComponent<MetaProgressionManager>();

            // 메타 데이터 로드
            Meta.Load();

            Debug.Log("[GameManager] 초기화 완료");
            Debug.Log($"[GameManager] 메타 골드: {Meta.TotalGold}, 언락 Form: {Meta.UnlockedFormCount}개");
        }

        private void Start()
        {
            // Player는 RunManager/PlayerStats에서 등록됨
            // FindPlayerStats() 제거 - RegisterPlayer() 방식으로 변경
        }


        // ====== Player 등록/해제 ======

        /// <summary>
        /// Player 등록 (PlayerStats 또는 RunManager에서 호출)
        /// </summary>
        public void RegisterPlayer(PlayerStats player)
        {
            if (player == null)
            {
                Debug.LogWarning("[GameManager] RegisterPlayer: player가 null입니다.");
                return;
            }

            PlayerStats = player;
            Debug.Log($"[GameManager] Player 등록 완료: HP {PlayerStats.CurrentHP}/{PlayerStats.MaxHP}");

            // 이벤트 발생 - 다른 Manager들이 참조 갱신할 수 있도록
            OnPlayerRegistered?.Invoke(player);
        }

        /// <summary>
        /// Player 해제 (씬 전환/파괴 시)
        /// </summary>
        public void UnregisterPlayer()
        {
            if (PlayerStats != null)
            {
                Debug.Log("[GameManager] Player 해제");
                PlayerStats = null;

                // 이벤트 발생 - 다른 Manager들이 참조 해제할 수 있도록
                OnPlayerUnregistered?.Invoke();
            }
        }


        // ====== 던전 클리어 처리 ======

        /// <summary>
        /// 던전 클리어 시 메타 골드 저장
        /// RoomManager.OnDungeonComplete에서 호출됨
        /// </summary>
        public void OnDungeonCleared()
        {
            int runGold = CurrentGold;

            // 메타 골드로 이전
            if (Meta != null && runGold > 0)
            {
                Meta.AddGold(runGold);
                Debug.Log($"[GameManager] 던전 클리어! 런 골드 {runGold} → 메타 골드 저장 완료");
            }
            else
            {
                Debug.LogWarning("[GameManager] 메타 골드 저장 실패");
            }
        }

        /// <summary>
        /// Form 언락 (보스 처치 등)
        /// </summary>
        /// <param name="formId">언락할 Form ID</param>
        public void UnlockForm(string formId)
        {
            if (Meta != null)
            {
                Meta.UnlockForm(formId);
            }
        }


        // ====== 일시정지/재개 ======

        /// <summary>
        /// 게임 일시정지
        /// </summary>
        public void Pause()
        {
            if (IsPaused)
            {
                Debug.LogWarning("[GameManager] 이미 일시정지 상태입니다.");
                return;
            }

            IsPaused = true;
            Time.timeScale = 0f;
            OnPaused?.Invoke();

            Debug.Log("[GameManager] 게임 일시정지");
        }

        /// <summary>
        /// 게임 재개
        /// </summary>
        public void Resume()
        {
            if (!IsPaused)
            {
                Debug.LogWarning("[GameManager] 일시정지 상태가 아닙니다.");
                return;
            }

            IsPaused = false;
            Time.timeScale = 1f;
            OnResumed?.Invoke();

            Debug.Log("[GameManager] 게임 재개");
        }

        /// <summary>
        /// 일시정지 토글 (ESC 키 등에서 사용)
        /// </summary>
        public void TogglePause()
        {
            if (IsPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }


        // ====== Context Menu (테스트용) ======

        [ContextMenu("디버그: 게임 상태 출력")]
        private void DebugLogGameState()
        {
            Debug.Log("========== Game State ==========");
            Debug.Log($"Is Paused: {IsPaused}");
            Debug.Log($"Is In Run: {IsInRun}");
            Debug.Log($"Current Stage: {CurrentStage}");
            Debug.Log($"Current Gold: {CurrentGold}");
            Debug.Log($"Player HP: {PlayerStats?.CurrentHP}/{PlayerStats?.MaxHP}");
            Debug.Log($"Meta Gold: {Meta?.TotalGold}");
            Debug.Log($"Unlocked Forms: {Meta?.UnlockedFormCount}");
            Debug.Log($"GameFlow State: {GameFlow?.CurrentStateId ?? "None"}");
            Debug.Log($"GameFlow Running: {GameFlow?.IsRunning ?? false}");
            Debug.Log("================================");
        }

        [ContextMenu("테스트: 메타 골드 1000 추가")]
        private void TestAddMetaGold()
        {
            if (Meta != null)
            {
                Meta.AddGold(1000);
            }
        }

        [ContextMenu("테스트: Form 언락 (TestForm)")]
        private void TestUnlockForm()
        {
            if (Meta != null)
            {
                Meta.UnlockForm("TestForm_" + Random.Range(1, 100));
            }
        }

        [ContextMenu("테스트: 게임 일시정지")]
        private void TestPause()
        {
            Pause();
        }

        [ContextMenu("테스트: 게임 재개")]
        private void TestResume()
        {
            Resume();
        }

        [ContextMenu("테스트: 일시정지 토글")]
        private void TestTogglePause()
        {
            TogglePause();
        }
    }
}
