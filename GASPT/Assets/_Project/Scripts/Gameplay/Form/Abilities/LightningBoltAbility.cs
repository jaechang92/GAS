using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using GASPT.Core.Pooling;
using GASPT.Gameplay.Effects;
using GASPT.Gameplay.Enemy;

namespace GASPT.Form
{
    /// <summary>
    /// 번개 - 신규 스킬
    /// 마우스 방향으로 직선 관통 공격 (최대 3명)
    /// 관통할 때마다 데미지 감소
    /// 오브젝트 풀링 적용
    /// </summary>
    public class LightningBoltAbility : BaseProjectileAbility
    {
        // ====== Ability 정보 ======

        public override string AbilityName => "Lightning Bolt";
        public override float Cooldown => 4f;  // 4초 쿨다운


        // ====== 스킬 설정 ======

        private const int BaseDamage = 40;
        private const int DamageDecayPerHit = 10;  // 관통할 때마다 -10
        private const int MaxPierceCount = 3;      // 최대 3명
        private const float LightningRange = 15f;  // 최대 사거리
        private const float LightningWidth = 0.5f; // 번개 두께


        // ====== 실행 ======

        public override async Task ExecuteAsync(GameObject caster, CancellationToken token)
        {
            // 쿨다운 체크
            if (!CheckCooldown())
            {
                return;
            }

            // 쿨다운 시작 (중복 실행 방지)
            StartCooldown();

            // 마우스 방향 계산
            Vector2 direction = GetMouseDirection(caster);

            Debug.Log($"[LightningBolt] 번개 발사! 방향: {direction}");

            // 번개 발사
            FireLightning(caster.transform.position, direction);

            await Task.CompletedTask;
        }

        /// <summary>
        /// 번개 발사 (직선 관통)
        /// </summary>
        private void FireLightning(Vector3 startPos, Vector2 direction)
        {
            // 번개 시각 효과 생성
            CreateLightningEffect(startPos, direction);

            // RaycastAll로 방향의 모든 적 찾기
            RaycastHit2D[] hits = Physics2D.RaycastAll(startPos, direction, LightningRange);

            // 거리 순으로 정렬 (가까운 적부터)
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

            int hitCount = 0;
            int currentDamage = BaseDamage;

            foreach (var hit in hits)
            {
                // 최대 관통 수 체크
                if (hitCount >= MaxPierceCount)
                    break;

                // Enemy 컴포넌트 확인
                Enemy enemy = hit.collider.GetComponent<Enemy>();

                if (enemy != null)
                {
                    // 데미지 적용
                    enemy.TakeDamage(currentDamage);

                    // 히트 효과 생성
                    CreateHitEffect(hit.point);

                    hitCount++;

                    // 데미지 감소
                    currentDamage -= DamageDecayPerHit;
                    if (currentDamage < 0)
                        currentDamage = 0;

                    Debug.Log($"[LightningBolt] {enemy.name}에게 {currentDamage}의 번개 데미지! (관통 {hitCount}/{MaxPierceCount})");
                }
            }

            if (hitCount > 0)
            {
                Debug.Log($"[LightningBolt] 총 {hitCount}명의 적 관통!");
            }
            else
            {
                Debug.Log("[LightningBolt] 적 명중 실패");
            }
        }

        /// <summary>
        /// 번개 시각 효과 생성 (풀 사용)
        /// </summary>
        private void CreateLightningEffect(Vector3 startPos, Vector2 direction)
        {
            // 번개 라인 효과 생성
            var effect = PoolManager.Instance.Spawn<VisualEffect>(startPos, Quaternion.identity);

            if (effect != null)
            {
                // 노란색 번개
                SpriteRenderer sr = effect.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.color = new Color(1f, 1f, 0.3f);  // 밝은 노란색
                }

                // 번개 방향으로 회전
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                effect.transform.rotation = Quaternion.Euler(0, 0, angle);

                // 길쭉한 번개 모양 (크기: 가로 15, 세로 0.5)
                effect.transform.localScale = new Vector3(LightningRange, LightningWidth, 1f);

                Color startColor = new Color(1f, 1f, 0.3f, 1f); // 노란색
                Color endColor = new Color(1f, 1f, 0.3f, 0f);   // 투명

                // 빠르게 페이드 아웃
                effect.Play(
                    duration: 0.2f,
                    startScale: 1f,
                    endScale: 1f,
                    startColor: startColor,
                    endColor: endColor
                    );
            }
            else
            {
                Debug.LogWarning("[LightningBoltAbility] VisualEffect를 풀에서 가져올 수 없습니다!");
            }
        }

        /// <summary>
        /// 히트 효과 생성 (적 명중 시)
        /// </summary>
        private void CreateHitEffect(Vector3 position)
        {
            var effect = PoolManager.Instance.Spawn<VisualEffect>(position, Quaternion.identity);

            if (effect != null)
            {
                // 하얀색 번개 충격
                SpriteRenderer sr = effect.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.color = Color.white;
                }

                Color startColor = Color.white;
                Color endColor = new Color(1f, 1f, 1f, 0f); // 투명

                // 작은 폭발 효과
                effect.Play(
                    duration: 0.3f,
                    startScale: 0.3f,
                    endScale: 1f,
                    startColor: startColor,
                    endColor: endColor
                );
            }
        }
    }
}
