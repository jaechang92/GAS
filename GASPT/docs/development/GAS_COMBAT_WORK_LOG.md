# 🎮 GAS 기반 Combat 시스템 통합 - 작업 일지

**작업 일자**: 2025-10-18
**작업 목표**: ComboSystem을 GAS와 통합하여 VFX/사운드 자동 지원

---

## 📋 작업 진행 상황

### ✅ 완료된 작업 (코드 레벨)

#### 1. 설계 문서 작성
- **파일**: `docs/development/GAS_COMBAT_INTEGRATION_DESIGN.md`
- **내용**: GAS-Combat 통합 아키텍처 설계
- **주요 내용**:
  - 현재 상황 분석 (ComboSystem 기반 → GAS 통합)
  - 통합 아키텍처 설계
  - 데이터 구조 설계 (ComboAbilityData, ComboAbility)
  - 마이그레이션 가이드

#### 2. ComboAbilityData.cs 생성
- **위치**: `Assets/_Project/Scripts/Gameplay/Combat/Data/ComboAbilityData.cs`
- **상속**: `GAS.Core.AbilityData`
- **주요 필드**:
  ```csharp
  - comboIndex (0/1/2)
  - damageMultiplier
  - baseDamage
  - hitboxSize, hitboxOffset, hitboxDuration, hitboxSpawnDelay
  - knockbackForce, stunDuration
  - targetLayers
  - showGizmos, debugLog
  ```
- **메서드**:
  - `GetFinalDamage()`: baseDamage * damageMultiplier
  - `GetStunDuration()`: stunDuration + (comboIndex * 0.1f)
  - `OnValidate()`: AbilityId, AbilityName 자동 설정

#### 3. ComboAbility.cs 생성
- **위치**: `Assets/_Project/Scripts/Gameplay/Combat/Abilities/ComboAbility.cs`
- **네임스페이스**: `Combat.Abilities`
- **상속**: `GAS.Core.Ability`
- **주요 구현**:
  ```csharp
  - ComboData 프로퍼티 (Data as ComboAbilityData)
  - ExecuteActiveAbility() 오버라이드:
    1. TriggerAnimation() - 애니메이션 자동 실행
    2. PlaySound() - 사운드 자동 재생
    3. SpawnEffect() - VFX 자동 생성
    4. CreateHitbox() - DamageSystem 통합
  - DrawHitboxDebug() - 콤보별 색상 (빨강/주황/노랑)
  ```

#### 4. PlayerAttackState.cs 리팩토링
- **위치**: `Assets/_Project/Scripts/Gameplay/Player/States/PlayerAttackState.cs`
- **변경 사항**:
  - 264줄 → 129줄 (51% 코드 감소)
  - `SpawnHitboxSync()` 제거 → GAS가 처리
  - `DrawHitboxDebug()` 제거 → GAS가 처리
  - Static 리소스 제거 (debugTexture, debugSprite)
  - Combat 의존성 제거 (Combat.Attack, Combat.Core)
- **새 로직**:
  ```csharp
  EnterStateSync():
    string abilityId = $"Combo_{currentComboIndex}";
    playerController.ActivateAbility(abilityId);
  ```

#### 5. PlayerController.cs 수정
- **위치**: `Assets/_Project/Scripts/Gameplay/Player/PlayerController.cs`
- **추가된 필드**:
  ```csharp
  [Header("GAS - Combo Abilities")]
  [SerializeField] private ComboAbilityData combo0Data;
  [SerializeField] private ComboAbilityData combo1Data;
  [SerializeField] private ComboAbilityData combo2Data;
  ```
- **추가된 메서드**:
  ```csharp
  private void InitializeComboAbilities()
  {
      abilitySystem.AddAbility(combo0Data); // Combo_0
      abilitySystem.AddAbility(combo1Data); // Combo_1
      abilitySystem.AddAbility(combo2Data); // Combo_2
  }
  ```

