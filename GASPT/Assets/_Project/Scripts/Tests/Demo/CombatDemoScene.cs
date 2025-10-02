using UnityEngine;
using Combat.Core;
using Core.Enums;
using System.Collections.Generic;

namespace Combat.Demo
{
    /// <summary>
    /// Combat 시스템 데모 씬
    /// 플레이어와 적을 생성하고 전투 시스템을 테스트
    /// </summary>
    public class CombatDemoScene : MonoBehaviour
    {
        [Header("생성 설정")]
        [SerializeField] private bool autoSetupOnStart = true;
        [SerializeField] private Vector3 playerSpawnPosition = Vector3.zero;
        [SerializeField] private Vector3 enemySpawnPosition = new Vector3(5f, 0f, 0f);

        [Header("프리팹 (옵션)")]
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private GameObject enemyPrefab;

        [Header("테스트 설정")]
        [SerializeField] private float testDamageAmount = 25f;
        [SerializeField] private DamageType testDamageType = DamageType.Physical;

        // 생성된 오브젝트
        private GameObject player;
        private GameObject enemy;
        private HealthSystem playerHealth;
        private HealthSystem enemyHealth;
        private DamageSystem damageSystem;

        // 테스트 데이터
        private List<string> eventLog = new List<string>();
        private int totalDamageDealt = 0;

        #region Unity 생명주기

        private void Start()
        {
            if (autoSetupOnStart)
            {
                SetupDemoScene();
            }
        }

        private void Update()
        {
            HandleKeyboardInput();
        }

        #endregion

        #region 씬 설정

        /// <summary>
        /// 데모 씬 초기 설정
        /// </summary>
        [ContextMenu("Setup Demo Scene")]
        public void SetupDemoScene()
        {
            LogEvent("=== Combat Demo Scene 설정 시작 ===");

            // DamageSystem 확인/생성
            SetupDamageSystem();

            // 플레이어 생성
            CreatePlayer();

            // 적 생성
            CreateEnemy();

            LogEvent("=== Combat Demo Scene 설정 완료 ===");
            PrintInstructions();
        }

        /// <summary>
        /// DamageSystem 설정
        /// </summary>
        private void SetupDamageSystem()
        {
            damageSystem = FindFirstObjectByType<DamageSystem>();

            if (damageSystem == null)
            {
                var go = new GameObject("DamageSystem");
                damageSystem = go.AddComponent<DamageSystem>();
                LogEvent("[DamageSystem] 생성 완료");
            }
            else
            {
                LogEvent("[DamageSystem] 기존 시스템 사용");
            }
        }

        /// <summary>
        /// 플레이어 생성
        /// </summary>
        private void CreatePlayer()
        {
            if (player != null)
            {
                Destroy(player);
            }

            if (playerPrefab != null)
            {
                player = Instantiate(playerPrefab, playerSpawnPosition, Quaternion.identity);
                player.name = "Player";
            }
            else
            {
                player = CreateCombatEntity("Player", playerSpawnPosition, Color.green);
            }

            playerHealth = player.GetComponent<HealthSystem>();
            if (playerHealth == null)
            {
                playerHealth = player.AddComponent<HealthSystem>();
            }

            // 이벤트 연결
            playerHealth.OnDamaged += (damage) => LogEvent($"[Player] {damage.amount} 데미지 받음 (타입: {damage.damageType})");
            playerHealth.OnHealed += (amount) => LogEvent($"[Player] {amount} 회복");
            playerHealth.OnDeath += () => LogEvent("[Player] 사망!");
            playerHealth.OnHealthChanged += (current, max) => LogEvent($"[Player] 체력: {current}/{max} ({(current/max*100):F1}%)");

            LogEvent($"[Player] 생성 완료 - 위치: {playerSpawnPosition}");
        }

