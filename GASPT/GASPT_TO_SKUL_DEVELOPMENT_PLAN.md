# GASPT → Skul 스타일 게임 구현 마스터플랜

> **프로젝트명**: GASPT (Generic Ability System + FSM)
> **목표**: Skul: The Hero Slayer 스타일 2D 플랫포머 액션 게임
> **작성일**: 2025-09-27
> **최종 업데이트**: 2025-10-03
> **현재 완성도**: 82% (기반 시스템 + UI 기반 완료)

---

## 📊 1. 현재 프로젝트 분석

### 1.1 구축 완료된 핵심 시스템 (완성도 95%)

#### ✅ FSM_Core 시스템
- **위치**: `Assets/Plugins/FSM_Core/`
- **특징**:
  - 최신 Unity API 사용 (Awaitable 기반 비동기 처리)
  - 성능 최적화된 상태 전환 (어로케이션 최소화)
  - 이벤트 기반 전환 시스템
  - Inspector 디버깅 도구 내장
- **활용도**: ⭐⭐⭐⭐⭐ (스컬별 상태 관리에 완벽)

#### ✅ GAS_Core 시스템
- **위치**: `Assets/Plugins/GAS_Core/`
- **특징**:
  - 게임플레이 어빌리티 시스템
  - 리소스 관리 (MP, 스태미나 등)
  - 어빌리티 쿨다운 시스템
  - 이펙트 및 태그 시스템
- **활용도**: ⭐⭐⭐⭐⭐ (스컬 능력을 어빌리티로 구현)

#### ✅ 커스텀 물리 시스템
- **위치**: `Assets/_Project/Scripts/Gameplay/Player/`
- **특징**:
  - Skul 스타일 정밀한 물리 처리
  - Transform 기반 이동
  - 지면 충돌 예측 및 보정
  - 운동량 보존 시스템
- **활용도**: ⭐⭐⭐⭐⭐ (이미 Skul 스타일로 최적화)

#### ✅ 플레이어 시스템
- **아키텍처**: 컴포넌트 조합 패턴 (SRP 준수)
- **구성 요소**:
  - `InputHandler.cs`: 입력 처리 전용
  - `PhysicsController.cs`: 커스텀 물리 시스템
  - `EnvironmentChecker.cs`: 환경 검사 (접지, 벽 감지)
  - `AnimationController.cs`: 애니메이션 제어
  - `GroundChecker.cs`: 정밀한 착지 감지
- **상태 시스템**: Idle, Move, Jump, Fall, Dash, Attack, Hit, Dead, WallGrab, WallJump, Slide

### 1.2 부분 구현된 시스템 (완성도 85%)

#### ✅ 매니저 시스템 (업데이트: 2025-10-03)
- **GameManager**: 기본 게임 상태 관리 + 골드/다이아 리소스 시스템 ✅
- **UIManager**: UI 패널 관리 및 로딩 오버레이
- **HUDManager**: 인게임 HUD 전체 관리 (신규 추가) ✅
- **GameFlowManager**: 게임 흐름 FSM 제어
- **AudioManager**: 기본 오디오 시스템
- **SceneBootstrap**: 씬 독립 테스트 환경 시스템 ✅

#### 🔄 씬 구조
- **Bootstrap**: `00_Bootstrap.unity` (초기화 씬)
- **UI 씬들**: MainMenu, LevelSelect, Settings, GameOver, LevelComplete
- **게임플레이**: `03_Gameplay.unity`, `04_PauseMenu.unity`

---

## 🎮 2. Skul 게임 핵심 시스템 분석

### 2.1 스컬 시스템 (게임의 핵심)
- **100개+ 플레이어블 캐릭터**: 각기 다른 능력과 외형
- **2개 스컬 동시 사용**: 실시간 전환 가능
- **스컬 각성 시스템**: 업그레이드를 통한 능력 향상
- **본 시스템**: 원하지 않는 스컬을 뼈로 변환하여 강화

### 2.2 전투 시스템
- **기본 공격**: 뼈 무기 기반 근접 공격
- **스컬 던지기**: 원거리 공격 + 텔레포트
- **각 스컬별 고유 어빌리티**: 2-3개의 특수 능력
- **콤보 시스템**: 스컬 전환을 통한 연계 공격

