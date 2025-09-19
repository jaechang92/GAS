// ===================================
// 파일: Assets/Scripts/Ability/Executors/AbilityExecutor.cs
// ===================================
using AbilitySystem.Platformer;
using Helper;
using System.Collections.Generic;
using UnityEngine;

namespace AbilitySystem
{
    /// <summary>
    /// 어빌리티 실행 로직의 추상 클래스
    /// </summary>
    public abstract class AbilityExecutor : ScriptableObject
    {
        [Header("실행자 설정")]
        [SerializeField] protected string executorId;
        [SerializeField] protected string executorName;

        [Header("효과 설정")]
        [SerializeField] protected GameObject effectPrefab;
        [SerializeField] protected AudioClip soundEffect;
        [SerializeField] protected float executionDelay = 0f;

        // 프로퍼티
        public string Id => executorId;
        public string Name => executorName;

        /// <summary>
        /// 어빌리티 실행 전 검증
        /// </summary>
        public virtual bool Validate(GameObject caster, PlatformerAbilityData data, List<IAbilityTarget> targets)
        {
            // 실행 가능 여부 검증
            return true;
        }

        /// <summary>
        /// 어빌리티 메인 실행 로직 (비동기)
        /// </summary>
        public abstract Awaitable ExecuteAsync(GameObject caster, PlatformerAbilityData data, List<IAbilityTarget> targets);

        /// <summary>
        /// 타겟 유효성 검사
        /// </summary>
        protected virtual bool IsValidTarget(GameObject caster, IAbilityTarget target, PlatformerAbilityData data)
        {
            // 타겟이 유효한지 검사
            return false;
        }

        /// <summary>
        /// 이펙트 생성
        /// </summary>
        protected virtual void SpawnEffect(Vector3 position, Quaternion rotation)
        {
            // 시각 효과 생성
        }

        /// <summary>
        /// 사운드 재생
        /// </summary>
        protected virtual void PlaySound(Vector3 position)
        {
            // 사운드 효과 재생
        }

        /// <summary>
        /// 실행 전 처리
        /// </summary>
        protected virtual Awaitable OnPreExecute(GameObject caster, SkulData data)
        {
            // 실행 전 준비 작업
            return AwaitableHelper.CompletedTask;
        }

        /// <summary>
        /// 실행 후 처리
        /// </summary>
        protected virtual Awaitable OnPostExecute(GameObject caster, SkulData data)
        {
            // 실행 후 정리 작업
            return AwaitableHelper.CompletedTask;
        }

        /// <summary>
        /// 범위 내 타겟 찾기
        /// </summary>
        protected List<IAbilityTarget> FindTargetsInRange(Vector3 center, float range, LayerMask targetLayer)
        {
            // 범위 내 모든 타겟 검색
            return new List<IAbilityTarget>();
        }
    }
}