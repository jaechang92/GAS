using GASPT.Core.Enums;
using UnityEngine;

namespace GASPT.StatusEffects
{
    /// <summary>
    /// StatusEffectVisual의 렌더링 관련 partial class
    /// VFX 관리, 색상 오버레이, 유틸리티, 디버그 메서드 포함
    /// </summary>
    public partial class StatusEffectVisual
    {
        // ====== VFX 관리 ======

        private void SpawnVFX(StatusEffectType effectType, int stackCount)
        {
            // 이미 VFX가 있으면 스킵
            if (activeVFX.ContainsKey(effectType))
            {
                return;
            }

            // VFX 프리팹 가져오기
            GameObject vfxPrefab = GetVfxPrefab(effectType);
            if (vfxPrefab == null)
            {
                Log($"VFX 프리팹 없음: {effectType}");
                return;
            }

            // VFX 생성
            Vector3 spawnPos = effectSpawnPoint != null ? effectSpawnPoint.position : transform.position;
            GameObject vfx = Instantiate(vfxPrefab, spawnPos, Quaternion.identity, transform);

            // 스케일 적용
            float baseScale = GetBaseScale(effectType);
            float finalScale = baseScale * (1f + (stackCount - 1) * stackScaleMultiplier);
            vfx.transform.localScale = Vector3.one * finalScale;

            // 색상 적용
            ApplyColorToVFX(vfx, effectType);

            // 추적 딕셔너리에 추가
            activeVFX[effectType] = vfx;

            Log($"VFX 생성: {effectType}, 스케일: {finalScale}");
        }

        private void DespawnVFX(StatusEffectType effectType)
        {
            if (!activeVFX.TryGetValue(effectType, out GameObject vfx))
            {
                return;
            }

            // VFX 제거
            if (vfx != null)
            {
                Destroy(vfx);
            }

            activeVFX.Remove(effectType);

            Log($"VFX 제거: {effectType}");
        }

        private void UpdateVFXIntensity(StatusEffectType effectType, int stackCount)
        {
            if (!activeVFX.TryGetValue(effectType, out GameObject vfx))
            {
                // VFX가 없으면 새로 생성
                SpawnVFX(effectType, stackCount);
                return;
            }

            if (vfx == null) return;

            // 스케일 업데이트
            float baseScale = GetBaseScale(effectType);
            float finalScale = baseScale * (1f + (stackCount - 1) * stackScaleMultiplier);
            vfx.transform.localScale = Vector3.one * finalScale;

            // 파티클 시스템 Emission Rate 증가 (선택적)
            var particles = vfx.GetComponentsInChildren<ParticleSystem>();
            foreach (var ps in particles)
            {
                var emission = ps.emission;
                var rateOverTime = emission.rateOverTime;
                // 스택당 20% 증가
                rateOverTime.constant = rateOverTime.constant * (1f + (stackCount - 1) * 0.2f);
            }

            Log($"VFX 강도 업데이트: {effectType}, 스택: {stackCount}, 스케일: {finalScale}");
        }

        private void ApplyColorToVFX(GameObject vfx, StatusEffectType effectType)
        {
            Color effectColor = GetOverlayColor(effectType);

            // ParticleSystem에 색상 적용
            var particles = vfx.GetComponentsInChildren<ParticleSystem>();
            foreach (var ps in particles)
            {
                var main = ps.main;
                main.startColor = effectColor;
            }

            // SpriteRenderer에 색상 적용
            var sprites = vfx.GetComponentsInChildren<SpriteRenderer>();
            foreach (var sr in sprites)
            {
                sr.color = effectColor;
            }
        }


        // ====== 색상 오버레이 관리 ======

        private void ApplyColorOverlay(StatusEffectType effectType)
        {
            if (targetRenderer == null) return;

            // 색상 오버레이 사용 여부 확인
            if (!UseColorOverlay(effectType)) return;

            // 오버레이 색상 가져오기
            Color overlayColor = GetOverlayColor(effectType);
            appliedOverlays[effectType] = overlayColor;

            // 블렌딩된 색상 계산
            UpdateTargetColor();
        }

        private void RemoveColorOverlay(StatusEffectType effectType)
        {
            if (!appliedOverlays.ContainsKey(effectType)) return;

            appliedOverlays.Remove(effectType);

            // 블렌딩된 색상 재계산
            UpdateTargetColor();
        }

