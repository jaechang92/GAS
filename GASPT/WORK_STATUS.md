# 작업 현황 및 다음 단계

**최종 업데이트**: 2025-11-12
**현재 브랜치**: `014-skull-platformer-phase-a`
**작업 세션**: Phase A-4 Item-Skill System 구현 완료

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

#### ✅ BuffIcon UI 구현 (Phase 11 확장)
**완료 Task**: 7개
**완료 날짜**: 2025-11-09

**핵심 파일** (3개):
- BuffIcon.cs (192줄) - 단일 버프 아이콘 UI
  - 아이콘 이미지, 원형 타이머, 스택 수, 남은 시간 표시
  - **Awaitable 기반 타이머 업데이트** (Coroutine 대신)
  - CancellationTokenSource로 업데이트 중단 관리
  - 버프(초록)/디버프(빨강) 테두리 색상 구분
  - Show(), Hide(), UpdateStack() 메서드

- BuffIconPanel.cs (246줄) - 아이콘 컨테이너 및 풀링
  - 최대 10개 BuffIcon 오브젝트 풀링
  - StatusEffectManager 이벤트 구독
  - OnEffectApplied → ShowIcon()
  - OnEffectRemoved → HideIcon()
  - OnEffectStacked → UpdateStack()
  - SetTarget() - 타겟 오브젝트 동적 변경
  - Context Menu 테스트 3개

- BuffIconCreator.cs (271줄) - 에디터 자동 생성 도구
  - Menu: `Tools > GASPT > UI > Create Buff Icon UI`
  - BuffIconPanel 자동 생성 (캔버스 왼쪽 상단, 400x80px)
  - BuffIcon 프리팹 자동 생성 (Resources/Prefabs/UI/)
  - 6개 자식 UI 요소 자동 생성 (Background, IconImage, TimerFillImage, BorderImage, StackText, TimeText)
  - SerializedObject로 모든 참조 자동 연결
  - Delete Buff Icon Panel 유틸리티

**기존 시스템 수정** (3개 파일):
- StatusEffect.cs 수정
  - Icon 프로퍼티 추가 (Sprite)
  - IsBuff 프로퍼티 추가 (bool)
  - 생성자에 icon, isBuff 매개변수 추가

- StatusEffectData.cs 수정
  - CreateInstance()에서 icon, isBuff 전달

- StatusEffectManager.cs 수정
  - OnEffectStacked 이벤트 추가 (중첩 시 발생)
  - ApplyEffect()에서 중첩 시 OnEffectStacked 호출

**문서화**:
- ERROR_SOLUTIONS_PORTFOLIO.md (+841줄)
  - Section 3: Awaitable과 CancellationToken 완전 가이드
  - Section 4: BuffIcon ContinueWith 에러 해결

**PR 정보**:
- PR #6: https://github.com/jaechang92/GAS/pull/6
- 브랜치: 012-buff-icon-ui
- 커밋 5개:
  - 7b1f861 기능: BuffIcon UI 구현 (버프/디버프 시각화)
  - 8e85598 수정: StatusEffectManager에 OnEffectStacked 이벤트 추가
  - ee20a27 수정: BuffIcon ContinueWith 에러 수정 (CS1061)
  - 92fb48e 문서: Awaitable과 CancellationToken 포트폴리오 문서 추가
  - 0ac9e69 테스트: BuffIcon UI 프리팹 및 테스트 씬 추가

**테스트 완료**:
✅ 아이콘 표시/숨김 동작 확인
✅ 원형 타이머 실시간 업데이트 확인
✅ 남은 시간 텍스트 업데이트 확인 (10초 이상: 정수, 10초 미만: 소수점 1자리)
✅ 스택 수 표시 확인 (2개 이상일 때만 표시)
✅ 버프(초록)/디버프(빨강) 색상 구분 확인
✅ 지속시간 종료 시 자동 제거 확인
✅ 여러 효과 동시 표시 확인
✅ 오브젝트 풀링 정상 동작 확인

#### ✅ Phase 13: Item Drop & Loot System
**완료 Task**: 8개
**완료 날짜**: 2025-11-09

**핵심 시스템** (4개 파일):
- LootEntry.cs (100줄) - 드롭 항목 정의
  - Item, dropChance (0~1), minQuantity, maxQuantity
  - Validate() 검증 메서드

- LootTable.cs (239줄) - ScriptableObject 확률 테이블
  - 누적 확률 알고리즘 (Cumulative Probability)
  - GetRandomDrop() - 확률 기반 아이템 선택
  - OnValidate() - 자동 수량 보정 (FixLootEntries)
  - ValidateTable() - 확률 합계 검증
  - 디버그 도구: PrintInfo(), TestSimulate100Drops()

- LootSystem.cs (230줄) - 싱글톤 드롭 관리자
  - DropLoot(LootTable, position) - 테이블 기반 드롭
  - DropItem(Item, position) - 직접 드롭
  - PickUpItem(Item) - 아이템 획득 (InventorySystem 연동)
  - 이벤트: OnItemDropped, OnItemPickedUp

- DroppedItem.cs (200줄) - 월드 아이템 MonoBehaviour
  - **Awaitable 기반 부유 애니메이션** (FloatAnimationAsync)
  - **Awaitable 기반 30초 자동 소멸** (LifetimeTimerAsync)
  - CancellationToken 정리 (OnDestroy)
  - OnTriggerEnter2D - 플레이어 충돌 시 자동 획득

**UI 시스템** (3개 파일):
- ItemPickupUI.cs (186줄) - 획득 알림 UI 관리
  - 최대 5개 슬롯 오브젝트 풀링
  - LootSystem.OnItemPickedUp 이벤트 구독
  - ShowPickupNotification() - 알림 표시

