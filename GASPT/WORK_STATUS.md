# 작업 현황 및 다음 단계

**최종 업데이트**: 2025-11-02
**현재 브랜치**: `009-skill-system`
**작업 세션**: Phase 12 (Skill System) 구현 완료

---

## 📊 현재 프로젝트 상태

### 완료된 Phase

#### ✅ Phase 1: Setup & Project Structure
- Core Enums (StatType, EquipmentSlot, EnemyType, StatusEffectType)
- Assembly Definition 문제 해결 (모두 제거, Assembly-CSharp로 통합)
- SingletonManager<T> 패턴 확립

#### ✅ Phase 2: GAS Core Implementation
- IAbility, IAbilitySystem 인터페이스
- Ability 베이스 클래스
- AbilityData ScriptableObject
- AbilitySystem 싱글톤
- Awaitable 비동기 패턴 (Coroutine 미사용)

#### ✅ Phase 3: Stat System (US1)
**완료 Task**: 8개
- PlayerStats.cs (295줄) - Dirty Flag 최적화
- Item.cs (85줄) - ScriptableObject
- StatPanelUI.cs (270줄) - 실시간 UI + 버프/디버프 표시
- StatPanelCreator.cs (242줄) - 에디터 도구
- 아이템 3개 에셋: FireSword, LeatherArmor, IronRing

#### ✅ Phase 4: Shop & Economy System (US2)
**완료 Task**: 7개
- CurrencySystem.cs (165줄) - 골드 관리 싱글톤
- InventorySystem.cs (230줄) - 인벤토리 싱글톤
- ShopSystem.cs (220줄) - 상점 로직
- ShopUI.cs (320줄) - 상점 UI
- ShopItemSlot.cs (71줄) - 독립 파일
- ShopUICreator.cs (480줄) - 에디터 도구

#### ✅ Phase 5: Enemy System (US3)
**완료 Task**: 6개
- EnemyData.cs (157줄) - 적 데이터 ScriptableObject
- Enemy.cs (493줄) - 적 MonoBehaviour + StatusEffect 통합
- EnemyNameTag.cs (122줄) - World Space UI
- BossHealthBar.cs (201줄) - Screen Space UI
- EnemyUICreator.cs (400줄) - 에디터 도구
- 적 3종 에셋: NormalGoblin, EliteOrc, FireDragon

#### ✅ Phase 6: Combat Integration
**완료 Task**: 5개
- DamageCalculator.cs (84줄) - 데미지 계산 유틸리티
  - CalculateDamageDealt() - 공격력 → 데미지 (100% ~ 120% 랜덤)
  - CalculateDamageReceived() - 방어력 적용 (방어력 × 0.5 감소)
- PlayerStats.cs 수정 - Combat 메서드 추가
  - TakeDamage(), Heal(), DealDamageTo(), Revive()
- Enemy.cs 수정 - DealDamageTo() 추가
- CombatTest.cs (280줄) - 6가지 전투 시나리오 테스트

#### ✅ Phase 7: Save/Load System
**완료 Task**: 5개
- SaveData.cs (118줄) - 직렬화 데이터 구조
  - GameSaveData, PlayerStatsData, CurrencyData, InventoryData
- SaveSystem.cs (SingletonManager 사용, 198줄) - JSON 기반 저장/로드 싱글톤
  - Save(), Load(), HasSaveFile(), DeleteSave()
- PlayerStats.cs 수정 - GetSaveData(), LoadFromSaveData()
- CurrencySystem.cs 수정 - Save/Load 통합
- InventorySystem.cs 수정 - Save/Load 통합
- SaveTest.cs (220줄) - 6가지 저장/로드 시나리오 테스트

#### ✅ Phase 8: Player HP Bar UI
**완료 Task**: 3개
- PlayerHealthBar.cs (390줄) - HP Bar UI 스크립트
  - HP 슬라이더, 텍스트, 색상 효과
  - 데미지/회복 플래시 애니메이션
  - 저체력/위험 체력 색상 변화
- PlayerHealthBarCreator.cs (241줄) - UI 자동 생성 에디터 도구
- **버그 수정**:
  - Revive() 시 OnHealed 이벤트 미발생 → 수정
  - Fill Image 스프라이트 미할당 → 수정
  - 이벤트 구독 타이밍 이슈 → 수정

