using UnityEngine;

namespace GASPT.Core.SceneManagement
{
    /// <summary>
    /// Scene/Room 전환 후 검증 및 재할당을 수행하는 인터페이스
    /// 각 시스템(카메라, UI 등)이 구현하여 필요한 참조를 검증/재할당
    /// </summary>
    public interface ISceneValidator
    {
        /// <summary>
        /// 검증기 이름 (디버그용)
        /// </summary>
        string ValidatorName { get; }

        /// <summary>
        /// 실행 우선순위 (낮을수록 먼저 실행)
        /// 0-10: 핵심 시스템 (Player, Camera)
        /// 11-50: 일반 시스템 (UI, Audio)
        /// 51-100: 부가 시스템
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// 검증 및 재할당 수행
        /// Scene/Room 로드 완료 후 호출됨
        /// </summary>
        /// <returns>성공 여부</returns>
        Awaitable<bool> ValidateAndReassignAsync();

        /// <summary>
        /// 검증기가 현재 활성 상태인지
        /// 비활성화된 검증기는 ValidateAllAsync에서 스킵됨
        /// </summary>
        bool IsValidatorActive { get; }
    }
}
