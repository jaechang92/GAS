# 종합 테스트 가이드

## 📌 개요
이전 작업들(Player/Enemy State 동기 전환, GameState 하이브리드 FSM, Combat 시스템)을 종합적으로 테스트하기 위한 가이드입니다.

## 🎯 테스트 목표
1. **Player State 시스템 검증** - 동기 방식 FSM 전환 확인
2. **Enemy State 시스템 검증** - 동기 방식 FSM 전환 확인
3. **GameFlow 시스템 검증** - 하이브리드 FSM 전환 확인
4. **Combat 시스템 검증** - 전투 및 데미지 시스템 확인

---

## 🚀 테스트 실행 방법

### 방법 1: 자동 테스트 러너 사용 (권장)

#### 1단계: TestScene 열기
```
Assets/_Project/Scenes/TestScene.unity
```

#### 2단계: ComprehensiveTestRunner 추가
1. Hierarchy에서 빈 GameObject 생성 (이름: `TestRunner`)
2. `ComprehensiveTestRunner` 컴포넌트 추가
3. Inspector에서 설정:
   - `Run On Start`: ✓ (체크)
   - `Test Interval`: 2.0

#### 3단계: Play 모드 실행
1. Unity 에디터에서 **Play** 버튼 클릭
2. Console 창에서 테스트 결과 확인

#### 4단계: 테스트 결과 확인
Console에서 다음과 같은 로그를 확인:
```
========================================
=== 종합 테스트 시작 ===
========================================

[1] Player State 시스템 테스트 시작
✓ [1] Player 초기 상태는 Idle - 성공
✓ [2] Player Attack 상태 전환 - 성공
✓ [3] Player Idle 상태 복귀 - 성공
[1] Player State 시스템 테스트 완료

[2] Enemy State 시스템 테스트 시작
✓ [6] Enemy 생성 성공 - 성공
[2] Enemy State 시스템 테스트 완료

[3] GameFlow 하이브리드 FSM 테스트 시작
✓ [11] GameFlowManager 생성 성공 - 성공
[3] GameFlow 하이브리드 FSM 테스트 완료

[4] Combat 시스템 통합 테스트 시작
✓ [16] Player HealthSystem 존재 - 성공
✓ [17] Enemy HealthSystem 존재 - 성공
✓ [18] Enemy 데미지 받음 - 성공
✓ [19] Player 데미지 받음 - 성공
✓ [20] Enemy 사망 처리 - 성공
[4] Combat 시스템 통합 테스트 완료

========================================
=== 테스트 결과 ===
총 테스트: 20
성공: 20
실패: 0
성공률: 100.0%
========================================
```

---

### 방법 2: 개별 테스트 실행 (Context Menu 사용)

#### 1단계: TestRunner 선택
Hierarchy에서 `TestRunner` GameObject 선택

#### 2단계: Context Menu에서 원하는 테스트 실행
Inspector 창에서 `ComprehensiveTestRunner` 컴포넌트 우클릭:
- `전체 테스트 실행`
- `Player 테스트만 실행`
- `Enemy 테스트만 실행`
- `GameFlow 테스트만 실행`
- `Combat 테스트만 실행`

---

### 방법 3: 기존 테스트 러너 사용

#### GameFlow 테스트
1. `GameFlowTestRunner` 컴포넌트 추가
2. Play 모드 실행
3. GameFlow 상태 전환 확인

#### Player 테스트
1. `PlayerControllerTestRunner` 컴포넌트 추가
2. Play 모드 실행
3. Player 상태 전환 확인

---

## 📊 테스트 체크리스트

### ✅ Player State 시스템
- [ ] Idle → Move 전환
- [ ] Move → Jump 전환
- [ ] Jump → Fall 전환
- [ ] Attack 상태 전환
- [ ] Dash 상태 전환
- [ ] Hit 상태 전환 (데미지 받을 때)
- [ ] Dead 상태 전환 (체력 0일 때)

### ✅ Enemy State 시스템
- [ ] Idle → Patrol 전환
- [ ] Patrol → Trace 전환 (플레이어 감지)
- [ ] Trace → Attack 전환 (공격 범위 진입)
- [ ] Attack → Trace 전환 (범위 이탈)
- [ ] Hit 상태 전환 (데미지 받을 때)
- [ ] Death 상태 전환 (체력 0일 때)

### ✅ GameFlow 시스템
- [ ] Preload → Main 전환 (초기 로딩)
- [ ] Main → Loading 전환 (게임 시작)
- [ ] Loading → Ingame 전환 (로딩 완료)
- [ ] Ingame → Pause 전환 (ESC 키)
- [ ] Pause → Ingame 전환 (Resume)
- [ ] Ingame → Menu 전환
- [ ] Menu → Ingame 전환

### ✅ Combat 시스템
- [ ] Player 체력 시스템 작동
- [ ] Enemy 체력 시스템 작동
- [ ] Player → Enemy 데미지 전달
- [ ] Enemy → Player 데미지 전달
- [ ] Hitbox/Hurtbox 충돌 감지
- [ ] 사망 처리 (Player/Enemy)

