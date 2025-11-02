# Resources 폴더 구조 가이드

## 📁 폴더 구조

```
Assets/
  Resources/                    # Unity Resources 폴더 (런타임 로딩용)
    ├── Prefabs/               # 프리팹 리소스
    │   ├── UI/                # UI 프리팹
    │   │   ├── DamageNumber.prefab
    │   │   └── HPBar.prefab
    │   ├── Enemy/             # 적 프리팹
    │   │   ├── Goblin.prefab
    │   │   └── Orc.prefab
    │   └── Effects/           # 이펙트 프리팹
    │       ├── HitEffect.prefab
    │       └── BuffEffect.prefab
    │
    ├── Data/                  # ScriptableObject 데이터
    │   ├── StatusEffects/     # 상태 이상 효과 데이터
    │   │   ├── AttackUp.asset
    │   │   ├── DefenseUp.asset
    │   │   └── Poison.asset
    │   ├── Enemies/           # 적 데이터
    │   │   └── Goblin.asset
    │   └── Items/             # 아이템 데이터
    │       └── HealthPotion.asset
    │
    ├── Audio/                 # 오디오 파일
    │   ├── BGM/               # 배경 음악
    │   │   ├── Title.mp3
    │   │   └── Battle.mp3
    │   └── SFX/               # 효과음
    │       ├── Attack.wav
    │       └── Hit.wav
    │
    └── Sprites/               # 스프라이트
        ├── Icons/             # UI 아이콘
        │   ├── Attack.png
        │   └── Defense.png
        └── StatusEffects/     # 상태 이상 아이콘
            ├── AttackUp.png
            └── Poison.png
```

## 🎯 사용 방법

### 1. GameResourceManager를 통한 로딩

```csharp
using GASPT.ResourceManagement;

// GameObject (Prefab) 로드
GameObject damageNumberPrefab = GameResourceManager.Instance.LoadPrefab("Prefabs/UI/DamageNumber");

// ScriptableObject 로드
StatusEffectData attackUp = GameResourceManager.Instance.LoadScriptableObject<StatusEffectData>("Data/StatusEffects/AttackUp");

// AudioClip 로드
AudioClip attackSound = GameResourceManager.Instance.LoadAudioClip("Audio/SFX/Attack");

// Sprite 로드
Sprite attackIcon = GameResourceManager.Instance.LoadSprite("Sprites/Icons/Attack");
```

### 2. ResourcePaths 상수 사용 (권장)

```csharp
using GASPT.ResourceManagement;

// 상수를 사용해서 타입 안전하게 로딩
GameObject damageNumberPrefab = GameResourceManager.Instance.LoadPrefab(ResourcePaths.Prefabs.UI.DamageNumber);

// 경로 오타 방지 및 IDE 자동완성 지원
StatusEffectData poison = GameResourceManager.Instance.LoadScriptableObject<StatusEffectData>(ResourcePaths.Data.StatusEffects.Poison);
```

### 3. 인스턴스 생성

```csharp
// 프리팹 로드 후 즉시 인스턴스화
GameObject instance = GameResourceManager.Instance.Instantiate("Prefabs/UI/DamageNumber");

// 위치/회전 지정
GameObject instance2 = GameResourceManager.Instance.Instantiate(
    "Prefabs/UI/DamageNumber",
    Vector3.zero,
    Quaternion.identity
);
```

## 📋 네이밍 규칙

### 파일명
- **PascalCase** 사용 (예: `DamageNumber.prefab`)
- 설명적이고 명확한 이름
- 버전 번호나 접미사 지양 (예: ~~`DamageNumber_v2.prefab`~~)

### 경로
- 소문자로 시작하지 않음 (예: `Prefabs/UI/` ✓, ~~`prefabs/ui/`~~ ✗)
- 복수형 사용 (예: `Prefabs`, `Effects`, `Icons`)

## ⚠️ 주의사항

### Resources 폴더 사용 시 주의점

1. **빌드 크기 증가**
   - Resources 폴더의 모든 파일은 빌드에 포함됩니다
   - 사용하지 않는 리소스는 제거하세요

2. **로딩 성능**
   - `Resources.Load()`는 동기 로딩입니다 (프레임 저하 가능)
   - 큰 리소스는 Awake/Start에서 미리 로드하세요
   - GameResourceManager의 캐싱 기능 활용

3. **경로 관리**
   - 하드코딩된 문자열 사용 지양
   - 반드시 `ResourcePaths` 상수 사용

4. **확장자 제외**
   - Resources.Load()는 확장자를 자동으로 처리합니다
   - 경로에 확장자 포함하지 마세요
   - 예: `"Prefabs/UI/DamageNumber"` ✓, ~~`"Prefabs/UI/DamageNumber.prefab"`~~ ✗

## 🚀 확장 가능성

### Addressables로 마이그레이션 (향후)

Resources 폴더는 작은 프로젝트에 적합합니다.
프로젝트가 커지면 **Addressables** 시스템으로 전환을 고려하세요:

- 런타임 다운로드 지원
- 메모리 효율적
- 빌드 크기 최적화

GameResourceManager는 Addressables와 호환되도록 설계되었습니다.

## 📝 ResourcePaths 업데이트 방법

새 리소스 추가 시 `ResourcePaths.cs`에 경로 상수를 추가하세요:

```csharp
// ResourcePaths.cs
public static class Prefabs
{
    public static class UI
    {
        public const string DamageNumber = "Prefabs/UI/DamageNumber";
        public const string NewUI = "Prefabs/UI/NewUI"; // 새로 추가
    }
}
```

## 🔍 디버깅

### 캐시 상태 확인

```csharp
// GameResourceManager의 Context Menu 사용
// Hierarchy에서 GameResourceManager 선택 → 우클릭 → Print Cache Info
```

### 리소스 존재 여부 확인

```csharp
bool exists = GameResourceManager.Instance.Exists("Prefabs/UI/DamageNumber");
if (!exists)
{
    Debug.LogError("리소스를 찾을 수 없습니다!");
}
```

## 📚 참고 자료

- [Unity Manual - Resources](https://docs.unity3d.com/Manual/BestPracticeUnderstandingPerformanceInUnity6.html)
- [Unity Manual - Addressables](https://docs.unity3d.com/Packages/com.unity.addressables@latest)
- 프로젝트 내 `GameResourceManager.cs`
- 프로젝트 내 `ResourcePaths.cs`
