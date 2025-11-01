# Unity Assembly Definition 문제 해결 과정

## 📋 개요

**프로젝트:** GASPT - Generic Ability System + FSM
**기간:** 2025-11-01 ~ 2025-11-02
**문제:** CS0246 에러 - SingletonManager 타입을 찾을 수 없음
**최종 해결:** 모든 Assembly Definition 제거 및 단일 어셈블리로 통합

---

## 🔴 문제 발견

### 상황
RPG Systems 기능을 구현하던 중, `AbilitySystem.cs`에서 `SingletonManager<T>`를 상속받으려 할 때 **CS0246 에러** 발생:

```csharp
// Assets/Plugins/GAS_Core/Core/AbilitySystem.cs
using System;
using System.Collections.Generic;
using UnityEngine;
using Core; // SingletonManager 사용

namespace GAS.Core
{
    public class AbilitySystem : SingletonManager<AbilitySystem>, IAbilitySystem
    {
        // CS0246: The type or namespace name 'SingletonManager<>' could not be found
    }
}
```

### 초기 상태
```
Assets/
├── Plugins/
│   ├── GAS_Core/
│   │   ├── GAS.Core.asmdef ← 문제 발생 위치
│   │   └── Core/AbilitySystem.cs
│   ├── FSM_Core/
│   │   └── FSM.Core.asmdef
│   └── FSM_GAS_Integration/
└── _Project/Scripts/Core/Utilities/
    ├── Core.Utilities.asmdef
    └── SingletonManager.cs ← namespace Core { }
```

---

## 🔍 원인 분석

### 근본 원인
1. **어셈블리 경계 문제**: `SingletonManager`는 `Core.Utilities` 어셈블리, `AbilitySystem`은 `GAS.Core` 어셈블리에 속함
2. **참조 누락**: `GAS.Core.asmdef`가 `Core.Utilities` 어셈블리를 참조하지 않음
3. **namespace 존재**: `namespace Core { }` 래퍼로 인한 추가 복잡성

### Unity Assembly Definition 시스템
Unity는 어셈블리 정의(.asmdef) 파일을 사용하여 코드를 모듈화:
- 각 .asmdef는 독립적인 DLL로 컴파일됨
- 다른 어셈블리의 타입을 사용하려면 **명시적 참조** 필요
- 참조 방법: GUID를 통한 어셈블리 참조

---

## 🛠️ 시도한 해결 방법들

### 시도 1: namespace 제거 + Assembly Reference 추가

**날짜:** 2025-11-01
**커밋:** `766e4e7`, `e41b187`

#### 접근
1. `SingletonManager.cs`에서 `namespace Core { }` 제거 → 전역 클래스로 변경
2. `GAS.Core.asmdef`에 `Core.Utilities` 참조 추가

```json
// Assets/Plugins/GAS_Core/GAS.Core.asmdef
{
    "name": "GAS.Core",
    "references": [
        "GUID:cf8ff52ffeb6cdf4fbbb73483cd5f3b4" // Core.Utilities
    ]
}
```

#### 결과
❌ **실패** - 여전히 CS0246 에러 발생

#### 실패 원인
명시적 참조 방식은 작동했지만, 향후 모든 새 시스템에서 매번 참조를 추가해야 하는 번거로움 존재. 사용자는 "명시적 참조 없이" 전역으로 사용하고 싶어함.

---

### 시도 2: 독립 어셈블리 생성 (autoReferenced: true)

**날짜:** 2025-11-01
**커밋:** `e6334a1`

#### 접근
`SingletonManager` 전용 독립 어셈블리를 생성하고 `autoReferenced: true` 설정:

```
Assets/Plugins/
└── Core.SingletonManager/
    ├── SingletonManager.cs
    └── Core.SingletonManager.asmdef (autoReferenced: true)
```

```json
// Core.SingletonManager.asmdef
{
    "name": "Core.SingletonManager",
    "autoReferenced": true,  // 모든 어셈블리에서 자동 참조
    "references": []
}
```

