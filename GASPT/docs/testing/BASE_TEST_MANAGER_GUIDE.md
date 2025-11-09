# BaseTestManager 사용 가이드

**작성일**: 2025-11-09
**목적**: 테스트 씬 제작 시 OnGUI 템플릿 제공

---

## 개요

`BaseTestManager`는 **모든 테스트 씬에서 재사용 가능한 OnGUI 베이스 클래스**입니다.

### 제공 기능
- ✅ 일시정지 (Pause) 기능
- ✅ FPS 표시
- ✅ UI 토글 (F10)
- ✅ 공통 정보 패널
- ✅ GUI 스타일 자동 설정

---

## 사용 방법

### 1. BaseTestManager 상속

```csharp
using UnityEngine;
using GASPT.Testing;

public class MyTestManager : BaseTestManager
{
    // 커스텀 필드
    [SerializeField] private GameObject testObject;

    // DrawCustomGUI 오버라이드 (필수)
    protected override void DrawCustomGUI(GUIStyle boxStyle, GUIStyle buttonStyle,
                                          GUIStyle labelStyle, GUIStyle titleStyle)
    {
        // 우측 상단 패널
        GUILayout.BeginArea(new Rect(Screen.width - 260, 10, 250, 400), boxStyle);

        GUILayout.Label("=== MY TEST CONTROLS ===", titleStyle);

        if (GUILayout.Button("Test Button 1", buttonStyle))
        {
            // 버튼 동작
            Debug.Log("Button 1 clicked!");
        }

        if (GUILayout.Button("Test Button 2", buttonStyle))
        {
            // 버튼 동작
        }

        GUILayout.Label($"Test Value: {testObject != null}", labelStyle);

        GUILayout.EndArea();
    }
}
```

---

## 예제

### 예제 1: 간단한 테스트 매니저

```csharp
using UnityEngine;
using GASPT.Testing;

public class SimpleTestManager : BaseTestManager
{
    private int clickCount = 0;

    protected override void DrawCustomGUI(GUIStyle boxStyle, GUIStyle buttonStyle,
                                          GUIStyle labelStyle, GUIStyle titleStyle)
    {
        GUILayout.BeginArea(new Rect(Screen.width - 260, 10, 250, 200), boxStyle);

        GUILayout.Label("=== SIMPLE TEST ===", titleStyle);
        GUILayout.Space(10);

        if (GUILayout.Button("Click Me!", buttonStyle))
        {
            clickCount++;
        }

        GUILayout.Label($"Clicks: {clickCount}", labelStyle);

        GUILayout.EndArea();
    }
}
```

---

### 예제 2: 오브젝트 생성 테스트

```csharp
using UnityEngine;
using GASPT.Testing;
using System.Collections.Generic;

public class SpawnTestManager : BaseTestManager
{
    [SerializeField] private GameObject prefab;
    private List<GameObject> spawnedObjects = new List<GameObject>();

    protected override void DrawCustomGUI(GUIStyle boxStyle, GUIStyle buttonStyle,
                                          GUIStyle labelStyle, GUIStyle titleStyle)
    {
        GUILayout.BeginArea(new Rect(Screen.width - 260, 10, 250, 300), boxStyle);

        GUILayout.Label("=== SPAWN TEST ===", titleStyle);
        GUILayout.Space(10);

        // Spawn Controls
        if (GUILayout.Button("Spawn Object", buttonStyle))
        {
            SpawnObject();
        }

        if (GUILayout.Button("Clear All", buttonStyle))
        {
            ClearAll();
        }

        GUILayout.Space(10);

        // Info
        GUILayout.Label($"Spawned: {spawnedObjects.Count}", labelStyle);

        GUILayout.EndArea();
    }

    private void SpawnObject()
    {
        if (prefab != null)
        {
            Vector3 randomPos = new Vector3(
                Random.Range(-5f, 5f),
                Random.Range(-5f, 5f),
                0f
            );
            GameObject obj = Instantiate(prefab, randomPos, Quaternion.identity);
            spawnedObjects.Add(obj);
        }
    }

    private void ClearAll()
    {
        foreach (var obj in spawnedObjects)
        {
            if (obj != null) Destroy(obj);
        }
        spawnedObjects.Clear();
    }
}
```

---

### 예제 3: 복수 패널 (우측 + 좌측)

