using UnityEngine;

namespace GASPT.UI.MVP
{
    /// <summary>
    /// 보스 체력바 View 인터페이스
    /// MVP 패턴의 View 계약
    /// </summary>
    public interface IBossHealthBarView
    {
        /// <summary>
        /// 보스 체력바 표시
        /// </summary>
        /// <param name="bossName">보스 이름</param>
        /// <param name="totalPhases">총 페이즈 수</param>
        void Show(string bossName, int totalPhases);

        /// <summary>
        /// 보스 체력바 숨기기
        /// </summary>
        void Hide();

        /// <summary>
        /// 체력 업데이트
        /// </summary>
        /// <param name="currentRatio">현재 체력 비율 (0~1)</param>
        void UpdateHealth(float currentRatio);

        /// <summary>
        /// 체력 업데이트 (애니메이션)
        /// </summary>
        /// <param name="currentRatio">목표 체력 비율</param>
        /// <param name="animationDuration">애니메이션 시간</param>
        void UpdateHealthAnimated(float currentRatio, float animationDuration = 0.3f);

        /// <summary>
        /// 페이즈 업데이트
        /// </summary>
        /// <param name="currentPhase">현재 페이즈 (1부터 시작)</param>
        /// <param name="totalPhases">총 페이즈 수</param>
        void UpdatePhase(int currentPhase, int totalPhases);

        /// <summary>
        /// 무적 상태 표시 설정
        /// </summary>
        /// <param name="invulnerable">무적 여부</param>
        void SetInvulnerable(bool invulnerable);

        /// <summary>
        /// 체력바 색상 설정
        /// </summary>
        /// <param name="color">체력바 색상</param>
        void SetHealthBarColor(Color color);

        /// <summary>
        /// 보스 이름 설정
        /// </summary>
        /// <param name="name">보스 이름</param>
        void SetBossName(string name);

        /// <summary>
        /// 페이즈 전환 연출
        /// </summary>
        /// <param name="newPhase">새 페이즈 번호</param>
        void PlayPhaseTransitionEffect(int newPhase);

        /// <summary>
        /// 시간 제한 표시 설정
        /// </summary>
        /// <param name="remainingTime">남은 시간 (초)</param>
        /// <param name="show">표시 여부</param>
        void UpdateTimeLimit(float remainingTime, bool show = true);
    }
}
