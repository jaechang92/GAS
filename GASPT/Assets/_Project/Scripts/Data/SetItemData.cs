using System;
using System.Collections.Generic;
using UnityEngine;
using GASPT.Core.Enums;

namespace GASPT.Data
{
    /// <summary>
    /// 세트 아이템 데이터 (ScriptableObject)
    /// 세트 효과 정의
    /// </summary>
    [CreateAssetMenu(fileName = "SetItemData", menuName = "GASPT/Items/SetItemData")]
    public class SetItemData : ScriptableObject
    {
        // ====== 세트 정보 ======

        [Header("세트 정보")]
        [Tooltip("세트 고유 ID")]
        public string setId;

        [Tooltip("세트 이름 (UI 표시용)")]
        public string setName;

        [TextArea(2, 3)]
        [Tooltip("세트 설명")]
        public string description;

        [Tooltip("세트에 포함된 아이템 ID 목록")]
        public List<string> itemIds = new List<string>();


        // ====== 세트 보너스 ======

        [Header("세트 보너스")]
        [Tooltip("세트 보너스 목록 (2세트, 4세트 등)")]
        public List<SetBonus> bonuses = new List<SetBonus>();


        // ====== 프로퍼티 ======

        /// <summary>
        /// 세트 아이템 개수
        /// </summary>
        public int PieceCount => itemIds?.Count ?? 0;


        // ====== Unity 콜백 ======

        private void OnValidate()
        {
            // setId 자동 생성
            if (string.IsNullOrEmpty(setId))
            {
                setId = $"SET_{name}";
            }

            // 보너스 정렬 (필요 피스 수 기준)
            if (bonuses != null)
            {
                bonuses.Sort((a, b) => a.requiredPieces.CompareTo(b.requiredPieces));
            }
        }


        // ====== 메서드 ======

        /// <summary>
        /// 특정 피스 수에서 활성화되는 보너스 반환
        /// </summary>
        /// <param name="equippedPieces">장착된 세트 피스 수</param>
        /// <returns>활성화된 보너스 목록</returns>
        public List<SetBonus> GetActiveBonuses(int equippedPieces)
        {
            List<SetBonus> active = new List<SetBonus>();

            foreach (SetBonus bonus in bonuses)
            {
                if (equippedPieces >= bonus.requiredPieces)
                {
                    active.Add(bonus);
                }
            }

            return active;
        }

        /// <summary>
        /// 세트에 아이템이 포함되어 있는지 확인
        /// </summary>
        /// <param name="itemId">아이템 ID</param>
        /// <returns>true = 포함</returns>
        public bool ContainsItem(string itemId)
        {
            return itemIds != null && itemIds.Contains(itemId);
        }
    }


    /// <summary>
    /// 세트 보너스 정의
    /// </summary>
    [Serializable]
    public class SetBonus
    {
        [Tooltip("필요 세트 피스 수 (2, 4 등)")]
        [Min(2)]
        public int requiredPieces = 2;

        [Tooltip("세트 효과 설명")]
        public string bonusDescription;

        [Tooltip("스탯 보너스 목록")]
        public List<StatModifier> statBonuses = new List<StatModifier>();


        /// <summary>
        /// 기본 생성자
        /// </summary>
        public SetBonus()
        {
        }

        /// <summary>
        /// 생성자
        /// </summary>
        public SetBonus(int requiredPieces, string description)
        {
            this.requiredPieces = requiredPieces;
            this.bonusDescription = description;
        }
    }
}
