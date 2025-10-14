# 전체 게임 플레이 데모 가이드

## 🎮 개요
**Preload → Main → Loading → Ingame** 전체 게임 흐름과 **캐릭터 이동 + 전투**를 체험할 수 있는 통합 데모입니다.

---

## 🚀 빠른 시작

### 방법 1: 전체 게임 흐름 체험 (권장)

#### 1단계: 새 씬 생성 또는 TestScene 사용
```
Assets/_Project/Scenes/TestScene.unity
```

#### 2단계: FullGamePlayDemo 추가
1. Hierarchy에서 빈 GameObject 생성 (이름: `GameDemo`)
2. `FullGamePlayDemo` 컴포넌트 추가
3. Inspector에서 설정:
   - **Auto Start**: ✓ (체크)
   - **Skip To Ingame**: ☐ (체크 해제) ← 전체 흐름 보기
   - **Use Resource Manifest**: ☐ (체크 해제) ← 매니페스트 없이 테스트
   - **Create Manifests If Missing**: ✓ (체크) ← 매니페스트 자동 생성
   - **Create Ground**: ✓ (체크)

#### 3단계: Play 버튼 클릭
Unity 에디터에서 **Play** 버튼 클릭하면 자동으로 실행됩니다.

#### 4단계: 게임 진행
1. **Preload 화면** (2초) → 자동으로 Main으로 전환
2. **Main Menu** 표시 → "게임 시작" 버튼 클릭
3. **Loading 화면** (진행률 표시) → 자동으로 Ingame으로 전환
4. **Ingame** → 캐릭터 조작 및 전투!

---

### 방법 2: 바로 Ingame으로 시작 (빠른 테스트)

#### 설정 변경:
- **Skip To Ingame**: ✓ (체크)

이렇게 하면 Main Menu, Loading을 건너뛰고 바로 게임 플레이가 시작됩니다.

---

### 방법 3: 리소스 매니페스트 사용 테스트

#### 설정 변경:
- **Use Resource Manifest**: ✓ (체크)
- **Create Manifests If Missing**: ✓ (체크)

이렇게 하면:
1. 게임 시작 시 자동으로 매니페스트 생성
2. Preload/Loading 상태에서 실제 리소스 로딩 시도
3. 로딩 진행률이 실제로 표시됨

**주의**: 리소스 매니페스트를 사용하려면 `Assets/_Project/Resources/` 폴더에 실제 리소스가 있어야 합니다.

---

## 🕹️ 플레이어 조작법

### 키보드
- **W / A / S / D**: 이동
- **Space**: 점프
- **Shift**: 대시
- **마우스 좌클릭**: 공격
- **ESC**: 일시정지 (Pause)

### 조작 팁
- **점프 + 대시**: 공중 대시로 멀리 이동 가능
- **공격 콤보**: 연속으로 공격 입력 시 콤보 공격
- **벽 점프**: 벽에 닿았을 때 점프로 벽 점프 가능

---

## 🎯 게임 목표

### 기본 목표
1. **Enemy들을 처치하라**: 2마리의 Enemy가 등장합니다
2. **생존하라**: Player 체력이 0이 되면 게임 오버

### Enemy 행동 패턴
- **Idle**: 대기 상태
- **Patrol**: 정찰 (일정 범위 내 왔다갔다)
- **Trace**: 플레이어 발견 시 추적
- **Attack**: 공격 범위 진입 시 공격

---

## 📊 화면 UI 설명

### Main Menu (메인 메뉴)
```
┌─────────────────────────────┐
│       GASPT DEMO            │
│                             │
│     [ 게임 시작 ]           │
│                             │
│   플레이어 조작:            │
│   WASD - 이동               │
│   Space - 점프              │
│   Shift - 대시              │
│   좌클릭 - 공격             │
└─────────────────────────────┘
```

