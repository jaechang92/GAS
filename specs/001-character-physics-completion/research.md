# 연구: CharacterPhysics 시스템 완성

**기능**: 001-character-physics-completion
**날짜**: 2025-10-28
**상태**: Phase 0 연구

## 개요

이 문서는 CharacterPhysics 시스템 완성을 위한 기술 연구 결과를 정리합니다.

## RT-001: 쿨다운 타이머 구현

**질문**: 낙하 플랫폼 통과 후 0.3초 쿨다운을 어떻게 구현할 것인가?

**평가한 옵션들**:
1. FixedUpdate + Time.fixedDeltaTime 누적 (동기)
2. async Awaitable.WaitForSecondsAsync (비동기)

**결정**: **FixedUpdate + Dictionary<Collider2D, float> 타이머**

**근거**:
- Constitution Principle VI 준수: Coroutine 금지
- 물리는 FixedUpdate에서 처리되므로 동기 타이머가 자연스러움
- 여러 플랫폼의 쿨다운을 Dictionary로 효율적으로 관리
- async는 I/O나 긴 작업에 적합, 0.3초 타이머는 동기가 더 단순

**구현 패턴**:


**고려했던 대안**:
- async Awaitable: 물리 타이밍과 맞지 않음, 불필요한 복잡성
- Coroutine: Constitution 위반

---

## RT-002: 벽 감지 Layer 아키텍처

**질문**: 벽 감지를 위한 별도 Layer가 필요한가?

**평가한 옵션들**:
1. 새로운 Wall Layer 추가
2. Ground Layer + Wallable Tag 조합
3. Ground Layer 재사용

**결정**: **Ground Layer 재사용 (Wall Layer 불필요)**

**근거**:
- 기존 Layer 시스템 단순 유지
- 벽도 플랫폼의 일종 (Ground로 간주 가능)
- BoxCast 방향으로 벽 vs 바닥 구분 가능
  - 좌우 BoxCast → 벽
  - 하단 BoxCast → 바닥
- Layer 추가는 프로젝트 복잡도 증가

**구현 패턴**:


**고려했던 대안**:
- 새 Wall Layer: 불필요한 복잡성, Layer 관리 부담
- Tag 조합: Layer만으로 충분, Tag는 플랫폼 타입 구분에 사용

---

## RT-003: Skull 이벤트 통합

**질문**: 스컬 변경 이벤트를 어떻게 구독하고 물리 특성을 적용할 것인가?

**조사 결과**:
- 기존 Skull 시스템 구조 확인 필요
- PlayerController에 OnSkullChanged 이벤트 존재 가정

**결정**: **이벤트 구독 + 즉시 적용 패턴**

**근거**:
- Unity의 C# 이벤트 패턴 사용
- 스컬 변경 시 물리 특성을 즉시 재계산
- ScriptableObject 참조를 교체하는 방식

**구현 패턴**:


**엣지 케이스 처리**:
- 공중/벽 슬라이딩 중 스컬 변경: 현재 속도를 새 배율로 재조정
- 초기화: Start에서 기본 프로필 로드

---

## RT-004: Unity Physics2D 일방향 플랫폼 패턴

**질문**: PlatformEffector2D vs 수동 Physics2D.IgnoreCollision?

**평가한 옵션들**:
1. PlatformEffector2D 컴포넌트 사용
2. Physics2D.IgnoreCollision 수동 관리
3. Raycast + Layer 변경

**결정**: **Physics2D.IgnoreCollision + 타이머 관리**

**근거**:
- PlatformEffector2D는 제어가 제한적 (쿨다운 커스터마이징 어려움)
- IgnoreCollision은 완전한 제어 가능
- 쿨다운 타이머와 통합 용이 (RT-001 패턴 재사용)

**구현 패턴**:


**충돌 조건**:
- 아래 방향 + 점프 입력: IgnoreCollision 활성화
- 위에서 떨어질 때 (velocity.y <= 0): 충돌 허용
- 아래에서 위로 점프: IgnoreCollision 유지

**고려했던 대안**:
- PlatformEffector2D: 제어 부족, Constitution 원칙과 맞지 않음
- Layer 변경: 프레임 단위 Layer 변경은 불안정, 다른 충돌에 영향

---

## 요약

| 연구 작업 | 결정 사항 | 핵심 기술 |
|-----------|-----------|-----------|
| RT-001 | FixedUpdate + Dictionary 타이머 | Time.fixedDeltaTime |
| RT-002 | Ground Layer 재사용 | Physics2D.BoxCast |
| RT-003 | C# 이벤트 구독 + 즉시 적용 | OnSkullChanged event |
| RT-004 | Physics2D.IgnoreCollision | 수동 충돌 관리 |

**Constitution 준수**: ✅ 모든 결정이 Principle VI (Async Pattern) 준수

**다음 단계**: Data Model 및 Contracts 생성 (Phase 1)