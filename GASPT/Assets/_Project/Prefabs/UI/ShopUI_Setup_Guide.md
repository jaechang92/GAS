# ShopUI 설정 가이드

Unity Editor에서 ShopUI를 설정하는 방법입니다.

---

## 🎯 목표

아이템 목록 표시 + 가격 + 구매 버튼 + 골드 표시가 있는 상점 UI 생성

---

## 📐 UI 구조

```
Canvas
└── ShopPanel
    ├── GoldText (TextMeshPro)
    ├── MessageText (TextMeshPro)
    └── ScrollView
        └── Viewport
            └── Content (ItemListParent)
```

**별도 프리팹:**
```
ItemSlotPrefab
├── Background (Image)
├── ItemNameText (TextMeshPro)
├── PriceText (TextMeshPro)
└── PurchaseButton (Button)
    └── ButtonText (TextMeshPro)
```

---

## 🔧 1단계: ShopPanel 생성

### 1. ShopPanel GameObject

1. Canvas 하위에 빈 GameObject 생성 (`ShopPanel`)
2. RectTransform 설정:
   - Anchor: Center
   - Pos: (0, 0, 0)
   - Width: 600
   - Height: 800

---

## 🔧 2단계: GoldText 생성

1. ShopPanel 하위에 `UI` → `Text - TextMeshPro` 생성 (`GoldText`)
2. RectTransform:
   - Anchor: Top Center
   - Pos X: 0
   - Pos Y: -30
   - Width: 560
   - Height: 40
3. TextMeshProUGUI:
   - Text: `Gold: 100`
   - Font Size: 28
   - Color: Yellow (R:255, G:200, B:0)
   - Alignment: Center, Middle

---

## 🔧 3단계: MessageText 생성

1. ShopPanel 하위에 `UI` → `Text - TextMeshPro` 생성 (`MessageText`)
2. RectTransform:
   - Anchor: Bottom Center
   - Pos X: 0
   - Pos Y: 30
   - Width: 560
   - Height: 40
3. TextMeshProUGUI:
   - Text: (비워둠)
   - Font Size: 24
   - Color: White
   - Alignment: Center, Middle
4. **GameObject 비활성화** (기본적으로 숨김)

---

## 🔧 4단계: ScrollView 생성

1. ShopPanel 하위에 `UI` → `Scroll View` 생성
2. RectTransform:
   - Anchor: Stretch (가로/세로 모두)
   - Left: 20
   - Right: 20
   - Top: 90
   - Bottom: 90
3. Scroll Rect 컴포넌트:
   - Vertical: true
   - Horizontal: false
4. **Content 찾기:**
   - Hierarchy: ScrollView > Viewport > Content
   - Content가 ItemListParent 역할을 함
5. Content RectTransform:
   - Anchor: Top Center
   - Width: 560
   - Height: 자동 (Content Size Fitter 사용 권장)
6. **Content Size Fitter 추가 (Content에):**
   - Vertical Fit: Preferred Size

---

## 🔧 5단계: ItemSlotPrefab 생성

### ItemSlot GameObject 생성

1. Hierarchy에서 빈 GameObject 생성 (`ItemSlot`)
2. RectTransform:
   - Width: 540
   - Height: 80

### Background Image

1. ItemSlot 하위에 `UI` → `Image` 생성 (`Background`)
2. Rect Transform: Stretch (전체)
3. Color: 회색 (R:100, G:100, B:100, A:200)

### ItemNameText

1. ItemSlot 하위에 `UI` → `Text - TextMeshPro` 생성 (`ItemNameText`)
2. RectTransform:
   - Anchor: Top Left
   - Pos: (20, -20)
   - Width: 300
   - Height: 30
3. TextMeshProUGUI:
   - Text: `Fire Sword`
   - Font Size: 22
   - Alignment: Left, Middle

### PriceText

1. ItemSlot 하위에 `UI` → `Text - TextMeshPro` 생성 (`PriceText`)
2. RectTransform:
   - Anchor: Bottom Left
   - Pos: (20, 10)
   - Width: 150
   - Height: 25
3. TextMeshProUGUI:
   - Text: `80 Gold`
   - Font Size: 18
   - Color: Yellow
   - Alignment: Left, Middle

### PurchaseButton

1. ItemSlot 하위에 `UI` → `Button` 생성 (`PurchaseButton`)
2. RectTransform:
   - Anchor: Middle Right
   - Pos: (-80, 0)
   - Width: 120
   - Height: 50
