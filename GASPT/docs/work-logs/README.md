# 📚 GASPT 작업 로그

> 프로젝트의 모든 작업 히스토리를 시간순/주제별로 정리한 문서입니다.

**최근 업데이트**: 2025-12-01

---

## 🎯 세션별 작업 로그

세 개의 독립적인 세션에서 작업이 진행됩니다:

| 세션 | 설명 | 최신 작업 |
|------|------|----------|
| 🔧 **[프로그래밍](programming/LATEST.md)** | 코드 작성, 시스템 구현, 리팩토링 | [LATEST.md](programming/LATEST.md) |
| 🎨 **[기획/아트](design/LATEST.md)** | 명세서 작성, 아트 프롬프트, 콘텐츠 기획 | [LATEST.md](design/LATEST.md) |
| 📁 **[공통/인프라](common/LATEST.md)** | 문서 정리, 설정 변경, 프로젝트 구조 | [LATEST.md](common/LATEST.md) |

---

## 📁 폴더 구조

```
work-logs/
├── README.md                   # 이 파일 - 인덱스
├── programming/                # 프로그래밍 세션
│   └── LATEST.md               # 프로그래밍 최신 작업
├── design/                     # 기획/아트 세션
│   └── LATEST.md               # 기획/아트 최신 작업
├── common/                     # 공통/인프라 세션
│   └── LATEST.md               # 문서/설정/구조 작업
├── file-inventory.md           # 생성된 파일 목록
├── 2025-11/                    # 월별 상세 로그
└── phase-history/              # Phase별 히스토리
```

---

## 📊 프로젝트 현황 요약

### 시스템 진행률
| 시스템 | 진행률 | 담당 세션 |
|--------|--------|-----------|
| 코어 시스템 | 100% | 프로그래밍 |
| 카메라 시스템 | 95% | 프로그래밍 |
| UI 시스템 | 90% | 프로그래밍 |
| 전투 시스템 | 80% | 프로그래밍 |
| 던전 시스템 | 85% | 프로그래밍 |
| 폼 시스템 | 30% | 양쪽 협업 |
| 아트 기획 | 100% | 기획/아트 |
| 아트 생성 | 0% | 기획/아트 |

---

## 📅 월별 작업 로그

### 2025년

| 월 | 주요 작업 | 문서 |
|----|----------|------|
| **11월** | MVP 패턴 통합, 절차적 던전 생성, 아트 에셋 기획 | [2025-11/](2025-11/) |
| **10월** | Phase C 완료, 인벤토리 시스템, 아이템 드롭 | *(아직 미생성)* |

---

## 🗂️ Phase별 작업 히스토리

| Phase | 주제 | 상태 | 문서 |
|-------|------|------|------|
| **Phase D** | UI 시스템 MVP 패턴 통합 | ✅ 완료 | [phase-D.md](phase-history/phase-D.md) |
| **Phase C** | 던전 진행 및 아이템 시스템 | ✅ 완료 | [phase-C.md](phase-history/phase-C.md) |
| **Phase B** | 에디터 도구 및 UI 통합 | ✅ 완료 | [phase-B.md](phase-history/phase-B.md) |
| **Phase A** | Form 시스템 및 Enemy AI | ✅ 완료 | [phase-A.md](phase-history/phase-A.md) |

---

## 🔍 빠른 검색

### 작업별 찾기
- **절차적 던전 생성** → [2025-11/procedural-dungeon-art-planning.md](2025-11/procedural-dungeon-art-planning.md)
- **MVP 패턴** → [2025-11/mvp-pattern-integration.md](2025-11/mvp-pattern-integration.md)
- **폼 컨텐츠 기획** → [specs/019-form-content-design/](../../specs/019-form-content-design/)
- **폼 교체 시스템** → [specs/017-form-swap-system/](../../specs/017-form-swap-system/)

### 시스템별 찾기
- **던전 생성 시스템** → 프로그래밍 세션
- **폼 시스템** → 양쪽 세션 (기획: design, 구현: programming)
- **UI 시스템** → 프로그래밍 세션
- **아트 에셋** → 기획/아트 세션

---

## 📖 업데이트 방법

각 세션에서 `/update-worklog` 명령어 사용:

```bash
# 프로그래밍 세션 (_programming 폴더)
/update-worklog              # programming/LATEST.md 업데이트

# 기획/아트 세션 (_design 폴더)
/update-worklog              # design/LATEST.md 업데이트

# 공통/인프라 세션 (그 외 폴더 또는 docs/설정 변경 시)
/update-worklog              # common/LATEST.md 업데이트
```

---

## 🔗 관련 문서

- [프로젝트 아키텍처](../architecture/PROJECT_ARCHITECTURE.md)
- [개발 로드맵](../development/PROJECT_MASTER_ROADMAP.md)
- [코딩 가이드라인](../development/CodingGuidelines.md)
- [파일 목록](file-inventory.md)

---

*최종 업데이트: 2025-12-01*
