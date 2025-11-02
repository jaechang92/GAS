using UnityEngine;
using TMPro;
using Core.Enums;
using GASPT.Stats;

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


        // ====== Unity 생명주기 ======

        private void Awake()
        {
            // PlayerStats가 할당되지 않았으면 자동으로 찾기
            if (playerStats == null)
            {
                playerStats = FindAnyObjectByType<PlayerStats>();

                if (playerStats == null)
                {
                    Debug.LogError("[StatPanelUI] PlayerStats를 찾을 수 없습니다. Scene에 PlayerStats 컴포넌트가 있는지 확인하세요.");
                }
            }

            ValidateReferences();
        }

        private void OnEnable()
        {
            if (playerStats != null)
            {
                // 이벤트 구독
                playerStats.OnStatChanged += OnStatChanged;

                // 초기 UI 업데이트
                UpdateAllStats();

                Debug.Log("[StatPanelUI] PlayerStats 이벤트 구독 완료");
            }
        }

        private void OnDisable()
        {
            if (playerStats != null)
            {
                // 이벤트 구독 해제
                playerStats.OnStatChanged -= OnStatChanged;

                Debug.Log("[StatPanelUI] PlayerStats 이벤트 구독 해제");
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
                    UpdateAttackText(newValue);
                    break;

                case StatType.Defense:
                    UpdateDefenseText(newValue);
                    break;

                default:
                    Debug.LogWarning($"[StatPanelUI] 알 수 없는 StatType: {statType}");
                    break;
            }

            Debug.Log($"[StatPanelUI] UI 업데이트: {statType} = {newValue}");
        }


        // ====== UI 업데이트 메서드 ======

        /// <summary>
        /// 모든 스탯 UI 업데이트
        /// </summary>
        private void UpdateAllStats()
        {
            if (playerStats == null) return;

            UpdateHPText(playerStats.CurrentHP);
            UpdateAttackText(playerStats.Attack);
            UpdateDefenseText(playerStats.Defense);

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
        /// 공격력 텍스트 업데이트
        /// </summary>
        private void UpdateAttackText(int value)
        {
            if (attackText != null)
            {
                attackText.text = $"Attack: {value}";
            }
        }

        /// <summary>
        /// 방어력 텍스트 업데이트
        /// </summary>
        private void UpdateDefenseText(int value)
        {
            if (defenseText != null)
            {
                defenseText.text = $"Defense: {value}";
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