3. Button 하위의 Text를 TextMeshPro로 교체:
   - Text: `Purchase`
   - Font Size: 20
   - Alignment: Center, Middle

### ShopItemSlot 스크립트 추가

1. ItemSlot GameObject에 `ShopItemSlot` 스크립트 추가
2. Inspector에서 참조 연결:
   - Item Name Text: ItemNameText
   - Price Text: PriceText
   - Purchase Button: PurchaseButton

### 프리팹으로 저장

1. ItemSlot을 `Assets/_Project/Prefabs/UI/` 폴더로 드래그
2. 프리팹 이름: `ItemSlotPrefab`
3. **Hierarchy에서 ItemSlot 삭제** (프리팹만 유지)

---

## 🔧 6단계: ShopUI 스크립트 연결

1. ShopPanel GameObject에 `ShopUI` 스크립트 추가
2. Inspector에서 참조 연결:
   - **Gold Text**: GoldText
   - **Item List Parent**: ScrollView > Viewport > Content
   - **Item Slot Prefab**: `Assets/_Project/Prefabs/UI/ItemSlotPrefab`
   - **Message Text**: MessageText
   - **Shop System**: Scene의 ShopSystem (나중에 연결)

---

## 🔧 7단계: ShopPanel 프리팹으로 저장

1. ShopPanel을 `Assets/_Project/Prefabs/UI/` 폴더로 드래그
2. 프리팹 이름: `ShopPanel`

---

## 🔧 8단계: ShopSystem 설정

### ShopSystem GameObject 생성

1. Hierarchy에 빈 GameObject 생성 (`ShopSystem`)
2. `ShopSystem` 스크립트 추가
3. Inspector에서 Shop Items 설정:
   - Size: 3
   - Element 0:
     - Item: FireSword
     - Price: 80
   - Element 1:
     - Item: LeatherArmor
     - Price: 120
   - Element 2:
     - Item: IronRing
     - Price: 50

### ShopPanel과 연결

1. ShopPanel 선택
2. ShopUI 컴포넌트의 Shop System에 ShopSystem GameObject 드래그

---

## ✅ 검증

1. ✅ ShopPanel 프리팹 존재
2. ✅ ItemSlotPrefab 존재
3. ✅ ShopUI 스크립트의 모든 참조 연결
4. ✅ ShopSystem에 아이템 3개 설정

---

## 🎮 테스트

### Play Mode 테스트

1. Scene에 다음이 있어야 함:
   - Player (PlayerStats 컴포넌트)
   - CurrencySystem GameObject (자동 생성됨)
   - InventorySystem GameObject (자동 생성됨)
   - ShopSystem GameObject
   - ShopPanel (활성화)

2. Play 버튼 클릭

3. 확인 사항:
   - ✅ 골드 표시: "Gold: 100"
   - ✅ 아이템 3개 표시
   - ✅ 각 아이템에 가격과 Purchase 버튼

4. Purchase 버튼 클릭:
   - ✅ 골드 충분 시: 아이템 구매, 골드 감소, "구매 완료" 메시지 (녹색)
   - ✅ 골드 부족 시: "골드가 부족합니다" 메시지 (빨간색)

### Console 로그 확인

```
[CurrencySystem] 초기화 완료 - 시작 골드: 100
[InventorySystem] 초기화 완료
[ShopSystem] 초기화 완료 - 상점 아이템 3개
[ShopUI] 상점 아이템 3개 표시 완료
[ShopSystem] 구매 성공: Fire Sword (80 골드)
[InventorySystem] 아이템 추가: Fire Sword (총 1개)
```

---

## 💡 팁

### Vertical Layout Group (Content)

Content에 Vertical Layout Group을 추가하면 아이템이 자동으로 정렬됩니다:

1. Content 선택
2. Add Component → Vertical Layout Group
3. 설정:
   - Spacing: 10
   - Child Force Expand: Width (체크), Height (해제)

### 아이템이 표시되지 않는 경우

1. Content Size Fitter 확인
2. ItemSlotPrefab이 올바르게 할당되었는지 확인
3. ShopSystem의 Shop Items에 아이템이 설정되었는지 확인
4. Console에서 에러 메시지 확인

---

**설정 완료 후 Phase 4 구현이 완료됩니다!**
