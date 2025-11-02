using UnityEngine;
using GASPT.Skills;
using GASPT.Stats;
using GASPT.Enemies;

namespace GASPT.Testing
{
    /// <summary>
    /// SkillSystem 테스트 스크립트
    /// Context Menu를 통해 다양한 테스트 실행
    /// </summary>
    public class SkillSystemTest : MonoBehaviour
    {
        [Header("테스트 설정")]
        [SerializeField] [Tooltip("테스트용 스킬 데이터 3개")]
        private SkillData testSkill1; // Damage 타입 (예: Fireball)

        [SerializeField]
        private SkillData testSkill2; // Heal 타입

        [SerializeField]
        private SkillData testSkill3; // Buff 타입

        [SerializeField] [Tooltip("테스트 대상 Enemy")]
        private GameObject testEnemy;

        [SerializeField] [Tooltip("플레이어 GameObject (자동 탐색 가능)")]
        private GameObject player;

        [SerializeField] [Tooltip("테스트 Enemy 자동 생성 여부")]
        private bool autoCreateEnemy = true;


        // ====== Unity 생명주기 ======

        private void Awake()
        {
            // 플레이어 자동 탐색
            if (player == null)
            {
                player = GameObject.FindWithTag("Player");
                if (player == null)
                {
                    Debug.LogWarning("[SkillSystemTest] Player 태그를 가진 GameObject를 찾을 수 없습니다.");
                }
            }

            // 테스트 Enemy 자동 생성
            if (autoCreateEnemy && testEnemy == null)
            {
                CreateTestEnemy();
            }
        }


        // ====== 자동 테스트 시퀀스 ======

        /// <summary>
        /// 전체 테스트 자동 실행 (권장)
        /// </summary>
        [ContextMenu("Run All Tests")]
        private void RunAllTests()
        {
            Debug.Log("========== SkillSystem 전체 테스트 시작 ==========");

            // 1. 초기 상태 확인
            Test01_CheckInitialState();

            // 2. 스킬 등록
            Test02_RegisterSkills();

            // 3. 마나 확인
            Test03_CheckMana();

            // 4. Damage 스킬 테스트
            Test04_TestDamageSkill();

            // 5. Heal 스킬 테스트
            Test05_TestHealSkill();

            // 6. Buff 스킬 테스트
            Test06_TestBuffSkill();

            // 7. 쿨다운 테스트
            Test07_TestCooldown();

            // 8. 마나 부족 테스트
            Test08_TestOutOfMana();

            Debug.Log("========== SkillSystem 전체 테스트 완료 ==========");
        }


        // ====== 개별 테스트 ======

        [ContextMenu("01. Check Initial State")]
        private void Test01_CheckInitialState()
        {
            Debug.Log("========== Test 01: 초기 상태 확인 ==========");

            if (player == null)
            {
                Debug.LogError("❌ Player가 null입니다.");
                return;
            }

            PlayerStats playerStats = player.GetComponent<PlayerStats>();
            if (playerStats == null)
            {
                Debug.LogError("❌ PlayerStats를 찾을 수 없습니다.");
                return;
            }

            Debug.Log($"✅ Player: {player.name}");
            Debug.Log($"✅ PlayerStats: HP {playerStats.CurrentHP}/{playerStats.MaxHP}, Mana {playerStats.CurrentMana}/{playerStats.MaxMana}");

            if (SkillSystem.HasInstance)
            {
                Debug.Log($"✅ SkillSystem: 초기화됨");
            }
            else
            {
                Debug.LogWarning("⚠️ SkillSystem이 초기화되지 않았습니다. (SingletonPreloader 실행 필요)");
            }

            Debug.Log("=======================================");
        }

        [ContextMenu("02. Register Skills")]
        private void Test02_RegisterSkills()
        {
            Debug.Log("========== Test 02: 스킬 등록 ==========");

            if (testSkill1 == null || testSkill2 == null || testSkill3 == null)
            {
                Debug.LogError("❌ 테스트 스킬 데이터가 설정되지 않았습니다.");
                return;
            }

            SkillSystem skillSystem = SkillSystem.Instance;
            if (skillSystem == null)
            {
                Debug.LogError("❌ SkillSystem을 찾을 수 없습니다.");
                return;
            }

            // 슬롯 0, 1, 2에 스킬 등록
            bool result1 = skillSystem.RegisterSkill(0, testSkill1);
            bool result2 = skillSystem.RegisterSkill(1, testSkill2);
            bool result3 = skillSystem.RegisterSkill(2, testSkill3);

            if (result1 && result2 && result3)
            {
                Debug.Log($"✅ 스킬 등록 성공:");
                Debug.Log($"  - 슬롯 0: {testSkill1.skillName}");
                Debug.Log($"  - 슬롯 1: {testSkill2.skillName}");
                Debug.Log($"  - 슬롯 2: {testSkill3.skillName}");
            }
            else
            {
                Debug.LogError("❌ 스킬 등록 실패");
            }

            Debug.Log("=======================================");
        }

