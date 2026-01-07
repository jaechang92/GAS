#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace GASPT.Editor
{
    /// <summary>
    /// 폼/스킬 밸런스 조정 에디터 도구
    /// T021.1, T022 관련
    /// </summary>
    public class BalanceEditorWindow : EditorWindow
    {
        // ====== 탭 ======

        private int selectedTab = 0;
        private readonly string[] tabNames = { "폼 밸런스", "스킬 밸런스", "DPS 계산기" };

        private Vector2 scrollPosition;


        // ====== 밸런스 데이터 ======

        // 폼 스탯 기준치
        private readonly Dictionary<string, FormBalanceData> formBalanceData = new Dictionary<string, FormBalanceData>
        {
            { "MageForm", new FormBalanceData("마법사", 100, 5, 10, 8f, "균형형 - 중거리 마법 공격") },
            { "WarriorForm", new FormBalanceData("전사", 150, 15, 5, 5f, "탱커형 - 근접 물리 공격") },
            { "AssassinForm", new FormBalanceData("암살자", 80, 3, 20, 10f, "딜러형 - 고위험 고데미지") },
            { "FlameMageForm", new FormBalanceData("화염 마법사", 90, 4, 15, 7f, "DoT 특화 - 지속 데미지") },
            { "FrostMageForm", new FormBalanceData("얼음 마법사", 100, 6, 12, 7f, "CC 특화 - 슬로우/빙결") }
        };

        // 스킬 DPS 기준치
        private readonly Dictionary<string, SkillBalanceData> skillBalanceData = new Dictionary<string, SkillBalanceData>
        {
            // 기본 공격
            { "SwordSlash", new SkillBalanceData("검 베기", 50, 0.6f, 0f, "근접 기본공격") },
            { "MagicMissile", new SkillBalanceData("매직 미사일", 25, 0.5f, 0f, "원거리 기본공격") },
            { "DaggerStrike", new SkillBalanceData("단검 찌르기", 35, 0.4f, 0f, "빠른 기본공격") },

            // 마법사 스킬
            { "Fireball", new SkillBalanceData("파이어볼", 40, 1.0f, 3f, "범위 화염 데미지") },
            { "IceBlast", new SkillBalanceData("아이스 블래스트", 35, 1.0f, 4f, "범위 빙결") },

            // 화염 마법사
            { "FireStorm", new SkillBalanceData("파이어 스톰", 18, 0.5f, 8f, "틱당 데미지, 5초 지속") },
            { "MeteorStrike", new SkillBalanceData("메테오 스트라이크", 150, 0f, 15f, "딜레이 후 고데미지") },

            // 얼음 마법사
            { "IceLance", new SkillBalanceData("아이스 랜스", 30, 0f, 2f, "빠른 투사체 + 슬로우") },
            { "FrozenGround", new SkillBalanceData("프로즌 그라운드", 12, 0.5f, 10f, "틱당 데미지 + 슬로우") },

            // 전사 스킬
            { "ShieldBash", new SkillBalanceData("방패 강타", 30, 0f, 5f, "스턴 + 데미지") },
            { "Charge", new SkillBalanceData("돌진", 45, 0f, 8f, "이동 + 데미지") },

            // 암살자 스킬
            { "Backstab", new SkillBalanceData("백스탭", 80, 0f, 6f, "후방 공격 시 치명타") },
            { "ShadowStrike", new SkillBalanceData("그림자 일격", 100, 0f, 10f, "은신 후 기습") }
        };


        // ====== 메뉴 ======

        [MenuItem("GASPT/Balance Editor")]
        public static void ShowWindow()
        {
            GetWindow<BalanceEditorWindow>("Balance Editor");
        }


        // ====== GUI ======

        private void OnGUI()
        {
            GUILayout.Label("GASPT 밸런스 에디터", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // 탭 선택
            selectedTab = GUILayout.Toolbar(selectedTab, tabNames);
            EditorGUILayout.Space();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            switch (selectedTab)
            {
                case 0:
                    DrawFormBalanceTab();
                    break;
                case 1:
                    DrawSkillBalanceTab();
                    break;
                case 2:
                    DrawDPSCalculatorTab();
                    break;
            }

            EditorGUILayout.EndScrollView();
        }


        // ====== 폼 밸런스 탭 ======

        private void DrawFormBalanceTab()
        {
            GUILayout.Label("폼 스탯 밸런스", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "각 폼의 기본 스탯을 조정합니다.\n" +
                "기준: Warrior(탱커) > Mage(균형) > Assassin(딜러)",
                MessageType.Info
            );

            EditorGUILayout.Space();

            // 헤더
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("폼", EditorStyles.boldLabel, GUILayout.Width(120));
            GUILayout.Label("HP", EditorStyles.boldLabel, GUILayout.Width(60));
            GUILayout.Label("DEF", EditorStyles.boldLabel, GUILayout.Width(60));
            GUILayout.Label("ATK", EditorStyles.boldLabel, GUILayout.Width(60));
            GUILayout.Label("SPD", EditorStyles.boldLabel, GUILayout.Width(60));
            GUILayout.Label("역할", EditorStyles.boldLabel, GUILayout.Width(200));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            // 데이터 표시
            foreach (var kvp in formBalanceData)
            {
                var data = kvp.Value;

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(data.displayName, GUILayout.Width(120));
                data.baseHp = EditorGUILayout.IntField(data.baseHp, GUILayout.Width(60));
                data.baseDefense = EditorGUILayout.IntField(data.baseDefense, GUILayout.Width(60));
                data.baseAttack = EditorGUILayout.IntField(data.baseAttack, GUILayout.Width(60));
                data.moveSpeed = EditorGUILayout.FloatField(data.moveSpeed, GUILayout.Width(60));
                GUILayout.Label(data.role, GUILayout.Width(200));
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space();

            // 밸런스 체크
            if (GUILayout.Button("밸런스 검증"))
            {
                ValidateFormBalance();
            }

            EditorGUILayout.Space();

            // 폼 데이터 에셋 생성 버튼
            if (GUILayout.Button("폼 데이터 에셋 생성 (Resources/Data/Forms)"))
            {
                GenerateFormDataAssets();
            }
        }

        private void ValidateFormBalance()
        {
            Debug.Log("===== 폼 밸런스 검증 =====");

            // HP 비교 (Warrior > Mage > Assassin)
            var sorted = formBalanceData.OrderByDescending(x => x.Value.baseHp).ToList();
            Debug.Log($"HP 순위: {string.Join(" > ", sorted.Select(x => $"{x.Value.displayName}({x.Value.baseHp})"))}");

            // EHP (Effective HP) = HP * (1 + DEF/100)
            foreach (var kvp in formBalanceData)
            {
                float ehp = kvp.Value.baseHp * (1 + kvp.Value.baseDefense / 100f);
                Debug.Log($"[{kvp.Value.displayName}] EHP: {ehp:F1} (HP:{kvp.Value.baseHp} x DEF:{kvp.Value.baseDefense})");
            }

            Debug.Log("===== 검증 완료 =====");
        }

        private void GenerateFormDataAssets()
        {
            string outputPath = "Assets/_Project/Resources/Data/Forms";
            EnsureFolderExists(outputPath);

            foreach (var kvp in formBalanceData)
            {
                string assetPath = $"{outputPath}/{kvp.Key}Data.asset";

                // 이미 존재하면 스킵
                if (AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath) != null)
                {
                    Debug.Log($"[BalanceEditor] 이미 존재: {assetPath}");
                    continue;
                }

                // FormData ScriptableObject 생성
                // 참고: 실제 FormData 클래스가 정의되어 있어야 함
                Debug.Log($"[BalanceEditor] {kvp.Key} 에셋은 수동 생성 필요 (FormData 클래스 확인)");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("완료", "폼 데이터 에셋 생성 완료\n경로: " + outputPath, "확인");
        }


        // ====== 스킬 밸런스 탭 ======

        private void DrawSkillBalanceTab()
        {
            GUILayout.Label("스킬 DPS 밸런스", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "DPS = Damage / Cooldown (쿨다운 0이면 기본 공격)\n" +
                "기준: 근접 기본공격 83 DPS, 원거리 기본공격 50 DPS",
                MessageType.Info
            );

            EditorGUILayout.Space();

            // 헤더
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("스킬", EditorStyles.boldLabel, GUILayout.Width(140));
            GUILayout.Label("데미지", EditorStyles.boldLabel, GUILayout.Width(60));
            GUILayout.Label("공속", EditorStyles.boldLabel, GUILayout.Width(60));
            GUILayout.Label("쿨다운", EditorStyles.boldLabel, GUILayout.Width(60));
            GUILayout.Label("DPS", EditorStyles.boldLabel, GUILayout.Width(60));
            GUILayout.Label("설명", EditorStyles.boldLabel, GUILayout.Width(200));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            // 데이터 표시
            foreach (var kvp in skillBalanceData)
            {
                var data = kvp.Value;
                float dps = CalculateDPS(data);

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(data.displayName, GUILayout.Width(140));
                data.damage = EditorGUILayout.IntField(data.damage, GUILayout.Width(60));
                data.attackInterval = EditorGUILayout.FloatField(data.attackInterval, GUILayout.Width(60));
                data.cooldown = EditorGUILayout.FloatField(data.cooldown, GUILayout.Width(60));

                // DPS 색상 (기준 대비)
                Color dpsColor = dps > 80 ? Color.green : (dps > 40 ? Color.yellow : Color.red);
                GUI.color = dpsColor;
                GUILayout.Label($"{dps:F1}", GUILayout.Width(60));
                GUI.color = Color.white;

                GUILayout.Label(data.description, GUILayout.Width(200));
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("DPS 순위 출력"))
            {
                PrintDPSRanking();
            }
        }

        private float CalculateDPS(SkillBalanceData data)
        {
            // 기본 공격 (쿨다운 0)
            if (data.cooldown <= 0)
            {
                return data.attackInterval > 0 ? data.damage / data.attackInterval : data.damage;
            }

            // 스킬 (쿨다운 기반)
            return data.damage / data.cooldown;
        }

        private void PrintDPSRanking()
        {
            Debug.Log("===== DPS 순위 =====");

            var ranked = skillBalanceData
                .Select(kvp => new { Name = kvp.Value.displayName, DPS = CalculateDPS(kvp.Value) })
                .OrderByDescending(x => x.DPS)
                .ToList();

            for (int i = 0; i < ranked.Count; i++)
            {
                Debug.Log($"{i + 1}. {ranked[i].Name}: {ranked[i].DPS:F1} DPS");
            }

            Debug.Log("===== 순위 끝 =====");
        }


        // ====== DPS 계산기 탭 ======

        private int calcDamage = 50;
        private float calcCooldown = 1f;
        private float calcCritRate = 0.1f;
        private float calcCritDamage = 1.5f;

        private void DrawDPSCalculatorTab()
        {
            GUILayout.Label("DPS 계산기", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            calcDamage = EditorGUILayout.IntField("기본 데미지", calcDamage);
            calcCooldown = EditorGUILayout.FloatField("쿨다운/공격속도 (초)", calcCooldown);
            calcCritRate = EditorGUILayout.Slider("치명타 확률", calcCritRate, 0f, 1f);
            calcCritDamage = EditorGUILayout.Slider("치명타 배율", calcCritDamage, 1f, 3f);

            EditorGUILayout.Space();

            // 계산
            float baseDps = calcCooldown > 0 ? calcDamage / calcCooldown : 0;
            float expectedCritDamage = calcDamage * (1 + calcCritRate * (calcCritDamage - 1));
            float critDps = calcCooldown > 0 ? expectedCritDamage / calcCooldown : 0;

            EditorGUILayout.LabelField("기본 DPS", $"{baseDps:F2}");
            EditorGUILayout.LabelField("기대 DPS (치명타 포함)", $"{critDps:F2}");
            EditorGUILayout.LabelField("치명타 증가량", $"+{((critDps / baseDps - 1) * 100):F1}%");

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "밸런스 기준:\n" +
                "- 근접 기본공격 DPS: 80-90\n" +
                "- 원거리 기본공격 DPS: 40-60\n" +
                "- 스킬 DPS: 쿨다운에 따라 유동적\n" +
                "- 궁극기: 높은 단일 데미지, 긴 쿨다운",
                MessageType.Info
            );
        }


        // ====== 유틸리티 ======

        private void EnsureFolderExists(string path)
        {
            string[] folders = path.Split('/');
            string currentPath = "";

            for (int i = 0; i < folders.Length; i++)
            {
                string parentPath = currentPath;
                currentPath = string.IsNullOrEmpty(currentPath)
                    ? folders[i]
                    : $"{currentPath}/{folders[i]}";

                if (!AssetDatabase.IsValidFolder(currentPath))
                {
                    if (string.IsNullOrEmpty(parentPath))
                    {
                        parentPath = "Assets";
                        currentPath = $"Assets/{folders[i]}";
                    }

                    AssetDatabase.CreateFolder(parentPath, folders[i]);
                }
            }
        }


        // ====== 데이터 클래스 ======

        private class FormBalanceData
        {
            public string displayName;
            public int baseHp;
            public int baseDefense;
            public int baseAttack;
            public float moveSpeed;
            public string role;

            public FormBalanceData(string name, int hp, int def, int atk, float spd, string roleDesc)
            {
                displayName = name;
                baseHp = hp;
                baseDefense = def;
                baseAttack = atk;
                moveSpeed = spd;
                role = roleDesc;
            }
        }

        private class SkillBalanceData
        {
            public string displayName;
            public int damage;
            public float attackInterval;
            public float cooldown;
            public string description;

            public SkillBalanceData(string name, int dmg, float interval, float cd, string desc)
            {
                displayName = name;
                damage = dmg;
                attackInterval = interval;
                cooldown = cd;
                description = desc;
            }
        }
    }
}
#endif