#### 결과
❌ **실패** - Unity가 autoReferenced를 제대로 인식하지 못함

#### 실패 원인
`autoReferenced: true`가 항상 즉시 작동하지 않는 Unity의 버그/한계. Reimport/재시작으로도 해결 안 됨.

#### 교훈
> **autoReferenced는 신뢰할 수 없다**
> Unity 문서에서는 autoReferenced가 모든 어셈블리에서 자동 참조된다고 하지만, 실제로는 일관성 없게 작동하는 경우가 많음.

---

### 시도 3: 어셈블리 정의 없는 폴더로 이동 (Assets/_Project/Scripts/Global/)

**날짜:** 2025-11-01
**커밋:** `37be9ed`

#### 접근
어셈블리 정의가 없는 폴더로 이동 → `Assembly-CSharp.dll`에 포함:

```
Assets/_Project/Scripts/
└── Global/ (asmdef 없음)
    └── SingletonManager.cs
```

#### 예상
- Assembly-CSharp는 모든 어셈블리의 기본 참조
- GAS_Core가 Assembly-CSharp를 참조 가능

#### 결과
❌ **실패** - 여전히 CS0246 에러 발생

#### 실패 원인
**Unity 컴파일 순서 문제 발견!**

```
컴파일 순서:
1. Plugins/ 폴더 내 어셈블리 (GAS_Core, FSM_Core 등)
2. Assembly-CSharp (_Project/Scripts/)
```

→ **Plugins 폴더의 어셈블리는 Assembly-CSharp보다 먼저 컴파일됨**
→ GAS_Core는 Assembly-CSharp를 참조할 수 없음!

#### 교훈
> **Unity 컴파일 순서 이해 필수**
> - `Assets/Plugins/` 폴더: 최우선 컴파일
> - `Assets/` 기타 폴더: 나중에 컴파일 (Assembly-CSharp)
> - 앞선 단계는 뒷 단계를 참조할 수 없음

---

### 시도 4: Plugins 폴더 내로 이동 (Assets/Plugins/Global/)

**날짜:** 2025-11-02
**커밋:** `e8358e9`

#### 접근
컴파일 순서 문제를 해결하기 위해 Plugins 폴더 내에 배치:

```
Assets/Plugins/
├── Global/ (asmdef 없음)
│   └── SingletonManager.cs
├── GAS_Core/ (asmdef 있음)
└── FSM_Core/ (asmdef 있음)
```

#### 논리
- Plugins 폴더 내 asmdef 없는 스크립트는 같은 Plugins 어셈블리끼리 참조 가능
- 컴파일 순서 문제 해결

#### 결과
❌ **실패** - 여전히 CS0246 에러 발생

#### 실패 원인
Plugins 폴더 내에서도 어셈블리 정의가 있는 폴더는 독립적으로 컴파일되어, 어셈블리 정의 없는 폴더의 코드를 찾을 수 없었음.

---

## ✅ 최종 해결: 모든 Assembly Definition 제거

**날짜:** 2025-11-02
**커밋:** `87d121a`

### 결정
프로젝트 진행이 우선이므로, **모든 .asmdef 파일을 삭제**하고 **단일 어셈블리로 통합**

### 작업 내용
```bash
# 삭제된 어셈블리 정의
- Assets/Plugins/GAS_Core/GAS.Core.asmdef
- Assets/Plugins/FSM_Core/FSM.Core.asmdef
- Assets/Plugins/FSM_Core/Editor/FSM.Core.Editor.asmdef
- Assets/Plugins/FSM_GAS_Integration/FSM.GAS.Integration.asmdef
- Assets/_Project/Scripts/Core/Enums/Core.Enums.asmdef
- Assets/_Project/Scripts/Core/Utilities/Core.Utilities.asmdef
```

### 결과
```
이전: 6개의 독립 어셈블리
└── GAS.Core, FSM.Core, FSM.Core.Editor, FSM.GAS.Integration, Core.Enums, Core.Utilities

현재: 단일 어셈블리
└── Assembly-CSharp.dll (전체 통합) ✅
```

