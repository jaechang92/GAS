# PlayerCharacter 수동 생성 가이드

PlayerCharacter 프리팩이 missing 에러가 발생할 때 수동으로 완전한 캐릭터를 만드는 방법입니다.

## 🛠️ 방법 1: 자동 생성 도구 사용 (권장)

### 1단계: 자동 생성 도구 실행
```
Unity 메뉴 → Tools → Project → Create Player Character
```

### 2단계: 자동 설정
1. **"자동으로 Skull.png 찾기"** 버튼 클릭
2. **"자동으로 Physics Config 찾기"** 버튼 클릭
3. **"PlayerCharacter 생성"** 버튼 클릭 (프리팩 생성)
   또는
   **"씬에 PlayerCharacter 배치"** 버튼 클릭 (직접 배치)

---

## 🔧 방법 2: 수동 생성 (단계별)

### 1단계: 기본 GameObject 생성
1. Hierarchy에서 우클릭 → **Create Empty**
2. 이름을 **"PlayerCharacter"**로 변경
3. Position을 **(0, 0, 0)**으로 설정

### 2단계: SpriteRenderer 추가
1. PlayerCharacter 선택
2. **Add Component** → **Sprite Renderer**
3. **Sprite** 필드에 **Skull.png** 할당
   - 경로: `Assets/_Project/Art/Sprites/Characters/Sample/Skull.png`
4. **Sorting Order**를 **10**으로 설정

### 3단계: BoxCollider2D 추가
1. **Add Component** → **Box Collider 2D**
2. **Size**: X=0.8, Y=0.9
3. **Offset**: X=0, Y=-0.05

### 4단계: 물리 시스템 컴포넌트 추가

#### RaycastController
1. **Add Component** → **RaycastController**
2. **Collision Mask**: Layer 3 (Ground) 체크
3. **Skin Width**: 0.08
4. **Horizontal Ray Count**: 4
5. **Vertical Ray Count**: 4

#### MovementCalculator
1. **Add Component** → **MovementCalculator**
2. **Config**: CharacterPhysicsConfig 할당
   - 경로: `Assets/_Project/Scripts/Data/CharacterPhysicsConfig.asset`

### 5단계: 플레이어 컨트롤 컴포넌트 추가

#### PlayerController
1. **Add Component** → **PlayerController**
2. 기본 설정 확인:
   - **Move Speed**: 8
   - **Jump Force**: 15
   - **Dash Speed**: 20
   - **Ground Layer Mask**: Layer 3 체크

#### InputHandler
1. **Add Component** → **InputHandler**

#### RaycastController
1. **Add Component** → **RaycastController**
2. **Collision Mask**: Layer 3 (Ground) 체크
3. **Skin Width**: 0.02
4. **Horizontal Ray Count**: 6
5. **Vertical Ray Count**: 6

#### AnimationController
1. **Add Component** → **AnimationController**

### 6단계: GAS 시스템 추가

#### AbilitySystem
1. **Add Component** → **AbilitySystem**
2. **Use Resource System**: 체크
3. **Initial Abilities**: 빈 상태로 유지

### 7단계: 프리팹으로 저장 (선택사항)
**리팩토링된 PlayerController는 모든 컴포넌트를 자동으로 연결합니다!**
1. PlayerCharacter를 Project 창의 `Assets/_Project/Prefabs/` 폴더로 드래그
2. 프리팹 이름을 **"PlayerCharacter"**로 설정

---

## ⚡ 빠른 설정 체크리스트

### 필수 컴포넌트 확인 (자동 생성)
- [ ] SpriteRenderer (Skull.png 할당)
- [ ] BoxCollider2D
- [ ] PlayerController ⭐ (다른 컴포넌트들을 자동으로 추가)

### 주요 설정 확인
- [ ] Sprite Sorting Order: 10
- [ ] CharacterPhysicsConfig 할당 (선택사항)

### 테스트 방법
1. Sample 씬 열기
2. PlayerCharacter를 씬에 배치
3. Play 버튼 누르기
4. WASD로 이동, Space로 점프, Shift로 대시 테스트

---

## 🔧 문제 해결

### Skull.png를 찾을 수 없는 경우
```
경로: Assets/_Project/Art/Sprites/Characters/Sample/Skull.png
```
해당 파일이 없다면 다른 캐릭터 스프라이트를 임시로 사용하세요.

### CharacterPhysicsConfig가 없는 경우
1. Project 창에서 우클릭
2. **Create** → **Character** → **Physics Config**
3. Skul Preset 적용

### 컴포넌트를 찾을 수 없는 경우
해당 스크립트 파일이 있는지 확인:
- `Assets/_Project/Scripts/Gameplay/Player/`

### Ground와 충돌하지 않는 경우
1. Ground 오브젝트의 Layer가 3인지 확인
2. PlayerController의 Ground Layer Mask에 Layer 3이 체크되어 있는지 확인
3. RaycastController의 Collision Mask에 Layer 3이 체크되어 있는지 확인

---

## 🎮 완성된 PlayerCharacter 기능

- **이동**: WASD 키
- **점프**: Space 키
- **대시**: Shift 키
- **원웨이 플랫폼 관통**: S + Space
- **벽 점프**: 벽에 붙어서 Space
- **더블 점프**: 공중에서 Space
- **FSM 상태 관리**: Idle/Move/Jump/Dash/Attack 등
- **GAS 어빌리티 시스템**: 확장 가능한 스킬 시스템
- **정밀한 물리**: Skul 스타일 플랫폼 액션

이제 완전히 기능하는 PlayerCharacter가 준비되었습니다!