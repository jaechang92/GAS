# GASPT 작업 현황

**최종 업데이트**: 2025-11-21
**현재 브랜치**: `master`

---

## 📌 Current Work

### 2025-11-21: 데이터/오브젝트 분리 아키텍처 구현

**작업 개요**: Player 씬 전환 시 데이터 유지를 위한 아키텍처 구현

**해결한 문제**:
1. FindAnyObjectByType<PlayerStats>() 남발 → 성능 문제
2. Player 씬 전환 시 참조 깨짐
3. Player 파괴/재생성 시 데이터 손실

**신규 파일**:
- `PlayerRunData.cs` - 런 데이터 클래스 (~150줄)
- `RunManager.cs` - 런 데이터 관리 싱글톤 (~300줄)

**수정 파일**:
- `PlayerStats.cs` - InitializeFromRunData/ToRunData, 자동 등록/해제
- `GameManager.cs` - RegisterPlayer/UnregisterPlayer
- `PlayerHealthBar.cs` - FindAnyObjectByType → RunManager 사용
- `PlayerManaBar.cs` - 동일 패턴 적용

**아키텍처**:
```
RunManager (DontDestroyOnLoad)
├── PlayerRunData (런 데이터 보관)
└── CurrentPlayer (현재 Player 참조)

Player (씬별 생성/파괴)
├── Start() → RunManager.RegisterPlayer() → 데이터 주입
└── OnDestroy() → RunManager.UnregisterPlayer() → 데이터 저장
```

**테스트 완료**: ✅ 2025-11-21
- SingletonPreloader 15개 싱글톤 초기화 확인
- Player 등록/해제 정상 동작
- RunManager 데이터 동기화 확인

**추가 리팩토링 완료**: ✅ 2025-11-21
- InventoryUI.cs - RunManager/GameManager 패턴 적용
- StatPanelUI.cs - line 55 수정
- SaveSystem.cs - line 117, 208 수정 (2곳)
- PlayerLevel.cs - line 105 수정

**다음 작업**:
- [ ] 씬 전환 데이터 유지 실제 테스트
- [ ] TestDungeonConfig 에셋 생성

---

## 📊 프로젝트 상태 요약

### 완료된 Phase 목록

| Phase | 내용 | 완료일 |
|-------|------|--------|
| 1 | Setup & Project Structure | - |
| 2 | GAS Core Implementation | - |
| 3 | Stat System (US1) | - |
| 4 | Shop & Economy System (US2) | - |
| 5 | Enemy System (US3) | - |
| 6 | Combat Integration | - |
| 7 | Save/Load System | - |
| 8 | Player HP Bar UI | - |
| 9 | Level & EXP System | - |
| 10 | Combat UI & Damage Numbers | - |
| 11 | Buff/Debuff System + BuffIcon UI | - |
| A-1~A-4 | MageForm, Enemy AI, Room System, Skill Item | - |
| B-1~B-3 | Editor Tools, Enemy Spawn, UI Integration | 2025-11-13 |
| C-1 | 적 타입 시스템 | 2025-11-15 |
| C-2 | 보스 전투 시스템 | 2025-11-16 |
| C-3 | 던전 진행 완성 | 2025-11-17 |
| C-4 | 아이템 드롭 및 인벤토리 | 2025-11-18 |
| - | 아키텍처 리팩토링 (GameManager) | 2025-11-19 |
| - | 동적 Room 로딩 시스템 | 2025-11-21 |
| - | 데이터/오브젝트 분리 (RunManager) | 2025-11-21 |
| - | UI 리팩토링 (FindAnyObjectByType 제거) | 2025-11-21 |

### 싱글톤 시스템 (12개)
GameResourceManager, PoolManager, DamageNumberPool, CurrencySystem, InventorySystem, PlayerLevel, SaveSystem, StatusEffectManager, SkillSystem, LootSystem, SkillItemManager, **RunManager** (NEW)

---

## 📚 참고

상세 작업 히스토리: `WORK_STATUS_OLD.md` 참조
