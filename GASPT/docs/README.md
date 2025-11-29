# 📚 GASPT 프로젝트 문서

> GASPT 프로젝트의 모든 문서를 한곳에서 탐색하세요

---

## 🗺️ 문서 맵

### 🎓 시작하기 (getting-started/)
> 프로젝트를 처음 접하는 분들을 위한 필수 가이드

| 문서 | 설명 | 예상 시간 |
|------|------|----------|
| **[빠른 시작](getting-started/QuickStart.md)** | 5분 만에 GASPT 실행하기 | 5분 |
| **[프로젝트 개요](getting-started/ProjectOverview.md)** | GASPT가 무엇인지, 어떤 기능이 있는지 | 10분 |
| **[폴더 구조](getting-started/FolderStructure.md)** | 파일이 어디에 있는지, 어떻게 구성되었는지 | 5분 |
| **[플레이어 설정](getting-started/PlayerSetup.md)** | 플레이어 캐릭터 만드는 방법 | 10분 |

**추천 학습 순서**: QuickStart → ProjectOverview → FolderStructure → PlayerSetup

---

### 💻 개발 문서 (development/)
> 프로젝트 개발에 참여하는 분들을 위한 문서

| 문서 | 설명 | 대상 |
|------|------|------|
| **[코딩 가이드라인](development/CodingGuidelines.md)** | 코딩 규칙 및 금지 사항 | 모든 개발자 |
| **[개발 로드맵](development/PROJECT_MASTER_ROADMAP.md)** | 전체 개발 계획 및 마일스톤 | 프로젝트 매니저 |
| **[Skul 시스템 설계](development/SkulSystemDesign.md)** | Skul 스타일 구현 마스터플랜 | 시스템 설계자 |

**필수 문서**:
- 개발 시작 전: **CodingGuidelines** ⚠️
- 기획 검토 시: **SkulSystemDesign** 📋

---

### 📝 작업 로그 (work-logs/) ⭐ NEW!
> 프로젝트의 모든 작업 히스토리 및 진행 상황

| 문서 | 설명 | 업데이트 |
|------|------|----------|
| **[작업 로그 홈](work-logs/README.md)** | 작업 로그 네비게이션 | - |
| **[최신 작업](work-logs/LATEST.md)** | 현재 진행 중인 작업 및 최근 완료 작업 | 수시 |
| **[월별 로그](work-logs/2025-11/)** | 월별 상세 작업 로그 | 작업 완료 시 |
| **[Phase 히스토리](work-logs/phase-history/)** | Phase별 개발 히스토리 | Phase 완료 시 |
| **[파일 목록](work-logs/file-inventory.md)** | 생성된 모든 파일 목록 | Phase 완료 시 |

**작업 로그 업데이트 방법**:
```bash
/update-worklog          # 기본 업데이트
/update-worklog --full   # 전체 업데이트
/update-worklog --phase  # Phase 완료 업데이트
```

---

### 🏗️ 아키텍처 (architecture/)
> 프로젝트 구조 및 설계 문서

| 문서 | 설명 | 대상 |
|------|------|------|
| **[프로젝트 아키텍처](architecture/PROJECT_ARCHITECTURE.md)** | 전체 시스템 구조 및 5-Layer 설계 | 시스템 아키텍트 |
| **[어셈블리 아키텍처](architecture/ASSEMBLY_ARCHITECTURE.md)** | Assembly Definition 의존성 구조 | 시스템 개발자 |
| **[어셈블리 검증 리포트](architecture/ASSEMBLY_VALIDATION_REPORT.md)** | 의존성 검증 결과 | 시스템 개발자 |
| **[아키텍처 다이어그램](architecture/ARCHITECTURE_DIAGRAMS.md)** | 시각화 자료 및 다이어그램 | 모든 개발자 |
| **[카메라 시스템 설계](architecture/CAMERA_SYSTEM_DESIGN.md)** | 카메라 아키텍처 및 Post-Processing | 그래픽스/연출 |
| **[MVP 아키텍처](architecture/MVP_ARCHITECTURE.md)** | UI MVP 패턴 설계 | UI 개발자 |

