# GASPT 프로젝트 작업 현황 및 다음 단계

**작성일**: 2025-10-29
**최종 업데이트**: 2025-10-29
**현재 브랜치**: `001-character-physics-completion`
**현재 Phase**: Phase 2 (Advanced Physics & Combat) - 85% 진행 중

---

## 📋 이번 세션 작업 완료 사항

### 1. 프로젝트 전체 분석 완료 ✅
- **사용 도구**: Task tool (Explore 에이전트)
- **분석 범위**:
  - 23개 Assembly Definitions 및 계층 구조
  - GAS Core, FSM Core, CharacterPhysics, Combat, Skull, Enemy AI 시스템
  - 코딩 규칙 및 기술 스택 분석
- **결과**: 프로젝트 전체 구조 및 현황 파악 완료

### 2. Game Design Document (GDD) 작성 완료 ✅
- **파일 위치**: `D:\JaeChang\UintyDev\GASPT\docs\GAME_DESIGN_DOCUMENT.md`
- **분량**: 약 2,700줄 (목차 포함 12개 섹션)
- **주요 내용**:
  - 프로젝트 개요, 게임 컨셉, 핵심 시스템 기획
  - GAS/FSM 통합 아키텍처, CharacterPhysics, Combat 시스템
  - UI/UX 설계, 적 AI 설계, 레벨 디자인
  - 사운드/VFX, 개발 로드맵, 기술 제약사항

### 3. GDD 검증 및 개선 완료 ✅
- **사용 도구**: plan-analyzer 에이전트
- **검증 결과**: ⭐⭐⭐⭐☆ (4/5) → **⭐⭐⭐⭐⭐ (5/5)**
- **개선 항목**:

#### P0 (즉시 수정 필요) - 완료 ✅
1. **Constitution 참조 추가** (섹션 0)
   - GASPT Constitution v1.1.0의 9가지 핵심 원칙 명시
   - 특히 Principle VI (Coroutine 절대 금지) 강조

2. **Skull 시스템 의존성 명확화** (섹션 3.5.7)
   - "Skull 없이도 플레이 가능" 설계 (완성 우선 원칙)
   - Phase 3 시작 전 필수 준비사항 체크리스트

3. **Bootstrap 초기화 순서 명시** (섹션 5.3.1)
   - Bootstrap → GameResourceManager → GameFlow → MainMenu → Ingame 전체 흐름

#### P1 (Phase 3 시작 전) - 완료 ✅
4. **Phase 2 완료 기준 명확화** (섹션 11.2.1~11.2.3)
   - CharacterPhysics 85% → 100% 완료 조건
   - Combat System 콤보 체이닝 안정성 테스트 필요성

5. **UI 이벤트 시스템 연결** (섹션 7.5)
   - HealthBarUI, ComboCounterUI, BossHealthBarUI 구현 예시
   - Observer 패턴 기반 이벤트 구독/해제

6. **적 AI 아키텍처 통일** (섹션 8.1)
   - 행동 트리 제거 → FSM Core 시스템 사용으로 통일
   - EnemyController FSM 구조 및 구현 예시

#### P2 (Phase 4 시작 전) - 완료 ✅
7. **레벨 제작 워크플로우** (섹션 9.4)
   - RoomData ScriptableObject 구조
   - DungeonGenerator, RoomEditorWindow, RoomTransitionManager 설계

8. **성능 최적화 목표 구체화** (섹션 12.2.1)
   - 동시 처리 한계: 적 20마리, 히트박스 50개, 파티클 30개
   - 프레임 예산: 물리 2ms, 렌더링 10ms, 스크립트 4ms

9. **사운드/VFX 에셋 가이드** (섹션 10.3)
   - 사운드 25개 + VFX 15개 체크리스트
   - 무료/유료 에셋 소스 추천
   - AudioManager, VFXManager 통합 가이드

---

## 🎯 현재 프로젝트 상태

### Phase별 완료 현황

