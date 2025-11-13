# ScriptableObject 폴더 구조 (최종)

**생성일**: 2025-11-12
**Phase**: A-4 Item-Skill System

---

## 📂 완전한 폴더 트리

```
Assets/_Project/Data/
│
├── 📁 Enemies/                    # 적 데이터 (EnemyData.asset)
│   ├── TestEnemies/               # 테스트용 적
│   └── (향후: Bosses/)            # 보스 적
│
├── 📁 Items/                      # 아이템 데이터
│   ├── Equipment/                 # 장비 아이템 (Item.asset)
│   │   ├── FireSword.asset
│   │   ├── LeatherArmor.asset
│   │   └── IronRing.asset
│   │
│   ├── SkillItems/                # ⭐ 스킬 아이템 (SkillItem.asset)
│   │   ├── SkillItem_IceBlast.asset
│   │   ├── SkillItem_LightningBolt.asset
│   │   ├── SkillItem_Shield.asset
│   │   ├── SkillItem_Fireball.asset
│   │   └── SkillItem_Teleport.asset
│   │
│   └── Consumables/               # 소모품 (향후 추가)
│
├── 📁 Skills/                     # 스킬 데이터 (SkillData.asset - 기존 시스템)
│   └── TestSkills/                # 테스트용 스킬
│       ├── TEST_FireballSkill.asset
│       ├── TEST_HealSkill.asset
│       └── TEST_AttackBuffSkill.asset
│
├── 📁 StatusEffects/              # 상태 효과 (StatusEffectData.asset)
│   ├── Buffs/                     # 버프 효과
│   │   └── TEST_AttackUp.asset
│   │
│   ├── Debuffs/                   # 디버프 효과
│   │
│   └── DoT/                       # DoT 효과
│
├── 📁 Loot/                       # ⭐ 드롭 테이블 (LootTable.asset)
│   ├── Enemy/                     # 일반 적 드롭
│   │   ├── Goblin_LootTable.asset
│   │   └── TestEnemy_LootTable.asset
│   │
│   ├── Boss/                      # 보스 드롭
│   │
│   └── Chest/                     # 상자 드롭 (향후)
│
├── 📁 Forms/                      # ⭐ Form 데이터 (FormData.asset)
│   ├── Mage/                      # 마법사 Form
│   │   └── MageFormData.asset
│   │
│   ├── Warrior/                   # 전사 Form (향후)
│   │
│   └── Assassin/                  # 암살자 Form (향후)
│
└── 📁 Rooms/                      # 방 데이터 (RoomData.asset)
    └── TestRoom_Normal.asset
```

---

## 🎯 각 폴더의 CreateAssetMenu

| 폴더 | 메뉴 경로 | 파일 타입 |
|------|----------|----------|
| Enemies/ | `Create > GASPT > Enemies > Enemy` | EnemyData.asset |
| Items/Equipment/ | `Create > GASPT > Items > Item` | Item.asset |
| Items/SkillItems/ | `Create > GASPT > Items > Skill Item` | SkillItem.asset ⭐ |
| Skills/ | `Create > GASPT > Skills > Skill` | SkillData.asset |
| StatusEffects/ | `Create > GASPT > StatusEffects > StatusEffect` | StatusEffectData.asset |
| Loot/ | `Create > GASPT > Loot > LootTable` | LootTable.asset ⭐ |
| Forms/ | `Create > GASPT > Form > Form Data` | FormData.asset ⭐ |
| Rooms/ | `Create > GASPT > Room > Room Data` | RoomData.asset |

---

## 📋 Phase A-4 작업 체크리스트

### ✅ 폴더 구조 생성 완료
- [x] Items/SkillItems/
- [x] Loot/Enemy/
- [x] Loot/Boss/
- [x] Loot/Chest/
- [x] Forms/Mage/
- [x] Forms/Warrior/
- [x] Forms/Assassin/
- [x] StatusEffects/Buffs/
- [x] StatusEffects/Debuffs/
- [x] StatusEffects/DoT/

