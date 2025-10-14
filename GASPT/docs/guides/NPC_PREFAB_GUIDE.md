# 🎮 NPC 프리팹 생성 가이드

> NPCPrefabMaker를 사용하여 NPC 프리팹을 자동으로 생성하는 가이드

---

## 🎯 개요

GASPT의 NPC 시스템은 **자동화 도구**를 제공합니다:
- **NPCPrefabMaker**: NPC 프리팹과 NPCData를 자동으로 생성
- **StoryNPC**: 스토리를 전달하는 NPC
- **ShopNPC**: 아이템/스킬을 판매하는 NPC

---

## 📊 시스템 구조

### **파일 구조**

```
Assets/_Project/
├── Prefabs/
│   └── NPC/
│       ├── StoryNPC_Villager.prefab
│       └── ShopNPC_Merchant.prefab
└── Resources/
    └── NPCData/
        ├── StoryNPC_VillagerData.asset
        └── ShopNPC_MerchantData.asset
```

### **NPC 프리팹 구성**

각 NPC 프리팹은 다음 컴포넌트를 포함합니다:
- ✅ **SpriteRenderer** - NPC 외형 (임시 단색 스프라이트)
- ✅ **BoxCollider2D** - Trigger로 설정, 플레이어 감지
- ✅ **StoryNPC / ShopNPC** - NPC 행동 스크립트
- ✅ **NPCData** - NPC 설정 데이터 (자동 연결)

---

## 🛠️ NPC 프리팹 생성 방법

### **방법 1: 자동 생성 (권장)**

Unity 에디터 상단 메뉴:

```
GASPT → NPC → Create All NPCs
```

이 메뉴는 다음을 자동으로 생성합니다:
1. **NPCData ScriptableObject**
   - StoryNPC_VillagerData.asset
   - ShopNPC_MerchantData.asset
2. **NPC Prefab**
   - StoryNPC_Villager.prefab
   - ShopNPC_Merchant.prefab

### **방법 2: 개별 생성**

Unity 에디터 상단 메뉴:

```
GASPT → NPC → Open NPC Prefab Maker
```

창이 열리면:
1. **StoryNPC Data 생성 (마을사람)** 버튼 클릭
2. **ShopNPC Data 생성 (상인)** 버튼 클릭
3. **StoryNPC Prefab 생성 (마을사람)** 버튼 클릭
4. **ShopNPC Prefab 생성 (상인)** 버튼 클릭

### **방법 3: 빠른 메뉴**

개별 생성용 빠른 메뉴:
```
GASPT → NPC → Create StoryNPC Data
GASPT → NPC → Create ShopNPC Data
```

---

## 📝 생성되는 NPCData 설정

### **StoryNPC_VillagerData**

| 항목 | 값 |
|------|------|
| NPC Name | 마을사람 |
| NPC Type | Story |
| Episode IDs | EP_STORY_001, EP_STORY_002 |
| Interaction Range | 2f |
| Interaction Key | E |
| Interaction Prompt | E를 눌러 대화하기 |

### **ShopNPC_MerchantData**

| 항목 | 값 |
|------|------|
| NPC Name | 상인 |
| NPC Type | Shop |
| Episode IDs | EP_SHOP_001 |
| Interaction Range | 2f |
| Interaction Key | E |
| Interaction Prompt | E를 눌러 상점 열기 |

---

## 🎨 프리팹 커스터마이징

### **1. 스프라이트 변경**

생성된 프리팹에는 임시 단색 스프라이트가 적용되어 있습니다:
- **StoryNPC**: 파란색 (0.3, 0.6, 0.9)
- **ShopNPC**: 주황색 (0.9, 0.6, 0.2)

실제 스프라이트로 교체하려면:

```
1. Project 창에서 프리팹 더블클릭
2. SpriteRenderer의 Sprite 필드 변경
3. 원하는 스프라이트 드래그 앤 드롭
4. Prefab 저장 (Ctrl+S)
```

### **2. Collider 크기 조정**

NPC 크기에 맞게 Collider 조정:

```
1. BoxCollider2D 컴포넌트 선택
2. Size: (1, 1.5) - 기본값
3. Offset: (0, 0.75) - 중심 위치
4. Edit Collider 버튼으로 시각적 조정 가능
```

### **3. NPCData 설정 변경**

Project 창에서 NPCData 파일 선택:

```
Assets/_Project/Resources/NPCData/StoryNPC_VillagerData
```

Inspector에서 수정 가능:
- **NPC Name**: NPC 이름
- **Episode IDs**: 대화 에피소드 ID 목록
- **Interaction Range**: 상호작용 거리
- **Interaction Prompt**: 상호작용 안내 텍스트

---

## 🎮 Lobby 씬에 NPC 배치

### **1. 프리팹 배치**

Lobby 씬을 열고:

```
1. Hierarchy에서 NPCSpawnPoints 찾기
2. Project → Assets/_Project/Prefabs/NPC/
3. StoryNPC_Villager.prefab을 StoryNPC_Spawn 위치에 드래그
4. ShopNPC_Merchant.prefab을 ShopNPC_Spawn 위치에 드래그
```

