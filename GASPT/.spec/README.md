# GASPT Spec Kit

> **Spec Kit**: GitHub 방식의 프로젝트 사양 정의 시스템
> **목적**: Claude Code AI가 프로젝트를 정확하게 이해하고 일관되게 작업하도록 돕는 구조화된 규칙 모음

---

## 📋 개요

Spec Kit은 YAML 형식으로 작성된 프로젝트 사양 문서 모음입니다. 이를 통해:
- ✅ 중복 작업 방지
- ✅ 일관된 코드 스타일 유지
- ✅ 기존 패턴 준수
- ✅ 명확한 프로젝트 컨텍스트 제공

---

## 📁 파일 구조

```
.spec/
├── README.md              # 이 파일 (Spec Kit 사용 가이드)
├── project.yaml           # 프로젝트 기본 정보
├── coding-rules.yaml      # 코딩 규칙 (네이밍, 패턴 등)
├── architecture.yaml      # 시스템 아키텍처
├── workflows.yaml         # AI 작업 프로세스
└── file-structure.yaml    # 파일 및 폴더 구조
```

---

## 📄 파일별 설명

### 1. `project.yaml`
**내용**: 프로젝트 기본 정보
- 프로젝트 이름, 버전, 설명
- Unity 버전, C# 버전
- 현재 Phase 및 완성도
- 개발 원칙
- 주요 문서 위치

**Claude가 알 수 있는 것**:
- 프로젝트가 무엇인지
- 현재 어느 단계인지
- 어떤 문서를 참조해야 하는지

---

### 2. `coding-rules.yaml`
**내용**: 코딩 규칙
- 네이밍 규칙 (camelCase, PascalCase)
- 비동기 패턴 (Awaitable 필수, Coroutine 금지)
- Singleton 패턴 (SingletonManager<T> 사용)
- Unity API 버전 (2023+, deprecated API 회피)
- Assembly Definition 규칙
- 금지 사항 및 권장 사항

**Claude가 알 수 있는 것**:
- 변수명을 어떻게 지어야 하는지
- 비동기 코드를 어떻게 작성해야 하는지
- 어떤 패턴을 사용하고 피해야 하는지

---

### 3. `architecture.yaml`
**내용**: 시스템 아키텍처
- 핵심 시스템 (GAS, FSM, GameFlow)
- 게임플레이 시스템 (Combat, Player, Enemy)
- 매니저 시스템 구조
- FSM 패턴
- 데이터 구조 (ScriptableObject)
- 이벤트 시스템
- Assembly 참조 관계

**Claude가 알 수 있는 것**:
- 프로젝트가 어떤 시스템으로 구성되어 있는지
- 각 시스템의 역할과 의존성
- Manager/State/Data 클래스 구조
- 순환 참조를 피하는 방법

---

### 4. `workflows.yaml`
**내용**: AI 작업 프로세스 (가장 중요!)
- 작업 시작 전 필수 체크리스트
- 코드 작성 프로세스
- 코드 작성 중 체크포인트
- 코드 작성 후 검증
- 문서 업데이트 가이드
- 자주 하는 실수 및 방지책
- 작업 시나리오별 가이드

**Claude가 알 수 있는 것**:
- 작업 시작 전에 무엇을 확인해야 하는지
- 코드 작성 중에 무엇을 체크해야 하는지
- 완료 후 무엇을 검증해야 하는지
- 자주 하는 실수와 해결책

---

### 5. `file-structure.yaml`
**내용**: 파일 및 폴더 구조
- 전체 폴더 구조
- Scripts 폴더 세부 구조
- 네이밍 규칙
- Assembly Definition 위치
- 파일 배치 규칙
- 파일 크기 제한
- 문서 구조

**Claude가 알 수 있는 것**:
- 새 파일을 어디에 만들어야 하는지
- 파일 이름을 어떻게 지어야 하는지
- 어떤 Assembly에 포함시켜야 하는지

---

## 🤖 Claude가 Spec Kit을 사용하는 방법

### 작업 시작 시

```yaml
# 1단계: 모든 Spec 파일 읽기
Read: .spec/project.yaml
Read: .spec/coding-rules.yaml
Read: .spec/architecture.yaml
Read: .spec/workflows.yaml
Read: .spec/file-structure.yaml

# 2단계: 현재 상태 파악
Read: docs/development/CurrentStatus.md

# 3단계: 작업 시작
```

### 코드 작성 시

