using System;
using System.Collections.Generic;
using UnityEngine;
using GASPT.Core.Enums;
using GASPT.Save;

namespace GASPT.Data
{
    /// <summary>
    /// 아이템 인스턴스 클래스
    /// ScriptableObject(템플릿)와 런타임 데이터를 분리하여 관리
    /// </summary>
    [Serializable]
    public class ItemInstance
    {
        // ====== 고유 식별 ======

        /// <summary>
        /// 고유 인스턴스 ID (GUID)
        /// </summary>
        public string instanceId;

        /// <summary>
        /// ItemData ScriptableObject 경로 (저장용)
        /// </summary>
        public string itemDataPath;


        // ====== 런타임 데이터 ======

        /// <summary>
        /// 랜덤 생성된 추가 스탯
        /// </summary>
        public List<StatModifier> randomStats = new List<StatModifier>();

        /// <summary>
        /// 현재 내구도 (-1 = 내구도 없음)
        /// </summary>
        public int currentDurability = -1;

        /// <summary>
        /// 장착 상태 플래그
        /// </summary>
        public bool isEquipped;

        /// <summary>
        /// 획득 시간 (UTC ticks)
        /// </summary>
        public long acquireTimeTicks;


        // ====== 런타임 캐시 (저장 안함) ======

        /// <summary>
        /// ItemData 런타임 캐시
        /// </summary>
        [NonSerialized]
        public ItemData cachedItemData;


        // ====== 프로퍼티 ======

        /// <summary>
        /// 획득 시간
        /// </summary>
        public DateTime AcquireTime => new DateTime(acquireTimeTicks, DateTimeKind.Utc);

        /// <summary>
        /// ItemData 유효 여부
        /// </summary>
        public bool IsValid => cachedItemData != null;

        /// <summary>
        /// 장비 데이터 캐스팅 (장비 아이템인 경우)
        /// </summary>
        public EquipmentData EquipmentData => cachedItemData as EquipmentData;

        /// <summary>
        /// 소비 데이터 캐스팅 (소비 아이템인 경우)
        /// </summary>
        public ConsumableData ConsumableData => cachedItemData as ConsumableData;

        /// <summary>
        /// 장비 아이템 여부
        /// </summary>
        public bool IsEquipment => cachedItemData is EquipmentData;

        /// <summary>
        /// 소비 아이템 여부
        /// </summary>
        public bool IsConsumable => cachedItemData is ConsumableData;


        // ====== 생성자 ======

        /// <summary>
        /// 기본 생성자 (직렬화용)
        /// </summary>
        public ItemInstance()
        {
            instanceId = Guid.NewGuid().ToString();
            acquireTimeTicks = DateTime.UtcNow.Ticks;
        }

        /// <summary>
        /// ItemData로부터 인스턴스 생성
        /// </summary>
        /// <param name="itemData">아이템 데이터</param>
        private ItemInstance(ItemData itemData) : this()
        {
            cachedItemData = itemData;

            // 에디터에서 에셋 경로 저장
#if UNITY_EDITOR
            itemDataPath = UnityEditor.AssetDatabase.GetAssetPath(itemData);
#else
            itemDataPath = itemData.name;
#endif

            // 장비인 경우 내구도 및 랜덤 스탯 설정
            if (itemData is EquipmentData equipData)
            {
                // 내구도 설정
                if (equipData.HasDurability)
                {
                    currentDurability = equipData.maxDurability;
                }

                // 랜덤 스탯 생성
                GenerateRandomStats(equipData);
            }
        }


        // ====== 팩토리 메서드 ======

        /// <summary>
        /// ItemData로부터 새 인스턴스 생성
        /// </summary>
        /// <param name="itemData">아이템 데이터</param>
        /// <returns>새 아이템 인스턴스</returns>
        public static ItemInstance CreateFromData(ItemData itemData)
        {
            if (itemData == null)
            {
                Debug.LogError("[ItemInstance] CreateFromData: itemData가 null입니다.");
                return null;
            }

            return new ItemInstance(itemData);
        }

