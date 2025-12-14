using UnityEngine;

namespace GASPT.Data
{
    /// <summary>
    /// UI 관련 사운드 데이터
    /// 버튼, 패널, 시스템 효과음 관리
    /// </summary>
    [CreateAssetMenu(fileName = "UISoundData", menuName = "GASPT/Audio/UI Sound Data")]
    public class UISoundData : ScriptableObject
    {
        // ====== 버튼 사운드 ======

        [Header("버튼")]
        [Tooltip("일반 버튼 클릭")]
        public AudioClip buttonClick;

        [Tooltip("버튼 호버 (선택)")]
        public AudioClip buttonHover;

        [Tooltip("뒤로 가기/취소 버튼")]
        public AudioClip buttonBack;

        [Tooltip("확인/수락 버튼")]
        public AudioClip buttonConfirm;


        // ====== 패널 사운드 ======

        [Header("패널")]
        [Tooltip("패널/창 열기")]
        public AudioClip panelOpen;

        [Tooltip("패널/창 닫기")]
        public AudioClip panelClose;

        [Tooltip("탭 전환")]
        public AudioClip tabSwitch;


        // ====== 게임 시스템 사운드 ======

        [Header("게임 시스템")]
        [Tooltip("아이템 획득")]
        public AudioClip itemPickup;

        [Tooltip("레벨업")]
        public AudioClip levelUp;

        [Tooltip("골드/재화 획득")]
        public AudioClip currencyGain;

        [Tooltip("퀘스트/미션 완료")]
        public AudioClip questComplete;

        [Tooltip("알림/경고")]
        public AudioClip notification;


        // ====== 폼/스킬 관련 사운드 ======

        [Header("폼/스킬")]
        [Tooltip("폼 변환")]
        public AudioClip formSwap;

        [Tooltip("스킬 쿨다운 완료")]
        public AudioClip skillReady;

        [Tooltip("스킬 사용 불가 (쿨다운/마나 부족)")]
        public AudioClip skillUnavailable;


        // ====== 던전/스테이지 사운드 ======

        [Header("던전/스테이지")]
        [Tooltip("포탈 진입")]
        public AudioClip portalEnter;

        [Tooltip("방 클리어")]
        public AudioClip roomClear;

        [Tooltip("보스 등장")]
        public AudioClip bossAppear;

        [Tooltip("던전 클리어")]
        public AudioClip dungeonClear;


        // ====== 볼륨 설정 ======

        [Header("볼륨 스케일")]
        [Range(0f, 1f)]
        [Tooltip("버튼 사운드 볼륨 스케일")]
        public float buttonVolumeScale = 0.7f;

        [Range(0f, 1f)]
        [Tooltip("패널 사운드 볼륨 스케일")]
        public float panelVolumeScale = 0.6f;

        [Range(0f, 1f)]
        [Tooltip("시스템 사운드 볼륨 스케일")]
        public float systemVolumeScale = 0.8f;
    }
}
