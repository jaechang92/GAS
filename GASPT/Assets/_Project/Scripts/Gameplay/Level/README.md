# Room System (Level Management)

**작성일**: 2025-11-10
**Phase**: A-3 (Room System - Procedural Dungeon)
**상태**: 완료 ✅

---

## 📖 개요

로그라이크 플랫포머용 방(Room) 단위 레벨 시스템입니다. 각 방은 독립적으로 적 스폰, 클리어 조건, 보상을 관리하며, 플레이어는 포탈을 통해 다음 방으로 이동합니다.

**Skul 스타일**: 방 → 전투 → 클리어 → 포탈 → 다음 방

---

## 🏗️ 아키텍처

### 클래스 구조

```
RoomManager (싱글톤)
    ├── Room[] (Scene의 모든 방)
    └── 방 전환 관리
        ↓
Room (MonoBehaviour)
    ├── RoomData (ScriptableObject)
    ├── EnemySpawnPoint[]
    ├── Portal
    └── 적 스폰/클리어 관리
        ↓
EnemySpawnPoint (MonoBehaviour)
    └── Enemy 생성 위치
        ↓
Portal (MonoBehaviour)
    └── 다음 방 이동
```

---

## 📋 주요 파일

### 1. RoomData.cs (143줄) - ScriptableObject
방 설정 데이터:

```csharp
[CreateAssetMenu(fileName = "RoomData", menuName = "GASPT/Level/Room Data")]
public class RoomData : ScriptableObject
{
    public string roomName;
    public RoomType roomType;           // Start, Normal, Elite, Boss, Rest, Shop, Treasure
    public int difficulty;              // 1~10

    public EnemySpawnData[] enemySpawns;
    public int minEnemyCount;
    public int maxEnemyCount;

    public ClearCondition clearCondition; // KillAllEnemies, Survival, BossKill, Automatic
    public float timeLimit;

    public int bonusGold;
    public int bonusExp;
}
```

**EnemySpawnData**:
```csharp
[System.Serializable]
public class EnemySpawnData
{
    public EnemyData enemyData;
    public float spawnChance;  // 0~1 (0이면 항상 스폰)
    public int minCount;
    public int maxCount;
}
```

### 2. Room.cs (320줄) - MonoBehaviour
방 상태 및 적 스폰 관리:

**주요 기능**:
- 적 스폰 (RoomData 기반)
- 클리어 조건 체크 (적 전멸, 시간 제한)
- 보상 지급 (골드/EXP)
- 이벤트 시스템 (OnRoomEnter, OnRoomClear, OnRoomFail)

**상태 머신**:
```
Inactive → Entering → InProgress → Cleared/Failed
```

**메서드**:
```csharp
public async Awaitable EnterRoomAsync()  // 방 진입
private void SpawnEnemies()              // 적 스폰
private void ClearRoom()                 // 클리어 처리
private void GiveRewards()               // 보상 지급
```

### 3. EnemySpawnPoint.cs (180줄) - MonoBehaviour
적 스폰 위치 마커:

**주요 기능**:
- EnemyData로부터 Enemy GameObject 생성
- BasicMeleeEnemy 동적 생성 (Rigidbody2D, Collider2D 자동 추가)
- Gizmos 시각화
- Context Menu 테스트

**메서드**:
```csharp
public GameObject SpawnEnemy(EnemyData data)
private GameObject CreateEnemyFromData(EnemyData data)
```

### 4. RoomManager.cs (200줄) - 싱글톤
여러 방 관리 및 전환:

**주요 기능**:
- Scene의 모든 Room 자동 탐색
- 방 전환 (다음 방, 특정 방)
- 던전 시작/클리어 관리
- 이벤트 시스템 (OnRoomChanged, OnRoomCleared)

**메서드**:
```csharp
public async Awaitable StartDungeonAsync()
public async Awaitable MoveToNextRoomAsync()
public async Awaitable MoveToRoomAsync(int roomIndex)
```

### 5. Portal.cs (240줄) - MonoBehaviour
다음 방 이동 포탈:

**주요 기능**:
- 플레이어 충돌 감지 (OnTriggerEnter2D)
- 방 클리어 시 자동 활성화
- 포탈 타입 (다음 방, 특정 방, 랜덤 방)
- 비주얼 업데이트 (색상, 이펙트)

**메서드**:
```csharp
public void SetActive(bool active)
private async Awaitable UsePortalAsync()
```

---

## 🧪 테스트 방법

### 1. Unity 에디터 셋업

#### Step 1: RoomData 생성
```
Assets 폴더에서 우클릭:
Create > GASPT > Level > Room Data

설정 예시:
- roomName: "Room 1 - Goblin Nest"
- roomType: Normal
- difficulty: 2
- minEnemyCount: 2
- maxEnemyCount: 4
- clearCondition: KillAllEnemies
- bonusGold: 50
- bonusExp: 20

EnemySpawnData 추가:
[0] enemyData: Goblin, spawnChance: 1, minCount: 2, maxCount: 3
```