### **2. 위치 조정**

SceneSetupTool이 생성한 기본 위치:
- **StoryNPC_Spawn**: (-5, 1, 0)
- **ShopNPC_Spawn**: (5, 1, 0)
- **QuestNPC_Spawn**: (0, 1, 0)

원하는 위치로 이동:
```
1. Hierarchy에서 NPC 선택
2. Transform 컴포넌트에서 Position 조정
3. 또는 Scene 뷰에서 드래그
```

---

## 🧪 테스트

### **1. Player 설정 확인**

Player GameObject에 "Player" 태그 필수:

```
1. Hierarchy에서 Player 선택
2. Inspector 상단 Tag → Player
```

### **2. DialoguePanel 설정 확인**

UIManager에 DialoguePanel 등록 확인:

```
1. Hierarchy에서 UIManager 선택
2. Inspector에서 Panel List 확인
3. DialoguePanel이 PanelType.Dialog로 등록되어 있어야 함
```

### **3. 테스트 플레이**

```
1. Lobby 씬 실행
2. NPC에게 접근 → "E를 눌러 대화하기" 표시
3. E키 입력 → 대화창 열림
4. Space/Enter로 대화 진행
5. 선택지 클릭
```

---

## 🔧 고급 기능

### **커스텀 NPC 타입 추가**

새로운 NPC 타입을 추가하려면:

```csharp
// 1. NPCType Enum에 추가
public enum NPCType
{
    Story,
    Shop,
    Quest,    // NEW
    Tutorial  // NEW
}

// 2. NPCPrefabMaker.cs에 생성 메서드 추가
private void CreateQuestNPCData()
{
    NPCData npcData = ScriptableObject.CreateInstance<NPCData>();
    npcData.npcName = "퀘스트 NPC";
    npcData.npcType = NPCType.Quest;
    npcData.episodeIDs.Add("EP_QUEST_001");
    // ... 저장 로직
}
```

### **NPCController 상속**

커스텀 NPC 동작 구현:

```csharp
using Gameplay.NPC;

public class QuestNPC : NPCController
{
    protected override void OnDialogueStarted()
    {
        base.OnDialogueStarted();

        // 퀘스트 시작 로직
        Debug.Log("퀘스트 NPC와 대화 시작!");
    }

    protected override void OnDialogueEnded()
    {
        base.OnDialogueEnded();

        // 퀘스트 완료 체크
        CheckQuestCompletion();
    }
}
```

---

## 🐛 문제 해결

### **"NPCData를 먼저 생성해주세요" 에러**

→ NPCData ScriptableObject를 먼저 생성해야 Prefab 생성 가능
→ GASPT → NPC → Create StoryNPC Data 실행

### **NPC와 상호작용 안 됨**

→ Player에 "Player" 태그 설정 확인
→ BoxCollider2D의 Is Trigger 체크 확인
→ NPCData가 프리팹에 연결되었는지 확인

### **대화창이 안 열림**

→ DialoguePanel Prefab이 생성되었는지 확인
→ UIManager에 DialoguePanel이 등록되었는지 확인
→ PanelType.Dialog로 등록되어 있는지 확인

### **스프라이트가 보이지 않음**

→ SpriteRenderer의 Sorting Layer 확인
→ Sorting Order가 배경보다 높은지 확인 (기본값: 10)
→ Camera의 Culling Mask 확인

---

## 📚 관련 문서

- **대화 시스템 가이드**: `docs/guides/DIALOGUE_SYSTEM_GUIDE.md`
- **Scene Setup Tool 가이드**: NPCPrefabMaker와 함께 사용
- **NPC 스크립트 레퍼런스**: `Assets/_Project/Scripts/Gameplay/NPC/`

---

## 📖 사용 예제

### **예제 1: 마을사람 NPC 배치**

```
1. GASPT → NPC → Create All NPCs 실행
2. Lobby 씬 열기
3. StoryNPC_Villager.prefab을 씬에 배치
4. 위치 조정: (-5, 1, 0)
5. 플레이 → NPC에 접근 → E키 → 대화 시작
```

### **예제 2: 상인 NPC 배치**

```
1. GASPT → NPC → Create All NPCs 실행
2. Lobby 씬 열기
3. ShopNPC_Merchant.prefab을 씬에 배치
4. 위치 조정: (5, 1, 0)
5. 플레이 → NPC에 접근 → E키 → 상점 열림
```

### **예제 3: 여러 스토리 NPC 배치**

```
1. StoryNPC_Villager.prefab 복제 (Ctrl+D)
2. 이름 변경: StoryNPC_Guard
3. NPCData 새로 생성:
   - Create → GASPT → NPC → NPC Data
   - 이름: StoryNPC_GuardData
   - Episode IDs: EP_GUARD_001
4. 프리팹에 새 NPCData 연결
5. 다른 위치에 배치
```

---

*작성일: 2025-10-15*
*작성자: GASPT 프로젝트팀*