#### ✅ Phase 9: Level & EXP System
**완료 Task**: 6개
- PlayerLevel.cs (279줄) - 레벨/EXP 관리 싱글톤
  - EXP 공식: RequiredEXP = Level × 100
  - 레벨업 보상: HP +10, Attack +2, Defense +1
  - 레벨업 시 HP 완전 회복 (Revive 호출)
  - Reflection으로 PlayerStats 기본 스탯 수정
- PlayerExpBar.cs (390줄) - EXP Bar UI 스크립트
  - 레벨 텍스트 (Lv.X)
  - EXP 슬라이더 및 텍스트 (X/Y)
  - 레벨업 애니메이션 (텍스트 스케일 + 색상 효과)
- PlayerExpBarCreator.cs (241줄) - UI 자동 생성 에디터 도구
  - Hierarchy 렌더링 순서 최적화 (LevelText 마지막 배치)
- LevelTest.cs (301줄) - 6가지 테스트 시나리오
- EnemyData.cs 수정 - expReward 필드 추가
- Enemy.cs 수정 - GiveExp() 메서드 추가
- **네임스페이스 수정**: GASPT.Enemy → GASPT.Enemies (CS0118 에러 해결)

#### ✅ Phase 10: Combat UI & Damage Numbers
**완료 Task**: 5개
- DamageNumber.cs (180줄) - 데미지 텍스트 애니메이션
  - 일반 데미지 (빨간색), 크리티컬 (노란색), 회복 (초록색), EXP (파란색)
  - 위로 떠오르는 애니메이션 + 페이드 아웃
  - 자동 풀링 복귀
- DamageNumberPool.cs (350줄) - 오브젝트 풀링 시스템
  - 공용 Canvas 사용 (성능 최적화)
  - 카메라 빌보드 효과
  - 자동 리소스 로딩 (GameResourceManager 사용)
- DamageNumberCreator.cs (150줄) - 프리팹 자동 생성 에디터 도구
- PlayerStats.cs 수정 - DamageNumber 표시 통합
- Enemy.cs 수정 - DamageNumber 표시 통합
- SingletonPreloader.cs 수정 - DamageNumberPool 사전 로딩

#### ✅ Phase 11: Buff/Debuff System (상태이상 시스템)
**완료 Task**: 10개

**핵심 시스템** (5개 파일):
- StatusEffectType.cs (46줄) - 16가지 효과 타입 Enum
  - 버프: AttackUp, DefenseUp, SpeedUp, CriticalRateUp
  - 디버프: AttackDown, DefenseDown, SpeedDown, Stun, Slow
  - DoT: Poison, Burn, Bleed
  - 특수: Invincible, Regeneration, Shield, Root
- StatusEffect.cs (259줄) - 효과 인스턴스 클래스
  - 효과 생명주기 관리 (Apply → Update → Remove)
  - 틱 기반 DoT 시스템
  - 스택 시스템 (중첩 효과)
  - 이벤트 시스템 (OnApplied, OnRemoved, OnTick)
- StatusEffectData.cs (112줄) - ScriptableObject 데이터
  - 디자이너 친화적 효과 정의
  - CreateInstance() 팩토리 메서드
- StatusEffectManager.cs (300줄) - 싱글톤 관리자
  - Dictionary<GameObject, List<StatusEffect>> 구조
  - Update 루프에서 모든 활성 효과 업데이트
  - 효과 적용/제거/조회 API
  - 이벤트 브로드캐스트
- StatusEffectTest.cs (520줄) - 18개 테스트 케이스
  - Context Menu 기반 테스트
  - 버프/디버프/DoT/회복/중첩/제거 테스트

**기존 시스템 통합** (3개 파일):
- PlayerStats.cs 수정
  - Attack/Defense 프로퍼티에 버프/디버프 적용
  - BaseAttack/BaseDefense 프로퍼티 추가
  - DoT 틱 처리 (Poison, Burn, Bleed) - 방어력 무시
  - Regeneration 틱 처리 (회복)
  - OnEnable에서 StatusEffectManager 이벤트 구독
- Enemy.cs 수정
  - Attack 프로퍼티에 버프/디버프 적용
  - DoT/Regeneration 틱 처리
  - OnEnable에서 StatusEffectManager 이벤트 구독
- SingletonPreloader.cs 수정
  - StatusEffectManager 사전 로딩 추가 (총 7개 싱글톤)

