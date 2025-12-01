# Phase B 완료 문서 (Playable Prototype)

**작성 날짜**: 2025-11-13
**브랜치**: `015-playable-prototype-phase-b1`
**작업 기간**: 2025-11-12 ~ 2025-11-13

---

## 📋 개요

Phase B는 **플레이 가능한 프로토타입** 구현을 목표로 하며, 다음 3개 서브 Phase로 구성됩니다:

- **Phase B-1**: Playable Prototype Editor Tools (에디터 자동화 도구)
- **Phase B-2**: Enemy Spawn & Combat System (적 스폰 및 전투 시스템)
- **Phase B-3**: UI System Integration (UI 시스템 통합)

모든 Phase B 작업이 완료되어 Unity 에디터에서 **즉시 플레이 가능한 프로토타입**이 완성되었습니다.

---

## ✅ Phase B-1: Playable Prototype Editor Tools

**완료 날짜**: 2025-11-12
**완료 Task**: 2개
**총 코드량**: 1,035줄

### 핵심 파일

#### 1. PrefabCreator.cs (470줄)
**경로**: `Assets/_Project/Scripts/Editor/PrefabCreator.cs`

**기능**:
- 게임에 필요한 모든 프리팹 자동 생성
- Placeholder 스프라이트 자동 생성 (PNG 저장)
- TextureImporter 설정 자동화 (Sprite, PPU 32, Point filter)

**생성 프리팹 목록**:
- `MageForm.prefab` - 플레이어 캐릭터 (PlayerController, FormInputHandler, MageForm)
- `MagicMissileProjectile.prefab` - 마법 투사체 (속도 15, 데미지 10)
- `FireballProjectile.prefab` - 화염구 투사체 (속도 8, 데미지 50, 폭발 반경 3)
- `VisualEffect.prefab` - 범용 시각 효과 (PooledObject)
- `BasicMeleeEnemy.prefab` - 근접 적 (PooledObject, PlatformerEnemy)
- `BuffIcon.prefab` - 버프 아이콘 UI (iconImage, timerFillImage, stackText, timeText, borderImage)
- `PickupSlot.prefab` - 아이템 슬롯 UI (ItemIcon, Quantity)

**주요 기능**:
```csharp
// 전체 프리팹 생성
CreateAllPrefabs()

// 개별 프리팹 생성
CreateMageFormPrefab()
CreateProjectilePrefabs()
CreateVisualEffectPrefab()
CreateBasicMeleeEnemyPrefab()
CreateUIPrefabs() // BuffIcon + PickupSlot
```

**버그 수정**:
- **3D Collider 문제**: `GameObject.CreatePrimitive(Cube)` → 수동 `BoxCollider2D` 추가
- **Sprite 참조 손실**: 메모리 텍스처 → PNG 파일 저장 (TextureImporter 설정)
- **EditorWindow GUI 레이아웃 오류**: `EditorApplication.delayCall` 사용하여 해결

#### 2. GameplaySceneCreator.cs (565줄)
**경로**: `Assets/_Project/Scripts/Editor/GameplaySceneCreator.cs`

**기능**:
- 플레이 가능한 GameplayScene 자동 생성
- 모든 게임 오브젝트 및 시스템 자동 배치
- 2D 플랫포머 환경 자동 구성

**생성 오브젝트 목록**:

1. **카메라 시스템**:
   - Main Camera (Orthographic, Size 10)
   - CameraFollow 컴포넌트 (플레이어 추적)

2. **싱글톤 시스템** (SingletonPreloader):
   - GameResourceManager, PoolManager, DamageNumberPool
   - CurrencySystem, InventorySystem, PlayerLevel
   - SaveSystem, StatusEffectManager, SkillSystem
   - LootSystem, SkillItemManager

3. **레벨 구조** (RoomManager + 3 Rooms):
   - StartRoom (시작 방)
   - Room_1 (일반 방, 적 스폰)
   - BossRoom (보스 방)

4. **환경 오브젝트**:
   - Ground (BoxCollider2D, Ground Layer)
   - Jump Platforms (2~4개/방, Ground Layer)

5. **플레이어**:
   - MageForm 프리팹 인스턴스
   - 시작 위치: (0, 2, 0)
   - Tag: "Player", Layer: Player

6. **적 스폰 포인트**:
   - Room_1, BossRoom에 2~4개씩 배치
   - TestGoblin EnemyData 자동 할당

7. **UI 시스템** (Canvas + EventSystem):
   - PlayerHealthBar (상단 중앙)
   - PlayerManaBar (HealthBar 아래)
   - PlayerExpBar (하단 중앙)
   - BuffIconPanel (좌상단)
   - ItemPickupUI (우하단)
   - RoomInfoUI (우상단)

