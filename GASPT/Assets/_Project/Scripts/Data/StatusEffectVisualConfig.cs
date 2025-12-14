using System;
using System.Collections.Generic;
using GASPT.Core.Enums;
using UnityEngine;

namespace GASPT.Data
{
    /// <summary>
    /// 상태 효과 시각화 설정 ScriptableObject
    /// 효과별 VFX 프리팹, 오버레이 색상 등을 정의
    /// </summary>
    [CreateAssetMenu(fileName = "StatusEffectVisualConfig", menuName = "GASPT/StatusEffects/VisualConfig")]
    public class StatusEffectVisualConfig : ScriptableObject
    {
        // ====== 효과별 시각 설정 ======

        [Serializable]
        public class EffectVisualEntry
        {
            [Tooltip("상태 효과 타입")]
            public StatusEffectType effectType;

            [Tooltip("VFX 프리팹 (파티클 시스템)")]
            public GameObject vfxPrefab;

            [Tooltip("색상 오버레이 색상")]
            public Color overlayColor = Color.white;

            [Tooltip("색상 오버레이 사용 여부")]
            public bool useColorOverlay = true;

            [Tooltip("VFX 기본 스케일")]
            [Range(0.1f, 3f)]
            public float baseScale = 1f;
        }

        [Header("효과별 시각 설정")]
        [SerializeField]
        private List<EffectVisualEntry> visualEntries = new List<EffectVisualEntry>();

        [Header("기본 설정")]
        [Tooltip("기본 VFX (개별 설정 없을 시 사용)")]
        [SerializeField]
        private GameObject defaultVfxPrefab;

        [Tooltip("기본 오버레이 색상")]
        [SerializeField]
        private Color defaultOverlayColor = new Color(1f, 1f, 1f, 0.3f);


        // ====== 캐시 ======

        private Dictionary<StatusEffectType, EffectVisualEntry> entryCache;


        // ====== 초기화 ======

        private void OnEnable()
        {
            BuildCache();
        }

        private void OnValidate()
        {
            BuildCache();
        }

        private void BuildCache()
        {
            entryCache = new Dictionary<StatusEffectType, EffectVisualEntry>();

            foreach (var entry in visualEntries)
            {
                if (!entryCache.ContainsKey(entry.effectType))
                {
                    entryCache[entry.effectType] = entry;
                }
                else
                {
                    Debug.LogWarning($"[StatusEffectVisualConfig] 중복된 효과 타입: {entry.effectType}");
                }
            }
        }


        // ====== 조회 메서드 ======

        /// <summary>
        /// 효과 타입에 해당하는 시각 설정 가져오기
        /// </summary>
        /// <param name="effectType">효과 타입</param>
        /// <returns>EffectVisualEntry 또는 null</returns>
        public EffectVisualEntry GetVisualEntry(StatusEffectType effectType)
        {
            if (entryCache == null)
            {
                BuildCache();
            }

            if (entryCache.TryGetValue(effectType, out var entry))
            {
                return entry;
            }

            return null;
        }

        /// <summary>
        /// 효과 타입에 해당하는 VFX 프리팹 가져오기
        /// </summary>
        /// <param name="effectType">효과 타입</param>
        /// <returns>VFX 프리팹 또는 기본 프리팹</returns>
        public GameObject GetVfxPrefab(StatusEffectType effectType)
        {
            var entry = GetVisualEntry(effectType);
            return entry?.vfxPrefab ?? defaultVfxPrefab;
        }

        /// <summary>
        /// 효과 타입에 해당하는 오버레이 색상 가져오기
        /// </summary>
        /// <param name="effectType">효과 타입</param>
        /// <returns>오버레이 색상</returns>
        public Color GetOverlayColor(StatusEffectType effectType)
        {
            var entry = GetVisualEntry(effectType);
            return entry?.overlayColor ?? defaultOverlayColor;
        }

        /// <summary>
        /// 효과 타입이 색상 오버레이를 사용하는지 확인
        /// </summary>
        /// <param name="effectType">효과 타입</param>
        /// <returns>색상 오버레이 사용 여부</returns>
        public bool UseColorOverlay(StatusEffectType effectType)
        {
            var entry = GetVisualEntry(effectType);
            return entry?.useColorOverlay ?? true;
        }

