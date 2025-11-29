using UnityEngine;

namespace GASPT.Gameplay.Level.Generation
{
    /// <summary>
    /// 시드 관리 정적 클래스
    /// 재현 가능한 랜덤 던전 생성을 위한 시드 및 Random.State 관리
    /// </summary>
    public static class SeedManager
    {
        // ====== 상태 ======

        private static int currentSeed;
        private static Random.State savedState;
        private static bool hasStateSaved;


        // ====== 프로퍼티 ======

        /// <summary>
        /// 현재 시드 값
        /// </summary>
        public static int CurrentSeed => currentSeed;

        /// <summary>
        /// 저장된 상태가 있는지 여부
        /// </summary>
        public static bool HasSavedState => hasStateSaved;


        // ====== 시드 설정 ======

        /// <summary>
        /// 시드 설정 및 Random 초기화
        /// </summary>
        public static void SetSeed(int seed)
        {
            currentSeed = seed;
            Random.InitState(seed);
            Debug.Log($"[SeedManager] 시드 설정: {seed}");
        }

        /// <summary>
        /// 현재 시드 반환
        /// </summary>
        public static int GetCurrentSeed()
        {
            return currentSeed;
        }

        /// <summary>
        /// 타임스탬프 기반 랜덤 시드 생성
        /// </summary>
        public static int GenerateRandomSeed()
        {
            // 현재 시간 기반으로 시드 생성
            int seed = (int)(System.DateTime.Now.Ticks % int.MaxValue);
            Debug.Log($"[SeedManager] 랜덤 시드 생성: {seed}");
            return seed;
        }

        /// <summary>
        /// 랜덤 시드 생성 및 적용
        /// </summary>
        public static int SetRandomSeed()
        {
            int seed = GenerateRandomSeed();
            SetSeed(seed);
            return seed;
        }


        // ====== Random.State 관리 ======

        /// <summary>
        /// 현재 Random.State 저장
        /// </summary>
        public static void SaveRandomState()
        {
            savedState = Random.state;
            hasStateSaved = true;
            Debug.Log("[SeedManager] Random.State 저장됨");
        }

        /// <summary>
        /// 저장된 Random.State 복원
        /// </summary>
        public static void RestoreRandomState()
        {
            if (!hasStateSaved)
            {
                Debug.LogWarning("[SeedManager] 저장된 Random.State가 없습니다!");
                return;
            }

            Random.state = savedState;
            Debug.Log("[SeedManager] Random.State 복원됨");
        }

        /// <summary>
        /// 저장된 상태 클리어
        /// </summary>
        public static void ClearSavedState()
        {
            hasStateSaved = false;
            Debug.Log("[SeedManager] 저장된 Random.State 클리어됨");
        }


        // ====== 유틸리티 ======

        /// <summary>
        /// 시드 기반 범위 내 랜덤 정수 (재현 가능)
        /// </summary>
        public static int Range(int min, int maxExclusive)
        {
            return Random.Range(min, maxExclusive);
        }

        /// <summary>
        /// 시드 기반 범위 내 랜덤 실수 (재현 가능)
        /// </summary>
        public static float Range(float min, float max)
        {
            return Random.Range(min, max);
        }

        /// <summary>
        /// 시드 기반 0~1 랜덤 값 (재현 가능)
        /// </summary>
        public static float Value()
        {
            return Random.value;
        }

        /// <summary>
        /// 확률 체크 (0~1)
        /// </summary>
        public static bool Chance(float probability)
        {
            return Random.value < probability;
        }


        // ====== 디버그 ======

        /// <summary>
        /// 현재 상태 로그 출력
        /// </summary>
        public static void LogCurrentState()
        {
            Debug.Log($"[SeedManager] CurrentSeed: {currentSeed}, HasSavedState: {hasStateSaved}");
        }
    }
}