### Loading Screen (로딩 화면)
```
┌─────────────────────────────┐
│      로딩 중...             │
│                             │
│   ████████░░░░  75%         │
│                             │
└─────────────────────────────┘
```

### Ingame HUD (인게임 UI)
```
┌─────────────────────────────┐
│ Player HP: 85 / 100         │
│ Enemy1 HP: 60 / 100         │
│ Enemy2 HP: 100 / 100        │
│                             │
│         [게임 화면]         │
│                             │
│ 조작: WASD-이동 | Space...  │
└─────────────────────────────┘
```

우측 상단에 현재 GameState가 표시됩니다:
- `GameState: Preload`
- `GameState: Main`
- `GameState: Loading`
- `GameState: Ingame`
- `GameState: Pause`

---

## 🎬 게임 흐름 상세

### 1. Preload 상태 (약 2초)
- **목적**: Essential 및 MainMenu 리소스 로딩
- **화면**: 로딩 화면 (진행률 표시)
- **자동 전환**: Main 상태로

### 2. Main 상태
- **목적**: 메인 메뉴 표시
- **화면**: Main Menu UI
- **사용자 액션**: "게임 시작" 버튼 클릭
- **전환**: Loading 상태로

### 3. Loading 상태 (약 5-6초)
- **목적**: Gameplay 리소스 로딩
- **화면**: 로딩 화면 (진행률 표시)
- **자동 전환**: Ingame 상태로

### 4. Ingame 상태
- **목적**: 실제 게임플레이
- **화면**: 인게임 HUD + 게임 화면
- **캐릭터**: Player, Enemy1, Enemy2 생성
- **전환**: ESC 키로 Pause 가능

### 5. Pause 상태
- **목적**: 게임 일시정지
- **화면**: Pause UI (반투명)
- **효과**: Time.timeScale = 0 (시간 정지)
- **전환**: ESC 키로 Resume (Ingame으로 복귀)

---

## 🌍 게임 환경

### 지형
- **지면**: 가로 30m, 세로 2m의 플랫폼
- **위치**: (0, -1, 0)
- **색상**: 회색

### 캐릭터 배치
- **Player**: (-8, 2, 0) - 왼쪽에서 시작
- **Enemy1**: (5, 2, 0) - 중앙 오른쪽
- **Enemy2**: (10, 2, 0) - 오른쪽

### 카메라
- **위치**: (0, 2, -10)
- **크기**: Orthographic Size 8
- **배경색**: 어두운 파란색

---

## ⚙️ 커스터마이징

### Inspector 설정

#### 데모 설정
- **Auto Start**: 자동 시작 여부
- **Skip To Ingame**: Main/Loading 건너뛰고 바로 Ingame 시작

#### 리소스 설정
- **Use Resource Manifest**: 리소스 매니페스트 사용 여부
  - ☐ (미사용): 리소스 로딩 없이 진행 (경고만 출력)
  - ✓ (사용): 실제 리소스 로딩 시도
- **Create Manifests If Missing**: 매니페스트 자동 생성 여부
  - ✓ (체크): 매니페스트가 없으면 자동 생성
  - ☐ (해제): 기존 매니페스트만 사용

#### 캐릭터 설정
- **Player Spawn Position**: Player 생성 위치
- **Enemy1 Spawn Position**: Enemy1 생성 위치
- **Enemy2 Spawn Position**: Enemy2 생성 위치

#### Enemy 데이터
- **Enemy Data**: EnemyData ScriptableObject 할당 가능
  - 없으면 기본 데이터로 자동 생성됨

#### 환경 설정
- **Create Ground**: 지면 자동 생성 여부
- **Ground Position**: 지면 위치
- **Ground Size**: 지면 크기

---

## 🐛 문제 해결

### Q: Player가 생성되지 않습니다.
**A**: Ingame 상태로 전환되면 자동으로 생성됩니다. Console에서 `[캐릭터] Player 생성 완료` 로그를 확인하세요.

