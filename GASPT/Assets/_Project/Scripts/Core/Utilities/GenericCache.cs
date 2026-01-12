using System;
using System.Collections.Generic;
using UnityEngine;

namespace GASPT.Core.Utilities
{
    /// <summary>
    /// Dirty Flag 패턴을 사용하는 캐시
    /// 값이 변경되면 dirty 상태가 되고, 접근 시 재계산
    /// EquipmentManager의 스탯 캐시 등에 사용
    /// </summary>
    /// <typeparam name="T">캐시할 값 타입</typeparam>
    public class DirtyCache<T>
    {
        private T cachedValue;
        private bool isDirty = true;
        private readonly Func<T> computeFunc;

        /// <summary>
        /// 캐시가 dirty 상태인지 여부
        /// </summary>
        public bool IsDirty => isDirty;

        /// <summary>
        /// 생성자
        /// </summary>
        /// <param name="computeFunc">값 계산 함수</param>
        public DirtyCache(Func<T> computeFunc)
        {
            this.computeFunc = computeFunc ?? throw new ArgumentNullException(nameof(computeFunc));
        }

        /// <summary>
        /// 캐시된 값 가져오기 (dirty면 재계산)
        /// </summary>
        public T Value
        {
            get
            {
                if (isDirty)
                {
                    cachedValue = computeFunc();
                    isDirty = false;
                }
                return cachedValue;
            }
        }

        /// <summary>
        /// 캐시 무효화 (다음 접근 시 재계산)
        /// </summary>
        public void Invalidate()
        {
            isDirty = true;
        }

        /// <summary>
        /// 강제로 값 설정 (dirty 해제)
        /// </summary>
        public void SetValue(T value)
        {
            cachedValue = value;
            isDirty = false;
        }

        /// <summary>
        /// 암시적 변환 (캐시 값으로)
        /// </summary>
        public static implicit operator T(DirtyCache<T> cache) => cache.Value;
    }

    /// <summary>
    /// 키 기반 리소스 캐시
    /// 요청 시 로드하고 캐시에 저장
    /// GameResourceManager의 리소스 캐시 등에 사용
    /// </summary>
    /// <typeparam name="TKey">키 타입</typeparam>
    /// <typeparam name="TValue">값 타입</typeparam>
    public class ResourceCache<TKey, TValue>
    {
        private readonly Dictionary<TKey, TValue> cache = new Dictionary<TKey, TValue>();
        private readonly Func<TKey, TValue> loadFunc;

        /// <summary>
        /// 현재 캐시된 항목 수
        /// </summary>
        public int Count => cache.Count;

        /// <summary>
        /// 생성자
        /// </summary>
        /// <param name="loadFunc">키로 값을 로드하는 함수</param>
        public ResourceCache(Func<TKey, TValue> loadFunc)
        {
            this.loadFunc = loadFunc ?? throw new ArgumentNullException(nameof(loadFunc));
        }

        /// <summary>
        /// 값 가져오기 (캐시에 없으면 로드)
        /// </summary>
        public TValue Get(TKey key)
        {
            if (cache.TryGetValue(key, out TValue value))
            {
                return value;
            }

            value = loadFunc(key);
            if (value != null)
            {
                cache[key] = value;
            }
            return value;
        }

        /// <summary>
        /// 캐시에 값이 있는지 확인
        /// </summary>
        public bool Contains(TKey key)
        {
            return cache.ContainsKey(key);
        }

        /// <summary>
        /// 캐시에서 값 가져오기 시도 (로드 없이)
        /// </summary>
        public bool TryGet(TKey key, out TValue value)
        {
            return cache.TryGetValue(key, out value);
        }

        /// <summary>
        /// 캐시에 값 직접 설정
        /// </summary>
        public void Set(TKey key, TValue value)
        {
            cache[key] = value;
        }

        /// <summary>
        /// 특정 항목 캐시에서 제거
        /// </summary>
        public bool Remove(TKey key)
        {
            return cache.Remove(key);
        }

        /// <summary>
        /// 모든 캐시 클리어
        /// </summary>
        public void Clear()
        {
            cache.Clear();
        }

        /// <summary>
        /// 모든 캐시된 값 순회
        /// </summary>
        public IEnumerable<KeyValuePair<TKey, TValue>> GetAll()
        {
            return cache;
        }

        /// <summary>
        /// 인덱서
        /// </summary>
        public TValue this[TKey key] => Get(key);
    }

    /// <summary>
    /// 지연 초기화 값 (Lazy<T>의 Unity 친화적 버전)
    /// </summary>
    /// <typeparam name="T">값 타입</typeparam>
    public class LazyValue<T>
    {
        private T value;
        private bool isInitialized;
        private readonly Func<T> initFunc;

        /// <summary>
        /// 초기화 여부
        /// </summary>
        public bool IsInitialized => isInitialized;

        /// <summary>
        /// 생성자
        /// </summary>
        /// <param name="initFunc">초기화 함수</param>
        public LazyValue(Func<T> initFunc)
        {
            this.initFunc = initFunc ?? throw new ArgumentNullException(nameof(initFunc));
        }

        /// <summary>
        /// 값 가져오기 (초기화 안 됐으면 초기화)
        /// </summary>
        public T Value
        {
            get
            {
                if (!isInitialized)
                {
                    value = initFunc();
                    isInitialized = true;
                }
                return value;
            }
        }

        /// <summary>
        /// 강제 재초기화
        /// </summary>
        public void Reset()
        {
            isInitialized = false;
            value = default;
        }

