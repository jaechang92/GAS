using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UI.Core;
using UI.Components;
using Combat.Core;

namespace UI.Panels
{
    /// <summary>
    /// 플레이어 HUD 패널
    /// HP/MP 바와 스킬 UI 표시
    /// </summary>
    public class PlayerHUDPanel : BasePanel
    {
        [Header("HP/MP 바")]
        [SerializeField] private Slider hpBar;
        [SerializeField] private TextMeshProUGUI hpText;
        [SerializeField] private Slider mpBar;
        [SerializeField] private TextMeshProUGUI mpText;

        [Header("스킬 슬롯")]
        [SerializeField] private SkillSlotUI skill1Slot;
        [SerializeField] private SkillSlotUI skill2Slot;

        [Header("스킬 설정")]
        [SerializeField] private KeyCode skill1Key = KeyCode.Q;
        [SerializeField] private KeyCode skill2Key = KeyCode.E;
        [SerializeField] private float skill1Cooldown = 5f;
        [SerializeField] private float skill2Cooldown = 8f;

        [Header("플레이어 참조")]
        [SerializeField] private GameObject playerObject;

        [Header("디버그")]
        [SerializeField] private bool showDebugLog = true;

        private HealthSystem playerHealth;
        private float currentMana = 100f;
        private float maxMana = 100f;

        protected override void Awake()
        {
            base.Awake();
        }

        private void Start()
        {
            // 플레이어 찾기
            FindPlayer();

            // 스킬 슬롯 초기화
            InitializeSkillSlots();

            // 초기 UI 업데이트
            UpdateHealthUI();
            UpdateManaUI();
        }

        private void Update()
        {
            // 스킬 키 입력 감지
            if (Input.GetKeyDown(skill1Key))
            {
                UseSkill1();
            }

            if (Input.GetKeyDown(skill2Key))
            {
                UseSkill2();
            }
        }

        /// <summary>
        /// 플레이어 찾기 및 HealthSystem 연결
        /// </summary>
        private void FindPlayer()
        {
            if (playerObject == null)
            {
                playerObject = GameObject.FindGameObjectWithTag("Player");
            }

            if (playerObject != null)
            {
                playerHealth = playerObject.GetComponent<HealthSystem>();

                if (playerHealth != null)
                {
                    // HealthSystem 이벤트 구독
                    playerHealth.OnHealthChanged += OnPlayerHealthChanged;
                    Log("플레이어 HealthSystem 연결 완료");
                }
                else
                {
                    Debug.LogWarning("[PlayerHUDPanel] Player에 HealthSystem이 없습니다!");
                }
            }
            else
            {
                Debug.LogWarning("[PlayerHUDPanel] Player를 찾을 수 없습니다!");
            }
        }

        /// <summary>
        /// 스킬 슬롯 초기화
        /// </summary>
        private void InitializeSkillSlots()
        {
            if (skill1Slot != null)
            {
                skill1Slot.SetKeyText(skill1Key.ToString());
                Log($"Skill1 슬롯 초기화: {skill1Key}");
            }

            if (skill2Slot != null)
            {
                skill2Slot.SetKeyText(skill2Key.ToString());
                Log($"Skill2 슬롯 초기화: {skill2Key}");
            }
        }

        #region HP/MP 업데이트

        /// <summary>
        /// 플레이어 체력 변경 이벤트 핸들러
        /// </summary>
        private void OnPlayerHealthChanged(float current, float max)
        {
            UpdateHealthUI(current, max);
        }

        /// <summary>
        /// 체력 UI 업데이트
        /// </summary>
        private void UpdateHealthUI()
        {
            if (playerHealth != null)
            {
                UpdateHealthUI(playerHealth.CurrentHealth, playerHealth.MaxHealth);
            }
        }

        /// <summary>
        /// 체력 UI 업데이트 (값 지정)
        /// </summary>
        private void UpdateHealthUI(float current, float max)
        {
            if (hpBar != null)
            {
                hpBar.value = current / max;
            }

            if (hpText != null)
            {
                hpText.text = $"{current:F0} / {max:F0}";
            }
        }

        /// <summary>
        /// 마나 UI 업데이트
        /// </summary>
        private void UpdateManaUI()
        {
            if (mpBar != null)
            {
                mpBar.value = currentMana / maxMana;
            }

            if (mpText != null)
            {
                mpText.text = $"{currentMana:F0} / {maxMana:F0}";
            }
        }

        /// <summary>
        /// 마나 소모
        /// </summary>
        private bool ConsumeMana(float amount)
        {
            if (currentMana >= amount)
            {
                currentMana -= amount;
                UpdateManaUI();
                return true;
            }

            Log("마나 부족!");
            return false;
        }

        #endregion

        #region 스킬 사용

        /// <summary>
        /// 스킬 1 사용
        /// </summary>
        private void UseSkill1()
        {
            if (skill1Slot == null || skill1Slot.IsOnCooldown)
            {
                Log("Skill1 쿨다운 중!");
                return;
            }

            // 마나 소모 (20 마나)
            if (!ConsumeMana(20f))
            {
                return;
            }

            Log("Skill1 사용!");

            // 쿨다운 시작
            skill1Slot.StartCooldown(skill1Cooldown);

            // TODO: 실제 스킬 로직 실행
            // 예: 대시, 공격 등
        }

        /// <summary>
        /// 스킬 2 사용
        /// </summary>
        private void UseSkill2()
        {
            if (skill2Slot == null || skill2Slot.IsOnCooldown)
            {
                Log("Skill2 쿨다운 중!");
                return;
            }

            // 마나 소모 (30 마나)
            if (!ConsumeMana(30f))
            {
                return;
            }

            Log("Skill2 사용!");

            // 쿨다운 시작
            skill2Slot.StartCooldown(skill2Cooldown);

            // TODO: 실제 스킬 로직 실행
            // 예: 특수 공격, 버프 등
        }

        #endregion

        #region 공개 API

        /// <summary>
        /// 플레이어 설정
        /// </summary>
        public void SetPlayer(GameObject player)
        {
            playerObject = player;
            FindPlayer();
            UpdateHealthUI();
        }

        /// <summary>
        /// 마나 설정
        /// </summary>
        public void SetMana(float current, float max)
        {
            currentMana = Mathf.Clamp(current, 0f, max);
            maxMana = max;
            UpdateManaUI();
        }

        /// <summary>
        /// 마나 회복
        /// </summary>
        public void RestoreMana(float amount)
        {
            currentMana = Mathf.Min(currentMana + amount, maxMana);
            UpdateManaUI();
        }

        #endregion

        private void OnDestroy()
        {
            // 이벤트 구독 해제
            if (playerHealth != null)
            {
                playerHealth.OnHealthChanged -= OnPlayerHealthChanged;
            }
        }

        private void Log(string message)
        {
            if (showDebugLog) Debug.Log($"[PlayerHUDPanel] {message}");
        }
    }
}
