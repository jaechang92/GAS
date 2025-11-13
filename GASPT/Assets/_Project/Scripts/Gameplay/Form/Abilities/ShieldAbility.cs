using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using GASPT.Core.Pooling;
using GASPT.Gameplay.Effects;
using GASPT.StatusEffects;

namespace GASPT.Form
{
    /// <summary>
    /// 보호막 - 신규 스킬
    /// 자신에게 무적 버프 적용 (3초간)
    /// 시각적 보호막 효과 표시
    /// 오브젝트 풀링 및 StatusEffect 시스템 사용
    /// </summary>
    public class ShieldAbility : IAbility
    {
        public string AbilityName => "Shield";
        public float Cooldown => 8f;  // 8초 쿨다운

        private float lastUsedTime;

        // 스킬 설정
        private const float ShieldDuration = 3f;  // 3초간 무적

        public async Task ExecuteAsync(GameObject caster, CancellationToken token)
        {
            // 쿨다운 체크
            if (Time.time - lastUsedTime < Cooldown)
            {
                Debug.Log("[Shield] 쿨다운 중...");
                return;
            }

            // 쿨다운 즉시 시작 (중복 실행 방지)
            lastUsedTime = Time.time;

            Debug.Log($"[Shield] 보호막 시전! (지속시간: {ShieldDuration}초)");

            // 보호막 효과 적용
            ApplyShieldEffect(caster);

            // 시각 효과 생성
            await CreateShieldVisualAsync(caster, token);
        }

        /// <summary>
        /// 보호막 효과 적용 (StatusEffect)
        /// </summary>
        private void ApplyShieldEffect(GameObject target)
        {
            if (StatusEffectManager.HasInstance)
            {
                // TODO: Invincible StatusEffectData 생성 및 적용
                // 현재는 로그만 출력
                Debug.Log($"[Shield] {target.name}에게 무적 효과 적용 (지속시간: {ShieldDuration}초)");

                // TODO: 실제 무적 StatusEffect 적용
                // StatusEffectData invincibleEffect = GameResourceManager.Instance.LoadScriptableObject<StatusEffectData>(...);
                // StatusEffectManager.Instance.ApplyEffect(target, invincibleEffect);
            }
            else
            {
                Debug.LogWarning("[Shield] StatusEffectManager가 존재하지 않습니다.");
            }
        }

        /// <summary>
        /// 보호막 시각 효과 생성 (Awaitable 기반)
        /// </summary>
        private async Task CreateShieldVisualAsync(GameObject caster, CancellationToken token)
        {
            // 풀에서 VisualEffect 가져오기
            var shieldEffect = PoolManager.Instance.Spawn<VisualEffect>(
                caster.transform.position,
                Quaternion.identity
            );

            if (shieldEffect == null)
            {
                Debug.LogWarning("[ShieldAbility] VisualEffect를 풀에서 가져올 수 없습니다!");
                return;
            }

            // 보호막을 캐스터의 자식으로 설정 (따라다니도록)
            shieldEffect.transform.SetParent(caster.transform);
            shieldEffect.transform.localPosition = Vector3.zero;

            // 청록색 반투명 보호막
            SpriteRenderer sr = shieldEffect.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.color = new Color(0.3f, 0.8f, 1f, 0.5f);  // 반투명 청록색
            }

            // 보호막 크기 (캐릭터를 감싸는 크기)
            shieldEffect.transform.localScale = new Vector3(2f, 2f, 1f);

            try
            {
                // ShieldDuration 동안 유지
                float elapsed = 0f;

                while (elapsed < ShieldDuration)
                {
                    // 취소 확인
                    if (token.IsCancellationRequested)
                    {
                        Debug.Log("[Shield] 보호막 효과 취소됨");
                        break;
                    }

                    // 다음 프레임까지 대기
                    await Awaitable.NextFrameAsync(token);
                    elapsed += Time.deltaTime;

                    // 보호막 펄스 애니메이션 (선택사항)
                    if (sr != null)
                    {
                        float alpha = 0.3f + Mathf.Sin(Time.time * 5f) * 0.2f;
                        sr.color = new Color(0.3f, 0.8f, 1f, alpha);
                    }
                }

                Debug.Log("[Shield] 보호막 지속시간 종료");
            }
            catch (System.OperationCanceledException)
            {
                Debug.Log("[Shield] 보호막 효과 취소됨 (OperationCanceledException)");
            }
            finally
            {
                // 보호막 효과 제거
                if (shieldEffect != null)
                {
                    shieldEffect.transform.SetParent(null);
                    PoolManager.Instance.Despawn(shieldEffect);
                }
            }
        }
    }
}
