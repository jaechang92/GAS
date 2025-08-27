// 파일 위치: Assets/Scripts/GAS/Learning/Phase2/Phase2TestController.cs
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GAS.Learning.Phase2
{
    /// <summary>
    /// Phase 2 통합 테스트 컨트롤러
    /// Tag System의 모든 개념을 실제로 적용하는 예제
    /// </summary>
    public class Phase2TestController : MonoBehaviour
    {
        [Header("테스트 모드")]
        [SerializeField] private TestMode currentTestMode = TestMode.Integration;

        private enum TestMode
        {
            BasicTag,       // Step 1 테스트
            Container,      // Step 2 테스트
            Requirement,    // Step 3 테스트
            Integration     // 통합 테스트
        }

        // 통합 캐릭터 시스템
        [Serializable]
        public class CharacterTagSystem
        {
            public string characterName;
            public TagContainer.SimpleTagContainer tags;
            public List<Skill> skills = new List<Skill>();
            public CharacterState currentState = CharacterState.Idle;

            public enum CharacterState
            {
                Idle,
                Combat,
                Moving,
                Casting,
                Dead
            }

            [Serializable]
            public class Skill
            {
                public string name;
                public float cooldown;
                public float remainingCooldown;
                public TagRequirement.SimpleTagRequirement requirement;
                public List<string> effectTags = new List<string>();

                public Skill(string name, float cooldown)
                {
                    this.name = name;
                    this.cooldown = cooldown;
                    this.requirement = new TagRequirement.SimpleTagRequirement(
                        $"{name}_Requirement",
                        TagRequirement.SimpleTagRequirement.RequirementType.RequireAll
                    );
                }

                public bool IsReady()
                {
                    return remainingCooldown <= 0;
                }

                public void Use(TagContainer.SimpleTagContainer container)
                {
                    if (!IsReady() || !requirement.IsSatisfied(container))
                    {
                        Debug.LogWarning($"[Skill] {name} 사용 불가!");
                        return;
                    }

                    remainingCooldown = cooldown;

                    // 효과 태그 적용
                    foreach (var tagName in effectTags)
                    {
                        var tag = GameplayTag.SimpleGameplayTag.RegisterTag(tagName);
                        container.AddTag(tag);
                    }

                    Debug.Log($"[Skill] {name} 사용! (쿨다운: {cooldown}초)");
                }

                public void UpdateCooldown(float deltaTime)
                {
                    if (remainingCooldown > 0)
                    {
                        remainingCooldown -= deltaTime;
                        if (remainingCooldown <= 0)
                        {
                            Debug.Log($"[Skill] {name} 준비 완료!");
                        }
                    }
                }
            }

            public void Initialize(string name)
            {
                characterName = name;
                tags = new TagContainer.SimpleTagContainer();
                InitializeSkills();
                UpdateStateTags();
            }

            private void InitializeSkills()
            {
                // 스킬 1: 파이어볼
                var fireball = new Skill("Fireball", 3f);
                fireball.requirement
                    .WithRequiredTags("State.Combat", "Resource.Mana")
                    .WithBlockingTags("Status.Silenced", "State.Dead");
                fireball.effectTags.Add("Damage.Fire");
                fireball.effectTags.Add("Effect.Burn");
                skills.Add(fireball);

                // 스킬 2: 힐
                var heal = new Skill("Heal", 5f);
                heal.requirement
                    .WithRequiredTags("Resource.Mana")
                    .WithBlockingTags("Status.Silenced", "State.Dead");
                heal.effectTags.Add("Effect.Heal");
                heal.effectTags.Add("Status.Buff.Regeneration");
                skills.Add(heal);

                // 스킬 3: 대쉬
                var dash = new Skill("Dash", 2f);
                dash.requirement
                    .WithRequiredTags("Resource.Stamina")
                    .WithBlockingTags("Status.Rooted", "State.Dead");
                dash.effectTags.Add("State.Movement.Dashing");
                dash.effectTags.Add("Status.Buff.Speed");
                skills.Add(dash);
            }

            public void ChangeState(CharacterState newState)
            {
                if (currentState == newState) return;

                currentState = newState;
                UpdateStateTags();
                Debug.Log($"[{characterName}] 상태 변경: {newState}");
            }

            private void UpdateStateTags()
            {
                // 모든 상태 태그 제거
                RemoveAllStateTags();

                // 새 상태 태그 추가
                switch (currentState)
                {
                    case CharacterState.Idle:
                        tags.AddTag(GameplayTag.SimpleGameplayTag.RegisterTag("State.Idle"));
                        break;
                    case CharacterState.Combat:
                        tags.AddTag(GameplayTag.SimpleGameplayTag.RegisterTag("State.Combat"));
                        break;
                    case CharacterState.Moving:
                        tags.AddTag(GameplayTag.SimpleGameplayTag.RegisterTag("State.Movement"));
                        break;
                    case CharacterState.Casting:
                        tags.AddTag(GameplayTag.SimpleGameplayTag.RegisterTag("State.Casting"));
                        break;
                    case CharacterState.Dead:
                        tags.AddTag(GameplayTag.SimpleGameplayTag.RegisterTag("State.Dead"));
                        break;
                }
            }

            private void RemoveAllStateTags()
            {
                var stateTags = tags.GetAllTags().Where(t => t.MatchesPattern("State*")).ToList();
                foreach (var tag in stateTags)
                {
                    tags.RemoveTag(tag);
                }
            }

            public void UpdateSkillCooldowns(float deltaTime)
            {
                foreach (var skill in skills)
                {
                    skill.UpdateCooldown(deltaTime);
                }
            }
        }

        // 콤보 시스템
        [Serializable]
        public class ComboSystem
        {
            private Queue<string> comboSequence = new Queue<string>();
            private float comboTimer = 0f;
            private float comboWindow = 2f;

            [SerializeField]
            private SerializableDictionary<string, List<string>> comboDictionary = new SerializableDictionary<string, List<string>>();

            public void Initialize()
            {
                // 콤보 정의
                comboDictionary["FireStorm"] = new List<string> { "Fire", "Fire", "Wind" };
                comboDictionary["IceSpear"] = new List<string> { "Ice", "Ice", "Ice" };
                comboDictionary["Lightning"] = new List<string> { "Fire", "Ice", "Wind" };
            }

            public void AddInput(string input)
            {
                if (comboTimer <= 0)
                {
                    comboSequence.Clear();
                }

                comboSequence.Enqueue(input);
                comboTimer = comboWindow;

                // 최대 5개 입력만 유지
                while (comboSequence.Count > 5)
                {
                    comboSequence.Dequeue();
                }

                Debug.Log($"[Combo] 입력: {input} | 시퀀스: {string.Join("-", comboSequence)}");

                CheckCombos();
            }

            private void CheckCombos()
            {
                var currentSequence = comboSequence.ToList();

                foreach (var combo in comboDictionary)
                {
                    if (MatchesCombo(currentSequence, combo.Value))
                    {
                        ExecuteCombo(combo.Key);
                        comboSequence.Clear();
                        break;
                    }
                }
            }

            private bool MatchesCombo(List<string> sequence, List<string> pattern)
            {
                if (sequence.Count < pattern.Count) return false;

                // 뒤에서부터 패턴 매칭
                int startIndex = sequence.Count - pattern.Count;
                for (int i = 0; i < pattern.Count; i++)
                {
                    if (sequence[startIndex + i] != pattern[i])
                        return false;
                }
                return true;
            }

            private void ExecuteCombo(string comboName)
            {
                Debug.Log($"★★★ 콤보 발동: {comboName} ★★★");
                // 콤보 효과 적용
            }

            public void Update(float deltaTime)
            {
                if (comboTimer > 0)
                {
                    comboTimer -= deltaTime;
                    if (comboTimer <= 0)
                    {
                        comboSequence.Clear();
                        Debug.Log("[Combo] 콤보 타이머 만료");
                    }
                }
            }
        }

        [Header("통합 시스템")]
        [SerializeField] private CharacterTagSystem playerSystem;
        [SerializeField] private CharacterTagSystem enemySystem;
        [SerializeField] private ComboSystem comboSystem;

        [Header("리소스")]
        [SerializeField] private float playerMana = 100f;
        [SerializeField] private float playerStamina = 100f;

        private async void Start()
        {
            InitializeSystems();

            await Task.Delay(1000);

            Debug.Log("=== Phase 2 통합 테스트 시작 ===");
            await RunIntegrationTest();
        }

        private void InitializeSystems()
        {
            // 플레이어 초기화
            playerSystem = new CharacterTagSystem();
            playerSystem.Initialize("Player");

            // 적 초기화
            enemySystem = new CharacterTagSystem();
            enemySystem.Initialize("Enemy");

            // 콤보 시스템 초기화
            comboSystem = new ComboSystem();
            comboSystem.Initialize();

            // 초기 리소스 태그
            UpdateResourceTags();
        }

        private void UpdateResourceTags()
        {
            // 마나 태그
            if (playerMana > 20)
            {
                playerSystem.tags.AddTag(GameplayTag.SimpleGameplayTag.RegisterTag("Resource.Mana"));
            }
            else
            {
                var manaTag = GameplayTag.SimpleGameplayTag.RegisterTag("Resource.Mana");
                playerSystem.tags.RemoveTag(manaTag);
            }

            // 스태미나 태그
            if (playerStamina > 10)
            {
                playerSystem.tags.AddTag(GameplayTag.SimpleGameplayTag.RegisterTag("Resource.Stamina"));
            }
            else
            {
                var staminaTag = GameplayTag.SimpleGameplayTag.RegisterTag("Resource.Stamina");
                playerSystem.tags.RemoveTag(staminaTag);
            }
        }

        private async Task RunIntegrationTest()
        {
            switch (currentTestMode)
            {
                case TestMode.BasicTag:
                    await TestBasicTags();
                    break;
                case TestMode.Container:
                    await TestContainerOperations();
                    break;
                case TestMode.Requirement:
                    await TestRequirementSystem();
                    break;
                case TestMode.Integration:
                    await TestFullIntegration();
                    break;
            }
        }

        private async Task TestBasicTags()
        {
            Debug.Log("\n=== 기본 태그 테스트 ===");

            var tag1 = GameplayTag.SimpleGameplayTag.RegisterTag("Test.Parent");
            var tag2 = GameplayTag.SimpleGameplayTag.RegisterTag("Test.Parent.Child");

            Debug.Log($"Child matches Parent: {tag2.Matches(tag1)}");
            Debug.Log($"Parent matches Child: {tag1.Matches(tag2)}");

            await Task.Delay(1000);
        }

        private async Task TestContainerOperations()
        {
            Debug.Log("\n=== 컨테이너 작업 테스트 ===");

            playerSystem.tags.Clear();
            playerSystem.tags.AddTag(GameplayTag.SimpleGameplayTag.RegisterTag("Test.Tag1"));
            playerSystem.tags.AddTag(GameplayTag.SimpleGameplayTag.RegisterTag("Test.Tag2"));

            Debug.Log($"태그 수: {playerSystem.tags.Count}");
            Debug.Log($"Has Test.Tag1: {playerSystem.tags.HasTag(GameplayTag.SimpleGameplayTag.RegisterTag("Test.Tag1"))}");

            await Task.Delay(1000);
        }

        private async Task TestRequirementSystem()
        {
            Debug.Log("\n=== 요구사항 시스템 테스트 ===");

            var requirement = new TagRequirement.SimpleTagRequirement("테스트 요구사항")
                .WithRequiredTags("State.Combat")
                .WithBlockingTags("State.Dead");

            playerSystem.ChangeState(CharacterTagSystem.CharacterState.Combat);
            Debug.Log($"전투 상태에서: {requirement.IsSatisfied(playerSystem.tags)}");

            playerSystem.ChangeState(CharacterTagSystem.CharacterState.Dead);
            Debug.Log($"죽은 상태에서: {requirement.IsSatisfied(playerSystem.tags)}");

            await Task.Delay(1000);
        }

        private async Task TestFullIntegration()
        {
            Debug.Log("\n=== 전체 통합 테스트 ===");

            // 1단계: 전투 준비
            Debug.Log("\n[1단계] 전투 준비");
            playerSystem.ChangeState(CharacterTagSystem.CharacterState.Idle);
            enemySystem.ChangeState(CharacterTagSystem.CharacterState.Idle);

            await Task.Delay(1500);

            // 2단계: 전투 시작
            Debug.Log("\n[2단계] 전투 시작");
            playerSystem.ChangeState(CharacterTagSystem.CharacterState.Combat);
            enemySystem.ChangeState(CharacterTagSystem.CharacterState.Combat);

            // 버프 적용
            playerSystem.tags.AddTag(GameplayTag.SimpleGameplayTag.RegisterTag("Status.Buff.AttackUp"));
            Debug.Log("공격력 버프 적용!");

            await Task.Delay(1500);

            // 3단계: 스킬 사용
            Debug.Log("\n[3단계] 스킬 사용");

            // 스킬 사용 가능 체크
            foreach (var skill in playerSystem.skills)
            {
                bool canUse = skill.IsReady() && skill.requirement.IsSatisfied(playerSystem.tags);
                Debug.Log($"{skill.name}: {(canUse ? "사용 가능" : "사용 불가")}");

                if (canUse && skill.name == "Fireball")
                {
                    skill.Use(playerSystem.tags);
                    ConsumeResource("Mana", 20);
                }
            }

            await Task.Delay(1500);

            // 4단계: 상태 이상
            Debug.Log("\n[4단계] 상태 이상 적용");

            enemySystem.tags.AddTag(GameplayTag.SimpleGameplayTag.RegisterTag("Status.Debuff.Poison"));
            enemySystem.tags.AddTag(GameplayTag.SimpleGameplayTag.RegisterTag("Status.Debuff.Slow"));
            Debug.Log("적에게 독, 슬로우 적용!");

            // 디버프 체크
            bool isPoisoned = enemySystem.tags.HasTag(GameplayTag.SimpleGameplayTag.RegisterTag("Status.Debuff.Poison"));
            bool isSlowed = enemySystem.tags.HasTag(GameplayTag.SimpleGameplayTag.RegisterTag("Status.Debuff.Slow"));
            Debug.Log($"적 상태 - 중독: {isPoisoned}, 슬로우: {isSlowed}");

            await Task.Delay(1500);

            // 5단계: 전투 종료
            Debug.Log("\n[5단계] 전투 종료");
            playerSystem.ChangeState(CharacterTagSystem.CharacterState.Idle);
            enemySystem.ChangeState(CharacterTagSystem.CharacterState.Dead);

            Debug.Log("\n최종 태그 상태:");
            Debug.Log($"[Player] {playerSystem.tags.GetDebugInfo()}");
            Debug.Log($"[Enemy] {enemySystem.tags.GetDebugInfo()}");
        }

        private void ConsumeResource(string resourceType, float amount)
        {
            switch (resourceType)
            {
                case "Mana":
                    playerMana -= amount;
                    Debug.Log($"마나 소비: {amount} (남은 마나: {playerMana})");
                    break;
                case "Stamina":
                    playerStamina -= amount;
                    Debug.Log($"스태미나 소비: {amount} (남은 스태미나: {playerStamina})");
                    break;
            }
            UpdateResourceTags();
        }

        private void Update()
        {
            // 스킬 쿨다운 업데이트
            if (playerSystem != null)
            {
                playerSystem.UpdateSkillCooldowns(Time.deltaTime);
            }
            if (enemySystem != null)
            {
                enemySystem.UpdateSkillCooldowns(Time.deltaTime);
            }

            // 콤보 시스템 업데이트
            if (comboSystem != null)
            {
                comboSystem.Update(Time.deltaTime);
            }

            // 리소스 회복
            playerMana = Mathf.Min(100, playerMana + Time.deltaTime * 5f);
            playerStamina = Mathf.Min(100, playerStamina + Time.deltaTime * 10f);

            // 입력 처리
            HandleInput();
        }

        private void HandleInput()
        {
            // 상태 변경
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                playerSystem.ChangeState(CharacterTagSystem.CharacterState.Idle);
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                playerSystem.ChangeState(CharacterTagSystem.CharacterState.Combat);
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                playerSystem.ChangeState(CharacterTagSystem.CharacterState.Moving);
            }

            // 스킬 사용
            if (Input.GetKeyDown(KeyCode.Q))
            {
                TryUseSkill(0); // Fireball
            }
            if (Input.GetKeyDown(KeyCode.W))
            {
                TryUseSkill(1); // Heal
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                TryUseSkill(2); // Dash
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                ClearPlayerTag(); // 태그 초기화
            }

            // 콤보 입력
            if (Input.GetKeyDown(KeyCode.Z))
            {
                comboSystem.AddInput("Fire");
            }
            if (Input.GetKeyDown(KeyCode.X))
            {
                comboSystem.AddInput("Ice");
            }
            if (Input.GetKeyDown(KeyCode.C))
            {
                comboSystem.AddInput("Wind");
            }

            // 상태 출력
            if (Input.GetKeyDown(KeyCode.Space))
            {
                PrintStatus();
            }
        }

        private void TryUseSkill(int index)
        {
            if (index < 0 || index >= playerSystem.skills.Count) return;

            var skill = playerSystem.skills[index];
            skill.Use(playerSystem.tags);

            // 리소스 소비
            if (skill.name == "Fireball" || skill.name == "Heal")
            {
                ConsumeResource("Mana", 20);
            }
            else if (skill.name == "Dash")
            {
                ConsumeResource("Stamina", 10);
            }
        }

        private void ClearPlayerTag()
        {
            playerSystem.tags.Clear();

        }

        private void PrintStatus()
        {
            Debug.Log("\n=== 현재 상태 ===");
            Debug.Log($"[Player] State: {playerSystem.currentState}");
            Debug.Log(playerSystem.tags.GetDebugInfo());
            Debug.Log($"Mana: {playerMana:F0}/100 | Stamina: {playerStamina:F0}/100");

            Debug.Log($"\n[Enemy] State: {enemySystem.currentState}");
            Debug.Log(enemySystem.tags.GetDebugInfo());
        }

        private void OnGUI()
        {
            // 메인 UI
            GUI.Box(new Rect(10, 10, 450, 350), "Phase 2 - 통합 테스트");

            // 테스트 모드
            GUI.Label(new Rect(20, 40, 200, 20), $"테스트 모드: {currentTestMode}");

            int y = 70;

            // 플레이어 정보
            GUI.Label(new Rect(20, y, 200, 20), $"=== 플레이어 ===");
            y += 25;
            GUI.Label(new Rect(20, y, 200, 20), $"상태: {playerSystem?.currentState}");
            y += 20;
            GUI.Label(new Rect(20, y, 200, 20), $"태그: {playerSystem?.tags.Count ?? 0}개");
            y += 20;

            // 리소스
            GUI.Label(new Rect(20, y, 200, 20), $"마나: {playerMana:F0}/100");
            y += 20;
            GUI.Label(new Rect(20, y, 200, 20), $"스태미나: {playerStamina:F0}/100");
            y += 25;

            // 스킬 쿨다운
            GUI.Label(new Rect(20, y, 200, 20), "=== 스킬 ===");
            y += 25;

            if (playerSystem != null)
            {
                for (int i = 0; i < playerSystem.skills.Count && i < 3; i++)
                {
                    var skill = playerSystem.skills[i];
                    string cooldownText = skill.IsReady() ? "준비됨" : $"{skill.remainingCooldown:F1}초";
                    GUI.Label(new Rect(20, y, 200, 20), $"{skill.name}: {cooldownText}");
                    y += 20;
                }
            }

            // 적 정보
            y = 95;
            GUI.Label(new Rect(240, y, 200, 20), $"=== 적 ===");
            y += 25;
            GUI.Label(new Rect(240, y, 200, 20), $"상태: {enemySystem?.currentState}");
            y += 20;
            GUI.Label(new Rect(240, y, 200, 20), $"태그: {enemySystem?.tags.Count ?? 0}개");

            // 조작법
            y = 250;
            GUI.Box(new Rect(10, y, 450, 100), "조작법");
            y += 25;
            GUI.Label(new Rect(20, y, 430, 70),
                "상태: 1-Idle 2-Combat 3-Moving\n" +
                "스킬: Q-Fireball W-Heal E-Dash\n" +
                "콤보: Z-Fire X-Ice C-Wind (Fire-Fire-Wind = FireStorm)\n" +
                "Space: 상태 출력");
        }
    }
}