```csharp
using UnityEngine;
using GASPT.Testing;

public class MultiPanelTestManager : BaseTestManager
{
    protected override void DrawCustomGUI(GUIStyle boxStyle, GUIStyle buttonStyle,
                                          GUIStyle labelStyle, GUIStyle titleStyle)
    {
        // 우측 상단 패널 (컨트롤)
        DrawRightPanel(boxStyle, buttonStyle, labelStyle, titleStyle);

        // 좌측 상단 패널 (정보)
        DrawLeftPanel(boxStyle, labelStyle, titleStyle);
    }

    private void DrawRightPanel(GUIStyle boxStyle, GUIStyle buttonStyle,
                                GUIStyle labelStyle, GUIStyle titleStyle)
    {
        GUILayout.BeginArea(new Rect(Screen.width - 260, 10, 250, 300), boxStyle);

        GUILayout.Label("=== CONTROLS ===", titleStyle);

        if (GUILayout.Button("Action 1", buttonStyle))
        {
            // Action
        }

        if (GUILayout.Button("Action 2", buttonStyle))
        {
            // Action
        }

        GUILayout.EndArea();
    }

    private void DrawLeftPanel(GUIStyle boxStyle, GUIStyle labelStyle, GUIStyle titleStyle)
    {
        GUILayout.BeginArea(new Rect(10, 10, 200, 200), boxStyle);

        GUILayout.Label("[ Info Panel ]", titleStyle);
        GUILayout.Label("Data 1: 100", labelStyle);
        GUILayout.Label("Data 2: 200", labelStyle);

        GUILayout.EndArea();
    }
}
```

---

## 제공되는 프로퍼티/메서드

### Protected 멤버

| 이름 | 타입 | 설명 |
|------|------|------|
| `isPaused` | bool | 일시정지 상태 |
| `showUI` | bool | UI 표시 여부 |
| `TogglePause()` | void | 일시정지 토글 |
| `MakeTex(w, h, color)` | Texture2D | 배경 텍스처 생성 |

### 필수 오버라이드

| 메서드 | 설명 |
|--------|------|
| `DrawCustomGUI(...)` | 커스텀 GUI 그리기 (필수 구현) |

---

## GUI 레이아웃 가이드

### 권장 위치

- **우측 상단**: 메인 컨트롤 패널
  ```csharp
  new Rect(Screen.width - 260, 10, 250, 높이)
  ```

- **좌측 상단**: 정보 패널
  ```csharp
  new Rect(10, 10, 너비, 높이)
  ```

- **좌측 하단**: 공통 시스템 정보 (BaseTestManager 제공)
  ```csharp
  new Rect(10, Screen.height - 120, 200, 110)
  ```

- **중앙**: 큰 UI나 모달
  ```csharp
  new Rect(Screen.width / 2 - 너비 / 2, Screen.height / 2 - 높이 / 2, 너비, 높이)
  ```

---

## 자동 제공 기능

### 좌측 하단 패널
BaseTestManager가 자동으로 표시:
- FPS (색상 변화: 초록/노랑/빨강)
- 일시정지 상태
- Pause 버튼
- F10 안내

### 키보드 단축키
- **F10**: UI 토글 (숨김/표시)

---

## 베스트 프랙티스

### 1. 섹션별로 나누기

```csharp
protected override void DrawCustomGUI(...)
{
    DrawPlayerControls(boxStyle, buttonStyle, labelStyle, titleStyle);
    DrawEnemyControls(boxStyle, buttonStyle, labelStyle, titleStyle);
    DrawDebugInfo(boxStyle, labelStyle, titleStyle);
}
```

### 2. 상수로 위치/크기 정의

```csharp
private const float PANEL_WIDTH = 250f;
private const float PANEL_X = Screen.width - PANEL_WIDTH - 10f;

protected override void DrawCustomGUI(...)
{
    GUILayout.BeginArea(new Rect(PANEL_X, 10, PANEL_WIDTH, 500), boxStyle);
    // ...
    GUILayout.EndArea();
}
```

### 3. GUILayout.Space()로 가독성 향상

```csharp
GUILayout.Label("=== Section 1 ===", titleStyle);
GUILayout.Space(10); // 간격
GUILayout.Button("Button 1", buttonStyle);
GUILayout.Button("Button 2", buttonStyle);
GUILayout.Space(10); // 간격
GUILayout.Label("=== Section 2 ===", titleStyle);
```

---

## CombatTestManager 참고

`CombatTestManager.cs`는 BaseTestManager를 **사용하지 않지만**, OnGUI 구현의 좋은 예제입니다.

향후 새로운 테스트 매니저는 **BaseTestManager를 상속**하여 작성하는 것을 권장합니다.

---

## 다음 단계

1. 새 테스트 씬 생성
2. BaseTestManager 상속 클래스 작성
3. DrawCustomGUI() 구현
4. Unity에서 테스트

---

**작성**: 2025-11-09
**버전**: 1.0
**관련 파일**: `BaseTestManager.cs`, `CombatTestManager.cs`
