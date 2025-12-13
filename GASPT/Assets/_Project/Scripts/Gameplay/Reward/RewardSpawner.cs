using System;
using System.Collections.Generic;
using UnityEngine;
using GASPT.Economy;
using GASPT.Gameplay.Form;
using Random = UnityEngine.Random;

namespace GASPT.Gameplay.Reward
{
    /// <summary>
    /// 방 클리어 시 보상 스폰 관리
    /// 골드, 체력 물약, 폼 픽업 등을 생성
    /// </summary>
    public class RewardSpawner : SingletonManager<RewardSpawner>
    {
        // ====== 설정 ======

        [Header("보상 설정")]
        [SerializeField] private RewardConfig defaultRewardConfig;

        [Header("프리팹")]
        [SerializeField] private GameObject goldPickupPrefab;
        [SerializeField] private GameObject healthPotionPrefab;
        [SerializeField] private GameObject manaPotionPrefab;

        [Header("스폰 설정")]
        [SerializeField] private float spawnRadius = 2f;
        [SerializeField] private float spawnHeight = 0.5f;
        [SerializeField] private float delayBetweenSpawns = 0.1f;


        // ====== 이벤트 ======

        /// <summary>보상 스폰 완료 이벤트</summary>
        public event Action<List<GameObject>> OnRewardsSpawned;


        // ====== 보상 스폰 ======

        /// <summary>
        /// 방 클리어 보상 스폰
        /// </summary>
        public async Awaitable<List<GameObject>> SpawnRoomRewardsAsync(Vector3 centerPosition, int roomDifficulty = 1)
        {
            var rewards = new List<GameObject>();
            var config = defaultRewardConfig;

            if (config == null)
            {
                Debug.LogWarning("[RewardSpawner] RewardConfig가 없습니다. 기본 보상 스폰");
                return await SpawnDefaultRewardsAsync(centerPosition, roomDifficulty);
            }

            // 골드 드랍
            if (Random.value <= config.goldDropChance)
            {
                int goldAmount = Random.Range(config.minGold, config.maxGold + 1) * roomDifficulty;
                var goldPickup = SpawnGoldPickup(centerPosition, goldAmount);
                if (goldPickup != null) rewards.Add(goldPickup);
            }

            await Awaitable.WaitForSecondsAsync(delayBetweenSpawns);

            // 체력 물약 드랍
            if (Random.value <= config.healthPotionDropChance)
            {
                var potion = SpawnHealthPotion(centerPosition);
                if (potion != null) rewards.Add(potion);
            }

            await Awaitable.WaitForSecondsAsync(delayBetweenSpawns);

            // 폼 드랍 (낮은 확률)
            if (Random.value <= config.formDropChance && config.formPool != null && config.formPool.Length > 0)
            {
                var form = config.formPool[Random.Range(0, config.formPool.Length)];
                var formPickup = SpawnFormPickup(centerPosition, form);
                if (formPickup != null) rewards.Add(formPickup);
            }

            Debug.Log($"[RewardSpawner] 보상 {rewards.Count}개 스폰 완료");
            OnRewardsSpawned?.Invoke(rewards);

            return rewards;
        }

        /// <summary>
        /// 기본 보상 스폰 (config 없을 때)
        /// </summary>
        private async Awaitable<List<GameObject>> SpawnDefaultRewardsAsync(Vector3 centerPosition, int difficulty)
        {
            var rewards = new List<GameObject>();

            // 항상 골드 드랍
            int goldAmount = 20 + (difficulty * 10);
            var goldPickup = SpawnGoldPickup(centerPosition, goldAmount);
            if (goldPickup != null) rewards.Add(goldPickup);

            await Awaitable.WaitForSecondsAsync(delayBetweenSpawns);

            // 30% 확률로 체력 물약
            if (Random.value <= 0.3f)
            {
                var potion = SpawnHealthPotion(centerPosition);
                if (potion != null) rewards.Add(potion);
            }

            OnRewardsSpawned?.Invoke(rewards);
            return rewards;
        }


        // ====== 개별 보상 스폰 ======

