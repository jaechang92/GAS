using UnityEngine;
using Player;
using Combat.Core;
using Combat.Attack;
using Core.Enums;
using System.Collections.Generic;

namespace Combat.Demo
{
    /// <summary>
    /// 플레이어 전투 시스템 데모
    /// Player + Combat 시스템 통합 테스트
    /// </summary>
    public class PlayerCombatDemo : MonoBehaviour
    {
        [Header("플레이어 설정")]
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private Vector3 playerSpawnPosition = Vector3.zero;

        [Header("적 설정")]
        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private Vector3[] enemySpawnPositions = new Vector3[]
        {
            new Vector3(5f, 0f, 0f),
            new Vector3(-5f, 0f, 0f),
            new Vector3(0f, 0f, 5f)
        };

        [Header("콤보 설정")]
        [SerializeField] private bool setupDefaultCombo = true;
        [SerializeField] private int comboCount = 3;

        [Header("UI 설정")]
        [SerializeField] private bool showUI = true;

        // 생성된 오브젝트
        private GameObject player;
        private PlayerController playerController;
        private List<GameObject> enemies = new List<GameObject>();

        // 통계
        private int totalHits = 0;
        private int totalKills = 0;
        private float totalDamageDealt = 0f;
        private List<string> eventLog = new List<string>();

        #region Unity 생명주기

        private void Start()
        {
            SetupDemoScene();
        }

        private void Update()
        {
            HandleKeyboardInput();
        }

        #endregion

        #region 씬 설정

        /// <summary>
        /// 데모 씬 설정
        /// </summary>
        [ContextMenu("Setup Player Combat Demo")]
        public void SetupDemoScene()
        {
            LogEvent("=== Player Combat Demo 설정 시작 ===");

            // DamageSystem 확인
            SetupDamageSystem();

            // 플레이어 생성
            CreatePlayer();

            // 적 생성
            CreateEnemies();

            // 콤보 설정
            if (setupDefaultCombo)
            {
                SetupPlayerCombo();
            }

            LogEvent("=== Player Combat Demo 설정 완료 ===");
            PrintInstructions();
        }

        /// <summary>
        /// DamageSystem 설정
        /// </summary>
        private void SetupDamageSystem()
        {
            var damageSystem = FindFirstObjectByType<DamageSystem>();
            if (damageSystem == null)
            {
                var go = new GameObject("DamageSystem");
                go.AddComponent<DamageSystem>();
                LogEvent("[DamageSystem] 생성 완료");
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
            }
            else
            {
                player = new GameObject("Player");
                player.transform.position = playerSpawnPosition;

                // 기본 컴포넌트 추가
                player.AddComponent<Rigidbody2D>().gravityScale = 3f;
                player.AddComponent<CapsuleCollider2D>();
                player.AddComponent<GAS.Core.AbilitySystem>();
            }

            player.name = "Player";

            // PlayerController 확인/추가
            playerController = player.GetComponent<PlayerController>();
            if (playerController == null)
            {
                playerController = player.AddComponent<PlayerController>();
            }

            // 이벤트 연결
            if (playerController.HealthSystem != null)
            {
                playerController.HealthSystem.OnDamaged += (damage) =>
                {
                    LogEvent($"[Player] {damage.amount} 데미지 받음");
                };

                playerController.HealthSystem.OnDeath += () =>
                {
                    LogEvent("[Player] 사망!");
                };
            }

            if (playerController.ComboSystem != null)
            {
                playerController.ComboSystem.OnComboStarted += (index) =>
                {
                    LogEvent($"[Player] 콤보 시작: {index}");
                };

                playerController.ComboSystem.OnComboAdvanced += (index) =>
                {
                    LogEvent($"[Player] 콤보 진행: {index}");
                };

                playerController.ComboSystem.OnComboCompleted += (index) =>
                {
                    LogEvent($"[Player] 콤보 완료!");
                };
            }

            LogEvent($"[Player] 생성 완료 - 위치: {playerSpawnPosition}");
        }

