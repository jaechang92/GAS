using System;
using System.Collections.Generic;

namespace GASPT.UI.Minimap
{
    /// <summary>
    /// 미니맵 뷰 인터페이스
    /// MVP 패턴의 View 역할
    /// </summary>
    public interface IMinimapView
    {
        /// <summary>
        /// 미니맵 표시
        /// </summary>
        void Show();

        /// <summary>
        /// 미니맵 숨김
        /// </summary>
        void Hide();

        /// <summary>
        /// 표시 여부
        /// </summary>
        bool IsVisible { get; }

        /// <summary>
        /// 전체 맵 데이터 설정 (초기화용)
        /// </summary>
        void SetMapData(List<MinimapNodeData> nodes, List<MinimapEdgeData> edges);

        /// <summary>
        /// 단일 노드 업데이트
        /// </summary>
        void UpdateNode(MinimapNodeData node);

        /// <summary>
        /// 여러 노드 업데이트
        /// </summary>
        void UpdateNodes(List<MinimapNodeData> nodes);

        /// <summary>
        /// 현재 위치 노드 설정
        /// </summary>
        void SetCurrentNode(string nodeId);

        /// <summary>
        /// 선택 가능 노드 하이라이트
        /// </summary>
        void HighlightSelectableNodes(List<string> nodeIds);

        /// <summary>
        /// 하이라이트 해제
        /// </summary>
        void ClearHighlights();

        /// <summary>
        /// 특정 노드로 포커스 이동 (카메라/스크롤 이동)
        /// </summary>
        void FocusOnNode(string nodeId, bool animate = true);

        /// <summary>
        /// 전체 맵이 보이도록 줌 조정
        /// </summary>
        void FitToView();

        /// <summary>
        /// 줌 레벨 설정
        /// </summary>
        void SetZoom(float zoomLevel);

        /// <summary>
        /// 미니맵 리셋 (초기 상태로)
        /// </summary>
        void Reset();


        // ====== 이벤트 ======

        /// <summary>
        /// 노드 클릭 이벤트
        /// </summary>
        event Action<string> OnNodeClicked;

        /// <summary>
        /// 노드 호버 이벤트
        /// </summary>
        event Action<string> OnNodeHovered;

        /// <summary>
        /// 노드 호버 종료 이벤트
        /// </summary>
        event Action OnNodeHoverExit;
    }
}
