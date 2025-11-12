# ScriptableObject 데이터 폴더 구조

**최종 업데이트**: 2025-11-12
**담당**: Phase A-4 Item-Skill System

---

## 📂 폴더 구조

```
Assets/_Project/Data/
├── Enemies/              # 적 데이터 (EnemyData)
│   ├── TestEnemies/      # 테스트용 적
│   └── Bosses/           # 보스 적 (향후 추가)
│
├── Items/                # 아이템 데이터
│   ├── Equipment/        # 장비 아이템 (Item.cs)
│   ├── SkillItems/       # 스킬 아이템 (SkillItem.cs) ⭐ 새로 추가
│   └── Consumables/      # 소모품 (향후 추가)
│
├── Skills/               # 스킬 데이터 (SkillData - 기존 스킬 시스템)
│   └── TestSkills/       # 테스트용 스킬
│
├── StatusEffects/        # 상태 효과 데이터 (StatusEffectData)
│   ├── Buffs/            # 버프 효과
│   ├── Debuffs/          # 디버프 효과
│   └── DoT/              # DoT 효과 (Poison, Burn, Bleed 등)
│
├── Loot/                 # 드롭 테이블 (LootTable) ⭐ 새로 추가
│   ├── Enemy/            # 적 드롭 테이블
│   ├── Boss/             # 보스 드롭 테이블
│   └── Chest/            # 상자 드롭 테이블 (향후 추가)
│
├── Forms/                # Form 데이터 (FormData) ⭐ 새로 추가
│   ├── Mage/             # 마법사 Form
│   ├── Warrior/          # 전사 Form (향후)
│   └── Assassin/         # 암살자 Form (향후)
│
└── Rooms/                # 방 데이터 (RoomData)
```

---

## 📋 폴더별 상세 설명

### 1️⃣ **Enemies/** - 적 데이터
- **파일 타입**: `EnemyData.asset`
- **스크립트**: `Assets/_Project/Scripts/Data/EnemyData.cs`
- **설명**: 모든 적의 스탯, 드롭 테이블, AI 설정 등을 정의
- **CreateAssetMenu**: `Create > GASPT > Enemies > Enemy`

**예시 파일**:
- `NormalGoblin.asset` - 일반 고블린
- `EliteOrc.asset` - 엘리트 오크
- `FireDragon.asset` - 보스 드래곤

