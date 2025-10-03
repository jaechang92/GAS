using UnityEngine;
using Combat.Core;
using Player;
using Combat.Attack;
using Core.Enums;
using System.Collections.Generic;
using GAS.Core;
using Tests;

namespace Combat.Demo
{
    /// <summary>
    /// 플레이어 전투 시스템 데모
    /// Player + Combat 시스템 통합 테스트
    /// SceneBootstrap을 사용하여 필요한 것만 초기화
    /// </summary>
    public class PlayerCombatDemo : MonoBehaviour
    {
        [Header("초기화")]
        [Tooltip("SceneBootstrap이 있으면 자동으로 찾아서 초기화를 기다림")]
        [SerializeField] private bool waitForSceneBootstrap = true;

        [Header("플레이어 설정")]
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private Vector3 playerSpawnPosition = Vector3.zero;

        [Header("적 설정")]
        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private Vector3[] enemySpawnPositions = new Vector3[]
        {
            new Vector3(5f, 0f, 0f),   // 오른쪽
            new Vector3(-5f, 0f, 0f),  // 왼쪽
            new Vector3(8f, 0f, 0f)    // 오른쪽 멀리 (2D이므로 Z=0)
        };

        [Header("콤보 설정")]
        [SerializeField] private bool setupDefaultCombo = true;
        [SerializeField] private int comboCount = 3;

        [Header("UI 설정")]
        [SerializeField] private bool showUI = true;

        [Header("환경 설정")]
        [SerializeField] private bool createGround = true;
        [SerializeField] private Vector3 groundPosition = new Vector3(0f, -3f, 0f);
        [SerializeField] private Vector2 groundSize = new Vector2(30f, 2f);

        // 생성된 오브젝트
        private GameObject player;
        private PlayerController playerController;
        private List<GameObject> enemies = new List<GameObject>();
        private GameObject ground;

        // 통계
        private int totalHits = 0;
        private int totalKills = 0;
        private float totalDamageDealt = 0f;
        private List<string> eventLog = new List<string>();

        #region Unity 생명주기

        private async void Start()
        {
            // SceneBootstrap이 있으면 초기화를 기다림
            if (waitForSceneBootstrap)
            {
                var bootstrap = FindFirstObjectByType<SceneBootstrap>();
                if (bootstrap != null)
                {
                    LogEvent("[Demo] SceneBootstrap 초기화 대기 중...");

                    // 초기화가 완료될 때까지 대기
                    while (!bootstrap.IsInitialized)
                    {
                        await Awaitable.NextFrameAsync(destroyCancellationToken);
                    }

                    LogEvent("[Demo] SceneBootstrap 초기화 완료!");
                }
                else
                {
                    LogEvent("[Demo] SceneBootstrap을 찾을 수 없습니다. 바로 시작합니다.");
                }
            }

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

            // 바닥 생성 (2D)
            if (createGround)
            {
                CreateGround();
            }

            // 플레이어 생성
            CreatePlayer();

            // 적 생성
            CreateEnemies();

            // 콤보 설정
            if (setupDefaultCombo)
            {
                SetupPlayerCombo();
            }

            // 카메라 설정 (2D)
            SetupCamera();

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

                // SpriteRenderer 추가 (2D 비주얼)
                var spriteRenderer = player.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = CreateSimpleSprite(Color.cyan);
                spriteRenderer.sortingOrder = 1; // 적보다 앞에 표시

                // Rigidbody2D 추가 (2D 물리)
                var rb = player.AddComponent<Rigidbody2D>();
                rb.gravityScale = 3f;
                rb.constraints = RigidbodyConstraints2D.FreezeRotation; // 회전 방지

                // CapsuleCollider2D 추가 (2D 충돌)
                var col = player.AddComponent<CapsuleCollider2D>();
                col.size = new Vector2(1f, 2f);
                col.direction = CapsuleDirection2D.Vertical;

                // AbilitySystem 추가
                player.AddComponent<AbilitySystem>();
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
        /// 기본 적 생성 (2D 방식)
        /// </summary>
        private GameObject CreateBasicEnemy(Vector3 position)
        {
            // 빈 GameObject 생성
            var enemy = new GameObject("Enemy");
            enemy.transform.position = position;

            // SpriteRenderer 추가 (2D 비주얼)
            var spriteRenderer = enemy.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = CreateSimpleSprite(Color.red);
            spriteRenderer.sortingOrder = 0;

            // Rigidbody2D 추가 (2D 물리)
            var rb = enemy.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f; // 적은 중력 영향 안 받음

            // CapsuleCollider2D 추가 (2D 충돌)
            var col = enemy.AddComponent<CapsuleCollider2D>();
            col.size = new Vector2(1f, 2f); // 캡슐 크기
            col.direction = CapsuleDirection2D.Vertical;

            // HealthSystem 추가
            var health = enemy.AddComponent<HealthSystem>();

            return enemy;
        }

        /// <summary>
        /// 간단한 사각형 스프라이트 생성
        /// </summary>
        private Sprite CreateSimpleSprite(Color color)
        {
            // 64x64 텍스처 생성
            int size = 64;
            Texture2D texture = new Texture2D(size, size);

            // 모든 픽셀을 지정된 색상으로 채움
            Color[] pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }
            texture.SetPixels(pixels);
            texture.Apply();

            // 텍스처를 Sprite로 변환
            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0, 0, size, size),
                new Vector2(0.5f, 0.5f), // pivot (중심점)
                100f // pixels per unit
            );

            return sprite;
        }

        /// <summary>
        /// 바닥 생성 (2D)
        /// </summary>
        private void CreateGround()
        {
            if (ground != null)
            {
                Destroy(ground);
            }

            ground = new GameObject("Ground");
            ground.transform.position = groundPosition;

            // SpriteRenderer 추가
            var spriteRenderer = ground.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = CreateSimpleSprite(new Color(0.3f, 0.3f, 0.3f)); // 회색
            spriteRenderer.sortingOrder = -1; // 배경

            // 스프라이트 크기 조정
            ground.transform.localScale = new Vector3(groundSize.x, groundSize.y, 1f);

            // BoxCollider2D 추가 (정적 충돌체)
            var col = ground.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1f, 1f); // localScale로 크기 조정되므로 1로 설정

            LogEvent($"[Ground] 생성 완료 - 위치: {groundPosition}, 크기: {groundSize}");
        }

        /// <summary>
        /// 카메라 설정 (2D Orthographic)
        /// </summary>
        private void SetupCamera()
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                // 메인 카메라가 없으면 생성
                var cameraObj = new GameObject("Main Camera");
                mainCamera = cameraObj.AddComponent<Camera>();
                cameraObj.tag = "MainCamera";
                LogEvent("[Camera] Main Camera 생성");
            }

            // Orthographic 모드로 설정 (2D)
            mainCamera.orthographic = true;
            mainCamera.orthographicSize = 5f; // 화면에 보이는 세로 범위

            // 카메라 위치 설정 (플레이어를 중심으로)
            mainCamera.transform.position = new Vector3(0f, 0f, -10f);

            // 배경 색상
            mainCamera.backgroundColor = new Color(0.1f, 0.1f, 0.15f); // 어두운 파란색

            LogEvent("[Camera] Orthographic 설정 완료");
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
            if (ground != null) Destroy(ground);
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