#### Step 2: Room GameObject 생성
```
1. Hierarchy에서 우클릭 > Create Empty
2. 이름: "Room_01"
3. 컴포넌트 추가:
   - Room (스크립트)
   - RoomData 할당

4. Room 하위에 Ground 생성:
   - 2D Sprite (Square)
   - BoxCollider2D
   - 스케일 (20, 1, 1)
```

#### Step 3: EnemySpawnPoint 생성
```
Room_01 하위에:
1. Create Empty x 4개
2. 이름: "SpawnPoint_01", "SpawnPoint_02", ...
3. 각각에 EnemySpawnPoint 스크립트 추가
4. 위치 조정 (Ground 위에 배치)
5. Show Gizmos: true

Room 설정:
- Auto Find Spawn Points: true (자동 탐색)
```

#### Step 4: Portal 생성
```
Room_01 하위에:
1. Create Empty
2. 이름: "Portal"
3. 컴포넌트 추가:
   - Portal (스크립트)
   - CircleCollider2D (Trigger 체크)
     - Radius: 1
   - SpriteRenderer (선택사항)

Portal 설정:
- Portal Type: NextRoom
- Auto Activate On Room Clear: true
- Start Active: false
```

#### Step 5: Room 복제 (여러 방 만들기)
```
1. Room_01 복제 → Room_02, Room_03
2. 각 방 위치 조정 (멀리 떨어뜨림)
3. 모든 방 비활성화 (GameObject.SetActive(false))
   - RoomManager가 자동으로 활성화
```

#### Step 6: RoomManager 설정
```
1. Hierarchy에서 빈 GameObject 생성
2. 이름: "RoomManager"
3. RoomManager 스크립트 추가
4. Auto Find Rooms: true (자동 탐색)
```

#### Step 7: Player 배치
```
1. MageForm Player를 Room_01 안에 배치
2. PlayerStats 컴포넌트 확인
```

---

### 2. 테스트 시나리오

#### 테스트 1: 단일 방 테스트
1. Scene에 Room_01만 활성화
2. Play 모드 진입
3. RoomManager 우클릭 > `Start Dungeon (Test)`
4. **기대 동작**:
   - Room_01 활성화 및 진입
   - 적 2~4마리 스폰 (Goblin)
   - Scene 뷰에서 스폰 포인트 Gizmos 확인

#### 테스트 2: 적 전멸 클리어
1. MageForm 스킬로 모든 적 처치
2. **기대 동작**:
   - 마지막 적 사망 시 Room.OnRoomClear 이벤트 발생
   - 보너스 골드 50, 경험치 20 지급
   - Portal 자동 활성화 (시안색)
   - Console에 "방 클리어!" 메시지

#### 테스트 3: Portal 사용
1. Portal에 Player 이동 (충돌)
2. **기대 동작**:
   - Portal.OnPlayerEnter() 호출
   - 0.3초 페이드 연출
   - Room_02로 자동 이동
   - Room_02 적 스폰

#### 테스트 4: 여러 방 순차 진행
1. Room_01 클리어 → Portal → Room_02 → Portal → Room_03
2. **기대 동작**:
   - 각 방마다 독립적으로 적 스폰
   - 보상 누적 (골드, EXP)
   - RoomManager가 현재 방 추적 (currentRoomIndex)

#### 테스트 5: 시간 제한 테스트
1. RoomData에서 timeLimit: 30 설정
2. 30초 동안 적을 처치하지 않음
3. **기대 동작**:
   - 30초 후 Room.OnRoomFail 이벤트 발생
   - Console에 "방 실패: 시간 초과" 메시지

#### 테스트 6: 던전 클리어
1. 모든 방 클리어 (Room_01 → Room_02 → Room_03)
2. Room_03 클리어 후 Portal 사용
3. **기대 동작**:
   - "더 이상 방이 없습니다! (던전 클리어)" 메시지
   - OnDungeonComplete() 호출

---

### 3. Context Menu 디버그

#### Room 우클릭:
```
- Print Room Info         - 방 정보 (상태, 스폰 포인트, 적 수)
- Force Clear Room        - 강제 클리어 (테스트용)
```

#### RoomManager 우클릭:
```
- Print Room List         - 모든 방 목록 및 상태
- Start Dungeon (Test)    - 던전 시작 (Room_01 진입)
- Move To Next Room (Test) - 다음 방으로 이동
```

#### EnemySpawnPoint 우클릭:
```
- Test Spawn              - 적 스폰 테스트 (Play 모드 필수)
- Print Info              - 스폰 포인트 정보
```

#### Portal 우클릭:
```
- Activate Portal         - 포탈 활성화
- Deactivate Portal       - 포탈 비활성화
- Print Portal Info       - 포탈 정보
```