        /// <summary>
        /// 저장 데이터로부터 인스턴스 복원
        /// InventoryManager, EquipmentManager 등에서 공통 사용
        /// </summary>
        /// <param name="instanceId">인스턴스 ID</param>
        /// <param name="itemDataPath">ItemData 에셋 경로</param>
        /// <param name="currentDurability">현재 내구도</param>
        /// <param name="isEquipped">장착 상태</param>
        /// <param name="acquireTimeTicks">획득 시간 (UTC ticks)</param>
        /// <param name="randomStats">랜덤 스탯 데이터</param>
        /// <returns>복원된 아이템 인스턴스 (실패 시 null)</returns>
        public static ItemInstance RestoreFromSaveData(
            string instanceId,
            string itemDataPath,
            int currentDurability,
            bool isEquipped,
            long acquireTimeTicks,
            List<StatModifierData> randomStats)
        {
            if (string.IsNullOrEmpty(itemDataPath))
            {
                Debug.LogWarning("[ItemInstance] RestoreFromSaveData: itemDataPath가 비어있습니다.");
                return null;
            }

            var instance = new ItemInstance
            {
                instanceId = string.IsNullOrEmpty(instanceId) ? Guid.NewGuid().ToString() : instanceId,
                itemDataPath = itemDataPath,
                currentDurability = currentDurability,
                isEquipped = isEquipped,
                acquireTimeTicks = acquireTimeTicks,
                randomStats = new List<StatModifier>()
            };

            // 랜덤 스탯 복원
            if (randomStats != null)
            {
                foreach (var statData in randomStats)
                {
                    instance.randomStats.Add(new StatModifier(
                        statData.statType,
                        statData.modifierType,
                        statData.value
                    ));
                }
            }

            // ItemData 캐시 로드
            instance.LoadCachedData();

            if (!instance.IsValid)
            {
                Debug.LogWarning($"[ItemInstance] RestoreFromSaveData: 아이템 로드 실패. 경로: {itemDataPath}");
                return null;
            }

            return instance;
        }


        // ====== 랜덤 스탯 생성 ======

        /// <summary>
        /// 장비 데이터 기반 랜덤 스탯 생성
        /// </summary>
        /// <param name="equipData">장비 데이터</param>
        public void GenerateRandomStats(EquipmentData equipData)
        {
            randomStats.Clear();

            if (equipData == null)
                return;

            if (equipData.maxRandomStats <= 0)
                return;

            if (equipData.possibleRandomStats == null || equipData.possibleRandomStats.Count == 0)
                return;

            // 랜덤 스탯 수 결정
            int statCount = UnityEngine.Random.Range(equipData.minRandomStats, equipData.maxRandomStats + 1);

            // 중복 방지용 사용된 스탯 목록
            List<StatType> usedStats = new List<StatType>();

            for (int i = 0; i < statCount; i++)
            {
                // 사용 가능한 스탯 필터링
                List<StatType> availableStats = new List<StatType>();
                foreach (var stat in equipData.possibleRandomStats)
                {
                    if (!usedStats.Contains(stat))
                    {
                        availableStats.Add(stat);
                    }
                }

                if (availableStats.Count == 0)
                    break;

                // 랜덤 스탯 선택
                StatType selectedStat = availableStats[UnityEngine.Random.Range(0, availableStats.Count)];
                usedStats.Add(selectedStat);

                // 랜덤 수치 결정 (등급에 따른 범위)
                float baseValue = GetRandomStatValue(selectedStat, equipData.rarity);

                // 스탯 수정자 추가
                randomStats.Add(new StatModifier(selectedStat, ModifierType.Flat, baseValue));
            }

            Debug.Log($"[ItemInstance] {equipData.itemName}: 랜덤 스탯 {randomStats.Count}개 생성");
        }