### 📝 다음 단계: ScriptableObject 생성

#### 1. SkillItem 생성 (5개)
**위치**: `Assets/_Project/Data/Items/SkillItems/`

```
우클릭 > Create > GASPT > Items > Skill Item
```

- [ ] SkillItem_IceBlast.asset
  - itemName: "Ice Blast Scroll"
  - abilityType: IceBlast
  - targetSlotIndex: 1
  - rarity: Rare
  - cooldown: 3s

- [ ] SkillItem_LightningBolt.asset
  - itemName: "Lightning Bolt Scroll"
  - abilityType: LightningBolt
  - targetSlotIndex: 2
  - rarity: Epic
  - cooldown: 4s

- [ ] SkillItem_Shield.asset
  - itemName: "Shield Amulet"
  - abilityType: Shield
  - targetSlotIndex: 3
  - rarity: Rare
  - cooldown: 8s

- [ ] SkillItem_Fireball.asset
  - itemName: "Fireball Scroll"
  - abilityType: Fireball
  - targetSlotIndex: 2
  - rarity: Common
  - cooldown: 5s

- [ ] SkillItem_Teleport.asset
  - itemName: "Teleport Scroll"
  - abilityType: Teleport
  - targetSlotIndex: 1
  - rarity: Rare
  - cooldown: 3s

#### 2. LootTable 생성 (2개)
**위치**: `Assets/_Project/Data/Loot/Enemy/`

```
우클릭 > Create > GASPT > Loot > LootTable
```

- [ ] Goblin_SkillLootTable.asset
  - Entry 0: SkillItem_IceBlast, 15% (0.15)
  - Entry 1: SkillItem_Shield, 10% (0.10)
  - Entry 2: 드롭 없음, 75% (0.75)

- [ ] TestEnemy_LootTable.asset
  - Entry 0: SkillItem_LightningBolt, 20% (0.20)
  - Entry 1: SkillItem_Fireball, 30% (0.30)
  - Entry 2: 드롭 없음, 50% (0.50)

#### 3. EnemyData 수정 (2개)
**위치**: `Assets/_Project/Data/Enemies/`

- [ ] TestGoblin.asset
  - lootTable 필드에 `Goblin_SkillLootTable` 연결

- [ ] Normal Goblin.asset (또는 NormalGoblin.asset)
  - lootTable 필드에 `Goblin_SkillLootTable` 연결

#### 4. FormData 생성 (1개)
**위치**: `Assets/_Project/Data/Forms/Mage/`

```
우클릭 > Create > GASPT > Form > Form Data
```

- [ ] MageFormData.asset
  - formName: "Mage"
  - formType: Mage
  - maxHealth: 80
  - moveSpeed: 7
  - jumpPower: 12

#### 5. 테스트 설정
**위치**: IntegrationTestScene.unity

- [ ] SkillItemTest 컴포넌트 추가
- [ ] MageForm GameObject 참조 연결
- [ ] 생성한 SkillItem 5개 연결
- [ ] LootTable 연결
- [ ] Context Menu 테스트 실행

---

## 🔍 폴더 찾기 팁

### Unity 에디터 Project 창에서:
1. `Assets/_Project/Data/` 폴더 열기
2. 원하는 하위 폴더로 이동
3. 우클릭 > Create > GASPT > ... 선택

### 파일 탐색기에서:
```
D:\JaeChang\UintyDev\GASPT\GASPT\Assets\_Project\Data\
```

---

## ⚠️ 주의사항

1. **에셋 이동**: Unity 에디터에서만 Drag & Drop으로 이동 (파일 탐색기 사용 금지!)
2. **네이밍**: 테스트 에셋은 `TEST_` 접두사, 스킬 아이템은 `SkillItem_` 접두사 사용
3. **README 확인**: 각 폴더의 README.md 파일 참조
4. **참조 끊김**: 에셋 이동 시 참조가 깨질 수 있으니 주의!

---

**작성일**: 2025-11-12
**작성자**: Claude Code
**관련 Phase**: A-4 Item-Skill System
