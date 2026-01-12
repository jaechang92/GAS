using UnityEngine;
using System.Collections.Generic;
using GASPT.Core.Enums;
using GASPT.Core.Extensions;

namespace GASPT.UI
{
    /// <summary>
    /// BuffIconPanel View (MVP 패턴)
    /// MonoBehaviour - 순수 렌더링 책임만 담당
    /// BuffIcon Pool 관리 및 표시/숨김
    /// </summary>
    public class BuffIconPanelView : MonoBehaviour, IBuffIconPanelView
    {
        // ====== UI 참조 ======

        [Header("References")]
        [SerializeField]
        [Tooltip("BuffIcon 프리팹")]
        private GameObject buffIconPrefab;

        [SerializeField]
        [Tooltip("아이콘 컨테이너 (LayoutGroup)")]
        private Transform iconContainer;

        [Header("Settings")]
        [SerializeField]
        [Tooltip("최대 아이콘 개수")]
        private int maxIcons = 10;

        [SerializeField]
        [Tooltip("타겟 오브젝트 (Player 등)")]
        private GameObject targetObject;

        // ====== Presenter ======

        private BuffIconPanelPresenter presenter;

        // ====== 내부 상태 ======

        private List<BuffIcon> iconPool = new List<BuffIcon>();
        private Dictionary<StatusEffectType, BuffIcon> activeIcons = new Dictionary<StatusEffectType, BuffIcon>();

        // ====== Unity 생명주기 ======

        private void Awake()
        {
            ValidateReferences();
        }

        private void Start()
        {
            InitializeIconPool();

            // targetObject가 null이면 자동으로 Player 찾기 후 Presenter 초기화
            if (targetObject == null)
            {
                InitializeWithPlayerSearchAsync().Forget();
            }
            else
            {
                // targetObject가 이미 설정되어 있으면 바로 Presenter 초기화
                InitializePresenter();
            }
        }

        private void OnEnable()
        {
            SubscribeToGameManagerEvents();
        }

        private void OnDisable()
        {
            UnsubscribeFromGameManagerEvents();
        }

        private void OnDestroy()
        {
            presenter?.Dispose();
        }

        // ====== 초기화 ======

        /// <summary>
        /// UI 참조 유효성 검증
        /// </summary>
        private void ValidateReferences()
        {
            if (buffIconPrefab == null)
            {
                Debug.LogError("[BuffIconPanelView] buffIconPrefab이 할당되지 않았습니다!");
            }

            if (iconContainer == null)
            {
                iconContainer = transform;
                Debug.LogWarning("[BuffIconPanelView] iconContainer가 설정되지 않아 자신으로 설정합니다.");
            }
        }

        /// <summary>
        /// BuffIcon Pool 초기화
        /// </summary>
        private void InitializeIconPool()
        {
            if (buffIconPrefab == null)
            {
                Debug.LogError("[BuffIconPanelView] buffIconPrefab이 null이어서 Pool을 생성할 수 없습니다!");
                return;
            }

            // 기존 Pool 정리
            iconPool.Clear();

            // Pool 생성
            for (int i = 0; i < maxIcons; i++)
            {
                GameObject iconObj = Instantiate(buffIconPrefab, iconContainer);
                BuffIcon icon = iconObj.GetComponent<BuffIcon>();

                if (icon != null)
                {
                    icon.Hide();
                    iconPool.Add(icon);
                }
                else
                {
                    Debug.LogError("[BuffIconPanelView] BuffIcon 컴포넌트가 프리팹에 없습니다!");
                    Destroy(iconObj);
                }
            }

            Debug.Log($"[BuffIconPanelView] BuffIcon Pool 생성 완료: {iconPool.Count}개");
        }

        /// <summary>
        /// Presenter 초기화
        /// </summary>
        private void InitializePresenter()
        {
            presenter = new BuffIconPanelPresenter(this);
            presenter.Initialize(targetObject);
        }

        // ====== IBuffIconPanelView 구현 ======

        /// <summary>
        /// 버프 아이콘 표시
        /// </summary>
        public void ShowBuffIcon(BuffIconViewModel viewModel)
        {
            if (viewModel == null)
            {
                Debug.LogWarning("[BuffIconPanelView] viewModel이 null입니다!");
                return;
            }

            // 이미 표시 중이면 무시
            if (activeIcons.ContainsKey(viewModel.EffectType))
            {
                Debug.LogWarning($"[BuffIconPanelView] {viewModel.EffectType}이 이미 표시 중입니다!");
                return;
            }

            // 사용 가능한 아이콘 찾기
            BuffIcon availableIcon = GetAvailableIcon();
            if (availableIcon == null)
            {
                Debug.LogWarning("[BuffIconPanelView] 사용 가능한 아이콘이 없습니다!");
                return;
            }

            // 아이콘 표시
            availableIcon.Show(viewModel.Effect, viewModel.Icon, viewModel.IsBuff);
            availableIcon.UpdateStack(viewModel.StackCount);

            activeIcons[viewModel.EffectType] = availableIcon;

            Debug.Log($"[BuffIconPanelView] ShowBuffIcon: {viewModel}");
        }

        /// <summary>
        /// 버프 아이콘 숨김
        /// </summary>
        public void HideBuffIcon(StatusEffectType effectType)
        {
            if (activeIcons.TryGetValue(effectType, out BuffIcon icon))
            {
                icon.Hide();
                activeIcons.Remove(effectType);

                Debug.Log($"[BuffIconPanelView] HideBuffIcon: {effectType}");
            }
        }

