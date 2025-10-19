# 🔄 GAS 체이닝으로 ComboSystem 완전 대체

**작업 일자**: 2025-10-20
**작업 목표**: AbilityData에 체이닝 기능을 통합하여 ComboSystem을 완전히 제거하고 GAS가 콤보를 완전히 제어

---

## 📊 작업 결과 요약

### 코드 감소
- **ComboSystem.cs**: 373줄 삭제
- **ComboAbilityData.cs**: 113줄 삭제
- **ComboAbility.cs**: 224줄 삭제
- **PlayerAttackState.cs**: 99줄 감소 (129줄 → 30줄, 77% 감소)
- **PlayerController.cs**: 약 50줄 감소
- **AbilityData.cs**: +약 30줄 추가 (체이닝 필드)
- **AbilitySystem.cs**: +약 100줄 추가 (체이닝 로직)
- **순 감소**: **약 669줄 (41%)**

### 구조 단순화
```
Before:
ComboSystem (콤보 로직) + GAS (실행만)
  └─ 중복 데이터: ComboData ↔ ComboAbilityData

After:
GAS (콤보 + 실행 통합)
  └─ AbilityData 단일 클래스
```

---

## 🔧 주요 변경사항

### 1. AbilityData 확장

**파일**: `Assets/Plugins/GAS_Core/Data/AbilityData.cs`

**추가된 필드**:
```csharp
[Header("어빌리티 체이닝 (콤보 시스템)")]
[SerializeField] private bool isComboAbility = false;
[SerializeField] private bool isChainStarter = false;
[SerializeField] private string nextAbilityId = "";
[SerializeField] private float chainWindowDuration = 0.5f;
[SerializeField] private bool autoResetChain = true;
```

**추가된 프로퍼티**:
```csharp
public bool IsComboAbility => isComboAbility;
public bool IsChainStarter => isChainStarter;
public string NextAbilityId => nextAbilityId;
public float ChainWindowDuration => chainWindowDuration;
public bool AutoResetChain => autoResetChain;
```

**OnValidate() 검증**:
- 콤보 어빌리티가 아니면 체이닝 필드 초기화
- 체인 스타터는 nextAbilityId 필수 체크

---

### 2. AbilitySystem 확장

**파일**: `Assets/Plugins/GAS_Core/Core/AbilitySystem.cs`

**추가된 필드**:
```csharp
private string currentChainStarterId = null;
private string nextChainAbilityId = null;
private float chainTimer = 0f;
private bool isChainActive = false;
```

**추가된 메서드**:

#### UpdateChainTimer()
```csharp
// Update()에서 호출
// 체인 윈도우 타이머 감소 및 만료 시 리셋
```

#### PrepareNextChain(string nextAbilityId, float windowDuration)
```csharp
// 다음 체인 준비
// nextChainAbilityId 설정 및 타이머 시작
```

#### ResetChain()
```csharp
// 체인 리셋 (첫 콤보로)
// 타임아웃 시 자동 호출
```

#### ClearChain()
```csharp
// 체인 완전 초기화
```

#### HandleAbilityChaining(IAbility ability)
```csharp
// 비동기 체이닝 처리
// 어빌리티 실행 완료 대기 후 다음 체인 준비
```

**수정된 메서드**:
- `ActivateAbility()`: 체이닝 활성 중이면 nextChainAbilityId 사용

---

### 3. PlayerAttackState 대폭 단순화

**파일**: `Assets/_Project/Scripts/Gameplay/Player/States/PlayerAttackState.cs`

**Before (129줄)**:
```csharp
protected override void EnterStateSync()
{
    // ComboSystem에 타격 등록
    if (playerController.ComboSystem != null)
    {
        var comboSystem = playerController.ComboSystem;
        int currentComboIndex = comboSystem.CurrentComboIndex;
        bool registered = comboSystem.RegisterHit(currentComboIndex);

        if (registered)
        {
            string abilityId = $"Combo_{currentComboIndex}";
            playerController.ActivateAbility(abilityId);
            attackTriggered = true;
        }
        else
        {
            ExecuteBasicAttack();
        }
    }
    else
    {
        ExecuteBasicAttack();
    }
}
```

