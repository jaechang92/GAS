using UnityEngine;
using GASPT.Gameplay.Form;

namespace GASPT.Meta
{
    /// <summary>
    /// 해금 가능한 폼 정의
    /// Soul을 사용하여 해금하면 드롭 풀에 추가됨
    /// </summary>
    [CreateAssetMenu(fileName = "NewUnlockableForm", menuName = "GASPT/Meta/Unlockable Form", order = 1)]
    public class UnlockableForm : ScriptableObject
    {
        [Header("기본 정보")]
        [Tooltip("해금 ID (저장용)")]
        public string unlockId;

        [Tooltip("해금할 폼 데이터")]
        public FormData formData;

        [Header("해금 조건")]
        [Tooltip("해금에 필요한 Soul")]
        [Range(1, 1000)]
        public int soulCost = 50;

        [Tooltip("선행 해금 조건 (없으면 바로 해금 가능)")]
        public UnlockableForm[] prerequisites;

        [Tooltip("최소 클리어 횟수 조건")]
        [Range(0, 100)]
        public int requiredClears = 0;

        [Header("표시 정보")]
        [Tooltip("해금 전 표시 이름 (???)")]
        public string lockedDisplayName = "???";

        [Tooltip("해금 전 힌트 설명")]
        [TextArea(1, 3)]
        public string unlockHint = "Soul을 사용하여 해금하세요.";

        [Header("드롭 설정")]
        [Tooltip("드롭 풀 가중치 (해금 후)")]
        [Range(1, 100)]
        public int dropWeight = 50;


        /// <summary>
        /// 유효성 검증
        /// </summary>
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(unlockId) && formData != null;
        }

        /// <summary>
        /// 에디터에서 ID 자동 생성
        /// </summary>
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(unlockId) && formData != null)
            {
                unlockId = $"unlock_{formData.formId}";
            }
        }
    }
}
