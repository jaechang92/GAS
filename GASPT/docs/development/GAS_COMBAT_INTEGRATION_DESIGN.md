# 🎮 GAS 기반 Combat 시스템 통합 설계

**작성일**: 2025-10-18
**Phase**: Phase 2.5 - VFX/사운드 시스템 통합
**목표**: ComboSystem을 GAS 어빌리티 시스템과 통합하여 VFX/사운드 지원 강화

---

## 📋 목차
- [개요](#개요)
- [현재 상황 분석](#현재-상황-분석)
- [통합 아키텍처](#통합-아키텍처)
- [구현 계획](#구현-계획)
- [예상 효과](#예상-효과)

---

## 🎯 개요

### 목적
현재 `ComboSystem` 기반의 공격 시스템을 `GAS (Gameplay Ability System)`와 통합하여:
1. **VFX 시스템** 자동화 (이펙트 Prefab 자동 생성)
2. **사운드 시스템** 자동화 (사운드 자동 재생)
3. **애니메이션 연동** 강화 (AnimationTrigger 지원)
4. **확장성** 향상 (새로운 스킬 추가 용이)

### 배경
- GAS Core에 이미 완성된 VFX/사운드 시스템 존재
- 현재는 ComboSystem만 사용 중 (GAS 미활용)
- 수동으로 VFX/사운드를 추가해야 하는 불편함

---

## 🔍 현재 상황 분석

### 기존 구조 (ComboSystem 기반)

```
PlayerAttackState
    ↓
ComboSystem.RegisterHit()
    ↓
SpawnHitboxSync(ComboData)
    ↓
DamageSystem.ApplyBoxDamage()
    ↓
[수동] DrawHitboxDebug() ← 디버그 전용, VFX 없음
```

**문제점**:
- ✅ 콤보 로직은 잘 작동
- ❌ VFX/사운드가 없음
- ❌ 이펙트 추가 시 매번 수동 코딩 필요
- ❌ GAS에 이미 있는 기능을 재발명

---

### GAS가 제공하는 기능

```csharp
// BasicAttack.cs (GAS 어빌리티)
protected override async Awaitable ExecuteActiveAbility(...)
{
    TriggerAnimation();     // ← 애니메이션 자동 트리거
    PlaySound();            // ← 사운드 자동 재생
    SpawnEffect();          // ← 이펙트 자동 생성
    CreateAndActivateHitbox();
}
```

**GAS가 이미 지원하는 것**:
- ✅ AbilityData.EffectPrefab → 자동 이펙트 생성
- ✅ AbilityData.SoundEffect → 자동 사운드 재생
- ✅ AbilityData.AnimationTrigger → 자동 애니메이션
- ✅ 쿨다운 관리
- ✅ 리소스 관리 (마나, 스태미나 등)

---

## 🏗️ 통합 아키텍처

### 통합 후 구조

```
PlayerAttackState
    ↓
[1] ComboSystem.GetCurrentComboData()
    ↓
[2] GAS.ActivateAbility("Combo_0/1/2")
    ↓
[3] ComboAbility.ExecuteActiveAbility()
    ↓
    ├─ PlaySound() ← GAS 자동
    ├─ SpawnEffect() ← GAS 자동
    ├─ TriggerAnimation() ← GAS 자동
    └─ CreateHitbox() + DamageSystem
```

**핵심 변경점**:
- ComboSystem: 콤보 로직만 담당 (어떤 콤보 단계인지 관리)
- GAS: 실제 공격 실행 + VFX/사운드 자동 처리
- DamageSystem: 데미지 적용 (기존과 동일)

---

## 📐 데이터 구조 설계

### 1. ComboAbilityData (ScriptableObject)

```csharp
[CreateAssetMenu(fileName = "ComboAttack", menuName = "GASPT/Abilities/ComboAttack")]
public class ComboAbilityData : AbilityData
{
    [Header("Combo 설정")]
    public int comboIndex = 0;              // 0:1단, 1:2단, 2:3단
    public float damageMultiplier = 1.0f;   // 데미지 배율

    [Header("Hitbox 설정")]
    public Vector2 hitboxSize = new Vector2(1.5f, 1f);
    public Vector2 hitboxOffset = new Vector2(0.5f, 0f);
    public float hitboxDuration = 0.2f;

    [Header("Knockback 설정")]
    public float knockbackForce = 5f;
    public float stunDuration = 0.3f;

    [Header("VFX/사운드 (AbilityData 상속)")]
    // EffectPrefab (부모 클래스에서 상속)
    // SoundEffect (부모 클래스에서 상속)
    // AnimationTrigger (부모 클래스에서 상속)
}
```

**특징**:
- `AbilityData`를 상속하여 GAS 기능 모두 사용
- ComboData의 모든 정보 포함
- VFX/사운드는 부모 클래스 필드 활용

---

### 2. ComboAbility (Ability 확장)

```csharp
public class ComboAbility : Ability
{
    protected override async Awaitable ExecuteActiveAbility(CancellationToken ct)
    {
        var comboData = Data as ComboAbilityData;

        // 1. GAS 기본 기능 (자동)
        TriggerAnimation();  // AnimationTrigger 자동 실행
        PlaySound();         // SoundEffect 자동 재생
        SpawnEffect();       // EffectPrefab 자동 생성

        // 2. Hitbox 생성 및 데미지
        await CreateHitboxAsync(comboData, ct);
    }

    private async Awaitable CreateHitboxAsync(ComboAbilityData data, CancellationToken ct)
    {
        // 기존 SpawnHitboxSync() 로직 이동
        Vector3 center = CalculateHitboxCenter(data);

        var damageData = DamageData.CreateWithKnockback(
            baseDamage * data.damageMultiplier,
            DamageType.Physical,
            owner,
            data.knockbackForce
        );

        DamageSystem.ApplyBoxDamage(center, data.hitboxSize, 0f, damageData, targetLayer);

        await Awaitable.WaitForSecondsAsync(data.hitboxDuration, ct);
    }
}
```

---

### 3. PlayerAttackState 리팩토링

```csharp
protected override void EnterStateSync()
{
    var comboSystem = playerController.ComboSystem;
    int currentIndex = comboSystem.CurrentComboIndex;

    // ComboSystem에 등록 (콤보 로직만)
    bool registered = comboSystem.RegisterHit(currentIndex);

    if (registered)
    {
        // GAS 어빌리티 활성화 (VFX/사운드 자동 처리)
        string abilityId = $"Combo_{currentIndex}";
        playerController.ActivateAbility(abilityId);

        attackTriggered = true;
    }
}
```

**핵심**:
- `SpawnHitboxSync()` 제거 → GAS가 처리
- `DrawHitboxDebug()` 제거 → GAS EffectPrefab으로 대체
- 콤보 인덱스만 관리, 실제 실행은 GAS에 위임

---

## 📝 구현 계획

### Phase 1: 데이터 및 어빌리티 클래스 생성
1. ✅ `ComboAbilityData.cs` 작성
2. ✅ `ComboAbility.cs` 작성
3. ✅ Assembly 참조 확인 (GAS.Core → Combat)

### Phase 2: PlayerAttackState 리팩토링
1. ✅ GAS 기반으로 공격 실행 변경
2. ✅ SpawnHitboxSync() 로직을 ComboAbility로 이동
3. ✅ ComboSystem 연동 유지

### Phase 3: ComboSystem 개선
1. ✅ GAS와 협력하도록 인터페이스 조정
2. ✅ 콤보 데이터 제공 메서드 추가

### Phase 4: AbilitySystem 초기화
1. ✅ PlayerController에서 3개 어빌리티 등록
   - `Combo_0` (1단 공격)
   - `Combo_1` (2단 공격)
   - `Combo_2` (3단 공격)

### Phase 5: VFX Placeholder 생성
1. ✅ 간단한 파티클 Prefab 3개 생성 (테스트용)
2. ✅ ScriptableObject 3개 생성 및 연결

### Phase 6: 테스트 및 검증
1. ✅ PlayerCombatDemo에서 테스트
2. ✅ VFX/사운드 작동 확인
3. ✅ 콤보 체인 정상 작동 확인

---

## 🎨 VFX 통합 예시

### Before (수동)
```csharp
// PlayerAttackState.cs
private async void DrawHitboxDebug(Vector3 center, Vector2 size, float duration)
{
    // 수동으로 GameObject 생성
    var go = new GameObject("Hitbox_Debug");
    var sr = go.AddComponent<SpriteRenderer>();
    sr.color = new Color(1f, 0f, 0f, 0.3f);
    // ... 수동 설정
}
```

### After (자동)
```csharp
// ComboAbilityData (ScriptableObject)
[SerializeField] private GameObject effectPrefab; // ← Prefab 할당만 하면 끝

// ComboAbility.cs
SpawnEffect(); // ← GAS가 자동으로 Instantiate + Destroy
```

**변화**:
- 코드 제거: `DrawHitboxDebug()` 삭제
- 작업 방식: Unity 에디터에서 Prefab 할당
- 확장성: 새 이펙트 추가 시 코드 수정 불필요

---

## 📊 예상 효과

### 1. 개발 생산성 향상
- ✅ 새 스킬 추가 시 ScriptableObject만 생성
- ✅ VFX/사운드는 Prefab/Clip 할당으로 완료
- ✅ 코드 수정 불필요

### 2. 아티스트 친화적
- ✅ 프로그래머 없이 이펙트 교체 가능
- ✅ Inspector에서 실시간 조정
- ✅ ScriptableObject로 버전 관리 용이

### 3. 시스템 통합
- ✅ Combat + GAS 완전 통합
- ✅ 기존 ComboSystem 로직 유지
- ✅ 추후 스킬 시스템 확장 용이

### 4. 코드 품질
- ✅ 중복 제거 (GAS 재발명 방지)
- ✅ 단일 책임 원칙 준수
- ✅ 확장 가능한 구조

---

## 🚧 주의사항

### 1. 콤보 인덱스 동기화
- ComboSystem의 인덱스와 GAS AbilityId 일치 필수
- `Combo_0`, `Combo_1`, `Combo_2` 명명 규칙 준수

### 2. 메모리 관리
- EffectPrefab은 GAS가 자동으로 Destroy (2초 후)
- Static 리소스 제거 (debugTexture, debugSprite)

### 3. 테스트 필수
- 각 콤보 단계별 VFX 정상 작동 확인
- 콤보 체인 유지 확인
- DamageSystem 연동 확인

---

## 🔄 마이그레이션 가이드

### 단계별 마이그레이션

1. **새 클래스 추가** (기존 코드 유지)
   - ComboAbilityData.cs
   - ComboAbility.cs

2. **PlayerController에 AbilitySystem 초기화 추가**
   ```csharp
   private void InitializeAbilities()
   {
       abilitySystem.AddAbility(combo0Data);
       abilitySystem.AddAbility(combo1Data);
       abilitySystem.AddAbility(combo2Data);
   }
   ```

3. **PlayerAttackState 리팩토링**
   - SpawnHitboxSync() → ComboAbility로 이동
   - GAS.ActivateAbility() 호출 추가

4. **테스트 및 검증**
   - 기존 기능 정상 작동 확인
   - VFX 추가 확인

5. **레거시 코드 제거**
   - DrawHitboxDebug() 삭제
   - Static 리소스 삭제

---

## 📚 참고 문서

- [GAS Core README](../../Assets/Plugins/GAS_Core/README.md)
- [GAS Usage Guide](../../Assets/Plugins/GAS_Core/USAGE_GUIDE.md)
- [Combat System Design](CombatSystemDesign.md)
- [Current Status](CurrentStatus.md)

---

**작성자**: GASPT Development Team + Claude Code
**버전**: 1.0
**상태**: 설계 완료, 구현 대기