        /// <summary>
        /// 적 생성
        /// </summary>
        private void CreateEnemy()
        {
            if (enemy != null)
            {
                Destroy(enemy);
            }

            if (enemyPrefab != null)
            {
                enemy = Instantiate(enemyPrefab, enemySpawnPosition, Quaternion.identity);
                enemy.name = "Enemy";
            }
            else
            {
                enemy = CreateCombatEntity("Enemy", enemySpawnPosition, Color.red);
            }

            enemyHealth = enemy.GetComponent<HealthSystem>();
            if (enemyHealth == null)
            {
                enemyHealth = enemy.AddComponent<HealthSystem>();
            }

            // 이벤트 연결
            enemyHealth.OnDamaged += (damage) => LogEvent($"[Enemy] {damage.amount} 데미지 받음 (타입: {damage.damageType})");
            enemyHealth.OnHealed += (amount) => LogEvent($"[Enemy] {amount} 회복");
            enemyHealth.OnDeath += () => LogEvent("[Enemy] 사망!");
            enemyHealth.OnHealthChanged += (current, max) => LogEvent($"[Enemy] 체력: {current}/{max} ({(current/max*100):F1}%)");

            LogEvent($"[Enemy] 생성 완료 - 위치: {enemySpawnPosition}");
        }

        /// <summary>
        /// 기본 전투 엔티티 생성
        /// </summary>
        private GameObject CreateCombatEntity(string name, Vector3 position, Color color)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.name = name;
            go.transform.position = position;
            go.transform.localScale = new Vector3(1f, 1.5f, 1f);

            // 시각적 구분을 위한 색상 설정
            var renderer = go.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = color;
            }

