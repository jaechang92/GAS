using System.Collections.Generic;
using UnityEngine;
using GASPT.Core.Enums;
using GASPT.Data;
using GASPT.Inventory;

namespace GASPT.Stats
{
    /// <summary>
    /// PlayerStats V2 시스템 통합
    /// EquipmentManager, SetItemBonusSystem과 연동
    /// </summary>
    public partial class PlayerStats
    {
        // ====== V2 시스템 연동 플래그 ======

        /// <summary>
        /// V2 시스템(EquipmentManager, SetItemBonusSystem) 사용 여부
        /// true: 새 시스템, false: 기존 Item 기반 시스템
        /// </summary>
        [Header("V2 시스템 설정")]
        [SerializeField] [Tooltip("V2 시스템 사용 여부 (EquipmentManager, SetItemBonusSystem)")]
        private bool useV2System = true;

        /// <summary>
        /// V2 이벤트 구독 여부
        /// </summary>
        private bool isV2Subscribed = false;


        // ====== V2 시스템 캐시 ======

        /// <summary>
        /// V2 장비 스탯 캐시
        /// </summary>
        private Dictionary<StatType, float> v2EquipmentStats = new Dictionary<StatType, float>();

        /// <summary>
        /// V2 세트 보너스 캐시
        /// </summary>
        private Dictionary<StatType, float> v2SetBonusStats = new Dictionary<StatType, float>();


        // ====== V2 시스템 초기화 ======

        /// <summary>
        /// V2 시스템 초기화 (Start에서 호출)
        /// </summary>
        private void InitializeV2System()
        {
            if (!useV2System)
                return;

            SubscribeToV2Events();
            RefreshV2Stats();

            Debug.Log("[PlayerStats] V2 시스템 통합 초기화 완료");
        }

        /// <summary>
        /// V2 이벤트 구독
        /// </summary>
        private void SubscribeToV2Events()
        {
            if (isV2Subscribed)
                return;

            // EquipmentManager 이벤트 구독
            if (EquipmentManager.HasInstance)
            {
                EquipmentManager.Instance.OnEquipmentStatsChanged += OnV2EquipmentStatsChanged;
            }

            // SetItemBonusSystem 이벤트 구독
            if (SetItemBonusSystem.HasInstance)
            {
                SetItemBonusSystem.Instance.OnSetStatsChanged += OnV2SetStatsChanged;
            }

            isV2Subscribed = true;
        }

        /// <summary>
        /// V2 이벤트 구독 해제
        /// </summary>
        private void UnsubscribeFromV2Events()
        {
            if (!isV2Subscribed)
                return;

            if (EquipmentManager.HasInstance)
            {
                EquipmentManager.Instance.OnEquipmentStatsChanged -= OnV2EquipmentStatsChanged;
            }

            if (SetItemBonusSystem.HasInstance)
            {
                SetItemBonusSystem.Instance.OnSetStatsChanged -= OnV2SetStatsChanged;
            }

            isV2Subscribed = false;
        }


        // ====== V2 이벤트 핸들러 ======

        /// <summary>
        /// V2 장비 스탯 변경 이벤트
        /// </summary>
        private void OnV2EquipmentStatsChanged()
        {
            RefreshV2EquipmentStats();
            isDirty = true;
            RecalculateIfDirty();
        }

        /// <summary>
        /// V2 세트 보너스 변경 이벤트
        /// </summary>
        private void OnV2SetStatsChanged()
        {
            RefreshV2SetBonusStats();
            isDirty = true;
            RecalculateIfDirty();
        }


        // ====== V2 스탯 조회 ======

        /// <summary>
        /// V2 스탯 새로고침
        /// </summary>
        public void RefreshV2Stats()
        {
            if (!useV2System)
                return;

            RefreshV2EquipmentStats();
            RefreshV2SetBonusStats();
            isDirty = true;
        }

        /// <summary>
        /// V2 장비 스탯 새로고침
        /// </summary>
        private void RefreshV2EquipmentStats()
        {
            v2EquipmentStats.Clear();

            if (!EquipmentManager.HasInstance)
                return;

            v2EquipmentStats = EquipmentManager.Instance.GetAllEquipmentBonuses();
        }

        /// <summary>
        /// V2 세트 보너스 새로고침
        /// </summary>
        private void RefreshV2SetBonusStats()
        {
            v2SetBonusStats.Clear();

            if (!SetItemBonusSystem.HasInstance)
                return;

            v2SetBonusStats = SetItemBonusSystem.Instance.GetTotalSetBonusStats();
        }

        /// <summary>
        /// V2 스탯 보너스 가져오기
        /// </summary>
        /// <param name="statType">스탯 종류</param>
        /// <returns>V2 장비 + 세트 보너스 합계</returns>
        public float GetV2StatBonus(StatType statType)
        {
            float equipBonus = v2EquipmentStats.GetValueOrDefault(statType, 0f);
            float setBonus = v2SetBonusStats.GetValueOrDefault(statType, 0f);
            return equipBonus + setBonus;
        }


        // ====== V2 스탯 적용 (RecalculateStats에서 호출) ======

        /// <summary>
        /// V2 장비/세트 보너스 적용
        /// RecalculateStats() 내에서 호출
        /// </summary>
        private void ApplyV2Bonuses()
        {
            if (!useV2System)
                return;

            // HP
            finalHP += Mathf.RoundToInt(GetV2StatBonus(StatType.HP));

            // Attack
            finalAttack += Mathf.RoundToInt(GetV2StatBonus(StatType.Attack));

            // Defense
            finalDefense += Mathf.RoundToInt(GetV2StatBonus(StatType.Defense));

            // Mana
            finalMana += Mathf.RoundToInt(GetV2StatBonus(StatType.Mana));
        }


        // ====== 디버그 ======

        /// <summary>
        /// V2 스탯 보너스 출력
        /// </summary>
        [ContextMenu("Print V2 Stats")]
        private void DebugPrintV2Stats()
        {
            Debug.Log("[PlayerStats] ========== V2 스탯 보너스 ==========");
            Debug.Log($"V2 시스템 사용: {useV2System}");

            if (!useV2System)
            {
                Debug.Log("V2 시스템 비활성화 상태");
                return;
            }

            Debug.Log("--- 장비 보너스 ---");
            foreach (var kvp in v2EquipmentStats)
            {
                if (kvp.Value != 0)
                {
                    Debug.Log($"  {kvp.Key}: +{kvp.Value}");
                }
            }

            Debug.Log("--- 세트 보너스 ---");
            foreach (var kvp in v2SetBonusStats)
            {
                if (kvp.Value != 0)
                {
                    Debug.Log($"  {kvp.Key}: +{kvp.Value}");
                }
            }

            Debug.Log("--- 총합 (주요 스탯) ---");
            Debug.Log($"  HP: +{GetV2StatBonus(StatType.HP)}");
            Debug.Log($"  Attack: +{GetV2StatBonus(StatType.Attack)}");
            Debug.Log($"  Defense: +{GetV2StatBonus(StatType.Defense)}");
            Debug.Log($"  Mana: +{GetV2StatBonus(StatType.Mana)}");

            Debug.Log("[PlayerStats] ========================================");
        }
    }
}
