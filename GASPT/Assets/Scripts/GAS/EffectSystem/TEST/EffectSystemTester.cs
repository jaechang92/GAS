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
        // ������Ʈ Ȯ��
        SetupTestObjects();
    }

    void SetupTestObjects()
    {
        // AttributeSetComponent �߰�
        if (!player.GetComponent<AttributeSetComponent>())
            player.AddComponent<AttributeSetComponent>();
        if (!enemy.GetComponent<AttributeSetComponent>())
            enemy.AddComponent<AttributeSetComponent>();

        // TagComponent �߰�
        if (!player.GetComponent<TagComponent>())
            player.AddComponent<TagComponent>();
        if (!enemy.GetComponent<TagComponent>())
            enemy.AddComponent<TagComponent>();
    }

    void Update()
    {
        // �׽�Ʈ Ű �Է�
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
        // ���� buff�� 3�� ����
        for (int i = 0; i < 3; i++)
        {
            var context = new EffectContext(player, player, buffEffect);
            buffEffect.Apply(context, player);
        }
    }
}