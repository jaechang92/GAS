# 🎮 Combat System Demo Guide

Combat 시스템을 테스트하고 실습할 수 있는 데모 씬 가이드입니다.

---

## 📋 목차

1. [빠른 시작](#빠른-시작)
2. [데모 씬 설정](#데모-씬-설정)
3. [조작 방법](#조작-방법)
4. [테스트 시나리오](#테스트-시나리오)
5. [스크립트 설명](#스크립트-설명)

---

## 🚀 빠른 시작

### 방법 1: 자동 설정 (가장 간단)

1. **새 씬 생성**
   - `File > New Scene` 또는 기존 TestScene 사용

2. **빈 GameObject 생성**
   - `Hierarchy > 우클릭 > Create Empty`
   - 이름: "CombatDemo"

3. **스크립트 추가**
   - `CombatDemoScene.cs` 컴포넌트 추가
   - Inspector에서 `Auto Setup On Start` 체크

4. **Play 버튼 클릭**
   - 자동으로 플레이어와 적 생성됨
   - GUI가 표시됨

### 방법 2: UI 컨트롤러 사용

1. **방법 1 완료 후**

2. **UI 컨트롤러 추가**
   - 같은 GameObject에 `CombatTestUI.cs` 추가

3. **Play 버튼 클릭**
   - 더 상세한 UI와 테스트 옵션 제공

---

## ⚙️ 데모 씬 설정

### CombatDemoScene 컴포넌트 설정

```
CombatDemoScene
├── 생성 설정
│   ├── Auto Setup On Start: true (자동 설정)
│   ├── Player Spawn Position: (0, 0, 0)
│   └── Enemy Spawn Position: (5, 0, 0)
│
├── 프리팹 (옵션)
│   ├── Player Prefab: (선택사항)
│   └── Enemy Prefab: (선택사항)
│
└── 테스트 설정
    ├── Test Damage Amount: 25
    └── Test Damage Type: Physical
```

### 자동 생성되는 요소

✅ **DamageSystem** - 싱글톤으로 자동 생성
✅ **Player** - 초록색 Capsule
✅ **Enemy** - 빨간색 Capsule
✅ **HealthSystem** - 각 엔티티에 자동 추가
✅ **이벤트 연결** - 데미지/회복/사망 이벤트 자동 연결

---

## 🎮 조작 방법

### 키보드 단축키

| 키 | 동작 |
|---|---|
| **1** | 플레이어가 적 공격 |
| **2** | 적이 플레이어 공격 |
| **3** | 플레이어 회복 |
| **4** | 적 회복 |
| **5** | 범위 공격 (플레이어 중심) |
| **6** | 크리티컬 공격 |
| **R** | 씬 리셋 |
| **H** | 도움말 출력 |
| **Tab** | UI 토글 (CombatTestUI 사용 시) |

### GUI 버튼

**기본 GUI (CombatDemoScene)**
- 좌측 상단: 체력 정보 및 조작 버튼
- 우측 하단: 이벤트 로그 (최근 10개)

**고급 GUI (CombatTestUI)**
- 메인 패널: 데미지 타입, 설정, 공격/회복 버튼
- 통계 패널: 공격 횟수, 크리티컬, 총 피해량
- 로그 패널: 전투 로그 (최근 20개)

---

## 🧪 테스트 시나리오

### 시나리오 1: 기본 전투 테스트

1. **Play 모드 진입**
2. **키보드 1번** 누르기 (플레이어 공격)
   - Enemy HP 감소 확인
   - 이벤트 로그 확인
3. **키보드 2번** 누르기 (적 공격)
   - Player HP 감소 확인
4. **키보드 3번** 누르기 (플레이어 회복)
   - HP 회복 확인

✅ **예상 결과**: 데미지/회복 정상 작동

### 시나리오 2: 데미지 타입 테스트

**CombatTestUI 사용 시:**

1. **데미지 타입 버튼 클릭**
   - Physical → Magical → Fire → Ice
2. **공격 실행**
3. **로그에서 데미지 타입 확인**

✅ **예상 결과**: 다양한 데미지 타입 적용

### 시나리오 3: 크리티컬 히트 테스트

1. **고급 옵션 표시** 체크
2. **크리티컬 활성화** 체크
3. **크리티컬 배율 설정** (2.0x ~ 5.0x)
4. **공격 실행**
5. **통계 패널에서 크리티컬 확률 확인**

✅ **예상 결과**: 크리티컬 적용 시 높은 데미지

### 시나리오 4: 범위 공격 테스트

1. **키보드 5번** 누르기 (범위 공격)
2. **콘솔 로그 확인**: 몇 개 타격했는지 표시
3. **Enemy가 범위 내에 있으면 데미지 받음**

✅ **예상 결과**: 범위 내 모든 타겟 타격

### 시나리오 5: 무적 상태 테스트

**Inspector에서 직접 테스트:**

1. **Play 모드에서 Enemy 선택**
2. **HealthSystem 컴포넌트 찾기**
3. **Is Invincible** 체크
4. **공격 실행**

✅ **예상 결과**: 데미지 무시됨

### 시나리오 6: 환경 데미지 (무적 무시)

1. **데미지 타입: Environmental 선택**
2. **Enemy 무적 상태로 설정**
3. **공격 실행**

✅ **예상 결과**: 무적 상태에도 데미지 적용

### 시나리오 7: 사망 처리 테스트

1. **연속 공격으로 Enemy HP 0으로 만들기**
2. **콘솔 로그 확인**: "[Enemy] 사망!" 메시지
3. **추가 공격 시도**

✅ **예상 결과**: 사망 후 데미지 무시

---

## 📜 스크립트 설명

### CombatDemoScene.cs

**목적**: 간단하고 빠른 Combat 시스템 테스트

**주요 기능**:
- ✅ 자동 씬 설정
- ✅ 플레이어/적 생성
- ✅ 이벤트 로깅
- ✅ 키보드 단축키
- ✅ Context Menu 테스트

**사용 예시**:
```csharp
// Inspector 우클릭 메뉴에서 실행 가능
[ContextMenu("Test: Player Attack Enemy")]
public void PlayerAttackEnemy()
{
    DamageSystem.ApplyDamage(enemy, testDamageAmount, testDamageType, player);
}
```

### CombatTestUI.cs

**목적**: 고급 테스트 및 통계 분석

**주요 기능**:
- ✅ 다양한 데미지 타입 선택
- ✅ 데미지/크리티컬 배율 조정
- ✅ 실시간 통계 표시
- ✅ 상세한 전투 로그
- ✅ 체력 바 시각화

**사용 예시**:
```csharp
// 고급 설정으로 공격
private void AttackTarget(GameObject attacker, GameObject target)
{
    float finalDamage = baseDamage * damageMultiplier;

    DamageData damage = enableCritical
        ? DamageData.CreateCritical(finalDamage, selectedDamageType, attacker, criticalMultiplier)
        : DamageData.Create(finalDamage, selectedDamageType, attacker);

    DamageSystem.ApplyDamage(target, damage);
}
```

---

## 🔧 커스터마이징

### 프리팹 사용

1. **플레이어/적 프리팹 생성**
2. **CombatDemoScene Inspector에 할당**
3. **Play 시 자동으로 프리팹 사용**

### 데미지 공식 변경

`DamageSystem.cs`에서 데미지 계산 로직 수정:

```csharp
private DamageData CalculateDamage(DamageData baseDamage)
{
    // 여기서 커스텀 데미지 계산
    return damage;
}
```

### 이벤트 커스터마이징

`CombatDemoScene.cs`의 이벤트 연결 부분:

```csharp
playerHealth.OnDamaged += (damage) =>
{
    // 커스텀 처리
    Debug.Log($"커스텀 이벤트: {damage.amount}");
};
```

---

## 📊 통계 분석

### CombatTestUI 통계 패널

| 항목 | 설명 |
|---|---|
| **총 공격 횟수** | 모든 공격 시도 횟수 |
| **크리티컬 히트** | 크리티컬로 적중한 횟수 |
| **총 피해량** | 누적 데미지 |
| **평균 피해** | 총 피해량 / 공격 횟수 |
| **크리티컬 확률** | 크리티컬 횟수 / 총 공격 횟수 |

---

## 🐛 문제 해결

### 문제: 씬에 아무것도 생성되지 않음

**해결**:
- `Auto Setup On Start` 체크 확인
- Console 창에서 에러 메시지 확인
- Inspector에서 `Setup Demo Scene` Context Menu 실행

### 문제: 데미지가 적용되지 않음

**해결**:
- 타겟에 `HealthSystem` 컴포넌트 있는지 확인
- `DamageSystem` 싱글톤 생성 확인
- 무적 상태 확인

### 문제: UI가 표시되지 않음

**해결**:
- `Show UI` 옵션 체크 확인
- Tab 키로 토글 시도
- Game View에서 확인 (Scene View 아님)

---

## 🎯 다음 단계

이 데모를 완료한 후:

1. ✅ **실제 게임 적용**: 플레이어 캐릭터에 통합
2. ✅ **Hitbox/Hurtbox 테스트**: 물리적 충돌 기반 전투
3. ✅ **어빌리티 시스템 연동**: GAS와 통합
4. ✅ **AI와 통합**: 적 AI가 Combat 시스템 사용

---

## 📝 요약

### 핵심 체크리스트

- [ ] 씬에 `CombatDemoScene` 추가
- [ ] `Auto Setup On Start` 활성화
- [ ] Play 모드 진입
- [ ] 키보드 1~6 테스트
- [ ] 다양한 데미지 타입 테스트
- [ ] 크리티컬 히트 테스트
- [ ] 범위 공격 테스트
- [ ] 통계 확인

### 기대 효과

✅ Combat 시스템 이해도 향상
✅ 데미지 계산 로직 확인
✅ 실전 적용 준비 완료

---

**작성일**: 2025-10-02
**작성자**: GASPT 개발팀
