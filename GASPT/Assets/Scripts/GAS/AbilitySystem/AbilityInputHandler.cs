// ================================
// File: Assets/Scripts/GAS/AbilitySystem/AbilityInputHandler.cs
// 어빌리티 입력 처리 시스템
// ================================
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GAS.AbilitySystem
{
    /// <summary>
    /// 어빌리티 입력을 처리하고 AbilitySystemComponent와 연결
    /// </summary>
    [RequireComponent(typeof(AbilitySystemComponent))]
    public class AbilityInputHandler : MonoBehaviour
    {
        [Header("Input Bindings")]
        [SerializeField] private List<AbilityInputBinding> inputBindings = new List<AbilityInputBinding>();

        [Header("Default Abilities")]
        [SerializeField] private GameplayAbility primaryAbility; // 마우스 좌클릭/Q
        [SerializeField] private GameplayAbility secondaryAbility; // 마우스 우클릭/E
        [SerializeField] private GameplayAbility utilityAbility; // Shift/Space
        [SerializeField] private GameplayAbility ultimateAbility; // R

        [Header("Settings")]
        [SerializeField] private bool useNewInputSystem = true;
        [SerializeField] private bool allowHoldActivation = true;
        [SerializeField] private bool debugMode = false;

        // Components
        private AbilitySystemComponent abilitySystem;
        private PlayerInput playerInput;

        // Input tracking
        private Dictionary<int, float> heldInputs = new Dictionary<int, float>();
        private Dictionary<int, bool> inputStates = new Dictionary<int, bool>();

        // Input IDs
        private const int PRIMARY_INPUT = 1;
        private const int SECONDARY_INPUT = 2;
        private const int UTILITY_INPUT = 3;
        private const int ULTIMATE_INPUT = 4;

        #region Unity Lifecycle

        private void Awake()
        {
            InitializeComponents();
            SetupInputBindings();
        }

        private void Start()
        {
            GrantDefaultAbilities();
        }

        private void Update()
        {
            if (useNewInputSystem && playerInput != null)
            {
                ProcessNewInputSystem();
            }
            else
            {
                ProcessLegacyInput();
            }

            UpdateHeldInputs();
        }

        private void OnDestroy()
        {
            CleanupInput();
        }

        #endregion

        #region Initialization

        private void InitializeComponents()
        {
            abilitySystem = GetComponent<AbilitySystemComponent>();

            if (useNewInputSystem)
            {
                playerInput = GetComponent<PlayerInput>();
                if (playerInput == null)
                {
                    playerInput = gameObject.AddComponent<PlayerInput>();
                }
            }
        }

        private void SetupInputBindings()
        {
            // 기본 입력 바인딩 설정
            if (inputBindings.Count == 0)
            {
                inputBindings.Add(new AbilityInputBinding(PRIMARY_INPUT, "Primary", KeyCode.Q));
                inputBindings.Add(new AbilityInputBinding(SECONDARY_INPUT, "Secondary", KeyCode.E));
                inputBindings.Add(new AbilityInputBinding(UTILITY_INPUT, "Utility", KeyCode.LeftShift));
                inputBindings.Add(new AbilityInputBinding(ULTIMATE_INPUT, "Ultimate", KeyCode.R));
            }

            // 입력 상태 초기화
            foreach (var binding in inputBindings)
            {
                inputStates[binding.inputId] = false;
                heldInputs[binding.inputId] = 0f;
            }
        }

        private void GrantDefaultAbilities()
        {
            if (primaryAbility != null)
            {
                abilitySystem.GiveAbility(primaryAbility, PRIMARY_INPUT);
            }

            if (secondaryAbility != null)
            {
                abilitySystem.GiveAbility(secondaryAbility, SECONDARY_INPUT);
            }

            if (utilityAbility != null)
            {
                abilitySystem.GiveAbility(utilityAbility, UTILITY_INPUT);
            }

            if (ultimateAbility != null)
            {
                abilitySystem.GiveAbility(ultimateAbility, ULTIMATE_INPUT);
            }
        }

        #endregion

        #region Input Processing

        private void ProcessNewInputSystem()
        {
            if (playerInput == null || playerInput.currentActionMap == null) return;

            var actionMap = playerInput.currentActionMap;

            // Primary (Attack/Ability1)
            var primaryAction = actionMap.FindAction("Fire");
            if (primaryAction != null)
            {
                ProcessInputAction(primaryAction, PRIMARY_INPUT);
            }

            // Secondary (Ability2)
            var secondaryAction = actionMap.FindAction("SecondaryFire");
            if (secondaryAction != null)
            {
                ProcessInputAction(secondaryAction, SECONDARY_INPUT);
            }

            // Utility (Dash/Ability3)
            var utilityAction = actionMap.FindAction("Dash");
            if (utilityAction != null)
            {
                ProcessInputAction(utilityAction, UTILITY_INPUT);
            }

            // Ultimate
            var ultimateAction = actionMap.FindAction("Ultimate");
            if (ultimateAction != null)
            {
                ProcessInputAction(ultimateAction, ULTIMATE_INPUT);
            }
        }

        private void ProcessInputAction(InputAction action, int inputId)
        {
            bool wasPressed = inputStates.GetValueOrDefault(inputId, false);
            bool isPressed = action.IsPressed();

            inputStates[inputId] = isPressed;

            // Press
            if (isPressed && !wasPressed)
            {
                OnInputPressed(inputId);
            }
            // Release
            else if (!isPressed && wasPressed)
            {
                OnInputReleased(inputId);
            }
            // Hold
            else if (isPressed && allowHoldActivation)
            {
                heldInputs[inputId] += Time.deltaTime;
            }
        }

        private void ProcessLegacyInput()
        {
            foreach (var binding in inputBindings)
            {
                bool wasPressed = inputStates.GetValueOrDefault(binding.inputId, false);
                bool isPressed = Input.GetKey(binding.keyCode) ||
                               (binding.alternativeKey != KeyCode.None && Input.GetKey(binding.alternativeKey));

                inputStates[binding.inputId] = isPressed;

                // Mouse input
                if (binding.inputId == PRIMARY_INPUT)
                {
                    isPressed = isPressed || Input.GetMouseButton(0);
                }
                else if (binding.inputId == SECONDARY_INPUT)
                {
                    isPressed = isPressed || Input.GetMouseButton(1);
                }

                // Process state changes
                if (isPressed && !wasPressed)
                {
                    OnInputPressed(binding.inputId);
                }
                else if (!isPressed && wasPressed)
                {
                    OnInputReleased(binding.inputId);
                }
                else if (isPressed && allowHoldActivation && binding.allowHold)
                {
                    heldInputs[binding.inputId] += Time.deltaTime;
                }
            }
        }

        private void UpdateHeldInputs()
        {
            var toProcess = new List<int>();

            foreach (var kvp in heldInputs)
            {
                if (kvp.Value > 0)
                {
                    var binding = GetBinding(kvp.Key);
                    if (binding != null && binding.allowHold && kvp.Value >= binding.minHoldTime)
                    {
                        toProcess.Add(kvp.Key);
                    }
                }
            }

            foreach (int inputId in toProcess)
            {
                OnInputHeld(inputId, heldInputs[inputId]);
            }
        }

        #endregion

        #region Input Events

        private void OnInputPressed(int inputId)
        {
            if (debugMode)
                Debug.Log($"[AbilityInput] Input {inputId} pressed");

            var inputData = CreateInputData(inputId);
            inputData.isPressed = true;

            // Try to activate ability
            abilitySystem.TryActivateAbilityByInput(inputId);

            // Reset hold timer
            heldInputs[inputId] = 0f;
        }

        private void OnInputReleased(int inputId)
        {
            if (debugMode)
                Debug.Log($"[AbilityInput] Input {inputId} released");

            var inputData = CreateInputData(inputId);
            inputData.isReleased = true;

            // Cancel ability if needed
            abilitySystem.CancelAbilityByInput(inputId);

            // Reset hold timer
            heldInputs[inputId] = 0f;
        }

        private void OnInputHeld(int inputId, float holdDuration)
        {
            var binding = GetBinding(inputId);
            if (binding == null || !binding.allowHold) return;

            var inputData = CreateInputData(inputId);
            inputData.isHeld = true;
            inputData.holdDuration = holdDuration;

            // Continuous activation for holdable abilities
            abilitySystem.TryActivateAbilityByInput(inputId);
        }

        #endregion

        #region Helper Methods

        private AbilityInputData CreateInputData(int inputId)
        {
            var data = new AbilityInputData(inputId);

            // Screen position
            data.screenPosition = Input.mousePosition;

            // World position
            if (Camera.main != null)
            {
                Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                worldPos.z = transform.position.z;
                data.worldPosition = worldPos;
            }

            // Target detection
            if (Camera.main != null)
            {
                RaycastHit2D hit = Physics2D.Raycast(
                    Camera.main.ScreenToWorldPoint(Input.mousePosition),
                    Vector2.zero
                );

                if (hit.collider != null)
                {
                    data.targetObject = hit.collider.gameObject;
                }
            }

            return data;
        }

        private AbilityInputBinding GetBinding(int inputId)
        {
            return inputBindings.Find(b => b.inputId == inputId);
        }

        private void CleanupInput()
        {
            inputStates.Clear();
            heldInputs.Clear();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 어빌리티를 특정 입력에 바인딩
        /// </summary>
        public void BindAbilityToInput(GameplayAbility ability, int inputId)
        {
            abilitySystem.GiveAbility(ability, inputId);
        }

        /// <summary>
        /// 입력 바인딩 변경
        /// </summary>
        public void ChangeInputBinding(int inputId, KeyCode newKey)
        {
            var binding = GetBinding(inputId);
            if (binding != null)
            {
                binding.keyCode = newKey;
            }
        }

        /// <summary>
        /// 모든 입력 활성화/비활성화
        /// </summary>
        public void SetInputEnabled(bool enabled)
        {
            this.enabled = enabled;
        }

        #endregion
    }
}