| Phase | 상태 | 완성도 | 비고 |
|-------|------|--------|------|
| **Phase 1: Core Systems** | ✅ 완료 | 100% | GAS Core, FSM Core, GameFlow 완료 |
| **Phase 2: Advanced Physics & Combat** | 🔄 진행 중 | 85% | 히트박스 정교화, 콤보 안정성 테스트 필요 |
| **Phase 3: Skull System** | ⏳ 대기 | 0% | Phase 2 완료 후 진입 |
| **Phase 4: Content** | ⏳ 대기 | 0% | 적, 보스, 던전 제작 |
| **Phase 5: Polish** | ⏳ 대기 | 0% | 애니메이션, VFX, 사운드 통합 |
| **Phase 6: Optimization** | ⏳ 대기 | 0% | Object Pool, 성능 최적화 |

### 최근 커밋 이력
```
c28c63c - refactor: 어빌리티 하드코딩 제거 - 추상적인 입력 타입 매핑 시스템 도입
5f05344 - feat: FSM과 AbilitySystem 통합 - 입력 기반 어빌리티 실행
ccfa101 - refactor: isChainStarter 및 autoResetChain 필드 제거
c162936 - refactor: 체인 진행 조건을 Chain Starter에서 NextAbilityId 존재 여부로 변경
bcf681e - fix: 체인 중복 진행 및 건너뛰기 버그 수정
```

**주의**: 콤보 체이닝 관련 최근 버그 수정 이력이 많음 → 안정성 집중 테스트 필요

---

## 🚀 다음 작업 옵션

### 옵션 1: 구현 계획 수립 (권장)
**목적**: GDD를 실제 코드 태스크로 변환
**도구**: planning-to-code-architect 에이전트
**예상 시간**: 1~2시간

**진행 방법**:
```
1. planning-to-code-architect 에이전트 호출
2. GAME_DESIGN_DOCUMENT.md를 입력으로 제공
3. 에이전트가 다음을 생성:
   - Phase별 구현 계획
   - 파일 위치 및 클래스 구조
   - 의존성 및 우선순위
   - 테스트 전략
```

**산출물**:
- 구체적인 코딩 태스크 리스트
- 각 태스크별 파일 위치 및 구현 가이드
- Phase 2 완료를 위한 실행 계획

### 옵션 2: Phase 2 완료 작업 (코드 구현)
**목적**: Phase 2 나머지 15% 완료
**도구**: code-generator 에이전트
**예상 시간**: 1주~2주

**필수 작업**:
1. **히트박스/허트박스 정교화 (30% → 70% → 100%)**
   - [ ] 히트박스 크기 조정 (공격별 정확한 범위)
   - [ ] 허트박스 레이어 분리 (머리/몸통/다리)
   - [ ] 디버그 시각화 (Gizmos로 박스 표시)

2. **콤보 체이닝 안정성 테스트**
   - [ ] 3단 콤보가 100% 연결되는지 테스트
   - [ ] 타이밍 윈도우(0.5초) 적절성 검증
   - [ ] 체인 중단 시 정상 리셋 확인
   - [ ] NextAbilityId 기반 체인 진행 버그 검증

**완료 후**:
- Phase 2 → Phase 3 진입 가능
- Skull 시스템 구현 시작

### 옵션 3: Speckit 워크플로우 실행
**목적**: 체계적인 개발 계획 수립
**도구**: /speckit.plan 또는 /speckit.tasks
**예상 시간**: 30분~1시간

**진행 방법**:
```bash
/speckit.plan    # 계획 문서 생성
/speckit.tasks   # 태스크 목록 생성
```

---

## 📌 중요 사항 (다음 작업 시 반드시 확인)

### 1. Phase 2 완료 전 Phase 3 진입 금지 ⚠️
**이유**: Phase 2가 불완전한 상태에서 Skull 시스템을 추가하면 버그가 복합적으로 발생할 위험이 높습니다.

