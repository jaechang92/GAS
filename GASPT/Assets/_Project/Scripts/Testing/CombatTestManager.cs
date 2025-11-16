using System.Collections.Generic;
using UnityEngine;
using GASPT.Stats;
using GASPT.Gameplay.Enemy;
using GASPT.Data;
using GASPT.Skills;
using GASPT.Level;
using GASPT.Economy;

namespace GASPT.Testing
{
    /// <summary>
    /// 통합 전투 테스트 씬 관리자
    /// 플레이어, 적, 스킬, UI를 통합하여 실제 게임플레이 테스트 환경 제공
    /// </summary>
    public class CombatTestManager : MonoBehaviour
    {
        // ====== 플레이어 설정 ======

        [Header("플레이어 설정")]
        [SerializeField] [Tooltip("플레이어 GameObject")]
        private GameObject playerObject;

        [SerializeField] [Tooltip("플레이어 스탯 컴포넌트")]
        private PlayerStats playerStats;


        // ====== 적 설정 ======

        [Header("적 설정")]
        [SerializeField] [Tooltip("약한 적 데이터")]
        private EnemyData weakEnemyData;

        [SerializeField] [Tooltip("일반 적 데이터")]
        private EnemyData normalEnemyData;

        [SerializeField] [Tooltip("강한 적 데이터")]
        private EnemyData strongEnemyData;

        [SerializeField] [Tooltip("적 생성 위치들")]
        private Transform[] spawnPoints;

        [SerializeField] [Tooltip("적 프리팹 (Enemy 컴포넌트 포함)")]
        private GameObject enemyPrefab;


        // ====== 스킬 설정 ======

        [Header("스킬 설정")]
        [SerializeField] [Tooltip("테스트용 스킬 데이터 목록 (최대 4개)")]
        private List<SkillData> testSkills = new List<SkillData>();


        // ====== UI 참조 ======

        [Header("UI 참조")]
        [SerializeField] [Tooltip("테스트 제어 패널 (선택)")]
        private GameObject testControlPanel;


        // ====== 내부 상태 ======

        /// <summary>
        /// 생성된 적 목록
        /// </summary>
        private List<GameObject> activeEnemies = new List<GameObject>();

        /// <summary>
        /// God Mode (무적) 활성화 여부
        /// </summary>
        private bool isGodModeActive = false;

        /// <summary>
        /// 다음 적 생성 인덱스 (SpawnPoint 순환용)
        /// </summary>
        private int nextSpawnIndex = 0;

        /// <summary>
        /// 일시정지 상태
        /// </summary>
        private bool isPaused = false;

        /// <summary>
        /// OnGUI UI 표시 여부
        /// </summary>
        private bool showUI = true;

        /// <summary>
        /// FPS 계산용 변수
        /// </summary>
        private float deltaTime = 0f;


        // ====== Unity 생명주기 ======

        private void Awake()
        {
            ValidateReferences();
        }

        private void Start()
        {
            InitializeTest();
        }

        private void Update()
        {
            HandleInput();
            CalculateFPS();
        }


        // ====== 초기화 ======

        /// <summary>
        /// 테스트 환경 초기화
        /// </summary>
        private void InitializeTest()
        {
            Debug.Log("[CombatTestManager] 테스트 환경 초기화 시작");

            // 플레이어 스킬 등록
            RegisterTestSkills();

            // 플레이어 체력/마나 풀 회복
            ResetPlayer();

            Debug.Log("[CombatTestManager] 테스트 환경 초기화 완료");
        }

        /// <summary>
        /// 참조 검증
        /// </summary>
        private void ValidateReferences()
        {
            if (playerObject == null)
            {
                playerObject = GameObject.FindWithTag("Player");
                if (playerObject == null)
                {
                    Debug.LogError("[CombatTestManager] Player GameObject를 찾을 수 없습니다!");
                    return;
                }
            }

            if (playerStats == null)
            {
                playerStats = playerObject.GetComponent<PlayerStats>();
                if (playerStats == null)
                {
                    Debug.LogError("[CombatTestManager] PlayerStats 컴포넌트를 찾을 수 없습니다!");
                }
            }
        }

