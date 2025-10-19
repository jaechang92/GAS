# GASPT 로그라이크 플랫포머 개발 로드맵

**프로젝트**: GASPT (Generic Ability System + FSM Platform Game)
**장르**: 로그라이크 플랫포머 (Skul: The Hero Slayer 스타일)
**현재 버전**: 0.7.5
**현재 Phase**: 2 (85% 완료)
**전체 완료도**: 75%

---

## 📊 현재 상태 (2025-10-19)

### ✅ 완료된 시스템 (Phase 1-2)

#### Core 시스템 (100%)
- ✅ GAS (Gameplay Ability System)
- ✅ FSM (Finite State Machine)
- ✅ GameFlow System
- ✅ Scene Management
- ✅ Resource Management

#### Combat & Physics (85%)
- ✅ Combat System (DamageSystem, HealthSystem, ComboSystem)
- ✅ CharacterPhysics (Skul 스타일 물리)
- ✅ Player System (FSM 상태 기반)
- ✅ Enemy AI System
- ✅ Hitbox/Hurtbox System
- ⚠️ GAS-Combat 통합 (Unity 에디터 작업 대기)

#### UI/UX (40%)
- ✅ Panel 기반 UI 시스템
- ✅ MainMenu, Loading, GameplayHUD, Pause Panel
- ✅ Dialogue 시스템
- ✅ NPC 시스템
- ⚠️ UI Prefab 재생성 필요

#### 개발 도구
- ✅ GASPT 통합 메뉴 시스템
- ✅ Prefab 자동 생성 도구
- ✅ Portal 시스템

---

## 🎯 Phase 2 완료 작업 (즉시 해야 할 일)

### 우선순위 1: Unity 에디터 작업
**예상 시간**: 1-2시간

1. **GAS-Combat 통합 완료**
   - ComboAbilityData ScriptableObject 3개 생성
   - PlayerController에 할당
   - 테스트 실행

2. **UI Prefab 재생성**
   - 기존 Prefab 삭제
   - GASPT → Prefabs → UI Panels → Create All Panels
   - Bootstrap 씬 테스트

3. **Portal 배치**
   - Lobby 씬에 Portal 배치
   - Lobby → Gameplay 전환 테스트

---

## 🚀 Phase 3: 로그라이크 콘텐츠 확장 (0% → 100%)

### 3.1 절차적 레벨 생성 시스템 (🔥 최우선)
**예상 시간**: 2-3주
**완료도**: 0%

#### Step 1: Room 시스템 (1주)
- [ ] **RoomData ScriptableObject**
  - 방 크기, 난이도, 적 스폰 포인트
  - 보상 정보 (골드, 아이템)
  - 입구/출구 위치

- [ ] **Room Generator**
  - Tilemap 기반 방 생성
  - 랜덤 플랫폼 배치
  - 장애물 배치 (가시, 함정 등)

- [ ] **Room Types**
  - NormalRoom: 일반 전투 방
  - EliteRoom: 엘리트 적 방
  - TreasureRoom: 보물 방
  - ShopRoom: 상점 방
  - BossRoom: 보스 방
  - RestRoom: 휴식 방

#### Step 2: Dungeon Generator (1주)
- [ ] **Dungeon Layout**
  - Graph 기반 방 연결 (BSP, Cellular Automata 등)
  - 시작 방 → 보스 방까지의 경로 보장
  - 선택적 경로 (보물, 상점 등)

- [ ] **Room Transition**
  - 문/포탈을 통한 방 이동
  - 현재 방 정리 (적 제거) 후 이동 가능
  - LoadingPanel로 전환 효과

- [ ] **Minimap System**
  - 방문한 방 표시
  - 현재 위치 표시
  - 방 타입별 아이콘

#### Step 3: 난이도 조절 (3-5일)
- [ ] **Progressive Difficulty**
  - 층수에 따른 적 강화
  - 적 종류 증가
  - 보상 증가

- [ ] **Scaling System**
  - 플레이어 레벨에 따른 조절
  - 메타 진행도 반영

---

### 3.2 아이템 시스템 (🔥 높은 우선순위)
**예상 시간**: 2-3주
**완료도**: 0%

#### Step 1: 아이템 데이터 구조 (3-5일)
- [ ] **ItemData ScriptableObject**
  - ItemType: Weapon, Armor, Consumable, Passive
  - Rarity: Common, Rare, Epic, Legendary
  - Stats: 공격력, 방어력, 체력 등
  - Special Effects (GAS 통합)

- [ ] **ItemDatabase**
  - 모든 아이템 등록
  - 희귀도별 풀 관리
  - 랜덤 아이템 선택

#### Step 2: 인벤토리 시스템 (1주)
- [ ] **Inventory Manager**
  - 아이템 추가/제거
  - 장착 시스템
  - 슬롯 제한
  - 아이템 정렬

- [ ] **Inventory UI**
  - 그리드 기반 UI
  - 드래그 앤 드롭
  - 아이템 툴팁
  - 장착 슬롯 시각화

