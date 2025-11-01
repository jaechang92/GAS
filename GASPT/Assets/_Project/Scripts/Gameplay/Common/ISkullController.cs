using UnityEngine;
using System.Threading;

namespace Gameplay.Common
{
    /// <summary>
    /// 스컬 컨트롤러 인터페이스
    /// 모든 스컬이 구현해야 하는 기본 기능들
    /// </summary>
    public interface ISkullController
    {
        /// <summary>
        /// 스컬 타입
        /// </summary>
        SkullType SkullType { get; }

        /// <summary>
        /// 스컬 데이터
        /// </summary>
        SkullData SkullData { get; }

        /// <summary>
        /// 현재 스컬이 활성화되어 있는지
        /// </summary>
        bool IsActive { get; }

        /// <summary>
        /// 스컬 장착 시 호출
        /// </summary>
        Awaitable OnEquip(CancellationToken cancellationToken = default);

        /// <summary>
        /// 스컬 해제 시 호출
        /// </summary>
        Awaitable OnUnequip(CancellationToken cancellationToken = default);

        /// <summary>
        /// 스컬 활성화 (교체 완료 후)
        /// </summary>
        void OnActivate();

        /// <summary>
        /// 스컬 비활성화 (교체 시작 전)
        /// </summary>
        void OnDeactivate();

        /// <summary>
        /// 기본 공격 실행
        /// </summary>
        Awaitable PerformPrimaryAttack(CancellationToken cancellationToken = default);

        /// <summary>
        /// 보조 공격 실행
        /// </summary>
        Awaitable PerformSecondaryAttack(CancellationToken cancellationToken = default);

        /// <summary>
        /// 궁극기 실행
        /// </summary>
        Awaitable PerformUltimate(CancellationToken cancellationToken = default);

        /// <summary>
        /// 스컬 던지기 실행
        /// </summary>
        Awaitable PerformSkullThrow(Vector2 direction, CancellationToken cancellationToken = default);

        /// <summary>
        /// 프레임별 업데이트
        /// </summary>
        void OnUpdate();

        /// <summary>
        /// 물리 프레임별 업데이트
        /// </summary>
        void OnFixedUpdate();

        /// <summary>
        /// 스컬의 현재 상태 정보 가져오기
        /// </summary>
        SkullStatus GetStatus();
    }

    /// <summary>
    /// 스컬 상태 정보
    /// </summary>
    [System.Serializable]
    public struct SkullStatus
    {
        public bool isReady;
        public float cooldownRemaining;
        public float manaRemaining;
        public bool canUseAbilities;

        public static SkullStatus Ready => new SkullStatus
        {
            isReady = true,
            cooldownRemaining = 0f,
            manaRemaining = 100f,
            canUseAbilities = true
        };

        public static SkullStatus NotReady(float cooldown = 0f) => new SkullStatus
        {
            isReady = false,
            cooldownRemaining = cooldown,
            manaRemaining = 0f,
            canUseAbilities = false
        };
    }
}