### Q: Enemy가 Player를 공격하지 않습니다.
**A**:
1. Enemy의 감지 범위(detectionRange)를 확인하세요.
2. Player에게 가까이 가면 자동으로 Trace → Attack 상태로 전환됩니다.
3. Console에서 `[EnemyController]` 로그를 확인하세요.

### Q: 조작이 되지 않습니다.
**A**:
1. InputHandler가 제대로 초기화되었는지 확인하세요.
2. Console에서 에러 메시지를 확인하세요.
3. Pause 상태가 아닌지 확인하세요 (Time.timeScale = 0).

### Q: 로딩이 너무 오래 걸립니다.
**A**:
1. ResourceManager가 실제 리소스를 로드하지 않으면 바로 완료됩니다.
2. Skip To Ingame 옵션을 사용하여 로딩을 건너뛸 수 있습니다.

### Q: 캐릭터가 땅을 뚫고 떨어집니다.
**A**:
1. Create Ground가 체크되어 있는지 확인하세요.
2. Ground의 BoxCollider2D가 제대로 설정되었는지 확인하세요.
3. Rigidbody2D의 Collision Detection을 Continuous로 설정하세요.

---

## 🎓 학습 포인트

이 데모를 통해 확인할 수 있는 시스템:

### 1. GameFlow FSM (하이브리드)
- ✅ Preload → Main → Loading → Ingame 자동 전환
- ✅ 비동기 리소스 로딩
- ✅ Pause/Resume 기능

### 2. Player State FSM (동기)
- ✅ Idle, Move, Jump, Fall, Dash, Attack 상태
- ✅ 입력 기반 상태 전환
- ✅ 물리 기반 이동

### 3. Enemy State FSM (동기)
- ✅ Idle, Patrol, Trace, Attack 상태
- ✅ AI 기반 자동 상태 전환
- ✅ 플레이어 감지 및 추적

### 4. Combat 시스템
- ✅ HealthSystem (체력 관리)
- ✅ DamageSystem (데미지 전달)
- ✅ Hit/Death 상태 처리

### 5. UI 시스템
- ✅ 상태별 UI 자동 전환
- ✅ 실시간 체력 표시
- ✅ 로딩 진행률 표시

---

## 📝 코드 구조

### FullGamePlayDemo.cs
```
- SetupEnvironment()      : 환경 설정 (지면, 카메라)
- SetupUI()               : UI 생성 및 설정
- StartFullGameFlow()     : GameFlow 시작
- CreateCharacters()      : Player, Enemy 생성
- UpdateHealthUI()        : 체력 UI 실시간 업데이트
```

---

## 🎮 다음 단계

### 추가할 수 있는 기능
1. **더 많은 Enemy**: Enemy 수 증가
2. **다양한 Enemy 타입**: 원거리 공격, 보스 등
3. **레벨 디자인**: 플랫폼, 장애물 추가
4. **파워업/아이템**: 체력 회복, 공격력 증가 등
5. **사운드/이펙트**: 공격 소리, 타격 이펙트
6. **스코어/스테이지**: 점수 시스템, 다음 스테이지 전환

---

## 📞 문의 및 지원

문제가 발생하면:
1. Unity Console 창에서 에러/경고 메시지 확인
2. `[GameFlow]`, `[캐릭터]`, `[UI]`, `[리소스]` 태그로 로그 필터링
3. FullGamePlayDemo의 Context Menu 사용:
   - `데모 시작`: 수동으로 데모 재시작
   - `캐릭터만 생성`: 캐릭터만 다시 생성
   - `리소스 매니페스트 생성`: 매니페스트 수동 생성

---

## 🎉 즐기세요!

**Preload → Main → Loading → Ingame** 전체 흐름과 **캐릭터 이동 + 전투**를 즐겁게 체험하세요!

Enemy를 모두 처치하고 생존하는 것이 목표입니다. 행운을 빕니다! 🎮

---

**마지막 업데이트**: 2025-10-12
**작성자**: Claude Code
