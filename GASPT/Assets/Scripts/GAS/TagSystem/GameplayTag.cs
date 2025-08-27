// 파일 위치: Assets/Scripts/GAS/Learning/Phase2/GameplayTag.cs
using UnityEngine;
using System;
using System.Collections.Generic;

namespace GAS.Learning.Phase2
{
    /// <summary>
    /// Step 1: GameplayTag 기초 - 계층적 태그 시스템 이해
    /// </summary>
    public class GameplayTag : MonoBehaviour
    {
        // 간단한 GameplayTag 구현
        [Serializable]
        public class SimpleGameplayTag
        {
            [SerializeField] private string fullName;
            private string[] segments;
            private static Dictionary<string, SimpleGameplayTag> registeredTags = new Dictionary<string, SimpleGameplayTag>();

            public string FullName => fullName;
            public string[] Segments => segments;

            // Private 생성자 - RegisterTag를 통해서만 생성
            private SimpleGameplayTag(string name)
            {
                this.fullName = name;
                this.segments = name.Split('.');
            }

            // 태그 등록 (싱글톤 패턴)
            public static SimpleGameplayTag RegisterTag(string tagName)
            {
                if (string.IsNullOrEmpty(tagName))
                {
                    Debug.LogError("태그 이름이 비어있습니다!");
                    return null;
                }

                // 이미 등록된 태그는 재사용
                if (registeredTags.TryGetValue(tagName, out var existingTag))
                {
                    return existingTag;
                }

                // 새 태그 생성 및 등록
                var newTag = new SimpleGameplayTag(tagName);
                registeredTags[tagName] = newTag;
                Debug.Log($"[Tag] 새 태그 등록: {tagName}");
                return newTag;
            }

            // 정확히 일치하는지 체크
            public bool Equals(SimpleGameplayTag other)
            {
                if (other == null) return false;
                return fullName == other.fullName;
            }

            // 계층적 매칭 (자식이 부모와 매치)
            public bool Matches(SimpleGameplayTag parent)
            {
                if (parent == null) return false;

                // 자신이 parent의 자식인지 체크
                if (parent.segments.Length > segments.Length)
                    return false;

                for (int i = 0; i < parent.segments.Length; i++)
                {
                    if (segments[i] != parent.segments[i])
                        return false;
                }

                return true;
            }

            // 패턴 매칭 (와일드카드 지원)
            public bool MatchesPattern(string pattern)
            {
                if (pattern.EndsWith("*"))
                {
                    string prefix = pattern.Substring(0, pattern.Length - 1);
                    return fullName.StartsWith(prefix);
                }
                return fullName == pattern;
            }

            // 부모 태그 가져오기
            public SimpleGameplayTag GetParent()
            {
                if (segments.Length <= 1) return null;

                string parentName = string.Join(".", segments, 0, segments.Length - 1);
                return RegisterTag(parentName);
            }

            // 깊이 가져오기
            public int GetDepth()
            {
                return segments.Length;
            }

            public override string ToString()
            {
                return fullName;
            }

            // 디버그용: 모든 등록된 태그 출력
            public static void PrintAllRegisteredTags()
            {
                Debug.Log($"=== 등록된 태그 ({registeredTags.Count}개) ===");
                foreach (var tag in registeredTags.Values)
                {
                    Debug.Log($"  - {tag.fullName} (깊이: {tag.GetDepth()})");
                }
            }
        }

        [Header("테스트용 태그들")]
        private SimpleGameplayTag stateTag;
        private SimpleGameplayTag idleTag;
        private SimpleGameplayTag combatTag;
        private SimpleGameplayTag attackingTag;
        private SimpleGameplayTag defendingTag;
        private SimpleGameplayTag movementTag;
        private SimpleGameplayTag runningTag;

        [Header("현재 활성 태그")]
        [SerializeField] private List<string> activeTags = new List<string>();

        private void Start()
        {
            Debug.Log("=== Phase 2 - Step 1: GameplayTag 기초 테스트 ===");
            InitializeTags();
            RunBasicTests();
        }

