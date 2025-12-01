# 타일맵 에셋 기획서

> **프로젝트**: GASPT (마법사 테마 2D 플랫포머 액션 로그라이트)
> **작성일**: 2025-11-30
> **목적**: Unity Tilemap 시스템용 타일셋 에셋 기획

---

## 목차

1. [타일맵 시스템 개요](#1-타일맵-시스템-개요)
2. [스테이지별 타일셋](#2-스테이지별-타일셋)
3. [공용 타일셋](#3-공용-타일셋)
4. [타일 규격 및 제작 가이드](#4-타일-규격-및-제작-가이드)
5. [아트 프롬프트](#5-아트-프롬프트)

---

# 1. 타일맵 시스템 개요

## 1.1 Unity Tilemap 구조

```
Tilemap Hierarchy
├── Background Layer (배경 장식)
├── Ground Layer (바닥/벽)
├── Platform Layer (플랫폼)
├── Decoration Layer (전경 장식)
└── Collision Layer (충돌 전용, 투명)
```

## 1.2 타일 크기 규격

| 타입 | 크기 | 용도 |
|------|------|------|
| 기본 타일 | 32x32 px | 바닥, 벽, 플랫폼 |
| 세부 타일 | 16x16 px | 작은 장식, 디테일 |
| 대형 타일 | 64x64 px | 특수 오브젝트, 대형 장식 |

## 1.3 타일 종류

### 지형 타일 (Terrain Tiles)
- **Ground**: 바닥 타일 (9-slice Rule Tile)
- **Wall**: 벽 타일 (상하좌우 연결)
- **Platform**: 점프 가능 플랫폼 (통과 가능)
- **Slope**: 경사면 (45도, 22.5도)

### 특수 타일 (Special Tiles)
- **Hazard**: 위험 지역 (독, 용암, 가시)
- **Interactive**: 상호작용 (레버, 버튼)
- **Animated**: 애니메이션 (물, 불, 독)
- **Breakable**: 파괴 가능 지형

### 장식 타일 (Decoration Tiles)
- **Background Props**: 배경 소품
- **Foreground Props**: 전경 소품
- **Ambient**: 분위기 요소 (안개, 입자)

---

# 2. 스테이지별 타일셋

## 2.1 Stage 1: 숲 (Forest)

### 테마 컨셉
- **분위기**: 신비로운 마법의 숲, 약간 어두운 분위기
- **색상**: 녹색/갈색 기반, 보라/파랑 마법 포인트
- **특징**: 거대 나무, 마법 버섯, 반딧불이

### 타일 구성

#### 지형 타일 (47개 Rule Tile 세트)

| 카테고리 | 타일 | 설명 |
|----------|------|------|
| 흙 바닥 | 9-slice + 변형 4종 | 풀이 자란 흙 바닥 |
| 돌 바닥 | 9-slice + 변형 4종 | 이끼 낀 돌 바닥 |
| 나무 플랫폼 | 좌/중/우 | 나뭇가지 플랫폼 |
| 버섯 플랫폼 | 좌/중/우 | 거대 버섯 플랫폼 |
| 뿌리 플랫폼 | 좌/중/우 | 나무 뿌리 플랫폼 |
| 경사면 | 45도 좌/우 | 흙 경사 |

#### 벽 타일 (13개)

| 타일 | 설명 |
|------|------|
| 흙 벽 | 좌/중/우 연결 |
| 돌 벽 | 좌/중/우 연결 |
| 뿌리 벽 | 장식용 |
| 동굴 입구 | 64x64 특수 |

#### 특수 타일 (8개)

| 타일 | 크기 | 애니메이션 | 설명 |
|------|------|-----------|------|
| 독 웅덩이 | 32x32 | 4프레임 | 데미지 지역 |
| 가시덤불 | 32x32 | - | 데미지 지역 |
| 탄성 버섯 | 32x32 | 3프레임 | 점프대 |
| 무너지는 나무 | 64x32 | - | 시간제 플랫폼 |
| 이동 연잎 | 32x32 | - | 이동 플랫폼 |
| 물 | 32x32 | 4프레임 | 장식/위험 |
| 반딧불이 | 16x16 | 3프레임 | 장식 |
| 마법 발광 | 16x16 | 3프레임 | 장식 |

#### 장식 타일 (20개+)

| 카테고리 | 타일 목록 |
|----------|----------|
| 나무 | 거대 나무, 중간 나무, 작은 나무, 그루터기 |
| 식물 | 풀(3종), 꽃(3종), 덤불(2종), 이끼 |
| 버섯 | 발광 버섯(3색), 일반 버섯(2종) |
| 돌 | 바위(3크기), 조약돌 |
| 기타 | 낙엽 더미, 쓰러진 나무, 부서진 카트 |

### 컬러 팔레트

| 요소 | 색상 코드 | 설명 |
|------|----------|------|
| 풀/잎 (밝음) | #4CAF50 | 라이트 그린 |
| 풀/잎 (어두움) | #2E7D32 | 다크 그린 |
| 흙 | #795548 | 브라운 |
| 나무껍질 | #5D4037 | 다크 브라운 |
| 돌 | #607D8B | 블루 그레이 |
| 마법 발광 | #9C27B0 | 퍼플 |
| 버섯 발광 | #00BCD4 | 시안 |

---

## 2.2 Stage 2: 동굴 (Cave)

### 테마 컨셉
- **분위기**: 어둡고 습한 지하 동굴, 결정/광석
- **색상**: 회색/파란색 기반, 결정 발광 포인트
- **특징**: 종유석/석순, 결정, 지하 호수

### 타일 구성

#### 지형 타일 (47개 Rule Tile 세트)

| 카테고리 | 타일 | 설명 |
|----------|------|------|
| 암석 바닥 | 9-slice + 변형 4종 | 거친 암석 |
| 물기 있는 바닥 | 9-slice + 변형 4종 | 습한 바닥 |
| 암석 플랫폼 | 좌/중/우 | 돌출 암석 |
| 결정 플랫폼 | 좌/중/우 | 결정 위 |
| 목재 다리 | 좌/중/우 | 가설 다리 |
| 경사면 | 45도 좌/우 | 암석 경사 |

#### 벽 타일 (15개)

| 타일 | 설명 |
|------|------|
| 암석 벽 | 좌/중/우 연결 |
| 결정 벽 | 발광하는 결정 포함 |
| 종유석 | 천장 장식 (3크기) |
| 석순 | 바닥 장식 (3크기) |
| 균열 벽 | 부서지는 벽 |

#### 특수 타일 (10개)

| 타일 | 크기 | 애니메이션 | 설명 |
|------|------|-----------|------|
| 용암 | 32x32 | 4프레임 | 데미지 지역 |
| 용암 분출 | 32x64 | 6프레임 | 간헐 데미지 |
| 물 표면 | 32x32 | 4프레임 | 장식 |
| 물 속 | 32x32 | 2프레임 | 물 영역 |
| 무너지는 바닥 | 32x32 | - | 시간제 |
| 이동 플랫폼 | 32x32 | - | 광산 카트 |
| 결정 발광 | 16x16 | 3프레임 | 장식 |
| 박쥐 | 16x16 | 4프레임 | 장식 |
| 가스 분출 | 32x32 | 4프레임 | 위험 |
| 낙석 | 32x32 | - | 트리거 함정 |

#### 장식 타일 (18개+)

| 카테고리 | 타일 목록 |
|----------|----------|
| 결정 | 파랑(3크기), 보라(3크기), 녹색(2크기) |
| 광석 | 금광, 은광, 구리광, 마석 |
| 암석 | 바위(3크기), 자갈 더미 |
| 기타 | 해골, 광산 장비, 사다리, 밧줄 |

### 컬러 팔레트

| 요소 | 색상 코드 | 설명 |
|------|----------|------|
| 암석 (밝음) | #78909C | 블루 그레이 |
| 암석 (어두움) | #455A64 | 다크 블루 그레이 |
| 결정 (파랑) | #03A9F4 | 라이트 블루 |
| 결정 (보라) | #7C4DFF | 디프 퍼플 |
| 용암 | #FF5722 | 딥 오렌지 |
| 물 | #0288D1 | 라이트 블루 |
| 이끼 | #558B2F | 올리브 그린 |

---

## 2.3 Stage 3: 유적 (Ruins)

### 테마 컨셉
- **분위기**: 고대 마법 문명의 유적, 신비로움
- **색상**: 베이지/금색 기반, 마법 룬 발광
- **특징**: 석조 건축, 마법 룬, 함정

### 타일 구성

#### 지형 타일 (47개 Rule Tile 세트)

| 카테고리 | 타일 | 설명 |
|----------|------|------|
| 석재 바닥 | 9-slice + 변형 4종 | 오래된 석재 |
| 균열 바닥 | 9-slice + 변형 4종 | 금 간 바닥 |
| 석재 플랫폼 | 좌/중/우 | 돌출 석재 |
| 기둥 플랫폼 | 좌/중/우 | 무너진 기둥 |
| 공중 플랫폼 | 좌/중/우 | 마법 부양 |
| 경사면 | 45도 좌/우 | 계단식 경사 |

#### 벽 타일 (17개)

| 타일 | 설명 |
|------|------|
| 석재 벽 | 좌/중/우 연결 |
| 문양 벽 | 마법 문양 새김 |
| 기둥 | 온전한/부서진 (각 2종) |
| 아치 | 상단 아치 (3분할) |
| 창문 | 장식 창문 (2종) |
| 석상 | 수호자 석상 (2종) |

#### 특수 타일 (12개)

| 타일 | 크기 | 애니메이션 | 설명 |
|------|------|-----------|------|
| 마법 함정 | 32x32 | 4프레임 | 데미지 지역 |
| 화살 발사 | 32x32 | 3프레임 | 벽 함정 |
| 바닥 가시 | 32x32 | 2프레임 | 숨겨진 가시 |
| 마법 장벽 | 32x64 | 4프레임 | 차단/해제 |
| 순간이동 | 32x32 | 4프레임 | 포탈 페어 |
| 압력판 | 32x32 | 2프레임 | 스위치 |
| 무너지는 바닥 | 32x32 | - | 시간제 |
| 이동 플랫폼 | 32x32 | - | 마법 플랫폼 |
| 룬 발광 | 16x16 | 3프레임 | 장식 |
| 마법 먼지 | 16x16 | 4프레임 | 입자 |
| 모래 | 32x32 | 2프레임 | 슬로우 지역 |
| 흐르는 모래 | 32x32 | 4프레임 | 위험 지역 |

#### 장식 타일 (22개+)

| 카테고리 | 타일 목록 |
|----------|----------|
| 건축 | 부서진 기둥, 아치 조각, 석재 블록 |
| 문양 | 벽 룬(4종), 바닥 룬(4종) |
| 식물 | 덩굴, 이끼, 마른 풀 |
| 유물 | 항아리(3종), 석관, 보물상자 |
| 기타 | 모래 더미, 거미줄, 뼈 |

### 컬러 팔레트

| 요소 | 색상 코드 | 설명 |
|------|----------|------|
| 석재 (밝음) | #D7CCC8 | 라이트 브라운 |
| 석재 (어두움) | #8D6E63 | 브라운 |
| 금 장식 | #FFD54F | 앰버 |
| 마법 룬 | #00E5FF | 시안 |
| 마법 발광 | #EA80FC | 핑크 |
| 모래 | #FFE082 | 앰버 |
| 이끼 | #689F38 | 라이트 그린 |

---

## 2.4 Stage 4: 성 (Castle)

### 테마 컨셉
- **분위기**: 어둡고 위협적인 마왕성
- **색상**: 검정/보라/붉은색 기반
- **특징**: 고딕 건축, 암흑 마법, 피

### 타일 구성

#### 지형 타일 (47개 Rule Tile 세트)

| 카테고리 | 타일 | 설명 |
|----------|------|------|
| 석재 바닥 | 9-slice + 변형 4종 | 검은 석재 |
| 카펫 바닥 | 9-slice + 변형 4종 | 붉은 카펫 |
| 석재 플랫폼 | 좌/중/우 | 돌출 석재 |
| 철제 플랫폼 | 좌/중/우 | 격자 철판 |
| 사슬 플랫폼 | 좌/중/우 | 매달린 발판 |
| 경사면 | 45도 좌/우 | 계단 |

#### 벽 타일 (18개)

| 타일 | 설명 |
|------|------|
| 벽돌 벽 | 좌/중/우 연결 |
| 장식 벽 | 문양/조각 포함 |
| 창문 | 고딕 창문 (3종) |
| 기둥 | 장식 기둥 (2종) |
| 아치 | 고딕 아치 (3분할) |
| 철창 | 감옥 창살 |
| 문 | 나무문, 철문 |

#### 특수 타일 (14개)

| 타일 | 크기 | 애니메이션 | 설명 |
|------|------|-----------|------|
| 스파이크 | 32x32 | 2프레임 | 숨겨진 가시 |
| 창 함정 | 32x64 | 3프레임 | 벽 창 |
| 회전 톱 | 32x32 | 4프레임 | 이동 함정 |
| 암흑 균열 | 32x32 | 4프레임 | 데미지 지역 |
| 화염 분출 | 32x64 | 6프레임 | 간헐 화염 |
| 전기 함정 | 32x32 | 4프레임 | 전기 장벽 |
| 무너지는 바닥 | 32x32 | - | 시간제 |
| 엘리베이터 | 32x32 | - | 수직 이동 |
| 횃불 | 16x32 | 4프레임 | 조명 |
| 촛대 | 32x64 | 3프레임 | 조명 |
| 암흑 입자 | 16x16 | 4프레임 | 장식 |
| 피 웅덩이 | 32x32 | 2프레임 | 장식 |
| 사슬 | 16x64 | - | 장식 |
| 거미줄 | 32x32 | - | 장식/슬로우 |

#### 장식 타일 (25개+)

| 카테고리 | 타일 목록 |
|----------|----------|
| 건축 | 기둥, 아치, 창문 프레임, 난간 |
| 가구 | 테이블, 의자, 왕좌, 책장 |
| 조명 | 횃불, 촛대, 샹들리에 |
| 장식 | 깃발, 태피스트리, 갑옷, 무기 진열 |
| 음침 | 해골, 관, 사슬, 철창, 고문 도구 |

### 컬러 팔레트

| 요소 | 색상 코드 | 설명 |
|------|----------|------|
| 석재 (밝음) | #424242 | 그레이 |
| 석재 (어두움) | #212121 | 다크 그레이 |
| 카펫/천 | #B71C1C | 다크 레드 |
| 금 장식 | #FFC107 | 앰버 |
| 암흑 마법 | #7B1FA2 | 퍼플 |
| 피 | #C62828 | 레드 |
| 횃불 빛 | #FF9800 | 오렌지 |

---

## 2.5 Stage 5: 보스 영역 (Boss Arena)

### 테마 컨셉
- **분위기**: 최종 결전 무대, 극적인 분위기
- **색상**: 검정/보라/금색, 강렬한 대비
- **특징**: 원형 아레나, 마법진, 차원 균열

### 타일 구성

#### 지형 타일 (30개)

| 카테고리 | 타일 | 설명 |
|----------|------|------|
| 아레나 바닥 | 원형 패턴, 중앙 마법진 | 특수 디자인 |
| 테두리 | 외곽 장식 | 금/보라 장식 |
| 부유 플랫폼 | 3크기 | 원형 플랫폼 |
| 기둥 | 대형 기둥 (4개) | 보스전 장식 |

#### 특수 타일 (8개)

| 타일 | 크기 | 애니메이션 | 설명 |
|------|------|-----------|------|
| 중앙 마법진 | 128x128 | 8프레임 | 보스 소환 |
| 차원 균열 | 64x64 | 6프레임 | 페이즈 변환 |
| 에너지 장벽 | 32x64 | 4프레임 | 영역 제한 |
| 암흑 폭발 | 64x64 | 8프레임 | 보스 공격 |
| 소환진 | 64x64 | 4프레임 | 졸개 소환 |
| 회복 지점 | 32x32 | 4프레임 | 힐 존 |
| 위험 지역 | 32x32 | 4프레임 | 데미지 존 |
| 먼지/입자 | 16x16 | 4프레임 | 분위기 |

### 컬러 팔레트

| 요소 | 색상 코드 | 설명 |
|------|----------|------|
| 바닥 | #1A1A2E | 다크 네이비 |
| 금 장식 | #FFD700 | 골드 |
| 마법진 | #E040FB | 핑크 퍼플 |
| 차원 균열 | #AA00FF | 퍼플 |
| 에너지 | #00E5FF | 시안 |

---

# 3. 공용 타일셋

## 3.1 시작방/로비 (Start Room)

### 타일 구성 (35개)

| 카테고리 | 타일 | 설명 |
|----------|------|------|
| 바닥 | 나무 바닥 (9-slice) | 따뜻한 분위기 |
| 벽 | 목조 벽 | 아늑한 실내 |
| 창문 | 마법 창문 | 빛 효과 |
| 문 | 포탈 문 (3종) | 스테이지 선택 |
| 장식 | 책장, 탁자, 화분 등 | 마법사 집 |

## 3.2 상점/휴식방

### 타일 구성 (25개)

| 카테고리 | 타일 | 설명 |
|----------|------|------|
| 바닥/벽 | 상점 테마 | 각 상점별 디자인 |
| 진열대 | 아이템 진열대 | 상호작용 오브젝트 |
| 카운터 | NPC 위치 | 거래 영역 |
| 장식 | 상점별 특화 | 무기/포션/마법서 |

## 3.3 포탈/문

### 공용 포탈 (8종)

| 타일 | 크기 | 애니메이션 | 용도 |
|------|------|-----------|------|
| 일반 포탈 | 64x96 | 6프레임 | 다음 방 이동 |
| 보스 포탈 | 64x96 | 6프레임 | 보스방 입장 |
| 비밀 포탈 | 64x96 | 6프레임 | 비밀방 |
| 탈출 포탈 | 64x96 | 6프레임 | 스테이지 클리어 |
| 귀환 포탈 | 64x96 | 6프레임 | 마을 귀환 |
| 상점 문 | 64x96 | 4프레임 | 상점 입장 |
| 보물 문 | 64x96 | 4프레임 | 보물방 |
| 잠긴 문 | 64x96 | 2프레임 | 열쇠 필요 |

---

# 4. 타일 규격 및 제작 가이드

## 4.1 Rule Tile 구조 (47 타일)

```
Standard Rule Tile Layout (47 tiles):

[01][02][03]  - 상단 (좌모서리/상단/우모서리)
[04][05][06]  - 중단 (좌측/중앙/우측)
[07][08][09]  - 하단 (좌모서리/하단/우모서리)

[10][11][12]  - 내부 모서리 (4방향)
[13]

[14-17]       - 단독 타일 변형
[18-21]       - 수평 연결 (끝/중간)
[22-25]       - 수직 연결 (끝/중간)

[26-33]       - T자/십자 연결
[34-41]       - 복잡한 연결 케이스
[42-47]       - 변형/랜덤 타일
```

## 4.2 스프라이트 시트 레이아웃

```
권장 시트 크기: 512x512 또는 1024x1024

32x32 타일 기준:
- 512x512 = 16x16 = 256 타일
- 1024x1024 = 32x32 = 1024 타일

시트 구성:
┌─────────────────────────────────┐
│  지형 타일 (상단 절반)           │
│  - Rule Tile 세트               │
│  - 변형 타일                    │
├─────────────────────────────────┤
│  특수/장식 타일 (하단 절반)      │
│  - 애니메이션 프레임            │
│  - 장식 오브젝트                │
└─────────────────────────────────┘
```

## 4.3 애니메이션 타일 규격

| 타입 | 프레임 수 | 속도 (fps) |
|------|----------|------------|
| 물/용암 | 4 | 6 |
| 불꽃/횃불 | 4-6 | 8 |
| 마법 효과 | 4-8 | 10 |
| 입자/반짝임 | 3-4 | 12 |
| 함정 작동 | 2-4 | 4-8 |

## 4.4 파일 명명 규칙

```
{스테이지}_{카테고리}_{이름}_{상태/변형}.png

예시:
- forest_ground_dirt_01.png
- forest_platform_mushroom_left.png
- cave_special_lava_anim.png
- castle_deco_torch_lit.png
```

---

# 5. 아트 프롬프트

## 5.1 공통 프롬프트 구조

```
[스타일] 2D pixel art style, game tileset, {테마} theme
[크기] {크기}x{크기} pixel tiles, {시트 구성}
[규격] 9-slice tileable, seamless edges
[색상] {팔레트 설명}
[배경] transparent background
[품질] clean pixels, no anti-aliasing, game-ready asset
```

---

## 5.2 Stage 1: 숲 타일셋 프롬프트

### 지형 타일셋 (Ground/Platform)

**한글**:
```
2D 픽셀아트 스타일, 마법의 숲 테마 게임 타일셋,
32x32 픽셀 타일, 512x512 스프라이트 시트,

포함 타일:
1. 풀이 자란 흙 바닥 - 9-slice 연결 타일 (위/양옆/모서리)
2. 이끼 낀 돌 바닥 - 9-slice 연결 타일
3. 나뭇가지 플랫폼 - 좌측 끝/중간/우측 끝
4. 거대 버섯 플랫폼 - 좌측 끝/중간/우측 끝
5. 나무 뿌리 플랫폼 - 좌측 끝/중간/우측 끝
6. 45도 경사면 - 좌향/우향

색상: 풀(#4CAF50, #2E7D32), 흙(#795548), 돌(#607D8B)
심리스 연결 가능, 투명 배경, 클린 픽셀, 안티앨리어싱 없음
```

**영문**:
```
2D pixel art style, magical forest theme game tileset,
32x32 pixel tiles, 512x512 sprite sheet,

Included tiles:
1. Grassy dirt ground - 9-slice connected tiles (top/sides/corners)
2. Mossy stone ground - 9-slice connected tiles
3. Tree branch platform - left end/middle/right end
4. Giant mushroom platform - left end/middle/right end
5. Tree root platform - left end/middle/right end
6. 45-degree slopes - left/right facing

Colors: grass(#4CAF50, #2E7D32), dirt(#795548), stone(#607D8B)
Seamless tileable, transparent background, clean pixels, no anti-aliasing
```

---

### 특수 타일 (Hazards/Interactive)

**한글**:
```
2D 픽셀아트 스타일, 마법의 숲 특수 타일,
32x32 픽셀, 애니메이션 프레임 포함,

포함 타일:
1. 독 웅덩이 - 4프레임 부글거림 애니메이션
2. 가시 덤불 - 위험 표시, 날카로운 가시
3. 탄성 버섯 - 3프레임 튕김 애니메이션
4. 무너지는 나무 - 금 간 상태, 64x32 크기
5. 이동 연잎 - 물 위 떠있는 플랫폼
6. 물 타일 - 4프레임 일렁임

색상: 독(#8BC34A 발광), 물(#2196F3), 버섯(#00BCD4 발광)
투명 배경, 클린 픽셀
```

**영문**:
```
2D pixel art style, magical forest special tiles,
32x32 pixels, animation frames included,

Included tiles:
1. Poison puddle - 4-frame bubbling animation
2. Thorn bush - danger indicator, sharp thorns
3. Bouncy mushroom - 3-frame bouncing animation
4. Crumbling log - cracked state, 64x32 size
5. Moving lily pad - floating platform on water
6. Water tile - 4-frame ripple animation

Colors: poison(#8BC34A glowing), water(#2196F3), mushroom(#00BCD4 glowing)
Transparent background, clean pixels
```

---

### 장식 타일 (Decorations)

**한글**:
```
2D 픽셀아트 스타일, 마법의 숲 장식 타일,
다양한 크기 (16x16 ~ 64x64),

포함 오브젝트:
나무: 거대 나무(64x64), 중간 나무(32x48), 작은 나무(32x32), 그루터기
식물: 풀(3종, 16x16), 꽃(3종, 16x16), 덤불(2종, 32x32)
버섯: 발광 버섯(파랑/보라/녹색, 16x16), 일반 버섯(2종)
돌: 바위(대/중/소), 조약돌(16x16)
기타: 낙엽 더미, 쓰러진 통나무, 부서진 수레

신비로운 마법 숲 분위기, 반딧불이 효과
투명 배경, 레이어링 가능
```

**영문**:
```
2D pixel art style, magical forest decoration tiles,
Various sizes (16x16 to 64x64),

Included objects:
Trees: giant tree(64x64), medium tree(32x48), small tree(32x32), stump
Plants: grass(3 types, 16x16), flowers(3 types, 16x16), bushes(2 types, 32x32)
Mushrooms: glowing mushrooms(blue/purple/green, 16x16), normal mushrooms(2 types)
Rocks: boulders(large/medium/small), pebbles(16x16)
Other: leaf pile, fallen log, broken cart

Mystical magical forest atmosphere, firefly effects
Transparent background, layerable
```

---

## 5.3 Stage 2: 동굴 타일셋 프롬프트

### 지형 타일셋

**한글**:
```
2D 픽셀아트 스타일, 크리스탈 동굴 테마 게임 타일셋,
32x32 픽셀 타일, 512x512 스프라이트 시트,

포함 타일:
1. 거친 암석 바닥 - 9-slice 연결 타일
2. 습한 바닥 (물기) - 9-slice 연결 타일
3. 암석 돌출 플랫폼 - 좌측 끝/중간/우측 끝
4. 크리스탈 플랫폼 - 발광 효과, 좌/중/우
5. 목재 다리 플랫폼 - 광산 스타일
6. 45도 경사면

색상: 암석(#78909C, #455A64), 크리스탈(#03A9F4, #7C4DFF)
종유석/석순 포함, 심리스 연결, 투명 배경
```

**영문**:
```
2D pixel art style, crystal cave theme game tileset,
32x32 pixel tiles, 512x512 sprite sheet,

Included tiles:
1. Rough rock ground - 9-slice connected tiles
2. Wet ground (moisture) - 9-slice connected tiles
3. Rocky outcrop platform - left end/middle/right end
4. Crystal platform - glowing effect, left/center/right
5. Wooden bridge platform - mine style
6. 45-degree slopes

Colors: rock(#78909C, #455A64), crystal(#03A9F4, #7C4DFF)
Stalactites/stalagmites included, seamless tileable, transparent background
```

---

### 특수 타일

**한글**:
```
2D 픽셀아트 스타일, 동굴 특수 타일,
32x32 픽셀, 애니메이션 프레임 포함,

포함 타일:
1. 용암 - 4프레임 흐름 애니메이션, 발광 효과
2. 용암 분출구 - 6프레임 간헐 분출, 32x64
3. 지하수 표면 - 4프레임 일렁임
4. 물 속 - 2프레임 어두운 물
5. 무너지는 바닥 - 금 간 암석
6. 광산 카트 이동 플랫폼
7. 크리스탈 발광 - 3프레임 빛남, 16x16
8. 가스 분출 - 4프레임 녹색 가스

색상: 용암(#FF5722, #FF9800), 물(#0288D1), 가스(#8BC34A)
```

**영문**:
```
2D pixel art style, cave special tiles,
32x32 pixels, animation frames included,

Included tiles:
1. Lava - 4-frame flow animation, glowing effect
2. Lava geyser - 6-frame intermittent burst, 32x64
3. Underground water surface - 4-frame ripple
4. Underwater - 2-frame dark water
5. Crumbling floor - cracked rock
6. Mine cart moving platform
7. Crystal glow - 3-frame shimmer, 16x16
8. Gas vent - 4-frame green gas

Colors: lava(#FF5722, #FF9800), water(#0288D1), gas(#8BC34A)
```

---

## 5.4 Stage 3: 유적 타일셋 프롬프트

### 지형 타일셋

**한글**:
```
2D 픽셀아트 스타일, 고대 마법 유적 테마 게임 타일셋,
32x32 픽셀 타일, 512x512 스프라이트 시트,

포함 타일:
1. 고대 석재 바닥 - 9-slice 연결, 마법 문양 장식
2. 균열된 바닥 - 9-slice, 금 가고 부서진 상태
3. 석재 플랫폼 - 조각된 장식, 좌/중/우
4. 무너진 기둥 플랫폼 - 좌/중/우
5. 마법 부유 플랫폼 - 발광 효과, 좌/중/우
6. 계단식 경사면 - 45도

색상: 석재(#D7CCC8, #8D6E63), 금 장식(#FFD54F), 마법(#00E5FF)
고대 이집트/그리스 느낌 혼합, 투명 배경
```

**영문**:
```
2D pixel art style, ancient magic ruins theme game tileset,
32x32 pixel tiles, 512x512 sprite sheet,

Included tiles:
1. Ancient stone floor - 9-slice connected, magic rune decorations
2. Cracked floor - 9-slice, cracked and broken state
3. Stone platform - carved decorations, left/center/right
4. Collapsed pillar platform - left/center/right
5. Magic floating platform - glowing effect, left/center/right
6. Stair-style slopes - 45 degrees

Colors: stone(#D7CCC8, #8D6E63), gold decoration(#FFD54F), magic(#00E5FF)
Ancient Egyptian/Greek feel mixed, transparent background
```

---

### 함정/특수 타일

**한글**:
```
2D 픽셀아트 스타일, 고대 유적 함정 타일,
32x32 픽셀, 애니메이션 프레임 포함,

포함 타일:
1. 마법 함정 - 4프레임, 바닥에서 마법 발사
2. 화살 발사기 - 3프레임, 벽 부착
3. 숨겨진 바닥 가시 - 2프레임, 숨김/발동
4. 마법 장벽 - 4프레임, 보라색 에너지, 32x64
5. 순간이동 패드 - 4프레임, 포탈 효과
6. 압력판 - 2프레임, 눌림/해제
7. 모래 지역 - 2프레임, 슬로우 효과
8. 흐르는 모래 - 4프레임, 퀵샌드

마법 룬 발광 효과, 고대 유물 느낌
```

**영문**:
```
2D pixel art style, ancient ruins trap tiles,
32x32 pixels, animation frames included,

Included tiles:
1. Magic trap - 4-frame, magic burst from floor
2. Arrow shooter - 3-frame, wall mounted
3. Hidden floor spikes - 2-frame, hidden/triggered
4. Magic barrier - 4-frame, purple energy, 32x64
5. Teleport pad - 4-frame, portal effect
6. Pressure plate - 2-frame, pressed/released
7. Sand area - 2-frame, slow effect
8. Quicksand - 4-frame, sinking sand

Magic rune glow effects, ancient artifact feel
```

---

## 5.5 Stage 4: 성 타일셋 프롬프트

### 지형 타일셋

**한글**:
```
2D 픽셀아트 스타일, 다크 판타지 마왕성 테마 게임 타일셋,
32x32 픽셀 타일, 512x512 스프라이트 시트,

포함 타일:
1. 검은 석재 바닥 - 9-slice 연결, 어둡고 위협적
2. 붉은 카펫 바닥 - 9-slice, 찢어진 가장자리
3. 석재 플랫폼 - 고딕 조각, 좌/중/우
4. 철제 격자 플랫폼 - 좌/중/우
5. 사슬 플랫폼 - 매달린 형태, 좌/중/우
6. 계단 - 45도 경사

색상: 석재(#424242, #212121), 카펫(#B71C1C), 금속(#37474F)
고딕 양식, 위협적인 분위기, 피 얼룩
```

**영문**:
```
2D pixel art style, dark fantasy demon castle theme game tileset,
32x32 pixel tiles, 512x512 sprite sheet,

Included tiles:
1. Black stone floor - 9-slice connected, dark and menacing
2. Red carpet floor - 9-slice, torn edges
3. Stone platform - gothic carvings, left/center/right
4. Iron grate platform - left/center/right
5. Chain platform - hanging form, left/center/right
6. Stairs - 45-degree slope

Colors: stone(#424242, #212121), carpet(#B71C1C), metal(#37474F)
Gothic style, threatening atmosphere, blood stains
```

---

### 함정/특수 타일

**한글**:
```
2D 픽셀아트 스타일, 마왕성 함정 타일,
32x32 픽셀, 애니메이션 프레임 포함,

포함 타일:
1. 숨겨진 스파이크 - 2프레임, 바닥에서 튀어나옴
2. 창 함정 - 3프레임, 벽에서 창 발사, 32x64
3. 회전 톱날 - 4프레임, 레일 이동
4. 암흑 균열 - 4프레임, 보라색 데미지 존
5. 화염 분출 - 6프레임, 간헐 분출, 32x64
6. 전기 함정 - 4프레임, 번개 장벽
7. 무너지는 바닥 - 금 간 석재
8. 엘리베이터 - 수직 이동 플랫폼

색상: 불(#FF9800), 전기(#FFEB3B), 암흑(#7B1FA2)
```

**영문**:
```
2D pixel art style, demon castle trap tiles,
32x32 pixels, animation frames included,

Included tiles:
1. Hidden spikes - 2-frame, pop up from floor
2. Spear trap - 3-frame, spears from wall, 32x64
3. Rotating sawblade - 4-frame, rail movement
4. Dark fissure - 4-frame, purple damage zone
5. Flame jet - 6-frame, intermittent burst, 32x64
6. Electric trap - 4-frame, lightning barrier
7. Crumbling floor - cracked stone
8. Elevator - vertical moving platform

Colors: fire(#FF9800), electric(#FFEB3B), dark(#7B1FA2)
```

---

### 장식 타일

**한글**:
```
2D 픽셀아트 스타일, 마왕성 장식 타일,
다양한 크기 (16x16 ~ 64x64),

포함 오브젝트:
건축: 고딕 기둥, 아치, 창문 프레임, 난간, 철창
가구: 테이블, 의자, 왕좌(64x64), 책장, 관
조명: 횃불(4프레임), 촛대(3프레임), 샹들리에
장식: 찢어진 깃발, 태피스트리, 갑옷 진열대, 무기 진열
음침: 해골, 뼈, 사슬, 고문 도구, 피 웅덩이, 거미줄

고딕 호러 분위기, 붉은/보라 조명
투명 배경, 레이어링 가능
```

**영문**:
```
2D pixel art style, demon castle decoration tiles,
Various sizes (16x16 to 64x64),

Included objects:
Architecture: gothic pillars, arches, window frames, railings, iron bars
Furniture: table, chair, throne(64x64), bookshelf, coffin
Lighting: torch(4-frame), candelabra(3-frame), chandelier
Decorations: torn flags, tapestry, armor display, weapon rack
Grim: skulls, bones, chains, torture tools, blood puddles, spider webs

Gothic horror atmosphere, red/purple lighting
Transparent background, layerable
```

---

## 5.6 Stage 5: 보스 아레나 프롬프트

### 아레나 타일셋

**한글**:
```
2D 픽셀아트 스타일, 최종 보스 아레나 타일셋,
특수 크기 (32x32 기본, 128x128 마법진),

포함 타일:
1. 아레나 바닥 - 원형 패턴, 검은 대리석
2. 중앙 마법진 - 128x128, 8프레임 회전 발광
3. 외곽 테두리 - 금/보라 장식
4. 부유 플랫폼 - 3크기, 마법 부양
5. 대형 기둥 - 64x128, 4개
6. 차원 균열 - 64x64, 6프레임
7. 에너지 장벽 - 32x64, 4프레임

색상: 바닥(#1A1A2E), 금(#FFD700), 마법(#E040FB, #AA00FF)
극적인 조명, 보스전 분위기
```

**영문**:
```
2D pixel art style, final boss arena tileset,
Special sizes (32x32 base, 128x128 magic circle),

Included tiles:
1. Arena floor - circular pattern, black marble
2. Central magic circle - 128x128, 8-frame rotating glow
3. Outer border - gold/purple decorations
4. Floating platforms - 3 sizes, magic levitation
5. Large pillars - 64x128, 4 pieces
6. Dimension crack - 64x64, 6-frame
7. Energy barrier - 32x64, 4-frame

Colors: floor(#1A1A2E), gold(#FFD700), magic(#E040FB, #AA00FF)
Dramatic lighting, boss battle atmosphere
```

---

## 5.7 공용 포탈/문 프롬프트

**한글**:
```
2D 픽셀아트 스타일, 게임 포탈/문 오브젝트,
64x96 픽셀, 6프레임 애니메이션,

포함 포탈/문:
1. 일반 포탈 - 파란 소용돌이, 다음 방 이동
2. 보스 포탈 - 붉은/보라 소용돌이, 해골 장식
3. 비밀 포탈 - 녹색 소용돌이, 숨겨진 느낌
4. 탈출 포탈 - 황금 소용돌이, 승리 느낌
5. 귀환 포탈 - 흰색 소용돌이, 안전한 느낌
6. 상점 문 - 나무 문, 간판
7. 보물 문 - 황금 테두리, 보석 장식
8. 잠긴 문 - 철문, 자물쇠

마법 입자 효과, 투명 배경
```

**영문**:
```
2D pixel art style, game portal/door objects,
64x96 pixels, 6-frame animation,

Included portals/doors:
1. Normal portal - blue vortex, next room transition
2. Boss portal - red/purple vortex, skull decoration
3. Secret portal - green vortex, hidden feel
4. Escape portal - golden vortex, victory feel
5. Return portal - white vortex, safe feel
6. Shop door - wooden door, signboard
7. Treasure door - gold trim, gem decoration
8. Locked door - iron door, padlock

Magic particle effects, transparent background
```

---

# 요약: 제작 우선순위

## Phase 1: 핵심 타일 (필수)

| 스테이지 | 항목 | 타일 수 | 우선순위 |
|----------|------|--------|----------|
| Stage 1 숲 | 지형 Rule Tile | 47개 | ★★★ |
| Stage 1 숲 | 특수 타일 | 8개 | ★★★ |
| 공용 | 포탈/문 | 8개 | ★★★ |
| Stage 1 숲 | 장식 타일 | 20개 | ★★ |

## Phase 2: 추가 스테이지

| 스테이지 | 항목 | 타일 수 | 우선순위 |
|----------|------|--------|----------|
| Stage 2 동굴 | 전체 타일셋 | 70개+ | ★★ |
| Stage 3 유적 | 전체 타일셋 | 75개+ | ★★ |
| Stage 4 성 | 전체 타일셋 | 80개+ | ★★ |

## Phase 3: 특수 영역

| 영역 | 항목 | 타일 수 | 우선순위 |
|------|------|--------|----------|
| Stage 5 보스 | 아레나 타일셋 | 35개 | ★ |
| 시작방 | 로비 타일셋 | 35개 | ★ |
| 상점 | 상점 타일셋 | 25개 | ★ |

---

**문서 버전**: v1.0
**최종 업데이트**: 2025-11-30
**작성자**: Claude Code Assistant

---

*이 문서는 GASPT 프로젝트의 타일맵 에셋 기획서입니다.*
