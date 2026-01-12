using System;
using System.Collections.Generic;
using UnityEngine;

namespace GASPT.Core.Utilities
{
    /// <summary>
    /// 단일 쿨다운 관리 구조체
    /// BaseAbility, BossPattern 등에서 사용
    /// </summary>
    [Serializable]
    public struct Cooldown
    {
        [SerializeField] private float duration;
        private float lastUseTime;

        /// <summary>
        /// 쿨다운 시간 (초)
        /// </summary>
        public float Duration
        {
            get => duration;
            set => duration = value;
        }

        /// <summary>
        /// 생성자
        /// </summary>
        /// <param name="duration">쿨다운 시간 (초)</param>
        public Cooldown(float duration)
        {
            this.duration = duration;
            this.lastUseTime = float.NegativeInfinity;
        }

        /// <summary>
        /// 쿨다운 중 여부
        /// </summary>
        public bool IsOnCooldown => Time.time - lastUseTime < duration;

        /// <summary>
        /// 사용 가능 여부
        /// </summary>
        public bool IsReady => !IsOnCooldown;

        /// <summary>
        /// 남은 쿨다운 시간 (초)
        /// </summary>
        public float RemainingTime => Mathf.Max(0f, duration - (Time.time - lastUseTime));

        /// <summary>
        /// 쿨다운 진행률 (0 = 시작, 1 = 완료)
        /// </summary>
        public float Progress => duration > 0f ? Mathf.Clamp01(1f - (RemainingTime / duration)) : 1f;

        /// <summary>
        /// 쿨다운 시작
        /// </summary>
        public void Start()
        {
            lastUseTime = Time.time;
        }

        /// <summary>
        /// 쿨다운 리셋 (즉시 사용 가능)
        /// </summary>
        public void Reset()
        {
            lastUseTime = float.NegativeInfinity;
        }

        /// <summary>
        /// 쿨다운 체크 후 시작 (사용 가능하면 true 반환 후 쿨다운 시작)
        /// </summary>
        public bool TryUse()
        {
            if (IsOnCooldown)
                return false;

            Start();
            return true;
        }

        /// <summary>
        /// 쿨다운 감소 (스킬 등으로 쿨다운 단축 시 사용)
        /// </summary>
        /// <param name="amount">감소할 시간 (초)</param>
        public void ReduceBy(float amount)
        {
            lastUseTime -= amount;
        }
    }

    /// <summary>
    /// 여러 쿨다운을 Key로 관리하는 트래커
    /// ConsumableManager, ShopSystem 등에서 사용
    /// </summary>
    /// <typeparam name="TKey">쿨다운 식별자 타입 (string, int, Enum 등)</typeparam>
    public class CooldownTracker<TKey>
    {
        /// <summary>
        /// 쿨다운 종료 시간 저장
        /// Key → EndTime (Time.time 기준)
        /// </summary>
        private readonly Dictionary<TKey, float> cooldownEndTimes = new Dictionary<TKey, float>();

        /// <summary>
        /// 쿨다운 시작 이벤트
        /// 매개변수: (Key, 쿨다운 시간)
        /// </summary>
        public event Action<TKey, float> OnCooldownStarted;

        /// <summary>
        /// 쿨다운 종료 이벤트
        /// 매개변수: (Key)
        /// </summary>
        public event Action<TKey> OnCooldownEnded;

        /// <summary>
        /// 쿨다운 시작
        /// </summary>
        /// <param name="key">식별자</param>
        /// <param name="duration">쿨다운 시간 (초)</param>
        public void Start(TKey key, float duration)
        {
            if (duration <= 0f)
                return;

            cooldownEndTimes[key] = Time.time + duration;
            OnCooldownStarted?.Invoke(key, duration);
        }

        /// <summary>
        /// 쿨다운 중 여부
        /// </summary>
        public bool IsOnCooldown(TKey key)
        {
            if (!cooldownEndTimes.TryGetValue(key, out float endTime))
                return false;

            return Time.time < endTime;
        }

        /// <summary>
        /// 사용 가능 여부
        /// </summary>
        public bool IsReady(TKey key)
        {
            return !IsOnCooldown(key);
        }

        /// <summary>
        /// 남은 쿨다운 시간 (초)
        /// </summary>
        public float GetRemainingTime(TKey key)
        {
            if (!cooldownEndTimes.TryGetValue(key, out float endTime))
                return 0f;

            return Mathf.Max(0f, endTime - Time.time);
        }

        /// <summary>
        /// 쿨다운 체크 후 시작 (사용 가능하면 true 반환 후 쿨다운 시작)
        /// </summary>
        public bool TryUse(TKey key, float duration)
        {
            if (IsOnCooldown(key))
                return false;

            Start(key, duration);
            return true;
        }

        /// <summary>
        /// 특정 쿨다운 리셋 (즉시 사용 가능)
        /// </summary>
        public void Reset(TKey key)
        {
            if (cooldownEndTimes.Remove(key))
            {
                OnCooldownEnded?.Invoke(key);
            }
        }

        /// <summary>
        /// 모든 쿨다운 리셋
        /// </summary>
        public void ResetAll()
        {
            var keys = new List<TKey>(cooldownEndTimes.Keys);
            cooldownEndTimes.Clear();

            foreach (var key in keys)
            {
                OnCooldownEnded?.Invoke(key);
            }
        }

        /// <summary>
        /// 특정 쿨다운 감소
        /// </summary>
        /// <param name="key">식별자</param>
        /// <param name="amount">감소할 시간 (초)</param>
        public void ReduceBy(TKey key, float amount)
        {
            if (cooldownEndTimes.TryGetValue(key, out float endTime))
            {
                cooldownEndTimes[key] = endTime - amount;
            }
        }

        /// <summary>
        /// 만료된 쿨다운 정리 (Update에서 호출)
        /// </summary>
        public void Tick()
        {
            var expiredKeys = new List<TKey>();

            foreach (var kvp in cooldownEndTimes)
            {
                if (Time.time >= kvp.Value)
                {
                    expiredKeys.Add(kvp.Key);
                }
            }

            foreach (var key in expiredKeys)
            {
                cooldownEndTimes.Remove(key);
                OnCooldownEnded?.Invoke(key);
            }
        }

        /// <summary>
        /// 현재 쿨다운 중인 항목 수
        /// </summary>
        public int ActiveCount => cooldownEndTimes.Count;

        /// <summary>
        /// 모든 쿨다운 정보 가져오기 (디버그용)
        /// </summary>
        public IEnumerable<KeyValuePair<TKey, float>> GetAllCooldowns()
        {
            foreach (var kvp in cooldownEndTimes)
            {
                yield return new KeyValuePair<TKey, float>(kvp.Key, GetRemainingTime(kvp.Key));
            }
        }

        /// <summary>
        /// 특정 쿨다운 연장
        /// </summary>
        /// <param name="key">식별자</param>
        /// <param name="amount">연장할 시간 (초)</param>
        public void ExtendBy(TKey key, float amount)
        {
            if (cooldownEndTimes.TryGetValue(key, out float endTime))
            {
                cooldownEndTimes[key] = endTime + amount;
            }
        }
    }

    /// <summary>
    /// string 키를 사용하는 CooldownTracker 확장
    /// </summary>
    public class StringCooldownTracker : CooldownTracker<string> { }

    /// <summary>
    /// int 키를 사용하는 CooldownTracker 확장
    /// </summary>
    public class IntCooldownTracker : CooldownTracker<int> { }
}