#### 6. Assembly 참조 문제 해결
- **생성**: `Combat.Abilities.asmdef`
  - 위치: `Assets/_Project/Scripts/Gameplay/Combat/Abilities/`
  - 참조: `Combat.Core`, `GAS.Core`, `Core.Enums`
- **수정**: `Combat.Core.asmdef`
  - `GAS.Core` 참조 추가

#### 7. Initialize 오버라이드 문제 해결
- **문제**: `Ability.Initialize()`는 virtual이 아님
- **해결**:
  ```csharp
  // Before (에러)
  public override void Initialize(...) { ... }

  // After (정상)
  private ComboAbilityData ComboData => Data as ComboAbilityData;
  ```

#### 8. 마지막 수정사항 (knockback 방향 수정)
- **파일**: `ComboAbility.cs:76`
- **수정**:
  ```csharp
  // Before
  ComboData.knockbackForce * facingDirection

  // After
  ComboData.knockbackForce * facingDirection * Vector2.right
  ```

---

## ⏳ 남은 작업 (Unity 에디터 작업)

### Step 1: ComboAbilityData 3개 ScriptableObject 생성

**위치**: `Assets/_Project/Data/Abilities/Player/`

#### Combo_0.asset (1단 공격)
```
Create > GASPT > Abilities > ComboAttack

설정값:
- Ability Id: "Combo_0"
- Ability Name: "1단 공격"
- Ability Type: Active
- comboIndex: 0
- damageMultiplier: 1.0
- baseDamage: 10
- hitboxSize: (1.5, 1.0)
- hitboxOffset: (0.5, 0.0)
- hitboxDuration: 0.2
- hitboxSpawnDelay: 0.1
- knockbackForce: 5
- stunDuration: 0.3
- targetLayers: Enemy 레이어 선택
- showGizmos: true
- debugLog: true (테스트 시)
```

#### Combo_1.asset (2단 공격)
```
설정값:
- Ability Id: "Combo_1"
- Ability Name: "2단 공격"
- Ability Type: Active
- comboIndex: 1
- damageMultiplier: 1.2
- baseDamage: 10
- hitboxSize: (2.0, 1.2)
- hitboxOffset: (0.6, 0.0)
- hitboxDuration: 0.25
- hitboxSpawnDelay: 0.15
- knockbackForce: 7
- stunDuration: 0.4
- targetLayers: Enemy 레이어 선택
- showGizmos: true
- debugLog: true
```

#### Combo_2.asset (3단 공격)
```
설정값:
- Ability Id: "Combo_2"
- Ability Name: "3단 공격"
- Ability Type: Active
- comboIndex: 2
- damageMultiplier: 1.5
- baseDamage: 10
- hitboxSize: (2.5, 1.5)
- hitboxOffset: (0.8, 0.0)
- hitboxDuration: 0.3
- hitboxSpawnDelay: 0.2
- knockbackForce: 10
- stunDuration: 0.5
- targetLayers: Enemy 레이어 선택
- showGizmos: true
- debugLog: true
```

### Step 2: PlayerController에 ScriptableObject 할당

1. Scene에서 Player GameObject 선택
2. Inspector에서 `PlayerController` 컴포넌트 찾기
3. **GAS - Combo Abilities** 섹션:
   - Combo 0 Data ← `Combo_0.asset` 드래그
   - Combo 1 Data ← `Combo_1.asset` 드래그
   - Combo 2 Data ← `Combo_2.asset` 드래그

### Step 3: (선택) VFX Placeholder Prefab 생성

**간단한 파티클 이펙트 생성**:

1. Hierarchy에서 빈 GameObject 생성 → 이름: "ComboEffect_1"
2. Particle System 추가:
   - Duration: 0.5
   - Start Lifetime: 0.3
   - Start Speed: 2
   - Start Color: 빨간색
   - Start Size: 0.5
   - Emission - Rate over Time: 20
   - Shape - Shape: Sphere, Radius: 0.2
3. Prefab으로 저장: `Assets/_Project/Prefabs/VFX/ComboEffect_1.prefab`
4. 같은 방식으로 `ComboEffect_2` (주황색), `ComboEffect_3` (노란색) 생성
5. 각 ComboAbilityData의 `EffectPrefab`에 할당

