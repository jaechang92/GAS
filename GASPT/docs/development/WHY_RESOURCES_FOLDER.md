# Resources 폴더를 사용하는 이유

**작성 날짜**: 2025-11-16
**질문**: 왜 `Assets/Resources/Prefabs`를 사용하는가?

---

## 🎯 핵심 답변

**프로젝트가 런타임 동적 로딩 시스템을 사용하기 때문입니다.**

---

## 📊 프로젝트 구조 분석

### 1. GameResourceManager - Resources.Load() 사용

**파일**: `GameResourceManager.cs`

```csharp
public T LoadPrefab<T>(string path) where T : Object
{
    // Resources.Load 실행
    T resource = Resources.Load<T>(path);  // ← Resources 폴더 필수!

    if (resource != null)
    {
        resourceCache[path] = resource;
        return resource;
    }

    return null;
}
```

**작동 방식**:
- `Resources.Load("Prefabs/Enemies/Boss")` ← "Resources" 폴더 내부 경로만 인식
- ✅ `Assets/Resources/Prefabs/Enemies/Boss.prefab` → 로드 성공
- ❌ `Assets/_Project/Prefabs/Enemies/Boss.prefab` → 로드 실패 (null)

---

### 2. PoolManager - GameResourceManager 사용

**파일**: `PoolManager.cs`

```csharp
public T Spawn<T>(string prefabPath, Vector3 position, Quaternion rotation) where T : Component
{
    // GameResourceManager를 통해 프리팹 로드
    T prefab = GameResourceManager.Instance.LoadPrefab<T>(prefabPath);

    if (prefab != null)
    {
        CreatePool<T>(prefab, initialSize: 10);
        return Spawn<T>(position, rotation);
    }

    return null;
}
```

**호출 예시**:
```csharp
// EnemyProjectile을 런타임에 동적 로드
PoolManager.Instance.Spawn<EnemyProjectile>(
    "Prefabs/Projectiles/EnemyProjectile",  // Resources 폴더 기준 경로
    position,
    rotation
);
```

---

### 3. BossEnemy - PoolManager 사용

**파일**: `BossEnemy.cs`

```csharp
private void ExecuteRangedAttack()
{
    // 풀에서 투사체 가져오기
    var projectile = PoolManager.Instance.Spawn<EnemyProjectile>(
        transform.position,
        Quaternion.identity
    );

    projectile.Initialize(direction, speed, damage);
}
```

**시스템 흐름**:
```
BossEnemy.ExecuteRangedAttack()
    ↓
PoolManager.Spawn<EnemyProjectile>()
    ↓
GameResourceManager.LoadPrefab("Prefabs/Projectiles/EnemyProjectile")
    ↓
Resources.Load<EnemyProjectile>("Prefabs/Projectiles/EnemyProjectile")
    ↓
Assets/Resources/Prefabs/Projectiles/EnemyProjectile.prefab 로드
```

---

## 🔍 Resources vs 일반 Assets 비교

### Resources 폴더 방식 (현재 프로젝트)

**장점**:
- ✅ 런타임 동적 로딩 가능
- ✅ 경로 문자열로 프리팹 로드
- ✅ 메모리 관리 유연 (필요할 때만 로드)
- ✅ 모드 시스템에 적합 (Form 전환 등)

**단점**:
- ❌ 빌드 크기 증가 (Resources 폴더 전체 포함)
- ❌ 타입 안정성 낮음 (문자열 경로 사용)
- ❌ 리팩토링 어려움 (경로 문자열)

**프로젝트 경로**:
```
Assets/Resources/
├── Prefabs/
│   ├── Enemies/
│   │   ├── BasicMeleeEnemy.prefab
│   │   ├── BossEnemy_FireDragon.prefab
│   │   └── ...
│   ├── Projectiles/
│   │   ├── EnemyProjectile.prefab
│   │   └── ...
│   └── UI/
│       └── BossHealthBar.prefab
```

---

### 일반 Assets 폴더 방식 (최신 Unity 권장)

**장점**:
- ✅ 타입 안정성 높음 (직접 참조)
- ✅ 리팩토링 쉬움 (Unity가 자동 업데이트)
- ✅ 빌드 최적화 (사용하는 것만 포함)
- ✅ Addressables 사용 가능

**단점**:
- ❌ 런타임 동적 로딩 어려움
- ❌ 직접 참조 필요 (Inspector 할당)
- ❌ 모든 프리팹이 메모리에 상주

