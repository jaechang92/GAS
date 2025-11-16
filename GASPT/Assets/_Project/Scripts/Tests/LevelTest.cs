using UnityEngine;
using GASPT.Stats;
using GASPT.Level;
using GASPT.Gameplay.Enemy;
using GASPT.Data;

namespace GASPT.Tests
{
    /// <summary>
    /// Level & EXP 시스템 통합 테스트
    /// Context Menu를 통해 다양한 시나리오 테스트
    /// </summary>
    public class LevelTest : MonoBehaviour
    {
        [Header("참조")]
        [SerializeField] [Tooltip("테스트용 플레이어 스탯")]
        private PlayerStats playerStats;

        [SerializeField] [Tooltip("테스트용 PlayerLevel")]
        private PlayerLevel playerLevel;

        [SerializeField] [Tooltip("테스트용 적 (EXP 지급 테스트)")]
        private Enemy testEnemy;


        // ====== Unity 생명주기 ======

        private void Awake()
        {
            // 자동 참조 찾기
            if (playerStats == null)
            {
                playerStats = FindAnyObjectByType<PlayerStats>();
            }

            if (playerLevel == null)
            {
                playerLevel = PlayerLevel.Instance;
            }

            if (testEnemy == null)
            {
                testEnemy = FindAnyObjectByType<Enemy>();
            }
        }


        // ====== 테스트 시나리오 ======

        /// <summary>
        /// 시나리오 1: 소량 EXP 획득 (레벨업 안 됨)
        /// </summary>
        [ContextMenu("1. Small EXP Gain (No Level Up)")]
        private void Test1_SmallExpGain()
        {
            Debug.Log("========== Test 1: Small EXP Gain ==========");

            if (playerLevel == null)
            {
                Debug.LogError("[LevelTest] PlayerLevel을 찾을 수 없습니다.");
                return;
            }

            int expToAdd = 30;

            Debug.Log($"[LevelTest] 현재 상태: Level {playerLevel.Level}, EXP {playerLevel.CurrentExp}/{playerLevel.RequiredExp}");
            Debug.Log($"[LevelTest] {expToAdd} EXP 추가...");

            playerLevel.AddExp(expToAdd);

            Debug.Log($"[LevelTest] 결과: Level {playerLevel.Level}, EXP {playerLevel.CurrentExp}/{playerLevel.RequiredExp}");
            Debug.Log("========== Test 1 완료 ==========");
        }

        /// <summary>
        /// 시나리오 2: 레벨업에 필요한 EXP 정확히 획득
        /// </summary>
        [ContextMenu("2. Exact EXP for Level Up")]
        private void Test2_ExactLevelUp()
        {
            Debug.Log("========== Test 2: Exact Level Up ==========");

            if (playerLevel == null)
            {
                Debug.LogError("[LevelTest] PlayerLevel을 찾을 수 없습니다.");
                return;
            }

            int expNeeded = playerLevel.RequiredExp - playerLevel.CurrentExp;

            Debug.Log($"[LevelTest] 현재 상태: Level {playerLevel.Level}, EXP {playerLevel.CurrentExp}/{playerLevel.RequiredExp}");
            Debug.Log($"[LevelTest] 레벨업까지 필요한 EXP: {expNeeded}");
            Debug.Log($"[LevelTest] {expNeeded} EXP 추가...");

            // 레벨업 전 스탯 저장
            int oldHP = playerStats.MaxHP;
            int oldAttack = playerStats.Attack;
            int oldDefense = playerStats.Defense;

            playerLevel.AddExp(expNeeded);

            // 레벨업 후 스탯 확인
            int newHP = playerStats.MaxHP;
            int newAttack = playerStats.Attack;
            int newDefense = playerStats.Defense;

            Debug.Log($"[LevelTest] 결과: Level {playerLevel.Level}, EXP {playerLevel.CurrentExp}/{playerLevel.RequiredExp}");
            Debug.Log($"[LevelTest] 스탯 변화: HP {oldHP} → {newHP}, Attack {oldAttack} → {newAttack}, Defense {oldDefense} → {newDefense}");
            Debug.Log("========== Test 2 완료 ==========");
        }

