// 파일 위치: Assets/Scripts/Ability/Core/Ability.cs
using AbilitySystem.Platformer;
using System;
using UnityEngine;
using System.Threading;

namespace AbilitySystem
{
    /// <summary>
    /// 런타임 어빌리티 인스턴스 클래스
    /// </summary>
    public class Ability
    {
        // 어빌리티 데이터 참조 (수정됨: SkulData → PlatformerAbilityData)
        private PlatformerAbilityData data;

        // 쿨다운 관리
        private AbilityCooldown cooldown;

        // 상태 관리
        private AbilityState currentState;

        // 소유자 정보
        private GameObject owner;
        private AbilitySystem abilitySystem;

        // 실행 관련
        private CancellationTokenSource cancellationTokenSource;

        // 이벤트
        public event Action<Ability> OnAbilityStarted;
        public event Action<Ability> OnAbilityCompleted;
        public event Action<Ability> OnCooldownStarted;
        public event Action<Ability> OnCooldownCompleted;

        // 프로퍼티
        public string Id => data?.abilityId;
        public string Name => data?.abilityName;
        public PlatformerAbilityData Data => data;
        public AbilityState State => currentState;
        public GameObject Owner => owner;
        public bool IsReady => currentState == AbilityState.Ready && cooldown.CanUse();
        public bool IsOnCooldown => cooldown.IsOnCooldown;
        public float CooldownProgress => cooldown.Progress;
        public float CooldownRemaining => cooldown.RemainingTime;

        /// <summary>
        /// 어빌리티 초기화
        /// </summary>
        public void Initialize(PlatformerAbilityData abilityData, GameObject abilityOwner)
        {
            data = abilityData;
            owner = abilityOwner;
            abilitySystem = owner.GetComponent<AbilitySystem>();

            // 쿨다운 초기화
            cooldown = new AbilityCooldown();
            cooldown.Initialize(data.cooldownTime);

            // 쿨다운 이벤트 연결
            cooldown.OnCooldownStarted += () => OnCooldownStarted?.Invoke(this);
            cooldown.OnCooldownCompleted += () => {
                currentState = AbilityState.Ready;
                OnCooldownCompleted?.Invoke(this);
            };

            // 초기 상태
            currentState = AbilityState.Ready;
            cancellationTokenSource = new CancellationTokenSource();

            Debug.Log($"Ability initialized: {data.abilityName}");
        }

        /// <summary>
        /// 어빌리티 사용 가능 여부 체크
        /// </summary>
        public bool CanUse()
        {
            // 상태 체크
            if (currentState != AbilityState.Ready) return false;

            // 쿨다운 체크
            if (!cooldown.CanUse()) return false;

            // 코스트 체크 (마나)
            if (abilitySystem != null && !data.CanAfford(abilitySystem.CurrentMana))
            {
                Debug.Log($"Not enough mana for {data.abilityName}");
                return false;
            }

            // 데이터 유효성 체크
            if (!data.ValidateData()) return false;

            return true;
        }

        /// <summary>
        /// 어빌리티 실행
        /// </summary>
        public async Awaitable ExecuteAsync()
        {
            if (!CanUse())
            {
                Debug.Log($"Cannot use ability: {data.abilityName}");
                return;
            }

            try
            {
                // 상태 변경
                currentState = AbilityState.Active;
                OnAbilityStarted?.Invoke(this);

                Debug.Log($"Executing ability: {data.abilityName}");

                // 애니메이션 트리거
                if (!string.IsNullOrEmpty(data.animationTrigger))
                {
                    var animator = owner.GetComponent<Animator>();
                    if (animator != null)
                    {
                        animator.SetTrigger(data.animationTrigger);
                        animator.speed = data.animationSpeed;
                    }
                }

                // 사운드 재생
                if (data.soundEffect != null)
                {
                    AudioSource.PlayClipAtPoint(data.soundEffect, owner.transform.position);
                }

                // 어빌리티 타입별 실행
                await ExecuteByType();

                // 쿨다운 시작
                StartCooldown();

                // 완료 이벤트
                OnAbilityCompleted?.Invoke(this);
            }
            catch (OperationCanceledException)
            {
                Debug.Log($"Ability cancelled: {data.abilityName}");
                currentState = AbilityState.Ready;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error executing ability {data.abilityName}: {e.Message}");
                currentState = AbilityState.Ready;
            }
        }

