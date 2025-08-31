using System;
using System.Collections.Generic;
using UnityEngine;
using GAS.Core;
using GAS.TagSystem;

namespace GAS.EffectSystem
{
    /// <summary>
    /// Effect 실행 시 필요한 컨텍스트 정보를 담는 클래스
    /// Instigator(시전자), Target(대상), 추가 파라미터 등을 관리
    /// </summary>
    [Serializable]
    public class EffectContext
    {
        #region Fields

        private GameObject instigator;
        private GameObject target;
        private GameplayEffect sourceEffect;
        private object sourceAbility;
        private float magnitude;
        private int stackCount;
        private float elapsedTime;
        private Dictionary<string, object> additionalData;
        private Vector3 hitPoint;
        private Vector3 hitNormal;
        private TagContainer contextTags;

        #endregion

        #region Properties

        /// <summary>
        /// Effect 시전자
        /// </summary>
        public GameObject Instigator
        {
            get => instigator;
            set => instigator = value;
        }

        /// <summary>
        /// Effect 대상
        /// </summary>
        public GameObject Target
        {
            get => target;
            set => target = value;
        }

        /// <summary>
        /// 원본 Effect
        /// </summary>
        public GameplayEffect SourceEffect
        {
            get => sourceEffect;
            set => sourceEffect = value;
        }

        /// <summary>
        /// Effect를 발생시킨 Ability (Phase 3에서 사용)
        /// </summary>
        public object SourceAbility
        {
            get => sourceAbility;
            set => sourceAbility = value;
        }

        /// <summary>
        /// Effect 강도 배수
        /// </summary>
        public float Magnitude
        {
            get => magnitude;
            set => magnitude = Mathf.Max(0f, value);
        }

        /// <summary>
        /// 현재 스택 수
        /// </summary>
        public int StackCount
        {
            get => stackCount;
            set => stackCount = Mathf.Max(0, value);
        }

        /// <summary>
        /// Effect 경과 시간
        /// </summary>
        public float ElapsedTime
        {
            get => elapsedTime;
            set => elapsedTime = Mathf.Max(0f, value);
        }

        /// <summary>
        /// 충돌 지점 (투사체 등에서 사용)
        /// </summary>
        public Vector3 HitPoint
        {
            get => hitPoint;
            set => hitPoint = value;
        }

        /// <summary>
        /// 충돌 표면 법선
        /// </summary>
        public Vector3 HitNormal
        {
            get => hitNormal;
            set => hitNormal = value;
        }

        /// <summary>
        /// 컨텍스트 태그
        /// </summary>
        public TagContainer ContextTags
        {
            get => contextTags;
            set => contextTags = value;
        }

        /// <summary>
        /// 추가 데이터 딕셔너리
        /// </summary>
        public Dictionary<string, object> AdditionalData
        {
            get => additionalData ??= new Dictionary<string, object>();
        }

        #endregion

        #region Constructors

        /// <summary>
        /// 기본 생성자
        /// </summary>
        public EffectContext()
        {
            magnitude = 1f;
            stackCount = 1;
            elapsedTime = 0f;
            additionalData = new Dictionary<string, object>();
            contextTags = new TagContainer();
        }

        /// <summary>
        /// 필수 파라미터를 받는 생성자
        /// </summary>
        /// <param name="instigator">시전자</param>
        /// <param name="target">대상</param>
        /// <param name="sourceEffect">원본 Effect</param>
        public EffectContext(GameObject instigator, GameObject target, GameplayEffect sourceEffect) : this()
        {
            this.instigator = instigator;
            this.target = target;
            this.sourceEffect = sourceEffect;
        }