**주요 기능**:
```csharp
// 전체 씬 생성
CreateGameplayScene()

// 개별 시스템 생성
CreateCamera()
CreateSingletonPreloader()
CreateRoomSystem()
CreatePlayer()
CreateEnemySpawnPoints()
CreateAllUI()
```

### 테스트 문서
- `PHASE_B1_TEST_GUIDE.md` (409줄) - 체크리스트 및 문제 해결 가이드

### 주요 커밋
```bash
e104efe - 수정: 2D Collider 및 32x32 스프라이트 적용
6c47442 - 수정: Placeholder 스프라이트 PNG 저장 및 참조 복구
a44670b - 문서: Phase B-1 테스트 가이드 작성
```

---

## ✅ Phase B-2: Enemy Spawn & Combat System

**완료 날짜**: 2025-11-12
**완료 Task**: 4개
**수정 파일**: 4개 (+107줄)

### 핵심 수정 사항

#### 1. GameplaySceneCreator.cs (+50줄)
**변경사항**:
- EnemySpawnPoint 자동 설정 추가
- TestGoblin EnemyData 자동 로드 및 할당
- 스폰 포인트를 Room GameObject의 자식으로 배치
- `Room.GetComponentsInChildren<EnemySpawnPoint>()` 호환

#### 2. PrefabCreator.cs (+40줄)
**변경사항**:
- Enemy Layer 자동 설정 (BasicMeleeEnemy)
- Projectile targetLayers 자동 설정 (MagicMissile, Fireball)
- Layer 6 "Enemy" 체크 및 경고 메시지

#### 3. RoomManager.cs (+10줄)
**변경사항**:
- `autoStartFirstRoom` 필드 추가 (기본값: true)
- Start()에서 첫 번째 방 자동 진입 로직 추가
- `StartDungeonAsync().Forget()` 자동 호출

#### 4. Room.cs (+17/-17줄)
**변경사항**:
- roomData null 체크 완화
- roomData 없을 때 스폰 포인트 기본 EnemyData 사용
- `SpawnFromSpawnPoints()` 로직 개선

### 시스템 통합 흐름

```
[게임 시작] → [RoomManager.Start()]
    ↓
[autoStartFirstRoom = true] → [StartDungeonAsync()]
    ↓
[StartRoom 진입] → [Room.EnterRoomAsync()]
    ↓
[Room_1 진입] → [SpawnEnemies()]
    ↓
[EnemySpawnPoint] → [PoolManager.Spawn<BasicMeleeEnemy>()]
    ↓
[Enemy 초기화] → [InitializeWithData(TestGoblin)]
    ↓
[플레이어 공격] → [Projectile 발사]
    ↓
[Physics2D.OverlapCircleAll] → [Enemy Layer 감지]
    ↓
[Enemy.TakeDamage()] → [HP 감소]
    ↓
[HP = 0] → [Enemy.Die()] → [DropGold(), GiveExp(), DropLoot()]
    ↓
[풀로 반환] → [PoolManager.Despawn()]
```

### 생성된 에셋
- 7개 Placeholder 텍스처 (PNG)
- 5개 프리팹 (MageForm, 2개 Projectile, BasicMeleeEnemy, VisualEffect)
- GameplayScene.unity (플레이 가능한 씬)

### 테스트 요구사항
1. Unity 에디터에서 "Enemy" Layer 추가 (Layer 6)
2. 프리팹 재생성 (Tools > GASPT > Prefab Creator)
3. GameplayScene 재생성 (Tools > GASPT > Gameplay Scene Creator)

### 테스트 문서
- `PHASE_B2_TEST_GUIDE.md` - 상세 테스트 케이스 및 체크리스트

### 주요 커밋
```bash
447d184 - 기능: Phase B-2 적 스폰 및 전투 시스템 완료
ea44f20 - 문서: Phase B-2 완료 및 테스트 가이드 작성
```

---

## ✅ Phase B-3: UI System Integration

**완료 날짜**: 2025-11-13
**완료 Task**: 5개
**신규/수정 파일**: 7개 (+500줄)

### 핵심 파일

#### 1. RoomInfoUI.cs (168줄) - **신규**
**경로**: `Assets/_Project/Scripts/UI/RoomInfoUI.cs`

**기능**:
- 현재 방 번호 및 총 방 수 실시간 표시
- 적 수 실시간 업데이트
- Unity 초기화 순서 문제 해결 (OnEnable → Start)