        /// <summary>
        /// 암시적 변환
        /// </summary>
        public static implicit operator T(LazyValue<T> lazy) => lazy.Value;
    }

    /// <summary>
    /// 타임 기반 캐시 (일정 시간 후 만료)
    /// </summary>
    /// <typeparam name="T">값 타입</typeparam>
    public class TimedCache<T>
    {
        private T cachedValue;
        private float cacheTime;
        private readonly float cacheDuration;
        private readonly Func<T> computeFunc;

        /// <summary>
        /// 캐시가 만료되었는지 여부
        /// </summary>
        public bool IsExpired => Time.time - cacheTime > cacheDuration;

        /// <summary>
        /// 남은 유효 시간
        /// </summary>
        public float RemainingTime => Mathf.Max(0f, cacheDuration - (Time.time - cacheTime));

        /// <summary>
        /// 생성자
        /// </summary>
        /// <param name="computeFunc">값 계산 함수</param>
        /// <param name="cacheDuration">캐시 유효 시간 (초)</param>
        public TimedCache(Func<T> computeFunc, float cacheDuration)
        {
            this.computeFunc = computeFunc ?? throw new ArgumentNullException(nameof(computeFunc));
            this.cacheDuration = cacheDuration;
            this.cacheTime = float.NegativeInfinity; // 처음에는 만료 상태
        }

        /// <summary>
        /// 캐시된 값 가져오기 (만료되면 재계산)
        /// </summary>
        public T Value
        {
            get
            {
                if (IsExpired)
                {
                    cachedValue = computeFunc();
                    cacheTime = Time.time;
                }
                return cachedValue;
            }
        }

        /// <summary>
        /// 강제로 캐시 무효화
        /// </summary>
        public void Invalidate()
        {
            cacheTime = float.NegativeInfinity;
        }

        /// <summary>
        /// 강제로 값 설정 및 타이머 리셋
        /// </summary>
        public void SetValue(T value)
        {
            cachedValue = value;
            cacheTime = Time.time;
        }

        /// <summary>
        /// 암시적 변환
        /// </summary>
        public static implicit operator T(TimedCache<T> cache) => cache.Value;
    }

    /// <summary>
    /// Dictionary 기반 Dirty 캐시
    /// 개별 키마다 dirty 상태 관리
    /// </summary>
    /// <typeparam name="TKey">키 타입</typeparam>
    /// <typeparam name="TValue">값 타입</typeparam>
    public class DirtyDictCache<TKey, TValue>
    {
        private readonly Dictionary<TKey, TValue> cache = new Dictionary<TKey, TValue>();
        private readonly HashSet<TKey> dirtyKeys = new HashSet<TKey>();
        private bool isAllDirty = true;
        private readonly Func<TKey, TValue> computeFunc;
        private readonly Func<Dictionary<TKey, TValue>> computeAllFunc;

        /// <summary>
        /// 현재 캐시된 항목 수
        /// </summary>
        public int Count => cache.Count;

        /// <summary>
        /// 생성자 (개별 키 계산)
        /// </summary>
        /// <param name="computeFunc">키별 값 계산 함수</param>
        public DirtyDictCache(Func<TKey, TValue> computeFunc)
        {
            this.computeFunc = computeFunc ?? throw new ArgumentNullException(nameof(computeFunc));
        }

        /// <summary>
        /// 생성자 (전체 계산)
        /// </summary>
        /// <param name="computeAllFunc">전체 값 계산 함수</param>
        public DirtyDictCache(Func<Dictionary<TKey, TValue>> computeAllFunc)
        {
            this.computeAllFunc = computeAllFunc ?? throw new ArgumentNullException(nameof(computeAllFunc));
        }

        /// <summary>
        /// 값 가져오기
        /// </summary>
        public TValue Get(TKey key)
        {
            // 전체 dirty면 전체 재계산
            if (isAllDirty && computeAllFunc != null)
            {
                cache.Clear();
                var computed = computeAllFunc();
                foreach (var kvp in computed)
                {
                    cache[kvp.Key] = kvp.Value;
                }
                isAllDirty = false;
                dirtyKeys.Clear();
            }

            // 개별 키 dirty면 재계산
            if (dirtyKeys.Contains(key) && computeFunc != null)
            {
                cache[key] = computeFunc(key);
                dirtyKeys.Remove(key);
            }

            return cache.TryGetValue(key, out TValue value) ? value : default;
        }

        /// <summary>
        /// 모든 값 가져오기
        /// </summary>
        public Dictionary<TKey, TValue> GetAll()
        {
            if (isAllDirty && computeAllFunc != null)
            {
                cache.Clear();
                var computed = computeAllFunc();
                foreach (var kvp in computed)
                {
                    cache[kvp.Key] = kvp.Value;
                }
                isAllDirty = false;
                dirtyKeys.Clear();
            }
            return new Dictionary<TKey, TValue>(cache);
        }

        /// <summary>
        /// 특정 키 무효화
        /// </summary>
        public void Invalidate(TKey key)
        {
            dirtyKeys.Add(key);
        }

        /// <summary>
        /// 전체 무효화
        /// </summary>
        public void InvalidateAll()
        {
            isAllDirty = true;
            dirtyKeys.Clear();
        }

        /// <summary>
        /// 캐시 클리어
        /// </summary>
        public void Clear()
        {
            cache.Clear();
            dirtyKeys.Clear();
            isAllDirty = true;
        }

        /// <summary>
        /// 인덱서
        /// </summary>
        public TValue this[TKey key] => Get(key);
    }
}
