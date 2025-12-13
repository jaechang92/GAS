using System;
using UnityEngine;
using GASPT.DTOs;
using GASPT.Stats;
using GASPT.Meta;

namespace GASPT.Core
{
    /// <summary>
    /// 런 데이터 관리자
    /// - 런 중 플레이어 데이터 보관 (DontDestroyOnLoad)
    /// - 씬 전환 시에도 데이터 유지
    /// - Player 생성/파괴 시 데이터 동기화
    /// </summary>
    public class RunManager : SingletonManager<RunManager>
    {
        // ===== 런 데이터 =====
        private PlayerRunData runData;

        /// <summary>
        /// 현재 런 데이터 (읽기 전용)
        /// </summary>
        public PlayerRunData RunData => runData;

        /// <summary>
        /// 런이 활성화 상태인지 여부
        /// </summary>
        public bool IsRunActive { get; private set; }

        /// <summary>
        /// 현재 Player 참조 (씬에 존재할 때만 유효)
        /// </summary>
        public PlayerStats CurrentPlayer { get; private set; }


        // ===== 이벤트 =====

        /// <summary>
        /// 새 런 시작 시 발생
        /// </summary>
        public event Action OnRunStarted;

        /// <summary>
        /// 런 종료 시 발생 (bool: 승리 여부)
        /// </summary>
        public event Action<bool> OnRunEnded;

        /// <summary>
        /// Player 등록 시 발생
        /// </summary>
        public event Action<PlayerStats> OnPlayerRegistered;

        /// <summary>
        /// Player 해제 시 발생
        /// </summary>
        public event Action OnPlayerUnregistered;


        // ===== 런 시작/종료 =====

        /// <summary>
        /// 새 런 시작
        /// </summary>
        public void StartNewRun()
        {
            if (IsRunActive)
            {
                Debug.LogWarning("[RunManager] 이미 런이 진행 중입니다. 기존 런을 종료합니다.");
                EndRun(false);
            }

            // 기본 런 데이터 생성
            runData = PlayerRunData.CreateDefault();

            // 메타 업그레이드 적용
            ApplyMetaUpgrades();

            IsRunActive = true;

            Debug.Log($"[RunManager] 새 런 시작! {runData}");
            OnRunStarted?.Invoke();
        }

        /// <summary>
        /// 메타 업그레이드가 적용된 런 시작
        /// </summary>
        public void StartNewRunWithData(PlayerRunData data)
        {
            if (IsRunActive)
            {
                Debug.LogWarning("[RunManager] 이미 런이 진행 중입니다. 기존 런을 종료합니다.");
                EndRun(false);
            }

            runData = data;
            IsRunActive = true;

            Debug.Log($"[RunManager] 런 시작 (커스텀 데이터)! {runData}");
            OnRunStarted?.Invoke();
        }

        /// <summary>
        /// 런 종료
        /// </summary>
        /// <param name="victory">승리 여부</param>
        public void EndRun(bool victory)
        {
            if (!IsRunActive)
            {
                Debug.LogWarning("[RunManager] 진행 중인 런이 없습니다.");
                return;
            }

            // 현재 Player 데이터 동기화
            if (CurrentPlayer != null)
            {
                SyncFromPlayer(CurrentPlayer);
            }

            Debug.Log($"[RunManager] 런 종료! 승리: {victory}, 최종 골드: {runData?.gold ?? 0}");

            // 메타 진행도에 런 결과 전달
            var meta = MetaProgressionManager.Instance;
            if (meta != null)
            {
                int stageReached = GameManager.Instance?.CurrentStage ?? 0;
                meta.EndRun(victory, stageReached);
                Debug.Log($"[RunManager] 메타 진행도 런 종료 처리 완료 (승리: {victory})");
            }

            OnRunEnded?.Invoke(victory);

            // 런 데이터 초기화
            runData = null;
            CurrentPlayer = null;
            IsRunActive = false;
        }


        // ===== Player 등록/해제 =====

        /// <summary>
        /// Player 등록 (Player 생성 시 호출)
        /// </summary>
        public void RegisterPlayer(PlayerStats player)
        {
            if (player == null)
            {
                Debug.LogWarning("[RunManager] RegisterPlayer: player가 null입니다.");
                return;
            }

            CurrentPlayer = player;

            // 런 데이터가 있으면 Player에 주입
            if (IsRunActive && runData != null)
            {
                SyncToPlayer(player);
                Debug.Log($"[RunManager] Player 등록 완료 + 데이터 주입: {runData}");
            }
            else
            {
                Debug.Log("[RunManager] Player 등록 완료 (런 데이터 없음)");
            }

            // GameManager에도 등록
            if (GameManager.HasInstance)
            {
                GameManager.Instance.RegisterPlayer(player);
            }

            OnPlayerRegistered?.Invoke(player);
        }

