using UnityEngine;

namespace GASPT.UI
{
    /// <summary>
    /// UI 베이스 클래스
    /// Panel GameObject를 가지고 Show/Hide 기능을 제공하는 공통 UI 클래스
    /// </summary>
    public abstract class BaseUI : MonoBehaviour
    {
        // ====== Panel 참조 ======

        [Header("Base UI")]
        [Tooltip("UI 패널 (SetActive로 제어됨)")]
        [SerializeField] protected GameObject panel;


        // ====== 설정 ======

        [Header("Base UI 설정")]
        [Tooltip("Awake 시 자동으로 숨김")]
        [SerializeField] protected bool hideOnAwake = true;


        // ====== Unity 생명주기 ======

        protected virtual void Awake()
        {
            // Panel 자동 찾기
            InitializePanel();

            // 자식 클래스 초기화
            Initialize();

            // 초기 상태: 숨김
            if (hideOnAwake)
            {
                Hide();
            }
        }


        // ====== 초기화 ======

        /// <summary>
        /// Panel GameObject 자동 찾기
        /// panel 필드가 null이면 "Panel"이라는 이름의 자식 GameObject를 찾음
        /// </summary>
        protected virtual void InitializePanel()
        {
            if (panel == null)
            {
                Transform panelTransform = transform.Find("Panel");
                if (panelTransform != null)
                {
                    panel = panelTransform.gameObject;
                    Debug.Log($"[{GetType().Name}] Panel 자동 찾기 완료");
                }
                else
                {
                    Debug.LogWarning($"[{GetType().Name}] 'Panel'이라는 이름의 자식 GameObject를 찾을 수 없습니다.");
                }
            }
        }

        /// <summary>
        /// 자식 클래스에서 추가 초기화를 위해 오버라이드할 수 있는 메서드
        /// Awake에서 InitializePanel() 이후, Hide() 이전에 호출됨
        /// </summary>
        protected virtual void Initialize()
        {
            // 자식 클래스에서 필요시 오버라이드
        }


        // ====== UI 표시/숨김 ======

        /// <summary>
        /// UI 표시
        /// </summary>
        public virtual void Show()
        {
            if (panel != null)
            {
                panel.SetActive(true);
            }
        }

        /// <summary>
        /// UI 숨김
        /// </summary>
        public virtual void Hide()
        {
            if (panel != null)
            {
                panel.SetActive(false);
            }
        }

        /// <summary>
        /// UI 토글 (표시 <-> 숨김)
        /// </summary>
        public virtual void Toggle()
        {
            if (IsVisible)
            {
                Hide();
            }
            else
            {
                Show();
            }
        }


        // ====== Getters ======

        /// <summary>
        /// UI가 현재 표시되고 있는지 여부
        /// </summary>
        public bool IsVisible => panel != null && panel.activeSelf;

        /// <summary>
        /// Panel GameObject 참조 (읽기 전용)
        /// </summary>
        public GameObject Panel => panel;


        // ====== Context Menu (디버그) ======

        [ContextMenu("Show UI")]
        protected void DebugShow()
        {
            Show();
        }

        [ContextMenu("Hide UI")]
        protected void DebugHide()
        {
            Hide();
        }

        [ContextMenu("Toggle UI")]
        protected void DebugToggle()
        {
            Toggle();
        }
    }
}