---

## 🔍 상세 테스트 시나리오

### 시나리오 1: Player 전투 테스트
1. **초기 상태 확인**
   - Player Idle 상태 확인
   - 체력 100% 확인

2. **공격 테스트**
   - Attack 키 입력 (기본: Mouse Left Click)
   - PlayerAttackState 진입 확인
   - 공격 애니메이션 재생 확인
   - Hitbox 활성화 확인

3. **피격 테스트**
   - Enemy 공격 받기
   - PlayerHitState 진입 확인
   - 체력 감소 확인
   - 넉백 효과 확인

### 시나리오 2: Enemy AI 테스트
1. **정찰 패턴 확인**
   - Enemy Idle 상태 시작
   - 일정 시간 후 Patrol 상태 전환
   - 정찰 범위 내 이동 확인

2. **플레이어 추적**
   - Player가 감지 범위 진입
   - EnemyTraceState 진입 확인
   - Player 방향으로 이동 확인

3. **공격 패턴**
   - 공격 범위 진입
   - EnemyAttackState 진입 확인
   - 공격 쿨다운 확인

### 시나리오 3: GameFlow 전환 테스트
1. **게임 시작 플로우**
   - Preload 상태에서 리소스 로딩
   - Main 메뉴 표시
   - 게임 시작 버튼 클릭
   - Loading 상태 진입
   - Ingame 상태 전환

2. **일시정지/재개**
   - Ingame에서 ESC 키
   - Pause 상태 진입
   - Time.timeScale = 0 확인
   - ESC 키로 Resume
   - Time.timeScale = 1 확인

---

## 🐛 예상 이슈 및 해결책

### 이슈 1: "PlayerController를 찾을 수 없습니다" 에러
**원인**: Player GameObject에 PlayerController 컴포넌트가 없음
**해결**: PlayerController 컴포넌트 자동 추가 확인

### 이슈 2: State 전환이 안 됨
**원인**: StateMachine 초기화 실패
**해결**:
1. PlayerController/EnemyController의 Awake() 호출 확인
2. InitializeFSM() 메서드 실행 확인
3. Console에서 초기화 로그 확인

### 이슈 3: Combat 시스템 작동 안 함
**원인**: HealthSystem 컴포넌트 누락
**해결**:
1. PlayerController에 HealthSystem 자동 추가 확인
2. EnemyController에 HealthSystem 자동 추가 확인

### 이슈 4: GameFlow 전환 실패
**원인**: GameFlowManager 초기화 실패
**해결**:
1. GameFlowManager GameObject 존재 확인
2. StateMachine 컴포넌트 확인
3. 초기 상태(Preload/Main) 확인

---

## 📝 테스트 결과 기록

### 테스트 날짜: ___________
### 테스터: ___________

| 테스트 항목 | 결과 (✓/✗) | 비고 |
|------------|-----------|------|
| Player Idle 상태 | | |
| Player Attack 상태 | | |
| Player Hit 상태 | | |
| Enemy Idle 상태 | | |
| Enemy Trace 상태 | | |
| Enemy Attack 상태 | | |
| GameFlow Main → Loading | | |
| GameFlow Loading → Ingame | | |
| GameFlow Pause/Resume | | |
| Combat Player → Enemy | | |
| Combat Enemy → Player | | |
| Combat Death 처리 | | |

---

## 🎓 참고 자료

### 관련 스크립트
- `PlayerController.cs` - Player FSM 관리
- `EnemyController.cs` - Enemy FSM 관리
- `GameFlowManager.cs` - GameFlow FSM 관리
- `ComprehensiveTestRunner.cs` - 종합 테스트 러너

### 관련 문서
- `Assets/FSM_Core/README.md` - FSM Core 시스템 설명
- `Assets/GAS_Core/README.md` - GAS Core 시스템 설명
- `ENCODING_GUIDE.md` - 한글 인코딩 가이드

---

## ❓ FAQ

**Q: 테스트가 자동으로 실행되지 않습니다.**
A: `Run On Start` 옵션이 체크되어 있는지 확인하세요.

**Q: Console에 로그가 너무 많습니다.**
A: Console 창에서 필터를 사용하여 특정 태그만 표시할 수 있습니다.

**Q: 특정 테스트만 실행하고 싶습니다.**
A: Context Menu를 사용하거나, 코드에서 해당 테스트 메서드만 호출하세요.

**Q: 테스트 결과를 파일로 저장하고 싶습니다.**
A: Unity Test Framework를 사용하거나, 별도의 로그 파일 작성 코드를 추가하세요.

---

## 📞 지원

문제가 발생하거나 질문이 있으면 다음을 확인하세요:
1. Unity Console 창의 에러 메시지
2. 각 컴포넌트의 초기화 로그
3. FSM 상태 전환 로그

**마지막 업데이트**: 2025-10-12
