using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using GASPT.Core.Enums;
using GASPT.Core.Pooling;

namespace GASPT.UI.Minimap
{
    /// <summary>
    /// 미니맵 노드 UI 컴포넌트
    /// 개별 노드의 시각적 표현을 담당
    /// </summary>
    public class MinimapNodeUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IPoolable
    {
        // ====== UI 요소 ======

        [Header("UI 요소")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image iconImage;
        [SerializeField] private Image borderImage;
        [SerializeField] private Image highlightImage;
        [SerializeField] private TextMeshProUGUI floorText;

        [Header("현재 위치 마커")]
        [SerializeField] private GameObject currentMarker;

        [Header("선택 가능 마커")]
        [SerializeField] private GameObject selectableMarker;


        // ====== 설정 참조 ======

        private MinimapConfig config;


        // ====== 상태 ======

        private MinimapNodeData nodeData;
        private bool isHighlighted;
        private bool isSelectable;
        private bool isCurrent;


        // ====== 애니메이션 ======

        private float pulseTimer;
        private Vector3 baseScale;


        // ====== 이벤트 ======

        public event Action<string> OnClicked;
        public event Action<string> OnHoverEnter;
        public event Action OnHoverExit;


        // ====== 프로퍼티 ======

        public string NodeId => nodeData.nodeId;
        public MinimapNodeData Data => nodeData;


        // ====== Unity 생명주기 ======

        private void Awake()
        {
            baseScale = transform.localScale;

            // 마커 초기 숨김
            if (currentMarker != null) currentMarker.SetActive(false);
            if (selectableMarker != null) selectableMarker.SetActive(false);
            if (highlightImage != null) highlightImage.enabled = false;
        }

        private void Update()
        {
            // 현재 노드 펄스 애니메이션
            if (isCurrent && config != null)
            {
                pulseTimer += Time.deltaTime * config.pulseSpeed;
                float pulse = 1f + Mathf.Sin(pulseTimer * Mathf.PI * 2f) * config.pulseAmount;
                transform.localScale = baseScale * pulse * config.currentNodeScale;
            }
        }


        // ====== 초기화 ======

        /// <summary>
        /// 노드 UI 초기화
        /// </summary>
        public void Initialize(MinimapNodeData data, MinimapConfig minimapConfig)
        {
            nodeData = data;
            config = minimapConfig;

            UpdateVisuals();
        }

        /// <summary>
        /// 노드 데이터 업데이트
        /// </summary>
        public void UpdateData(MinimapNodeData data)
        {
            nodeData = data;
            UpdateVisuals();
        }


        // ====== 시각적 업데이트 ======

        private void UpdateVisuals()
        {
            if (config == null) return;

            // 표시 여부
            gameObject.SetActive(nodeData.IsVisible);

            if (!nodeData.IsVisible) return;

            // 위치 설정
            var rectTransform = GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = nodeData.position;
            }

            // 크기 설정
            float typeScale = config.GetNodeScaleForType(nodeData.roomType);
            baseScale = Vector3.one * config.nodeSize * typeScale;
            transform.localScale = baseScale;

            // 색상 설정
            UpdateColors();

            // 아이콘 설정
            UpdateIcon();

            // 층 텍스트
            if (floorText != null)
            {
                floorText.text = nodeData.floor.ToString();
                floorText.enabled = nodeData.state != MinimapNodeState.Hidden;
            }

            // 현재 위치 마커
            UpdateCurrentMarker();

            // 선택 가능 마커
            UpdateSelectableMarker();
        }

