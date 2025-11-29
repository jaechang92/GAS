# 추가 아트 에셋 기획

> **프로젝트**: GASPT (마법사 테마 2D 플랫포머 액션 로그라이트)
> **작성일**: 2025-11-30
> **목적**: 기존 기획에서 누락된 아트 에셋 추가 기획

---

## 목차

1. [추가 스테이지 (3~5)](#1-추가-스테이지-3-5)
2. [추가 몬스터 (스테이지별)](#2-추가-몬스터-스테이지별)
3. [추가 보스 (3~5)](#3-추가-보스-3-5)
4. [로그라이트 시스템 UI](#4-로그라이트-시스템-ui)
5. [추가 플레이어 캐릭터 (3~4번째)](#5-추가-플레이어-캐릭터-3-4번째)
6. [스킬 아이콘](#6-스킬-아이콘)
7. [NPC 상점/서비스](#7-npc-상점서비스)
8. [미니맵/맵 시스템](#8-미니맵맵-시스템)
9. [타이틀/로딩 화면](#9-타이틀로딩-화면)
10. [업적/도감 시스템](#10-업적도감-시스템)
11. [추가 이펙트](#11-추가-이펙트)
12. [애니메이션 스프라이트 시트](#12-애니메이션-스프라이트-시트)

---

# 1. 추가 스테이지 (3~5)

## 1.1 스테이지 3: 마법 도서관 (Arcane Library)

### 컨셉
- 버려진 고대 마법사들의 도서관
- 떠다니는 책들, 살아있는 책장, 마법 문양
- 지식을 탐하는 유령들

### 배경 레이어

#### Layer 4 - 천장/하늘

**한글**:
```
2D 픽셀아트 스타일, 다크 판타지 마법 도서관 천장,
거대한 아치형 천장, 마법 문양이 새겨진 돔,
떠다니는 촛불과 마나 오브, 거미줄,
스테인드글라스에서 들어오는 보라색 빛,
512x256 픽셀, 패럴랙스용
```

**영문**:
```
2D pixel art style, dark fantasy magic library ceiling,
massive arched ceiling, dome with carved magic runes,
floating candles and mana orbs, spider webs,
purple light coming through stained glass,
512x256 pixels, for parallax
```

**컬러**: #1A1A2E (어두운), #4B0082 (마법빛), #DAA520 (촛불)

---

#### Layer 3 - 원경 (거대한 책장)

**한글**:
```
2D 픽셀아트 스타일, 마법 도서관 원경,
하늘까지 닿는 거대한 책장들의 실루엣,
책장 사이로 보이는 떠다니는 책들,
사다리와 발코니, 마법 조명,
512x256 픽셀, 투명 배경
```

**영문**:
```
2D pixel art style, magic library distant view,
silhouettes of massive bookshelves reaching the ceiling,
floating books visible between shelves,
ladders and balconies, magical lighting,
512x256 pixels, transparent background
```

---

#### Layer 2 - 중경 (책장과 통로)

**한글**:
```
2D 픽셀아트 스타일, 마법 도서관 중경,
오래된 나무 책장, 먼지 쌓인 책들,
떠다니는 책 몇 권, 촛대와 독서대,
바닥에 흩어진 양피지, 마법 두루마리,
512x256 픽셀, 투명 배경
```

**영문**:
```
2D pixel art style, magic library middle ground,
old wooden bookshelves, dust-covered books,
few floating books, candelabra and reading desks,
scattered parchments on floor, magic scrolls,
512x256 pixels, transparent background
```

---

#### Layer 1 - 근경 (전투 영역)

**한글**:
```
2D 픽셀아트 스타일, 마법 도서관 근경,
떨어진 책들, 깨진 잉크병, 깃펜,
책상 잔해, 찢어진 페이지,
바닥에 마법 문양 일부,
512x256 픽셀, 투명 배경
```

**영문**:
```
2D pixel art style, magic library foreground,
fallen books, broken ink bottles, quill pens,
desk debris, torn pages,
partial magic runes on floor,
512x256 pixels, transparent background
```

---

## 1.2 스테이지 4: 용암 동굴 (Inferno Cavern)

### 컨셉
- 지하 깊숙한 용암 지대
- 불의 정령, 용암 생물
- 열기와 위험

### 배경 레이어

#### Layer 4 - 상층부

**한글**:
```
2D 픽셀아트 스타일, 용암 동굴 상층부,
거대한 동굴 천장, 종유석,
붉은 빛이 아래에서 올라옴,
열기로 인한 왜곡 효과,
512x256 픽셀, 패럴랙스용
```

**영문**:
```
2D pixel art style, inferno cavern upper layer,
massive cave ceiling, stalactites,
red glow rising from below,
heat distortion effect,
512x256 pixels, for parallax
```

**컬러**: #2D1B0E (동굴), #FF4500 (용암빛), #FF6600 (열기)

---

#### Layer 3 - 원경 (용암 호수)

**한글**:
```
2D 픽셀아트 스타일, 용암 동굴 원경,
거대한 용암 호수, 솟아오르는 용암 기둥,
화산 바위 실루엣, 불꽃 입자,
붉은/주황 그라데이션,
512x256 픽셀, 투명 배경
```

**영문**:
```
2D pixel art style, inferno cavern distant view,
massive lava lake, rising lava pillars,
volcanic rock silhouettes, fire particles,
red/orange gradient,
512x256 pixels, transparent background
```

---

#### Layer 2 - 중경 (바위 형성물)

**한글**:
```
2D 픽셀아트 스타일, 용암 동굴 중경,
검게 탄 바위 형성물, 용암 폭포,
냉각된 용암 흔적, 화산 가스 분출구,
빛나는 광석 결정, 뼈 더미,
512x256 픽셀, 투명 배경
```

**영문**:
```
2D pixel art style, inferno cavern middle ground,
charred rock formations, lava waterfalls,
cooled lava traces, volcanic gas vents,
glowing ore crystals, bone piles,
512x256 pixels, transparent background
```

---

#### Layer 1 - 근경 (전투 영역)

**한글**:
```
2D 픽셀아트 스타일, 용암 동굴 근경,
냉각된 용암 바닥, 금 간 바위,
작은 용암 웅덩이, 화산재,
불씨와 잔불, 녹은 금속 흔적,
512x256 픽셀, 투명 배경
```

**영문**:
```
2D pixel art style, inferno cavern foreground,
cooled lava floor, cracked rocks,
small lava puddles, volcanic ash,
embers and lingering flames, melted metal traces,
512x256 pixels, transparent background
```

---

## 1.3 스테이지 5: 어둠의 왕좌 (Throne of Darkness)

### 컨셉
- 최종 보스 영역
- 순수한 어둠과 타락
- 웅장하고 위압적인 분위기

### 배경 레이어

#### Layer 4 - 심연의 하늘

**한글**:
```
2D 픽셀아트 스타일, 어둠의 왕좌 하늘,
끝없는 어둠의 공허, 붉은/보라 소용돌이,
타락한 별들, 찢어진 차원 균열,
암흑 에너지 흐름,
512x256 픽셀, 패럴랙스용
```

**영문**:
```
2D pixel art style, throne of darkness sky,
endless dark void, red/purple vortex,
corrupted stars, torn dimensional rifts,
dark energy flows,
512x256 pixels, for parallax
```

**컬러**: #0D0D0D (공허), #4B0082 (암흑), #8B0000 (타락)

---

#### Layer 3 - 원경 (어둠의 성채)

**한글**:
```
2D 픽셀아트 스타일, 어둠의 왕좌 원경,
거대한 암흑 성채 실루엣, 뾰족한 첨탑,
떠다니는 바위 파편, 번개,
왕좌로 향하는 길,
512x256 픽셀, 투명 배경
```

**영문**:
```
2D pixel art style, throne of darkness distant view,
massive dark citadel silhouette, pointed spires,
floating rock fragments, lightning,
path leading to throne,
512x256 pixels, transparent background
```

---

#### Layer 2 - 중경 (타락한 성전)

**한글**:
```
2D 픽셀아트 스타일, 어둠의 왕좌 중경,
무너진 기둥과 제단, 타락한 조각상,
암흑 에너지 수정, 피로 물든 카펫,
사슬에 묶인 해골, 꺼진 촛대,
512x256 픽셀, 투명 배경
```

**영문**:
```
2D pixel art style, throne of darkness middle ground,
ruined pillars and altars, corrupted statues,
dark energy crystals, blood-stained carpet,
chained skeletons, extinguished candelabras,
512x256 pixels, transparent background
```

---

#### Layer 1 - 근경 (왕좌 앞)

**한글**:
```
2D 픽셀아트 스타일, 어둠의 왕좌 근경,
금이 간 대리석 바닥, 암흑 마법진,
흩어진 갑옷과 무기, 타락한 유물,
왕좌에서 뻗어나오는 암흑 에너지,
512x256 픽셀, 투명 배경
```

**영문**:
```
2D pixel art style, throne of darkness foreground,
cracked marble floor, dark magic circle,
scattered armor and weapons, corrupted relics,
dark energy emanating from throne,
512x256 pixels, transparent background
```

---

## 1.4 추가 스테이지 타일셋

### 마법 도서관 타일셋

**한글**:
```
2D 픽셀아트 스타일, 마법 도서관 플랫폼 타일셋,
1. 나무 책장 플랫폼 (밟을 수 있는 책장 상단)
2. 떠다니는 책 플랫폼 (움직이는)
3. 마법 발판 (룬 문양)
4. 사다리/밧줄
5. 깨지는 바닥 (종이 재질)
9-slice 형태, 각 16x16 픽셀, 투명 배경
```

**영문**:
```
2D pixel art style, magic library platform tileset,
1. Wooden bookshelf platform (walkable shelf top)
2. Floating book platform (moving)
3. Magic foothold (rune pattern)
4. Ladder/rope
5. Breakable floor (paper material)
9-slice format, each 16x16 pixels, transparent background
```

---

### 용암 동굴 타일셋

**한글**:
```
2D 픽셀아트 스타일, 용암 동굴 플랫폼 타일셋,
1. 현무암 플랫폼 (검은 바위)
2. 냉각 용암 플랫폼 (금이 간 표면)
3. 떠다니는 암석 (이동 플랫폼)
4. 용암 분출구 (간헐적 데미지)
5. 열 상승 기류 (높이 점프)
9-slice 형태, 각 16x16 픽셀, 투명 배경
```

**영문**:
```
2D pixel art style, inferno cavern platform tileset,
1. Basalt platform (black rock)
2. Cooled lava platform (cracked surface)
3. Floating rock (moving platform)
4. Lava geyser (intermittent damage)
5. Heat updraft (high jump)
9-slice format, each 16x16 pixels, transparent background
```

---

### 어둠의 왕좌 타일셋

**한글**:
```
2D 픽셀아트 스타일, 어둠의 왕좌 플랫폼 타일셋,
1. 타락한 대리석 플랫폼 (검은 금)
2. 암흑 에너지 발판 (보라색 발광)
3. 떠다니는 파편 (불안정)
4. 차원 균열 (순간이동)
5. 뼈 다리 (해골로 만들어진)
9-slice 형태, 각 16x16 픽셀, 투명 배경
```

**영문**:
```
2D pixel art style, throne of darkness platform tileset,
1. Corrupted marble platform (black veins)
2. Dark energy foothold (purple glow)
3. Floating debris (unstable)
4. Dimensional rift (teleport)
5. Bone bridge (made of skulls)
9-slice format, each 16x16 pixels, transparent background
```

---

# 2. 추가 몬스터 (스테이지별)

## 2.1 스테이지 3: 마법 도서관 몬스터

### 살아있는 책 (Living Tome)

**한글**:
```
2D 픽셀아트 스타일, 다크 판타지 살아있는 책 몬스터, SD 비율,
두꺼운 고서가 펼쳐져 날개처럼 퍼덕임,
표지에 눈 하나, 이빨이 달린 페이지,
책등에서 사슬 달랑거림, 마법 문자 떠다님,
정면 뷰, 클린 라인아트, 투명 배경, 32x32 해상도
```

**영문**:
```
2D pixel art style, dark fantasy living tome monster, SD proportion,
thick ancient book flapping open like wings,
single eye on cover, pages with teeth,
chains dangling from spine, floating magic letters,
front view, clean lineart, transparent background, 32x32 resolution
```

---

### 유령 학자 (Phantom Scholar)

**한글**:
```
2D 픽셀아트 스타일, 다크 판타지 유령 학자, SD 비율(2등신),
반투명 파란/하얀 유령, 낡은 로브 착용,
깃펜과 두루마리 들고 있음, 안경,
하반신이 연기처럼 흐릿함,
정면 뷰, 클린 라인아트, 투명 배경, 48x48 해상도
```

**영문**:
```
2D pixel art style, dark fantasy phantom scholar, SD proportion (2-head tall),
translucent blue/white ghost, wearing old robe,
holding quill and scroll, glasses,
lower body fading like smoke,
front view, clean lineart, transparent background, 48x48 resolution
```

---

### 마법 갑옷 (Enchanted Armor)

**한글**:
```
2D 픽셀아트 스타일, 다크 판타지 마법 갑옷, SD 비율(2등신),
주인 없이 움직이는 빈 갑옷, 파란 마법 빛,
갑옷 틈새로 에너지 새어나옴,
낡은 할버드 들고 있음, 위협적 자세,
정면 뷰, T포즈, 클린 라인아트, 투명 배경, 48x48 해상도
```

**영문**:
```
2D pixel art style, dark fantasy enchanted armor, SD proportion (2-head tall),
empty armor moving without owner, blue magic light,
energy seeping through armor gaps,
holding old halberd, threatening stance,
front view, T-pose, clean lineart, transparent background, 48x48 resolution
```

---

### 잉크 정령 (Ink Elemental)

**한글**:
```
2D 픽셀아트 스타일, 다크 판타지 잉크 정령, SD 비율,
검은 잉크로 이루어진 액체 형태,
촉수처럼 뻗어나가는 팔, 하얀 눈,
바닥에 잉크 자국 흘리며 이동,
정면 뷰, 클린 라인아트, 투명 배경, 32x32 해상도
```

**영문**:
```
2D pixel art style, dark fantasy ink elemental, SD proportion,
liquid form made of black ink,
arms extending like tentacles, white eyes,
leaving ink trails while moving,
front view, clean lineart, transparent background, 32x32 resolution
```

---

## 2.2 스테이지 4: 용암 동굴 몬스터

### 용암 슬라임 (Lava Slime)

**한글**:
```
2D 픽셀아트 스타일, 다크 판타지 용암 슬라임, SD 비율,
붉은/주황 반투명 젤리 몸체, 내부에 화염 핵,
표면에서 불꽃 튀어오름, 검게 굳은 껍질 일부,
바닥에 용암 흔적 남김,
정면 뷰, 클린 라인아트, 투명 배경, 32x32 해상도
```

**영문**:
```
2D pixel art style, dark fantasy lava slime, SD proportion,
red/orange translucent jelly body, fire core inside,
flames popping from surface, partially blackened shell,
leaving lava traces on ground,
front view, clean lineart, transparent background, 32x32 resolution
```

---

### 화염 임프 (Fire Imp)

**한글**:
```
2D 픽셀아트 스타일, 다크 판타지 화염 임프, SD 비율(2등신),
붉은 피부의 작은 악마, 작은 뿔과 박쥐 날개,
손에서 화염 피어오름, 사악한 미소,
뾰족한 꼬리, 날카로운 발톱,
정면 뷰, 클린 라인아트, 투명 배경, 32x32 해상도
```

**영문**:
```
2D pixel art style, dark fantasy fire imp, SD proportion (2-head tall),
small demon with red skin, small horns and bat wings,
flames rising from hands, evil grin,
pointed tail, sharp claws,
front view, clean lineart, transparent background, 32x32 resolution
```

---

### 용암 골렘 (Magma Golem)

**한글**:
```
2D 픽셀아트 스타일, 다크 판타지 용암 골렘, SD 비율(2.5등신),
검은 바위 몸체, 균열 사이로 용암 빛,
거대한 팔, 무거운 발걸음,
어깨와 등에서 화염 분출,
정면 뷰, T포즈, 클린 라인아트, 투명 배경, 64x64 해상도
```

**영문**:
```
2D pixel art style, dark fantasy magma golem, SD proportion (2.5-head tall),
black rock body, lava light through cracks,
massive arms, heavy footsteps,
flames erupting from shoulders and back,
front view, T-pose, clean lineart, transparent background, 64x64 resolution
```

---

### 불새 (Phoenix Hatchling)

**한글**:
```
2D 픽셀아트 스타일, 다크 판타지 어린 불새, SD 비율,
화염으로 이루어진 작은 새, 주황/빨강/노랑,
꼬리 깃털에서 불꽃 흩날림,
날개 펼친 비행 포즈,
정면 뷰, 클린 라인아트, 투명 배경, 24x24 해상도
```

**영문**:
```
2D pixel art style, dark fantasy phoenix hatchling, SD proportion,
small bird made of flames, orange/red/yellow,
flames scattering from tail feathers,
flying pose with spread wings,
front view, clean lineart, transparent background, 24x24 resolution
```

---

## 2.3 스테이지 5: 어둠의 왕좌 몬스터

### 그림자 망령 (Shadow Wraith)

**한글**:
```
2D 픽셀아트 스타일, 다크 판타지 그림자 망령, SD 비율(2등신),
검은 연기로 이루어진 인간형 유령,
붉게 빛나는 눈, 길게 늘어진 팔,
하반신이 연기처럼 흩어짐, 위협적인 자세,
정면 뷰, 클린 라인아트, 투명 배경, 48x48 해상도
```

**영문**:
```
2D pixel art style, dark fantasy shadow wraith, SD proportion (2-head tall),
humanoid ghost made of black smoke,
glowing red eyes, elongated arms,
lower body dissipating like smoke, threatening stance,
front view, clean lineart, transparent background, 48x48 resolution
```

---

### 암흑 기사 (Dark Knight)

**한글**:
```
2D 픽셀아트 스타일, 다크 판타지 암흑 기사, SD 비율(2등신),
검은 중갑옷, 암흑 에너지로 빛나는 눈,
대검과 방패, 갑옷에서 암흑 입자,
타락한 기사보다 작은 일반 몬스터 버전,
정면 뷰, T포즈, 클린 라인아트, 투명 배경, 48x48 해상도
```

**영문**:
```
2D pixel art style, dark fantasy dark knight, SD proportion (2-head tall),
black heavy armor, eyes glowing with dark energy,
greatsword and shield, dark particles from armor,
smaller regular monster version of fallen knight,
front view, T-pose, clean lineart, transparent background, 48x48 resolution
```

---

### 악마 (Lesser Demon)

**한글**:
```
2D 픽셀아트 스타일, 다크 판타지 하급 악마, SD 비율(2등신),
붉은/보라 피부, 큰 뿔, 박쥐 날개,
긴 꼬리, 날카로운 발톱과 이빨,
근육질 체형, 사악하고 흉폭한 표정,
정면 뷰, T포즈, 클린 라인아트, 투명 배경, 48x48 해상도
```

**영문**:
```
2D pixel art style, dark fantasy lesser demon, SD proportion (2-head tall),
red/purple skin, large horns, bat wings,
long tail, sharp claws and teeth,
muscular build, evil and fierce expression,
front view, T-pose, clean lineart, transparent background, 48x48 resolution
```

---

### 영혼 포식자 (Soul Devourer)

**한글**:
```
2D 픽셀아트 스타일, 다크 판타지 영혼 포식자, SD 비율,
떠다니는 어둠 구체, 입처럼 벌어진 균열,
주변에 빨려 들어가는 영혼 입자들,
촉수처럼 뻗어나오는 암흑 에너지,
정면 뷰, 클린 라인아트, 투명 배경, 48x48 해상도
```

**영문**:
```
2D pixel art style, dark fantasy soul devourer, SD proportion,
floating dark sphere, mouth-like crack opening,
soul particles being sucked in around it,
dark energy extending like tentacles,
front view, clean lineart, transparent background, 48x48 resolution
```

---

# 3. 추가 보스 (3~5)

## 3.1 스테이지 3 보스: 지식의 수호자 (Guardian of Knowledge)

### 컨셉
- 거대한 살아있는 책장/고서 집합체
- 지식을 지키다가 타락한 존재
- 책과 마법으로 공격

### 페이즈 1 (책장 형태)

**한글**:
```
2D 픽셀아트 스타일, 다크 판타지 지식의 수호자 보스, SD 비율,
거대한 나무 책장이 인간형으로 변형,
책들이 몸체를 이루고, 중앙에 빛나는 고서(코어),
팔처럼 뻗어나온 책장, 다리는 뿌리 형태,
얼굴 위치에 거대한 눈 하나,
정면 뷰, 위협적 포즈, 클린 라인아트, 투명 배경, 128x128 해상도
```

**영문**:
```
2D pixel art style, dark fantasy guardian of knowledge boss, SD proportion,
massive wooden bookshelf transformed into humanoid,
books forming the body, glowing ancient tome (core) in center,
bookshelves extending like arms, root-shaped legs,
single large eye at face position,
front view, threatening pose, clean lineart, transparent background, 128x128 resolution
```

---

### 페이즈 2 (해방된 지식)

**한글**:
```
2D 픽셀아트 스타일, 지식의 수호자 2페이즈,
책장 파편이 떠다니며 회전, 책들이 날아다님,
중앙에 노출된 고서 코어, 강한 마법 발광,
마법 문자들이 주변을 감쌈,
클린 라인아트, 투명 배경, 128x128 해상도
```

**영문**:
```
2D pixel art style, guardian of knowledge phase 2,
bookshelf fragments floating and rotating, books flying around,
exposed ancient tome core in center, strong magic glow,
magic letters surrounding,
clean lineart, transparent background, 128x128 resolution
```

---

## 3.2 스테이지 4 보스: 화산의 심장 (Heart of the Volcano)

### 컨셉
- 용암에서 태어난 원초적 존재
- 순수한 화염과 암석
- 분노의 화신

### 페이즈 1 (용암 거인)

**한글**:
```
2D 픽셀아트 스타일, 다크 판타지 화산의 심장 보스, SD 비율(2.5등신),
용암과 암석으로 이루어진 거대한 거인,
몸통 중앙에 빛나는 화염 핵(심장),
팔에서 용암이 흘러내림, 어깨에서 화염 분출,
눈에서 용암 빛, 발 주변 바닥이 녹음,
정면 뷰, T포즈, 클린 라인아트, 투명 배경, 128x128 해상도
```

**영문**:
```
2D pixel art style, dark fantasy heart of the volcano boss, SD proportion (2.5-head tall),
massive giant made of lava and rock,
glowing flame core (heart) in chest center,
lava dripping from arms, flames erupting from shoulders,
lava light in eyes, floor melting around feet,
front view, T-pose, clean lineart, transparent background, 128x128 resolution
```

---

### 페이즈 2 (폭발)

**한글**:
```
2D 픽셀아트 스타일, 화산의 심장 2페이즈,
외피가 부서지며 순수 화염 형태 노출,
용암 폭포처럼 흘러내리는 몸체,
화염 핵이 더 크고 밝게 빛남,
주변에 화염 소용돌이,
클린 라인아트, 투명 배경, 128x128 해상도
```

**영문**:
```
2D pixel art style, heart of the volcano phase 2,
outer shell breaking to expose pure flame form,
body flowing down like lava waterfall,
flame core larger and brighter,
fire vortex surrounding,
clean lineart, transparent background, 128x128 resolution
```

---

## 3.3 스테이지 5 보스: 어둠의 군주 (Lord of Darkness)

### 컨셉
- 최종 보스
- 순수한 어둠의 화신
- 여러 형태로 변신

### 페이즈 1 (인간형)

**한글**:
```
2D 픽셀아트 스타일, 다크 판타지 어둠의 군주 최종 보스, SD 비율(3등신),
위엄 있는 암흑 왕, 거대한 어둠의 왕관,
검은/보라 로브, 얼굴은 어둠 속 붉은 눈만 보임,
한 손에 암흑 에너지 지팡이, 망토 나부낌,
주변에 암흑 에너지 소용돌이,
정면 뷰, 위엄 있는 포즈, 클린 라인아트, 투명 배경, 128x128 해상도
```

**영문**:
```
2D pixel art style, dark fantasy lord of darkness final boss, SD proportion (3-head tall),
majestic dark king, massive crown of darkness,
black/purple robe, only red eyes visible in dark face,
dark energy staff in one hand, cape flowing,
dark energy vortex surrounding,
front view, majestic pose, clean lineart, transparent background, 128x128 resolution
```

---

### 페이즈 2 (반인반수)

**한글**:
```
2D 픽셀아트 스타일, 어둠의 군주 2페이즈,
상반신은 인간형, 하반신은 거대한 암흑 뱀,
더 큰 뿔, 네 개의 팔 (두 개는 무기, 두 개는 마법),
갑옷 파편, 더 강한 암흑 오라,
클린 라인아트, 투명 배경, 160x160 해상도
```

**영문**:
```
2D pixel art style, lord of darkness phase 2,
humanoid upper body, massive dark serpent lower body,
larger horns, four arms (two with weapons, two for magic),
armor fragments, stronger dark aura,
clean lineart, transparent background, 160x160 resolution
```

---

### 페이즈 3 (순수 암흑)

**한글**:
```
2D 픽셀아트 스타일, 어둠의 군주 최종 페이즈,
형체가 사라지고 순수한 암흑 에너지체,
중앙에 붉게 빛나는 핵, 여러 개의 눈,
촉수처럼 뻗어나가는 암흑,
공간을 뒤틀리게 하는 효과,
클린 라인아트, 투명 배경, 192x192 해상도
```

**영문**:
```
2D pixel art style, lord of darkness final phase,
physical form dissolved into pure dark energy entity,
red glowing core at center, multiple eyes,
darkness extending like tentacles,
space distortion effect,
clean lineart, transparent background, 192x192 resolution
```

---

# 4. 로그라이트 시스템 UI

## 4.1 런 선택/결과 화면

### 캐릭터 선택 화면

**한글**:
```
2D 픽셀아트 스타일, 캐릭터 선택 UI,
가로로 나열된 캐릭터 슬롯들,
선택된 캐릭터 확대 표시,
캐릭터 스탯/스킬 미리보기 패널,
잠금/해금 상태 표시,
마법 두루마리 테두리, 투명 배경, 320x180 크기
```

**영문**:
```
2D pixel art style, character selection UI,
character slots arranged horizontally,
enlarged display of selected character,
character stats/skills preview panel,
locked/unlocked status display,
magic scroll border, transparent background, 320x180 size
```

---

### 런 결과 화면

**한글**:
```
2D 픽셀아트 스타일, 런 결과 화면 UI,
상단: 성공/실패 배너,
중앙: 획득 아이템/골드 목록,
하단: 경험치 바, 언락된 항목,
양피지/고서 스타일 프레임,
투명 배경, 280x200 크기
```

**영문**:
```
2D pixel art style, run result screen UI,
top: success/failure banner,
center: acquired items/gold list,
bottom: experience bar, unlocked items,
parchment/tome style frame,
transparent background, 280x200 size
```

---

## 4.2 업그레이드/영구 강화 UI

### 메타 진행 트리

**한글**:
```
2D 픽셀아트 스타일, 메타 진행 스킬 트리 UI,
나무 형태의 업그레이드 경로,
각 노드: 잠금/해금/활성화 상태,
연결선, 필요 재화 표시,
마법 나무 테마, 투명 배경, 300x250 크기
```

**영문**:
```
2D pixel art style, meta progression skill tree UI,
tree-shaped upgrade path,
each node: locked/unlocked/activated states,
connection lines, required currency display,
magic tree theme, transparent background, 300x250 size
```

---

### 영구 업그레이드 아이콘 세트

**한글**:
```
2D 픽셀아트 스타일, 영구 업그레이드 아이콘 세트,
1. 체력 증가 (하트 +)
2. 마나 증가 (별 +)
3. 공격력 증가 (검 +)
4. 방어력 증가 (방패 +)
5. 이동속도 (날개 달린 부츠)
6. 쿨다운 감소 (모래시계)
7. 크리티컬 (별표 칼날)
8. 시작 골드 (금화)
각 24x24 픽셀, 투명 배경
```

**영문**:
```
2D pixel art style, permanent upgrade icon set,
1. Health increase (heart +)
2. Mana increase (star +)
3. Attack increase (sword +)
4. Defense increase (shield +)
5. Move speed (winged boots)
6. Cooldown reduce (hourglass)
7. Critical (starred blade)
8. Starting gold (gold coin)
each 24x24 pixels, transparent background
```

---

## 4.3 인런 보상 선택 UI

### 레벨업/보상 선택 창

**한글**:
```
2D 픽셀아트 스타일, 보상 선택 팝업 UI,
3개의 보상 카드 나란히 배치,
카드: 마법 카드 디자인, 테두리 발광,
아이콘 + 이름 + 설명 레이아웃,
선택 시 하이라이트, 투명 배경, 280x120 크기
```

**영문**:
```
2D pixel art style, reward selection popup UI,
3 reward cards arranged side by side,
cards: magic card design, glowing border,
icon + name + description layout,
highlight on selection, transparent background, 280x120 size
```

---

### 보상 카드 프레임 (등급별)

**한글**:
```
2D 픽셀아트 스타일, 보상 카드 프레임 세트,
1. 일반 (회색 테두리)
2. 희귀 (파란 테두리, 은은한 발광)
3. 전설 (보라 테두리, 강한 발광)
4. 유니크 (금 테두리, 무지개 효과)
마법 문양 장식, 각 64x80 픽셀, 투명 배경
```

**영문**:
```
2D pixel art style, reward card frame set,
1. Common (gray border)
2. Rare (blue border, soft glow)
3. Legendary (purple border, strong glow)
4. Unique (gold border, rainbow effect)
magic rune decoration, each 64x80 pixels, transparent background
```

---

## 4.4 패시브/렐릭 UI

### 패시브 슬롯 UI

**한글**:
```
2D 픽셀아트 스타일, 패시브 아이템 슬롯 UI,
화면 상단/측면에 배치되는 작은 슬롯들,
최대 6개 슬롯, 정사각형,
빈 슬롯/채워진 슬롯/비활성화 상태,
각 슬롯 24x24 픽셀, 투명 배경
```

**영문**:
```
2D pixel art style, passive item slot UI,
small slots placed at top/side of screen,
maximum 6 slots, square shaped,
empty/filled/disabled states,
each slot 24x24 pixels, transparent background
```

---

### 패시브/렐릭 아이콘 세트 (20종)

**한글**:
```
2D 픽셀아트 스타일, 패시브 렐릭 아이콘 세트, 20종,

공격 계열:
1. 불의 심장 (화염 데미지 증가)
2. 얼음 결정 (빙결 확률)
3. 번개 돌 (연쇄 번개)
4. 그림자 단검 (관통)
5. 피의 보석 (흡혈)

방어 계열:
6. 마나 방패 (피해 흡수)
7. 가시 갑옷 (반사 데미지)
8. 재생의 반지 (체력 재생)
9. 회피의 부적 (회피 확률)
10. 불사의 심장 (1회 부활)

유틸 계열:
11. 바람의 부츠 (이동속도)
12. 시간의 모래 (쿨다운)
13. 자석 (아이템 자동 수집)
14. 탐욕의 동전 (골드 증가)
15. 경험의 책 (경험치 증가)

특수 계열:
16. 연쇄의 고리 (스킬 연계)
17. 분노의 눈 (저체력 시 강화)
18. 완벽의 구슬 (풀피 시 강화)
19. 혼돈의 주사위 (랜덤 효과)
20. 융합의 돌 (스킬 합성)

각 24x24 픽셀, 투명 배경
```

**영문**:
```
2D pixel art style, passive relic icon set, 20 types,

Attack:
1. Heart of Fire (fire damage increase)
2. Ice Crystal (freeze chance)
3. Lightning Stone (chain lightning)
4. Shadow Dagger (pierce)
5. Blood Gem (lifesteal)

Defense:
6. Mana Shield (damage absorb)
7. Thorn Armor (reflect damage)
8. Ring of Regeneration (health regen)
9. Evasion Amulet (dodge chance)
10. Undying Heart (1 revival)

Utility:
11. Wind Boots (move speed)
12. Sands of Time (cooldown)
13. Magnet (auto collect)
14. Greed Coin (gold increase)
15. Tome of Experience (exp increase)

Special:
16. Chain Link (skill combo)
17. Eye of Fury (low HP buff)
18. Orb of Perfection (full HP buff)
19. Dice of Chaos (random effect)
20. Stone of Fusion (skill merge)

each 24x24 pixels, transparent background
```

---

# 5. 추가 플레이어 캐릭터 (3~4번째)

## 5.1 번개 마법사 (Tempest)

### 컨셉
- 속도와 연쇄 공격 특화
- 빠른 이동, 순간이동
- 적에게 전이되는 번개

### 기본 스프라이트

**한글**:
```
2D 픽셀아트 스타일, 다크 판타지 번개 마법사 캐릭터, SD 비율(2등신),
노란색과 하얀색의 로브, 번개 문양 장식,
한 손에 번개가 감긴 금속 지팡이,
뾰족하게 세운 파란 머리카락, 눈에서 전기 스파크,
로브 끝에서 정전기 입자 흩날림,
정면 뷰, T포즈, 클린 라인아트, 투명 배경, 64x64 해상도
```

**영문**:
```
2D pixel art style, dark fantasy lightning wizard character, SD proportion (2-head tall),
yellow and white robe, lightning pattern decoration,
metal staff wrapped with lightning in one hand,
spiky blue hair standing up, electric sparks from eyes,
static particles scattering from robe edges,
front view, T-pose, clean lineart, transparent background, 64x64 resolution
```

---

### 스킬 세트

1. **기본공격: 전격탄** - 빠른 연사의 작은 번개구
2. **스킬1: 체인 라이트닝** - 적에게 연쇄 전이
3. **스킬2: 썬더 스텝** - 번개와 함께 순간이동
4. **궁극기: 천둥 폭풍** - 광역 번개 세례

---

## 5.2 소환술사 (Summoner)

### 컨셉
- 소환수로 전투
- 직접 전투보다 지휘
- 다양한 소환수 조합

### 기본 스프라이트

**한글**:
```
2D 픽셀아트 스타일, 다크 판타지 소환술사 캐릭터, SD 비율(2등신),
초록색과 갈색의 드루이드풍 로브, 동물 뼈 장식,
한 손에 영혼이 감긴 나무 지팡이,
긴 녹색 머리카락에 꽃/잎 장식, 부드러운 표정,
발 주변에 작은 정령 1~2마리,
정면 뷰, T포즈, 클린 라인아트, 투명 배경, 64x64 해상도
```

**영문**:
```
2D pixel art style, dark fantasy summoner character, SD proportion (2-head tall),
green and brown druid-style robe, animal bone decorations,
wooden staff with spirits wrapped in one hand,
long green hair with flower/leaf accessories, gentle expression,
1-2 small spirits around feet,
front view, T-pose, clean lineart, transparent background, 64x64 resolution
```

---

### 소환수 스프라이트

**한글**:
```
2D 픽셀아트 스타일, 소환수 세트, SD 비율,

1. 작은 정령 (기본 소환수)
   - 둥글고 귀여운 빛나는 구체, 16x16

2. 골렘 (탱커)
   - 돌로 된 작은 인간형, 24x24

3. 늑대 영혼 (공격수)
   - 반투명한 파란 늑대, 24x24

4. 페어리 (힐러)
   - 작은 날개 달린 요정, 16x16

5. 피닉스 (궁극기)
   - 화려한 불새, 32x32

각 투명 배경, 애니메이션용
```

**영문**:
```
2D pixel art style, summon set, SD proportion,

1. Small Spirit (basic summon)
   - Round cute glowing orb, 16x16

2. Golem (tank)
   - Small stone humanoid, 24x24

3. Wolf Spirit (attacker)
   - Translucent blue wolf, 24x24

4. Fairy (healer)
   - Small winged fairy, 16x16

5. Phoenix (ultimate)
   - Magnificent firebird, 32x32

each transparent background, for animation
```

---

# 6. 스킬 아이콘

## 6.1 캐릭터별 스킬 아이콘

### 원소 마법사 (아르카나) 스킬 아이콘

**한글**:
```
2D 픽셀아트 스타일, 스킬 아이콘 세트, 원소 마법사,

1. 마나 볼트: 파란/보라 에너지 구체
2. 플레임 버스트: 부채꼴 화염
3. 프로스트 노바: 원형 얼음 파동
4. 아케인 스톰: 하늘에서 내리꽂히는 보라 소용돌이

정사각형 프레임, 각 32x32 픽셀, 투명 배경
```

**영문**:
```
2D pixel art style, skill icon set, elemental wizard,

1. Mana Bolt: blue/purple energy sphere
2. Flame Burst: fan-shaped flames
3. Frost Nova: circular ice wave
4. Arcane Storm: purple vortex striking from sky

square frame, each 32x32 pixels, transparent background
```

---

### 암흑 마법사 (말레피스) 스킬 아이콘

**한글**:
```
2D 픽셀아트 스타일, 스킬 아이콘 세트, 암흑 마법사,

1. 암흑 화살: 검보라 날카로운 화살
2. 생명력 흡수: 붉은 줄기 연결
3. 저주의 폭발: 해골 마법진
4. 영혼 수확: 유령들 소용돌이

정사각형 프레임, 각 32x32 픽셀, 투명 배경
```

**영문**:
```
2D pixel art style, skill icon set, dark sorcerer,

1. Shadow Arrow: dark purple sharp arrow
2. Life Drain: red stream connection
3. Curse Explosion: skull magic circle
4. Soul Harvest: ghosts swirling

square frame, each 32x32 pixels, transparent background
```

---

### 번개 마법사 (템페스트) 스킬 아이콘

**한글**:
```
2D 픽셀아트 스타일, 스킬 아이콘 세트, 번개 마법사,

1. 전격탄: 작은 번개 구체
2. 체인 라이트닝: 연결된 번개 화살표
3. 썬더 스텝: 번개 실루엣
4. 천둥 폭풍: 구름과 다중 번개

정사각형 프레임, 각 32x32 픽셀, 투명 배경
```

**영문**:
```
2D pixel art style, skill icon set, lightning wizard,

1. Spark Shot: small lightning sphere
2. Chain Lightning: connected lightning arrows
3. Thunder Step: lightning silhouette
4. Thunder Storm: clouds and multiple lightnings

square frame, each 32x32 pixels, transparent background
```

---

### 소환술사 스킬 아이콘

**한글**:
```
2D 픽셀아트 스타일, 스킬 아이콘 세트, 소환술사,

1. 정령 공격: 작은 정령 + 공격선
2. 골렘 소환: 돌 주먹
3. 늑대 소환: 늑대 머리
4. 피닉스 소환: 불새 날개

정사각형 프레임, 각 32x32 픽셀, 투명 배경
```

**영문**:
```
2D pixel art style, skill icon set, summoner,

1. Spirit Attack: small spirit + attack line
2. Summon Golem: stone fist
3. Summon Wolf: wolf head
4. Summon Phoenix: firebird wings

square frame, each 32x32 pixels, transparent background
```

---

# 7. NPC 상점/서비스

## 7.1 상점 NPC

### 포션 상인

**한글**:
```
2D 픽셀아트 스타일, 다크 판타지 포션 상인 NPC, SD 비율(2등신),
푸짐한 체형의 친근한 노인, 기름진 앞치마,
머리에 고글, 손에 끓는 비커,
등 뒤에 다양한 포션병이 달린 가방,
웃는 얼굴, 콧수염,
정면 뷰, 클린 라인아트, 투명 배경, 64x64 해상도
```

**영문**:
```
2D pixel art style, dark fantasy potion merchant NPC, SD proportion (2-head tall),
plump friendly elderly person, stained apron,
goggles on head, bubbling beaker in hand,
backpack with various potion bottles behind,
smiling face, mustache,
front view, clean lineart, transparent background, 64x64 resolution
```

---

### 대장장이

**한글**:
```
2D 픽셀아트 스타일, 다크 판타지 대장장이 NPC, SD 비율(2등신),
근육질의 드워프, 긴 갈색 수염,
가죽 앞치마, 한 손에 망치,
이마에 땀방울, 그을린 흔적,
옆에 모루, 뒤에 화로 실루엣,
정면 뷰, 클린 라인아트, 투명 배경, 64x64 해상도
```

**영문**:
```
2D pixel art style, dark fantasy blacksmith NPC, SD proportion (2-head tall),
muscular dwarf, long brown beard,
leather apron, hammer in one hand,
sweat on forehead, soot marks,
anvil beside, forge silhouette behind,
front view, clean lineart, transparent background, 64x64 resolution
```

---

### 마법 상인 (지팡이/로브)

**한글**:
```
2D 픽셀아트 스타일, 다크 판타지 마법 상인 NPC, SD 비율(2등신),
신비로운 분위기의 엘프 여성, 긴 은색 머리,
화려한 보라색 로브, 별과 달 장식,
손에 빛나는 수정 구슬,
가게 뒤에 진열된 지팡이들,
정면 뷰, 클린 라인아트, 투명 배경, 64x64 해상도
```

**영문**:
```
2D pixel art style, dark fantasy magic merchant NPC, SD proportion (2-head tall),
mysterious elf woman, long silver hair,
fancy purple robe, star and moon decorations,
glowing crystal ball in hand,
staffs displayed behind shop,
front view, clean lineart, transparent background, 64x64 resolution
```

---

## 7.2 서비스 NPC

### 회복 NPC (제단/샘)

**한글**:
```
2D 픽셀아트 스타일, 마법 회복 샘 오브젝트,
빛나는 파란/흰색 물이 솟아오르는 작은 분수,
주변에 마법 문양, 부유하는 빛 입자,
사용 가능/쿨다운 상태,
48x48 픽셀, 투명 배경
```

**영문**:
```
2D pixel art style, magic recovery fountain object,
small fountain with glowing blue/white water rising,
magic runes around, floating light particles,
available/cooldown states,
48x48 pixels, transparent background
```

---

### 축복 제단

**한글**:
```
2D 픽셀아트 스타일, 축복 제단 오브젝트,
고대의 돌 제단, 중앙에 빛나는 보석,
주변에 촛불, 마법 문양 바닥,
활성화 시 빛 기둥 효과,
64x64 픽셀, 투명 배경
```

**영문**:
```
2D pixel art style, blessing altar object,
ancient stone altar, glowing gem in center,
candles around, magic rune floor,
light pillar effect when activated,
64x64 pixels, transparent background
```

---

# 8. 미니맵/맵 시스템

## 8.1 미니맵 요소

### 미니맵 프레임

**한글**:
```
2D 픽셀아트 스타일, 미니맵 UI 프레임,
둥근 나침반 형태, 금속/나무 테두리,
방위표시, 마법 장식,
내부는 맵 표시 영역 (비움),
80x80 픽셀, 투명 배경
```

**영문**:
```
2D pixel art style, minimap UI frame,
round compass shape, metal/wood border,
direction markers, magic decoration,
interior is map display area (empty),
80x80 pixels, transparent background
```

---

### 맵 아이콘 세트

**한글**:
```
2D 픽셀아트 스타일, 미니맵 아이콘 세트,

1. 플레이어 위치: 파란 화살표
2. 적: 빨간 점
3. 보스: 빨간 해골
4. 상자: 노란 상자
5. 상점: 금화
6. 포탈: 보라 소용돌이
7. 회복: 초록 십자가
8. 비밀: 물음표
9. 잠긴 문: 자물쇠
10. 보스 방: 왕관

각 8x8 픽셀, 투명 배경
```

**영문**:
```
2D pixel art style, minimap icon set,

1. Player position: blue arrow
2. Enemy: red dot
3. Boss: red skull
4. Chest: yellow box
5. Shop: gold coin
6. Portal: purple vortex
7. Recovery: green cross
8. Secret: question mark
9. Locked door: padlock
10. Boss room: crown

each 8x8 pixels, transparent background
```

---

## 8.2 던전 맵 시스템

### 방 타입 아이콘

**한글**:
```
2D 픽셀아트 스타일, 던전 맵 방 아이콘 세트,

1. 일반 전투 방: 검 아이콘
2. 엘리트 방: 두 개의 검
3. 보스 방: 왕관/해골
4. 상점 방: 금화 가방
5. 보물 방: 황금 상자
6. 이벤트 방: 물음표
7. 휴식 방: 모닥불
8. 시작 방: 집
9. 탈출 방: 포탈

각 16x16 픽셀, 투명 배경
```

**영문**:
```
2D pixel art style, dungeon map room icon set,

1. Normal combat room: sword icon
2. Elite room: two swords
3. Boss room: crown/skull
4. Shop room: gold bag
5. Treasure room: golden chest
6. Event room: question mark
7. Rest room: campfire
8. Start room: house
9. Exit room: portal

each 16x16 pixels, transparent background
```

---

### 던전 맵 연결선

**한글**:
```
2D 픽셀아트 스타일, 던전 맵 경로 연결선,
점선/실선 버전, 가로/세로/대각선,
활성화/비활성화 색상,
4픽셀 너비, 투명 배경
```

**영문**:
```
2D pixel art style, dungeon map path connection lines,
dotted/solid line versions, horizontal/vertical/diagonal,
active/inactive colors,
4 pixel width, transparent background
```

---

# 9. 타이틀/로딩 화면

## 9.1 타이틀 화면

### 게임 로고

**한글**:
```
2D 픽셀아트 스타일, GASPT 게임 로고,
마법적인 서체, 보라색/금색 그라데이션,
문자 주변에 마법 입자 효과,
"마법사" 테마를 반영한 디자인,
지팡이/별/마법진 장식 요소,
256x64 픽셀, 투명 배경
```

**영문**:
```
2D pixel art style, GASPT game logo,
magical font, purple/gold gradient,
magic particle effect around letters,
design reflecting "wizard" theme,
staff/star/magic circle decorative elements,
256x64 pixels, transparent background
```

---

### 타이틀 배경

**한글**:
```
2D 픽셀아트 스타일, 게임 타이틀 배경,
어둠 속에서 빛나는 마법 탑,
별이 빛나는 밤하늘, 떠다니는 마나 입자,
전경에 마법사 실루엣,
신비롭고 모험적인 분위기,
320x180 픽셀 (16:9 비율)
```

**영문**:
```
2D pixel art style, game title background,
magic tower glowing in darkness,
starry night sky, floating mana particles,
wizard silhouette in foreground,
mysterious and adventurous atmosphere,
320x180 pixels (16:9 ratio)
```

---

## 9.2 로딩 화면

### 로딩 애니메이션

**한글**:
```
2D 픽셀아트 스타일, 로딩 애니메이션 스프라이트,
회전하는 마법진/마나 구체,
8프레임 루프, 부드러운 회전,
파란/보라 발광 효과,
32x32 픽셀, 투명 배경
```

**영문**:
```
2D pixel art style, loading animation sprite,
rotating magic circle/mana orb,
8-frame loop, smooth rotation,
blue/purple glow effect,
32x32 pixels, transparent background
```

---

### 로딩 팁 프레임

**한글**:
```
2D 픽셀아트 스타일, 로딩 팁 표시 프레임,
두루마리/양피지 스타일,
텍스트 표시 영역, 마법 테두리,
200x48 픽셀, 투명 배경
```

**영문**:
```
2D pixel art style, loading tip display frame,
scroll/parchment style,
text display area, magic border,
200x48 pixels, transparent background
```

---

# 10. 업적/도감 시스템

## 10.1 업적 UI

### 업적 배지 프레임 (등급별)

**한글**:
```
2D 픽셀아트 스타일, 업적 배지 프레임 세트,
1. 브론즈: 구리색 원형 메달
2. 실버: 은색 원형 메달
3. 골드: 금색 원형 메달
4. 플래티넘: 보라색 빛나는 메달
잠금 상태는 회색 실루엣,
각 32x32 픽셀, 투명 배경
```

**영문**:
```
2D pixel art style, achievement badge frame set,
1. Bronze: copper circular medal
2. Silver: silver circular medal
3. Gold: golden circular medal
4. Platinum: purple glowing medal
locked state is gray silhouette,
each 32x32 pixels, transparent background
```

---

### 업적 아이콘 (카테고리별)

**한글**:
```
2D 픽셀아트 스타일, 업적 아이콘 세트, 카테고리별,

전투:
1. 첫 승리: 검
2. 100킬: 해골 더미
3. 보스 처치: 왕관 깨진
4. 무피해 클리어: 방패 빛남

탐험:
5. 첫 던전: 문
6. 비밀 발견: 물음표
7. 모든 방 탐험: 지도
8. 숨겨진 보물: 상자

수집:
9. 첫 아이템: 별
10. 풀셋: 완전한 갑옷
11. 모든 캐릭터: 인물들
12. 레어 수집: 보석

각 24x24 픽셀, 투명 배경
```

**영문**:
```
2D pixel art style, achievement icon set, by category,

Combat:
1. First victory: sword
2. 100 kills: skull pile
3. Boss kill: broken crown
4. No damage clear: glowing shield

Exploration:
5. First dungeon: door
6. Secret found: question mark
7. All rooms explored: map
8. Hidden treasure: chest

Collection:
9. First item: star
10. Full set: complete armor
11. All characters: figures
12. Rare collection: gems

each 24x24 pixels, transparent background
```

---

## 10.2 도감/백과사전 UI

### 몬스터 도감 카드

**한글**:
```
2D 픽셀아트 스타일, 몬스터 도감 카드 프레임,
상단: 몬스터 이미지 영역 (64x64)
중단: 이름/등급 표시
하단: 정보 텍스트 영역
양피지 스타일, 해골/뿔 장식 테두리,
96x128 픽셀, 투명 배경
```

**영문**:
```
2D pixel art style, monster encyclopedia card frame,
top: monster image area (64x64)
middle: name/grade display
bottom: info text area
parchment style, skull/horn decorated border,
96x128 pixels, transparent background
```

---

### 아이템 도감 카드

**한글**:
```
2D 픽셀아트 스타일, 아이템 도감 카드 프레임,
상단: 아이템 이미지 영역 (48x48)
중단: 이름/등급/타입 표시
하단: 효과 설명 영역
마법 두루마리 스타일, 별/마법진 장식,
80x120 픽셀, 투명 배경
```

**영문**:
```
2D pixel art style, item encyclopedia card frame,
top: item image area (48x48)
middle: name/grade/type display
bottom: effect description area
magic scroll style, star/magic circle decoration,
80x120 pixels, transparent background
```

---

# 11. 추가 이펙트

## 11.1 레벨업/강화 이펙트

### 레벨업 이펙트

**한글**:
```
2D 픽셀아트 스타일, 레벨업 이펙트,
캐릭터 주변 빛 기둥 상승,
금색/흰색 입자 흩날림,
마법진이 발밑에서 확산,
8프레임 애니메이션, 64x96 크기, 투명 배경
```

**영문**:
```
2D pixel art style, level up effect,
light pillar rising around character,
gold/white particles scattering,
magic circle spreading from feet,
8-frame animation, 64x96 size, transparent background
```

---

### 장비 강화 이펙트

**한글**:
```
2D 픽셀아트 스타일, 장비 강화 이펙트,
아이템 주변 회전하는 마법 문양,
별 모양 반짝임, 에너지 흡수 효과,
성공 시 폭발적 빛, 실패 시 연기,
6프레임 애니메이션, 48x48 크기, 투명 배경
```

**영문**:
```
2D pixel art style, equipment enhance effect,
rotating magic runes around item,
star-shaped sparkles, energy absorption effect,
explosive light on success, smoke on failure,
6-frame animation, 48x48 size, transparent background
```

---

## 11.2 UI 이펙트

### 버튼 호버/클릭 이펙트

**한글**:
```
2D 픽셀아트 스타일, UI 버튼 이펙트,
1. 호버: 테두리 발광, 마법 입자
2. 클릭: 누르는 효과, 빛 번쩍
3. 비활성: 어두운 오버레이
각 상태별 프레임, 투명 배경
```

**영문**:
```
2D pixel art style, UI button effects,
1. Hover: border glow, magic particles
2. Click: press effect, light flash
3. Disabled: dark overlay
frames for each state, transparent background
```

---

### 획득/드롭 이펙트

**한글**:
```
2D 픽셀아트 스타일, 아이템 획득/드롭 이펙트,
1. 드롭: 위에서 떨어지며 바운스
2. 대기: 살짝 위아래 부유
3. 획득: 플레이어 방향으로 빨려감
4. 획득 완료: 반짝 사라짐
각 4-6프레임, 투명 배경
```

**영문**:
```
2D pixel art style, item acquire/drop effects,
1. Drop: falling from above with bounce
2. Idle: slight up-down floating
3. Acquire: being pulled toward player
4. Acquire complete: sparkle vanish
each 4-6 frames, transparent background
```

---

## 11.3 환경 이펙트

### 비/눈 효과

**한글**:
```
2D 픽셀아트 스타일, 날씨 효과 레이어,
1. 비: 대각선 빗줄기, 물방울 튀김
2. 눈: 천천히 내리는 눈송이
3. 재/먼지: 떠다니는 입자
각 루프 가능한 타일, 투명 배경
```

**영문**:
```
2D pixel art style, weather effect layer,
1. Rain: diagonal rain streaks, water splashes
2. Snow: slowly falling snowflakes
3. Ash/dust: floating particles
each loopable tile, transparent background
```

---

### 조명 효과

**한글**:
```
2D 픽셀아트 스타일, 조명 효과 오버레이,
1. 횃불 빛: 따뜻한 주황 원형 그라데이션
2. 마법 빛: 파란/보라 원형 발광
3. 용암 빛: 붉은 맥동하는 빛
각 2-4프레임 깜빡임, 반투명, 다양한 크기
```

**영문**:
```
2D pixel art style, lighting effect overlay,
1. Torch light: warm orange circular gradient
2. Magic light: blue/purple circular glow
3. Lava light: red pulsating light
each 2-4 frame flicker, semi-transparent, various sizes
```

---

# 12. 애니메이션 스프라이트 시트

## 12.1 플레이어 애니메이션

### 공통 애니메이션 목록

```
모든 플레이어 캐릭터에 필요한 애니메이션:

1. Idle (대기) - 4프레임
2. Run (달리기) - 6프레임
3. Jump (점프) - 3프레임 (상승/정점/하강)
4. Fall (낙하) - 2프레임
5. Land (착지) - 2프레임
6. Attack1 (기본공격) - 4프레임
7. Attack2 (스킬1) - 6프레임
8. Attack3 (스킬2) - 6프레임
9. Ultimate (궁극기) - 8프레임
10. Hit (피격) - 2프레임
11. Death (사망) - 6프레임
12. Dash (대시) - 3프레임
```

---

### 애니메이션 스프라이트 시트 프롬프트 (원소 마법사 예시)

**한글**:
```
2D 픽셀아트 스타일, 원소 마법사 캐릭터 스프라이트 시트,
SD 비율(2등신), 보라색/파란색 로브, 크리스탈 지팡이,

Idle: 숨쉬기 움직임, 로브 살랑, 4프레임
Run: 달리는 포즈, 로브 나부낌, 6프레임
Jump: 점프 준비 → 공중 → 착지 준비
Attack: 지팡이 휘두르며 마법 시전

각 프레임 64x64 픽셀, 가로로 나열,
클린 라인아트, 투명 배경
```

**영문**:
```
2D pixel art style, elemental wizard character sprite sheet,
SD proportion (2-head tall), purple/blue robe, crystal staff,

Idle: breathing movement, robe swaying, 4 frames
Run: running pose, robe flowing, 6 frames
Jump: jump prep → airborne → landing prep
Attack: staff swinging with spell casting

each frame 64x64 pixels, arranged horizontally,
clean lineart, transparent background
```

---

## 12.2 몬스터 애니메이션

### 일반 몬스터 애니메이션 목록

```
모든 몬스터에 필요한 기본 애니메이션:

1. Idle (대기) - 4프레임
2. Walk/Move (이동) - 4프레임
3. Attack (공격) - 4프레임
4. Hit (피격) - 2프레임
5. Death (사망) - 4프레임
6. Spawn (등장) - 4프레임 (선택)
```

---

### 보스 추가 애니메이션

```
보스에게 추가로 필요한 애니메이션:

1. Special Attack 1~3 - 각 6프레임
2. Phase Transition - 8프레임
3. Enrage - 4프레임
4. Stagger (경직) - 2프레임
```

---

# 요약: 추가 필요 아트 에셋

## 스테이지/배경

| 항목 | 개수 | 우선순위 |
|------|------|----------|
| 스테이지 3 배경 (4레이어) | 4 | 보통 |
| 스테이지 4 배경 (4레이어) | 4 | 보통 |
| 스테이지 5 배경 (4레이어) | 4 | 보통 |
| 추가 타일셋 (3세트) | 3 | 보통 |

## 캐릭터/몬스터

| 항목 | 개수 | 우선순위 |
|------|------|----------|
| 추가 플레이어 캐릭터 | 2 | 보통 |
| 스테이지 3 몬스터 | 4 | 보통 |
| 스테이지 4 몬스터 | 4 | 보통 |
| 스테이지 5 몬스터 | 4 | 보통 |
| 추가 보스 | 3 | 보통 |

## UI/시스템

| 항목 | 개수 | 우선순위 |
|------|------|----------|
| 로그라이트 UI | 10+ | 높음 |
| 스킬 아이콘 | 16+ | 높음 |
| 미니맵 요소 | 15+ | 보통 |
| 타이틀/로딩 | 5+ | 낮음 |
| 업적/도감 UI | 10+ | 낮음 |

## NPC/이펙트

| 항목 | 개수 | 우선순위 |
|------|------|----------|
| 상점/서비스 NPC | 5+ | 보통 |
| 추가 이펙트 | 20+ | 보통 |
| 애니메이션 시트 | 캐릭터당 12종 | 높음 |

---

**문서 버전**: v1.0
**최종 업데이트**: 2025-11-30
**작성자**: Claude Code Assistant

---

*이 문서는 기존 기획에서 누락된 아트 에셋을 보완하기 위한 추가 기획입니다.*