**활용 시나리오**:
- 새 시스템 설계 시: **PROJECT_ARCHITECTURE** 📐
- 의존성 확인 시: **ASSEMBLY_ARCHITECTURE** 🔗
- 구조 이해 시: **ARCHITECTURE_DIAGRAMS** 📊
- 카메라/연출 시: **CAMERA_SYSTEM_DESIGN** 🎥

---

### 📖 가이드 (guides/)
> 기능별 사용 가이드 및 튜토리얼

| 문서 | 설명 | 대상 |
|------|------|------|
| **[FSM-GAS 통합](guides/FSM_GAS_INTEGRATION_SUMMARY.md)** | FSM과 GAS 연동 방법 | 게임플레이 개발자 |
| **[UI 시스템 설계](guides/UI_SYSTEM_DESIGN.md)** | Panel 기반 UI 구조와 사용법 | UI 개발자 |
| **[게임 플로우 가이드](guides/FULL_GAME_FLOW_GUIDE.md)** | 게임 흐름 및 상태 관리 | 게임플레이 개발자 |
| **[게임 데모 가이드](guides/FULL_GAME_DEMO_GUIDE.md)** | 전체 게임 데모 실행 방법 | 테스터 |
| **[Prefab 생성 가이드](guides/PREFAB_CREATION_GUIDE.md)** | Prefab 제작 및 설정 방법 | 콘텐츠 제작자 |
| **[Prefab Maker 사용법](guides/PREFAB_MAKER_USAGE.md)** | 자동 Prefab 생성 도구 | 콘텐츠 제작자 |

**추천 순서**:
- UI 개발: **UI_SYSTEM_DESIGN** → **FULL_GAME_FLOW_GUIDE**
- 게임플레이: **FSM_GAS_INTEGRATION_SUMMARY** → **FULL_GAME_FLOW_GUIDE**
- 콘텐츠 제작: **PREFAB_CREATION_GUIDE** → **PREFAB_MAKER_USAGE**

---

### 📝 레퍼런스 (reference/)
> 빠른 참조 자료

| 문서 | 설명 | 대상 |
|------|------|------|
| **[빠른 참조](reference/QUICK_REFERENCE.md)** | 자주 사용하는 기능 및 API 모음 | 모든 개발자 |

**활용법**:
- 개발 중 빠른 API 찾기
- 코드 스니펫 참조
- 일반적인 패턴 확인

---

### 🎨 아트 리소스 (art/)
> 캐릭터 및 에셋 디자인 가이드

| 문서 | 설명 | 대상 |
|------|------|------|
| **[NPC 캐릭터 디자인 프롬프트](art/NPC_CHARACTER_DESIGN_PROMPTS.md)** | namobanana AI용 NPC 디자인 프롬프트 | 아트 제작자 |

**활용법**:
- AI 이미지 생성 시 프롬프트 참조
- 컬러 팔레트 및 스타일 가이드 확인
- Unity 스프라이트 설정 참조

---

### 🧪 테스트 문서 (testing/)
> 테스트 및 QA를 위한 가이드

| 문서 | 설명 | 대상 |
|------|------|------|
| **[테스트 가이드](testing/TESTING_GUIDE.md)** | 전체 테스트 방법 및 절차 | 테스터, QA |
| **[테스트 리포트](testing/TEST_REPORT.md)** | 테스트 결과 및 이슈 | 테스터, QA |
| **[레거시 테스트 가이드](testing/TestingGuide.md)** | 이전 테스트 문서 (참고용) | 참고 |

**테스트 범위**:
- ✅ 단위 테스트 (Unit Tests)
- ✅ 통합 테스트 (Integration Tests)
- ✅ 인게임 테스트 (Gameplay Tests)
- ✅ 성능 테스트 (Performance Tests)

---

### 🔧 인프라 문서 (infrastructure/)
> 개발 환경 설정 및 문제 해결

