using System;
using GASPT.UI.MVP.ViewModels;

namespace GASPT.UI.MVP.Views
{
    /// <summary>
    /// 런 결과 화면 View 인터페이스
    /// MVP 패턴에서 Presenter가 View를 제어하기 위한 계약
    /// </summary>
    public interface IRunResultView
    {
        // ====== View → Presenter 이벤트 ======

        /// <summary>
        /// 로비로 돌아가기 버튼 클릭
        /// </summary>
        event Action OnReturnToLobbyRequested;

        /// <summary>
        /// 재시작 버튼 클릭 (빠른 재시작)
        /// </summary>
        event Action OnRestartRequested;

        /// <summary>
        /// View가 열릴 때 발생
        /// </summary>
        event Action OnViewOpened;

        /// <summary>
        /// View가 닫힐 때 발생
        /// </summary>
        event Action OnViewClosed;


        // ====== Presenter → View 속성 ======

        /// <summary>
        /// UI 표시 여부
        /// </summary>
        bool IsVisible { get; }


        // ====== Presenter → View 명령 ======

        /// <summary>
        /// 런 결과 데이터 표시
        /// </summary>
        /// <param name="viewModel">표시할 결과 데이터</param>
        void DisplayResults(RunResultViewModel viewModel);

        /// <summary>
        /// UI 표시
        /// </summary>
        void ShowUI();

        /// <summary>
        /// UI 숨김
        /// </summary>
        void HideUI();

        /// <summary>
        /// 재화 획득 애니메이션 재생 (선택적)
        /// </summary>
        /// <param name="bone">획득한 Bone</param>
        /// <param name="soul">획득한 Soul</param>
        void PlayCurrencyAnimation(int bone, int soul);

        /// <summary>
        /// 신기록 달성 이펙트 재생 (선택적)
        /// </summary>
        void PlayNewRecordEffect();

        /// <summary>
        /// 버튼 활성화/비활성화 (애니메이션 중 대기용)
        /// </summary>
        /// <param name="enabled">활성화 여부</param>
        void SetButtonsEnabled(bool enabled);
    }
}
