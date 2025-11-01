using UnityEngine;

namespace Gameplay.Common
{
    /// <summary>
    /// 스컬별 이동 특성 데이터를 저장하는 ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "SkullMovementProfile", menuName = "GASPT/Physics/Skull Movement Profile", order = 0)]
    public class SkullMovementProfile : ScriptableObject
    {
        [Header("스컬 정보")]
        [Tooltip("스컬 식별 이름")]
        [SerializeField] private string skullName = "";

        [Header("이동 특성 배율")]
        [Tooltip("이동 속도 배율 (0.1 ~ 3.0)")]
        [Range(0.1f, 3.0f)]
        [SerializeField] private float moveSpeedMultiplier = 1.0f;

        [Tooltip("점프 높이 배율 (0.1 ~ 3.0)")]
        [Range(0.1f, 3.0f)]
        [SerializeField] private float jumpHeightMultiplier = 1.0f;

        [Tooltip("공중 제어력 배율 (0.1 ~ 3.0)")]
        [Range(0.1f, 3.0f)]
        [SerializeField] private float airControlMultiplier = 1.0f;

        [Header("벽 점프 배율")]
        [Tooltip("벽 점프 수평 속도 배율 (0.1 ~ 3.0)")]
        [Range(0.1f, 3.0f)]
        [SerializeField] private float wallJumpHorizontalMultiplier = 1.0f;

        [Tooltip("벽 점프 수직 속도 배율 (0.1 ~ 3.0)")]
        [Range(0.1f, 3.0f)]
        [SerializeField] private float wallJumpVerticalMultiplier = 1.0f;

        // Public Properties
        public string SkullName => skullName;
        public float MoveSpeedMultiplier => moveSpeedMultiplier;
        public float JumpHeightMultiplier => jumpHeightMultiplier;
        public float AirControlMultiplier => airControlMultiplier;
        public float WallJumpHorizontalMultiplier => wallJumpHorizontalMultiplier;
        public float WallJumpVerticalMultiplier => wallJumpVerticalMultiplier;

        /// <summary>
        /// 프로필 검증
        /// </summary>
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(skullName))
            {
                Debug.LogWarning($"SkullMovementProfile: skullName이 비어있습니다.");
            }

            // 배율 범위 검증 (Range attribute가 처리하지만 추가 로그)
            if (moveSpeedMultiplier < 0.1f || moveSpeedMultiplier > 3.0f)
                Debug.LogWarning($"SkullMovementProfile [{skullName}]: moveSpeedMultiplier가 범위를 벗어났습니다.");

            if (jumpHeightMultiplier < 0.1f || jumpHeightMultiplier > 3.0f)
                Debug.LogWarning($"SkullMovementProfile [{skullName}]: jumpHeightMultiplier가 범위를 벗어났습니다.");

            if (airControlMultiplier < 0.1f || airControlMultiplier > 3.0f)
                Debug.LogWarning($"SkullMovementProfile [{skullName}]: airControlMultiplier가 범위를 벗어났습니다.");

            if (wallJumpHorizontalMultiplier < 0.1f || wallJumpHorizontalMultiplier > 3.0f)
                Debug.LogWarning($"SkullMovementProfile [{skullName}]: wallJumpHorizontalMultiplier가 범위를 벗어났습니다.");

            if (wallJumpVerticalMultiplier < 0.1f || wallJumpVerticalMultiplier > 3.0f)
                Debug.LogWarning($"SkullMovementProfile [{skullName}]: wallJumpVerticalMultiplier가 범위를 벗어났습니다.");
        }
    }
}