**UI 시각화** (1개 파일):
- StatPanelUI.cs 수정
  - 버프/디버프 색상 표시 (초록/빨강)
  - "기본값 → 현재값" 형식 표시
  - StatusEffectManager 이벤트 구독

**버그 수정** (3개 커밋):
- StatusEffectManager 중첩 시 이벤트 발생
- PlayerStats OnStatChanged 이벤트 트리거
- 이벤트 구독 타이밍 문제 해결 (Awake → OnEnable)

#### ✅ Phase 12: Skill System (스킬 시스템)
**완료 Task**: 12개

**핵심 시스템** (4개 파일):
- SkillEnums.cs (46줄) - 스킬 타입 Enum
  - SkillType: Damage, Heal, Buff, Utility
  - TargetType: Self, Enemy, Area, Ally
- SkillData.cs (165줄) - ScriptableObject 스킬 데이터
  - 스킬 기본 정보 (이름, 아이콘, 설명)
  - 스킬 타입 및 타겟팅
  - 마나 비용, 쿨다운, 캐스팅 시간
  - 효과값 (데미지, 힐, 버프)
  - 범위 및 타겟 수
  - 비주얼/사운드 연동
- Skill.cs (280줄) - 스킬 실행 및 쿨다운 로직
  - TryExecute() - 쿨다운/마나/타겟 검증
  - Execute() - 타입별 효과 적용 (Damage/Heal/Buff)
  - RunCooldownTimer() - async Awaitable 쿨다운
  - GetCooldownRatio() - UI용 진행도 (0.0~1.0)
  - 이벤트: OnCooldownStart, OnCooldownComplete
- SkillSystem.cs (320줄) - 싱글톤 스킬 슬롯 관리
  - Dictionary<int, Skill> 슬롯 구조 (0~3번)
  - RegisterSkill() - 슬롯에 스킬 등록
  - TryUseSkill() - 스킬 사용 시도
  - GetSkill(), GetCooldownRatio() - 조회 메서드
  - 이벤트: OnSkillRegistered, OnSkillUsed, OnSkillFailed, OnCooldownChanged

**UI 시스템** (3개 파일):
- SkillSlotUI.cs (330줄) - 단일 스킬 슬롯 UI
  - 스킬 아이콘 표시
  - 쿨다운 Radial360 오버레이 (fillAmount)
  - 쿨다운 카운트다운 텍스트 (X.Xs)
  - 단축키 표시 (1, 2, 3, 4)
  - 마나 부족 시 비활성 오버레이
  - 키보드 입력 처리 (Alpha1~4)
  - RegisterSkill(), ClearSlot(), UpdateCooldownUI()
- SkillUIPanel.cs (200줄) - 4개 슬롯 관리 패널
  - SkillSystem 이벤트 구독
  - 슬롯 인덱스 자동 설정
  - 기존 스킬 로드 (LoadExistingSkills)
  - Context Menu: Print Slot Status, Reload All Skills
- SkillUICreator.cs (264줄) - UI 자동 생성 에디터 도구
  - Canvas 자동 생성/찾기 (1920x1080, ScreenSpaceOverlay)
  - SkillUIPanel 하단 중앙 배치 (400x80px)
  - 4개 SkillSlot 자동 생성 (각 80x80px)
  - 6개 자식 오브젝트 자동 생성 (Icon, CooldownOverlay, CooldownText, HotkeyText, DisabledOverlay)
  - SerializedObject로 모든 참조 자동 연결
  - HorizontalLayoutGroup 레이아웃
  - Delete Skill UI Panel 유틸리티

**테스트 도구** (2개 파일):
- SkillSystemTest.cs (430줄) - 8개 Context Menu 테스트
  - Test01: 초기 상태 확인 (Player, PlayerStats, SkillSystem)
  - Test02: 스킬 등록 (3개 슬롯)
  - Test03: 마나 확인 (TrySpendMana, RegenerateMana)
  - Test04: Damage 스킬 테스트 (Enemy HP 감소)
  - Test05: Heal 스킬 테스트 (Player HP 회복)
  - Test06: Buff 스킬 테스트 (Attack 증가)
  - Test07: 쿨다운 테스트 (재사용 블로킹)
  - Test08: 마나 부족 테스트 (사용 불가)