        /// <summary>
        /// 어빌리티 타입별 실행
        /// </summary>
        private async Awaitable ExecuteByType()
        {
            switch (data.abilityType)
            {
                case PlatformerAbilityType.BasicAttack:
                    await ExecuteBasicAttack();
                    break;

                case PlatformerAbilityType.Skill:
                    await ExecuteSkill();
                    break;

                case PlatformerAbilityType.Ultimate:
                    await ExecuteUltimate();
                    break;

                case PlatformerAbilityType.Movement:
                    await ExecuteMovement();
                    break;

                case PlatformerAbilityType.Passive:
                    // 패시브는 즉시 적용
                    ExecutePassive();
                    break;

                default:
                    await Awaitable.NextFrameAsync(cancellationTokenSource.Token);
                    break;
            }
        }

        /// <summary>
        /// 기본 공격 실행
        /// </summary>
        private async Awaitable ExecuteBasicAttack()
        {
            // 히트박스 생성
            Vector2 attackPosition = GetAttackPosition();

            // 적 감지
            Collider2D[] hits = Physics2D.OverlapBoxAll(
                attackPosition,
                data.hitboxSize,
                0f,
                LayerMask.GetMask("Enemy")
            );

            // 데미지 처리
            foreach (var hit in hits)
            {
                ApplyDamageToTarget(hit.gameObject);
            }

            // 이펙트 생성
            if (data.effectPrefab != null)
            {
                GameObject effect = GameObject.Instantiate(
                    data.effectPrefab,
                    attackPosition,
                    Quaternion.identity
                );
                GameObject.Destroy(effect, 2f);
            }

            // 히트박스 지속시간
            await Awaitable.WaitForSecondsAsync(
                data.hitboxDuration,
                cancellationTokenSource.Token
            );
        }

        /// <summary>
        /// 스킬 실행
        /// </summary>
        private async Awaitable ExecuteSkill()
        {
            // 차징 스킬인 경우
            if (data.isChargeSkill)
            {
                float chargeTime = 0f;
                while (chargeTime < data.maxChargeTime)
                {
                    chargeTime += Time.deltaTime;

                    // 차징 중 취소 체크
                    if (data.cancelable && Input.GetKeyUp(KeyCode.X))
                    {
                        break;
                    }

                    await Awaitable.NextFrameAsync(cancellationTokenSource.Token);
                }

                // 차징 비율에 따른 데미지 증가
                float chargeRatio = chargeTime / data.maxChargeTime;
                // 데미지 적용시 chargeRatio 활용
            }

            // 다단 히트 처리
            if (data.isMultiHit)
            {
                for (int i = 0; i < data.hitCount; i++)
                {
                    await ExecuteHit();
                    await Awaitable.WaitForSecondsAsync(
                        0.1f,
                        cancellationTokenSource.Token
                    );
                }
            }
            else
            {
                await ExecuteHit();
            }
        }

        /// <summary>
        /// 궁극기 실행
        /// </summary>
        private async Awaitable ExecuteUltimate()
        {
            // 화면 효과
            Debug.Log($"Ultimate ability activated: {data.abilityName}");

            // 광역 데미지
            Collider2D[] hits = Physics2D.OverlapCircleAll(
                owner.transform.position,
                data.range,
                LayerMask.GetMask("Enemy")
            );

            foreach (var hit in hits)
            {
                ApplyDamageToTarget(hit.gameObject, 2f); // 궁극기는 2배 데미지
            }

            await Awaitable.WaitForSecondsAsync(0.5f, cancellationTokenSource.Token);
        }

        /// <summary>
        /// 이동 스킬 실행 (대시 등)
        /// </summary>
        private async Awaitable ExecuteMovement()
        {
            Rigidbody2D rb = owner.GetComponent<Rigidbody2D>();
            if (rb == null) return;

            // 대시 방향 결정
            float direction = owner.transform.localScale.x > 0 ? 1f : -1f;

            // 중력 임시 비활성화
            float originalGravity = rb.gravityScale;
            rb.gravityScale = 0f;

            // 대시 실행
            rb.linearVelocity = new Vector2(direction * data.range * 10f, 0);

            await Awaitable.WaitForSecondsAsync(0.2f, cancellationTokenSource.Token);

            // 중력 복구
            rb.gravityScale = originalGravity;
        }

        /// <summary>
        /// 패시브 실행
        /// </summary>
        private void ExecutePassive()
        {
            Debug.Log($"Passive ability applied: {data.abilityName}");
            // 패시브 효과는 BuffSystem에서 처리
        }