### 2.3 이동 시스템
- **정밀한 플랫폼 액션**: 픽셀 퍼펙트 컨트롤
- **대시/회피**: 무적 프레임이 있는 회피 기동
- **벽 점프**: 제한적인 벽면 이동

### 2.4 로그라이크 요소
- **랜덤 생성 맵**: 매번 다른 레벨 구성
- **아이템 시너지**: 다양한 아이템 조합 효과
- **진행도 기반 해금**: 새로운 스컬과 아이템 획득

---

## 🗺️ 3. 구현 로드맵 (단계별 개발 계획)

### 🎯 Phase 1: 기본 플레이어블 프로토타입 (2-3주)

#### 1.1 기본 스컬 시스템 구축 (1주)
**우선순위**: 🔥 HIGH

**구현 파일**:
```
Assets/_Project/Scripts/Gameplay/Skull/
├── Core/
│   ├── SkullSystem.cs           # 스컬 교체 메인 시스템
│   ├── BaseSkull.cs             # 모든 스컬의 기본 클래스
│   ├── SkullDatabase.cs         # ScriptableObject 기반 스컬 데이터
│   └── SkullManager.cs          # 스컬 인벤토리 관리
├── Types/
│   ├── DefaultSkull.cs          # 기본 뼈 전사 스컬
│   ├── MageSkull.cs             # 마법사 스컬
│   └── WarriorSkull.cs          # 전사 스컬
└── Data/
    ├── SkullData.cs             # 스컬 기본 데이터 구조
    └── SkullStats.cs            # 스컬 능력치 데이터
```

**핵심 구현**:
```csharp
// GAS_Core 연동 예시
public class SkullAbility : Ability
{
    [SerializeField] private SkullType skullType;
    [SerializeField] private GameObject skullPrefab;
    [SerializeField] private SkullStats stats;

    public override async Awaitable ExecuteAbility(IGameplayContext context)
    {
        // 스컬별 고유 능력 실행
    }
}

// FSM_Core 연동 예시
public class SkullChangeState : PlayerBaseState
{
    public override async Awaitable OnEnterState(CancellationToken token)
    {
        // 스컬 교체 애니메이션 및 능력치 변경
    }
}
```

#### 1.2 기본 전투 시스템 (1주)
**우선순위**: 🔥 HIGH

**구현 파일**:
```
Assets/_Project/Scripts/Gameplay/Combat/
├── CombatController.cs          # 기본 공격 처리
├── SkullThrowAbility.cs         # 스컬 던지기 + 텔레포트
├── HitboxSystem.cs              # 데미지 판정 시스템
├── HealthSystem.cs              # 체력 관리
├── DamageSystem.cs              # 데미지 계산
└── CombatPhysics.cs             # 전투 관련 물리 처리
```

#### 1.3 기본 적 AI (1주)
**우선순위**: 🔥 HIGH

**구현 파일**:
```
Assets/_Project/Scripts/Gameplay/Enemy/
├── Core/
│   ├── EnemyController.cs       # 기본 적 AI
│   ├── EnemyStates.cs           # 적 행동 상태들
│   └── EnemySpawner.cs          # 적 생성 시스템
├── Types/
│   ├── BasicMeleeEnemy.cs       # 근접 공격 적
│   ├── RangedEnemy.cs           # 원거리 공격 적
│   └── BossEnemy.cs             # 보스 적
└── AI/
    ├── EnemyAI.cs               # AI 행동 로직
    └── PatrolBehavior.cs        # 순찰 행동
```

### 🎯 Phase 2: 확장된 스컬 시스템 (3-4주)

#### 2.1 다양한 스컬 타입 구현 (2주)
**우선순위**: 🟡 MEDIUM

**스컬 타입별 구현**:
- **MageSkull.cs**: 마법사 타입 (원거리 마법 공격)
- **WarriorSkull.cs**: 전사 타입 (강력한 근접 공격)
- **AssassinSkull.cs**: 암살자 타입 (빠른 이동, 은신)
- **TankSkull.cs**: 탱커 타입 (방어 중심, 높은 체력)
- **RiderSkull.cs**: 특수 타입 (바이크 탑승, 고속 이동)

