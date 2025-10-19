using UnityEngine;
using System.Threading;
using GAS.Core;
using Combat.Core;
using Combat.Data;

namespace Combat.Abilities
{
    /// <summary>
    /// 콤보 공격 어빌리티 실행기
    /// GAS Ability를 확장하여 ComboAbilityData 기반 공격 실행
    /// </summary>
    public class ComboAbility : Ability
    {
        /// <summary>
        /// ComboAbilityData 프로퍼티 (캐스팅)
        /// </summary>
        private ComboAbilityData ComboData => Data as ComboAbilityData;

        /// <summary>
        /// 액티브 어빌리티 실행 (GAS 오버라이드)
        /// </summary>
        protected override async Awaitable ExecuteActiveAbility(CancellationToken cancellationToken)
        {
            if (ComboData == null || owner == null)
            {
                Debug.LogError("[ComboAbility] 실행 실패: ComboData 또는 owner가 null");
                return;
            }

            LogDebug($"콤보 {ComboData.comboIndex + 1}단 공격 실행");

            // 1. GAS 기본 기능 (자동)
            TriggerAnimation();  // AnimationTrigger 자동 실행
            PlaySound();         // SoundEffect 자동 재생
            SpawnEffect();       // EffectPrefab 자동 생성

            // 2. Hitbox 생성 딜레이
            if (ComboData.hitboxSpawnDelay > 0f)
            {
                await Awaitable.WaitForSecondsAsync(ComboData.hitboxSpawnDelay, cancellationToken);
            }

            // 3. Hitbox 생성 및 데미지 적용
            CreateHitbox();

            // 4. Hitbox 지속 시간 대기
            await Awaitable.WaitForSecondsAsync(ComboData.hitboxDuration, cancellationToken);

            LogDebug($"콤보 {ComboData.comboIndex + 1}단 공격 완료");
        }

        /// <summary>
        /// 히트박스 생성 및 데미지 적용
        /// </summary>
        private void CreateHitbox()
        {
            if (ComboData == null || owner == null) return;

            // 플레이어 위치 및 방향
            Vector3 ownerPosition = owner.transform.position;
            float facingDirection = Mathf.Sign(owner.transform.localScale.x);

            // 히트박스 중심 위치 계산 (소유자 기준)
            Vector2 hitboxOffset = new Vector2(
                ComboData.hitboxOffset.x * facingDirection,
                ComboData.hitboxOffset.y
            );
            Vector3 hitboxCenter = ownerPosition + (Vector3)hitboxOffset;

            // 데미지 데이터 생성
            var damageData = DamageData.CreateWithKnockback(
                ComboData.GetFinalDamage(),
                ComboData.DamageType,
                owner,
                ComboData.knockbackForce * facingDirection * Vector2.right
            );

            // 스턴 시간 설정
            damageData.stunDuration = ComboData.GetStunDuration();

            // 박스 범위 데미지 적용
            var hitTargets = DamageSystem.ApplyBoxDamage(
                hitboxCenter,
                ComboData.hitboxSize,
                0f, // 회전 없음
                damageData,
                ComboData.targetLayers
            );

            LogDebug($"히트박스 생성: {hitTargets.Count}개 타격, 데미지: {ComboData.GetFinalDamage()}");

            // 디버그 시각화
            if (ComboData.showGizmos)
            {
                DrawHitboxDebug(hitboxCenter, ComboData.hitboxSize, ComboData.hitboxDuration);
            }
        }

        /// <summary>
        /// 애니메이션 트리거 (GAS 기본 기능)
        /// </summary>
        private void TriggerAnimation()
        {
            if (ComboData == null || owner == null) return;

            if (!string.IsNullOrEmpty(ComboData.AnimationTrigger))
            {
                var animator = owner.GetComponent<Animator>();
                if (animator != null)
                {
                    animator.SetTrigger(ComboData.AnimationTrigger);
                    LogDebug($"애니메이션 트리거: {ComboData.AnimationTrigger}");
                }
            }
        }

        /// <summary>
        /// 사운드 재생 (GAS 기본 기능)
        /// </summary>
        private void PlaySound()
        {
            if (ComboData == null || owner == null) return;

            if (ComboData.SoundEffect != null)
            {
                AudioSource.PlayClipAtPoint(ComboData.SoundEffect, owner.transform.position);
                LogDebug($"사운드 재생: {ComboData.SoundEffect.name}");
            }
        }

        /// <summary>
        /// 이펙트 생성 (GAS 기본 기능)
        /// </summary>
        private void SpawnEffect()
        {
            if (ComboData == null || owner == null) return;

            if (ComboData.EffectPrefab != null)
            {
                Vector3 effectPosition = owner.transform.position + (Vector3)ComboData.hitboxOffset;
                var effect = Object.Instantiate(
                    ComboData.EffectPrefab,
                    effectPosition,
                    owner.transform.rotation
                );
                Object.Destroy(effect, 2f); // 2초 후 자동 파괴
                LogDebug($"이펙트 생성: {ComboData.EffectPrefab.name}");
            }
        }

        /// <summary>
        /// 히트박스 디버그 시각화 (비동기)
        /// </summary>
        private async void DrawHitboxDebug(Vector3 center, Vector2 size, float duration)
        {
            if (ComboData == null) return;

            var go = new GameObject($"Hitbox_Debug_Combo{ComboData.comboIndex}");
            go.transform.position = center;

            // SpriteRenderer로 시각화
            var sr = go.AddComponent<SpriteRenderer>();

            // 콤보 단계별로 색상 변경
            Color[] comboColors = {
                new Color(1f, 0f, 0f, 0.3f),    // 1단: 빨간색
                new Color(1f, 0.5f, 0f, 0.3f),  // 2단: 주황색
                new Color(1f, 1f, 0f, 0.3f)     // 3단: 노란색
            };

            int colorIndex = Mathf.Clamp(ComboData.comboIndex, 0, comboColors.Length - 1);
            sr.color = comboColors[colorIndex];

            // 간단한 사각형 스프라이트
            var texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();

            var sprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
            sr.sprite = sprite;
            go.transform.localScale = new Vector3(size.x, size.y, 1f);

            // 일정 시간 후 파괴
            await Awaitable.WaitForSecondsAsync(duration);
            if (go != null)
            {
                Object.Destroy(texture);
                Object.Destroy(sprite);
                Object.Destroy(go);
            }
        }

        /// <summary>
        /// 디버그 로그 (조건부)
        /// </summary>
        private void LogDebug(string message)
        {
            if (ComboData != null && ComboData.debugLog)
            {
                Debug.Log($"[ComboAbility] {message}");
            }
        }

        /// <summary>
        /// 어빌리티 실행 가능 여부 확인 (오버라이드)
        /// </summary>
        public override bool CanExecute()
        {
            // 기본 조건 확인 (쿨다운, 리소스 등)
            if (!base.CanExecute()) return false;

            // 추가 조건: Owner가 존재하는지 확인
            if (owner == null)
            {
                LogDebug("실행 불가: Owner가 null");
                return false;
            }

            return true;
        }
    }
}