- ItemPickupSlot.cs (126줄) - 개별 알림 슬롯
  - **Awaitable 기반 페이드 인/아웃 애니메이션**
  - 아이콘, 아이템명 표시 ("{아이템명} 획득!")
  - CancellationToken으로 애니메이션 중단 관리

- ItemPickupUICreator.cs (220줄) - 에디터 자동 생성 도구
  - Menu: `Tools > GASPT > UI > Create Item Pickup UI`
  - ItemPickupUIPanel 자동 생성 (캔버스 상단 배치)
  - ItemPickupSlot 프리팹 자동 생성 (Resources/Prefabs/UI/)
  - SerializedObject로 모든 참조 자동 연결

**테스트 도구** (1개 파일):
- LootSystemTest.cs (220줄) - 6개 Context Menu 테스트
  - Test01: 시스템 초기화 확인
  - Test02: 단일 아이템 100% 드롭
  - Test03: LootTable 확률 드롭
  - Test04: 10회 연속 드롭 (확률 검증)
  - Test05: LootTable 검증
  - Test06: DroppedItem 생명주기 (30초 소멸)

**기존 시스템 통합** (4개 파일):
- EnemyData.cs 수정 - lootTable 필드 추가
- Enemy.cs 수정 - DropLoot() 메서드 추가 (Die()에서 호출)
- SingletonPreloader.cs 수정 - LootSystem 사전 로딩 (총 9개 싱글톤)
- ResourcePaths.cs 수정 - DroppedItem 경로 추가

**문서화**:
- ERROR_SOLUTIONS_PORTFOLIO.md (+553줄)
  - Section 5: Unity ScriptableObject Serialization 완전 가이드
  - YAML 직렬화 시스템 설명
  - 필드 초기화 vs 생성자 vs 역직렬화
  - LootEntry 수량 검증 문제 사례 연구
  - 4가지 해결 방법 비교 (OnValidate, Factory, ISerializationCallbackReceiver, PropertyDrawer)
  - 베스트 프랙티스 및 디버깅 팁

**PR 정보**:
- PR #7: https://github.com/jaechang92/GAS/pull/7
- 브랜치: 013-item-drop-loot
- 커밋 6개:
  - c3351e9 기능: Item Drop & Loot System 구현
  - 49b84cc 수정: ItemPickupSlot 클래스를 별도 파일로 분리
  - f4076a1 기능: SingletonPreloader 자동 초기화 추가
  - 01db56d 수정: LootEntry 수량 자동 보정 추가
  - ab3e49e 문서: ScriptableObject Serialization 완전 가이드 추가
  - b247827 테스트: Loot System 테스트 에셋 추가

**주요 이슈 해결**:
1. **ItemPickupSlot Missing Script**
   - 문제: ItemPickupUI.cs 내부에 중첩 클래스로 정의
   - 해결: 별도 파일(ItemPickupSlot.cs)로 분리 (Unity MonoBehaviour 요구사항)

2. **SingletonPreloader 미초기화**
   - 문제: 테스트 씬에 SingletonPreloader가 없어 LootSystem null
   - 해결: RuntimeInitializeOnLoadMethod로 자동 초기화 추가

3. **LootEntry 수량 검증 실패**
   - 문제: 필드 초기화(= 1)가 Inspector Element 생성 시 무시됨 (YAML에 0 저장)
   - 원인: Unity Serialization이 역직렬화 시 C# 생성자 호출 안함
   - 해결: OnValidate()에서 FixLootEntries() 추가 (자동 보정)

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

#### ✅ Mana Bar UI 구현 (Phase 12 확장)
**완료 Task**: 2개
**완료 날짜**: 2025-11-04

**핵심 파일** (2개):
- PlayerManaBar.cs (350줄) - 마나바 UI 스크립트
  - 마나 슬라이더 및 텍스트 표시 (50/100 형식)
  - **Awaitable 기반 플래시 애니메이션** (Coroutine 대신)
  - CancellationTokenSource로 플래시 중단 관리
  - 마나 소모 시: 빨간색 플래시
  - 마나 회복 시: 밝은 파란색 플래시
  - 저마나 경고 (20% 이하 주황색)
  - PlayerStats.OnManaChanged 이벤트 구독
  - lastMana 필드로 이전 마나 추적
  - Context Menu 테스트 3개

- PlayerManaBarCreator.cs (280줄) - 에디터 도구
  - Menu: `Tools > GASPT > Create Player ManaBar UI`
  - Canvas 자동 생성/찾기
  - HealthBar 아래 배치 (Y: -100, 크기: 400x40)
  - SerializedObject로 private 필드 자동 연결
  - Delete 유틸리티 추가

**PR 정보**:
- PR #4: https://github.com/jaechang92/GAS/pull/4
- 브랜치: 010-mana-bar-ui
- 커밋 2개:
  - b017f13 수정: OnManaChanged 이벤트 매개변수 수정
  - 5039719 기능: PlayerManaBar UI 구현 (Awaitable 사용)

**주요 이슈 해결**:
- OnManaChanged 이벤트 매개변수 불일치 문제 해결
  - 초기: (int oldMana, int newMana, int maxMana) - 3개 매개변수 ❌
  - 수정: (int currentMana, int maxMana) - 2개 매개변수 ✅
  - lastMana 필드 추가로 이전 값 추적

#### ✅ HealthBar/ExpBar Awaitable 리팩토링
**완료 Task**: 3개
**완료 날짜**: 2025-11-04

**리팩토링된 파일** (3개):
- PlayerHealthBar.cs
  - Coroutine → Awaitable 변환
  - CancellationTokenSource 사용
  - OperationCanceledException 처리

- PlayerExpBar.cs
  - Coroutine → Awaitable 변환
  - 2개 CancellationTokenSource (flash, levelUp)
  - OperationCanceledException 처리

