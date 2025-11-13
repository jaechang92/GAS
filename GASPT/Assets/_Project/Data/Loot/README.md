# Loot 폴더

**용도**: 드롭 테이블 ScriptableObject 저장
**파일 타입**: `LootTable.asset`
**스크립트**: `Assets/_Project/Scripts/Loot/LootTable.cs`

---

## 생성 방법

Unity 에디터에서:
```
우클릭 > Create > GASPT > Loot > LootTable
```

---

## 설정 방법

1. **lootEntries 배열**:
   - `item`: 드롭할 아이템 (Item 또는 SkillItem)
   - `dropChance`: 드롭 확률 (0.0 ~ 1.0, 예: 0.3 = 30%)
   - `minQuantity`: 최소 수량 (기본값: 1)
   - `maxQuantity`: 최대 수량 (기본값: 1)

2. **예시 설정**:
   ```
   Entry 0: FireSword, 20% (0.2)
   Entry 1: SkillItem_IceBlast, 15% (0.15)
   Entry 2: SkillItem_Shield, 10% (0.1)
   Entry 3: 아무것도 드롭 안함, 55% (0.55)
   ```

---

## 하위 폴더 구분

- **Enemy/**: 일반 적 드롭 테이블
- **Boss/**: 보스 드롭 테이블
- **Chest/**: 상자 드롭 테이블 (향후 추가)

---

## EnemyData 연결

1. `EnemyData.asset` 열기
2. `lootTable` 필드에 생성한 LootTable 드래그
3. 적 처치 시 `Enemy.Die()` → `DropLoot()` 자동 호출

---

## 테스트 방법

1. LootTable 선택
2. 우클릭 > `Test: Simulate 100 Drops`
3. 콘솔에서 드롭 통계 확인
