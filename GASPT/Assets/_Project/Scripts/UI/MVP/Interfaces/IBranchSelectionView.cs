using System;
using System.Collections.Generic;
using GASPT.Gameplay.Level.Graph;

namespace GASPT.UI.MVP
{
    /// <summary>
    /// 분기 선택 UI 뷰 인터페이스
    /// </summary>
    public interface IBranchSelectionView
    {
        /// <summary>
        /// UI 표시
        /// </summary>
        void Show();

        /// <summary>
        /// UI 숨김
        /// </summary>
        void Hide();

        /// <summary>
        /// 선택 옵션 설정
        /// </summary>
        void SetOptions(List<BranchOptionData> options);

        /// <summary>
        /// 현재 선택 인덱스 설정
        /// </summary>
        void SetSelectedIndex(int index);

        /// <summary>
        /// 옵션 선택 이벤트
        /// </summary>
        event Action<int> OnOptionSelected;

        /// <summary>
        /// 취소 이벤트
        /// </summary>
        event Action OnCancelled;
    }


    /// <summary>
    /// 분기 선택 옵션 데이터
    /// </summary>
    [System.Serializable]
    public class BranchOptionData
    {
        /// <summary>
        /// 연결된 노드
        /// </summary>
        public DungeonNode node;

        /// <summary>
        /// 방 타입 이름
        /// </summary>
        public string typeName;

        /// <summary>
        /// 방 타입 아이콘 이름
        /// </summary>
        public string iconName;

        /// <summary>
        /// 난이도 표시 텍스트
        /// </summary>
        public string difficultyText;

        /// <summary>
        /// 보상 힌트 텍스트
        /// </summary>
        public string rewardHint;


        public BranchOptionData(DungeonNode node)
        {
            this.node = node;
            this.typeName = GetTypeName(node.roomType);
            this.iconName = GetIconName(node.roomType);
            this.difficultyText = GetDifficultyText(node);
            this.rewardHint = GetRewardHint(node.roomType);
        }

        private static string GetTypeName(Gameplay.Level.RoomType type)
        {
            return type switch
            {
                Gameplay.Level.RoomType.Normal => "전투",
                Gameplay.Level.RoomType.Elite => "엘리트",
                Gameplay.Level.RoomType.Boss => "보스",
                Gameplay.Level.RoomType.Shop => "상점",
                Gameplay.Level.RoomType.Rest => "휴식",
                Gameplay.Level.RoomType.Treasure => "보물",
                Gameplay.Level.RoomType.Start => "시작",
                _ => "알 수 없음"
            };
        }

        private static string GetIconName(Gameplay.Level.RoomType type)
        {
            return type switch
            {
                Gameplay.Level.RoomType.Normal => "icon_combat",
                Gameplay.Level.RoomType.Elite => "icon_elite",
                Gameplay.Level.RoomType.Boss => "icon_boss",
                Gameplay.Level.RoomType.Shop => "icon_shop",
                Gameplay.Level.RoomType.Rest => "icon_rest",
                Gameplay.Level.RoomType.Treasure => "icon_treasure",
                _ => "icon_unknown"
            };
        }

        private static string GetDifficultyText(DungeonNode node)
        {
            if (node.roomData == null) return "";

            int difficulty = node.roomData.difficulty;
            return difficulty switch
            {
                <= 3 => "쉬움",
                <= 6 => "보통",
                <= 8 => "어려움",
                _ => "매우 어려움"
            };
        }

        private static string GetRewardHint(Gameplay.Level.RoomType type)
        {
            return type switch
            {
                Gameplay.Level.RoomType.Normal => "골드, 경험치",
                Gameplay.Level.RoomType.Elite => "희귀 보상",
                Gameplay.Level.RoomType.Boss => "스테이지 클리어",
                Gameplay.Level.RoomType.Shop => "아이템 구매",
                Gameplay.Level.RoomType.Rest => "체력 회복",
                Gameplay.Level.RoomType.Treasure => "보물 상자",
                _ => ""
            };
        }
    }
}
