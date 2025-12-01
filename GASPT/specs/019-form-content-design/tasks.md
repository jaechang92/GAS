# 폼/캐릭터 컨텐츠 구현 태스크

**기능 번호**: 019
**생성일**: 2025-12-01
**총 태스크**: 34개
**예상 기간**: 4주

---

## Phase 1: 기반 시스템 (Setup)

> 폼 시스템의 핵심 데이터 구조와 기본 마법사를 구현합니다.

### 완료 조건
- [ ] FormData ScriptableObject 정의 완료
- [ ] 기본 마법사 폼 플레이 가능
- [ ] FormManager와 통합 완료

### 태스크 목록

- [ ] T001 [P] FormType, FormRarity 열거형 정의 in `Assets/_Project/Scripts/Forms/FormEnums.cs`
- [ ] T002 [P] FormStats 구조체 정의 in `Assets/_Project/Scripts/Forms/FormStats.cs`
- [ ] T003 FormData ScriptableObject 정의 in `Assets/_Project/Scripts/Forms/FormData.cs`
- [ ] T004 FormInstance 런타임 클래스 생성 in `Assets/_Project/Scripts/Forms/FormInstance.cs`
- [ ] T005 BasicMage.asset 생성 in `Assets/Resources/Data/Forms/BasicMage.asset`
- [ ] T006 MagicArrow 스킬 데이터 생성 in `Assets/Resources/Data/Skills/Form/Basic/MagicArrow.asset`
- [ ] T007 MagicShield 스킬 데이터 생성 in `Assets/Resources/Data/Skills/Form/Basic/MagicShield.asset`
- [ ] T008 FormManager 기본 통합 in `Assets/_Project/Scripts/Forms/FormManager.cs`

---

## Phase 2: 화염 마법사 (US1 - Power Form)

> 높은 데미지와 범위 공격 특화 폼을 구현합니다.

### 사용자 스토리
*"플레이어로서 화염 마법사로 변신하여 강력한 범위 공격으로 다수의 적을 처치하고 싶다"*

### 완료 조건
- [ ] 화염 마법사 폼으로 교체 가능
- [ ] Fireball, FireStorm 스킬 사용 가능
- [ ] 화상 상태 효과 적용

### 태스크 목록

- [ ] T009 [US1] FlameMage.asset 생성 in `Assets/Resources/Data/Forms/FlameMage.asset`
- [ ] T010 [US1] Burn 상태 효과 정의 in `Assets/Resources/Data/StatusEffects/Burn.asset`
- [ ] T011 [US1] Fireball 스킬 구현 (범위 폭발) in `Assets/Resources/Data/Skills/Form/Flame/Fireball.asset`
- [ ] T012 [US1] FireStorm 스킬 구현 (지속 범위) in `Assets/Resources/Data/Skills/Form/Flame/FireStorm.asset`
- [ ] T013 [US1] 화염 마법사 VFX 프리팹 생성 in `Assets/_Project/Prefabs/VFX/Forms/Flame/`

---

## Phase 3: 얼음 마법사 (US2 - Control Form)

> CC기와 안전한 원거리 전투 특화 폼을 구현합니다.

### 사용자 스토리
*"플레이어로서 얼음 마법사로 변신하여 적을 감속/빙결시키고 안전하게 처치하고 싶다"*

### 완료 조건
- [ ] 얼음 마법사 폼으로 교체 가능
- [ ] IceLance, FrozenGround 스킬 사용 가능
- [ ] 감속/빙결 상태 효과 적용

### 태스크 목록

- [ ] T014 [US2] FrostMage.asset 생성 in `Assets/Resources/Data/Forms/FrostMage.asset`
- [ ] T015 [P] [US2] Slow 상태 효과 정의 in `Assets/Resources/Data/StatusEffects/Slow.asset`
- [ ] T016 [P] [US2] Freeze 상태 효과 정의 in `Assets/Resources/Data/StatusEffects/Freeze.asset`
- [ ] T017 [US2] IceLance 스킬 구현 (관통 투사체) in `Assets/Resources/Data/Skills/Form/Frost/IceLance.asset`
- [ ] T018 [US2] FrozenGround 스킬 구현 (지속 범위) in `Assets/Resources/Data/Skills/Form/Frost/FrozenGround.asset`
- [ ] T019 [US2] 얼음 마법사 VFX 프리팹 생성 in `Assets/_Project/Prefabs/VFX/Forms/Frost/`

---

## Phase 4: 번개 마법사 (US3 - Speed Form)

> 빠른 공격과 기동성 특화 폼을 구현합니다.

### 사용자 스토리
*"플레이어로서 번개 마법사로 변신하여 빠른 연속 공격과 대시로 적을 압도하고 싶다"*

### 완료 조건
- [ ] 번개 마법사 폼으로 교체 가능
- [ ] ChainLightning, ThunderRush 스킬 사용 가능
- [ ] 기절 상태 효과 적용

### 태스크 목록

