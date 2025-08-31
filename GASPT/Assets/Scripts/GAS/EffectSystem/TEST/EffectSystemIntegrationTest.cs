// EffectSystemIntegrationTest.cs
using GAS.AttributeSystem;
using GAS.EffectSystem;
using GAS.TagSystem;
using UnityEngine;

public class EffectSystemIntegrationTest : MonoBehaviour
{
    [Header("Test Effects")]
    public InstantEffect damageEffect;
    public DurationEffect buffEffect;
    public PeriodicEffect dotEffect;
    public InfiniteEffect passiveEffect;

    private GameObject player;
    private GameObject enemy;

    void Start()
    {
        SetupTestEnvironment();
        RunAllTests();
    }

    void SetupTestEnvironment()
    {
        // Player 持失
        player = new GameObject("Player");
        player.AddComponent<AttributeSetComponent>();
        player.AddComponent<TagComponent>();
        player.AddComponent<EffectComponent>();

        // Enemy 持失
        enemy = new GameObject("Enemy");
        enemy.AddComponent<AttributeSetComponent>();
        enemy.AddComponent<TagComponent>();
        enemy.AddComponent<EffectComponent>();
    }

    void RunAllTests()
    {
        StartCoroutine(TestSequence());
    }

    async Awaitable TestSequence()
    {
        Debug.Log("=== Effect System Integration Test Started ===");

        // Test 1: Instant Damage
        Debug.Log("Test 1: Instant Damage");
        EffectManager.Instance.ApplyEffect(damageEffect, player, enemy, 1.0f);
        await Awaitable.WaitForSecondsAsync(1f);

        // Test 2: Duration Buff
        Debug.Log("Test 2: Duration Buff");
        EffectManager.Instance.ApplyEffect(buffEffect, player, player, 1.5f);
        await Awaitable.WaitForSecondsAsync(2f);

        // Test 3: Periodic DOT
        Debug.Log("Test 3: Periodic DOT");
        EffectManager.Instance.ApplyEffect(dotEffect, player, enemy, 1.0f);
        await Awaitable.WaitForSecondsAsync(5f);

        // Test 4: Infinite Passive
        Debug.Log("Test 4: Infinite Passive");
        EffectManager.Instance.ApplyEffect(passiveEffect, player, player, 2.0f);
        await Awaitable.WaitForSecondsAsync(2f);

        // Test 5: AOE Effect
        Debug.Log("Test 5: AOE Effect");
        EffectManager.Instance.ApplyAOEEffect(
            damageEffect,
            player,
            player.transform.position,
            10f,
            LayerMask.GetMask("Default")
        );
        await Awaitable.WaitForSecondsAsync(1f);

        // Test 6: Chain Effect
        Debug.Log("Test 6: Chain Effect");
        await EffectManager.Instance.ApplyChainEffectAsync(
            damageEffect,
            player,
            enemy,
            3,
            5f,
            0.2f
        );

        // Test 7: Purge/Dispel
        Debug.Log("Test 7: Purge/Dispel");
        enemy.GetComponent<EffectComponent>().PurgeDebuffs();
        player.GetComponent<EffectComponent>().DispelBuffs();

        Debug.Log("=== All Tests Completed ===");
        PrintStatistics();
    }

    void PrintStatistics()
    {
        var manager = EffectManager.Instance;
        Debug.Log($"Total Active Effects: {manager.ActiveEffectCount}");
        Debug.Log($"Player Effects: {manager.GetTargetEffects(player).Count}");
        Debug.Log($"Enemy Effects: {manager.GetTargetEffects(enemy).Count}");
    }
}