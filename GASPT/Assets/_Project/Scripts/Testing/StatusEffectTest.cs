using UnityEngine;
using GASPT.Data;
using GASPT.Stats;
using GASPT.Gameplay.Enemies;
using GASPT.Core.Enums;

namespace GASPT.StatusEffects
{
    /// <summary>
    /// StatusEffect 시스템 테스트 스크립트
    /// Context Menu를 통해 다양한 효과를 테스트할 수 있음
    /// </summary>
    public class StatusEffectTest : MonoBehaviour
    {
        [Header("Test Targets")]
        [SerializeField] [Tooltip("테스트 대상 플레이어")]
        private PlayerStats playerStats;

        [SerializeField] [Tooltip("테스트 대상 적")]
        private Enemy enemy;


        [Header("Test Settings")]
        [SerializeField] [Tooltip("버프 효과값")]
        private float buffValue = 10f;

        [SerializeField] [Tooltip("디버프 효과값")]
        private float debuffValue = 5f;

        [SerializeField] [Tooltip("DoT 틱당 데미지")]
        private float dotTickDamage = 5f;

        [SerializeField] [Tooltip("효과 지속 시간 (초)")]
        private float duration = 5f;

        [SerializeField] [Tooltip("DoT 틱 간격 (초)")]
        private float tickInterval = 1f;


        // ====== Unity 생명주기 ======

        private void Start()
        {
            FindTestTargets();
        }


        // ====== 대상 찾기 ======

        /// <summary>
        /// 테스트 대상 자동 찾기
        /// </summary>
        private void FindTestTargets()
        {
            if (playerStats == null)
            {
                playerStats = FindAnyObjectByType<PlayerStats>();

                if (playerStats != null)
                {
                    Debug.Log($"[StatusEffectTest] PlayerStats 자동 할당: {playerStats.gameObject.name}");
                }
                else
                {
                    Debug.LogWarning("[StatusEffectTest] PlayerStats를 찾을 수 없습니다.");
                }
            }

            if (enemy == null)
            {
                enemy = FindAnyObjectByType<Enemy>();

                if (enemy != null)
                {
                    Debug.Log($"[StatusEffectTest] Enemy 자동 할당: {enemy.gameObject.name}");
                }
                else
                {
                    Debug.LogWarning("[StatusEffectTest] Enemy를 찾을 수 없습니다.");
                }
            }
        }


        // ====== 버프 테스트 ======

        /// <summary>
        /// 플레이어에게 AttackUp 버프 적용
        /// </summary>
        [ContextMenu("Test/Player/Apply AttackUp")]
        private void TestPlayerAttackUp()
        {
            if (!ValidatePlayer()) return;

            StatusEffectData data = CreateEffectData(StatusEffectType.AttackUp, "공격력 증가", buffValue, duration);
            StatusEffectManager.Instance.ApplyEffect(playerStats.gameObject, data);

            Debug.Log($"[Test] 플레이어 AttackUp 적용 - 공격력: {playerStats.Attack}");
        }

        /// <summary>
        /// 플레이어에게 DefenseUp 버프 적용
        /// </summary>
        [ContextMenu("Test/Player/Apply DefenseUp")]
        private void TestPlayerDefenseUp()
        {
            if (!ValidatePlayer()) return;

            StatusEffectData data = CreateEffectData(StatusEffectType.DefenseUp, "방어력 증가", buffValue, duration);
            StatusEffectManager.Instance.ApplyEffect(playerStats.gameObject, data);

            Debug.Log($"[Test] 플레이어 DefenseUp 적용 - 방어력: {playerStats.Defense}");
        }


        // ====== 디버프 테스트 ======

        /// <summary>
        /// 플레이어에게 AttackDown 디버프 적용
        /// </summary>
        [ContextMenu("Test/Player/Apply AttackDown")]
        private void TestPlayerAttackDown()
        {
            if (!ValidatePlayer()) return;

            StatusEffectData data = CreateEffectData(StatusEffectType.AttackDown, "공격력 감소", debuffValue, duration, false);
            StatusEffectManager.Instance.ApplyEffect(playerStats.gameObject, data);

            Debug.Log($"[Test] 플레이어 AttackDown 적용 - 공격력: {playerStats.Attack}");
        }

        /// <summary>
        /// 플레이어에게 DefenseDown 디버프 적용
        /// </summary>
        [ContextMenu("Test/Player/Apply DefenseDown")]
        private void TestPlayerDefenseDown()
        {
            if (!ValidatePlayer()) return;

            StatusEffectData data = CreateEffectData(StatusEffectType.DefenseDown, "방어력 감소", debuffValue, duration, false);
            StatusEffectManager.Instance.ApplyEffect(playerStats.gameObject, data);

            Debug.Log($"[Test] 플레이어 DefenseDown 적용 - 방어력: {playerStats.Defense}");
        }