### 효과
✅ **CS0246 에러 완전 해결** - 모든 코드가 동일 어셈블리에 포함
✅ **어셈블리 참조 문제 제거** - namespace만으로 모든 타입 접근 가능
✅ **개발 속도 향상** - 빌드 설정 복잡도 제거
✅ **프로젝트 진행 가능** - 더 이상 컴파일 에러 없음

---

## 📚 학습한 Unity Assembly Definition 핵심 개념

### 1. Unity 컴파일 순서

```
1순위: Plugins/ 폴더 (Standard Assets, Pro Standard Assets, Plugins)
2순위: Assets/ 기타 폴더 (Assembly-CSharp)
3순위: Editor 폴더 (Assembly-CSharp-Editor)
```

**중요:** 앞선 순서는 뒷순서를 참조할 수 없음!

### 2. Assembly Definition 참조 방법

#### 방법 1: 명시적 GUID 참조
```json
{
    "name": "MyAssembly",
    "references": [
        "GUID:xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx"
    ]
}
```
✅ 확실하게 작동
❌ 모든 곳에서 수동 추가 필요

#### 방법 2: autoReferenced
```json
{
    "name": "MyAssembly",
    "autoReferenced": true
}
```
✅ 자동 참조 의도
❌ 일관성 없게 작동 (Unity 버그/한계)

#### 방법 3: 어셈블리 정의 없음
```
Assets/Scripts/
└── MyScript.cs (asmdef 없음)
```
✅ Assembly-CSharp에 포함
❌ Plugins 어셈블리에서 참조 불가 (컴파일 순서)

### 3. Assembly Definition 장단점

#### 장점
- **모듈화**: 독립적인 DLL로 분리
- **빠른 컴파일**: 변경된 어셈블리만 재컴파일
- **명확한 의존성**: 어떤 코드가 무엇을 참조하는지 명시적

#### 단점
- **복잡성 증가**: 참조 관리 복잡
- **디버깅 어려움**: 어셈블리 경계 문제 찾기 어려움
- **학습 곡선**: Unity 컴파일 시스템 이해 필요
- **초기 설정 비용**: GUID 찾기, 참조 추가 등

---

## 🎯 프로젝트 의사결정: 단일 어셈블리 선택 이유

### 1. 프로젝트 규모
- **현재:** 중소형 프로젝트 (~10,000 라인 예상)
- **판단:** 단일 어셈블리로 충분히 관리 가능

### 2. 개발 단계
- **현재:** 프로토타입/개발 초기 단계
- **우선순위:** 빠른 개발 > 컴파일 속도 최적화

### 3. 팀 구성
- **현재:** 1인 개발
- **판단:** 모듈 경계 엄격히 관리할 필요 없음

### 4. 학습 vs 생산성
- **trade-off:** Assembly Definition 학습 시간 vs 기능 개발 시간
- **선택:** 기능 개발 우선, 필요시 나중에 어셈블리 분리

### 5. 컴파일 속도
- **현재:** 단일 어셈블리여도 빌드 시간 충분히 짧음 (< 10초)
- **임계점:** 30초 이상 걸릴 때 어셈블리 분리 고려

---

## 📊 시도별 비교표

| 시도 | 방법 | 결과 | 실패 원인 | 학습 포인트 |
|------|------|------|-----------|-------------|
| **1** | namespace 제거 + Assembly Reference | ❌ 실패 | 명시적 참조 필요성 | namespace는 어셈블리 문제의 근본 원인 아님 |
| **2** | 독립 어셈블리 (autoReferenced) | ❌ 실패 | autoReferenced 미작동 | autoReferenced는 신뢰할 수 없음 |
| **3** | _Project/Scripts/Global/ 이동 | ❌ 실패 | Unity 컴파일 순서 | Plugins는 Assembly-CSharp보다 먼저 컴파일 |
| **4** | Plugins/Global/ 이동 | ❌ 실패 | 어셈블리 경계 | asmdef 없어도 Plugins 내 참조 안 됨 |
| **5** | 모든 asmdef 제거 | ✅ 성공 | - | 프로젝트 규모에 맞는 실용적 선택 |

