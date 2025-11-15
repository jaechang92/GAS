# ResourcePaths 사용 가이드

> **작성일**: 2025-11-15
> **목적**: 프로젝트의 리소스 경로 중앙 집중식 관리 가이드

---

## 📋 목차

1. [개요](#1-개요)
2. [ResourcePaths란?](#2-resourcepaths란)
3. [사용 방법](#3-사용-방법)
4. [새 경로 추가 방법](#4-새-경로-추가-방법)
5. [주의사항](#5-주의사항)
6. [FAQ](#6-faq)

---

## 1. 개요

### 문제점

기존 방식에서는 리소스 경로를 문자열로 하드코딩했습니다:

```csharp
// ❌ 나쁜 예 - 하드코딩된 경로
GameObject prefab = Resources.Load<GameObject>("Prefabs/Player/MageForm");

// 문제점:
// 1. 오타 발생 가능 ("Prefabs/Palyer/MageForm")
// 2. 경로 변경 시 모든 코드 찾아서 수정
// 3. IDE 자동완성 불가
// 4. 컴파일 타임 체크 불가
```

### 해결책

ResourcePaths를 사용하여 중앙에서 경로를 관리합니다:

```csharp
// ✅ 좋은 예 - ResourcePaths 사용
GameObject prefab = Resources.Load<GameObject>(ResourcePaths.Prefabs.Player.MageForm);

// 장점:
// 1. 오타 방지 (컴파일 에러 발생)
// 2. 경로 변경 시 한 곳만 수정
// 3. IDE 자동완성 지원
// 4. 리팩토링 안전
```

---

## 2. ResourcePaths란?

### 위치

```
Assets/_Project/Scripts/Resources/ResourcePaths.cs
```

### 구조

```csharp
namespace GASPT.ResourceManagement
{
    public static class ResourcePaths
    {
        public static class Prefabs
        {
            public static class Player { ... }
            public static class UI { ... }
            public static class Enemies { ... }
            public static class Effects { ... }
            public static class Projectiles { ... }
        }

        public static class Data
        {
            public static class StatusEffects { ... }
            public static class Enemies { ... }
            public static class Items { ... }
        }

        public static class Audio
        {
            public static class BGM { ... }
            public static class SFX { ... }
        }

        public static class Sprites
        {
            public static class Icons { ... }
            public static class StatusEffects { ... }
        }
    }
}
```

### 경로 규칙

- **Resources 폴더 기준** 상대 경로
- **Resources/ 제외** (자동으로 추가됨)
- **확장자 제외** (.prefab, .asset 등)

**예시:**

| 실제 파일 경로 | ResourcePaths 정의 |
|---|---|
| `Assets/Resources/Prefabs/Player/MageForm.prefab` | `"Prefabs/Player/MageForm"` |
| `Assets/Resources/Data/Enemies/Goblin.asset` | `"Data/Enemies/Goblin"` |
| `Assets/Resources/Audio/BGM/Title.mp3` | `"Audio/BGM/Title"` |

---

## 3. 사용 방법

### 3.1 Prefab 로드

#### GameResourceManager 사용 (권장)

```csharp
using GASPT.ResourceManagement;

// Prefab 로드
GameObject mageFormPrefab = GameResourceManager.Instance.LoadPrefab(
    ResourcePaths.Prefabs.Player.MageForm
);

// Instantiate
GameObject player = Instantiate(mageFormPrefab);
```

#### Resources.Load 직접 사용

```csharp
using GASPT.ResourceManagement;

GameObject enemyPrefab = Resources.Load<GameObject>(
    ResourcePaths.Prefabs.Enemies.RangedGoblin
);
```

### 3.2 ScriptableObject 로드

```csharp
using GASPT.ResourceManagement;

// StatusEffectData 로드
StatusEffectData attackUpData = GameResourceManager.Instance.LoadScriptableObject<StatusEffectData>(
    ResourcePaths.Data.StatusEffects.AttackUp
);

// EnemyData 로드
EnemyData rangedGoblinData = GameResourceManager.Instance.LoadScriptableObject<EnemyData>(
    ResourcePaths.Data.Enemies.RangedGoblin
);
```

### 3.3 오브젝트 풀에서 사용

```csharp
using GASPT.ResourceManagement;
using GASPT.Core.Pooling;

// PoolManager의 Spawn 오버로드 사용
FireballProjectile projectile = PoolManager.Instance.Spawn<FireballProjectile>(
    ResourcePaths.Prefabs.Projectiles.FireballProjectile,
    position,
    rotation
);

// 풀이 없으면 자동 생성됨
```

### 3.4 Initializer에서 사용

```csharp
using GASPT.ResourceManagement;

public static void InitializeRangedEnemyPool()
{
    // Resources 폴더에서 프리팹 로드
    GameObject enemyPrefab = GameResourceManager.Instance.LoadPrefab(
        ResourcePaths.Prefabs.Enemies.Ranged
    );

    // 풀 생성
    PoolManager.Instance.CreatePool(
        enemyPrefab.GetComponent<RangedEnemy>(),
        initialSize: 3,
        canGrow: true
    );
}
```

### 3.5 에디터 스크립트에서 사용

```csharp
using UnityEngine;
using UnityEditor;
using GASPT.ResourceManagement;

public class MyEditorScript : EditorWindow
{
    private void CreatePlayer()
    {
        // Resources.Load는 에디터에서도 작동
        GameObject mageFormPrefab = Resources.Load<GameObject>(
            ResourcePaths.Prefabs.Player.MageForm
        );

        if (mageFormPrefab != null)
        {
            GameObject player = PrefabUtility.InstantiatePrefab(mageFormPrefab) as GameObject;
        }
    }
}
```

---

## 4. 새 경로 추가 방법

### 4.1 Prefab 경로 추가

**시나리오**: 새로운 `WarriorForm` 프리팹을 추가하고 싶습니다.

**파일 위치**: `Assets/Resources/Prefabs/Player/WarriorForm.prefab`

**단계:**

1. `ResourcePaths.cs` 파일 열기
2. `Prefabs.Player` 클래스 찾기
3. 새 상수 추가:

```csharp
public static class Player
{
    public const string MageForm = "Prefabs/Player/MageForm";

    /// <summary>
    /// WarriorForm Prefab 경로
    /// Resources/Prefabs/Player/WarriorForm.prefab
    /// </summary>
    public const string WarriorForm = "Prefabs/Player/WarriorForm"; // ✅ 추가
}
```

4. 저장 및 사용:

```csharp
GameObject warriorPrefab = GameResourceManager.Instance.LoadPrefab(
    ResourcePaths.Prefabs.Player.WarriorForm
);
```

### 4.2 Data 경로 추가

**시나리오**: 새로운 `HealthPotion` 아이템 데이터를 추가하고 싶습니다.

**파일 위치**: `Assets/Resources/Data/Items/HealthPotion.asset`

**단계:**

1. `ResourcePaths.cs` 파일 열기
2. `Data.Items` 클래스 찾기
3. 새 상수 추가:

```csharp
public static class Items
{
    /// <summary>
    /// HealthPotion 데이터
    /// Resources/Data/Items/HealthPotion.asset
    /// </summary>
    public const string HealthPotion = "Data/Items/HealthPotion"; // ✅ 추가
}
```

4. 저장 및 사용:

```csharp
ItemData healthPotion = GameResourceManager.Instance.LoadScriptableObject<ItemData>(
    ResourcePaths.Data.Items.HealthPotion
);
```

### 4.3 새 카테고리 추가

**시나리오**: `Skills` 카테고리를 새로 만들고 싶습니다.

**단계:**

1. `ResourcePaths.cs`에서 `Prefabs` 클래스에 새 static class 추가:

```csharp
public static class Prefabs
{
    public static class Player { ... }
    public static class UI { ... }
    public static class Enemies { ... }

    // ✅ 새 카테고리 추가
    public static class Skills
    {
        /// <summary>
        /// Fireball Skill Prefab 경로
        /// Resources/Prefabs/Skills/Fireball.prefab
        /// </summary>
        public const string Fireball = "Prefabs/Skills/Fireball";
    }
}
```

2. 사용:

```csharp
GameObject fireballSkill = GameResourceManager.Instance.LoadPrefab(
    ResourcePaths.Prefabs.Skills.Fireball
);
```

---

## 5. 주의사항

### ⚠️ 5.1 경로 문자열 규칙

**올바른 형식:**

```csharp
// ✅ Resources/ 제외, 확장자 제외
public const string MageForm = "Prefabs/Player/MageForm";
```

**잘못된 형식:**

```csharp
// ❌ Resources/ 포함 (잘못됨!)
public const string MageForm = "Resources/Prefabs/Player/MageForm";

// ❌ 확장자 포함 (잘못됨!)
public const string MageForm = "Prefabs/Player/MageForm.prefab";

// ❌ Assets/ 포함 (에디터 전용 경로, Resources.Load 불가)
public const string MageForm = "Assets/Resources/Prefabs/Player/MageForm";
```

### ⚠️ 5.2 인스턴스 데이터에 경로 저장 금지

**잘못된 예:**

```csharp
// ❌ EnemyData (ScriptableObject)에 projectilePrefabPath 필드 추가
public class EnemyData : ScriptableObject
{
    public string projectilePrefabPath; // ❌ 잘못됨!
}

// 문제: 모든 RangedEnemy가 같은 EnemyProjectile 사용하므로
// 인스턴스 데이터가 아닌 공유 리소스임
```

**올바른 예:**

```csharp
// ✅ ResourcePaths에 정의
public static class Projectiles
{
    public const string EnemyProjectile = "Prefabs/Projectiles/EnemyProjectile";
}

// ✅ 코드에서 직접 참조
EnemyProjectile projectile = PoolManager.Instance.Spawn<EnemyProjectile>(
    ResourcePaths.Prefabs.Projectiles.EnemyProjectile,
    position,
    rotation
);
```

### ⚠️ 5.3 에디터 전용 경로는 별도 관리

**에디터 스크립트 (Assets/ 경로):**

```csharp
// ✅ 에디터 스크립트에 const 정의
public class PrefabCreator : EditorWindow
{
    private const string PlayerPrefabsPath = "Assets/Resources/Prefabs/Player";

    private void CreatePrefab()
    {
        // AssetDatabase.CreateAsset, PrefabUtility 등 에디터 API 사용
        AssetDatabase.CreateFolder(PlayerPrefabsPath, "NewFolder");
    }
}
```

**런타임 스크립트 (Resources/ 경로):**

```csharp
// ✅ ResourcePaths 사용
GameObject prefab = GameResourceManager.Instance.LoadPrefab(
    ResourcePaths.Prefabs.Player.MageForm
);
```

### ⚠️ 5.4 존재하지 않는 경로

ResourcePaths에 경로를 정의했다고 해서 파일이 자동으로 생성되지 않습니다!

**체크리스트:**

1. ✅ ResourcePaths에 경로 추가
2. ✅ 실제 파일을 Resources 폴더에 생성
3. ✅ 파일 경로와 ResourcePaths 상수 일치 확인

---

## 6. FAQ

### Q1. ResourcePaths를 사용해야 하는 이유는?

**A:** 하드코딩된 경로 문자열은 오타, 리팩토링 어려움, IDE 지원 부족 등의 문제가 있습니다. ResourcePaths를 사용하면:

- ✅ 컴파일 타임 체크 (오타 방지)
- ✅ IDE 자동완성
- ✅ 경로 변경 시 한 곳만 수정
- ✅ 코드 가독성 향상

### Q2. 기존 하드코딩된 경로를 어떻게 찾나요?

**A:** 프로젝트 전체 검색:

```bash
# Grep 사용
grep -r "\"Prefabs/" Assets/_Project/Scripts
grep -r "\"Data/" Assets/_Project/Scripts
```

또는 Visual Studio / Rider에서 `"Prefabs/` 검색

### Q3. Resources 폴더가 아닌 Addressables를 사용해도 되나요?

**A:** 네! Addressables로 전환할 경우:

1. ResourcePaths를 AddressablePaths로 이름 변경
2. 경로 대신 Address Key 사용
3. GameResourceManager를 Addressables API로 대체

ResourcePaths 패턴은 동일하게 적용 가능합니다.

### Q4. 모든 리소스를 ResourcePaths에 추가해야 하나요?

**A:** 아니요. **코드에서 로드하는 리소스만** 추가하면 됩니다.

**추가할 것:**
- ✅ 코드에서 Resources.Load로 로드하는 Prefab
- ✅ ScriptableObject 데이터
- ✅ Audio Clip
- ✅ Texture, Sprite

**추가하지 않아도 되는 것:**
- ❌ Inspector에서 SerializeField로 할당하는 Prefab
- ❌ Scene에 직접 배치된 GameObject
- ❌ 에디터 전용 에셋

### Q5. 경로를 변경하려면 어떻게 하나요?

**시나리오**: `Prefabs/Player/MageForm`을 `Prefabs/Forms/Mage`로 변경

**단계:**

1. Unity에서 실제 파일 이동:
   ```
   Assets/Resources/Prefabs/Player/MageForm.prefab
   →
   Assets/Resources/Prefabs/Forms/Mage.prefab
   ```

2. ResourcePaths.cs 수정:
   ```csharp
   // Before
   public const string MageForm = "Prefabs/Player/MageForm";

   // After
   public const string MageForm = "Prefabs/Forms/Mage";
   ```

3. 컴파일 → 모든 코드 자동 업데이트 ✅

### Q6. 예시 경로(예정, 예시)는 언제 사용하나요?

**A:** 아직 구현되지 않았지만 **향후 추가 예정**인 경로를 미리 정의할 때 사용합니다.

```csharp
/// <summary>
/// WarriorForm Prefab 경로 (예정)
/// Resources/Prefabs/Player/WarriorForm.prefab
/// </summary>
public const string WarriorForm = "Prefabs/Player/WarriorForm"; // 아직 파일 없음
```

이렇게 하면:
- ✅ 코드 작성 시 미리 참조 가능
- ✅ 나중에 파일만 추가하면 바로 작동
- ⚠️ 주의: 파일이 없으면 로드 실패 (null 반환)

---

## 📚 관련 문서

- **RESOURCES_GUIDE.md** - Resources 폴더 구조 및 사용법
- **WORK_STATUS.md** - 프로젝트 전체 현황
- **Phase C-1 완료 보고** - ResourcePaths 정리 작업 내역

---

## 📝 변경 이력

### 2025-11-15 - ResourcePaths 정리 작업

**변경 사항:**

1. **ResourcePaths.cs 개선**
   - 파일 상단에 상세한 사용 가이드 주석 추가
   - `Prefabs.Player` 클래스 추가 (MageForm, WarriorForm, RogueForm)
   - `Prefabs.UI` 클래스에 BuffIcon, PickupSlot 추가
   - `Data.Enemies` 클래스에 Phase C-1 적 데이터 경로 추가
     - BasicMeleeGoblin
     - RangedGoblin
     - FlyingBat
     - EliteOrc

2. **GameplaySceneCreator.cs 수정**
   - `using GASPT.ResourceManagement;` 추가
   - 하드코딩된 경로 제거: `$"{PrefabsPath}/Player/MageForm"`
   - ResourcePaths 사용: `ResourcePaths.Prefabs.Player.MageForm`
   - 불필요한 `PrefabsPath` 상수 제거

3. **문서 작성**
   - RESOURCE_PATHS_GUIDE.md (현재 파일) 작성

**결과:**

- ✅ 프로젝트 전체에서 하드코딩된 경로 제거 완료
- ✅ 모든 런타임 리소스 로드가 ResourcePaths 사용
- ✅ 중앙 집중식 경로 관리 체계 확립

---

**작성자**: Claude Code
**버전**: 1.0
**마지막 수정**: 2025-11-15
