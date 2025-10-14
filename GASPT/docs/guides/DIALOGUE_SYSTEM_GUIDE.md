# 📖 대화 시스템 가이드

> 엑셀 기반 에피소드 대화 시스템 사용 가이드

---

## 🎯 개요

GASPT의 대화 시스템은 **실무 방식**으로 설계되었습니다:
- 기획자가 **엑셀로 대화 작성** → CSV 저장
- **에피소드 ID**로 NPC별 대화 관리
- 선택지 분기, 상점 구매 등 다양한 기능 지원

---

## 📊 시스템 구조

### **핵심 컴포넌트**

```
DialogueDatabase (싱글톤)
    ↓ CSV 로드
DialogueManager (싱글톤)
    ↓ 대화 재생
NPCController → DialoguePanel (UI)
    ↓ 플레이어 상호작용
Player
```

### **데이터 흐름**

```
1. [게임 시작] DialogueDatabase가 CSV 로드
2. [플레이어가 NPC 접근] "E키를 눌러 대화하기" 표시
3. [E키 입력] NPCController가 에피소드 ID 전달
4. [DialogueManager] 에피소드 재생 시작
5. [DialoguePanel] 대화 UI 표시
6. [선택지/계속] 노드 진행
7. [대화 종료] Panel 닫기
```

---

## 📁 CSV 파일 구조

### **1. EpisodeTable.csv** (에피소드 정보)

| EpisodeID | EpisodeName | NPCName | EpisodeType | Description |
|-----------|-------------|---------|-------------|-------------|
| EP_STORY_001 | 첫 만남 | 마을사람 | Story | 마을사람과의 첫 대화 |
| EP_SHOP_001 | 상점 대화 | 상인 | Shop | 상인과의 상점 대화 |

**컬럼 설명:**
- `EpisodeID`: 고유 ID (예: EP_STORY_001)
- `EpisodeName`: 에피소드 이름
- `NPCName`: NPC 이름
- `EpisodeType`: Story, Shop, Quest, Tutorial, Event
- `Description`: 설명

### **2. DialogueTable.csv** (대화 노드)

| EpisodeID | NodeID | SpeakerName | DialogueText | NextNodeID | HasChoices |
|-----------|--------|-------------|--------------|------------|------------|
| EP_STORY_001 | 1 | 마을사람 | 안녕하세요 용사님! | 2 | FALSE |
| EP_STORY_001 | 2 | 마을사람 | 도와주실 수 있나요? | 0 | TRUE |

**컬럼 설명:**
- `EpisodeID`: 속한 에피소드 ID
- `NodeID`: 노드 번호 (1부터 시작, 1이 시작 노드)
- `SpeakerName`: 화자 이름
- `DialogueText`: 대사 내용
- `NextNodeID`: 다음 노드 번호 (0이면 종료)
- `HasChoices`: 선택지 있음 여부 (TRUE/FALSE)

### **3. ChoiceTable.csv** (선택지)

| EpisodeID | NodeID | ChoiceID | ChoiceText | NextNodeID | RequiredGold | RewardItem |
|-----------|--------|----------|------------|------------|--------------|------------|
| EP_STORY_001 | 2 | 1 | 예, 돕겠습니다 | 3 | 0 | |
| EP_SHOP_001 | 2 | 1 | 체력 물약 (50G) | 3 | 50 | HealthPotion |

**컬럼 설명:**
- `EpisodeID`: 속한 에피소드 ID
- `NodeID`: 선택지가 표시되는 노드 번호
- `ChoiceID`: 선택지 번호 (해당 노드 내에서)
- `ChoiceText`: 선택지 텍스트
- `NextNodeID`: 선택 시 이동할 노드 (999면 종료)
- `RequiredGold`: 필요한 골드 (상점용, 0이면 무료)
- `RewardItem`: 보상 아이템 ID (상점용)

---

## 🎨 에피소드 작성 가이드

### **예제 1: 간단한 스토리 대화**

```csv
# EpisodeTable.csv
EP_STORY_001,첫 만남,마을사람,Story,마을사람과의 첫 대화

# DialogueTable.csv
EP_STORY_001,1,마을사람,안녕하세요!,2,FALSE
EP_STORY_001,2,마을사람,반갑습니다!,0,FALSE
```

**결과:**
1. "안녕하세요!" → Continue
2. "반갑습니다!" → 종료

### **예제 2: 선택지가 있는 대화**

```csv
# DialogueTable.csv
EP_STORY_001,1,마을사람,도와주실 수 있나요?,0,TRUE

# ChoiceTable.csv
EP_STORY_001,1,1,예 돕겠습니다,2,0,
EP_STORY_001,1,2,나중에요,3,0,
```

**결과:**
1. "도와주실 수 있나요?" → 선택지 표시
   - "예, 돕겠습니다" → Node 2로 이동
   - "나중에요" → Node 3로 이동

### **예제 3: 상점 대화**

```csv
# DialogueTable.csv
EP_SHOP_001,1,상인,어서오세요!,2,FALSE
EP_SHOP_001,2,상인,뭘 사시겠어요?,0,TRUE
EP_SHOP_001,3,상인,구매 감사합니다!,2,FALSE

# ChoiceTable.csv
EP_SHOP_001,2,1,체력 물약 (50G),3,50,HealthPotion
EP_SHOP_001,2,2,공격력 스킬 (100G),3,100,AttackBoost
EP_SHOP_001,2,3,돌아가기,999,0,
```

**결과:**
1. "어서오세요!" → Continue
2. "뭘 사시겠어요?" → 선택지 표시
   - "체력 물약 (50G)" → 골드 50 차감 → Node 3
   - "공격력 스킬 (100G)" → 골드 100 차감 → Node 3
   - "돌아가기" → 대화 종료 (999)