        /// <summary>
        /// 테스트 스킬 등록
        /// </summary>
        private void RegisterTestSkills()
        {
            if (testSkills == null || testSkills.Count == 0)
            {
                Debug.LogWarning("[CombatTestManager] 등록할 스킬이 없습니다.");
                return;
            }

            SkillSystem skillSystem = SkillSystem.Instance;
            if (skillSystem == null)
            {
                Debug.LogError("[CombatTestManager] SkillSystem을 찾을 수 없습니다!");
                return;
            }

            for (int i = 0; i < testSkills.Count && i < 4; i++)
            {
                if (testSkills[i] != null)
                {
                    skillSystem.RegisterSkill(i, testSkills[i]);
                    Debug.Log($"[CombatTestManager] 스킬 등록: Slot {i} - {testSkills[i].skillName}");
                }
            }
        }


        // ====== 테스트 제어 (Public API) ======

        /// <summary>
        /// 테스트 초기화 - 플레이어 체력/마나 회복
        /// </summary>
        [ContextMenu("Reset Test")]
        public void ResetTest()
        {
            ResetPlayer();
            ClearAllEnemies();
            Debug.Log("[CombatTestManager] 테스트 초기화 완료");
        }

        /// <summary>
        /// 플레이어 체력/마나 풀 회복
        /// </summary>
        [ContextMenu("Reset Player")]
        public void ResetPlayer()
        {
            if (playerStats == null) return;

            playerStats.Revive(); // 체력 풀 회복
            playerStats.RegenerateMana(playerStats.MaxMana); // 마나 풀 회복
            Debug.Log($"[CombatTestManager] 플레이어 초기화: HP {playerStats.CurrentHP}/{playerStats.MaxHP}, Mana {playerStats.CurrentMana}/{playerStats.MaxMana}");
        }

        /// <summary>
        /// 약한 적 생성
        /// </summary>
        [ContextMenu("Spawn Weak Enemy")]
        public void SpawnWeakEnemy()
        {
            SpawnEnemy(weakEnemyData, Color.green);
        }

        /// <summary>
        /// 일반 적 생성
        /// </summary>
        [ContextMenu("Spawn Normal Enemy")]
        public void SpawnNormalEnemy()
        {
            SpawnEnemy(normalEnemyData, Color.yellow);
        }

        /// <summary>
        /// 강한 적 생성
        /// </summary>
        [ContextMenu("Spawn Strong Enemy")]
        public void SpawnStrongEnemy()
        {
            SpawnEnemy(strongEnemyData, Color.red);
        }

        /// <summary>
        /// 모든 적 제거
        /// </summary>
        [ContextMenu("Clear All Enemies")]
        public void ClearAllEnemies()
        {
            foreach (GameObject enemy in activeEnemies)
            {
                if (enemy != null)
                {
                    Destroy(enemy);
                }
            }
            activeEnemies.Clear();
            Debug.Log("[CombatTestManager] 모든 적 제거 완료");
        }


        // ====== 치트 기능 ======

        /// <summary>
        /// 플레이어 레벨 설정
        /// </summary>
        public void SetPlayerLevel(int level)
        {
            PlayerLevel playerLevel = PlayerLevel.Instance;
            if (playerLevel == null)
            {
                Debug.LogError("[CombatTestManager] PlayerLevel을 찾을 수 없습니다!");
                return;
            }

            // 현재 레벨과 차이 계산
            int currentLevel = playerLevel.Level;
            int levelDifference = level - currentLevel;

            if (levelDifference > 0)
            {
                // 레벨업
                int expNeeded = 0;
                for (int i = 0; i < levelDifference; i++)
                {
                    expNeeded += (currentLevel + i) * 100;
                }
                playerLevel.AddExp(expNeeded);
            }
            else if (levelDifference < 0)
            {
                Debug.LogWarning("[CombatTestManager] 레벨 다운은 지원하지 않습니다.");
            }

            Debug.Log($"[CombatTestManager] 플레이어 레벨 설정: {currentLevel} → {level}");
        }

