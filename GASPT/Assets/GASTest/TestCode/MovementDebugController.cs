// ===================================
// 파일: Assets/Scripts/Ability/Debug/MovementDebugController.cs
// ===================================
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AbilitySystemDebug.Platformer
{
    /// <summary>
    /// 플레이어 이동 테스트 디버그용 컨트롤러
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class MovementDebugController : MonoBehaviour
    {
        [Header("===== 디버그 설정 =====")]
        [SerializeField] private bool showDebugInfo = true;
        [SerializeField] private bool useAlternativeInput = false;

        [Header("컴포넌트 체크")]
        [SerializeField] private bool hasPlayerInput;
        [SerializeField] private bool hasRigidbody2D;
        [SerializeField] private bool inputActionsAssigned;
        [SerializeField] private bool moveActionFound;

        [Header("입력 상태")]
        [SerializeField] private Vector2 currentMoveInput;
        [SerializeField] private Vector2 rawInputValue;
        [SerializeField] private bool isReceivingInput;
        [SerializeField] private string lastInputTime;

        [Header("이동 상태")]
        [SerializeField] private Vector2 currentVelocity;
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private bool isMoving;

        [Header("컴포넌트 참조")]
        private AbilityInputActions inputActions;
        private PlayerInput playerInput;
        private InputAction moveAction;
        private Rigidbody2D rb;
        private TextMeshProUGUI debugText;

        private void OnEnable()
        {
            inputActions.Player.Enable();
            inputActions.Player.Move.performed += OnMovePerformed;
            inputActions.Player.Move.canceled += OnMoveCanceled;
        }

        private void Awake()
        {
            Debug.Log("=== MovementDebugController 시작 ===");

            inputActions = new AbilityInputActions();



            // Rigidbody2D 체크
            rb = GetComponent<Rigidbody2D>();
            hasRigidbody2D = rb != null;
            if (!hasRigidbody2D)
            {
                Debug.LogError(" Rigidbody2D가 없습니다!");
                rb = gameObject.AddComponent<Rigidbody2D>();
                rb.gravityScale = 2f;
                rb.freezeRotation = true;
                Debug.Log("V Rigidbody2D 자동 추가됨");
            }

            // PlayerInput 체크
            playerInput = GetComponent<PlayerInput>();
            hasPlayerInput = playerInput != null;

            if (!hasPlayerInput)
            {
                Debug.LogError("X PlayerInput 컴포넌트가 없습니다!");
                playerInput = gameObject.AddComponent<PlayerInput>();
                Debug.Log("V PlayerInput 자동 추가됨 - Input Actions Asset을 할당해주세요!");
            }

            SetupDebugUI();
        }

        private void Start()
        {
            SetupInput();

            // 컴포넌트 상태 로그
            LogComponentStatus();
        }

        /// <summary>
        /// 입력 시스템 설정
        /// </summary>
        private void SetupInput()
        {
            if (playerInput != null && playerInput.actions != null)
            {
                inputActionsAssigned = true;

                // Move 액션 찾기 (여러 경로 시도)
                string[] possiblePaths = {
                    "Player/Move",
                    "Gameplay/Move",
                    "Move",
                    "Player/Movement",
                    "Movement"
                };

                foreach (string path in possiblePaths)
                {
                    moveAction = playerInput.actions.FindAction(path);
                    if (moveAction != null)
                    {
                        moveActionFound = true;
                        Debug.Log($"V Move 액션을 찾았습니다: {path}");

                        // 콜백 설정
                        moveAction.performed += OnMovePerformed;
                        moveAction.canceled += OnMoveCanceled;

                        // 액션 활성화
                        moveAction.Enable();
                        break;
                    }
                }

                if (!moveActionFound)
                {
                    Debug.LogError("X Move �׼��� ã�� �� �����ϴ�! Action Map�� Ȯ���ϼ���.");
                    Debug.Log("�õ��� ��ε�: " + string.Join(", ", possiblePaths));
                }

                // ��ü �׼� �� Ȱ��ȭ
                playerInput.ActivateInput();

                // ���� �׼� �� ���� ���
                Debug.Log($"���� Action Map: {playerInput.currentActionMap?.name ?? "����"}");

                // ��� �׼� ���
                if (playerInput.actions != null)
                {
                    Debug.Log("=== ��� ������ ��� �׼� ===");
                    foreach (var action in playerInput.actions)
                    {
                        Debug.Log($"- {action.name} (���: {action.actionMap?.name}/{action.name})");
                    }
                }
            }
            else
            {
                inputActionsAssigned = false;
                Debug.LogError("X Input Actions Asset�� �Ҵ���� �ʾҽ��ϴ�!");
            }
        }

        /// <summary>
        /// �̵� �Է� ����
        /// </summary>
        private void OnMovePerformed(InputAction.CallbackContext context)
        {
            rawInputValue = context.ReadValue<Vector2>();
            currentMoveInput = rawInputValue;
            isReceivingInput = true;
            lastInputTime = Time.time.ToString("F2");

            Debug.Log($" Move Input: {rawInputValue}");
        }

        /// <summary>
        /// �̵� �Է� ����
        /// </summary>
        private void OnMoveCanceled(InputAction.CallbackContext context)
        {
            rawInputValue = Vector2.zero;
            currentMoveInput = Vector2.zero;
            isReceivingInput = false;

            Debug.Log(" Move Input ����");
        }

        public Vector2 actionValueDebug;

        private void Update()
        {
            //// ��ü �Է� �ý��� (�׽�Ʈ��)
            //if (useAlternativeInput || !moveActionFound)
            //{
            //    float horizontal = Input.GetAxisRaw("Horizontal");
            //    float vertical = Input.GetAxisRaw("Vertical");
            //    currentMoveInput = new Vector2(horizontal, vertical);

            //    if (currentMoveInput != Vector2.zero)
            //    {
            //        isReceivingInput = true;
            //        lastInputTime = Time.time.ToString("F2");
            //    }
            //}

            //// Input System���� ���� �б� �õ�
            //if (moveAction != null && moveAction.enabled)
            //{
            //    Vector2 actionValue = moveAction.ReadValue<Vector2>();
            //    if (actionValue != rawInputValue)
            //    {
            //        rawInputValue = actionValue;
            //        currentMoveInput = actionValue;
            //        if (actionValue != Vector2.zero)
            //        {
            //            isReceivingInput = true;
            //            lastInputTime = Time.time.ToString("F2");
            //        }
            //    }
            //}

            UpdateDebugUI();
        }

        private void FixedUpdate()
        {
            if (rb != null)
            {
                // �̵� ó��
                Vector2 targetVelocity = new Vector2(currentMoveInput.x * moveSpeed, rb.linearVelocityY);
                rb.linearVelocity = targetVelocity;

                currentVelocity = rb.linearVelocity;
                isMoving = Mathf.Abs(currentVelocity.x) > 0.1f;

                // �̵� �α� (���� ������ ���� ���� �̵� �ÿ���)
                if (isMoving && Time.frameCount % 30 == 0) // 0.5�ʸ���
                {
                    Debug.Log($" �̵� ��: �ӵ�={currentVelocity}, �Է�={currentMoveInput}");
                }
            }
        }

        /// <summary>
        /// ������Ʈ ���� �α�
        /// </summary>
        private void LogComponentStatus()
        {
            Debug.Log("=== ������Ʈ ���� üũ ===");
            Debug.Log($"PlayerInput: {(hasPlayerInput ? "V" : "X")}");
            Debug.Log($"Rigidbody2D: {(hasRigidbody2D ? "V" : "X")}");
            Debug.Log($"Input Actions �Ҵ�: {(inputActionsAssigned ? "V" : "X")}");
            Debug.Log($"Move Action ã��: {(moveActionFound ? "V" : "X")}");

            if (rb != null)
            {
                Debug.Log($"Rigidbody2D ����:");
                Debug.Log($"- Gravity Scale: {rb.gravityScale}");
                Debug.Log($"- Is Kinematic: {rb.bodyType}");
                Debug.Log($"- Freeze Rotation: {rb.freezeRotation}");
                Debug.Log($"- Mass: {rb.mass}");
                Debug.Log($"- Drag: {rb.linearDamping}");
            }
        }

        /// <summary>
        /// ����� UI ����
        /// </summary>
        private void SetupDebugUI()
        {
            if (!showDebugInfo) return;

            GameObject canvas = GameObject.Find("Debug Canvas");
            if (canvas == null)
            {
                canvas = new GameObject("Debug Canvas");
                Canvas canvasComp = canvas.AddComponent<Canvas>();
                canvasComp.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.AddComponent<UnityEngine.UI.CanvasScaler>();
                canvas.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            }

            GameObject debugPanel = new GameObject("Movement Debug Panel");
            debugPanel.transform.SetParent(canvas.transform);
            RectTransform rect = debugPanel.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(1, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(1, 1);
            rect.anchoredPosition = new Vector2(-10, -10);
            rect.sizeDelta = new Vector2(400, 300);

            UnityEngine.UI.Image bg = debugPanel.AddComponent<UnityEngine.UI.Image>();
            bg.color = new Color(0, 0, 0, 0.8f);

            GameObject textObj = new GameObject("Debug Text");
            textObj.transform.SetParent(debugPanel.transform);
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(10, 10);
            textRect.offsetMax = new Vector2(-10, -10);

            debugText = textObj.AddComponent<TextMeshProUGUI>();
            debugText.fontSize = 12;
            debugText.color = Color.white;
        }

        /// <summary>
        /// ����� UI ������Ʈ
        /// </summary>
        private void UpdateDebugUI()
        {
            if (debugText == null) return;

            string info = "=== �̵� ����� ���� ===\n\n";

            info += "[������Ʈ ����]\n";
            info += $"PlayerInput: {(hasPlayerInput ? "V" : "X")}\n";
            info += $"Rigidbody2D: {(hasRigidbody2D ? "V" : "X")}\n";
            info += $"Input Actions: {(inputActionsAssigned ? "V" : "X")}\n";
            info += $"Move Action: {(moveActionFound ? "V" : "X")}\n\n";

            info += "[�Է� ����]\n";
            info += $"���� �Է�: {currentMoveInput}\n";
            info += $"Raw �Է�: {rawInputValue}\n";
            info += $"�Է� �޴� ��: {(isReceivingInput ? "��" : "�ƴϿ�")}\n";
            info += $"������ �Է�: {lastInputTime}\n";
            info += $"��ü �Է�: {(useAlternativeInput ? "���" : "�̻��")}\n\n";

            info += "[�̵� ����]\n";
            info += $"���� �ӵ�: {currentVelocity}\n";
            info += $"�̵� �ӵ�: {moveSpeed}\n";
            info += $"�̵� ��: {(isMoving ? "��" : "�ƴϿ�")}\n\n";

            info += "[���۹�]\n";
            info += "�̵�: A/D �Ǵ� ��/��\n";
            info += "����: Space (���� ���� �ʿ�)\n";

            if (!moveActionFound)
            {
                info += "\n<color=red>! Move Action�� ã�� �� ����!</color>\n";
                info += "<color=yellow>Legacy Input ��� ��...</color>";
            }

            debugText.text = info;
        }

        private void OnDestroy()
        {
            if (inputActions != null)
            {
                inputActions.Dispose();
                inputActions.Player.Move.performed -= OnMovePerformed;
                inputActions.Player.Move.canceled -= OnMoveCanceled;

            }
            // �ݹ� ����
            if (moveAction != null)
            {
                moveAction.performed -= OnMovePerformed;
                moveAction.canceled -= OnMoveCanceled;
            }
        }

        /// <summary>
        /// ���� �̵� �׽�Ʈ
        /// </summary>
        [ContextMenu("Force Move Right")]
        public void ForceTestMoveRight()
        {
            currentMoveInput = Vector2.right;
            Debug.Log("���� �̵� �׽�Ʈ: ������");
        }

        [ContextMenu("Force Move Left")]
        public void ForceTestMoveLeft()
        {
            currentMoveInput = Vector2.left;
            Debug.Log("���� �̵� �׽�Ʈ: ����");
        }

        [ContextMenu("Stop Movement")]
        public void StopMovement()
        {
            currentMoveInput = Vector2.zero;
            Debug.Log("�̵� ����");
        }
    }
}
