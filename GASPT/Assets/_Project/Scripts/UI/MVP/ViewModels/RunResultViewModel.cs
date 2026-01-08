namespace GASPT.UI.MVP.ViewModels
{
    /// <summary>
    /// 런 결과 화면에 표시할 데이터 모델
    /// </summary>
    public class RunResultViewModel
    {
        // ====== 결과 상태 ======

        /// <summary>
        /// 클리어 여부 (true: 클리어, false: 사망)
        /// </summary>
        public bool IsCleared { get; set; }

        /// <summary>
        /// 결과 타이틀 (예: "던전 클리어!", "게임 오버")
        /// </summary>
        public string ResultTitle { get; set; }

        /// <summary>
        /// 결과 부제목 (예: "스테이지 3 완료!", "스테이지 2에서 사망")
        /// </summary>
        public string ResultSubtitle { get; set; }


        // ====== 런 통계 ======

        /// <summary>
        /// 도달한 스테이지 번호
        /// </summary>
        public int StageReached { get; set; }

        /// <summary>
        /// 클리어한 방 수
        /// </summary>
        public int RoomsCleared { get; set; }

        /// <summary>
        /// 처치한 적 수
        /// </summary>
        public int EnemiesKilled { get; set; }

        /// <summary>
        /// 플레이 시간 (초)
        /// </summary>
        public float PlayTimeSeconds { get; set; }

        /// <summary>
        /// 포맷된 플레이 시간 문자열 (예: "05:32")
        /// </summary>
        public string PlayTimeFormatted => FormatTime(PlayTimeSeconds);


        // ====== 획득 재화 ======

        /// <summary>
        /// 이번 런에서 획득한 골드
        /// </summary>
        public int GoldEarned { get; set; }

        /// <summary>
        /// 이번 런에서 획득한 Bone (확정 전)
        /// </summary>
        public int BoneEarned { get; set; }

        /// <summary>
        /// 이번 런에서 획득한 Soul (확정 전)
        /// </summary>
        public int SoulEarned { get; set; }


        // ====== 누적 통계 (선택적) ======

        /// <summary>
        /// 총 보유 Bone (확정 후)
        /// </summary>
        public int TotalBone { get; set; }

        /// <summary>
        /// 총 보유 Soul (확정 후)
        /// </summary>
        public int TotalSoul { get; set; }

        /// <summary>
        /// 최고 도달 스테이지
        /// </summary>
        public int HighestStage { get; set; }

        /// <summary>
        /// 총 클리어 횟수
        /// </summary>
        public int TotalClears { get; set; }

        /// <summary>
        /// 신기록 달성 여부 (최고 스테이지 갱신)
        /// </summary>
        public bool IsNewRecord { get; set; }


        // ====== 유틸리티 ======

        /// <summary>
        /// 시간을 MM:SS 형식으로 포맷
        /// </summary>
        private string FormatTime(float seconds)
        {
            int totalSeconds = (int)seconds;
            int minutes = totalSeconds / 60;
            int secs = totalSeconds % 60;
            return $"{minutes:D2}:{secs:D2}";
        }

        /// <summary>
        /// 기본 생성자
        /// </summary>
        public RunResultViewModel()
        {
            ResultTitle = "런 종료";
            ResultSubtitle = "";
        }

        /// <summary>
        /// 클리어 결과 생성
        /// </summary>
        public static RunResultViewModel CreateClearResult(int stage, int rooms, int enemies, float time, int gold, int bone, int soul)
        {
            return new RunResultViewModel
            {
                IsCleared = true,
                ResultTitle = "던전 클리어!",
                ResultSubtitle = $"스테이지 {stage} 완료!",
                StageReached = stage,
                RoomsCleared = rooms,
                EnemiesKilled = enemies,
                PlayTimeSeconds = time,
                GoldEarned = gold,
                BoneEarned = bone,
                SoulEarned = soul
            };
        }

        /// <summary>
        /// 사망 결과 생성
        /// </summary>
        public static RunResultViewModel CreateDeathResult(int stage, int rooms, int enemies, float time, int gold, int bone, int soul)
        {
            return new RunResultViewModel
            {
                IsCleared = false,
                ResultTitle = "게임 오버",
                ResultSubtitle = $"스테이지 {stage}에서 사망",
                StageReached = stage,
                RoomsCleared = rooms,
                EnemiesKilled = enemies,
                PlayTimeSeconds = time,
                GoldEarned = gold,
                BoneEarned = bone,
                SoulEarned = soul
            };
        }
    }
}