- SkillSystemTestSetup.cs (500줄) - 원클릭 테스트 환경 생성
  - Menu: Tools > GASPT > 🚀 One-Click Setup
  - 테스트 씬 자동 생성 (SkillSystemTest.unity)
  - Player + PlayerStats 생성 (baseMana: 100)
  - Enemy + EnemyData 생성
  - SkillSystemTest 컴포넌트 생성 및 참조 연결
  - 3개 SkillData 자동 생성 (Fireball, Heal, AttackBuff)
  - 1개 EnemyData 자동 생성 (TEST_Enemy)
  - 1개 StatusEffectData 자동 생성 (TEST_AttackUp)
  - Reflection으로 private 필드 설정
  - SerializedObject로 참조 연결

**기존 시스템 통합** (2개 파일):
- PlayerStats.cs 수정 - 마나 시스템 추가
  - baseMana 필드 (기본값: 100)
  - currentMana, finalMana 내부 상태
  - MaxMana, CurrentMana 프로퍼티
  - TrySpendMana(int) - 마나 소비 (부족 시 false)
  - RegenerateMana(int) - 마나 회복 (MaxMana 제한)
  - OnManaChanged 이벤트
  - RecalculateStats()에 마나 계산 추가
  - Context Menu 테스트 메서드 3개
- SingletonPreloader.cs 수정
  - SkillSystem 사전 로딩 추가 (총 8개 싱글톤)

**테스트 에셋** (5개):
- TEST_FireballSkill.asset - Damage 스킬 (마나 20, 쿨다운 3초, 데미지 50)
- TEST_HealSkill.asset - Heal 스킬 (마나 15, 쿨다운 5초, 회복 30)
- TEST_AttackBuffSkill.asset - Buff 스킬 (마나 25, 쿨다운 8초)
- TEST_AttackUp.asset - Attack +10 버프 (지속시간 5초)
- TEST_Enemy.asset - 테스트용 Enemy (HP 100, Attack 15)

**문서화**:
- SKILL_SYSTEM_TEST_GUIDE.md - 수동 테스트 가이드
- SKILL_SYSTEM_ONE_CLICK_TEST.md - 원클릭 도구 가이드

#### ✅ 추가 구현: GameResourceManager (리소스 관리 시스템)
**완료 Task**: 6개

**핵심 시스템** (2개 파일):
- GameResourceManager.cs (251줄) - 싱글톤 리소스 관리자
  - Resources.Load() 래핑 및 캐싱 시스템
  - 타입별 로딩 메서드:
    - LoadPrefab() - GameObject
    - LoadScriptableObject<T>() - ScriptableObject
    - LoadAudioClip() - AudioClip
    - LoadSprite() - Sprite
    - LoadTextAsset() - TextAsset
  - 인스턴스화 메서드:
    - Instantiate(path, parent)
    - Instantiate(path, position, rotation, parent)
  - 캐싱 관리:
    - UnloadResource(path)
    - UnloadAllResources()
    - PrintCacheInfo() (디버그용)
- ResourcePaths.cs (195줄) - 리소스 경로 상수 관리
  - 카테고리별 구분 (Prefabs, Data, Audio, Sprites)
  - IDE 자동완성 지원
  - 타입 안전성 보장

**리팩토링** (2개 파일):
- DamageNumberPool.cs 수정
  - damageNumberPrefab SerializeField 제거
  - GameResourceManager를 통한 자동 로딩
  - LoadDamageNumberPrefab() 메서드 추가
- SingletonPreloader.cs 수정
  - GameResourceManager 최우선 순위 사전 로딩
  - 총 7개 싱글톤 관리

**문서화**:
- RESOURCES_GUIDE.md (220줄) - Resources 폴더 구조 가이드
  - 폴더 구조 정의
  - 사용 방법 및 예제
  - 네이밍 규칙
  - 주의사항 및 베스트 프랙티스

---

## 🎯 현재 작업 상태

### Git 상태
```bash
브랜치: 009-skill-system
원격 푸시: 완료
최종 커밋: fa7c6cb (에셋: 테스트 에셋 및 Unity 생성 파일 추가)
Phase 12 커밋: eff2bbe ~ fa7c6cb (총 6개 커밋)
```

### 주요 커밋 목록 (Phase 12)
```
fa7c6cb 에셋: 테스트 에셋 및 Unity 생성 파일 추가
44f5632 기능: Skill UI 시스템 구현 (SkillSlotUI, SkillUIPanel, SkillUICreator)
38113eb 도구: SkillSystem 원클릭 테스트 환경 자동 생성 툴
5dd9ac0 테스트: SkillSystem 테스트 스크립트 및 가이드 작성
658687a 기능: Skill 클래스 및 SkillSystem 싱글톤 구현
eff2bbe 기능: SkillData ScriptableObject 및 PlayerStats 마나 시스템 추가
```