        /// <summary>
        /// Player 해제 (Player 파괴 전 호출)
        /// </summary>
        public void UnregisterPlayer(PlayerStats player)
        {
            if (player == null || CurrentPlayer != player)
            {
                return;
            }

            // 런 데이터에 현재 상태 저장
            if (IsRunActive && runData != null)
            {
                SyncFromPlayer(player);
                Debug.Log($"[RunManager] Player 해제 + 데이터 저장: {runData}");
            }

            CurrentPlayer = null;

            // GameManager에서도 해제
            if (GameManager.HasInstance)
            {
                GameManager.Instance.UnregisterPlayer();
            }

            OnPlayerUnregistered?.Invoke();
        }


        // ===== 데이터 동기화 =====

        /// <summary>
        /// Player → RunData 동기화 (Player 파괴 전)
        /// </summary>
        public void SyncFromPlayer(PlayerStats player)
        {
            if (runData == null || player == null) return;

            runData.currentHP = player.CurrentHP;
            runData.maxHP = player.MaxHP;
            runData.currentMana = player.CurrentMana;
            runData.maxMana = player.MaxMana;
            runData.baseAttack = player.BaseAttack;
            runData.baseDefense = player.BaseDefense;

            Debug.Log($"[RunManager] SyncFromPlayer: {runData}");
        }

        /// <summary>
        /// RunData → Player 동기화 (Player 생성 후)
        /// </summary>
        public void SyncToPlayer(PlayerStats player)
        {
            if (runData == null || player == null) return;

            player.InitializeFromRunData(runData);

            Debug.Log($"[RunManager] SyncToPlayer: {runData}");
        }


        // ===== 런 진행 =====

        /// <summary>
        /// 스테이지 진행
        /// </summary>
        public void AdvanceStage()
        {
            if (!IsRunActive || runData == null) return;

            runData.currentStage++;
            runData.clearedRooms++;

            Debug.Log($"[RunManager] 스테이지 진행: {runData.currentStage}");
        }

        /// <summary>
        /// 골드 추가
        /// </summary>
        public void AddGold(int amount)
        {
            if (!IsRunActive || runData == null) return;

            runData.gold += amount;
            Debug.Log($"[RunManager] 골드 추가: +{amount} (총: {runData.gold})");
        }

        /// <summary>
        /// 골드 사용
        /// </summary>
        public bool SpendGold(int amount)
        {
            if (!IsRunActive || runData == null) return false;

            if (runData.gold < amount)
            {
                Debug.LogWarning($"[RunManager] 골드 부족: 필요 {amount}, 보유 {runData.gold}");
                return false;
            }

            runData.gold -= amount;
            Debug.Log($"[RunManager] 골드 사용: -{amount} (남은: {runData.gold})");
            return true;
        }


        // ===== 메타 업그레이드 =====

        /// <summary>
        /// 메타 업그레이드를 런 데이터에 적용
        /// </summary>
        private void ApplyMetaUpgrades()
        {
            if (runData == null) return;

            var meta = MetaProgressionManager.Instance;
            if (meta == null) return;

            // 메타 업그레이드 적용 (예시)
            // runData.maxHP += meta.GetUpgradeLevel("BonusHP") * 10;
            // runData.baseAttack += meta.GetUpgradeLevel("BonusAttack") * 2;

            runData.currentHP = runData.maxHP;
            runData.currentMana = runData.maxMana;

            Debug.Log("[RunManager] 메타 업그레이드 적용 완료");
        }


        // ===== 디버그 =====

        [ContextMenu("디버그: 런 상태 출력")]
        private void DebugLogRunState()
        {
            Debug.Log("========== Run State ==========");
            Debug.Log($"Is Run Active: {IsRunActive}");
            Debug.Log($"Has Player: {CurrentPlayer != null}");
            Debug.Log($"Run Data: {runData?.ToString() ?? "null"}");
            Debug.Log("===============================");
        }

        [ContextMenu("테스트: 새 런 시작")]
        private void TestStartNewRun()
        {
            StartNewRun();
        }

        [ContextMenu("테스트: 런 종료 (승리)")]
        private void TestEndRunVictory()
        {
            EndRun(true);
        }

        [ContextMenu("테스트: 런 종료 (패배)")]
        private void TestEndRunDefeat()
        {
            EndRun(false);
        }
    }
}
