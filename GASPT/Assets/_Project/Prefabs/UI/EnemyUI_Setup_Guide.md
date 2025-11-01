# Enemy UI 설정 가이드

Unity Editor에서 EnemyNameTag와 BossHealthBar UI 프리팹을 생성하는 방법입니다.

---

## 1. EnemyNameTag (Named 적용 이름표)

### 목적
Named 적 위에 이름을 표시하는 3D 월드 스페이스 UI

### UI 구조
```
EnemyNameTag (Canvas - World Space)
└── NameText (TextMeshPro)
    └── Background (Image, 선택 사항)
```

### 생성 방법

#### 1-1. Canvas 생성

1. Hierarchy 우클릭 → `UI` → `Canvas` 생성
2. 이름: `EnemyNameTag`
3. Canvas 설정:
   - Render Mode: **World Space**
   - Width: 200
   - Height: 50
   - Scale: (0.01, 0.01, 0.01)
   - Sorting Layer: (기본값 또는 UI 전용 레이어)

#### 1-2. NameText 생성

1. EnemyNameTag 하위에 `UI` → `Text - TextMeshPro` 생성
2. 이름: `NameText`
3. RectTransform:
   - Anchor: Stretch (가로/세로 모두)
   - Left: 0, Right: 0, Top: 0, Bottom: 0
4. TextMeshProUGUI:
   - Text: "Elite Orc" (예시)
   - Font Size: 24
   - Color: Yellow (R:255, G:200, B:0)
   - Alignment: Center, Middle
   - Auto Size: (선택 사항) Min: 18, Max: 24

#### 1-3. Background (선택 사항)

1. NameText 하위에 `UI` → `Image` 생성
2. 이름: `Background`
3. RectTransform:
   - Anchor: Stretch
   - Left: -10, Right: -10, Top: -5, Bottom: -5
4. Image:
   - Color: 검은색 반투명 (R:0, G:0, B:0, A:150)
5. Hierarchy에서 Background를 NameText 위로 이동 (배경이 되도록)

#### 1-4. EnemyNameTag 스크립트 추가

1. EnemyNameTag GameObject 선택
2. `Add Component` → `EnemyNameTag` 스크립트 추가
3. Inspector 설정:
   - **Name Text**: NameText 드래그
   - **Background Image**: Background 드래그 (있는 경우)
   - **Face Camera**: ✅ (체크)
   - **Vertical Offset**: 1.5

#### 1-5. 프리팹으로 저장

1. EnemyNameTag를 `Assets/_Project/Prefabs/UI/` 폴더로 드래그
2. 프리팹 이름: `EnemyNameTag`
3. Hierarchy에서 EnemyNameTag 삭제 (프리팹만 유지)

---

## 2. BossHealthBar (Boss 전용 체력바)

### 목적
화면 상단에 Boss의 이름과 체력바를 표시하는 Screen Space UI

### UI 구조
```
Canvas (Screen Space - Overlay)
└── BossHealthBar
    ├── BossNameText (TextMeshPro)
    ├── HealthBarBackground (Image)
    └── HealthBarFill (Image)
    └── HealthText (TextMeshPro)
```

### 생성 방법

#### 2-1. Canvas 확인/생성

1. 기존 Canvas가 있으면 사용, 없으면 새로 생성
   - Render Mode: **Screen Space - Overlay**
   - Canvas Scaler: Scale With Screen Size (1920x1080)

#### 2-2. BossHealthBar GameObject 생성

1. Canvas 하위에 빈 GameObject 생성
2. 이름: `BossHealthBar`
3. RectTransform:
   - Anchor: Top Center
   - Pos: (0, -50, 0)
   - Width: 600
   - Height: 80

#### 2-3. BossNameText 생성

1. BossHealthBar 하위에 `UI` → `Text - TextMeshPro` 생성
2. 이름: `BossNameText`
3. RectTransform:
   - Anchor: Top Center
   - Pos: (0, -10, 0)
   - Width: 600
   - Height: 30
4. TextMeshProUGUI:
   - Text: "Fire Dragon"
   - Font Size: 28
   - Color: Red (R:255, G:50, B:50)
   - Alignment: Center, Middle
   - Font Style: Bold

#### 2-4. HealthBarBackground 생성

1. BossHealthBar 하위에 `UI` → `Image` 생성
2. 이름: `HealthBarBackground`
3. RectTransform:
   - Anchor: Bottom Center
   - Pos: (0, 15, 0)
   - Width: 560
   - Height: 30
