# 구현 계획서: First Level with RPG Systems

**프로젝트 관리 정보**
- **브랜치**: `004-rpg-systems`
- **작성일**: 2025-11-01
- **스펙 문서**: [spec.md](./spec.md)
- **기능**: First Level with RPG Systems
- **상태**: 진행 중 (Phase 0)

---

## 1. 요약 (Executive Summary)

### 1.1 목표
액션 플랫포머를 7개 RPG 시스템 통합으로 완전한 액션-RPG로 변환

### 1.2 핵심 성과물
- 스탯 시스템: HP(100), Attack(15), Defense(5) + 장비 보너스
- 아이템 시스템: ScriptableObject 기반 장비 (+5~+20 보너스)
- 인벤토리: 무제한 슬롯, 장착/해제
- 상점: 골드 거래 (<1초 로드)
- 경제: 골드 추적 및 거래
- 적 타입: Normal/Named/Boss 3가지
- 세이브/로드: JSON 저장 (<2초, <100KB)

### 1.3 게임플레이 루프
시작 → 적 처치 → 골드 획득 → 상점 방문 → 아이템 구매 → 스탯 증가 → 반복

---

## 2. 기술 컨텍스트

### 2.1 개발 환경
- 언어: C# 12
- 엔진: Unity 2023.3+ (Unity 6.0+)
- 테스트: Unity Test Framework (async void)
- 패턴: async/await Awaitable (Coroutine 금지)

### 2.2 성능 목표
- 스탯 재계산: <50ms
- 세이브/로드: <2초
- 레벨 로드: <5초
- 상점 UI: <1초
- 프레임율: 60 FPS (최대 8개 적)
- 저장 파일: <100KB

### 2.3 제약사항
- 동시 적: 최대 8개
- 저장 형식: JSON
- 인벤토리: 무제한 슬롯
- 최소 아이템: 6개
- 적 타입: 3가지 정확히
- UI: 화면 한 번에 표시

---

## 3. 헌장 검증 (GATE)

```
[✅] I. 완성 우선 원칙
[✅] II. 단계적 개발 원칙
[✅] III. 생산성 우선 원칙
[✅] IV. 플레이어 경험 우선 원칙
[✅] V. SOLID 원칙
[⚠️] VI. 비동기 패턴 (검증 필요)
[✅] VII. 지역화 (UTF-8)
[✅] VIII. 토큰 효율성
[✅] IX. Unity 6.0+ 호환성

결과: 조건부 합격 - Phase 0 비동기 연구 필수
```

---

## 4. Phase 0: 연구 및 검증

### 4.1 연구 주제
1. 비동기 파일 I/O 패턴 (세이브/로드 <2초)
2. ScriptableObject 성능 (로드, 캐싱)
3. UI 아키텍처 (상점 UI <1초)
4. 스탯 계산 최적화 (<50ms)
5. 적 타입 시스템 설계
6. 세이브 데이터 스키마 (<100KB)

### 4.2 산출물
- research.md: 기술 결정 및 검증

---

## 5. Phase 1: 설계 및 계약

### 5.1 산출물
- data-model.md: 엔티티, 상태 전이, 관계도
- api-contracts.md: 5개 주요 인터페이스
- quickstart.md: 6개 빠른 시작 항목

### 5.2 핵심 엔티티
- PlayerStats: 스탯 관리
- Item: 장비 정의
- InventorySystem: 보관/장착
- ShopSystem: 구매
- EnemyType: 적 분류
- SaveData: 진행도 저장

---

## 6. Phase 2: 작업 분해

### 6.1 우선순위 매핑
```
P1: 스탯 시스템      → StatSystem (1단계)
P2: 상점/경제        → ItemSystem + Economy (2단계)
P3: 적 타입          → EnemyTypeSystem (3단계)
P4: 전투 통합        → Combat Integration (4단계)
P5: 세이브/로드      → SaveLoadManager (5단계)
P6: 레벨 완성        → UI + E2E (6단계)
```

### 6.2 산출물
- tasks.md: 30-50개 작업 분해 (/speckit.tasks)

---

## 7. 개발 패턴

### 7.1 코딩 스타일
- 한국어 주석 (UTF-8)
- camelCase (언더스코어 금지)
- async/await Awaitable만 사용
- Unity 6.0+ API (linearVelocity, FindAnyObjectByType)

### 7.2 아키텍처
- 이벤트 기반 (OnStatChanged, OnGoldChanged)
- 싱글톤 매니저 (EconomyManager, SaveLoadManager)
- ScriptableObject 데이터 중심

### 7.3 성능 최적화
- 스탯 캐싱 (더티 플래그)
- 루프 기반 계산 (LINQ 제한)
- 배치 처리

---

## 8. 위험 요소

| 위험 | 확률 | 영향 | 완화 |
|------|------|------|------|
| 비동기 <2초 미달 | 중 | 높 | Phase 0 측정 |
| JSON <100KB 초과 | 낮 | 중 | 데이터 최소화 |
| 스탯 <50ms 미달 | 낮 | 중 | 캐싱 |
| UI <1초 미달 | 낮 | 낮 | 최적화 |

---

## 9. 성공 기준

### Phase 0 완료
- research.md 검증됨
- 기술 리스크 제거됨

### Phase 1 완료
- 3개 설계 문서 작성됨
- API 계약 확정됨

### Phase 2 완료
- tasks.md 생성됨 (30-50개)
- 의존성 순서 정렬됨

### 최종 (구현 후)
- 51 FR 구현
- 성능 기준 달성
- 테스트 >90%
- 0% 데이터 손상

---

## 10. 타임라인

| Phase | 기간 | 산출물 |
|-------|------|--------|
| 0 | 2-3일 | research.md |
| 1 | 2-3일 | 설계 문서 |
| 2 | 1-2일 | tasks.md |
| 3 | 8-10일 | 소스 코드 |
| 4-6 | 5-7일 | 테스트 완료 |

**총 기간**: 18-26일 (3-4주)

### 마일스톤
- M1: Phase 0 (Day 3)
- M2: Phase 1 (Day 6)
- M3: Phase 2 (Day 8)
- M4: StatSystem (Day 14)
- M5: 코어 (Day 18)
- M6: 기능 (Day 22)
- M7: 릴리스 (Day 26)

---

## 11. 참고 자료

- Unity JSON: https://docs.unity3d.com/6000.0/Documentation/Manual/JSONSerialization.html
- Newtonsoft.Json: https://github.com/jilleJr/Newtonsoft.Json-for-Unity
- File async: https://learn.microsoft.com/en-us/dotnet/api/system.io.file.writealltextasync
- GAS Core: ../003-first-playable-level/plan.md
- 프로젝트 헌장: GASPT/CLAUDE.md

---

## 12. 다음 단계

1. ✅ plan.md 완료
2. ⏳ Phase 0: research.md 작성
3. ⏳ Phase 1: 설계 문서 (data-model.md, api-contracts.md, quickstart.md)
4. ⏳ Phase 2: tasks.md 생성 (/speckit.tasks)
5. ⏳ Phase 3: 구현 시작

---

**버전**: 1.0
**상태**: Draft
**편집**: 2025-11-01