**UI 구성**:
- `roomText` - "Room 1 / 3"
- `enemyText` - "Enemies: 3"

**이벤트 구독**:
- `RoomManager.OnRoomChanged` - 방 변경 감지
- `Room.OnEnemyCountChanged` - 적 수 변경 감지

**주요 버그 수정**:
- **Unity 초기화 순서 문제**: OnEnable에서 `RoomManager.HasInstance = false` 발생
- **해결**: OnEnable → Start로 변경하여 싱글톤 Awake 완료 보장

#### 2. GameplaySceneCreator.cs (+318줄)
**변경사항**:
- CreateAllUI() 메서드 확장 (6개 UI 자동 생성)
- CreatePlayerHealthBarUI() - HP 바 생성
- CreatePlayerManaBarUI() - Mana 바 생성
- CreatePlayerExpBarUI() - EXP 바 생성
- CreateBuffIconPanelUI() - 버프 아이콘 패널 생성
- CreateItemPickupUI() - 아이템 획득 UI 생성
- CreateRoomInfoUI() - 방 정보 UI 생성 (NEW)
- CreateUISlider(), CreateUIText() 헬퍼 메서드 추가
- Ground/Platform Layer 설정 추가

#### 3. PrefabCreator.cs (+152줄)
**변경사항**:
- CreateUIPrefabs() 추가 (BuffIcon + PickupSlot)
- CreateBuffIconPrefab() - BuffIcon 컴포넌트 및 모든 UI 참조 설정
- CreatePickupSlotPrefab() - 아이템 슬롯 프리팹 생성

**BuffIcon 구조**:
```
BuffIcon (GameObject + BuffIcon 컴포넌트)
├─ Border (Image) - 테두리 (버프/디버프 색상)
├─ Background (Image) - 어두운 배경
├─ Icon (Image) - 버프/디버프 아이콘
├─ TimerFill (Image, Radial360) - 타이머 시각화
├─ StackCount (TextMeshProUGUI) - 스택 수 (우하단)
└─ TimeText (TextMeshProUGUI) - 남은 시간 (중앙 하단)
```

#### 4. 기존 UI 컴포넌트 검증 ✅

**PlayerHealthBar.cs** (390줄):
- PlayerStats 이벤트 구독: OnDamaged, OnHealed, OnDeath, OnStatChanged
- Start()에서 PlayerStats 자동 검색
- HP 바 및 텍스트 실시간 업데이트
- 데미지/회복 플래시 효과
- HP 비율에 따른 색상 변화

**PlayerManaBar.cs** (유사 구조):
- PlayerStats.OnManaChanged 이벤트 구독
- Mana 소모/회복 플래시 효과
- 저마나(20% 이하) 색상 경고

**PlayerExpBar.cs** (유사 구조):
- PlayerLevel.OnExpChanged, OnLevelUp 이벤트 구독
- EXP 획득 골드색 플래시
- 레벨업 노란색 애니메이션

**BuffIconPanel.cs**:
- StatusEffectManager 이벤트 구독
- 버프 아이콘 풀링 (최대 10개)
- 버프/디버프 자동 업데이트

### 주요 버그 수정

#### 버그 1: RoomInfoUI 적 수 미업데이트
**문제**: 적 처치 시 RoomInfoUI의 Enemies 수가 감소하지 않음

**원인**: Unity 초기화 순서 문제
```
OnEnable (RoomInfoUI) → Awake (RoomManager) → Start (RoomManager)
```
- RoomInfoUI.OnEnable() 시점에 `RoomManager.instance = null`
- `HasInstance = false` → 이벤트 구독 실패

**해결**:
- OnEnable/OnDisable → Start/OnDestroy로 변경
- Start()는 모든 Awake() 완료 후 실행 → 싱글톤 보장

#### 버그 2: RoomManager 방 순서 랜덤
**문제**: autoFindRooms로 방 초기화 시 순서 랜덤 (BossRoom, StartRoom, Room_1 무작위)

**원인**: `FindObjectsByType(..., FindObjectsSortMode.None)`

**해결**: `SortRooms()` 메서드 추가
```csharp
rooms.Sort((a, b) =>
{
    if (a.name.Contains("StartRoom")) return -1;
    if (b.name.Contains("StartRoom")) return 1;
    if (a.name.Contains("BossRoom")) return 1;
    if (b.name.Contains("BossRoom")) return -1;
    return a.transform.position.x.CompareTo(b.transform.position.x);
});
```

#### 버그 3: Enemy 컴포넌트 중복
**문제**: BasicMeleeEnemy.Prefab에 Enemy.cs와 BasicMeleeEnemy.cs 동시 존재

