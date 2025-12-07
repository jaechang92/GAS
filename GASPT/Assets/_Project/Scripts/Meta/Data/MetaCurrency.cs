using System;
using UnityEngine;

namespace GASPT.Meta
{
    /// <summary>
    /// 메타 재화 관리 클래스
    /// Bone: 일반 재화 (적 처치, 상자에서 획득)
    /// Soul: 희귀 재화 (보스 처치, 스테이지 클리어)
    /// </summary>
    [Serializable]
    public class MetaCurrency
    {
        // ====== 영구 재화 ======

        [SerializeField]
        private int bone;

        [SerializeField]
        private int soul;

        // ====== 임시 재화 (런 중) ======

        [SerializeField]
        private int tempBone;

        [SerializeField]
        private int tempSoul;


        // ====== 프로퍼티 ======

        /// <summary>
        /// 영구 Bone 보유량
        /// </summary>
        public int Bone => bone;

        /// <summary>
        /// 영구 Soul 보유량
        /// </summary>
        public int Soul => soul;

        /// <summary>
        /// 현재 런에서 획득한 임시 Bone
        /// </summary>
        public int TempBone => tempBone;

        /// <summary>
        /// 현재 런에서 획득한 임시 Soul
        /// </summary>
        public int TempSoul => tempSoul;


        // ====== 이벤트 ======

        /// <summary>
        /// Bone 변경 이벤트 (이전 값, 새 값)
        /// </summary>
        public event Action<int, int> OnBoneChanged;

        /// <summary>
        /// Soul 변경 이벤트 (이전 값, 새 값)
        /// </summary>
        public event Action<int, int> OnSoulChanged;

        /// <summary>
        /// 임시 Bone 변경 이벤트 (이전 값, 새 값)
        /// </summary>
        public event Action<int, int> OnTempBoneChanged;

        /// <summary>
        /// 임시 Soul 변경 이벤트 (이전 값, 새 값)
        /// </summary>
        public event Action<int, int> OnTempSoulChanged;


        // ====== 생성자 ======

        public MetaCurrency()
        {
            bone = 0;
            soul = 0;
            tempBone = 0;
            tempSoul = 0;
        }

        public MetaCurrency(int initialBone, int initialSoul)
        {
            bone = Mathf.Max(0, initialBone);
            soul = Mathf.Max(0, initialSoul);
            tempBone = 0;
            tempSoul = 0;
        }


        // ====== 임시 재화 관리 (런 중) ======

        /// <summary>
        /// 런 중 임시 Bone 추가
        /// </summary>
        public void AddTempBone(int amount)
        {
            if (amount <= 0)
            {
                Debug.LogWarning($"[MetaCurrency] AddTempBone: 유효하지 않은 값 {amount}");
                return;
            }

            int oldValue = tempBone;
            tempBone += amount;

            Debug.Log($"[MetaCurrency] TempBone +{amount} ({oldValue} → {tempBone})");
            OnTempBoneChanged?.Invoke(oldValue, tempBone);
        }

        /// <summary>
        /// 런 중 임시 Soul 추가
        /// </summary>
        public void AddTempSoul(int amount)
        {
            if (amount <= 0)
            {
                Debug.LogWarning($"[MetaCurrency] AddTempSoul: 유효하지 않은 값 {amount}");
                return;
            }

            int oldValue = tempSoul;
            tempSoul += amount;

            Debug.Log($"[MetaCurrency] TempSoul +{amount} ({oldValue} → {tempSoul})");
            OnTempSoulChanged?.Invoke(oldValue, tempSoul);
        }

        /// <summary>
        /// 런 종료 시 임시 재화를 영구 재화로 확정
        /// </summary>
        public void ConfirmTempCurrency()
        {
            if (tempBone > 0)
            {
                int oldBone = bone;
                bone += tempBone;
                Debug.Log($"[MetaCurrency] Bone 확정: +{tempBone} ({oldBone} → {bone})");
                OnBoneChanged?.Invoke(oldBone, bone);
            }

            if (tempSoul > 0)
            {
                int oldSoul = soul;
                soul += tempSoul;
                Debug.Log($"[MetaCurrency] Soul 확정: +{tempSoul} ({oldSoul} → {soul})");
                OnSoulChanged?.Invoke(oldSoul, soul);
            }

            // 임시 재화 초기화
            tempBone = 0;
            tempSoul = 0;
        }

