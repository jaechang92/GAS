using UnityEngine;

using Gameplay.Common;
using System.Threading;

namespace Skull.Tests.Mocks
{
    /// <summary>
    /// 테스트용 모킹 스컬 컨트롤러
    /// </summary>
    public class MockSkullController : ISkullController
    {
        private SkullType skullType;
        private SkullData skullData;
        private bool isActive;
        private SkullStatus status;

        // 테스트 검증용 호출 기록
        public int OnEquipCallCount { get; private set; }
        public int OnUnequipCallCount { get; private set; }
        public int OnActivateCallCount { get; private set; }
        public int OnDeactivateCallCount { get; private set; }
        public int PrimaryAttackCallCount { get; private set; }
        public int SecondaryAttackCallCount { get; private set; }
        public int UltimateCallCount { get; private set; }
        public int SkullThrowCallCount { get; private set; }
        public int OnUpdateCallCount { get; private set; }
        public int OnFixedUpdateCallCount { get; private set; }

        // 마지막 호출 파라미터
        public Vector2 LastThrowDirection { get; private set; }
        public CancellationToken LastCancellationToken { get; private set; }

        // 응답 설정
        public float EquipDelay { get; set; } = 0f;
        public float UnequipDelay { get; set; } = 0f;
        public float AttackDelay { get; set; } = 0f;
        public bool ShouldThrowException { get; set; } = false;
        public string ExceptionMessage { get; set; } = "Mock Exception";

        public MockSkullController(SkullType type)
        {
            skullType = type;
            isActive = false;
            status = SkullStatus.Ready;

            // 기본 스컬 데이터 생성
            CreateMockSkullData();
        }

        #region ISkullController 구현

        public SkullType SkullType => skullType;
        public SkullData SkullData => skullData;
        public bool IsActive => isActive;

        public async Awaitable OnEquip(CancellationToken cancellationToken = default)
        {
            OnEquipCallCount++;
            LastCancellationToken = cancellationToken;

            if (ShouldThrowException)
                throw new System.Exception(ExceptionMessage);

            if (EquipDelay > 0f)
                await Awaitable.WaitForSecondsAsync(EquipDelay, cancellationToken);
        }

        public async Awaitable OnUnequip(CancellationToken cancellationToken = default)
        {
            OnUnequipCallCount++;
            LastCancellationToken = cancellationToken;

            if (ShouldThrowException)
                throw new System.Exception(ExceptionMessage);

            if (UnequipDelay > 0f)
                await Awaitable.WaitForSecondsAsync(UnequipDelay, cancellationToken);
        }

        public void OnActivate()
        {
            OnActivateCallCount++;
            isActive = true;

            if (ShouldThrowException)
                throw new System.Exception(ExceptionMessage);
        }

        public void OnDeactivate()
        {
            OnDeactivateCallCount++;
            isActive = false;

            if (ShouldThrowException)
                throw new System.Exception(ExceptionMessage);
        }

        public async Awaitable PerformPrimaryAttack(CancellationToken cancellationToken = default)
        {
            PrimaryAttackCallCount++;
            LastCancellationToken = cancellationToken;

            if (ShouldThrowException)
                throw new System.Exception(ExceptionMessage);

            if (AttackDelay > 0f)
                await Awaitable.WaitForSecondsAsync(AttackDelay, cancellationToken);
        }

        public async Awaitable PerformSecondaryAttack(CancellationToken cancellationToken = default)
        {
            SecondaryAttackCallCount++;
            LastCancellationToken = cancellationToken;

            if (ShouldThrowException)
                throw new System.Exception(ExceptionMessage);

            if (AttackDelay > 0f)
                await Awaitable.WaitForSecondsAsync(AttackDelay, cancellationToken);
        }

        public async Awaitable PerformUltimate(CancellationToken cancellationToken = default)
        {
            UltimateCallCount++;
            LastCancellationToken = cancellationToken;

            if (ShouldThrowException)
                throw new System.Exception(ExceptionMessage);

            if (AttackDelay > 0f)
                await Awaitable.WaitForSecondsAsync(AttackDelay, cancellationToken);
        }

        public async Awaitable PerformSkullThrow(Vector2 direction, CancellationToken cancellationToken = default)
        {
            SkullThrowCallCount++;
            LastThrowDirection = direction;
            LastCancellationToken = cancellationToken;

            if (ShouldThrowException)
                throw new System.Exception(ExceptionMessage);

            if (AttackDelay > 0f)
                await Awaitable.WaitForSecondsAsync(AttackDelay, cancellationToken);
        }

        public void OnUpdate()
        {
            OnUpdateCallCount++;

            if (ShouldThrowException)
                throw new System.Exception(ExceptionMessage);
        }

        public void OnFixedUpdate()
        {
            OnFixedUpdateCallCount++;

            if (ShouldThrowException)
                throw new System.Exception(ExceptionMessage);
        }

        public SkullStatus GetStatus()
        {
            if (ShouldThrowException)
                throw new System.Exception(ExceptionMessage);

            return status;
        }

