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
    public partial class StatusEffectVisual : MonoBehaviour
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
    }
}
