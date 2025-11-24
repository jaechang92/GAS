using System;
using UnityEngine;

namespace GASPT.UI
{
    /// <summary>
    /// ResourceBar View 인터페이스 (MVP 패턴)
    /// Presenter가 View를 제어하기 위한 계약
    /// </summary>
    public interface IResourceBarView
    {
        // ====== 이벤트 (View → Presenter) ======
        // ResourceBar는 사용자 입력이 없으므로 이벤트 없음

        // ====== View 제어 메서드 (Presenter → View) ======

        /// <summary>
        /// 리소스 바 업데이트 (슬라이더 + 텍스트)
        /// </summary>
        void UpdateResourceBar(ResourceBarViewModel viewModel);

        /// <summary>
        /// 색상 플래시 효과 (데미지, 회복 등)
        /// </summary>
        void FlashColor(Color flashColor, Color normalColor, float duration);

        /// <summary>
        /// 바 색상 즉시 변경 (플래시 없이)
        /// </summary>
        void SetBarColor(Color color);

        /// <summary>
        /// View 표시/숨김
        /// </summary>
        void Show();
        void Hide();
    }
}