        /// <summary>
        /// 스탯 종류와 등급에 따른 랜덤 수치 생성
        /// </summary>
        private float GetRandomStatValue(StatType statType, ItemRarity rarity)
        {
            // 기본 범위 (등급에 따라 스케일)
            float multiplier = ItemConstants.GetStatMultiplier(rarity);

            // 스탯 종류별 기본 범위
            float minValue, maxValue;

            switch (statType)
            {
                case StatType.HP:
                    minValue = 10f;
                    maxValue = 50f;
                    break;
                case StatType.Attack:
                    minValue = 3f;
                    maxValue = 15f;
                    break;
                case StatType.Defense:
                    minValue = 2f;
                    maxValue = 10f;
                    break;
                case StatType.Mana:
                    minValue = 5f;
                    maxValue = 25f;
                    break;
                case StatType.Speed:
                    minValue = 0.1f;
                    maxValue = 0.5f;
                    break;
                case StatType.CriticalRate:
                    minValue = 1f;
                    maxValue = 5f;
                    break;
                default:
                    minValue = 1f;
                    maxValue = 10f;
                    break;
            }

            // 등급 배율 적용
            float value = UnityEngine.Random.Range(minValue, maxValue) * multiplier;

            // 정수형 스탯은 반올림
            if (statType == StatType.HP || statType == StatType.Attack ||
                statType == StatType.Defense || statType == StatType.Mana)
            {
                value = Mathf.Round(value);
            }

            return value;
        }


        // ====== 스탯 계산 ======

        /// <summary>
        /// 모든 스탯 수정자 반환 (기본 + 랜덤)
        /// </summary>
        /// <returns>스탯 수정자 목록</returns>
        public List<StatModifier> GetAllStatModifiers()
        {
            List<StatModifier> allModifiers = new List<StatModifier>();

            // 장비인 경우 기본 스탯 추가
            if (EquipmentData != null)
            {
                allModifiers.AddRange(EquipmentData.baseStats);
            }

            // 랜덤 스탯 추가
            allModifiers.AddRange(randomStats);

            return allModifiers;
        }


        // ====== 내구도 ======

        /// <summary>
        /// 내구도 감소
        /// </summary>
        /// <param name="amount">감소량</param>
        /// <returns>true = 파괴됨</returns>
        public bool ReduceDurability(int amount = 1)
        {
            if (currentDurability < 0)
                return false; // 내구도 없음

            currentDurability -= amount;

            if (currentDurability <= 0)
            {
                currentDurability = 0;
                return true; // 파괴됨
            }

            return false;
        }

        /// <summary>
        /// 내구도 복구
        /// </summary>
        /// <param name="amount">복구량</param>
        public void RepairDurability(int amount)
        {
            if (currentDurability < 0)
                return; // 내구도 없음

            if (EquipmentData == null)
                return;

            currentDurability = Mathf.Min(currentDurability + amount, EquipmentData.maxDurability);
        }


        // ====== 캐시 관리 ======

        /// <summary>
        /// ItemData 캐시 로드
        /// </summary>
        public void LoadCachedData()
        {
            if (cachedItemData != null)
                return;

            if (string.IsNullOrEmpty(itemDataPath))
            {
                Debug.LogWarning("[ItemInstance] LoadCachedData: itemDataPath가 비어있습니다.");
                return;
            }

#if UNITY_EDITOR
            cachedItemData = UnityEditor.AssetDatabase.LoadAssetAtPath<ItemData>(itemDataPath);
#else
            // 빌드 환경에서는 Resources 또는 Addressables 사용
            cachedItemData = Resources.Load<ItemData>(itemDataPath);
#endif

            if (cachedItemData == null)
            {
                Debug.LogWarning($"[ItemInstance] LoadCachedData: 아이템을 찾을 수 없습니다. 경로: {itemDataPath}");
            }
        }


        // ====== 디버그 ======

        /// <summary>
        /// 디버그용 문자열 출력
        /// </summary>
        public override string ToString()
        {
            string name = cachedItemData != null ? cachedItemData.itemName : "Unknown";
            string equipped = isEquipped ? "[E]" : "";
            return $"{equipped}{name} (ID: {instanceId.Substring(0, 8)}...)";
        }
    }
}