        // ====== DoT 테스트 ======

        /// <summary>
        /// 플레이어에게 Poison 적용
        /// </summary>
        [ContextMenu("Test/Player/Apply Poison")]
        private void TestPlayerPoison()
        {
            if (!ValidatePlayer()) return;

            StatusEffectData data = CreateEffectData(StatusEffectType.Poison, "독", dotTickDamage, duration, false, tickInterval);
            StatusEffectManager.Instance.ApplyEffect(playerStats.gameObject, data);

            Debug.Log($"[Test] 플레이어 Poison 적용 - {tickInterval}초마다 {dotTickDamage} 데미지");
        }

        /// <summary>
        /// 플레이어에게 Burn 적용
        /// </summary>
        [ContextMenu("Test/Player/Apply Burn")]
        private void TestPlayerBurn()
        {
            if (!ValidatePlayer()) return;

            StatusEffectData data = CreateEffectData(StatusEffectType.Burn, "화상", dotTickDamage, duration, false, tickInterval);
            StatusEffectManager.Instance.ApplyEffect(playerStats.gameObject, data);

            Debug.Log($"[Test] 플레이어 Burn 적용 - {tickInterval}초마다 {dotTickDamage} 데미지");
        }

        /// <summary>
        /// 플레이어에게 Bleed 적용
        /// </summary>
        [ContextMenu("Test/Player/Apply Bleed")]
        private void TestPlayerBleed()
        {
            if (!ValidatePlayer()) return;

            StatusEffectData data = CreateEffectData(StatusEffectType.Bleed, "출혈", dotTickDamage, duration, false, tickInterval);
            StatusEffectManager.Instance.ApplyEffect(playerStats.gameObject, data);

            Debug.Log($"[Test] 플레이어 Bleed 적용 - {tickInterval}초마다 {dotTickDamage} 데미지");
        }


        // ====== 회복 테스트 ======

        /// <summary>
        /// 플레이어에게 Regeneration 적용
        /// </summary>
        [ContextMenu("Test/Player/Apply Regeneration")]
        private void TestPlayerRegeneration()
        {
            if (!ValidatePlayer()) return;

            StatusEffectData data = CreateEffectData(StatusEffectType.Regeneration, "재생", dotTickDamage, duration, true, tickInterval);
            StatusEffectManager.Instance.ApplyEffect(playerStats.gameObject, data);

            Debug.Log($"[Test] 플레이어 Regeneration 적용 - {tickInterval}초마다 {dotTickDamage} 회복");
        }


        // ====== 적 테스트 ======

        /// <summary>
        /// 적에게 AttackDown 디버프 적용
        /// </summary>
        [ContextMenu("Test/Enemy/Apply AttackDown")]
        private void TestEnemyAttackDown()
        {
            if (!ValidateEnemy()) return;

            StatusEffectData data = CreateEffectData(StatusEffectType.AttackDown, "공격력 감소", debuffValue, duration, false);
            StatusEffectManager.Instance.ApplyEffect(enemy.gameObject, data);

            Debug.Log($"[Test] 적 AttackDown 적용 - 공격력: {enemy.Attack}");
        }

        /// <summary>
        /// 적에게 Poison 적용
        /// </summary>
        [ContextMenu("Test/Enemy/Apply Poison")]
        private void TestEnemyPoison()
        {
            if (!ValidateEnemy()) return;

            StatusEffectData data = CreateEffectData(StatusEffectType.Poison, "독", dotTickDamage, duration, false, tickInterval);
            StatusEffectManager.Instance.ApplyEffect(enemy.gameObject, data);

            Debug.Log($"[Test] 적 Poison 적용 - {tickInterval}초마다 {dotTickDamage} 데미지");
        }


        // ====== 복합 테스트 ======

        /// <summary>
        /// 플레이어에게 여러 버프 동시 적용
        /// </summary>
        [ContextMenu("Test/Player/Apply Multiple Buffs")]
        private void TestPlayerMultipleBuffs()
        {
            if (!ValidatePlayer()) return;

            StatusEffectData attackUp = CreateEffectData(StatusEffectType.AttackUp, "공격력 증가", buffValue, duration);
            StatusEffectData defenseUp = CreateEffectData(StatusEffectType.DefenseUp, "방어력 증가", buffValue, duration);

            StatusEffectManager.Instance.ApplyEffect(playerStats.gameObject, attackUp);
            StatusEffectManager.Instance.ApplyEffect(playerStats.gameObject, defenseUp);

            Debug.Log($"[Test] 플레이어 다중 버프 적용 - 공격력: {playerStats.Attack}, 방어력: {playerStats.Defense}");
        }

