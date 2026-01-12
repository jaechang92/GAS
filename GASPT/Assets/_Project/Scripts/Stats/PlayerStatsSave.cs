using System.Collections.Generic;
using GASPT.Core.Enums;
using GASPT.Data;
using GASPT.Save;
using UnityEngine;

namespace GASPT.Stats
{
    /// <summary>
    /// PlayerStats 저장/로드 관련 partial class
    /// </summary>
    public partial class PlayerStats
    {
        // ====== ISaveable 인터페이스 구현 ======

        /// <summary>
        /// ISaveable 인터페이스: 저장 가능 객체 고유 ID
        /// </summary>
        public string SaveID => "PlayerStats";

        /// <summary>
        /// ISaveable.GetSaveData() 명시적 구현
        /// 내부적으로 구체적 타입의 GetSaveData()를 호출합니다
        /// </summary>
        object ISaveable.GetSaveData()
        {
            return GetSaveData();
        }

        /// <summary>
        /// ISaveable.LoadFromSaveData(object) 명시적 구현
        /// 타입 검증 후 구체적 타입의 LoadFromSaveData()를 호출합니다
        /// </summary>
        void ISaveable.LoadFromSaveData(object data)
        {
            if (data is PlayerStatsData statsData)
            {
                LoadFromSaveData(statsData);
            }
            else
            {
                Debug.LogError($"[PlayerStats] ISaveable.LoadFromSaveData(): 잘못된 데이터 타입입니다. Expected: PlayerStatsData, Got: {data?.GetType().Name}");
            }
        }


        // ====== Save/Load (기존 방식) ======

        /// <summary>
        /// 현재 플레이어 스탯 데이터를 저장용 구조로 반환합니다
        /// </summary>
        public PlayerStatsData GetSaveData()
        {
            PlayerStatsData data = new PlayerStatsData();

            // 현재 HP 저장
            data.currentHP = currentHP;

            // 장착된 아이템 저장
            data.equippedItems = new List<EquippedItemEntry>();

            foreach (var kvp in equippedItems)
            {
                if (kvp.Value != null)
                {
                    // ScriptableObject의 에셋 경로 저장
#if UNITY_EDITOR
                    string assetPath = UnityEditor.AssetDatabase.GetAssetPath(kvp.Value);
                    data.equippedItems.Add(new EquippedItemEntry(kvp.Key, assetPath));
#else
                    Debug.LogWarning($"[PlayerStats] GetSaveData(): 빌드 환경에서는 아이템 저장이 지원되지 않습니다. 슬롯: {kvp.Key}");
#endif
                }
            }

            return data;
        }

        /// <summary>
        /// 저장된 데이터로부터 플레이어 스탯을 복원합니다
        /// </summary>
        public void LoadFromSaveData(PlayerStatsData data)
        {
            if (data == null)
            {
                Debug.LogError("[PlayerStats] LoadFromSaveData(): data가 null입니다.");
                return;
            }

            // 현재 HP 복원
            currentHP = data.currentHP;
            currentHP = Mathf.Clamp(currentHP, 0, MaxHP);

            // 모든 아이템 장착 해제
            equippedItems.Clear();
            isDirty = true;

            // 장착된 아이템 복원
            if (data.equippedItems != null)
            {
                foreach (var entry in data.equippedItems)
                {
                    if (string.IsNullOrEmpty(entry.itemPath))
                    {
                        Debug.LogWarning($"[PlayerStats] LoadFromSaveData(): 빈 아이템 경로입니다. 슬롯: {entry.slot}");
                        continue;
                    }

#if UNITY_EDITOR
                    // 에셋 경로로부터 아이템 로드
                    Item item = UnityEditor.AssetDatabase.LoadAssetAtPath<Item>(entry.itemPath);

                    if (item != null)
                    {
                        equippedItems[entry.slot] = item;
                    }
                    else
                    {
                        Debug.LogWarning($"[PlayerStats] LoadFromSaveData(): 아이템을 찾을 수 없습니다. 경로: {entry.itemPath}");
                    }
#else
                    Debug.LogWarning($"[PlayerStats] LoadFromSaveData(): 빌드 환경에서는 아이템 불러오기가 지원되지 않습니다. 슬롯: {entry.slot}");
#endif
                }
            }

            // 스탯 재계산
            isDirty = true;
            RecalculateIfDirty();
        }
    }
}