---

## 🛠️ Unity에서 NPC 설정

### **1. NPCData ScriptableObject 생성**

```
1. Project 창에서 우클릭
2. Create → GASPT → NPC → NPC Data
3. 이름: "StoryNPC_VillagerData"
4. Inspector에서 설정:
   - NPC Name: "마을사람"
   - NPC Type: Story
   - Episode IDs:
     * Element 0: EP_STORY_001
     * Element 1: EP_STORY_002
```

### **2. Lobby 씬에 NPC 배치**

```
1. 빈 GameObject 생성: "NPC_Villager"
2. 컴포넌트 추가:
   - StoryNPC (또는 ShopNPC)
   - SpriteRenderer (NPC 이미지)
   - BoxCollider2D (Trigger 체크)
3. StoryNPC Inspector 설정:
   - NPC Data: StoryNPC_VillagerData 할당
   - Show Debug Log: 체크 (디버그용)
```

### **3. Player에 "Player" 태그 설정**

```
1. Player GameObject 선택
2. Inspector 상단에서 Tag → Player
```

---

## 🎮 게임 플레이 흐름

### **플레이어 시점**

```
1. [로비 씬 입장]
2. [NPC에게 접근] → "E를 눌러 대화하기" 표시
3. [E키 입력] → 대화창 열림
4. [대사 표시] → Space/Enter 또는 Continue 버튼
5. [선택지 표시] → 버튼 클릭
6. [대화 종료] → 대화창 닫힘
```

### **개발자 시점**

```csharp
// NPCController가 자동으로 처리:
1. OnTriggerEnter2D → OnPlayerEnterRange()
2. Input.GetKeyDown(E) → OnInteract()
3. DialogueManager.StartEpisode("EP_STORY_001")
4. DialoguePanel이 자동으로 열림
5. 대화 진행...
6. DialoguePanel이 자동으로 닫힘
```

---

## 💻 코드 예제

### **수동으로 대화 시작**

```csharp
using Gameplay.Dialogue;

// 특정 에피소드 재생
DialogueManager.Instance.StartEpisode("EP_STORY_001");
```

### **대화 이벤트 구독**

```csharp
using Gameplay.Dialogue;

void Start()
{
    DialogueManager.Instance.OnDialogueStarted += OnDialogueStart;
    DialogueManager.Instance.OnDialogueEnded += OnDialogueEnd;
}

void OnDialogueStart(string episodeID)
{
    Debug.Log($"대화 시작: {episodeID}");
}

void OnDialogueEnd()
{
    Debug.Log("대화 종료");
}
```

### **커스텀 NPC 만들기**

```csharp
using Gameplay.NPC;
using Gameplay.Dialogue;

public class QuestNPC : NPCController
{
    protected override void OnDialogueStarted()
    {
        base.OnDialogueStarted();

        // 퀘스트 시작 로직
        Debug.Log("퀘스트 NPC와 대화 시작!");
    }
}
```

---

## 🔧 고급 기능

### **조건부 에피소드 선택**

```csharp
public class ConditionalNPC : NPCController
{
    protected override string GetCurrentEpisodeID()
    {
        // 퀘스트 완료 여부에 따라 다른 에피소드
        if (QuestManager.IsQuestCompleted("Quest001"))
        {
            return "EP_STORY_AFTER_QUEST";
        }
        else
        {
            return "EP_STORY_BEFORE_QUEST";
        }
    }
}
```

### **상점 구매 커스터마이징**

```csharp
// DialogueManager.cs의 ProcessShopPurchase() 메서드 수정
// 아이템 지급, 스킬 추가 등 로직 추가
```

---

## 📝 체크리스트

### **CSV 작성 체크리스트**

- [ ] EpisodeTable에 에피소드 정보 추가
- [ ] DialogueTable에 대화 노드 추가
- [ ] NodeID는 1부터 시작
- [ ] NextNodeID가 0이면 종료
- [ ] 선택지가 있으면 HasChoices = TRUE
- [ ] ChoiceTable에 선택지 추가
- [ ] 종료 선택지는 NextNodeID = 999

### **Unity 설정 체크리스트**

- [ ] CSV 파일을 Resources/Dialogues에 배치
- [ ] NPCData ScriptableObject 생성
- [ ] NPC GameObject에 StoryNPC/ShopNPC 추가
- [ ] SpriteRenderer 설정
- [ ] Collider2D를 Trigger로 설정
- [ ] Player에 "Player" 태그 설정
- [ ] DialoguePanel Prefab 생성 및 UIManager에 등록

---

## 🐛 문제 해결

### **"에피소드를 찾을 수 없습니다"**
→ CSV 파일 경로 확인 (Resources/Dialogues)
→ EpisodeID 철자 확인

### **대화창이 안 열림**
→ DialoguePanel Prefab이 UIManager에 등록되었는지 확인
→ PanelType.Dialog로 등록되어 있는지 확인

### **NPC와 상호작용 안 됨**
→ Player에 "Player" 태그 설정 확인
→ Collider2D가 Trigger인지 확인
→ NPCData가 할당되었는지 확인

### **골드 차감 안 됨**
→ GameManager가 씬에 있는지 확인
→ GameManager의 골드 초기값 확인

---

## 📚 추가 자료

- **샘플 CSV**: `Assets/_Project/Resources/Dialogues/`
- **샘플 NPC**: `Assets/_Project/Scripts/Gameplay/NPC/Types/`
- **코드 레퍼런스**: 각 클래스 주석 참조

---

*작성일: 2025-10-15*
*작성자: GASPT 프로젝트팀*