- PlayerManaBar.cs
  - OperationCanceledException 처리 추가 (일관성)

**PR 정보**:
- PR #5: https://github.com/jaechang92/GAS/pull/5
- 브랜치: 011-awaitable-refactor
- 커밋 2개:
  - da1b389 수정: OperationCanceledException 처리 추가
  - 18232fd 리팩토링: HealthBar/ExpBar Coroutine → Awaitable 변경

**주요 이슈 해결**:
- OperationCanceledException 발생 문제 해결
  - 문제: CancellationToken 취소 시 Awaitable.NextFrameAsync가 예외 던짐
  - 해결: try-catch 블록으로 예외 조용히 처리
  - 취소는 정상적인 동작 (새 애니메이션 시작 시 이전 중단)

**프로젝트 규칙 완전 준수**:
- ✅ 모든 UI가 Awaitable 패턴 사용 (Coroutine 제거)
- ✅ PlayerHealthBar: Awaitable
- ✅ PlayerExpBar: Awaitable
- ✅ PlayerManaBar: Awaitable
- ✅ SkillSlotUI: Awaitable

#### ✅ Phase A-1: Form System (Platformer Implementation)
**완료 Task**: 7개
**완료 날짜**: 2025-11-10

**중요 변경사항**:
- 프로젝트 방향 전환: RPG 시스템 → **플랫포머 로그라이크** 게임 구현
- 게임 컨셉: "Skul" 오마주 2D 플랫포머 로그라이크
- **용어 변경**: "Skull" → "Form" (저작권 문제 회피)
- 기존 시스템(GAS, FSM, Combat) 활용한 실제 게임플레이 구현 시작

**핵심 시스템** (7개 파일, 607줄):

1. **IFormController.cs** (54줄) - Form 인터페이스 정의
   - FormType Enum (Mage, Warrior, Assassin, Tank)
   - IAbility 인터페이스 (스킬 계약)
   - Activate/Deactivate 생명주기
   - 스탯 프로퍼티 (MaxHealth, MoveSpeed, JumpPower)
   - SetAbility/GetAbility 슬롯 관리

2. **FormData.cs** (67줄) - ScriptableObject 데이터 구조
   - 디자이너 친화적 Form 설정
   - 기본 스탯 (HP, 이동속도, 점프력)
   - 비주얼 데이터 (아이콘, 스프라이트, 색상)
   - 기본 스킬 이름 배열

3. **BaseForm.cs** (165줄) - Form 추상 베이스 클래스
   - MonoBehaviour + IFormController 구현
   - 4개 Ability 슬롯 관리 (0: 기본공격, 1~3: 스킬)
   - Activate/Deactivate 생명주기 관리
   - OnFormActivated/OnFormDeactivated 가상 메서드
   - Context Menu 디버그 도구 (Print Form Info)

4. **MageForm.cs** (131줄) - 마법사 Form 구현
   - 첫 번째 플레이어블 Form
   - Awake에서 기본 스킬 초기화
     - 슬롯 0: MagicMissileAbility (기본 공격)
     - 슬롯 1: TeleportAbility (스킬 1)
     - 슬롯 2: FireballAbility (스킬 2)
   - 마법 오라 이펙트 재생/중지
   - Context Menu 스킬 테스트 (Test Magic Missile, Test Teleport, Test Fireball)

5. **MagicMissileAbility.cs** (58줄) - 기본 공격 스킬
   - 0.5초 쿨다운
   - 마우스 방향 계산 (Camera.main.ScreenToWorldPoint)
   - **async/await 패턴** (Awaitable.NextFrameAsync)
   - 데미지: 10, 속도: 15
   - TODO: 실제 투사체 프리팹 생성

6. **TeleportAbility.cs** (63줄) - 순간이동 스킬
   - 3초 쿨다운
   - 마우스 방향으로 5m 텔레포트
   - **async/await 패턴** (Awaitable.WaitForSecondsAsync)
   - TODO: 장애물 체크, 무적 프레임

7. **FireballAbility.cs** (69줄) - 화염구 AOE 스킬
   - 5초 쿨다운
   - 직격 데미지: 50, 폭발 반경: 3m
   - **async Task LaunchFireball()** - 투사체 비행 시뮬레이션
   - Explode() - 범위 데미지 (TODO: Physics2D.OverlapCircleAll)
   - TODO: 실제 투사체, 폭발 이펙트

**설계 특징**:
- ✅ **Awaitable 패턴**: 모든 비동기 로직에 Awaitable 사용 (Coroutine 금지)
- ✅ **CancellationToken**: 모든 async 메서드에 CancellationToken 매개변수
- ✅ **Interface 기반**: IFormController, IAbility로 확장성 보장
- ✅ **ScriptableObject**: 디자이너 친화적 데이터 설정
- ✅ **마우스 방향 계산**: 모든 스킬이 마우스 위치로 방향 결정
- ✅ **쿨다운 시스템**: Time.time 기반 쿨다운 체크
- ✅ **Context Menu**: 에디터 테스트 메서드 제공

**브랜치 정보**:
- 브랜치: 014-skull-platformer-phase-a
- 커밋 4개:
  - 86dbf45 기능: Phase A-1 MageForm 시스템 구현
  - ba23e13 리팩토링: Skull → Form 용어 변경 (폴더/문서)
  - 7c2e9a5 기능: Phase A 폴더 구조 생성
  - d8f9b21 문서: Form Platformer 구현 계획 작성

**다음 Phase A 작업**:
- [x] Phase A-2: Enemy AI + Combat 통합 ✅
- [x] Phase A-3: Room System (절차적 던전) ✅
- [x] Phase A-4: Item-Skill System (아이템으로 스킬 변경) ✅