| 문서 | 설명 | 사용 시기 |
|------|------|----------|
| **[인코딩 가이드](infrastructure/EncodingGuide.md)** | 한글 인코딩 문제 해결 및 방지 | 한글 주석 깨질 때 |

**일반적인 문제**:
- 한글 주석이 깨짐 → EncodingGuide 참조
- Git 설정 문제 → EncodingGuide > Git 설정 섹션
- 에디터 설정 → EncodingGuide > 에디터 설정 섹션

---

### 📦 작업 히스토리 (archive/)
> 과거 작업 기록 및 변경 이력

| 문서 | 설명 | 업데이트 주기 |
|------|------|---------------|
| **[작업 일지](archive/Worklog.md)** | 일별 작업 기록 및 상세 로그 | 매일 |

**참고 사항**:
- 최신 작업 내용은 [CurrentStatus.md](development/CurrentStatus.md) 참조
- 일별 상세 로그는 [Worklog.md](archive/Worklog.md) 참조

---

## 🎯 시나리오별 문서 가이드

### 🆕 신규 개발자
```
1. QuickStart.md - 5분 만에 실행
2. ProjectOverview.md - 프로젝트 이해
3. CodingGuidelines.md - 코딩 규칙 숙지
4. work-logs/LATEST.md - 현재 상황 파악
5. 작업 시작!
```

### 🔄 기존 개발자 (재시작)
```
1. work-logs/LATEST.md - 최신 상황 확인
2. work-logs/README.md - 작업 로그 확인
3. 작업 시작!
```

### 🧪 테스터
```
1. QuickStart.md - 환경 설정
2. TestingGuide.md - 테스트 방법 숙지
3. PlayerSetup.md - 필요 시 참조
4. 테스트 시작!
```

### 🎨 기여자 (신규)
```
1. ProjectOverview.md - 프로젝트 이해
2. SkulSystemDesign.md - 시스템 설계 이해
3. Roadmap.md - 개발 계획 파악
4. CodingGuidelines.md - 코딩 규칙 숙지
5. 기여 영역 선택 및 시작!
```

---

## 📖 문서 작성 규칙

### 문서 위치
- **시작 가이드**: `getting-started/` - 신규 사용자 대상
- **개발 문서**: `development/` - 개발자 대상
- **아키텍처**: `architecture/` - 시스템 구조 문서
- **가이드**: `guides/` - 기능별 사용 가이드
- **레퍼런스**: `reference/` - 빠른 참조 자료
- **테스트 문서**: `testing/` - 테스터 대상
- **인프라 문서**: `infrastructure/` - 환경 설정 관련
- **히스토리**: `archive/` - 과거 기록

