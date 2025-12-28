using UnityEngine;
using GASPT.Core.Enums;
using GASPT.StatusEffects;

namespace GASPT.Data
{
    /// <summary>
    /// 소비 아이템 데이터 (ScriptableObject)
    /// ItemData를 상속받아 소비 전용 필드 추가
    /// </summary>
    [CreateAssetMenu(fileName = "ConsumableData", menuName = "GASPT/Items/ConsumableData")]
    public class ConsumableData : ItemData
    {
        // ====== 소비 효과 ======

        [Header("소비 효과")]
        [Tooltip("효과 타입")]
        public ConsumeType consumeType = ConsumeType.Heal;

        [Tooltip("효과 수치 (HP 회복량, 버프 수치 등)")]
        [Min(0)]
        public float effectValue;

        [Tooltip("지속 시간 (초, 0 = 즉시)")]
        [Min(0)]
        public float effectDuration;

        [Tooltip("재사용 대기시간 (초)")]
        [Min(0)]
        public float cooldown;


        // ====== 버프 효과 ======

        [Header("버프 효과 (Buff 타입용)")]
        [Tooltip("적용할 버프 효과 (consumeType이 Buff일 때)")]
        public StatusEffectData buffEffect;


        // ====== 프로퍼티 ======

        /// <summary>
        /// 즉시 효과 여부
        /// </summary>
        public bool IsInstant => effectDuration <= 0f;

        /// <summary>
        /// 쿨다운 여부
        /// </summary>
        public bool HasCooldown => cooldown > 0f;

        /// <summary>
        /// 버프 타입 여부
        /// </summary>
        public bool IsBuffType => consumeType == ConsumeType.Buff;


        // ====== Unity 콜백 ======

        private void OnValidate()
        {
            // 카테고리를 소비로 강제
            category = ItemCategory.Consumable;

            // 소비 아이템은 기본적으로 스택 가능
            if (!stackable)
            {
                stackable = true;
                maxStack = 99;
            }

            // itemId 자동 생성 (비어있을 때)
            if (string.IsNullOrEmpty(itemId))
            {
                string prefix = consumeType switch
                {
                    ConsumeType.Heal => "POT",
                    ConsumeType.HealOverTime => "POT",
                    ConsumeType.RestoreMana => "POT",
                    ConsumeType.Buff => "BUF",
                    ConsumeType.Cleanse => "CLN",
                    ConsumeType.Teleport => "SCR",
                    ConsumeType.Revive => "REV",
                    _ => "CON"
                };
                itemId = $"{prefix}_{name}";
            }

            // Buff 타입인데 buffEffect가 없으면 경고
            if (consumeType == ConsumeType.Buff && buffEffect == null)
            {
                Debug.LogWarning($"[ConsumableData] {itemName}: Buff 타입이지만 buffEffect가 설정되지 않았습니다.");
            }
        }


        // ====== 디버그 ======

        /// <summary>
        /// 디버그용 문자열 출력
        /// </summary>
        public override string ToString()
        {
            string effect = consumeType switch
            {
                ConsumeType.Heal => $"HP +{effectValue}",
                ConsumeType.HealOverTime => $"HP +{effectValue}/{effectDuration}s",
                ConsumeType.RestoreMana => $"MP +{effectValue}",
                ConsumeType.Buff => buffEffect != null ? buffEffect.displayName : "버프",
                ConsumeType.Cleanse => "상태이상 해제",
                ConsumeType.Teleport => "이동",
                ConsumeType.Revive => "부활",
                _ => "효과"
            };

            return $"[{rarity}] {itemName} ({effect})";
        }
    }
}