        /// <summary>
        /// 단일 히트 실행
        /// </summary>
        private async Awaitable ExecuteHit()
        {
            Vector2 attackPosition = GetAttackPosition();

            Collider2D[] hits = Physics2D.OverlapBoxAll(
                attackPosition,
                data.hitboxSize,
                0f,
                LayerMask.GetMask("Enemy")
            );

            foreach (var hit in hits)
            {
                ApplyDamageToTarget(hit.gameObject);
            }

            await Awaitable.WaitForSecondsAsync(
                data.hitboxDuration,
                cancellationTokenSource.Token
            );
        }

        /// <summary>
        /// 공격 위치 계산
        /// </summary>
        private Vector2 GetAttackPosition()
        {
            Vector2 position = owner.transform.position;

            switch (data.attackDirection)
            {
                case AttackDirection.Forward:
                    float direction = owner.transform.localScale.x > 0 ? 1f : -1f;
                    position += new Vector2(direction, 0) * data.range + data.hitboxOffset;
                    break;

                case AttackDirection.Up:
                    position += Vector2.up * data.range + data.hitboxOffset;
                    break;

                case AttackDirection.Down:
                    position += Vector2.down * data.range + data.hitboxOffset;
                    break;

                case AttackDirection.Air:
                    position += data.hitboxOffset;
                    break;
            }

            return position;
        }

        /// <summary>
        /// 데미지 적용
        /// </summary>
        private void ApplyDamageToTarget(GameObject target, float multiplier = 1f)
        {
            IAbilityTarget abilityTarget = target.GetComponent<IAbilityTarget>();
            if (abilityTarget != null && abilityTarget.IsTargetable)
            {
                // 기본 데미지 계산
                float damage = 10f * data.damageMultiplier * multiplier;

                // 컨트롤러에서 스컬 정보와 콤보 정보 가져오기
                var controller = owner.GetComponent<PlatformerAbilityController>();
                if (controller != null && controller.CurrentSkul != null)
                {
                    // 스컬의 공격력 적용
                    damage *= controller.CurrentSkul.attackPower / 10f;

                    // 기본 공격인 경우 콤보 배율 적용
                    if (data.abilityType == PlatformerAbilityType.BasicAttack)
                    {
                        // PlatformerAbilityController에서 현재 콤보 정보를 가져와야 하는데
                        // 현재 구조상 직접 접근이 어려우므로 데미지 계산은 컨트롤러에서 처리하도록 함
                        // 또는 Ability 생성시 콤보 정보를 전달받도록 수정 필요
                    }
                }

                abilityTarget.TakeDamage(damage, owner);

                // 넉백 적용
                if (data.knockbackPower > 0)
                {
                    Vector2 knockbackDir = (target.transform.position - owner.transform.position).normalized;
                    abilityTarget.ApplyKnockback(knockbackDir, data.knockbackPower);
                }

                Debug.Log($"Applied {damage:F1} damage to {target.name}");
            }
        }

        /// <summary>
        /// 쿨다운 업데이트 (매 프레임 호출)
        /// </summary>
        public void UpdateCooldown(float deltaTime)
        {
            cooldown.Update(deltaTime);
        }

        /// <summary>
        /// 쿨다운 시작
        /// </summary>
        private void StartCooldown()
        {
            currentState = AbilityState.Cooldown;
            cooldown.StartCooldown();
        }

        /// <summary>
        /// 어빌리티 강제 중단
        /// </summary>
        public void Cancel()
        {
            if (currentState == AbilityState.Active)
            {
                cancellationTokenSource?.Cancel();
                currentState = AbilityState.Ready;
                Debug.Log($"Ability cancelled: {data.abilityName}");
            }
        }

        /// <summary>
        /// 어빌리티 리셋
        /// </summary>
        public void Reset()
        {
            currentState = AbilityState.Ready;
            cooldown.Reset();
            cancellationTokenSource?.Cancel();
            cancellationTokenSource = new CancellationTokenSource();

            Debug.Log($"Ability reset: {data.abilityName}");
        }

        /// <summary>
        /// 쿨다운 감소
        /// </summary>
        public void ReduceCooldown(float amount)
        {
            cooldown.ReduceCooldown(amount);
        }

        /// <summary>
        /// 리소스 정리
        /// </summary>
        public void Dispose()
        {
            cancellationTokenSource?.Cancel();
            cancellationTokenSource?.Dispose();
        }
    }
}