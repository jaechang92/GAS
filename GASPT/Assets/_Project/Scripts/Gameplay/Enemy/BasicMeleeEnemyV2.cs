using UnityEngine;
using GASPT.Core.Pooling;
using GASPT.Data;
using GASPT.Gameplay.Enemies.AI;

namespace GASPT.Gameplay.Enemies
{
    /// <summary>
    /// 기본 근접 공격 적 V2
    /// 새로운 AI 시스템 (EnemyAIController) 사용
    /// Enemy를 상속하고 EnemyAIController로 AI 로직 위임
    ///
    /// 사용법:
    /// 1. GameObject에 이 컴포넌트 추가
    /// 2. EnemyData 설정
    /// 3. EnemyAIController가 자동으로 추가됨
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(PooledObject))]
    [RequireComponent(typeof(EnemyAIController))]
    public class BasicMeleeEnemyV2 : Enemy
    {
        // ====== AI 컨트롤러 ======

        private EnemyAIController aiController;


        // ====== 디버그 ======

        [Header("디버그")]
        [SerializeField] private bool showDebugLogs = false;


        // ====== 프로퍼티 ======

        /// <summary>
        /// 현재 AI 상태 이름
        /// </summary>
        public string CurrentStateName => aiController?.CurrentStateName ?? "None";


        // ====== Unity 생명주기 ======

        protected override void Start()
        {
            base.Start();

            InitializeAIController();
        }


        // ====== 초기화 ======

        /// <summary>
        /// AI 컨트롤러 초기화
        /// </summary>
        private void InitializeAIController()
        {
            aiController = GetComponent<EnemyAIController>();

            if (aiController != null)
            {
                // Enemy 연결
                aiController.SetEnemy(this);

                // EnemyData가 없으면 Enemy의 Data 사용
                if (Data != null)
                {
                    aiController.SetEnemyData(Data);
                }

                if (showDebugLogs)
                {
                    Debug.Log($"[BasicMeleeEnemyV2] {Data?.enemyName} AI 컨트롤러 초기화 완료");
                }
            }
            else
            {
                Debug.LogError($"[BasicMeleeEnemyV2] {Data?.enemyName} AI 컨트롤러를 찾을 수 없습니다!");
            }
        }


        // ====== 데이터 설정 ======

        /// <summary>
        /// 외부에서 EnemyData 설정 및 초기화
        /// EnemySpawnPoint 등에서 호출
        /// </summary>
        public new void InitializeWithData(EnemyData data)
        {
            base.InitializeWithData(data);

            // AI 컨트롤러에도 데이터 전달
            if (aiController != null)
            {
                aiController.SetEnemyData(data);
            }
        }


        // ====== 디버그 ======

        [ContextMenu("Print BasicMeleeEnemyV2 Info")]
        private void DebugPrintInfo()
        {
            Debug.Log($"=== BasicMeleeEnemyV2 Info ===\n" +
                     $"Enemy: {Data?.enemyName}\n" +
                     $"HP: {CurrentHp}/{MaxHp}\n" +
                     $"IsDead: {IsDead}\n" +
                     $"AI State: {CurrentStateName}\n" +
                     $"Position: {transform.position}\n" +
                     $"==============================");
        }
    }
}