        /// <summary>
        /// 골드 픽업 스폰
        /// </summary>
        public GameObject SpawnGoldPickup(Vector3 centerPosition, int amount)
        {
            Vector3 spawnPos = GetRandomSpawnPosition(centerPosition);

            GameObject pickup;
            if (goldPickupPrefab != null)
            {
                pickup = Instantiate(goldPickupPrefab, spawnPos, Quaternion.identity);
            }
            else
            {
                pickup = CreateDefaultGoldPickup(spawnPos);
            }

            var goldPickup = pickup.GetComponent<GoldPickup>();
            if (goldPickup == null)
            {
                goldPickup = pickup.AddComponent<GoldPickup>();
            }
            goldPickup.Initialize(amount);

            Debug.Log($"[RewardSpawner] 골드 픽업 스폰: {amount}G at {spawnPos}");
            return pickup;
        }

        /// <summary>
        /// 체력 물약 스폰
        /// </summary>
        public GameObject SpawnHealthPotion(Vector3 centerPosition)
        {
            Vector3 spawnPos = GetRandomSpawnPosition(centerPosition);

            GameObject pickup;
            if (healthPotionPrefab != null)
            {
                pickup = Instantiate(healthPotionPrefab, spawnPos, Quaternion.identity);
            }
            else
            {
                pickup = CreateDefaultHealthPotion(spawnPos);
            }

            Debug.Log($"[RewardSpawner] 체력 물약 스폰 at {spawnPos}");
            return pickup;
        }

        /// <summary>
        /// 폼 픽업 스폰
        /// </summary>
        public GameObject SpawnFormPickup(Vector3 centerPosition, FormData formData)
        {
            if (formData == null) return null;

            Vector3 spawnPos = GetRandomSpawnPosition(centerPosition);
            var pickup = FormPickup.CreateFormPickup(formData, spawnPos);

            Debug.Log($"[RewardSpawner] 폼 픽업 스폰: {formData.formName} at {spawnPos}");
            return pickup?.gameObject;
        }


        // ====== 유틸리티 ======

        /// <summary>
        /// 랜덤 스폰 위치 계산
        /// </summary>
        private Vector3 GetRandomSpawnPosition(Vector3 center)
        {
            Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
            return new Vector3(
                center.x + randomCircle.x,
                center.y + spawnHeight,
                center.z
            );
        }

        /// <summary>
        /// 기본 골드 픽업 생성 (프리팹 없을 때)
        /// </summary>
        private GameObject CreateDefaultGoldPickup(Vector3 position)
        {
            GameObject obj = new GameObject("GoldPickup");
            obj.transform.position = position;
            obj.tag = "Pickup";

            // 스프라이트
            var sr = obj.AddComponent<SpriteRenderer>();
            sr.color = new Color(1f, 0.85f, 0f); // 금색
            sr.sortingOrder = 10;

            // Collider
            var collider = obj.AddComponent<CircleCollider2D>();
            collider.radius = 0.3f;
            collider.isTrigger = true;

            return obj;
        }

        /// <summary>
        /// 기본 체력 물약 생성 (프리팹 없을 때)
        /// </summary>
        private GameObject CreateDefaultHealthPotion(Vector3 position)
        {
            GameObject obj = new GameObject("HealthPotion");
            obj.transform.position = position;
            obj.tag = "Pickup";

            // 스프라이트
            var sr = obj.AddComponent<SpriteRenderer>();
            sr.color = new Color(1f, 0.2f, 0.2f); // 빨간색
            sr.sortingOrder = 10;

            // Collider
            var collider = obj.AddComponent<CircleCollider2D>();
            collider.radius = 0.3f;
            collider.isTrigger = true;

            // HealthPotion 컴포넌트
            obj.AddComponent<HealthPotion>();

            return obj;
        }
    }


    /// <summary>
    /// 보상 설정 ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "RewardConfig", menuName = "GASPT/Reward/RewardConfig")]
    public class RewardConfig : ScriptableObject
    {
        [Header("골드")]
        public float goldDropChance = 1f;
        public int minGold = 10;
        public int maxGold = 50;

        [Header("체력 물약")]
        public float healthPotionDropChance = 0.3f;
        public int healthPotionHealAmount = 30;

        [Header("마나 물약")]
        public float manaPotionDropChance = 0.2f;
        public int manaPotionAmount = 20;

        [Header("폼 드랍")]
        [Range(0f, 1f)]
        public float formDropChance = 0.1f;
        public FormData[] formPool;
    }
}
