using UnityEngine;
using UnityEngine.UI;
using GASPT.Gameplay.Level.Graph;
using GASPT.Core.Pooling;

namespace GASPT.UI.Minimap
{
    /// <summary>
    /// 미니맵 엣지(연결선) UI 컴포넌트
    /// 노드 간 연결을 시각적으로 표현
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class MinimapEdgeUI : MonoBehaviour, IPoolable
    {
        // ====== UI 요소 ======

        private Image lineImage;
        private RectTransform rectTransform;


        // ====== 설정 참조 ======

        private MinimapConfig config;


        // ====== 상태 ======

        private MinimapEdgeData edgeData;


        // ====== 프로퍼티 ======

        public string FromNodeId => edgeData.fromNodeId;
        public string ToNodeId => edgeData.toNodeId;
        public MinimapEdgeData Data => edgeData;


        // ====== Unity 생명주기 ======

        private void Awake()
        {
            lineImage = GetComponent<Image>();
            rectTransform = GetComponent<RectTransform>();
        }


        // ====== 초기화 ======

        /// <summary>
        /// 엣지 UI 초기화
        /// </summary>
        public void Initialize(MinimapEdgeData data, MinimapConfig minimapConfig)
        {
            edgeData = data;
            config = minimapConfig;

            UpdateVisuals();
        }

        /// <summary>
        /// 엣지 데이터 업데이트
        /// </summary>
        public void UpdateData(MinimapEdgeData data)
        {
            edgeData = data;
            UpdateVisuals();
        }


        // ====== 시각적 업데이트 ======

        private void UpdateVisuals()
        {
            if (config == null || lineImage == null || rectTransform == null) return;

            // 표시 여부
            gameObject.SetActive(edgeData.isVisible);

            if (!edgeData.isVisible) return;

            // 위치 및 회전 계산
            Vector2 direction = edgeData.toPosition - edgeData.fromPosition;
            float distance = direction.magnitude;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            // 중심점 위치
            Vector2 midPoint = (edgeData.fromPosition + edgeData.toPosition) / 2f;
            rectTransform.anchoredPosition = midPoint;

            // 회전
            rectTransform.rotation = Quaternion.Euler(0, 0, angle);

            // 크기 (길이 x 두께)
            rectTransform.sizeDelta = new Vector2(distance, config.lineWidth);

            // 색상
            UpdateColor();
        }

        private void UpdateColor()
        {
            if (lineImage == null || config == null) return;

            Color lineColor;

            // 비밀 경로
            if (edgeData.edgeType == EdgeType.Secret)
            {
                lineColor = config.secretLineColor;
            }
            // 이동한 경로
            else if (edgeData.isTraversed)
            {
                lineColor = config.traversedLineColor;
            }
            // 발견된 경로
            else if (edgeData.isVisible)
            {
                lineColor = config.revealedLineColor;
            }
            // 숨겨진 경로
            else
            {
                lineColor = config.hiddenLineColor;
            }

            lineImage.color = lineColor;
        }


        // ====== 상태 변경 ======

        /// <summary>
        /// 이동 경로로 표시
        /// </summary>
        public void SetTraversed(bool traversed)
        {
            edgeData.isTraversed = traversed;
            UpdateColor();
        }

        /// <summary>
        /// 하이라이트 설정
        /// </summary>
        public void SetHighlighted(bool highlighted)
        {
            if (lineImage == null || config == null) return;

            if (highlighted)
            {
                lineImage.color = config.currentColor;
                // 두께 증가
                if (rectTransform != null)
                {
                    var size = rectTransform.sizeDelta;
                    size.y = config.lineWidth * 1.5f;
                    rectTransform.sizeDelta = size;
                }
            }
            else
            {
                UpdateColor();
                // 두께 복원
                if (rectTransform != null)
                {
                    var size = rectTransform.sizeDelta;
                    size.y = config.lineWidth;
                    rectTransform.sizeDelta = size;
                }
            }
        }

        // ====== IPoolable 구현 ======

        public void OnSpawn()
        {
            // 풀에서 꺼내질 때 초기화
        }

        public void OnDespawn()
        {
            // 풀로 반환될 때 정리
            if (rectTransform != null)
            {
                rectTransform.sizeDelta = Vector2.one;
                rectTransform.rotation = Quaternion.identity;
            }
        }
    }
}