### 싱글톤 시스템 현황 (8개)
1. **GameResourceManager** - 리소스 자동 로딩 및 캐싱
2. **SkillSystem** - 스킬 슬롯 관리 및 실행 (NEW)
3. **DamageNumberPool** - 데미지 텍스트 풀링
4. **CurrencySystem** - 골드 관리
5. **InventorySystem** - 인벤토리 관리
6. **PlayerLevel** - 레벨/EXP 관리
7. **SaveSystem** - 저장/로드
8. **StatusEffectManager** - 상태이상 효과 관리

### PR 생성 대기
- **Phase 12 (Skill System) PR**: 생성 필요
  - **Base 브랜치**: master
  - **Compare 브랜치**: 009-skill-system
  - **포함 내용**:
    - SkillData ScriptableObject
    - Skill 실행 로직 (async Awaitable 쿨다운)
    - SkillSystem 싱글톤
    - PlayerStats 마나 시스템
    - SkillSlotUI (키보드 입력, 쿨다운 애니메이션)
    - SkillUIPanel (이벤트 구독)
    - SkillUICreator (자동 UI 생성 도구)
    - SkillSystemTest (8개 테스트)
    - SkillSystemTestSetup (원클릭 테스트 환경)
    - 테스트 에셋 5개

---

## 📂 중요 파일 위치

### 코드
```
Assets/_Project/Scripts/
├── Core/
│   ├── SingletonManager.cs
│   └── SingletonPreloader.cs (7개 싱글톤 관리)
├── Core/Enums/
│   ├── StatType.cs
│   ├── EquipmentSlot.cs
│   ├── EnemyType.cs
│   └── StatusEffectType.cs (NEW)
├── Stats/
│   └── PlayerStats.cs (Combat, Save/Load, StatusEffect 통합)
├── Data/
│   ├── Item.cs
│   ├── EnemyData.cs
│   └── StatusEffectData.cs (NEW)
├── Economy/
│   └── CurrencySystem.cs
├── Inventory/
│   └── InventorySystem.cs
├── Shop/
│   └── ShopSystem.cs
├── Enemies/
│   └── Enemy.cs (StatusEffect 통합)
├── Combat/
│   └── DamageCalculator.cs
├── Save/
│   ├── SaveData.cs
│   └── SaveSystem.cs (SingletonManager 사용)
├── Level/
│   └── PlayerLevel.cs
├── Skills/ (NEW)
│   ├── SkillEnums.cs
│   ├── SkillData.cs
│   ├── Skill.cs
│   └── SkillSystem.cs
├── StatusEffects/
│   ├── StatusEffect.cs
│   ├── StatusEffectManager.cs
│   └── StatusEffectTest.cs
├── Resources/
│   ├── GameResourceManager.cs
│   └── ResourcePaths.cs
├── UI/
│   ├── StatPanelUI.cs (버프/디버프 표시)
│   ├── ShopUI.cs
│   ├── ShopItemSlot.cs
│   ├── EnemyNameTag.cs
│   ├── BossHealthBar.cs
│   ├── PlayerHealthBar.cs
│   ├── PlayerExpBar.cs
│   ├── DamageNumber.cs
│   ├── DamageNumberPool.cs (자동 로딩)
│   ├── SkillSlotUI.cs (NEW)
│   └── SkillUIPanel.cs (NEW)
├── Editor/
│   ├── StatPanelCreator.cs
│   ├── ShopUICreator.cs
│   ├── EnemyUICreator.cs
│   ├── PlayerHealthBarCreator.cs
│   ├── PlayerExpBarCreator.cs
│   ├── DamageNumberCreator.cs
│   ├── SkillUICreator.cs (NEW)
│   └── SkillSystemTestSetup.cs (NEW)
└── Testing/ (Tests에서 이름 변경)
    ├── CombatTest.cs
    ├── SaveTest.cs
    ├── LevelTest.cs
    ├── StatusEffectTest.cs
    └── SkillSystemTest.cs (NEW)
```

