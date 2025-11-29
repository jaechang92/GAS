# NPC 캐릭터 디자인 프롬프트 가이드

> **프로젝트**: GASPT (Skul: The Hero Slayer 스타일 2D 플랫포머 로그라이크)
> **작성일**: 2025-11-27
> **목적**: namobanana AI를 활용한 NPC 캐릭터 2D 아트 생성용 프롬프트

---

## 목차

1. [프로젝트 아트 스타일 개요](#1-프로젝트-아트-스타일-개요)
2. [NPC 캐릭터 프롬프트](#2-npc-캐릭터-프롬프트)
   - [상점 NPC (Merchant)](#21-상점-npc-merchant)
   - [대장장이 NPC (Blacksmith)](#22-대장장이-npc-blacksmith)
   - [마녀/연금술사 NPC (Alchemist)](#23-마녀연금술사-npc-alchemist)
   - [가이드 NPC (Guide)](#24-가이드-npc-guide)
   - [미스터리 NPC (Mystery)](#25-미스터리-npc-mystery)
3. [공통 스타일 가이드라인](#3-공통-스타일-가이드라인)
4. [Unity 스프라이트 설정](#4-unity-스프라이트-설정)
5. [에셋 폴더 구조](#5-에셋-폴더-구조)
6. [애니메이션 가이드](#6-애니메이션-가이드)
7. [컬러 팔레트 레퍼런스](#7-컬러-팔레트-레퍼런스)

---

## 1. 프로젝트 아트 스타일 개요

### 게임 컨셉
- **장르**: Skul: The Hero Slayer 스타일 2D 플랫포머 액션 로그라이크
- **세계관**: 다크 판타지 (마법사, 전사, 암살자 등 다양한 클래스 존재)
- **주인공**: 스컬(해골) 캐릭터 - 100개 이상의 다양한 스컬 타입으로 전환 가능
- **핵심 요소**: 랜덤 생성 맵, 아이템 시너지, 진행도 기반 해금

### Skul 스타일 아트 특징
| 요소 | 적용 방법 |
|------|----------|
| **SD 비율** | 2등신 (머리:몸 = 1:1) |
| **외곽선** | 진한 검정(#000000) 2px 두께 |
| **음영** | 2단계 단순 음영 (밝음/어두움) |
| **컬러** | 채도 높은 원색 + 다크한 배경톤 |
| **표정** | 과장된 눈 표현, 단순한 입 |
| **실루엣** | 역할이 명확히 드러나는 특징적 형태 |

### 권장 해상도
- **기본**: 64x64 픽셀
- **고해상도**: 128x128 픽셀 (Unity에서 다운스케일)
- **스프라이트 시트**: 256x256 또는 512x512

---

## 2. NPC 캐릭터 프롬프트

### 2.1 상점 NPC (Merchant)

#### 캐릭터 컨셉
- **역할**: 던전 내 아이템/장비 판매, 플레이어와의 거래 상호작용
- **시각적 특징**: 상인임을 알리는 가방/보따리, 금화, 저울 등의 소품
- **성격 표현**: 약간 교활하지만 친근한 느낌의 다크 판타지 상인

#### 프롬프트 (한글)
```
2D 픽셀아트 스타일, 다크 판타지 상인 캐릭터, 작고 귀여운 SD 비율(2등신),
후드 달린 낡은 망토 착용, 등에 큰 보따리 짊어짐, 허리에 금화 주머니,
한 손에 랜턴 들고 있음, 노란 눈빛이 후드 안에서 빛남, 정면 뷰, T포즈,
클린 라인아트, 단순한 음영, 투명 배경, 게임 스프라이트용, 64x64 해상도에 적합한 디테일
```

#### 프롬프트 (영문)
```
2D pixel art style, dark fantasy merchant character, cute chibi SD proportion (2-head tall),
wearing worn hooded cloak, large sack bundle on back, gold pouch on belt,
holding lantern in one hand, glowing yellow eyes visible under hood, front view, T-pose,
clean lineart, simple shading, transparent background, game sprite asset,
detail suitable for 64x64 resolution
```

#### 컬러 팔레트
| 부위 | 색상 코드 | 설명 |
|------|----------|------|
| 망토 (어두운) | #3D2B1F | 갈색 |
| 망토 (밝은) | #5C4033 | 밝은 갈색 |
| 눈 발광 | #FFD700 | 금색 |
| 금화 (밝은) | #DAA520 | 골든로드 |
| 금화 (어두운) | #B8860B | 다크 골든로드 |
| 랜턴 빛 | #FFA500 | 주황색 |

#### 애니메이션 스펙
- **분리 레이어**: 머리(후드+눈), 몸통(망토), 왼팔(랜턴), 오른팔, 보따리, 다리
- **Idle**: 4프레임 (호흡+랜턴 흔들림)
- **Talk**: 6프레임 (고개 끄덕임)
- **Sell**: 4프레임 (아이템 건네는 동작)
- **키 포즈**: 기본 서기, 인사, 아이템 제시, 기뻐하기(판매 성공)

---

### 2.2 대장장이 NPC (Blacksmith)

#### 캐릭터 컨셉
- **역할**: 스컬 강화, 장비 수리, 업그레이드 서비스
- **시각적 특징**: 근육질 체형, 대장장이 앞치마, 망치, 모루 근처 배치
- **성격 표현**: 묵직하고 신뢰감 있는 장인의 느낌

#### 프롬프트 (한글)
```
2D 픽셀아트 스타일, 다크 판타지 대장장이 캐릭터, 작고 귀여운 SD 비율(2등신),
근육질 실루엣, 가죽 앞치마 착용, 한 손에 커다란 망치 들고 있음,
얼굴에 그을음 자국, 붉은 눈 또는 불꽃 반사되는 눈,
짧은 뿔 또는 뾰족한 귀(악마/오크 혼혈 느낌), 정면 뷰, T포즈,
클린 라인아트, 단순한 음영, 투명 배경, 게임 스프라이트용
```

#### 프롬프트 (영문)
```
2D pixel art style, dark fantasy blacksmith character, cute chibi SD proportion (2-head tall),
muscular silhouette, wearing leather apron, holding large hammer in one hand,
soot marks on face, red eyes or eyes reflecting fire glow,
short horns or pointed ears (demon/orc hybrid look), front view, T-pose,
clean lineart, simple shading, transparent background, game sprite asset
```

#### 컬러 팔레트
| 부위 | 색상 코드 | 설명 |
|------|----------|------|
| 피부 | #8B4513 | 짙은 갈색/그을린 톤 |
| 앞치마 (어두운) | #3C280D | 다크 브라운 |
| 앞치마 (밝은) | #654321 | 브라운 |
| 눈/불꽃 (밝은) | #FF6347 | 토마토 레드 |
| 눈/불꽃 (어두운) | #FF4500 | 오렌지 레드 |
| 망치 금속 | #708090 | 슬레이트 그레이 |
| 망치 손잡이 | #4A4A4A | 다크 그레이 |

#### 애니메이션 스펙
- **분리 레이어**: 머리(뿔 포함), 몸통(앞치마), 왼팔, 오른팔(망치), 다리
- **Idle**: 4프레임 (호흡+망치 살짝 흔들림)
- **Forge**: 8프레임 (망치질 애니메이션)
- **Nod**: 4프레임 (수락 고개 끄덕임)
- **키 포즈**: 기본 서기, 망치 들어올림, 내려치기, 작업 완료 포즈

---

### 2.3 마녀/연금술사 NPC (Alchemist)

#### 캐릭터 컨셉
- **역할**: 포션 제작 및 판매, 일시적 버프 제공, 저주 해제
- **시각적 특징**: 마녀 모자, 물약병, 가마솥, 신비로운 분위기
- **성격 표현**: 괴짜스럽고 신비로운, 약간 미쳐보이는 과학자 느낌

#### 프롬프트 (한글)
```
2D 픽셀아트 스타일, 다크 판타지 연금술사 마녀 캐릭터, 작고 귀여운 SD 비율(2등신),
뾰족한 마녀 모자(찢어지고 낡은), 헝클어진 머리카락, 보라색 또는 초록색 로브,
허리에 여러 물약병 매달림, 한 손에 부글거리는 물약 들고 있음,
큰 눈에 소용돌이 무늬(광기 표현), 정면 뷰, T포즈,
클린 라인아트, 투명 배경, 게임 스프라이트용
```

#### 프롬프트 (영문)
```
2D pixel art style, dark fantasy alchemist witch character, cute chibi SD proportion (2-head tall),
pointy witch hat (torn and worn), messy wild hair, purple or green robe,
multiple potion bottles hanging on belt, holding bubbling potion in one hand,
large eyes with spiral pattern (expressing madness), front view, T-pose,
clean lineart, transparent background, game sprite asset
```

#### 컬러 팔레트
| 부위 | 색상 코드 | 설명 |
|------|----------|------|
| 로브 (메인) | #4B0082 | 인디고 |
| 로브 (하이라이트) | #8B008B | 다크 마젠타 |
| 물약 (체력) | #FF0000, #DC143C | 빨강 |
| 물약 (마나) | #00BFFF, #1E90FF | 파랑 |
| 물약 (독) | #32CD32, #00FF00 | 초록 |
| 눈 발광 | #FFFF00 | 노란색 |

#### 애니메이션 스펙
- **분리 레이어**: 모자, 머리카락, 얼굴, 몸통(로브), 양팔, 물약(발광 효과 별도)
- **Idle**: 6프레임 (물약 흔들기+눈 깜빡임)
- **Brew**: 8프레임 (물약 섞기 동작)
- **Cackle**: 6프레임 (웃음 애니메이션)
- **키 포즈**: 기본 서기, 물약 흔들기, 물약 건네기, 가마솥 젓기, 깜짝 놀람

---

### 2.4 가이드 NPC (Guide)

#### 캐릭터 컨셉
- **역할**: 튜토리얼 진행, 게임 힌트 제공, 스토리 전달자
- **시각적 특징**: 친근하고 신뢰감 있는 외형, 책/두루마리 소지
- **성격 표현**: 현명하고 온화한 조력자, 유령/정령 느낌도 가능

#### 프롬프트 (한글)
```
2D 픽셀아트 스타일, 다크 판타지 가이드 정령 캐릭터, 작고 귀여운 SD 비율(2등신),
반투명한 유령 같은 몸체, 두건 또는 작은 후드 착용, 부드러운 푸른 발광 효과,
손에 오래된 두루마리 또는 작은 책 들고 있음,
온화한 표정의 빈 눈구멍(해골 가이드) 또는 부드러운 눈,
하체가 연기처럼 흐릿하게 사라짐, 정면 뷰,
클린 라인아트, 투명 배경, 게임 스프라이트용
```

#### 프롬프트 (영문)
```
2D pixel art style, dark fantasy guide spirit character, cute chibi SD proportion (2-head tall),
semi-transparent ghostly body, wearing small hood or cowl, soft blue glow effect,
holding ancient scroll or small book,
gentle expression with empty eye sockets (skeleton guide) or soft kind eyes,
lower body fading like smoke, front view,
clean lineart, transparent background, game sprite asset
```

#### 컬러 팔레트
| 부위 | 색상 코드 | 설명 |
|------|----------|------|
| 몸체 (메인) | #87CEEB | 하늘색 (반투명) |
| 몸체 (하이라이트) | #B0E0E6 | 연한 파랑 |
| 발광 (메인) | #00CED1 | 다크 터콰이즈 |
| 발광 (밝은) | #40E0D0 | 터콰이즈 |
| 책/두루마리 | #DEB887, #D2B48C | 베이지/탄 |
| 눈구멍 발광 | #FFFFFF, #E0FFFF | 흰색/연한 시안 |

#### 애니메이션 스펙
- **분리 레이어**: 머리(후드), 상체, 팔(책/두루마리), 하체(연기 효과 별도)
- **Idle**: 6프레임 (둥실둥실 떠다니는 효과)
- **Talk**: 8프레임 (책 펼치며 설명)
- **Point**: 4프레임 (방향 가리키기)
- **키 포즈**: 떠있는 기본 자세, 책 펼침, 손가락으로 가리킴, 고개 끄덕임

---

### 2.5 미스터리 NPC (Mystery)

#### 캐릭터 컨셉
- **역할**: 랜덤 이벤트 제공, 축복/저주 선택지, 도박 요소
- **시각적 특징**: 정체불명, 수상하고 불길한 분위기, 가면 또는 얼굴 가림
- **성격 표현**: 트릭스터, 알 수 없는 의도, 위험하지만 매력적인

#### 프롬프트 (한글)
```
2D 픽셀아트 스타일, 다크 판타지 미스터리 캐릭터, 작고 귀여운 SD 비율(2등신),
전신을 덮는 검은 로브와 후드, 하얀 가면(웃는 표정 또는 무표정) 착용,
손에 타로 카드 또는 주사위 들고 있음, 로브 아래로 보라색 또는 붉은 빛 새어나옴,
불길하지만 귀여운 분위기, 정면 뷰, T포즈,
클린 라인아트, 투명 배경, 게임 스프라이트용
```

#### 프롬프트 (영문)
```
2D pixel art style, dark fantasy mystery character, cute chibi SD proportion (2-head tall),
full body covered in black robe and hood, wearing white mask (smiling or expressionless),
holding tarot cards or dice in hand, purple or red light glowing from under robe,
ominous yet cute atmosphere, front view, T-pose,
clean lineart, transparent background, game sprite asset
```

#### 컬러 팔레트
| 부위 | 색상 코드 | 설명 |
|------|----------|------|
| 로브 (메인) | #1C1C1C | 거의 검정 |
| 로브 (하이라이트) | #2F2F2F | 다크 그레이 |
| 가면 (메인) | #FFFAFA | 스노우 화이트 |
| 가면 (음영) | #F5F5F5 | 화이트 스모크 |
| 발광 (축복) | #9400D3, #8A2BE2 | 보라색 계열 |
| 발광 (저주) | #8B0000, #DC143C | 붉은색 계열 |
| 카드 테두리 | #FFD700 | 금색 |
| 카드 뒷면 | #4B0082 | 인디고 |

#### 애니메이션 스펙
- **분리 레이어**: 가면, 후드, 로브 상체, 양팔(카드/주사위), 로브 하체, 발광 효과
- **Idle**: 6프레임 (로브 나부낌+불길한 발광 깜빡임)
- **Offer**: 6프레임 (카드 내밀기)
- **Laugh**: 8프레임 (어깨 들썩이는 웃음)
- **Vanish**: 8프레임 (사라지는 효과)
- **키 포즈**: 기본 서기, 카드 섞기, 카드 제시, 손 벌리기(선택 요구), 기뻐하기/실망하기

---

## 3. 공통 스타일 가이드라인

### 필수 키워드 (모든 프롬프트에 포함)
```
2D pixel art style, dark fantasy, cute chibi SD proportion (2-head tall),
front view, T-pose, clean lineart, simple shading, transparent background, game sprite asset
```

### 선택적 키워드
| 용도 | 키워드 |
|------|--------|
| 해상도 지정 | `detail suitable for 64x64 resolution` |
| 애니메이션용 | `animation ready, separated limbs` |
| 스프라이트 시트 | `sprite sheet, multiple poses` |
| 방향 변형 | `side view`, `back view`, `3/4 view` |

### 피해야 할 키워드
- `realistic` - 픽셀아트와 충돌
- `3D` - 2D 스타일 유지 필요
- `complex shading` - 단순 음영 유지
- `photorealistic` - 스타일과 불일치

---

## 4. Unity 스프라이트 설정

### Import Settings
```
Texture Type: Sprite (2D and UI)
Sprite Mode: Single (기본) / Multiple (스프라이트 시트)
Pixels Per Unit: 64
Filter Mode: Point (no filter)
Compression: None
Max Size: 256 (64x64 기준 4x4 시트용)
```

### 스프라이트 설정 코드 예시
```csharp
// TextureImporter 설정 (에디터 스크립트용)
TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
importer.textureType = TextureImporterType.Sprite;
importer.spritePixelsPerUnit = 64;
importer.filterMode = FilterMode.Point;
importer.textureCompression = TextureImporterCompression.Uncompressed;
```

### 발광 효과 처리
- 물약, 눈 발광 등은 **Additive 블렌딩** 별도 레이어로 처리
- Unity Sprite Renderer의 Material을 `Sprites-Default`에서 `Sprites-Additive`로 변경
- 또는 별도의 발광 스프라이트를 오버레이

---

## 5. 에셋 폴더 구조

### 권장 폴더 구조
```
Assets/_Project/Art/Characters/NPCs/
├── Merchant/
│   ├── Merchant_Idle.png
│   ├── Merchant_Talk.png
│   ├── Merchant_Sell.png
│   ├── Merchant_SpriteSheet.png
│   └── Merchant_Animations/
│       ├── Merchant_Idle.anim
│       ├── Merchant_Talk.anim
│       └── Merchant_Sell.anim
├── Blacksmith/
│   ├── Blacksmith_Idle.png
│   ├── Blacksmith_Forge.png
│   ├── Blacksmith_Nod.png
│   ├── Blacksmith_SpriteSheet.png
│   └── Blacksmith_Animations/
├── Alchemist/
│   ├── Alchemist_Idle.png
│   ├── Alchemist_Brew.png
│   ├── Alchemist_Cackle.png
│   ├── Alchemist_SpriteSheet.png
│   ├── Alchemist_Glow.png (발광 효과 별도)
│   └── Alchemist_Animations/
├── Guide/
│   ├── Guide_Idle.png
│   ├── Guide_Talk.png
│   ├── Guide_Point.png
│   ├── Guide_SpriteSheet.png
│   ├── Guide_SmokeEffect.png (연기 효과 별도)
│   └── Guide_Animations/
└── Mystery/
    ├── Mystery_Idle.png
    ├── Mystery_Offer.png
    ├── Mystery_Laugh.png
    ├── Mystery_Vanish.png
    ├── Mystery_SpriteSheet.png
    ├── Mystery_Glow_Blessing.png (축복 발광)
    ├── Mystery_Glow_Curse.png (저주 발광)
    └── Mystery_Animations/
```

### 네이밍 컨벤션
```
{캐릭터명}_{상태/애니메이션}.png
{캐릭터명}_SpriteSheet.png
{캐릭터명}_{특수효과}.png
```

---

## 6. 애니메이션 가이드

### 스프라이트 시트 구성 권장
```
각 NPC당 권장 시트 구성 (4행 x N열):
- Row 1: Idle (4-6 프레임)
- Row 2: Talk/Interact (6-8 프레임)
- Row 3: Special Action (6-8 프레임)
- Row 4: Reaction (기쁨/슬픔/놀람 각 2프레임)
```

### 프레임 레이트 권장
| 애니메이션 타입 | FPS | 설명 |
|---------------|-----|------|
| Idle | 6-8 | 느긋한 호흡 표현 |
| Talk | 10-12 | 자연스러운 대화 |
| Action | 12-15 | 동적인 동작 |
| Special Effect | 15-24 | 빠른 효과 |

### Unity Animator 설정
```csharp
// AnimatorController 생성 시 권장 파라미터
// Bool: isTalking, isWorking
// Trigger: onInteract, onSell, onComplete
// Int: reactionType (0: neutral, 1: happy, 2: sad, 3: surprised)
```

---

## 7. 컬러 팔레트 레퍼런스

### 전체 NPC 공통 팔레트
```
// 다크 판타지 기본 톤
Background Dark:   #1A1A2E
Background Mid:    #16213E
Background Light:  #0F3460

// 강조색
Accent Gold:       #FFD700
Accent Purple:     #9400D3
Accent Red:        #DC143C
Accent Blue:       #00BFFF
Accent Green:      #32CD32

// 발광 효과
Glow White:        #FFFFFF
Glow Yellow:       #FFFF00
Glow Cyan:         #00FFFF
Glow Magenta:      #FF00FF
```

### 팔레트 적용 규칙
1. **주 색상**: 캐릭터당 2-3가지 주요 색상 사용
2. **음영**: 주 색상의 명도만 조절 (채도 유지)
3. **하이라이트**: 흰색 또는 연한 색상으로 포인트
4. **외곽선**: 항상 #000000 (순수 검정)

---

## 부록: 프롬프트 작성 팁

### namobanana AI 사용 시 주의사항
1. **일관성 유지**: 같은 프로젝트의 NPC들은 동일한 스타일 키워드 사용
2. **시드 활용**: 마음에 드는 결과물의 시드 저장하여 변형 생성
3. **후보정 필수**: AI 생성 이미지는 Aseprite 등에서 픽셀 정리 필요
4. **레이어 분리**: 생성 후 수동으로 파츠 분리 작업 진행

### 프롬프트 변형 예시
```
// 측면 뷰가 필요한 경우
기존: "front view, T-pose"
변경: "side view, walking pose"

// 4방향 뷰가 필요한 경우
"front view, side view, back view, 3/4 view, character turnaround sheet"

// 표정 변화가 필요한 경우
"expression sheet, neutral, happy, angry, sad, surprised"
```

---

## 참고 자료

- [Skul: The Hero Slayer 공식 아트북](https://store.steampowered.com/app/1147560/Skul_The_Hero_Slayer/)
- [픽셀아트 기초 가이드](https://lospec.com/pixel-art-tutorials)
- [Unity 2D 스프라이트 문서](https://docs.unity3d.com/Manual/Sprites.html)

---

**문서 버전**: v1.0
**최종 업데이트**: 2025-11-27
**작성자**: Claude Code Assistant
