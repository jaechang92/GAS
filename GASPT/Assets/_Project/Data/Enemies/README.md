# Enemy Data 생성 가이드

Unity Editor에서 Enemy ScriptableObject 에셋을 생성하는 방법입니다.

---

## 생성 방법

1. Project 창에서 이 폴더 (`Assets/_Project/Data/Enemies/`) 선택
2. 우클릭 → `Create` → `GASPT` → `Enemies` → `Enemy`
3. 파일 이름 입력 (예: `NormalGoblin`)
4. Inspector에서 데이터 설정

---

## 필수 Enemy 3종

### 1. NormalGoblin (일반 몹)

**파일명**: `NormalGoblin.asset`

**설정값**:
```
타입:
- Enemy Type: Normal

기본 정보:
- Enemy Name: "Normal Goblin"
- Icon: (선택 사항)

스탯:
- Max Hp: 30
- Attack: 5

보상:
- Min Gold Drop: 15
- Max Gold Drop: 25

UI:
- Show Name Tag: false (체크 해제)
- Show Boss Health Bar: false (체크 해제)
```

---

### 2. EliteOrc (네임드 몹)

**파일명**: `EliteOrc.asset`

**설정값**:
```
타입:
- Enemy Type: Named

기본 정보:
- Enemy Name: "Elite Orc"
- Icon: (선택 사항)

스탯:
- Max Hp: 60
- Attack: 10

보상:
- Min Gold Drop: 40
- Max Gold Drop: 60

UI:
- Show Name Tag: true (체크)
- Show Boss Health Bar: false (체크 해제)
```

---

### 3. FireDragon (보스 몹)

**파일명**: `FireDragon.asset`

**설정값**:
```
타입:
- Enemy Type: Boss

기본 정보:
- Enemy Name: "Fire Dragon"
- Icon: (선택 사항)

스탯:
- Max Hp: 150
- Attack: 15

보상:
- Min Gold Drop: 100
- Max Gold Drop: 150

UI:
- Show Name Tag: false (체크 해제)
- Show Boss Health Bar: true (체크)
```

---

## 데이터 검증

생성 후 Inspector에서 다음을 확인하세요:

1. ✅ Min Gold Drop ≤ Max Gold Drop
2. ✅ Named 타입은 Show Name Tag가 체크되어 있음
3. ✅ Boss 타입은 Show Boss Health Bar가 체크되어 있음

OnValidate() 메서드가 자동으로 경고를 표시합니다.

---

## 테스트

Console에서 경고 메시지를 확인하세요:

```
[EnemyData] Normal Goblin: Normal 타입은 일반적으로 HP가 30-50 정도입니다.
[EnemyData] Elite Orc: Named 타입은 showNameTag를 true로 설정하는 것이 권장됩니다.
```

이러한 경고는 데이터 설정이 일반적인 패턴과 다를 때 표시됩니다.

---

## 사용 예시

```csharp
// Enemy 스크립트에서 사용
public class Enemy : MonoBehaviour
{
    [SerializeField] private EnemyData enemyData;

    private void Start()
    {
        int hp = enemyData.maxHp;
        int goldDrop = enemyData.GetRandomGoldDrop();
        Debug.Log($"{enemyData.enemyName}: HP={hp}, Gold={goldDrop}");
    }
}
```

---

**다음 단계**: Enemy MonoBehaviour 구현 후 이 데이터를 사용합니다.