4. Image:
   - Color: 검은색 (R:50, G:50, B:50)

#### 2-5. HealthBarFill 생성

1. HealthBarBackground 하위에 `UI` → `Image` 생성
2. 이름: `HealthBarFill`
3. RectTransform:
   - Anchor: Stretch (Left)
   - Left: 0, Right: 0, Top: 0, Bottom: 0
4. Image:
   - Color: 초록색 (R:0, G:255, B:0)
   - **Image Type: Filled**
   - **Fill Method: Horizontal**
   - **Fill Origin: Left**
   - **Fill Amount: 1** (100%)

#### 2-6. HealthText 생성

1. HealthBarBackground 하위에 `UI` → `Text - TextMeshPro` 생성
2. 이름: `HealthText`
3. RectTransform:
   - Anchor: Stretch
   - Left: 0, Right: 0, Top: 0, Bottom: 0
4. TextMeshProUGUI:
   - Text: "150 / 150"
   - Font Size: 20
   - Color: White
   - Alignment: Center, Middle
   - Font Style: Bold
   - **Enable**: Shadow (그림자 효과)

#### 2-7. BossHealthBar 스크립트 추가

1. BossHealthBar GameObject 선택
2. `Add Component` → `BossHealthBar` 스크립트 추가
3. Inspector 설정:
   - **Boss Name Text**: BossNameText 드래그
   - **Health Bar Fill**: HealthBarFill 드래그
   - **Health Text**: HealthText 드래그
   - **Healthy Color**: 초록색 (R:0, G:255, B:0)
   - **Danger Color**: 빨간색 (R:255, G:0, B:0)
   - **Fill Speed**: 5

#### 2-8. 프리팹으로 저장

1. BossHealthBar를 `Assets/_Project/Prefabs/UI/` 폴더로 드래그
2. 프리팹 이름: `BossHealthBar`
3. **Hierarchy에서 BossHealthBar 삭제하지 않음** (Scene에 유지)

---

## 사용 방법

### EnemyNameTag 사용

```csharp
// Named 적 생성 시
public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject enemyNameTagPrefab;

    private void SpawnNamedEnemy(Enemy enemy)
    {
        if (enemy.Data.showNameTag)
        {
            GameObject nameTagObj = Instantiate(enemyNameTagPrefab);
            EnemyNameTag nameTag = nameTagObj.GetComponent<EnemyNameTag>();
            nameTag.Initialize(enemy);
        }
    }
}
```

### BossHealthBar 사용

```csharp
// Boss 적 생성 시
public class EnemySpawner : MonoBehaviour
{
    private BossHealthBar bossHealthBar;

    private void SpawnBossEnemy(Enemy enemy)
    {
        if (enemy.Data.showBossHealthBar)
        {
            // Scene에 있는 BossHealthBar 찾기
            bossHealthBar = FindAnyObjectByType<BossHealthBar>();
            bossHealthBar.Initialize(enemy);
        }
    }
}
```

---

## 검증 체크리스트

### EnemyNameTag
- ✅ Canvas Render Mode = World Space
- ✅ Canvas Scale = (0.01, 0.01, 0.01)
- ✅ NameText 참조 연결
- ✅ Face Camera 체크
- ✅ Vertical Offset = 1.5

### BossHealthBar
- ✅ Canvas Render Mode = Screen Space - Overlay
- ✅ BossNameText, HealthBarFill, HealthText 참조 연결
- ✅ HealthBarFill Image Type = Filled
- ✅ Fill Method = Horizontal
- ✅ Healthy Color = 초록색, Danger Color = 빨간색

---

## 테스트

### EnemyNameTag 테스트
1. Scene에 Enemy GameObject 생성
2. Enemy 스크립트 추가 및 EliteOrc EnemyData 할당
3. Play 모드 진입
4. EnemyNameTag 프리팹을 Hierarchy에 드래그
5. Inspector에서 Initialize(enemy) 호출 (테스트용)
6. 결과: 적 위에 노란색 이름표 표시

### BossHealthBar 테스트
1. Scene에 Enemy GameObject 생성
2. Enemy 스크립트 추가 및 FireDragon EnemyData 할당
3. BossHealthBar는 이미 Scene에 있음
4. Play 모드 진입
5. Inspector에서 Initialize(enemy) 호출
6. Console에서 "Take 10 Damage" Context Menu 실행
7. 결과: 화면 상단에 체력바 표시, 데미지 받으면 체력바 감소

---

**다음 단계**: Phase 5 구현 완료, Phase 6 (Combat Integration) 진행
