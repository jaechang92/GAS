using System;
using System.Collections.Generic;
using System.Linq;
using Core.Enums;
using GASPT.Data;
using UnityEngine;

namespace GASPT.StatusEffects
{
    /// <summary>
    /// 상태 이상 효과 관리 싱글톤
    /// 모든 대상의 StatusEffect를 중앙에서 관리하고 업데이트
    /// </summary>
    public class StatusEffectManager : SingletonManager<StatusEffectManager>
    {
        // ====== 효과 관리 ======

        /// <summary>
        /// 대상별 활성 효과 목록
        /// Key: GameObject (대상), Value: List of StatusEffect
        /// </summary>
        private Dictionary<GameObject, List<StatusEffect>> activeEffects = new Dictionary<GameObject, List<StatusEffect>>();


        // ====== 이벤트 ======

        /// <summary>
        /// 효과 적용 시 이벤트
        /// 매개변수: (대상, StatusEffect)
        /// </summary>
        public event Action<GameObject, StatusEffect> OnEffectApplied;

        /// <summary>
        /// 효과 제거 시 이벤트
        /// 매개변수: (대상, StatusEffect)
        /// </summary>
        public event Action<GameObject, StatusEffect> OnEffectRemoved;


        // ====== Unity 생명주기 ======

        protected override void OnAwake()
        {
            Debug.Log("[StatusEffectManager] 초기화 완료");
        }

        private void Update()
        {
            UpdateAllEffects(Time.deltaTime);
        }


        // ====== 효과 적용 ======

        /// <summary>
        /// 대상에게 효과 적용
        /// </summary>
        /// <param name="target">대상 GameObject</param>
        /// <param name="effectData">적용할 효과 데이터</param>
        /// <returns>생성된 StatusEffect 인스턴스</returns>
        public StatusEffect ApplyEffect(GameObject target, StatusEffectData effectData)
        {
            if (target == null || effectData == null)
            {
                Debug.LogWarning("[StatusEffectManager] ApplyEffect: target 또는 effectData가 null입니다.");
                return null;
            }

            // 대상의 효과 목록 가져오기 (없으면 생성)
            if (!activeEffects.ContainsKey(target))
            {
                activeEffects[target] = new List<StatusEffect>();
            }

            List<StatusEffect> targetEffects = activeEffects[target];

            // 같은 타입의 효과가 이미 있는지 확인
            StatusEffect existingEffect = targetEffects.Find(e => e.EffectType == effectData.effectType);

            if (existingEffect != null)
            {
                // 중첩 처리
                existingEffect.Stack();
                Debug.Log($"[StatusEffectManager] {effectData.displayName} 효과 중첩 - 대상: {target.name}");

                // 중첩 시에도 이벤트 발생 (UI 업데이트를 위해)
                OnEffectApplied?.Invoke(target, existingEffect);

                return existingEffect;
            }

            // 새 효과 생성 및 적용
            StatusEffect newEffect = effectData.CreateInstance();
            newEffect.Apply(target);

            // 효과 목록에 추가
            targetEffects.Add(newEffect);

            Debug.Log($"[StatusEffectManager] {effectData.displayName} 효과 적용 - 대상: {target.name}");

            OnEffectApplied?.Invoke(target, newEffect);

            return newEffect;
        }


        // ====== 효과 제거 ======

        /// <summary>
        /// 특정 효과 제거
        /// </summary>
        /// <param name="target">대상</param>
        /// <param name="effectType">제거할 효과 타입</param>
        public void RemoveEffect(GameObject target, StatusEffectType effectType)
        {
            if (target == null || !activeEffects.ContainsKey(target))
            {
                return;
            }

            List<StatusEffect> targetEffects = activeEffects[target];
            StatusEffect effect = targetEffects.Find(e => e.EffectType == effectType);

            if (effect != null)
            {
                effect.Remove();
                targetEffects.Remove(effect);

                Debug.Log($"[StatusEffectManager] {effect.DisplayName} 효과 제거 - 대상: {target.name}");

                OnEffectRemoved?.Invoke(target, effect);

                // 목록이 비었으면 Dictionary에서 제거
                if (targetEffects.Count == 0)
                {
                    activeEffects.Remove(target);
                }
            }
        }

