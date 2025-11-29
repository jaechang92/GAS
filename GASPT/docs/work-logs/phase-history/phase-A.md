# Phase A: Form 시스템 기초

**기간**: 2025-09-21 ~ 2025-09-24 (추정)
**상태**: ✅ 완료

---

## 📌 Phase 개요

Form (Skull) 시스템의 기초를 구축한 Phase입니다. MageForm을 시작으로 Enemy AI, Room System, Skill Item 시스템까지 구현하여 게임의 핵심 메커니즘을 완성했습니다.

---

## Phase A-1: MageForm 시스템 구현

**커밋**: `86dbf45` - 기능: Phase A-1 MageForm 시스템 구현
**날짜**: 2025-09-21 (추정)

### 구현 내용
- BaseForm 추상 클래스 생성
- IFormController 인터페이스 정의
- MageForm 구현 (마법 미사일, 파이어볼)
- FormInputHandler 입력 처리

### 생성된 파일
```
Assets/_Project/Scripts/Gameplay/Form/Core/
├── BaseForm.cs
├── IFormController.cs
├── FormData.cs
└── AbilityType.cs

Assets/_Project/Scripts/Gameplay/Form/
├── FormInputHandler.cs
└── Implementations/
    └── MageForm.cs

Assets/_Project/Scripts/Gameplay/Form/Abilities/
├── BaseAbility.cs
├── BaseProjectileAbility.cs
├── FireballAbility.cs
├── MagicMissileAbility.cs
├── LightningBoltAbility.cs
└── IceBlastAbility.cs
```

### 주요 기능
- ✅ Form 전환 시스템 (Q키)
- ✅ Ability 실행 시스템
- ✅ Projectile 기반 스킬
- ✅ 4가지 마법 스킬 (MagicMissile, Fireball, LightningBolt, IceBlast)

### 아키텍처 패턴
```
BaseForm (추상 클래스)
    ↓ 상속
MageForm (구현체)
    ↓ 사용
BaseAbility (추상 클래스)
    ↓ 상속
MagicMissileAbility, FireballAbility...
```

---

## Phase A-2: Enemy AI + Combat 통합

**커밋**: `02d36c0` - 기능: Phase A-2 Enemy AI + Combat 통합 완료
**날짜**: 2025-09-22 (추정)

### 구현 내용
- Enemy AI FSM 구현
- 적 타입별 구현 (BasicMelee, Ranged, Flying)
- Combat System 통합
- Projectile 시스템

### 생성된 파일
```
Assets/_Project/Scripts/Gameplay/Enemy/
├── BasicMeleeEnemy.cs
├── RangedEnemy.cs
├── FlyingEnemy.cs
└── EliteEnemy.cs

Assets/_Project/Scripts/Gameplay/Projectiles/
├── Projectile.cs
├── MagicMissileProjectile.cs
└── FireballProjectile.cs

Assets/_Project/Scripts/Combat/
├── DamageCalculator.cs
└── CombatTest.cs
```

### 주요 기능
- ✅ 적 AI 순찰/추적 로직 (FSM 기반)
- ✅ 공격 범위 감지
- ✅ 투사체 발사 (원거리 적)
- ✅ 체력 시스템 통합
- ✅ 피격/사망 처리

### Enemy AI 상태 흐름
```
Idle (대기)
  ↓ 플레이어 감지
Patrol (순찰)
  ↓ 플레이어 발견
Chase (추적)
  ↓ 공격 범위 진입
Attack (공격)
  ↓ 체력 0
Death (사망)
```

---

## Phase A-3: Room System (절차적 던전)

**커밋**: `439cf08` - 기능: Phase A-3 Room System (절차적 던전) 완료
**날짜**: 2025-09-23 (추정)

### 구현 내용
- RoomData ScriptableObject
- EnemySpawnPoint 시스템
- Room 기반 전투 로직

### 생성된 파일
```
Assets/_Project/Scripts/Gameplay/Level/Room/
├── RoomData.cs
└── EnemySpawnPoint.cs
```

