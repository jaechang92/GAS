using UnityEngine;

namespace GASPT.Form
{
    /// <summary>
    /// 투사체 기반 Ability의 기본 클래스
    /// 마우스 방향 계산 등 공통 로직 제공
    /// </summary>
    public abstract class BaseProjectileAbility : BaseAbility
    {
        // ====== 마우스 입력 처리 ======

        /// <summary>
        /// 마우스 위치 가져오기 (월드 좌표)
        /// </summary>
        /// <returns>마우스의 월드 좌표 (z = 0)</returns>
        protected Vector3 GetMousePosition()
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            return mousePos;
        }

        /// <summary>
        /// 캐스터에서 마우스 방향으로 향하는 정규화된 방향 벡터
        /// </summary>
        /// <param name="caster">시전자</param>
        /// <returns>정규화된 방향 벡터</returns>
        protected Vector2 GetMouseDirection(GameObject caster)
        {
            Vector3 mousePos = GetMousePosition();
            Vector2 direction = (mousePos - caster.transform.position).normalized;
            return direction;
        }

        /// <summary>
        /// 캐스터에서 마우스까지의 거리
        /// </summary>
        /// <param name="caster">시전자</param>
        /// <returns>거리</returns>
        protected float GetMouseDistance(GameObject caster)
        {
            Vector3 mousePos = GetMousePosition();
            return Vector3.Distance(caster.transform.position, mousePos);
        }


        // ====== 시작 위치 계산 ======

        /// <summary>
        /// 투사체 발사 시작 위치 계산 (캐스터 위치 + 오프셋)
        /// </summary>
        /// <param name="caster">시전자</param>
        /// <param name="offset">오프셋 (기본값: 약간 앞쪽)</param>
        /// <returns>발사 시작 위치</returns>
        protected Vector3 GetProjectileStartPosition(GameObject caster, Vector2? offset = null)
        {
            Vector2 actualOffset = offset ?? Vector2.right * 0.5f; // 기본 오프셋: 오른쪽 0.5m
            return caster.transform.position + (Vector3)actualOffset;
        }

        /// <summary>
        /// 마우스 방향으로 오프셋된 발사 시작 위치 계산
        /// </summary>
        /// <param name="caster">시전자</param>
        /// <param name="offsetDistance">방향 기준 오프셋 거리</param>
        /// <returns>발사 시작 위치</returns>
        protected Vector3 GetProjectileStartPositionTowardsMouse(GameObject caster, float offsetDistance = 0.5f)
        {
            Vector2 direction = GetMouseDirection(caster);
            return caster.transform.position + (Vector3)(direction * offsetDistance);
        }
    }
}
