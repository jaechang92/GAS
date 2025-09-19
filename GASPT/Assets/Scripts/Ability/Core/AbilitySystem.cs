// 파일 위치: Assets/Scripts/Ability/Core/AbilitySystem.cs
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using AbilitySystem.Platformer;
using System;

namespace AbilitySystem
{
    /// <summary>
    /// 캐릭터의 어빌리티를 관리하는 시스템 - 재설계 버전
    /// </summary>
    public class AbilitySystem : MonoBehaviour
    {
        [Header("어빌리티 설정")]
        [SerializeField] private List<SkulData> initialSkuls = new List<SkulData>();

        // 등록된 어빌리티 목록 (새로운 Ability 클래스 사용)
        private Dictionary<string, Ability> abilities = new Dictionary<string, Ability>();

        // 현재 활성 스컬
        private SkulData currentSkul;

        // 캐릭터 스탯 참조
        [Header("캐릭터 스탯")]
        [SerializeField] private int maxMana = 100;
        [SerializeField] private int currentMana = 100;
        [SerializeField] private int maxStamina = 100;
        [SerializeField] private int currentStamina = 100;

        // 프로퍼티
        public int CurrentMana => currentMana;
        public int CurrentStamina => currentStamina;
        public int MaxMana => maxMana;
        public int MaxStamina => maxStamina;
        public IReadOnlyDictionary<string, Ability> Abilities => abilities;
        public SkulData CurrentSkul => currentSkul;

        // 이벤트
        public event Action<string, float> OnAbilityUsed;
        public event Action<string> OnCooldownStarted;
        public event Action<string> OnCooldownEnded;
        public event Action<int> OnManaChanged;
        public event Action<int> OnStaminaChanged;

        /// <summary>
        /// 시스템 초기화
        /// </summary>
        private void Awake()
        {
            // 초기 스탯 설정
            currentMana = maxMana;
            currentStamina = maxStamina;

            // 초기 스컬 로드
            foreach (var skulData in initialSkuls)
            {
                if (skulData != null)
                {
                    RegisterSkul(skulData);
                    if (currentSkul == null)
                    {
                        currentSkul = skulData;
                    }
                }
            }
        }

        /// <summary>
        /// 매 프레임 업데이트
        /// </summary>
        private void Update()
        {
            // 모든 어빌리티의 쿨다운 업데이트
            foreach (var ability in abilities.Values)
            {
                ability.UpdateCooldown(Time.deltaTime);
            }

            // 스태미나 자동 회복
            RegenerateStamina();
        }

        /// <summary>
        /// 어빌리티 사용 가능 여부 체크
        /// </summary>
        public bool CanUseAbility(string abilityId)
        {
            // 어빌리티가 존재하지 않으면
            if (!abilities.ContainsKey(abilityId))
            {
                Debug.LogWarning($"Ability {abilityId} not found!");
                return false;
            }

            // Ability 클래스의 CanUse 메서드 활용
            return abilities[abilityId].CanUse();
        }

        /// <summary>
        /// 어빌리티 실행 (새로운 구조)
        /// </summary>
        public async Awaitable ExecuteAbility(PlatformerAbilityData abilityData, GameObject caster)
        {
            if (abilityData == null)
            {
                Debug.LogError("AbilityData is null!");
                return;
            }

            string abilityId = abilityData.abilityId;

            // 어빌리티가 없으면 임시로 생성
            if (!abilities.ContainsKey(abilityId))
            {
                RegisterAbility(abilityId, abilityData);
            }

            // 사용 가능 체크
            if (!CanUseAbility(abilityId))
            {
                Debug.Log($"Cannot use ability: {abilityId}");
                return;
            }

            // 리소스 소비
            ConsumeResources(abilityData);

            // Ability 클래스를 통해 실행
            Ability ability = abilities[abilityId];

            // 쿨다운 시작 이벤트 구독
            void OnCooldownStart(Ability ab)
            {
                OnCooldownStarted?.Invoke(ab.Id);
            }

            void OnCooldownEnd(Ability ab)
            {
                OnCooldownEnded?.Invoke(ab.Id);
            }

            ability.OnCooldownStarted += OnCooldownStart;
            ability.OnCooldownCompleted += OnCooldownEnd;

            // 어빌리티 실행
            await ability.ExecuteAsync();

            // 이벤트 구독 해제
            ability.OnCooldownStarted -= OnCooldownStart;
            ability.OnCooldownCompleted -= OnCooldownEnd;

            // 이벤트 발생
            OnAbilityUsed?.Invoke(abilityId, abilityData.cooldownTime);
        }

