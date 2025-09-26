# 🎮 Player Scripts

이 폴더는 플레이어 캐릭터 관련 모든 스크립트를 포함합니다.

## 📁 폴더 구조

```
Player/
├── PlayerController.cs          # 메인 플레이어 컨트롤러
├── InputHandler.cs             # 입력 처리 전담
├── PhysicsController.cs        # 물리 시스템 전담
├── EnvironmentChecker.cs       # 환경 검사 전담
├── AnimationController.cs      # 애니메이션 제어 전담
├── GroundChecker.cs           # 지면 검사 전담
├── PlayerStateType.cs         # 상태/이벤트 정의
├── PlayerStats.cs             # 플레이어 능력치
├── PlayerStateTransitions.cs  # 상태 전환 규칙
├── PlayerSetupGuide.cs        # 설정 도우미
├── States/                    # 플레이어 상태들
│   ├── PlayerBaseState.cs     # 베이스 상태
│   ├── PlayerIdleState.cs     # 대기 상태
│   ├── PlayerMoveState.cs     # 이동 상태
│   ├── PlayerJumpState.cs     # 점프 상태
│   ├── PlayerFallState.cs     # 낙하 상태
│   ├── PlayerDashState.cs     # 대시 상태
│   ├── PlayerAttackState.cs   # 공격 상태
│   ├── PlayerHitState.cs      # 피격 상태
│   ├── PlayerDeadState.cs     # 사망 상태
│   ├── PlayerWallGrabState.cs # 벽잡기 상태
│   ├── PlayerWallJumpState.cs # 벽점프 상태
│   └── PlayerSlideState.cs    # 슬라이딩 상태
└── FSM_PLAYER_USAGE_GUIDE.md  # 사용 가이드
```

## 🔧 아키텍처

플레이어 시스템은 **단일책임원칙**을 준수하는 **컴포넌트 조합 패턴**으로 설계되었습니다:

- **PlayerController**: 메인 코디네이터 (FSM 상태 관리)
- **InputHandler**: 입력 처리 전담
- **PhysicsController**: 커스텀 물리 시스템
- **EnvironmentChecker**: 환경 검사 (벽, 대시 쿨다운)
- **AnimationController**: 애니메이션 제어
- **GroundChecker**: 지면 검사

## 📋 사용법

1. **캐릭터 생성**: PlayerSetupGuide 컴포넌트 사용
2. **상태 추가**: PlayerBaseState를 상속하여 새 상태 생성
3. **커스터마이징**: 각 컴포넌트별 개별 설정 가능

자세한 내용은 `FSM_PLAYER_USAGE_GUIDE.md`를 참조하세요.