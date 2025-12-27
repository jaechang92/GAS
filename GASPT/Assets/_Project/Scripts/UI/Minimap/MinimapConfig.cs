using UnityEngine;
using GASPT.Core.Enums;

namespace GASPT.UI.Minimap
{
    /// <summary>
    /// 미니맵 설정 ScriptableObject
    /// 미니맵 표시 방식과 스타일을 정의
    /// </summary>
    [CreateAssetMenu(fileName = "MinimapConfig", menuName = "GASPT/UI/Minimap Config")]
    public class MinimapConfig : ScriptableObject
    {
        // ====== 레이아웃 설정 ======

        [Header("레이아웃")]
        [Tooltip("노드 간 수평 간격")]
        public float nodeSpacingX = 80f;

        [Tooltip("노드 간 수직 간격 (층 간 거리)")]
        public float nodeSpacingY = 60f;

        [Tooltip("미니맵 패딩")]
        public Vector2 padding = new Vector2(20f, 20f);

        [Tooltip("미니맵 최대 크기")]
        public Vector2 maxSize = new Vector2(400f, 600f);


        // ====== 노드 스타일 ======

        [Header("노드 크기")]
        [Tooltip("기본 노드 크기")]
        public float nodeSize = 24f;

        [Tooltip("현재 위치 노드 크기 배율")]
        public float currentNodeScale = 1.3f;

        [Tooltip("보스 노드 크기 배율")]
        public float bossNodeScale = 1.5f;


        // ====== 색상 설정 ======

        [Header("노드 색상 - 상태별")]
        public Color hiddenColor = new Color(0.3f, 0.3f, 0.3f, 0.3f);
        public Color revealedColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        public Color unvisitedColor = new Color(0.7f, 0.7f, 0.7f, 1f);
        public Color visitedColor = new Color(0.4f, 0.6f, 0.8f, 1f);
        public Color currentColor = new Color(1f, 0.8f, 0.2f, 1f);
        public Color clearedColor = new Color(0.3f, 0.8f, 0.3f, 1f);

        [Header("노드 색상 - 방 타입별")]
        public Color normalRoomColor = Color.white;
        public Color eliteRoomColor = new Color(0.8f, 0.4f, 0.8f, 1f);
        public Color bossRoomColor = new Color(1f, 0.3f, 0.3f, 1f);
        public Color shopRoomColor = new Color(0.3f, 0.8f, 0.3f, 1f);
        public Color restRoomColor = new Color(0.5f, 0.8f, 1f, 1f);
        public Color treasureRoomColor = new Color(1f, 0.85f, 0.3f, 1f);
        public Color startRoomColor = new Color(0.6f, 0.6f, 0.6f, 1f);


        // ====== 연결선 설정 ======

        [Header("연결선")]
        [Tooltip("연결선 두께")]
        public float lineWidth = 2f;

        [Tooltip("미발견 연결선 색상")]
        public Color hiddenLineColor = new Color(0.3f, 0.3f, 0.3f, 0.2f);

        [Tooltip("발견된 연결선 색상")]
        public Color revealedLineColor = new Color(0.5f, 0.5f, 0.5f, 0.6f);

        [Tooltip("이동한 연결선 색상")]
        public Color traversedLineColor = new Color(0.4f, 0.6f, 0.8f, 1f);

        [Tooltip("비밀 연결선 색상")]
        public Color secretLineColor = new Color(0.8f, 0.4f, 0.8f, 0.5f);


        // ====== 아이콘 설정 ======

        [Header("아이콘")]
        public Sprite normalRoomIcon;
        public Sprite eliteRoomIcon;
        public Sprite bossRoomIcon;
        public Sprite shopRoomIcon;
        public Sprite restRoomIcon;
        public Sprite treasureRoomIcon;
        public Sprite startRoomIcon;
        public Sprite unknownRoomIcon;

        [Tooltip("현재 위치 마커 아이콘")]
        public Sprite currentMarkerIcon;

        [Tooltip("선택 가능 표시 아이콘")]
        public Sprite selectableMarkerIcon;


        // ====== 애니메이션 설정 ======

        [Header("애니메이션")]
        [Tooltip("노드 선택 시 펄스 애니메이션 속도")]
        public float pulseSpeed = 2f;

        [Tooltip("펄스 애니메이션 크기 범위")]
        public float pulseAmount = 0.15f;

        [Tooltip("노드 발견 시 페이드인 시간")]
        public float revealFadeDuration = 0.3f;

        [Tooltip("현재 위치 이동 시 애니메이션 시간")]
        public float moveAnimationDuration = 0.5f;


        // ====== 상호작용 설정 ======

        [Header("상호작용")]
        [Tooltip("노드 호버 시 확대 배율")]
        public float hoverScale = 1.15f;

        [Tooltip("노드 클릭 가능 여부")]
        public bool allowNodeClick = true;

        [Tooltip("미니맵 드래그 가능 여부")]
        public bool allowDrag = true;

        [Tooltip("미니맵 줌 가능 여부")]
        public bool allowZoom = true;

        [Tooltip("최소 줌 레벨")]
        public float minZoom = 0.5f;

        [Tooltip("최대 줌 레벨")]
        public float maxZoom = 2f;


        // ====== 헬퍼 메서드 ======

        /// <summary>
        /// 상태에 따른 노드 색상 반환
        /// </summary>
        public Color GetStateColor(MinimapNodeState state)
        {
            return state switch
            {
                MinimapNodeState.Hidden => hiddenColor,
                MinimapNodeState.Revealed => revealedColor,
                MinimapNodeState.Unvisited => unvisitedColor,
                MinimapNodeState.Visited => visitedColor,
                MinimapNodeState.Current => currentColor,
                MinimapNodeState.Cleared => clearedColor,
                _ => unvisitedColor
            };
        }

        /// <summary>
        /// 방 타입에 따른 색상 반환
        /// </summary>
        public Color GetRoomTypeColor(RoomType type)
        {
            return type switch
            {
                RoomType.Normal => normalRoomColor,
                RoomType.Elite => eliteRoomColor,
                RoomType.Boss => bossRoomColor,
                RoomType.Shop => shopRoomColor,
                RoomType.Rest => restRoomColor,
                RoomType.Treasure => treasureRoomColor,
                RoomType.Start => startRoomColor,
                _ => normalRoomColor
            };
        }

        /// <summary>
        /// 방 타입에 따른 아이콘 반환
        /// </summary>
        public Sprite GetRoomTypeIcon(RoomType type)
        {
            return type switch
            {
                RoomType.Normal => normalRoomIcon,
                RoomType.Elite => eliteRoomIcon,
                RoomType.Boss => bossRoomIcon,
                RoomType.Shop => shopRoomIcon,
                RoomType.Rest => restRoomIcon,
                RoomType.Treasure => treasureRoomIcon,
                RoomType.Start => startRoomIcon,
                _ => unknownRoomIcon
            };
        }

        /// <summary>
        /// 방 타입에 따른 노드 크기 배율 반환
        /// </summary>
        public float GetNodeScaleForType(RoomType type)
        {
            return type switch
            {
                RoomType.Boss => bossNodeScale,
                _ => 1f
            };
        }
    }
}