        private void InitializeTags()
        {
            Debug.Log("\n[테스트 1] 태그 등록 및 계층 구조");

            // 계층적 태그 등록
            stateTag = SimpleGameplayTag.RegisterTag("State");
            idleTag = SimpleGameplayTag.RegisterTag("State.Idle");
            combatTag = SimpleGameplayTag.RegisterTag("State.Combat");
            attackingTag = SimpleGameplayTag.RegisterTag("State.Combat.Attacking");
            defendingTag = SimpleGameplayTag.RegisterTag("State.Combat.Defending");
            movementTag = SimpleGameplayTag.RegisterTag("State.Movement");
            runningTag = SimpleGameplayTag.RegisterTag("State.Movement.Running");

            // 능력 태그
            SimpleGameplayTag.RegisterTag("Ability.Skill.Fireball");
            SimpleGameplayTag.RegisterTag("Ability.Skill.Heal");
            SimpleGameplayTag.RegisterTag("Ability.Ultimate.Meteor");

            // 상태 태그
            SimpleGameplayTag.RegisterTag("Status.Buff.Speed");
            SimpleGameplayTag.RegisterTag("Status.Buff.Strength");
            SimpleGameplayTag.RegisterTag("Status.Debuff.Slow");
            SimpleGameplayTag.RegisterTag("Status.Debuff.Poison");

            SimpleGameplayTag.PrintAllRegisteredTags();
        }

        private void RunBasicTests()
        {
            TestExactMatching();
            TestHierarchicalMatching();
            TestPatternMatching();
            TestParentRelationship();
            TestTagReuse();
        }

        // 테스트 1: 정확한 매칭
        private void TestExactMatching()
        {
            Debug.Log("\n[테스트 2] 정확한 매칭 테스트");

            var combat1 = SimpleGameplayTag.RegisterTag("State.Combat");
            var combat2 = SimpleGameplayTag.RegisterTag("State.Combat");

            Debug.Log($"combat1 == combat2: {combat1.Equals(combat2)} (같은 인스턴스: {ReferenceEquals(combat1, combat2)})");
            Debug.Log($"combat != idle: {!combatTag.Equals(idleTag)}");
        }

        // 테스트 2: 계층적 매칭
        private void TestHierarchicalMatching()
        {
            Debug.Log("\n[테스트 3] 계층적 매칭 테스트");

            // 자식이 부모와 매치하는지
            Debug.Log($"State.Combat.Attacking matches State: {attackingTag.Matches(stateTag)}");
            Debug.Log($"State.Combat.Attacking matches State.Combat: {attackingTag.Matches(combatTag)}");
            Debug.Log($"State.Combat matches State.Combat.Attacking: {combatTag.Matches(attackingTag)} (부모는 자식과 매치 안됨)");

            // 다른 브랜치끼리는 매치 안됨
            Debug.Log($"State.Combat matches State.Movement: {combatTag.Matches(movementTag)}");
        }

        // 테스트 3: 패턴 매칭
        private void TestPatternMatching()
        {
            Debug.Log("\n[테스트 4] 패턴 매칭 테스트");

            var fireballTag = SimpleGameplayTag.RegisterTag("Ability.Skill.Fireball");

            Debug.Log($"Fireball matches 'Ability*': {fireballTag.MatchesPattern("Ability*")}");
            Debug.Log($"Fireball matches 'Ability.Skill*': {fireballTag.MatchesPattern("Ability.Skill*")}");
            Debug.Log($"Fireball matches 'Status*': {fireballTag.MatchesPattern("Status*")}");
            Debug.Log($"Fireball matches exact: {fireballTag.MatchesPattern("Ability.Skill.Fireball")}");
        }

        // 테스트 4: 부모 관계
        private void TestParentRelationship()
        {
            Debug.Log("\n[테스트 5] 부모 태그 관계 테스트");

            var parent = attackingTag.GetParent();
            Debug.Log($"State.Combat.Attacking의 부모: {parent}");

            var grandParent = parent?.GetParent();
            Debug.Log($"State.Combat의 부모: {grandParent}");

            var root = grandParent?.GetParent();
            Debug.Log($"State의 부모: {root} (null = 루트)");

            Debug.Log($"\n깊이 테스트:");
            Debug.Log($"State 깊이: {stateTag.GetDepth()}");
            Debug.Log($"State.Combat 깊이: {combatTag.GetDepth()}");
            Debug.Log($"State.Combat.Attacking 깊이: {attackingTag.GetDepth()}");
        }

