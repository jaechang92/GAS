# 🎮 GASPT 프로젝트 현재 진행 상황

**최종 업데이트**: 2025-10-02
**현재 Phase**: Phase 2 - Combat 시스템 + Player 통합

---

## 📋 목차
- [완료된 작업](#완료된-작업)
- [수정된 버그](#수정된-버그)
- [다음 작업 예정](#다음-작업-예정)
- [알려진 제약사항](#알려진-제약사항)
- [테스트 방법](#테스트-방법)

---

## ✅ 완료된 작업

### Phase 1: Core 시스템 구축 ✅
- [x] GAS (Gameplay Ability System) Core 구현
- [x] FSM (Finite State Machine) Core 구현
- [x] GameFlow 시스템 (Main/Loading/Ingame/Pause/Menu/Lobby)
- [x] 한글 인코딩 문제 해결 (.gitattributes, .editorconfig)

### Phase 2: Combat 시스템 ✅
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

- [x] **Damage Types (Core.Enums)**
  - Physical, Magical, True
  - Fire, Ice, Lightning, Poison, Dark, Holy
  - Environmental, Percent

### Phase 2: Player-Combat 통합 ✅
- [x] **PlayerController 개선**
  - HealthSystem 통합
  - ComboSystem 통합
  - AttackAnimationHandler 통합
  - Combat 프로퍼티 추가 (IsAlive, IsAttacking)

- [x] **PlayerAttackState 완전 재작성**
  - ComboSystem과 완전 연동
  - RegisterHit() 호출 통합
  - 콤보 데이터 기반 공격 실행
  - 애니메이션 연동

- [x] **FSM 전환 규칙 추가**
  - Attack → Attack 전환 (콤보 공격용)

### Assembly Definition 구조 완성 ✅
- [x] **Core**
  - Core.Enums.asmdef

- [x] **Plugins**
  - GAS.Core.asmdef
  - FSM.Core.asmdef
  - FSM.Core.Editor.asmdef

- [x] **Combat**
  - Combat.Core.asmdef
  - Combat.Hitbox.asmdef
  - Combat.Attack.asmdef

- [x] **Player**
  - Player.asmdef (NEW)

- [x] **Tests**
  - Combat.Tests.Unit.asmdef
  - Combat.Demo.asmdef

### 데모 및 테스트 도구 ✅
- [x] **CombatDemoScene.cs**
  - 키보드 단축키로 테스트 가능
  - 실시간 통계 표시

- [x] **PlayerCombatDemo.cs**
  - 완전 자동 설정 (플레이어 + 적 3개)
  - 3단 콤보 자동 구성
  - 실시간 이벤트 로그
  - GUI 통계 및 제어

- [x] **문서 작성**
  - COMBAT_DEMO_GUIDE.md
  - PLAYER_COMBAT_DEMO_GUIDE.md

---

## 🐛 수정된 버그

### 1. ComboSystem.RegisterHit() 중복 호출 문제 ✅
**문제**:
- PlayerAttackState와 BasicAttack에서 각각 RegisterHit() 호출
- 콤보가 2배 빠르게 진행 (1타 → 3타 스킵)

**수정**:
- BasicAttack.cs에서 RegisterHit() 제거
- PlayerAttackState에서만 콤보 관리

**커밋**: `22fa261`

---

### 2. 콤보 공격 FSM 전환 규칙 누락 ✅
**문제**:
- Attack → Attack 전환 규칙이 없음
- 공격 중 재공격 입력이 무시됨
- 콤보 시스템 작동 불가

**수정**:
- PlayerController.cs SetupTransitions()에 추가
```csharp
stateMachine.AddTransition("Attack", "Attack", "AttackPressed");
```

**커밋**: `22fa261`

---

### 3. ComboSystem 논리 오류 (이전 수정) ✅
**문제**:
- StartCombo()에서 currentComboIndex = 0으로 초기화
- 첫 콤보가 2번 실행됨

**수정**:
- StartCombo()에서 인덱스 초기화 제거
- 항상 AdvanceCombo() 호출로 진행

**커밋**: `98f1a2c` (이전 세션)

---

### 4. Assembly Definition 참조 문제 ✅

#### 4-1. HitboxController Core.Enums 참조 에러
**수정**: Combat.Hitbox.asmdef, Combat.Attack.asmdef에 Core.Enums 추가

#### 4-2. Player Assembly Definition 누락
**수정**: Player.asmdef 생성

#### 4-3. Unity.InputSystem 중복 참조
**수정**: GUID 참조 제거, 이름 참조만 유지

**커밋**: `3470210`

#### 4-4. FSM.Core Assembly Definition 누락
**문제**: FSM_Core 폴더는 존재하지만 .asmdef 없음

**수정**:
- FSM.Core.asmdef 생성
- FSM.Core.Editor.asmdef 생성

**커밋**: `6f487e5`

---

## 📝 다음 작업 예정

### 🎯 우선순위 1: Unity 에디터 테스트 (즉시 진행 가능)
- [ ] **PlayerCombatDemo 실전 테스트**
  - 빈 씬에서 GameObject + PlayerCombatDemo 추가
  - Play 모드 진입 → 자동 설정 확인
  - 콤보 공격 테스트 (마우스 좌클릭 연타)
  - 버그 발견 시 리포트

- [ ] **테스트 시나리오 실행**
  - 기본 전투 (이동 + 공격)
  - 3단 콤보 진행 (0.5초 내 연타)
  - 콤보 리셋 (1초 대기 or U키)
  - 적 처치 및 통계 확인

- [ ] **발견된 문제 수정**
  - 물리 엔진 충돌 문제
  - 애니메이션 누락 처리
  - 히트박스 위치/크기 조정

### 🎯 우선순위 2: Phase 2 마무리 작업
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

### 🎯 우선순위 3: Phase 3 - 적 AI 시스템 (다음 단계)
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

### 🎯 우선순위 4: Skull 시스템 통합 (Phase 4)
- [ ] **Skull + Combat 연동**
  - SkullThrowAbility 개선
  - 각 Skull별 고유 공격
  - Skull 전환 시 콤보 리셋

- [ ] **Skull별 어빌리티**
  - Warrior: 강력한 근접 공격
  - Mage: 원거리 마법 공격
  - Default: 균형잡힌 공격

### 🎯 우선순위 5: 시스템 최적화 및 확장
- [ ] **성능 최적화**
  - Object Pooling (히트박스, 이펙트)
  - Coroutine → Awaitable 전환 완료

- [ ] **데이터 관리**
  - ScriptableObject로 콤보 데이터 관리
  - 적 데이터 ScriptableObject화

- [ ] **UI 시스템**
  - 체력바 UI
  - 콤보 카운터 UI
  - 스킬 쿨타임 UI

---

## ⚠️ 알려진 제약사항

### 1. 애니메이션 시스템
- **현재 상태**: AttackAnimationHandler 구현 완료
- **제약**: 실제 애니메이션 클립이 없음
- **대응**: 애니메이션 없이도 로직은 작동, 클립 추가 시 바로 적용 가능

### 2. 물리 충돌 감지
- **현재 상태**: 히트박스 생성 및 Collider2D 설정 완료
- **제약**: 2D/3D 혼용 시 충돌 감지 문제 가능
- **대응**: 프로젝트가 2D인지 3D인지 명확히 해야 함

### 3. Input System
- **현재 상태**: Legacy Input (Input.GetMouseButtonDown) 사용
- **제약**: New Input System으로 전환 필요
- **대응**: InputHandler에서 Input System 통합 예정

### 4. 테스트 환경
- **현재 상태**: EditMode 테스트는 통과
- **제약**: Unity 에디터 PlayMode 테스트 미실시
- **대응**: 다음 단계에서 실전 테스트 진행

---

## 🧪 테스트 방법

### PlayerCombatDemo 빠른 시작

#### 방법 1: 빈 씬에서 시작
1. **새 씬 생성**
   ```
   File > New Scene
   ```

2. **빈 GameObject 생성**
   ```
   Hierarchy > 우클릭 > Create Empty
   이름: "PlayerCombatDemo"
   ```

3. **스크립트 추가**
   - `PlayerCombatDemo` 컴포넌트 추가
   - Play 모드 진입

4. **자동 설정**
   - 플레이어 자동 생성
   - 적 3개 자동 생성
   - 콤보 시스템 자동 설정

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

**콤보별 특성:**
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
├── PlayerController.cs           # 메인 컨트롤러
├── InputHandler.cs               # 입력 처리
├── States/                       # FSM 상태들
│   ├── PlayerAttackState.cs      # 공격 상태 (Combat 통합)
│   └── ...
└── Player.asmdef                 # Assembly Definition
```

### Demo & Tests
```
Assets/_Project/Scripts/Tests/
├── Demo/
│   ├── PlayerCombatDemo.cs       # 플레이어 전투 데모
│   ├── CombatDemoScene.cs        # Combat 데모
│   ├── PLAYER_COMBAT_DEMO_GUIDE.md
│   └── COMBAT_DEMO_GUIDE.md
└── Unit/Combat/                  # 단위 테스트
```

### Documentation
```
GASPT_DEVELOPMENT_ROADMAP.md      # 전체 개발 로드맵
CURRENT_PROGRESS.md               # 현재 진행 상황 (이 문서)
ENCODING_GUIDE.md                 # 한글 인코딩 가이드
```

---

## 🚀 다음 세션 시작 시 체크리스트

작업을 다시 시작할 때 이 체크리스트를 확인하세요:

- [ ] Unity 에디터 열기
- [ ] 자동 컴파일 완료 대기
- [ ] Console 창에서 에러 확인
- [ ] 이 문서 (CURRENT_PROGRESS.md) 읽기
- [ ] PlayerCombatDemo 테스트 실행
- [ ] 발견된 버그 기록
- [ ] "다음 작업 예정" 섹션에서 우선순위 확인

---

## 📞 참고 문서

- **Combat 데모 가이드**: `Assets/_Project/Scripts/Tests/Demo/COMBAT_DEMO_GUIDE.md`
- **Player Combat 데모 가이드**: `Assets/_Project/Scripts/Tests/Demo/PLAYER_COMBAT_DEMO_GUIDE.md`
- **전체 로드맵**: `GASPT_DEVELOPMENT_ROADMAP.md`

---

**작성일**: 2025-10-02
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
