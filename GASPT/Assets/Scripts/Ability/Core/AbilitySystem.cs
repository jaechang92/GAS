// ���� ��ġ: Assets/Scripts/Ability/Core/AbilitySystem.cs
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using AbilitySystem.Platformer;
using System;

namespace AbilitySystem
{
    /// <summary>
    /// ĳ������ �����Ƽ�� �����ϴ� �ý��� - �缳�� ����
    /// </summary>
    public class AbilitySystem : MonoBehaviour
    {
        [Header("�����Ƽ ����")]
        [SerializeField] private List<SkulData> initialSkuls = new List<SkulData>();

        // ��ϵ� �����Ƽ ��� (���ο� Ability Ŭ���� ���)
        private Dictionary<string, Ability> abilities = new Dictionary<string, Ability>();

        // ���� Ȱ�� ����
        private SkulData currentSkul;

        // ĳ���� ���� ����
        [Header("ĳ���� ����")]
        [SerializeField] private int maxMana = 100;
        [SerializeField] private int currentMana = 100;
        [SerializeField] private int maxStamina = 100;
        [SerializeField] private int currentStamina = 100;

        // ������Ƽ
        public int CurrentMana => currentMana;
        public int CurrentStamina => currentStamina;
        public int MaxMana => maxMana;
        public int MaxStamina => maxStamina;
        public IReadOnlyDictionary<string, Ability> Abilities => abilities;
        public SkulData CurrentSkul => currentSkul;

        // �̺�Ʈ
        public event Action<string, float> OnAbilityUsed;
        public event Action<string> OnCooldownStarted;
        public event Action<string> OnCooldownEnded;
        public event Action<int> OnManaChanged;
        public event Action<int> OnStaminaChanged;