        /// <summary>
        /// 모든 스킬 쿨다운 초기화
        /// </summary>
        [ContextMenu("Reset All Cooldowns")]
        public void ResetAllCooldowns()
        {
            SkillSystem skillSystem = SkillSystem.Instance;
            if (skillSystem == null) return;

            for (int i = 0; i < 4; i++)
            {
                Skill skill = skillSystem.GetSkill(i);
                if (skill != null)
                {
                    // 쿨다운 초기화는 SkillSystem에서 제공하지 않으므로
                    // 스킬 재등록으로 우회
                    if (i < testSkills.Count && testSkills[i] != null)
                    {
                        skillSystem.RegisterSkill(i, testSkills[i]);
                    }
                }
            }

            Debug.Log("[CombatTestManager] 모든 스킬 쿨다운 초기화 완료");
        }

        /// <summary>
        /// God Mode 토글
        /// </summary>
        [ContextMenu("Toggle God Mode")]
        public void ToggleGodMode()
        {
            isGodModeActive = !isGodModeActive;
            Debug.Log($"[CombatTestManager] God Mode: {(isGodModeActive ? "활성화" : "비활성화")}");

            // God Mode 활성화 시 플레이어를 TakeDamage에서 보호하는 로직은
            // PlayerStats에 직접 추가하거나, 이벤트를 통해 처리 필요
            // 현재는 로그만 출력
        }

        /// <summary>
        /// 골드 추가
        /// </summary>
        public void AddGold(int amount)
        {
            CurrencySystem currencySystem = CurrencySystem.Instance;
            if (currencySystem == null)
            {
                Debug.LogError("[CombatTestManager] CurrencySystem을 찾을 수 없습니다!");
                return;
            }

            currencySystem.AddGold(amount);
            Debug.Log($"[CombatTestManager] 골드 추가: +{amount}");
        }


        // ====== 디버그 정보 ======

        /// <summary>
        /// 플레이어 스탯 정보 출력
        /// </summary>
        [ContextMenu("Log Player Stats")]
        public void LogPlayerStats()
        {
            if (playerStats == null)
            {
                Debug.LogWarning("[CombatTestManager] PlayerStats가 없습니다.");
                return;
            }

            Debug.Log("========== Player Stats ==========");
            Debug.Log($"HP: {playerStats.CurrentHP}/{playerStats.MaxHP}");
            Debug.Log($"Mana: {playerStats.CurrentMana}/{playerStats.MaxMana}");
            Debug.Log($"Attack: {playerStats.Attack}");
            Debug.Log($"Defense: {playerStats.Defense}");
            Debug.Log($"Level: {PlayerLevel.Instance?.Level ?? 0}");
            Debug.Log($"Gold: {CurrencySystem.Instance?.Gold ?? 0}");
            Debug.Log("==================================");
        }

        /// <summary>
        /// 활성 적 목록 출력
        /// </summary>
        [ContextMenu("Log Active Enemies")]
        public void LogActiveEnemies()
        {
            Debug.Log($"========== Active Enemies ({activeEnemies.Count}) ==========");
            for (int i = 0; i < activeEnemies.Count; i++)
            {
                if (activeEnemies[i] != null)
                {
                    Enemy enemy = activeEnemies[i].GetComponent<Enemy>();
                    if (enemy != null)
                    {
                        Debug.Log($"Enemy {i}: {enemy.Data.enemyName} - HP {enemy.CurrentHp}/{enemy.MaxHp}");
                    }
                }
            }
            Debug.Log("==================================");
        }

        /// <summary>
        /// 스킬 상태 출력
        /// </summary>
        [ContextMenu("Log Skill Status")]
        public void LogSkillStatus()
        {
            SkillSystem skillSystem = SkillSystem.Instance;
            if (skillSystem == null)
            {
                Debug.LogWarning("[CombatTestManager] SkillSystem을 찾을 수 없습니다.");
                return;
            }

            Debug.Log("========== Skill Status ==========");
            for (int i = 0; i < 4; i++)
            {
                Skill skill = skillSystem.GetSkill(i);
                if (skill != null)
                {
                    float cooldownRatio = skillSystem.GetCooldownRatio(i);
                    bool isReady = cooldownRatio <= 0f;
                    Debug.Log($"Slot {i}: {skill.Data.name} - {(isReady ? "준비" : $"쿨다운 {cooldownRatio * 100:F0}%")}");
                }
                else
                {
                    Debug.Log($"Slot {i}: 비어있음");
                }
            }
            Debug.Log("==================================");
        }


        // ====== 내부 메서드 ======

