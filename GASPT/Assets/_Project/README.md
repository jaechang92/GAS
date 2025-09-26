# 🎮 GASPT Project - 개선된 폴더 구조

> **GASPT**: Generic Ability System + FSM Based Player Controller
> Unity 2023+ 기반 모듈형 게임 개발 프로젝트

## 🏗️ 프로젝트 구조 개요

이 프로젝트는 **확장성**, **유지보수성**, **재사용성**을 고려한 체계적인 폴더 구조를 사용합니다.

```
Assets/
├── _Project/                    # 🎯 메인 작업 영역
│   ├── Art/                    # 🎨 모든 아트 에셋
│   ├── Scripts/                # 📜 게임 로직 스크립트
│   ├── Prefabs/               # 📦 게임 오브젝트 프리팹
│   ├── Scenes/                # 🎬 게임 씬들
│   └── Settings/              # ⚙️ 프로젝트 설정
├── Plugins/                   # 🔌 독립적 시스템 & 라이브러리
│   ├── FSM_Core/             # 유한상태머신
│   ├── GAS_Core/             # 게임플레이 어빌리티 시스템
│   └── ThirdParty/           # 외부 라이브러리
└── [Unity Generated]/        # Unity 자동 생성 폴더들
```

## 🎯 핵심 특징

### 🔧 컴포넌트 분리 아키텍처
- **단일책임원칙(SRP)** 준수
- **이벤트 기반 통신**으로 느슨한 결합
- **컴포넌트 조합 패턴**으로 유연한 확장

### 📁 체계적 폴더 구조
- **논리적 분류**: 기능별, 타입별 명확한 분류
- **확장 용이**: 새 기능 추가 시 명확한 위치
- **팀 협업**: 모든 팀원이 쉽게 이해할 수 있는 구조

### 🔌 모듈화된 시스템
- **FSM Core**: 상태 관리 시스템
- **GAS Core**: 어빌리티 시스템
- **독립적 운용**: 각 시스템을 다른 프로젝트에서 재사용 가능

## 📋 폴더별 가이드

### 🎯 `_Project/` - 메인 작업 영역
프로젝트 고유의 모든 에셋과 스크립트가 위치합니다.

#### 🎨 `Art/` - 아트 에셋 통합 관리
```
Art/
├── Sprites/          # 2D 이미지 (캐릭터, 환경, UI)
├── Animations/       # 애니메이션 파일들
├── Materials/        # 머티리얼 및 셰이더
└── PhysicsMaterials/ # 물리 머티리얼
```

#### 📜 `Scripts/` - 게임 로직
```
Scripts/
├── Core/            # 핵심 시스템 (매니저, 유틸리티)
├── Gameplay/        # 게임플레이 로직 (플레이어, 엔티티)
├── UI/             # 사용자 인터페이스
├── Data/           # ScriptableObject 등 데이터
└── Tools/          # 에디터 도구 및 유틸리티
```

#### 📦 `Prefabs/` - 게임 오브젝트
```
Prefabs/
├── Characters/      # 캐릭터 프리팹
├── Environment/     # 환경 오브젝트
└── UI/             # UI 프리팹
```

### 🔌 `Plugins/` - 독립적 시스템
재사용 가능한 시스템들과 외부 라이브러리가 위치합니다.

## 🚀 시작하기

### 1. 새 에셋 생성 시
```
📁 스크립트: _Project/Scripts/[Core|Gameplay|UI]/
📁 스프라이트: _Project/Art/Sprites/[Characters|Environment|UI]/
📁 프리팹: _Project/Prefabs/[Characters|Environment|UI]/
📁 씬: _Project/Scenes/
```

### 2. 플레이어 캐릭터 만들기
```csharp
// 1. PlayerSetupGuide 컴포넌트 추가
// 2. Inspector에서 "플레이어 컴포넌트 자동 설정" 클릭
// 3. 설정 조정 후 플레이 테스트
```

### 3. 새 시스템 추가 시
- **게임 고유 로직**: `_Project/Scripts/Gameplay/`
- **재사용 가능 시스템**: `Plugins/`

## 🛠️ 개발 도구

### Unity 에디터 확장
```
Tools > Project > Organize Folder Structure
```
- 폴더 구조 자동 정리
- 에셋 이동 및 정리

### 디버깅 도구
- **PlayerController**: 상태 및 컴포넌트 디버깅
- **FSM Visualizer**: 상태 전환 시각화
- **GAS Inspector**: 어빌리티 및 이펙트 모니터링

## 📚 상세 가이드

### 플레이어 시스템
- 📖 [FSM 플레이어 사용 가이드](Scripts/Gameplay/Player/FSM_PLAYER_USAGE_GUIDE.md)
- 📖 [컴포넌트 분리 아키텍처](Scripts/Gameplay/Player/README.md)

### 폴더 구조
- 📖 [폴더 구조 상세 가이드](FOLDER_STRUCTURE_GUIDE.md)
- 📖 [네이밍 컨벤션](FOLDER_STRUCTURE_GUIDE.md#네이밍-컨벤션)

### 시스템별 가이드
- 📖 [FSM Core 시스템](../Plugins/FSM_Core/README.md)
- 📖 [GAS Core 시스템](../Plugins/GAS_Core/README.md)

## 🎯 프로젝트 목표

### 단기 목표 ✅
- [x] 단일책임원칙 기반 플레이어 컨트롤러 구현
- [x] FSM 기반 상태 관리 시스템
- [x] 착지 시스템 개선 (묻힘 현상 해결)
- [x] 체계적 폴더 구조 구축

### 장기 목표 🎯
- [ ] GAS 기반 스킬 시스템 완성
- [ ] AI 시스템 (플레이어와 동일한 컴포넌트 구조 활용)
- [ ] 인벤토리 및 아이템 시스템
- [ ] 멀티플레이어 지원

## 🤝 기여 가이드

### 코드 컨벤션
- **C# 네이밍**: PascalCase 사용
- **파일 네이밍**: 설명적이고 일관된 네이밍
- **주석**: 공개 API는 XML 문서 주석 필수

### 폴더 규칙
- **새 스크립트**: 적절한 폴더에 생성
- **임시 파일**: Assets 루트 레벨 생성 금지
- **정리**: 주기적으로 사용하지 않는 파일 정리

## 📞 지원 및 문의

- **개발자**: JaeChang
- **프로젝트**: GASPT (Generic Ability System + FSM Player)
- **Unity 버전**: 2023.3+
- **라이선스**: MIT License

---

🎮 **Happy Game Development!** 🎮