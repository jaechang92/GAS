# GASPT 프로젝트 컨텍스트

> **자동 로드**: 이 파일은 Claude Code가 매 세션 시작 시 자동으로 읽습니다.

---

## 🎯 작업 시작 전 필수 체크 (CRITICAL)

### 1단계: Constitution 읽기 (MUST)
```bash
Read: .specify/memory/constitution.md
```
**프로젝트 헌법 - 모든 규칙과 워크플로우가 통합되어 있습니다**

### 2단계: 현재 상태 파악 (MUST)
```bash
Read: docs/development/CurrentStatus.md
```
최근 작업, 다음 할 일, 수정된 버그 확인

---

## ⚡ 핵심 규칙 (빠른 참조)

### 네이밍
- ✅ `camelCase` (private fields, NO underscores)
- ✅ `PascalCase` (methods, properties, classes)
- ❌ `snake_case`, `_underscore` 접두사 금지

### 비동기
- ✅ `async Awaitable`
- ❌ `IEnumerator`, `yield return` 절대 금지

### Singleton
- ✅ `SingletonManager<T>` 상속
- ✅ `OnSingletonAwake()` 구현
- ❌ 수동 Singleton 구현 금지

### Unity API (2023+)
- ✅ `FindAnyObjectByType<T>()`, `rb.linearVelocity`
- ❌ `FindObjectOfType<T>()` (deprecated), `rb.velocity` (deprecated)

---

## 📚 상세 문서 참조

모든 상세 규칙, 워크플로우, 파일 구조는 **Constitution**에 통합되어 있습니다:

- **Core Principles** (9가지 핵심 원칙)
- **Detailed Coding Standards** (상세 코딩 규칙)
- **Development Workflow** (AI 에이전트 작업 프로세스)
- **File Structure Guidelines** (파일 구조 및 배치 규칙)
- **Code Review Requirements** (코드 리뷰 체크리스트)

**Constitution 위치**: `.specify/memory/constitution.md`

---

## 🗂️ 주요 파일 위치 (빠른 참조)

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

## 💡 빠른 명령어

### 문서 검색
```bash
find docs -name '*.md' | grep -i [키워드]
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

## 🚫 자주 하는 실수 (즉시 확인!)

1. **Constitution 확인 안 함** → Constitution 먼저 읽기
2. **CurrentStatus.md 확인 안 함** → 중복 작업 방지
3. **기존 패턴 무시** → 같은 타입 클래스 먼저 Read
4. **Deprecated API 사용** → Constitution Principle IX 참조
5. **영향 범위 미확인** → Grep으로 참조 파일 찾기

---

## ✅ 성공 기준

- ✅ Constitution 준수
- ✅ 일관된 코드 스타일
- ✅ 기존 패턴 준수
- ✅ CurrentStatus.md 업데이트
- ✅ 컴파일 에러 없음

---

**프로젝트**: GASPT (Generic Ability System + FSM Platform Game)
**Unity 버전**: 2023.3+
**Phase**: 2 (Combat & Physics) - 85% 완료
**Constitution 버전**: 1.1.0
**최종 업데이트**: 2025-10-25

---

## 📖 추가 참조

- **Constitution**: `.specify/memory/constitution.md` - 전체 규칙과 워크플로우
- **CurrentStatus**: `docs/development/CurrentStatus.md` - 현재 진행 상황
- **아키텍처**: `docs/architecture/` - 시스템 아키텍처 문서
- **Global Settings**: `C:\Users\JaeChang\.claude\CLAUDE.md` - 전역 설정