        /// <summary>
        /// 효과 타입의 기본 스케일 가져오기
        /// </summary>
        /// <param name="effectType">효과 타입</param>
        /// <returns>기본 스케일</returns>
        public float GetBaseScale(StatusEffectType effectType)
        {
            var entry = GetVisualEntry(effectType);
            return entry?.baseScale ?? 1f;
        }


        // ====== 기본 프리팹 접근자 ======

        public GameObject DefaultVfxPrefab => defaultVfxPrefab;
        public Color DefaultOverlayColor => defaultOverlayColor;


        // ====== 에디터 유틸리티 ======

#if UNITY_EDITOR
        [ContextMenu("기본 효과 엔트리 생성")]
        private void CreateDefaultEntries()
        {
            visualEntries.Clear();

            // 화상 (Burn)
            visualEntries.Add(new EffectVisualEntry
            {
                effectType = StatusEffectType.Burn,
                overlayColor = new Color(1f, 0.42f, 0f, 0.4f), // #FF6B00
                useColorOverlay = true,
                baseScale = 1f
            });

            // 독 (Poison)
            visualEntries.Add(new EffectVisualEntry
            {
                effectType = StatusEffectType.Poison,
                overlayColor = new Color(0.6f, 0.2f, 0.8f, 0.4f), // #9932CC
                useColorOverlay = true,
                baseScale = 1f
            });

            // 출혈 (Bleed)
            visualEntries.Add(new EffectVisualEntry
            {
                effectType = StatusEffectType.Bleed,
                overlayColor = new Color(0.86f, 0.08f, 0.24f, 0.4f), // #DC143C
                useColorOverlay = true,
                baseScale = 1f
            });

            // 감속 (Slow)
            visualEntries.Add(new EffectVisualEntry
            {
                effectType = StatusEffectType.Slow,
                overlayColor = new Color(0f, 0.75f, 1f, 0.3f), // #00BFFF
                useColorOverlay = true,
                baseScale = 1f
            });

            // 기절 (Stun)
            visualEntries.Add(new EffectVisualEntry
            {
                effectType = StatusEffectType.Stun,
                overlayColor = new Color(1f, 0.84f, 0f, 0.4f), // #FFD700
                useColorOverlay = true,
                baseScale = 1f
            });

            // 공격력 증가 (AttackUp) - 버프
            visualEntries.Add(new EffectVisualEntry
            {
                effectType = StatusEffectType.AttackUp,
                overlayColor = new Color(1f, 0.5f, 0.5f, 0.2f), // 연한 빨강
                useColorOverlay = true,
                baseScale = 0.8f
            });

            // 방어력 증가 (DefenseUp) - 버프
            visualEntries.Add(new EffectVisualEntry
            {
                effectType = StatusEffectType.DefenseUp,
                overlayColor = new Color(0.5f, 0.5f, 1f, 0.2f), // 연한 파랑
                useColorOverlay = true,
                baseScale = 0.8f
            });

            // 속도 증가 (SpeedUp) - 버프
            visualEntries.Add(new EffectVisualEntry
            {
                effectType = StatusEffectType.SpeedUp,
                overlayColor = new Color(0.5f, 1f, 0.5f, 0.2f), // 연한 초록
                useColorOverlay = true,
                baseScale = 0.8f
            });

            // 무적 (Invincible)
            visualEntries.Add(new EffectVisualEntry
            {
                effectType = StatusEffectType.Invincible,
                overlayColor = new Color(1f, 1f, 0.8f, 0.3f), // 연한 노랑
                useColorOverlay = true,
                baseScale = 1.2f
            });

            // 재생 (Regeneration)
            visualEntries.Add(new EffectVisualEntry
            {
                effectType = StatusEffectType.Regeneration,
                overlayColor = new Color(0.3f, 1f, 0.3f, 0.2f), // 초록
                useColorOverlay = true,
                baseScale = 0.8f
            });

            Debug.Log("[StatusEffectVisualConfig] 기본 효과 엔트리 10개 생성 완료");

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
#endif
    }
}