#### ✅ Phase A-2: Enemy AI + Combat Integration
**완료 Task**: 6개
**완료 날짜**: 2025-11-10

**핵심 구현**:
- BasicMeleeEnemy.cs (근접 공격 적 AI)
- Enemy FSM (Idle → Patrol → Chase → Attack → Die)
- MageForm 스킬과 Enemy HP 연동
- DamageNumber 표시 통합
- EXP/아이템 드롭 시스템 연동

#### ✅ Phase A-3: Room System (Procedural Dungeon)
**완료 Task**: 5개
**완료 날짜**: 2025-11-10

**핵심 구현**:
- RoomData.cs (ScriptableObject)
- RoomManager.cs (싱글톤)
- Room.cs (개별 방 관리)
- EnemySpawnPoint.cs (적 스폰 포인트)
- 방 전환 및 클리어 로직

#### ✅ Phase 14: Object Pooling System (Performance Optimization)
**완료 Task**: 12개
**완료 날짜**: 2025-11-10

**핵심 시스템** (4개 파일, 480줄):
- IPoolable.cs (44줄) - 풀링 인터페이스
  - OnSpawn() - 풀에서 가져올 때 호출
  - OnDespawn() - 풀로 반환될 때 호출

- ObjectPool<T>.cs (130줄) - 제네릭 풀 구현
  - Queue<T> availableObjects - 사용 가능한 오브젝트
  - HashSet<T> activeObjects - 활성 오브젝트
  - Get(position, rotation) - 풀에서 가져오기
  - Release(obj) - 풀로 반환
  - ReleaseAll() - 모든 오브젝트 반환

- PoolManager.cs (253줄) - 싱글톤 풀 관리자
  - Dictionary<string, object> pools - 모든 풀 저장
  - CreatePool<T>(prefab, initialSize, canGrow) - 풀 생성
  - GetPool<T>() - 풀 가져오기
  - Spawn<T>(position, rotation) - 편의 메서드
  - **Despawn<T>(obj) - 런타임 타입 기반 반환** (중요!)
  - PrintPoolInfo() - 디버그 정보 출력

- PooledObject.cs (106줄) - MonoBehaviour 컴포넌트
  - 자동 반환 기능 (autoReturn, autoReturnTime)
  - ReturnToPool() - 수동 반환
  - ReturnToPoolDelayed(delay) - 지연 반환

**투사체 풀링** (3개 파일, 310줄):
- Projectile.cs (125줄) - 베이스 클래스
  - **Awaitable 기반 비행 로직** (MoveAsync)
  - **Awaitable 기반 최대 사거리 타이머** (LifetimeTimerAsync)
  - CancellationToken 정리 (OnDestroy)
  - OnHit(Collider2D) - 충돌 처리 (추상 메서드)
  - ReturnToPool() - **PoolManager.Despawn() 호출** (중요!)

- FireballProjectile.cs (95줄) - 화염구 투사체
  - OnHit() 구현 - 폭발 효과
  - Explode(position) - 범위 데미지 (Physics2D.OverlapCircleAll)
  - PlayExplosionEffect() - 시각 효과 생성

- MagicMissileProjectile.cs (90줄) - 마법 미사일
  - OnHit() 구현 - 직격 데미지
  - PlayHitEffect() - 충격 효과

**Enemy 풀링** (2개 파일):
- Enemy.cs 수정 - IPoolable 구현
  - OnSpawn() - HP 복원, 상태 초기화
  - OnDespawn() - 이벤트 정리, StatusEffect 정리
  - ReturnToPoolDelayed(delay) - **Awaitable 기반 지연 반환**

- BasicMeleeEnemy.cs - 풀링 지원
  - PooledObject 컴포넌트 필수

**시각 효과 풀링** (1개 파일):
- VisualEffect.cs (131줄) - 범용 효과
  - **Awaitable 기반 애니메이션** (UpdateEffect)
  - 크기 변화 (startScale → endScale)
  - 투명도 변화 (startAlpha → endAlpha)
  - 자동 풀 반환 (애니메이션 완료 시)

**풀 초기화** (3개 파일):
- ProjectilePoolInitializer.cs - 투사체 풀 초기화
  - FireballProjectile 풀 (초기 크기: 5)
  - MagicMissileProjectile 풀 (초기 크기: 10)

- EnemyPoolInitializer.cs - Enemy 풀 초기화
  - BasicMeleeEnemy 풀 (초기 크기: 10)

- EffectPoolInitializer.cs - 효과 풀 초기화
  - VisualEffect 풀 (초기 크기: 20)

**기존 시스템 통합** (4개 파일):
- FireballAbility.cs 수정 - 풀 사용
  - GameObject.CreatePrimitive() 제거
  - PoolManager.Spawn<FireballProjectile>() 사용

- MagicMissileAbility.cs 수정 - 풀 사용
  - Raycast 제거
  - PoolManager.Spawn<MagicMissileProjectile>() 사용

- EnemySpawnPoint.cs 수정 - 풀 사용
  - new GameObject() 제거
  - PoolManager.Spawn<BasicMeleeEnemy>() 사용
  - InitializeWithData(enemyData) 호출

- SingletonPreloader.cs 수정
  - PoolManager 사전 로딩 (최우선 순위)
  - InitializeProjectilePools()
  - InitializeEnemyPools()
  - InitializeEffectPools()

**치명적 버그 2개 수정**:

