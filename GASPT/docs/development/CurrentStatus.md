# 🎮 GASPT 프로젝트 현재 진행 상황

**최종 업데이트**: 2025-10-04
**현재 Phase**: Phase 2 - Combat 시스템 + CharacterPhysics 개선
**전체 완성도**: **84%**

---

## 📋 목차
- [완료된 작업](#완료된-작업)
- [최근 작업 (2025-10-04)](#최근-작업-2025-10-04)
- [수정된 버그](#수정된-버그)
- [다음 작업 예정](#다음-작업-예정)
- [테스트 방법](#테스트-방법)

---

## ✅ 완료된 작업

### Phase 1: Core 시스템 구축 ✅ (100%)
- [x] GAS (Gameplay Ability System) Core 구현
- [x] FSM (Finite State Machine) Core 구현
- [x] GameFlow 시스템 (Main/Loading/Ingame/Pause/Menu/Lobby)
- [x] 한글 인코딩 문제 해결 (.gitattributes, .editorconfig)

### Phase 2: Combat 시스템 ✅ (70%)
- [x] **Combat Core 구현**
  - DamageSystem (싱글톤)
  - HealthSystem
  - ComboSystem
  - DamageData / HitData 구조

- [x] **Attack 시스템**
  - BasicAttack 어빌리티 (GAS 기반)
  - HitboxController
  - AttackAnimationHandler
  - ComboData 구조

- [x] **콤보 체인 시스템** ⭐ (2025-10-04 완성)
  - CheckComboInput() 메서드로 콤보 윈도우 내 입력 감지
  - 연속 공격 입력 시 1→2→3 콤보 자동 체인
  - ComboSystem과 완전 통합

- [x] **Damage Types (Core.Enums)**
  - Physical, Magical, True
  - Fire, Ice, Lightning, Poison, Dark, Holy
  - Environmental, Percent

### Phase 2: CharacterPhysics 개선 ✅ (85%)
- [x] **점프 메커니즘 안정성** ⭐ (2025-10-04 완성)
  - 3가지 안전장치 추가 (접지 강제 리셋, 하강 감지, 키 릴리즈)
  - 좁은 공간 점프 상태 리셋 문제 해결
  - 천장 충돌 시 즉시 하강 전환

- [x] **충돌 감지 정확성** ⭐ (2025-10-04 완성)
  - Player 자신의 Collider를 Ground로 감지하는 버그 수정
  - OverlapBoxAll + 자기 자신 필터링 적용

- [x] **Layer 시스템 자동화** ⭐ (2025-10-04 완성)
  - Ground Layer 자동 설정 + 경고 시스템
  - Player Layer 자동 설정 + 충돌 감지

### Phase 2: Player-Combat 통합 ✅
- [x] **PlayerController 개선**
  - HealthSystem 통합
  - ComboSystem 통합
  - AttackAnimationHandler 통합
  - Combat 프로퍼티 추가 (IsAlive, IsAttacking)
  - InputHandler → PlayerInput 프로퍼티 변경 (명명 충돌 해결)

- [x] **PlayerAttackState 완전 재작성**
  - ComboSystem과 완전 연동
  - RegisterHit() 호출 통합
  - 콤보 데이터 기반 공격 실행
  - 애니메이션 연동

- [x] **입력 시스템 개선** ⭐ (2025-10-04 완성)
  - State 기반 입력 리셋 (Attack/Jump/Dash)
  - 반복 실행 가능한 입력 처리
  - 명명 충돌 해결 (InputHandler → PlayerInput)

### Phase 4: UI/UX ✅ (40%)
- [x] **HUD 시스템** (2025-10-03 완성)
  - HealthBarUI (보라색 게이지, lerp 애니메이션)
  - ItemSlotUI (쿨다운, 개수 표시)
  - ResourcePanel (골드/다이아)
  - PlayerInfoPanel (플레이어 정보 통합)
  - HUDManager (HUD 전체 관리)
  - HUD 프리팹 자동 생성 도구

### 기술 인프라 개선 ✅
- [x] **ResourceManager → GameResourceManager 리팩토링** ⭐ (2025-10-04)
  - System.Resources.ResourceManager와의 이름 충돌 방지
  - 6개 파일 참조 업데이트

- [x] **DictionaryInspectorHelper 개선** ⭐ (2025-10-04)
  - 내부 Dictionary 추가 (실제 Dictionary 기능 제공)
  - 래퍼 메서드 구현 (ContainsKey, Remove, indexer 등)
  - 자동 동기화 메커니즘

- [x] **Assembly Definition 최적화** ⭐ (2025-10-04)
  - Combat.Demo.asmdef에 Core.Utilities 참조 추가
  - 순환 참조 회피

### 데모 및 테스트 도구 ✅
- [x] **PlayerCombatDemo.cs** ⭐ (2025-10-04 개선)
  - 완전 자동 설정 (플레이어 + 적 3개)
  - Layer 자동 설정 + 경고 시스템
  - Config Override 자동 할당 (Reflection)
  - BoxCollider2D 요구사항 준수
  - 14단계 테스트 체크리스트

- [x] **문서 작성**
  - PLAYER_COMBAT_DEMO_TEST_CHECKLIST.md (2025-10-04)
  - COMBAT_DEMO_GUIDE.md
  - PLAYER_COMBAT_DEMO_GUIDE.md

---

## 🆕 최근 작업 (2025-10-04)

### 1. PlayerCombatDemo 테스트 체크리스트 생성 ⭐
**목적**: Combat 시스템 검증을 위한 체계적인 테스트 가이드

- 14단계 상세 테스트 절차
- Pass/Fail 판정 기준 명시
- 예상 결과 및 확인 방법 제공

**파일**: `Assets/_Project/Scripts/Tests/Demo/PLAYER_COMBAT_DEMO_TEST_CHECKLIST.md`

---

### 2. CharacterPhysics 물리 시스템 개선 ⭐⭐
**목적**: 정밀한 플랫포머 물리 구현 및 버그 수정

#### 좁은 공간 점프 문제 해결
**3가지 안전장치 추가** (CharacterPhysics.cs:114-130):
1. 접지 상태에서 hasJumped 강제 리셋
2. 하강 시작 시 isJumping 종료
3. 기존 점프 키 릴리즈 로직 유지

```csharp
// 안전장치 1: 접지 상태에서 점프 상태 강제 리셋
if (isGrounded && hasJumped && rb.linearVelocity.y <= 0.1f)
{
    hasJumped = false;
    isJumping = false;
}

// 안전장치 2: 하강 시작 시 isJumping 종료
if (isJumping && rb.linearVelocity.y <= 0)
{
    isJumping = false;
}
```

#### 자기 자신 충돌 제외
- CheckGroundCollision() 개선 (CharacterPhysics.cs:353-369)
- `Physics2D.OverlapBox` → `OverlapBoxAll` 변경
- 자기 자신 Collider 필터링: `if (hitCollider != col && hitCollider.gameObject != gameObject)`

**커밋**: `fe26c85`, `1a98e5e`, `a79f7f4`

---

### 3. 콤보 체인 시스템 구현 ⭐⭐
**목적**: 연속 공격 입력으로 1→2→3 콤보 체인 실현

#### PlayerAttackState 개선
**CheckComboInput() 메서드 추가** (PlayerAttackState.cs:107-132):
- 콤보 윈도우 내에서 다음 공격 입력 감지
- 콤보 인덱스 확인 및 Attack State 재진입
- 중복 입력 방지를 위한 입력 리셋

**커밋**: `85e4d4a`

---

### 4. ResourceManager → GameResourceManager 리팩토링 ⭐
**목적**: System.Resources.ResourceManager와의 이름 충돌 방지

- 클래스명 변경: `ResourceManager` → `GameResourceManager`
- 6개 파일 참조 업데이트

**커밋**: `0a42969`

---

### 5. DictionaryInspectorHelper 개선 ⭐
**목적**: GameResourceManager에서 Dictionary 메서드 사용 가능하도록 확장

- 내부 Dictionary 추가
- 래퍼 메서드 구현 (Count, ContainsKey, Add, Remove, TryGetValue, indexer)
- 자동 동기화 메커니즘

**커밋**: `c0d2cb8`

---

### 6. 입력 리셋 시스템 구현
**목적**: 공격/대시/점프 입력 반복 실행 가능하도록 수정

- PlayerAttackState.cs (ExitState:87): `ResetAttack()`
- PlayerJumpState.cs (ExitState:51): `ResetJump()`
- PlayerDashState.cs (ExitState:87): `ResetDash()`
- PlayerController 프로퍼티 변경: InputHandler → PlayerInput

**커밋**: `7c0f9f1`

---

### 7. 어셈블리 참조 최적화
**목적**: CS0012 컴파일 에러 해결

- Combat.Demo.asmdef에 `"Core.Utilities"` 참조 추가
- 순환 참조 회피 (Player ← Core.Managers)

**커밋**: `539ee2b`

---

### 8. PlayerCombatDemo 개선
**목적**: 테스트 환경 자동화 및 휴먼 에러 방지

- Ground Layer 자동 설정 + 경고 시스템
- Player Layer 자동 설정 + 충돌 감지
- CapsuleCollider2D → BoxCollider2D 변경
- SetupCharacterPhysicsConfig() 추가 (Reflection)

**커밋**: `fe26c85`, `1a98e5e`

---

## 🐛 수정된 버그 (2025-10-04)

### 1. 입력 반복 실행 불가 ✅
**문제**: Attack/Dash/Jump 버튼이 1회만 동작
**원인**: InputHandler가 입력 상태를 리셋하지 않음
**해결**: State OnExitState에서 ResetAttack/Dash/Jump 호출

### 2. PlayerController 프로퍼티 명명 혼동 ✅
**문제**: InputHandler 클래스명 = 프로퍼티명
**해결**: 프로퍼티명 변경 (InputHandler → PlayerInput)

### 3. 체력바 색상 미표시 ✅
**문제**: GUI.Box는 GUI.color를 무시
**해결**: GUI.DrawTexture + Texture2D.whiteTexture 사용

### 4. 점프 동작 안됨 ✅
**원인 1**: Ground Layer 미설정
**원인 2**: SkulPhysicsConfig 미로드
**해결**: Layer 자동 설정 + 경고 시스템 + Config 자동 할당

### 5. 자기 자신 충돌 감지 ✅
**문제**: Physics2D.OverlapBox가 Player의 Collider도 감지
**해결**: OverlapBoxAll + 자기 자신 필터링

### 6. Collider 타입 불일치 ✅
**문제**: CharacterPhysics는 BoxCollider2D 요구, PlayerCombatDemo는 CapsuleCollider2D 생성
**해결**: BoxCollider2D로 변경

### 7. 좁은 공간 점프 상태 리셋 안됨 ✅
**문제**: 천장 충돌 시 hasJumped/isJumping 리셋 안됨
**해결**: 3가지 안전장치 추가 (강제 리셋, 하강 감지, 키 릴리즈)

### 8. CS0012 컴파일 에러 ✅
**문제**: Combat.Demo.asmdef에 Core.Utilities 참조 누락
**해결**: Core.Utilities 참조 추가

### 9. 콤보 체인 안됨 ✅
**문제**: Attack State 종료 후 Idle로 전환되어 콤보 끊김
**해결**: CheckComboInput()으로 콤보 윈도우 내 입력 감지 및 재진입

---

## 📝 다음 작업 예정

### 🎯 우선순위 1: 테스트 완료
- [ ] PlayerCombatDemo 14단계 체크리스트 완전 실행
- [ ] 다양한 환경에서 점프 테스트 (좁은 공간, 경사면 등)
- [ ] 콤보 체인 3단계 모두 테스트
- [ ] 벽 점프 시스템 테스트 (WallJumpState 검증)
- [ ] 대시 쿨다운 시스템 테스트
- [ ] 히트 반응 시스템 테스트 (HitState, 넉백)

### 🎯 우선순위 2: Phase 2 마무리
- [ ] **애니메이션 통합**
  - 공격 애니메이션 클립 추가
  - Animator Controller 설정
  - AttackAnimationHandler와 연동 검증

- [ ] **히트박스 시각화**
  - Gizmos를 통한 히트박스 표시
  - 디버그 모드 개선

- [ ] **사운드 통합**
  - 공격 사운드 이펙트 추가
  - 타격 사운드 추가

- [ ] **VFX 추가**
  - 공격 이펙트 파티클
  - 타격 이펙트
  - 콤보 완료 이펙트

### 🎯 우선순위 3: Phase 3 - 적 AI 시스템
- [ ] **Enemy AI 기반 구조**
  - EnemyController
  - EnemyFSM
  - EnemyHealthSystem 통합

- [ ] **AI 행동 패턴**
  - Idle, Patrol, Chase, Attack
  - 감지 시스템 (시야, 청각)
  - 공격 패턴

- [ ] **적 종류별 구현**
  - 기본 적 (근접 공격)
  - 원거리 적
  - 보스 적

### 🎯 우선순위 4: Skull 시스템 통합
- [ ] **Skull + Combat 연동**
  - SkullThrowAbility 개선
  - 각 Skull별 고유 공격
  - Skull 전환 시 콤보 리셋

- [ ] **Skull별 어빌리티**
  - Warrior: 강력한 근접 공격
  - Mage: 원거리 마법 공격
  - Default: 균형잡힌 공격

---

## 🧪 테스트 방법

### PlayerCombatDemo 빠른 시작

#### 방법 1: 빈 씬에서 시작
1. **새 씬 생성**: `File > New Scene`
2. **빈 GameObject 생성**: Hierarchy > 우클릭 > Create Empty → 이름: "PlayerCombatDemo"
3. **스크립트 추가**: `PlayerCombatDemo` 컴포넌트 추가 → Play 모드 진입
4. **자동 설정**: 플레이어 + 적 3개 + 콤보 시스템 자동 설정

#### 조작 방법
| 키 | 동작 |
|---|---|
| **WASD** | 이동 |
| **Space** | 점프 |
| **LShift** | 대시 |
| **마우스 좌클릭** | 공격 (콤보) |
| **T** | 플레이어 체력 회복 |
| **Y** | 적 재생성 |
| **U** | 콤보 리셋 |
| **I** | 통계 초기화 |
| **R** | 씬 리셋 |
| **H** | 도움말 |

#### 콤보 시스템 테스트
1. 공격 버튼 클릭 → 1단 공격
2. 0.5초 내 재클릭 → 2단 공격
3. 0.5초 내 재클릭 → 3단 공격 (완료)

**콤보별 특성**:
- **1단**: 데미지 1.0x, 범위 1.5x1.0
- **2단**: 데미지 1.2x, 범위 1.8x1.0
- **3단**: 데미지 1.4x, 범위 2.1x1.0

---

## 📁 주요 파일 위치

### Core Systems
```
Assets/Plugins/GAS_Core/          # Gameplay Ability System
Assets/Plugins/FSM_Core/          # Finite State Machine
Assets/_Project/Scripts/Core/Enums/  # 공용 Enum (DamageType 등)
```

### Combat System
```
Assets/_Project/Scripts/Gameplay/Combat/
├── Core/                         # DamageSystem, HealthSystem, ComboSystem
├── Hitbox/                       # HitboxController
├── Attack/                       # BasicAttack, AttackAnimationHandler
└── Data/                         # ComboData
```

### Player System
```
Assets/_Project/Scripts/Gameplay/Player/
├── CharacterPhysics.cs           # 커스텀 물리 시스템
├── PlayerController.cs           # 메인 컨트롤러
├── InputHandler.cs               # 입력 처리
├── States/                       # FSM 상태들
│   ├── PlayerAttackState.cs      # 공격 상태 (Combat 통합)
│   ├── PlayerJumpState.cs        # 점프 상태
│   └── PlayerDashState.cs        # 대시 상태
└── Player.asmdef                 # Assembly Definition
```

### Demo & Tests
```
Assets/_Project/Scripts/Tests/
├── Demo/
│   ├── PlayerCombatDemo.cs                      # 플레이어 전투 데모
│   ├── PLAYER_COMBAT_DEMO_TEST_CHECKLIST.md    # 14단계 체크리스트
│   ├── PLAYER_COMBAT_DEMO_GUIDE.md
│   └── COMBAT_DEMO_GUIDE.md
└── Unit/Combat/                                 # 단위 테스트
```

### Documentation
```
docs/
├── getting-started/             # 시작 가이드
│   ├── QuickStart.md           # 5분 빠른 시작
│   ├── ProjectOverview.md      # 프로젝트 개요
│   ├── FolderStructure.md      # 폴더 구조
│   └── PlayerSetup.md          # 플레이어 설정
├── development/                # 개발 문서
│   ├── Roadmap.md              # 개발 로드맵
│   ├── CurrentStatus.md        # 현재 진행 상황 (이 문서)
│   ├── CodingGuidelines.md     # 코딩 규칙
│   └── SkulSystemDesign.md     # Skul 시스템 설계
├── testing/                    # 테스트 가이드
├── infrastructure/             # 인프라
└── archive/                    # 히스토리
    └── Worklog.md              # 작업 일지
```

---

## 🚀 다음 세션 시작 시 체크리스트

작업을 다시 시작할 때 이 체크리스트를 확인하세요:

- [ ] Unity 에디터 열기
- [ ] 자동 컴파일 완료 대기
- [ ] Console 창에서 에러 확인
- [ ] 이 문서 (CurrentStatus.md) 읽기
- [ ] PlayerCombatDemo 테스트 실행
- [ ] 발견된 버그 기록
- [ ] "다음 작업 예정" 섹션에서 우선순위 확인

---

## 📞 참고 문서

- **빠른 시작**: [QuickStart.md](../getting-started/QuickStart.md)
- **프로젝트 개요**: [ProjectOverview.md](../getting-started/ProjectOverview.md)
- **개발 로드맵**: [Roadmap.md](Roadmap.md)
- **코딩 가이드라인**: [CodingGuidelines.md](CodingGuidelines.md)
- **테스트 가이드**: [TestingGuide.md](../testing/TestingGuide.md)
- **작업 일지**: [Worklog.md](../archive/Worklog.md)

---

**작성일**: 2025-10-02
**최종 업데이트**: 2025-10-04
**작성자**: GASPT 개발팀 + Claude Code

---

## 💡 팁

### 빠른 디버깅
1. PlayerController의 `showDebugInfo` 활성화
2. ComboSystem의 이벤트 로그 확인
3. Gizmos 표시로 히트박스 시각화

### 성능 모니터링
- Unity Profiler 사용
- Stats 창에서 Draw Calls 확인
- 히트박스 생성/파괴 빈도 모니터링

### 코드 수정 시 주의사항
- Assembly Definition 참조 확인
- Awaitable 패턴 준수 (Coroutine 사용 금지)
- 한글 주석 인코딩 (UTF-8)
- CRLF 줄바꿈 사용
