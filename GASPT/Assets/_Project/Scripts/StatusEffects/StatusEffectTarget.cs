using GASPT.Core.Enums;
using GASPT.Core.Extensions;
using GASPT.Core;
using GASPT.Data;
using UnityEngine;

namespace GASPT.StatusEffects
{
    /// <summary>
    /// 상태 효과를 받을 수 있는 컴포넌트
    /// 적, 플레이어 등에 부착하여 사용
    /// </summary>
    public class StatusEffectTarget : MonoBehaviour, IStatusEffectTarget
    {
        [Header("기본 설정")]
        [SerializeField] private bool showDebugLogs = true;

        [Header("상태 효과 데이터 (캐싱용)")]
        [SerializeField] private StatusEffectData stunData;
        [SerializeField] private StatusEffectData slowData;
        [SerializeField] private StatusEffectData burnData;
        [SerializeField] private StatusEffectData poisonData;
        [SerializeField] private StatusEffectData bleedData;
        [SerializeField] private StatusEffectData attackUpData;
        [SerializeField] private StatusEffectData speedUpData;
        [SerializeField] private StatusEffectData invincibleData;

        // 컴포넌트 캐싱
        private IDamageable damageable;
        private Rigidbody2D rb;
        private float originalSpeed;

        private void Awake()
        {
            damageable = GetComponent<IDamageable>();
            rb = GetComponent<Rigidbody2D>();
        }

        private void OnEnable()
        {
            // StatusEffectManager 이벤트 구독
            if (StatusEffectManager.Instance != null)
            {
                StatusEffectManager.Instance.OnEffectApplied += HandleEffectApplied;
                StatusEffectManager.Instance.OnEffectRemoved += HandleEffectRemoved;
            }
        }

        private void OnDisable()
        {
            // 이벤트 구독 해제
            if (StatusEffectManager.Instance != null)
            {
                StatusEffectManager.Instance.OnEffectApplied -= HandleEffectApplied;
                StatusEffectManager.Instance.OnEffectRemoved -= HandleEffectRemoved;
            }
        }

        // ===== IStatusEffectTarget 구현 =====

        /// <summary>
        /// 상태 효과 적용
        /// </summary>
        public void ApplyStatusEffect(StatusEffectType effectType, float duration, float value = 0f)
        {
            StatusEffectData data = GetStatusEffectData(effectType, duration, value);

            if (data != null)
            {
                StatusEffectManager.Instance?.ApplyEffect(gameObject, data);
            }
            else
            {
                // 데이터가 없으면 동적으로 생성
                ApplyDynamicEffect(effectType, duration, value);
            }

            if (showDebugLogs)
                Debug.Log($"[StatusEffectTarget] {effectType} 효과 적용 요청 - 대상: {gameObject.name}");
        }

        /// <summary>
        /// 상태 효과 제거
        /// </summary>
        public void RemoveStatusEffect(StatusEffectType effectType)
        {
            StatusEffectManager.Instance?.RemoveEffect(gameObject, effectType);
        }

        /// <summary>
        /// 특정 상태 효과 보유 여부
        /// </summary>
        public bool HasStatusEffect(StatusEffectType effectType)
        {
            return StatusEffectManager.Instance?.HasEffect(gameObject, effectType) ?? false;
        }

        /// <summary>
        /// 모든 상태 효과 제거
        /// </summary>
        public void ClearAllStatusEffects()
        {
            StatusEffectManager.Instance?.RemoveAllEffects(gameObject);
        }

        /// <summary>
        /// 모든 디버프 제거
        /// </summary>
        public void ClearDebuffs()
        {
            StatusEffectManager.Instance?.RemoveAllDebuffs(gameObject);
        }

        /// <summary>
        /// 모든 버프 제거
        /// </summary>
        public void ClearBuffs()
        {
            StatusEffectManager.Instance?.RemoveAllBuffs(gameObject);
        }

        // ===== 내부 메서드 =====

        /// <summary>
        /// 효과 타입에 맞는 데이터 가져오기
        /// </summary>
        private StatusEffectData GetStatusEffectData(StatusEffectType effectType, float duration, float value)
        {
            return effectType switch
            {
                StatusEffectType.Stun => stunData,
                StatusEffectType.Slow => slowData,
                StatusEffectType.Burn => burnData,
                StatusEffectType.Poison => poisonData,
                StatusEffectType.Bleed => bleedData,
                StatusEffectType.AttackUp => attackUpData,
                StatusEffectType.SpeedUp => speedUpData,
                StatusEffectType.Invincible => invincibleData,
                _ => null
            };
        }