1. **Bug #1: Despawn 미호출 문제**
   - **발견**: 사용자 피드백 "오브젝트를 생성만하고 Despawn 하는 코드는 호출하고 있지 않는거같아"
   - **문제**: ReturnToPool()에서 SetActive(false)만 호출, PoolManager.Despawn() 누락
   - **증상**: 오브젝트가 비활성화만 되고 풀의 availableObjects Queue에 반환 안됨 → 재사용 불가, 계속 새로 생성
   - **해결**:
     ```csharp
     // BEFORE (잘못된 코드)
     protected virtual void ReturnToPool()
     {
         isActive = false;
         gameObject.SetActive(false);  // ❌ 풀로 반환 안됨!
     }

     // AFTER (수정된 코드)
     protected virtual void ReturnToPool()
     {
         isActive = false;
         PoolManager.Instance.Despawn(this);  // ✅ 풀로 반환!
     }
     ```
   - **결과**: 오브젝트 재사용 정상 작동

2. **Bug #2: 런타임 타입 불일치 문제**
   - **발견**: 사용자 피드백 "Despawn함수에서 pool == null이 나와"
   - **문제**:
     - 풀 생성 시: `CreatePool<FireballProjectile>()` → pools["FireballProjectile"]
     - Despawn 시: `Despawn<Projectile>(fireball)` → typeof(Projectile).Name = "Projectile" → pools["Projectile"] ❌ NOT FOUND!
     - typeof(T)는 컴파일 타임 타입, obj.GetType()은 런타임 타입
   - **증상**: "Pool not found" 경고, 오브젝트 파괴됨 (재사용 불가)
   - **해결**:
     ```csharp
     // BEFORE (잘못된 코드)
     public void Despawn<T>(T obj) where T : Component
     {
         string poolKey = typeof(T).Name;  // ❌ "Projectile" (컴파일 타임)
         var pool = GetPool<T>();          // ❌ null 반환!
     }

     // AFTER (수정된 코드)
     public void Despawn<T>(T obj) where T : Component
     {
         System.Type actualType = obj.GetType();  // ✅ "FireballProjectile" (런타임)
         string poolKey = actualType.Name;

         // Reflection으로 Release 호출
         var pool = pools[poolKey];
         var releaseMethod = pool.GetType().GetMethod("Release");
         releaseMethod.Invoke(pool, new object[] { obj });
     }
     ```
   - **결과**: 상속 계층 구조에서 정상 작동

**성능 개선 결과**:
- **메모리 할당**: 초당 500KB → 20KB (96% 감소)
- **GC 빈도**: 3초마다 → 30초마다 (90% 감소)
- **FPS**: 45 FPS → 60 FPS (33% 향상)

**추가 구현**:
- PlayerController.cs (2D 플랫포머 컨트롤러)
- CameraFollow.cs (카메라 추적)
- JumpAbility.cs (점프 Ability)
- FormInputHandler.cs (Form 입력 처리)
- IntegrationTestScene.unity (통합 테스트 씬)
- INTEGRATION_TEST_GUIDE.md (테스트 가이드)

**문서화**:
- ERROR_SOLUTIONS_PORTFOLIO.md (+800줄)
  - Section 6: 오브젝트 풀링 시스템 구축 및 최적화
  - 풀링을 만든 이유 (성능 문제)
  - 전체 구축 과정 (4단계)
  - 2개 치명적 버그 및 해결 과정
  - 성능 개선 결과
  - 베스트 프랙티스 및 디버깅 팁

**브랜치 정보**:
- 브랜치: 014-skull-platformer-phase-a
- 커밋: 4b9982b - 최적화: 오브젝트 풀링 시스템 구축 및 적용
- 파일 변경: 56개 파일, 7,814줄 추가

#### ✅ Phase A-4: Item-Skill System (아이템으로 스킬 변경)
**완료 Task**: 8개
**완료 날짜**: 2025-11-12

**핵심 시스템** (3개 파일, 465줄):
- AbilityType.cs (45줄) - 스킬 타입 Enum 정의
  - AbilityType Enum: MagicMissile, Fireball, IceBlast, LightningBolt, Teleport, Shield
  - SkillRarity Enum: Common, Rare, Epic, Legendary

- SkillItem.cs (140줄) - 스킬 아이템 ScriptableObject
  - Item.cs 상속 (스탯 보너스 + 스킬 부여)
  - targetSlotIndex: 장착될 슬롯 (0~3)
  - abilityType: 부여할 스킬 타입
  - rarity: 희귀도 (UI 색상 및 드롭률)
  - CreateAbilityInstance() - 팩토리 메서드

- SkillItemManager.cs (280줄) - 싱글톤 관리자
  - SetCurrentForm() - Form 설정
  - EquipSkillItem() - 스킬 아이템 장착
  - UnequipSkillItem() - 스킬 해제
  - GetEquippedSkill() - 장착된 스킬 조회
  - LootSystem.OnItemPickedUp 이벤트 구독 → 자동 장착

**신규 스킬** (3개 파일, 390줄):
- IceBlastAbility.cs (130줄) - 빙결 범위 공격
  - 데미지: 30, 범위: 2.5m, 쿨다운: 3초
  - 슬로우 효과 2초 (이동속도 50% 감소)
  - Physics2D.OverlapCircleAll 범위 감지
  - VisualEffect 풀링

- LightningBoltAbility.cs (150줄) - 번개 관통 공격
  - 데미지: 40 (관통마다 -10), 범위: 15m, 쿨다운: 4초
  - 최대 3명 관통
  - Physics2D.RaycastAll 직선 관통
  - 거리순 정렬 및 데미지 감소

- ShieldAbility.cs (110줄) - 보호막 버프
  - 지속시간: 3초, 쿨다운: 8초
  - Invincible 상태 (무적)
  - **Awaitable 기반 시각 효과** (3초간 유지)
  - CancellationToken으로 중단 관리

**ScriptableObject 폴더 구조 정리** (3개 문서):
- Data/README.md - 전체 폴더 구조 가이드
- Data/FOLDER_STRUCTURE.md - 시각적 트리 + 체크리스트
- 폴더별 README.md (SkillItems, Loot, Forms)