        /// <summary>
        /// �ý��� �ʱ�ȭ
        /// </summary>
        private void Awake()
        {
            // �ʱ� ���� ����
            currentMana = maxMana;
            currentStamina = maxStamina;

            // �ʱ� ���� �ε�
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
        /// �� ������ ������Ʈ
        /// </summary>
        private void Update()
        {
            // ��� �����Ƽ�� ��ٿ� ������Ʈ
            foreach (var ability in abilities.Values)
            {
                ability.UpdateCooldown(Time.deltaTime);
            }

            // ���¹̳� �ڵ� ȸ��
            RegenerateStamina();
        }

        /// <summary>
        /// �����Ƽ ��� ���� ���� üũ
        /// </summary>
        public bool CanUseAbility(string abilityId)
        {
            // �����Ƽ�� �������� ������
            if (!abilities.ContainsKey(abilityId))
            {
                Debug.LogWarning($"Ability {abilityId} not found!");
                return false;
            }

            // Ability Ŭ������ CanUse �޼��� Ȱ��
            return abilities[abilityId].CanUse();
        }

        /// <summary>
        /// �����Ƽ ���� (���ο� ����)
        /// </summary>
        public async Awaitable ExecuteAbility(PlatformerAbilityData abilityData, GameObject caster)
        {
            if (abilityData == null)
            {
                Debug.LogError("AbilityData is null!");
                return;
            }

            string abilityId = abilityData.abilityId;

            // �����Ƽ�� ������ �ӽ÷� ����
            if (!abilities.ContainsKey(abilityId))
            {
                RegisterAbility(abilityId, abilityData);
            }

            // ��� ���� üũ
            if (!CanUseAbility(abilityId))
            {
                Debug.Log($"Cannot use ability: {abilityId}");
                return;
            }

            // ���ҽ� �Һ�
            ConsumeResources(abilityData);

            // Ability Ŭ������ ���� ����
            Ability ability = abilities[abilityId];

            // ��ٿ� ���� �̺�Ʈ ����
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

            // �����Ƽ ����
            await ability.ExecuteAsync();

            // �̺�Ʈ ���� ����
            ability.OnCooldownStarted -= OnCooldownStart;
            ability.OnCooldownCompleted -= OnCooldownEnd;

            // �̺�Ʈ �߻�
            OnAbilityUsed?.Invoke(abilityId, abilityData.cooldownTime);
        }

        /// <summary>
        /// ���� ��� (������ ��� �����Ƽ ���)
        /// </summary>
        public bool RegisterSkul(SkulData skulData)
        {
            if (skulData == null) return false;

            Debug.Log($"Registering Skul: {skulData.skulName}");

            // �⺻ ����
            if (skulData.basicAttack != null)
            {
                RegisterAbility($"{skulData.skulId}_basic", skulData.basicAttack);
            }

            // ��ų 1
            if (skulData.skill1 != null)
            {
                RegisterAbility($"{skulData.skulId}_skill1", skulData.skill1);
            }

            // ��ų 2
            if (skulData.skill2 != null)
            {
                RegisterAbility($"{skulData.skulId}_skill2", skulData.skill2);
            }

            // ���
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

            // �нú�
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
        /// ���� �����Ƽ ���
        /// </summary>
        private void RegisterAbility(string id, PlatformerAbilityData data)
        {
            if (data == null) return;

            // ID ����
            data.abilityId = id;

            // ���� �����Ƽ�� ������ ����
            if (abilities.ContainsKey(id))
            {
                abilities[id].Dispose();
                abilities.Remove(id);
            }

            // �� Ability �ν��Ͻ� ����
            Ability newAbility = new Ability();
            newAbility.Initialize(data, gameObject);

            // ��ųʸ��� �߰�
            abilities[id] = newAbility;

            Debug.Log($"Registered ability: {data.abilityName} ({id})");
        }

        /// <summary>
        /// �����Ƽ ����
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
        /// �����Ƽ ��� �õ� (���Ž� ����)
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
        /// Ư�� �����Ƽ ��������
        /// </summary>
        public Ability GetAbility(string abilityId)
        {
            return abilities.TryGetValue(abilityId, out var ability) ? ability : null;
        }

        /// <summary>
        /// ��� �����Ƽ ��������
        /// </summary>
        public List<Ability> GetAllAbilities()
        {
            return abilities.Values.ToList();
        }

        /// <summary>
        /// ��� ������ �����Ƽ ��� ��������
        /// </summary>
        public List<Ability> GetUsableAbilities()
        {
            return abilities.Values.Where(a => a.CanUse()).ToList();
        }

        /// <summary>
        /// ���� ��ٿ� �ð� ��������
        /// </summary>
        public float GetCooldownRemaining(string abilityId)
        {
            return abilities.TryGetValue(abilityId, out var ability)
                ? ability.CooldownRemaining
                : 0f;
        }

        /// <summary>
        /// ��ٿ� ����� �������� (0~1)
        /// </summary>
        public float GetCooldownProgress(string abilityId)
        {
            return abilities.TryGetValue(abilityId, out var ability)
                ? ability.CooldownProgress
                : 1f;
        }

        /// <summary>
        /// �ڽ�Ʈ �Һ� ó��
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
        /// ���� ȸ��
        /// </summary>
        public void RestoreMana(int amount)
        {
            currentMana = Mathf.Min(maxMana, currentMana + amount);
            OnManaChanged?.Invoke(currentMana);
        }

        /// <summary>
        /// ���¹̳� �ڵ� ȸ��
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
        /// ��� �����Ƽ ��ٿ� ����
        /// </summary>
        public void ResetAllCooldowns()
        {
            foreach (var ability in abilities.Values)
            {
                ability.Reset();
            }
        }

        /// <summary>
        /// Ư�� �����Ƽ ��ٿ� ����
        /// </summary>
        public void ResetCooldown(string abilityId)
        {
            if (abilities.ContainsKey(abilityId))
            {
                abilities[abilityId].Reset();
            }
        }

        /// <summary>
        /// Ư�� �����Ƽ ��ٿ� ����
        /// </summary>
        public void ReduceCooldown(string abilityId, float amount)
        {
            if (abilities.ContainsKey(abilityId))
            {
                abilities[abilityId].ReduceCooldown(amount);
            }
        }

        /// <summary>
        /// ���� ����
        /// </summary>
        public void ChangeSkul(SkulData newSkul)
        {
            if (newSkul == null) return;

            // ���� ������ �����Ƽ ����
            if (currentSkul != null)
            {
                UnregisterSkul(currentSkul);
            }

            // �� ���� ����
            currentSkul = newSkul;
            RegisterSkul(newSkul);

            Debug.Log($"Changed to Skul: {newSkul.skulName}");
        }

        /// <summary>
        /// ���� �����Ƽ ����
        /// </summary>
        private void UnregisterSkul(SkulData skulData)
        {
            if (skulData == null) return;

            UnregisterAbility($"{skulData.skulId}_basic");
            UnregisterAbility($"{skulData.skulId}_skill1");
            UnregisterAbility($"{skulData.skulId}_skill2");
            UnregisterAbility($"{skulData.skulId}_dash");

            // �нú� ����
            for (int i = 0; i < 10; i++) // �ִ� 10�� �нú� ����
            {
                UnregisterAbility($"{skulData.skulId}_passive{i}");
            }
        }

        /// <summary>
        /// �ý��� ����
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