        // 테스트 5: 태그 재사용
        private void TestTagReuse()
        {
            Debug.Log("\n[테스트 6] 태그 재사용 테스트");

            int beforeCount = GetRegisteredTagCount();

            // 같은 태그를 여러 번 등록 시도
            var tag1 = SimpleGameplayTag.RegisterTag("Test.Reuse");
            var tag2 = SimpleGameplayTag.RegisterTag("Test.Reuse");
            var tag3 = SimpleGameplayTag.RegisterTag("Test.Reuse");

            int afterCount = GetRegisteredTagCount();

            Debug.Log($"등록 전 태그 수: {beforeCount}");
            Debug.Log($"등록 후 태그 수: {afterCount}");
            Debug.Log($"증가한 태그 수: {afterCount - beforeCount} (1개만 증가해야 함)");
            Debug.Log($"모두 같은 인스턴스?: {ReferenceEquals(tag1, tag2) && ReferenceEquals(tag2, tag3)}");
        }

        private int GetRegisteredTagCount()
        {
            // Reflection을 사용하여 private static 필드 접근
            var field = typeof(SimpleGameplayTag).GetField("registeredTags",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            var dict = field.GetValue(null) as Dictionary<string, SimpleGameplayTag>;
            return dict?.Count ?? 0;
        }

        // 실제 게임 시뮬레이션
        private void SimulateGameplay()
        {
            Debug.Log("\n=== 게임플레이 시뮬레이션 ===");

            // 현재 상태: Idle
            activeTags.Clear();
            activeTags.Add(idleTag.FullName);
            Debug.Log($"현재 상태: {string.Join(", ", activeTags)}");

            // 전투 진입
            activeTags.Clear();
            activeTags.Add(combatTag.FullName);
            Debug.Log($"전투 진입: {string.Join(", ", activeTags)}");

            // 공격 중
            activeTags.Add(attackingTag.FullName);
            Debug.Log($"공격 시작: {string.Join(", ", activeTags)}");

            // 스킬 사용 가능 체크
            bool canUseSkill = CheckTagCondition("State.Combat");
            Debug.Log($"스킬 사용 가능? (State.Combat 필요): {canUseSkill}");

            // 이동 가능 체크
            bool canMove = !CheckTagCondition("State.Combat.Attacking");
            Debug.Log($"이동 가능? (!State.Combat.Attacking): {canMove}");
        }

        private bool CheckTagCondition(string tagName)
        {
            var requiredTag = SimpleGameplayTag.RegisterTag(tagName);

            foreach (var activeTagName in activeTags)
            {
                var activeTag = SimpleGameplayTag.RegisterTag(activeTagName);
                if (activeTag.Matches(requiredTag))
                {
                    return true;
                }
            }
            return false;
        }

        private void Update()
        {
            // 키보드 입력으로 상태 전환 테스트
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                SetState("State.Idle");
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                SetState("State.Combat");
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                SetState("State.Combat.Attacking");
            }
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                SetState("State.Movement.Running");
            }
            if (Input.GetKeyDown(KeyCode.Space))
            {
                SimulateGameplay();
            }
        }

        private void SetState(string tagName)
        {
            activeTags.Clear();
            activeTags.Add(tagName);
            Debug.Log($"상태 변경: {tagName}");
        }

        private void OnGUI()
        {
            GUI.Box(new Rect(10, 10, 300, 180), "Phase 2 - GameplayTag 기초");

            int y = 40;
            GUI.Label(new Rect(20, y, 280, 20), "=== 태그 계층 구조 ===");
            y += 25;
            GUI.Label(new Rect(20, y, 280, 20), "State");
            y += 20;
            GUI.Label(new Rect(30, y, 270, 20), "├─ State.Idle");
            y += 20;
            GUI.Label(new Rect(30, y, 270, 20), "├─ State.Combat");
            y += 20;
            GUI.Label(new Rect(40, y, 260, 20), "│  ├─ .Attacking");
            y += 20;
            GUI.Label(new Rect(40, y, 260, 20), "│  └─ .Defending");
            y += 20;
            GUI.Label(new Rect(30, y, 270, 20), "└─ State.Movement");
            y += 30;

            GUI.Label(new Rect(20, y, 280, 20), "1-4: 상태 변경  Space: 시뮬레이션");

            // 현재 활성 태그
            y = 200;
            GUI.Box(new Rect(10, y, 300, 60), "현재 활성 태그");
            y += 25;
            string activeTagsStr = activeTags.Count > 0 ? string.Join(", ", activeTags) : "없음";
            GUI.Label(new Rect(20, y, 280, 30), activeTagsStr);
        }
    }
}