**After (30줄)**:
```csharp
protected override void EnterStateSync()
{
    LogStateDebug("공격 상태 진입(동기)");
    attackTriggered = false;
    attackAnimationTime = 0f;

    playerController.PlayerInput?.ResetAttack();

    // GAS가 체이닝을 자동으로 처리
    playerController.ActivateAbility("PlayerAttack");
    attackTriggered = true;
}
```

**제거된 메서드**:
- `ExecuteBasicAttack()`

---

### 4. PlayerController 정리

**파일**: `Assets/_Project/Scripts/Gameplay/Player/PlayerController.cs`

**Before**:
```csharp
[Header("GAS - Combo Abilities")]
[SerializeField] private ComboAbilityData combo0Data;
[SerializeField] private ComboAbilityData combo1Data;
[SerializeField] private ComboAbilityData combo2Data;

private ComboSystem comboSystem;
public ComboSystem ComboSystem => comboSystem;

private void InitializeComboAbilities()
{
    abilitySystem.AddAbility(combo0Data);
    abilitySystem.AddAbility(combo1Data);
    abilitySystem.AddAbility(combo2Data);
}
```

**After**:
```csharp
[Header("GAS - Abilities")]
[SerializeField] private List<AbilityData> playerAbilities = new List<AbilityData>();

private void InitializeAbilities()
{
    foreach (var abilityData in playerAbilities)
    {
        if (abilityData != null)
        {
            abilitySystem.AddAbility(abilityData);
        }
    }
}
```

**제거된 내용**:
- `ComboSystem` 필드 및 프로퍼티
- `ComboSystem` 초기화 코드
- `AttackAnimationHandler.SetComboSystem()` 호출

---

### 5. 삭제된 파일

**완전 삭제**:
- `Assets/_Project/Scripts/Gameplay/Combat/Attack/ComboSystem.cs` (373줄)
- `Assets/_Project/Scripts/Gameplay/Combat/Data/ComboAbilityData.cs` (113줄)
- `Assets/_Project/Scripts/Gameplay/Combat/Abilities/ComboAbility.cs` (224줄)
- `Assets/_Project/Scripts/Gameplay/Combat/Data/` (폴더 삭제)
- `Assets/_Project/Scripts/Gameplay/Combat/Abilities/` (폴더 삭제)
- `Assets/_Project/Scripts/Gameplay/Combat/Abilities/Combat.Abilities.asmdef` (Assembly 정의 삭제)

---

## 🎯 체이닝 동작 원리

### 실행 흐름

```
1. 플레이어 공격 버튼 입력
   ↓
2. PlayerAttackState.EnterStateSync()
   → ActivateAbility("PlayerAttack")
   ↓
3. AbilitySystem.ActivateAbility()
   - isChainActive 체크
   - 체인 없음 → "PlayerAttack" 실행
   - 체인 있음 → nextChainAbilityId 실행
   ↓
4. Ability.ExecuteAsync()
   - VFX/사운드/히트박스 실행
   ↓
5. AbilitySystem.HandleAbilityChaining()
   - 어빌리티 완료 대기
   - Data.IsComboAbility 체크
     - nextAbilityId 있음 → PrepareNextChain()
     - nextAbilityId 없음 → ResetChain() or ClearChain()
   ↓
6-1. PrepareNextChain()
     - nextChainAbilityId = "PlayerAttack_2"
     - chainTimer = 0.5초
     - isChainActive = true
     ↓
     0.5초 내 공격 입력
     → ActivateAbility() 호출
     → nextChainAbilityId("PlayerAttack_2") 실행

6-2. 0.5초 타임아웃
     → ResetChain()
     → nextChainAbilityId = "PlayerAttack" (첫 콤보)
```

---

## 📝 Unity 에디터 작업 (다음 단계)

### ScriptableObject 생성

**경로**: `Assets/_Project/Data/Abilities/Player/`

