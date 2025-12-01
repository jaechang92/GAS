using UnityEngine;
using UnityEditor;
using GASPT.Forms;
using System.Collections.Generic;

namespace GASPT.Editor
{
    /// <summary>
    /// 폼 시스템 통합 테스트 에디터 윈도우
    /// Play 모드 없이 폼 시스템 기능 테스트
    /// </summary>
    public class FormSystemTestWindow : EditorWindow
    {
        // ====== 상태 ======

        private Vector2 scrollPosition;
        private FormData[] allForms;
        private int selectedPrimaryIndex;
        private int selectedSecondaryIndex;

        // 테스트용 폼 인스턴스
        private FormInstance testPrimaryForm;
        private FormInstance testSecondaryForm;

        // UI 상태
        private bool showFormList = true;
        private bool showInstanceTest = true;
        private bool showStatsTest = true;
        private bool showAwakeningTest = true;


        [MenuItem("Tools/GASPT/Forms/Form System Test Window", false, 200)]
        public static void ShowWindow()
        {
            var window = GetWindow<FormSystemTestWindow>("Form System Test");
            window.minSize = new Vector2(400, 600);
        }

        private void OnEnable()
        {
            LoadAllForms();
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            DrawHeader();
            EditorGUILayout.Space(10);

            DrawFormListSection();
            EditorGUILayout.Space(10);

            DrawInstanceTestSection();
            EditorGUILayout.Space(10);

            DrawStatsTestSection();
            EditorGUILayout.Space(10);

            DrawAwakeningTestSection();
            EditorGUILayout.Space(10);

            DrawQuickActions();

            EditorGUILayout.EndScrollView();
        }


        // ====== 섹션 그리기 ======

