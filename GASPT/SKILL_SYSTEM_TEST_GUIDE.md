# Skill System 테스트 가이드

## 📋 개요

Skill System의 핵심 로직을 테스트하기 위한 가이드입니다.
Context Menu를 통해 8가지 테스트를 순차적으로 실행합니다.

---

## ✅ 사전 준비

### 1. 씬 설정

**Bootstrap 또는 테스트 씬 사용**
- Bootstrap 씬 사용 시: SingletonPreloader가 자동으로 SkillSystem 초기화
- 새 씬 사용 시: 빈 GameObject에 SingletonPreloader 추가

### 2. Player GameObject 설정

**필수 컴포넌트:**
- Tag: "Player"
- `PlayerStats` 컴포넌트
  - Base HP: 100
  - Base Attack: 10
  - Base Defense: 5
  - **Base Mana: 100** ← 중요!

### 3. Enemy GameObject 설정 (옵션)

**방법 A: 기존 Enemy Prefab 사용**
- Hierarchy에 Enemy Prefab 배치
- EnemyData 설정 완료된 상태

**방법 B: SkillSystemTest가 자동 생성**
- `autoCreateEnemy = true` 설정
- 단, Enemy 컴포넌트는 수동으로 추가 필요

---

## 🎯 테스트 절차

### Step 1: SkillSystemTest 추가

1. Hierarchy에서 빈 GameObject 생성
2. 이름: "SkillSystemTest"
3. `SkillSystemTest.cs` 컴포넌트 추가

### Step 2: 테스트용 SkillData 생성

**3개의 SkillData ScriptableObject 생성:**

#### 1️⃣ FireballSkill (Damage 타입)
```
Create > GASPT > Skills > Skill

설정:
- Skill Name: "Fireball"
- Skill Type: Damage
- Target Type: Enemy
- Mana Cost: 20
- Cooldown: 3초
- Damage Amount: 50
- Skill Range: 10
- Description: "적에게 화염구를 발사합니다"
```

#### 2️⃣ HealSkill (Heal 타입)
```
Create > GASPT > Skills > Skill

설정:
- Skill Name: "Heal"
- Skill Type: Heal
- Target Type: Self
- Mana Cost: 15
- Cooldown: 5초
- Heal Amount: 30
- Description: "자신의 체력을 회복합니다"
```

#### 3️⃣ AttackBuffSkill (Buff 타입)
```
Create > GASPT > Skills > Skill

설정:
- Skill Name: "AttackUp Buff"
- Skill Type: Buff
- Target Type: Self
- Mana Cost: 10
- Cooldown: 10초
- Status Effect: AttackUp (기존 StatusEffectData 사용)
- Description: "공격력을 증가시킵니다"
```

### Step 3: SkillSystemTest Inspector 설정

```
SkillSystemTest 컴포넌트:
- Test Skill 1: FireballSkill
- Test Skill 2: HealSkill
- Test Skill 3: AttackBuffSkill
- Test Enemy: (Hierarchy의 Enemy GameObject 드래그 or null로 자동 생성)
- Player: (자동 탐색됨, 수동 설정도 가능)
- Auto Create Enemy: true
```

### Step 4: 테스트 실행

**Unity Play Mode 진입**

SkillSystemTest GameObject 우클릭 → Context Menu 선택

---

## 🧪 테스트 케이스

### ✅ Test 01: Check Initial State
**목적**: 초기 상태 확인
**실행**: `01. Check Initial State`
**예상 결과**:
```
✅ Player: Player
✅ PlayerStats: HP 100/100, Mana 100/100
✅ SkillSystem: 초기화됨
```

---

### ✅ Test 02: Register Skills
**목적**: 스킬 등록
**실행**: `02. Register Skills`
**예상 결과**:
```
✅ 스킬 등록 성공:
  - 슬롯 0: Fireball
  - 슬롯 1: Heal
  - 슬롯 2: AttackUp Buff
```

---

### ✅ Test 03: Check Mana
**목적**: 마나 소비/회복 확인
**실행**: `03. Check Mana`
**예상 결과**:
```
현재 마나: 100/100
마나 20 소비 테스트...
소비 결과: ✅ 성공
현재 마나: 80/100
마나 30 회복 테스트...
현재 마나: 100/100
```

---

### ✅ Test 04: Test Damage Skill
**목적**: Damage 타입 스킬 (Fireball)
**실행**: `04. Test Damage Skill (Slot 0)`
**예상 결과**:
```
✅ 스킬 사용 성공!
사용 후 마나: 80/100 (20 소비)
사용 후 적 HP: 50/100 (50 데미지)
```
**확인 사항**:
- 마나 20 소비 ✅
- Enemy HP 50 감소 ✅
- DamageNumber 표시 ✅

---

### ✅ Test 05: Test Heal Skill
**목적**: Heal 타입 스킬
**실행**: `05. Test Heal Skill (Slot 1)`
**예상 결과**:
```
✅ 스킬 사용 성공!
사용 후 HP: 100/100 (30 회복)
사용 후 마나: 65/100 (15 소비)
```
**확인 사항**:
- 마나 15 소비 ✅
- Player HP 30 회복 ✅
- DamageNumber (회복) 표시 ✅

---

