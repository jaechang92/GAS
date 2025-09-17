// ===================================
// 파일: Assets/Scripts/Ability/Data/SkulData.cs
// ===================================
using System.Collections.Generic;
using UnityEngine;

namespace AbilitySystem.Platformer
{
    /// <summary>
    /// 스컬(캐릭터) 데이터
    /// </summary>
    [CreateAssetMenu(fileName = "NewSkul", menuName = "Ability/Platformer/SkulData")]
    public class SkulData : ScriptableObject
    {
        [Header("스컬 정보")]
        public string skulId;
        public string skulName;
        [TextArea(3, 5)]
        public string description;
        public Sprite icon;
        public SkulType skulType;

        [Header("기본 스탯")]
        public float attackPower = 10f;
        public float attackSpeed = 1f;
        public float moveSpeed = 5f;
        public float jumpPower = 10f;

        [Header("어빌리티")]
        public PlatformerAbilityData basicAttack;      // 기본 공격
        public PlatformerAbilityData skill1;           // 스킬 1
        public PlatformerAbilityData skill2;           // 스킬 2
        public PlatformerAbilityData dashAbility;      // 대시
        public List<PlatformerAbilityData> passives;   // 패시브 목록

        [Header("콤보 설정")]
        public int maxComboCount = 3;
        public float comboResetTime = 1f;
        public float[] comboDamageMultipliers = { 1f, 1.2f, 1.5f };
    }
}