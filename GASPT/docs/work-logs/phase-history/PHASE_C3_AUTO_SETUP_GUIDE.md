# Phase C-3 자동 셋업 가이드

**작성 날짜**: 2025-11-17
**목적**: Phase C-3 던전 진행 UI를 1클릭으로 자동 생성

---

## 📋 개요

Phase C-3에서 필요한 모든 UI 프리팹을 **자동으로 생성**하는 Editor Tool입니다.

### 생성되는 에셋

1. **PortalUI.prefab** - E키 안내 UI
   - Canvas + Panel + Text
   - "E 키를 눌러 다음 방으로 이동" 메시지

2. **DungeonCompleteUI.prefab** - 던전 클리어 UI
   - Canvas + Panel + Title + Reward Text + Buttons
   - "던전 클리어!" 타이틀
   - 총 획득 골드/경험치 표시
   - "다음 던전", "메인 메뉴" 버튼

3. **Scene 연결**
   - RoomManager에 DungeonCompleteUI 연결
   - 모든 Portal에 PortalUI 연결

---

## 🚀 사용 방법 (1클릭 자동 생성)

### 1단계: Editor Tool 열기

```
Unity 메뉴 → Tools → GASPT → Phase C-3 Setup Creator
```

### 2단계: 전체 자동 생성

```
1. "🚀 모든 UI 자동 생성" 버튼 클릭
2. 잠시 대기 (1~2초)
3. "Phase C-3 Setup 완료!" 다이얼로그 확인
4. "확인" 클릭
```

**완료!** 이제 모든 UI 프리팹이 생성되었습니다.

### 3단계: Scene 연결 (GameplayScene 열기)

```
1. GameplayScene 열기
2. Tools → GASPT → Phase C-3 Setup Creator
3. "3. Scene 연결" 버튼 클릭
```

**완료!** 모든 연결이 자동으로 설정되었습니다.

---

## 📁 생성 위치

| 에셋 | 경로 |
|------|------|
| PortalUI.prefab | `Assets/Resources/Prefabs/UI/PortalUI.prefab` |
| DungeonCompleteUI.prefab | `Assets/Resources/Prefabs/UI/DungeonCompleteUI.prefab` |

---

## 🔍 생성된 UI 구조

### PortalUI.prefab

```
PortalUI (Canvas)
├─ PortalUI (Component)
└─ Panel (Image)
   └─ Text
      - "E 키를 눌러 다음 방으로 이동"
      - 폰트 크기: 24
      - 정렬: 중앙
      - 색상: 흰색
```

**설정**:
- Canvas Render Mode: Screen Space - Overlay
- Panel 위치: 하단 중앙 (Y: 100)
- Panel 크기: 400x80
- 배경 색상: 검정 (투명도 70%)

### DungeonCompleteUI.prefab

```
DungeonCompleteUI (Canvas)
├─ DungeonCompleteUI (Component)
└─ Panel (Image, 전체 화면)
   ├─ TitleText
   │  - "던전 클리어!"
   │  - 폰트 크기: 48
   │  - 색상: 노란색
   │
   ├─ RewardText
   │  - 총 획득 골드/경험치
   │  - 폰트 크기: 24
   │  - 색상: 흰색
   │
   ├─ NextDungeonButton
   │  - "다음 던전"
   │  - 크기: 200x50
   │  - 색상: 파란색
   │
   └─ MainMenuButton
      - "메인 메뉴"
      - 크기: 200x50
      - 색상: 파란색
```

**설정**:
- Canvas Render Mode: Screen Space - Overlay
- Panel 배경: 검정 (투명도 90%)
- 시간 정지: 활성화 (Time.timeScale = 0)

---

## ⚙️ 개별 생성 (선택사항)

전체 자동 생성 대신 개별로 생성할 수도 있습니다.

### 1. PortalUI만 생성

```
Tools → GASPT → Phase C-3 Setup Creator
→ "1. PortalUI.prefab 생성" 버튼 클릭
```

### 2. DungeonCompleteUI만 생성

```
Tools → GASPT → Phase C-3 Setup Creator
→ "2. DungeonCompleteUI.prefab 생성" 버튼 클릭
```

### 3. Scene 연결만 실행

```
GameplayScene 열기
→ Tools → GASPT → Phase C-3 Setup Creator
→ "3. Scene 연결" 버튼 클릭
```

---

## 🧪 테스트 방법

### 1. PortalUI 테스트

```
1. GameplayScene 실행
2. 적 모두 처치 (방 클리어)
3. 포탈 근처로 이동
4. PortalUI가 표시되는지 확인
   - "E 키를 눌러 다음 방으로 이동" 메시지
5. E키 입력
6. 다음 방으로 이동되는지 확인
```