        [ContextMenu("03. Check Mana")]
        private void Test03_CheckMana()
        {
            Debug.Log("========== Test 03: 마나 확인 ==========");

            PlayerStats playerStats = player.GetComponent<PlayerStats>();
            if (playerStats == null)
            {
                Debug.LogError("❌ PlayerStats를 찾을 수 없습니다.");
                return;
            }

            Debug.Log($"현재 마나: {playerStats.CurrentMana}/{playerStats.MaxMana}");

            // 마나 소비 테스트
            Debug.Log("마나 20 소비 테스트...");
            bool spendResult = playerStats.TrySpendMana(20);
            Debug.Log($"소비 결과: {(spendResult ? "✅ 성공" : "❌ 실패")}");
            Debug.Log($"현재 마나: {playerStats.CurrentMana}/{playerStats.MaxMana}");

            // 마나 회복 테스트
            Debug.Log("마나 30 회복 테스트...");
            playerStats.RegenerateMana(30);
            Debug.Log($"현재 마나: {playerStats.CurrentMana}/{playerStats.MaxMana}");

            Debug.Log("=======================================");
        }

        [ContextMenu("04. Test Damage Skill (Slot 0)")]
        private void Test04_TestDamageSkill()
        {
            Debug.Log("========== Test 04: Damage 스킬 테스트 ==========");

            if (testEnemy == null)
            {
                Debug.LogError("❌ testEnemy가 null입니다. Enemy를 생성하세요.");
                return;
            }

            SkillSystem skillSystem = SkillSystem.Instance;
            PlayerStats playerStats = player.GetComponent<PlayerStats>();

            Debug.Log($"사용 전 마나: {playerStats.CurrentMana}/{playerStats.MaxMana}");

            Enemy enemy = testEnemy.GetComponent<Enemy>();
            if (enemy != null)
            {
                Debug.Log($"사용 전 적 HP: {enemy.CurrentHP}/{enemy.MaxHP}");
            }

            // 슬롯 0 스킬 사용 (Damage)
            bool result = skillSystem.TryUseSkill(0, testEnemy);

            if (result)
            {
                Debug.Log($"✅ 스킬 사용 성공!");
                Debug.Log($"사용 후 마나: {playerStats.CurrentMana}/{playerStats.MaxMana}");
                if (enemy != null)
                {
                    Debug.Log($"사용 후 적 HP: {enemy.CurrentHP}/{enemy.MaxHP}");
                }
            }
            else
            {
                Debug.LogError($"❌ 스킬 사용 실패");
            }

            Debug.Log("=======================================");
        }

        [ContextMenu("05. Test Heal Skill (Slot 1)")]
        private void Test05_TestHealSkill()
        {
            Debug.Log("========== Test 05: Heal 스킬 테스트 ==========");

            SkillSystem skillSystem = SkillSystem.Instance;
            PlayerStats playerStats = player.GetComponent<PlayerStats>();

            // 테스트를 위해 HP를 약간 감소
            playerStats.TakeDamage(20);

            Debug.Log($"사용 전 HP: {playerStats.CurrentHP}/{playerStats.MaxHP}");
            Debug.Log($"사용 전 마나: {playerStats.CurrentMana}/{playerStats.MaxMana}");

            // 슬롯 1 스킬 사용 (Heal)
            bool result = skillSystem.TryUseSkill(1);

            if (result)
            {
                Debug.Log($"✅ 스킬 사용 성공!");
                Debug.Log($"사용 후 HP: {playerStats.CurrentHP}/{playerStats.MaxHP}");
                Debug.Log($"사용 후 마나: {playerStats.CurrentMana}/{playerStats.MaxMana}");
            }
            else
            {
                Debug.LogError($"❌ 스킬 사용 실패");
            }

            Debug.Log("=======================================");
        }

        [ContextMenu("06. Test Buff Skill (Slot 2)")]
        private void Test06_TestBuffSkill()
        {
            Debug.Log("========== Test 06: Buff 스킬 테스트 ==========");

            SkillSystem skillSystem = SkillSystem.Instance;
            PlayerStats playerStats = player.GetComponent<PlayerStats>();

            Debug.Log($"사용 전 Attack: {playerStats.Attack}");
            Debug.Log($"사용 전 마나: {playerStats.CurrentMana}/{playerStats.MaxMana}");

            // 슬롯 2 스킬 사용 (Buff)
            bool result = skillSystem.TryUseSkill(2);

            if (result)
            {
                Debug.Log($"✅ 스킬 사용 성공!");
                Debug.Log($"사용 후 Attack: {playerStats.Attack} (버프 적용됨)");
                Debug.Log($"사용 후 마나: {playerStats.CurrentMana}/{playerStats.MaxMana}");
            }
            else
            {
                Debug.LogError($"❌ 스킬 사용 실패");
            }

            Debug.Log("=======================================");
        }

