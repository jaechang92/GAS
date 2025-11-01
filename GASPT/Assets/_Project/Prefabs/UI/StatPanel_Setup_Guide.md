# StatPanel UI 프리팹 생성 가이드

Unity Editor에서 StatPanel UI 프리팹을 생성하는 방법입니다.

---

## 🎯 목표

플레이어의 HP, Attack, Defense를 실시간으로 표시하는 UI 패널 생성

---

## 📐 UI 구조

```
Canvas (Screen Space - Overlay)
└── StatPanel
    ├── Background (Image)
    ├── HP_Text (TextMeshPro - Text)
    ├── Attack_Text (TextMeshPro - Text)
    └── Defense_Text (TextMeshPro - Text)
    └── StatPanelUI (Script)
```

---

## 🔧 생성 방법

### 1. Canvas 생성

1. Hierarchy 우클릭 → `UI` → `Canvas`
2. Canvas 설정:
   - Render Mode: `Screen Space - Overlay`
   - UI Scale Mode: `Scale With Screen Size`
   - Reference Resolution: `1920 x 1080`

---

### 2. StatPanel GameObject 생성

1. Canvas 하위에 빈 GameObject 생성 (`StatPanel`)
2. RectTransform 설정:
   - Anchor Preset: `Top Left`
   - Pos X: 20
   - Pos Y: -20
   - Width: 300
   - Height: 150

---

### 3. Background Image 추가

1. StatPanel 하위에 `UI` → `Image` 생성 (`Background`)
2. RectTransform:
   - Anchor: Stretch (가로/세로 모두)
   - Left: 0, Right: 0, Top: 0, Bottom: 0
3. Image 컴포넌트:
   - Color: 검은색 반투명 (R:0, G:0, B:0, A:150)

---

### 4. HP_Text 생성

1. StatPanel 하위에 `UI` → `Text - TextMeshPro` 생성 (`HP_Text`)
2. RectTransform:
   - Anchor Preset: `Top Left`
   - Pos X: 20
   - Pos Y: -30
   - Width: 260
   - Height: 30
3. TextMeshProUGUI 컴포넌트:
   - Text: `HP: 100`
   - Font Size: 24
   - Color: 흰색
   - Alignment: Left, Middle

---

### 5. Attack_Text 생성

1. `HP_Text` 복제 (Ctrl+D)
2. 이름: `Attack_Text`
3. RectTransform:
   - Pos Y: -70
4. TextMeshProUGUI:
   - Text: `Attack: 10`

---

### 6. Defense_Text 생성

1. `Attack_Text` 복제
2. 이름: `Defense_Text`
3. RectTransform:
   - Pos Y: -110
4. TextMeshProUGUI:
   - Text: `Defense: 5`

---

### 7. StatPanelUI 스크립트 추가

1. `StatPanel` GameObject 선택
2. Add Component → `StatPanelUI` 스크립트 추가
3. Inspector에서 참조 할당:
   - **HP Text**: `HP_Text` 드래그
   - **Attack Text**: `Attack_Text` 드래그
   - **Defense Text**: `Defense_Text` 드래그
   - **Player Stats**: Scene의 Player GameObject의 PlayerStats 드래그

---

### 8. 프리팹 생성

1. Hierarchy에서 `StatPanel` GameObject를
2. `Assets/_Project/Prefabs/UI/` 폴더로 드래그
3. 프리팹 이름: `StatPanel`

---

## ✅ 검증

프리팹 생성 후 확인할 사항:

1. ✅ StatPanel 프리팹이 `Assets/_Project/Prefabs/UI/`에 존재
2. ✅ StatPanelUI 스크립트가 StatPanel에 추가됨
3. ✅ 모든 TextMeshPro 참조가 올바르게 할당됨
4. ✅ PlayerStats 참조 할당 (Play Mode에서 자동 찾기 가능)

---

## 🎮 테스트 방법

### 1. Scene 설정

1. Scene에 Player GameObject 생성
2. PlayerStats 컴포넌트 추가
3. StatPanel 프리팹을 Scene에 배치

### 2. Play Mode 테스트

1. Play 버튼 클릭
2. Console에서 로그 확인:
   ```
   [PlayerStats] 초기화 완료 - HP: 100, Attack: 10, Defense: 5
   [StatPanelUI] PlayerStats 이벤트 구독 완료
   [StatPanelUI] 모든 스탯 UI 업데이트 완료
   ```

3. Game View에서 UI 확인:
   ```
   HP: 100
   Attack: 10
   Defense: 5
   ```

### 3. 아이템 장착 테스트

테스트 스크립트를 작성하여 아이템 장착:

```csharp
// TestItemEquip.cs (임시 테스트용)
public class TestItemEquip : MonoBehaviour
{
    public PlayerStats playerStats;
    public Item testItem;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            playerStats.EquipItem(testItem);
        }
    }
}
```

**예상 결과:**
- E 키를 누르면 아이템 장착
- UI가 즉시 업데이트됨 (예: Attack: 10 → 15)
- Console에 로그 출력

---

## 🎨 선택적 개선 사항

### 시각적 개선

1. **색상 코딩**
   - HP: 빨간색
   - Attack: 주황색
   - Defense: 파란색

2. **아이콘 추가**
   - 각 스탯 앞에 아이콘 이미지 추가

3. **애니메이션**
   - 스탯 변경 시 숫자 펄스 효과
   - DOTween 사용

---

**프리팹 생성 완료 후 Phase 3 구현이 완료됩니다!**