        /// <summary>
        /// 시나리오 3: 대량 EXP 획득 (여러 레벨업)
        /// </summary>
        [ContextMenu("3. Multiple Level Ups")]
        private void Test3_MultipleLevelUps()
        {
            Debug.Log("========== Test 3: Multiple Level Ups ==========");

            if (playerLevel == null || playerStats == null)
            {
                Debug.LogError("[LevelTest] PlayerLevel 또는 PlayerStats를 찾을 수 없습니다.");
                return;
            }

            int expToAdd = 500; // 대량 EXP

            Debug.Log($"[LevelTest] 현재 상태: Level {playerLevel.Level}, EXP {playerLevel.CurrentExp}/{playerLevel.RequiredExp}");
            Debug.Log($"[LevelTest] {expToAdd} EXP 추가...");

            // 레벨업 전 상태 저장
            int oldLevel = playerLevel.Level;
            int oldHP = playerStats.MaxHP;
            int oldAttack = playerStats.Attack;
            int oldDefense = playerStats.Defense;

            playerLevel.AddExp(expToAdd);

            // 레벨업 후 상태 확인
            int newLevel = playerLevel.Level;
            int levelGained = newLevel - oldLevel;
            int newHP = playerStats.MaxHP;
            int newAttack = playerStats.Attack;
            int newDefense = playerStats.Defense;

            Debug.Log($"[LevelTest] 결과: Level {oldLevel} → {newLevel} (+{levelGained} levels)");
            Debug.Log($"[LevelTest] EXP: {playerLevel.CurrentExp}/{playerLevel.RequiredExp}");
            Debug.Log($"[LevelTest] 스탯 변화: HP {oldHP} → {newHP} (+{newHP - oldHP}), Attack {oldAttack} → {newAttack} (+{newAttack - oldAttack}), Defense {oldDefense} → {newDefense} (+{newDefense - oldDefense})");
            Debug.Log("========== Test 3 완료 ==========");
        }

        /// <summary>
        /// 시나리오 4: Enemy 처치 시 EXP 획득
        /// </summary>
        [ContextMenu("4. Gain EXP from Enemy Death")]
        private void Test4_EnemyExpGain()
        {
            Debug.Log("========== Test 4: Enemy EXP Gain ==========");

            if (testEnemy == null)
            {
                Debug.LogError("[LevelTest] testEnemy가 설정되지 않았습니다. Inspector에서 Enemy를 할당하세요.");
                return;
            }

            if (playerLevel == null)
            {
                Debug.LogError("[LevelTest] PlayerLevel을 찾을 수 없습니다.");
                return;
            }

            Debug.Log($"[LevelTest] 현재 상태: Level {playerLevel.Level}, EXP {playerLevel.CurrentExp}/{playerLevel.RequiredExp}");
            Debug.Log($"[LevelTest] {testEnemy.Data.enemyName} 처치 (EXP 보상: {testEnemy.Data.expReward})...");

            // Enemy 처치 (HP를 0으로 만들어 Die() 호출)
            testEnemy.TakeDamage(testEnemy.CurrentHp);

            Debug.Log($"[LevelTest] 결과: Level {playerLevel.Level}, EXP {playerLevel.CurrentExp}/{playerLevel.RequiredExp}");
            Debug.Log("========== Test 4 완료 ==========");
        }

