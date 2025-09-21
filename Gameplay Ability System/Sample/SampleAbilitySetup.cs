// ===================================
// 파일: Assets/Scripts/Ability/Sample/SampleAbilitySetup.cs
// ===================================
using UnityEngine;
using System.Collections.Generic;
using AbilitySystem.Platformer;

namespace AbilitySystem
{
    /// <summary>
    /// 테스트용 어빌리티 셋업 헬퍼
    /// </summary>
    public class SampleAbilitySetup : MonoBehaviour
    {
        [Header("테스트 설정")]
        [SerializeField] private AbilitySystem playerAbilitySystem;
        [SerializeField] private PlatformerAbilityController abilityController;
        [SerializeField] private List<SkulData> testAbilities;

        [Header("테스트 컨트롤")]
        [SerializeField] private bool autoSetupOnStart = true;
        [SerializeField] private bool enableDebugKeys = true;

        /// <summary>
        /// 자동 셋업
        /// </summary>
        private void Start()
        {
            // 테스트 어빌리티 자동 등록
        }

        /// <summary>
        /// 테스트 어빌리티 등록
        /// </summary>
        public void SetupTestAbilities()
        {
            // 미리 정의된 어빌리티 등록
        }

        /// <summary>
        /// 기본 파이어볼 생성
        /// </summary>
        private SkulData CreateFireballAbility()
        {
            // 파이어볼 어빌리티 데이터 생성
            return null;
        }

        /// <summary>
        /// 기본 힐 생성
        /// </summary>
        private SkulData CreateHealAbility()
        {
            // 힐 어빌리티 데이터 생성
            return null;
        }

        /// <summary>
        /// 디버그 키 처리
        /// </summary>
        private void Update()
        {
            // 테스트용 단축키 처리
        }

        /// <summary>
        /// 모든 쿨다운 리셋 (테스트용)
        /// </summary>
        [ContextMenu("Reset All Cooldowns")]
        public void ResetAllCooldowns()
        {
            // 디버그용 쿨다운 초기화
        }

        /// <summary>
        /// 무한 리소스 토글 (테스트용)
        /// </summary>
        [ContextMenu("Toggle Infinite Resources")]
        public void ToggleInfiniteResources()
        {
            // 디버그용 무한 마나/스태미나
        }

        /// <summary>
        /// 성능 테스트
        /// </summary>
        [ContextMenu("Run Performance Test")]
        public void RunPerformanceTest()
        {
            // 다수 어빌리티 동시 실행 테스트
        }
    }
}