        /// <summary>
        /// 적 생성
        /// </summary>
        private void CreateEnemies()
        {
            // 기존 적 제거
            foreach (var enemy in enemies)
            {
                if (enemy != null) Destroy(enemy);
            }
            enemies.Clear();

            foreach (var spawnPos in enemySpawnPositions)
            {
                GameObject enemy;

                if (enemyPrefab != null)
                {
                    enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
                }
                else
                {
                    enemy = CreateBasicEnemy(spawnPos);
                }

                enemy.name = $"Enemy_{enemies.Count}";
                enemies.Add(enemy);

                // HealthSystem 이벤트 연결
                var health = enemy.GetComponent<HealthSystem>();
                if (health != null)
                {
                    health.OnDamaged += (damage) =>
                    {
                        totalHits++;
                        totalDamageDealt += damage.amount;
                        LogEvent($"[{enemy.name}] {damage.amount} 데미지");
                    };

                    health.OnDeath += () =>
                    {
                        totalKills++;
                        LogEvent($"[{enemy.name}] 사망!");
                    };
                }
            }

            LogEvent($"[Enemy] {enemies.Count}개 생성 완료");
        }

        /// <summary>
        /// 기본 적 생성
        /// </summary>
        private GameObject CreateBasicEnemy(Vector3 position)
        {
            var enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            enemy.transform.position = position;
            enemy.GetComponent<Renderer>().material.color = Color.red;

            // HealthSystem 추가
            var health = enemy.AddComponent<HealthSystem>();

            // Rigidbody2D 추가 (범위 공격 감지용)
            var rb = enemy.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;

            // Collider 추가
            var col = enemy.AddComponent<CapsuleCollider2D>();

            return enemy;
        }

        /// <summary>
        /// 플레이어 콤보 설정
        /// </summary>
        private void SetupPlayerCombo()
        {
            if (playerController == null || playerController.ComboSystem == null)
            {
                LogEvent("[Combo] PlayerController 또는 ComboSystem 없음");
                return;
            }

            var comboSystem = playerController.ComboSystem;
            comboSystem.ClearCombos();

            for (int i = 0; i < comboCount; i++)
            {
                var comboData = new ComboData
                {
                    comboName = $"Combo_{i + 1}",
                    comboIndex = i,
                    damageMultiplier = 1f + (i * 0.2f), // 1.0, 1.2, 1.4...
                    animationTrigger = $"Attack_{i}",
                    animationSpeed = 1f + (i * 0.1f),
                    hitboxSize = new Vector2(1.5f + i * 0.3f, 1f),
                    hitboxOffset = new Vector2(0.8f, 0f),
                    hitboxDuration = 0.2f,
                    knockbackForce = new Vector2(5f + i * 2f, 2f),
                    inputWindowStart = 0f,
                    inputWindowEnd = 0.5f
                };

                comboSystem.AddCombo(comboData);
            }

            comboSystem.SetComboWindowTime(0.5f);
            comboSystem.SetComboResetTime(1f);

            LogEvent($"[Combo] {comboCount}단 콤보 설정 완료");
        }

        #endregion

        #region 키보드 입력

        private void HandleKeyboardInput()
        {
            // T: 플레이어 체력 회복
            if (Input.GetKeyDown(KeyCode.T))
            {
                HealPlayer();
            }
            // Y: 적 재생성
            else if (Input.GetKeyDown(KeyCode.Y))
            {
                CreateEnemies();
            }
            // U: 콤보 리셋
            else if (Input.GetKeyDown(KeyCode.U))
            {
                ResetCombo();
            }
            // I: 통계 초기화
            else if (Input.GetKeyDown(KeyCode.I))
            {
                ResetStatistics();
            }
            // R: 씬 리셋
            else if (Input.GetKeyDown(KeyCode.R))
            {
                ResetScene();
            }
            // H: 도움말
            else if (Input.GetKeyDown(KeyCode.H))
            {
                PrintInstructions();
            }
        }

        #endregion

        #region 액션

        /// <summary>
        /// 플레이어 회복
        /// </summary>
        [ContextMenu("Test: Heal Player")]
        public void HealPlayer()
        {
            if (playerController?.HealthSystem != null)
            {
                playerController.HealthSystem.Heal(50f);
                LogEvent("[Player] 체력 회복 +50");
            }
        }

        /// <summary>
        /// 콤보 리셋
        /// </summary>
        [ContextMenu("Test: Reset Combo")]
        public void ResetCombo()
        {
            if (playerController?.ComboSystem != null)
            {
                playerController.ComboSystem.ResetCombo();
                LogEvent("[Combo] 리셋됨");
            }
        }

