# 다음 아트 에셋 기획

> **프로젝트**: GASPT (마법사 테마 2D 플랫포머 액션 로그라이트)
> **작성일**: 2025-11-30
> **목적**: 추가로 필요한 아트 에셋 기획 및 프롬프트

---

## 목차

1. [배경/환경 아트](#1-배경환경-아트)
2. [타일셋/플랫폼](#2-타일셋플랫폼)
3. [UI/HUD](#3-uihud)
4. [아이템/장비 아이콘](#4-아이템장비-아이콘)
5. [추가 NPC](#5-추가-npc)
6. [오브젝트/소품](#6-오브젝트소품)
7. [추가 이펙트](#7-추가-이펙트)

---

# 1. 배경/환경 아트

## 1.1 스테이지 1: 독의 늪지대 (Toxic Swamp)

### 배경 레이어 구성

```
Layer 4 (최원경): 어두운 하늘, 달/별
Layer 3 (원경): 죽은 나무 실루엣, 안개
Layer 2 (중경): 늪지 풍경, 독버섯, 썩은 나무
Layer 1 (근경): 독 웅덩이, 풀, 바위
Layer 0 (전경): 안개 효과, 독 입자
```

### 프롬프트

#### Layer 4 - 하늘 (최원경)

**한글**:
```
2D 픽셀아트 스타일, 다크 판타지 늪지대 밤하늘 배경,
어두운 보라색/녹색 톤의 불길한 하늘,
구름 사이로 비치는 초승달, 희미한 별들,
독 안개가 하늘까지 올라온 느낌,
패럴랙스 스크롤용, 수평 타일링 가능,
512x256 크기, 게임 배경용
```

**영문**:
```
2D pixel art style, dark fantasy swamp night sky background,
ominous sky in dark purple/green tones,
crescent moon visible through clouds, faint stars,
poison mist rising up to the sky,
for parallax scrolling, horizontally tileable,
512x256 size, game background asset
```

---

#### Layer 3 - 원경 (죽은 숲)

**한글**:
```
2D 픽셀아트 스타일, 다크 판타지 늪지대 원경,
죽은 나무들의 검은 실루엣, 가지가 뒤틀린 형태,
짙은 안개 속에 희미하게 보이는 나무들,
녹색/보라색 안개 효과,
패럴랙스 스크롤용, 수평 타일링 가능,
512x256 크기, 투명 배경
```

**영문**:
```
2D pixel art style, dark fantasy swamp distant view,
black silhouettes of dead trees, twisted branches,
trees faintly visible through thick fog,
green/purple fog effect,
for parallax scrolling, horizontally tileable,
512x256 size, transparent background
```

---

#### Layer 2 - 중경 (늪지 풍경)

**한글**:
```
2D 픽셀아트 스타일, 다크 판타지 늪지대 중경,
썩은 나무와 독버섯 군락, 늪지 물웅덩이,
나무에 매달린 이끼, 부글거리는 독 웅덩이,
녹색 발광 버섯, 죽은 동물 뼈,
패럴랙스 스크롤용, 수평 타일링 가능,
512x256 크기, 투명 배경
```

**영문**:
```
2D pixel art style, dark fantasy swamp middle ground,
rotting trees and poison mushroom clusters, swamp water pools,
moss hanging from trees, bubbling toxic puddles,
green glowing mushrooms, dead animal bones,
for parallax scrolling, horizontally tileable,
512x256 size, transparent background
```

---

#### Layer 1 - 근경 (전투 영역)

**한글**:
```
2D 픽셀아트 스타일, 다크 판타지 늪지대 근경,
캐릭터가 서있는 영역 배경, 독 웅덩이와 바위,
늪지 풀과 갈대, 독버섯, 썩은 나무 그루터기,
디테일한 텍스처, 상호작용 오브젝트와 구분되는 배경,
패럴랙스 스크롤용, 512x256 크기, 투명 배경
```

**영문**:
```
2D pixel art style, dark fantasy swamp foreground,
background for character standing area, toxic puddles and rocks,
swamp grass and reeds, poison mushrooms, rotting tree stumps,
detailed textures, background distinguishable from interactive objects,
for parallax scrolling, 512x256 size, transparent background
```

---

#### 컬러 팔레트 - 늪지대

| 요소 | 색상 코드 | 설명 |
|------|----------|------|
| 하늘 (어두운) | #1A1A2E | 다크 네이비 |
| 하늘 (밝은) | #2D3A4A | 다크 슬레이트 |
| 안개 | #3D5C3D | 다크 그린 |
| 독 발광 | #32CD32 | 라임 그린 |
| 물 | #2F4F4F | 다크 슬레이트 그레이 |
| 나무 | #3C280D | 다크 브라운 |
| 버섯 발광 | #ADFF2F | 그린 옐로우 |

---

## 1.2 스테이지 2: 버려진 성채 (Abandoned Citadel)

### 배경 레이어 구성

```
Layer 4 (최원경): 폭풍우 하늘, 번개
Layer 3 (원경): 무너진 성벽, 탑 실루엣
Layer 2 (중경): 성 내부 구조물, 기둥, 아치
Layer 1 (근경): 바닥 잔해, 깨진 갑옷/무기
Layer 0 (전경): 먼지 입자, 암흑 에너지
```

### 프롬프트

#### Layer 4 - 폭풍우 하늘

**한글**:
```
2D 픽셀아트 스타일, 다크 판타지 폭풍우 하늘,
검은 구름과 번개, 붉은/보라색 번개 효과,
불길한 소용돌이 구름, 비 내리는 효과,
암흑 에너지가 하늘을 뒤덮은 느낌,
패럴랙스 스크롤용, 512x256 크기
```

**영문**:
```
2D pixel art style, dark fantasy stormy sky,
black clouds and lightning, red/purple lightning effects,
ominous swirling clouds, rain effect,
dark energy covering the sky,
for parallax scrolling, 512x256 size
```

---

#### Layer 3 - 원경 (무너진 성)

**한글**:
```
2D 픽셀아트 스타일, 다크 판타지 무너진 성채 원경,
거대한 성벽과 탑의 실루엣, 일부 무너진 구조,
암흑 에너지가 성 주변을 감싸고 있음,
불타는 횃불 빛, 붉은 창문 빛,
패럴랙스 스크롤용, 512x256 크기, 투명 배경
```

**영문**:
```
2D pixel art style, dark fantasy ruined citadel distant view,
silhouettes of massive walls and towers, partially collapsed structures,
dark energy surrounding the castle,
burning torch lights, red window lights,
for parallax scrolling, 512x256 size, transparent background
```

---

#### Layer 2 - 중경 (성 내부)

**한글**:
```
2D 픽셀아트 스타일, 다크 판타지 성채 내부 중경,
거대한 석조 기둥, 아치형 통로, 무너진 벽,
찢어진 깃발과 태피스트리, 거미줄,
암흑 에너지 균열, 깨진 스테인드글라스,
패럴랙스 스크롤용, 512x256 크기, 투명 배경
```

**영문**:
```
2D pixel art style, dark fantasy citadel interior middle ground,
massive stone pillars, arched passages, collapsed walls,
torn flags and tapestries, spider webs,
dark energy cracks, broken stained glass,
for parallax scrolling, 512x256 size, transparent background
```

---

#### Layer 1 - 근경 (전투 영역)

**한글**:
```
2D 픽셀아트 스타일, 다크 판타지 성채 근경,
바닥의 돌 잔해, 깨진 갑옷과 무기 조각,
해골과 뼈, 피 얼룩, 금이 간 바닥,
암흑 에너지가 스며든 균열,
512x256 크기, 투명 배경
```

**영문**:
```
2D pixel art style, dark fantasy citadel foreground,
stone debris on floor, broken armor and weapon pieces,
skulls and bones, blood stains, cracked floor,
cracks infused with dark energy,
512x256 size, transparent background
```

---

#### 컬러 팔레트 - 성채

| 요소 | 색상 코드 | 설명 |
|------|----------|------|
| 하늘 | #0D0D1A | 거의 검정 |
| 번개 | #DC143C | 크림슨 |
| 석재 (밝은) | #696969 | 딤 그레이 |
| 석재 (어두운) | #2F2F2F | 다크 그레이 |
| 암흑 에너지 | #4B0082 | 인디고 |
| 횃불 빛 | #FF6600 | 오렌지 |
| 피 | #8B0000 | 다크 레드 |

---

## 1.3 시작 마을/로비 (Wizard's Haven)

### 배경 레이어 구성

```
Layer 3 (원경): 마법 탑, 별이 빛나는 하늘
Layer 2 (중경): 마법 상점들, 포션 가게, 대장간
Layer 1 (근경): 길, 장식물, 가로등
Layer 0 (전경): 마법 입자, 반딧불이
```

### 프롬프트

#### 마을 전체 배경

**한글**:
```
2D 픽셀아트 스타일, 판타지 마법사 마을 배경,
따뜻한 분위기의 밤 풍경, 별이 빛나는 하늘,
중앙에 거대한 마법 탑, 주변에 아기자기한 상점들,
마법 가로등에서 은은한 빛, 공중에 떠다니는 마나 입자,
포션 가게, 대장간, 마법 서점 등 건물들,
평화롭고 안전한 분위기, 512x256 크기
```

**영문**:
```
2D pixel art style, fantasy wizard village background,
warm night atmosphere, starry sky,
large magic tower in center, cute shops around,
soft light from magic street lamps, floating mana particles,
potion shop, blacksmith, magic bookstore buildings,
peaceful and safe atmosphere, 512x256 size
```

---

#### 컬러 팔레트 - 마을

| 요소 | 색상 코드 | 설명 |
|------|----------|------|
| 하늘 | #191970 | 미드나잇 블루 |
| 별 | #FFD700 | 골드 |
| 건물 | #DEB887 | 벌리우드 |
| 지붕 | #8B4513 | 새들 브라운 |
| 가로등 빛 | #FFA500 | 오렌지 |
| 마나 입자 | #E6E6FA | 라벤더 |

---

# 2. 타일셋/플랫폼

## 2.1 늪지대 타일셋

### 기본 플랫폼 타일

**한글**:
```
2D 픽셀아트 스타일, 다크 판타지 늪지대 플랫폼 타일셋,
이끼 낀 돌 플랫폼, 썩은 나무 플랫폼, 독버섯 플랫폼,
각 타일 16x16 또는 32x32 크기,
타일 연결 가능한 9-slice 형태,
상단/중단/하단, 좌측/중앙/우측 모서리 포함,
투명 배경, 게임 타일셋용
```

**영문**:
```
2D pixel art style, dark fantasy swamp platform tileset,
mossy stone platform, rotting wood platform, poison mushroom platform,
each tile 16x16 or 32x32 size,
9-slice tileable format,
top/middle/bottom, left/center/right corners included,
transparent background, game tileset asset
```

---

### 특수 타일

**한글**:
```
2D 픽셀아트 스타일, 늪지대 특수 타일,
1. 독 웅덩이 타일 (애니메이션용 부글거림)
2. 무너지는 플랫폼 (금 간 나무)
3. 점프대 (탄성있는 버섯)
4. 가시 함정 (독 가시)
5. 이동 플랫폼 (떠다니는 연잎)
각 32x32 크기, 투명 배경
```

**영문**:
```
2D pixel art style, swamp special tiles,
1. Toxic puddle tile (bubbling animation)
2. Crumbling platform (cracked wood)
3. Jump pad (bouncy mushroom)
4. Spike trap (poison thorns)
5. Moving platform (floating lily pad)
each 32x32 size, transparent background
```

---

## 2.2 성채 타일셋

### 기본 플랫폼 타일

**한글**:
```
2D 픽셀아트 스타일, 다크 판타지 성채 플랫폼 타일셋,
석조 플랫폼, 무너진 벽 플랫폼, 금속 격자 플랫폼,
각 타일 16x16 또는 32x32 크기,
9-slice 타일 연결 형태,
균열과 피 얼룩 디테일 포함,
투명 배경, 게임 타일셋용
```

**영문**:
```
2D pixel art style, dark fantasy citadel platform tileset,
stone platform, ruined wall platform, metal grate platform,
each tile 16x16 or 32x32 size,
9-slice tileable format,
cracks and blood stain details included,
transparent background, game tileset asset
```

---

### 특수 타일

**한글**:
```
2D 픽셀아트 스타일, 성채 특수 타일,
1. 암흑 균열 (데미지 장판)
2. 무너지는 바닥 (금 간 석재)
3. 점프대 (마법 룬 플랫폼)
4. 가시 함정 (창 함정)
5. 이동 플랫폼 (움직이는 석판)
6. 사슬/엘리베이터
각 32x32 크기, 투명 배경
```

**영문**:
```
2D pixel art style, citadel special tiles,
1. Dark crack (damage zone)
2. Crumbling floor (cracked stone)
3. Jump pad (magic rune platform)
4. Spike trap (spear trap)
5. Moving platform (moving stone slab)
6. Chain/elevator
each 32x32 size, transparent background
```

---

# 3. UI/HUD

## 3.1 메인 HUD

### 체력바/마나바

**한글**:
```
2D 픽셀아트 스타일, 게임 HUD 체력바와 마나바,
체력바: 붉은색 게이지, 어두운 테두리, 하트 아이콘,
마나바: 파란색/보라색 게이지, 별 또는 마나 결정 아이콘,
게이지 감소 시 깜빡임 효과용 프레임,
다크 판타지 스타일 장식 테두리,
투명 배경, 각 200x32 크기
```

**영문**:
```
2D pixel art style, game HUD health bar and mana bar,
health bar: red gauge, dark border, heart icon,
mana bar: blue/purple gauge, star or mana crystal icon,
frames for blinking effect when gauge decreases,
dark fantasy style decorative border,
transparent background, each 200x32 size
```

---

### 스킬 슬롯 UI

**한글**:
```
2D 픽셀아트 스타일, 게임 스킬 슬롯 UI,
4개 스킬 슬롯 (기본공격, 스킬1, 스킬2, 궁극기),
마법 문양 테두리, 쿨다운 오버레이용 어두운 버전,
선택됨/비활성화/쿨다운 상태 각각,
키 바인딩 표시 영역,
투명 배경, 각 슬롯 48x48 크기
```

**영문**:
```
2D pixel art style, game skill slot UI,
4 skill slots (basic attack, skill1, skill2, ultimate),
magic rune border, dark version for cooldown overlay,
selected/disabled/cooldown states each,
key binding display area,
transparent background, each slot 48x48 size
```

---

### 보스 체력바

**한글**:
```
2D 픽셀아트 스타일, 보스 체력바 UI,
화면 상단에 표시되는 긴 형태,
보스 이름 표시 영역, 페이즈 표시,
붉은/보라색 게이지, 해골/뿔 장식,
위협적인 다크 판타지 스타일,
투명 배경, 400x48 크기
```

**영문**:
```
2D pixel art style, boss health bar UI,
long format displayed at top of screen,
boss name display area, phase indicator,
red/purple gauge, skull/horn decorations,
menacing dark fantasy style,
transparent background, 400x48 size
```

---

## 3.2 메뉴 UI

### 메인 메뉴 버튼

**한글**:
```
2D 픽셀아트 스타일, 게임 메인 메뉴 버튼 세트,
시작, 설정, 종료 버튼,
기본/호버/클릭 상태 각각,
마법 책 또는 두루마리 스타일,
보라색/금색 테두리, 마법 문양,
투명 배경, 각 160x48 크기
```

**영문**:
```
2D pixel art style, game main menu button set,
start, settings, exit buttons,
normal/hover/click states each,
magic book or scroll style,
purple/gold border, magic runes,
transparent background, each 160x48 size
```

---

### 인벤토리 UI

**한글**:
```
2D 픽셀아트 스타일, 게임 인벤토리 UI,
아이템 슬롯 그리드, 각 슬롯 32x32,
장비 슬롯 (무기, 방어구, 악세서리),
가방/상자 스타일 프레임,
선택/장착됨/잠금 상태,
투명 배경, 전체 256x256 크기
```

**영문**:
```
2D pixel art style, game inventory UI,
item slot grid, each slot 32x32,
equipment slots (weapon, armor, accessory),
bag/chest style frame,
selected/equipped/locked states,
transparent background, total 256x256 size
```

---

### 대화창 UI

**한글**:
```
2D 픽셀아트 스타일, 게임 대화창 UI,
NPC 대화용 말풍선/프레임,
캐릭터 초상화 영역 (64x64),
텍스트 영역, 다음 버튼,
두루마리/양피지 스타일,
투명 배경, 320x128 크기
```

**영문**:
```
2D pixel art style, game dialogue box UI,
speech bubble/frame for NPC dialogue,
character portrait area (64x64),
text area, next button,
scroll/parchment style,
transparent background, 320x128 size
```

---

## 3.3 팝업/알림

### 아이템 획득 팝업

**한글**:
```
2D 픽셀아트 스타일, 아이템 획득 알림 팝업,
중앙에 아이템 아이콘 슬롯,
아이템 이름/등급 표시 영역,
반짝이는 효과, 등급별 테두리 색상,
일반(회색), 희귀(파랑), 전설(보라), 유니크(금)
투명 배경, 160x80 크기
```

**영문**:
```
2D pixel art style, item acquisition notification popup,
item icon slot in center,
item name/grade display area,
sparkling effect, border color by grade,
common(gray), rare(blue), legendary(purple), unique(gold)
transparent background, 160x80 size
```

---

### 데미지 숫자

**한글**:
```
2D 픽셀아트 스타일, 데미지 숫자 폰트,
0-9 숫자, 일반/치명타/회복/독 데미지 색상,
일반: 흰색, 치명타: 노란색/주황색, 회복: 초록색, 독: 보라색,
숫자 외곽선 포함, 팝업 애니메이션용,
투명 배경, 각 숫자 16x24 크기
```

**영문**:
```
2D pixel art style, damage number font,
0-9 numbers, normal/critical/heal/poison damage colors,
normal: white, critical: yellow/orange, heal: green, poison: purple,
number outlines included, for popup animation,
transparent background, each number 16x24 size
```

---

# 4. 아이템/장비 아이콘

## 4.1 소비 아이템

### 포션류

**한글**:
```
2D 픽셀아트 스타일, 게임 포션 아이콘 세트,
1. 체력 포션 (빨간 액체, 하트 라벨)
2. 마나 포션 (파란 액체, 별 라벨)
3. 해독 포션 (초록 액체, 해골 X표시)
4. 버프 포션 (노란 액체, 화살표 위)
5. 만능 포션 (무지개 액체)
각 유리병 형태, 투명 배경, 24x24 크기
```

**영문**:
```
2D pixel art style, game potion icon set,
1. Health potion (red liquid, heart label)
2. Mana potion (blue liquid, star label)
3. Antidote potion (green liquid, skull X mark)
4. Buff potion (yellow liquid, arrow up)
5. Universal potion (rainbow liquid)
each glass bottle form, transparent background, 24x24 size
```

---

### 음식/재료

**한글**:
```
2D 픽셀아트 스타일, 게임 음식/재료 아이콘 세트,
1. 빵 (체력 소량 회복)
2. 고기 (체력 중량 회복)
3. 마법 과일 (마나 회복)
4. 슬라임 젤리 (제작 재료)
5. 뼈 조각 (제작 재료)
6. 원소 정수 (제작 재료)
각 투명 배경, 24x24 크기
```

**영문**:
```
2D pixel art style, game food/material icon set,
1. Bread (small health restore)
2. Meat (medium health restore)
3. Magic fruit (mana restore)
4. Slime jelly (crafting material)
5. Bone fragment (crafting material)
6. Elemental essence (crafting material)
each transparent background, 24x24 size
```

---

## 4.2 장비 아이콘

### 무기 - 지팡이류

**한글**:
```
2D 픽셀아트 스타일, 마법사 지팡이 아이콘 세트,
1. 견습생 지팡이 (나무, 작은 보석)
2. 마나 지팡이 (은색, 파란 보석)
3. 화염 지팡이 (붉은 나무, 불꽃 보석)
4. 얼음 지팡이 (얼어붙은, 얼음 결정)
5. 암흑 지팡이 (검은색, 해골 장식)
6. 대마법사 지팡이 (금색, 거대 보석)
각 32x32 크기, 투명 배경
```

**영문**:
```
2D pixel art style, wizard staff icon set,
1. Apprentice staff (wood, small gem)
2. Mana staff (silver, blue gem)
3. Fire staff (red wood, flame gem)
4. Ice staff (frozen, ice crystal)
5. Dark staff (black, skull ornament)
6. Archmage staff (gold, large gem)
each 32x32 size, transparent background
```

---

### 방어구 - 로브류

**한글**:
```
2D 픽셀아트 스타일, 마법사 로브 아이콘 세트,
1. 천 로브 (회색, 기본)
2. 견습생 로브 (파란색, 기본 문양)
3. 화염술사 로브 (붉은색, 화염 문양)
4. 빙결술사 로브 (하늘색, 얼음 문양)
5. 암흑술사 로브 (검은색, 보라 문양)
6. 대마법사 로브 (보라색, 금 문양)
각 32x32 크기, 투명 배경
```

**영문**:
```
2D pixel art style, wizard robe icon set,
1. Cloth robe (gray, basic)
2. Apprentice robe (blue, basic pattern)
3. Pyromancer robe (red, flame pattern)
4. Cryomancer robe (sky blue, ice pattern)
5. Dark sorcerer robe (black, purple pattern)
6. Archmage robe (purple, gold pattern)
each 32x32 size, transparent background
```

---

### 악세서리

**한글**:
```
2D 픽셀아트 스타일, 마법사 악세서리 아이콘 세트,
1. 마나 반지 (은색, 파란 보석)
2. 생명력 목걸이 (붉은 보석 펜던트)
3. 원소 귀걸이 (속성별 색상)
4. 마법 부적 (보라색 발광)
5. 마법사 모자 (뾰족한 모자)
6. 마법 망토 (나부끼는 형태)
각 24x24 크기, 투명 배경
```

**영문**:
```
2D pixel art style, wizard accessory icon set,
1. Mana ring (silver, blue gem)
2. Life necklace (red gem pendant)
3. Elemental earring (color by element)
4. Magic amulet (purple glow)
5. Wizard hat (pointed hat)
6. Magic cloak (flowing form)
each 24x24 size, transparent background
```

---

## 4.3 특수 아이템

### 스킬 스크롤

**한글**:
```
2D 픽셀아트 스타일, 스킬 스크롤 아이콘 세트,
두루마리 형태, 속성별 색상과 문양,
1. 화염 스크롤 (붉은색, 불꽃 문양)
2. 얼음 스크롤 (파란색, 눈꽃 문양)
3. 번개 스크롤 (노란색, 번개 문양)
4. 암흑 스크롤 (보라색, 해골 문양)
5. 희귀 스크롤 (금테, 반짝임)
각 24x24 크기, 투명 배경
```

**영문**:
```
2D pixel art style, skill scroll icon set,
scroll form, color and pattern by element,
1. Fire scroll (red, flame pattern)
2. Ice scroll (blue, snowflake pattern)
3. Lightning scroll (yellow, lightning pattern)
4. Dark scroll (purple, skull pattern)
5. Rare scroll (gold trim, sparkling)
each 24x24 size, transparent background
```

---

### 열쇠/던전 아이템

**한글**:
```
2D 픽셀아트 스타일, 던전 아이템 아이콘 세트,
1. 일반 열쇠 (철제)
2. 보스 열쇠 (금색, 해골)
3. 비밀 열쇠 (보라색 발광)
4. 지도 조각
5. 보물 상자 (닫힘/열림)
6. 포탈 스톤 (순간이동)
각 24x24 크기, 투명 배경
```

**영문**:
```
2D pixel art style, dungeon item icon set,
1. Normal key (iron)
2. Boss key (gold, skull)
3. Secret key (purple glow)
4. Map piece
5. Treasure chest (closed/open)
6. Portal stone (teleport)
each 24x24 size, transparent background
```

---

# 5. 추가 NPC

## 5.1 마을 NPC

### 마법 학자 (Skill Master)

**한글**:
```
2D 픽셀아트 스타일, 다크 판타지 마법 학자 NPC, SD 비율(2등신),
늙은 마법사, 긴 흰 수염, 둥근 안경,
보라색 대마법사 로브, 별 무늬 모자,
두꺼운 마법 서적 들고 있음, 주변에 떠다니는 책들,
현명하고 친근한 표정,
정면 뷰, T포즈, 클린 라인아트, 투명 배경, 64x64 해상도
```

**영문**:
```
2D pixel art style, dark fantasy magic scholar NPC, SD proportion (2-head tall),
old wizard, long white beard, round glasses,
purple archmage robe, star pattern hat,
holding thick magic tome, floating books around,
wise and friendly expression,
front view, T-pose, clean lineart, transparent background, 64x64 resolution
```

---

### 모험가 길드장

**한글**:
```
2D 픽셀아트 스타일, 다크 판타지 길드장 NPC, SD 비율(2등신),
강인한 여성 전사, 짧은 붉은 머리, 눈에 흉터,
가죽 갑옷과 모피 어깨보호대, 허리에 큰 도끼,
팔짱 낀 위엄 있는 자세, 신뢰감 주는 표정,
정면 뷰, T포즈, 클린 라인아트, 투명 배경, 64x64 해상도
```

**영문**:
```
2D pixel art style, dark fantasy guild master NPC, SD proportion (2-head tall),
strong female warrior, short red hair, scar on eye,
leather armor and fur shoulder guards, large axe on waist,
arms crossed dignified pose, trustworthy expression,
front view, T-pose, clean lineart, transparent background, 64x64 resolution
```

---

### 정보상 (Information Broker)

**한글**:
```
2D 픽셀아트 스타일, 다크 판타지 정보상 NPC, SD 비율(2등신),
수상한 분위기, 후드로 얼굴 가림, 노란 눈만 보임,
낡은 검은 망토, 허리에 비밀 주머니들,
손에 비밀 문서/쪽지, 그림자 속에 숨은 느낌,
정면 뷰, 클린 라인아트, 투명 배경, 64x64 해상도
```

**영문**:
```
2D pixel art style, dark fantasy information broker NPC, SD proportion (2-head tall),
suspicious atmosphere, face hidden by hood, only yellow eyes visible,
old black cloak, secret pouches on belt,
holding secret documents/notes, hiding in shadows feel,
front view, clean lineart, transparent background, 64x64 resolution
```

---

# 6. 오브젝트/소품

## 6.1 상호작용 오브젝트

### 상자류

**한글**:
```
2D 픽셀아트 스타일, 게임 상자 오브젝트 세트,
1. 나무 상자 (일반, 닫힘/열림)
2. 철제 상자 (희귀, 자물쇠)
3. 황금 상자 (전설, 빛나는 효과)
4. 저주받은 상자 (보라 오라, 해골 장식)
5. 미믹 상자 (몬스터, 이빨)
각 닫힘/열림/파괴 상태, 32x32 크기, 투명 배경
```

**영문**:
```
2D pixel art style, game chest object set,
1. Wooden chest (normal, closed/open)
2. Iron chest (rare, locked)
3. Golden chest (legendary, glowing effect)
4. Cursed chest (purple aura, skull decoration)
5. Mimic chest (monster, teeth)
each closed/open/destroyed state, 32x32 size, transparent background
```

---

### 문/포탈

**한글**:
```
2D 픽셀아트 스타일, 게임 문/포탈 오브젝트 세트,
1. 나무 문 (일반, 닫힘/열림)
2. 철문 (잠김/열림)
3. 보스 문 (거대, 해골 장식, 열쇠 필요)
4. 마법 포탈 (파란 소용돌이, 애니메이션)
5. 탈출 포탈 (녹색, 클리어 시)
각 64x96 크기, 투명 배경
```

**영문**:
```
2D pixel art style, game door/portal object set,
1. Wooden door (normal, closed/open)
2. Iron door (locked/open)
3. Boss door (large, skull decoration, key required)
4. Magic portal (blue vortex, animated)
5. Escape portal (green, on clear)
each 64x96 size, transparent background
```

---

### 함정류

**한글**:
```
2D 픽셀아트 스타일, 게임 함정 오브젝트 세트,
1. 바닥 가시 (숨김/발동)
2. 화살 발사기 (벽, 발사 애니메이션)
3. 회전 톱날 (이동 패턴)
4. 독 분사기 (녹색 가스)
5. 낙석 (위에서 떨어짐)
각 32x32 크기, 투명 배경, 애니메이션 프레임 포함
```

**영문**:
```
2D pixel art style, game trap object set,
1. Floor spikes (hidden/triggered)
2. Arrow shooter (wall, firing animation)
3. Rotating sawblade (movement pattern)
4. Poison sprayer (green gas)
5. Falling rocks (dropping from above)
each 32x32 size, transparent background, animation frames included
```

---

## 6.2 장식 오브젝트

### 늪지대 장식

**한글**:
```
2D 픽셀아트 스타일, 늪지대 장식 오브젝트 세트,
1. 독버섯 (다양한 크기, 발광)
2. 썩은 나무 그루터기
3. 해골/뼈 더미
4. 독 웅덩이 (부글거림)
5. 거미줄
6. 늪지 풀/갈대
7. 반딧불이 (녹색 발광)
다양한 크기, 투명 배경
```

**영문**:
```
2D pixel art style, swamp decoration object set,
1. Poison mushrooms (various sizes, glowing)
2. Rotting tree stump
3. Skull/bone pile
4. Toxic puddle (bubbling)
5. Spider web
6. Swamp grass/reeds
7. Fireflies (green glow)
various sizes, transparent background
```

---

### 성채 장식

**한글**:
```
2D 픽셀아트 스타일, 성채 장식 오브젝트 세트,
1. 깨진 갑옷/투구
2. 녹슨 무기 (검, 창, 방패)
3. 촛대/횃불 (불 애니메이션)
4. 찢어진 깃발/태피스트리
5. 깨진 스테인드글라스
6. 거미줄
7. 사슬/족쇄
8. 해골/뼈
다양한 크기, 투명 배경
```

**영문**:
```
2D pixel art style, citadel decoration object set,
1. Broken armor/helmet
2. Rusty weapons (sword, spear, shield)
3. Candelabra/torch (fire animation)
4. Torn flags/tapestry
5. Broken stained glass
6. Spider web
7. Chains/shackles
8. Skull/bones
various sizes, transparent background
```

---

# 7. 추가 이펙트

## 7.1 환경 이펙트

### 날씨/분위기

**한글**:
```
2D 픽셀아트 스타일, 환경 이펙트 세트,
1. 비 (내리는 빗줄기)
2. 안개 (투명하게 흐르는)
3. 독 안개 (녹색, 위험 표시)
4. 먼지 입자 (떠다니는)
5. 마나 입자 (파란/보라 빛나는)
6. 번개 (화면 플래시)
7. 불씨/재 (떨어지는)
각 루프 애니메이션, 투명 배경
```

**영문**:
```
2D pixel art style, environmental effect set,
1. Rain (falling raindrops)
2. Fog (flowing transparent)
3. Poison mist (green, danger indicator)
4. Dust particles (floating)
5. Mana particles (blue/purple glowing)
6. Lightning (screen flash)
7. Embers/ash (falling)
each loop animation, transparent background
```

---

## 7.2 상태 이펙트

### 버프/디버프

**한글**:
```
2D 픽셀아트 스타일, 상태 이펙트 세트,
1. 화상 (캐릭터 주변 불꽃)
2. 빙결 (얼음 덮인 효과)
3. 독 (보라색 기포)
4. 슬로우 (파란 사슬)
5. 스턴 (머리 위 별)
6. 무적 (황금 오라)
7. 공격력 증가 (붉은 오라)
8. 방어력 증가 (파란 방패)
캐릭터에 씌워지는 형태, 투명 배경
```

**영문**:
```
2D pixel art style, status effect set,
1. Burn (flames around character)
2. Freeze (ice covered effect)
3. Poison (purple bubbles)
4. Slow (blue chains)
5. Stun (stars above head)
6. Invincible (golden aura)
7. Attack boost (red aura)
8. Defense boost (blue shield)
overlay on character form, transparent background
```

---

## 7.3 히트/사망 이펙트

### 타격 이펙트

**한글**:
```
2D 픽셀아트 스타일, 타격 이펙트 세트,
1. 물리 타격 (흰색 별/충격)
2. 화염 타격 (불꽃 튀김)
3. 얼음 타격 (얼음 조각)
4. 번개 타격 (전기 스파크)
5. 암흑 타격 (보라색 파열)
6. 치명타 (큰 노란 별)
각 3-4프레임 애니메이션, 32x32 크기, 투명 배경
```

**영문**:
```
2D pixel art style, hit effect set,
1. Physical hit (white star/impact)
2. Fire hit (flame sparks)
3. Ice hit (ice shards)
4. Lightning hit (electric sparks)
5. Dark hit (purple burst)
6. Critical hit (large yellow star)
each 3-4 frame animation, 32x32 size, transparent background
```

---

### 사망 이펙트

**한글**:
```
2D 픽셀아트 스타일, 몬스터 사망 이펙트 세트,
1. 일반 사망 (연기/먼지)
2. 슬라임 사망 (점액 튀김)
3. 해골 사망 (뼈 흩어짐)
4. 원소 사망 (원소 폭발)
5. 보스 사망 (대폭발, 빛)
각 6-8프레임 애니메이션, 투명 배경
```

**영문**:
```
2D pixel art style, monster death effect set,
1. Normal death (smoke/dust)
2. Slime death (slime splatter)
3. Skeleton death (bones scatter)
4. Elemental death (element explosion)
5. Boss death (big explosion, light)
each 6-8 frame animation, transparent background
```

---

# 요약: 다음 작업 우선순위

## 높음 (필수)

| 카테고리 | 항목 | 개수 |
|----------|------|------|
| 배경 | 1스테이지 늪지대 (4레이어) | 4 |
| 배경 | 2스테이지 성채 (4레이어) | 4 |
| 타일셋 | 늪지대 플랫폼 | 1세트 |
| 타일셋 | 성채 플랫폼 | 1세트 |
| UI | 체력바/마나바/스킬슬롯 | 3 |
| UI | 보스 체력바 | 1 |

## 보통 (권장)

| 카테고리 | 항목 | 개수 |
|----------|------|------|
| 배경 | 시작 마을 | 3레이어 |
| UI | 메뉴 버튼/인벤토리 | 2 |
| 아이템 | 포션/음식 아이콘 | 10+ |
| 아이템 | 장비 아이콘 | 15+ |
| 오브젝트 | 상자/문/포탈 | 10+ |

## 낮음 (추후)

| 카테고리 | 항목 | 개수 |
|----------|------|------|
| NPC | 추가 마을 NPC | 3 |
| 장식 | 환경 장식 오브젝트 | 20+ |
| 이펙트 | 환경/상태 이펙트 | 15+ |

---

**문서 버전**: v1.0
**최종 업데이트**: 2025-11-30
**작성자**: Claude Code Assistant

---

*이 문서는 GASPT 프로젝트의 다음 단계 아트 에셋 기획입니다.*
