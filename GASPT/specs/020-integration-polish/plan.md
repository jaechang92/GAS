# GASPT 통합 및 폴리싱 계획

**기능 번호**: 020
**작성일**: 2025-12-14

---

## 1. 기술 스택

| 영역 | 기술 |
|------|------|
| 엔진 | Unity 6.0 |
| 언어 | C# |
| 패턴 | MVP, Singleton, Object Pool |
| VFX | Unity Particle System |
| Audio | Unity Audio System |

---

## 2. 구현 전략

### 2.1 시각 효과 시스템
```
StatusEffectVisual
├─ ISpriteEffect (색상 변경)
├─ IParticleEffect (파티클 생성)
└─ StatusEffectManager 연동
```

### 2.2 사운드 시스템
```
AudioManager (Singleton)
├─ SFXPool (오디오 풀링)
├─ BGMController (배경음악)
└─ Volume Settings
```

### 2.3 폼 확장 구조
```
BaseForm
├─ MageForm ✅
├─ WarriorForm ✅
├─ AssassinForm ✅
├─ FlameMageForm (예정)
└─ FrostMageForm (예정)
```

---

## 3. 파일 구조

```
Assets/_Project/Scripts/
├─ StatusEffects/
│   ├─ StatusEffectVisual.cs (신규)
│   └─ StatusEffectManager.cs (수정)
├─ Audio/
│   ├─ AudioManager.cs (신규)
│   └─ SFXPool.cs (신규)
├─ Gameplay/Form/
│   ├─ Implementations/
│   │   ├─ FlameMageForm.cs (신규)
│   │   └─ FrostMageForm.cs (신규)
│   └─ Abilities/
│       ├─ FireStormAbility.cs (신규)
│       ├─ MeteorStrikeAbility.cs (신규)
│       ├─ IceLanceAbility.cs (신규)
│       └─ FrozenGroundAbility.cs (신규)
└─ ...

Assets/_Project/Prefabs/
├─ VFX/
│   └─ StatusEffects/
│       ├─ BurnVFX.prefab (신규)
│       ├─ FreezeVFX.prefab (신규)
│       └─ SlowVFX.prefab (신규)
└─ ...
```

---

## 4. 의존성

| 시스템 | 의존 대상 |
|--------|----------|
| StatusEffectVisual | StatusEffectManager |
| SFXPool | AudioManager |
| FlameMageForm | BaseForm, FormData |
| FrostMageForm | BaseForm, FormData |

---

## 5. 단계별 목표

| Phase | 목표 | 결과물 |
|-------|------|--------|
| 2 | 시각 피드백 | 상태 효과 VFX |
| 3 | 오디오 피드백 | 효과음 시스템 |
| 4 | 컨텐츠 확장 | 추가 폼 2종 |
| 5 | 밸런싱 | 최종 조정 |

---

*최종 수정: 2025-12-14*
