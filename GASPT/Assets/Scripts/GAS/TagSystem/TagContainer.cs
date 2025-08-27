// 파일 위치: Assets/Scripts/GAS/Learning/Phase2/TagContainer.cs
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GAS.Learning.Phase2
{
    /// <summary>
    /// Step 2: TagContainer - 여러 태그를 관리하는 컨테이너
    /// </summary>
    public class TagContainer : MonoBehaviour
    {
        // TagContainer 구현
        [Serializable]
        public class SimpleTagContainer
        {
            // 태그와 스택 카운트를 저장
            private Dictionary<GameplayTag.SimpleGameplayTag, int> tags;

            [SerializeField] private List<string> debugTagList = new List<string>(); // Inspector용

            // 이벤트
            public event Action<GameplayTag.SimpleGameplayTag> OnTagAdded;
            public event Action<GameplayTag.SimpleGameplayTag> OnTagRemoved;
            public event Action OnAnyTagChanged;

            public int Count => tags?.Count ?? 0;

            public SimpleTagContainer()
            {
                tags = new Dictionary<GameplayTag.SimpleGameplayTag, int>();
            }

            // 태그 추가 (스택 지원)
            public bool AddTag(GameplayTag.SimpleGameplayTag tag)
            {
                if (tag == null) return false;

                if (tags.ContainsKey(tag))
                {
                    tags[tag]++;
                    Debug.Log($"[Container] 태그 스택 증가: {tag} (스택: {tags[tag]})");
                }
                else
                {
                    tags[tag] = 1;
                    debugTagList.Add(tag.FullName);
                    OnTagAdded?.Invoke(tag);
                    Debug.Log($"[Container] 태그 추가: {tag}");
                }

                OnAnyTagChanged?.Invoke();
                return true;
            }

            // 태그 제거 (스택 감소)
            public bool RemoveTag(GameplayTag.SimpleGameplayTag tag)
            {
                if (tag == null || !tags.ContainsKey(tag)) return false;

                tags[tag]--;
                Debug.Log($"[Container] 태그 스택 감소: {tag} (남은 스택: {tags[tag]})");

                if (tags[tag] <= 0)
                {
                    tags.Remove(tag);
                    debugTagList.Remove(tag.FullName);
                    OnTagRemoved?.Invoke(tag);
                    Debug.Log($"[Container] 태그 완전 제거: {tag}");
                }

                OnAnyTagChanged?.Invoke();
                return true;
            }

            // 특정 태그 보유 체크
            public bool HasTag(GameplayTag.SimpleGameplayTag tag)
            {
                return tag != null && tags.ContainsKey(tag);
            }

            // 계층적 태그 체크 (부모 태그도 매칭)
            public bool HasTagMatching(GameplayTag.SimpleGameplayTag parentTag)
            {
                if (parentTag == null) return false;

                foreach (var tag in tags.Keys)
                {
                    if (tag.Matches(parentTag))
                    {
                        return true;
                    }
                }
                return false;
            }

            // 패턴 매칭
            public bool HasTagMatchingPattern(string pattern)
            {
                foreach (var tag in tags.Keys)
                {
                    if (tag.MatchesPattern(pattern))
                    {
                        return true;
                    }
                }
                return false;
            }

            // 모든 태그 보유 체크
            public bool HasAllTags(params GameplayTag.SimpleGameplayTag[] requiredTags)
            {
                foreach (var tag in requiredTags)
                {
                    if (!HasTag(tag))
                    {
                        return false;
                    }
                }
                return true;
            }

            // 하나라도 보유 체크
            public bool HasAnyTag(params GameplayTag.SimpleGameplayTag[] checkTags)
            {
                foreach (var tag in checkTags)
                {
                    if (HasTag(tag))
                    {
                        return true;
                    }
                }
                return false;
            }

            // 모두 없음 체크
            public bool HasNoneTags(params GameplayTag.SimpleGameplayTag[] checkTags)
            {
                return !HasAnyTag(checkTags);
            }

            // 태그 스택 수 가져오기
            public int GetTagStack(GameplayTag.SimpleGameplayTag tag)
            {
                if (tag != null && tags.TryGetValue(tag, out int stack))
                {
                    return stack;
                }
                return 0;
            }

            // 모든 태그 가져오기
            public List<GameplayTag.SimpleGameplayTag> GetAllTags()
            {
                return new List<GameplayTag.SimpleGameplayTag>(tags.Keys);
            }

            // 컨테이너 병합
            public void AppendTags(SimpleTagContainer other)
            {
                if (other == null) return;

                foreach (var kvp in other.tags)
                {
                    for (int i = 0; i < kvp.Value; i++)
                    {
                        AddTag(kvp.Key);
                    }
                }
            }

            // 컨테이너 차집합
            public void RemoveTags(SimpleTagContainer other)
            {
                if (other == null) return;

                foreach (var kvp in other.tags)
                {
                    for (int i = 0; i < kvp.Value; i++)
                    {
                        RemoveTag(kvp.Key);
                    }
                }
            }

            // 모든 태그 제거
            public void Clear()
            {
                tags.Clear();
                debugTagList.Clear();
                OnAnyTagChanged?.Invoke();
                Debug.Log("[Container] 모든 태그 제거됨");
            }

            // 디버그 정보
            public string GetDebugInfo()
            {
                if (tags.Count == 0) return "태그 없음";

                var info = $"태그 {tags.Count}개:\n";
                foreach (var kvp in tags)
                {
                    info += $"  - {kvp.Key} (스택: {kvp.Value})\n";
                }
                return info;
            }
        }

        [Header("캐릭터 태그 컨테이너")]
        [SerializeField] private SimpleTagContainer playerTags;
        [SerializeField] private SimpleTagContainer enemyTags;

        [Header("테스트 설정")]
        [SerializeField] private bool showDebugInfo = true;

        // 태그 참조
        private GameplayTag.SimpleGameplayTag stateIdleTag;
        private GameplayTag.SimpleGameplayTag stateCombatTag;
        private GameplayTag.SimpleGameplayTag stateAttackingTag;
        private GameplayTag.SimpleGameplayTag statusBuffSpeedTag;
        private GameplayTag.SimpleGameplayTag statusDebuffSlowTag;
        private GameplayTag.SimpleGameplayTag cooldownSkillTag;

        private void Start()
        {
            InitializeContainers();
            RegisterTags();
            SubscribeEvents();

            Debug.Log("=== Phase 2 - Step 2: TagContainer 테스트 ===");
            RunContainerTests();
        }

        private void InitializeContainers()
        {
            playerTags = new SimpleTagContainer();
            enemyTags = new SimpleTagContainer();
        }

        private void RegisterTags()
        {
            // 상태 태그
            stateIdleTag = GameplayTag.SimpleGameplayTag.RegisterTag("State.Idle");
            stateCombatTag = GameplayTag.SimpleGameplayTag.RegisterTag("State.Combat");
            stateAttackingTag = GameplayTag.SimpleGameplayTag.RegisterTag("State.Combat.Attacking");

            // 버프/디버프 태그
            statusBuffSpeedTag = GameplayTag.SimpleGameplayTag.RegisterTag("Status.Buff.Speed");
            statusDebuffSlowTag = GameplayTag.SimpleGameplayTag.RegisterTag("Status.Debuff.Slow");

            // 쿨다운 태그
            cooldownSkillTag = GameplayTag.SimpleGameplayTag.RegisterTag("Cooldown.Skill.Primary");
        }

        private void SubscribeEvents()
        {
            // 플레이어 태그 이벤트
            playerTags.OnTagAdded += (tag) =>
            {
                if (showDebugInfo)
                    Debug.Log($"[Player] 태그 추가됨: {tag}");
            };

            playerTags.OnTagRemoved += (tag) =>
            {
                if (showDebugInfo)
                    Debug.Log($"[Player] 태그 제거됨: {tag}");
            };

            // 적 태그 이벤트
            enemyTags.OnAnyTagChanged += () =>
            {
                if (showDebugInfo)
                    Debug.Log($"[Enemy] 태그 변경됨. 현재 태그 수: {enemyTags.Count}");
            };
        }

        private void RunContainerTests()
        {
            TestBasicOperations();
            TestStackSystem();
            TestTagChecking();
            TestContainerOperations();
            TestGameplayScenario();
        }

        // 테스트 1: 기본 작업
        private void TestBasicOperations()
        {
            Debug.Log("\n========== 테스트 1: 기본 작업 ==========");

            playerTags.Clear();

            // 태그 추가
            playerTags.AddTag(stateIdleTag);
            Debug.Log($"태그 수: {playerTags.Count}");

            // 태그 체크
            Debug.Log($"Has State.Idle: {playerTags.HasTag(stateIdleTag)}");
            Debug.Log($"Has State.Combat: {playerTags.HasTag(stateCombatTag)}");

            // 태그 제거
            playerTags.RemoveTag(stateIdleTag);
            Debug.Log($"제거 후 태그 수: {playerTags.Count}");
        }

        // 테스트 2: 스택 시스템
        private void TestStackSystem()
        {
            Debug.Log("\n========== 테스트 2: 스택 시스템 ==========");

            playerTags.Clear();

            // 같은 태그 여러 번 추가
            playerTags.AddTag(statusBuffSpeedTag);
            playerTags.AddTag(statusBuffSpeedTag);
            playerTags.AddTag(statusBuffSpeedTag);

            Debug.Log($"Speed 버프 스택: {playerTags.GetTagStack(statusBuffSpeedTag)}");
            Debug.Log($"태그 수: {playerTags.Count} (하나만 카운트)");

            // 스택 감소
            playerTags.RemoveTag(statusBuffSpeedTag);
            Debug.Log($"제거 후 Speed 버프 스택: {playerTags.GetTagStack(statusBuffSpeedTag)}");
            Debug.Log($"여전히 버프 있음?: {playerTags.HasTag(statusBuffSpeedTag)}");

            // 모든 스택 제거
            playerTags.RemoveTag(statusBuffSpeedTag);
            playerTags.RemoveTag(statusBuffSpeedTag);
            Debug.Log($"모두 제거 후 버프 있음?: {playerTags.HasTag(statusBuffSpeedTag)}");
        }

        // 테스트 3: 태그 체크
        private void TestTagChecking()
        {
            Debug.Log("\n========== 테스트 3: 태그 체크 ==========");

            playerTags.Clear();
            playerTags.AddTag(stateCombatTag);
            playerTags.AddTag(stateAttackingTag);
            playerTags.AddTag(statusBuffSpeedTag);

            // 계층적 매칭
            var stateTag = GameplayTag.SimpleGameplayTag.RegisterTag("State");
            Debug.Log($"Has parent 'State': {playerTags.HasTagMatching(stateTag)}");

            // 패턴 매칭
            Debug.Log($"Has pattern 'State*': {playerTags.HasTagMatchingPattern("State*")}");
            Debug.Log($"Has pattern 'Status.Buff*': {playerTags.HasTagMatchingPattern("Status.Buff*")}");

            // 복수 태그 체크
            Debug.Log($"Has ALL (Combat, Attacking): {playerTags.HasAllTags(stateCombatTag, stateAttackingTag)}");
            Debug.Log($"Has ALL (Combat, Idle): {playerTags.HasAllTags(stateCombatTag, stateIdleTag)}");
            Debug.Log($"Has ANY (Idle, Combat): {playerTags.HasAnyTag(stateIdleTag, stateCombatTag)}");
            Debug.Log($"Has NONE (Idle, Slow): {playerTags.HasNoneTags(stateIdleTag, statusDebuffSlowTag)}");
        }

        // 테스트 4: 컨테이너 연산
        private void TestContainerOperations()
        {
            Debug.Log("\n========== 테스트 4: 컨테이너 연산 ==========");

            // 플레이어 설정
            playerTags.Clear();
            playerTags.AddTag(stateCombatTag);
            playerTags.AddTag(statusBuffSpeedTag);

            // 적 설정
            enemyTags.Clear();
            enemyTags.AddTag(stateAttackingTag);
            enemyTags.AddTag(statusDebuffSlowTag);

            Debug.Log("초기 상태:");
            Debug.Log($"Player: {string.Join(", ", playerTags.GetAllTags().Select(t => t.ToString()))}");
            Debug.Log($"Enemy: {string.Join(", ", enemyTags.GetAllTags().Select(t => t.ToString()))}");

            // 임시 컨테이너 생성
            var tempContainer = new SimpleTagContainer();
            tempContainer.AddTag(cooldownSkillTag);
            tempContainer.AddTag(statusDebuffSlowTag);

            // 플레이어에 추가
            playerTags.AppendTags(tempContainer);
            Debug.Log($"\n추가 후 Player: {string.Join(", ", playerTags.GetAllTags().Select(t => t.ToString()))}");

            // 플레이어에서 제거
            playerTags.RemoveTags(tempContainer);
            Debug.Log($"제거 후 Player: {string.Join(", ", playerTags.GetAllTags().Select(t => t.ToString()))}");
        }

        // 테스트 5: 게임플레이 시나리오
        private void TestGameplayScenario()
        {
            Debug.Log("\n========== 테스트 5: 게임플레이 시나리오 ==========");

            playerTags.Clear();

            // 시나리오: 전투 시작
            Debug.Log("\n[시나리오] 전투 시작");
            playerTags.AddTag(stateCombatTag);

            // 공격 시작
            Debug.Log("\n[시나리오] 공격 시작");
            playerTags.AddTag(stateAttackingTag);

            // 스킬 사용 가능 체크
            bool canUseSkill = playerTags.HasTag(stateCombatTag) &&
                              !playerTags.HasTag(cooldownSkillTag);
            Debug.Log($"스킬 사용 가능?: {canUseSkill}");

            // 스킬 사용
            if (canUseSkill)
            {
                Debug.Log("\n[시나리오] 스킬 사용!");
                playerTags.AddTag(cooldownSkillTag);

                // 버프 적용
                playerTags.AddTag(statusBuffSpeedTag);
            }

            // 이동 가능 체크
            bool canMove = !playerTags.HasTag(stateAttackingTag);
            Debug.Log($"이동 가능?: {canMove}");

            // 디버프 적용
            Debug.Log("\n[시나리오] 적으로부터 슬로우 디버프");
            playerTags.AddTag(statusDebuffSlowTag);

            // 속도 버프와 슬로우 디버프 동시 체크
            bool hasSpeedBuff = playerTags.HasTag(statusBuffSpeedTag);
            bool hasSlowDebuff = playerTags.HasTag(statusDebuffSlowTag);
            Debug.Log($"속도 버프: {hasSpeedBuff}, 슬로우 디버프: {hasSlowDebuff}");

            if (hasSpeedBuff && hasSlowDebuff)
            {
                Debug.Log("버프와 디버프가 상쇄됨!");
            }

            Debug.Log($"\n최종 태그 상태:\n{playerTags.GetDebugInfo()}");
        }

        private void Update()
        {
            HandleInput();
        }

        private void HandleInput()
        {
            // 플레이어 태그 조작
            if (Input.GetKeyDown(KeyCode.Q))
            {
                playerTags.AddTag(stateCombatTag);
                Debug.Log("전투 태그 추가");
            }
            if (Input.GetKeyDown(KeyCode.W))
            {
                playerTags.AddTag(statusBuffSpeedTag);
                Debug.Log("속도 버프 추가");
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                playerTags.AddTag(statusDebuffSlowTag);
                Debug.Log("슬로우 디버프 추가");
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                playerTags.Clear();
                Debug.Log("모든 태그 제거");
            }

            // 적 태그 조작
            if (Input.GetKeyDown(KeyCode.A))
            {
                enemyTags.AddTag(stateAttackingTag);
                Debug.Log("[Enemy] 공격 태그 추가");
            }
            if (Input.GetKeyDown(KeyCode.S))
            {
                enemyTags.Clear();
                Debug.Log("[Enemy] 모든 태그 제거");
            }

            // 상태 출력
            if (Input.GetKeyDown(KeyCode.Space))
            {
                PrintStatus();
            }
        }

        private void PrintStatus()
        {
            Debug.Log("\n=== 현재 태그 상태 ===");
            Debug.Log($"[Player]\n{playerTags.GetDebugInfo()}");
            Debug.Log($"[Enemy]\n{enemyTags.GetDebugInfo()}");
        }

        private void OnGUI()
        {
            // 플레이어 태그 표시
            GUI.Box(new Rect(10, 10, 350, 200), "Phase 2 - TagContainer");

            int y = 40;
            GUI.Label(new Rect(20, y, 330, 20), $"=== Player Tags ({playerTags?.Count ?? 0}) ===");
            y += 25;

            if (playerTags != null && playerTags.Count > 0)
            {
                foreach (var tag in playerTags.GetAllTags())
                {
                    int stack = playerTags.GetTagStack(tag);
                    string stackInfo = stack > 1 ? $" x{stack}" : "";
                    GUI.Label(new Rect(30, y, 320, 20), $" {tag}{stackInfo}");
                    y += 20;
                }
            }
            else
            {
                GUI.Label(new Rect(30, y, 320, 20), "태그 없음");
                y += 20;
            }

            // 적 태그 표시
            y += 10;
            GUI.Label(new Rect(20, y, 330, 20), $"=== Enemy Tags ({enemyTags?.Count ?? 0}) ===");
            y += 25;

            if (enemyTags != null && enemyTags.Count > 0)
            {
                foreach (var tag in enemyTags.GetAllTags())
                {
                    GUI.Label(new Rect(30, y, 320, 20), $" {tag}");
                    y += 20;
                }
            }
            else
            {
                GUI.Label(new Rect(30, y, 320, 20), "태그 없음");
                y += 20;
            }

            // 조작 설명
            GUI.Box(new Rect(10, 220, 350, 80), "조작법");
            GUI.Label(new Rect(20, 245, 330, 50),
                "Player: Q-전투 W-버프 E-디버프 R-초기화\n" +
                "Enemy: A-공격 S-초기화\n" +
                "Space: 상태 출력");
        }
    }
}