        [ContextMenu("07. Test Cooldown")]
        private void Test07_TestCooldown()
        {
            Debug.Log("========== Test 07: 쿨다운 테스트 ==========");

            SkillSystem skillSystem = SkillSystem.Instance;

            Debug.Log("스킬 사용 (첫 번째)...");
            bool firstUse = skillSystem.TryUseSkill(0, testEnemy);
            Debug.Log($"첫 번째 사용: {(firstUse ? "✅ 성공" : "❌ 실패")}");

            Debug.Log("즉시 다시 사용 시도 (쿨다운 중)...");
            bool secondUse = skillSystem.TryUseSkill(0, testEnemy);
            Debug.Log($"두 번째 사용: {(secondUse ? "✅ 성공 (예상 밖)" : "❌ 실패 (쿨다운 중, 정상)")}");

            Skill skill = skillSystem.GetSkill(0);
            if (skill != null)
            {
                Debug.Log($"쿨다운 상태: {skill.IsOnCooldown}");
                Debug.Log($"남은 시간: {skill.RemainingCooldown:F1}초");
                Debug.Log($"쿨다운 진행도: {skill.GetCooldownRatio():P0}");
            }

            Debug.Log("=======================================");
        }

        [ContextMenu("08. Test Out Of Mana")]
        private void Test08_TestOutOfMana()
        {
            Debug.Log("========== Test 08: 마나 부족 테스트 ==========");

            PlayerStats playerStats = player.GetComponent<PlayerStats>();
            SkillSystem skillSystem = SkillSystem.Instance;

            // 현재 마나 확인
            Debug.Log($"현재 마나: {playerStats.CurrentMana}/{playerStats.MaxMana}");

            // 마나를 거의 소진
            int manaToSpend = playerStats.CurrentMana - 5; // 5만 남김
            if (manaToSpend > 0)
            {
                playerStats.TrySpendMana(manaToSpend);
                Debug.Log($"마나 {manaToSpend} 소비 → 현재 마나: {playerStats.CurrentMana}");
            }

            // 마나 부족 상태에서 스킬 사용 시도
            Skill skill = skillSystem.GetSkill(0);
            if (skill != null)
            {
                Debug.Log($"스킬 마나 비용: {skill.Data.manaCost}");
                Debug.Log($"스킬 사용 시도 (마나 부족 예상)...");

                bool result = skillSystem.TryUseSkill(0, testEnemy);
                Debug.Log($"사용 결과: {(result ? "✅ 성공 (예상 밖)" : "❌ 실패 (마나 부족, 정상)")}");
            }

            // 마나 복구
            Debug.Log("마나 전체 회복...");
            playerStats.RegenerateMana(playerStats.MaxMana);
            Debug.Log($"현재 마나: {playerStats.CurrentMana}/{playerStats.MaxMana}");

            Debug.Log("=======================================");
        }


        // ====== 유틸리티 ======

        /// <summary>
        /// 테스트용 Enemy 생성
        /// </summary>
        [ContextMenu("Create Test Enemy")]
        private void CreateTestEnemy()
        {
            // 기존 테스트 Enemy 제거
            if (testEnemy != null)
            {
                DestroyImmediate(testEnemy);
            }

            // 새 GameObject 생성
            testEnemy = new GameObject("TestEnemy");
            testEnemy.transform.position = player != null ? player.transform.position + Vector3.right * 2f : Vector3.right * 2f;

            // Enemy 컴포넌트 필요 (ScriptableObject 없이는 수동으로 추가 불가능)
            // 실제로는 Hierarchy에서 EnemyData를 가진 Enemy Prefab을 Instantiate하거나
            // Inspector에서 직접 설정해야 합니다.

            Debug.Log($"[SkillSystemTest] TestEnemy 생성됨: {testEnemy.name} (Enemy 컴포넌트는 수동으로 추가 필요)");
        }

        /// <summary>
        /// 플레이어 상태 출력
        /// </summary>
        [ContextMenu("Print Player Stats")]
        private void PrintPlayerStats()
        {
            if (player == null)
            {
                Debug.LogError("Player가 null입니다.");
                return;
            }

            PlayerStats playerStats = player.GetComponent<PlayerStats>();
            if (playerStats == null)
            {
                Debug.LogError("PlayerStats를 찾을 수 없습니다.");
                return;
            }

            Debug.Log("========== Player Stats ==========");
            Debug.Log($"HP: {playerStats.CurrentHP}/{playerStats.MaxHP}");
            Debug.Log($"Mana: {playerStats.CurrentMana}/{playerStats.MaxMana}");
            Debug.Log($"Attack: {playerStats.Attack}");
            Debug.Log($"Defense: {playerStats.Defense}");
            Debug.Log($"IsDead: {playerStats.IsDead}");
            Debug.Log("==================================");
        }

        /// <summary>
        /// 스킬 슬롯 상태 출력
        /// </summary>
        [ContextMenu("Print Skill Slots")]
        private void PrintSkillSlots()
        {
            if (!SkillSystem.HasInstance)
            {
                Debug.LogError("SkillSystem이 초기화되지 않았습니다.");
                return;
            }

            SkillSystem.Instance.DebugPrintSkillSlots();
        }
    }
}
