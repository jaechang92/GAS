using UnityEngine;
using TMPro;
using GASPT.Core.Enums;
using GASPT.Stats;
using GASPT.StatusEffects;

namespace GASPT.UI
{
    /// <summary>
    /// 플레이어 스탯을 표시하는 UI 패널
    /// PlayerStats의 OnStatChanged 이벤트를 구독하여 실시간 업데이트
    /// </summary>
    public class StatPanelUI : MonoBehaviour
    {
        // ====== UI 요소 ======

        [Header("UI 요소")]
        [SerializeField] [Tooltip("HP 표시 텍스트")]
        private TextMeshProUGUI hpText;

        [SerializeField] [Tooltip("공격력 표시 텍스트")]
        private TextMeshProUGUI attackText;

        [SerializeField] [Tooltip("방어력 표시 텍스트")]
        private TextMeshProUGUI defenseText;


        // ====== PlayerStats 참조 ======

        [Header("참조")]
        [SerializeField] [Tooltip("플레이어 스탯 컴포넌트")]
        private PlayerStats playerStats;


        // ====== 색상 설정 ======

        [Header("색상 설정")]
        [SerializeField] [Tooltip("버프 적용 시 텍스트 색상")]
        private Color buffColor = new Color(0.2f, 1f, 0.2f); // 밝은 초록색

        [SerializeField] [Tooltip("디버프 적용 시 텍스트 색상")]
        private Color debuffColor = new Color(1f, 0.2f, 0.2f); // 밝은 빨간색

        [SerializeField] [Tooltip("기본 텍스트 색상")]
        private Color normalColor = Color.white;


        // ====== Unity 생명주기 ======

        private void Awake()
        {
            // PlayerStats가 할당되지 않았으면 자동으로 찾기
            if (playerStats == null)
            {
                // RunManager 우선
                if (GASPT.Core.RunManager.HasInstance && GASPT.Core.RunManager.Instance.CurrentPlayer != null)
                {
                    playerStats = GASPT.Core.RunManager.Instance.CurrentPlayer;
                }
                // GameManager 차선
                else if (GASPT.Core.GameManager.HasInstance && GASPT.Core.GameManager.Instance.PlayerStats != null)
                {
                    playerStats = GASPT.Core.GameManager.Instance.PlayerStats;
                }

                if (playerStats == null)
                {
                    Debug.LogWarning("[StatPanelUI] PlayerStats를 찾을 수 없습니다. 런이 시작되면 자동으로 연결됩니다.");
                }
            }

            ValidateReferences();
        }

        private void OnEnable()
        {
            if (playerStats != null)
            {
                // 이벤트 구독
                playerStats.OnStatsChanged += OnStatChanged;

                // 초기 UI 업데이트
                UpdateAllStats();

                Debug.Log("[StatPanelUI] PlayerStats 이벤트 구독 완료");
            }

            // StatusEffectManager 이벤트 구독
            StatusEffectManager manager = StatusEffectManager.Instance;

            if (manager != null)
            {
                // 중복 구독 방지를 위해 먼저 구독 해제
                manager.OnEffectApplied -= OnEffectChanged;
                manager.OnEffectRemoved -= OnEffectChanged;

                // 구독
                manager.OnEffectApplied += OnEffectChanged;
                manager.OnEffectRemoved += OnEffectChanged;

                Debug.Log("[StatPanelUI] StatusEffectManager 이벤트 구독 완료");
            }
            else
            {
                Debug.LogError("[StatPanelUI] StatusEffectManager를 찾을 수 없습니다.");
            }
        }

        private void OnDisable()
        {
            if (playerStats != null)
            {
                // 이벤트 구독 해제
                playerStats.OnStatsChanged -= OnStatChanged;

                Debug.Log("[StatPanelUI] PlayerStats 이벤트 구독 해제");
            }

            // StatusEffectManager 이벤트 구독 해제
            if (StatusEffectManager.HasInstance)
            {
                StatusEffectManager.Instance.OnEffectApplied -= OnEffectChanged;
                StatusEffectManager.Instance.OnEffectRemoved -= OnEffectChanged;

                Debug.Log("[StatPanelUI] StatusEffectManager 이벤트 구독 해제");
            }
        }


        // ====== 이벤트 핸들러 ======

        /// <summary>
        /// PlayerStats의 OnStatChanged 이벤트 핸들러
        /// </summary>
        /// <param name="statType">변경된 스탯 타입</param>
        /// <param name="oldValue">이전 값</param>
        /// <param name="newValue">새 값</param>
        private void OnStatChanged(StatType statType, int oldValue, int newValue)
        {
            switch (statType)
            {
                case StatType.HP:
                    UpdateHPText(newValue);
                    break;

                case StatType.Attack:
                    UpdateAttackText();
                    break;

                case StatType.Defense:
                    UpdateDefenseText();
                    break;

                default:
                    Debug.LogWarning($"[StatPanelUI] 알 수 없는 StatType: {statType}");
                    break;
            }

            Debug.Log($"[StatPanelUI] UI 업데이트: {statType} = {newValue}");
        }