        /// <summary>
        /// 버프 스택 수 업데이트
        /// </summary>
        public void UpdateBuffStack(StatusEffectType effectType, int stackCount)
        {
            if (activeIcons.TryGetValue(effectType, out BuffIcon icon))
            {
                icon.UpdateStack(stackCount);

                Debug.Log($"[BuffIconPanelView] UpdateBuffStack: {effectType} stack={stackCount}");
            }
        }

        /// <summary>
        /// 모든 버프 아이콘 숨김
        /// </summary>
        public void ClearAllIcons()
        {
            foreach (var icon in iconPool)
            {
                icon.Hide();
            }
            activeIcons.Clear();

            Debug.Log("[BuffIconPanelView] ClearAllIcons");
        }

        /// <summary>
        /// View 표시
        /// </summary>
        public void Show()
        {
            gameObject.SetActive(true);
        }

        /// <summary>
        /// View 숨김
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
        }

        // ====== Private 메서드 ======

        /// <summary>
        /// 사용 가능한 아이콘 찾기
        /// </summary>
        private BuffIcon GetAvailableIcon()
        {
            foreach (var icon in iconPool)
            {
                if (!icon.IsActive)
                    return icon;
            }
            return null;
        }

        // ====== GameManager 이벤트 구독 ======

        private void SubscribeToGameManagerEvents()
        {
            if (GASPT.Core.GameManager.HasInstance)
            {
                GASPT.Core.GameManager.Instance.OnPlayerRegistered += OnPlayerRegistered;
                GASPT.Core.GameManager.Instance.OnPlayerUnregistered += OnPlayerUnregistered;
            }
        }

        private void UnsubscribeFromGameManagerEvents()
        {
            if (GASPT.Core.GameManager.HasInstance)
            {
                GASPT.Core.GameManager.Instance.OnPlayerRegistered -= OnPlayerRegistered;
                GASPT.Core.GameManager.Instance.OnPlayerUnregistered -= OnPlayerUnregistered;
            }
        }

        /// <summary>
        /// Player 등록 시 호출 (씬 전환 후 Player 재생성)
        /// </summary>
        private void OnPlayerRegistered(GASPT.Stats.PlayerStats player)
        {
            SetTarget(player.gameObject);
            Debug.Log($"[BuffIconPanelView] Player 참조 갱신 완료 (이벤트): {player.name}");
        }

        /// <summary>
        /// Player 해제 시 호출 (씬 전환 전 Player 파괴)
        /// </summary>
        private void OnPlayerUnregistered()
        {
            ClearAllIcons();
            Debug.Log("[BuffIconPanelView] Player 참조 해제 (이벤트)");
        }

        /// <summary>
        /// Player 자동 검색 후 Presenter 초기화 (비동기)
        /// </summary>
        private async Awaitable InitializeWithPlayerSearchAsync()
        {
            int maxAttempts = 50;
            int attempts = 0;

            while (targetObject == null && attempts < maxAttempts)
            {
                // RunManager 우선
                if (GASPT.Core.RunManager.HasInstance && GASPT.Core.RunManager.Instance.CurrentPlayer != null)
                {
                    targetObject = GASPT.Core.RunManager.Instance.CurrentPlayer.gameObject;
                    Debug.Log("[BuffIconPanelView] RunManager에서 Player 찾기 성공!");
                    break;
                }

                // GameManager 차선
                if (GASPT.Core.GameManager.HasInstance && GASPT.Core.GameManager.Instance.PlayerStats != null)
                {
                    targetObject = GASPT.Core.GameManager.Instance.PlayerStats.gameObject;
                    Debug.Log("[BuffIconPanelView] GameManager에서 Player 찾기 성공!");
                    break;
                }

                await Awaitable.WaitForSecondsAsync(0.1f);
                attempts++;
            }

            if (targetObject == null)
            {
                Debug.LogWarning("[BuffIconPanelView] Player를 찾을 수 없습니다. (타임아웃)");
            }

            // Player를 찾았든 못 찾았든 Presenter 초기화
            InitializePresenter();
        }

        // ====== Public 메서드 ======

        /// <summary>
        /// 타겟 오브젝트 설정
        /// </summary>
        public void SetTarget(GameObject target)
        {
            targetObject = target;
            presenter?.SetTarget(target);
        }

        // ====== Context Menu (테스트용) ======

        [ContextMenu("Print Active Icons")]
        private void PrintActiveIcons()
        {
            Debug.Log($"[BuffIconPanelView] 활성 아이콘: {activeIcons.Count}개");
            foreach (var kvp in activeIcons)
            {
                Debug.Log($"  - {kvp.Key}: {kvp.Value.name}");
            }
        }

        [ContextMenu("Clear All Icons (Test)")]
        private void TestClearAllIcons()
        {
            ClearAllIcons();
        }

        [ContextMenu("Reload Active Effects (Test)")]
        private void TestReloadActiveEffects()
        {
            presenter?.SetTarget(targetObject);
        }

        [ContextMenu("Automatically reference variables")]
        private void AutoReferenceVariables()
        {
            if (iconContainer == null)
            {
                iconContainer = transform;
                Debug.LogWarning("[BuffIconPanelView] iconContainer가 설정되지 않아 자신으로 설정합니다.");
            }
        }
    }
}
