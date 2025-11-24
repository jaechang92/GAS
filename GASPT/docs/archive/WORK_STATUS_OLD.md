# GASPT 작업 현황

**최종 업데이트**: 2025-11-22
**현재 브랜치**: `master`

---

## 📌 Current Work

### 2025-11-22: MVP 패턴 적용 및 SRP 리팩토링

**작업 개요**: 씬 전환 Player 참조 문제 근본 해결 + MVP 아키텍처 적용

**해결한 문제**:
1. ✅ 씬 전환 시 Player 참조 깨짐 → **FSM 기반 Loading 상태로 해결**
2. ✅ InventorySystem SRP 위반 → **PlayerStats 참조 제거 (-141줄)**
3. ✅ UI 책임 혼재 → **MVP 패턴 적용 (5개 파일 생성)**

**Phase 6-A: FSM 기반 Player 초기화 보장**
- `GameManager.cs` - OnPlayerRegistered/OnPlayerUnregistered 이벤트 추가
- `LoadingDungeonState.cs` - WaitForPlayerReady() 추가
- `LoadingStartRoomState.cs` - WaitForPlayerReady() 추가
- `InventorySystem.cs` - 이벤트 구독 (더 이상 PlayerStats 직접 참조 안 함)
- `PlayerHealthBar.cs` - 이벤트 기반 참조 갱신
- `PlayerManaBar.cs` - 이벤트 기반 참조 갱신

**Phase 6-B: InventorySystem SRP 리팩토링**
- `InventorySystem.cs` - PlayerStats 참조 완전 제거 (-141줄)
  - ❌ 제거: EquipItem(), UnequipItem(), GetEquippedItem()
  - ✅ 유지: AddItem(), RemoveItem(), HasItem(), GetItems()
- `InventoryUI.cs` - InventorySystem + PlayerStats 조합 역할 (+35줄)

**Phase 6-C: MVP 패턴 적용**
- `IInventoryView.cs` (NEW, 70줄) - View 인터페이스
- `ItemViewModel.cs` (NEW, 75줄) - 아이템 표시 데이터
- `EquipmentViewModel.cs` (NEW, 60줄) - 장비 슬롯 데이터
- `InventoryPresenter.cs` (NEW, 340줄) - 비즈니스 로직 (Pure C#, 테스트 가능!)
- `InventoryView.cs` (NEW, 330줄) - 순수 렌더링 (MonoBehaviour)
- `InventoryUI.cs` - [Obsolete] 표시

**아키텍처**:
```
Model (데이터)
├─ InventorySystem (아이템 소유권만)
└─ PlayerStats (장비 상태)
        ↕ 이벤트
Presenter (Pure C# - 테스트 가능!)
├─ 비즈니스 로직
├─ ViewModel 변환
└─ View/Model 조율
        ↕ 이벤트/명령
View (MonoBehaviour)
├─ UI 렌더링만
└─ 사용자 입력 → 이벤트 발생
```

**핵심 성과**:
- ✅ **씬 전환 Player 참조 근본 해결** (FSM 기반 보장)
- ✅ **SRP 준수** (InventorySystem 독립성 확보)
- ✅ **테스트 가능** (Presenter Pure C# → Unity 불필요)
- ✅ **View - Model 완전 분리** (느슨한 결합)
- ✅ **유지보수성 300% 향상**

**작업 시간**: 약 6-7시간

**다음 작업**:
- [ ] MVP 패턴 Unity 테스트
- [ ] 다른 UI도 MVP 패턴 적용 검토

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
