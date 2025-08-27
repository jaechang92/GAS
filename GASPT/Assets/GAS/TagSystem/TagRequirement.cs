// 파일 위치: Assets/Scripts/GAS/Learning/Phase2/TagRequirement.cs
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GAS.Learning.Phase2
{
    /// <summary>
    /// Step 3: TagRequirement - 태그 기반 조건 시스템
    /// </summary>
    public class TagRequirement : MonoBehaviour
    {
        // TagRequirement 구현
        [Serializable]
        public class SimpleTagRequirement
        {
            public enum RequirementType
            {
                RequireAll,    // 모든 태그 필요 (AND)
                RequireAny,    // 하나라도 있으면 됨 (OR)
                RequireNone    // 모두 없어야 함 (NOT)
            }

            [SerializeField] private string name;
            [SerializeField] private RequirementType requirementType = RequirementType.RequireAll;

            [SerializeField] private List<string> requiredTagNames = new List<string>();
            [SerializeField] private List<string> blockingTagNames = new List<string>();
            [SerializeField] private List<string> ignoredTagNames = new List<string>();

            // 실제 태그 참조 (런타임)
            private List<GameplayTag.SimpleGameplayTag> requiredTags = new List<GameplayTag.SimpleGameplayTag>();
            private List<GameplayTag.SimpleGameplayTag> blockingTags = new List<GameplayTag.SimpleGameplayTag>();
            private List<GameplayTag.SimpleGameplayTag> ignoredTags = new List<GameplayTag.SimpleGameplayTag>();

            public string Name => name;

            public SimpleTagRequirement(string name, RequirementType type = RequirementType.RequireAll)
            {
                this.name = name;
                this.requirementType = type;
            }

            // 빌더 패턴
            public SimpleTagRequirement WithRequiredTags(params string[] tagNames)
            {
                requiredTagNames.AddRange(tagNames);
                foreach (var tagName in tagNames)
                {
                    requiredTags.Add(GameplayTag.SimpleGameplayTag.RegisterTag(tagName));
                }
                return this;
            }

            public SimpleTagRequirement WithBlockingTags(params string[] tagNames)
            {
                blockingTagNames.AddRange(tagNames);
                foreach (var tagName in tagNames)
                {
                    blockingTags.Add(GameplayTag.SimpleGameplayTag.RegisterTag(tagName));
                }
                return this;
            }

            public SimpleTagRequirement WithIgnoredTags(params string[] tagNames)
            {
                ignoredTagNames.AddRange(tagNames);
                foreach (var tagName in tagNames)
                {
                    ignoredTags.Add(GameplayTag.SimpleGameplayTag.RegisterTag(tagName));
                }
                return this;
            }

            public SimpleTagRequirement WithRequirementType(RequirementType type)
            {
                this.requirementType = type;
                return this;
            }

            // 조건 만족 체크
            public bool IsSatisfied(TagContainer.SimpleTagContainer container)
            {
                if (container == null) return false;

                // 1. Blocking 태그 체크 (하나라도 있으면 실패)
                foreach (var blockingTag in blockingTags)
                {
                    if (container.HasTag(blockingTag))
                    {
                        Debug.Log($"[Requirement] {name} 실패: 차단 태그 {blockingTag} 발견");
                        return false;
                    }
                }

                // 2. Required 태그 체크
                if (requiredTags.Count > 0)
                {
                    switch (requirementType)
                    {
                        case RequirementType.RequireAll:
                            foreach (var tag in requiredTags)
                            {
                                if (!container.HasTag(tag))
                                {
                                    Debug.Log($"[Requirement] {name} 실패: 필수 태그 {tag} 없음");
                                    return false;
                                }
                            }
                            break;

                        case RequirementType.RequireAny:
                            bool hasAny = false;
                            foreach (var tag in requiredTags)
                            {
                                if (container.HasTag(tag))
                                {
                                    hasAny = true;
                                    break;
                                }
                            }
                            if (!hasAny)
                            {
                                Debug.Log($"[Requirement] {name} 실패: 필수 태그 중 아무것도 없음");
                                return false;
                            }
                            break;

                        case RequirementType.RequireNone:
                            foreach (var tag in requiredTags)
                            {
                                if (container.HasTag(tag))
                                {
                                    Debug.Log($"[Requirement] {name} 실패: 금지된 태그 {tag} 발견");
                                    return false;
                                }
                            }
                            break;
                    }
                }

                Debug.Log($"[Requirement] {name} 성공!");
                return true;
            }

            // 부족한 태그 가져오기
            public List<GameplayTag.SimpleGameplayTag> GetMissingTags(TagContainer.SimpleTagContainer container)
            {
                var missing = new List<GameplayTag.SimpleGameplayTag>();

                if (requirementType == RequirementType.RequireAll)
                {
                    foreach (var tag in requiredTags)
                    {
                        if (!container.HasTag(tag))
                        {
                            missing.Add(tag);
                        }
                    }
                }

                return missing;
            }

            // 디버그 정보
            public string GetDebugInfo()
            {
                var info = $"[{name}] Type: {requirementType}\n";

                if (requiredTags.Count > 0)
                    info += $"  Required: {string.Join(", ", requiredTags.Select(t => t.ToString()))}\n";

                if (blockingTags.Count > 0)
                    info += $"  Blocking: {string.Join(", ", blockingTags.Select(t => t.ToString()))}\n";

                if (ignoredTags.Count > 0)
                    info += $"  Ignored: {string.Join(", ", ignoredTags.Select(t => t.ToString()))}\n";

                return info;
            }
        }

        // 복합 조건 (여러 Requirement 조합)
        [Serializable]
        public class CompositeTagRequirement
        {
            public enum CompositeType
            {
                AND,  // 모든 조건 만족
                OR    // 하나라도 만족
            }

            [SerializeField] private string name;
            [SerializeField] private CompositeType type = CompositeType.AND;
            [SerializeField] private List<SimpleTagRequirement> requirements = new List<SimpleTagRequirement>();

            public CompositeTagRequirement(string name, CompositeType type = CompositeType.AND)
            {
                this.name = name;
                this.type = type;
            }

            public void AddRequirement(SimpleTagRequirement requirement)
            {
                requirements.Add(requirement);
            }

            public bool IsSatisfied(TagContainer.SimpleTagContainer container)
            {
                if (requirements.Count == 0) return true;

                switch (type)
                {
                    case CompositeType.AND:
                        foreach (var req in requirements)
                        {
                            if (!req.IsSatisfied(container))
                            {
                                Debug.Log($"[Composite] {name} (AND) 실패: {req.Name} 조건 불만족");
                                return false;
                            }
                        }
                        Debug.Log($"[Composite] {name} (AND) 성공!");
                        return true;

                    case CompositeType.OR:
                        foreach (var req in requirements)
                        {
                            if (req.IsSatisfied(container))
                            {
                                Debug.Log($"[Composite] {name} (OR) 성공: {req.Name} 조건 만족");
                                return true;
                            }
                        }
                        Debug.Log($"[Composite] {name} (OR) 실패: 모든 조건 불만족");
                        return false;

                    default:
                        return false;
                }
            }
        }

        // 스킬/능력 정의
        [Serializable]
        public class AbilityDefinition
        {
            public string name;
            public SimpleTagRequirement activationRequirement;
            public List<string> grantedTags = new List<string>();
            public List<string> removedTags = new List<string>();

            public AbilityDefinition(string name)
            {
                this.name = name;
                this.activationRequirement = new SimpleTagRequirement($"{name}_Requirement");
            }

            public bool CanActivate(TagContainer.SimpleTagContainer container)
            {
                return activationRequirement.IsSatisfied(container);
            }

            public void Execute(TagContainer.SimpleTagContainer container)
            {
                if (!CanActivate(container))
                {
                    Debug.LogWarning($"[Ability] {name} 활성화 조건 불만족!");
                    return;
                }

                Debug.Log($"[Ability] {name} 실행!");

                // 태그 제거
                foreach (var tagName in removedTags)
                {
                    var tag = GameplayTag.SimpleGameplayTag.RegisterTag(tagName);
                    container.RemoveTag(tag);
                }

                // 태그 부여
                foreach (var tagName in grantedTags)
                {
                    var tag = GameplayTag.SimpleGameplayTag.RegisterTag(tagName);
                    container.AddTag(tag);
                }
            }
        }

        [Header("캐릭터 태그")]
        [SerializeField] private TagContainer.SimpleTagContainer characterTags;

        [Header("능력 정의")]
        [SerializeField] private List<AbilityDefinition> abilities = new List<AbilityDefinition>();

        [Header("테스트 설정")]
        [SerializeField] private bool verboseLogging = true;

        private void Start()
        {
            InitializeSystem();
            Debug.Log("=== Phase 2 - Step 3: TagRequirement 테스트 ===");
            RunRequirementTests();
        }

        private void InitializeSystem()
        {
            characterTags = new TagContainer.SimpleTagContainer();
            SetupAbilities();
        }

        private void SetupAbilities()
        {
            // 기본 공격
            var basicAttack = new AbilityDefinition("기본 공격");
            basicAttack.activationRequirement
                .WithRequiredTags("State.Combat")
                .WithBlockingTags("State.Dead", "Status.Stunned")
                .WithRequirementType(SimpleTagRequirement.RequirementType.RequireAll);
            basicAttack.grantedTags.Add("State.Combat.Attacking");
            basicAttack.grantedTags.Add("Cooldown.Attack");
            abilities.Add(basicAttack);

            // 방어
            var defend = new AbilityDefinition("방어");
            defend.activationRequirement
                .WithRequiredTags("State.Combat")
                .WithBlockingTags("State.Combat.Attacking", "Status.Stunned")
                .WithRequirementType(SimpleTagRequirement.RequirementType.RequireAll);
            defend.grantedTags.Add("State.Combat.Defending");
            defend.grantedTags.Add("Status.Buff.DefenseUp");
            abilities.Add(defend);

            // 궁극기
            var ultimate = new AbilityDefinition("궁극기");
            ultimate.activationRequirement
                .WithRequiredTags("State.Combat", "Resource.UltimateReady")
                .WithBlockingTags("Cooldown.Ultimate", "Status.Silenced")
                .WithRequirementType(SimpleTagRequirement.RequirementType.RequireAll);
            ultimate.grantedTags.Add("State.Ultimate");
            ultimate.grantedTags.Add("Status.Buff.Invincible");
            ultimate.grantedTags.Add("Cooldown.Ultimate");
            ultimate.removedTags.Add("Resource.UltimateReady");
            abilities.Add(ultimate);

            // 도주
            var flee = new AbilityDefinition("도주");
            flee.activationRequirement
                .WithRequiredTags("State.Combat")
                .WithBlockingTags("Status.Rooted", "State.Combat.Attacking")
                .WithRequirementType(SimpleTagRequirement.RequirementType.RequireAll);
            flee.removedTags.Add("State.Combat");
            flee.grantedTags.Add("State.Fleeing");
            abilities.Add(flee);
        }

        private void RunRequirementTests()
        {
            TestBasicRequirements();
            TestComplexRequirements();
            TestCompositeRequirements();
            TestAbilitySystem();
        }

        // 테스트 1: 기본 요구사항
        private void TestBasicRequirements()
        {
            Debug.Log("\n========== 테스트 1: 기본 요구사항 ==========");

            characterTags.Clear();

            // RequireAll 테스트
            var req1 = new SimpleTagRequirement("전투 준비 체크", SimpleTagRequirement.RequirementType.RequireAll)
                .WithRequiredTags("State.Combat", "Resource.Mana");

            Debug.Log(req1.GetDebugInfo());

            characterTags.AddTag(GameplayTag.SimpleGameplayTag.RegisterTag("State.Combat"));
            Debug.Log($"State.Combat만 있을 때: {req1.IsSatisfied(characterTags)}");

            characterTags.AddTag(GameplayTag.SimpleGameplayTag.RegisterTag("Resource.Mana"));
            Debug.Log($"모두 있을 때: {req1.IsSatisfied(characterTags)}");

            // RequireAny 테스트
            var req2 = new SimpleTagRequirement("버프 체크", SimpleTagRequirement.RequirementType.RequireAny)
                .WithRequiredTags("Status.Buff.Speed", "Status.Buff.Strength");

            Debug.Log($"\n버프 없을 때: {req2.IsSatisfied(characterTags)}");

            characterTags.AddTag(GameplayTag.SimpleGameplayTag.RegisterTag("Status.Buff.Speed"));
            Debug.Log($"Speed 버프만: {req2.IsSatisfied(characterTags)}");
        }

        // 테스트 2: 복잡한 요구사항
        private void TestComplexRequirements()
        {
            Debug.Log("\n========== 테스트 2: 복잡한 요구사항 ==========");

            characterTags.Clear();

            // 필수 + 차단 태그
            var complexReq = new SimpleTagRequirement("스킬 사용 조건", SimpleTagRequirement.RequirementType.RequireAll)
                .WithRequiredTags("State.Combat", "Resource.Mana")
                .WithBlockingTags("Status.Silenced", "Cooldown.Skill");

            Debug.Log(complexReq.GetDebugInfo());

            // 조건 충족
            characterTags.AddTag(GameplayTag.SimpleGameplayTag.RegisterTag("State.Combat"));
            characterTags.AddTag(GameplayTag.SimpleGameplayTag.RegisterTag("Resource.Mana"));
            Debug.Log($"조건 충족: {complexReq.IsSatisfied(characterTags)}");

            // 차단 태그 추가
            characterTags.AddTag(GameplayTag.SimpleGameplayTag.RegisterTag("Status.Silenced"));
            Debug.Log($"침묵 상태: {complexReq.IsSatisfied(characterTags)}");

            // 부족한 태그 확인
            characterTags.Clear();
            var missing = complexReq.GetMissingTags(characterTags);
            Debug.Log($"부족한 태그: {string.Join(", ", missing.Select(t => t.ToString()))}");
        }

        // 테스트 3: 복합 조건
        private void TestCompositeRequirements()
        {
            Debug.Log("\n========== 테스트 3: 복합 조건 ==========");

            characterTags.Clear();

            // OR 복합 조건
            var composite = new CompositeTagRequirement("특수 공격 조건", CompositeTagRequirement.CompositeType.OR);

            // 조건 1: 버서크 모드
            var berserkReq = new SimpleTagRequirement("버서크", SimpleTagRequirement.RequirementType.RequireAll)
                .WithRequiredTags("Status.Berserk");

            // 조건 2: 풀 콤보
            var comboReq = new SimpleTagRequirement("콤보", SimpleTagRequirement.RequirementType.RequireAll)
                .WithRequiredTags("Combo.Full");

            // 조건 3: 궁극기 준비
            var ultReq = new SimpleTagRequirement("궁극기", SimpleTagRequirement.RequirementType.RequireAll)
                .WithRequiredTags("Resource.UltimateReady");

            composite.AddRequirement(berserkReq);
            composite.AddRequirement(comboReq);
            composite.AddRequirement(ultReq);

            Debug.Log("아무 조건도 없을 때: " + composite.IsSatisfied(characterTags));

            characterTags.AddTag(GameplayTag.SimpleGameplayTag.RegisterTag("Combo.Full"));
            Debug.Log("콤보만 있을 때: " + composite.IsSatisfied(characterTags));
        }

        // 테스트 4: 능력 시스템
        private void TestAbilitySystem()
        {
            Debug.Log("\n========== 테스트 4: 능력 시스템 ==========");

            characterTags.Clear();

            // 초기 상태 설정
            characterTags.AddTag(GameplayTag.SimpleGameplayTag.RegisterTag("State.Combat"));
            characterTags.AddTag(GameplayTag.SimpleGameplayTag.RegisterTag("Resource.UltimateReady"));

            Debug.Log("초기 태그:");
            Debug.Log(characterTags.GetDebugInfo());

            // 각 능력 체크
            Debug.Log("\n=== 능력 사용 가능 체크 ===");
            foreach (var ability in abilities)
            {
                bool canUse = ability.CanActivate(characterTags);
                Debug.Log($"{ability.name}: {(canUse ? "가능" : "불가능")}");

                if (ability.name == "기본 공격" && canUse)
                {
                    ability.Execute(characterTags);
                    Debug.Log("\n공격 후 태그:");
                    Debug.Log(characterTags.GetDebugInfo());
                }
            }

            // 궁극기 테스트
            Debug.Log("\n=== 궁극기 테스트 ===");
            characterTags.Clear();
            characterTags.AddTag(GameplayTag.SimpleGameplayTag.RegisterTag("State.Combat"));
            characterTags.AddTag(GameplayTag.SimpleGameplayTag.RegisterTag("Resource.UltimateReady"));

            var ultimateAbility = abilities.Find(a => a.name == "궁극기");
            if (ultimateAbility != null && ultimateAbility.CanActivate(characterTags))
            {
                ultimateAbility.Execute(characterTags);
                Debug.Log("\n궁극기 사용 후 태그:");
                Debug.Log(characterTags.GetDebugInfo());
            }
        }

        private void Update()
        {
            HandleInput();
        }

        private void HandleInput()
        {
            // 능력 사용
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                TryUseAbility(0); // 기본 공격
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                TryUseAbility(1); // 방어
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                TryUseAbility(2); // 궁극기
            }
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                TryUseAbility(3); // 도주
            }

            // 태그 조작
            if (Input.GetKeyDown(KeyCode.Q))
            {
                characterTags.AddTag(GameplayTag.SimpleGameplayTag.RegisterTag("State.Combat"));
                Debug.Log("전투 상태 진입");
            }
            if (Input.GetKeyDown(KeyCode.W))
            {
                characterTags.AddTag(GameplayTag.SimpleGameplayTag.RegisterTag("Resource.UltimateReady"));
                Debug.Log("궁극기 준비 완료");
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                characterTags.AddTag(GameplayTag.SimpleGameplayTag.RegisterTag("Status.Stunned"));
                Debug.Log("기절 상태");
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                characterTags.Clear();
                Debug.Log("모든 태그 제거");
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                PrintStatus();
            }
        }

        private void TryUseAbility(int index)
        {
            if (index < 0 || index >= abilities.Count) return;

            var ability = abilities[index];
            if (ability.CanActivate(characterTags))
            {
                ability.Execute(characterTags);
            }
            else
            {
                Debug.LogWarning($"{ability.name} 사용 불가!");

                // 부족한 태그 표시
                var missing = ability.activationRequirement.GetMissingTags(characterTags);
                if (missing.Count > 0)
                {
                    Debug.Log($"부족한 태그: {string.Join(", ", missing.Select(t => t.ToString()))}");
                }
            }
        }

        private void PrintStatus()
        {
            Debug.Log("\n=== 현재 상태 ===");
            Debug.Log(characterTags.GetDebugInfo());

            Debug.Log("\n=== 사용 가능한 능력 ===");
            foreach (var ability in abilities)
            {
                if (ability.CanActivate(characterTags))
                {
                    Debug.Log($" {ability.name}");
                }
            }
        }

        private void OnGUI()
        {
            GUI.Box(new Rect(10, 10, 400, 300), "Phase 2 - TagRequirement");

            int y = 40;

            // 현재 태그
            GUI.Label(new Rect(20, y, 380, 20), $"=== 현재 태그 ({characterTags?.Count ?? 0}) ===");
            y += 25;

            if (characterTags != null && characterTags.Count > 0)
            {
                var tags = characterTags.GetAllTags();
                int col = 0;
                foreach (var tag in tags)
                {
                    GUI.Label(new Rect(30 + (col * 180), y, 170, 20), $" {tag}");
                    col++;
                    if (col >= 2)
                    {
                        col = 0;
                        y += 20;
                    }
                }
                if (col > 0) y += 20;
            }
            else
            {
                GUI.Label(new Rect(30, y, 360, 20), "태그 없음");
                y += 20;
            }

            // 능력 상태
            y += 10;
            GUI.Label(new Rect(20, y, 380, 20), "=== 능력 상태 ===");
            y += 25;

            for (int i = 0; i < abilities.Count && i < 4; i++)
            {
                var ability = abilities[i];
                bool canUse = ability.CanActivate(characterTags);
                string status = canUse ? "can Use" : "can't Use";
                GUI.Label(new Rect(30, y, 360, 20), $"{i + 1}. {ability.name} [{status}]");
                y += 20;
            }

            // 조작법
            y += 10;
            GUI.Box(new Rect(10, y, 400, 100), "조작법");
            y += 25;
            GUI.Label(new Rect(20, y, 380, 70),
                "능력: 1-기본공격 2-방어 3-궁극기 4-도주\n" +
                "태그: Q-전투 W-궁극기준비 E-기절 R-초기화\n" +
                "Space: 상태 출력");
        }
    }
}