#### Step 3: 드롭 시스템 (3-5일)
- [ ] **Loot System**
  - 적 처치 시 드롭
  - 보물 상자
  - 희귀도 기반 확률
  - 드롭 테이블

- [ ] **Item Pickup**
  - 바닥에 떨어진 아이템
  - E키 상호작용
  - 자동 습득 옵션

#### Step 4: 장비 시스템 (1주)
- [ ] **Equipment Slots**
  - 무기, 방어구, 악세서리
  - 스탯 적용
  - 장비 효과 (GAS Ability)

- [ ] **Synergy System**
  - 세트 아이템 보너스
  - 조합 효과

---

### 3.3 스컬 교체 시스템 (🎯 핵심 메커니즘)
**예상 시간**: 3-4주
**완료도**: 0%

#### Step 1: 스컬 데이터 구조 (1주)
- [ ] **SkullData ScriptableObject**
  - 기본 스탯
  - 고유 스킬 (GAS Ability)
  - 애니메이션 세트
  - 스프라이트 세트

- [ ] **Skull Types**
  - Basic: 기본 스컬
  - Warrior: 근접 전투형
  - Mage: 원거리 마법형
  - Assassin: 빠른 공격형
  - Tank: 방어형

#### Step 2: 교체 시스템 (1-2주)
- [ ] **Skull Manager**
  - 현재 스컬 관리
  - 소유 스컬 목록
  - 교체 로직 (Q키)
  - 쿨다운 관리

- [ ] **Transform System**
  - 스프라이트 교체
  - 애니메이션 교체
  - 스탯 적용
  - GAS Ability 교체

- [ ] **Transform Effect**
  - VFX 효과
  - 사운드 효과
  - 무적 시간

#### Step 3: 각성 시스템 (1주)
- [ ] **Awakening System**
  - 조건: 골드, 아이템 등
  - 각성 스컬 (강화 버전)
  - 추가 스킬
  - 스탯 대폭 증가

- [ ] **Awakening UI**
  - 각성 선택 화면
  - 스킬 미리보기
  - 확인/취소

---

### 3.4 메타 진행 시스템
**예상 시간**: 1-2주
**완료도**: 0%

#### Step 1: 영구 업그레이드 (1주)
- [ ] **Meta Currency**
  - 뼈 (Bone): 플레이 중 획득
  - 영혼 (Soul): 보스 처치 시 획득

- [ ] **Upgrade Tree**
  - 체력 증가
  - 공격력 증가
  - 새로운 스컬 해금
  - 시작 아이템 해금

- [ ] **Persistence System**
  - PlayerPrefs 또는 JSON 저장
  - 업그레이드 상태 저장
  - 해금 스컬 저장

#### Step 2: 업적/도전 과제 (3-5일)
- [ ] **Achievement System**
  - 조건 체크
  - 보상 지급
  - UI 표시

- [ ] **Daily/Weekly Challenges**
  - 랜덤 도전 과제
  - 추가 보상

---

## 📅 Phase 4: UI/UX 완성 (40% → 100%)

### 4.1 게임플레이 UI (2주)
**예상 시간**: 2주

- [ ] **InventoryPanel**
  - 그리드 UI
  - 드래그 앤 드롭
  - 장비 슬롯

- [ ] **SkullPanel**
  - 소유 스컬 목록
  - 스킬 정보
  - 각성 버튼

- [ ] **MapPanel**
  - 던전 미니맵
  - 방 정보
  - 목표 표시

- [ ] **ShopPanel**
  - 아이템 구매
  - 골드 표시
  - 희귀도별 필터

- [ ] **UpgradePanel** (메타 진행)
  - 업그레이드 트리
  - 비용 표시
  - 미리보기

### 4.2 HUD 개선 (1주)
- [ ] 스컬 정보 (현재 스컬, 교체 쿨다운)
- [ ] 층수/방 정보
- [ ] 보유 골드/뼈/영혼
- [ ] 버프/디버프 아이콘
- [ ] 보스 체력바

### 4.3 효과 및 피드백 (1주)
- [ ] 데미지 숫자 표시
- [ ] 크리티컬 효과
- [ ] 레벨업 효과
- [ ] 아이템 획득 알림
- [ ] 스킬 쿨다운 표시

---

## 🎨 Phase 5: 비주얼 및 사운드 (현재 미정)

### 5.1 VFX 시스템
- [ ] 공격 이펙트
- [ ] 스킬 이펙트
- [ ] 히트 이펙트
- [ ] 환경 이펙트
- [ ] 파티클 시스템 최적화

### 5.2 사운드 시스템
- [ ] BGM 시스템
- [ ] SFX 시스템
- [ ] 사운드 풀링
- [ ] 믹서 그룹 설정

### 5.3 애니메이션
- [ ] 플레이어 애니메이션 세트
- [ ] 적 애니메이션 세트
- [ ] UI 애니메이션
- [ ] Transition 효과

---

## ⚡ Phase 6: 최적화 및 배포 (0% → 100%)