**해결**:
- Enemy.cs를 `abstract class`로 변경
- PrefabCreator에서 Enemy 컴포넌트 추가 제거

#### 버그 4: EditorWindow GUI 레이아웃 오류
**문제**: "EndLayoutGroup: BeginLayoutGroup must be called first" 오류

**원인**: OnGUI() 버튼 클릭 시 heavy 메서드 즉시 호출 → IMGUI 레이아웃 스택 손상

**해결**: `EditorApplication.delayCall` 사용하여 다음 프레임까지 실행 지연

### 주요 커밋
```bash
2447fc7 - 수정: BuffIcon 프리팹에 BuffIcon 컴포넌트 추가
3fbec73 - 기능: BuffIcon 및 PickupSlot UI 프리팹 생성 기능 추가
b04b858 - 설정: 플레이어 및 씬 테스트 설정 업데이트
d9b13a0 - 수정: RoomInfoUI Unity 초기화 순서 문제 해결
20045f6 - 업데이트: Phase B-3 완료 후 GameplayScene 최종 상태
e13c11f - 문서: EditorWindow GUI 레이아웃 오류 포트폴리오 추가
e67dceb - 수정: EditorWindow GUI 레이아웃 오류 해결
475291f - 기능: Phase B-3 UI 시스템 통합 및 Ground Layer 설정
1f0e4cf - 수정: RoomManager 방 순서 정렬 및 Enemy abstract class 변경
```

---

## 📊 Phase B 전체 통계

### 작업량 요약
| Phase | 설명 | 파일 수 | 코드량 | 상태 |
|-------|------|---------|--------|------|
| **B-1** | Playable Prototype Editor Tools | 2개 신규 | 1,035줄 | ✅ 완료 |
| **B-2** | Enemy Spawn & Combat System | 4개 수정 | +107줄 | ✅ 완료 |
| **B-3** | UI System Integration | 7개 신규/수정 | +500줄 | ✅ 완료 |
| **총합** | **Phase B 전체** | **13개** | **~1,642줄** | **✅ 완료** |

### 생성된 에셋
- **7개** Prefab 파일
- **7개** PNG Placeholder 텍스처
- **1개** GameplayScene.unity
- **3개** 테스트 문서 (B1, B2, B-Complete)
- **1개** 에러 솔루션 포트폴리오 추가 (Section 7)

### 수정된 버그
| 번호 | 버그 내용 | 심각도 | 해결 방법 |
|------|-----------|--------|-----------|
| 1 | 3D Collider 문제 (GameObject.CreatePrimitive) | 높음 | 수동 BoxCollider2D 추가 |
| 2 | Sprite 참조 손실 (메모리 텍스처) | 높음 | PNG 파일 저장 + TextureImporter |
| 3 | EditorWindow GUI 레이아웃 오류 | 중간 | EditorApplication.delayCall |
| 4 | RoomInfoUI 적 수 미업데이트 | 높음 | OnEnable → Start (초기화 순서) |
| 5 | RoomManager 방 순서 랜덤 | 중간 | SortRooms() 메서드 추가 |
| 6 | Enemy 컴포넌트 중복 | 낮음 | abstract class + PrefabCreator 수정 |

---

## 🎮 플레이 테스트 체크리스트

### 필수 설정 (Unity 에디터)
- [ ] Layer 6을 "Enemy"로 추가 (Project Settings > Tags and Layers)
- [ ] Layer 7을 "Player"로 추가
- [ ] Layer 8을 "Ground"로 추가

### 프리팹 생성
- [ ] Tools > GASPT > Prefab Creator 실행
- [ ] "🚀 모든 프리팹 생성" 버튼 클릭
- [ ] 생성 확인: `Assets/Resources/Prefabs/` 폴더

### 씬 생성
- [ ] Tools > GASPT > Gameplay Scene Creator 실행
- [ ] "🚀 GameplayScene 생성" 버튼 클릭
- [ ] 생성 확인: `Assets/_Project/Scenes/GameplayScene.unity`

### 게임플레이 테스트

#### 1. 플레이어 컨트롤
- [ ] A/D 키로 좌우 이동
- [ ] Space 키로 점프
- [ ] 플랫폼에 정상 착지
- [ ] 카메라가 플레이어 추적

#### 2. UI 시스템
- [ ] **HealthBar**: 상단 중앙에 HP 바 표시
- [ ] **ManaBar**: HealthBar 아래 Mana 바 표시
- [ ] **ExpBar**: 하단 중앙에 EXP 바 및 레벨 표시
- [ ] **RoomInfoUI**: 우상단에 "Room 1 / 3" 및 "Enemies: X" 표시
- [ ] **BuffIconPanel**: 좌상단 (현재 버프 없음)
- [ ] **ItemPickupUI**: 우하단 (현재 아이템 없음)