        /// <summary>
        /// 플레이어에게 여러 디버프 동시 적용
        /// </summary>
        [ContextMenu("Test/Player/Apply Multiple Debuffs")]
        private void TestPlayerMultipleDebuffs()
        {
            if (!ValidatePlayer()) return;

            StatusEffectData attackDown = CreateEffectData(StatusEffectType.AttackDown, "공격력 감소", debuffValue, duration, false);
            StatusEffectData defenseDown = CreateEffectData(StatusEffectType.DefenseDown, "방어력 감소", debuffValue, duration, false);

            StatusEffectManager.Instance.ApplyEffect(playerStats.gameObject, attackDown);
            StatusEffectManager.Instance.ApplyEffect(playerStats.gameObject, defenseDown);

            Debug.Log($"[Test] 플레이어 다중 디버프 적용 - 공격력: {playerStats.Attack}, 방어력: {playerStats.Defense}");
        }


        // ====== 중첩 테스트 ======

        /// <summary>
        /// AttackUp 중첩 테스트 (3회)
        /// </summary>
        [ContextMenu("Test/Player/Stack AttackUp x3")]
        private void TestPlayerStackAttackUp()
        {
            if (!ValidatePlayer()) return;

            StatusEffectData data = CreateEffectData(StatusEffectType.AttackUp, "공격력 증가", buffValue, duration, true, 0f, 3);

            for (int i = 0; i < 3; i++)
            {
                StatusEffectManager.Instance.ApplyEffect(playerStats.gameObject, data);
            }

            Debug.Log($"[Test] 플레이어 AttackUp x3 중첩 - 공격력: {playerStats.Attack}");
        }


        // ====== 효과 제거 테스트 ======

        /// <summary>
        /// 플레이어의 모든 효과 제거
        /// </summary>
        [ContextMenu("Test/Player/Remove All Effects")]
        private void TestPlayerRemoveAllEffects()
        {
            if (!ValidatePlayer()) return;

            StatusEffectManager.Instance.RemoveAllEffects(playerStats.gameObject);

            Debug.Log($"[Test] 플레이어 모든 효과 제거 - 공격력: {playerStats.Attack}, 방어력: {playerStats.Defense}");
        }

        /// <summary>
        /// 적의 모든 효과 제거
        /// </summary>
        [ContextMenu("Test/Enemy/Remove All Effects")]
        private void TestEnemyRemoveAllEffects()
        {
            if (!ValidateEnemy()) return;

            StatusEffectManager.Instance.RemoveAllEffects(enemy.gameObject);

            Debug.Log($"[Test] 적 모든 효과 제거 - 공격력: {enemy.Attack}");
        }


        // ====== 상태 조회 테스트 ======

        /// <summary>
        /// 플레이어의 활성 효과 출력
        /// </summary>
        [ContextMenu("Test/Player/Print Active Effects")]
        private void TestPlayerPrintActiveEffects()
        {
            if (!ValidatePlayer()) return;

            var effects = StatusEffectManager.Instance.GetActiveEffects(playerStats.gameObject);

            Debug.Log("========== 플레이어 활성 효과 ==========");
            Debug.Log($"총 {effects.Count}개의 효과:");

            foreach (var effect in effects)
            {
                Debug.Log($"  - {effect.ToString()}");
            }

            Debug.Log("======================================");
        }

        /// <summary>
        /// 적의 활성 효과 출력
        /// </summary>
        [ContextMenu("Test/Enemy/Print Active Effects")]
        private void TestEnemyPrintActiveEffects()
        {
            if (!ValidateEnemy()) return;

            var effects = StatusEffectManager.Instance.GetActiveEffects(enemy.gameObject);

            Debug.Log("========== 적 활성 효과 ==========");
            Debug.Log($"총 {effects.Count}개의 효과:");

            foreach (var effect in effects)
            {
                Debug.Log($"  - {effect.ToString()}");
            }

            Debug.Log("===================================");
        }


        // ====== StatusEffectManager 테스트 ======

        /// <summary>
        /// StatusEffectManager 모든 활성 효과 출력
        /// </summary>
        [ContextMenu("Test/Manager/Print All Active Effects")]
        private void TestManagerPrintAllEffects()
        {
            if (StatusEffectManager.HasInstance)
            {
                // StatusEffectManager의 Context Menu 호출
                StatusEffectManager.Instance.SendMessage("PrintAllActiveEffects");
            }
            else
            {
                Debug.LogError("[Test] StatusEffectManager가 존재하지 않습니다.");
            }
        }


        // ====== 시각 효과 테스트 ======