### 문서
```
GASPT/
├── WORK_STATUS.md (현재 파일)
├── RESOURCES_GUIDE.md
├── SKILL_SYSTEM_TEST_GUIDE.md (NEW)
├── SKILL_SYSTEM_ONE_CLICK_TEST.md (NEW)
├── specs/
└── docs/
```

---

## 📊 Phase별 완료 통계

| Phase | 설명 | 파일 수 | 코드 라인 | 상태 |
|-------|------|---------|-----------|------|
| Phase 1 | Setup & Project Structure | 3 | ~100 | ✅ 완료 |
| Phase 2 | GAS Core | 5 | ~500 | ✅ 완료 |
| Phase 3 | Stat System | 4 | ~812 | ✅ 완료 |
| Phase 4 | Shop & Economy | 5 | ~1,486 | ✅ 완료 |
| Phase 5 | Enemy System | 5 | ~1,118 | ✅ 완료 |
| Phase 6 | Combat Integration | 2 | ~364 | ✅ 완료 |
| Phase 7 | Save/Load System | 4 | ~536 | ✅ 완료 |
| Phase 8 | Player HP Bar UI | 2 | ~631 | ✅ 완료 |
| Phase 9 | Level & EXP System | 4 | ~1,211 | ✅ 완료 |
| Phase 10 | Combat UI & Damage Numbers | 3 | ~680 | ✅ 완료 |
| Phase 11 | Buff/Debuff System | 9 | ~1,691 | ✅ 완료 |
| 추가 | GameResourceManager | 3 | ~666 | ✅ 완료 |
| Phase 12 | Skill System | 11 | ~2,489 | ✅ 완료 |
| **합계** | **12개 Phase + 추가** | **60개** | **~12,284줄** | **✅ 완료** |

---

## 🚀 다음 작업 옵션

### 옵션 1: PR 생성 및 머지 (Phase 12 - Skill System)

**수행 단계**:
1. PR 제목: "Skill System 구현 (Phase 12)"
2. PR 본문:
```markdown
## Summary
Phase 12: Skill System 구현 완료
- 스킬 데이터, 실행 로직, UI, 테스트 도구 모두 구현
- 마나 시스템 추가
- 8개 싱글톤으로 확장

## 핵심 시스템
- **SkillData**: ScriptableObject 스킬 정의
- **Skill**: 쿨다운, 실행 로직 (async Awaitable)
- **SkillSystem**: 슬롯 관리 싱글톤
- **PlayerStats 마나 시스템**: TrySpendMana, RegenerateMana

## UI 시스템
- **SkillSlotUI**: 아이콘, 쿨다운 애니메이션, 키보드 입력
- **SkillUIPanel**: 4개 슬롯 관리, 이벤트 구독
- **SkillUICreator**: 자동 UI 생성 에디터 도구

## 테스트
- **SkillSystemTest**: 8개 Context Menu 테스트
- **SkillSystemTestSetup**: 원클릭 테스트 환경 생성
- 테스트 에셋 5개 (Fireball, Heal, AttackBuff 등)

## Test plan
- [ ] Unity에서 SkillSystemTest 씬 열기
- [ ] Tools > GASPT > Create Skill UI Panel 실행
- [ ] Play 모드에서 Context Menu로 스킬 등록
- [ ] 키보드 1,2,3,4로 스킬 사용 테스트
- [ ] 쿨다운 애니메이션 확인
- [ ] 마나 부족 상태 확인

🤖 Generated with [Claude Code](https://claude.com/claude-code)
```

3. GitHub에서 PR 생성
4. 리뷰 후 머지

**머지 후**:
```bash
git checkout master
git pull origin master
git branch -d 009-skill-system  # 로컬 브랜치 삭제
```

---

### 옵션 2: BuffIconUI 구현 (Phase 11 확장)

**Phase 11 완성도 향상**:
- [ ] BuffIconUI 프리팹
- [ ] BuffIconPool 오브젝트 풀링
- [ ] 활성 버프/디버프 아이콘 표시
- [ ] 지속시간 표시 (원형 타이머)
- [ ] 스택 수 표시

---

### 옵션 3: Item Drop & Loot System

**Phase 13 새 기능**:
- [ ] LootTable ScriptableObject
- [ ] DropSystem 싱글톤
- [ ] 아이템 드롭 로직 (확률 기반)
- [ ] 드롭 아이템 UI
- [ ] Enemy에 LootTable 연동

---

### 옵션 4: Mana Bar UI 구현

