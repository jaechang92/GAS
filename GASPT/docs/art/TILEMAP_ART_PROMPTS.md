# 타일맵 아트 프롬프트 모음

> **프로젝트**: GASPT (마법사 테마 2D 플랫포머 액션 로그라이트)
> **작성일**: 2025-11-30
> **목적**: namobanana AI 생성용 상세 프롬프트

---

## 공통 아트 스타일 명세

| 항목 | 값 |
|------|-----|
| 스타일 | 2D 픽셀아트 |
| 기본 타일 크기 | 32x32 px |
| 스프라이트 시트 | 512x512 px |
| 배경 | 투명 (transparent) |
| 라인 | 클린 픽셀, 안티앨리어싱 없음 |
| 타일링 | 심리스 (seamless tileable) |

---

# Stage 1: 숲 (Forest)

## 1. 지형 타일 (Ground Tiles)

### 1.1 풀이 자란 흙 바닥 (9-slice Rule Tile)

**한글**:
```
32x32 픽셀아트 타일셋, 9조각 지형 타일, 풀이 자란 흙 바닥, 위쪽에 짧은 녹색 풀, 어두운 갈색 흙 레이어, 다크 판타지 숲 분위기, 심리스 타일링, 클린 픽셀 no 안티앨리어싱, 투명 배경, 게임 에셋, 사이드뷰
```

**영문**:
```
32x32 pixel art tileset, 9-slice terrain tile, grass-covered dirt ground, short green grass on top layer, dark brown soil underneath, dark fantasy forest atmosphere, seamless tiling edges, clean pixels no anti-aliasing, transparent background, game asset, side view, 2D platformer style
```

