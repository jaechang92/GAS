# SkillSystem 원클릭 테스트 가이드 🚀

## 📋 개요

**단 2번의 클릭으로 SkillSystem 테스트 환경 완성!**
- 테스트 씬 자동 생성
- 모든 오브젝트 자동 생성 (Player, Enemy, SkillSystemTest, SingletonPreloader)
- 더미 데이터 자동 생성 (SkillData 3개, EnemyData, StatusEffectData)
- 모든 참조 자동 연결

---

## 🎯 사용 방법 (2 Step)

### Step 1: Unity 에디터에서 메뉴 실행

```
Unity 상단 메뉴:
Tools > GASPT > 🚀 One-Click Setup (Create + Setup)
```

**실행 결과:**
- `Assets/_Project/Scenes/SkillSystemTest.unity` 씬 생성 ✅
- `Assets/_Project/Data/Skills/` 폴더에 SkillData 3개 생성 ✅
- `Assets/_Project/Data/Enemies/` 폴더에 EnemyData 생성 ✅
- `Assets/_Project/Data/StatusEffects/` 폴더에 StatusEffectData 생성 ✅
- Hierarchy에 Player, TestEnemy, SkillSystemTest, SingletonPreloader 생성 ✅
- 모든 참조 자동 연결 ✅

**Console 출력:**
```
✓ 테스트 씬 생성 완료: Assets/_Project/Scenes/SkillSystemTest.unity
✓ FireballSkill 생성
✓ HealSkill 생성
✓ AttackUp StatusEffect 생성
✓ BuffSkill 생성
✓ EnemyData 생성
✓ SingletonPreloader 생성
✓ Player 생성
✓ TestEnemy 생성
✓ SkillSystemTest 생성
✅ 준비 완료! Play 버튼을 누르고 SkillSystemTest 우클릭 > Run All Tests!
```

### Step 2: Play 모드에서 테스트 실행

1. **Play 버튼** 클릭 ▶️
2. **Hierarchy에서 SkillSystemTest 우클릭**
3. **`Run All Tests` 선택**

**테스트 자동 실행:**
```
========== SkillSystem 전체 테스트 시작 ==========
✅ Test 01: 초기 상태 확인
✅ Test 02: 스킬 등록
✅ Test 03: 마나 소비/회복
✅ Test 04: Damage 스킬
✅ Test 05: Heal 스킬
✅ Test 06: Buff 스킬
✅ Test 07: 쿨다운
✅ Test 08: 마나 부족
========== SkillSystem 전체 테스트 완료 ==========
```

---

## 🎨 생성되는 오브젝트

### Hierarchy (씬 오브젝트)

```
SkillSystemTest (씬)
├── SingletonPreloader (8개 싱글톤 초기화)
├── Player (Tag: Player)
│   └── PlayerStats 컴포넌트
│       - Base HP: 100
│       - Base Attack: 10
│       - Base Defense: 5
│       - Base Mana: 100
├── TestEnemy
│   └── Enemy 컴포넌트
│       - EnemyData: TEST_Enemy
│       - HP: 100
│       - Attack: 15
└── SkillSystemTest
    - Test Skill 1: TEST_FireballSkill
    - Test Skill 2: TEST_HealSkill
    - Test Skill 3: TEST_AttackBuffSkill
    - Test Enemy: TestEnemy (자동 연결)
```

### Project (ScriptableObject 에셋)

```
Assets/_Project/Data/
├── Skills/
│   ├── TEST_FireballSkill.asset
│   │   - Damage: 50, Mana: 20, Cooldown: 3s
│   ├── TEST_HealSkill.asset
│   │   - Heal: 30, Mana: 15, Cooldown: 5s
│   └── TEST_AttackBuffSkill.asset
│       - Buff: AttackUp, Mana: 10, Cooldown: 10s
├── Enemies/
│   └── TEST_Enemy.asset
│       - HP: 100, Attack: 15, Defense: 5
└── StatusEffects/
    └── TEST_AttackUp.asset
        - Value: +10 Attack, Duration: 10s
```

---

## 🛠️ 추가 메뉴

Unity 에디터 상단 메뉴 `Tools > GASPT`:

### 1. 🚀 One-Click Setup (Create + Setup) ⭐ 추천
**원클릭으로 모든 것 생성**
- 씬 생성 + 오브젝트 생성 + 데이터 생성 + 참조 연결

### 2. Create Skill System Test Scene
**테스트 씬만 생성** (오브젝트는 생성 안 함)
- `Assets/_Project/Scenes/SkillSystemTest.unity` 생성
- 빈 씬 상태

### 3. Setup Skill System Test Scene
**현재 씬에 테스트 환경 구축** (씬 생성 안 함)
- Player, Enemy, SkillSystemTest, SingletonPreloader 생성
- 더미 데이터 생성
- 기존 씬에 추가하고 싶을 때 사용

### 4. Clear Test Scene
**테스트 씬의 모든 오브젝트 삭제**
- Hierarchy의 모든 GameObject 제거
- ScriptableObject 에셋은 유지

### 5. Delete Test Assets
**테스트용 ScriptableObject 에셋 삭제**
- `TEST_` 접두사 파일 모두 삭제
- Hierarchy 오브젝트는 유지

---

## 📊 테스트 체크리스트

Play 모드에서 `Run All Tests` 실행 후 확인:

- [ ] **초기화**: ✅ SkillSystem 싱글톤 생성됨
- [ ] **스킬 등록**: ✅ 3개 스킬이 슬롯 0, 1, 2에 등록됨
- [ ] **마나 소비**: ✅ 스킬 사용 시 마나 감소
- [ ] **Damage 스킬**: ✅ Enemy HP 50 감소
- [ ] **Heal 스킬**: ✅ Player HP 30 회복
- [ ] **Buff 스킬**: ✅ Attack +10 증가
- [ ] **쿨다운**: ✅ 사용 후 재사용 불가, 시간 경과 후 재사용 가능
- [ ] **마나 부족**: ✅ 마나 부족 시 사용 차단
- [ ] **DamageNumber**: ✅ 데미지/회복 숫자 표시 (DamageNumberPool 있으면)