        /// <summary>
        /// StatusEffect 변경 시 이벤트 핸들러
        /// </summary>
        /// <param name="target">대상 GameObject</param>
        /// <param name="effect">변경된 효과</param>
        private void OnEffectChanged(GameObject target, StatusEffect effect)
        {
            // 플레이어가 아니면 무시
            if (playerStats == null || target != playerStats.gameObject)
            {
                return;
            }

            // 공격력/방어력 관련 효과만 처리
            if (effect.EffectType == StatusEffectType.AttackUp ||
                effect.EffectType == StatusEffectType.AttackDown)
            {
                UpdateAttackText();
            }
            else if (effect.EffectType == StatusEffectType.DefenseUp ||
                     effect.EffectType == StatusEffectType.DefenseDown)
            {
                UpdateDefenseText();
            }
        }


        // ====== UI 업데이트 메서드 ======

        /// <summary>
        /// 모든 스탯 UI 업데이트
        /// </summary>
        private void UpdateAllStats()
        {
            if (playerStats == null) return;

            UpdateHPText(playerStats.CurrentHP);
            UpdateAttackText();
            UpdateDefenseText();

            Debug.Log("[StatPanelUI] 모든 스탯 UI 업데이트 완료");
        }

        /// <summary>
        /// HP 텍스트 업데이트
        /// </summary>
        private void UpdateHPText(int value)
        {
            if (hpText != null)
            {
                hpText.text = $"HP: {value}";
            }
        }

        /// <summary>
        /// 공격력 텍스트 업데이트 (버프/디버프 상태 반영)
        /// </summary>
        private void UpdateAttackText()
        {
            if (attackText == null || playerStats == null) return;

            int currentAttack = playerStats.Attack;

            // StatusEffect 확인
            bool hasBuff = false;
            bool hasDebuff = false;
            int baseAttack = playerStats.BaseAttack; // 기본 공격력 (버프/디버프 없이)

            if (StatusEffectManager.HasInstance)
            {
                hasBuff = StatusEffectManager.Instance.HasEffect(playerStats.gameObject, StatusEffectType.AttackUp);
                hasDebuff = StatusEffectManager.Instance.HasEffect(playerStats.gameObject, StatusEffectType.AttackDown);
            }

            // 텍스트 설정
            if (hasBuff)
            {
                attackText.text = $"Attack: {baseAttack} → {currentAttack}";
                attackText.color = buffColor;
            }
            else if (hasDebuff)
            {
                attackText.text = $"Attack: {baseAttack} → {currentAttack}";
                attackText.color = debuffColor;
            }
            else
            {
                attackText.text = $"Attack: {currentAttack}";
                attackText.color = normalColor;
            }
        }

        /// <summary>
        /// 방어력 텍스트 업데이트 (버프/디버프 상태 반영)
        /// </summary>
        private void UpdateDefenseText()
        {
            if (defenseText == null || playerStats == null) return;

            int currentDefense = playerStats.Defense;

            // StatusEffect 확인
            bool hasBuff = false;
            bool hasDebuff = false;
            int baseDefense = playerStats.BaseDefense; // 기본 방어력 (버프/디버프 없이)

            if (StatusEffectManager.HasInstance)
            {
                hasBuff = StatusEffectManager.Instance.HasEffect(playerStats.gameObject, StatusEffectType.DefenseUp);
                hasDebuff = StatusEffectManager.Instance.HasEffect(playerStats.gameObject, StatusEffectType.DefenseDown);
            }

            // 텍스트 설정
            if (hasBuff)
            {
                defenseText.text = $"Defense: {baseDefense} → {currentDefense}";
                defenseText.color = buffColor;
            }
            else if (hasDebuff)
            {
                defenseText.text = $"Defense: {baseDefense} → {currentDefense}";
                defenseText.color = debuffColor;
            }
            else
            {
                defenseText.text = $"Defense: {currentDefense}";
                defenseText.color = normalColor;
            }
        }


        // ====== 유효성 검증 ======

        /// <summary>
        /// 필수 참조 유효성 검증
        /// </summary>
        private void ValidateReferences()
        {
            bool isValid = true;

            if (hpText == null)
            {
                Debug.LogError("[StatPanelUI] hpText가 할당되지 않았습니다.");
                isValid = false;
            }

            if (attackText == null)
            {
                Debug.LogError("[StatPanelUI] attackText가 할당되지 않았습니다.");
                isValid = false;
            }

            if (defenseText == null)
            {
                Debug.LogError("[StatPanelUI] defenseText가 할당되지 않았습니다.");
                isValid = false;
            }

            if (playerStats == null)
            {
                Debug.LogError("[StatPanelUI] playerStats가 할당되지 않았습니다.");
                isValid = false;
            }

            if (!isValid)
            {
                Debug.LogError("[StatPanelUI] 필수 참조가 누락되었습니다. Inspector에서 확인하세요.");
            }
        }


        // ====== 공개 메서드 (외부 호출용) ======

        /// <summary>
        /// UI 강제 업데이트 (외부에서 호출 가능)
        /// </summary>
        public void ForceUpdateUI()
        {
            UpdateAllStats();
        }
    }
}