#### 2.2 스컬 각성 시스템 (1-2주)
**우선순위**: 🟡 MEDIUM

**구현 파일**:
```
Assets/_Project/Scripts/Gameplay/Skull/Evolution/
├── SkullEvolution.cs            # 스컬 진화 로직
├── BoneSystem.cs                # 뼈 수집 및 강화 시스템
├── EvolutionData.cs             # 진화 조건 데이터
└── UI/
    └── SkullUpgradeUI.cs        # 강화 인터페이스
```

### 🎯 Phase 3: 게임플레이 콘텐츠 (4-5주)

#### 3.1 레벨 시스템 (2-3주)
**우선순위**: 🟡 MEDIUM

**구현 파일**:
```
Assets/_Project/Scripts/Gameplay/Level/
├── Generation/
│   ├── LevelGenerator.cs        # 절차적 레벨 생성
│   ├── RoomDatabase.cs          # 방 템플릿 데이터
│   └── RoomConnector.cs         # 방 연결 로직
├── Management/
│   ├── LevelManager.cs          # 레벨 진행 관리
│   ├── DoorSystem.cs            # 방 간 이동 시스템
│   └── CheckpointSystem.cs      # 체크포인트 관리
└── Data/
    ├── LevelData.cs             # 레벨 메타데이터
    └── RoomData.cs              # 방 데이터 구조
```

#### 3.2 아이템 시스템 (2주)
**우선순위**: 🟡 MEDIUM

**구현 파일**:
```
Assets/_Project/Scripts/Gameplay/Item/
├── Core/
│   ├── ItemSystem.cs            # 아이템 수집 및 효과
│   ├── ItemDatabase.cs          # 아이템 데이터베이스
│   └── InventorySystem.cs       # 인벤토리 관리
├── Effects/
│   ├── ItemEffect.cs            # 아이템 효과 기본 클래스
│   ├── StatModifier.cs          # 능력치 변경 효과
│   └── ItemSynergy.cs           # 아이템 조합 효과
└── UI/
    └── InventoryUI.cs           # 인벤토리 UI
```

### 🎯 Phase 4: 고급 시스템 (3-4주)

#### 4.1 이펙트 및 피드백 (2주)
**우선순위**: 🟢 LOW

**구현 파일**:
```
Assets/_Project/Scripts/Effects/
├── VFXManager.cs                # 시각 효과 관리
├── ScreenShake.cs               # 화면 흔들림
├── ParticlePooler.cs            # 파티클 풀링
├── SoundManager.cs              # 사운드 이펙트
└── FeedbackSystem.cs            # 게임플레이 피드백
```

#### 4.2 UI/UX 완성 (1-2주)
**우선순위**: 🔥 HIGH → 🟡 MEDIUM (부분 완료)
**완성도**: 40% (2025-10-03 업데이트)

**구현 완료된 파일** ✅:
```
Assets/_Project/Scripts/UI/HUD/
├── HUDManager.cs                # HUD 전체 관리
├── HealthBarUI.cs               # 체력바 게이지 UI
├── ItemSlotUI.cs                # 아이템/스킬 슬롯 UI
├── PlayerInfoPanel.cs           # 플레이어 정보 패널
├── ResourcePanel.cs             # 골드/다이아 표시
└── UI.HUD.asmdef                # 어셈블리 정의

Assets/_Project/Scripts/Editor/UI/
└── HUDPrefabCreator.cs          # HUD 프리팹 자동 생성 도구

Assets/_Project/Prefabs/UI/
└── GameHUD.prefab               # 완성된 HUD 프리팹
```

**구현 예정 파일**:
```
Assets/_Project/Scripts/UI/
├── Game/
│   ├── SkullSelectionUI.cs      # 스컬 선택 인터페이스
│   └── MiniMapUI.cs             # 미니맵 UI
├── Menu/
│   ├── MainMenuUI.cs            # 메인 메뉴
│   ├── SettingsUI.cs            # 설정 메뉴
│   └── ProgressUI.cs            # 진행도 표시
└── Core/
    └── UITransition.cs          # UI 전환 효과
```

---

## 🚀 4. 구현 우선순위 및 일정