**Phase 2 완료 조건**:
- [x] 벽 점프/슬라이딩 완벽 동작
- [x] 낙하 플랫폼 재충돌 방지
- [x] Skull 프로필 교체 지원
- [ ] **히트박스 크기가 시각적 범위와 일치** (남은 작업)
- [ ] **허트박스가 스프라이트와 일치** (남은 작업)
- [ ] **디버그 박스 시각화 가능** (남은 작업)
- [ ] **3단 콤보 안정적 동작** (집중 테스트 필요)

### 2. Skull 시스템은 "확장 기능"으로 설계
**완성 우선 원칙**: Skull 없이도 전체 게임 플레이 가능해야 함
- DefaultProfile로 단독 동작 보장
- 기본 어빌리티(공격, 점프, 대시)는 Skull 독립적

### 3. 코딩 규칙 준수 (Constitution Principle VI - NON-NEGOTIABLE)
- **Coroutine 절대 금지** ❌ → Awaitable 사용 ✅
- 변수명에 언더스코어(_) 금지 → camelCase 사용
- Unity 6.0+ API 사용 (velocity → linearVelocity)
- 한글 주석 허용 (UTF-8 인코딩)
- 파일당 500줄 제한 (초과 시 분할)

---

## 📚 관련 문서

| 문서 | 경로 | 용도 |
|------|------|------|
| **Game Design Document** | `D:\JaeChang\UintyDev\GASPT\docs\GAME_DESIGN_DOCUMENT.md` | 전체 게임 설계 문서 (2,700줄) |
| **Constitution** | `.specify/memory/constitution.md` | 프로젝트 헌법 (9가지 핵심 원칙) |
| **CODE_CONTEXT** | `.claude/CODE_CONTEXT.md` | 프로젝트 컨텍스트 정보 |
| **README** | `D:\JaeChang\UintyDev\GASPT\GASPT\README.md` | 프로젝트 개요 |
| **WORK (현재 문서)** | `D:\JaeChang\UintyDev\GASPT\docs\WORK.md` | 작업 현황 및 다음 단계 |

---

## 💡 추천 다음 작업 순서

1. **즉시 (이번 세션 또는 다음 세션 시작 시)**:
   - 옵션 1: planning-to-code-architect 에이전트로 구현 계획 수립
   - Phase 2 완료를 위한 구체적인 코딩 태스크 리스트 생성

2. **Phase 2 완료 (1~2주)**:
   - 히트박스/허트박스 정교화 (70% 달성)
   - 콤보 체이닝 안정성 테스트 및 버그 수정
   - Phase 2 완료 조건 전체 체크

3. **Phase 3 진입 (Phase 2 완료 후)**:
   - DefaultSkull 최소 구현
   - GAS/FSM/CharacterPhysics 통합 검증
   - Skull 교체 UI 추가

4. **Phase 4~6 (Phase 3 완료 후)**:
   - 콘텐츠 제작 (적, 보스, 던전)
   - 애니메이션/VFX/사운드 통합
   - 성능 최적화 (Object Pool)

---

## 🔍 검증 체크리스트

다음 작업 시작 전 확인:
- [ ] 이 문서(WORK.md) 읽고 현재 상태 파악
- [ ] GAME_DESIGN_DOCUMENT.md에서 해당 Phase 섹션 확인
- [ ] Constitution의 9가지 원칙 숙지
- [ ] 최근 커밋 이력 확인 (콤보 체이닝 버그 수정 多)
- [ ] 현재 브랜치 확인 (`001-character-physics-completion`)

---

## 📞 문의 및 참고

- **프로젝트 경로**: `D:\JaeChang\UintyDev\GASPT\GASPT`
- **주요 브랜치**: `master` (메인), `001-character-physics-completion` (현재)
- **Unity 버전**: 6.0+
- **타겟 플랫폼**: Windows PC (x64)

---

**마지막 업데이트**: 2025-10-29
**다음 업데이트 예정**: 구현 계획 수립 후