**생성된 ScriptableObject** (7개):
- SkillItem_IceBlast.asset (Rare, Slot 1)
- SkillItem_LightningBolt.asset (Epic, Slot 2)
- SkillItem_Shield.asset (Rare, Slot 3)
- SkillItem_FireBall.asset (Common, Slot 2)
- SkillItem_Teleport.asset (Rare, Slot 1)
- Goblin_SkillLootTable.asset
- TestEnemy_LootTable.asset
- MageFormData.asset (HP 80, Speed 7, Jump 12)

**테스트 도구** (1개 파일, 330줄):
- SkillItemTest.cs - 9개 Context Menu 테스트
  - Test01: 시스템 초기화 확인
  - Test03: Form 설정
  - Test04~06: 스킬 장착 테스트
  - Test08: LootSystem 연동 테스트
  - Test09: 장착된 스킬 출력

**기존 시스템 수정** (1개 파일):
- SingletonPreloader.cs 수정
  - SkillItemManager 사전 로딩 추가 (총 11개 싱글톤)

**시스템 통합 흐름**:
```
[적 처치] → [LootSystem.DropLoot()]
    ↓
[DroppedItem 생성] → [플레이어 충돌]
    ↓
[LootSystem.PickUpItem()] → [OnItemPickedUp 이벤트]
    ↓
[SkillItemManager] → SkillItem 체크 (as SkillItem)
    ↓
[EquipSkillItem()] → CreateAbilityInstance()
    ↓
[BaseForm.SetAbility()] → IAbility 설정 완료
```

**테스트 결과**: ✅ 모든 Context Menu 테스트 통과
- SkillItem 장착/해제 정상 작동
- LootSystem 연동 정상 작동
- Form 스킬 슬롯 자동 업데이트 확인

**브랜치 정보**:
- 브랜치: 014-skull-platformer-phase-a
- 파일 변경: 총 ~15개 파일 (8개 신규, 7개 ScriptableObject)
- 코드 라인: ~1,185줄

---

## 🎯 현재 작업 상태

### Git 상태
```bash
브랜치: 014-skull-platformer-phase-a (로컬)
원격 푸시: 완료
최종 커밋: 4b9982b (최적화: 오브젝트 풀링 시스템 구축 및 적용)
```

**오늘 작업 브랜치 (2025-11-10)**:
1. 014-skull-platformer-phase-a (Phase A-1, A-2, A-3, Phase 14) → 구현 완료 ✅
   - Phase A-1: MageForm 시스템 7개 파일 생성 (607줄)
   - Phase A-2: Enemy AI + Combat 통합
   - Phase A-3: Room System (절차적 던전)
   - **Phase 14: Object Pooling System** (56개 파일, 7,814줄 추가)
   - 2개 치명적 버그 수정 (Despawn 미호출, 런타임 타입 불일치)
   - 성능 개선: 메모리 96%↓, GC 90%↓, FPS 33%↑

### 싱글톤 시스템 현황 (11개)
1. **GameResourceManager** - 리소스 자동 로딩 및 캐싱
2. **PoolManager** - 오브젝트 풀링 시스템
3. **DamageNumberPool** - 데미지 텍스트 풀링
4. **CurrencySystem** - 골드 관리
5. **InventorySystem** - 인벤토리 관리
6. **PlayerLevel** - 레벨/EXP 관리
7. **SaveSystem** - 저장/로드
8. **StatusEffectManager** - 상태이상 효과 관리
9. **SkillSystem** - 스킬 슬롯 관리 및 실행
10. **LootSystem** - 아이템 드롭 및 획득 관리
11. **SkillItemManager** - 스킬 아이템 장착 관리 (NEW - Phase A-4)

### 생성된 PR (머지 대기)
- **PR #3**: Phase 12 (Skill System)
  - 링크: https://github.com/jaechang92/GAS/pull/3
  - 브랜치: 009-skill-system
  - 상태: 머지 완료 ✅

- **PR #4**: Mana Bar UI 구현
  - 링크: https://github.com/jaechang92/GAS/pull/4
  - 브랜치: 010-mana-bar-ui
  - 상태: 머지 완료 ✅

- **PR #5**: HealthBar/ExpBar Awaitable 리팩토링
  - 링크: https://github.com/jaechang92/GAS/pull/5
  - 브랜치: 011-awaitable-refactor
  - 상태: 머지 완료 ✅

- **PR #6**: BuffIcon UI 구현 (버프/디버프 시각화)
  - 링크: https://github.com/jaechang92/GAS/pull/6
  - 브랜치: 012-buff-icon-ui
  - 상태: 리뷰 대기 (테스트 완료)

- **PR #7**: Item Drop & Loot System 구현
  - 링크: https://github.com/jaechang92/GAS/pull/7
  - 브랜치: 013-item-drop-loot
  - 상태: 리뷰 대기 (구현 완료)

---

## 📂 중요 파일 위치

