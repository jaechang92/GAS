using System.Collections.Generic;
using UnityEngine;

namespace GAS.Core
{
    /// <summary>
    /// 범용 어빌리티 실행기 추상 클래스
    /// </summary>
    public abstract class AbilityExecutor : ScriptableObject
    {
        [Header("실행기 정보")]
        [SerializeField] protected string executorId;
        [SerializeField] protected string executorName;
        [SerializeField] protected string description;

        [Header("효과 설정")]
        [SerializeField] protected GameObject effectPrefab;
        [SerializeField] protected AudioClip soundEffect;
        [SerializeField] protected float executionDelay = 0f;

        // 프로퍼티
        public string Id => executorId;
        public string Name => executorName;
        public string Description => description;

        /// <summary>
        /// 어빌리티 실행 전 검증
        /// </summary>
        public virtual bool Validate(GameObject caster, IAbilityData data, List<IAbilityTarget> targets)
        {
            // 기본 검증 로직 구현
            if (caster == null || data == null) return false;
            return true;
        }

        /// <summary>
        /// 어빌리티 실행 메인 로직 (비동기)
        /// </summary>
        public abstract Awaitable<bool> ExecuteAsync(GameObject caster, IAbilityData data, List<IAbilityTarget> targets);

        /// <summary>
        /// 타겟 유효성 검사
        /// </summary>
        protected virtual bool IsValidTarget(GameObject caster, IAbilityTarget target, IAbilityData data)
        {
            if (target == null || !target.IsTargetable || !target.IsAlive)
                return false;

            // 팀 체크 (필요시 구현)
            return true;
        }

        /// <summary>
        /// 이펙트 생성
        /// </summary>
        protected virtual GameObject SpawnEffect(Vector3 position, Quaternion rotation = default)
        {
            if (effectPrefab == null) return null;

            var effect = Instantiate(effectPrefab, position, rotation);

            // 자동 정리 (5초 후)
            if (effect != null)
            {
                Destroy(effect, 5f);
            }

            return effect;
        }

        /// <summary>
        /// 사운드 재생
        /// </summary>
        protected virtual void PlaySound(Vector3 position)
        {
            if (soundEffect != null)
            {
                AudioSource.PlayClipAtPoint(soundEffect, position);
            }
        }

        /// <summary>
        /// 실행 전 처리
        /// </summary>
        protected virtual async Awaitable OnPreExecute(GameObject caster, IAbilityData data)
        {
            // 실행 지연
            if (executionDelay > 0)
            {
                await Awaitable.WaitForSecondsAsync(executionDelay);
            }

            // 이펙트 및 사운드
            SpawnEffect(caster.transform.position);
            PlaySound(caster.transform.position);
        }

        /// <summary>
        /// 실행 후 처리
        /// </summary>
        protected virtual async Awaitable OnPostExecute(GameObject caster, IAbilityData data)
        {
            // 후처리 로직 구현 가능
            await Awaitable.NextFrameAsync();
        }

        /// <summary>
        /// 범위 내 타겟 찾기
        /// </summary>
        protected virtual List<IAbilityTarget> FindTargetsInRange(Vector3 center, float range, LayerMask targetLayer)
        {
            var targets = new List<IAbilityTarget>();

            var colliders = Physics.OverlapSphere(center, range, targetLayer);

            foreach (var collider in colliders)
            {
                var target = collider.GetComponent<IAbilityTarget>();
                if (target != null && target.IsTargetable)
                {
                    targets.Add(target);
                }
            }

            return targets;
        }

        /// <summary>
        /// 2D 범위 내 타겟 찾기
        /// </summary>
        protected virtual List<IAbilityTarget> FindTargetsInRange2D(Vector2 center, float range, LayerMask targetLayer)
        {
            var targets = new List<IAbilityTarget>();

            var colliders = Physics2D.OverlapCircleAll(center, range, targetLayer);

            foreach (var collider in colliders)
            {
                var target = collider.GetComponent<IAbilityTarget>();
                if (target != null && target.IsTargetable)
                {
                    targets.Add(target);
                }
            }

            return targets;
        }

        /// <summary>
        /// 방향성 타겟 찾기
        /// </summary>
        protected virtual List<IAbilityTarget> FindTargetsInDirection(
            Vector3 origin, Vector3 direction, float range, float angle, LayerMask targetLayer)
        {
            var targets = new List<IAbilityTarget>();
            var allTargets = FindTargetsInRange(origin, range, targetLayer);

            foreach (var target in allTargets)
            {
                Vector3 toTarget = (target.Transform.position - origin).normalized;
                float targetAngle = Vector3.Angle(direction, toTarget);

                if (targetAngle <= angle * 0.5f)
                {
                    targets.Add(target);
                }
            }

            return targets;
        }

        /// <summary>
        /// 라인 타겟 찾기
        /// </summary>
        protected virtual List<IAbilityTarget> FindTargetsInLine(
            Vector3 start, Vector3 end, float width, LayerMask targetLayer)
        {
            var targets = new List<IAbilityTarget>();

            // 라인 중심점과 방향 계산
            Vector3 center = (start + end) * 0.5f;
            Vector3 direction = (end - start).normalized;
            float distance = Vector3.Distance(start, end);

            // 박스 캐스트로 라인 상의 타겟 찾기
            var hits = Physics.BoxCastAll(center, new Vector3(width * 0.5f, width * 0.5f, distance * 0.5f),
                direction, Quaternion.LookRotation(direction), 0, targetLayer);

            foreach (var hit in hits)
            {
                var target = hit.collider.GetComponent<IAbilityTarget>();
                if (target != null && target.IsTargetable)
                {
                    targets.Add(target);
                }
            }

            return targets;
        }
    }
}