        /// <summary>
        /// 런 시작 시 임시 재화 초기화
        /// </summary>
        public void ResetTempCurrency()
        {
            tempBone = 0;
            tempSoul = 0;
            Debug.Log("[MetaCurrency] 임시 재화 초기화");
        }


        // ====== 영구 재화 소비 ======

        /// <summary>
        /// Bone 소비 시도
        /// </summary>
        /// <param name="amount">소비할 양</param>
        /// <returns>성공 여부</returns>
        public bool TrySpendBone(int amount)
        {
            if (amount <= 0)
            {
                Debug.LogWarning($"[MetaCurrency] TrySpendBone: 유효하지 않은 값 {amount}");
                return false;
            }

            if (bone < amount)
            {
                Debug.LogWarning($"[MetaCurrency] TrySpendBone: Bone 부족 (필요: {amount}, 보유: {bone})");
                return false;
            }

            int oldValue = bone;
            bone -= amount;

            Debug.Log($"[MetaCurrency] Bone -{amount} ({oldValue} → {bone})");
            OnBoneChanged?.Invoke(oldValue, bone);

            return true;
        }

        /// <summary>
        /// Soul 소비 시도
        /// </summary>
        /// <param name="amount">소비할 양</param>
        /// <returns>성공 여부</returns>
        public bool TrySpendSoul(int amount)
        {
            if (amount <= 0)
            {
                Debug.LogWarning($"[MetaCurrency] TrySpendSoul: 유효하지 않은 값 {amount}");
                return false;
            }

            if (soul < amount)
            {
                Debug.LogWarning($"[MetaCurrency] TrySpendSoul: Soul 부족 (필요: {amount}, 보유: {soul})");
                return false;
            }

            int oldValue = soul;
            soul -= amount;

            Debug.Log($"[MetaCurrency] Soul -{amount} ({oldValue} → {soul})");
            OnSoulChanged?.Invoke(oldValue, soul);

            return true;
        }

        /// <summary>
        /// 재화 타입에 따른 소비 시도
        /// </summary>
        public bool TrySpend(CurrencyType type, int amount)
        {
            return type switch
            {
                CurrencyType.Bone => TrySpendBone(amount),
                CurrencyType.Soul => TrySpendSoul(amount),
                _ => false
            };
        }

        /// <summary>
        /// 특정 재화가 충분한지 확인
        /// </summary>
        public bool HasEnough(CurrencyType type, int amount)
        {
            return type switch
            {
                CurrencyType.Bone => bone >= amount,
                CurrencyType.Soul => soul >= amount,
                _ => false
            };
        }

        /// <summary>
        /// 특정 타입의 재화량 반환
        /// </summary>
        public int GetAmount(CurrencyType type)
        {
            return type switch
            {
                CurrencyType.Bone => bone,
                CurrencyType.Soul => soul,
                _ => 0
            };
        }


        // ====== 직접 설정 (저장/로드용) ======

        /// <summary>
        /// 저장 데이터로부터 재화 설정 (LoadFromSaveData용)
        /// </summary>
        public void SetFromSaveData(int savedBone, int savedSoul)
        {
            bone = Mathf.Max(0, savedBone);
            soul = Mathf.Max(0, savedSoul);
            tempBone = 0;
            tempSoul = 0;

            Debug.Log($"[MetaCurrency] 저장 데이터로 설정 - Bone: {bone}, Soul: {soul}");
        }


        // ====== 디버그 ======

        /// <summary>
        /// 테스트용 Bone 추가 (영구)
        /// </summary>
        public void DebugAddBone(int amount)
        {
            int oldValue = bone;
            bone += amount;
            bone = Mathf.Max(0, bone);

            Debug.Log($"[MetaCurrency] DEBUG: Bone {oldValue} → {bone}");
            OnBoneChanged?.Invoke(oldValue, bone);
        }

        /// <summary>
        /// 테스트용 Soul 추가 (영구)
        /// </summary>
        public void DebugAddSoul(int amount)
        {
            int oldValue = soul;
            soul += amount;
            soul = Mathf.Max(0, soul);

            Debug.Log($"[MetaCurrency] DEBUG: Soul {oldValue} → {soul}");
            OnSoulChanged?.Invoke(oldValue, soul);
        }

        public override string ToString()
        {
            return $"[MetaCurrency] Bone: {bone}, Soul: {soul}, TempBone: {tempBone}, TempSoul: {tempSoul}";
        }
    }
}
