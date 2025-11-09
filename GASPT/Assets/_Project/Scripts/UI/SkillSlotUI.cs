using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GASPT.Skills;

namespace GASPT.UI
{
    /// <summary>
    /// 단일 스킬 슬롯 UI
    /// 스킬 아이콘, 쿨다운 오버레이, 단축키 표시
    /// </summary>
    public class SkillSlotUI : MonoBehaviour
    {
        [Header("슬롯 설정")]
        [SerializeField] [Tooltip("슬롯 인덱스 (0~3)")]
        private int slotIndex = 0;

        [SerializeField] [Tooltip("단축키 (1, 2, 3, 4 등)")]
        private KeyCode hotkey = KeyCode.Alpha1;


        [Header("UI 참조")]
        [SerializeField] [Tooltip("스킬 아이콘 Image")]
        private Image iconImage;

        [SerializeField] [Tooltip("쿨다운 오버레이 Image (fillAmount 사용)")]
        private Image cooldownOverlay;

        [SerializeField] [Tooltip("쿨다운 텍스트 (남은 시간)")]
        private TextMeshProUGUI cooldownText;

        [SerializeField] [Tooltip("단축키 텍스트")]
        private TextMeshProUGUI hotkeyText;

        [SerializeField] [Tooltip("비활성 오버레이 (마나 부족 시)")]
        private Image disabledOverlay;


        [Header("색상 설정")]
        [SerializeField] [Tooltip("쿨다운 오버레이 색상")]
        private Color cooldownColor = new Color(0f, 0f, 0f, 0.7f);

        [SerializeField] [Tooltip("마나 부족 색상")]
        private Color outOfManaColor = new Color(0.5f, 0.5f, 0.5f, 0.7f);


        // ====== 내부 상태 ======

        private Skill currentSkill;
        private bool isRegistered = false;


        // ====== Unity 생명주기 ======

        private void Start()
        {
            InitializeUI();
            UpdateHotkeyText();
        }

        private void Update()
        {
            // 키 입력 처리
            if (Input.GetKeyDown(hotkey) && isRegistered)
            {
                TryUseSkill();
            }

            // 쿨다운 UI 업데이트 (매 프레임)
            if (isRegistered && currentSkill != null && currentSkill.IsOnCooldown)
            {
                UpdateCooldownUI();
            }
        }


        // ====== 초기화 ======

        private void InitializeUI()
        {
            // 초기 상태: 빈 슬롯
            if (iconImage != null)
            {
                iconImage.enabled = false;
            }

            if (cooldownOverlay != null)
            {
                cooldownOverlay.enabled = false;
                cooldownOverlay.color = cooldownColor;
            }

            if (cooldownText != null)
            {
                cooldownText.enabled = false;
            }

            if (disabledOverlay != null)
            {
                disabledOverlay.enabled = false;
                disabledOverlay.color = outOfManaColor;
            }
        }


        // ====== 스킬 등록 ======

        /// <summary>
        /// 스킬 등록 (외부에서 호출)
        /// </summary>
        public void RegisterSkill(Skill skill)
        {
            if (skill == null)
            {
                Debug.LogWarning($"[SkillSlotUI] RegisterSkill(): skill이 null입니다. 슬롯: {slotIndex}");
                ClearSlot();
                return;
            }

            currentSkill = skill;
            isRegistered = true;

            // 아이콘 설정
            if (iconImage != null && skill.Data.icon != null)
            {
                iconImage.sprite = skill.Data.icon;
                iconImage.enabled = true;
            }
            else
            {
                // 아이콘 없으면 기본 표시
                if (iconImage != null)
                {
                    iconImage.enabled = true;
                    iconImage.color = Color.white;
                }
            }

            // 초기 상태 업데이트
            UpdateCooldownUI();

            Debug.Log($"[SkillSlotUI] 슬롯 {slotIndex}에 스킬 등록: {skill.Data.skillName}");
        }

        /// <summary>
        /// 슬롯 비우기
        /// </summary>
        public void ClearSlot()
        {
            currentSkill = null;
            isRegistered = false;

            if (iconImage != null)
            {
                iconImage.enabled = false;
            }

            if (cooldownOverlay != null)
            {
                cooldownOverlay.enabled = false;
            }

            if (cooldownText != null)
            {
                cooldownText.enabled = false;
            }

            if (disabledOverlay != null)
            {
                disabledOverlay.enabled = false;
            }

            Debug.Log($"[SkillSlotUI] 슬롯 {slotIndex} 비우기 완료");
        }


