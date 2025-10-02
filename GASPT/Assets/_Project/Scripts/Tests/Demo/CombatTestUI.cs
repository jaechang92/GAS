using UnityEngine;
using Combat.Core;
using Core.Enums;
using System.Collections.Generic;

namespace Combat.Demo
{
    /// <summary>
    /// Combat 시스템 테스트용 UI 컨트롤러
    /// 다양한 데미지 타입과 설정을 테스트할 수 있는 UI 제공
    /// </summary>
    public class CombatTestUI : MonoBehaviour
    {
        [Header("참조")]
        [SerializeField] private GameObject player;
        [SerializeField] private GameObject enemy;

        [Header("UI 설정")]
        [SerializeField] private bool showUI = true;
        [SerializeField] private float uiScale = 1f;
        [SerializeField] private KeyCode toggleUIKey = KeyCode.Tab;

        [Header("테스트 설정")]
        [SerializeField] private float baseDamage = 25f;
        [SerializeField] private float healAmount = 30f;
        [SerializeField] private float radialDamageRadius = 5f;

        // UI 상태
        private Vector2 scrollPosition;
        private DamageType selectedDamageType = DamageType.Physical;
        private bool showAdvancedOptions = false;
        private float damageMultiplier = 1f;
        private bool enableCritical = false;
        private float criticalMultiplier = 2f;

        // 통계
        private int totalAttacks = 0;
        private int criticalHits = 0;
        private float totalDamageDealt = 0f;
        private List<string> combatLog = new List<string>();

        #region Unity 생명주기

        private void Start()
        {
            // 자동으로 플레이어/적 찾기
            if (player == null)
                player = GameObject.Find("Player");
            if (enemy == null)
                enemy = GameObject.Find("Enemy");
        }

        private void Update()
        {
            if (Input.GetKeyDown(toggleUIKey))
            {
                showUI = !showUI;
            }
        }

        #endregion

        #region GUI

        private void OnGUI()
        {
            if (!showUI) return;

            GUI.matrix = Matrix4x4.Scale(new Vector3(uiScale, uiScale, 1f));

            DrawMainPanel();
            DrawStatisticsPanel();
            DrawCombatLogPanel();
        }

        private void DrawMainPanel()
        {
            GUILayout.BeginArea(new Rect(10, 10, 400, 600));

            // 헤더
            GUILayout.Box("=== Combat System Test UI ===", GUILayout.Height(30));

            // 대상 정보
            DrawTargetInfo();

            GUILayout.Space(10);

            // 데미지 타입 선택
            DrawDamageTypeSelector();

            GUILayout.Space(10);

            // 데미지 설정
            DrawDamageSettings();

            GUILayout.Space(10);

            // 공격 버튼
            DrawAttackButtons();

            GUILayout.Space(10);

            // 회복 버튼
            DrawHealButtons();

            GUILayout.Space(10);

            // 특수 공격
            DrawSpecialAttacks();

            GUILayout.Space(10);

            // 유틸리티
            DrawUtilityButtons();

            GUILayout.EndArea();
        }