**기술 명세**:
- 해상도: 96x96 (32x32 x 9 타일 = 3x3 그리드)
- 구성: 모서리 4개 + 가장자리 4개 + 중앙 1개
- 컬러 팔레트: 풀(#2d5a27, #3d7a37, #4d9a47), 흙(#3d2817, #5a3d27, #7a5237)

---

### 1.2 이끼 낀 돌 바닥 (9-slice Rule Tile)

**한글**:
```
32x32 픽셀아트 타일셋, 9조각 지형 타일, 이끼 낀 돌바닥, 회색 돌 위에 녹색 이끼 패치, 금이 간 돌 텍스처, 다크 판타지 던전 분위기, 심리스 타일링, 클린 픽셀, 투명 배경, 게임 에셋, 사이드뷰
```

**영문**:
```
32x32 pixel art tileset, 9-slice terrain tile, mossy stone floor, gray stone with green moss patches, cracked stone texture, dark fantasy dungeon atmosphere, seamless tiling, clean pixels no anti-aliasing, transparent background, game asset, side view, 2D platformer style
```

---

### 1.3 경사면 타일 (45도)

**한글**:
```
32x32 픽셀아트, 45도 경사면 타일 세트, 왼쪽 상승 경사 + 오른쪽 상승 경사, 풀이 자란 흙 표면, 경사면에 맞춰 자란 풀, 다크 판타지 숲, 클린 픽셀, 투명 배경, 2D 플랫포머 게임 에셋
```

**영문**:
```
32x32 pixel art, 45 degree slope tile set, left ascending slope and right ascending slope pair, grass-covered dirt surface, grass growing along slope angle, dark fantasy forest style, clean pixels no anti-aliasing, transparent background, 2D platformer game asset, side view
```

---

## 2. 플랫폼 타일 (Platform Tiles)

### 2.1 나뭇가지 플랫폼 (좌/중/우)

**한글**:
```
32x32 픽셀아트, 나뭇가지 플랫폼 3조각 세트, 왼쪽 끝 + 중앙 반복 + 오른쪽 끝, 두꺼운 갈색 나뭇가지, 위에 약간의 이끼, 다크 판타지 숲, 클린 픽셀, 투명 배경, 원패스 점프 플랫폼, 2D 플랫포머
```

**영문**:
```
32x32 pixel art, tree branch platform 3-piece set, left end piece + middle repeatable + right end piece, thick brown tree branch, slight moss on top surface, dark fantasy forest, clean pixels no anti-aliasing, transparent background, one-way jump through platform, 2D platformer game asset, side view
```

---

### 2.2 거대 버섯 플랫폼 (좌/중/우)

**한글**:
```
32x32 픽셀아트, 거대 버섯 플랫폼 3조각 세트, 왼쪽 가장자리 + 중앙 반복 + 오른쪽 가장자리, 보라색 버섯 갓, 흰색 점 무늬, 아래쪽 그림자, 마법의 숲 분위기, 클린 픽셀, 투명 배경, 2D 플랫포머 게임
```

**영문**:
```
32x32 pixel art, giant mushroom platform 3-piece set, left edge + middle repeatable + right edge, purple mushroom cap, white polka dot pattern, subtle glow effect, underside shadow, magical forest atmosphere, clean pixels no anti-aliasing, transparent background, 2D platformer game asset
```

---

### 2.3 나무 뿌리 플랫폼 (좌/중/우)

**한글**:
```
32x32 픽셀아트, 나무 뿌리 플랫폼 3조각 세트, 왼쪽 끝 + 중앙 반복 + 오른쪽 끝, 굵은 갈색 나무 뿌리, 나무껍질 텍스처, 위쪽 평평한 표면, 다크 판타지 숲, 클린 픽셀, 투명 배경
```

**영문**:
```
32x32 pixel art, tree root platform 3-piece set, left end + middle repeatable + right end, thick brown tree root, bark texture detail, flat walkable surface on top, dark fantasy forest style, clean pixels no anti-aliasing, transparent background, 2D platformer game asset
```

---

## 3. 특수 타일 (Special Tiles)

### 3.1 독 웅덩이 (4프레임 애니메이션)

**한글**:
```
32x32 픽셀아트 스프라이트 시트, 독 웅덩이 4프레임 애니메이션, 보라색 독액, 거품이 부글부글 올라오는 효과, 녹색 연기, 다크 판타지, 가로 정렬 스프라이트 시트, 클린 픽셀, 투명 배경, 게임 장애물
```

**영문**:
```
32x32 pixel art sprite sheet, poison pool 4-frame animation, purple toxic liquid, bubbling effect with rising bubbles, green mist vapor, dark fantasy style, horizontal sprite sheet layout, clean pixels no anti-aliasing, transparent background, game hazard obstacle
```

**애니메이션 가이드**:
- Frame 1: 작은 거품 생성
- Frame 2: 거품 상승
- Frame 3: 거품 터짐
- Frame 4: 표면 잔잔 + 새 거품 시작
- 권장 FPS: 6fps (0.67초 루프)

---

### 3.2 가시 덤불

**한글**:
```
32x32 픽셀아트, 가시 덤불 타일, 날카로운 검은 가시, 어두운 녹색 덤불, 위험해 보이는 뾰족한 형태, 다크 판타지 숲, 클린 픽셀, 투명 배경, 데미지 장애물, 2D 플랫포머
```

**영문**:
```
32x32 pixel art, thorn bush tile, sharp black thorns pointing upward, dark green bush base, dangerous spiky appearance, dark fantasy forest, clean pixels no anti-aliasing, transparent background, damage hazard obstacle, 2D platformer game asset
```

---

### 3.3 탄성 버섯 점프대 (3프레임)

**한글**:
```
32x32 픽셀아트 스프라이트 시트, 탄성 버섯 3프레임 애니메이션, 빨간색 버섯 갓과 흰 점무늬, 통통한 형태, 프레임1 기본 + 프레임2 눌림 + 프레임3 튕김, 마법의 숲, 클린 픽셀, 투명 배경, 점프대 기믹
```

**영문**:
```
32x32 pixel art sprite sheet, bouncy mushroom 3-frame animation, red mushroom cap with white spots, chubby bouncy shape, frame1 idle + frame2 compressed squash + frame3 stretched bounce, magical forest style, clean pixels no anti-aliasing, transparent background, jump pad game mechanic
```

**애니메이션 가이드**:
- Frame 1: 기본 상태 (Idle)
- Frame 2: 눌린 상태 (Squash) - 플레이어 착지 시
- Frame 3: 튕기는 상태 (Stretch) - 반동

---

### 3.4 물 타일 (4프레임)

**한글**:
```
32x32 픽셀아트 스프라이트 시트, 물 타일 4프레임 애니메이션, 푸른 물 표면, 반짝이는 하이라이트, 부드러운 물결 효과, 심리스 타일링, 마법의 숲 연못, 클린 픽셀, 투명 배경
```

**영문**:
```
32x32 pixel art sprite sheet, water tile 4-frame animation, blue water surface, shimmering highlight sparkles, gentle ripple wave effect, seamless tiling edges, magical forest pond, clean pixels no anti-aliasing, transparent background, 2D platformer environment
```

---

### 3.5 무너지는 나무 플랫폼 (5프레임)

**한글**:
```
32x32 픽셀아트 스프라이트 시트, 무너지는 나무 플랫폼 5프레임, 오래된 나무 판자, 프레임별로 점점 갈라지고 부서지는 효과, 나무 파편, 다크 판타지 숲, 클린 픽셀, 투명 배경, 시간제한 플랫폼
```

**영문**:
```
32x32 pixel art sprite sheet, crumbling wood platform 5 frames, old wooden plank, progressive cracking and breaking effect across frames, wood debris particles, dark fantasy forest, clean pixels no anti-aliasing, transparent background, timed platform game mechanic
```

---

## 4. 장식 타일 (Decoration Tiles)

### 4.1 나무 세트 (거대/중간/작은)

**한글**:
```
픽셀아트 나무 세트 3종, 거대 나무 128x192 + 중간 나무 64x96 + 작은 나무 32x48, 어두운 갈색 두꺼운 줄기, 짙은 녹색 잎, 신비로운 마법의 숲 분위기, 약간의 보라색 마법 빛, 클린 픽셀, 투명 배경, 2D 플랫포머 배경 에셋
```

**영문**:
```
pixel art tree set 3 sizes, large tree 128x192 + medium tree 64x96 + small tree 32x48, dark brown thick trunk, deep green foliage, mysterious magical forest atmosphere, subtle purple magical glow, clean pixels no anti-aliasing, transparent background, 2D platformer background asset, front view
```

---

### 4.2 풀/꽃/덤불

**한글**:
```
32x32 픽셀아트 스프라이트 시트, 숲 식물 장식 세트, 짧은 풀 + 긴 풀 + 작은 꽃 + 큰 꽃 + 덤불, 녹색 계열 풀, 보라색과 파란색 마법 꽃, 다크 판타지 숲, 클린 픽셀, 투명 배경
```

**영문**:
```
32x32 pixel art sprite sheet, forest plant decoration set, short grass + tall grass + small flowers + large flowers + bush, green tone grass varieties, purple and blue magical flowers, dark fantasy forest theme, clean pixels no anti-aliasing, transparent background, 2D game decoration assets
```

---

### 4.3 발광 버섯 (파랑/보라/녹색)

**한글**:
```
32x32 픽셀아트 스프라이트 시트, 발광 버섯 3색 세트, 파란색 발광 버섯 + 보라색 발광 버섯 + 녹색 발광 버섯, 작고 귀여운 버섯 형태, 글로우 효과 표현, 마법의 숲, 클린 픽셀, 투명 배경, 장식 에셋
```

**영문**:
```
32x32 pixel art sprite sheet, glowing mushroom 3-color set, blue glowing mushroom + purple glowing mushroom + green glowing mushroom, small cute mushroom shape, bioluminescent glow effect, magical forest atmosphere, clean pixels no anti-aliasing, transparent background, decoration game asset
```

---

### 4.4 바위 세트 (대/중/소)

**한글**:
```
픽셀아트 바위 세트 3종, 큰 바위 64x48 + 중간 바위 32x32 + 작은 바위 16x16, 회색 돌, 녹색 이끼 패치, 자연스러운 형태, 다크 판타지 숲, 클린 픽셀, 투명 배경
```

**영문**:
```
pixel art rock set 3 sizes, large rock 64x48 + medium rock 32x32 + small rock 16x16, gray stone, green moss patches, natural organic shapes, dark fantasy forest style, clean pixels no anti-aliasing, transparent background, 2D platformer environment asset
```

---

### 4.5 반딧불이 효과 (4프레임)

**한글**:
```
8x8 픽셀아트 스프라이트 시트, 반딧불이 4프레임 애니메이션, 노란색 빛나는 점, 밝기 변화 애니메이션, 부드러운 글로우, 마법의 숲 분위기, 클린 픽셀, 투명 배경, 파티클 에셋
```

**영문**:
```
8x8 pixel art sprite sheet, firefly 4-frame animation, yellow glowing dot, brightness pulsing animation cycle, soft glow effect, magical forest atmosphere, clean pixels no anti-aliasing, transparent background, particle effect game asset
```

---

## 5. 숲 스테이지 컬러 팔레트

| 용도 | 메인 | 밝음 | 어두움 |
|-----|------|------|--------|
| 풀/이끼 | #3d7a37 | #4d9a47 | #2d5a27 |
| 흙/나무 | #5a3d27 | #7a5237 | #3d2817 |
| 돌 | #5a5a5a | #6a6a6a | #4a4a4a |
| 마법(보라) | #8a4aaa | #aa5aca | #6a3a8a |
| 마법(파랑) | #4a6aaa | #6a8aca | #3a5a8a |
| 독 | #6a2a8a | #8a3aaa | #4a1a6a |

---

# Stage 2: 동굴 (Cave)

## 1. 지형 타일

### 1.1 암석 바닥 타일셋 (9-slice)

**한글**:
```
2D 픽셀아트 스타일, 어두운 크리스탈 동굴 테마 게임 타일셋,
32x32 픽셀 타일, 512x512 스프라이트 시트,

포함 타일:
1. 거친 암석 바닥 - 9-slice 연결 타일 (47개 Rule Tile 세트)
   - 상단 가장자리에 균열과 작은 돌 디테일
   - 습기로 인한 어두운 반점
2. 습한 바닥 - 9-slice 연결 타일
   - 물기 반사 효과, 푸른 빛
   - 이끼와 작은 버섯
3. 경사면 - 45도 좌향/우향

색상: 암석 밝은(#78909C), 암석 어두운(#455A64), 물기(#0288D1)
심리스 연결 가능, 투명 배경, 클린 픽셀, 안티앨리어싱 없음
다크 판타지 분위기, Unity Tilemap 호환
```

**영문**:
```
2D pixel art style, dark crystal cave theme game tileset,
32x32 pixel tiles, 512x512 sprite sheet,

Included tiles:
1. Rough rock ground - 9-slice connected tiles (47-piece Rule Tile set)
   - Cracks and small stone details on top edges
   - Dark spots from moisture
2. Wet ground - 9-slice connected tiles
   - Water reflection effect, bluish tint
   - Moss and small mushrooms
3. Slopes - 45-degree left/right facing

Colors: rock light(#78909C), rock dark(#455A64), moisture(#0288D1)
Seamless tileable, transparent background, clean pixels, no anti-aliasing
Dark fantasy atmosphere, Unity Tilemap compatible
```

---

### 1.2 플랫폼 타일셋

**한글**:
```
2D 픽셀아트 스타일, 동굴 플랫폼 타일셋,
32x32 픽셀 타일,

포함 플랫폼:
1. 암석 돌출 플랫폼 - 좌측 끝/중간/우측 끝 (3피스)
   - 벽에서 자연스럽게 돌출된 형태
   - 균열과 이끼 디테일
2. 크리스탈 플랫폼 - 좌/중/우 (3피스)
   - 파란색/보라색 크리스탈 상단
   - 내부 발광 효과
   - 날카로운 결정 형태
3. 목재 다리 플랫폼 - 좌/중/우 (3피스)
   - 광산 스타일 나무 판자
   - 밧줄로 고정된 형태

투명 배경, 캐릭터가 위로 점프해서 통과 가능한 원웨이 플랫폼
```

**영문**:
```
2D pixel art style, cave platform tileset,
32x32 pixel tiles,

Included platforms:
1. Rocky outcrop platform - left end/middle/right end (3 pieces)
   - Natural protrusion from wall
   - Crack and moss details
2. Crystal platform - left/center/right (3 pieces)
   - Blue/purple crystal top
   - Internal glow effect
   - Sharp crystalline shape
3. Wooden bridge platform - left/center/right (3 pieces)
   - Mine-style wooden planks
   - Rope-fastened form

Transparent background, one-way platform for jump-through from below
```

---

## 2. 특수 타일

### 2.1 용암 타일 (4프레임)

**한글**:
```
2D 픽셀아트 스타일, 동굴 용암 타일,
32x32 픽셀, 4프레임 루프 애니메이션,

용암 특성:
- 붉은색/주황색 끓어오르는 액체
- 표면에 검은 응고된 바위 조각
- 밝은 발광 효과
- 작은 불꽃 튀김
- 부글거리는 기포 애니메이션

색상: 용암 밝은(#FF9800), 용암 어두운(#FF5722), 바위(#424242)
투명 배경, 데미지 영역 표시용
```

**영문**:
```
2D pixel art style, cave lava tile,
32x32 pixels, 4-frame loop animation,

Lava characteristics:
- Red/orange boiling liquid
- Black solidified rock fragments on surface
- Bright glow effect
- Small fire sparks
- Bubbling animation

Colors: lava bright(#FF9800), lava dark(#FF5722), rock(#424242)
Transparent background, for damage zone indication
```

---

### 2.2 가스 분출 (4프레임)

**한글**:
```
2D 픽셀아트 스타일, 독성 가스 분출구,
32x32 픽셀 기본, 32x64 분출 상태, 4프레임 애니메이션,

가스 분출구 특성:
- 바닥의 균열/구멍에서 분출
- 녹색/노란색 독성 가스
- 반투명 가스 입자
- 간헐적 분출 패턴

상태:
1. 휴면 - 균열만 보임, 미세한 연기
2. 경고 - 균열에서 가스 스멀스멀
3. 분출 - 강한 가스 기둥 (32x64)
4. 감소 - 분출 약해짐

색상: 가스(#8BC34A, #CDDC39), 균열(#37474F)
투명 배경, 주기적 데미지 영역
```

**영문**:
```
2D pixel art style, toxic gas vent,
32x32 pixels base, 32x64 eruption state, 4-frame animation,

Gas vent characteristics:
- Eruption from floor crack/hole
- Green/yellow toxic gas
- Semi-transparent gas particles
- Intermittent eruption pattern

Colors: gas(#8BC34A, #CDDC39), crack(#37474F)
Transparent background, periodic damage zone
```

---

## 3. 장식 타일

### 3.1 크리스탈 세트

**한글**:
```
2D 픽셀아트 스타일, 동굴 크리스탈 장식 세트,
다양한 크기 (16x16 ~ 48x48),

포함 크리스탈:
1. 파란 크리스탈 - 소(16x16)/중(24x24)/대(32x48)
   - 밝은 파란색 발광, 내부 굴절 표현
2. 보라 크리스탈 - 소/중/대
   - 진한 보라색, 신비로운 발광
3. 녹색 크리스탈 - 소/중
   - 에메랄드 빛, 독성 느낌

크리스탈 배치:
- 바닥에서 자라는 형태
- 천장에 매달린 형태
- 벽에 박힌 형태

색상: 파랑(#03A9F4), 보라(#7C4DFF), 녹색(#4CAF50)
발광 효과 포함, 투명 배경
```

**영문**:
```
2D pixel art style, cave crystal decoration set,
Various sizes (16x16 to 48x48),

Included crystals:
1. Blue crystal - small(16x16)/medium(24x24)/large(32x48)
   - Bright blue glow, internal refraction
2. Purple crystal - small/medium/large
   - Deep purple, mystical glow
3. Green crystal - small/medium
   - Emerald light, toxic feel

Colors: blue(#03A9F4), purple(#7C4DFF), green(#4CAF50)
Glow effect included, transparent background
```

---

## 4. 동굴 컬러 팔레트

| 요소 | 색상 코드 | 설명 |
|------|----------|------|
| 암석 (밝음) | #78909C | 블루 그레이 |
| 암석 (어두움) | #455A64 | 다크 블루 그레이 |
| 결정 (파랑) | #03A9F4 | 라이트 블루 |
| 결정 (보라) | #7C4DFF | 디프 퍼플 |
| 용암 | #FF5722 | 딥 오렌지 |
| 물 | #0288D1 | 라이트 블루 |

---

# Stage 3: 유적 (Ruins)

## 1. 지형 타일

### 1.1 석재 바닥 타일셋 (9-slice)

**한글**:
```
2D 픽셀아트 스타일, 고대 마법 유적 테마 게임 타일셋,
32x32 픽셀 타일, 512x512 스프라이트 시트,

포함 타일:
1. 고대 석재 바닥 - 9-slice 연결 타일 (47개 Rule Tile 세트)
   - 오래된 석재 블록 패턴
   - 새겨진 마법 문양 디테일
   - 금빛 테두리 장식
2. 균열 바닥 - 9-slice 연결 타일
   - 금이 가고 부서진 상태
   - 틈 사이로 모래/먼지
3. 계단식 경사면 - 45도 좌향/우향

색상: 석재 밝은(#D7CCC8), 석재 어두운(#8D6E63), 금 장식(#FFD54F)
고대 이집트/그리스 혼합 스타일
심리스 연결, 투명 배경, Unity Tilemap 호환
```

**영문**:
```
2D pixel art style, ancient magic ruins theme game tileset,
32x32 pixel tiles, 512x512 sprite sheet,

Included tiles:
1. Ancient stone floor - 9-slice connected tiles (47-piece Rule Tile set)
   - Old stone block pattern
   - Carved magic rune details
   - Gold trim decoration
2. Cracked floor - 9-slice connected tiles
   - Cracked and broken state
   - Sand/dust in gaps
3. Stair-style slopes - 45-degree left/right facing

Colors: stone light(#D7CCC8), stone dark(#8D6E63), gold decoration(#FFD54F)
Ancient Egyptian/Greek mixed style
Seamless tileable, transparent background, Unity Tilemap compatible
```

---

## 2. 특수 타일

### 2.1 마법 함정 (4프레임)

**한글**:
```
2D 픽셀아트 스타일, 마법 함정 타일,
32x32 픽셀, 4프레임 애니메이션,

마법 함정 특성:
- 바닥에 새겨진 마법진
- 밟으면 마법 에너지 폭발
- 시안/핑크색 마법 발광

상태/프레임:
1. 휴면 - 희미한 룬 문양
2. 감지 - 플레이어 접근 시 발광 시작
3. 활성화 - 마법진 전체 발광
4. 폭발 - 에너지 방출, 데미지

색상: 룬(#00E5FF), 폭발(#EA80FC), 바닥(#8D6E63)
투명 배경, 주기적 데미지
```

**영문**:
```
2D pixel art style, magic trap tile,
32x32 pixels, 4-frame animation,

Magic trap characteristics:
- Magic circle carved on floor
- Magic energy explosion when stepped on
- Cyan/pink magic glow

States/Frames:
1. Dormant - faint rune pattern
2. Detection - starts glowing when player approaches
3. Activated - entire magic circle glowing
4. Explosion - energy release, damage

Colors: rune(#00E5FF), explosion(#EA80FC), floor(#8D6E63)
Transparent background, periodic damage
```

---

### 2.2 화살 발사기 (3프레임)

**한글**:
```
2D 픽셀아트 스타일, 화살 발사 함정,
32x32 픽셀 발사기, 16x8 화살, 3프레임 애니메이션,

화살 발사기 특성:
- 벽에 설치된 석조 장치
- 해골 또는 사자 머리 모양
- 입에서 화살 발사

상태:
1. 대기 - 닫힌 입, 눈에 희미한 빛
2. 준비 - 입 벌어짐, 눈 발광
3. 발사 - 화살 나가는 순간

색상: 석재(#8D6E63), 화살(#5D4037), 촉(#607D8B)
투명 배경
```

**영문**:
```
2D pixel art style, arrow shooter trap,
32x32 pixel shooter, 16x8 arrow, 3-frame animation,

Arrow shooter characteristics:
- Stone device mounted on wall
- Skull or lion head shape
- Arrows shoot from mouth

States:
1. Idle - closed mouth, faint glow in eyes
2. Ready - mouth opening, eyes glowing
3. Fire - arrow release moment

Colors: stone(#8D6E63), arrow(#5D4037), tip(#607D8B)
Transparent background
```

---

## 3. 유적 컬러 팔레트

| 요소 | 색상 코드 | 설명 |
|------|----------|------|
| 석재 (밝음) | #D7CCC8 | 라이트 브라운 |
| 석재 (어두움) | #8D6E63 | 브라운 |
| 금 장식 | #FFD54F | 앰버 |
| 마법 룬 | #00E5FF | 시안 |
| 마법 발광 | #EA80FC | 핑크 |
| 모래 | #FFE082 | 앰버 |

---

# Stage 4: 성 (Castle)

## 1. 지형 타일

### 1.1 석재/카펫 바닥 타일셋

**한글**:
```
2D 픽셀아트 스타일, 다크 판타지 마왕성 테마 게임 타일셋,
32x32 픽셀 타일, 512x512 스프라이트 시트,

포함 타일:
1. 검은 석재 바닥 - 9-slice 연결 타일 (47개 Rule Tile 세트)
   - 어둡고 위협적인 검은 돌
   - 균열과 핏자국 디테일
   - 고딕 문양 새김
2. 붉은 카펫 바닥 - 9-slice 연결 타일
   - 찢어진 가장자리
   - 금실 테두리 장식
   - 피 얼룩
3. 계단 - 45도 경사
   - 검은 석재 계단
   - 붉은 카펫 깔린 버전

색상: 석재 밝은(#424242), 석재 어두운(#212121), 카펫(#B71C1C), 금(#FFC107)
고딕 양식, 위협적인 분위기
심리스 연결, 투명 배경, Unity Tilemap 호환
```

**영문**:
```
2D pixel art style, dark fantasy demon castle theme game tileset,
32x32 pixel tiles, 512x512 sprite sheet,

Included tiles:
1. Black stone floor - 9-slice connected tiles (47-piece Rule Tile set)
   - Dark and menacing black stone
   - Crack and bloodstain details
   - Gothic pattern carvings
2. Red carpet floor - 9-slice connected tiles
   - Torn edges
   - Gold thread border decoration
   - Blood stains
3. Stairs - 45-degree slope
   - Black stone stairs
   - Version with red carpet

Colors: stone light(#424242), stone dark(#212121), carpet(#B71C1C), gold(#FFC107)
Gothic style, threatening atmosphere
Seamless tileable, transparent background, Unity Tilemap compatible
```

---

## 2. 특수 타일

### 2.1 스파이크 함정 (2프레임)

**한글**:
```
2D 픽셀아트 스타일, 숨겨진 스파이크 함정,
32x32 픽셀, 2프레임,

스파이크 특성:
- 바닥에서 튀어나오는 철 가시
- 날카롭고 피 묻은 끝
- 빠른 발동 속도

상태:
1. 숨김 - 바닥 균열만 보임
2. 발동 - 가시 완전히 돌출, 피 튀김 효과

색상: 철(#37474F), 피(#C62828)
투명 배경
```

**영문**:
```
2D pixel art style, hidden spike trap,
32x32 pixels, 2 frames,

Spike characteristics:
- Iron spikes popping from floor
- Sharp and bloody tips
- Fast activation speed

States:
1. Hidden - only floor crack visible
2. Triggered - spikes fully extended, blood splash effect

Colors: iron(#37474F), blood(#C62828)
Transparent background
```

---

### 2.2 회전 톱 (4프레임)

**한글**:
```
2D 픽셀아트 스타일, 회전 톱날 함정,
32x32 픽셀 톱날, 4프레임 회전 애니메이션,

회전 톱 특성:
- 철제 원형 톱날
- 레일을 따라 이동
- 고속 회전
- 불꽃 튀김 효과

구성 요소:
1. 톱날 - 4프레임 회전 (360도 분할)
2. 레일 - 수평/수직/대각선 (16x16 타일)
3. 불꽃 - 접촉 시 이펙트

색상: 톱날(#607D8B), 날카로운 부분(#CFD8DC), 불꽃(#FF9800)
투명 배경
```

**영문**:
```
2D pixel art style, rotating sawblade trap,
32x32 pixel sawblade, 4-frame rotation animation,

Rotating saw characteristics:
- Iron circular sawblade
- Moves along rail
- High-speed rotation
- Spark effect

Components:
1. Sawblade - 4-frame rotation (360-degree split)
2. Rail - horizontal/vertical/diagonal (16x16 tiles)
3. Sparks - effect on contact

Colors: sawblade(#607D8B), sharp edge(#CFD8DC), sparks(#FF9800)
Transparent background
```

---

### 2.3 화염 분출 (6프레임)

**한글**:
```
2D 픽셀아트 스타일, 화염 분출 함정,
32x64 픽셀, 6프레임 애니메이션,

화염 분출 특성:
- 벽/바닥의 악마 조각상 입에서 분출
- 간헐적 분출 패턴
- 강력한 불꽃 기둥

프레임 구성:
1. 휴면 - 조각상만, 입에 불씨
2. 경고 - 입에서 연기, 눈 발광
3-4. 분출 시작 - 화염 성장
5. 최대 분출 - 전체 화염 기둥
6. 소멸 - 화염 사라짐

색상: 화염(#FF5722, #FF9800, #FFEB3B), 조각상(#37474F)
투명 배경
```

**영문**:
```
2D pixel art style, flame jet trap,
32x64 pixels, 6-frame animation,

Flame jet characteristics:
- Erupts from demon statue mouth on wall/floor
- Intermittent eruption pattern
- Powerful flame pillar

Frame composition:
1. Dormant - statue only, ember in mouth
2. Warning - smoke from mouth, eyes glowing
3-4. Eruption start - flame growing
5. Maximum eruption - full flame pillar
6. Fade - flame disappearing

Colors: flame(#FF5722, #FF9800, #FFEB3B), statue(#37474F)
Transparent background
```

---

## 3. 장식 타일

### 3.1 건축 장식

**한글**:
```
2D 픽셀아트 스타일, 마왕성 건축 장식 세트,
다양한 크기,

포함 오브젝트:
1. 고딕 기둥 - 2종 (32x96)
   - 뾰족한 상단 장식
   - 악마 얼굴 조각
2. 철창 - 감옥 창살 (32x64)
   - 녹슨 철봉
   - 구부러진/부서진 버전
3. 횃불 - 4프레임 애니메이션 (16x32)
   - 벽 부착형 금속 홀더
   - 타오르는 불꽃
4. 고딕 창문 - 3종 (32x64)
   - 뾰족한 아치형
   - 깨진 스테인드글라스

색상: 석재(#424242), 철(#37474F), 불(#FF9800)
투명 배경
```

**영문**:
```
2D pixel art style, demon castle architecture decoration set,
Various sizes,

Included objects:
1. Gothic pillars - 2 types (32x96)
   - Pointed top decoration
   - Demon face carving
2. Iron bars - prison bars (32x64)
   - Rusty iron bars
   - Bent/broken version
3. Torch - 4-frame animation (16x32)
   - Wall-mounted metal holder
   - Burning flame
4. Gothic windows - 3 types (32x64)
   - Pointed arch shape
   - Broken stained glass

Colors: stone(#424242), iron(#37474F), fire(#FF9800)
Transparent background
```

---

### 3.2 음침한 장식

**한글**:
```
2D 픽셀아트 스타일, 마왕성 음침한 장식 세트,
다양한 크기,

포함 오브젝트:
1. 해골 - 3종 (16x16)
   - 인간/악마 두개골
   - 벽 장식/바닥 흩어짐
2. 사슬 - (16x64)
   - 천장에서 늘어진 형태
   - 족쇄/수갑 달린 버전
3. 깃발/태피스트리 - (32x64)
   - 찢어진 붉은 깃발
   - 해골/악마 문양
4. 피 웅덩이 - 2프레임 (32x16)
   - 바닥의 핏자국
   - 살짝 일렁이는 효과
5. 거미줄 - (32x32)
   - 모서리/기둥용
   - 슬로우 영역

색상: 뼈(#ECEFF1), 피(#B71C1C, #C62828), 천(#7B1FA2)
투명 배경
```

**영문**:
```
2D pixel art style, demon castle grim decoration set,
Various sizes,

Included objects:
1. Skulls - 3 types (16x16)
   - Human/demon skulls
   - Wall decoration/scattered on floor
2. Chains - (16x64)
   - Hanging from ceiling
   - Version with shackles
3. Flags/tapestry - (32x64)
   - Torn red flags
   - Skull/demon patterns
4. Blood puddle - 2 frames (32x16)
   - Bloodstain on floor
   - Slight ripple effect
5. Spider webs - (32x32)
   - For corners/pillars
   - Slow zone

Colors: bone(#ECEFF1), blood(#B71C1C, #C62828), cloth(#7B1FA2)
Transparent background
```

---

## 4. 성 스테이지 컬러 팔레트

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

# 공용 타일셋

## 포탈/문 (8종)

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

# Unity 임포트 가이드

## Sprite Import Settings

```
Texture Type: Sprite (2D and UI)
Sprite Mode: Multiple
Pixels Per Unit: 32
Filter Mode: Point (no filter)
Compression: None
Max Size: 1024 또는 2048
```

## Rule Tile 설정

1. 47개 타일을 포함한 스프라이트 시트 임포트
2. Sprite Editor에서 Grid by Cell Size (32x32) 슬라이싱
3. Rule Tile 에셋 생성 후 룰 설정
4. Tilemap Renderer의 Sort Order 설정

## Animated Tile 설정

1. 애니메이션 프레임을 순서대로 슬라이싱
2. Animated Tile 에셋 생성
3. 프레임 추가 및 FPS 설정
4. 루프 모드 설정 (Loop/Ping-Pong)

---

# 애니메이션 FPS 가이드

| 타일 유형 | 프레임 수 | 권장 FPS |
|----------|----------|---------|
| 물/용암 | 4 | 6 |
| 불꽃/횃불 | 4-6 | 8 |
| 마법 효과 | 4-8 | 10 |
| 입자/반짝임 | 3-4 | 12 |
| 함정 작동 | 2-4 | 4-8 |
| 고속 회전 | 4 | 16 |

---

**문서 버전**: v1.0
**최종 업데이트**: 2025-11-30
**작성자**: Claude Code Assistant + namobanana Agent

---

*이 문서는 GASPT 프로젝트의 타일맵 아트 프롬프트 모음입니다.*
