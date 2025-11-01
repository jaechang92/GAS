# Quickstart: CharacterPhysics System Completion

**Feature**: 001-character-physics-completion
**Date**: 2025-10-28

## Overview

이 가이드는 CharacterPhysics 시스템의 새로운 기능(벽 점프, 낙하 플랫폼, 스컬별 이동)을 빠르게 테스트하는 방법을 설명합니다.

## Prerequisites

- Unity 2023.3+ 설치
- GASPT 프로젝트 열기
- `001-character-physics-completion` 브랜치 체크아웃

---

## Demo Scene Setup

### 방법 1: 기존 Demo Scene 사용 (권장)

1. `Assets/_Project/Scenes/PhysicsCompletionDemo.unity` 열기
2. Play 버튼 클릭
3. 아래 조작 가이드 참고

### 방법 2: 새 Demo Scene 생성

```
1. 새 씬 생성: File > New Scene
2. 빈 GameObject 생성: "PhysicsDemoRunner"
3. PhysicsCompletionDemo.cs 컴포넌트 추가
4. Play 모드 진입 → 자동 설정
```

---

## 조작 가이드

### 기본 조작
| 키 | 동작 |
|----|------|
| **WASD** | 이동 |
| **Space** | 점프 |
| **LShift** | 대시 |
| **마우스 좌클릭** | 공격 |

### 벽 점프 조작 ⭐ NEW
| 키 | 동작 |
|----|------|
| **A/D + Space** | 벽에 닿은 상태에서 점프 → 벽 점프 |
| **A/D (hold)** | 벽에 닿은 상태에서 유지 → 벽 슬라이딩 |
| **반대 방향 키** | 벽에서 떨어지기 |

### 낙하 플랫폼 조작 ⭐ NEW
| 키 | 동작 |
|----|------|
| **S + Space** | 플랫폼 위에서 아래 방향 + 점프 → 플랫폼 통과 |
| **Space** | 플랫폼 아래에서 위로 점프 → 자유 통과 |

### 스컬 변경 조작 ⭐ NEW
| 키 | 동작 |
|----|------|
| **1** | 기본 스컬 (균형잡힌 이동) |
| **2** | 전사 스컬 (묵직하고 강력) |
| **3** | 마법사 스컬 (가볍고 민첩) |

### 디버그 조작
| 키 | 동작 |
|----|------|
| **R** | 씬 리셋 |
| **F12** | 디버그 정보 토글 |
| **G** | Gizmos 토글 |

---

## Test Scenarios

### 시나리오 1: 벽 점프 수직 통로 (P1)

**목표**: 양쪽 벽을 번갈아 점프하며 15 유닛 높이까지 상승

**Setup**:
```
수직 통로 구조:
│     │  ← 15 유닛 높이
│     │
│     │
│  ▣  │  ← 플레이어 시작 위치
└─────┘
```

**Steps**:
1. 수직 통로 입구에 플레이어 배치
2. **D 키** 누른 상태에서 **Space** → 오른쪽 벽 슬라이딩
3. **Space** 다시 누르기 → 왼쪽으로 벽 점프
4. **A 키** 누른 상태에서 **Space** → 왼쪽 벽 슬라이딩
5. 반복하여 최상단 도달

**Success Criteria**:
- ✅ 벽 슬라이딩 속도 < 30% 낙하 속도
- ✅ 5초 이내 최상단 도달
- ✅ 연속 벽 점프 안정성 (프레임 드롭 없음)

---

### 시나리오 2: 낙하 플랫폼 3층 구조 (P2)

**목표**: 아래 방향 + 점프로 각 층을 통과하여 바닥 도달

**Setup**:
```
─────────  ← 3층
   ▣       ← 플레이어 시작
─────────  ← 2층

─────────  ← 1층

═════════  ← 바닥 (Solid)
```

**Steps**:
1. 최상층(3층)에 플레이어 배치
2. **S + Space** 동시 입력 → 2층 통과
3. **S + Space** 다시 입력 → 1층 통과
4. 바닥에 착지

**Success Criteria**:
- ✅ 2초 이내 바닥 도달
- ✅ 각 플랫폼 통과 성공률 100%
- ✅ 아래에서 위로 점프 시 각 층 착지 가능

**Reverse Test**:
1. 바닥에서 **Space**로 점프
2. 각 플랫폼에 정상 착지 확인

---

### 시나리오 3: 스컬별 이동 차이 체감 (P3)

**목표**: 3가지 스컬 간 이동 특성 차이 확인

**Setup**:
```
────────┐      장애물 코스:
        │      - 긴 점프 구간
  ▣     │      - 좁은 통로
────────┘      - 공중 방향 전환
```