        /// <summary>
        /// 동적으로 효과 적용 (데이터 없을 때)
        /// </summary>
        private void ApplyDynamicEffect(StatusEffectType effectType, float duration, float value)
        {
            // 임시 StatusEffect 생성
            var effect = new StatusEffect(
                effectType: effectType,
                displayName: effectType.ToString(),
                description: $"{effectType} 효과",
                value: value,
                duration: duration,
                tickInterval: effectType.IsDoT() ? 1f : 0f,
                maxStack: 3,
                icon: null,
                isBuff: effectType.IsBuff()
            );

            effect.Apply(gameObject);

            // 즉시 효과 적용
            ApplyImmediateEffect(effectType, value);
        }

        /// <summary>
        /// 즉시 효과 적용 (스턴, 슬로우 등)
        /// </summary>
        private void ApplyImmediateEffect(StatusEffectType effectType, float value)
        {
            switch (effectType)
            {
                case StatusEffectType.Stun:
                    // 스턴: 이동 중지
                    if (rb != null)
                    {
                        rb.linearVelocity = Vector2.zero;
                    }
                    break;

                case StatusEffectType.Slow:
                    // 슬로우: 속도 감소 (value = 감소 비율 %)
                    // CharacterController나 다른 이동 시스템과 연동 필요
                    break;

                case StatusEffectType.Invincible:
                    // 무적
                    damageable?.SetInvincible(true);
                    break;
            }
        }

        // ===== 이벤트 핸들러 =====

        /// <summary>
        /// 효과 적용 시 호출
        /// </summary>
        private void HandleEffectApplied(GameObject target, StatusEffect effect)
        {
            if (target != gameObject) return;

            if (showDebugLogs)
                Debug.Log($"[StatusEffectTarget] 효과 적용됨: {effect.DisplayName}");

            // 효과별 추가 처리
            switch (effect.EffectType)
            {
                case StatusEffectType.Stun:
                    OnStunApplied();
                    break;
                case StatusEffectType.Slow:
                    OnSlowApplied(effect.Value);
                    break;
                case StatusEffectType.Invincible:
                    damageable?.SetInvincible(true);
                    break;
            }

            // DoT 효과 틱 구독
            if (effect.EffectType.IsDoT())
            {
                effect.OnTick += HandleDoTTick;
            }
        }

        /// <summary>
        /// 효과 제거 시 호출
        /// </summary>
        private void HandleEffectRemoved(GameObject target, StatusEffect effect)
        {
            if (target != gameObject) return;

            if (showDebugLogs)
                Debug.Log($"[StatusEffectTarget] 효과 제거됨: {effect.DisplayName}");

            // 효과별 해제 처리
            switch (effect.EffectType)
            {
                case StatusEffectType.Stun:
                    OnStunRemoved();
                    break;
                case StatusEffectType.Slow:
                    OnSlowRemoved();
                    break;
                case StatusEffectType.Invincible:
                    damageable?.SetInvincible(false);
                    break;
            }

            // DoT 틱 구독 해제
            if (effect.EffectType.IsDoT())
            {
                effect.OnTick -= HandleDoTTick;
            }
        }

        /// <summary>
        /// DoT 틱 핸들러
        /// </summary>
        private void HandleDoTTick(StatusEffect effect, float damage)
        {
            // DoT 대미지 적용
            var damageType = effect.EffectType switch
            {
                StatusEffectType.Burn => DamageType.Fire,
                StatusEffectType.Poison => DamageType.Poison,
                StatusEffectType.Bleed => DamageType.Bleed,
                _ => DamageType.Normal
            };

            damageable?.TakeDamage(damage, damageType);

            if (showDebugLogs)
                Debug.Log($"[StatusEffectTarget] {effect.DisplayName} 틱 대미지: {damage}");
        }

        // ===== 효과별 처리 =====

        private void OnStunApplied()
        {
            // 스턴 시 모든 행동 중지
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }
            // TODO: 애니메이터 상태 변경
        }

        private void OnStunRemoved()
        {
            // 스턴 해제
            // TODO: 정상 상태로 복귀
        }

        private void OnSlowApplied(float slowPercent)
        {
            // 슬로우 적용
            // TODO: CharacterPhysics 또는 이동 시스템과 연동
        }

        private void OnSlowRemoved()
        {
            // 슬로우 해제
            // TODO: 원래 속도로 복귀
        }
    }
}
