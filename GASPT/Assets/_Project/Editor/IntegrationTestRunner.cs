#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GASPT.Editor
{
    /// <summary>
    /// 020 통합 테스트 러너
    /// T024 관련 - 전체 시스템 통합 테스트
    /// </summary>
    public class IntegrationTestRunner : EditorWindow
    {
        // ====== 테스트 결과 ======

        private List<TestResult> testResults = new List<TestResult>();
        private Vector2 scrollPosition;
        private bool isRunning = false;


        // ====== 메뉴 ======

        [MenuItem("GASPT/Integration Test Runner")]
        public static void ShowWindow()
        {
            GetWindow<IntegrationTestRunner>("Integration Tests");
        }


        // ====== GUI ======

        private void OnGUI()
        {
            GUILayout.Label("GASPT 통합 테스트 러너", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // 테스트 실행 버튼
            EditorGUI.BeginDisabledGroup(isRunning);
            if (GUILayout.Button("모든 테스트 실행", GUILayout.Height(30)))
            {
                RunAllTests();
            }
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space();

            // 개별 테스트 버튼
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("스크립트 검증")) RunScriptValidationTests();
            if (GUILayout.Button("리소스 검증")) RunResourceValidationTests();
            if (GUILayout.Button("풀 시스템 검증")) RunPoolSystemTests();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            // 결과 요약
            if (testResults.Count > 0)
            {
                int passed = testResults.Count(t => t.passed);
                int failed = testResults.Count(t => !t.passed);

                EditorGUILayout.BeginHorizontal();
                GUI.color = Color.green;
                GUILayout.Label($"통과: {passed}", EditorStyles.boldLabel);
                GUI.color = Color.red;
                GUILayout.Label($"실패: {failed}", EditorStyles.boldLabel);
                GUI.color = Color.white;
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space();

                // 결과 목록
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

                foreach (var result in testResults)
                {
                    DrawTestResult(result);
                }

                EditorGUILayout.EndScrollView();
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("결과 초기화"))
            {
                testResults.Clear();
            }
        }

        private void DrawTestResult(TestResult result)
        {
            EditorGUILayout.BeginHorizontal("box");

            // 상태 아이콘
            GUI.color = result.passed ? Color.green : Color.red;
            GUILayout.Label(result.passed ? "✓" : "✗", GUILayout.Width(20));
            GUI.color = Color.white;

            // 테스트 이름
            GUILayout.Label(result.testName, GUILayout.Width(250));

            // 메시지
            if (!result.passed)
            {
                GUILayout.Label(result.message, EditorStyles.wordWrappedLabel);
            }
            else
            {
                GUILayout.Label("OK");
            }

            EditorGUILayout.EndHorizontal();
        }


        // ====== 테스트 실행 ======

        private void RunAllTests()
        {
            isRunning = true;
            testResults.Clear();

            try
            {
                RunScriptValidationTests();
                RunResourceValidationTests();
                RunPoolSystemTests();
                RunFormSystemTests();
                RunAudioSystemTests();
                RunStatusEffectTests();

                Debug.Log($"[IntegrationTest] 테스트 완료 - 통과: {testResults.Count(t => t.passed)}, 실패: {testResults.Count(t => !t.passed)}");
            }
            finally
            {
                isRunning = false;
            }
        }


        // ====== 스크립트 검증 ======

        private void RunScriptValidationTests()
        {
            // 필수 클래스 존재 확인
            ValidateClassExists("GASPT.Core.Pooling.PoolManager", "PoolManager");
            ValidateClassExists("GASPT.Core.Pooling.ObjectPool`1", "ObjectPool<T>");
            ValidateClassExists("GASPT.Audio.AudioManager", "AudioManager");
            ValidateClassExists("GASPT.StatusEffects.StatusEffectManager", "StatusEffectManager");
            ValidateClassExists("GASPT.Gameplay.Forms.FormSwapSystem", "FormSwapSystem");

            // 인터페이스 구현 확인
            ValidateInterfaceImplementation("GASPT.Core.Pooling.IPoolable", "IPoolable 구현체");
        }

        private void ValidateClassExists(string fullTypeName, string displayName)
        {
            Type type = FindType(fullTypeName);
            AddResult($"{displayName} 클래스 존재", type != null,
                type == null ? $"{fullTypeName} 클래스를 찾을 수 없습니다." : "");
        }

        private void ValidateInterfaceImplementation(string interfaceName, string displayName)
        {
            Type interfaceType = FindType(interfaceName);
            if (interfaceType == null)
            {
                AddResult($"{displayName} 인터페이스", false, $"{interfaceName}을 찾을 수 없습니다.");
                return;
            }

            // 구현체 찾기
            var implementations = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => {
                    try { return a.GetTypes(); }
                    catch { return new Type[0]; }
                })
                .Where(t => interfaceType.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                .ToList();

            AddResult($"{displayName}", implementations.Count > 0,
                implementations.Count == 0 ? "구현체 없음" : $"{implementations.Count}개 구현체");
        }


        // ====== 리소스 검증 ======

        private void RunResourceValidationTests()
        {
            // 프리팹 경로 확인
            ValidateResourcePath("Prefabs/Projectiles/FireballProjectile", "FireballProjectile 프리팹");
            ValidateResourcePath("Prefabs/Projectiles/MagicMissileProjectile", "MagicMissileProjectile 프리팹");
            ValidateResourcePath("Prefabs/Projectiles/IceLanceProjectile", "IceLanceProjectile 프리팹");
            ValidateResourcePath("Prefabs/Enemies/BasicMeleeEnemy", "BasicMeleeEnemy 프리팹");

            // VFX 폴더 확인
            ValidateFolderExists("Assets/_Project/Prefabs/VFX/StatusEffects", "StatusEffects VFX 폴더");

            // 데이터 폴더 확인
            ValidateFolderExists("Assets/_Project/Resources/Data", "Data 폴더");
        }

        private void ValidateResourcePath(string resourcePath, string displayName)
        {
            var resource = Resources.Load(resourcePath);
            AddResult($"{displayName}", resource != null,
                resource == null ? $"리소스 경로: {resourcePath}" : "");

            if (resource != null)
            {
                Resources.UnloadAsset(resource);
            }
        }

        private void ValidateFolderExists(string folderPath, string displayName)
        {
            bool exists = AssetDatabase.IsValidFolder(folderPath);
            AddResult($"{displayName}", exists,
                exists ? "" : $"폴더 없음: {folderPath}");
        }


        // ====== 풀 시스템 테스트 ======

        private void RunPoolSystemTests()
        {
            // PoolManager 싱글톤 확인
            Type poolManagerType = FindType("GASPT.Core.Pooling.PoolManager");
            if (poolManagerType == null)
            {
                AddResult("PoolManager 싱글톤", false, "PoolManager 타입 없음");
                return;
            }

            // Instance 프로퍼티 확인
            var instanceProp = poolManagerType.GetProperty("Instance",
                BindingFlags.Public | BindingFlags.Static);

            AddResult("PoolManager.Instance 프로퍼티", instanceProp != null, "");

            // HasPool 메서드 확인
            var hasPoolMethod = poolManagerType.GetMethod("HasPool",
                BindingFlags.Public | BindingFlags.Instance);

            AddResult("PoolManager.HasPool 메서드", hasPoolMethod != null, "");

            // CreatePool 메서드 확인
            var createPoolMethod = poolManagerType.GetMethod("CreatePool",
                BindingFlags.Public | BindingFlags.Instance);

            AddResult("PoolManager.CreatePool 메서드", createPoolMethod != null, "");
        }


        // ====== 폼 시스템 테스트 ======

        private void RunFormSystemTests()
        {
            // 폼 클래스 확인
            ValidateClassExists("GASPT.Gameplay.Forms.MageForm", "MageForm");
            ValidateClassExists("GASPT.Gameplay.Forms.WarriorForm", "WarriorForm");
            ValidateClassExists("GASPT.Gameplay.Forms.AssassinForm", "AssassinForm");
            ValidateClassExists("GASPT.Gameplay.Forms.FlameMageForm", "FlameMageForm");
            ValidateClassExists("GASPT.Gameplay.Forms.FrostMageForm", "FrostMageForm");

            // 어빌리티 클래스 확인
            ValidateClassExists("GASPT.Gameplay.Forms.FireStormAbility", "FireStormAbility");
            ValidateClassExists("GASPT.Gameplay.Forms.MeteorStrikeAbility", "MeteorStrikeAbility");
            ValidateClassExists("GASPT.Gameplay.Forms.IceLanceAbility", "IceLanceAbility");
            ValidateClassExists("GASPT.Gameplay.Forms.FrozenGroundAbility", "FrozenGroundAbility");
        }


        // ====== 오디오 시스템 테스트 ======

        private void RunAudioSystemTests()
        {
            ValidateClassExists("GASPT.Audio.AudioManager", "AudioManager");
            ValidateClassExists("GASPT.Audio.SFXPool", "SFXPool");
            ValidateClassExists("GASPT.Audio.CombatAudioBridge", "CombatAudioBridge");
            ValidateClassExists("GASPT.Audio.UISoundPlayer", "UISoundPlayer");

            // 오디오 데이터 ScriptableObject
            ValidateClassExists("GASPT.Data.CombatSoundData", "CombatSoundData");
            ValidateClassExists("GASPT.Data.UISoundData", "UISoundData");
        }


        // ====== 상태 효과 테스트 ======

        private void RunStatusEffectTests()
        {
            ValidateClassExists("GASPT.StatusEffects.StatusEffectManager", "StatusEffectManager");
            ValidateClassExists("GASPT.StatusEffects.StatusEffectVisual", "StatusEffectVisual");
            ValidateClassExists("GASPT.StatusEffects.StatusEffectTarget", "StatusEffectTarget");

            // 상태 효과 데이터
            ValidateClassExists("GASPT.Data.StatusEffectVisualConfig", "StatusEffectVisualConfig");
        }


        // ====== 유틸리티 ======

        private Type FindType(string fullName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    var type = assembly.GetType(fullName);
                    if (type != null) return type;
                }
                catch { }
            }
            return null;
        }

        private void AddResult(string testName, bool passed, string message = "")
        {
            testResults.Add(new TestResult
            {
                testName = testName,
                passed = passed,
                message = message
            });
        }


        // ====== 결과 구조체 ======

        private struct TestResult
        {
            public string testName;
            public bool passed;
            public string message;
        }
    }
}
#endif
