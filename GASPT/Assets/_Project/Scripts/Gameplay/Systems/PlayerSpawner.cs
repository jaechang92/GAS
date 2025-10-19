using UnityEngine;
using Core.Managers;
using Player;
using Combat.Core;

namespace Gameplay.Systems
{
    /// <summary>
    /// 플레이어 동적 생성 시스템
    /// StartPosition에서 Player Prefab 인스턴스화 및 GameDataManager 데이터 적용
    /// </summary>
    public class PlayerSpawner : MonoBehaviour
    {
        [Header("=== 필수 참조 ===")]
        [Tooltip("생성할 플레이어 Prefab")]
        [SerializeField] private GameObject playerPrefab;

        [Tooltip("플레이어 생성 위치 (없으면 이 오브젝트 위치)")]
        [SerializeField] private Transform startPosition;

        [Header("=== 디버그 ===")]
        [Tooltip("디버그 로그 출력")]
        [SerializeField] private bool debugLog = true;

        /// <summary>
        /// 생성된 플레이어 인스턴스
        /// </summary>
        private GameObject spawnedPlayer;

        /// <summary>
        /// 생성된 플레이어 컨트롤러
        /// </summary>
        public PlayerController PlayerController { get; private set; }

        private void Start()
        {
            // 자동으로 플레이어 생성
            SpawnPlayer();
        }

        /// <summary>
        /// 플레이어 생성 및 데이터 적용
        /// </summary>
        public void SpawnPlayer()
        {
            if (playerPrefab == null)
            {
                Debug.LogError("[PlayerSpawner] playerPrefab이 할당되지 않았습니다!");
                return;
            }

            // 기존 플레이어 제거
            if (spawnedPlayer != null)
            {
                Destroy(spawnedPlayer);
                LogDebug("기존 플레이어 제거");
            }

            // 생성 위치 결정
            Vector3 spawnPos = startPosition != null ? startPosition.position : transform.position;
            Quaternion spawnRot = startPosition != null ? startPosition.rotation : transform.rotation;

            // 플레이어 인스턴스화
            spawnedPlayer = Instantiate(playerPrefab, spawnPos, spawnRot);
            spawnedPlayer.name = "Player"; // 이름 정리

            LogDebug($"플레이어 생성 완료: 위치 {spawnPos}");

            // PlayerController 참조
            PlayerController = spawnedPlayer.GetComponent<PlayerController>();
            if (PlayerController == null)
            {
                Debug.LogError("[PlayerSpawner] 생성된 플레이어에 PlayerController가 없습니다!");
                return;
            }

            // GameDataManager에서 데이터 적용
            ApplyPlayerData();
        }

        /// <summary>
        /// GameDataManager의 데이터를 플레이어에 적용
        /// </summary>
        private void ApplyPlayerData()
        {
            if (PlayerController == null)
            {
                Debug.LogError("[PlayerSpawner] PlayerController가 null입니다!");
                return;
            }

            // GameDataManager 참조
            var dataManager = GameDataManager.Instance;
            if (dataManager == null)
            {
                Debug.LogError("[PlayerSpawner] GameDataManager를 찾을 수 없습니다!");
                return;
            }

            var playerData = dataManager.PlayerData;

            // HealthSystem에 HP/MP 적용
            var healthSystem = PlayerController.HealthSystem;
            if (healthSystem != null)
            {
                healthSystem.SetMaxHealth(playerData.maxHP);
                healthSystem.SetCurrentHealth(playerData.currentHP);
                LogDebug($"체력 적용: {playerData.currentHP}/{playerData.maxHP}");

                // TODO: MP 시스템 구현 시 추가
                // healthSystem.SetMaxMana(playerData.maxMP);
                // healthSystem.SetMana(playerData.currentMP);
            }
            else
            {
                Debug.LogWarning("[PlayerSpawner] PlayerController에 HealthSystem이 없습니다.");
            }

            // TODO: 스텟 적용 (공격력, 방어력, 이동속도 등)
            // 현재 PlayerController에 스텟 적용 인터페이스가 없으므로 나중에 확장
            // PlayerController에 SetStats(PlayerRuntimeData) 메서드 추가 필요

            // TODO: 스킬/어빌리티 적용
            // playerData.equippedAbilityIds를 기반으로 AbilitySystem에 어빌리티 추가
            // 현재는 ComboAbilityData가 Inspector에서 직접 할당되므로 나중에 동적 로드로 변경

            LogDebug("플레이어 데이터 적용 완료");
        }

        /// <summary>
        /// 플레이어 제거
        /// </summary>
        public void DespawnPlayer()
        {
            if (spawnedPlayer != null)
            {
                Destroy(spawnedPlayer);
                spawnedPlayer = null;
                PlayerController = null;

                LogDebug("플레이어 제거 완료");
            }
        }

        /// <summary>
        /// 현재 플레이어 데이터를 GameDataManager에 저장
        /// </summary>
        public void SavePlayerData()
        {
            if (PlayerController == null)
            {
                Debug.LogWarning("[PlayerSpawner] 저장할 플레이어가 없습니다.");
                return;
            }

            var dataManager = GameDataManager.Instance;
            if (dataManager == null)
            {
                Debug.LogError("[PlayerSpawner] GameDataManager를 찾을 수 없습니다!");
                return;
            }

            var playerData = dataManager.PlayerData;

            // HealthSystem에서 HP/MP 가져오기
            var healthSystem = PlayerController.HealthSystem;
            if (healthSystem != null)
            {
                playerData.currentHP = healthSystem.CurrentHealth;
                playerData.maxHP = healthSystem.MaxHealth;
                LogDebug($"플레이어 데이터 저장: HP {playerData.currentHP}/{playerData.maxHP}");

                // TODO: MP 시스템 구현 시 추가
                // playerData.currentMP = healthSystem.CurrentMana;
                // playerData.maxMP = healthSystem.MaxMana;
            }

            // TODO: 스텟, 스킬, 아이템 등 저장

            LogDebug("플레이어 데이터 저장 완료");
        }

        /// <summary>
        /// 디버그 로그
        /// </summary>
        private void LogDebug(string message)
        {
            if (debugLog)
            {
                Debug.Log($"[PlayerSpawner] {message}");
            }
        }

        private void OnDestroy()
        {
            // 씬 전환 전 데이터 저장
            SavePlayerData();
        }

#if UNITY_EDITOR
        /// <summary>
        /// 에디터 Gizmo 표시 (StartPosition)
        /// </summary>
        private void OnDrawGizmos()
        {
            Vector3 gizmoPos = startPosition != null ? startPosition.position : transform.position;

            // 파란색 구 표시
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(gizmoPos, 0.5f);

            // 위쪽 화살표
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(gizmoPos, gizmoPos + Vector3.up * 1f);
        }

        /// <summary>
        /// 씬 뷰 라벨 표시
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            Vector3 labelPos = startPosition != null ? startPosition.position : transform.position;
            labelPos += Vector3.up * 1.5f;

            UnityEditor.Handles.Label(labelPos, "Player Spawn Point");
        }
#endif
    }
}
