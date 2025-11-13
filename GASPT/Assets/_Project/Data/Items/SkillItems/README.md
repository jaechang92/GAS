# SkillItems 폴더

**용도**: 스킬 아이템 ScriptableObject 저장
**파일 타입**: `SkillItem.asset`
**스크립트**: `Assets/_Project/Scripts/Data/SkillItem.cs`

---

## 생성 방법

Unity 에디터에서:
```
우클릭 > Create > GASPT > Items > Skill Item
```

---

## 필수 설정

1. **itemName**: 아이템 이름 (예: "Ice Blast Scroll")
2. **description**: 스킬 설명 (예: "빙결 범위 공격 스킬")
3. **icon**: 아이템 아이콘 Sprite
4. **abilityType**: 스킬 타입 (IceBlast, Fireball, LightningBolt, Shield 등)
5. **targetSlotIndex**: 장착될 슬롯 (0: 기본공격, 1-3: 스킬)
6. **rarity**: 희귀도 (Common, Rare, Epic, Legendary)

---

## 예시 파일 목록

- `SkillItem_IceBlast.asset` - 빙결 (Slot 1, Rare, 3초 쿨다운)
- `SkillItem_LightningBolt.asset` - 번개 (Slot 2, Epic, 4초 쿨다운)
- `SkillItem_Shield.asset` - 보호막 (Slot 3, Rare, 8초 쿨다운)
- `SkillItem_Fireball.asset` - 화염구 (Slot 2, Common, 5초 쿨다운)
- `SkillItem_Teleport.asset` - 순간이동 (Slot 1, Rare, 3초 쿨다운)

---

## 시스템 통합

1. **LootTable에 추가**: 적 드롭 테이블에 이 스킬 아이템 추가
2. **자동 장착**: 플레이어가 획득하면 SkillItemManager가 자동으로 Form에 장착
3. **UI 갱신**: OnSkillEquipped 이벤트로 SkillUIPanel 자동 업데이트