### 2. DungeonCompleteUI 테스트

```
1. GameplayScene 실행
2. 모든 방 클리어 (마지막 방까지)
3. DungeonCompleteUI가 표시되는지 확인
   - "던전 클리어!" 타이틀
   - 총 획득 골드/경험치 표시
   - 버튼 2개 (다음 던전, 메인 메뉴)
4. 시간이 정지되었는지 확인 (Time.timeScale = 0)
5. "다음 던전" 버튼 클릭
6. 씬이 재시작되는지 확인
```

---

## 🎨 UI 커스터마이징

생성된 프리팹을 직접 수정할 수 있습니다.

### PortalUI 커스터마이징

```
1. Resources/Prefabs/UI/PortalUI.prefab 더블클릭
2. Panel 위치/크기 조정
3. Text 폰트/색상 변경
4. 저장
```

### DungeonCompleteUI 커스터마이징

```
1. Resources/Prefabs/UI/DungeonCompleteUI.prefab 더블클릭
2. Title/Reward Text 스타일 변경
3. Button 위치/색상 조정
4. 배경 Panel 투명도 조정
5. 저장
```

---

## ⚠️ 주의사항

### 1. Scene 연결은 GameplayScene 필요

Scene 연결 기능은 **GameplayScene이 열려 있어야** 작동합니다.

```
❌ 틀린 순서:
1. 새 Scene 열기
2. Scene 연결 실행 → RoomManager, Portal을 찾을 수 없음

✅ 올바른 순서:
1. GameplayScene 열기
2. Scene 연결 실행 → 정상 작동
```

### 2. 기존 프리팹 덮어쓰기

이미 PortalUI.prefab이나 DungeonCompleteUI.prefab이 있으면 **덮어씁니다**.

```
⚠️ 커스터마이징한 UI가 있다면:
1. 백업 후 실행
2. 또는 개별 생성 버튼으로 선택적 생성
```

### 3. UI 폰트 문제

기본 폰트(LegacyRuntime.ttf)가 표시되지 않으면:

```
해결 방법:
1. 생성된 프리팹 열기
2. Text 컴포넌트 선택
3. Font 필드에 원하는 폰트 할당
4. 저장
```

---

## 🔧 수동 연결 (자동 실패 시)

자동 연결이 실패하면 수동으로 연결할 수 있습니다.

### Portal에 PortalUI 연결

```
1. Hierarchy에서 Portal 선택
2. Inspector → Portal 컴포넌트
3. Portal UI 필드에 PortalUI 할당
   - Scene의 PortalUI 드래그
4. 모든 Portal에 반복
```

### RoomManager에 DungeonCompleteUI 연결

```
1. Hierarchy에서 RoomManager 선택
2. Inspector → Room Manager 컴포넌트
3. Dungeon Complete UI 필드에 DungeonCompleteUI 할당
   - Scene의 DungeonCompleteUI 드래그
```

---

## 📊 시간 절약 효과

| 작업 | 수동 시간 | 자동 시간 | 절약 |
|------|-----------|----------|------|
| PortalUI 생성 | ~5분 | 1초 | 99% |
| DungeonCompleteUI 생성 | ~10분 | 1초 | 99% |
| Scene 연결 | ~3분 | 1초 | 99% |
| **총합** | **~18분** | **3초** | **99%** |

---

## 🎯 완료 체크리스트

Phase C-3 셋업이 완료되었는지 확인하세요.

### 프리팹 생성 확인
- [ ] `Resources/Prefabs/UI/PortalUI.prefab` 존재
- [ ] `Resources/Prefabs/UI/DungeonCompleteUI.prefab` 존재

### Scene 연결 확인
- [ ] GameplayScene 열림
- [ ] RoomManager → Dungeon Complete UI 할당됨
- [ ] Portal들 → Portal UI 할당됨

### 기능 테스트 확인
- [ ] 방 클리어 시 포탈 활성화
- [ ] 포탈 근처에서 PortalUI 표시
- [ ] E키로 다음 방 이동
- [ ] 던전 완주 시 DungeonCompleteUI 표시
- [ ] 시간 정지 작동
- [ ] 버튼 클릭 시 씬 재시작

---

## 📝 다음 단계

Phase C-3 셋업 완료 후:

1. **테스트**: 전체 플레이 시나리오 검증
2. **밸런싱**: 보상 수치 조정
3. **커밋**: Git 커밋 및 푸시
4. **Phase C-4**: 아이템 드롭 및 장착 구현

---

**작성자**: Claude Code
**최종 수정**: 2025-11-17