        /// <summary>
        /// 스컬 등록 (스컬의 모든 어빌리티 등록)
        /// </summary>
        public bool RegisterSkul(SkulData skulData)
        {
            if (skulData == null) return false;

            Debug.Log($"Registering Skul: {skulData.skulName}");

            // 기본 공격
            if (skulData.basicAttack != null)
            {
                RegisterAbility($"{skulData.skulId}_basic", skulData.basicAttack);
            }

            // 스킬 1
            if (skulData.skill1 != null)
            {
                RegisterAbility($"{skulData.skulId}_skill1", skulData.skill1);
            }

            // 스킬 2
            if (skulData.skill2 != null)
            {
                RegisterAbility($"{skulData.skulId}_skill2", skulData.skill2);
            }

            // 대시
            if (skulData.dashAbility != null)
            {
                RegisterAbility($"{skulData.skulId}_dash", skulData.dashAbility);
            }
            else
            {
                RegisterAbility($"{currentSkul?.skulId}_basic_dash", new PlatformerAbilityData 
                {
                    abilityName = "Basic Dash",
                    abilityId = $"{currentSkul?.skulId}_basic_dash",
                    cooldownTime = 1f,
                    dashDistance = 5f,
                    dashDuration = 0.2f
                });
            }

            // 패시브
            if (skulData.passives != null)
            {
                for (int i = 0; i < skulData.passives.Count; i++)
                {
                    RegisterAbility($"{skulData.skulId}_passive{i}", skulData.passives[i]);
                }
            }

            return true;
        }

        /// <summary>
        /// 개별 어빌리티 등록
        /// </summary>
        private void RegisterAbility(string id, PlatformerAbilityData data)
        {
            if (data == null) return;

            // ID 설정
            data.abilityId = id;

            // 기존 어빌리티가 있으면 제거
            if (abilities.ContainsKey(id))
            {
                abilities[id].Dispose();
                abilities.Remove(id);
            }

            // 새 Ability 인스턴스 생성
            Ability newAbility = new Ability();
            newAbility.Initialize(data, gameObject);

            // 딕셔너리에 추가
            abilities[id] = newAbility;

            Debug.Log($"Registered ability: {data.abilityName} ({id})");
        }