        /// <summary>
        /// 적 생성
        /// </summary>
        private void SpawnEnemy(EnemyData data, Color color)
        {
            if (data == null)
            {
                Debug.LogError("[CombatTestManager] EnemyData가 null입니다!");
                return;
            }

            if (enemyPrefab == null)
            {
                Debug.LogError("[CombatTestManager] Enemy Prefab이 할당되지 않았습니다!");
                return;
            }

            // 생성 위치 결정
            Vector3 spawnPosition = GetNextSpawnPosition();

            // 적 생성
            GameObject enemyObject = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
            enemyObject.name = $"{data.enemyName}_{activeEnemies.Count}";

            // Enemy 컴포넌트 설정
            Enemy enemy = enemyObject.GetComponent<Enemy>();
            if (enemy != null)
            {
                // EnemyData 설정 (Reflection 사용)
                System.Reflection.FieldInfo dataField = enemy.GetType().GetField("enemyData",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (dataField != null)
                {
                    dataField.SetValue(enemy, data);
                }

                // 재초기화
                System.Reflection.MethodInfo initMethod = enemy.GetType().GetMethod("Initialize",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (initMethod != null)
                {
                    initMethod.Invoke(enemy, null);
                }

                // 사망 이벤트 구독
                enemy.OnDeath += OnEnemyDeath;
            }

            // 비주얼 색상 설정 (SpriteRenderer가 있다면)
            SpriteRenderer spriteRenderer = enemyObject.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.color = color;
            }

            // 목록에 추가
            activeEnemies.Add(enemyObject);

            Debug.Log($"[CombatTestManager] 적 생성: {data.enemyName} at {spawnPosition}");
        }

        /// <summary>
        /// 다음 생성 위치 가져오기
        /// </summary>
        private Vector3 GetNextSpawnPosition()
        {
            if (spawnPoints != null && spawnPoints.Length > 0)
            {
                Vector3 position = spawnPoints[nextSpawnIndex].position;
                nextSpawnIndex = (nextSpawnIndex + 1) % spawnPoints.Length;
                return position;
            }
            else
            {
                // SpawnPoint가 없으면 플레이어 우측에 랜덤 생성
                Vector3 playerPosition = playerObject != null ? playerObject.transform.position : Vector3.zero;
                return playerPosition + new Vector3(Random.Range(3f, 6f), Random.Range(-2f, 2f), 0f);
            }
        }

        /// <summary>
        /// 적 사망 이벤트 핸들러
        /// </summary>
        private void OnEnemyDeath(Enemy enemy)
        {
            if (enemy != null && enemy.gameObject != null)
            {
                activeEnemies.Remove(enemy.gameObject);
                Debug.Log($"[CombatTestManager] 적 사망: {enemy.Data.enemyName} (남은 적: {activeEnemies.Count})");
            }
        }

        /// <summary>
        /// 키보드 입력 처리
        /// </summary>
        private void HandleInput()
        {
            // 스킬 사용 (1, 2, 3, 4)
            HandleSkillInput();

            // F1: 플레이어 초기화
            if (Input.GetKeyDown(KeyCode.F1))
            {
                ResetPlayer();
            }

            // F2: 약한 적 생성
            if (Input.GetKeyDown(KeyCode.F2))
            {
                SpawnWeakEnemy();
            }

            // F3: 일반 적 생성
            if (Input.GetKeyDown(KeyCode.F3))
            {
                SpawnNormalEnemy();
            }

            // F4: 강한 적 생성
            if (Input.GetKeyDown(KeyCode.F4))
            {
                SpawnStrongEnemy();
            }

            // F5: 모든 적 제거
            if (Input.GetKeyDown(KeyCode.F5))
            {
                ClearAllEnemies();
            }

            // F9: God Mode 토글
            if (Input.GetKeyDown(KeyCode.F9))
            {
                ToggleGodMode();
            }

            // F12: 디버그 정보 출력
            if (Input.GetKeyDown(KeyCode.F12))
            {
                LogPlayerStats();
                LogActiveEnemies();
                LogSkillStatus();
            }

            // F10: UI 토글
            if (Input.GetKeyDown(KeyCode.F10))
            {
                showUI = !showUI;
            }
        }

        /// <summary>
        /// 스킬 입력 처리 (1, 2, 3, 4 키)
        /// </summary>
        private void HandleSkillInput()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                UseSkill(0);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                UseSkill(1);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                UseSkill(2);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                UseSkill(3);
            }
        }

