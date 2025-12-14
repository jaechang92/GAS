using System.Collections.Generic;
using GASPT.Core.Enums;
using GASPT.Data;
using UnityEngine;

namespace GASPT.StatusEffects
{
    /// <summary>
    /// 상태 효과 시각화 컴포넌트
    /// Entity(Player/Enemy)에 부착하여 상태 효과 시 VFX와 색상 오버레이 표시
    /// </summary>
    public class StatusEffectVisual : MonoBehaviour
    {
        // ====== 설정 ======

        [Header("참조")]
        [Tooltip("색상 오버레이를 적용할 SpriteRenderer")]
        [SerializeField] private SpriteRenderer targetRenderer;

        [Tooltip("VFX 스폰 위치 (null이면 transform 사용)")]
        [SerializeField] private Transform effectSpawnPoint;

        [Tooltip("시각 효과 설정 에셋")]
        [SerializeField] private StatusEffectVisualConfig visualConfig;

        [Header("색상 오버레이 설정")]
        [Tooltip("색상 오버레이 강도 (0~1)")]
        [Range(0f, 1f)]
        [SerializeField] private float colorOverlayIntensity = 0.4f;

        [Tooltip("색상 전환 속도")]
        [SerializeField] private float colorTransitionSpeed = 5f;

        [Header("VFX 설정")]
        [Tooltip("스택당 스케일 증가 비율")]
        [Range(0f, 0.5f)]
        [SerializeField] private float stackScaleMultiplier = 0.15f;

        [Header("자동 설정")]
        [Tooltip("SpriteRenderer를 자동으로 찾기")]
        [SerializeField] private bool autoFindRenderer = true;

        [Header("디버그")]
        [SerializeField] private bool logDebugInfo = false;


        // ====== 상태 ======

        // 활성 VFX 추적: 효과 타입 -> VFX GameObject
        private Dictionary<StatusEffectType, GameObject> activeVFX = new Dictionary<StatusEffectType, GameObject>();

        // 적용된 오버레이 색상: 효과 타입 -> 색상
        private Dictionary<StatusEffectType, Color> appliedOverlays = new Dictionary<StatusEffectType, Color>();

        // 효과 스택 수: 효과 타입 -> 스택 수
        private Dictionary<StatusEffectType, int> effectStacks = new Dictionary<StatusEffectType, int>();

        // 원본 색상
        private Color originalColor = Color.white;

        // 목표 색상 (부드러운 전환용)
        private Color targetColor = Color.white;

        // 초기화 완료 여부
        private bool isInitialized = false;


        // ====== Unity 생명주기 ======

        private void Awake()
        {
            Initialize();
        }

        private void OnEnable()
        {
            SubscribeToEvents();
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
            CleanupAllEffects();
        }

        private void Update()
        {
            UpdateColorTransition();
        }


        // ====== 초기화 ======

        private void Initialize()
        {
            if (isInitialized) return;

            // SpriteRenderer 자동 탐색
            if (autoFindRenderer && targetRenderer == null)
            {
                targetRenderer = GetComponentInChildren<SpriteRenderer>();
            }

            // 원본 색상 저장
            if (targetRenderer != null)
            {
                originalColor = targetRenderer.color;
                targetColor = originalColor;
            }

            // 스폰 포인트 기본값
            if (effectSpawnPoint == null)
            {
                effectSpawnPoint = transform;
            }

            // VisualConfig 로드 (Resources에서)
            if (visualConfig == null)
            {
                visualConfig = Resources.Load<StatusEffectVisualConfig>("Data/StatusEffectVisualConfig");
            }

            isInitialized = true;

            Log("초기화 완료");
        }


        // ====== 이벤트 구독 ======

        private void SubscribeToEvents()
        {
            if (StatusEffectManager.Instance == null)
            {
                Log("StatusEffectManager가 없습니다. 이벤트 구독 실패.");
                return;
            }

            StatusEffectManager.Instance.OnEffectApplied += HandleEffectApplied;
            StatusEffectManager.Instance.OnEffectRemoved += HandleEffectRemoved;
            StatusEffectManager.Instance.OnEffectStacked += HandleEffectStacked;

            Log("StatusEffectManager 이벤트 구독 완료");
        }

        private void UnsubscribeFromEvents()
        {
            if (StatusEffectManager.Instance == null) return;

            StatusEffectManager.Instance.OnEffectApplied -= HandleEffectApplied;
            StatusEffectManager.Instance.OnEffectRemoved -= HandleEffectRemoved;
            StatusEffectManager.Instance.OnEffectStacked -= HandleEffectStacked;
        }


        // ====== 이벤트 핸들러 ======

        private void HandleEffectApplied(GameObject target, StatusEffect effect)
        {
            // 자신에게 적용된 효과만 처리
            if (target != gameObject) return;

            Log($"효과 적용: {effect.EffectType}");

            // 스택 초기화
            effectStacks[effect.EffectType] = 1;

            // VFX 스폰
            SpawnVFX(effect.EffectType, 1);

            // 색상 오버레이 적용
            ApplyColorOverlay(effect.EffectType);
        }

        private void HandleEffectRemoved(GameObject target, StatusEffect effect)
        {
            // 자신에게 적용된 효과만 처리
            if (target != gameObject) return;

            Log($"효과 제거: {effect.EffectType}");

            // 스택 제거
            effectStacks.Remove(effect.EffectType);

            // VFX 제거
            DespawnVFX(effect.EffectType);

            // 색상 오버레이 제거
            RemoveColorOverlay(effect.EffectType);
        }

        private void HandleEffectStacked(GameObject target, StatusEffect effect, int newStackCount)
        {
            // 자신에게 적용된 효과만 처리
            if (target != gameObject) return;

            Log($"효과 중첩: {effect.EffectType}, 스택: {newStackCount}");

            // 스택 업데이트
            effectStacks[effect.EffectType] = newStackCount;

            // VFX 강도 업데이트
            UpdateVFXIntensity(effect.EffectType, newStackCount);
        }


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


        // ====== 정리 ======

        private void CleanupAllEffects()
        {
            // 모든 VFX 제거
            foreach (var vfx in activeVFX.Values)
            {
                if (vfx != null)
                {
                    Destroy(vfx);
                }
            }
            activeVFX.Clear();

            // 색상 오버레이 정리
            appliedOverlays.Clear();
            effectStacks.Clear();

            // 원본 색상 복원 (즉시)
            if (targetRenderer != null)
            {
                targetRenderer.color = originalColor;
                targetColor = originalColor;
            }

            Log("모든 효과 정리 완료");
        }


        // ====== 공개 메서드 ======

        /// <summary>
        /// 원본 색상 수동 설정 (폼 변경 등에서 사용)
        /// </summary>
        public void SetOriginalColor(Color color)
        {
            originalColor = color;
            UpdateTargetColor();
        }

        /// <summary>
        /// VisualConfig 수동 설정
        /// </summary>
        public void SetVisualConfig(StatusEffectVisualConfig config)
        {
            visualConfig = config;
        }

        /// <summary>
        /// 활성 효과 수 반환
        /// </summary>
        public int ActiveEffectCount => activeVFX.Count;

        /// <summary>
        /// 특정 효과의 VFX가 활성 상태인지 확인
        /// </summary>
        public bool HasActiveVFX(StatusEffectType effectType)
        {
            return activeVFX.ContainsKey(effectType);
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
