// ===================================
// 파일: Assets/Scripts/Ability/Core/AbilitySystem.cs
// ===================================
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace AbilitySystem
{
    /// <summary>
    /// 캐릭터의 어빌리티를 관리하는 시스템
    /// </summary>
    public class AbilitySystem : MonoBehaviour
    {
        [Header("어빌리티 설정")]
        [SerializeField] private List<AbilityData> initialAbilities = new List<AbilityData>();

        // 등록된 어빌리티 목록
        private Dictionary<string, Ability> abilities = new Dictionary<string, Ability>();

        // 캐릭터 스탯 참조 (임시)
        [Header("캐릭터 스탯")]
        [SerializeField] private int maxMana = 100;
        [SerializeField] private int currentMana = 100;
        [SerializeField] private int maxStamina = 100;
        [SerializeField] private int currentStamina = 100;

        // 프로퍼티
        public int CurrentMana => currentMana;
        public int CurrentStamina => currentStamina;
        public IReadOnlyDictionary<string, Ability> Abilities => abilities;

        /// <summary>
        /// 시스템 초기화
        /// </summary>
        private void Awake()
        {
            // 초기 어빌리티 로드
        }

        /// <summary>
        /// 매 프레임 쿨다운 업데이트
        /// </summary>
        private void Update()
        {
            // 모든 어빌리티의 쿨다운 업데이트
        }

        /// <summary>
        /// 어빌리티 등록
        /// </summary>
        public bool RegisterAbility(AbilityData abilityData)
        {
            // 새로운 어빌리티를 시스템에 등록
            return false;
        }

        /// <summary>
        /// 어빌리티 제거
        /// </summary>
        public bool UnregisterAbility(string abilityId)
        {
            // 등록된 어빌리티 제거
            return false;
        }

        /// <summary>
        /// 어빌리티 사용 시도
        /// </summary>
        public async Awaitable TryUseAbility(string abilityId)
        {
            // 어빌리티 사용 가능 여부 체크 후 실행
            await Awaitable.NextFrameAsync();
        }

        /// <summary>
        /// 특정 어빌리티 가져오기
        /// </summary>
        public Ability GetAbility(string abilityId)
        {
            // ID로 어빌리티 검색
            return null;
        }

        /// <summary>
        /// 모든 어빌리티 가져오기
        /// </summary>
        public List<Ability> GetAllAbilities()
        {
            // 등록된 모든 어빌리티 반환
            return abilities.Values.ToList();
        }

        /// <summary>
        /// 사용 가능한 어빌리티 목록 가져오기
        /// </summary>
        public List<Ability> GetUsableAbilities()
        {
            // 현재 사용 가능한 어빌리티만 필터링
            return null;
        }

        /// <summary>
        /// 코스트 소비 처리
        /// </summary>
        private bool ConsumeResources(AbilityData abilityData)
        {
            // 마나, 스태미나 등 소비
            return false;
        }

        /// <summary>
        /// 모든 어빌리티 쿨다운 리셋
        /// </summary>
        public void ResetAllCooldowns()
        {
            // 모든 어빌리티의 쿨다운 초기화
        }

        /// <summary>
        /// 시스템 정리
        /// </summary>
        private void OnDestroy()
        {
            // 리소스 정리
        }
    }
}