# Camera System Design

> GASPT 프로젝트의 카메라 시스템 아키텍처 및 Post-Processing 설계 문서

---

## 목차

1. [개요](#1-개요)
2. [현업 카메라 구조 분석](#2-현업-카메라-구조-분석)
3. [GASPT 카메라 아키텍처](#3-gaspt-카메라-아키텍처)
4. [Post-Processing 시스템](#4-post-processing-시스템)
5. [CameraManager 설계](#5-cameramanager-설계)
6. [카메라 경계 시스템](#6-카메라-경계-시스템)
7. [카메라 효과 시스템](#7-카메라-효과-시스템)
8. [구현 계획](#8-구현-계획)

---

## 1. 개요

### 1.1 카메라 시스템의 역할

게임 카메라는 단순히 화면을 보여주는 것 이상의 역할을 수행합니다:

| 역할 | 설명 | 중요도 |
|------|------|--------|
| **시점 제공** | 플레이어에게 게임 월드를 보여줌 | 필수 |
| **추적 (Follow)** | 플레이어/타겟을 따라다님 | 필수 |
| **경계 제한 (Bounds)** | 화면이 게임 밖을 비추지 않도록 제한 | 높음 |
| **연출 효과** | Shake, Zoom, Blur 등 시각 효과 | 높음 |
| **분위기 연출** | Post-Processing으로 색감, 분위기 조절 | 중간 |

### 1.2 설계 목표

1. **동적 경계 관리**: 씬/Room별로 다른 카메라 경계 지원
2. **효과 시스템**: Shake, Zoom, Blur 등 다양한 효과
3. **Post-Processing 연동**: URP Volume과 통합
4. **성능 최적화**: 모바일 대응 가능한 효율적 구조
5. **확장성**: 새로운 효과 추가 용이

---

## 2. 현업 카메라 구조 분석

### 2.1 단일 카메라 구조 (Single Camera)

```
Main Camera
├── 게임 월드 렌더링
├── UI 렌더링
└── 모든 효과 처리
```

**장점:**
- 구조가 단순함
- 성능 오버헤드 최소
- 설정 및 관리 용이

**단점:**
- 효과 분리 불가능
- 특정 레이어만 효과 적용 불가
- 복잡한 연출에 한계

**적합한 프로젝트:** 소규모, 2D 캐주얼, 모바일

---

### 2.2 다중 카메라 구조 (Multi-Camera)

```
┌─────────────────────────────────────────────────────────┐
│                    Camera Stack                          │
├─────────────────────────────────────────────────────────┤
│  Base Camera (Depth 0)                                   │
│  ├── 배경 렌더링 (Background Layer)                      │
│  └── Clear Flags: Skybox/Solid Color                    │
├─────────────────────────────────────────────────────────┤
│  Main Camera (Depth 1)                                   │
│  ├── 게임 월드 렌더링 (Player, Enemy, Environment)       │
│  ├── Post-Processing 적용                                │
│  └── Clear Flags: Depth Only                            │
├─────────────────────────────────────────────────────────┤
│  Effect Camera (Depth 2)                                 │
│  ├── 이펙트 전용 (Particle, VFX)                         │
│  ├── 별도 Post-Processing 가능                          │
│  └── Clear Flags: Depth Only                            │
├─────────────────────────────────────────────────────────┤
│  UI Camera (Depth 3)                                     │
│  ├── UI 전용 렌더링 (Screen Space - Camera)              │
│  ├── Post-Processing 미적용 (선명한 UI)                  │
│  └── Clear Flags: Depth Only                            │
└─────────────────────────────────────────────────────────┘
```

**장점:**
- 레이어별 효과 완전 분리
- 독립적인 렌더링 설정
- 복잡한 연출 구현 가능
- 월드에만 블러, UI는 선명하게 유지 가능

**단점:**
- Draw Call 증가
- 설정 복잡도 증가
- 카메라 간 동기화 필요

**적합한 프로젝트:** AAA, 콘솔, 연출이 중요한 게임

---

### 2.3 URP Camera Stack (Unity 권장)

```
┌─────────────────────────────────────────────────────────┐
│              URP Camera Stack (Unity 2021+)              │
├─────────────────────────────────────────────────────────┤
│  Base Camera                                             │
│  ├── Render Type: Base                                   │
│  ├── 게임 월드 + 배경                                    │
│  └── Post-Processing Volume 적용                        │
│                                                          │
│  Overlay Cameras (Stack에 추가)                          │
│  ├── Effect Camera (Render Type: Overlay)               │
│  │   └── 특수 이펙트 레이어                              │
│  └── UI Camera (Render Type: Overlay)                   │
│      └── UI 전용 (Post-Processing 제외)                 │
└─────────────────────────────────────────────────────────┘
```

**장점:**
- Unity 최적화 방식
- 단일 렌더 패스로 성능 향상
- Overlay 카메라는 Clear 없이 합성

**단점:**
- URP 필수
- 레거시 프로젝트 마이그레이션 어려움

---

### 2.4 역할별 카메라 분리 (AAA급)

| 카메라 | 역할 | Depth | Culling Mask | Post-Processing |
|--------|------|-------|--------------|-----------------|
| **Background Camera** | 배경, 스카이박스 | -1 | Background | 없음 |
| **Main Camera** | 플레이어, 적, 환경 | 0 | Default, Player, Enemy | Bloom, Color Grading |
| **Effect Camera** | 파티클, VFX | 1 | Effects | Motion Blur |
| **Minimap Camera** | 미니맵 렌더링 | - | Minimap | 없음 (RenderTexture) |
| **UI Camera** | UI 전용 | 10 | UI | 없음 |

---

### 2.5 구조별 비교 요약

| 항목 | 단일 카메라 | 2-Camera | 다중 카메라 | URP Stack |
|------|------------|----------|-------------|-----------|
| **복잡도** | 낮음 | 중간 | 높음 | 중간 |
| **성능** | 최고 | 좋음 | 보통 | 좋음 |
| **UI 블러 분리** | 불가 | 가능 | 가능 | 가능 |
| **레이어별 효과** | 불가 | 제한적 | 완전 | 완전 |
| **구현 난이도** | 쉬움 | 보통 | 어려움 | 보통 |
| **URP 필요** | 아니오 | 아니오 | 아니오 | 예 |

---

## 3. GASPT 카메라 아키텍처

### 3.1 선택: 2-Camera 구조

GASPT는 **2D 로그라이크 플랫포머**로, 다음을 고려하여 **2-Camera 구조**를 채택합니다:

| 고려 사항 | 결정 |
|-----------|------|
| 프로젝트 규모 | 중소규모 → 단순 구조 선호 |
| UI 블러 분리 필요 | 일시정지 시 배경만 블러 → 2-Camera 필요 |
| 성능 요구사항 | 모바일 대응 가능 → 과도한 카메라 분리 지양 |
| 개발 리소스 | 1인 개발 → 관리 용이한 구조 |

---

### 3.2 최종 아키텍처

```
┌─────────────────────────────────────────────────────────┐
│                  GASPT Camera Structure                  │
├─────────────────────────────────────────────────────────┤
│                                                          │
│  PersistentManagers Scene                                │
│  ├─────────────────────────────────────────────────────┤
│  │  Main Camera (Depth 0)                               │
│  │  ├── Tag: MainCamera                                 │
│  │  ├── Culling Mask: Everything except UI              │
│  │  ├── Post-Processing: Volume 적용                    │
│  │  │    ├── Bloom (기본)                               │
│  │  │    ├── Vignette (피격 시)                         │
│  │  │    ├── Color Grading (던전별)                     │
│  │  │    └── Chromatic Aberration (피격 시)             │
│  │  │                                                    │
│  │  └── CameraManager (컴포넌트)                        │
│  │       ├── Follow (플레이어 추적)                      │
│  │       ├── Bounds (경계 제한)                         │
│  │       ├── Shake (화면 흔들림)                        │
│  │       └── Zoom (확대/축소)                           │
│  │                                                       │
│  ├─────────────────────────────────────────────────────┤
│  │  UI Camera (Depth 10) - 선택사항                     │
│  │  ├── Culling Mask: UI Only                           │
│  │  ├── Clear Flags: Depth Only                         │
│  │  ├── Post-Processing: 없음 (선명한 UI)               │
│  │  └── Canvas (Screen Space - Camera)                  │
│  │                                                       │
│  └─────────────────────────────────────────────────────┤
│                                                          │
│  Content Scene (StartRoom / GameplayScene)               │
│  ├─────────────────────────────────────────────────────┤
│  │  CameraBoundsProvider                                │
│  │  ├── 씬 전체 경계 또는                               │
│  │  └── Room별 개별 경계                                │
│  │                                                       │
│  └─────────────────────────────────────────────────────┤
│                                                          │
└─────────────────────────────────────────────────────────┘
```

---

### 3.3 카메라 설정 상세

#### Main Camera 설정

| 속성 | 값 | 설명 |
|------|-----|------|
| Clear Flags | Solid Color | 2D 게임에 적합 |
| Background | #1A1A26 | 어두운 배경색 |
| Projection | Orthographic | 2D 카메라 |
| Size | 5 ~ 10 | 기본 줌 레벨 |
| Near Clip | -10 | 2D 레이어 지원 |
| Far Clip | 100 | 충분한 깊이 |
| Culling Mask | ~UI | UI 레이어 제외 |
| Depth | 0 | 기본 카메라 |

#### UI Camera 설정 (선택사항)

| 속성 | 값 | 설명 |
|------|-----|------|
| Clear Flags | Depth Only | 배경 유지, UI만 렌더 |
| Projection | Orthographic | UI용 |
| Culling Mask | UI | UI 레이어만 |
| Depth | 10 | Main Camera 위에 렌더 |
| Post-Processing | 없음 | UI 선명도 유지 |

---

## 4. Post-Processing 시스템

### 4.1 Post-Processing이란?

**Post-Processing(후처리)**은 카메라가 씬을 렌더링한 **이후**에 최종 이미지에 시각적 효과를 적용하는 기술입니다.

```
┌─────────────┐    ┌─────────────┐    ┌─────────────┐    ┌─────────────┐
│  3D/2D      │ →  │  프레임     │ →  │  Post-      │ →  │  최종       │
│  렌더링     │    │  버퍼       │    │  Processing │    │  화면 출력  │
└─────────────┘    └─────────────┘    └─────────────┘    └─────────────┘
```

**원리:**
1. GPU가 3D/2D 오브젝트를 렌더링하여 프레임 버퍼에 저장
2. Post-Processing 셰이더가 프레임 버퍼의 모든 픽셀을 처리
3. 색상, 밝기, 왜곡 등의 효과를 적용
4. 최종 결과를 화면에 출력

---

### 4.2 Unity Post-Processing 종류

| 렌더 파이프라인 | 사용 방식 | 특징 |
|----------------|----------|------|
| **Built-in** | Post Processing Stack v2 | 레거시, 별도 패키지 |
| **URP** | Volume 시스템 내장 | 권장, 최적화됨 |
| **HDRP** | Volume 시스템 내장 | 고사양, 최다 효과 |

---

### 4.3 주요 Post-Processing 효과 상세

#### Bloom (블룸)

```
원리: 밝은 픽셀 추출 → 가우시안 블러 → 원본에 합성

설정값:
├── Threshold: 밝기 임계값 (기본 0.9)
├── Intensity: 블룸 강도 (기본 1.0)
├── Scatter: 확산 정도 (기본 0.7)
└── Tint: 색조 (기본 White)
```

**게임 활용:**
- 마법 이펙트 강조
- 폭발 효과 강화
- 조명/광원 표현

---

#### Vignette (비네트)

```
원리: 화면 가장자리에서 중앙으로 그라데이션 어둡게

설정값:
├── Color: 비네트 색상 (기본 Black)
├── Center: 중심점 (기본 0.5, 0.5)
├── Intensity: 강도 (기본 0.3)
├── Smoothness: 부드러움 (기본 0.3)
└── Rounded: 원형 여부 (기본 false)
```

**게임 활용:**
- 기본: 약한 검은색 비네트 → 집중감
- 피격: 강한 빨간색 비네트 → 위험 알림
- 저체력: 지속적 빨간 비네트 → 긴장감

---

#### Color Grading (색보정)

```
원리: LUT(Look-Up Table) 또는 개별 색상 조절

설정값:
├── Mode: LDR / HDR
├── Temperature: 색온도 (-100 ~ 100)
├── Tint: 색조 (-100 ~ 100)
├── Saturation: 채도 (-100 ~ 100)
├── Contrast: 대비 (-100 ~ 100)
└── Lift/Gamma/Gain: 톤 조절
```

**게임 활용:**
- 던전별 분위기: 얼음던전 → 푸른 색조
- 보스전: 높은 대비, 어두운 톤
- 안전지대: 따뜻한 색온도

---

#### Chromatic Aberration (색수차)

```
원리: RGB 채널을 각각 다른 방향으로 오프셋

설정값:
├── Intensity: 강도 (0 ~ 1)
└── Fast Mode: 성능 모드 (true 권장)
```

**게임 활용:**
- 피격 시 순간적 적용
- 시공간 왜곡 연출
- 독/상태이상 효과

---

#### Motion Blur (모션 블러)

```
원리: 이전 프레임과 현재 프레임 블렌딩

설정값:
├── Mode: Camera / Object
├── Quality: 샘플링 품질
├── Intensity: 블러 강도
└── Clamp: 최대 블러 길이
```

**게임 활용:**
- 대시/회피 시 적용
- 빠른 카메라 이동
- 슬로우모션 해제 시

---

#### Depth of Field (피사계 심도)

```
원리: 포커스 거리 기준 앞/뒤 블러 처리

설정값:
├── Mode: Gaussian / Bokeh
├── Focus Distance: 초점 거리
├── Aperture: 조리개 (f-stop)
└── Focal Length: 초점 길이
```

**게임 활용:**
- 보스 등장 연출
- 컷씬/대화 장면
- UI 활성화 시 배경 블러

---

### 4.4 URP Volume 시스템

```
┌─────────────────────────────────────────────────────────┐
│                    Volume System                         │
├─────────────────────────────────────────────────────────┤
│                                                          │
│  Global Volume (씬 전체 적용)                            │
│  ├── Is Global: true                                     │
│  ├── Priority: 0 (기본)                                  │
│  ├── Weight: 1.0                                         │
│  └── Profile                                             │
│       ├── Bloom (항상 적용)                              │
│       ├── Vignette (기본값)                              │
│       └── Color Grading (기본값)                         │
│                                                          │
├─────────────────────────────────────────────────────────┤
│                                                          │
│  Local Volume (특정 영역만 적용)                          │
│  ├── Is Global: false                                    │
│  ├── Collider: Box/Sphere (Trigger)                     │
│  ├── Priority: 1 (Global보다 높음)                       │
│  ├── Blend Distance: 3.0 (부드러운 전환)                 │
│  └── Profile                                             │
│       └── 해당 영역 전용 효과                            │
│                                                          │
├─────────────────────────────────────────────────────────┤
│                                                          │
│  ※ Priority가 높은 Volume이 우선 적용                    │
│  ※ Blend Distance로 Volume 간 부드러운 전환             │
│  ※ Weight로 효과 강도 조절 (애니메이션 가능)            │
│                                                          │
└─────────────────────────────────────────────────────────┘
```

---

### 4.5 GASPT Post-Processing 활용 계획

| 상황 | 효과 | 구현 방식 | Priority |
|------|------|----------|----------|
| **기본 화면** | Bloom(약) + Vignette(약) | Global Volume | 0 |
| **플레이어 피격** | Vignette(빨강) + Chromatic | 코드로 일시 적용 | - |
| **저체력** | Vignette(빨강, 지속) | 코드로 Weight 조절 | - |
| **보스 등장** | Vignette(강) + Zoom | 연출용 코드 | - |
| **스킬 시전** | Bloom(강) | 일시적 Intensity 증가 | - |
| **일시정지** | DoF (배경 블러) | UI Camera 분리 | - |
| **얼음 던전** | Color Grading(푸른) | Local Volume | 1 |
| **화염 던전** | Color Grading(붉은) + Bloom(강) | Local Volume | 1 |

---

## 5. CameraManager 설계

### 5.1 클래스 다이어그램

```
┌─────────────────────────────────────────────────────────┐
│                    CameraManager                         │
│            (SingletonManager<CameraManager>)            │
├─────────────────────────────────────────────────────────┤
│  Fields:                                                 │
│  ├── Camera mainCamera                                   │
│  ├── Transform target                                    │
│  ├── CameraBounds currentBounds                         │
│  ├── Vector3 shakeOffset                                │
│  ├── float baseOrthographicSize                         │
│  ├── float currentZoom                                  │
│  ├── Volume postProcessVolume                           │
│  └── VolumeProfile baseProfile                          │
├─────────────────────────────────────────────────────────┤
│  Properties:                                             │
│  ├── Camera MainCamera { get; }                         │
│  ├── Transform Target { get; }                          │
│  ├── bool HasBounds { get; }                            │
│  └── float CurrentZoom { get; }                         │
├─────────────────────────────────────────────────────────┤
│  Methods:                                                │
│  ├── SetTarget(Transform target)                        │
│  ├── ClearTarget()                                      │
│  ├── SetBounds(CameraBounds bounds)                     │
│  ├── ClearBounds()                                      │
│  ├── Shake(float intensity, float duration)             │
│  ├── StopShake()                                        │
│  ├── Zoom(float targetSize, float duration, Ease)       │
│  ├── ResetZoom(float duration)                          │
│  ├── SetVignette(Color color, float intensity)          │
│  ├── ResetVignette(float duration)                      │
│  ├── SetBloom(float intensity)                          │
│  └── ClampToBounds(Vector3 position) → Vector3          │
├─────────────────────────────────────────────────────────┤
│  Events:                                                 │
│  ├── OnTargetChanged(Transform)                         │
│  ├── OnBoundsChanged(CameraBounds)                      │
│  ├── OnShakeStarted()                                   │
│  ├── OnShakeEnded()                                     │
│  └── OnZoomChanged(float)                               │
└─────────────────────────────────────────────────────────┘
```

---

### 5.2 핵심 기능

#### 플레이어 추적 (Follow)

```csharp
// 부드러운 추적 알고리즘
Vector3 targetPosition = target.position + offset;
targetPosition = ClampToBounds(targetPosition);
targetPosition.z = transform.position.z; // Z 고정

transform.position = Vector3.SmoothDamp(
    transform.position,
    targetPosition,
    ref velocity,
    smoothTime
);
```

#### 경계 제한 (Bounds Clamping)

```csharp
// 카메라가 경계 밖을 비추지 않도록 제한
public Vector3 ClampToBounds(Vector3 position)
{
    if (currentBounds == null) return position;

    float halfHeight = mainCamera.orthographicSize;
    float halfWidth = halfHeight * mainCamera.aspect;

    float minX = currentBounds.Min.x + halfWidth;
    float maxX = currentBounds.Max.x - halfWidth;
    float minY = currentBounds.Min.y + halfHeight;
    float maxY = currentBounds.Max.y - halfHeight;

    return new Vector3(
        Mathf.Clamp(position.x, minX, maxX),
        Mathf.Clamp(position.y, minY, maxY),
        position.z
    );
}
```

#### 화면 흔들림 (Shake)

```csharp
// Perlin Noise 기반 자연스러운 흔들림
async Awaitable ShakeRoutine(float intensity, float duration)
{
    float elapsed = 0f;
    float seed = Random.value * 100f;

    while (elapsed < duration)
    {
        float x = (Mathf.PerlinNoise(seed, elapsed * shakeFrequency) - 0.5f) * 2f;
        float y = (Mathf.PerlinNoise(seed + 1, elapsed * shakeFrequency) - 0.5f) * 2f;

        float decay = 1f - (elapsed / duration); // 감쇠
        shakeOffset = new Vector3(x, y, 0) * intensity * decay;

        elapsed += Time.deltaTime;
        await Awaitable.NextFrameAsync();
    }

    shakeOffset = Vector3.zero;
}
```

---

## 6. 카메라 경계 시스템

### 6.1 CameraBounds 구조체

```csharp
[System.Serializable]
public struct CameraBounds
{
    public Vector2 min;
    public Vector2 max;

    public Vector2 Min => min;
    public Vector2 Max => max;
    public Vector2 Size => max - min;
    public Vector2 Center => (min + max) / 2f;

    public bool Contains(Vector2 point)
    {
        return point.x >= min.x && point.x <= max.x &&
               point.y >= min.y && point.y <= max.y;
    }
}
```

---

### 6.2 CameraBoundsProvider 컴포넌트

```
┌─────────────────────────────────────────────────────────┐
│                  CameraBoundsProvider                    │
│              (MonoBehaviour - Content Scene)            │
├─────────────────────────────────────────────────────────┤
│  Fields:                                                 │
│  ├── CameraBounds bounds                                │
│  ├── bool autoRegisterOnEnable                          │
│  ├── bool useColliderBounds (Collider에서 자동 계산)    │
│  └── Color gizmoColor                                   │
├─────────────────────────────────────────────────────────┤
│  Methods:                                                │
│  ├── RegisterToCameraManager()                          │
│  ├── UnregisterFromCameraManager()                      │
│  └── CalculateBoundsFromCollider()                      │
├─────────────────────────────────────────────────────────┤
│  Editor:                                                 │
│  └── OnDrawGizmosSelected() - 경계 시각화               │
└─────────────────────────────────────────────────────────┘
```

---

### 6.3 경계 적용 시나리오

| 씬/상황 | 경계 설정 방식 | 비고 |
|---------|---------------|------|
| **StartRoom** | 씬에 하나의 Provider | 고정 크기 |
| **GameplayScene** | Room별 Provider | Room 진입 시 전환 |
| **보스룸** | 별도 Provider | 보스 영역 제한 |

```
Room 전환 시 경계 업데이트 흐름:
┌──────────────┐    ┌──────────────┐    ┌──────────────┐
│  Room 진입   │ →  │  Provider    │ →  │  Camera      │
│  이벤트      │    │  감지        │    │  Bounds 갱신 │
└──────────────┘    └──────────────┘    └──────────────┘
```

---

## 7. 카메라 효과 시스템

### 7.1 효과 목록

| 효과 | 트리거 | 강도 | 지속시간 |
|------|--------|------|----------|
| **Shake (약)** | 일반 피격 | 0.1 | 0.1초 |
| **Shake (중)** | 강공격 피격 | 0.3 | 0.2초 |
| **Shake (강)** | 폭발, 보스 공격 | 0.5 | 0.3초 |
| **Zoom In** | 보스 등장 | 0.8x | 1.0초 |
| **Zoom Out** | 대규모 전투 | 1.2x | 0.5초 |
| **Vignette (빨강)** | 피격 | 0.5 | 0.2초 |
| **Vignette (검정)** | 페이드 아웃 | 1.0 | 0.5초 |
| **Chromatic** | 피격, 상태이상 | 0.5 | 0.1초 |

---

### 7.2 효과 조합 예시

#### 플레이어 피격

```csharp
public async void OnPlayerHit(float damage)
{
    float intensity = Mathf.Clamp01(damage / maxHealth);

    // 동시 실행
    _ = CameraManager.Instance.Shake(0.2f * intensity, 0.15f);
    _ = CameraManager.Instance.SetVignette(Color.red, 0.4f * intensity, 0.1f);
    _ = CameraManager.Instance.SetChromaticAberration(0.3f * intensity, 0.1f);
}
```

#### 보스 등장

```csharp
public async void OnBossAppear()
{
    // 순차 실행
    await CameraManager.Instance.Zoom(0.7f, 1.0f, Ease.OutCubic);
    await Awaitable.WaitForSecondsAsync(2.0f);
    await CameraManager.Instance.ResetZoom(0.5f);
}
```

---

## 8. 구현 계획

### 8.1 단계별 계획

| 단계 | 작업 | 예상 파일 | 우선순위 |
|------|------|----------|----------|
| **1** | CameraManager 기본 구조 | CameraManager.cs | 높음 |
| **2** | Follow 시스템 (기존 통합) | CameraManager.cs | 높음 |
| **3** | Bounds 시스템 | CameraBounds.cs, CameraBoundsProvider.cs | 높음 |
| **4** | Shake 효과 | CameraManager.cs (확장) | 중간 |
| **5** | Zoom 효과 | CameraManager.cs (확장) | 중간 |
| **6** | Post-Processing 연동 | CameraManager.cs (Volume 제어) | 중간 |
| **7** | 에디터 도구 | CameraBoundsProviderEditor.cs | 낮음 |

---

### 8.2 파일 구조

```
Assets/_Project/Scripts/
├── Camera/
│   ├── CameraManager.cs              # 메인 카메라 매니저
│   ├── CameraBounds.cs               # 경계 구조체
│   ├── CameraBoundsProvider.cs       # 경계 제공자 컴포넌트
│   └── CameraEffects/
│       ├── CameraShake.cs            # 흔들림 효과 (선택)
│       └── CameraZoom.cs             # 줌 효과 (선택)
│
└── Editor/
    └── Camera/
        └── CameraBoundsProviderEditor.cs  # 경계 에디터
```

---

### 8.3 기존 시스템과의 통합

| 기존 시스템 | 통합 방식 |
|-------------|----------|
| **CameraFollow** | CameraManager에 기능 흡수 |
| **PersistentManagersSceneCreator** | CameraManager 컴포넌트 추가 |
| **GameFlowStateMachine** | Room 전환 시 Bounds 업데이트 |
| **PlayerStats** | 피격 시 효과 트리거 |
| **RoomManager** | Room 진입 시 Bounds 전환 |

---

## 참고 자료

### Unity 공식 문서
- [URP Post-Processing](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@latest)
- [Volume Framework](https://docs.unity3d.com/Packages/com.unity.render-pipelines.core@latest)
- [Camera Stacking in URP](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@latest)

### 관련 문서
- [Scene Management System](SceneManagementSystem.md)
- [Project Architecture](PROJECT_ARCHITECTURE.md)
- [Architecture Diagrams](ARCHITECTURE_DIAGRAMS.md)

---

*최종 업데이트: 2025-11-26*
*작성자: GASPT 개발팀*