```yaml
# workflows.yaml의 체크리스트를 따름
- 네이밍 규칙 확인 (coding-rules.yaml)
- 비동기 패턴 확인 (coding-rules.yaml)
- Singleton 패턴 확인 (architecture.yaml)
- 파일 위치 확인 (file-structure.yaml)
```

### 완료 후

```yaml
# 검증 및 문서 업데이트
- 영향 범위 확인
- 일관성 체크
- CurrentStatus.md 업데이트
```

---

## 📚 주요 참조 경로

### 중요도 순으로 읽어야 할 파일

1. **workflows.yaml** ⭐⭐⭐ (가장 중요!)
   - 작업 프로세스 전체 가이드
   - 실수 방지 체크리스트

2. **coding-rules.yaml** ⭐⭐⭐
   - 코드 작성 규칙
   - 금지 사항

3. **architecture.yaml** ⭐⭐
   - 시스템 구조 이해
   - 의존성 관계

4. **project.yaml** ⭐
   - 프로젝트 기본 정보
   - 현재 상태

5. **file-structure.yaml** ⭐
   - 파일 배치 규칙
   - 네이밍 규칙

---

## 🔄 업데이트 가이드

### Spec 파일을 언제 업데이트하는가?

#### `project.yaml`
- Phase 변경 시
- 주요 시스템 완성 시
- 프로젝트 정보 변경 시

#### `coding-rules.yaml`
- 새로운 코딩 규칙 추가 시
- 기존 규칙 변경 시
- 새로운 금지 사항 발견 시

#### `architecture.yaml`
- 새로운 시스템 추가 시
- 아키텍처 변경 시
- Assembly 참조 관계 변경 시

#### `workflows.yaml`
- 새로운 작업 프로세스 추가 시
- 자주 하는 실수 발견 시
- 작업 시나리오 추가 시

#### `file-structure.yaml`
- 폴더 구조 변경 시
- 새로운 파일 타입 추가 시
- 네이밍 규칙 변경 시

---

## ✅ Spec Kit의 장점

### 기존 방식 (Markdown)과 비교

| 항목 | Markdown 문서 | Spec Kit (YAML) |
|------|--------------|-----------------|
| 구조화 | 📝 텍스트 기반 | ✅ 키-값 구조 |
| 파싱 | 🐢 느림 (전체 읽기) | ⚡ 빠름 (섹션별) |
| 검색 | 🔍 수동 검색 | 🎯 키로 직접 접근 |
| 일관성 | ⚠️ 서술 방식 다양 | ✅ 일관된 형식 |
| 유지보수 | 📄 여러 문서 수정 | 📋 중앙 집중 관리 |

### 실제 사용 예시

**Before (Markdown):**
```
Claude가 여러 문서를 읽어야 함:
1. CodingGuidelines.md 읽기
2. ProjectOverview.md 읽기
3. FolderStructure.md 읽기
→ 시간 소요, 정보 놓칠 가능성
```

**After (Spec Kit):**
```yaml
# .spec/*.yaml 파일 읽기
→ 구조화된 정보를 빠르게 파악
→ 필요한 섹션만 참조 가능
→ 일관된 규칙 적용
```

---

## 🚀 시작하기

### Claude Code가 처음 사용할 때

```bash
# 1. 모든 Spec 파일 읽기
Read: .spec/project.yaml
Read: .spec/coding-rules.yaml
Read: .spec/architecture.yaml
Read: .spec/workflows.yaml
Read: .spec/file-structure.yaml

# 2. CurrentStatus.md 읽기
Read: docs/development/CurrentStatus.md

# 3. 작업 시작!
```

### 매 작업 세션마다

```bash
# 필수: workflows.yaml 참조
Read: .spec/workflows.yaml

# 선택: 필요한 규칙만 참조
Read: .spec/coding-rules.yaml#naming
Read: .spec/architecture.yaml#managers
```

---

## 📞 문서 참조

Spec Kit과 함께 사용하는 주요 문서:
- **docs/development/CurrentStatus.md** - 현재 작업 상황 (필수)
- **docs/development/CodingGuidelines.md** - 상세 코딩 가이드
- **docs/getting-started/ProjectOverview.md** - 프로젝트 개요

---

## 🎯 목표

Spec Kit을 통해:
- ✅ **일관성**: 모든 코드가 동일한 패턴 따름
- ✅ **효율성**: 빠른 컨텍스트 파악
- ✅ **정확성**: 실수 최소화
- ✅ **유지보수성**: 중앙 집중 관리

---

**버전**: 1.0
**작성일**: 2025-10-12
**작성자**: GASPT Development Team