**Steps**:
1. **1번** 키로 기본 스컬 선택
   - 장애물 코스 통과하며 시간 측정
2. **2번** 키로 전사 스컬 선택
   - 동일한 코스 통과 (묵직한 느낌)
3. **3번** 키로 마법사 스컬 선택
   - 동일한 코스 통과 (가볍고 민첩)

**Success Criteria**:
- ✅ 스컬 간 이동 시간 차이 15% 이상
- ✅ 스컬 변경 후 0.1초 이내 특성 적용
- ✅ 공중 제어력 차이 체감

**특성 비교**:
| 스컬 | 이동 속도 | 점프 높이 | 공중 제어 |
|------|-----------|-----------|-----------|
| 기본 | 100% | 100% | 100% |
| 전사 | 90% | 85% | 80% |
| 마법사 | 115% | 110% | 125% |

---

## Edge Case Tests

### 엣지 케이스 1: 벽 점프 연속 실행
```
Input: 좌우 벽 사이에서 무한 벽 점프 반복
Expected: 시스템 안정성 유지, 60 FPS 이상
```

### 엣지 케이스 2: 벽 슬라이딩 중 대시
```
Input: 벽 슬라이딩 중 Shift (대시) 입력
Expected: 벽에서 떨어지며 대시 수행
```

### 엣지 케이스 3: 플랫폼 통과 직후 재점프
```
Input: 플랫폼 통과 직후 (0.3초 이내) 위로 점프
Expected: 플랫폼 관통 (쿨다운 유지)
```

### 엣지 케이스 4: 공중 스컬 변경
```
Input: 공중에서 스컬 변경
Expected: 현재 속도가 새 배율로 재조정
```

---

## Debugging

### Gizmos 정보 (G 키로 토글)

**벽 감지** (노란색 선):
- 좌우 BoxCast 범위 표시
- 벽 감지 시 빨간색으로 변경

**플랫폼 쿨다운** (파란색 원):
- 쿨다운 중인 플랫폼 표시
- 크기 = 남은 쿨다운 시간 비율

**스컬 프로필** (텍스트):
- 현재 적용 중인 배율 표시

### 디버그 로그 (F12 키로 토글)

```
[CharacterPhysics] Wall Slide Started: Left
[CharacterPhysics] Wall Jump: Left → Right (H: 12.0, V: 12.75)
[OneWayPlatform] Passthrough Requested: Player
[CharacterPhysics] Skull Profile Applied: Warrior (0.9, 0.85, 0.8)
```

---

## Performance Monitoring

### Unity Profiler 사용

1. **Window > Analysis > Profiler** 열기
2. Play 모드에서 테스트 수행
3. **CPU Usage** 탭 확인

**예상 성능**:
- CharacterPhysics.FixedUpdate: < 0.5ms
- Wall Detection (BoxCast): < 0.1ms
- Platform Cooldown Update: < 0.05ms
- Total Physics: < 1ms

---

## Troubleshooting

### 문제 1: 벽 점프가 동작하지 않음
**원인**: Ground Layer 설정 누락
**해결**:
1. Edit > Project Settings > Tags and Layers
2. Layer 8 = "Ground" 확인
3. 벽 오브젝트의 Layer = Ground

### 문제 2: 플랫폼 통과 안됨
**원인**: OneWayPlatform 컴포넌트 미부착
**해결**:
1. 플랫폼 GameObject 선택
2. Add Component > OneWayPlatform
3. Platform Type = OneWay 설정

### 문제 3: 스컬 변경 시 이동 특성 미적용
**원인**: SkullMovementProfile 미할당
**해결**:
1. Assets/Data/Physics/ 폴더에 프로필 생성
2. CharacterPhysics Inspector에서 Default Profile 할당

### 문제 4: 한글 깨짐
**원인**: UTF-8 인코딩 문제
**해결**:
- .gitattributes 및 .editorconfig 확인
- 파일을 UTF-8로 다시 저장

---

## Next Steps

테스트 완료 후:
1. ✅ Success Criteria 충족 확인
2. ✅ Edge Cases 검증
3. ✅ 버그 리포트 작성 (발견 시)
4. ⏳ `/speckit.tasks` 명령으로 구현 작업 생성
5. ⏳ 실제 게임 레벨에 통합

---

## Support

**문제 발생 시**:
1. Console 로그 확인 (Ctrl+Shift+C)
2. Profiler에서 성능 병목 확인
3. Gizmos로 물리 상태 시각화
4. Demo Scene 리셋 후 재시도

**문서 참조**:
- spec.md - 기능 명세
- research.md - 기술 결정사항
- data-model.md - 데이터 구조
- contracts/ - API 문서