---

## 🔧 주요 설정 값

### 권장 RoomData 설정

| 방 타입 | Difficulty | Min Enemy | Max Enemy | Clear Condition | Bonus Gold | Bonus Exp |
|---------|-----------|-----------|-----------|----------------|------------|-----------|
| Start   | 1 | 0 | 0 | Automatic | 0 | 0 |
| Normal  | 2~4 | 2 | 5 | KillAllEnemies | 50 | 20 |
| Elite   | 5~7 | 3 | 6 | KillAllEnemies | 100 | 50 |
| Boss    | 8~10 | 1 | 1 | BossKill | 200 | 100 |
| Rest    | 0 | 0 | 0 | Automatic | 0 | 0 |

### Portal 설정
- **NextRoom**: 일반적인 순차 진행
- **SpecificRoom**: 분기 경로 (선택지)
- **RandomRoom**: 랜덤 던전

---

## ⚠️ 주의사항

### 1. Room GameObject 비활성화 필수
```
- 모든 Room을 비활성화 상태로 시작
- RoomManager가 자동으로 활성화/비활성화 관리
- 수동으로 활성화하면 충돌 가능
```

### 2. EnemyData 할당
```
- RoomData에 EnemySpawnData 배열 설정 필수
- 또는 EnemySpawnPoint에 개별 EnemyData 설정
- 둘 다 없으면 적 스폰 안 됨
```

### 3. PlayerStats 필수
```
- Portal은 PlayerStats 컴포넌트로 플레이어 감지
- Player GameObject에 PlayerStats 필수
```

### 4. Collider Layer 설정
```
- Portal Collider는 반드시 Trigger 체크
- Player와 충돌하지 않도록 Layer 설정
```

### 5. 현재 제한사항
- **절차적 생성 미구현**: 수동으로 방 배치 필요
- **방 전환 연출 미흡**: 페이드 인/아웃만 0.5초
- **던전 생성 알고리즘 없음**: 방 순서 고정
- **미니맵/방 구조 UI 없음**

---

## 📝 TODO

### Phase A-3 완료 항목 ✅
- [x] RoomData ScriptableObject
- [x] Room MonoBehaviour
- [x] EnemySpawnPoint 컴포넌트
- [x] RoomManager 싱글톤
- [x] Portal 컴포넌트
- [x] 방 클리어 조건 (적 전멸, 시간 제한)
- [x] 보상 지급 (골드, EXP)

### 향후 개선 사항
- [ ] 절차적 던전 생성 (알고리즘)
- [ ] 방 배치 템플릿 (프리팹)
- [ ] 방 전환 연출 개선 (카메라 이동, 페이드)
- [ ] 미니맵 UI
- [ ] 방 타입별 특수 로직 (Rest, Shop, Treasure)
- [ ] 보스 방 특수 연출
- [ ] 분기 경로 (선택지)

---

## 🔗 시스템 통합

### Enemy System (Phase A-2)
- ✅ Enemy.OnDeath 이벤트로 적 사망 감지
- ✅ 골드/EXP 자동 드롭 (Room 보너스와 별도)
- ✅ DamageNumber 표시

### Form System (Phase A-1)
- ✅ MageForm 스킬로 적 공격
- ✅ 방 내부에서 전투

### RPG Systems (Phase 1~13)
- ✅ CurrencySystem - 골드 지급
- ✅ PlayerLevel - EXP 지급
- ✅ StatusEffectManager - 버프/디버프
- ✅ LootSystem - 아이템 드롭 (enemySpawns에 lootTable 설정 시)

---

## 🐛 알려진 이슈

### 1. EnemySpawnPoint.CreateEnemyFromData() Reflection 사용
```csharp
// Reflection으로 private 필드 설정
// 나중에 프리팹 기반으로 교체 필요
var enemyDataField = typeof(GASPT.Enemies.Enemy).GetField("enemyData",
    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
```

### 2. Room.CheckClearConditionAsync() 무한 루프
```csharp
// CancellationToken 미사용 시 메모리 누수 가능
// OnDestroy에서 roomCts.Cancel() 필수
```

### 3. Portal 중복 사용 방지
```csharp
// SetActive(false)로 중복 사용 방지
// 하지만 빠르게 여러 번 진입 시 문제 가능
```

---

## 📚 참고 문서

- **RoomData.cs** - 방 데이터 ScriptableObject
- **Room.cs** - 방 MonoBehaviour
- **EnemySpawnPoint.cs** - 적 스폰 마커
- **RoomManager.cs** - 방 관리 싱글톤
- **Portal.cs** - 포탈 컴포넌트
- **Enemy/README.md** - Enemy 시스템 가이드
- **Form/README.md** - Form 시스템 가이드

---

**최종 업데이트**: 2025-11-10
**작성자**: Phase A-3 Implementation