### ✅ Test 06: Test Buff Skill
**목적**: Buff 타입 스킬 (StatusEffect 적용)
**실행**: `06. Test Buff Skill (Slot 2)`
**예상 결과**:
```
✅ 스킬 사용 성공!
사용 전 Attack: 10
사용 후 Attack: 20 (버프 적용됨)
사용 후 마나: 55/100 (10 소비)
```
**확인 사항**:
- 마나 10 소비 ✅
- Attack +10 버프 ✅
- StatPanel UI에 Attack 변경 반영 ✅

---

### ✅ Test 07: Test Cooldown
**목적**: 쿨다운 작동 확인
**실행**: `07. Test Cooldown`
**예상 결과**:
```
스킬 사용 (첫 번째)...
첫 번째 사용: ✅ 성공
즉시 다시 사용 시도 (쿨다운 중)...
두 번째 사용: ❌ 실패 (쿨다운 중, 정상)
쿨다운 상태: True
남은 시간: 2.9초
쿨다운 진행도: 97%
```
**확인 사항**:
- 첫 번째 사용 성공 ✅
- 쿨다운 중 사용 차단 ✅
- 쿨다운 시간 감소 ✅
- 3초 후 재사용 가능 ✅

---

### ✅ Test 08: Test Out Of Mana
**목적**: 마나 부족 시 실패 확인
**실행**: `08. Test Out Of Mana`
**예상 결과**:
```
현재 마나: 55/100
마나 50 소비 → 현재 마나: 5
스킬 마나 비용: 20
스킬 사용 시도 (마나 부족 예상)...
사용 결과: ❌ 실패 (마나 부족, 정상)
마나 전체 회복...
현재 마나: 100/100
```
**확인 사항**:
- 마나 부족 시 사용 차단 ✅
- 마나 환불 없음 (소비 안 됨) ✅

---

## 🚀 전체 자동 테스트

**한 번에 모든 테스트 실행:**
```
우클릭 > Run All Tests
```

모든 테스트를 자동으로 순차 실행하고 결과를 출력합니다.

---

## 📊 추가 유틸리티

### Print Player Stats
```
우클릭 > Print Player Stats
```
플레이어 상태 출력 (HP, Mana, Attack, Defense)

### Print Skill Slots
```
우클릭 > Print Skill Slots
```
등록된 스킬 슬롯 정보 출력

### Create Test Enemy
```
우클릭 > Create Test Enemy
```
테스트용 Enemy GameObject 생성

---

## ✅ 테스트 체크리스트

실행 후 다음 항목을 확인하세요:

- [ ] **초기화**: SkillSystem 싱글톤 생성됨
- [ ] **스킬 등록**: 3개 스킬이 슬롯 0, 1, 2에 등록됨
- [ ] **마나 소비**: 스킬 사용 시 마나 감소
- [ ] **마나 회복**: RegenerateMana 작동
- [ ] **Damage 스킬**: Enemy HP 감소
- [ ] **Heal 스킬**: Player HP 회복
- [ ] **Buff 스킬**: StatusEffect 적용, Attack 증가
- [ ] **쿨다운**: 스킬 사용 후 재사용 불가
- [ ] **쿨다운 완료**: 3초/5초/10초 후 재사용 가능
- [ ] **마나 부족**: 마나 부족 시 사용 차단
- [ ] **이벤트**: Console에 로그 정상 출력
- [ ] **DamageNumber**: 데미지/회복 숫자 표시

---

## 🐛 예상 버그/이슈

### 문제 1: "Player를 찾을 수 없습니다"
**원인**: Player Tag 미설정
**해결**: Player GameObject에 Tag: "Player" 설정

### 문제 2: "PlayerStats를 찾을 수 없습니다"
**원인**: PlayerStats 컴포넌트 없음
**해결**: Player에 PlayerStats 컴포넌트 추가

### 문제 3: "SkillSystem이 초기화되지 않았습니다"
**원인**: SingletonPreloader 없음
**해결**: Bootstrap 씬 사용 또는 SingletonPreloader 추가

### 문제 4: "Enemy 컴포넌트를 찾을 수 없습니다"
**원인**: testEnemy에 Enemy 컴포넌트 없음
**해결**:
- Enemy Prefab 사용 또는
- testEnemy에 수동으로 Enemy 컴포넌트 + EnemyData 설정

### 문제 5: "StatusEffect 적용 안 됨"
**원인**: StatusEffectManager 초기화 안 됨 또는 StatusEffectData 설정 안 됨
**해결**:
- Bootstrap 씬 사용 (SingletonPreloader)
- AttackBuffSkill의 Status Effect 필드에 AttackUp.asset 설정

---

## 📝 테스트 결과 보고

테스트 완료 후 다음 정보를 확인하세요:

```
✅ 통과한 테스트: 8/8
❌ 실패한 테스트: 0/8
⚠️ 경고/이슈: (있다면 기록)

버그 리스트:
(발견된 버그 기록)

개선 사항:
(개선이 필요한 부분 기록)
```

---

## 🎓 다음 단계

테스트 통과 후:
1. **버그 수정** (발견 시)
2. **커밋**: "테스트: SkillSystem 기본 로직 검증 완료"
3. **UI 구현**: SkillSlotUI (시각적 표시)
4. **통합 테스트**: UI + 로직 함께 테스트

---

**Happy Testing! 🎮**
