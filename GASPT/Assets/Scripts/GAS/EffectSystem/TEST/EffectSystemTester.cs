// EffectSystemTester.cs
using UnityEngine;
using GAS.EffectSystem;
using GAS.AttributeSystem;
using GAS.TagSystem;

public class EffectSystemTester : MonoBehaviour
{
    [Header("Test Setup")]
    public GameObject player;
    public GameObject enemy;

    [Header("Test Effects")]
    public InstantEffect damageEffect;
    public InstantEffect healEffect;
    public DurationEffect buffEffect;
    public DurationEffect debuffEffect;

    void Start()
    {
        // 컴포넌트 확인
        SetupTestObjects();
    }

    void SetupTestObjects()
    {
        // AttributeSetComponent 추가
        if (!player.GetComponent<AttributeSetComponent>())
            player.AddComponent<AttributeSetComponent>();
        if (!enemy.GetComponent<AttributeSetComponent>())
            enemy.AddComponent<AttributeSetComponent>();

        // TagComponent 추가
        if (!player.GetComponent<TagComponent>())
            player.AddComponent<TagComponent>();
        if (!enemy.GetComponent<TagComponent>())
            enemy.AddComponent<TagComponent>();
    }

    void Update()
    {
        // 테스트 키 입력
        if (Input.GetKeyDown(KeyCode.Alpha1))
            TestInstantDamage();
        if (Input.GetKeyDown(KeyCode.Alpha2))
            TestInstantHeal();
        if (Input.GetKeyDown(KeyCode.Alpha3))
            TestDurationBuff();
        if (Input.GetKeyDown(KeyCode.Alpha4))
            TestDurationDebuff();
        if (Input.GetKeyDown(KeyCode.Alpha5))
            TestStacking();
    }

    void TestInstantDamage()
    {
        Debug.Log("=== Test Instant Damage ===");
        var context = new EffectContext(player, enemy, damageEffect);
        context.Magnitude = 1.0f;
        damageEffect.Apply(context, enemy);
    }

    void TestInstantHeal()
    {
        Debug.Log("=== Test Instant Heal ===");
        var context = new EffectContext(player, player, healEffect);
        context.Magnitude = 1.0f;
        healEffect.Apply(context, player);
    }

    void TestDurationBuff()
    {
        Debug.Log("=== Test Duration Buff ===");
        var context = new EffectContext(player, player, buffEffect);
        context.Magnitude = 1.0f;
        buffEffect.Apply(context, player);
    }

    void TestDurationDebuff()
    {
        Debug.Log("=== Test Duration Debuff ===");
        var context = new EffectContext(player, enemy, debuffEffect);
        context.Magnitude = 1.0f;
        debuffEffect.Apply(context, enemy);
    }

    void TestStacking()
    {
        Debug.Log("=== Test Effect Stacking ===");
        // 같은 buff를 3번 적용
        for (int i = 0; i < 3; i++)
        {
            var context = new EffectContext(player, player, buffEffect);
            buffEffect.Apply(context, player);
        }
    }
}