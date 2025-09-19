

// ===================================
// 파일: Assets/Scripts/Ability/Executors/BuffAbilityExecutor.cs
// ===================================
using AbilitySystem.Platformer;
using Helper;
using System.Collections.Generic;
using UnityEngine;

namespace AbilitySystem
{
    /// <summary>
    /// 버프/디버프를 적용하는 어빌리티 실행자
    /// </summary>
    [CreateAssetMenu(fileName = "BuffExecutor", menuName = "Ability/Executors/Buff")]
    public class BuffAbilityExecutor : AbilityExecutor
    {
        [Header("버프 설정")]
        [SerializeField] private string buffId;
        [SerializeField] private float duration = 5f;
        [SerializeField] private bool isStackable = false;
        [SerializeField] private int maxStacks = 1;

        [Header("스탯 변경")]
        [SerializeField] private float attackModifier = 0f;
        [SerializeField] private float defenseModifier = 0f;
        [SerializeField] private float speedModifier = 0f;
        [SerializeField] private float cooldownReduction = 0f;

        [Header("버프 타입")]
        [SerializeField] private bool isBuff = true;  // true: 버프, false: 디버프
        [SerializeField] private bool isDispellable = true;
        [SerializeField] private bool isPurgeable = true;

        /// <summary>
        /// 버프 어빌리티 실행
        /// </summary>
        public override async Awaitable ExecuteAsync(GameObject caster, PlatformerAbilityData data, List<IAbilityTarget> targets)
        {
            // 타겟에게 버프/디버프 적용
            await AwaitableHelper.CompletedTask;
        }

        /// <summary>
        /// 버프 적용
        /// </summary>
        private void ApplyBuff(GameObject caster, IAbilityTarget target)
        {
            // 버프 효과 적용
        }

        /// <summary>
        /// 디버프 적용
        /// </summary>
        private void ApplyDebuff(GameObject caster, IAbilityTarget target)
        {
            // 디버프 효과 적용
        }

        /// <summary>
        /// 스택 처리
        /// </summary>
        private void HandleStacking(IAbilityTarget target)
        {
            // 버프 스택 관리
        }

        /// <summary>
        /// 버프 갱신
        /// </summary>
        private void RefreshBuff(IAbilityTarget target)
        {
            // 기존 버프 지속시간 갱신
        }

        /// <summary>
        /// 버프 제거
        /// </summary>
        private void RemoveBuff(IAbilityTarget target)
        {
            // 버프 효과 제거
        }

        /// <summary>
        /// 버프 아이콘 표시
        /// </summary>
        private void ShowBuffIcon(IAbilityTarget target, bool isActive)
        {
            // UI에 버프 아이콘 표시/제거
        }
    }
}