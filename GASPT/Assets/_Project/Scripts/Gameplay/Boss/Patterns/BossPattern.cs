using System;
using UnityEngine;
using GASPT.Core.Enums;
using GASPT.Core.Utilities;

namespace GASPT.Gameplay.Boss
{
    /// <summary>
    /// 보스 패턴 기본 클래스 (추상)
    /// 모든 보스 패턴은 이 클래스를 상속받아 구현
    /// </summary>
    [Serializable]
    public abstract class BossPattern
    {
        // ====== 기본 정보 ======

        [Header("기본 정보")]
        [Tooltip("패턴 이름")]
        public string patternName = "Pattern";

        [Tooltip("패턴 타입")]
        public PatternType patternType = PatternType.Melee;


        // ====== 스탯 ======

        [Header("스탯")]
        [Tooltip("기본 데미지")]
        [Range(1, 200)]
        public int damage = 10;

        [Tooltip("쿨다운 (초)")]
        [Range(0.5f, 30f)]
        public float cooldown = 3f;

        [Tooltip("텔레그래프 표시 시간 (초)")]
        [Range(0f, 5f)]
        public float telegraphDuration = 1f;


        // ====== 가중치 ======

        [Header("가중치 및 제한")]
        [Tooltip("패턴 선택 가중치 (높을수록 자주 선택됨)")]
        [Range(1, 100)]
        public int weight = 10;

        [Tooltip("최소 페이즈 요구 (1 = Phase 1부터 사용 가능)")]
        [Range(1, 4)]
        public int minPhase = 1;

        [Tooltip("최소 사용 거리")]
        [Range(0f, 20f)]
        public float minRange = 0f;

        [Tooltip("최대 사용 거리")]
        [Range(0f, 50f)]
        public float maxRange = 15f;


        // ====== 상태 ======

        /// <summary>
        /// 쿨다운 타이머 (Cooldown struct 사용)
        /// </summary>
        protected Cooldown cooldownTimer;
        protected bool isExecuting = false;
        protected bool isCancelled = false;


        // ====== 프로퍼티 ======

        /// <summary>
        /// 쿨다운 완료 여부
        /// </summary>
        public bool IsOnCooldown => cooldownTimer.IsOnCooldown;

        /// <summary>
        /// 쿨다운 남은 시간
        /// </summary>
        public float RemainingCooldown => cooldownTimer.RemainingTime;

        /// <summary>
        /// 현재 실행 중 여부
        /// </summary>
        public bool IsExecuting => isExecuting;


        // ====== 추상 메서드 ======

        /// <summary>
        /// 텔레그래프 표시 (공격 예고)
        /// </summary>
        /// <param name="boss">보스 인스턴스</param>
        /// <param name="targetPosition">대상 위치</param>
        public abstract void ShowTelegraph(BaseBoss boss, Vector3 targetPosition);

        /// <summary>
        /// 패턴 실행
        /// </summary>
        /// <param name="boss">보스 인스턴스</param>
        /// <param name="target">대상 Transform</param>
        public abstract Awaitable Execute(BaseBoss boss, Transform target);


        // ====== 공통 메서드 ======

        /// <summary>
        /// 패턴 사용 가능 여부 확인
        /// </summary>
        /// <param name="currentPhase">현재 페이즈 (1부터 시작)</param>
        /// <param name="distanceToTarget">대상까지의 거리</param>
        public virtual bool CanUse(int currentPhase, float distanceToTarget)
        {
            // 쿨다운 체크
            if (IsOnCooldown)
                return false;

            // 페이즈 체크
            if (currentPhase < minPhase)
                return false;

            // 거리 체크
            if (distanceToTarget < minRange || distanceToTarget > maxRange)
                return false;

            // 실행 중 체크
            if (isExecuting)
                return false;

            return true;
        }

        /// <summary>
        /// 패턴 취소
        /// </summary>
        public virtual void Cancel()
        {
            isCancelled = true;
        }

        /// <summary>
        /// 쿨다운 시작 (사용 후 호출)
        /// </summary>
        public void ResetCooldown()
        {
            // cooldown 필드 값으로 Duration 설정 후 시작
            cooldownTimer.Duration = cooldown;
            cooldownTimer.Start();
        }

        /// <summary>
        /// 쿨다운 강제 완료 (디버그용)
        /// </summary>
        public void ForceCooldownComplete()
        {
            cooldownTimer.Reset();
        }


        // ====== 보호 메서드 ======

        /// <summary>
        /// 패턴 실행 시작 처리
        /// </summary>
        protected void BeginExecution()
        {
            isExecuting = true;
            isCancelled = false;
        }

        /// <summary>
        /// 패턴 실행 종료 처리
        /// </summary>
        protected void EndExecution()
        {
            isExecuting = false;
            ResetCooldown(); // 쿨다운 시작
        }

        /// <summary>
        /// 취소 확인 (Awaitable 내에서 사용)
        /// </summary>
        protected bool IsCancelled()
        {
            return isCancelled;
        }

        /// <summary>
        /// 범위 내 플레이어에게 데미지 적용
        /// </summary>
        protected void ApplyDamageInRadius(Vector3 center, float radius, int damageAmount)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(center, radius);

            foreach (var hit in hits)
            {
                var playerStats = hit.GetComponent<GASPT.Stats.PlayerStats>();
                if (playerStats != null && !playerStats.IsDead)
                {
                    playerStats.TakeDamage(damageAmount);
                    Debug.Log($"[BossPattern] {patternName}: 플레이어에게 {damageAmount} 데미지!");
                }
            }
        }

        /// <summary>
        /// 직선 범위 내 플레이어에게 데미지 적용
        /// </summary>
        protected void ApplyDamageInLine(Vector3 start, Vector3 end, float width, int damageAmount)
        {
            Vector2 direction = (end - start).normalized;
            float distance = Vector2.Distance(start, end);

            RaycastHit2D[] hits = Physics2D.BoxCastAll(
                start,
                new Vector2(width, 0.1f),
                Vector2.SignedAngle(Vector2.right, direction),
                direction,
                distance
            );

            foreach (var hit in hits)
            {
                var playerStats = hit.collider.GetComponent<GASPT.Stats.PlayerStats>();
                if (playerStats != null && !playerStats.IsDead)
                {
                    playerStats.TakeDamage(damageAmount);
                    Debug.Log($"[BossPattern] {patternName}: 플레이어에게 {damageAmount} 데미지!");
                }
            }
        }


        // ====== 디버그 ======

        public override string ToString()
        {
            return $"[{patternType}] {patternName}: DMG={damage}, CD={cooldown}s, Weight={weight}";
        }
    }
}