---

## 🐛 문제 해결

### 문제 1: "Player 태그를 가진 GameObject를 찾을 수 없습니다"
**원인**: Player GameObject의 Tag가 설정 안 됨
**해결**:
```
Hierarchy > Player 선택
Inspector > Tag > Player 선택
```

### 문제 2: "SkillSystem이 초기화되지 않았습니다"
**원인**: SingletonPreloader가 없거나 작동 안 함
**해결**:
```
1. Hierarchy에 SingletonPreloader 있는지 확인
2. 없으면 메뉴 다시 실행
3. Play 모드 진입 시 Console에서 초기화 로그 확인
```

### 문제 3: "Enemy 컴포넌트를 찾을 수 없습니다"
**원인**: TestEnemy에 Enemy 컴포넌트 없음
**해결**:
```
Hierarchy > TestEnemy 선택
Inspector에서 Enemy 컴포넌트 확인
없으면 메뉴 다시 실행
```

### 문제 4: 메뉴가 보이지 않음
**원인**: Unity 에디터 컴파일 오류
**해결**:
```
1. Console 확인 (에러 메시지 확인)
2. Assets > Reimport All
3. Unity 재시작
```

### 문제 5: 기존 테스트 환경과 충돌
**원인**: 이전 테스트 오브젝트가 남아있음
**해결**:
```
Tools > GASPT > Clear Test Scene (오브젝트 삭제)
Tools > GASPT > Delete Test Assets (에셋 삭제)
다시 One-Click Setup 실행
```

---

## 🔄 반복 테스트 워크플로우

### 시나리오 1: 코드 수정 후 재테스트
```
1. 코드 수정 (Skill.cs, SkillSystem.cs 등)
2. Play 모드 진입
3. SkillSystemTest > Run All Tests
4. 결과 확인
```
**에셋/오브젝트 재생성 불필요** ✅

### 시나리오 2: 데이터 수정 후 재테스트
```
1. ScriptableObject 수정 (TEST_FireballSkill 등)
2. Play 모드 진입
3. SkillSystemTest > Run All Tests
4. 결과 확인
```
**에셋/오브젝트 재생성 불필요** ✅

### 시나리오 3: 처음부터 다시 시작
```
1. Tools > GASPT > Clear Test Scene
2. Tools > GASPT > Delete Test Assets
3. Tools > GASPT > 🚀 One-Click Setup
4. Play 모드 진입
5. SkillSystemTest > Run All Tests
```

---

## 📝 테스트 결과 예시

### ✅ 성공 예시

```
========== SkillSystem 전체 테스트 시작 ==========
========== Test 01: 초기 상태 확인 ==========
✅ Player: Player
✅ PlayerStats: HP 100/100, Mana 100/100
✅ SkillSystem: 초기화됨
=======================================

========== Test 02: 스킬 등록 ==========
✅ 스킬 등록 성공:
  - 슬롯 0: TEST Fireball
  - 슬롯 1: TEST Heal
  - 슬롯 2: TEST Attack Buff
=======================================

========== Test 04: Damage 스킬 테스트 ==========
사용 전 마나: 100/100
사용 전 적 HP: 100/100
✅ 스킬 사용 성공!
사용 후 마나: 80/100
사용 후 적 HP: 50/100
=======================================

... (생략)

========== SkillSystem 전체 테스트 완료 ==========
```

### ❌ 실패 예시 (문제 있을 때)

```
========== Test 04: Damage 스킬 테스트 ==========
❌ testEnemy가 null입니다. Enemy를 생성하세요.
=======================================
```
→ 해결: `Tools > GASPT > Setup Skill System Test Scene` 다시 실행

---

## 🎓 다음 단계

테스트 통과 후:

### 1. **버그 수정** (실패 시)
- Console 로그 확인
- 해당 코드 수정
- 재테스트

### 2. **UI 구현** (통과 시) ⭐
```
SkillSlotUI 구현
- 스킬 아이콘 표시
- 쿨다운 오버레이
- 단축키 표시 (1, 2, 3, 4)
- 마나 부족 시 회색 처리
```

### 3. **통합 테스트**
```
UI + 로직 함께 테스트
실제 게임 플레이 시나리오 테스트
```

### 4. **PR 생성**
```
git commit
git push
PR 생성
```

---

## 🎮 실전 사용 예시

### 개별 테스트 실행

Play 모드에서 `SkillSystemTest` 우클릭:

```
01. Check Initial State           → 초기 상태 확인
02. Register Skills               → 스킬 등록
03. Check Mana                    → 마나 시스템 테스트
04. Test Damage Skill (Slot 0)    → Fireball 테스트
05. Test Heal Skill (Slot 1)      → Heal 테스트
06. Test Buff Skill (Slot 2)      → Buff 테스트
07. Test Cooldown                 → 쿨다운 테스트
08. Test Out Of Mana              → 마나 부족 테스트

Print Player Stats                → Player 상태 출력
Print Skill Slots                 → 스킬 슬롯 상태 출력
```

---

## ✅ 최종 요약

| 단계 | 작업 | 소요 시간 |
|------|------|-----------|
| 1 | `Tools > GASPT > 🚀 One-Click Setup` | **10초** |
| 2 | `Play` → `Run All Tests` | **30초** |
| **총합** | | **40초** |

**40초 만에 SkillSystem 테스트 완료!** 🎉

---

**Happy Testing! 🚀**