### 문서 형식
- **Markdown** 사용 (.md)
- **제목**: `# 제목 (H1)`로 시작
- **이모지**: 적절히 사용 (🎮 ✅ 🔧 등)
- **코드 블록**: ` ```csharp` 사용

### 문서 업데이트
- **work-logs/LATEST.md**: `/update-worklog` 명령으로 자동 업데이트
- **work-logs/월별 로그**: `/update-worklog --full` 명령으로 생성
- **work-logs/Phase 히스토리**: `/update-worklog --phase` 명령으로 생성
- **기타 문서**: 관련 작업 완료 시 수동 업데이트

---

## 🔍 빠른 검색

### 자주 찾는 문서
- **지금 뭘 해야 하나요?** → [LATEST.md](work-logs/LATEST.md) ⭐
- **어떻게 시작하나요?** → [QuickStart.md](getting-started/QuickStart.md)
- **코딩 규칙이 뭐죠?** → [CodingGuidelines.md](development/CodingGuidelines.md)
- **작업 히스토리는?** → [work-logs/](work-logs/README.md) ⭐
- **아키텍처가 궁금해요** → [PROJECT_ARCHITECTURE.md](architecture/PROJECT_ARCHITECTURE.md)
- **UI 어떻게 만드나요?** → [UI_SYSTEM_DESIGN.md](guides/UI_SYSTEM_DESIGN.md)
- **한글이 깨져요!** → [EncodingGuide.md](infrastructure/EncodingGuide.md)
- **테스트 어떻게 하나요?** → [TESTING_GUIDE.md](testing/TESTING_GUIDE.md)
- **전체 계획이 궁금해요** → [PROJECT_MASTER_ROADMAP.md](development/PROJECT_MASTER_ROADMAP.md)

### 키워드 검색
- **GAS** → FSM_GAS_INTEGRATION_SUMMARY.md, ProjectOverview.md
- **FSM** → FSM_GAS_INTEGRATION_SUMMARY.md, FULL_GAME_FLOW_GUIDE.md
- **UI** → UI_SYSTEM_DESIGN.md, QUICK_REFERENCE.md
- **아키텍처** → PROJECT_ARCHITECTURE.md, ASSEMBLY_ARCHITECTURE.md
- **카메라** → CAMERA_SYSTEM_DESIGN.md
- **Post-Processing** → CAMERA_SYSTEM_DESIGN.md
- **리팩토링** → REFACTORING_PLAN.md, SCENE_REFACTORING_NOTES.md
- **Combat** → CURRENT_WORK.md, Roadmap.md
- **테스트** → TESTING_GUIDE.md, TEST_REPORT.md
- **인코딩** → EncodingGuide.md

---

## 📊 문서 통계

### 문서 개수
- **시작 가이드**: 5개
- **개발 문서**: 3개
- **작업 로그**: 8개
- **아키텍처**: 8개 (카메라 시스템 추가)
- **가이드**: 7개
- **레퍼런스**: 2개
- **아트 리소스**: 1개 (NPC 디자인 프롬프트)
- **테스트 문서**: 5개
- **인프라 문서**: 2개
- **히스토리**: 4개
- **전체**: 45개 문서

### 최근 업데이트
- **2025-11-27**: NPC 캐릭터 디자인 프롬프트 문서 추가 (art/ 폴더 신설)
- **2025-11-26**: 카메라 시스템 설계 문서 추가, Additive Scene Loading 구현
- **2025-11-26**: work-logs 폴더 신설, 작업 로그 통합 및 자동화
- **2025-11-22**: MVP 패턴 통합 완료
- **2025-11-19**: 문서 정리 및 UI 시스템 리팩토링

---

## 🤝 문서 기여

### 문서 개선 제안
1. 오타 발견 → 즉시 수정 후 커밋
2. 내용 추가 필요 → 해당 문서에 추가 후 커밋
3. 새 문서 필요 → 적절한 폴더에 생성 후 이 README 업데이트

### 문서 리뷰
- **신규 문서**: 작성 후 팀 리뷰
- **기존 문서 수정**: 중요 변경 시 팀 공유
- **정기 점검**: 월 1회 문서 정확성 검토

---

## 💡 팁

### 효율적인 문서 활용
1. **즐겨찾기**: 자주 보는 문서는 브라우저 즐겨찾기 추가
2. **검색 활용**: Ctrl+F로 문서 내 키워드 검색
3. **링크 활용**: 문서 간 링크를 따라 연관 정보 탐색

### 문서 읽는 순서
- **처음**: QuickStart → ProjectOverview
- **개발 전**: CodingGuidelines → CurrentStatus
- **작업 중**: CurrentStatus (수시 확인)
- **문제 발생**: 관련 문서 검색 (EncodingGuide 등)

---

**📚 모든 문서는 프로젝트 루트의 `docs/` 폴더에 있습니다.**

**🔙 [프로젝트 메인으로 돌아가기](../README.md)**

---

*최종 업데이트: 2025-11-26*
*GASPT 프로젝트 문서팀*

---

## 🆕 새로운 기능: 작업 로그 자동화

작업 완료 후 `/update-worklog` 명령만 입력하면:
- ✅ LATEST.md 자동 업데이트
- ✅ 월별 상세 로그 자동 생성
- ✅ Git 커밋 히스토리 반영

[자세히 보기 →](work-logs/README.md)