        /// <summary>
        /// 전체 파라미터를 받는 생성자
        /// </summary>
        public EffectContext(
            GameObject instigator,
            GameObject target,
            GameplayEffect sourceEffect,
            float magnitude = 1f,
            int stackCount = 1) : this(instigator, target, sourceEffect)
        {
            this.magnitude = magnitude;
            this.stackCount = stackCount;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 추가 데이터 설정
        /// </summary>
        public void SetData(string key, object value)
        {
            if (string.IsNullOrEmpty(key)) return;

            AdditionalData[key] = value;
        }

        /// <summary>
        /// 추가 데이터 가져오기
        /// </summary>
        public T GetData<T>(string key, T defaultValue = default)
        {
            if (string.IsNullOrEmpty(key)) return defaultValue;

            if (AdditionalData.TryGetValue(key, out var value) && value is T typedValue)
            {
                return typedValue;
            }

            return defaultValue;
        }

        /// <summary>
        /// 추가 데이터 존재 여부 확인
        /// </summary>
        public bool HasData(string key)
        {
            return !string.IsNullOrEmpty(key) && AdditionalData.ContainsKey(key);
        }

        /// <summary>
        /// 추가 데이터 제거
        /// </summary>
        public bool RemoveData(string key)
        {
            if (string.IsNullOrEmpty(key)) return false;

            return AdditionalData.Remove(key);
        }

        /// <summary>
        /// 모든 추가 데이터 제거
        /// </summary>
        public void ClearData()
        {
            AdditionalData.Clear();
        }

        /// <summary>
        /// 컨텍스트 태그 추가
        /// </summary>
        public void AddContextTag(GameplayTag tag)
        {
            contextTags?.AddTag(tag);
        }

        /// <summary>
        /// 컨텍스트 태그 제거
        /// </summary>
        public void RemoveContextTag(GameplayTag tag)
        {
            contextTags?.RemoveTag(tag);
        }

        /// <summary>
        /// 컨텍스트 태그 확인
        /// </summary>
        public bool HasContextTag(GameplayTag tag)
        {
            return contextTags?.HasTag(tag) ?? false;
        }

        /// <summary>
        /// 컨텍스트 복사 (Deep Copy)
        /// </summary>
        public EffectContext Clone()
        {
            var clone = new EffectContext
            {
                instigator = instigator,
                target = target,
                sourceEffect = sourceEffect,
                sourceAbility = sourceAbility,
                magnitude = magnitude,
                stackCount = stackCount,
                elapsedTime = elapsedTime,
                hitPoint = hitPoint,
                hitNormal = hitNormal,
                contextTags = contextTags?.Clone()
            };

            // 추가 데이터 복사
            foreach (var kvp in AdditionalData)
            {
                clone.AdditionalData[kvp.Key] = kvp.Value;
            }

            return clone;
        }

        /// <summary>
        /// 컨텍스트 리셋
        /// </summary>
        public void Reset()
        {
            instigator = null;
            target = null;
            sourceEffect = null;
            sourceAbility = null;
            magnitude = 1f;
            stackCount = 1;
            elapsedTime = 0f;
            hitPoint = Vector3.zero;
            hitNormal = Vector3.up;
            contextTags?.Clear();
            AdditionalData.Clear();
        }

        /// <summary>
        /// 유효성 검증
        /// </summary>
        public bool IsValid()
        {
            // 최소한 target은 있어야 함
            return target != null && sourceEffect != null;
        }

        /// <summary>
        /// Instigator와 Target이 같은지 확인
        /// </summary>
        public bool IsSelfTarget()
        {
            return instigator != null && target != null && instigator == target;
        }

        /// <summary>
        /// 거리 계산 (Instigator와 Target 간)
        /// </summary>
        public float GetDistance()
        {
            if (instigator == null || target == null) return 0f;

            return Vector3.Distance(instigator.transform.position, target.transform.position);
        }

        /// <summary>
        /// 방향 벡터 계산 (Instigator에서 Target으로)
        /// </summary>
        public Vector3 GetDirection()
        {
            if (instigator == null || target == null) return Vector3.forward;

            return (target.transform.position - instigator.transform.position).normalized;
        }

        /// <summary>
        /// 컨텍스트 정보 문자열 반환
        /// </summary>
        public override string ToString()
        {
            return $"EffectContext: {sourceEffect?.name ?? "Unknown"} " +
                   $"[{instigator?.name ?? "None"} -> {target?.name ?? "None"}] " +
                   $"Mag: {magnitude:F2}, Stack: {stackCount}";
        }

        #endregion

        #region Static Factory Methods

        /// <summary>
        /// Self-target 컨텍스트 생성
        /// </summary>
        public static EffectContext CreateSelfContext(GameObject self, GameplayEffect effect)
        {
            return new EffectContext(self, self, effect);
        }

        /// <summary>
        /// AOE 컨텍스트 생성
        /// </summary>
        public static EffectContext CreateAOEContext(GameObject instigator, GameObject target, GameplayEffect effect, Vector3 center)
        {
            var context = new EffectContext(instigator, target, effect);
            context.SetData("AOECenter", center);
            context.SetData("AOERadius", 5f); // 기본값
            return context;
        }

        /// <summary>
        /// 투사체 컨텍스트 생성
        /// </summary>
        public static EffectContext CreateProjectileContext(
            GameObject instigator,
            GameObject target,
            GameplayEffect effect,
            Vector3 hitPoint,
            Vector3 hitNormal)
        {
            var context = new EffectContext(instigator, target, effect)
            {
                hitPoint = hitPoint,
                hitNormal = hitNormal
            };
            context.AddContextTag(new GameplayTag("Effect.Source.Projectile"));
            return context;
        }

        #endregion
    }

    /// <summary>
    /// EffectContext Pool을 위한 정적 클래스
    /// 메모리 할당 최적화
    /// </summary>
    public static class EffectContextPool
    {
        private static readonly Stack<EffectContext> pool = new Stack<EffectContext>();
        private const int MaxPoolSize = 50;

        /// <summary>
        /// Pool에서 Context 가져오기
        /// </summary>
        public static EffectContext Get()
        {
            if (pool.Count > 0)
            {
                var context = pool.Pop();
                context.Reset();
                return context;
            }

            return new EffectContext();
        }

        /// <summary>
        /// Pool에 Context 반환
        /// </summary>
        public static void Return(EffectContext context)
        {
            if (context == null || pool.Count >= MaxPoolSize) return;

            context.Reset();
            pool.Push(context);
        }

        /// <summary>
        /// Pool 초기화
        /// </summary>
        public static void Clear()
        {
            pool.Clear();
        }

        /// <summary>
        /// Pool 사전 할당
        /// </summary>
        public static void Prewarm(int count)
        {
            for (int i = 0; i < count && pool.Count < MaxPoolSize; i++)
            {
                pool.Push(new EffectContext());
            }
        }
    }
}