        private void DrawTargetInfo()
        {
            GUILayout.Label("=== 대상 정보 ===", GUI.skin.box);

            if (player != null)
            {
                var health = player.GetComponent<HealthSystem>();
                if (health != null)
                {
                    float percentage = health.HealthPercentage * 100f;
                    GUILayout.Label($"플레이어: {health.CurrentHealth:F0}/{health.MaxHealth:F0} ({percentage:F1}%)");
                    DrawHealthBar(health.HealthPercentage, Color.green);
                }
            }
            else
            {
                GUILayout.Label("플레이어: 없음");
            }

            if (enemy != null)
            {
                var health = enemy.GetComponent<HealthSystem>();
                if (health != null)
                {
                    float percentage = health.HealthPercentage * 100f;
                    GUILayout.Label($"적: {health.CurrentHealth:F0}/{health.MaxHealth:F0} ({percentage:F1}%)");
                    DrawHealthBar(health.HealthPercentage, Color.red);
                }
            }
            else
            {
                GUILayout.Label("적: 없음");
            }
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

        private void DrawDamageTypeSelector()
        {
            GUILayout.Label("=== 데미지 타입 ===", GUI.skin.box);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Physical"))
                selectedDamageType = DamageType.Physical;
            if (GUILayout.Button("Magical"))
                selectedDamageType = DamageType.Magical;
            if (GUILayout.Button("True"))
                selectedDamageType = DamageType.True;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Fire"))
                selectedDamageType = DamageType.Fire;
            if (GUILayout.Button("Ice"))
                selectedDamageType = DamageType.Ice;
            if (GUILayout.Button("Lightning"))
                selectedDamageType = DamageType.Lightning;
            GUILayout.EndHorizontal();

            GUILayout.Label($"선택된 타입: {selectedDamageType}", GUI.skin.box);
        }

        private void DrawDamageSettings()
        {
            showAdvancedOptions = GUILayout.Toggle(showAdvancedOptions, "고급 옵션 표시");

            if (showAdvancedOptions)
            {
                GUILayout.Label("=== 데미지 설정 ===", GUI.skin.box);

                GUILayout.BeginHorizontal();
                GUILayout.Label($"기본 데미지: {baseDamage:F0}");
                baseDamage = GUILayout.HorizontalSlider(baseDamage, 1f, 100f, GUILayout.Width(200));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label($"데미지 배율: {damageMultiplier:F2}x");
                damageMultiplier = GUILayout.HorizontalSlider(damageMultiplier, 0.1f, 5f, GUILayout.Width(200));
                GUILayout.EndHorizontal();

                enableCritical = GUILayout.Toggle(enableCritical, "크리티컬 활성화");

                if (enableCritical)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label($"크리티컬 배율: {criticalMultiplier:F1}x");
                    criticalMultiplier = GUILayout.HorizontalSlider(criticalMultiplier, 1.5f, 5f, GUILayout.Width(200));
                    GUILayout.EndHorizontal();
                }
            }
        }

        private void DrawAttackButtons()
        {
            GUILayout.Label("=== 공격 ===", GUI.skin.box);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("플레이어 → 적", GUILayout.Height(40)))
            {
                AttackTarget(player, enemy);
            }
            if (GUILayout.Button("적 → 플레이어", GUILayout.Height(40)))
            {
                AttackTarget(enemy, player);
            }
            GUILayout.EndHorizontal();
        }

