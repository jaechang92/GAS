# GASPT 프로젝트 컨텍스트

> **자동 로드**: 이 파일은 Claude Code가 매 세션 시작 시 자동으로 읽습니다.

---

## 🎯 작업 시작 전 필수 체크

### 1단계: Spec Kit 읽기 (MUST)
```bash
Read: .spec/workflows.yaml        # 작업 프로세스 (가장 중요!)
Read: .spec/coding-rules.yaml     # 코딩 규칙
Read: .spec/architecture.yaml     # 시스템 구조
```

### 2단계: 현재 상태 파악 (MUST)
```bash
Read: docs/development/CurrentStatus.md    # 최근 작업, 다음 할 일
```

### 3단계: 기존 패턴 파악
같은 타입의 클래스를 먼저 Read하고 패턴 따르기:
- Manager 작성 → 다른 Manager 클래스 읽기
- State 작성 → 같은 Entity의 State 읽기
- UI 작성 → 기존 UI 클래스 읽기

---

## ⚡ 핵심 규칙 (절대 지킬 것)

### 네이밍
- ✅ `camelCase` (private fields)
- ✅ `PascalCase` (methods, properties, classes)
- ❌ `snake_case` 금지
- ❌ `_underscore` 접두사 금지

### 비동기
- ✅ `async Awaitable`
- ✅ `await Awaitable.NextFrameAsync()`
- ❌ `IEnumerator` 절대 금지
- ❌ `yield return` 절대 금지

### Singleton
- ✅ `SingletonManager<T>` 상속
- ✅ `OnSingletonAwake()` 구현
- ❌ 수동 Singleton 구현 금지

### Unity API (2023+)
- ✅ `FindAnyObjectByType<T>()`
- ✅ `rb.linearVelocity`
- ❌ `FindObjectOfType<T>()` (deprecated)
- ❌ `rb.velocity` (deprecated)

---

## 🚫 자주 하는 실수 7가지

1. **기존 문서 확인 안 함**
   - 해결: `find docs -name "*.md" | grep -i [키워드]`

2. **기존 패턴 무시**
   - 해결: 같은 타입 클래스 먼저 Read

3. **코드 스타일 불일치**
   - 해결: 기존 코드와 변수명/메서드명 스타일 확인

4. **Deprecated API 사용**
   - 해결: `.spec/coding-rules.yaml#unity_api` 참조

5. **CurrentStatus.md 미업데이트**
   - 해결: 작업 완료 시 즉시 업데이트

6. **중복 작업**
   - 해결: Grep으로 기존 구현 확인 후 재사용

7. **영향 범위 미확인**
   - 해결: `Grep: "ClassName" pattern: "*.cs" output_mode: "files_with_matches"`

---

## 📋 작업 프로세스 (간단 버전)

### Manager 작성 시
```
1. 다른 Manager 클래스 Read
2. SingletonManager<T> 상속
3. OnSingletonAwake() 구현
4. CurrentStatus.md 업데이트
```

### State 작성 시
```
1. 같은 Entity의 다른 State Read
2. BaseState/GameState 상속
3. OnEnter/OnExit/OnUpdate 구현 (Awaitable)
4. FSM에 상태 등록
```

### 새 시스템 추가 시
```
1. 설계 문서 작성 (docs/development/)
2. .asmdef 생성 (필요시)
3. 순환 참조 확인
4. Demo/Test 스크립트 작성
5. CurrentStatus.md 업데이트
```

---

## 🗂️ 주요 파일 위치

### Manager 클래스
- 위치: `Assets/_Project/Scripts/Core/Managers/`
- 네이밍: `[Name]Manager.cs`
- 상속: `SingletonManager<T>`

### State 클래스
- 위치: `[Entity]/States/`
- 네이밍: `[Entity][Action]State.cs`
- 상속: `BaseState` or `GameState`

### Data 클래스
- 위치: `[System]/Data/`
- 네이밍: `[Type]Data.cs`
- 상속: `ScriptableObject`

---

## 📚 참고 문서 (우선순위)

1. ⭐⭐⭐ `.spec/workflows.yaml` - 작업 프로세스
2. ⭐⭐⭐ `docs/development/CurrentStatus.md` - 현재 상황
3. ⭐⭐ `.spec/coding-rules.yaml` - 코딩 규칙
4. ⭐⭐ `.spec/architecture.yaml` - 시스템 구조
5. ⭐ `.spec/file-structure.yaml` - 파일 배치

---

## 💡 빠른 명령어

### 문서 검색
```bash
find docs -name "*.md" | grep -i [키워드]
```

### 코드 검색
```bash
Grep: "[클래스명]" pattern: "*.cs" output_mode: "files_with_matches"
```

### 패턴 참조
```bash
Glob: "**/*Manager.cs"    # Manager 클래스들
Glob: "**/*State.cs"      # State 클래스들
```

---

## ✅ 성공 기준

- ✅ 중복 작업 없음
- ✅ 일관된 코드 스타일
- ✅ 기존 패턴 준수
- ✅ 문서 최신 상태
- ✅ 컴파일 에러 없음
- ✅ 실수 최소화

---

**프로젝트**: GASPT (Generic Ability System + FSM Platform Game)
**Unity 버전**: 2023.3+
**Phase**: 2 (Combat & Physics) - 85% 완료
**최종 업데이트**: 2025-10-12