        /// <summary>
        /// 플레이어에게 Slow 적용 (시각 효과 테스트)
        /// </summary>
        [ContextMenu("Test/Visual/Player Slow")]
        private void TestPlayerSlow()
        {
            if (!ValidatePlayer()) return;

            StatusEffectData data = CreateEffectData(StatusEffectType.Slow, "감속", debuffValue, duration, false);
            StatusEffectManager.Instance.ApplyEffect(playerStats.gameObject, data);

            Debug.Log($"[Test Visual] 플레이어 Slow 적용 - 파란색 오버레이 + VFX");
        }

        /// <summary>
        /// 플레이어에게 Stun 적용 (시각 효과 테스트)
        /// </summary>
        [ContextMenu("Test/Visual/Player Stun")]
        private void TestPlayerStun()
        {
            if (!ValidatePlayer()) return;

            StatusEffectData data = CreateEffectData(StatusEffectType.Stun, "기절", 0f, duration, false);
            StatusEffectManager.Instance.ApplyEffect(playerStats.gameObject, data);

            Debug.Log($"[Test Visual] 플레이어 Stun 적용 - 노란색 오버레이 + VFX");
        }

        /// <summary>
        /// 적에게 Burn + Slow 동시 적용 (복합 시각 효과 테스트)
        /// </summary>
        [ContextMenu("Test/Visual/Enemy Burn+Slow")]
        private void TestEnemyBurnAndSlow()
        {
            if (!ValidateEnemy()) return;

            StatusEffectData burnData = CreateEffectData(StatusEffectType.Burn, "화상", dotTickDamage, duration, false, tickInterval);
            StatusEffectData slowData = CreateEffectData(StatusEffectType.Slow, "감속", debuffValue, duration, false);

            StatusEffectManager.Instance.ApplyEffect(enemy.gameObject, burnData);
            StatusEffectManager.Instance.ApplyEffect(enemy.gameObject, slowData);

            Debug.Log($"[Test Visual] 적 Burn+Slow 동시 적용 - 색상 블렌딩 확인");
        }

        /// <summary>
        /// 플레이어 StatusEffectVisual 상태 출력
        /// </summary>
        [ContextMenu("Test/Visual/Print Player Visual Status")]
        private void TestPrintPlayerVisualStatus()
        {
            if (!ValidatePlayer()) return;

            var visual = playerStats.GetComponent<StatusEffectVisual>();
            if (visual != null)
            {
                visual.SendMessage("DebugPrintStatus", SendMessageOptions.DontRequireReceiver);
            }
            else
            {
                Debug.LogWarning("[Test Visual] 플레이어에 StatusEffectVisual 컴포넌트가 없습니다.");
            }
        }

        /// <summary>
        /// 적 StatusEffectVisual 상태 출력
        /// </summary>
        [ContextMenu("Test/Visual/Print Enemy Visual Status")]
        private void TestPrintEnemyVisualStatus()
        {
            if (!ValidateEnemy()) return;

            var visual = enemy.GetComponent<StatusEffectVisual>();
            if (visual != null)
            {
                visual.SendMessage("DebugPrintStatus", SendMessageOptions.DontRequireReceiver);
            }
            else
            {
                Debug.LogWarning("[Test Visual] 적에 StatusEffectVisual 컴포넌트가 없습니다.");
            }
        }


        // ====== 유틸리티 메서드 ======

        /// <summary>
        /// StatusEffectData 동적 생성
        /// </summary>
        private StatusEffectData CreateEffectData(
            StatusEffectType effectType,
            string displayName,
            float value,
            float duration,
            bool isBuff = true,
            float tickInterval = 0f,
            int maxStack = 1)
        {
            StatusEffectData data = ScriptableObject.CreateInstance<StatusEffectData>();

            data.effectType = effectType;
            data.displayName = displayName;
            data.description = $"테스트용 {displayName} 효과";
            data.value = value;
            data.duration = duration;
            data.tickInterval = tickInterval;
            data.maxStack = maxStack;
            data.isBuff = isBuff;

            return data;
        }

        /// <summary>
        /// 플레이어 유효성 검증
        /// </summary>
        private bool ValidatePlayer()
        {
            if (playerStats == null)
            {
                Debug.LogError("[Test] PlayerStats가 할당되지 않았습니다.");
                return false;
            }

            if (!StatusEffectManager.HasInstance)
            {
                Debug.LogError("[Test] StatusEffectManager가 존재하지 않습니다.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 적 유효성 검증
        /// </summary>
        private bool ValidateEnemy()
        {
            if (enemy == null)
            {
                Debug.LogError("[Test] Enemy가 할당되지 않았습니다.");
                return false;
            }

            if (!StatusEffectManager.HasInstance)
            {
                Debug.LogError("[Test] StatusEffectManager가 존재하지 않습니다.");
                return false;
            }

            return true;
        }
    }
}
