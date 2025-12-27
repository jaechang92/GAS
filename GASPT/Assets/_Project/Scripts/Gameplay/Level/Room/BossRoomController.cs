using System;
using UnityEngine;
using GASPT.Data;
using GASPT.Gameplay.Boss;
using GASPT.UI.Boss;
using GASPT.UI.MVP;

namespace GASPT.Gameplay.Level
{
    /// <summary>
    /// 보스 방 컨트롤러
    /// 보스 방 입장/클리어 처리
    /// </summary>
    public class BossRoomController : MonoBehaviour
    {
        // ====== 설정 ======

        [Header("보스 설정")]
        [SerializeField]
        private BossData bossData;

        [SerializeField]
        private GameObject bossPrefab;

        [SerializeField]
        private Transform bossSpawnPoint;


        [Header("방 설정")]
        [SerializeField]
        private GameObject[] doors;

        [SerializeField]
        private Collider2D roomTrigger;


        // ====== 상태 ======

        private bool isPlayerInRoom = false;
        private bool isBossSpawned = false;
        private bool isCleared = false;
        private BaseBoss currentBoss;


        // ====== 이벤트 ======

        /// <summary>
        /// 보스 방 입장 이벤트
        /// </summary>
        public event Action OnRoomEntered;

        /// <summary>
        /// 보스 방 클리어 이벤트
        /// </summary>
        public event Action OnRoomCleared;

        /// <summary>
        /// 보스 전투 시작 이벤트
        /// </summary>
        public event Action<BaseBoss> OnBossFightStarted;


        // ====== 프로퍼티 ======

        public bool IsCleared => isCleared;
        public BaseBoss CurrentBoss => currentBoss;


        // ====== 트리거 ======

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (isCleared || isBossSpawned) return;

            // 플레이어 확인
            if (other.CompareTag("Player"))
            {
                OnPlayerEnterRoom();
            }
        }


        // ====== 방 입장 ======

        private async void OnPlayerEnterRoom()
        {
            if (isPlayerInRoom) return;

            isPlayerInRoom = true;

            Debug.Log("[BossRoomController] 플레이어 보스 방 입장!");

            // 1. 문 잠금
            LockDoors(true);

            // 2. 이벤트 발생
            OnRoomEntered?.Invoke();

            // 3. 보스 등장 연출
            await SpawnBossWithIntro();
        }


        // ====== 보스 스폰 ======

        private async Awaitable SpawnBossWithIntro()
        {
            if (bossData == null || bossPrefab == null)
            {
                Debug.LogError("[BossRoomController] bossData 또는 bossPrefab이 null입니다.");
                return;
            }

            isBossSpawned = true;

            // 등장 연출 재생
            if (BossIntroController.Instance != null)
            {
                await BossIntroController.Instance.PlayIntro(
                    bossData,
                    bossSpawnPoint,
                    bossPrefab
                );

                currentBoss = BossIntroController.Instance.CurrentBoss;
            }
            else
            {
                // 연출 없이 직접 스폰
                currentBoss = SpawnBossDirect();

                // 체력바 표시
                if (BossHealthBarPresenter.Instance != null)
                {
                    BossHealthBarPresenter.Instance.BindBoss(currentBoss);
                }

                // 전투 시작
                currentBoss.StartCombat();
            }

            // 보스 사망 이벤트 구독
            if (currentBoss != null)
            {
                currentBoss.OnBossDefeated += HandleBossDefeated;
                OnBossFightStarted?.Invoke(currentBoss);
            }
        }

        private BaseBoss SpawnBossDirect()
        {
            Vector3 spawnPos = bossSpawnPoint != null
                ? bossSpawnPoint.position
                : transform.position;

            GameObject bossObj = Instantiate(bossPrefab, spawnPos, Quaternion.identity);
            BaseBoss boss = bossObj.GetComponent<BaseBoss>();

            if (boss != null)
            {
                boss.InitializeWithData(bossData);
            }

            return boss;
        }


        // ====== 보스 처치 ======

        private void HandleBossDefeated(BaseBoss boss)
        {
            Debug.Log("[BossRoomController] 보스 처치 완료!");

            isCleared = true;

            // 이벤트 구독 해제
            boss.OnBossDefeated -= HandleBossDefeated;

            // 문 열기
            LockDoors(false);

            // 클리어 이벤트
            OnRoomCleared?.Invoke();

            // 다음 스테이지 전환 (선택적)
            // StageManager.Instance.OnBossCleared();
        }


        // ====== 문 제어 ======

        private void LockDoors(bool locked)
        {
            if (doors == null) return;

            foreach (var door in doors)
            {
                if (door != null)
                {
                    // 문 활성화/비활성화
                    door.SetActive(locked);

                    // 또는 Collider 활성화/비활성화
                    var col = door.GetComponent<Collider2D>();
                    if (col != null)
                    {
                        col.enabled = locked;
                    }
                }
            }

            Debug.Log($"[BossRoomController] 문 {(locked ? "잠금" : "열림")}");
        }


        // ====== 외부 API ======

        /// <summary>
        /// 보스 데이터 설정 (런타임)
        /// </summary>
        public void SetBossData(BossData data, GameObject prefab)
        {
            bossData = data;
            bossPrefab = prefab;
        }

        /// <summary>
        /// 강제 클리어 (디버그용)
        /// </summary>
        [ContextMenu("강제 클리어")]
        public void ForceClear()
        {
            if (currentBoss != null)
            {
                Destroy(currentBoss.gameObject);
            }

            isCleared = true;
            LockDoors(false);
            OnRoomCleared?.Invoke();

            Debug.Log("[BossRoomController] 강제 클리어됨");
        }


        // ====== 기즈모 ======

        private void OnDrawGizmos()
        {
            // 보스 스폰 위치 표시
            if (bossSpawnPoint != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(bossSpawnPoint.position, 1f);
                Gizmos.DrawIcon(bossSpawnPoint.position, "BossSpawn", true);
            }
        }
    }
}
