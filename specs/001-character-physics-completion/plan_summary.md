# 구현 계획: CharacterPhysics 시스템 완성

**브랜치**: \ | **날짜**: 2025-10-28 | **명세**: [spec.md](./spec.md)

## 요약

CharacterPhysics 시스템에 3가지 핵심 플랫포머 메커닉을 추가:
1. **벽 점프 및 슬라이딩** (P1): 수직 공간 탐험
2. **낙하 플랫폼** (P2): 일방향 통과 시스템
3. **스컬별 이동 특성** (P3): 클래스 차별화

기존 CharacterPhysics.cs를 확장하며 ScriptableObject 기반 데이터 관리 유지.

## 계획 진행 상태

| 단계 | 상태 | 산출물 |
|------|------|--------|
| Phase 0: 연구 | ✅ 완료 | [research.md](./research.md) |
| Phase 1: 설계 | ✅ 완료 | [data-model.md](./data-model.md), [contracts/](./contracts/), [quickstart.md](./quickstart.md) |
| Phase 2: 작업 목록 | ⏳ 대기 중 | \ 실행 필요 |

## 연구 결정사항 (Phase 0)

| 작업 | 결정 | 기술 |
|------|------|------|
| RT-001 | FixedUpdate + Dictionary 타이머 | Time.fixedDeltaTime |
| RT-002 | Ground Layer 재사용 | Physics2D.BoxCast |
| RT-003 | C# 이벤트 구독 패턴 | OnSkullChanged event |
| RT-004 | Physics2D.IgnoreCollision | 수동 충돌 관리 |

**세부 내용**: [research.md](./research.md)

## 데이터 모델 (Phase 1)

**정의된 6개 Entity**:
1. SkullMovementProfile (ScriptableObject)
2. WallDetectionData (Struct)
3. OneWayPlatformData (Component)
4. CharacterPhysicsState (확장)
5. WallDirection (Enum)
6. PlatformType (Enum)

**세부 내용**: [data-model.md](./data-model.md)

## API 계약 (Phase 1)

**3개 API 계약**:
1. [CharacterPhysicsAPI.md](./contracts/CharacterPhysicsAPI.md) - 벽 점프, 플랫폼, 스컬 API
2. [OneWayPlatformAPI.md](./contracts/OneWayPlatformAPI.md) - 낙하 플랫폼 API

## Constitution 준수

**모든 9가지 원칙**: ✅ 통과

**주요 체크**:
- ✅ Principle VI: NO Coroutines (FixedUpdate 타이머 사용)
- ✅ Principle IX: linearVelocity, FindAnyObjectByType
- ✅ CamelCase 네이밍, SOLID 원칙

## 구현 범위

**수정할 파일**:
- CharacterPhysics.cs (벽 점프, 플랫폼 로직 추가)
- InputHandler.cs (아래 방향 키 입력 추가)

**생성할 파일**:
- SkullMovementProfile.cs (ScriptableObject)
- OneWayPlatform.cs (Component)
- WallDirection.cs (Enum)
- PlatformType.cs (Enum)
- PlayerWallSlideState.cs (FSM State - 옵션)
- PhysicsCompletionDemo.cs (Demo Scene)

## 다음 단계

1. ✅ Phase 0-1 완료
2. ⏳ **\ 실행** - 구현 작업 목록 생성
3. ⏳ 구현 시작
4. ⏳ Demo Scene 테스트
5. ⏳ Success Criteria 검증

## 빠른 링크

- **명세**: [spec.md](./spec.md) - 기능 명세
- **연구**: [research.md](./research.md) - 기술 결정
- **데이터 모델**: [data-model.md](./data-model.md) - Entity 정의
- **API 계약**: [contracts/](./contracts/) - API 문서
- **빠른 시작**: [quickstart.md](./quickstart.md) - 테스트 가이드
- **체크리스트**: [checklists/requirements.md](./checklists/requirements.md) - 품질 검증

---

**상태**: ✅ 계획 완료, \ 실행 준비 완료