### 6.1 최적화 (2-3주)
- [ ] **Object Pooling**
  - 적, 투사체, 이펙트
  - 아이템 드롭

- [ ] **Level Streaming**
  - 방 단위 로드/언로드
  - Addressables 시스템

- [ ] **메모리 최적화**
  - 텍스처 압축
  - 오디오 압축
  - GC 최소화

- [ ] **성능 프로파일링**
  - CPU 프로파일링
  - GPU 프로파일링
  - 메모리 프로파일링

### 6.2 밸런싱 (2주)
- [ ] 난이도 곡선 조절
- [ ] 아이템 밸런스
- [ ] 스컬 밸런스
- [ ] 보상 밸런스
- [ ] 플레이 테스트

### 6.3 배포 준비 (1주)
- [ ] 빌드 최적화
- [ ] 플랫폼별 테스트 (Windows, Mac, Linux)
- [ ] 세이브/로드 시스템 안정화
- [ ] 크래시 리포팅

---

## 📈 단계별 우선순위

### 🔥 최우선 (즉시 시작)
1. Phase 2 완료 (Unity 에디터 작업)
2. 절차적 레벨 생성 (Room 시스템)
3. 기본 아이템 시스템

### 🎯 높은 우선순위 (Phase 3 시작)
1. 스컬 교체 시스템 (핵심 메커니즘)
2. 인벤토리 UI
3. 던전 생성기

### ⭐ 중간 우선순위
1. 메타 진행 시스템
2. 업적 시스템
3. 상점 시스템

### 💎 낮은 우선순위 (마지막)
1. VFX/사운드
2. 애니메이션 폴리싱
3. 추가 콘텐츠

---

## 🗓️ 예상 일정

### Phase 2 완료 (현재)
**기간**: 1-2일
**작업**: Unity 에디터 작업

### Phase 3-1: 기본 로그라이크 메커니즘
**기간**: 6-8주
**작업**:
- Week 1-2: Room 시스템
- Week 3-4: Dungeon Generator
- Week 5-6: 기본 아이템 시스템
- Week 7-8: 스컬 교체 시스템 기초

### Phase 3-2: 콘텐츠 확장
**기간**: 4-6주
**작업**:
- Week 9-10: 스컬 교체 완성
- Week 11-12: 아이템 확장
- Week 13-14: 메타 진행

### Phase 4: UI/UX 완성
**기간**: 3-4주
**작업**:
- Week 15-16: 게임플레이 UI
- Week 17: HUD 개선
- Week 18: 효과 및 피드백

### Phase 5-6: 비주얼, 최적화, 배포
**기간**: 4-6주
**작업**:
- Week 19-20: VFX/사운드
- Week 21-22: 최적화
- Week 23-24: 밸런싱 및 배포

**총 예상 기간**: 약 6개월 (24주)

---

## 💡 개발 팁

### 로그라이크 핵심 원칙
1. **Run 기반 플레이**: 한 번 죽으면 처음부터
2. **절차적 생성**: 매번 다른 던전
3. **영구 진행**: 메타 업그레이드로 점진적 강화
4. **높은 재플레이성**: 다양한 빌드, 스컬, 아이템 조합

### Skul 스타일 참고 포인트
- 빠른 전투 템포
- 다양한 스컬 (50개 이상)
- 스킬 조합 (시너지)
- 각성 시스템
- 화려한 VFX

### 개발 순서
1. **플레이어블 빌드 우선**: 기본 루프 먼저 완성
2. **콘텐츠는 나중에**: 시스템 먼저, 데이터는 나중에
3. **밸런싱은 마지막**: 시스템 완성 후 조절
4. **테스트 플레이**: 매주 플레이 가능한 상태 유지

---

## 📚 참고 자료

### Unity Asset Store
- **ProBuilder**: 레벨 디자인
- **DOTween**: 애니메이션
- **Odin Inspector**: 에디터 확장
- **Addressables**: 리소스 관리

### 오픈소스 참고
- Dungeon Generation 알고리즘
- Roguelike Toolkit
- Item System Templates

### 학습 자료
- Skul GDC 발표 (게임 디자인)
- Roguelike 디자인 패턴
- Procedural Generation 기법

---

## 🎯 마일스톤

### Milestone 1: Playable Prototype (Phase 3-1 완료)
- 기본 던전 생성
- 스컬 교체 동작
- 아이템 획득/사용
- 1층 클리어 가능

### Milestone 2: Core Loop Complete (Phase 3-2 완료)
- 5층 던전
- 10개 스컬
- 50개 아이템
- 메타 진행

### Milestone 3: Content Complete (Phase 4 완료)
- 모든 UI 완성
- 모든 스컬 구현
- 밸런싱 완료

### Milestone 4: Release (Phase 6 완료)
- 최적화 완료
- 버그 수정
- 배포 준비

---

**작성일**: 2025-10-19
**다음 업데이트**: Phase 2 완료 후
**관련 문서**:
- `docs/development/CURRENT_WORK.md`
- `docs/development/GAS_COMBAT_INTEGRATION_DESIGN.md`
- `.spec/project.yaml`
