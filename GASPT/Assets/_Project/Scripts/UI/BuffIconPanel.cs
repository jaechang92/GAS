using UnityEngine;
using System.Collections.Generic;
using GASPT.StatusEffects;
using Core.Enums;

namespace GASPT.UI
{
    /// <summary>
    /// 버프/디버프 아이콘 패널
    /// 여러 BuffIcon을 관리하고 StatusEffectManager 이벤트 구독
    /// </summary>
    public class BuffIconPanel : MonoBehaviour
    {
        // ====== 설정 ======

        [Header("References")]
        [SerializeField] private GameObject buffIconPrefab;
        [SerializeField] private Transform iconContainer;

        [Header("Settings")]
        [SerializeField] private int maxIcons = 10;
        [SerializeField] private GameObject targetObject; // Player 등


        // ====== 내부 상태 ======

        private List<BuffIcon> iconPool = new List<BuffIcon>();
        private Dictionary<StatusEffectType, BuffIcon> activeIcons = new Dictionary<StatusEffectType, BuffIcon>();


        // ====== 초기화 ======

        private void Start()
        {
            InitializeIconPool();
            SubscribeToEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }


        // ====== 초기화 메서드 ======

        private void InitializeIconPool()
        {
            if (buffIconPrefab == null)
            {
                Debug.LogError("[BuffIconPanel] buffIconPrefab이 할당되지 않았습니다!");
                return;
            }

            if (iconContainer == null)
            {
                iconContainer = transform;
            }

            // 풀 생성
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
                    Debug.LogError("[BuffIconPanel] BuffIcon 컴포넌트가 프리팹에 없습니다!");
                }
            }
        }

        // ====== 이벤트 구독 ======

        private void SubscribeToEvents()
        {
            if (StatusEffectManager.HasInstance)
            {
                StatusEffectManager.Instance.OnEffectApplied += OnEffectApplied;
                StatusEffectManager.Instance.OnEffectRemoved += OnEffectRemoved;
                StatusEffectManager.Instance.OnEffectStacked += OnEffectStacked;
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (StatusEffectManager.HasInstance)
            {
                StatusEffectManager.Instance.OnEffectApplied -= OnEffectApplied;
                StatusEffectManager.Instance.OnEffectRemoved -= OnEffectRemoved;
                StatusEffectManager.Instance.OnEffectStacked -= OnEffectStacked;
            }
        }


        // ====== 이벤트 핸들러 ======

        private void OnEffectApplied(GameObject target, StatusEffect effect)
        {
            // 타겟 오브젝트가 아니면 무시
            if (targetObject != null && target != targetObject)
                return;

            // 이미 표시 중이면 무시
            if (activeIcons.ContainsKey(effect.EffectType))
                return;

            ShowIcon(effect);
        }

        private void OnEffectRemoved(GameObject target, StatusEffect effect)
        {
            // 타겟 오브젝트가 아니면 무시
            if (targetObject != null && target != targetObject)
                return;

            HideIcon(effect.EffectType);
        }

        private void OnEffectStacked(GameObject target, StatusEffect effect, int newStack)
        {
            // 타겟 오브젝트가 아니면 무시
            if (targetObject != null && target != targetObject)
                return;

            // 스택 수 업데이트
            if (activeIcons.TryGetValue(effect.EffectType, out BuffIcon icon))
            {
                icon.UpdateStack(newStack);
            }
        }


        // ====== 아이콘 표시/숨김 ======

        private void ShowIcon(StatusEffect effect)
        {
            // 사용 가능한 아이콘 찾기
            BuffIcon availableIcon = GetAvailableIcon();
            if (availableIcon == null)
            {
                Debug.LogWarning("[BuffIconPanel] 사용 가능한 아이콘이 없습니다!");
                return;
            }

            // 아이콘 표시 (StatusEffect에서 직접 icon과 isBuff 가져오기)
            availableIcon.Show(effect, effect.Icon, effect.IsBuff);
            availableIcon.UpdateStack(effect.StackCount);

            activeIcons[effect.EffectType] = availableIcon;
        }

        private void HideIcon(StatusEffectType effectType)
        {
            if (activeIcons.TryGetValue(effectType, out BuffIcon icon))
            {
                icon.Hide();
                activeIcons.Remove(effectType);
            }
        }

        private BuffIcon GetAvailableIcon()
        {
            foreach (var icon in iconPool)
            {
                if (!icon.IsActive)
                    return icon;
            }
            return null;
        }


        // ====== Public 메서드 ======

        /// <summary>
        /// 타겟 오브젝트 설정
        /// </summary>
        public void SetTarget(GameObject target)
        {
            targetObject = target;

            // 기존 아이콘 모두 숨김
            ClearAllIcons();

            // 타겟의 활성 효과 다시 로드
            ReloadActiveEffects();
        }

        /// <summary>
        /// 모든 아이콘 숨김
        /// </summary>
        public void ClearAllIcons()
        {
            foreach (var icon in iconPool)
            {
                icon.Hide();
            }
            activeIcons.Clear();
        }

        /// <summary>
        /// 활성 효과 다시 로드
        /// </summary>
        private void ReloadActiveEffects()
        {
            if (targetObject == null || !StatusEffectManager.HasInstance)
                return;

            var activeEffects = StatusEffectManager.Instance.GetActiveEffects(targetObject);
            foreach (var effect in activeEffects)
            {
                ShowIcon(effect);
            }
        }


        // ====== Context Menu (테스트용) ======

        [ContextMenu("Print Active Icons")]
        private void PrintActiveIcons()
        {
            Debug.Log($"[BuffIconPanel] 활성 아이콘: {activeIcons.Count}개");
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
            ReloadActiveEffects();
        }
    }
}