**Skill System 확장**:
- [ ] PlayerManaBar.cs (HealthBar와 유사한 구조)
- [ ] PlayerManaBarCreator.cs (자동 생성 도구)
- [ ] 마나 회복 애니메이션
- [ ] 마나 부족 경고 효과

---

## 🧪 빠른 테스트 방법 (Unity에서)

### 에디터 도구로 UI 생성
```
Tools > GASPT > Create StatPanel UI
Tools > GASPT > Create ShopUI
Tools > GASPT > Create Enemy UIs
Tools > GASPT > Create Player HealthBar UI
Tools > GASPT > Create Player ExpBar UI
Tools > GASPT > Create DamageNumber Prefab
Tools > GASPT > Create Skill UI Panel (NEW)
Tools > GASPT > 🚀 One-Click Setup (SkillSystemTest) (NEW)
```

### Context Menu로 빠른 테스트

**SkillSystemTest** (NEW):
- 우클릭 → `Run All Tests` (전체 테스트 자동 실행)
- 우클릭 → `01. Check Initial State` (초기 상태 확인)
- 우클릭 → `02. Register Skills` (스킬 등록)
- 우클릭 → `03. Check Mana` (마나 확인)
- 우클릭 → `04. Test Damage Skill (Slot 0)` (Fireball)
- 우클릭 → `05. Test Heal Skill (Slot 1)` (Heal)
- 우클릭 → `06. Test Buff Skill (Slot 2)` (AttackBuff)
- 우클릭 → `07. Test Cooldown` (쿨다운 테스트)
- 우클릭 → `08. Test Out Of Mana` (마나 부족 테스트)
- 우클릭 → `Print Player Stats` (플레이어 상태 출력)
- 우클릭 → `Print Skill Slots` (스킬 슬롯 상태)

**SkillUIPanel** (NEW):
- 우클릭 → `Print Slot Status` (슬롯 UI 상태 확인)
- 우클릭 → `Reload All Skills` (모든 스킬 재로드)

**StatusEffectTest**:
- 우클릭 → `Test/Player/Apply AttackUp` (버프 적용)
- 우클릭 → `Test/Player/Apply AttackDown` (디버프 적용)
- 우클릭 → `Test/Player/Apply Poison` (DoT 적용)
- 우클릭 → `Test/Player/Apply Regeneration` (회복)
- 우클릭 → `Test/Player/Remove All Effects` (효과 제거)
- 우클릭 → `Test/Player/Print Active Effects` (활성 효과 확인)

**DamageNumberPool**:
- DamageNumber는 자동으로 표시됨 (데미지/회복/EXP 시)

**GameResourceManager**:
- 우클릭 → `Print Cache Info` (캐시 상태 확인)

**PlayerStats**:
- 우클릭 → `Take 10 Damage (Test)` → DamageNumber 표시됨
- 우클릭 → `Heal 20 HP (Test)` → 회복 텍스트 표시됨
- 우클릭 → `Test Mana Spend (20)` (NEW)
- 우클릭 → `Test Mana Regen (30)` (NEW)
- 우클릭 → `Print Mana Info` (NEW)

**Enemy**:
- 우클릭 → `Instant Death (Test)` → EXP Number 표시됨

**PlayerLevel**:
- 우클릭 → `Add 50 EXP (Test)` → EXP Number 표시됨

---

## 📝 작업 재개 시 체크리스트

### 1. Git 상태 확인
```bash
cd D:/JaeChang/UintyDev/GASPT/GASPT
git status
git log --oneline -10
git branch
```

### 2. 현재 브랜치 확인
```bash
# 현재 브랜치가 009-skill-system인지 확인
git branch --show-current
```

### 3. Unity 테스트 (선택)
- SkillSystemTest 씬 열기
- Tools > GASPT > Create Skill UI Panel
- Play 모드에서 Context Menu로 스킬 등록
- 키보드 1,2,3,4로 스킬 사용 테스트

### 4. 다음 작업 선택
- PR 생성 및 머지 (Phase 12) → 옵션 1
- BuffIconUI 구현 → 옵션 2
- Item Drop System → 옵션 3
- Mana Bar UI → 옵션 4

---

## 🔍 주요 명령어 요약

### Git 명령어
```bash
# 현재 상태 확인
git status
git log --oneline -10
git branch

# PR 생성 후 머지됐으면
git checkout master
git pull origin master

# 새 Phase 시작
git checkout -b 010-next-phase

# 변경사항 커밋
git add .
git commit -m "커밋 메시지"
git push origin <브랜치명>
```