        private void UpdateTargetColor()
        {
            if (targetRenderer == null) return;

            if (appliedOverlays.Count == 0)
            {
                // 모든 오버레이 제거됨 -> 원본 색상으로
                targetColor = originalColor;
            }
            else
            {
                // 모든 오버레이 색상 블렌딩
                Color blendedOverlay = Color.clear;

                foreach (var overlay in appliedOverlays.Values)
                {
                    // Additive 블렌딩
                    blendedOverlay.r += overlay.r * overlay.a;
                    blendedOverlay.g += overlay.g * overlay.a;
                    blendedOverlay.b += overlay.b * overlay.a;
                    blendedOverlay.a += overlay.a;
                }

                // 평균화
                if (appliedOverlays.Count > 0)
                {
                    blendedOverlay.r /= appliedOverlays.Count;
                    blendedOverlay.g /= appliedOverlays.Count;
                    blendedOverlay.b /= appliedOverlays.Count;
                    blendedOverlay.a = Mathf.Clamp01(blendedOverlay.a / appliedOverlays.Count);
                }

                // 원본 색상과 오버레이 블렌딩
                targetColor = Color.Lerp(originalColor, blendedOverlay, colorOverlayIntensity);
                targetColor.a = originalColor.a; // 알파는 유지
            }
        }

        private void UpdateColorTransition()
        {
            if (targetRenderer == null) return;

            // 부드러운 색상 전환
            if (targetRenderer.color != targetColor)
            {
                targetRenderer.color = Color.Lerp(
                    targetRenderer.color,
                    targetColor,
                    Time.deltaTime * colorTransitionSpeed
                );
            }
        }


        // ====== 유틸리티 ======

        private GameObject GetVfxPrefab(StatusEffectType effectType)
        {
            if (visualConfig != null)
            {
                return visualConfig.GetVfxPrefab(effectType);
            }
            return null;
        }

        private Color GetOverlayColor(StatusEffectType effectType)
        {
            if (visualConfig != null)
            {
                return visualConfig.GetOverlayColor(effectType);
            }

            // 기본 색상 (visualConfig 없을 때)
            return effectType switch
            {
                StatusEffectType.Burn => new Color(1f, 0.42f, 0f, 0.4f),
                StatusEffectType.Poison => new Color(0.6f, 0.2f, 0.8f, 0.4f),
                StatusEffectType.Bleed => new Color(0.86f, 0.08f, 0.24f, 0.4f),
                StatusEffectType.Slow => new Color(0f, 0.75f, 1f, 0.3f),
                StatusEffectType.Stun => new Color(1f, 0.84f, 0f, 0.4f),
                _ => Color.white
            };
        }

        private bool UseColorOverlay(StatusEffectType effectType)
        {
            if (visualConfig != null)
            {
                return visualConfig.UseColorOverlay(effectType);
            }
            return true;
        }

        private float GetBaseScale(StatusEffectType effectType)
        {
            if (visualConfig != null)
            {
                return visualConfig.GetBaseScale(effectType);
            }
            return 1f;
        }


        // ====== 디버그 ======

        private void Log(string message)
        {
            if (logDebugInfo)
            {
                Debug.Log($"[StatusEffectVisual] {gameObject.name}: {message}");
            }
        }


        // ====== 에디터 ======

        [ContextMenu("모든 효과 정리 (테스트)")]
        private void DebugCleanupAll()
        {
            CleanupAllEffects();
        }

        [ContextMenu("현재 상태 출력")]
        private void DebugPrintStatus()
        {
            Debug.Log($"[StatusEffectVisual] {gameObject.name} 상태:");
            Debug.Log($"  - 활성 VFX: {activeVFX.Count}개");
            foreach (var kvp in activeVFX)
            {
                Debug.Log($"    - {kvp.Key}: {(kvp.Value != null ? kvp.Value.name : "null")}");
            }
            Debug.Log($"  - 적용된 오버레이: {appliedOverlays.Count}개");
            foreach (var kvp in appliedOverlays)
            {
                Debug.Log($"    - {kvp.Key}: {kvp.Value}");
            }
            Debug.Log($"  - 원본 색상: {originalColor}");
            Debug.Log($"  - 목표 색상: {targetColor}");
        }
    }
}