### 🔥 Phase 1 (즉시 시작) - MVP 구현 (3주)
| 주차 | 작업 내용 | 예상 소요시간 | 완료 조건 |
|------|-----------|---------------|-----------|
| 1주 | 기본 스컬 교체 시스템 | 40시간 | 3개 스컬 간 전환 가능 |
| 2주 | 기본 전투 + 적 AI | 40시간 | 스컬 던지기, 기본 적 처치 가능 |
| 3주 | 기본 레벨 구성 | 40시간 | 1개 레벨에서 게임 루프 완성 |

### 🟡 Phase 2 (MVP 완성 후) - 콘텐츠 확장 (4주)
| 주차 | 작업 내용 | 예상 소요시간 | 완료 조건 |
|------|-----------|---------------|-----------|
| 4-5주 | 5-10개 추가 스컬 | 80시간 | 다양한 플레이 스타일 제공 |
| 6주 | 스컬 각성 시스템 | 40시간 | 스컬 강화 및 진화 가능 |
| 7주 | 3-5개 기본 레벨 | 40시간 | 레벨 진행 시스템 완성 |

### 🟢 Phase 3 (알파 버전) - 시스템 완성 (6주)
| 주차 | 작업 내용 | 예상 소요시간 | 완료 조건 |
|------|-----------|---------------|-----------|
| 8-9주 | 절차적 레벨 생성 | 80시간 | 무한 재플레이 가능 |
| 10-11주 | 아이템 시스템 | 80시간 | 아이템 수집 및 시너지 |
| 12-13주 | 이펙트 및 폴리시 | 80시간 | 게임 완성도 향상 |

---

## 💡 5. 기존 시스템 활용 전략

### 5.1 GAS_Core 확장 활용
```csharp
// 스컬 능력을 GAS Ability로 구현
public class SkullMagicMissile : Ability
{
    protected override async Awaitable ExecuteAbility(IGameplayContext context)
    {
        // 마법 미사일 발사 로직
        var projectile = ProjectilePool.Get();
        projectile.Initialize(context.Owner.transform.position, targetDirection);
        await projectile.Launch();
    }
}

// 뼈 시스템을 GAS Resource로 관리
public class BoneResource : IGameplayResource
{
    public float CurrentAmount { get; private set; }
    public float MaxAmount { get; private set; } = 999f;

    public void AddBones(int amount)
    {
        CurrentAmount = Mathf.Min(CurrentAmount + amount, MaxAmount);
        OnResourceChanged?.Invoke(CurrentAmount, MaxAmount);
    }
}
```

### 5.2 FSM_Core 확장 활용
```csharp
// 스컬별 특수 상태 추가
public class SkullMageChannelingState : PlayerBaseState
{
    [SerializeField] private float channelingTime = 2f;

    public override async Awaitable OnEnterState(CancellationToken token)
    {
        // 마법 차징 상태
        await Awaitable.WaitForSecondsAsync(channelingTime, token);
    }
}

// 적 AI도 FSM 기반으로 구현
public class EnemyPatrolState : State
{
    public override async Awaitable OnEnterState(CancellationToken token)
    {
        // 순찰 로직
    }
}
```

### 5.3 물리 시스템 확장 활용
```csharp
// 기존 PhysicsController 확장
public class SkullPhysics : PhysicsController
{
    public async Awaitable PerformSkullThrow(Vector2 direction)
    {
        // 스컬 던지기 물리 처리
        var throwPosition = transform.position + (Vector3)direction * throwDistance;

        // 텔레포트 예약
        await AwaitableHelper.WaitForSecondsAsync(0.5f);
        transform.position = throwPosition;
    }
}
```

---

## 🎯 6. 개발 팁 및 고려사항

### 6.1 기존 자산 최대 활용
- **GAS_Core**: 스컬 능력을 어빌리티로 구현하여 확장성 확보
- **FSM_Core**: 복잡한 스컬별 상태 관리를 FSM으로 체계화
- **물리 시스템**: 이미 Skul 스타일로 튜닝되어 있어 바로 활용 가능

### 6.2 확장성 고려
- **데이터 드리븐**: ScriptableObject 기반으로 스컬/아이템 데이터 관리
- **모듈화**: 각 스컬을 독립적인 모듈로 설계하여 추가 용이성 확보
- **성능**: 오브젝트 풀링으로 스컬 교체 시 GC 최소화

