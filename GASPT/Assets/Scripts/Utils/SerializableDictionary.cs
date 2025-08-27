using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Unity Inspector에 보이도록 직렬화되는 Dictionary 래퍼.
/// - 인스펙터에서는 키/값 쌍을 리스트로 편집
/// - 런타임에서는 Dictionary처럼 사용 (Add, Remove, TryGetValue 등)
/// </summary>
[Serializable]
public class SerializableDictionary<TKey, TValue> :
    ISerializationCallbackReceiver, IEnumerable<KeyValuePair<TKey, TValue>>
{
    // 실제 직렬화 대상: 키/값 리스트
    [SerializeField] private List<TKey> keys = new List<TKey>();
    [SerializeField] private List<TValue> values = new List<TValue>();

    // 런타임 조회/연산용 캐시 딕셔너리 (비직렬화)
    private Dictionary<TKey, TValue> dict;

    /// <summary>내부 Dictionary 인스턴스 보장</summary>
    private Dictionary<TKey, TValue> Dictionary
    {
        get
        {
            if (dict == null)
                RebuildDictionary();
            return dict;
        }
    }

    /// <summary>현재 원소 수</summary>
    public int Count => Dictionary.Count;

    /// <summary>키 존재 여부</summary>
    public bool ContainsKey(TKey key) => Dictionary.ContainsKey(key);

    /// <summary>값 가져오기 시도</summary>
    public bool TryGetValue(TKey key, out TValue value) => Dictionary.TryGetValue(key, out value);

    /// <summary>인덱서</summary>
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

    /// <summary>키 컬렉션</summary>
    public Dictionary<TKey, TValue>.KeyCollection Keys => Dictionary.Keys;

    /// <summary>값 컬렉션</summary>
    public Dictionary<TKey, TValue>.ValueCollection Values => Dictionary.Values;

    /// <summary>추가</summary>
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

    /// <summary>삭제</summary>
    public bool Remove(TKey key)
    {
        if (!Dictionary.ContainsKey(key)) return false;

        // 리스트에서도 같은 인덱스 제거
        int index = keys.FindIndex(k => EqualityComparer<TKey>.Default.Equals(k, key));
        if (index >= 0)
        {
            keys.RemoveAt(index);
            values.RemoveAt(index);
        }
        return Dictionary.Remove(key);
    }

    /// <summary>초기화</summary>
    public void Clear()
    {
        Dictionary.Clear();
        keys.Clear();
        values.Clear();
    }

    /// <summary>열거자</summary>
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => Dictionary.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    // ---------- Serialization Hooks ----------

    // 직렬화 직전: 런타임 딕셔너리를 리스트로 반영
    public void OnBeforeSerialize()
    {
        // dict가 비어있어도 리스트를 신뢰하는 편이 편집 시 안전
        // (드로어에서 수정한 리스트를 그대로 내보냄)
        // 특별히 dict가 null이면 리스트를 기준으로 재구성
        if (dict == null)
            RebuildDictionary();
        else
            SyncListsFromDictionary();
    }

    // 역직렬화 직후: 리스트에서 딕셔너리 재구성
    public void OnAfterDeserialize()
    {
        RebuildDictionary();
    }

    // ---------- 내부 유틸 ----------

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

            if (k == null && default(TKey) == null) // 참조형 TKey일 때 null 키 방지
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

        // 리스트 길이 불일치/중복 등으로 정리 필요 시 리스트를 딕셔너리 기준으로 재동기화
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