        /// <summary>
        /// 스킬 사용
        /// </summary>
        private void UseSkill(int slotIndex)
        {
            SkillSystem skillSystem = SkillSystem.Instance;
            if (skillSystem == null)
            {
                Debug.LogWarning("[CombatTestManager] SkillSystem을 찾을 수 없습니다.");
                return;
            }

            if (playerObject == null)
            {
                Debug.LogWarning("[CombatTestManager] Player GameObject가 없습니다.");
                return;
            }

            // 타겟 결정 (첫 번째 살아있는 적 또는 null)
            GameObject target = GetFirstAliveEnemy();

            // 스킬 사용 시도
            bool success = skillSystem.TryUseSkill(slotIndex, target);

            if (success)
            {
                Skill skill = skillSystem.GetSkill(slotIndex);
                string targetName = target != null ? target.name : "Self";
                Debug.Log($"[CombatTestManager] 스킬 사용 성공: Slot {slotIndex} ({skill?.Data.name}) → {targetName}");
            }
            else
            {
                Debug.LogWarning($"[CombatTestManager] 스킬 사용 실패: Slot {slotIndex}");
            }
        }

        /// <summary>
        /// 첫 번째 살아있는 적 가져오기
        /// </summary>
        private GameObject GetFirstAliveEnemy()
        {
            foreach (GameObject enemyObj in activeEnemies)
            {
                if (enemyObj != null)
                {
                    Enemy enemy = enemyObj.GetComponent<Enemy>();
                    if (enemy != null && !enemy.IsDead)
                    {
                        return enemyObj;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 일시정지 토글
        /// </summary>
        public void TogglePause()
        {
            isPaused = !isPaused;
            Time.timeScale = isPaused ? 0f : 1f;
            Debug.Log($"[CombatTestManager] 일시정지: {(isPaused ? "활성화" : "비활성화")}");
        }

        /// <summary>
        /// FPS 계산
        /// </summary>
        private void CalculateFPS()
        {
            deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        }


        // ====== OnGUI 테스트 UI ======

        private void OnGUI()
        {
            if (!showUI) return;

            // GUI 스타일 설정
            GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
            boxStyle.normal.background = MakeTex(2, 2, new Color(0f, 0f, 0f, 0.8f));

            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.fontSize = 12;
            buttonStyle.normal.textColor = Color.white;

            GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.fontSize = 12;
            labelStyle.normal.textColor = Color.white;

            GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.fontSize = 14;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.normal.textColor = Color.yellow;

            // 메인 UI 영역 (우측 상단)
            GUILayout.BeginArea(new Rect(Screen.width - 260, 10, 250, Screen.height - 20), boxStyle);

            GUILayout.Label("=== COMBAT TEST CONTROLS ===", titleStyle);
            GUILayout.Space(10);

            // ===== Player Controls =====
            GUILayout.Label("[ Player Controls ]", titleStyle);

            if (GUILayout.Button("F1: Reset Player", buttonStyle))
                ResetPlayer();

            if (GUILayout.Button($"God Mode: {(isGodModeActive ? "ON" : "OFF")}", buttonStyle))
                ToggleGodMode();

            if (GUILayout.Button($"Pause: {(isPaused ? "ON" : "OFF")}", buttonStyle))
                TogglePause();

            GUILayout.Space(10);

            // ===== Enemy Controls =====
            GUILayout.Label("[ Enemy Controls ]", titleStyle);

            if (GUILayout.Button("F2: Spawn Weak Enemy", buttonStyle))
                SpawnWeakEnemy();

            if (GUILayout.Button("F3: Spawn Normal Enemy", buttonStyle))
                SpawnNormalEnemy();

            if (GUILayout.Button("F4: Spawn Strong Enemy", buttonStyle))
                SpawnStrongEnemy();

            if (GUILayout.Button("F5: Clear All Enemies", buttonStyle))
                ClearAllEnemies();

            GUILayout.Space(10);

            // ===== Skill Controls =====
            GUILayout.Label("[ Skill Controls ]", titleStyle);

            if (GUILayout.Button("Reset All Cooldowns", buttonStyle))
                ResetAllCooldowns();

            GUILayout.Space(10);

            // ===== Cheat Controls =====
            GUILayout.Label("[ Cheat Controls ]", titleStyle);

            if (GUILayout.Button("Add 100 Gold", buttonStyle))
                AddGold(100);

            if (GUILayout.Button("Level Up", buttonStyle))
            {
                if (PlayerLevel.Instance != null)
                    SetPlayerLevel(PlayerLevel.Instance.Level + 1);
            }

            GUILayout.Space(10);

            // ===== Real-time Info =====
            GUILayout.Label("[ Real-time Info ]", titleStyle);

            if (playerStats != null)
            {
                GUILayout.Label($"HP: {playerStats.CurrentHP}/{playerStats.MaxHP}", labelStyle);
                GUILayout.Label($"Mana: {playerStats.CurrentMana}/{playerStats.MaxMana}", labelStyle);
                GUILayout.Label($"Attack: {playerStats.Attack}", labelStyle);
                GUILayout.Label($"Defense: {playerStats.Defense}", labelStyle);
            }

            if (PlayerLevel.Instance != null)
            {
                GUILayout.Label($"Level: {PlayerLevel.Instance.Level}", labelStyle);
                GUILayout.Label($"EXP: {PlayerLevel.Instance.CurrentExp}/{PlayerLevel.Instance.RequiredExp}", labelStyle);
            }

            if (CurrencySystem.Instance != null)
            {
                GUILayout.Label($"Gold: {CurrencySystem.Instance.Gold}", labelStyle);
            }

            GUILayout.Label($"Enemies: {activeEnemies.Count}", labelStyle);

            // FPS 표시
            float fps = 1.0f / deltaTime;
            Color fpsColor = fps >= 60 ? Color.green : fps >= 30 ? Color.yellow : Color.red;
            GUIStyle fpsStyle = new GUIStyle(labelStyle);
            fpsStyle.normal.textColor = fpsColor;
            GUILayout.Label($"FPS: {fps:F0}", fpsStyle);

            GUILayout.Space(10);

            // ===== Debug Logs =====
            GUILayout.Label("[ Debug Logs ]", titleStyle);

            if (GUILayout.Button("F12: Log All Status", buttonStyle))
            {
                LogPlayerStats();
                LogActiveEnemies();
                LogSkillStatus();
            }

            GUILayout.Space(10);

            // ===== Help =====
            GUILayout.Label("[ Shortcuts ]", titleStyle);
            GUILayout.Label("F10: Toggle UI", labelStyle);
            GUILayout.Label("1,2,3,4: Use Skills", labelStyle);

            GUILayout.EndArea();

            // 스킬 쿨다운 표시 (좌측 하단)
            DrawSkillCooldowns(labelStyle, titleStyle, boxStyle);
        }

        /// <summary>
        /// 스킬 쿨다운 UI 표시
        /// </summary>
        private void DrawSkillCooldowns(GUIStyle labelStyle, GUIStyle titleStyle, GUIStyle boxStyle)
        {
            SkillSystem skillSystem = SkillSystem.Instance;
            if (skillSystem == null) return;

            GUILayout.BeginArea(new Rect(10, Screen.height - 150, 200, 140), boxStyle);

            GUILayout.Label("[ Skill Cooldowns ]", titleStyle);

            for (int i = 0; i < 4; i++)
            {
                Skill skill = skillSystem.GetSkill(i);
                if (skill != null)
                {
                    float cooldownRatio = skillSystem.GetCooldownRatio(i);
                    bool isReady = cooldownRatio <= 0f;

                    string status = isReady ? "READY" : $"CD: {cooldownRatio * 100:F0}%";
                    Color statusColor = isReady ? Color.green : Color.red;

                    GUIStyle skillStyle = new GUIStyle(labelStyle);
                    skillStyle.normal.textColor = statusColor;

                    GUILayout.Label($"[{i + 1}] {skill.Data.name}: {status}", skillStyle);
                }
                else
                {
                    GUILayout.Label($"[{i + 1}] Empty", labelStyle);
                }
            }

            GUILayout.EndArea();
        }

        /// <summary>
        /// OnGUI용 텍스처 생성 (배경색)
        /// </summary>
        private Texture2D MakeTex(int width, int height, Color color)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; i++)
                pix[i] = color;

            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }
    }
}
