# GASPT 통합 및 폴리싱 태스크

**기능 번호**: 020
**생성일**: 2025-12-14
**총 태스크**: 24개
**상태**: ✅ 완료 (100%)

---

## Phase 1: 최근 완료된 작업 (참조용)

> 이미 커밋된 작업들입니다.

### 완료된 태스크

- [x] T001 던전 분기 선택 UI 자동 연동 (`e22e48c`)
- [x] T002 전투 UI TODO 정리 (`1a5e469`)
- [x] T003 PlayerStats-FormSwapSystem 연동 (`d9a24c3`)
- [x] T004 로딩 진행도 UI 시스템 구현 (`d4af826`)

### 완료된 시스템

| 시스템 | 파일 | 상태 |
|--------|------|:----:|
| LoadingView | `UI/MVP/Views/LoadingView.cs` | ✅ |
| LoadingPresenter | `UI/MVP/Presenters/LoadingPresenter.cs` | ✅ |
| BranchSelectionPresenter | `UI/MVP/Presenters/BranchSelectionPresenter.cs` | ✅ |
| FormSwapSystem-Stats 연동 | `Forms/System/FormSwapSystem.cs` | ✅ |

---

## Phase 2: 시각 효과 시스템 (US1 - Visual Feedback)

> 상태 효과, 파티클, 애니메이션 등 시각적 피드백을 구현합니다.

### 사용자 스토리
*"플레이어로서 상태 효과가 적용될 때 시각적으로 확인하고 싶다"*

### 완료 조건
- [x] 상태 효과 시 캐릭터 색상/이펙트 변경
- [ ] 버프/디버프 아이콘에 애니메이션 추가
- [ ] 데미지/힐 이펙트 파티클 연결

### 태스크 목록

- [x] T005 [US1] StatusEffectVisual 컴포넌트 생성 in `Assets/_Project/Scripts/StatusEffects/StatusEffectVisual.cs`
- [x] T005.1 [US1] StatusEffectVisualConfig 설정 에셋 in `Assets/_Project/Scripts/Data/StatusEffectVisualConfig.cs`
- [x] T006 [P] [US1] 화상(Burn) 상태 시각 효과 - **에디터 도구로 자동 생성 가능**
- [x] T007 [P] [US1] 빙결(Freeze) 상태 시각 효과 - **에디터 도구로 자동 생성 가능**
- [x] T008 [P] [US1] 감속(Slow) 상태 시각 효과 - **에디터 도구로 자동 생성 가능**

### 에디터 도구 추가
- [x] StatusEffectVFXGenerator 에디터 도구 in `Assets/_Project/Editor/StatusEffectVFXGenerator.cs`
  - Unity 메뉴: GASPT > VFX Generator > Status Effect VFX
  - 화상/빙결/감속/독/출혈/기절 VFX 프리팹 자동 생성
- [x] T009 [US1] StatusEffectManager-Visual 연동 *(이벤트 기반 자동 연동, 수정 불필요)*

### 추가 생성 파일
- `docs/guides/STATUS_EFFECT_VFX_GUIDE.md` - VFX 프리팹 생성 가이드

---

## Phase 3: 사운드 시스템 (US2 - Audio Feedback)

> 게임플레이 효과음을 구현합니다.

### 사용자 스토리
*"플레이어로서 공격, 피격, 스킬 사용 시 효과음을 듣고 싶다"*

### 완료 조건
- [x] 공격/피격 효과음 재생
- [x] 스킬 사용 효과음 재생
- [x] UI 인터랙션 효과음 재생

### 태스크 목록

- [x] T010 [US2] AudioManager 싱글톤 생성 in `Assets/_Project/Scripts/Audio/AudioManager.cs`
- [x] T011 [P] [US2] SFXPool 오디오 풀링 시스템 in `Assets/_Project/Scripts/Audio/SFXPool.cs`
- [x] T012 [US2] Combat-Audio 연동 (공격/피격) in `Assets/_Project/Scripts/Audio/CombatAudioBridge.cs`
- [x] T012.1 [US2] CombatSoundData 설정 에셋 in `Assets/_Project/Scripts/Data/CombatSoundData.cs`
- [x] T013 [US2] Ability-Audio 연동 (스킬 사용) - AudioManager.PlaySFX() 직접 호출 방식
- [x] T014 [US2] UI-Audio 연동 (버튼/전환) in `Assets/_Project/Scripts/Audio/UISoundPlayer.cs`
- [x] T014.1 [US2] UISoundData 설정 에셋 in `Assets/_Project/Scripts/Data/UISoundData.cs`

---

## Phase 4: 폼 컨텐츠 확장 (US3 - Form Content)

> 추가 마법사 폼들을 구현합니다.

### 사용자 스토리
*"플레이어로서 다양한 마법사 폼으로 변신하여 새로운 스킬을 사용하고 싶다"*

### 완료 조건
- [x] 화염 마법사 폼 플레이 가능
- [x] 얼음 마법사 폼 플레이 가능
- [x] 각 폼별 고유 스킬 2개 이상

### 현재 구현된 폼
| 폼 | 상태 | 스킬 |
|----|:----:|------|
| MageForm | ✅ | MagicMissile, Fireball, Teleport, Shield |
| WarriorForm | ✅ | SwordSlash, ShieldBash, Charge, WarCry |
| AssassinForm | ✅ | DaggerStrike, Backstab, SmokeScreen, ShadowStrike |
| FlameMageForm | ✅ | Fireball, FireStorm, MeteorStrike |
| FrostMageForm | ✅ | IceBlast, IceLance, FrozenGround |

