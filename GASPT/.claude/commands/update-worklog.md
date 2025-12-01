# 작업 로그 업데이트

작업 로그를 자동으로 업데이트합니다.

---

## 🔍 세션 감지

현재 작업 디렉토리와 변경 파일을 기준으로 세션을 자동 감지합니다:

| 작업 디렉토리 | 세션 | 업데이트 파일 |
|---------------|------|---------------|
| `_programming` 포함 | 프로그래밍 | `docs/work-logs/programming/LATEST.md` |
| `_design` 포함 | 기획/아트 | `docs/work-logs/design/LATEST.md` |
| 그 외 (docs, 설정, 구조 변경) | 공통/인프라 | `docs/work-logs/common/LATEST.md` |

**자동 판별 기준**:
- `.cs` 파일 변경 → programming
- `specs/`, `docs/art/` 변경 → design
- `docs/`, `.claude/`, 설정 파일 변경 → common

---

## 📋 수행 작업

### 1. 세션 확인
- 현재 작업 디렉토리 확인
- `_programming` 또는 `_design` 키워드로 세션 판별

### 2. 최근 커밋 확인
- `git log -5 --oneline` 실행
- `git diff HEAD~1 --stat` 실행하여 변경된 파일 확인

### 3. 세션별 LATEST.md 업데이트

**프로그래밍 세션**: `docs/work-logs/programming/LATEST.md`
**기획/아트 세션**: `docs/work-logs/design/LATEST.md`
**공통/인프라 세션**: `docs/work-logs/common/LATEST.md`

**업데이트 내용**:
- 날짜를 오늘 날짜로 변경
- 브랜치를 현재 브랜치로 변경
- "현재 진행 중" 섹션에 최신 작업 추가
  - 작업명: 최근 커밋 메시지에서 추출
  - 날짜: 커밋 날짜
  - 주요 변경사항: 변경된 파일 기반
- 이전 "현재 진행 중" 작업은 "최근 완료" 섹션으로 이동 (최대 3개 유지)

### 4. 월별 상세 로그 생성 (옵션)
**조건**: 중요한 작업 완료 시 (커밋 메시지에 "기능:", "리팩토링:", "수정:" 포함)

**프로그래밍 경로**: `docs/work-logs/programming/YYYY-MM/`
**기획/아트 경로**: `docs/work-logs/design/YYYY-MM/`
**공통/인프라 경로**: `docs/work-logs/common/YYYY-MM/`

**파일명**: 커밋 메시지 기반 kebab-case (예: `dungeon-system-refactor.md`)

### 5. README.md 갱신
**파일**: `docs/work-logs/README.md`

**업데이트 내용**:
- "최근 업데이트" 날짜 갱신

---

## 🎯 옵션 처리

### `--full`
전체 업데이트 수행:
- 해당 세션 LATEST.md 업데이트
- 월별 상세 로그 생성 (강제)
- README.md 갱신

### `--phase {phase-name}`
Phase 완료 업데이트:
- 해당 Phase 히스토리 파일 업데이트
- file-inventory.md 갱신 (생성된 파일 목록)

### 옵션 없음 (기본)
빠른 업데이트:
- 해당 세션 LATEST.md만 업데이트
- README.md 날짜만 갱신

---

## ✅ 완료 후 출력

업데이트 완료 후 다음 정보를 출력:

```
✅ 작업 로그 업데이트 완료!

📂 세션: 프로그래밍 (_programming)

📝 변경된 파일:
- docs/work-logs/programming/LATEST.md
- docs/work-logs/README.md

📌 최신 작업:
- 던전 생성 시스템 리팩토링 (2025-11-30)

🎯 다음 작업 제안:
- [ ] Unity 컴파일 확인
```

---

## 📖 사용 예시

### 프로그래밍 세션에서
```bash
# _programming 폴더에서 실행
/update-worklog
# → docs/work-logs/programming/LATEST.md 업데이트
```

### 기획/아트 세션에서
```bash
# _design 폴더에서 실행
/update-worklog
# → docs/work-logs/design/LATEST.md 업데이트
```

### 공통/인프라 세션에서
```bash
# 프로젝트 루트 또는 docs 폴더에서 실행
/update-worklog
# → docs/work-logs/common/LATEST.md 업데이트
```

### 전체 업데이트
```
/update-worklog --full
```

---

## 🔧 주의사항

1. **세션 자동 감지**: 작업 디렉토리와 변경 파일로 판별
   - `_programming` → programming
   - `_design` → design
   - 그 외 (docs, 설정 변경) → common
2. **날짜 형식**: YYYY-MM-DD 형식 사용
3. **한글 인코딩**: UTF-8 사용
4. **파일명**: kebab-case 사용 (소문자, 하이픈)
5. **월별 폴더**: 존재하지 않으면 자동 생성

---

**이 명령은 세션별로 독립적인 작업 로그를 관리하여 프로젝트 히스토리를 추적하기 쉽게 만듭니다.**