        private void UpdateColors()
        {
            Color stateColor = config.GetStateColor(nodeData.state);
            Color typeColor = config.GetRoomTypeColor(nodeData.roomType);

            // 배경: 상태 색상
            if (backgroundImage != null)
            {
                backgroundImage.color = stateColor;
            }

            // 테두리: 방 타입 색상
            if (borderImage != null)
            {
                borderImage.color = typeColor;
                // 숨겨진 상태면 테두리도 연하게
                if (nodeData.state == MinimapNodeState.Hidden || nodeData.state == MinimapNodeState.Revealed)
                {
                    var borderColor = borderImage.color;
                    borderColor.a *= 0.5f;
                    borderImage.color = borderColor;
                }
            }

            // 아이콘: 방 타입 색상
            if (iconImage != null)
            {
                iconImage.color = typeColor;
            }
        }

        private void UpdateIcon()
        {
            if (iconImage == null || config == null) return;

            // 숨겨진 상태면 물음표 아이콘
            if (nodeData.state == MinimapNodeState.Hidden || nodeData.state == MinimapNodeState.Revealed)
            {
                iconImage.sprite = config.unknownRoomIcon;
            }
            else
            {
                iconImage.sprite = config.GetRoomTypeIcon(nodeData.roomType);
            }

            // 아이콘이 없으면 숨김
            iconImage.enabled = iconImage.sprite != null;
        }

        private void UpdateCurrentMarker()
        {
            isCurrent = nodeData.state == MinimapNodeState.Current;

            if (currentMarker != null)
            {
                currentMarker.SetActive(isCurrent);
            }

            // 현재 노드면 스케일 리셋 (펄스 애니메이션 시작)
            if (isCurrent)
            {
                pulseTimer = 0f;
            }
            else
            {
                transform.localScale = baseScale;
            }
        }

        private void UpdateSelectableMarker()
        {
            if (selectableMarker != null)
            {
                selectableMarker.SetActive(isSelectable);
            }
        }


        // ====== 상태 변경 ======

        /// <summary>
        /// 현재 노드로 설정
        /// </summary>
        public void SetAsCurrent(bool current)
        {
            isCurrent = current;
            nodeData = nodeData.WithState(current ? MinimapNodeState.Current : MinimapNodeState.Visited);
            UpdateCurrentMarker();
            UpdateColors();
        }

        /// <summary>
        /// 선택 가능 상태 설정
        /// </summary>
        public void SetSelectable(bool selectable)
        {
            isSelectable = selectable;
            UpdateSelectableMarker();
        }

        /// <summary>
        /// 하이라이트 설정
        /// </summary>
        public void SetHighlighted(bool highlighted)
        {
            isHighlighted = highlighted;

            if (highlightImage != null)
            {
                highlightImage.enabled = highlighted;
            }

            // 하이라이트 시 스케일 업
            if (config != null && !isCurrent)
            {
                transform.localScale = baseScale * (highlighted ? config.hoverScale : 1f);
            }
        }


        // ====== 이벤트 핸들러 ======

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!config.allowNodeClick) return;

            SetHighlighted(true);
            OnHoverEnter?.Invoke(nodeData.nodeId);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            SetHighlighted(false);
            OnHoverExit?.Invoke();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!config.allowNodeClick) return;
            if (!isSelectable && !isCurrent) return;

            OnClicked?.Invoke(nodeData.nodeId);
        }

        // ====== IPoolable 구현 ======

        /// <summary>
        /// 풀에서 꺼내질 때 호출
        /// </summary>
        public void OnSpawn()
        {
            // 이벤트는 MinimapView에서 다시 등록됨
            isHighlighted = false;
            isSelectable = false;
            isCurrent = false;
            pulseTimer = 0f;
        }

        /// <summary>
        /// 풀로 반환될 때 호출
        /// </summary>
        public void OnDespawn()
        {
            // 이벤트 정리
            OnClicked = null;
            OnHoverEnter = null;
            OnHoverExit = null;

            // 마커 숨김
            if (currentMarker != null) currentMarker.SetActive(false);
            if (selectableMarker != null) selectableMarker.SetActive(false);
            if (highlightImage != null) highlightImage.enabled = false;

            // 스케일 초기화
            transform.localScale = Vector3.one;
        }
    }
}