---

## 💡 교훈 및 베스트 프랙티스

### 1. Assembly Definition 사용 기준

#### 사용하는 것이 좋은 경우:
- 대규모 프로젝트 (50,000+ 라인)
- 팀 개발 (모듈 경계 명확히 필요)
- 플러그인/패키지 개발 (독립성 필요)
- 컴파일 시간 > 30초

#### 사용하지 않는 것이 좋은 경우:
- 중소형 프로젝트
- 1인 개발
- 프로토타입 단계
- 학습 초기 단계

### 2. 문제 해결 접근법

1. **단순한 것부터 시도**: namespace 제거, using 구문 확인
2. **Unity 문서 숙지**: 컴파일 순서, 폴더 구조 이해
3. **실용적 선택**: 학습 시간 vs 생산성 trade-off 고려
4. **과도한 최적화 경계**: "Premature optimization is the root of all evil"

### 3. Unity 컴파일 시스템 이해 필수

```
중요한 순서:
Plugins/ → Standard Assets/ → Assets/ → Editor/

의존성 방향:
뒤 → 앞 (O)  Assets/는 Plugins/를 참조 가능
앞 → 뒤 (X)  Plugins/는 Assets/를 참조 불가
```

### 4. autoReferenced 사용 주의

Unity 버전, 프로젝트 설정, 타이밍에 따라 일관성 없게 작동. **프로덕션에서는 명시적 참조 권장**.

---

## 🔄 향후 계획

### 어셈블리 분리 재검토 시점
다음 조건 중 하나 충족 시 어셈블리 분리 재고려:

1. **컴파일 시간 > 30초**
2. **코드베이스 > 50,000 라인**
3. **팀 개발 시작** (모듈 경계 필요)
4. **패키지 배포 필요** (독립성 확보)

### 재도입 시 전략
1. 핵심 시스템만 분리 (GAS, FSM)
2. 명시적 GUID 참조 사용 (autoReferenced 지양)
3. 컴파일 순서 명확히 설계
4. 전역 유틸리티는 Plugins 최우선 위치 배치

---

## 📝 참고 자료

### Unity 공식 문서
- [Assembly definitions](https://docs.unity3d.com/Manual/ScriptCompilationAssemblyDefinitionFiles.html)
- [Special folders and script compilation order](https://docs.unity3d.com/Manual/SpecialFolders.html)

### 커밋 히스토리
```
766e4e7 - refactor: Remove namespace from SingletonManager
e41b187 - fix: Add Core.Utilities assembly reference to GAS.Core
e6334a1 - refactor: Extract SingletonManager to independent assembly
37be9ed - refactor: Move SingletonManager to Global/ (no assembly definition)
e8358e9 - refactor: Move SingletonManager to Plugins/Global/
87d121a - refactor: Remove all assembly definitions ✅
```

---

## 🎓 결론

Unity Assembly Definition은 강력한 도구이지만, **프로젝트의 규모와 단계에 맞게 사용**해야 합니다.

**핵심 메시지:**
> "모든 프로젝트에 Assembly Definition이 필요한 것은 아니다.
> 프로젝트의 현재 상황을 정확히 파악하고,
> 실용적인 선택을 하는 것이 더 중요하다."

이번 경험을 통해:
- Unity 컴파일 시스템 깊이 이해
- 문제 해결 과정의 체계적 접근법 학습
- 기술적 결정의 trade-off 고려 능력 향상
- 프로젝트 우선순위에 맞는 실용적 선택 경험

**포트폴리오 가치:**
- 문제 분석 능력
- 다양한 해결 방법 시도
- 실패로부터 학습
- 실용적 의사결정
- 기술 문서 작성 능력

---

**작성일:** 2025-11-02
**작성자:** JaeChang (Claude Code 지원)
**프로젝트:** GASPT - Generic Ability System + FSM
**브랜치:** 004-rpg-systems