### Step 4: 테스트

**테스트 씬**: PlayerCombatDemo

**테스트 항목**:
1. ✅ Unity 컴파일 성공 확인
2. ✅ Player GameObject에 ComboAbilityData 3개 할당 확인
3. ✅ 공격 키 입력 시 1→2→3 콤보 전환 확인
4. ✅ 히트박스 디버그 시각화 확인 (빨강→주황→노랑)
5. ✅ 데미지 적용 확인 (Enemy 체력 감소)
6. ✅ 넉백 효과 확인 (오른쪽 방향)
7. ✅ VFX 이펙트 생성 확인 (Prefab 할당 시)
8. ✅ 콘솔 로그 확인 (debugLog = true일 때)

**예상 로그**:
```
[ComboAbility] 콤보 1단 공격 실행
[ComboAbility] 애니메이션 트리거: Attack1
[ComboAbility] 히트박스 생성: 1개 타격, 데미지: 10
[ComboAbility] 콤보 1단 공격 완료
```

### Step 5: 문서 업데이트

**업데이트할 파일**:
- `docs/development/CurrentStatus.md`
  - Phase 2 → Phase 2.5 변경
  - "GAS-Combat 통합 완료" 추가
  - VFX/사운드 시스템 통합 완료 표시

**추가할 내용**:
```markdown
## Phase 2.5: VFX/사운드 시스템 통합 ✅ (2025-10-18)

### 완료된 작업
- ✅ GAS 기반 Combat 시스템 통합
- ✅ ComboAbilityData ScriptableObject 생성
- ✅ ComboAbility 실행기 구현
- ✅ PlayerAttackState GAS 기반 리팩토링
- ✅ Assembly 참조 구조 개선

### 주요 개선 사항
- 코드 51% 감소 (PlayerAttackState: 264줄 → 129줄)
- VFX/사운드 자동 처리
- ScriptableObject 기반 확장 가능
- Combat 의존성 제거

### 다음 작업
- Unity 에디터에서 ComboAbilityData 3개 생성 및 할당
- VFX Placeholder Prefab 생성
- 통합 테스트
```

---

## 🔧 기술적 세부사항

### Assembly 구조
```
Combat.Abilities (새로 생성)
  ├─> Combat.Core (DamageSystem, DamageData)
  ├─> GAS.Core (Ability 상속)
  └─> Core.Enums (DamageType)

Combat.Core (수정)
  ├─> GAS.Core (AbilityData 상속) ← 새로 추가
  ├─> Core.Enums
  └─> Core.Utilities

Player (기존)
  ├─> Combat.Core (ComboAbilityData)
  ├─> GAS.Core (ActivateAbility)
  └─> Combat.Attack
```

### 핵심 데이터 흐름
```
PlayerAttackState.EnterStateSync()
    ↓
ComboSystem.RegisterHit() → 콤보 인덱스 결정
    ↓
PlayerController.ActivateAbility("Combo_0/1/2")
    ↓
GAS.AbilitySystem.ActivateAbility()
    ↓
ComboAbility.ExecuteActiveAbility()
    ├─> TriggerAnimation() → Animator.SetTrigger()
    ├─> PlaySound() → AudioSource.PlayClipAtPoint()
    ├─> SpawnEffect() → Instantiate(EffectPrefab)
    └─> CreateHitbox() → DamageSystem.ApplyBoxDamage()
```

### 주요 클래스 설명

**ComboAbilityData**:
- ScriptableObject 기반
- AbilityData 상속으로 GAS 기능 모두 사용
- Inspector에서 데미지/히트박스/VFX 설정 가능

**ComboAbility**:
- Ability 상속
- ExecuteActiveAbility() 오버라이드
- DamageSystem과 통합

**PlayerAttackState**:
- GAS 기반으로 완전 리팩토링
- ComboSystem은 콤보 로직만 담당
- 실제 공격은 GAS에 위임

---

## ⚠️ 주의사항