        /// <summary>
        /// 시나리오 5: 레벨업 시 HP 회복 확인
        /// </summary>
        [ContextMenu("5. HP Recovery on Level Up")]
        private void Test5_HPRecoveryOnLevelUp()
        {
            Debug.Log("========== Test 5: HP Recovery on Level Up ==========");

            if (playerStats == null || playerLevel == null)
            {
                Debug.LogError("[LevelTest] PlayerStats 또는 PlayerLevel을 찾을 수 없습니다.");
                return;
            }

            // 플레이어 HP를 절반으로 감소
            int damageAmount = playerStats.CurrentHP / 2;
            playerStats.TakeDamage(damageAmount);

            Debug.Log($"[LevelTest] 현재 HP: {playerStats.CurrentHP}/{playerStats.MaxHP}");
            Debug.Log($"[LevelTest] 현재 Level: {playerLevel.Level}, EXP: {playerLevel.CurrentExp}/{playerLevel.RequiredExp}");

            // 레벨업에 필요한 EXP 계산 및 추가
            int expNeeded = playerLevel.RequiredExp - playerLevel.CurrentExp;
            Debug.Log($"[LevelTest] {expNeeded} EXP 추가하여 레벨업...");

            playerLevel.AddExp(expNeeded);

            Debug.Log($"[LevelTest] 레벨업 후 HP: {playerStats.CurrentHP}/{playerStats.MaxHP}");
            Debug.Log($"[LevelTest] 레벨업 후 Level: {playerLevel.Level}");

            if (playerStats.CurrentHP == playerStats.MaxHP)
            {
                Debug.Log("[LevelTest] ✓ 레벨업 시 HP가 MaxHP로 회복되었습니다!");
            }
            else
            {
                Debug.LogWarning($"[LevelTest] ✗ HP 회복 실패! 현재 HP: {playerStats.CurrentHP}, MaxHP: {playerStats.MaxHP}");
            }

            Debug.Log("========== Test 5 완료 ==========");
        }

        /// <summary>
        /// 시나리오 6: 최대 레벨 도달 테스트
        /// </summary>
        [ContextMenu("6. Max Level Test")]
        private void Test6_MaxLevel()
        {
            Debug.Log("========== Test 6: Max Level ==========");

            if (playerLevel == null)
            {
                Debug.LogError("[LevelTest] PlayerLevel을 찾을 수 없습니다.");
                return;
            }

            Debug.Log($"[LevelTest] 현재 Level: {playerLevel.Level}");
            Debug.Log($"[LevelTest] 최대 레벨까지 EXP 지급...");

            // 최대 레벨까지 대량 EXP 지급
            int massiveExp = 100000;
            playerLevel.AddExp(massiveExp);

            Debug.Log($"[LevelTest] 결과 Level: {playerLevel.Level}");
            Debug.Log($"[LevelTest] IsMaxLevel: {playerLevel.IsMaxLevel}");
            Debug.Log($"[LevelTest] EXP: {playerLevel.CurrentExp}/{playerLevel.RequiredExp}");

            if (playerLevel.IsMaxLevel)
            {
                Debug.Log("[LevelTest] ✓ 최대 레벨에 도달했습니다!");

                // 최대 레벨에서 EXP 추가 시도
                Debug.Log("[LevelTest] 최대 레벨에서 EXP 추가 시도...");
                playerLevel.AddExp(100);
                Debug.Log($"[LevelTest] 최종 Level: {playerLevel.Level} (변화 없어야 함)");
            }

            Debug.Log("========== Test 6 완료 ==========");
        }


        // ====== 현재 상태 출력 ======

        /// <summary>
        /// 현재 레벨 및 스탯 상태 출력
        /// </summary>
        [ContextMenu("Print Current Status")]
        private void PrintCurrentStatus()
        {
            Debug.Log("========== Current Status ==========");

            if (playerLevel != null)
            {
                Debug.Log($"Level: {playerLevel.Level}");
                Debug.Log($"EXP: {playerLevel.CurrentExp}/{playerLevel.RequiredExp}");
                Debug.Log($"Is Max Level: {playerLevel.IsMaxLevel}");
            }
            else
            {
                Debug.LogWarning("PlayerLevel을 찾을 수 없습니다.");
            }

            if (playerStats != null)
            {
                Debug.Log($"HP: {playerStats.CurrentHP}/{playerStats.MaxHP}");
                Debug.Log($"Attack: {playerStats.Attack}");
                Debug.Log($"Defense: {playerStats.Defense}");
            }
            else
            {
                Debug.LogWarning("PlayerStats를 찾을 수 없습니다.");
            }

            Debug.Log("====================================");
        }
    }
}