            return go;
        }

        #endregion

        #region 키보드 입력

        private void HandleKeyboardInput()
        {
            if (player == null || enemy == null) return;

            // 숫자 1: 플레이어가 적 공격
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                PlayerAttackEnemy();
            }
            // 숫자 2: 적이 플레이어 공격
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                EnemyAttackPlayer();
            }
            // 숫자 3: 플레이어 회복
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                HealPlayer();
            }
            // 숫자 4: 적 회복
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                HealEnemy();
            }
            // 숫자 5: 범위 공격 (플레이어 중심)
            else if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                RadialAttackAroundPlayer();
            }
            // 숫자 6: 크리티컬 공격
            else if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                CriticalAttack();
            }
            // R: 씬 리셋
            else if (Input.GetKeyDown(KeyCode.R))
            {
                ResetScene();
            }
            // H: 도움말 출력
            else if (Input.GetKeyDown(KeyCode.H))
            {
                PrintInstructions();
            }
        }

        #endregion

        #region 전투 액션

        /// <summary>
        /// 플레이어가 적 공격
        /// </summary>
        [ContextMenu("Test: Player Attack Enemy")]
        public void PlayerAttackEnemy()
        {
            if (enemy == null) return;

            LogEvent($"[Action] 플레이어가 적을 공격!");
            bool success = DamageSystem.ApplyDamage(enemy, testDamageAmount, testDamageType, player);

            if (success)
            {
                totalDamageDealt += (int)testDamageAmount;
            }
        }

        /// <summary>
        /// 적이 플레이어 공격
        /// </summary>
        [ContextMenu("Test: Enemy Attack Player")]
        public void EnemyAttackPlayer()
        {
            if (player == null) return;

            LogEvent($"[Action] 적이 플레이어를 공격!");
            DamageSystem.ApplyDamage(player, testDamageAmount, testDamageType, enemy);
        }

        /// <summary>
        /// 플레이어 회복
        /// </summary>
        [ContextMenu("Test: Heal Player")]
        public void HealPlayer()
        {
            if (playerHealth == null) return;

            LogEvent($"[Action] 플레이어 회복!");
            playerHealth.Heal(50f);
        }

        /// <summary>
        /// 적 회복
        /// </summary>
        [ContextMenu("Test: Heal Enemy")]
        public void HealEnemy()
        {
            if (enemyHealth == null) return;

            LogEvent($"[Action] 적 회복!");
            enemyHealth.Heal(50f);
        }

        /// <summary>
        /// 범위 공격
        /// </summary>
        [ContextMenu("Test: Radial Attack")]
        public void RadialAttackAroundPlayer()
        {
            if (player == null) return;

            LogEvent($"[Action] 플레이어 주변 범위 공격!");
            var damage = DamageData.Create(40f, DamageType.Magical, player);
            var hits = DamageSystem.ApplyRadialDamage(player.transform.position, 10f, damage, LayerMask.GetMask("Default"));

            LogEvent($"[Result] {hits.Count}개 타겟 타격");
        }

        /// <summary>
        /// 크리티컬 공격
        /// </summary>
        [ContextMenu("Test: Critical Attack")]
        public void CriticalAttack()
        {
            if (enemy == null) return;

            LogEvent($"[Action] 크리티컬 공격!");
            var damage = DamageData.CreateCritical(testDamageAmount, testDamageType, player, 2.5f);
            DamageSystem.ApplyDamage(enemy, damage);
        }

        /// <summary>
        /// 넉백 공격
        /// </summary>
        [ContextMenu("Test: Knockback Attack")]
        public void KnockbackAttack()
        {
            if (enemy == null) return;

            LogEvent($"[Action] 넉백 공격!");
            Vector2 knockback = Vector2.right * 10f;
            DamageSystem.ApplyDamageWithKnockback(enemy, testDamageAmount, testDamageType, player, knockback);
        }

        #endregion

        #region 씬 관리

        /// <summary>
        /// 씬 리셋
        /// </summary>
        [ContextMenu("Reset Scene")]
        public void ResetScene()
        {
            LogEvent("=== 씬 리셋 ===");
            eventLog.Clear();
            totalDamageDealt = 0;

            if (player != null) Destroy(player);
            if (enemy != null) Destroy(enemy);

            SetupDemoScene();
        }

        #endregion

        #region 로그 및 UI

        private void LogEvent(string message)
        {
            eventLog.Add($"[{Time.time:F2}s] {message}");
            Debug.Log(message);

            // 최근 50개 이벤트만 유지
            if (eventLog.Count > 50)
            {
                eventLog.RemoveAt(0);
            }
        }

        private void PrintInstructions()
        {
            string instructions = @"
=== Combat Demo Scene 조작법 ===

[기본 공격]
1 : 플레이어가 적 공격
2 : 적이 플레이어 공격

[회복]
3 : 플레이어 회복
4 : 적 회복

[특수 공격]
5 : 범위 공격 (플레이어 중심)
6 : 크리티컬 공격

[기타]
R : 씬 리셋
H : 도움말 출력

[Inspector 메뉴]
- 우클릭 > Context Menu로 테스트 가능
";
            Debug.Log(instructions);
            LogEvent("도움말 출력됨 (Console 확인)");
        }

        #endregion

        #region GUI

        private void OnGUI()
        {
            // 좌측 상단 상태 표시
            GUILayout.BeginArea(new Rect(10, 10, 300, 500));

            GUILayout.Label("=== Combat Demo Scene ===", GUI.skin.box);

            if (player != null && playerHealth != null)
            {
                GUILayout.Label($"Player HP: {playerHealth.CurrentHealth:F0}/{playerHealth.MaxHealth:F0}");
            }

            if (enemy != null && enemyHealth != null)
            {
                GUILayout.Label($"Enemy HP: {enemyHealth.CurrentHealth:F0}/{enemyHealth.MaxHealth:F0}");
            }

            GUILayout.Space(10);
            GUILayout.Label($"Total Damage Dealt: {totalDamageDealt}");

            GUILayout.Space(10);

            // 버튼
            if (GUILayout.Button("Player Attack Enemy (1)"))
                PlayerAttackEnemy();

            if (GUILayout.Button("Enemy Attack Player (2)"))
                EnemyAttackPlayer();

            if (GUILayout.Button("Heal Player (3)"))
                HealPlayer();

            if (GUILayout.Button("Heal Enemy (4)"))
                HealPlayer();

            if (GUILayout.Button("Radial Attack (5)"))
                RadialAttackAroundPlayer();

            if (GUILayout.Button("Critical Attack (6)"))
                CriticalAttack();

            GUILayout.Space(10);

            if (GUILayout.Button("Reset Scene (R)"))
                ResetScene();

            GUILayout.EndArea();

            // 우측 하단 이벤트 로그
            GUILayout.BeginArea(new Rect(Screen.width - 410, Screen.height - 310, 400, 300));
            GUILayout.Box("=== Event Log ===");

            GUILayout.BeginScrollView(Vector2.zero, GUILayout.Height(250));
            for (int i = eventLog.Count - 1; i >= Mathf.Max(0, eventLog.Count - 10); i--)
            {
                GUILayout.Label(eventLog[i]);
            }
            GUILayout.EndScrollView();

            GUILayout.EndArea();
        }

        #endregion
    }
}
