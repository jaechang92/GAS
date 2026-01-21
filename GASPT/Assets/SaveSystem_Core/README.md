# SaveSystem_Core

Unity용 범용 JSON 저장 시스템

## 특징

- **JSON 기반**: JsonUtility 사용
- **제네릭 설계**: 모든 직렬화 가능 타입 저장
- **이벤트 지원**: 저장/로드 성공/실패 이벤트
- **프로젝트 독립**: 데이터 구조는 프로젝트에서 정의

## 구조

```
SaveSystem_Core/
├── SaveSystem.cs       # 저장 시스템 메인
└── README.md
```

## 사용 방법

### 1. 저장 데이터 정의

```csharp
[Serializable]
public class GameSaveData : SaveDataBase
{
    public int level;
    public int score;
    public string playerName;
    public List<string> unlockedItems;
}
```

### 2. 저장

```csharp
var saveData = new GameSaveData
{
    level = 5,
    score = 1000,
    playerName = "Player1"
};
saveData.UpdateSaveTime();

SaveSystem.Instance.Save(saveData, "gamesave.json");
```

### 3. 불러오기

```csharp
// 기본 로드
var data = SaveSystem.Instance.Load<GameSaveData>("gamesave.json");

// 없으면 새로 생성
var data = SaveSystem.Instance.LoadOrCreate<GameSaveData>("gamesave.json");
```

### 4. 파일 관리

```csharp
// 존재 확인
bool exists = SaveSystem.Instance.FileExists("gamesave.json");

// 삭제
SaveSystem.Instance.DeleteFile("gamesave.json");

// 정보 확인
string info = SaveSystem.Instance.GetFileInfo("gamesave.json");

// 모든 저장 파일
string[] files = SaveSystem.Instance.GetAllSaveFiles();
```

### 5. 이벤트 구독

```csharp
SaveSystem.Instance.OnSaved += (path) => Debug.Log($"저장됨: {path}");
SaveSystem.Instance.OnLoaded += (path) => Debug.Log($"로드됨: {path}");
SaveSystem.Instance.OnSaveFailed += (error) => Debug.LogError($"저장 실패: {error}");
SaveSystem.Instance.OnLoadFailed += (error) => Debug.LogError($"로드 실패: {error}");
```

### 6. JSON 문자열 직접 처리

```csharp
// 직렬화
string json = SaveSystem.Instance.Serialize(saveData);

// 역직렬화
var data = SaveSystem.Instance.Deserialize<GameSaveData>(json);
```

## 저장 경로

- **Windows**: `%USERPROFILE%\AppData\LocalLow\<Company>\<Product>\`
- **macOS**: `~/Library/Application Support/<Company>/<Product>/`
- **Linux**: `~/.config/unity3d/<Company>/<Product>/`

## 의존성

없음 (독립 패키지)

## 다른 프로젝트에서 사용

1. `SaveSystem_Core` 폴더 복사
2. 프로젝트에 맞는 저장 데이터 클래스 정의

## 주의사항

- `JsonUtility`는 Dictionary를 지원하지 않음 → List로 대체
- 중첩 클래스는 `[Serializable]` 필수
- 프로퍼티는 저장되지 않음 → 필드 사용

## 라이선스

MIT License
