using System;
using Core.Enums;

namespace GASPT.UI
{
    /// <summary>
    /// BuffIconPanel View 인터페이스 (MVP 패턴)
    /// Presenter가 View를 제어하기 위한 계약
    /// </summary>
    public interface IBuffIconPanelView
    {
        // ====== 이벤트 (View → Presenter) ======
        // BuffIconPanel은 사용자 입력이 없으므로 이벤트 없음

        // ====== View 제어 메서드 (Presenter → View) ======

        /// <summary>
        /// 버프 아이콘 표시
        /// </summary>
        void ShowBuffIcon(BuffIconViewModel viewModel);

        /// <summary>
        /// 버프 아이콘 숨김
        /// </summary>
        void HideBuffIcon(StatusEffectType effectType);

        /// <summary>
        /// 버프 스택 수 업데이트
        /// </summary>
        void UpdateBuffStack(StatusEffectType effectType, int stackCount);

        /// <summary>
        /// 모든 버프 아이콘 숨김
        /// </summary>
        void ClearAllIcons();

        /// <summary>
        /// View 표시/숨김
        /// </summary>
        void Show();
        void Hide();
    }
}
