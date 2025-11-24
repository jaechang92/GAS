using System;
using UnityEngine;
using GASPT.Save;

namespace GASPT.Economy
{
    /// <summary>
    /// 골드 화폐 시스템 싱글톤
    /// 골드 추가, 소비, 이벤트 관리
    /// ISaveable 인터페이스 구현으로 SaveManager 지원
    /// </summary>
    public class CurrencySystem : SingletonManager<CurrencySystem>, ISaveable
    {
        // ====== 골드 ======

        [Header("골드")]
        [SerializeField] [Tooltip("시작 골드")]
        private int startingGold = 100;

        private int currentGold;


        // ====== 이벤트 ======

        /// <summary>
        /// 골드 변경 시 발생하는 이벤트
        /// 매개변수: (이전 골드, 새 골드)
        /// </summary>
        public event Action<int, int> OnGoldChanged;


        // ====== 프로퍼티 ======

        /// <summary>
        /// 현재 골드 (읽기 전용)
        /// </summary>
        public int Gold => currentGold;


        // ====== Unity 생명주기 ======

        protected override void OnAwake()
        {
            currentGold = startingGold;
            Debug.Log($"[CurrencySystem] 초기화 완료 - 시작 골드: {currentGold}");
        }


        // ====== 골드 관리 ======

        /// <summary>
        /// 골드 추가
        /// </summary>
        /// <param name="amount">추가할 골드 양</param>
        public void AddGold(int amount)
        {
            if (amount <= 0)
            {
                Debug.LogWarning($"[CurrencySystem] AddGold(): amount가 0 이하입니다: {amount}");
                return;
            }

            int oldGold = currentGold;
            currentGold += amount;

            // 이벤트 발생
            OnGoldChanged?.Invoke(oldGold, currentGold);

            Debug.Log($"[CurrencySystem] 골드 추가: {oldGold} → {currentGold} (+{amount})");
        }

        /// <summary>
        /// 골드 소비 시도
        /// </summary>
        /// <param name="amount">소비할 골드 양</param>
        /// <returns>true: 소비 성공, false: 골드 부족</returns>
        public bool TrySpendGold(int amount)
        {
            if (amount <= 0)
            {
                Debug.LogWarning($"[CurrencySystem] TrySpendGold(): amount가 0 이하입니다: {amount}");
                return false;
            }

            // 골드 부족
            if (currentGold < amount)
            {
                Debug.Log($"[CurrencySystem] 골드 부족: 현재 {currentGold}, 필요 {amount} (부족 {amount - currentGold})");
                return false;
            }

            // 골드 소비
            int oldGold = currentGold;
            currentGold -= amount;

            // 이벤트 발생
            OnGoldChanged?.Invoke(oldGold, currentGold);

            Debug.Log($"[CurrencySystem] 골드 소비: {oldGold} → {currentGold} (-{amount})");
            return true;
        }

        /// <summary>
        /// 골드 설정 (디버그/치트용)
        /// </summary>
        /// <param name="amount">설정할 골드</param>
        public void SetGold(int amount)
        {
            if (amount < 0)
            {
                Debug.LogWarning($"[CurrencySystem] SetGold(): amount가 음수입니다: {amount}");
                amount = 0;
            }

            int oldGold = currentGold;
            currentGold = amount;

            // 이벤트 발생
            OnGoldChanged?.Invoke(oldGold, currentGold);

            Debug.Log($"[CurrencySystem] 골드 설정: {oldGold} → {currentGold}");
        }

        public void ResetGold()
        {
            SetGold(startingGold);
        }

        /// <summary>
        /// 골드가 충분한지 확인
        /// </summary>
        /// <param name="amount">필요한 골드</param>
        /// <returns>true: 충분함, false: 부족함</returns>
        public bool HasEnoughGold(int amount)
        {
            return currentGold >= amount;
        }


        // ====== 디버그 ======

        /// <summary>
        /// 현재 골드 정보 출력
        /// </summary>
        [ContextMenu("Print Gold Info")]
        public void DebugPrintGold()
        {
            Debug.Log($"[CurrencySystem] 현재 골드: {currentGold}");
        }

        /// <summary>
        /// 테스트용 골드 추가 (Context Menu)
        /// </summary>
        [ContextMenu("Add 100 Gold (Test)")]
        private void DebugAddGold()
        {
            AddGold(100);
        }

        /// <summary>
        /// 테스트용 골드 소비 (Context Menu)
        /// </summary>
        [ContextMenu("Spend 50 Gold (Test)")]
        private void DebugSpendGold()
        {
            TrySpendGold(50);
        }


        // ====== ISaveable 인터페이스 구현 ======

        /// <summary>
        /// ISaveable 인터페이스: 저장 가능 객체 고유 ID
        /// </summary>
        public string SaveID => "CurrencySystem";

        /// <summary>
        /// ISaveable.GetSaveData() 명시적 구현
        /// 내부적으로 구체적 타입의 GetSaveData()를 호출합니다
        /// </summary>
        object ISaveable.GetSaveData()
        {
            return GetSaveData();
        }

        /// <summary>
        /// ISaveable.LoadFromSaveData(object) 명시적 구현
        /// 타입 검증 후 구체적 타입의 LoadFromSaveData()를 호출합니다
        /// </summary>
        void ISaveable.LoadFromSaveData(object data)
        {
            if (data is CurrencyData currencyData)
            {
                LoadFromSaveData(currencyData);
            }
            else
            {
                Debug.LogError($"[CurrencySystem] ISaveable.LoadFromSaveData(): 잘못된 데이터 타입입니다. Expected: CurrencyData, Got: {data?.GetType().Name}");
            }
        }


        // ====== Save/Load (기존 방식) ======

        /// <summary>
        /// 현재 화폐 데이터를 저장용 구조로 반환합니다
        /// </summary>
        public CurrencyData GetSaveData()
        {
            CurrencyData data = new CurrencyData();
            data.gold = currentGold;

            Debug.Log($"[CurrencySystem] GetSaveData(): Gold={data.gold}");

            return data;
        }

        /// <summary>
        /// 저장된 데이터로부터 화폐 시스템을 복원합니다
        /// </summary>
        public void LoadFromSaveData(CurrencyData data)
        {
            if (data == null)
            {
                Debug.LogError("[CurrencySystem] LoadFromSaveData(): data가 null입니다.");
                return;
            }

            int oldGold = currentGold;
            currentGold = Mathf.Max(0, data.gold);

            // 이벤트 발생
            OnGoldChanged?.Invoke(oldGold, currentGold);

            Debug.Log($"[CurrencySystem] LoadFromSaveData(): Gold={oldGold} → {currentGold}");
        }
    }
}
