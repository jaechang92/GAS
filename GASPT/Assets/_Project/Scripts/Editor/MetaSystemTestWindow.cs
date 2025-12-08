#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using GASPT.Meta;

namespace GASPT.Editor
{
    /// <summary>
    /// 메타 시스템 통합 테스트 에디터 윈도우
    /// 업그레이드, 업적, 해금 시스템을 테스트
    /// </summary>
    public class MetaSystemTestWindow : EditorWindow
    {
        // ====== 상태 ======

        private Vector2 scrollPosition;
        private bool showCurrency = true;
        private bool showUpgrades = true;
        private bool showAchievements = true;
        private bool showUnlocks = true;
        private bool showSpecial = true;

        private int boneToAdd = 1000;
        private int soulToAdd = 100;


        // ====== 윈도우 ======

        [MenuItem("GASPT/Meta/Test Window")]
        public static void ShowWindow()
        {
            var window = GetWindow<MetaSystemTestWindow>("Meta System Test");
            window.minSize = new Vector2(400, 500);
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            DrawHeader();

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("플레이 모드에서만 테스트할 수 있습니다.", MessageType.Warning);
                DrawAssetGenerators();
                EditorGUILayout.EndScrollView();
                return;
            }

            DrawCurrencySection();
            DrawUpgradeSection();
            DrawAchievementSection();
            DrawUnlockSection();
            DrawSpecialSection();
            DrawRunSimulation();

            EditorGUILayout.EndScrollView();
        }

        private void OnInspectorUpdate()
        {
            Repaint();
        }


        // ====== 헤더 ======

