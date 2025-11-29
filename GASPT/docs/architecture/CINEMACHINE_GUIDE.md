# Cinemachine 완전 가이드

> GASPT 프로젝트를 위한 Cinemachine 학습 및 활용 가이드

**작성일**: 2025-11-27
**대상**: 2D 메트로바니아 스타일 게임
**Unity 버전**: 6.0+
**Cinemachine 버전**: 3.x

---

## 목차

1. [Cinemachine 기초](#1-cinemachine-기초)
2. [핵심 컴포넌트](#2-핵심-컴포넌트)
3. [2D 게임 설정](#3-2d-게임-설정)
4. [Room 기반 경계 시스템](#4-room-기반-경계-시스템)
5. [Dead Zone & Look Ahead](#5-dead-zone--look-ahead)
6. [카메라 전환](#6-카메라-전환)
7. [Impulse 시스템 (화면 흔들림)](#7-impulse-시스템-화면-흔들림)
8. [보스전 연출](#8-보스전-연출)
9. [컷씬 & Timeline](#9-컷씬--timeline)
10. [기존 시스템과의 통합](#10-기존-시스템과의-통합)
11. [실전 레시피](#11-실전-레시피)

---

## 1. Cinemachine 기초

### 1.1 Cinemachine이란?

Cinemachine은 Unity의 **절차적 카메라 시스템**입니다. 코드 없이 복잡한 카메라 동작을 구현할 수 있습니다.

```
기존 방식: 직접 카메라 Transform 조작 → 복잡한 코드 필요
Cinemachine: 설정 기반 → 인스펙터에서 설정, 자동 블렌딩
```

### 1.2 기본 개념

```
┌─────────────────────────────────────────────────┐
│                Unity Camera                      │
│  (실제 렌더링하는 카메라, Brain 컴포넌트 부착)   │
└───────────────────────┬─────────────────────────┘
                        │
                        ▼
┌─────────────────────────────────────────────────┐
│              Cinemachine Brain                   │
│  (Virtual Camera들을 관리, 블렌딩 처리)         │
└───────────────────────┬─────────────────────────┘
                        │
        ┌───────────────┼───────────────┐
        ▼               ▼               ▼
┌───────────────┐ ┌───────────────┐ ┌───────────────┐
│ Virtual Cam 1 │ │ Virtual Cam 2 │ │ Virtual Cam 3 │
│  (플레이어)   │ │   (보스전)    │ │   (컷씬)      │
└───────────────┘ └───────────────┘ └───────────────┘
```

### 1.3 패키지 설치

1. **Window > Package Manager** 열기
2. **Unity Registry** 선택
3. **Cinemachine** 검색
4. **Install** 클릭

> Unity 6.0+에서는 Cinemachine 3.x가 기본입니다.

---

## 2. 핵심 컴포넌트

### 2.1 Cinemachine Brain

**역할**: Main Camera에 부착, Virtual Camera들을 관리

```csharp
// Main Camera에 자동으로 추가됨
[RequireComponent(typeof(Camera))]
public class CinemachineBrain : MonoBehaviour
{
    // 업데이트 방식: FixedUpdate, LateUpdate, SmartUpdate
    public CinemachineBrain.UpdateMethod updateMethod;

    // 블렌딩 방식
    public CinemachineBlendDefinition defaultBlend;
}
```

**주요 설정**:
| 설정 | 권장값 | 설명 |
|-----|-------|------|
| Update Method | Smart Update | 물리/렌더링 자동 조정 |
| Blend Update | Late Update | 부드러운 전환 |
| Default Blend | Ease In Out, 0.5s | 기본 전환 시간 |

### 2.2 Cinemachine Virtual Camera (2D용)

**역할**: 카메라의 "설정"을 정의하는 가상 카메라

```csharp
// 2D 게임에서는 CinemachineCamera 사용 (3.x 버전)
public class CinemachineCamera : CinemachineVirtualCameraBase
{
    // 추적 대상
    public Transform Follow;

    // 화면 중심 대상 (보통 Follow와 동일)
    public Transform LookAt;

    // 우선순위 (높을수록 활성화)
    public int Priority;
}
```

### 2.3 Extensions (확장 기능)

Virtual Camera에 추가 기능을 부여:

| Extension | 용도 |
|-----------|------|
| **CinemachineConfiner2D** | 카메라 경계 제한 |
| **CinemachineImpulseListener** | 화면 흔들림 수신 |
| **CinemachinePixelPerfect** | 픽셀 아트용 정렬 |
| **CinemachinePostProcessing** | Post Processing 연동 |

---

## 3. 2D 게임 설정

### 3.1 기본 설정 단계

#### Step 1: Main Camera 설정

```
Main Camera (GameObject)
├── Camera (Component)
│   ├── Projection: Orthographic
│   └── Size: 5 (게임에 맞게 조정)
└── CinemachineBrain (Component)
    ├── Update Method: Smart Update
    └── Default Blend: Ease In Out, 0.5s
```

#### Step 2: Virtual Camera 생성

메뉴: **GameObject > Cinemachine > Virtual Camera**

```
CM vcam1 (GameObject)
└── CinemachineCamera (Component)
    ├── Follow: Player
    ├── LookAt: (비워두기 - 2D에서는 보통 불필요)
    └── Body: Position Composer (2D)
```

### 3.2 Position Composer 설정 (2D 추적)

**Inspector에서 Body 섹션** (Cinemachine 3.x 구조):

```
CinemachinePositionComposer
├── Lookahead
│   ├── Time: 0.2 (플레이어 이동 예측)
│   ├── Smoothing: 5
│   └── IgnoreY: true (수직 점프 시 카메라 흔들림 방지)
├── Damping: (0.5, 0.5, 0) (X, Y, Z 지연)
├── Composition (ScreenComposerSettings)
│   ├── ScreenPosition: (0, 0) (화면 중앙, Cinemachine 3.x: 0=중앙, ±1=가장자리)
│   ├── DeadZone
│   │   └── Size: (0.1, 0.1) (카메라 움직이지 않는 중앙 영역)
│   └── HardLimits (SoftZone 대체)
│       └── Size: (0.8, 0.8) (타겟이 벗어날 수 없는 최대 영역)
└── CameraDistance: 10 (Z축 거리)
```

> **참고**: Cinemachine 3.x에서 **SoftZone이 HardLimits로 대체**되었습니다.

**코드로 설정 시** (Cinemachine 3.x API):
```csharp
// Composition을 통해 DeadZone, HardLimits 접근
var composition = positionComposer.Composition;

// DeadZone 설정 - 카메라가 움직이지 않는 영역
var deadZone = composition.DeadZone;
deadZone.Size = new Vector2(0.1f, 0.1f);
composition.DeadZone = deadZone;

// HardLimits 설정 - 타겟이 벗어날 수 없는 최대 영역 (기존 SoftZone 대체)
var hardLimits = composition.HardLimits;
hardLimits.Size = new Vector2(0.8f, 0.8f);
composition.HardLimits = hardLimits;

// ScreenPosition 설정 (Cinemachine 3.x: 0 = 중앙, ±1 = 가장자리)
composition.ScreenPosition = new Vector2(0f, 0f);

positionComposer.Composition = composition;
```

### 3.3 설정 시각화

```
┌──────────────────────────────────────────┐
│                 Soft Zone                 │
│  ┌────────────────────────────────────┐  │
│  │          Blue Area                 │  │
│  │  ┌──────────────────────────────┐  │  │
│  │  │       Dead Zone              │  │  │
│  │  │  ┌────────────────────────┐  │  │  │
│  │  │  │   🎮 Player            │  │  │  │
│  │  │  │   (여기서는 카메라      │  │  │  │
│  │  │  │    움직이지 않음)       │  │  │  │
│  │  │  └────────────────────────┘  │  │  │
│  │  └──────────────────────────────┘  │  │
│  └────────────────────────────────────┘  │
└──────────────────────────────────────────┘
```

---

## 4. Room 기반 경계 시스템

### 4.1 CinemachineConfiner2D 사용

메트로바니아에서 가장 중요한 기능입니다.

#### Step 1: Confiner 영역 생성

```
Room_01 (GameObject)
├── PolygonCollider2D (Component)
│   ├── Is Trigger: true ✓
│   └── Used By Composite: false
└── Points: 룸 경계 정의
```

#### Step 2: Virtual Camera에 Confiner 추가

```
CM vcam1
└── Extensions
    └── CinemachineConfiner2D
        ├── Bounding Shape 2D: Room_01의 Collider
        ├── Damping: 0.5 (경계 도달 시 부드러움)
        └── Max Window Size: 0 (자동)
```

### 4.2 다중 Room 처리

```csharp
// RoomTrigger.cs - Room 진입 시 Confiner 변경
using UnityEngine;
using Unity.Cinemachine;

public class RoomConfinerTrigger : MonoBehaviour
{
    [SerializeField] private Collider2D roomBounds;
    [SerializeField] private CinemachineConfiner2D confiner;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Confiner 경계 변경
            confiner.BoundingShape2D = roomBounds;
            confiner.InvalidateBoundingShapeCache();
        }
    }
}
```

### 4.3 비정형 Room 지원

PolygonCollider2D를 사용하면 어떤 모양도 가능:

```
    ┌───────┐
    │       │
    │   ┌───┴───┐
    │   │       │
┌───┴───┤       │
│       │       │
│       └───────┘
└───────┘
L자형 Room도 가능!
```

---

## 5. Dead Zone & Look Ahead

### 5.1 Dead Zone 이해

**Dead Zone**: 플레이어가 이 영역 안에서 움직여도 카메라가 따라가지 않는 영역

```
게임 느낌에 따른 설정:

액션 게임 (빠른 반응):
├── Dead Zone Width: 0.0 ~ 0.1
└── Dead Zone Height: 0.0 ~ 0.1

탐험 게임 (여유로움):
├── Dead Zone Width: 0.2 ~ 0.3
└── Dead Zone Height: 0.1 ~ 0.2

메트로바니아 권장:
├── Dead Zone Width: 0.1
└── Dead Zone Height: 0.05
```

### 5.2 Look Ahead 이해

**Look Ahead**: 플레이어 이동 방향으로 카메라가 미리 이동

```
Lookahead Time: 0 (없음)
┌──────────────────────────┐
│                          │
│      🎮 →→→              │
│    Player               │
└──────────────────────────┘

Lookahead Time: 0.5 (있음)
┌──────────────────────────┐
│                          │
│  🎮 ───────→  👁️        │
│           카메라 미리 이동│
└──────────────────────────┘
```

**권장 설정**:
```
Lookahead Time: 0.2 ~ 0.4
Lookahead Smoothing: 5 ~ 10
Lookahead Ignore Y: true (수직 점프 시 카메라 흔들림 방지)
```

### 5.3 Damping (감쇠)

카메라가 타겟을 따라가는 속도:

```
Damping 0: 즉시 따라감 (딱딱함)
Damping 0.5: 부드럽게 따라감 (권장)
Damping 2+: 매우 느리게 따라감 (드라마틱)

메트로바니아 권장:
├── X Damping: 0.3 ~ 0.5
└── Y Damping: 0.5 ~ 0.8 (수직은 좀 더 부드럽게)
```

---

## 6. 카메라 전환

### 6.1 Priority 기반 전환

```csharp
// 높은 Priority를 가진 Virtual Camera가 활성화됨

// 기본 카메라 (항상 존재)
vcamPlayer.Priority = 10;

// 보스전 카메라 (보스 등장 시)
vcamBoss.Priority = 20;  // 활성화됨!

// 보스전 종료 시
vcamBoss.Priority = 0;   // 다시 vcamPlayer로
```

### 6.2 게임 오브젝트 활성화/비활성화

```csharp
// 더 간단한 방법: 게임오브젝트로 제어
vcamBoss.gameObject.SetActive(true);   // 활성화
vcamBoss.gameObject.SetActive(false);  // 비활성화
```

### 6.3 커스텀 블렌딩

**Inspector의 Brain 설정**:
```
Custom Blends
├── From: vcamPlayer  To: vcamBoss  → Cut (즉시 전환)
├── From: vcamBoss    To: vcamPlayer → Ease In Out, 1s
└── Default: Ease In Out, 0.5s
```

### 6.4 코드로 블렌딩 제어

```csharp
using Unity.Cinemachine;

public class CameraTransitionController : MonoBehaviour
{
    [SerializeField] private CinemachineCamera playerCam;
    [SerializeField] private CinemachineCamera bossCam;

    public void TransitionToBoss()
    {
        // Priority 방식
        bossCam.Priority = playerCam.Priority + 1;
    }

    public void TransitionToPlayer()
    {
        bossCam.Priority = 0;
    }
}
```

---

## 7. Impulse 시스템 (화면 흔들림)

### 7.1 기본 구조

```
Impulse Source (이벤트 발생지)
        │
        ▼ 신호 전송
Impulse Listener (Virtual Camera)
        │
        ▼ 카메라 흔들림
```

### 7.2 Impulse Source 설정

**피격, 폭발 등의 이벤트에 추가**:

```csharp
using Unity.Cinemachine;
using UnityEngine;

public class DamageImpulseSource : MonoBehaviour
{
    [SerializeField] private CinemachineImpulseSource impulseSource;

    public void TriggerDamageShake(float intensity = 1f)
    {
        // 강도에 따른 흔들림
        impulseSource.GenerateImpulse(intensity);
    }

    public void TriggerDirectionalShake(Vector3 direction)
    {
        // 방향성 있는 흔들림 (피격 방향)
        impulseSource.GenerateImpulse(direction);
    }
}
```

### 7.3 Impulse Listener 설정

Virtual Camera에 Extension으로 추가:

```
CM vcamPlayer
└── Extensions
    └── CinemachineImpulseListener
        ├── Use 2D Distance: true ✓
        ├── Gain: 1 (흔들림 강도 배율)
        └── Channel Mask: Default (필터링)
```

### 7.4 Impulse 프리셋

```csharp
// ImpulsePresets.cs
[CreateAssetMenu(fileName = "ImpulsePreset", menuName = "Camera/Impulse Preset")]
public class ImpulsePreset : ScriptableObject
{
    public NoiseSettings noiseProfile;
    public float amplitude = 0.5f;
    public float frequency = 1f;
    public float duration = 0.3f;
}

// 사용
public void ApplyPreset(ImpulsePreset preset)
{
    impulseSource.ImpulseDefinition.AmplitudeGain = preset.amplitude;
    impulseSource.ImpulseDefinition.FrequencyGain = preset.frequency;
    impulseSource.ImpulseDefinition.TimeEnvelope.SustainTime = preset.duration;
}
```

### 7.5 Noise Profile 커스터마이징

**Assets > Create > Cinemachine > Noise Settings**:

```
Noise Settings
├── Position Noise
│   ├── X: Amplitude 0.3, Frequency 15
│   └── Y: Amplitude 0.3, Frequency 15
└── Rotation Noise
    └── Z: Amplitude 0.5, Frequency 10 (약간의 회전)
```

---

## 8. 보스전 연출

### 8.1 보스 등장 시퀀스

```csharp
using Unity.Cinemachine;
using UnityEngine;

public class BossEncounterCamera : MonoBehaviour
{
    [Header("카메라")]
    [SerializeField] private CinemachineCamera playerCam;
    [SerializeField] private CinemachineCamera bossIntroCam;
    [SerializeField] private CinemachineCamera bossFightCam;

    [Header("타겟")]
    [SerializeField] private Transform bossTransform;
    [SerializeField] private Transform arenaCenter;

    [Header("Impulse")]
    [SerializeField] private CinemachineImpulseSource groundPoundSource;

    public async Awaitable PlayBossIntro()
    {
        // 1. 보스 줌인 (보스를 바라봄)
        bossIntroCam.Follow = bossTransform;
        bossIntroCam.Priority = 100;

        await Awaitable.WaitForSecondsAsync(2f);

        // 2. 보스 착지 흔들림
        groundPoundSource.GenerateImpulse(2f);

        await Awaitable.WaitForSecondsAsync(1f);

        // 3. 전투 카메라로 전환 (아레나 중심)
        bossFightCam.Follow = arenaCenter;
        bossFightCam.Priority = 50;
        bossIntroCam.Priority = 0;

        // 4. 플레이어 카메라로 천천히 복귀
        await Awaitable.WaitForSecondsAsync(1.5f);
        bossFightCam.Priority = 0;
    }
}
```

### 8.2 보스전용 카메라 설정

```
BossFight Virtual Camera
├── Follow: Arena Center (또는 Player+Boss 중간점)
├── Body
│   ├── Dead Zone: 0.3, 0.2 (넓은 영역)
│   ├── Soft Zone: 0.9, 0.8
│   └── Damping: 0.8, 0.8 (느린 추적)
└── Extensions
    └── CinemachineConfiner2D
        └── Bounding Shape: Boss Arena Collider
```

### 8.3 보스 페이즈 전환 연출

```csharp
public async Awaitable PlayPhaseTransition(int phase)
{
    switch (phase)
    {
        case 2:
            // 줌인 + 슬로우모션 느낌
            await ZoomTo(0.7f, 0.5f);

            // 강한 흔들림
            impulseSource.GenerateImpulse(3f);

            // 줌아웃
            await ZoomTo(1f, 0.3f);
            break;

        case 3:
            // 화면 전체 흔들림 + 줌아웃 (위협감)
            await ZoomTo(1.3f, 1f);
            impulseSource.GenerateImpulse(5f);
            break;
    }
}

private async Awaitable ZoomTo(float targetZoom, float duration)
{
    // Lens Ortho Size 조절
    float startSize = bossFightCam.Lens.OrthographicSize;
    float targetSize = startSize * targetZoom;
    float elapsed = 0f;

    while (elapsed < duration)
    {
        elapsed += Time.deltaTime;
        float t = elapsed / duration;
        bossFightCam.Lens.OrthographicSize = Mathf.Lerp(startSize, targetSize, t);
        await Awaitable.NextFrameAsync();
    }
}
```

---

## 9. 컷씬 & Timeline

### 9.1 Timeline 기초

Timeline은 Unity의 시퀀싱 도구로, 컷씬 제작에 필수입니다.

```
Timeline 구조:
┌─────────────────────────────────────────────────┐
│ Timeline Asset                                   │
├─────────────────────────────────────────────────┤
│ Track 1: Cinemachine Track (카메라 전환)        │
│ ├── Shot 1: Intro Camera (0s ~ 2s)              │
│ ├── Shot 2: Boss Camera (2s ~ 5s)               │
│ └── Shot 3: Player Camera (5s ~ 7s)             │
├─────────────────────────────────────────────────┤
│ Track 2: Animation Track (캐릭터 애니메이션)     │
│ └── Boss Intro Animation (0s ~ 5s)              │
├─────────────────────────────────────────────────┤
│ Track 3: Audio Track (사운드)                    │
│ └── Boss Theme Start (3s)                       │
└─────────────────────────────────────────────────┘
```

### 9.2 Cinemachine Track 설정

1. **Timeline 창 열기**: Window > Sequencing > Timeline
2. **Cinemachine Track 추가**: + > Cinemachine Track
3. **Brain 바인딩**: Track에 Brain이 있는 GameObject 연결
4. **Shot 추가**: Track 우클릭 > Add Cinemachine Shot

```
Cinemachine Shot Inspector:
├── Virtual Camera: 사용할 Virtual Camera
├── Ease In Duration: 전환 시작 부드러움
└── Ease Out Duration: 전환 종료 부드러움
```

### 9.3 컷씬 예시: 스토리 인트로

```csharp
using UnityEngine;
using UnityEngine.Playables;

public class IntroCutscene : MonoBehaviour
{
    [SerializeField] private PlayableDirector director;
    [SerializeField] private GameObject playerInputBlocker;

    public async Awaitable PlayIntro()
    {
        // 플레이어 입력 차단
        playerInputBlocker.SetActive(true);

        // 컷씬 재생
        director.Play();

        // 컷씬 종료 대기
        while (director.state == PlayState.Playing)
        {
            await Awaitable.NextFrameAsync();
        }

        // 입력 복구
        playerInputBlocker.SetActive(false);
    }

    public void SkipCutscene()
    {
        director.time = director.duration;
        director.Evaluate();
        director.Stop();
    }
}
```

### 9.4 Signal을 이용한 이벤트 연동

```csharp
// Timeline Signal 수신
using UnityEngine;
using UnityEngine.Timeline;

public class CutsceneSignalReceiver : MonoBehaviour, INotificationReceiver
{
    public void OnNotify(Playable origin, INotification notification, object context)
    {
        if (notification is BossAppearSignal)
        {
            // 보스 등장 이펙트
            SpawnBossEffects();
        }
        else if (notification is CameraShakeSignal shakeSignal)
        {
            // 카메라 흔들림
            TriggerShake(shakeSignal.intensity);
        }
    }
}
```

---

## 10. 기존 시스템과의 통합

### 10.1 CameraEffects 유지

기존의 Post-Processing 효과(CameraEffects)는 그대로 유지합니다.

```
통합 구조:
┌─────────────────────────────────────┐
│           Main Camera               │
├─────────────────────────────────────┤
│ ├── Camera                          │
│ ├── CinemachineBrain (카메라 이동)  │
│ └── Post-Processing 연동            │
├─────────────────────────────────────┤
│ CameraEffects (기존 시스템 유지)    │
│ ├── Bloom 제어                      │
│ ├── Vignette 제어                   │
│ └── ChromaticAberration 제어        │
└─────────────────────────────────────┘
```

### 10.2 CameraManager 역할 변경

기존 CameraManager는 **Cinemachine Wrapper**로 변경합니다.

```csharp
// CameraManager.cs (수정 버전)
using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : SingletonBehaviour<CameraManager>
{
    [Header("Cinemachine")]
    [SerializeField] private CinemachineCamera playerVirtualCamera;
    [SerializeField] private CinemachineImpulseSource impulseSource;

    [Header("Post-Processing")]
    [SerializeField] private CameraEffects cameraEffects;

    // 플레이어 추적 대상 설정
    public void SetFollowTarget(Transform target)
    {
        if (playerVirtualCamera != null)
        {
            playerVirtualCamera.Follow = target;
        }
    }

    // 화면 흔들림 (Cinemachine Impulse 사용)
    public void Shake(float intensity = 1f)
    {
        impulseSource?.GenerateImpulse(intensity);
    }

    // 방향성 흔들림
    public void ShakeDirectional(Vector2 direction, float intensity = 1f)
    {
        impulseSource?.GenerateImpulse(new Vector3(direction.x, direction.y, 0) * intensity);
    }

    // 줌 효과 (Ortho Size 조절)
    public void SetZoom(float zoomMultiplier)
    {
        // zoomMultiplier: 1 = 기본, 0.5 = 2배 확대, 2 = 2배 축소
        if (playerVirtualCamera != null)
        {
            float baseSize = 5f; // 기본 Ortho Size
            playerVirtualCamera.Lens.OrthographicSize = baseSize * zoomMultiplier;
        }
    }

    // Post-Processing 효과 (기존 CameraEffects 활용)
    public void PlayHitEffect(float intensity = 0.5f, float duration = 0.2f)
    {
        cameraEffects?.PlayHitEffect(intensity, duration);
        Shake(intensity);
    }

    public void PlayDeathEffect()
    {
        cameraEffects?.PlayDeathEffect();
    }
}
```

### 10.3 CameraBoundsProvider 마이그레이션

기존 CameraBoundsProvider를 Cinemachine Confiner용으로 변환:

```csharp
// CinemachineRoomConfiner.cs (새로운 버전)
using Unity.Cinemachine;
using UnityEngine;

public class CinemachineRoomConfiner : MonoBehaviour
{
    [SerializeField] private Collider2D roomCollider;
    [SerializeField] private CinemachineConfiner2D confiner;

    [Header("자동 탐색")]
    [SerializeField] private bool autoFindConfiner = true;

    private void Start()
    {
        if (autoFindConfiner && confiner == null)
        {
            // Active Virtual Camera에서 Confiner 찾기 (Cinemachine 3.x API)
            if (CinemachineBrain.ActiveBrainCount > 0)
            {
                var brain = CinemachineBrain.GetActiveBrain(0);
                var vcam = brain?.ActiveVirtualCamera as CinemachineCamera;
                if (vcam != null)
                {
                    confiner = vcam.GetComponent<CinemachineConfiner2D>();
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && confiner != null)
        {
            confiner.BoundingShape2D = roomCollider;
            confiner.InvalidateBoundingShapeCache();
            Debug.Log($"[CinemachineRoomConfiner] 룸 경계 변경: {gameObject.name}");
        }
    }
}
```

---

## 11. 실전 레시피

### 11.1 메트로바니아 기본 설정

```
프로젝트 초기 설정:

1. Main Camera
   └── CinemachineBrain
       ├── Update Method: Smart Update
       └── Default Blend: Ease In Out, 0.5s

2. CM PlayerCamera (Virtual Camera)
   ├── Follow: Player
   ├── Priority: 10
   ├── Body: Position Composer
   │   ├── Lookahead Time: 0.3
   │   ├── Damping X: 0.5, Y: 0.7
   │   ├── Dead Zone: 0.1, 0.05
   │   └── Soft Zone: 0.8, 0.6
   └── Extensions
       ├── CinemachineConfiner2D
       │   └── Bounding Shape: Current Room
       └── CinemachineImpulseListener
           └── Gain: 1

3. Room (각 방마다)
   └── PolygonCollider2D
       └── Is Trigger: true

4. Impulse Source (Player 또는 Global)
   └── CinemachineImpulseSource
       └── Noise Profile: Handheld Normal
```

### 11.2 피격 시 카메라 반응

```csharp
public class PlayerDamageCamera : MonoBehaviour
{
    [SerializeField] private CameraManager cameraManager;

    public void OnDamaged(float damage, Vector2 knockbackDir)
    {
        // 데미지 비례 흔들림
        float shakeIntensity = Mathf.Clamp(damage / 50f, 0.1f, 1f);

        // 방향성 흔들림 (넉백 방향)
        cameraManager.ShakeDirectional(knockbackDir, shakeIntensity);

        // Post-Processing 효과
        cameraManager.PlayHitEffect(shakeIntensity * 0.5f);
    }
}
```

### 11.3 Room 전환 시 카메라

```csharp
public class RoomTransition : MonoBehaviour
{
    [SerializeField] private CinemachineConfiner2D confiner;
    [SerializeField] private Collider2D newRoomBounds;
    [SerializeField] private float transitionDamping = 0.5f;

    public async Awaitable TransitionToRoom()
    {
        // 기존 Damping 저장
        float originalDamping = confiner.Damping;

        // 부드러운 전환을 위해 Damping 증가
        confiner.Damping = transitionDamping;

        // 경계 변경
        confiner.BoundingShape2D = newRoomBounds;
        confiner.InvalidateBoundingShapeCache();

        // 전환 완료 대기
        await Awaitable.WaitForSecondsAsync(0.5f);

        // Damping 복구
        confiner.Damping = originalDamping;
    }
}
```

### 11.4 스킬 사용 시 줌 효과

```csharp
public class SkillCameraEffect : MonoBehaviour
{
    [SerializeField] private CameraManager cameraManager;

    public async Awaitable PlayUltimateSkillCamera()
    {
        // 줌인
        cameraManager.SetZoom(0.8f);

        // 슬로우모션 (선택)
        Time.timeScale = 0.3f;

        await Awaitable.WaitForSecondsAsync(0.5f);

        // 복구
        Time.timeScale = 1f;
        cameraManager.SetZoom(1f);

        // 스킬 임팩트 흔들림
        cameraManager.Shake(1.5f);
    }
}
```

---

## 부록: 트러블슈팅

### Q: 카메라가 떨리는 현상

**원인**: FixedUpdate와 LateUpdate 불일치
**해결**: Brain의 Update Method를 "Smart Update"로 설정

### Q: Confiner 경계에서 튀는 현상

**원인**: Damping이 너무 낮음
**해결**: Confiner의 Damping을 0.3~0.5로 설정

### Q: Impulse가 작동하지 않음

**확인사항**:
1. Virtual Camera에 ImpulseListener 있는지 확인
2. Impulse Source의 Raw Signal이 설정되어 있는지 확인
3. Channel Mask가 일치하는지 확인

### Q: 여러 Virtual Camera 전환 시 깜빡임

**해결**: Brain의 Default Blend 시간을 늘리거나, 전환하기 전 Priority를 먼저 조정

---

## 참고 자료

- [Unity Cinemachine 공식 문서](https://docs.unity3d.com/Packages/com.unity.cinemachine@3.0/)
- [Cinemachine GitHub](https://github.com/Unity-Technologies/com.unity.cinemachine)

---

**다음 단계**:
1. 패키지 설치 후 Basic Setup 따라하기
2. Player Virtual Camera 설정
3. 첫 번째 Room에 Confiner 적용
4. 피격 시 Impulse 테스트

*이 문서는 GASPT 프로젝트 카메라 시스템 학습을 위해 작성되었습니다.*
