using UnityEngine;
using UnityEngine.UI;
using Combat.Core;

namespace UI.HUD
{
    /// <summary>
    /// 플레이어 정보 패널 (왼쪽 하단)
    /// 캐릭터 아이콘, 체력바, 스킬/아이템 슬롯 관리
    /// </summary>
    public class PlayerInfoPanel : MonoBehaviour
    {
        [Header("UI 컴포넌트")]
        [SerializeField] private Image characterIcon;
        [SerializeField] private HealthBarUI healthBar;
        [SerializeField] private ItemSlotUI[] itemSlots = new ItemSlotUI[2];

        [Header("설정")]
        [SerializeField] private Sprite defaultCharacterIcon;
        [SerializeField] private bool autoConnectToPlayer = true;

        private HealthSystem connectedHealthSystem;

        private void Start()
        {
            // 기본 아이콘 설정
            if (characterIcon != null && defaultCharacterIcon != null)
            {
                characterIcon.sprite = defaultCharacterIcon;
            }

            // 자동으로 플레이어 연결
            if (autoConnectToPlayer)
            {
                TryConnectToPlayer();
            }
        }

        private void OnDestroy()
        {
            // 이벤트 해제
            DisconnectHealthSystem();
        }

        /// <summary>
        /// 플레이어의 HealthSystem에 자동 연결 시도
        /// </summary>
        private void TryConnectToPlayer()
        {
            // Player 태그로 플레이어 찾기
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                HealthSystem healthSystem = playerObj.GetComponent<HealthSystem>();
                if (healthSystem != null)
                {
                    ConnectToHealthSystem(healthSystem);
                }
            }
        }

        /// <summary>
        /// HealthSystem 연결
        /// </summary>
        public void ConnectToHealthSystem(HealthSystem healthSystem)
        {
            // 기존 연결 해제
            DisconnectHealthSystem();

            if (healthSystem == null) return;

            connectedHealthSystem = healthSystem;

            // 이벤트 구독
            connectedHealthSystem.OnHealthChanged += OnHealthChanged;
            connectedHealthSystem.OnMaxHealthChanged += OnMaxHealthChanged;

            // 현재 체력으로 초기화
            if (healthBar != null)
            {
                healthBar.Initialize(connectedHealthSystem.CurrentHealth, connectedHealthSystem.MaxHealth);
            }

            Debug.Log("[PlayerInfoPanel] HealthSystem 연결 완료");
        }

        /// <summary>
        /// HealthSystem 연결 해제
        /// </summary>
        public void DisconnectHealthSystem()
        {
            if (connectedHealthSystem != null)
            {
                connectedHealthSystem.OnHealthChanged -= OnHealthChanged;
                connectedHealthSystem.OnMaxHealthChanged -= OnMaxHealthChanged;
                connectedHealthSystem = null;
            }
        }

        /// <summary>
        /// 체력 변경 이벤트 핸들러
        /// </summary>
        private void OnHealthChanged(float newHealth)
        {
            if (healthBar != null && connectedHealthSystem != null)
            {
                healthBar.UpdateHealth(newHealth, connectedHealthSystem.MaxHealth);
            }
        }

        /// <summary>
        /// 최대 체력 변경 이벤트 핸들러
        /// </summary>
        private void OnMaxHealthChanged(float newMaxHealth)
        {
            if (healthBar != null && connectedHealthSystem != null)
            {
                healthBar.UpdateHealth(connectedHealthSystem.CurrentHealth, newMaxHealth);
            }
        }

        /// <summary>
        /// 캐릭터 아이콘 변경
        /// </summary>
        public void SetCharacterIcon(Sprite icon)
        {
            if (characterIcon != null)
            {
                characterIcon.sprite = icon;
            }
        }

        /// <summary>
        /// 특정 슬롯에 아이템 설정
        /// </summary>
        public void SetItemSlot(int slotIndex, Sprite icon, int count = 1)
        {
            if (slotIndex >= 0 && slotIndex < itemSlots.Length && itemSlots[slotIndex] != null)
            {
                itemSlots[slotIndex].SetItem(icon, count);
            }
        }

        /// <summary>
        /// 특정 슬롯 비우기
        /// </summary>
        public void ClearItemSlot(int slotIndex)
        {
            if (slotIndex >= 0 && slotIndex < itemSlots.Length && itemSlots[slotIndex] != null)
            {
                itemSlots[slotIndex].ClearSlot();
            }
        }

        /// <summary>
        /// 모든 슬롯 비우기
        /// </summary>
        public void ClearAllSlots()
        {
            foreach (var slot in itemSlots)
            {
                if (slot != null)
                {
                    slot.ClearSlot();
                }
            }
        }

        /// <summary>
        /// 특정 슬롯 쿨다운 시작
        /// </summary>
        public void StartSlotCooldown(int slotIndex, float duration)
        {
            if (slotIndex >= 0 && slotIndex < itemSlots.Length && itemSlots[slotIndex] != null)
            {
                itemSlots[slotIndex].StartCooldown(duration);
            }
        }

        /// <summary>
        /// 슬롯 UI 컴포넌트 가져오기
        /// </summary>
        public ItemSlotUI GetSlot(int slotIndex)
        {
            if (slotIndex >= 0 && slotIndex < itemSlots.Length)
            {
                return itemSlots[slotIndex];
            }
            return null;
        }

        /// <summary>
        /// 체력바 UI 컴포넌트 가져오기
        /// </summary>
        public HealthBarUI GetHealthBar()
        {
            return healthBar;
        }
    }
}
