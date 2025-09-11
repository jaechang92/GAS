// ===================================
// 파일: Assets/Scripts/Ability/Targeting/TargetIndicator.cs
// ===================================
using UnityEngine;

namespace AbilitySystem
{
    /// <summary>
    /// 타겟 위치 표시 인디케이터
    /// </summary>
    public class TargetIndicator : MonoBehaviour
    {
        [Header("인디케이터 설정")]
        [SerializeField] private GameObject singleTargetIndicator;
        [SerializeField] private GameObject areaIndicator;
        [SerializeField] private GameObject directionalIndicator;
        [SerializeField] private LineRenderer rangeLineRenderer;

        [Header("시각 효과")]
        [SerializeField] private Material validMaterial;
        [SerializeField] private Material invalidMaterial;
        [SerializeField] private float rotationSpeed = 50f;

        private TargetType currentType;
        private bool isValid;

        /// <summary>
        /// 인디케이터 초기화
        /// </summary>
        private void Awake()
        {
            // 인디케이터 오브젝트 설정
        }

        /// <summary>
        /// 인디케이터 표시
        /// </summary>
        public void Show(TargetType type, Vector3 position, float size = 1f)
        {
            // 타입에 맞는 인디케이터 활성화
        }

        /// <summary>
        /// 인디케이터 숨기기
        /// </summary>
        public void Hide()
        {
            // 모든 인디케이터 비활성화
        }

        /// <summary>
        /// 위치 업데이트
        /// </summary>
        public void UpdatePosition(Vector3 position)
        {
            // 인디케이터 위치 갱신
        }

        /// <summary>
        /// 방향 업데이트
        /// </summary>
        public void UpdateDirection(Vector3 direction)
        {
            // 방향성 인디케이터 회전
        }

        /// <summary>
        /// 유효성 표시 업데이트
        /// </summary>
        public void SetValidity(bool valid)
        {
            // 유효/무효 상태 시각화
        }

        /// <summary>
        /// 크기 조절
        /// </summary>
        public void SetSize(float radius)
        {
            // 범위 인디케이터 크기 설정
        }

        /// <summary>
        /// 애니메이션 업데이트
        /// </summary>
        private void Update()
        {
            // 인디케이터 회전 애니메이션
        }

        /// <summary>
        /// 범위 원 그리기
        /// </summary>
        public void DrawRangeCircle(Vector3 center, float radius)
        {
            // LineRenderer로 원형 범위 표시
        }
    }
}