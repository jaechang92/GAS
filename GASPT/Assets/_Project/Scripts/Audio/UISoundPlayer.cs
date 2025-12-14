using GASPT.Data;
using UnityEngine;

namespace GASPT.Audio
{
    /// <summary>
    /// UI 효과음 재생 헬퍼
    /// 버튼 OnClick, 패널 전환 등에서 호출
    /// </summary>
    public class UISoundPlayer : MonoBehaviour
    {
        // ====== 설정 ======

        [Header("사운드 데이터")]
        [SerializeField] private UISoundData soundData;


        // ====== 싱글톤 접근 (선택적) ======

        private static UISoundPlayer instance;

        /// <summary>
        /// 전역 인스턴스 (존재하는 경우)
        /// </summary>
        public static UISoundPlayer Instance => instance;

        private void Awake()
        {
            // 선택적 싱글톤: 첫 번째 인스턴스만 등록
            if (instance == null)
            {
                instance = this;
            }
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }


        // ====== 버튼 사운드 ======

        /// <summary>
        /// 버튼 클릭 효과음
        /// </summary>
        public void PlayButtonClick()
        {
            PlaySound(soundData?.buttonClick, soundData?.buttonVolumeScale ?? 0.7f);
        }

        /// <summary>
        /// 버튼 호버 효과음
        /// </summary>
        public void PlayButtonHover()
        {
            PlaySound(soundData?.buttonHover, soundData?.buttonVolumeScale ?? 0.7f);
        }

        /// <summary>
        /// 뒤로 가기/취소 효과음
        /// </summary>
        public void PlayButtonBack()
        {
            PlaySound(soundData?.buttonBack, soundData?.buttonVolumeScale ?? 0.7f);
        }

        /// <summary>
        /// 확인/수락 효과음
        /// </summary>
        public void PlayButtonConfirm()
        {
            PlaySound(soundData?.buttonConfirm, soundData?.buttonVolumeScale ?? 0.7f);
        }


        // ====== 패널 사운드 ======

        /// <summary>
        /// 패널 열기 효과음
        /// </summary>
        public void PlayPanelOpen()
        {
            PlaySound(soundData?.panelOpen, soundData?.panelVolumeScale ?? 0.6f);
        }

        /// <summary>
        /// 패널 닫기 효과음
        /// </summary>
        public void PlayPanelClose()
        {
            PlaySound(soundData?.panelClose, soundData?.panelVolumeScale ?? 0.6f);
        }

        /// <summary>
        /// 탭 전환 효과음
        /// </summary>
        public void PlayTabSwitch()
        {
            PlaySound(soundData?.tabSwitch, soundData?.panelVolumeScale ?? 0.6f);
        }


        // ====== 시스템 사운드 ======

        /// <summary>
        /// 아이템 획득 효과음
        /// </summary>
        public void PlayItemPickup()
        {
            PlaySound(soundData?.itemPickup, soundData?.systemVolumeScale ?? 0.8f);
        }

        /// <summary>
        /// 레벨업 효과음
        /// </summary>
        public void PlayLevelUp()
        {
            PlaySound(soundData?.levelUp, soundData?.systemVolumeScale ?? 0.8f);
        }

        /// <summary>
        /// 재화 획득 효과음
        /// </summary>
        public void PlayCurrencyGain()
        {
            PlaySound(soundData?.currencyGain, soundData?.systemVolumeScale ?? 0.8f);
        }

        /// <summary>
        /// 퀘스트 완료 효과음
        /// </summary>
        public void PlayQuestComplete()
        {
            PlaySound(soundData?.questComplete, soundData?.systemVolumeScale ?? 0.8f);
        }

        /// <summary>
        /// 알림 효과음
        /// </summary>
        public void PlayNotification()
        {
            PlaySound(soundData?.notification, soundData?.systemVolumeScale ?? 0.8f);
        }


        // ====== 폼/스킬 사운드 ======

        /// <summary>
        /// 폼 변환 효과음
        /// </summary>
        public void PlayFormSwap()
        {
            PlaySound(soundData?.formSwap, soundData?.systemVolumeScale ?? 0.8f);
        }

        /// <summary>
        /// 스킬 준비 완료 효과음
        /// </summary>
        public void PlaySkillReady()
        {
            PlaySound(soundData?.skillReady, soundData?.systemVolumeScale ?? 0.8f);
        }

        /// <summary>
        /// 스킬 사용 불가 효과음
        /// </summary>
        public void PlaySkillUnavailable()
        {
            PlaySound(soundData?.skillUnavailable, soundData?.systemVolumeScale ?? 0.8f);
        }


        // ====== 던전 사운드 ======

        /// <summary>
        /// 포탈 진입 효과음
        /// </summary>
        public void PlayPortalEnter()
        {
            PlaySound(soundData?.portalEnter, soundData?.systemVolumeScale ?? 0.8f);
        }

        /// <summary>
        /// 방 클리어 효과음
        /// </summary>
        public void PlayRoomClear()
        {
            PlaySound(soundData?.roomClear, soundData?.systemVolumeScale ?? 0.8f);
        }

        /// <summary>
        /// 보스 등장 효과음
        /// </summary>
        public void PlayBossAppear()
        {
            PlaySound(soundData?.bossAppear, soundData?.systemVolumeScale ?? 0.8f);
        }

        /// <summary>
        /// 던전 클리어 효과음
        /// </summary>
        public void PlayDungeonClear()
        {
            PlaySound(soundData?.dungeonClear, soundData?.systemVolumeScale ?? 0.8f);
        }


        // ====== 내부 메서드 ======

        private void PlaySound(AudioClip clip, float volumeScale)
        {
            if (clip == null || AudioManager.Instance == null) return;

            AudioManager.Instance.PlaySFX(clip, volumeScale);
        }


        // ====== 정적 헬퍼 (싱글톤 사용) ======

        /// <summary>
        /// 정적으로 버튼 클릭 사운드 재생
        /// </summary>
        public static void PlayClick()
        {
            instance?.PlayButtonClick();
        }

        /// <summary>
        /// 정적으로 버튼 뒤로 가기 사운드 재생
        /// </summary>
        public static void PlayBack()
        {
            instance?.PlayButtonBack();
        }

        /// <summary>
        /// 정적으로 확인 사운드 재생
        /// </summary>
        public static void PlayConfirm()
        {
            instance?.PlayButtonConfirm();
        }


        // ====== 에디터/디버그 ======

        [ContextMenu("Test Button Click")]
        private void DebugButtonClick()
        {
            PlayButtonClick();
        }

        [ContextMenu("Test Panel Open")]
        private void DebugPanelOpen()
        {
            PlayPanelOpen();
        }

        [ContextMenu("Test Level Up")]
        private void DebugLevelUp()
        {
            PlayLevelUp();
        }
    }
}