        private void DrawHeader()
        {
            EditorGUILayout.Space(10);
            GUILayout.Label("메타 진행 시스템 테스트", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
        }


        // ====== 에셋 생성기 ======

        private void DrawAssetGenerators()
        {
            EditorGUILayout.Space(10);
            GUILayout.Label("에셋 생성 도구", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("기본 업그레이드 생성", GUILayout.Height(30)))
            {
                UpgradeAssetGenerator.GenerateDefaultUpgrades();
            }

            if (GUILayout.Button("기본 업적 생성", GUILayout.Height(30)))
            {
                AchievementAssetGenerator.GenerateDefaultAchievements();
            }

            EditorGUILayout.EndHorizontal();
        }


        // ====== 재화 섹션 ======

        private void DrawCurrencySection()
        {
            EditorGUILayout.Space(10);
            showCurrency = EditorGUILayout.Foldout(showCurrency, "재화 관리", true, EditorStyles.foldoutHeader);

            if (!showCurrency) return;

            var metaManager = MetaProgressionManager.Instance;
            if (metaManager == null)
            {
                EditorGUILayout.HelpBox("MetaProgressionManager가 없습니다.", MessageType.Error);
                return;
            }

            EditorGUI.indentLevel++;

            // 현재 재화
            EditorGUILayout.LabelField("현재 Bone", metaManager.Currency.Bone.ToString());
            EditorGUILayout.LabelField("현재 Soul", metaManager.Currency.Soul.ToString());

            EditorGUILayout.Space(5);

            // Bone 추가
            EditorGUILayout.BeginHorizontal();
            boneToAdd = EditorGUILayout.IntField("Bone 추가량", boneToAdd);
            if (GUILayout.Button("추가", GUILayout.Width(60)))
            {
                metaManager.Currency.DebugAddBone(boneToAdd);
                Debug.Log($"[Test] Bone +{boneToAdd}");
            }
            EditorGUILayout.EndHorizontal();

            // Soul 추가
            EditorGUILayout.BeginHorizontal();
            soulToAdd = EditorGUILayout.IntField("Soul 추가량", soulToAdd);
            if (GUILayout.Button("추가", GUILayout.Width(60)))
            {
                metaManager.Currency.DebugAddSoul(soulToAdd);
                Debug.Log($"[Test] Soul +{soulToAdd}");
            }
            EditorGUILayout.EndHorizontal();

            EditorGUI.indentLevel--;
        }


        // ====== 업그레이드 섹션 ======

        private void DrawUpgradeSection()
        {
            EditorGUILayout.Space(10);
            showUpgrades = EditorGUILayout.Foldout(showUpgrades, "업그레이드", true, EditorStyles.foldoutHeader);

            if (!showUpgrades) return;

            var metaManager = MetaProgressionManager.Instance;
            if (metaManager == null) return;

            EditorGUI.indentLevel++;

            foreach (var upgrade in metaManager.GetAllUpgrades())
            {
                if (upgrade == null) continue;

                int level = metaManager.GetUpgradeLevel(upgrade.upgradeId);
                bool isMaxLevel = level >= upgrade.maxLevel;
                int cost = isMaxLevel ? 0 : upgrade.GetUpgradeCost(level);

                EditorGUILayout.BeginHorizontal();

                // 이름과 레벨
                string labelText = $"{upgrade.upgradeName}: Lv{level}/{upgrade.maxLevel}";
                EditorGUILayout.LabelField(labelText, GUILayout.Width(180));

                // 효과
                if (level > 0)
                {
                    EditorGUILayout.LabelField(upgrade.GetEffectDescription(level), GUILayout.Width(120));
                }
                else
                {
                    EditorGUILayout.LabelField("-", GUILayout.Width(120));
                }

                // 구매 버튼
                GUI.enabled = !isMaxLevel;
                string buttonText = isMaxLevel ? "MAX" : $"{cost} {upgrade.currencyType}";
                if (GUILayout.Button(buttonText, GUILayout.Width(80)))
                {
                    metaManager.TryPurchaseUpgrade(upgrade.upgradeId);
                }
                GUI.enabled = true;

                EditorGUILayout.EndHorizontal();
            }

            EditorGUI.indentLevel--;
        }


        // ====== 업적 섹션 ======

        private void DrawAchievementSection()
        {
            EditorGUILayout.Space(10);
            showAchievements = EditorGUILayout.Foldout(showAchievements, "업적", true, EditorStyles.foldoutHeader);

            if (!showAchievements) return;

            var achievementManager = AchievementManager.Instance;
            if (achievementManager == null)
            {
                EditorGUILayout.HelpBox("AchievementManager가 없습니다.", MessageType.Warning);
                return;
            }

            EditorGUI.indentLevel++;

            EditorGUILayout.LabelField($"완료: {achievementManager.CompletedCount} / {achievementManager.TotalCount}");
            EditorGUILayout.Space(5);

            foreach (var achievement in achievementManager.AllAchievements)
            {
                if (achievement == null) continue;

                int progress = achievementManager.GetProgress(achievement.achievementId);
                bool isCompleted = achievementManager.IsCompleted(achievement.achievementId);

                EditorGUILayout.BeginHorizontal();

                // 이름
                EditorGUILayout.LabelField(achievement.achievementName, GUILayout.Width(150));

                // 상태
                string status = isCompleted ? "완료" : achievement.GetProgressText(progress);
                Color statusColor = isCompleted ? Color.green : Color.white;
                GUI.color = statusColor;
                EditorGUILayout.LabelField(status, GUILayout.Width(80));
                GUI.color = Color.white;

                // 보상
                EditorGUILayout.LabelField(achievement.GetRewardText(), GUILayout.Width(100));

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("모든 업적 완료"))
            {
                foreach (var achievement in achievementManager.AllAchievements)
                {
                    if (!achievementManager.IsCompleted(achievement.achievementId))
                    {
                        achievementManager.SetProgress(achievement.achievementId, achievement.targetValue);
                    }
                }
            }
            if (GUILayout.Button("업적 초기화"))
            {
                // Reset via context menu method
                Debug.Log("[Test] 업적 초기화는 AchievementManager의 ContextMenu를 사용하세요.");
            }
            EditorGUILayout.EndHorizontal();

            EditorGUI.indentLevel--;
        }


        // ====== 해금 섹션 ======

        private void DrawUnlockSection()
        {
            EditorGUILayout.Space(10);
            showUnlocks = EditorGUILayout.Foldout(showUnlocks, "해금", true, EditorStyles.foldoutHeader);

            if (!showUnlocks) return;

            var unlockManager = UnlockManager.Instance;
            if (unlockManager == null)
            {
                EditorGUILayout.HelpBox("UnlockManager가 없습니다.", MessageType.Warning);
                return;
            }

            EditorGUI.indentLevel++;

            foreach (var form in unlockManager.AllUnlockableForms)
            {
                if (form == null) continue;

                bool isUnlocked = unlockManager.IsUnlocked(form.formId);

                EditorGUILayout.BeginHorizontal();

                // 이름
                EditorGUILayout.LabelField(form.displayName, GUILayout.Width(150));

                // 상태
                if (isUnlocked)
                {
                    GUI.color = Color.green;
                    EditorGUILayout.LabelField("해금됨", GUILayout.Width(80));
                    GUI.color = Color.white;
                }
                else
                {
                    EditorGUILayout.LabelField($"{form.soulCost} Soul", GUILayout.Width(80));
                }

                // 해금 버튼
                GUI.enabled = !isUnlocked && unlockManager.CanUnlock(form);
                if (GUILayout.Button(isUnlocked ? "-" : "해금", GUILayout.Width(60)))
                {
                    unlockManager.TryUnlock(form);
                }
                GUI.enabled = true;

                EditorGUILayout.EndHorizontal();
            }

            EditorGUI.indentLevel--;
        }


        // ====== 특수 업그레이드 섹션 ======

        private void DrawSpecialSection()
        {
            EditorGUILayout.Space(10);
            showSpecial = EditorGUILayout.Foldout(showSpecial, "특수 업그레이드 (런타임)", true, EditorStyles.foldoutHeader);

            if (!showSpecial) return;

            var applier = SpecialUpgradeApplier.Instance;
            var metaManager = MetaProgressionManager.Instance;

            EditorGUI.indentLevel++;

            if (metaManager != null)
            {
                EditorGUILayout.LabelField("추가 대시", $"+{metaManager.GetExtraDash()}");
                EditorGUILayout.LabelField("부활 횟수", $"{metaManager.GetReviveCount()}회");
                EditorGUILayout.LabelField("시작 골드", $"{metaManager.GetStartGold()}G");
            }

            EditorGUILayout.Space(5);

            if (applier != null)
            {
                EditorGUILayout.LabelField("현재 런 - 남은 부활", $"{applier.RemainingRevives}회");
                EditorGUILayout.LabelField("현재 런 - 총 대시", $"{applier.TotalDashCount}회");

                if (GUILayout.Button("부활 사용 테스트"))
                {
                    applier.TryRevive();
                }
            }
            else
            {
                EditorGUILayout.HelpBox("SpecialUpgradeApplier가 없습니다.", MessageType.Info);
            }

            EditorGUI.indentLevel--;
        }


        // ====== 런 시뮬레이션 ======

        private void DrawRunSimulation()
        {
            EditorGUILayout.Space(10);
            GUILayout.Label("런 시뮬레이션", EditorStyles.boldLabel);

            var metaManager = MetaProgressionManager.Instance;
            if (metaManager == null) return;

            EditorGUILayout.LabelField("런 진행 중", metaManager.IsInRun ? "예" : "아니오");

            EditorGUILayout.BeginHorizontal();

            if (!metaManager.IsInRun)
            {
                if (GUILayout.Button("런 시작", GUILayout.Height(30)))
                {
                    metaManager.StartRun();
                }
            }
            else
            {
                if (GUILayout.Button("런 클리어", GUILayout.Height(30)))
                {
                    // 임시 재화 추가
                    metaManager.Currency.AddTempBone(500);
                    metaManager.Currency.AddTempSoul(25);
                    metaManager.EndRun(true, 5, 100);
                }

                if (GUILayout.Button("런 실패", GUILayout.Height(30)))
                {
                    metaManager.Currency.AddTempBone(200);
                    metaManager.EndRun(false, 2, 30);
                }
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            // 업적 진행 시뮬레이션
            GUILayout.Label("업적 진행 시뮬레이션", EditorStyles.miniLabel);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("적 10 처치"))
            {
                var achievementManager = AchievementManager.Instance;
                if (achievementManager != null)
                {
                    for (int i = 0; i < 10; i++)
                    {
                        achievementManager.ReportEnemyKill(false);
                    }
                }
            }

            if (GUILayout.Button("보스 1 처치"))
            {
                var achievementManager = AchievementManager.Instance;
                if (achievementManager != null)
                {
                    achievementManager.ReportEnemyKill(true);
                }
            }

            if (GUILayout.Button("폼 수집"))
            {
                var achievementManager = AchievementManager.Instance;
                if (achievementManager != null)
                {
                    achievementManager.ReportFormCollected();
                }
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);

            // 초기화
            GUI.color = Color.red;
            if (GUILayout.Button("전체 진행 초기화", GUILayout.Height(25)))
            {
                if (EditorUtility.DisplayDialog("경고", "모든 진행 데이터를 초기화하시겠습니까?", "초기화", "취소"))
                {
                    metaManager.ResetProgress();
                }
            }
            GUI.color = Color.white;
        }
    }
}
#endif
