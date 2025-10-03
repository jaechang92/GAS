# 🗂️ GASPT 프로젝트 폴더 구조 가이드

## 📋 개요

GASPT 프로젝트는 확장성과 유지보수성을 고려한 체계적인 폴더 구조를 사용합니다. 이 가이드는 각 폴더의 목적과 사용법을 설명합니다.

## 🏗️ 전체 폴더 구조

```
Assets/
├── _Project/                    # 🎯 프로젝트 전용 폴더 (메인 작업 영역)
│   ├── Art/                    # 🎨 아트 에셋 통합 관리
│   │   ├── Sprites/           # 2D 스프라이트 이미지
│   │   │   ├── Characters/    # 캐릭터 스프라이트
│   │   │   ├── Environment/   # 환경 스프라이트
│   │   │   └── UI/           # UI 관련 스프라이트
│   │   ├── Animations/        # 애니메이션 파일들
│   │   ├── Materials/         # 머티리얼 및 셰이더
│   │   └── PhysicsMaterials/  # 물리 머티리얼
│   ├── Scripts/              # 📜 프로젝트 스크립트 (게임 로직)
│   │   ├── Core/             # 핵심 시스템 (매니저, 유틸리티)
│   │   │   ├── Managers/     # 게임 매니저들
│   │   │   └── Utilities/    # 유틸리티 클래스들
│   │   ├── Gameplay/         # 🎮 게임플레이 로직
│   │   │   ├── Player/       # 플레이어 관련 (이동됨)
│   │   │   ├── Entities/     # 게임 엔티티들
│   │   │   └── Systems/      # 게임플레이 시스템들
│   │   ├── UI/              # 🖥️ UI 관련 스크립트
│   │   ├── Data/            # 📊 ScriptableObject 등 데이터
│   │   └── Tools/           # 🔧 에디터 도구 및 유틸리티
│   ├── Prefabs/             # 📦 프리팹 관리
│   │   ├── Characters/      # 캐릭터 프리팹
│   │   ├── Environment/     # 환경 오브젝트 프리팹
│   │   └── UI/             # UI 프리팹
│   ├── Scenes/              # 🎬 씬 파일들
│   └── Settings/            # ⚙️ 프로젝트 설정 파일들
├── Plugins/                 # 🔌 외부 시스템 및 플러그인
│   ├── FSM_Core/           # FSM 시스템 (코어)
│   ├── GAS_Core/           # GAS 시스템 (코어)
│   └── ThirdParty/         # 서드파티 라이브러리
└── StreamingAssets/        # 📁 스트리밍 에셋 (런타임 로드)
```

## 📁 폴더별 상세 설명

### 🎯 `_Project/` - 프로젝트 메인 폴더

> **목적**: 게임 개발 시 주로 작업하는 모든 에셋들을 포함
> **특징**: 언더스코어(_)로 시작하여 폴더 목록 최상단에 표시

