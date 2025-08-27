// 파일 위치: Assets/Scripts/GAS/Learning/Phase1/BaseAttribute.cs
using UnityEngine;
using System;

namespace GAS.Learning.Phase1
{
    /// <summary>
    /// Step 1: Attribute의 기본 구조 이해
    /// </summary>
    public class BaseAttribute : MonoBehaviour
    {
        // 간단한 Attribute 클래스 구현
        [System.Serializable]
        public class SimpleAttribute
        {
            [SerializeField] private string name;
            [SerializeField] private float baseValue;
            [SerializeField] private float currentValue;
            [SerializeField] private float minValue;
            [SerializeField] private float maxValue;

            // 값 변경 이벤트
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

            // 현재 값 설정 (범위 제한 적용)
            public void SetCurrentValue(float value)
            {
                float oldValue = currentValue;
                currentValue = Mathf.Clamp(value, minValue, maxValue);

                if (Math.Abs(oldValue - currentValue) > 0.01f)
                {
                    OnValueChanged?.Invoke(oldValue, currentValue);
                    Debug.Log($"[{name}] 값 변경: {oldValue:F1} → {currentValue:F1}");
                }
            }

            // 현재 값에 더하기
            public void AddToCurrentValue(float amount)
            {
                SetCurrentValue(currentValue + amount);
            }

            // 퍼센트 가져오기 (0~1)
            public float GetPercentage()
            {
                if (maxValue - minValue == 0) return 0;
                return (currentValue - minValue) / (maxValue - minValue);
            }

            // 디버그 정보
            public override string ToString()
            {
                return $"{name}: {currentValue:F1}/{maxValue:F1} ({GetPercentage():P0})";
            }
        }

        // 테스트용 속성들
        [Header("캐릭터 속성")]
        [SerializeField] private SimpleAttribute health;
        [SerializeField] private SimpleAttribute mana;
        [SerializeField] private SimpleAttribute stamina;

        private void Start()
        {
            InitializeAttributes();
            SubscribeToEvents();

            Debug.Log("=== Phase 1 - Step 1: 기본 Attribute 테스트 시작 ===");
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
                Debug.Log($"Health 변경 감지: {oldVal} → {newVal}");

                // 체력이 30% 이하일 때 경고
                if (health.GetPercentage() <= 0.3f && oldVal > newVal)
                {
                    Debug.LogWarning("체력이 위험 수준입니다!");
                }
            };

            mana.OnValueChanged += (oldVal, newVal) =>
            {
                Debug.Log($"Mana 변경 감지: {oldVal} → {newVal}");
            };
        }

        private void TestBasicOperations()
        {
            // 테스트 1: 데미지 받기
            Debug.Log("\n[테스트 1] 데미지 처리");
            Debug.Log($"초기 상태: {health}");
            TakeDamage(30f);
            Debug.Log($"30 데미지 후: {health}");

            // 테스트 2: 힐 받기
            Debug.Log("\n[테스트 2] 힐 처리");
            Heal(20f);
            Debug.Log($"20 힐 후: {health}");

            // 테스트 3: 오버힐 테스트
            Debug.Log("\n[테스트 3] 오버힐 테스트");
            Heal(100f);
            Debug.Log($"100 힐 시도 후: {health}");

            // 테스트 4: 마나 사용
            Debug.Log("\n[테스트 4] 마나 사용");
            Debug.Log($"초기 마나: {mana}");
            UseMana(20f);
            Debug.Log($"20 마나 사용 후: {mana}");

            // 테스트 5: 마나 회복
            Debug.Log("\n[테스트 5] 마나 회복");
            RestoreMana(15f);
            Debug.Log($"15 마나 회복 후: {mana}");

            // 테스트 6: 범위 제한 테스트
            Debug.Log("\n[테스트 6] 범위 제한 테스트");
            TakeDamage(200f);
            Debug.Log($"200 데미지 후 (최소값 제한): {health}");

            // 최종 상태 출력
            PrintAllAttributes();
        }

        // 게임플레이 메서드들
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

            Debug.LogWarning("마나가 부족합니다!");
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
            Debug.Log("\n=== 모든 속성 상태 ===");
            Debug.Log(health);
            Debug.Log(mana);
            Debug.Log(stamina);
        }

        // Unity Editor 테스트용
        private void Update()
        {
            // 키보드 입력으로 테스트
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                Debug.Log("데미지 10 받음");
                TakeDamage(10f);
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                Debug.Log("힐 10 받음");
                Heal(10f);
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                Debug.Log("마나 5 사용");
                UseMana(5f);
            }
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                Debug.Log("마나 10 회복");
                RestoreMana(10f);
            }
            if (Input.GetKeyDown(KeyCode.Space))
            {
                PrintAllAttributes();
            }
        }

        private void OnGUI()
        {
            GUI.Box(new Rect(10, 10, 300, 150), "Phase 1 - Attribute 기초");

            int y = 40;
            GUI.Label(new Rect(20, y, 280, 20), $"Health: {health?.CurrentValue:F1}/{health?.MaxValue:F1}");
            y += 25;
            GUI.Label(new Rect(20, y, 280, 20), $"Mana: {mana?.CurrentValue:F1}/{mana?.MaxValue:F1}");
            y += 25;
            GUI.Label(new Rect(20, y, 280, 20), $"Stamina: {stamina?.CurrentValue:F1}/{stamina?.MaxValue:F1}");
            y += 30;
            GUI.Label(new Rect(20, y, 280, 40), "1:데미지 2:힐 3:마나사용 4:마나회복\nSpace: 상태 출력");
        }
    }
}