**TestEnemies/** 하위 폴더:
- 개발/테스트 전용 적 데이터
- `TEST_` 접두사 사용

---

### 2️⃣ **Items/** - 아이템 데이터

#### 📦 **Items/Equipment/** - 장비 아이템
- **파일 타입**: `Item.asset`
- **스크립트**: `Assets/_Project/Scripts/Data/Item.cs`
- **설명**: 스탯 보너스를 제공하는 장비 아이템
- **CreateAssetMenu**: `Create > GASPT > Items > Item`

**예시 파일**:
- `FireSword.asset` - 화염 검 (공격력 +15)
- `LeatherArmor.asset` - 가죽 갑옷 (방어력 +10)
- `IronRing.asset` - 철 반지 (HP +20)

#### ⚡ **Items/SkillItems/** - 스킬 아이템 ⭐ NEW
- **파일 타입**: `SkillItem.asset`
- **스크립트**: `Assets/_Project/Scripts/Data/SkillItem.cs`
- **설명**: 획득 시 Form의 스킬 슬롯에 자동 장착되는 아이템
- **CreateAssetMenu**: `Create > GASPT > Items > Skill Item`

**예시 파일**:
- `SkillItem_IceBlast.asset` - 빙결 스킬 아이템 (Slot 1, Rare)
- `SkillItem_LightningBolt.asset` - 번개 스킬 아이템 (Slot 2, Epic)
- `SkillItem_Shield.asset` - 보호막 스킬 아이템 (Slot 3, Rare)
- `SkillItem_Fireball.asset` - 화염구 스킬 아이템 (Slot 2, Common)
- `SkillItem_Teleport.asset` - 순간이동 스킬 아이템 (Slot 1, Rare)

**필수 설정**:
- `abilityType`: 어떤 스킬을 부여할지 (IceBlast, Fireball, Teleport 등)
- `targetSlotIndex`: 장착될 슬롯 (0: 기본공격, 1-3: 스킬)
- `rarity`: 희귀도 (Common, Rare, Epic, Legendary)

#### 🍶 **Items/Consumables/** - 소모품 (향후 추가)
- 포션, 스크롤 등 1회용 아이템

---

### 3️⃣ **Skills/** - 스킬 데이터 (기존 스킬 시스템)
- **파일 타입**: `SkillData.asset`
- **스크립트**: `Assets/_Project/Scripts/Skills/SkillData.cs`
- **설명**: 기존 스킬 시스템용 (SkillSystem, 마나 소비 등)
- **CreateAssetMenu**: `Create > GASPT > Skills > Skill`

**참고**: SkillItem과 혼동 주의!
- `SkillData`: 기존 스킬 시스템 (SkillSystem, UI 슬롯 등)
- `SkillItem`: Form 전용 스킬 아이템 (Phase A-4 신규)

---

### 4️⃣ **StatusEffects/** - 상태 효과 데이터
- **파일 타입**: `StatusEffectData.asset`
- **스크립트**: `Assets/_Project/Scripts/Data/StatusEffectData.cs`
- **설명**: 버프, 디버프, DoT 효과 정의
- **CreateAssetMenu**: `Create > GASPT > StatusEffects > StatusEffect`

**하위 폴더 구분**:
- **Buffs/**: AttackUp, DefenseUp, SpeedUp, Invincible 등
- **Debuffs/**: AttackDown, DefenseDown, Slow, Stun, Root 등
- **DoT/**: Poison, Burn, Bleed, Regeneration 등

**예시 파일**:
- `Buffs/AttackUp.asset` - 공격력 증가 버프
- `Debuffs/Slow.asset` - 이동속도 감소 디버프
- `DoT/Poison.asset` - 독 데미지 (틱당 5 데미지)

---

### 5️⃣ **Loot/** - 드롭 테이블 ⭐ NEW
- **파일 타입**: `LootTable.asset`
- **스크립트**: `Assets/_Project/Scripts/Loot/LootTable.cs`
- **설명**: 적 처치/상자 오픈 시 드롭될 아이템 확률 테이블
- **CreateAssetMenu**: `Create > GASPT > Loot > LootTable`

**하위 폴더 구분**:
- **Enemy/**: 일반 적 드롭 테이블
- **Boss/**: 보스 드롭 테이블
- **Chest/**: 상자 드롭 테이블 (향후)

**예시 파일**:
- `Enemy/Goblin_LootTable.asset` - 고블린 드롭 (일반 아이템 70%, 스킬 아이템 30%)
- `Boss/Dragon_LootTable.asset` - 드래곤 드롭 (Epic 스킬 50%, 골드 많음)

**설정 방법**:
1. `LootTable.asset` 생성
2. `lootEntries` 배열에 아이템 추가
3. `dropChance` (0~1) 설정 (예: 0.2 = 20%)
4. `minQuantity`, `maxQuantity` 설정
5. `EnemyData.lootTable`에 연결

---

### 6️⃣ **Forms/** - Form 데이터 ⭐ NEW
- **파일 타입**: `FormData.asset`
- **스크립트**: `Assets/_Project/Scripts/Gameplay/Form/Core/FormData.cs`
- **설명**: 플레이어 Form의 스탯, 비주얼, 기본 스킬 정의
- **CreateAssetMenu**: `Create > GASPT > Form > Form Data`

**하위 폴더 구분**:
- **Mage/**: 마법사 Form 관련 데이터
- **Warrior/**: 전사 Form 관련 데이터 (향후)
- **Assassin/**: 암살자 Form 관련 데이터 (향후)

**예시 파일**:
- `Mage/MageFormData.asset` - 마법사 기본 스탯 (HP 80, Speed 7, Jump 12)
- `Warrior/WarriorFormData.asset` - 전사 기본 스탯 (HP 150, Speed 5, Jump 8)

---

### 7️⃣ **Rooms/** - 방 데이터
- **파일 타입**: `RoomData.asset`
- **스크립트**: `Assets/_Project/Scripts/Gameplay/Level/Room/RoomData.cs`
- **설명**: 던전 방의 적 스폰, 레이아웃 정의
- **CreateAssetMenu**: `Create > GASPT > Room > Room Data`

**예시 파일**:
- `TestRoom_Normal.asset` - 일반 난이도 테스트 방

---

## 🎯 추천 작업 순서

### Phase A-4 작업 시 (현재):

1. **SkillItem 생성** (5개)
   ```
   위치: Assets/_Project/Data/Items/SkillItems/
   ```
   - `SkillItem_IceBlast.asset`
   - `SkillItem_LightningBolt.asset`
   - `SkillItem_Shield.asset`
   - `SkillItem_Fireball.asset`
   - `SkillItem_Teleport.asset`

2. **LootTable 생성** (2개)
   ```
   위치: Assets/_Project/Data/Loot/Enemy/
   ```
   - `Goblin_SkillLootTable.asset` - 고블린 스킬 드롭
   - `TestEnemy_LootTable.asset` - 테스트용

3. **EnemyData 수정**
   ```
   위치: Assets/_Project/Data/Enemies/
   ```
   - 기존 `TestGoblin.asset`, `NormalGoblin.asset`의 `lootTable` 필드에 연결

4. **FormData 생성** (1개)
   ```
   위치: Assets/_Project/Data/Forms/Mage/
   ```
   - `MageFormData.asset` - 마법사 기본 스탯 정의

---

## ⚠️ 주의사항

### 네이밍 규칙:
- **테스트 에셋**: `TEST_` 접두사 사용 (예: `TEST_Enemy.asset`)
- **실제 에셋**: 의미 있는 이름 사용 (예: `FireDragon.asset`)
- **스킬 아이템**: `SkillItem_` 접두사 사용 (예: `SkillItem_IceBlast.asset`)

### 폴더 이동 주의:
- Unity 에디터에서만 에셋 이동 (Drag & Drop)
- 파일 탐색기에서 이동 시 `.meta` 파일도 함께 이동
- 참조가 깨질 수 있으므로 주의!

### Resources 폴더:
- `Assets/Resources/Data/`는 **Runtime 로딩 전용**
- GameResourceManager가 사용하는 에셋만 배치
- 일반 에셋은 `Assets/_Project/Data/`에 저장

---

## 📚 관련 문서

- **리소스 가이드**: `RESOURCES_GUIDE.md`
- **작업 현황**: `WORK_STATUS.md`
- **Phase A-4 구현 계획**: 위 섹션 참조

---

**작성일**: 2025-11-12
**작성자**: Claude Code (Phase A-4 Item-Skill System)