#### 🎨 `Art/` - 아트 에셋 관리
- **Sprites/**: 모든 2D 스프라이트 이미지
  - `Characters/`: 플레이어, NPC, 적 캐릭터 스프라이트
  - `Environment/`: 배경, 타일, 환경 오브젝트
  - `UI/`: 버튼, 아이콘, UI 요소
- **Animations/**: Animator Controller, Animation Clip
- **Materials/**: 머티리얼, 셰이더 파일
- **PhysicsMaterials/**: 2D/3D 물리 머티리얼

#### 📜 `Scripts/` - 프로젝트 스크립트
- **Core/**: 게임의 핵심 시스템
  - `Managers/`: GameManager, SceneManager 등
  - `Utilities/`: 헬퍼 클래스, 확장 메서드
- **Gameplay/**: 실제 게임플레이 로직
  - `Player/`: 플레이어 컨트롤러 및 관련 컴포넌트
  - `Entities/`: 게임 내 엔티티 (적, NPC 등)
  - `Systems/`: 게임플레이 시스템 (전투, 인벤토리 등)
- **UI/**: UI 관련 스크립트
- **Data/**: ScriptableObject, 게임 데이터
- **Tools/**: 에디터 확장, 개발 도구

#### 📦 `Prefabs/` - 프리팹 관리
- **Characters/**: 플레이어, 적 캐릭터 프리팹
- **Environment/**: 환경 오브젝트, 플랫폼
- **UI/**: UI 패널, 버튼 등

#### 🎬 `Scenes/` - 씬 관리
- 게임의 모든 씬 파일들
- 씬별 조명 설정, 포스트 프로세싱 설정

#### ⚙️ `Settings/` - 설정 파일들
- 렌더 파이프라인 설정
- 인풋 시스템 설정
- 프로젝트 전용 설정들

### 🔌 `Plugins/` - 외부 시스템

> **목적**: 재사용 가능한 코어 시스템들과 서드파티 라이브러리

- **FSM_Core/**: 유한상태머신 시스템 (독립적)
- **GAS_Core/**: 게임플레이 어빌리티 시스템 (독립적)
- **ThirdParty/**: 외부에서 가져온 라이브러리들

## 🔄 기존 구조에서 새 구조로 이동

### 이동 매핑 테이블

| 기존 위치 | 새 위치 | 이유 |
|-----------|---------|------|
| `Assets/Scripts/Player/` | `Assets/_Project/Scripts/Gameplay/Player/` | 게임플레이 로직으로 분류 |
| `Assets/Scripts/Managers/` | `Assets/_Project/Scripts/Core/Managers/` | 핵심 시스템으로 분류 |
| `Assets/Scripts/Helper/` | `Assets/_Project/Scripts/Core/Utilities/` | 유틸리티로 명칭 통일 |
| `Assets/Image/` | `Assets/_Project/Art/Sprites/` | 아트 에셋 통합 |
| `Assets/Animation/` | `Assets/_Project/Art/Animations/` | 아트 에셋 통합 |
| `Assets/PhysicsMaterial/` | `Assets/_Project/Art/PhysicsMaterials/` | 아트 에셋 통합 |
| `Assets/Scenes/` | `Assets/_Project/Scenes/` | 프로젝트 전용 영역으로 이동 |
| `Assets/FSM_Core/` | `Assets/Plugins/FSM_Core/` | 플러그인으로 분류 |
| `Assets/GAS_Core/` | `Assets/Plugins/GAS_Core/` | 플러그인으로 분류 |

### 🛠️ 이동 방법

1. **Unity Editor 사용 (권장)**:
   ```
   Tools > Project > Organize Folder Structure
   ```

2. **수동 이동**:
   - Unity에서 직접 드래그 앤 드롭
   - 참조가 자동으로 업데이트됨

## 📋 네이밍 컨벤션

### 폴더 네이밍
- **PascalCase** 사용: `Scripts`, `Prefabs`, `Characters`
- **언더스코어 접두사**: 프로젝트 메인 폴더는 `_Project`
- **복수형** 사용: `Scripts`, `Animations`, `Materials`

### 파일 네이밍
- **PascalCase** 사용: `PlayerController.cs`, `EnemySprite.png`
- **설명적 네이밍**: `PlayerIdleAnimation.anim`, `FireballEffect.prefab`
- **접미사 사용**:
  - 스크립트: `.cs`
  - 프리팹: `.prefab`
  - 머티리얼: `.mat`

## 🎯 사용 가이드라인

### ✅ 권장사항

1. **새 에셋 생성 시**: 적절한 폴더에 바로 생성
2. **관련 파일 그룹화**: 같은 기능의 파일들은 같은 폴더에
3. **일관된 네이밍**: 프로젝트 전체에 일관된 네이밍 사용
4. **정기적 정리**: 주기적으로 불필요한 파일 정리

### ❌ 피해야 할 것

1. **루트 레벨 파일**: Assets 직하에 파일 생성 금지
2. **임시 폴더**: `Temp`, `Test` 등의 임시 폴더 남용
3. **깊은 중첩**: 5단계 이상의 깊은 폴더 구조
4. **모호한 네이밍**: `Stuff`, `Misc`, `Other` 등의 모호한 이름

## 🔧 유지보수

### 정기 점검 사항

1. **빈 폴더 정리**: 사용하지 않는 빈 폴더 제거
2. **참조 확인**: 이동 후 모든 참조가 올바른지 확인
3. **중복 제거**: 중복된 에셋 파일 정리
4. **네이밍 일관성**: 네이밍 컨벤션 준수 확인

### 확장 시 고려사항

- 새로운 시스템 추가 시 `Plugins/` 폴더 활용
- 대규모 아트 에셋 추가 시 서브 폴더 구조 확장
- 다중 씬 프로젝트 시 `Scenes/` 폴더 구조 세분화

## 🎉 이점

### 개발 효율성
- **빠른 파일 찾기**: 논리적 구조로 파일 위치 예측 가능
- **협업 향상**: 팀원 모두가 동일한 구조 이해
- **유지보수 용이**: 체계적 구조로 코드/에셋 관리 편의

### 프로젝트 확장성
- **모듈화**: 각 시스템이 독립적으로 관리
- **재사용성**: Plugins 폴더의 시스템들을 다른 프로젝트에서 재사용
- **확장 용이**: 새로운 기능 추가 시 명확한 위치 지정

이 폴더 구조를 통해 GASPT 프로젝트가 더욱 체계적이고 확장 가능한 구조를 갖게 됩니다! 🚀