**사용 예시**:
```csharp
[SerializeField] private GameObject enemyPrefab;  // Inspector에서 할당

void Spawn()
{
    Instantiate(enemyPrefab, position, rotation);
}
```

---

## 🎯 프로젝트 선택: Resources 폴더

### 왜 이 프로젝트는 Resources를 사용하는가?

**1. Form 시스템 (MageForm, WarriorForm 등)**
- 플레이어가 런타임에 Form 전환
- Form별로 다른 스킬/투사체 사용
- 동적 로딩 필요

**2. 적 타입 다양성**
- 여러 종류의 적 (Melee, Ranged, Flying, Elite, Boss)
- 방마다 다른 적 스폰
- 런타임에 EnemyData 기반으로 동적 생성

**3. 오브젝트 풀링**
- 투사체, 이펙트, 적을 풀링
- 필요할 때만 풀 생성
- 메모리 효율성

**4. 모듈화**
- GameResourceManager로 중앙 관리
- 경로 문자열로 유연한 로딩
- 확장성 높음

---

## 📝 일관성 유지

### 기존 PrefabCreator와 동일한 경로 사용

**PrefabCreator.cs**:
```csharp
private const string PrefabsPath = "Assets/Resources/Prefabs";
private const string PlayerPrefabsPath = "Assets/Resources/Prefabs/Player";
private const string ProjectilesPrefabsPath = "Assets/Resources/Prefabs/Projectiles";
private const string EnemiesPrefabsPath = "Assets/Resources/Prefabs/Enemies";
private const string UIPrefabsPath = "Assets/Resources/Prefabs/UI";
```

**BossSetupCreator.cs** (수정 후):
```csharp
private const string PrefabsPath = "Assets/Resources/Prefabs";
private const string EnemyPrefabsPath = "Assets/Resources/Prefabs/Enemies";
private const string ProjectilePrefabsPath = "Assets/Resources/Prefabs/Projectiles";
private const string UIPrefabsPath = "Assets/Resources/Prefabs/UI";
```

**일관성 유지**:
- ✅ 모든 Editor Tool이 동일한 경로 사용
- ✅ 팀 전체가 동일한 규칙 따름
- ✅ 런타임 로딩 시스템과 호환

---

## ⚠️ 주의사항

### ScriptableObject는 일반 Assets 폴더

**EnemyData, StatusEffectData 등**:
```
Assets/_Project/Data/
├── Enemies/
│   ├── FireDragon.asset       ← ScriptableObject
│   ├── BasicGoblin.asset
│   └── ...
```

**이유**:
- ScriptableObject는 데이터 파일
- 코드에서 직접 참조로 사용 (`enemyData` 필드)
- Resources.Load() 불필요
- Inspector에서 할당

---

### Prefab은 Resources 폴더

**GameObject Prefab들**:
```
Assets/Resources/Prefabs/
├── Enemies/
│   ├── BossEnemy_FireDragon.prefab   ← GameObject Prefab
│   ├── BasicMeleeEnemy.prefab
│   └── ...
```

**이유**:
- 런타임 동적 로딩 필요
- `Resources.Load()` 또는 `GameResourceManager.LoadPrefab()` 사용
- PoolManager가 경로 문자열로 로드

---

## 🚀 결론

### Resources 폴더를 사용하는 이유

1. **프로젝트 아키텍처**:
   - GameResourceManager가 Resources.Load() 사용
   - PoolManager가 런타임 동적 로딩
   - Form 시스템의 유연한 전환

2. **일관성**:
   - 기존 PrefabCreator와 동일한 경로
   - 팀 전체 규칙 통일

3. **기능 요구사항**:
   - 다양한 적 타입 동적 스폰
   - Form별 다른 스킬/투사체
   - 오브젝트 풀링 시스템

### 경로 규칙 정리

| 에셋 타입 | 경로 | 이유 |
|-----------|------|------|
| **ScriptableObject** | `Assets/_Project/Data/` | 직접 참조, Inspector 할당 |
| **GameObject Prefab** | `Assets/Resources/Prefabs/` | 런타임 동적 로딩 |

---

**작성자**: Claude Code
**최종 수정**: 2025-11-16
**참고**: Unity 공식 문서에서는 Resources 폴더 사용을 최소화하고 Addressables 사용을 권장하지만, 이 프로젝트의 아키텍처는 Resources 기반으로 설계되어 있습니다.