### Unity 에디터 도구
```
Tools > GASPT > Create StatPanel UI
Tools > GASPT > Create ShopUI
Tools > GASPT > Create Enemy UIs
Tools > GASPT > Create Player HealthBar UI
Tools > GASPT > Create Player ExpBar UI
Tools > GASPT > Create DamageNumber Prefab
Tools > GASPT > Create Skill UI Panel (NEW)
Tools > GASPT > 🚀 One-Click Setup (SkillSystemTest) (NEW)
```

### ScriptableObject 생성
```
Create > GASPT > Items > Item
Create > GASPT > Enemies > Enemy
Create > GASPT > StatusEffects > StatusEffect
Create > GASPT > Skills > Skill (NEW)
```

---

## ⚠️ 알아두면 좋은 정보

### GameResourceManager 사용
```csharp
// BEFORE (수동 할당)
[SerializeField] private GameObject prefab;

// AFTER (자동 로딩)
GameObject prefab = GameResourceManager.Instance.LoadPrefab(ResourcePaths.Prefabs.UI.DamageNumber);
```

### StatusEffect 사용 예시
```csharp
// 버프 적용
StatusEffectData attackUp = GameResourceManager.Instance.LoadScriptableObject<StatusEffectData>(
    ResourcePaths.Data.StatusEffects.AttackUp
);
StatusEffectManager.Instance.ApplyEffect(player.gameObject, attackUp);

// 효과 확인
bool hasBuff = StatusEffectManager.Instance.HasEffect(player.gameObject, StatusEffectType.AttackUp);

// 효과 제거
StatusEffectManager.Instance.RemoveEffect(player.gameObject, StatusEffectType.AttackUp);
```

### 이벤트 구독 패턴 (중요!)
```csharp
// Awake가 아닌 OnEnable에서 구독
private void OnEnable()
{
    StatusEffectManager manager = StatusEffectManager.Instance;  // HasInstance 사용 금지
    if (manager != null)
    {
        manager.OnEffectApplied += OnEffectApplied;
    }
}

private void OnDisable()
{
    if (StatusEffectManager.HasInstance)  // OnDisable에서만 HasInstance 사용
    {
        StatusEffectManager.Instance.OnEffectApplied -= OnEffectApplied;
    }
}
```

### Unity 버전
- Unity 6.0 이상
- CS0618 경고 주의 (deprecated API)
  - velocity → linearVelocity
  - FindObjectOfType → FindAnyObjectByType

### 코딩 규칙
- 카멜케이스 사용 (변수명에 '_' 붙이지 않음)
- 한글 주석 허용
- 500줄 넘으면 파일 분할
- Coroutine 사용 금지 (Awaitable 사용)

---

## 🎯 추천 작업 순서

**다음에 작업 재개 시 추천 순서**:

1. **이 파일(WORK_STATUS.md) 먼저 읽기** ✅
2. **Git 상태 확인** (`git status`, `git log`)
3. **Phase 12 PR 생성** (옵션 1) - 우선 추천
4. **PR 머지**
5. **다음 Phase 기획 및 시작** (BuffIconUI, Item Drop, Mana Bar 등)

---

## 💡 빠른 재개를 위한 팁

### Claude Code와 다시 대화 시작할 때
1. 이 파일(`WORK_STATUS.md`) 내용 공유
2. 현재 브랜치 알려주기: `009-skill-system`
3. 하고 싶은 작업 명시:
   - "Phase 12 PR 생성하고 싶어"
   - "BuffIconUI 구현하고 싶어"
   - "Mana Bar UI 구현하고 싶어"
   - "Item Drop System 시작하고 싶어"

---

## 📚 참고 문서

### 프로젝트 문서
1. **WORK_STATUS.md** (현재 파일) - 전체 작업 현황
2. **RESOURCES_GUIDE.md** - Resources 폴더 구조 및 사용법
3. **specs/004-rpg-systems/** - 기능 명세 및 Task 목록

---

**작성일**: 2025-11-02
**다음 예정 작업**: Phase 12 PR 생성 (우선) 또는 다음 Phase 시작
**브랜치**: 009-skill-system
**상태**: Phase 12 (Skill System) 완료, 푸시 완료, PR 생성 대기

🚀 **수고하셨습니다! Phase 12 (Skill System) 완료!**
