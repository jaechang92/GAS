# 테스트 아이템 생성 가이드

Unity Editor에서 테스트 아이템 3개를 생성하는 방법입니다.

---

## 📋 생성할 아이템 목록

1. **FireSword** (Fire Sword) - 무기
2. **LeatherArmor** (Leather Armor) - 방어구
3. **IronRing** (Iron Ring) - 악세서리

---

## 🔧 아이템 생성 방법

### 1. FireSword (Fire Sword)

1. Unity Editor에서 `Assets/_Project/Data/Items/` 폴더로 이동
2. 우클릭 → `Create` → `GASPT` → `Items` → `Item`
3. 생성된 파일 이름을 `FireSword`로 변경
4. Inspector에서 다음 값 설정:

```
Item Name: Fire Sword
Description: 불꽃이 깃든 검
Slot: Weapon
HP Bonus: 0
Attack Bonus: 5
Defense Bonus: 0
```

---

### 2. LeatherArmor (Leather Armor)

1. `Assets/_Project/Data/Items/` 폴더에서
2. 우클릭 → `Create` → `GASPT` → `Items` → `Item`
3. 파일 이름: `LeatherArmor`
4. Inspector 설정:

```
Item Name: Leather Armor
Description: 가죽으로 만든 가벼운 갑옷
Slot: Armor
HP Bonus: 20
Attack Bonus: 0
Defense Bonus: 3
```

---

### 3. IronRing (Iron Ring)

1. `Assets/_Project/Data/Items/` 폴더에서
2. 우클릭 → `Create` → `GASPT` → `Items` → `Item`
3. 파일 이름: `IronRing`
4. Inspector 설정:

```
Item Name: Iron Ring
Description: 단단한 철로 만든 반지
Slot: Accessory
HP Bonus: 10
Attack Bonus: 0
Defense Bonus: 0
```

---

## ✅ 검증

아이템 생성 후 각 아이템을 선택하여 Inspector에서 다음을 확인:

1. ✅ Item Name이 올바르게 설정됨
2. ✅ Slot이 올바르게 설정됨 (Weapon/Armor/Accessory)
3. ✅ Bonus 값이 올바르게 설정됨

---

## 📊 아이템 스탯 요약

| 아이템 | 슬롯 | HP | Attack | Defense |
|--------|------|-----|---------|---------|
| Fire Sword | Weapon | 0 | +5 | 0 |
| Leather Armor | Armor | +20 | 0 | +3 |
| Iron Ring | Accessory | +10 | 0 | 0 |

**모든 아이템 장착 시:**
- HP: +30
- Attack: +5
- Defense: +3

---

## 🎮 테스트 방법

1. Scene에 빈 GameObject 생성 (`Player`)
2. `PlayerStats` 컴포넌트 추가
3. Play Mode에서 Console 확인:
   - 기본 스탯: HP 100, Attack 10, Defense 5

4. Inspector에서 `PlayerStats` 컴포넌트 찾기
5. Debug 메뉴나 스크립트로 아이템 장착 테스트:

```csharp
// 테스트 코드 예시
playerStats.EquipItem(fireSword);
playerStats.DebugPrintStats();
// 예상 결과: HP 100, Attack 15, Defense 5
```

---

**생성 완료 후 다음 단계로 진행합니다.**