        // ====== 스킬 사용 ======

        private void TryUseSkill()
        {
            if (!isRegistered || currentSkill == null)
            {
                return;
            }

            // SkillSystem을 통해 스킬 사용
            if (SkillSystem.HasInstance)
            {
                // TODO: 타겟팅 시스템 구현 시 타겟 전달
                SkillSystem.Instance.TryUseSkill(slotIndex);
            }
        }


        // ====== UI 업데이트 ======

        /// <summary>
        /// 쿨다운 UI 업데이트 (매 프레임)
        /// </summary>
        private void UpdateCooldownUI()
        {
            if (currentSkill == null)
            {
                return;
            }

            // 쿨다운 중인지 확인
            bool isOnCooldown = currentSkill.IsOnCooldown;

            // 쿨다운 오버레이
            if (cooldownOverlay != null)
            {
                cooldownOverlay.enabled = isOnCooldown;
                if (isOnCooldown)
                {
                    cooldownOverlay.fillAmount = currentSkill.GetCooldownRatio();
                }
            }

            // 쿨다운 텍스트
            if (cooldownText != null)
            {
                cooldownText.enabled = isOnCooldown;
                if (isOnCooldown)
                {
                    cooldownText.text = $"{currentSkill.RemainingCooldown:F1}s";
                }
            }

            // 마나 부족 체크
            UpdateManaCheck();
        }

        /// <summary>
        /// 마나 부족 체크 및 UI 업데이트
        /// </summary>
        private void UpdateManaCheck()
        {
            if (currentSkill == null || disabledOverlay == null)
            {
                return;
            }

            // Player 찾기
            GameObject player = GameObject.FindWithTag("Player");
            if (player == null)
            {
                return;
            }

            var playerStats = player.GetComponent<Stats.PlayerStats>();
            if (playerStats == null)
            {
                return;
            }

            // 마나 부족 여부
            bool outOfMana = playerStats.CurrentMana < currentSkill.Data.manaCost;

            disabledOverlay.enabled = outOfMana;
        }

        /// <summary>
        /// 단축키 텍스트 업데이트
        /// </summary>
        private void UpdateHotkeyText()
        {
            if (hotkeyText != null)
            {
                // KeyCode를 숫자로 변환 (Alpha1 → "1")
                string key = hotkey.ToString().Replace("Alpha", "");
                hotkeyText.text = key;
            }
        }


        // ====== Public 접근자 ======

        /// <summary>
        /// 슬롯 인덱스 설정 (외부에서 호출)
        /// </summary>
        public void SetSlotIndex(int index)
        {
            slotIndex = index;

            // 단축키도 자동 설정 (0→1, 1→2, 2→3, 3→4)
            switch (index)
            {
                case 0: hotkey = KeyCode.Alpha1; break;
                case 1: hotkey = KeyCode.Alpha2; break;
                case 2: hotkey = KeyCode.Alpha3; break;
                case 3: hotkey = KeyCode.Alpha4; break;
            }

            UpdateHotkeyText();
        }

        /// <summary>
        /// 현재 슬롯 인덱스
        /// </summary>
        public int SlotIndex => slotIndex;

        /// <summary>
        /// 등록된 스킬
        /// </summary>
        public Skill CurrentSkill => currentSkill;

        /// <summary>
        /// 스킬 등록 여부
        /// </summary>
        public bool IsRegistered => isRegistered;


        // ====== Context Menu (테스트용) ======

        [ContextMenu("Test Register Dummy Skill")]
        private void TestRegisterDummySkill()
        {
            Debug.Log($"[SkillSlotUI] 더미 스킬 등록 테스트 (슬롯 {slotIndex})");
            // 실제로는 SkillData가 필요하므로 테스트 불가
            // SkillUIPanel에서 SkillSystem을 통해 등록해야 함
        }

        [ContextMenu("Test Clear Slot")]
        private void TestClearSlot()
        {
            ClearSlot();
        }
    }
}