        #endregion

        #region 테스트 헬퍼 메서드

        /// <summary>
        /// 모든 호출 카운트 초기화
        /// </summary>
        public void ResetCallCounts()
        {
            OnEquipCallCount = 0;
            OnUnequipCallCount = 0;
            OnActivateCallCount = 0;
            OnDeactivateCallCount = 0;
            PrimaryAttackCallCount = 0;
            SecondaryAttackCallCount = 0;
            UltimateCallCount = 0;
            SkullThrowCallCount = 0;
            OnUpdateCallCount = 0;
            OnFixedUpdateCallCount = 0;
        }

        /// <summary>
        /// 스컬 상태 설정
        /// </summary>
        public void SetStatus(SkullStatus newStatus)
        {
            status = newStatus;
        }

        /// <summary>
        /// 활성화 상태 설정
        /// </summary>
        public void SetActive(bool active)
        {
            isActive = active;
        }

        /// <summary>
        /// 스컬 타입 변경
        /// </summary>
        public void SetSkullType(SkullType type)
        {
            skullType = type;
        }

        /// <summary>
        /// 예외 발생 설정
        /// </summary>
        public void SetException(bool shouldThrow, string message = "Mock Exception")
        {
            ShouldThrowException = shouldThrow;
            ExceptionMessage = message;
        }

        /// <summary>
        /// 지연 시간 설정
        /// </summary>
        public void SetDelays(float equipDelay = 0f, float unequipDelay = 0f, float attackDelay = 0f)
        {
            EquipDelay = equipDelay;
            UnequipDelay = unequipDelay;
            AttackDelay = attackDelay;
        }

        /// <summary>
        /// 모킹 스컬 데이터 생성
        /// </summary>
        private void CreateMockSkullData()
        {
            skullData = ScriptableObject.CreateInstance<SkullData>();

            // 리플렉션을 사용하여 private 필드 설정 (테스트용)
            var nameField = typeof(SkullData).GetField("skullName",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            nameField?.SetValue(skullData, $"Mock {skullType} Skull");

            var typeField = typeof(SkullData).GetField("skullType",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            typeField?.SetValue(skullData, skullType);

            var statsField = typeof(SkullData).GetField("baseStats",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            statsField?.SetValue(skullData, CreateMockStats());
        }

        /// <summary>
        /// 모킹 스탯 생성
        /// </summary>
        private SkullStats CreateMockStats()
        {
            switch (skullType)
            {
                case SkullType.Default:
                    return SkullStats.CreateDefault();
                case SkullType.Mage:
                    return SkullStats.CreateMage();
                case SkullType.Warrior:
                    return SkullStats.CreateWarrior();
                default:
                    return SkullStats.CreateDefault();
            }
        }

        #endregion

        #region 검증 헬퍼

        /// <summary>
        /// 특정 메서드가 호출되었는지 검증
        /// </summary>
        public bool WasMethodCalled(string methodName, int expectedCallCount = 1)
        {
            return methodName.ToLower() switch
            {
                "onequip" => OnEquipCallCount == expectedCallCount,
                "onunequip" => OnUnequipCallCount == expectedCallCount,
                "onactivate" => OnActivateCallCount == expectedCallCount,
                "ondeactivate" => OnDeactivateCallCount == expectedCallCount,
                "performprimaryattack" => PrimaryAttackCallCount == expectedCallCount,
                "performsecondaryattack" => SecondaryAttackCallCount == expectedCallCount,
                "performultimate" => UltimateCallCount == expectedCallCount,
                "performskullthrow" => SkullThrowCallCount == expectedCallCount,
                "onupdate" => OnUpdateCallCount == expectedCallCount,
                "onfixedupdate" => OnFixedUpdateCallCount == expectedCallCount,
                _ => false
            };
        }

        /// <summary>
        /// 총 호출 횟수 반환
        /// </summary>
        public int GetTotalCallCount()
        {
            return OnEquipCallCount + OnUnequipCallCount + OnActivateCallCount + OnDeactivateCallCount +
                   PrimaryAttackCallCount + SecondaryAttackCallCount + UltimateCallCount + SkullThrowCallCount +
                   OnUpdateCallCount + OnFixedUpdateCallCount;
        }

        /// <summary>
        /// 디버그 정보 출력
        /// </summary>
        public string GetDebugInfo()
        {
            return $"MockSkull [{skullType}] - Active: {isActive}\n" +
                   $"Calls: Equip={OnEquipCallCount}, Unequip={OnUnequipCallCount}, " +
                   $"Activate={OnActivateCallCount}, Deactivate={OnDeactivateCallCount}\n" +
                   $"Attacks: Primary={PrimaryAttackCallCount}, Secondary={SecondaryAttackCallCount}, " +
                   $"Ultimate={UltimateCallCount}, Throw={SkullThrowCallCount}\n" +
                   $"Updates: Update={OnUpdateCallCount}, FixedUpdate={OnFixedUpdateCallCount}";
        }

        #endregion
    }
}