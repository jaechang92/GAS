// ���� ��ġ: Assets/Scripts/GAS/Learning/Phase1/BaseAttribute.cs
using UnityEngine;
using System;

namespace GAS.Learning.Phase1
{
    /// <summary>
    /// Step 1: Attribute�� �⺻ ���� ����
    /// </summary>
    public class BaseAttribute : MonoBehaviour
    {
        // ������ Attribute Ŭ���� ����
        [System.Serializable]
        public class SimpleAttribute
        {
            [SerializeField] private string name;
            [SerializeField] private float baseValue;
            [SerializeField] private float currentValue;
            [SerializeField] private float minValue;
            [SerializeField] private float maxValue;

            // �� ���� �̺�Ʈ
            public event Action<float, float> OnValueChanged;

            public string Name => name;
            public float BaseValue => baseValue;
            public float CurrentValue => currentValue;
            public float MinValue => minValue;
            public float MaxValue => maxValue;

            public SimpleAttribute(string name, float baseValue, float minValue, float maxValue)
            {
                this.name = name;
                this.baseValue = baseValue;
                this.minValue = minValue;
                this.maxValue = maxValue;
                this.currentValue = baseValue;
            }

            // ���� �� ���� (���� ���� ����)
            public void SetCurrentValue(float value)
            {
                float oldValue = currentValue;
                currentValue = Mathf.Clamp(value, minValue, maxValue);

                if (Math.Abs(oldValue - currentValue) > 0.01f)
                {
                    OnValueChanged?.Invoke(oldValue, currentValue);
                    Debug.Log($"[{name}] �� ����: {oldValue:F1} �� {currentValue:F1}");
                }
            }

            // ���� ���� ���ϱ�
            public void AddToCurrentValue(float amount)
            {
                SetCurrentValue(currentValue + amount);
            }

            // �ۼ�Ʈ �������� (0~1)
            public float GetPercentage()
            {
                if (maxValue - minValue == 0) return 0;
                return (currentValue - minValue) / (maxValue - minValue);
            }

            // ����� ����
            public override string ToString()
            {
                return $"{name}: {currentValue:F1}/{maxValue:F1} ({GetPercentage():P0})";
            }
        }

        // �׽�Ʈ�� �Ӽ���
        [Header("ĳ���� �Ӽ�")]
        [SerializeField] private SimpleAttribute health;
        [SerializeField] private SimpleAttribute mana;
        [SerializeField] private SimpleAttribute stamina;

        private void Start()
        {
            InitializeAttributes();
            SubscribeToEvents();

            Debug.Log("=== Phase 1 - Step 1: �⺻ Attribute �׽�Ʈ ���� ===");
            TestBasicOperations();
        }

        private void InitializeAttributes()
        {
            health = new SimpleAttribute("Health", 100f, 0f, 100f);
            mana = new SimpleAttribute("Mana", 50f, 0f, 50f);
            stamina = new SimpleAttribute("Stamina", 100f, 0f, 100f);
        }

        private void SubscribeToEvents()
        {
            health.OnValueChanged += (oldVal, newVal) =>
            {
                Debug.Log($"Health ���� ����: {oldVal} �� {newVal}");

                // ü���� 30% ������ �� ���
                if (health.GetPercentage() <= 0.3f && oldVal > newVal)
                {
                    Debug.LogWarning("ü���� ���� �����Դϴ�!");
                }
            };

            mana.OnValueChanged += (oldVal, newVal) =>
            {
                Debug.Log($"Mana ���� ����: {oldVal} �� {newVal}");
            };
        }

        private void TestBasicOperations()
        {
            // �׽�Ʈ 1: ������ �ޱ�
            Debug.Log("\n[�׽�Ʈ 1] ������ ó��");
            Debug.Log($"�ʱ� ����: {health}");
            TakeDamage(30f);
            Debug.Log($"30 ������ ��: {health}");

            // �׽�Ʈ 2: �� �ޱ�
            Debug.Log("\n[�׽�Ʈ 2] �� ó��");
            Heal(20f);
            Debug.Log($"20 �� ��: {health}");

            // �׽�Ʈ 3: ������ �׽�Ʈ
            Debug.Log("\n[�׽�Ʈ 3] ������ �׽�Ʈ");
            Heal(100f);
            Debug.Log($"100 �� �õ� ��: {health}");

            // �׽�Ʈ 4: ���� ���
            Debug.Log("\n[�׽�Ʈ 4] ���� ���");
            Debug.Log($"�ʱ� ����: {mana}");
            UseMana(20f);
            Debug.Log($"20 ���� ��� ��: {mana}");

            // �׽�Ʈ 5: ���� ȸ��
            Debug.Log("\n[�׽�Ʈ 5] ���� ȸ��");
            RestoreMana(15f);
            Debug.Log($"15 ���� ȸ�� ��: {mana}");

            // �׽�Ʈ 6: ���� ���� �׽�Ʈ
            Debug.Log("\n[�׽�Ʈ 6] ���� ���� �׽�Ʈ");
            TakeDamage(200f);
            Debug.Log($"200 ������ �� (�ּҰ� ����): {health}");

            // ���� ���� ���
            PrintAllAttributes();
        }

        // �����÷��� �޼����
        public void TakeDamage(float damage)
        {
            health.AddToCurrentValue(-damage);
        }

        public void Heal(float amount)
        {
            health.AddToCurrentValue(amount);
        }

        public bool UseMana(float amount)
        {
            if (mana.CurrentValue >= amount)
            {
                mana.AddToCurrentValue(-amount);
                return true;
            }

            Debug.LogWarning("������ �����մϴ�!");
            return false;
        }

        public void RestoreMana(float amount)
        {
            mana.AddToCurrentValue(amount);
        }

        public void UseStamina(float amount)
        {
            stamina.AddToCurrentValue(-amount);
        }

        private void PrintAllAttributes()
        {
            Debug.Log("\n=== ��� �Ӽ� ���� ===");
            Debug.Log(health);
            Debug.Log(mana);
            Debug.Log(stamina);
        }

        // Unity Editor �׽�Ʈ��
        private void Update()
        {
            // Ű���� �Է����� �׽�Ʈ
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                Debug.Log("������ 10 ����");
                TakeDamage(10f);
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                Debug.Log("�� 10 ����");
                Heal(10f);
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                Debug.Log("���� 5 ���");
                UseMana(5f);
            }
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                Debug.Log("���� 10 ȸ��");
                RestoreMana(10f);
            }
            if (Input.GetKeyDown(KeyCode.Space))
            {
                PrintAllAttributes();
            }
        }

        private void OnGUI()
        {
            GUI.Box(new Rect(10, 10, 300, 150), "Phase 1 - Attribute ����");

            int y = 40;
            GUI.Label(new Rect(20, y, 280, 20), $"Health: {health?.CurrentValue:F1}/{health?.MaxValue:F1}");
            y += 25;
            GUI.Label(new Rect(20, y, 280, 20), $"Mana: {mana?.CurrentValue:F1}/{mana?.MaxValue:F1}");
            y += 25;
            GUI.Label(new Rect(20, y, 280, 20), $"Stamina: {stamina?.CurrentValue:F1}/{stamina?.MaxValue:F1}");
            y += 30;
            GUI.Label(new Rect(20, y, 280, 40), "1:������ 2:�� 3:������� 4:����ȸ��\nSpace: ���� ���");
        }
    }
}