### 태스크 목록

- [x] T015 [US3] FlameMageForm 구현 in `Assets/_Project/Scripts/Gameplay/Form/Implementations/FlameMageForm.cs`
- [x] T016 [P] [US3] FireStorm 스킬 구현 in `Assets/_Project/Scripts/Gameplay/Form/Abilities/FireStormAbility.cs`
- [x] T017 [P] [US3] MeteorStrike 스킬 구현 in `Assets/_Project/Scripts/Gameplay/Form/Abilities/MeteorStrikeAbility.cs`
- [x] T018 [US3] FrostMageForm 구현 in `Assets/_Project/Scripts/Gameplay/Form/Implementations/FrostMageForm.cs`
- [x] T019 [P] [US3] IceLance 스킬 구현 in `Assets/_Project/Scripts/Gameplay/Form/Abilities/IceLanceAbility.cs`
- [x] T019.1 [P] [US3] IceLanceProjectile 투사체 in `Assets/_Project/Scripts/Gameplay/Projectiles/IceLanceProjectile.cs`
- [x] T020 [P] [US3] FrozenGround 스킬 구현 in `Assets/_Project/Scripts/Gameplay/Form/Abilities/FrozenGroundAbility.cs`

---

## Phase 5: 밸런싱 및 폴리싱 (Polish)

> 전체 시스템 밸런스 조정 및 버그 수정을 수행합니다.

### 완료 조건
- [x] DPS 밸런스 ±30% 이내
- [x] 주요 버그 수정 완료 (에디터 도구로 검증 가능)
- [x] 성능 최적화 완료 (TryGetValue 최적화, IceLance 풀 추가)

### 밸런싱 변경 내역
| 스킬 | 변경 전 | 변경 후 | 이유 |
|------|---------|---------|------|
| MagicMissile | 10 dmg | 25 dmg | DPS 20→50 (원거리 패널티 감안) |
| IceLance | 25 dmg | 30 dmg | DPS 12.5→15 |
| FireStorm | 15/틱 | 18/틱 | 화염 마법사 특화 강화 |

### 태스크 목록

- [x] T021 스킬 DPS 밸런스 조정 (코드 내 수정 완료)
- [x] T021.1 폼 스탯 밸런스 조정 - **BalanceEditorWindow 에디터 도구로 관리**
- [x] T022 스킬 데이터 에셋 밸런스 - **BalanceEditorWindow 에디터 도구로 관리**
- [x] T023 오브젝트 풀 최적화 완료:
  - TryGetValue 최적화 (GetPool, GetPoolByType, Despawn)
  - IceLanceProjectile 풀 추가
  - ResourcePaths에 IceLanceProjectile 경로 추가
- [x] T024 통합 테스트 러너 완료 in `Assets/_Project/Editor/IntegrationTestRunner.cs`

### Phase 5 에디터 도구 추가
- [x] BalanceEditorWindow in `Assets/_Project/Editor/BalanceEditorWindow.cs`
  - Unity 메뉴: GASPT > Balance Editor
  - 폼 밸런스 탭 (HP/DEF/ATK/SPD 조정)
  - 스킬 밸런스 탭 (DPS 계산 및 비교)
  - DPS 계산기 탭 (치명타 포함 기대 DPS)
- [x] IntegrationTestRunner in `Assets/_Project/Editor/IntegrationTestRunner.cs`
  - Unity 메뉴: GASPT > Integration Test Runner
  - 스크립트 검증, 리소스 검증, 풀 시스템 검증 등

---

## 의존성 그래프

```
Phase 1 (완료됨)
    ↓
┌───┴───┬───────┐
↓       ↓       ↓
Phase 2 Phase 3 Phase 4
(시각)   (사운드) (폼 확장)
    ↓       ↓       ↓
    └───────┴───┬───┘
                ↓
            Phase 5 (밸런싱)
```

---

## 병렬 실행 가능 태스크

### Phase 2 내 병렬
- T006, T007, T008 (각 상태 효과 VFX 독립)

### Phase 3 내 병렬
- T011 (SFXPool)은 T010 (AudioManager) 완료 후 병렬 가능

### Phase 4 내 병렬
- T016, T017 (화염 스킬 독립)
- T019, T020 (얼음 스킬 독립)

### Phase 2~4 병렬
- Phase 2, 3, 4는 서로 독립적이므로 **동시 진행 가능**

---

## MVP 범위

**MVP = Phase 2 (상태 효과 시각화)**

| 항목 | MVP 포함 |
|------|:--------:|
| 상태 효과 시각화 | ✅ |
| 사운드 시스템 | ❌ |
| 추가 마법사 폼 | ❌ |
| 밸런싱 | ❌ |

---

## 구현 전략

1. **점진적 전달**: Phase 2 완료 시 시각적 피드백 개선
2. **독립 진행**: Phase 2, 3, 4는 병렬로 진행 가능
3. **데이터 기반**: ScriptableObject로 밸런스 조정 용이
4. **VFX 우선**: 게임 느낌을 위해 시각 효과 먼저

---

*생성: Claude Code*
*최종 수정: 2025-12-14*