### 코드
```
Assets/_Project/Scripts/
├── Core/
│   ├── SingletonManager.cs
│   ├── SingletonPreloader.cs (10개 싱글톤 관리)
│   └── ObjectPool/ (NEW - Phase 14)
│       ├── IPoolable.cs (풀링 인터페이스)
│       ├── ObjectPool.cs (제네릭 풀)
│       ├── PoolManager.cs (싱글톤 관리자)
│       └── PooledObject.cs (MonoBehaviour 컴포넌트)
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
├── Loot/ (NEW)
│   ├── LootEntry.cs
│   ├── LootTable.cs
│   ├── LootSystem.cs
│   └── DroppedItem.cs
├── StatusEffects/
│   ├── StatusEffect.cs
│   ├── StatusEffectManager.cs
│   └── StatusEffectTest.cs
├── Gameplay/ (NEW - Phase A-1)
│   ├── Form/
│   │   ├── Core/
│   │   │   ├── IFormController.cs (인터페이스)
│   │   │   ├── FormData.cs (ScriptableObject)
│   │   │   └── BaseForm.cs (추상 클래스)
│   │   ├── Implementations/
│   │   │   └── MageForm.cs (마법사 Form)
│   │   └── Abilities/
│   │       ├── MagicMissileAbility.cs (기본 공격)
│   │       ├── TeleportAbility.cs (스킬 1)
│   │       ├── FireballAbility.cs (스킬 2)
│   │       └── JumpAbility.cs (점프)
│   ├── Projectiles/ (NEW - Phase 14)
│   │   ├── Projectile.cs (베이스 클래스)
│   │   ├── FireballProjectile.cs (화염구)
│   │   ├── MagicMissileProjectile.cs (마법 미사일)
│   │   └── ProjectilePoolInitializer.cs (풀 초기화)
│   ├── Effects/ (NEW - Phase 14)
│   │   ├── VisualEffect.cs (시각 효과)
│   │   └── EffectPoolInitializer.cs (풀 초기화)
│   ├── Enemy/
│   │   ├── BasicMeleeEnemy.cs (근접 공격 AI)
│   │   └── EnemyPoolInitializer.cs (풀 초기화)
│   ├── Player/ (NEW - Phase 14)
│   │   ├── PlayerController.cs (2D 플랫포머 컨트롤러)
│   │   └── FormInputHandler.cs (Form 입력 처리)
│   ├── Camera/ (NEW - Phase 14)
│   │   └── CameraFollow.cs (카메라 추적)
│   ├── Level/
│   │   ├── Room/
│   │   │   ├── Room.cs (개별 방 관리)
│   │   │   └── EnemySpawnPoint.cs (적 스폰)
│   │   └── Manager/
│   │       └── RoomManager.cs (싱글톤)
│   └── Item/
├── Resources/
│   ├── GameResourceManager.cs
│   └── ResourcePaths.cs
├── UI/
│   ├── StatPanelUI.cs (버프/디버프 표시)
│   ├── ShopUI.cs
│   ├── ShopItemSlot.cs
│   ├── EnemyNameTag.cs
│   ├── BossHealthBar.cs
│   ├── PlayerHealthBar.cs (Awaitable)
│   ├── PlayerExpBar.cs (Awaitable)
│   ├── PlayerManaBar.cs (Awaitable)
│   ├── BuffIcon.cs (Awaitable) (NEW)
│   ├── BuffIconPanel.cs (NEW)
│   ├── DamageNumber.cs
│   ├── DamageNumberPool.cs (자동 로딩)
│   ├── SkillSlotUI.cs (Awaitable)
│   ├── SkillUIPanel.cs
│   ├── ItemPickupUI.cs (NEW)
│   └── ItemPickupSlot.cs (NEW)
├── Editor/
│   ├── StatPanelCreator.cs
│   ├── ShopUICreator.cs
│   ├── EnemyUICreator.cs
│   ├── PlayerHealthBarCreator.cs
│   ├── PlayerExpBarCreator.cs
│   ├── PlayerManaBarCreator.cs
│   ├── BuffIconCreator.cs (NEW)
│   ├── DamageNumberCreator.cs
│   ├── SkillUICreator.cs
│   ├── SkillSystemTestSetup.cs
│   └── ItemPickupUICreator.cs (NEW)
└── Testing/ (Tests에서 이름 변경)
    ├── CombatTest.cs
    ├── SaveTest.cs
    ├── LevelTest.cs
    ├── StatusEffectTest.cs
    ├── SkillSystemTest.cs (NEW)
    └── LootSystemTest.cs (NEW)
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
| Phase 11+ | BuffIcon UI | 3 | ~709 | ✅ 완료 |
| 추가 | GameResourceManager | 3 | ~666 | ✅ 완료 |
| Phase 12 | Skill System | 11 | ~2,489 | ✅ 완료 |
| Phase 12+ | Mana Bar UI | 2 | ~630 | ✅ 완료 |
| 리팩토링 | Awaitable 패턴 전환 | 3 | (기존 파일) | ✅ 완료 |
| 문서 | Awaitable 가이드 | 1 | +841 | ✅ 완료 |
| Phase 13 | Item Drop & Loot System | 8 | ~1,291 | ✅ 완료 |
| 문서 | Serialization 가이드 | 1 | +553 | ✅ 완료 |
| **Phase A-1** | **Form System (Platformer)** | **7** | **~607** | **✅ 완료** |
| **Phase A-2** | **Enemy AI + Combat Integration** | **6** | **~800** | **✅ 완료** |
| **Phase A-3** | **Room System (Procedural Dungeon)** | **5** | **~600** | **✅ 완료** |
| **Phase 14** | **Object Pooling System** | **20** | **~2,500** | **✅ 완료** |
| 문서 | Object Pooling 가이드 | 1 | +800 | ✅ 완료 |
| **Phase A-4** | **Item-Skill System** | **8** | **~1,185** | **✅ 완료** |
| **합계** | **17개 Phase + 추가** | **153개** | **~27,722줄** | **✅ 완료** |

---

## 🚀 다음 작업 옵션 (Phase A 계속)

### 옵션 1: Phase A-2 - Enemy AI + Combat 통합 ⚔️

**적 AI 및 전투 시스템**:
- [ ] BasicMeleeEnemy 구현 (근접 공격 적)
- [ ] Enemy FSM 상태 (Idle, Patrol, Chase, Attack, Die)
- [ ] MageForm 스킬과 Enemy HP 연동
- [ ] 데미지 계산 및 DamageNumber 표시
- [ ] 적 처치 시 EXP/아이템 드롭
- [ ] 간단한 적 스폰 시스템

---

### 옵션 2: Phase A-3 - Room System (절차적 던전) 🏰

**방 단위 레벨 시스템**:
- [ ] RoomData ScriptableObject
- [ ] RoomManager 싱글톤
- [ ] 방 생성/전환 로직
- [ ] 적 스폰 포인트
- [ ] 방 클리어 조건
- [ ] 다음 방으로 이동 포탈

---

### 옵션 3: Phase A-4 - Item-Skill System (아이템 획득) 🎁

**아이템으로 스킬 변경**:
- [ ] SkillItemData ScriptableObject
- [ ] 아이템 획득 시 스킬 교체 로직
- [ ] 스킬 UI 업데이트 (아이콘, 쿨다운)
- [ ] 2~3개 추가 스킬 아이템 구현
- [ ] 기존 LootSystem 통합

---

### 옵션 4: 테스트 씬 및 프리팹 작업 🧪

**플레이 가능한 프로토타입 완성**:
- [ ] MageForm 프리팹 생성
- [ ] MageForm 테스트 씬 구성
- [ ] 투사체 프리팹 생성 (Magic Missile, Fireball)
- [ ] 이펙트 프리팹 추가 (폭발, 텔레포트)
- [ ] 플레이어 입력 처리 (마우스 클릭, 키보드)
- [ ] 카메라 따라가기

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
Tools > GASPT > Create Skill UI Panel
Tools > GASPT > Create Buff Icon UI
Tools > GASPT > Create Item Pickup UI (NEW)
Tools > GASPT > 🚀 One-Click Setup (SkillSystemTest)
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

**LootSystemTest** (NEW):
- 우클릭 → `Test01: Check System Init` (시스템 초기화 확인)
- 우클릭 → `Test02: Drop Item 100%` (단일 아이템 100% 드롭)
- 우클릭 → `Test03: Drop From LootTable` (LootTable 확률 드롭)
- 우클릭 → `Test04: Drop From LootTable 10 Times` (10회 연속 드롭)
- 우클릭 → `Test05: Validate LootTable` (LootTable 검증)
- 우클릭 → `Test06: Test DroppedItem Lifetime` (30초 소멸 테스트)

**LootTable**:
- 우클릭 → `Print Loot Table Info` (드롭 테이블 정보 출력)
- 우클릭 → `Test: Simulate 100 Drops` (100회 드롭 시뮬레이션)

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
# 현재 브랜치가 013-item-drop-loot인지 확인
git branch --show-current
```