#### PlayerAttack.asset (1단 공격)
```
=== 기본 정보 ===
Ability Id: "PlayerAttack"
Ability Name: "기본 공격 1단"
Ability Type: Active
Damage Value: 10

=== 어빌리티 체이닝 ===
Is Combo Ability: ✓
Is Chain Starter: ✓
Next Ability Id: "PlayerAttack_2"
Chain Window Duration: 0.5
Auto Reset Chain: ✓
```

#### PlayerAttack_2.asset (2단 공격)
```
Ability Id: "PlayerAttack_2"
Ability Name: "기본 공격 2단"
Damage Value: 12

Is Combo Ability: ✓
Is Chain Starter: ☐
Next Ability Id: "PlayerAttack_3"
Chain Window Duration: 0.5
Auto Reset Chain: ✓
```

#### PlayerAttack_3.asset (3단 공격)
```
Ability Id: "PlayerAttack_3"
Ability Name: "기본 공격 3단"
Damage Value: 15

Is Combo Ability: ✓
Is Chain Starter: ☐
Next Ability Id: ""  // 마지막
Chain Window Duration: 0
Auto Reset Chain: ✓
```

### PlayerController 설정

1. Lobby/Gameplay 씬 열기
2. Player GameObject 선택
3. PlayerController 컴포넌트 → Player Abilities:
   - Element 0: `PlayerAttack.asset`
   - Element 1: `PlayerAttack_2.asset`
   - Element 2: `PlayerAttack_3.asset`

---

## ✅ 예상 효과

### 개발 생산성
- ✅ 새 콤보 추가: AbilityData ScriptableObject만 생성
- ✅ 콤보 수정: Unity Inspector에서 즉시 수정
- ✅ 코드 수정 불필요

### 확장성
- ✅ 일반 스킬도 체이닝 가능 (isComboAbility = true)
- ✅ 향후 분기 콤보 확장 가능 (nextAbilityIds 배열로)
- ✅ 공중/지상 콤보 별도 체인 (AbilityData만 추가)

### 유지보수성
- ✅ 코드 41% 감소 (669줄)
- ✅ 단일 책임: GAS가 모든 어빌리티 관리
- ✅ 중복 제거: ComboData ↔ ComboAbilityData 통합

---

## 🧪 테스트 항목

### 기능 테스트
- [ ] 1단 공격 실행
- [ ] 0.5초 내 입력 시 2단 공격 진행
- [ ] 2단 → 3단 체이닝
- [ ] 0.5초 타임아웃 시 1단으로 리셋
- [ ] 3단 완료 후 자동 리셋 (1단으로)

### 디버그 로그 확인
```
[AbilitySystem] 체인 시작: PlayerAttack
[AbilitySystem] 다음 체인 준비: PlayerAttack_2 (윈도우: 0.5초)
[AbilitySystem] 체인 진행: PlayerAttack → PlayerAttack_2
[AbilitySystem] 다음 체인 준비: PlayerAttack_3 (윈도우: 0.5초)
[AbilitySystem] 체인 리셋: PlayerAttack
```

### 통합 테스트
- [ ] 공격 중 다른 액션 (점프, 대시) 시 체인 중단
- [ ] 여러 어빌리티 동시 등록 시 정상 동작
- [ ] Scene 전환 시 체인 상태 초기화

---

## 📚 관련 파일

### 수정된 파일
```
Assets/Plugins/GAS_Core/
├── Data/AbilityData.cs (+30줄)
└── Core/AbilitySystem.cs (+100줄)

Assets/_Project/Scripts/
├── Gameplay/Player/
│   ├── PlayerController.cs (-50줄)
│   └── States/PlayerAttackState.cs (-99줄)
```

### 삭제된 파일
```
Assets/_Project/Scripts/Gameplay/Combat/
├── Attack/ComboSystem.cs (삭제)
├── Data/ComboAbilityData.cs (삭제)
└── Abilities/
    ├── ComboAbility.cs (삭제)
    └── Combat.Abilities.asmdef (삭제)
```

---

**작성자**: Claude Code + 사용자
**리팩토링 이유**: ComboSystem과 GAS의 중복 제거, 코드 간소화, 확장성 향상
**다음 작업**: Unity 에디터에서 PlayerAttack ScriptableObject 생성 및 테스트