        /// <summary>
        /// 어빌리티 제거
        /// </summary>
        public bool UnregisterAbility(string abilityId)
        {
            if (abilities.ContainsKey(abilityId))
            {
                abilities[abilityId].Dispose();
                abilities.Remove(abilityId);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 어빌리티 사용 시도 (레거시 지원)
        /// </summary>
        public async Awaitable TryUseAbility(string abilityId)
        {
            if (abilities.ContainsKey(abilityId))
            {
                await abilities[abilityId].ExecuteAsync();
            }
            else
            {
                Debug.LogWarning($"Ability {abilityId} not found");
                await Awaitable.NextFrameAsync();
            }
        }

        /// <summary>
        /// 특정 어빌리티 가져오기
        /// </summary>
        public Ability GetAbility(string abilityId)
        {
            return abilities.TryGetValue(abilityId, out var ability) ? ability : null;
        }

        /// <summary>
        /// 모든 어빌리티 가져오기
        /// </summary>
        public List<Ability> GetAllAbilities()
        {
            return abilities.Values.ToList();
        }

        /// <summary>
        /// 사용 가능한 어빌리티 목록 가져오기
        /// </summary>
        public List<Ability> GetUsableAbilities()
        {
            return abilities.Values.Where(a => a.CanUse()).ToList();
        }

        /// <summary>
        /// 현재 쿨다운 시간 가져오기
        /// </summary>
        public float GetCooldownRemaining(string abilityId)
        {
            return abilities.TryGetValue(abilityId, out var ability)
                ? ability.CooldownRemaining
                : 0f;
        }

        /// <summary>
        /// 쿨다운 진행률 가져오기 (0~1)
        /// </summary>
        public float GetCooldownProgress(string abilityId)
        {
            return abilities.TryGetValue(abilityId, out var ability)
                ? ability.CooldownProgress
                : 1f;
        }

        /// <summary>
        /// 코스트 소비 처리
        /// </summary>
        private bool ConsumeResources(PlatformerAbilityData abilityData)
        {
            if (abilityData.manaCost > 0)
            {
                currentMana = Mathf.Max(0, currentMana - abilityData.manaCost);
                OnManaChanged?.Invoke(currentMana);
            }

            return true;
        }

        /// <summary>
        /// 마나 회복
        /// </summary>
        public void RestoreMana(int amount)
        {
            currentMana = Mathf.Min(maxMana, currentMana + amount);
            OnManaChanged?.Invoke(currentMana);
        }

        /// <summary>
        /// 스태미나 자동 회복
        /// </summary>
        private void RegenerateStamina()
        {
            if (currentStamina < maxStamina)
            {
                currentStamina = Mathf.Min(maxStamina, currentStamina + (int)(20 * Time.deltaTime));
                OnStaminaChanged?.Invoke(currentStamina);
            }
        }

        /// <summary>
        /// 모든 어빌리티 쿨다운 리셋
        /// </summary>
        public void ResetAllCooldowns()
        {
            foreach (var ability in abilities.Values)
            {
                ability.Reset();
            }
        }

        /// <summary>
        /// 특정 어빌리티 쿨다운 리셋
        /// </summary>
        public void ResetCooldown(string abilityId)
        {
            if (abilities.ContainsKey(abilityId))
            {
                abilities[abilityId].Reset();
            }
        }

        /// <summary>
        /// 특정 어빌리티 쿨다운 감소
        /// </summary>
        public void ReduceCooldown(string abilityId, float amount)
        {
            if (abilities.ContainsKey(abilityId))
            {
                abilities[abilityId].ReduceCooldown(amount);
            }
        }

        /// <summary>
        /// 스컬 변경
        /// </summary>
        public void ChangeSkul(SkulData newSkul)
        {
            if (newSkul == null) return;

            // 기존 스컬의 어빌리티 제거
            if (currentSkul != null)
            {
                UnregisterSkul(currentSkul);
            }

            // 새 스컬 설정
            currentSkul = newSkul;
            RegisterSkul(newSkul);

            Debug.Log($"Changed to Skul: {newSkul.skulName}");
        }

        /// <summary>
        /// 스컬 어빌리티 제거
        /// </summary>
        private void UnregisterSkul(SkulData skulData)
        {
            if (skulData == null) return;

            UnregisterAbility($"{skulData.skulId}_basic");
            UnregisterAbility($"{skulData.skulId}_skill1");
            UnregisterAbility($"{skulData.skulId}_skill2");
            UnregisterAbility($"{skulData.skulId}_dash");

            // 패시브 제거
            for (int i = 0; i < 10; i++) // 최대 10개 패시브 가정
            {
                UnregisterAbility($"{skulData.skulId}_passive{i}");
            }
        }

        /// <summary>
        /// 시스템 정리
        /// </summary>
        private void OnDestroy()
        {
            foreach (var ability in abilities.Values)
            {
                ability.Dispose();
            }
            abilities.Clear();
        }
    }
}