### 6.3 플레이어 경험 우선
- **즉각적인 피드백**: 스컬 교체 시 즉시 느껴지는 변화
- **직관적인 컨트롤**: 기존 입력 시스템 기반으로 자연스러운 조작
- **시각적 명확성**: 스컬별 뚜렷한 시각적 차별화

---

## 🏆 7. 성공 지표 및 마일스톤

### 7.1 Phase 1 MVP 성공 조건
- [ ] 3개 기본 스컬 간 즉시 전환 가능
- [ ] 스컬 던지기 + 텔레포트 시스템 완성
- [ ] 기본 적과의 전투 시스템 동작
- [ ] 1개 레벨에서 완전한 게임 루프 경험 가능

### 7.2 Phase 2 확장 성공 조건
- [ ] 10개 이상 스컬로 다양한 플레이 스타일 제공
- [ ] 스컬 각성 시스템으로 progression 경험
- [ ] 5개 이상 레벨에서 연속 플레이 가능
- [ ] 기본 아이템 시스템 동작

### 7.3 Phase 3 완성 성공 조건
- [ ] 절차적 레벨 생성으로 무한 재플레이성 확보
- [ ] 아이템 시너지 시스템으로 깊이 있는 전략성 제공
- [ ] 완성도 있는 비주얼 및 오디오 피드백
- [ ] 플레이어 데이터 저장/로드 시스템

---

## 📋 8. 리스크 관리 및 대안

### 8.1 기술적 리스크
**리스크**: 스컬 교체 시 성능 문제
**대안**: 오브젝트 풀링 및 미리 로드 시스템 구축

**리스크**: 절차적 레벨 생성 복잡성
**대안**: 먼저 수동 레벨로 시작 후 점진적 자동화

### 8.2 스코프 리스크
**리스크**: 스컬 종류가 너무 많아짐
**대안**: 핵심 5-10개 스컬에 집중 후 점진적 확장

**리스크**: 기능 추가로 인한 일정 지연
**대안**: MVP 우선 완성 후 단계적 기능 추가

---

## 🚀 9. 결론 및 추천사항

### 9.1 현재 프로젝트 강점
- **95% 완성된 견고한 기반 시스템**
- **Skul 게임에 최적화된 아키텍처**
- **상업적 수준의 코드 품질**
- **확장성을 고려한 모듈 설계**

### 9.2 추천 시작점
**이번 주**: 기본 스컬 교체 시스템 구현에 집중
**다음 주**: 3개 기본 스컬(기본/마법사/전사) 완성
**3주차**: 기본 전투 + 적 AI로 플레이 가능한 프로토타입 완성

### 9.3 성공 확률 평가
- **기반 시스템 완성도**: ⭐⭐⭐⭐⭐ (95%)
- **아키텍처 적합성**: ⭐⭐⭐⭐⭐ (Skul에 최적화)
- **개발 효율성**: ⭐⭐⭐⭐⭐ (기존 자산 활용)
- **전체 성공 확률**: ⭐⭐⭐⭐⭐ (매우 높음)

**최종 결론**: 현재 GASPT 프로젝트는 Skul 스타일 게임을 구현하기에 **이상적인 상태**입니다. 기반 시스템이 탄탄하므로 **게임플레이 콘텐츠 구현에 집중**하면 단기간에 플레이 가능한 고품질 프로토타입을 완성할 수 있습니다! 🎮

---

## 📅 업데이트 이력

### v1.1 - 2025-10-03
- **HUD 시스템 완성**
  - 5개 핵심 UI 컴포넌트 구현
  - GameManager 리소스 시스템 확장
  - HUD 프리팹 자동 생성 도구 추가
  - SceneBootstrap 테스트 환경 개선
- **현재 완성도**: 80% → 82%
- **Phase 4.2 UI/UX**: 0% → 40% 진행

### v1.0 - 2025-09-27
- 초기 문서 작성
- 전체 개발 로드맵 수립
- 기반 시스템 분석 완료

---

**문서 버전**: v1.1
**마지막 업데이트**: 2025-10-03
**작성자**: Claude Code Assistant