        private void DrawHealButtons()
        {
            GUILayout.Label("=== 회복 ===", GUI.skin.box);

            GUILayout.BeginHorizontal();
            GUILayout.Label($"회복량: {healAmount:F0}");
            healAmount = GUILayout.HorizontalSlider(healAmount, 10f, 100f, GUILayout.Width(200));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button($"플레이어 회복 ({healAmount:F0})"))
            {
                HealTarget(player);
            }
            if (GUILayout.Button($"적 회복 ({healAmount:F0})"))
            {
                HealTarget(enemy);
            }
            GUILayout.EndHorizontal();
        }

        private void DrawSpecialAttacks()
        {
            GUILayout.Label("=== 특수 공격 ===", GUI.skin.box);

            if (GUILayout.Button($"범위 공격 (반경: {radialDamageRadius:F1}m)"))
            {
                RadialAttack();
            }

            GUILayout.BeginHorizontal();
            GUILayout.Label($"범위: {radialDamageRadius:F1}m");
            radialDamageRadius = GUILayout.HorizontalSlider(radialDamageRadius, 1f, 20f, GUILayout.Width(200));
            GUILayout.EndHorizontal();
        }

        private void DrawUtilityButtons()
        {
            GUILayout.Label("=== 유틸리티 ===", GUI.skin.box);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("통계 초기화"))
            {
                ResetStatistics();
            }
            if (GUILayout.Button("로그 지우기"))
            {
                combatLog.Clear();
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("플레이어 완전 회복"))
            {
                if (player != null)
                    player.GetComponent<HealthSystem>()?.HealFull();
            }
            if (GUILayout.Button("적 완전 회복"))
            {
                if (enemy != null)
                    enemy.GetComponent<HealthSystem>()?.HealFull();
            }
            GUILayout.EndHorizontal();
        }

        private void DrawStatisticsPanel()
        {
            GUILayout.BeginArea(new Rect(420, 10, 300, 200));
            GUILayout.Box("=== 통계 ===");

            GUILayout.Label($"총 공격 횟수: {totalAttacks}");
            GUILayout.Label($"크리티컬 히트: {criticalHits}");
            GUILayout.Label($"총 피해량: {totalDamageDealt:F1}");

            if (totalAttacks > 0)
            {
                float avgDamage = totalDamageDealt / totalAttacks;
                float critRate = (float)criticalHits / totalAttacks * 100f;
                GUILayout.Label($"평균 피해: {avgDamage:F1}");
                GUILayout.Label($"크리티컬 확률: {critRate:F1}%");
            }

            GUILayout.EndArea();
        }

        private void DrawCombatLogPanel()
        {
            GUILayout.BeginArea(new Rect(10, 620, 710, 200));
            GUILayout.Box("=== 전투 로그 ===");

            scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(160));

            for (int i = combatLog.Count - 1; i >= 0 && i >= combatLog.Count - 20; i--)
            {
                GUILayout.Label(combatLog[i]);
            }

            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        #endregion

        #region Combat Actions

        private void AttackTarget(GameObject attacker, GameObject target)
        {
            if (attacker == null || target == null)
            {
                LogCombat("공격 실패: 대상이 없음");
                return;
            }

            float finalDamage = baseDamage * damageMultiplier;
            DamageData damage;

            if (enableCritical)
            {
                damage = DamageData.CreateCritical(finalDamage, selectedDamageType, attacker, criticalMultiplier);
                criticalHits++;
            }
            else
            {
                damage = DamageData.Create(finalDamage, selectedDamageType, attacker);
            }

            bool success = DamageSystem.ApplyDamage(target, damage);

            if (success)
            {
                totalAttacks++;
                totalDamageDealt += finalDamage;

                string attackerName = attacker.name;
                string targetName = target.name;
                string critText = enableCritical ? " [크리티컬]" : "";

                LogCombat($"{attackerName} → {targetName}: {finalDamage:F1} {selectedDamageType} 데미지{critText}");
            }
        }

        private void HealTarget(GameObject target)
        {
            if (target == null)
            {
                LogCombat("회복 실패: 대상이 없음");
                return;
            }

            var health = target.GetComponent<HealthSystem>();
            if (health != null)
            {
                health.Heal(healAmount);
                LogCombat($"{target.name} 회복: {healAmount:F0}");
            }
        }

        private void RadialAttack()
        {
            if (player == null)
            {
                LogCombat("범위 공격 실패: 플레이어 없음");
                return;
            }

            float finalDamage = baseDamage * damageMultiplier;
            var damage = DamageData.Create(finalDamage, selectedDamageType, player);

            var hits = DamageSystem.ApplyRadialDamage(
                player.transform.position,
                radialDamageRadius,
                damage,
                LayerMask.GetMask("Default")
            );

            totalAttacks++;
            totalDamageDealt += finalDamage * hits.Count;

            LogCombat($"범위 공격: {hits.Count}개 타겟 타격, 총 {finalDamage * hits.Count:F1} 데미지");
        }

        #endregion

        #region Utilities

        private void LogCombat(string message)
        {
            string timeStamp = $"[{Time.time:F2}s]";
            combatLog.Add($"{timeStamp} {message}");

            if (combatLog.Count > 100)
            {
                combatLog.RemoveAt(0);
            }

            Debug.Log($"[CombatLog] {message}");
        }

        private void ResetStatistics()
        {
            totalAttacks = 0;
            criticalHits = 0;
            totalDamageDealt = 0f;
            LogCombat("통계 초기화됨");
        }

        #endregion
    }
}
