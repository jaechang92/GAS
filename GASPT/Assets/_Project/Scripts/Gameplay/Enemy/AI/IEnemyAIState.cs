namespace GASPT.Gameplay.Enemies.AI
{
    /// <summary>
    /// 적 AI 상태 인터페이스
    /// 모든 AI 상태 클래스가 구현해야 하는 계약
    /// </summary>
    public interface IEnemyAIState
    {
        /// <summary>
        /// 상태 이름 (디버깅용)
        /// </summary>
        string StateName { get; }

        /// <summary>
        /// 상태 진입 시 호출
        /// 초기화, 애니메이션 시작 등
        /// </summary>
        /// <param name="context">AI 컨텍스트</param>
        void Enter(EnemyAIContext context);

        /// <summary>
        /// 상태 퇴장 시 호출
        /// 정리, 리소스 해제 등
        /// </summary>
        /// <param name="context">AI 컨텍스트</param>
        void Exit(EnemyAIContext context);

        /// <summary>
        /// 매 프레임 업데이트 (Update)
        /// 상태 로직 실행
        /// </summary>
        /// <param name="context">AI 컨텍스트</param>
        void Update(EnemyAIContext context);

        /// <summary>
        /// 물리 업데이트 (FixedUpdate)
        /// 이동, 물리 연산 등
        /// </summary>
        /// <param name="context">AI 컨텍스트</param>
        void PhysicsUpdate(EnemyAIContext context);

        /// <summary>
        /// 상태 전환 조건 체크
        /// 전환할 상태가 있으면 반환, 없으면 null
        /// </summary>
        /// <param name="context">AI 컨텍스트</param>
        /// <returns>전환할 상태 또는 null (현재 상태 유지)</returns>
        IEnemyAIState CheckTransitions(EnemyAIContext context);
    }
}
