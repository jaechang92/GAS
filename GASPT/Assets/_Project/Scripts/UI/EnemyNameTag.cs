using UnityEngine;
using TMPro;
using GASPT.Gameplay.Enemies;
using GASPT.CameraSystem;

namespace GASPT.UI
{
    /// <summary>
    /// 적 이름표 UI (Named 적 전용)
    /// 적 위에 이름을 표시
    /// </summary>
    public class EnemyNameTag : MonoBehaviour
    {
        // ====== UI 요소 ======

        [Header("UI 요소")]
        [SerializeField] [Tooltip("적 이름 텍스트")]
        private TextMeshProUGUI nameText;

        [SerializeField] [Tooltip("배경 이미지 (선택 사항)")]
        private UnityEngine.UI.Image backgroundImage;


        // ====== 설정 ======

        [Header("설정")]
        [SerializeField] [Tooltip("카메라를 향해 회전 (빌보드)")]
        private bool faceCamera = true;

        [SerializeField] [Tooltip("적 위 오프셋 (Y 좌표)")]
        private float verticalOffset = 1.5f;


        // ====== Enemy 참조 ======

        private Enemy targetEnemy;
        private Camera mainCamera;


        // ====== Unity 생명주기 ======

        private void Awake()
        {
            mainCamera = CameraManager.Instance?.MainCamera;

            if (mainCamera == null)
            {
                Debug.LogWarning("[EnemyNameTag] Main Camera를 찾을 수 없습니다. (CameraManager 초기화 필요)");
            }

            ValidateReferences();
        }

        private void LateUpdate()
        {
            // 빌보드 효과 (카메라를 항상 향함)
            if (faceCamera && mainCamera != null)
            {
                transform.rotation = mainCamera.transform.rotation;
            }

            // 적 위치 추적
            if (targetEnemy != null)
            {
                Vector3 targetPosition = targetEnemy.transform.position;
                targetPosition.y += verticalOffset;
                transform.position = targetPosition;
            }
        }


        // ====== 초기화 ======

        /// <summary>
        /// 이름표 초기화
        /// </summary>
        /// <param name="enemy">대상 적</param>
        public void Initialize(Enemy enemy)
        {
            if (enemy == null)
            {
                Debug.LogError("[EnemyNameTag] Initialize(): enemy가 null입니다.");
                return;
            }

            targetEnemy = enemy;

            // 이름 텍스트 설정
            if (nameText != null && enemy.Data != null)
            {
                nameText.text = enemy.Data.enemyName;
            }

            // 사망 이벤트 구독
            targetEnemy.OnDeath += OnEnemyDeath;

            Debug.Log($"[EnemyNameTag] {enemy.Data?.enemyName} 이름표 초기화 완료");
        }


        // ====== 이벤트 핸들러 ======

        /// <summary>
        /// 적 사망 시 이름표 제거
        /// </summary>
        private void OnEnemyDeath(Enemy enemy)
        {
            Debug.Log($"[EnemyNameTag] {enemy.Data?.enemyName} 사망으로 이름표 제거");
            Destroy(gameObject);
        }


        // ====== 유효성 검증 ======

        /// <summary>
        /// 필수 참조 유효성 검증
        /// </summary>
        private void ValidateReferences()
        {
            if (nameText == null)
            {
                Debug.LogError("[EnemyNameTag] nameText가 null입니다. Inspector에서 설정하세요.");
            }
        }


        // ====== 정리 ======

        private void OnDestroy()
        {
            // 이벤트 구독 해제
            if (targetEnemy != null)
            {
                targetEnemy.OnDeath -= OnEnemyDeath;
            }
        }
    }
}
