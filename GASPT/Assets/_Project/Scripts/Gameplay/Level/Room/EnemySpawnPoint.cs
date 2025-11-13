using UnityEngine;
using GASPT.Data;
using GASPT.Core.Pooling;
using GASPT.Gameplay.Enemy;

namespace GASPT.Gameplay.Level
{
    /// <summary>
    /// 적 스폰 포인트
    /// 방 안에서 적이 생성될 위치를 표시
    /// 오브젝트 풀링 지원
    /// </summary>
    public class EnemySpawnPoint : MonoBehaviour
    {
        // ====== 스폰 설정 ======

        [Header("스폰 설정")]
        [Tooltip("스폰할 적 데이터 (옵션, Room이 제공하지 않으면 사용)")]
        [SerializeField] private EnemyData enemyData;

        [Tooltip("스폰 시 이펙트 (선택사항)")]
        [SerializeField] private GameObject spawnEffect;

        [Tooltip("스폰 지연 시간 (초)")]
        [Range(0f, 5f)]
        [SerializeField] private float spawnDelay = 0f;


        // ====== 디버그 ======

        [Header("디버그")]
        [SerializeField] private bool showGizmos = true;
        [SerializeField] private Color gizmoColor = Color.red;
        [SerializeField] private float gizmoRadius = 0.5f;


        // ====== 프로퍼티 ======

        public EnemyData EnemyData => enemyData;
        public float SpawnDelay => spawnDelay;


        // ====== 스폰 실행 ======

        /// <summary>
        /// 적 스폰 (EnemyData 제공)
        /// </summary>
        public GameObject SpawnEnemy(EnemyData data)
        {
            if (data == null)
            {
                Debug.LogError($"[EnemySpawnPoint] {name}: EnemyData가 null입니다!");
                return null;
            }

            // BasicMeleeEnemy 프리팹이 필요 (TODO: Resources에서 로드)
            GameObject enemyPrefab = CreateEnemyFromData(data);

            if (enemyPrefab == null)
            {
                Debug.LogError($"[EnemySpawnPoint] {name}: Enemy 생성 실패!");
                return null;
            }

            // 스폰 이펙트 재생
            PlaySpawnEffect();

            Debug.Log($"[EnemySpawnPoint] {data.enemyName} 스폰 at {transform.position}");

            return enemyPrefab;
        }

        /// <summary>
        /// 적 스폰 (기본 EnemyData 사용)
        /// </summary>
        public GameObject SpawnEnemy()
        {
            if (enemyData == null)
            {
                Debug.LogError($"[EnemySpawnPoint] {name}: 기본 EnemyData가 설정되지 않았습니다!");
                return null;
            }

            return SpawnEnemy(enemyData);
        }


        // ====== Enemy 생성 (풀 사용) ======

        /// <summary>
        /// EnemyData로부터 Enemy GameObject 생성 (풀 사용)
        /// </summary>
        private GameObject CreateEnemyFromData(EnemyData data)
        {
            // 풀에서 BasicMeleeEnemy 가져오기
            var enemy = PoolManager.Instance.Spawn<BasicMeleeEnemy>(
                transform.position,
                Quaternion.identity
            );

            if (enemy == null)
            {
                Debug.LogError($"[EnemySpawnPoint] {name}: 풀에서 BasicMeleeEnemy를 가져올 수 없습니다!");
                return null;
            }

            // EnemyData 설정 및 초기화
            enemy.InitializeWithData(data);

            // Sprite 업데이트 (icon이 있으면 교체)
            if (data.icon != null)
            {
                var sr = enemy.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.sprite = data.icon;
                }
            }

            return enemy.gameObject;
        }


        // ====== 이펙트 ======

        /// <summary>
        /// 스폰 이펙트 재생
        /// </summary>
        private void PlaySpawnEffect()
        {
            if (spawnEffect != null)
            {
                GameObject effect = Instantiate(spawnEffect, transform.position, Quaternion.identity);
                Destroy(effect, 2f); // 2초 후 제거
            }
        }


        // ====== Gizmos ======

        private void OnDrawGizmos()
        {
            if (!showGizmos) return;

            Gizmos.color = gizmoColor;
            Gizmos.DrawWireSphere(transform.position, gizmoRadius);

            // 위쪽 화살표
            Gizmos.DrawLine(transform.position, transform.position + Vector3.up * gizmoRadius * 2f);
        }

        private void OnDrawGizmosSelected()
        {
            if (!showGizmos) return;

            // 선택 시 채워진 구
            Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 0.3f);
            Gizmos.DrawSphere(transform.position, gizmoRadius);

            // 적 정보 표시
            if (enemyData != null)
            {
#if UNITY_EDITOR
                UnityEditor.Handles.Label(transform.position + Vector3.up * (gizmoRadius * 2.5f),
                    $"{enemyData.enemyName}\nHP: {enemyData.maxHp}");
#endif
            }
        }


        // ====== 디버그 ======

        [ContextMenu("Test Spawn")]
        private void TestSpawn()
        {
            if (Application.isPlaying)
            {
                SpawnEnemy();
            }
            else
            {
                Debug.LogWarning("[EnemySpawnPoint] Play 모드에서만 스폰 가능합니다.");
            }
        }

        [ContextMenu("Print Info")]
        private void PrintInfo()
        {
            Debug.Log($"[EnemySpawnPoint] {name}\n" +
                     $"Position: {transform.position}\n" +
                     $"EnemyData: {(enemyData != null ? enemyData.enemyName : "None")}\n" +
                     $"Spawn Delay: {spawnDelay}s");
        }
    }
}
