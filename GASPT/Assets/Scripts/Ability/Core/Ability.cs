// ===================================
// 파일: Assets/Scripts/Ability/Core/Ability.cs
// ===================================
using AbilitySystem.Platformer;
using System;
using Unity.VisualScripting;
using UnityEngine;

namespace AbilitySystem
{
    /// <summary>
    /// 런타임 어빌리티 인스턴스 클래스
    /// </summary>
    public class Ability
    {
        // 어빌리티 데이터 참조
        private SkulData data;

        // 상태 관리
        private AbilityState currentState;

        // 소유자 정보
        private GameObject owner;

        // 이벤트
        public event Action<Ability> OnAbilityStarted;
        public event Action<Ability> OnAbilityCompleted;
        public event Action<Ability> OnCooldownStarted;
        public event Action<Ability> OnCooldownCompleted;

        // 프로퍼티
        public string Id => data?.skulId;
        public string Name => data?.skulName;
        public SkulData Data => data;
        public AbilityState State => currentState;
        public bool IsReady => currentState == AbilityState.Ready;

        /// <summary>
        /// 어빌리티 초기화
        /// </summary>
        public void Initialize(SkulData abilityData, GameObject abilityOwner)
        {
            // 데이터와 소유자 설정
        }

        /// <summary>
        /// 어빌리티 사용 가능 여부 체크
        /// </summary>
        public bool CanUse()
        {
            // 상태, 쿨다운, 코스트 등 종합적으로 체크
            return false;
        }

        /// <summary>
        /// 어빌리티 실행
        /// </summary>
        public async Awaitable ExecuteAsync()
        {
            // 어빌리티 실행 로직 (비동기)
            await Awaitable.NextFrameAsync();
        }

        /// <summary>
        /// 쿨다운 업데이트 (매 프레임 호출)
        /// </summary>
        public void UpdateCooldown(float deltaTime)
        {
            // 쿨다운 시간 감소 처리
        }

        /// <summary>
        /// 쿨다운 시작
        /// </summary>
        private void StartCooldown()
        {
            // 쿨다운 시작 처리
        }

        /// <summary>
        /// 어빌리티 강제 중단
        /// </summary>
        public void Cancel()
        {
            // 실행 중인 어빌리티 취소
        }

        /// <summary>
        /// 어빌리티 리셋
        /// </summary>
        public void Reset()
        {
            // 상태 초기화
        }
    }
}