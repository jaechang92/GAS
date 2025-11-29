# 플레이어 마법사 캐릭터 디자인 가이드

> **프로젝트**: GASPT (마법사 테마 2D 플랫포머 액션 로그라이트)
> **작성일**: 2025-11-29
> **목적**: 플레이어 마법사 캐릭터 기획서 및 namobanana AI용 아트 프롬프트

---

## 목차

1. [캐릭터 기획 개요](#1-캐릭터-기획-개요)
2. [기본 스탯 설계](#2-기본-스탯-설계)
3. [스킬 시스템](#3-스킬-시스템)
   - [기본 공격: 마나 볼트](#31-기본-공격-마나-볼트)
   - [스킬1: 플레임 버스트](#32-스킬1-플레임-버스트)
   - [스킬2: 프로스트 노바](#33-스킬2-프로스트-노바)
   - [궁극기: 아케인 스톰](#34-궁극기-아케인-스톰)
4. [FSM 상태 설계](#4-fsm-상태-설계)
5. [캐릭터 아트 프롬프트](#5-캐릭터-아트-프롬프트)
6. [스킬 이펙트 아트 프롬프트](#6-스킬-이펙트-아트-프롬프트)
7. [애니메이션 가이드](#7-애니메이션-가이드)
8. [Unity 구현 가이드](#8-unity-구현-가이드)
9. [에셋 폴더 구조](#9-에셋-폴더-구조)

---

## 1. 캐릭터 기획 개요

### 캐릭터 컨셉

| 항목 | 내용 |
|------|------|
| **이름** | 아르카나 (Arcana) - 또는 플레이어 지정 |
| **클래스** | 원소 마법사 (Elemental Wizard) |
| **테마** | 다중 원소를 다루는 젊은 마법사 |
| **성격** | 호기심 많고 용감한, 성장하는 영웅 |
| **외형 컨셉** | 로브를 입은 SD 스타일 마법사, 마법 지팡이 소지 |

### 디자인 철학

```
1. 명확한 실루엣: 마법사임을 즉시 알 수 있는 외형
2. 애니메이션 친화적: 로브, 망토의 자연스러운 움직임
3. 스킬 연출 공간: 마법 이펙트가 돋보이는 심플한 디자인
4. 성장 표현 가능: 장비/강화에 따른 외형 변화 여지
```

### 세계관 배경

던전의 깊은 곳에서 고대의 마법을 찾아 헤매는 젊은 마법사. 다양한 원소의 힘을 습득하며 던전을 정복해 나간다. 화염, 얼음, 순수 마나 등 여러 속성의 마법을 자유자재로 다루는 것이 목표.

---

## 2. 기본 스탯 설계

### 기본 스탯 테이블

| 스탯 | 기본값 | 최대값 | 설명 |
|------|--------|--------|------|
| **체력 (HP)** | 100 | 500 | 생명력 |
| **마나 (MP)** | 100 | 300 | 스킬 사용 자원 |
| **공격력 (ATK)** | 10 | 100 | 기본 마법 데미지 계수 |
| **마법력 (MAG)** | 15 | 150 | 스킬 데미지 계수 |
| **방어력 (DEF)** | 5 | 50 | 받는 데미지 감소 |
| **이동속도 (SPD)** | 5.0 | 8.0 | 초당 이동 거리 |
| **마나 재생 (MPR)** | 5/s | 20/s | 초당 마나 회복량 |

### 스탯 성장 공식

```csharp
// ScriptableObject: PlayerStatsData
[CreateAssetMenu(fileName = "WizardStats", menuName = "GASPT/Player/WizardStats")]
public class WizardStatsData : ScriptableObject
{
    [Header("기본 스탯")]
    public float baseHealth = 100f;
    public float baseMana = 100f;
    public float baseAttack = 10f;
    public float baseMagic = 15f;
    public float baseDefense = 5f;
    public float baseMoveSpeed = 5f;
    public float baseManaRegen = 5f;

    [Header("레벨당 성장률")]
    public float healthPerLevel = 10f;
    public float manaPerLevel = 8f;
    public float attackPerLevel = 2f;
    public float magicPerLevel = 3f;
}
```

---

## 3. 스킬 시스템

### 스킬 개요

| 슬롯 | 스킬명 | 속성 | 쿨다운 | 마나 소모 |
|------|--------|------|--------|-----------|
| 기본공격 | 마나 볼트 | 순수 마나 | 0.3초 | 0 |
| 스킬1 | 플레임 버스트 | 화염 | 5초 | 25 |
| 스킬2 | 프로스트 노바 | 얼음 | 8초 | 35 |
| 궁극기 | 아케인 스톰 | 아케인 | 30초 | 80 |

---

### 3.1 기본 공격: 마나 볼트

#### 스킬 정보

| 항목 | 내용 |
|------|------|
| **이름** | 마나 볼트 (Mana Bolt) |
| **타입** | 원거리 투사체 |
| **속성** | Pure (순수 마나) |
| **사거리** | 10 유닛 |
| **투사체 속도** | 15 유닛/초 |

#### 데미지 공식

```
기본 데미지 = ATK * 1.0
최종 데미지 = 기본 데미지 * (1 + MAG * 0.01)
```

#### GAS 태그 설정

```yaml
ability_tags:
  - Ability.Attack.Basic
  - Ability.Element.Pure
  - Ability.Type.Projectile

block_tags: []  # 차단 없음

cancel_tags:
  - Ability.Attack.Basic  # 연속 발사 가능
```

#### 시각적 연출

- **투사체**: 파란색/보라색 빛나는 마나 구체
- **발사**: 지팡이 끝에서 마나 집중 → 발사
- **적중**: 작은 마나 폭발 이펙트
- **꼬리 효과**: 투사체 뒤로 마나 입자 흩날림

---

### 3.2 스킬1: 플레임 버스트

#### 스킬 정보

| 항목 | 내용 |
|------|------|
| **이름** | 플레임 버스트 (Flame Burst) |
| **타입** | 전방 범위 공격 |
| **속성** | Fire (화염) |
| **범위** | 전방 부채꼴 60도, 반경 4 유닛 |
| **쿨다운** | 5초 |
| **마나 소모** | 25 |

#### 데미지 공식

```
기본 데미지 = MAG * 2.5
화상 데미지 = MAG * 0.3 (3초간 매초)
최종 데미지 = 기본 데미지 + 화상 데미지 * 3
```

#### 추가 효과

- **화상 (Burn)**: 3초간 지속 데미지
- **방어력 감소**: 10% (3초간)

#### GAS 태그 설정

```yaml
ability_tags:
  - Ability.Attack.Skill
  - Ability.Element.Fire
  - Ability.Type.AoE
  - Ability.Effect.DoT

block_tags:
  - Ability.Attack.Basic  # 스킬 시전 중 기본공격 불가

cancel_tags: []
```

#### 시각적 연출

- **시전**: 지팡이를 전방으로 휘두르며 화염 집중
- **폭발**: 부채꼴 형태의 화염 분출
- **잔여 효과**: 바닥에 잠시 남는 불씨
- **적 피격**: 화염에 휩싸이는 이펙트 + 화상 아이콘

---

### 3.3 스킬2: 프로스트 노바

#### 스킬 정보

| 항목 | 내용 |
|------|------|
| **이름** | 프로스트 노바 (Frost Nova) |
| **타입** | 자신 중심 원형 범위 |
| **속성** | Ice (얼음) |
| **범위** | 반경 3.5 유닛 원형 |
| **쿨다운** | 8초 |
| **마나 소모** | 35 |

#### 데미지 공식

```
기본 데미지 = MAG * 2.0
빙결 보너스 = 빙결 상태 적에게 +50% 추가 데미지
최종 데미지 = 기본 데미지 * (빙결 ? 1.5 : 1.0)
```

#### 추가 효과

- **슬로우**: 이동속도 40% 감소 (3초)
- **빙결 확률**: 20% 확률로 1초간 빙결 (행동 불가)

#### GAS 태그 설정

```yaml
ability_tags:
  - Ability.Attack.Skill
  - Ability.Element.Ice
  - Ability.Type.AoE
  - Ability.Effect.CC

block_tags:
  - Ability.Attack.Basic
  - Ability.Movement.Dash

cancel_tags: []
```

#### 시각적 연출

- **시전**: 캐릭터가 살짝 공중으로 뜨며 마나 집중
- **폭발**: 캐릭터 중심으로 원형 얼음 파동 확산
- **잔여 효과**: 바닥에 얼음 결정 흩어짐
- **적 피격**: 얼음 결정이 적에게 달라붙는 이펙트

---

### 3.4 궁극기: 아케인 스톰

#### 스킬 정보

| 항목 | 내용 |
|------|------|
| **이름** | 아케인 스톰 (Arcane Storm) |
| **타입** | 지정 위치 범위 지속 공격 |
| **속성** | Arcane (아케인) |
| **범위** | 반경 5 유닛 원형 |
| **지속시간** | 4초 |
| **쿨다운** | 30초 |
| **마나 소모** | 80 |

#### 데미지 공식

```
틱당 데미지 = MAG * 1.5
총 틱 수 = 8회 (0.5초마다)
총 데미지 = MAG * 1.5 * 8 = MAG * 12
```

#### 추가 효과

- **마나 흡수**: 틱당 5% 확률로 마나 5 회복
- **관통**: 방어력 30% 무시

#### GAS 태그 설정

```yaml
ability_tags:
  - Ability.Attack.Ultimate
  - Ability.Element.Arcane
  - Ability.Type.AoE
  - Ability.Type.Channeled

block_tags:
  - Ability.Attack.Basic
  - Ability.Attack.Skill
  - Ability.Movement.Dash

cancel_tags:
  - State.Hit  # 피격 시 취소
```

#### 시각적 연출

- **시전**: 지팡이를 하늘로 치켜들며 마법진 생성
- **유지**: 지정 위치에 보라색 에너지 소용돌이
- **틱 데미지**: 번개/마나 폭발 이펙트
- **종료**: 마지막 큰 폭발 후 잔상 사라짐

---

## 4. FSM 상태 설계

### 상태 다이어그램

```
                    ┌─────────┐
                    │  Idle   │◄──────────────────┐
                    └────┬────┘                   │
                         │                        │
              ┌──────────┼──────────┐            │
              ▼          ▼          ▼            │
         ┌────────┐ ┌────────┐ ┌────────┐       │
         │  Move  │ │  Jump  │ │ Attack │       │
         └────────┘ └───┬────┘ └───┬────┘       │
              │         │          │            │
              │    ┌────▼────┐     │            │
              │    │  Fall   │     │            │
              │    └────┬────┘     │            │
              │         │          │            │
              ▼         ▼          ▼            │
         ┌────────────────────────────┐         │
         │           Skill            │         │
         │  (Skill1/Skill2/Ultimate)  │         │
         └─────────────┬──────────────┘         │
                       │                        │
              ┌────────┼────────┐              │
              ▼        ▼        ▼              │
         ┌────────┐ ┌────────┐ ┌────────┐     │
         │  Dash  │ │  Hit   │ │ Death  │     │
         └────┬───┘ └────┬───┘ └────────┘     │
              │          │                     │
              └──────────┴─────────────────────┘
```

### 상태별 설명

| 상태 | 설명 | 전이 조건 |
|------|------|-----------|
| **Idle** | 대기 상태 | 입력 없음 시 |
| **Move** | 이동 상태 | 방향키 입력 |
| **Jump** | 점프 상태 | 점프키 입력 |
| **Fall** | 낙하 상태 | 점프 정점 도달 |
| **Attack** | 기본공격 상태 | 공격키 입력 |
| **Skill** | 스킬 시전 상태 | 스킬키 입력 |
| **Dash** | 대시 상태 | 대시키 입력 |
| **Hit** | 피격 상태 | 데미지 받음 |
| **Death** | 사망 상태 | HP ≤ 0 |

---

## 5. 캐릭터 아트 프롬프트

### 5.1 기본 캐릭터 스프라이트

#### 캐릭터 컨셉 설명

젊은 마법사 캐릭터. 보라색/파란색 계열의 마법사 로브를 입고, 마법 지팡이를 들고 있다.
머리카락은 짧거나 중간 길이, 눈은 마나의 힘으로 은은하게 빛난다.
SD 비율(2등신)의 귀여우면서도 신비로운 분위기.

#### 프롬프트 (한글)

```
2D 픽셀아트 스타일, 다크 판타지 젊은 마법사 캐릭터, 작고 귀여운 SD 비율(2등신),
보라색과 파란색 그라데이션의 마법사 로브 착용, 로브 가장자리에 마법 문양 장식,
한 손에 보라색 크리스탈이 박힌 나무 지팡이 들고 있음,
짧은 은백색 머리카락, 마나 에너지로 은은하게 빛나는 보라색 눈,
로브 아래로 부츠 살짝 보임, 허리에 물약 파우치,
정면 뷰, T포즈, 클린 라인아트, 단순한 음영, 투명 배경,
게임 스프라이트용, 64x64 해상도에 적합한 디테일,
애니메이션용 파츠 분리 고려한 명확한 관절 구분
```

#### 프롬프트 (영문)

```
2D pixel art style, dark fantasy young wizard character, cute chibi SD proportion (2-head tall),
wearing purple and blue gradient wizard robe, magical rune decorations on robe edges,
holding wooden staff with purple crystal embedded in one hand,
short silver-white hair, purple eyes softly glowing with mana energy,
boots slightly visible under robe, potion pouch on belt,
front view, T-pose, clean lineart, simple shading, transparent background,
game sprite asset, detail suitable for 64x64 resolution,
clear joint separation for animation-ready parts
```

#### 컬러 팔레트

| 부위 | 색상 코드 | 설명 |
|------|----------|------|
| 로브 (메인) | #4B0082 | 인디고 |
| 로브 (밝은) | #6A5ACD | 슬레이트 블루 |
| 로브 (어두운) | #2E0854 | 다크 퍼플 |
| 로브 문양 | #00BFFF | 딥 스카이 블루 |
| 머리카락 | #E8E8E8 | 실버 화이트 |
| 머리카락 (음영) | #C0C0C0 | 실버 |
| 눈 발광 | #9370DB | 미디엄 퍼플 |
| 눈 하이라이트 | #E6E6FA | 라벤더 |
| 피부 | #FFE4C4 | 비스크 |
| 피부 (음영) | #DEB887 | 벌리우드 |
| 지팡이 (나무) | #8B4513 | 새들 브라운 |
| 지팡이 (크리스탈) | #9932CC | 다크 오키드 |
| 부츠 | #2F1810 | 다크 브라운 |

#### 추가 뷰 프롬프트

**측면 뷰 (한글)**
```
2D 픽셀아트 스타일, 다크 판타지 젊은 마법사 캐릭터, SD 비율(2등신),
보라색/파란색 마법사 로브, 은백색 짧은 머리, 크리스탈 지팡이,
측면 뷰, 걷는 포즈, 로브 자락 나부낌,
클린 라인아트, 투명 배경, 게임 스프라이트용
```

**측면 뷰 (영문)**
```
2D pixel art style, dark fantasy young wizard character, SD proportion (2-head tall),
purple/blue wizard robe, short silver-white hair, crystal staff,
side view, walking pose, flowing robe hem,
clean lineart, transparent background, game sprite asset
```

---

### 5.2 표정 시트

#### 프롬프트 (한글)

```
2D 픽셀아트 스타일, 마법사 캐릭터 표정 시트, SD 비율(2등신),
같은 캐릭터의 다양한 표정 모음:
1. 기본(중립) - 차분한 표정
2. 기쁨 - 눈 웃음, 입꼬리 올라감
3. 분노 - 찡그린 눈썹, 이글거리는 눈
4. 집중 - 눈 빛남, 진지한 표정
5. 피격 - 고통스러운 표정, 눈 질끈 감음
6. 승리 - 자신감 넘치는 미소
각 표정 정면 얼굴 클로즈업, 클린 라인아트, 투명 배경
```

#### 프롬프트 (영문)

```
2D pixel art style, wizard character expression sheet, SD proportion (2-head tall),
various expressions of the same character:
1. Neutral - calm expression
2. Happy - smiling eyes, upturned lips
3. Angry - furrowed brows, intense eyes
4. Focus - glowing eyes, serious look
5. Hurt - pained expression, eyes squeezed shut
6. Victory - confident smile
each expression front face close-up, clean lineart, transparent background
```

---

## 6. 스킬 이펙트 아트 프롬프트

### 6.1 마나 볼트 (기본공격)

#### 프롬프트 (한글)

```
2D 픽셀아트 스타일, 마법 투사체 이펙트, 마나 볼트,
빛나는 파란색/보라색 에너지 구체, 중심이 밝고 가장자리가 흐릿함,
뒤로 마나 입자 꼬리 효과, 빛나는 후광,
다양한 프레임: 생성(작음) → 비행(중간) → 폭발(확산),
투명 배경, 게임 이펙트 스프라이트 시트,
32x32 기본 크기, 발광 효과 강조
```

#### 프롬프트 (영문)

```
2D pixel art style, magic projectile effect, mana bolt,
glowing blue/purple energy sphere, bright center with blurred edges,
trailing mana particle tail effect, glowing aura,
multiple frames: spawn(small) → flight(medium) → explosion(spread),
transparent background, game effect sprite sheet,
32x32 base size, emphasized glow effect
```

#### 컬러 팔레트

| 요소 | 색상 코드 | 설명 |
|------|----------|------|
| 핵심 | #FFFFFF | 흰색 (중심부) |
| 메인 | #00BFFF | 딥 스카이 블루 |
| 서브 | #9370DB | 미디엄 퍼플 |
| 테두리 | #4B0082 | 인디고 |
| 파티클 | #E6E6FA | 라벤더 |

---

### 6.2 플레임 버스트 (스킬1)

#### 프롬프트 (한글)

```
2D 픽셀아트 스타일, 화염 마법 이펙트, 플레임 버스트,
전방으로 분출하는 부채꼴 형태의 화염,
주황색/빨간색/노란색 화염 그라데이션,
화염 가장자리에 불꽃 입자 흩날림,
프레임 순서: 집중(지팡이 끝 불꽃) → 분출(화염 확산) → 소멸(잔불),
투명 배경, 게임 이펙트 스프라이트 시트,
64x64 크기, 강렬한 발광 효과
```

#### 프롬프트 (영문)

```
2D pixel art style, fire magic effect, flame burst,
fan-shaped flame bursting forward,
orange/red/yellow flame gradient,
fire particles scattering at flame edges,
frame sequence: charge(flame at staff tip) → burst(flame spread) → fade(embers),
transparent background, game effect sprite sheet,
64x64 size, intense glow effect
```

#### 컬러 팔레트

| 요소 | 색상 코드 | 설명 |
|------|----------|------|
| 핵심 | #FFFFFF | 흰색 (가장 뜨거운 부분) |
| 밝은 화염 | #FFD700 | 골드 |
| 메인 화염 | #FF6600 | 오렌지 |
| 어두운 화염 | #FF0000 | 레드 |
| 테두리 | #8B0000 | 다크 레드 |
| 연기 | #4A4A4A | 다크 그레이 |

---

### 6.3 프로스트 노바 (스킬2)

#### 프롬프트 (한글)

```
2D 픽셀아트 스타일, 얼음 마법 이펙트, 프로스트 노바,
원형으로 확산되는 얼음 파동, 중심에서 바깥으로 퍼져나감,
하늘색/흰색/연한 파란색 얼음 크리스탈,
날카로운 얼음 조각들이 방사형으로 튀어나옴,
프레임 순서: 집중(발밑 마법진) → 폭발(얼음 파동) → 결정화(바닥 얼음),
투명 배경, 게임 이펙트 스프라이트 시트,
64x64 크기, 차가운 발광 효과
```

#### 프롬프트 (영문)

```
2D pixel art style, ice magic effect, frost nova,
circular ice wave expanding, spreading from center outward,
sky blue/white/light blue ice crystals,
sharp ice shards shooting out radially,
frame sequence: charge(magic circle at feet) → burst(ice wave) → crystallize(floor ice),
transparent background, game effect sprite sheet,
64x64 size, cold glow effect
```

#### 컬러 팔레트

| 요소 | 색상 코드 | 설명 |
|------|----------|------|
| 핵심 | #FFFFFF | 흰색 |
| 밝은 얼음 | #E0FFFF | 라이트 시안 |
| 메인 얼음 | #87CEEB | 스카이 블루 |
| 어두운 얼음 | #4169E1 | 로열 블루 |
| 크리스탈 | #ADD8E6 | 라이트 블루 |
| 서리 | #F0F8FF | 앨리스 블루 |

---

### 6.4 아케인 스톰 (궁극기)

#### 프롬프트 (한글)

```
2D 픽셀아트 스타일, 아케인 마법 이펙트, 아케인 스톰,
하늘에서 내리꽂히는 보라색 에너지 소용돌이,
거대한 마법진이 바닥에 나타남, 위에서 번개와 마나 폭발,
보라색/마젠타/흰색 에너지 조합,
프레임: 마법진 생성 → 소용돌이 형성 → 번개 낙하(반복) → 최종 폭발,
투명 배경, 게임 이펙트 스프라이트 시트,
128x128 크기, 강렬한 발광 및 입자 효과
```

#### 프롬프트 (영문)

```
2D pixel art style, arcane magic effect, arcane storm,
purple energy vortex striking down from sky,
large magic circle appearing on ground, lightning and mana explosions from above,
purple/magenta/white energy combination,
frames: magic circle spawn → vortex formation → lightning strikes(loop) → final explosion,
transparent background, game effect sprite sheet,
128x128 size, intense glow and particle effects
```

#### 컬러 팔레트

| 요소 | 색상 코드 | 설명 |
|------|----------|------|
| 핵심 | #FFFFFF | 흰색 |
| 메인 에너지 | #9400D3 | 다크 바이올렛 |
| 서브 에너지 | #8B008B | 다크 마젠타 |
| 번개 | #DA70D6 | 오키드 |
| 마법진 | #4B0082 | 인디고 |
| 입자 | #E6E6FA | 라벤더 |
| 폭발 | #FF00FF | 마젠타 |

---

## 7. 애니메이션 가이드

### 캐릭터 애니메이션 스펙

| 애니메이션 | 프레임 수 | FPS | 루프 | 설명 |
|-----------|----------|-----|------|------|
| Idle | 6 | 8 | Yes | 호흡 + 로브 흔들림 |
| Walk | 8 | 12 | Yes | 걷기 사이클 |
| Run | 6 | 15 | Yes | 달리기 사이클 |
| Jump_Up | 3 | 12 | No | 점프 상승 |
| Jump_Fall | 2 | 10 | Yes | 낙하 |
| Jump_Land | 3 | 15 | No | 착지 |
| Attack_Basic | 4 | 15 | No | 마나 볼트 발사 |
| Skill1_Cast | 6 | 12 | No | 플레임 버스트 시전 |
| Skill2_Cast | 6 | 12 | No | 프로스트 노바 시전 |
| Ultimate_Cast | 10 | 10 | No | 아케인 스톰 시전 |
| Dash | 4 | 20 | No | 대시 |
| Hit | 3 | 15 | No | 피격 |
| Death | 8 | 10 | No | 사망 |

### 분리 레이어 (파츠)

```
캐릭터 레이어 구조:
├── Head (머리)
│   ├── Hair (머리카락)
│   ├── Face (얼굴)
│   └── Eyes (눈 - 발광 효과 별도)
├── Body (몸통)
│   ├── Robe_Upper (로브 상체)
│   └── Robe_Lower (로브 하체)
├── Left_Arm (왼팔)
├── Right_Arm (오른팔)
│   └── Staff (지팡이)
├── Left_Leg (왼다리)
├── Right_Leg (오른다리)
└── Effects (이펙트 레이어)
    ├── Eye_Glow (눈 발광)
    ├── Staff_Glow (지팡이 발광)
    └── Robe_Particles (로브 마법 입자)
```

### 키 포즈 가이드

| 포즈 | 설명 | 용도 |
|------|------|------|
| T-Pose | 팔 수평, 정면 | 기본 스프라이트 |
| Idle_Base | 자연스러운 서기 | Idle 애니메이션 기준 |
| Cast_Ready | 지팡이 앞으로 | 스킬 시전 시작 |
| Cast_Release | 지팡이 휘두름 | 스킬 발동 순간 |
| Jump_Apex | 최고점 포즈 | 점프 정점 |
| Hit_Recoil | 뒤로 젖혀짐 | 피격 리액션 |

---

## 8. Unity 구현 가이드

### ScriptableObject 데이터 구조

```csharp
// 스킬 데이터 SO
[CreateAssetMenu(fileName = "SkillData", menuName = "GASPT/Skills/SkillData")]
public class SkillData : ScriptableObject
{
    [Header("기본 정보")]
    public string skillName;
    public string skillNameKR;
    public Sprite icon;
    public string description;

    [Header("스킬 타입")]
    public SkillType skillType;
    public ElementType elementType;
    public TargetType targetType;

    [Header("수치")]
    public float baseDamage;
    public float damageCoefficient;
    public float cooldown;
    public float manaCost;
    public float range;
    public float radius;

    [Header("이펙트")]
    public GameObject castEffect;
    public GameObject hitEffect;
    public GameObject projectilePrefab;

    [Header("애니메이션")]
    public string animationTrigger;
    public float castTime;
}

public enum SkillType { Basic, Skill, Ultimate }
public enum ElementType { Pure, Fire, Ice, Arcane }
public enum TargetType { Single, AoE, Self, Projectile }
```

### Import Settings

```
Texture Type: Sprite (2D and UI)
Sprite Mode: Multiple (스프라이트 시트)
Pixels Per Unit: 64
Filter Mode: Point (no filter)
Compression: None
Max Size: 512
```

### Animator Controller 파라미터

```
// Bool
isMoving
isGrounded
isCasting
isDashing

// Trigger
attack
skill1
skill2
ultimate
hit
death
land

// Float
moveSpeed
verticalVelocity

// Int
skillIndex (0: basic, 1: skill1, 2: skill2, 3: ultimate)
```

---

## 9. 에셋 폴더 구조

### 권장 폴더 구조

```
Assets/_Project/Art/Characters/Player/Wizard/
├── Sprites/
│   ├── Wizard_Idle.png
│   ├── Wizard_Walk.png
│   ├── Wizard_Run.png
│   ├── Wizard_Jump.png
│   ├── Wizard_Attack.png
│   ├── Wizard_Skill1.png
│   ├── Wizard_Skill2.png
│   ├── Wizard_Ultimate.png
│   ├── Wizard_Dash.png
│   ├── Wizard_Hit.png
│   ├── Wizard_Death.png
│   └── Wizard_Expressions.png
├── Animations/
│   ├── Wizard_Idle.anim
│   ├── Wizard_Walk.anim
│   ├── Wizard_Run.anim
│   ├── Wizard_Jump.anim
│   ├── Wizard_Attack.anim
│   ├── Wizard_Skill1.anim
│   ├── Wizard_Skill2.anim
│   ├── Wizard_Ultimate.anim
│   ├── Wizard_Dash.anim
│   ├── Wizard_Hit.anim
│   ├── Wizard_Death.anim
│   └── WizardAnimatorController.controller
└── Data/
    └── WizardStatsData.asset

Assets/_Project/Art/Effects/Skills/Wizard/
├── ManaBolt/
│   ├── ManaBolt_Spawn.png
│   ├── ManaBolt_Flight.png
│   ├── ManaBolt_Hit.png
│   └── ManaBolt_SpriteSheet.png
├── FlameBurst/
│   ├── FlameBurst_Charge.png
│   ├── FlameBurst_Burst.png
│   ├── FlameBurst_Fade.png
│   └── FlameBurst_SpriteSheet.png
├── FrostNova/
│   ├── FrostNova_Circle.png
│   ├── FrostNova_Burst.png
│   ├── FrostNova_Ice.png
│   └── FrostNova_SpriteSheet.png
└── ArcaneStorm/
    ├── ArcaneStorm_Circle.png
    ├── ArcaneStorm_Vortex.png
    ├── ArcaneStorm_Lightning.png
    ├── ArcaneStorm_Explosion.png
    └── ArcaneStorm_SpriteSheet.png
```

### 네이밍 컨벤션

```
{캐릭터명}_{상태/애니메이션}.png
{스킬명}_{단계}.png
{스킬명}_SpriteSheet.png
```

---

## 부록: 밸런싱 노트

### 데미지 비교표 (레벨 1 기준)

| 스킬 | 단일 타겟 | 범위 | DPS | 비고 |
|------|----------|------|-----|------|
| 마나 볼트 | 15 | 단일 | 50 | 연사 가능 |
| 플레임 버스트 | 37.5 + DoT | 부채꼴 | ~15 | 화상 포함 |
| 프로스트 노바 | 30~45 | 원형 | ~5 | CC기 |
| 아케인 스톰 | 180 (총) | 원형 | 45 | 궁극기 |

### 마나 효율 분석

| 스킬 | 마나 | 데미지 | 효율 (데미지/마나) |
|------|------|--------|-------------------|
| 마나 볼트 | 0 | 15 | ∞ (무료) |
| 플레임 버스트 | 25 | 37.5 | 1.5 |
| 프로스트 노바 | 35 | 30~45 | 0.86~1.28 |
| 아케인 스톰 | 80 | 180 | 2.25 |

### 테스트 포인트

- [ ] 기본공격만으로 일반 몬스터 처치 가능한가?
- [ ] 스킬1 쿨다운 중 기본공격으로 DPS 유지 가능한가?
- [ ] 스킬2의 CC 효과가 위기 탈출에 유용한가?
- [ ] 궁극기가 보스전에서 의미 있는 데미지인가?
- [ ] 마나 관리가 적절한 긴장감을 주는가?

---

**문서 버전**: v1.0
**최종 업데이트**: 2025-11-29
**작성자**: Claude Code Assistant

---

*이 문서는 GASPT 프로젝트의 플레이어 마법사 캐릭터 기획 및 아트 에셋 제작을 위한 종합 가이드입니다.*