#### 3. 전투 시스템
- [ ] 마우스 좌클릭으로 Fireball 발사
- [ ] Room_1에 적 2~4마리 스폰
- [ ] Fireball이 적에게 명중
- [ ] 적 HP 감소 + DamageNumber 표시
- [ ] 적 사망 시 골드/EXP 획득
- [ ] **RoomInfoUI의 Enemies 수 감소** (3 → 2 → 1 → 0)
- [ ] 적 풀로 반환 (1초 후)

#### 4. 방 시스템
- [ ] StartRoom에서 시작
- [ ] Room_1에 적 스폰
- [ ] 모든 적 처치 시 방 클리어 메시지
- [ ] RoomInfoUI가 "Room 2 / 3"으로 변경 (다음 방 이동 시)

#### 5. 레벨 & EXP 시스템
- [ ] 적 처치 시 EXP Number 표시 (+50 EXP)
- [ ] ExpBar 증가
- [ ] 레벨업 시 레벨 텍스트 애니메이션
- [ ] 레벨업 시 HP 완전 회복

#### 6. HP & 데미지 시스템
- [ ] 적 공격 받을 시 HP 감소
- [ ] HealthBar 빨간색 플래시
- [ ] DamageNumber 표시
- [ ] HP 0 시 사망 처리

### 성능 테스트
- [ ] FPS 30+ 유지
- [ ] 메모리 사용량 안정
- [ ] GC 호출 최소화 (풀링 시스템)

---

## 🔧 트러블슈팅

### 문제 1: "Enemy" Layer가 없습니다
**증상**: Console에 경고 메시지 출력

**해결**:
1. Edit > Project Settings > Tags and Layers
2. Layer 6을 "Enemy"로 설정
3. 프리팹 재생성

### 문제 2: 적이 스폰되지 않음
**원인**: EnemyData가 할당되지 않음

**해결**:
1. `Assets/_Project/Data/Enemies/TestGoblin.asset` 존재 확인
2. GameplayScene 재생성

### 문제 3: UI가 표시되지 않음
**원인**: PlayerStats 또는 PlayerLevel이 씬에 없음

**해결**:
1. Player GameObject에 PlayerStats 컴포넌트 추가
2. SingletonPreloader에서 PlayerLevel 자동 생성 확인

### 문제 4: RoomInfoUI 적 수가 업데이트되지 않음
**해결**: 이미 수정 완료 (Start()로 초기화 순서 보장)

### 문제 5: Projectile이 적을 감지하지 못함
**원인**: targetLayers 설정 오류

**해결**:
1. 프리팹 재생성 (Layer 자동 설정)
2. 적 Layer가 "Enemy"인지 확인

---

## 📈 다음 단계 권장사항

### 옵션 1: Phase B 완료 및 Master 병합 ✅
**권장 작업**:
1. 모든 변경사항 커밋
2. PR 생성 및 리뷰
3. Master 병합
4. Phase C 기획 시작

### 옵션 2: Phase B-4 - 다양한 적 추가
**작업 내용**:
- RangedEnemy (원거리 적)
- FlyingEnemy (비행 적)
- BossEnemy (보스 적)
- 적별 고유 패턴 및 AI

### 옵션 3: Phase B-5 - 추가 Form 구현
**작업 내용**:
- WarriorForm (전사 폼)
- RogueForm (도적 폼)
- Form 전환 시스템

---

## 📝 결론

Phase B (Playable Prototype)의 모든 서브 Phase가 성공적으로 완료되었습니다:

- ✅ **Phase B-1**: 에디터 자동화 도구 완성
- ✅ **Phase B-2**: 적 스폰 및 전투 시스템 완성
- ✅ **Phase B-3**: UI 시스템 완전 통합

Unity 에디터에서 **Tools > GASPT > Gameplay Scene Creator**를 실행하면 즉시 플레이 가능한 프로토타입이 생성됩니다.

모든 시스템이 통합되어 다음 항목들이 정상 작동합니다:
- 플레이어 이동 및 점프
- 적 스폰 및 AI
- 전투 시스템 (투사체, 데미지, 사망)
- UI 시스템 (HP, Mana, EXP, 방 정보, 적 수)
- 레벨 & EXP 시스템
- 오브젝트 풀링 시스템

**Phase B 완료! 🎉**

---

**작성자**: Claude Code
**최종 수정**: 2025-11-13