### 1. Assembly 참조 순환 방지
- Combat.Abilities는 Combat.Core를 참조
- Combat.Core는 GAS.Core를 참조
- Player는 Combat.Core를 참조
- **절대 순환 참조 생성하지 말 것**

### 2. ComboAbilityData 설정 체크리스트
- ✅ Ability Id: "Combo_0/1/2" 형식 필수
- ✅ Ability Type: Active 필수
- ✅ comboIndex: 0/1/2 정확히 설정
- ✅ targetLayers: Enemy 레이어 선택
- ✅ showGizmos: true (테스트 시)

### 3. 테스트 시 확인사항
- PlayerController에 3개 ComboAbilityData 모두 할당됐는지
- ComboSystem이 정상 작동하는지
- DamageSystem이 타겟을 찾는지 (targetLayers 확인)
- 디버그 로그가 출력되는지 (debugLog = true)

---

## 📚 관련 파일 목록

### 코드 파일
```
Assets/_Project/Scripts/Gameplay/Combat/
├── Data/
│   └── ComboAbilityData.cs (새로 생성)
├── Abilities/
│   ├── ComboAbility.cs (새로 생성)
│   └── Combat.Abilities.asmdef (새로 생성)
└── Combat.Core.asmdef (수정)

Assets/_Project/Scripts/Gameplay/Player/
├── PlayerController.cs (수정)
└── States/
    └── PlayerAttackState.cs (수정)
```

### 문서 파일
```
docs/development/
├── GAS_COMBAT_INTEGRATION_DESIGN.md (새로 생성)
├── GAS_COMBAT_WORK_LOG.md (이 파일)
└── CurrentStatus.md (업데이트 필요)
```

### Unity 에셋 (생성 필요)
```
Assets/_Project/Data/Abilities/Player/
├── Combo_0.asset (생성 필요)
├── Combo_1.asset (생성 필요)
└── Combo_2.asset (생성 필요)

Assets/_Project/Prefabs/VFX/ (선택)
├── ComboEffect_1.prefab (선택)
├── ComboEffect_2.prefab (선택)
└── ComboEffect_3.prefab (선택)
```

---

## 🎯 다음 세션 체크리스트

**재시작 시 순서**:

1. ✅ Unity 에디터 열기
2. ✅ 컴파일 에러 확인 (없어야 정상)
3. ✅ 이 문서 읽기 (`GAS_COMBAT_WORK_LOG.md`)
4. ✅ ComboAbilityData 3개 생성 (Step 1)
5. ✅ PlayerController에 할당 (Step 2)
6. ✅ (선택) VFX Prefab 생성 (Step 3)
7. ✅ PlayerCombatDemo 테스트 (Step 4)
8. ✅ CurrentStatus.md 업데이트 (Step 5)

**예상 소요 시간**: 30분 ~ 1시간

---

## 💡 트러블슈팅

### Q: 컴파일 에러가 발생하면?
**A**: Assembly 참조 확인
- Combat.Abilities.asmdef 존재 확인
- Combat.Core.asmdef에 GAS.Core 참조 확인

### Q: ComboAbility가 실행되지 않으면?
**A**: PlayerController 설정 확인
- combo0/1/2Data가 할당됐는지 확인
- AbilitySystem 컴포넌트 존재 확인
- InitializeComboAbilities() 호출 확인

### Q: 히트박스가 작동하지 않으면?
**A**: 레이어 설정 확인
- ComboAbilityData.targetLayers에 Enemy 선택됐는지
- Enemy GameObject의 Layer가 "Enemy"인지
- DamageSystem이 LayerMask 정상 처리하는지

### Q: VFX가 생성되지 않으면?
**A**: Prefab 할당 확인
- ComboAbilityData.EffectPrefab이 null이 아닌지
- Prefab이 유효한지
- SpawnEffect() 로그 확인

---

**작성자**: Claude Code
**최종 업데이트**: 2025-10-18
**다음 작업자**: Unity 에디터 작업 필요
**예상 완료 시간**: 30분 ~ 1시간
