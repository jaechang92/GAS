using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Unity Inspector�� ���̵��� ����ȭ�Ǵ� Dictionary ����.
/// - �ν����Ϳ����� Ű/�� ���� ����Ʈ�� ����
/// - ��Ÿ�ӿ����� Dictionaryó�� ��� (Add, Remove, TryGetValue ��)
/// </summary>
[Serializable]
public class SerializableDictionary<TKey, TValue> :
    ISerializationCallbackReceiver, IEnumerable<KeyValuePair<TKey, TValue>>
{
    // ���� ����ȭ ���: Ű/�� ����Ʈ
    [SerializeField] private List<TKey> keys = new List<TKey>();
    [SerializeField] private List<TValue> values = new List<TValue>();

    // ��Ÿ�� ��ȸ/����� ĳ�� ��ųʸ� (������ȭ)
    private Dictionary<TKey, TValue> dict;

    /// <summary>���� Dictionary �ν��Ͻ� ����</summary>
    private Dictionary<TKey, TValue> Dictionary
    {
        get
        {
            if (dict == null)
                RebuildDictionary();
            return dict;
        }
    }

    /// <summary>���� ���� ��</summary>
    public int Count => Dictionary.Count;

    /// <summary>Ű ���� ����</summary>
    public bool ContainsKey(TKey key) => Dictionary.ContainsKey(key);

    /// <summary>�� �������� �õ�</summary>
    public bool TryGetValue(TKey key, out TValue value) => Dictionary.TryGetValue(key, out value);

    /// <summary>�ε���</summary>
    public TValue this[TKey key]
    {
        get => Dictionary[key];
        set
        {
            if (Dictionary.ContainsKey(key))
            {
                Dictionary[key] = value;
                SyncListsFromDictionary();
            }
            else
            {
                Add(key, value);
            }
        }
    }

    /// <summary>Ű �÷���</summary>
    public Dictionary<TKey, TValue>.KeyCollection Keys => Dictionary.Keys;

    /// <summary>�� �÷���</summary>
    public Dictionary<TKey, TValue>.ValueCollection Values => Dictionary.Values;

    /// <summary>�߰�</summary>
    public void Add(TKey key, TValue value)
    {
        if (Dictionary.ContainsKey(key))
        {
            Debug.LogWarning($"[SerializableDictionary] Duplicate key ignored: {key}");
            return;
        }
        Dictionary.Add(key, value);
        keys.Add(key);
        values.Add(value);
    }

    /// <summary>����</summary>
    public bool Remove(TKey key)
    {
        if (!Dictionary.ContainsKey(key)) return false;

        // ����Ʈ������ ���� �ε��� ����
        int index = keys.FindIndex(k => EqualityComparer<TKey>.Default.Equals(k, key));
        if (index >= 0)
        {
            keys.RemoveAt(index);
            values.RemoveAt(index);
        }
        return Dictionary.Remove(key);
    }

    /// <summary>�ʱ�ȭ</summary>
    public void Clear()
    {
        Dictionary.Clear();
        keys.Clear();
        values.Clear();
    }

    /// <summary>������</summary>
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => Dictionary.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    // ---------- Serialization Hooks ----------

    // ����ȭ ����: ��Ÿ�� ��ųʸ��� ����Ʈ�� �ݿ�
    public void OnBeforeSerialize()
    {
        // dict�� ����־ ����Ʈ�� �ŷ��ϴ� ���� ���� �� ����
        // (��ξ�� ������ ����Ʈ�� �״�� ������)
        // Ư���� dict�� null�̸� ����Ʈ�� �������� �籸��
        if (dict == null)
            RebuildDictionary();
        else
            SyncListsFromDictionary();
    }

    // ������ȭ ����: ����Ʈ���� ��ųʸ� �籸��
    public void OnAfterDeserialize()
    {
        RebuildDictionary();
    }

    // ---------- ���� ��ƿ ----------

    private void RebuildDictionary()
    {
        dict = new Dictionary<TKey, TValue>(
            keys != null ? keys.Count : 0,
            EqualityComparer<TKey>.Default
        );

        if (keys == null || values == null) return;

        int count = Math.Min(keys.Count, values.Count);
        var seen = new HashSet<TKey>(EqualityComparer<TKey>.Default);

        for (int i = 0; i < count; i++)
        {
            var k = keys[i];
            var v = values[i];

            if (k == null && default(TKey) == null) // ������ TKey�� �� null Ű ����
            {
                Debug.LogWarning("[SerializableDictionary] Null key skipped.");
                continue;
            }
            if (!seen.Add(k))
            {
                Debug.LogWarning($"[SerializableDictionary] Duplicate key detected and skipped: {k}");
                continue;
            }
            dict[k] = v;
        }

        // ����Ʈ ���� ����ġ/�ߺ� ������ ���� �ʿ� �� ����Ʈ�� ��ųʸ� �������� �絿��ȭ
        SyncListsFromDictionary();
    }

    private void SyncListsFromDictionary()
    {
        if (dict == null)
            dict = new Dictionary<TKey, TValue>(EqualityComparer<TKey>.Default);

        keys ??= new List<TKey>();
        values ??= new List<TValue>();

        keys.Clear();
        values.Clear();

        foreach (var kv in dict)
        {
            keys.Add(kv.Key);
            values.Add(kv.Value);
        }
    }
}
