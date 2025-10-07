# 🚀 GASPT 빠른 시작 가이드

> 5분 안에 GASPT 프로젝트를 실행하고 테스트하세요!

## 📋 사전 요구사항

- **Unity 2023.3 이상**
- **Git** 설치
- **Visual Studio 2022** 또는 **Rider**

---

## ⚡ 1단계: 프로젝트 열기 (1분)

### Unity Hub에서 프로젝트 추가
1. Unity Hub 열기
2. **Add** → **Add project from disk**
3. `D:\JaeChang\UintyDev\GASPT\GASPT` 폴더 선택
4. Unity 6000.2.6f2 버전으로 프로젝트 열기

### 초기 컴파일 대기
- 첫 실행 시 스크립트 자동 컴파일 (약 30초)
- Console 창에서 에러 확인 (에러 없어야 정상)

---

## 🎮 2단계: Combat 시스템 테스트 (2분)

### PlayerCombatDemo 실행

1. **새 씬 생성**
   ```
   File > New Scene
   ```

2. **빈 GameObject 생성**
   ```
   Hierarchy > 우클릭 > Create Empty
   이름: "CombatDemoRunner"
   ```

3. **스크립트 추가**
   - `PlayerCombatDemo` 컴포넌트 추가
   - Play 버튼 클릭 ▶️

4. **자동 설정 확인**
   - 플레이어 자동 생성 ✅
   - 적 3개 자동 생성 ✅
   - 바닥 플랫폼 자동 생성 ✅
   - 3단 콤보 자동 설정 ✅

### 조작 방법
| 키 | 동작 |
|---|---|
| **WASD** | 이동 |
| **Space** | 점프 |
| **LShift** | 대시 |
| **마우스 좌클릭** | 공격 (콤보) |
| **T** | 체력 회복 |
| **H** | 도움말 |

### 테스트 시나리오
1. ✅ **이동 테스트**: WASD로 플레이어 이동 확인
2. ✅ **점프 테스트**: Space로 점프 (좁은 공간 포함)
3. ✅ **대시 테스트**: LShift로 대시
4. ✅ **콤보 테스트**: 마우스 좌클릭 연타로 1→2→3단 콤보
5. ✅ **적 처치**: 적 3마리 모두 처치

---

## 📊 3단계: 현재 상태 확인 (1분)

### 프로젝트 진행 상황
- **Phase 1 (Core 시스템)**: ✅ 100% 완료
- **Phase 2 (Combat + Player)**: ✅ 84% 완료
  - CharacterPhysics: ✅ 85%
  - Combat System: ✅ 70%
- **Phase 4 (UI/UX)**: ✅ 40% 완료

### 주요 완성 기능
- ✅ GAS (Gameplay Ability System)
- ✅ FSM (Finite State Machine)
- ✅ Combat System (콤보 체인)
- ✅ CharacterPhysics (정밀한 점프)
- ✅ HUD System (체력바, 리소스)

---

## 📚 4단계: 문서 탐색 (1분)

### 필수 문서
1. **[프로젝트 개요](ProjectOverview.md)** - GASPT가 무엇인지
2. **[폴더 구조](FolderStructure.md)** - 파일이 어디에 있는지
3. **[개발 로드맵](../development/Roadmap.md)** - 앞으로 할 일
4. **[현재 상황](../development/CurrentStatus.md)** - 지금까지 한 일

### 개발 시작 시
1. **[코딩 가이드라인](../development/CodingGuidelines.md)** - 코딩 규칙
2. **[플레이어 설정](PlayerSetup.md)** - 플레이어 캐릭터 만들기
3. **[테스트 가이드](../testing/TestingGuide.md)** - 테스트 방법

---

## 🎯 다음 단계

### 개발자라면
1. [코딩 가이드라인](../development/CodingGuidelines.md) 숙지
2. [현재 진행 상황](../development/CurrentStatus.md) 확인
3. [개발 로드맵](../development/Roadmap.md)에서 다음 작업 선택

### 테스터라면
1. [테스트 가이드](../testing/TestingGuide.md) 확인
2. PlayerCombatDemo 전체 테스트 실행
3. 발견된 버그 리포트

### 기여자라면
1. [프로젝트 개요](ProjectOverview.md)로 시스템 이해
2. [Skul 시스템 설계](../development/SkulSystemDesign.md) 검토
3. 기여 가능한 영역 확인

---

## 🐛 문제 해결

### 컴파일 에러 발생 시
1. Unity 버전 확인 (2023.3 이상)
2. Console 창에서 에러 메시지 확인
3. [인코딩 가이드](../infrastructure/EncodingGuide.md) 참고

### 테스트 동작 안 함
1. PlayerCombatDemo가 올바르게 추가되었는지 확인
2. Play 모드 진입 후 자동 설정 대기 (약 1초)
3. Console 로그 확인

### Layer 설정 문제
- PlayerCombatDemo가 자동으로 설정하지만, 경고 메시지 확인
- Edit > Project Settings > Tags and Layers에서 "Ground", "Player" Layer 추가

---

## 💡 팁

### 빠른 디버깅
- **F12 키**: GUI 통계 표시/숨김
- **Console 로그**: `[PlayerCombatDemo]` 태그로 필터링
- **Gizmos**: Scene 뷰에서 히트박스 시각화 가능

### 성능 확인
- **Window > Analysis > Profiler** 열기
- Play 모드에서 FPS 확인 (60fps 목표)

### 추가 학습
- [작업 일지](../archive/Worklog.md)에서 최근 변경사항 확인
- [개발 로드맵](../development/Roadmap.md)에서 전체 계획 파악

---

**축하합니다! 🎉**
GASPT 프로젝트를 성공적으로 실행했습니다!

다음은 [프로젝트 개요](ProjectOverview.md)를 읽고 시스템을 더 깊이 이해해보세요.