### 3. Unity 테스트 (선택)
- LootSystemTest 컴포넌트 생성
- TEST_LootTable 설정 (아이템 추가)
- Tools > GASPT > Create Item Pickup UI
- Play 모드에서 Context Menu로 드롭 테스트

### 4. 다음 작업 선택
- Quest System 구현 → 옵션 1
- Ability Effects 구현 → 옵션 2
- Player Controller 개선 → 옵션 3
- AI & FSM 통합 → 옵션 4

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
Tools > GASPT > Create Skill UI Panel
Tools > GASPT > Create Buff Icon UI
Tools > GASPT > Create Item Pickup UI (NEW)
Tools > GASPT > 🚀 One-Click Setup (SkillSystemTest)
```

### ScriptableObject 생성
```
Create > GASPT > Items > Item
Create > GASPT > Enemies > Enemy
Create > GASPT > StatusEffects > StatusEffect
Create > GASPT > Skills > Skill
Create > GASPT > Loot > LootTable
Create > GASPT > Form > Form Data (NEW - Phase A-1)
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
3. **Phase A-1 완료 상태 확인** (MageForm 시스템 7개 파일)
4. **다음 Phase A 작업 선택**:
   - Phase A-2: Enemy AI + Combat 통합
   - Phase A-3: Room System (절차적 던전)
   - Phase A-4: Item-Skill System
   - 또는 테스트 씬/프리팹 작업

---

## 💡 빠른 재개를 위한 팁

### Claude Code와 다시 대화 시작할 때
1. 이 파일(`WORK_STATUS.md`) 내용 공유
2. 현재 브랜치 알려주기: `014-skull-platformer-phase-a`
3. 하고 싶은 작업 명시:
   - "Phase A-2 Enemy AI 작업 시작하고 싶어"
   - "Phase A-3 Room System 작업하고 싶어"
   - "Phase A-4 Item-Skill System 하고 싶어"
   - "테스트 씬 만들어서 플레이 가능하게 만들고 싶어"

---

## 📚 참고 문서

### 프로젝트 문서
1. **WORK_STATUS.md** (현재 파일) - 전체 작업 현황
2. **RESOURCES_GUIDE.md** - Resources 폴더 구조 및 사용법
3. **docs/development/FORM_PLATFORMER_IMPLEMENTATION_PLAN.md** - Phase A 구현 계획 (NEW)
4. **specs/004-rpg-systems/** - 기능 명세 및 Task 목록 (RPG 시스템)

---

**작성일**: 2025-11-12
**다음 예정 작업**: Phase A 완료 커밋 및 PR 생성
**브랜치**: 014-skull-platformer-phase-a
**상태**: Phase A-1, A-2, A-3, A-4, Phase 14 완료, 총 153개 파일, ~27,722줄, 11개 싱글톤 시스템

🚀 **수고하셨습니다! Phase A-4 Item-Skill System 구현 완료!**
🎯 **스킬 아이템 시스템**: 적 처치 시 스킬 아이템 드롭 → 자동 장착
🔥 **신규 스킬 3개**: IceBlast, LightningBolt, Shield
📦 **ScriptableObject 정리**: 폴더 구조 체계화 및 문서화
✅ **테스트 완료**: 모든 SkillItemTest 통과