- [ ] T020 [US3] ThunderMage.asset 생성 in `Assets/Resources/Data/Forms/ThunderMage.asset`
- [ ] T021 [US3] Stun 상태 효과 정의 in `Assets/Resources/Data/StatusEffects/Stun.asset`
- [ ] T022 [US3] ChainLightning 스킬 구현 (연쇄 타격) in `Assets/Resources/Data/Skills/Form/Thunder/ChainLightning.asset`
- [ ] T023 [US3] ThunderRush 스킬 구현 (대시 공격) in `Assets/Resources/Data/Skills/Form/Thunder/ThunderRush.asset`
- [ ] T024 [US3] 번개 마법사 VFX 프리팹 생성 in `Assets/_Project/Prefabs/VFX/Forms/Thunder/`

---

## Phase 5: 암흑 마법사 (US4 - Drain Form)

> 흡혈과 고위험 고보상 플레이 특화 폼을 구현합니다.

### 사용자 스토리
*"플레이어로서 암흑 마법사로 변신하여 위험을 감수하고 흡혈로 생존하며 강력한 공격을 하고 싶다"*

### 완료 조건
- [ ] 암흑 마법사 폼으로 교체 가능
- [ ] LifeDrain, SoulExplosion 스킬 사용 가능
- [ ] HP 소모 메커닉 작동

### 태스크 목록

- [ ] T025 [US4] DarkMage.asset 생성 in `Assets/Resources/Data/Forms/DarkMage.asset`
- [ ] T026 [US4] Vampirism 상태 효과 정의 in `Assets/Resources/Data/StatusEffects/Vampirism.asset`
- [ ] T027 [US4] LifeDrain 스킬 구현 (채널링 흡혈) in `Assets/Resources/Data/Skills/Form/Dark/LifeDrain.asset`
- [ ] T028 [US4] SoulExplosion 스킬 구현 (HP 소모 폭발) in `Assets/Resources/Data/Skills/Form/Dark/SoulExplosion.asset`
- [ ] T029 [US4] 암흑 마법사 VFX 프리팹 생성 in `Assets/_Project/Prefabs/VFX/Forms/Dark/`

---

## Phase 6: 시너지 및 각성 시스템 (US5 - Meta Systems)

> 폼 교체 시너지와 각성 시스템을 구현합니다.

### 사용자 스토리
*"플레이어로서 폼을 교체하며 시너지 효과를 얻고, 폼을 각성시켜 더 강해지고 싶다"*

### 완료 조건
- [ ] 폼 교체 시 시너지 버프 발동
- [ ] 각성 시 등급 상승 및 스탯 증가
- [ ] 각성 VFX 효과 재생

### 태스크 목록

- [ ] T030 [US5] FormSynergy 시스템 구현 in `Assets/_Project/Scripts/Forms/FormSynergy.cs`
- [ ] T031 [US5] FormAwakening 시스템 구현 in `Assets/_Project/Scripts/Forms/FormAwakening.cs`
- [ ] T032 [US5] 각성 VFX 프리팹 생성 in `Assets/_Project/Prefabs/VFX/Forms/Awakening/`

---

## Phase 7: 밸런싱 및 폴리싱 (Polish)

> 전체 시스템 밸런스 조정 및 최종 테스트를 수행합니다.

### 완료 조건
- [ ] DPS 밸런스 ±30% 이내
- [ ] 모든 폼/스킬 정상 작동 확인
- [ ] SFX 통합 완료

### 태스크 목록

- [ ] T033 FormDropTable 구현 (드롭 확률) in `Assets/_Project/Scripts/Forms/FormDropTable.cs`
- [ ] T034 DPS 밸런스 테스트 및 조정 in `Assets/Resources/Data/Forms/*.asset`

---

## 의존성 그래프

```
Phase 1 (기반)
    ↓
┌───┴───┬───────┬───────┐
↓       ↓       ↓       ↓
Phase 2 Phase 3 Phase 4 Phase 5
(화염)   (얼음)   (번개)   (암흑)
    ↓       ↓       ↓       ↓
    └───────┴───┬───┴───────┘
                ↓
            Phase 6 (시너지/각성)
                ↓
            Phase 7 (밸런싱)
```

---

## 병렬 실행 가능 태스크

### Phase 1 내 병렬
- T001, T002 (Enum과 Struct 독립)

### Phase 2~5 병렬
- Phase 2, 3, 4, 5는 Phase 1 완료 후 **동시 진행 가능**

### Phase 3 내 병렬
- T015, T016 (Slow, Freeze 상태 효과 독립)

---

## MVP 범위 (최소 구현)

**MVP = Phase 1 + Phase 2**

| 항목 | MVP 포함 |
|------|:--------:|
| 기본 마법사 | ✅ |
| 화염 마법사 | ✅ |
| 얼음 마법사 | ❌ |
| 번개 마법사 | ❌ |
| 암흑 마법사 | ❌ |
| 시너지 시스템 | ❌ |
| 각성 시스템 | ❌ |

---

## 구현 전략

1. **점진적 전달**: 각 Phase 완료 시 플레이 가능한 폼 추가
2. **테스트 우선**: 각 폼 구현 후 즉시 테스트
3. **데이터 기반**: ScriptableObject로 밸런스 조정 용이하게
4. **VFX 후순위**: 기능 완성 후 시각 효과 추가

---

*생성: GASPT Task Generator*
*최종 수정: 2025-12-01*