        /// <summary>
        /// 대상의 모든 효과 제거
        /// </summary>
        /// <param name="target">대상</param>
        public void RemoveAllEffects(GameObject target)
        {
            if (target == null || !activeEffects.ContainsKey(target))
            {
                return;
            }

            List<StatusEffect> targetEffects = new List<StatusEffect>(activeEffects[target]);

            foreach (StatusEffect effect in targetEffects)
            {
                effect.Remove();
                OnEffectRemoved?.Invoke(target, effect);
            }

            activeEffects.Remove(target);

            Debug.Log($"[StatusEffectManager] 모든 효과 제거 - 대상: {target.name}");
        }


        // ====== 업데이트 ======

        /// <summary>
        /// 모든 활성 효과 업데이트
        /// </summary>
        private void UpdateAllEffects(float deltaTime)
        {
            // 제거할 대상 목록
            List<GameObject> targetsToRemove = new List<GameObject>();

            foreach (var kvp in activeEffects)
            {
                GameObject target = kvp.Key;
                List<StatusEffect> effects = kvp.Value;

                // 제거할 효과 목록
                List<StatusEffect> effectsToRemove = new List<StatusEffect>();

                // 각 효과 업데이트
                foreach (StatusEffect effect in effects)
                {
                    bool shouldContinue = effect.Update(deltaTime);

                    if (!shouldContinue)
                    {
                        effectsToRemove.Add(effect);
                    }
                }

                // 만료된 효과 제거
                foreach (StatusEffect effect in effectsToRemove)
                {
                    effects.Remove(effect);
                    OnEffectRemoved?.Invoke(target, effect);
                }

                // 효과가 하나도 없으면 대상 제거 예약
                if (effects.Count == 0)
                {
                    targetsToRemove.Add(target);
                }
            }

            // 효과가 없는 대상 제거
            foreach (GameObject target in targetsToRemove)
            {
                activeEffects.Remove(target);
            }
        }


        // ====== 조회 메서드 ======

        /// <summary>
        /// 대상의 모든 활성 효과 가져오기
        /// </summary>
        /// <param name="target">대상</param>
        /// <returns>활성 효과 목록 (읽기 전용)</returns>
        public IReadOnlyList<StatusEffect> GetActiveEffects(GameObject target)
        {
            if (target == null || !activeEffects.ContainsKey(target))
            {
                return new List<StatusEffect>();
            }

            return activeEffects[target].AsReadOnly();
        }

        /// <summary>
        /// 대상이 특정 효과를 가지고 있는지 확인
        /// </summary>
        /// <param name="target">대상</param>
        /// <param name="effectType">효과 타입</param>
        /// <returns>효과가 있으면 true</returns>
        public bool HasEffect(GameObject target, StatusEffectType effectType)
        {
            if (target == null || !activeEffects.ContainsKey(target))
            {
                return false;
            }

            return activeEffects[target].Any(e => e.EffectType == effectType);
        }

        /// <summary>
        /// 대상의 특정 효과 가져오기
        /// </summary>
        /// <param name="target">대상</param>
        /// <param name="effectType">효과 타입</param>
        /// <returns>StatusEffect 또는 null</returns>
        public StatusEffect GetEffect(GameObject target, StatusEffectType effectType)
        {
            if (target == null || !activeEffects.ContainsKey(target))
            {
                return null;
            }

            return activeEffects[target].Find(e => e.EffectType == effectType);
        }


        // ====== Context Menu (테스트용) ======

        [ContextMenu("Print All Active Effects")]
        private void PrintAllActiveEffects()
        {
            Debug.Log("========== Active Status Effects ==========");
            Debug.Log($"Total Targets: {activeEffects.Count}");

            foreach (var kvp in activeEffects)
            {
                GameObject target = kvp.Key;
                List<StatusEffect> effects = kvp.Value;

                Debug.Log($"Target: {target.name} ({effects.Count} effects)");

                foreach (StatusEffect effect in effects)
                {
                    Debug.Log($"  - {effect.ToString()}");
                }
            }

            Debug.Log("==========================================");
        }

        [ContextMenu("Remove All Effects (Test)")]
        private void TestRemoveAllEffects()
        {
            List<GameObject> targets = new List<GameObject>(activeEffects.Keys);

            foreach (GameObject target in targets)
            {
                RemoveAllEffects(target);
            }

            Debug.Log("[StatusEffectManager] 모든 효과 제거 완료 (테스트)");
        }
    }
}