        private void DrawHeader()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label("폼 시스템 테스트 도구", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("새로고침", EditorStyles.toolbarButton))
            {
                LoadAllForms();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawFormListSection()
        {
            showFormList = EditorGUILayout.Foldout(showFormList, "📋 등록된 폼 목록", true);
            if (!showFormList) return;

            EditorGUI.indentLevel++;

            if (allForms == null || allForms.Length == 0)
            {
                EditorGUILayout.HelpBox("등록된 폼이 없습니다.\n'Create Default Forms' 메뉴를 실행하세요.", MessageType.Warning);

                if (GUILayout.Button("기본 폼 5종 생성"))
                {
                    FormAssetCreator.CreateDefaultForms();
                    LoadAllForms();
                }
            }
            else
            {
                EditorGUILayout.LabelField($"총 {allForms.Length}개 폼 등록됨");

                foreach (var form in allForms)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"• {form.formName}", GUILayout.Width(150));
                    EditorGUILayout.LabelField($"[{form.formType}]", GUILayout.Width(80));
                    EditorGUILayout.LabelField($"{form.baseRarity}", GUILayout.Width(80));

                    if (GUILayout.Button("선택", GUILayout.Width(50)))
                    {
                        Selection.activeObject = form;
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }

            EditorGUI.indentLevel--;
        }

        private void DrawInstanceTestSection()
        {
            showInstanceTest = EditorGUILayout.Foldout(showInstanceTest, "🎮 폼 인스턴스 테스트", true);
            if (!showInstanceTest) return;

            EditorGUI.indentLevel++;

            if (allForms == null || allForms.Length == 0)
            {
                EditorGUILayout.HelpBox("폼 데이터가 필요합니다.", MessageType.Info);
                EditorGUI.indentLevel--;
                return;
            }

            // 폼 선택
            string[] formNames = GetFormNames();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Primary 폼:", GUILayout.Width(100));
            selectedPrimaryIndex = EditorGUILayout.Popup(selectedPrimaryIndex, formNames);
            if (GUILayout.Button("생성", GUILayout.Width(50)))
            {
                testPrimaryForm = new FormInstance(allForms[selectedPrimaryIndex]);
                Debug.Log($"[Test] Primary 폼 생성: {testPrimaryForm}");
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Secondary 폼:", GUILayout.Width(100));
            selectedSecondaryIndex = EditorGUILayout.Popup(selectedSecondaryIndex, formNames);
            if (GUILayout.Button("생성", GUILayout.Width(50)))
            {
                testSecondaryForm = new FormInstance(allForms[selectedSecondaryIndex]);
                Debug.Log($"[Test] Secondary 폼 생성: {testSecondaryForm}");
            }
            EditorGUILayout.EndHorizontal();

            // 현재 인스턴스 정보
            EditorGUILayout.Space(5);
            DrawInstanceInfo("Primary", testPrimaryForm);
            DrawInstanceInfo("Secondary", testSecondaryForm);

            EditorGUI.indentLevel--;
        }

        private void DrawInstanceInfo(string label, FormInstance instance)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField($"{label} 인스턴스:", EditorStyles.boldLabel);

            if (instance == null)
            {
                EditorGUILayout.LabelField("  (없음)");
            }
            else
            {
                EditorGUILayout.LabelField($"  이름: {instance.FormName}");
                EditorGUILayout.LabelField($"  타입: {instance.FormType}");
                EditorGUILayout.LabelField($"  등급: {instance.CurrentRarity}");
                EditorGUILayout.LabelField($"  각성: {instance.AwakeningLevel}/{instance.MaxAwakeningLevel}");
                EditorGUILayout.LabelField($"  유효: {instance.IsValid()}");
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawStatsTestSection()
        {
            showStatsTest = EditorGUILayout.Foldout(showStatsTest, "📊 스탯 테스트", true);
            if (!showStatsTest) return;

            EditorGUI.indentLevel++;

            if (testPrimaryForm == null)
            {
                EditorGUILayout.HelpBox("Primary 폼 인스턴스를 먼저 생성하세요.", MessageType.Info);
                EditorGUI.indentLevel--;
                return;
            }

            var stats = testPrimaryForm.CurrentStats;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("현재 스탯:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"  공격력: {stats.attackPower:F1}");
            EditorGUILayout.LabelField($"  공격속도: {stats.attackSpeed:F2}");
            EditorGUILayout.LabelField($"  치명타 확률: {stats.criticalChance:P1}");
            EditorGUILayout.LabelField($"  치명타 데미지: {stats.criticalDamage:F2}x");
            EditorGUILayout.LabelField($"  최대 체력 보너스: {stats.maxHealthBonus:F0}");
            EditorGUILayout.LabelField($"  방어력: {stats.defense:F1}");
            EditorGUILayout.LabelField($"  이동속도: {stats.moveSpeed:F2}");
            EditorGUILayout.LabelField($"  점프력: {stats.jumpPower:F2}");
            EditorGUILayout.LabelField($"  쿨다운 감소: {stats.cooldownReduction:P0}");
            EditorGUILayout.LabelField($"  마나 회복: {stats.manaRegen:F2}");
            EditorGUILayout.EndVertical();

            // 스탯 연산 테스트
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("스탯 연산 테스트:", EditorStyles.boldLabel);

            if (GUILayout.Button("1.5배 스탯 계산"))
            {
                var multiplied = stats.ApplyMultiplier(1.5f);
                Debug.Log($"[Test] 1.5배 스탯: {multiplied}");
            }

            if (testSecondaryForm != null)
            {
                if (GUILayout.Button("Primary + Secondary 스탯 합산"))
                {
                    var combined = testPrimaryForm.CurrentStats + testSecondaryForm.CurrentStats;
                    Debug.Log($"[Test] 합산 스탯: {combined}");
                }
            }

            EditorGUI.indentLevel--;
        }

        private void DrawAwakeningTestSection()
        {
            showAwakeningTest = EditorGUILayout.Foldout(showAwakeningTest, "⭐ 각성 테스트", true);
            if (!showAwakeningTest) return;

            EditorGUI.indentLevel++;

            if (testPrimaryForm == null)
            {
                EditorGUILayout.HelpBox("Primary 폼 인스턴스를 먼저 생성하세요.", MessageType.Info);
                EditorGUI.indentLevel--;
                return;
            }

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("각성 상태:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"  현재 레벨: {testPrimaryForm.AwakeningLevel}");
            EditorGUILayout.LabelField($"  최대 레벨: {testPrimaryForm.MaxAwakeningLevel}");
            EditorGUILayout.LabelField($"  진행률: {testPrimaryForm.AwakeningProgress:P0}");
            EditorGUILayout.LabelField($"  최대 각성: {testPrimaryForm.IsMaxAwakening}");
            EditorGUILayout.EndVertical();

            // 각성 버튼
            EditorGUILayout.BeginHorizontal();
            GUI.enabled = !testPrimaryForm.IsMaxAwakening;
            if (GUILayout.Button("각성 실행"))
            {
                bool result = testPrimaryForm.Awaken();
                Debug.Log($"[Test] 각성 결과: {result}, 새 레벨: {testPrimaryForm.AwakeningLevel}");
            }
            GUI.enabled = true;

            if (GUILayout.Button("리셋 (재생성)"))
            {
                testPrimaryForm = new FormInstance(allForms[selectedPrimaryIndex]);
                Debug.Log($"[Test] 폼 리셋: {testPrimaryForm}");
            }
            EditorGUILayout.EndHorizontal();

            // 등급별 스탯 미리보기
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("각성 레벨별 스탯 미리보기:", EditorStyles.boldLabel);

            var data = testPrimaryForm.Data;
            if (data != null)
            {
                for (int i = 0; i <= data.maxAwakeningLevel; i++)
                {
                    var stats = data.GetStatsAtAwakening(i);
                    var rarity = data.GetRarityAtAwakening(i);
                    EditorGUILayout.LabelField($"  Lv.{i} [{rarity}]: 공격력 {stats.attackPower:F1}, 공속 {stats.attackSpeed:F2}");
                }
            }

            EditorGUI.indentLevel--;
        }

        private void DrawQuickActions()
        {
            EditorGUILayout.LabelField("⚡ 빠른 작업", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("폼 에셋 생성"))
            {
                FormAssetCreator.CreateDefaultForms();
                LoadAllForms();
            }

            if (GUILayout.Button("픽업 프리팹 생성"))
            {
                FormPickupCreator.CreateFormPickupPrefab();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("테스트 픽업 스폰"))
            {
                FormPickupCreator.SpawnTestFormPickup();
            }

            if (GUILayout.Button("전체 픽업 스폰"))
            {
                FormPickupCreator.SpawnAllFormPickupsInRow();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            if (GUILayout.Button("📁 폼 에셋 폴더 열기"))
            {
                string path = "Assets/Resources/Data/Forms";
                var obj = AssetDatabase.LoadAssetAtPath<Object>(path);
                if (obj != null)
                {
                    Selection.activeObject = obj;
                    EditorGUIUtility.PingObject(obj);
                }
            }
        }


        // ====== 유틸리티 ======

        private void LoadAllForms()
        {
            allForms = Resources.LoadAll<FormData>("Data/Forms");

            if (allForms.Length > 0)
            {
                selectedPrimaryIndex = Mathf.Clamp(selectedPrimaryIndex, 0, allForms.Length - 1);
                selectedSecondaryIndex = Mathf.Clamp(selectedSecondaryIndex, 0, allForms.Length - 1);
            }

            Repaint();
        }

        private string[] GetFormNames()
        {
            if (allForms == null) return new string[0];

            var names = new List<string>();
            foreach (var form in allForms)
            {
                names.Add($"{form.formName} ({form.formType})");
            }
            return names.ToArray();
        }
    }
}
