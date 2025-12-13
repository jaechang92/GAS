using System;
using UnityEngine;
using GASPT.StatusEffects;
using GASPT.Core.Enums;

namespace GASPT.UI
{
    /// <summary>
    /// BuffIconPanel Presenter (MVP 패턴)
    /// Pure C# - Unity 없이 테스트 가능
    /// 비즈니스 로직: Model(StatusEffectManager) ↔ View(IBuffIconPanelView) 조율
    /// </summary>
    public class BuffIconPanelPresenter
    {
        // ====== 참조 ======

        private readonly IBuffIconPanelView view;
        private GameObject targetObject; // Player 등

        // ====== 생성자 ======

        /// <summary>
        /// Presenter 생성자
        /// </summary>
        /// <param name="view">View 인터페이스</param>
        public BuffIconPanelPresenter(IBuffIconPanelView view)
        {
            this.view = view ?? throw new ArgumentNullException(nameof(view));
        }

        // ====== 초기화 ======

        /// <summary>
        /// Presenter 초기화
        /// </summary>
        public void Initialize(GameObject target)
        {
            targetObject = target;

            // StatusEffectManager 이벤트 구독
            SubscribeToEvents();

            // 초기 상태 로드 (이미 적용된 효과가 있을 수 있음)
            ReloadActiveEffects();

            Debug.Log($"[BuffIconPanelPresenter] 초기화 완료: Target={target?.name ?? "null"}");
        }

        /// <summary>
        /// Presenter 해제
        /// </summary>
        public void Dispose()
        {
            UnsubscribeFromEvents();
            targetObject = null;
        }

        // ====== 이벤트 구독 ======

        private void SubscribeToEvents()
        {
            if (StatusEffectManager.HasInstance)
            {
                StatusEffectManager.Instance.OnEffectApplied += OnEffectApplied;
                StatusEffectManager.Instance.OnEffectRemoved += OnEffectRemoved;
                StatusEffectManager.Instance.OnEffectStacked += OnEffectStacked;

                Debug.Log("[BuffIconPanelPresenter] StatusEffectManager 이벤트 구독 완료");
            }
            else
            {
                Debug.LogWarning("[BuffIconPanelPresenter] StatusEffectManager가 없어서 이벤트를 구독할 수 없습니다.");
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

        /// <summary>
        /// 상태 효과 적용 (버프/디버프 추가)
        /// </summary>
        private void OnEffectApplied(GameObject target, StatusEffect effect)
        {
            // 타겟 오브젝트가 아니면 무시
            if (targetObject != null && target != targetObject)
                return;

            Debug.Log($"[BuffIconPanelPresenter] OnEffectApplied: {effect.EffectType} on {target.name}");

            // ViewModel 생성
            var viewModel = new BuffIconViewModel(effect);

            // View 업데이트
            view.ShowBuffIcon(viewModel);
        }

        /// <summary>
        /// 상태 효과 제거 (버프/디버프 제거)
        /// </summary>
        private void OnEffectRemoved(GameObject target, StatusEffect effect)
        {
            // 타겟 오브젝트가 아니면 무시
            if (targetObject != null && target != targetObject)
                return;

            Debug.Log($"[BuffIconPanelPresenter] OnEffectRemoved: {effect.EffectType} on {target.name}");

            // View 업데이트
            view.HideBuffIcon(effect.EffectType);
        }

        /// <summary>
        /// 상태 효과 스택 변경
        /// </summary>
        private void OnEffectStacked(GameObject target, StatusEffect effect, int newStack)
        {
            // 타겟 오브젝트가 아니면 무시
            if (targetObject != null && target != targetObject)
                return;

            Debug.Log($"[BuffIconPanelPresenter] OnEffectStacked: {effect.EffectType} stack={newStack} on {target.name}");

            // View 업데이트
            view.UpdateBuffStack(effect.EffectType, newStack);
        }

        // ====== Public 메서드 ======

        /// <summary>
        /// 타겟 오브젝트 변경
        /// </summary>
        public void SetTarget(GameObject target)
        {
            targetObject = target;

            // 기존 아이콘 모두 숨김
            view.ClearAllIcons();

            // 새 타겟의 활성 효과 로드
            ReloadActiveEffects();

            Debug.Log($"[BuffIconPanelPresenter] 타겟 변경: {target?.name ?? "null"}");
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
                var viewModel = new BuffIconViewModel(effect);
                view.ShowBuffIcon(viewModel);
            }

            Debug.Log($"[BuffIconPanelPresenter] 활성 효과 로드 완료: {activeEffects.Count}개");
        }

        /// <summary>
        /// 모든 아이콘 숨김
        /// </summary>
        public void ClearAllIcons()
        {
            view.ClearAllIcons();
        }
    }
}