### 주요 기능
- ✅ 방별 적 스폰 설정
- ✅ 방 클리어 조건
- ✅ 스폰 포인트 관리
- ✅ 적 웨이브 시스템

### RoomData 구조
```csharp
[CreateAssetMenu]
public class RoomData : ScriptableObject
{
    public string roomName;
    public List<EnemySpawnPoint> spawnPoints;
    public int difficulty;
    public bool isBossRoom;
}
```

---

## Phase A-4: Item-Skill System

**커밋**: `c9171e3` - 기능: Phase A-4 Item-Skill System 구현
**날짜**: 2025-09-24 (추정)

### 구현 내용
- SkillItem, SkillData ScriptableObject
- SkillSystem, SkillItemManager
- Skill UI 시스템

### 생성된 파일
```
Assets/_Project/Scripts/Skills/
├── SkillData.cs
├── SkillEnums.cs
├── SkillSystem.cs
└── Skill.cs

Assets/_Project/Scripts/Data/
└── SkillItem.cs

Assets/_Project/Scripts/Gameplay/Item/
└── SkillItemManager.cs

Assets/_Project/Scripts/UI/
├── SkillSlotUI.cs
└── SkillUIPanel.cs

Assets/_Project/Scripts/Testing/
├── SkillItemTest.cs
└── SkillSystemTest.cs
```

### 주요 기능
- ✅ 스킬 아이템 획득
- ✅ 스킬 장착/사용
- ✅ 쿨다운 관리
- ✅ Skill UI 표시
- ✅ 스킬 슬롯 시스템

### 스킬 시스템 플로우
```
SkillItem 획득
    ↓
SkillItemManager 등록
    ↓
SkillSystem에 장착
    ↓
SkillSlotUI 업데이트
    ↓
플레이어가 스킬 사용
    ↓
쿨다운 시작
```

---

## 📊 Phase A 통계

### 생성된 파일 수
- **Core (Form)**: 8개
- **Abilities**: 6개
- **Enemy**: 4개
- **Projectiles**: 3개
- **Combat**: 2개
- **Room**: 2개
- **Skills**: 9개
- **총계**: **34개 파일**

### 주요 커밋
```bash
86dbf45 - 기능: Phase A-1 MageForm 시스템 구현
02d36c0 - 기능: Phase A-2 Enemy AI + Combat 통합 완료
439cf08 - 기능: Phase A-3 Room System (절차적 던전) 완료
c9171e3 - 기능: Phase A-4 Item-Skill System 구현
```

---

## 🎯 Phase A 성과

### 핵심 시스템 구축
- ✅ Form (Skull) 전환 시스템
- ✅ Ability 실행 프레임워크
- ✅ Enemy AI FSM
- ✅ Room 기반 전투
- ✅ Skill Item 관리

### 아키텍처 확립
- ✅ ScriptableObject 기반 데이터 관리
- ✅ FSM 패턴 적용 (Enemy AI)
- ✅ 상속 구조 설계 (BaseForm, BaseAbility)

### 게임플레이 구현
- ✅ 4가지 마법 스킬
- ✅ 3가지 적 타입
- ✅ Projectile 시스템
- ✅ 스킬 쿨다운

---

## 💡 배운 점

1. **ScriptableObject의 강력함**
   - RoomData, SkillData로 데이터 관리 용이
   - 디자이너 친화적인 구조

2. **FSM 패턴의 효과**
   - Enemy AI 상태 관리 명확
   - 디버깅 용이

3. **상속 구조 설계의 중요성**
   - BaseForm, BaseAbility로 확장 용이
   - 새 Form/Ability 추가 간단

---

## 🔗 다음 Phase

Phase A에서 구축한 기초 위에 Phase B에서는:
- 에디터 자동화 도구
- 프리팹 시스템
- 적 스폰 자동화
- UI 시스템 통합

[Phase B로 이동 →](phase-B.md)

---

**작업 기간**: 약 4일
**난이도**: ⭐⭐⭐ (새 시스템 구축)
**만족도**: ⭐⭐⭐⭐ (탄탄한 기초 확립)