        /// <summary>
        /// 통계 초기화
        /// </summary>
        [ContextMenu("Test: Reset Statistics")]
        public void ResetStatistics()
        {
            totalHits = 0;
            totalKills = 0;
            totalDamageDealt = 0f;
            LogEvent("[Statistics] 초기화됨");
        }

        /// <summary>
        /// 씬 리셋
        /// </summary>
        [ContextMenu("Reset Scene")]
        public void ResetScene()
        {
            eventLog.Clear();
            ResetStatistics();

            if (player != null) Destroy(player);
            foreach (var enemy in enemies)
            {
                if (enemy != null) Destroy(enemy);
            }
            enemies.Clear();

            SetupDemoScene();
        }

        #endregion

        #region 로그 및 UI

        private void LogEvent(string message)
        {
            eventLog.Add($"[{Time.time:F2}s] {message}");
            Debug.Log(message);

            if (eventLog.Count > 50)
            {
                eventLog.RemoveAt(0);
            }
        }

        private void PrintInstructions()
        {
            string instructions = @"
=== Player Combat Demo 조작법 ===

[플레이어 조작]
WASD : 이동
Space : 점프
LShift : 대시
마우스 좌클릭 : 공격 (콤보)

[테스트 기능]
T : 플레이어 체력 회복
Y : 적 재생성
U : 콤보 리셋
I : 통계 초기화
R : 씬 리셋
H : 도움말

[콤보 시스템]
- 공격 버튼을 0.5초 내에 연타하면 콤보 진행
- 최대 3단 콤보
- 콤보마다 데미지와 범위 증가
";
            Debug.Log(instructions);
            LogEvent("도움말 출력됨 (Console 확인)");
        }

        #endregion

        #region GUI

        private void OnGUI()
        {
            if (!showUI) return;

            // 플레이어 정보
            GUILayout.BeginArea(new Rect(10, 10, 400, 300));
            GUILayout.Box("=== Player Combat Demo ===");

            if (playerController != null)
            {
                var health = playerController.HealthSystem;
                if (health != null)
                {
                    GUILayout.Label($"Player HP: {health.CurrentHealth:F0}/{health.MaxHealth:F0}");
                    DrawHealthBar(health.HealthPercentage, Color.green);
                }

                var combo = playerController.ComboSystem;
                if (combo != null)
                {
                    GUILayout.Label($"Combo: {combo.CurrentComboIndex} / {combo.GetComboCount()}");
                    GUILayout.Label($"Combo Active: {combo.IsComboActive}");
                }
            }

            GUILayout.Space(10);

            // 통계
            GUILayout.Label("=== 통계 ===");
            GUILayout.Label($"총 타격: {totalHits}");
            GUILayout.Label($"총 처치: {totalKills}");
            GUILayout.Label($"총 피해: {totalDamageDealt:F1}");

            GUILayout.Space(10);

            // 버튼
            if (GUILayout.Button("체력 회복 (T)"))
                HealPlayer();
            if (GUILayout.Button("적 재생성 (Y)"))
                CreateEnemies();
            if (GUILayout.Button("씬 리셋 (R)"))
                ResetScene();

            GUILayout.EndArea();

            // 이벤트 로그
            GUILayout.BeginArea(new Rect(Screen.width - 510, 10, 500, 400));
            GUILayout.Box("=== Event Log ===");

            GUILayout.BeginScrollView(Vector2.zero, GUILayout.Height(350));
            for (int i = eventLog.Count - 1; i >= Mathf.Max(0, eventLog.Count - 15); i--)
            {
                GUILayout.Label(eventLog[i]);
            }
            GUILayout.EndScrollView();

            GUILayout.EndArea();
        }

        private void DrawHealthBar(float percentage, Color barColor)
        {
            Rect barRect = GUILayoutUtility.GetRect(380, 20);
            GUI.Box(barRect, "");

            Rect fillRect = new Rect(barRect.x + 2, barRect.y + 2, (barRect.width - 4) * percentage, barRect.height - 4);
            Color oldColor = GUI.color;
            GUI.color = barColor;
            GUI.Box(fillRect, "");
            GUI.color = oldColor;
        }

        #endregion
    }
}
