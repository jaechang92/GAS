using UnityEngine;
using Unity.Cinemachine;

namespace GASPT.CameraSystem
{
    /// <summary>
    /// Cinemachine Impulse (화면 흔들림) 헬퍼
    /// 게임 이벤트에 쉽게 흔들림 효과를 추가할 수 있는 유틸리티
    /// </summary>
    public class CinemachineImpulseHelper : MonoBehaviour
    {
        // ====== 싱글톤 ======

        private static CinemachineImpulseHelper instance;
        public static CinemachineImpulseHelper Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindAnyObjectByType<CinemachineImpulseHelper>();
                }
                return instance;
            }
        }


        // ====== Impulse Source ======

        [Header("Impulse Sources")]
        [Tooltip("기본 흔들림 소스")]
        [SerializeField] private CinemachineImpulseSource defaultSource;

        [Tooltip("강한 흔들림 소스 (보스, 대형 폭발 등)")]
        [SerializeField] private CinemachineImpulseSource heavySource;

        [Tooltip("방향성 흔들림 소스 (피격, 넉백 등)")]
        [SerializeField] private CinemachineImpulseSource directionalSource;


        // ====== 프리셋 강도 ======

        [Header("프리셋 강도")]
        [SerializeField] private float lightIntensity = 0.1f;
        [SerializeField] private float mediumIntensity = 0.3f;
        [SerializeField] private float heavyIntensity = 0.5f;
        [SerializeField] private float explosionIntensity = 1f;
        [SerializeField] private float bossIntensity = 2f;


        // ====== Unity 생명주기 ======

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else if (instance != this)
            {
                Destroy(gameObject);
                return;
            }

            // 기본 소스 자동 설정
            if (defaultSource == null)
            {
                defaultSource = GetComponent<CinemachineImpulseSource>();
                if (defaultSource == null)
                {
                    defaultSource = gameObject.AddComponent<CinemachineImpulseSource>();
                }
            }
        }


        // ====== 기본 흔들림 ======

        /// <summary>
        /// 기본 흔들림
        /// </summary>
        public void Shake(float intensity = 1f)
        {
            if (defaultSource != null)
            {
                defaultSource.GenerateImpulse(intensity);
            }
        }

        /// <summary>
        /// 약한 흔들림 (착지, 작은 충돌)
        /// </summary>
        public void ShakeLight()
        {
            Shake(lightIntensity);
        }

        /// <summary>
        /// 중간 흔들림 (일반 피격)
        /// </summary>
        public void ShakeMedium()
        {
            Shake(mediumIntensity);
        }

        /// <summary>
        /// 강한 흔들림 (강한 공격, 폭발)
        /// </summary>
        public void ShakeHeavy()
        {
            if (heavySource != null)
            {
                heavySource.GenerateImpulse(heavyIntensity);
            }
            else
            {
                Shake(heavyIntensity);
            }
        }

        /// <summary>
        /// 폭발 흔들림
        /// </summary>
        public void ShakeExplosion()
        {
            if (heavySource != null)
            {
                heavySource.GenerateImpulse(explosionIntensity);
            }
            else
            {
                Shake(explosionIntensity);
            }
        }

        /// <summary>
        /// 보스 흔들림 (보스 등장, 강력한 공격)
        /// </summary>
        public void ShakeBoss()
        {
            if (heavySource != null)
            {
                heavySource.GenerateImpulse(bossIntensity);
            }
            else
            {
                Shake(bossIntensity);
            }
        }


        // ====== 방향성 흔들림 ======

        /// <summary>
        /// 방향성 흔들림 (피격 방향)
        /// </summary>
        public void ShakeDirectional(Vector2 direction, float intensity = 1f)
        {
            CinemachineImpulseSource source = directionalSource ?? defaultSource;
            if (source != null)
            {
                source.GenerateImpulse(new Vector3(direction.x, direction.y, 0) * intensity);
            }
        }

        /// <summary>
        /// 피격 흔들림 (피격 위치에서 반대 방향)
        /// </summary>
        public void ShakeFromDamage(Vector2 damageSource, Vector2 targetPosition, float damageAmount)
        {
            Vector2 direction = (targetPosition - damageSource).normalized;
            float intensity = Mathf.Clamp(damageAmount / 50f, lightIntensity, mediumIntensity);
            ShakeDirectional(direction, intensity);
        }


        // ====== 위치 기반 흔들림 ======

        /// <summary>
        /// 특정 위치에서 발생하는 흔들림 (거리에 따라 감쇠)
        /// </summary>
        public void ShakeAtPosition(Vector2 position, float intensity = 1f, float maxDistance = 20f)
        {
            // CameraManager에서 카메라 가져오기
            Camera cam = CameraManager.Instance?.MainCamera;
            if (cam == null) return;

            float distance = Vector2.Distance(position, cam.transform.position);
            if (distance > maxDistance) return;

            // 거리에 따른 감쇠
            float falloff = 1f - (distance / maxDistance);
            float finalIntensity = intensity * falloff;

            Shake(finalIntensity);
        }

        /// <summary>
        /// 폭발 효과 (위치 기반 + 강한 흔들림)
        /// </summary>
        public void ShakeExplosionAt(Vector2 position, float radius = 15f)
        {
            ShakeAtPosition(position, explosionIntensity, radius);
        }


        // ====== 커스텀 흔들림 ======

        /// <summary>
        /// 커스텀 벡터 흔들림
        /// </summary>
        public void ShakeCustom(Vector3 velocity)
        {
            if (defaultSource != null)
            {
                defaultSource.GenerateImpulse(velocity);
            }
        }


        // ====== 에디터 유틸리티 ======

#if UNITY_EDITOR
        [ContextMenu("Test Light Shake")]
        private void TestLightShake() => ShakeLight();

        [ContextMenu("Test Medium Shake")]
        private void TestMediumShake() => ShakeMedium();

        [ContextMenu("Test Heavy Shake")]
        private void TestHeavyShake() => ShakeHeavy();

        [ContextMenu("Test Explosion Shake")]
        private void TestExplosionShake() => ShakeExplosion();

        [ContextMenu("Test Boss Shake")]
        private void TestBossShake() => ShakeBoss();

        [ContextMenu("Setup Default Impulse Sources")]
        private void SetupDefaultSources()
        {
            // Default Source
            if (defaultSource == null)
            {
                defaultSource = gameObject.AddComponent<CinemachineImpulseSource>();
            }

            // Heavy Source (별도 GameObject)
            var heavyObj = new GameObject("HeavyImpulseSource");
            heavyObj.transform.SetParent(transform);
            heavySource = heavyObj.AddComponent<CinemachineImpulseSource>();

            // Directional Source (별도 GameObject)
            var dirObj = new GameObject("DirectionalImpulseSource");
            dirObj.transform.SetParent(transform);
            directionalSource = dirObj.AddComponent<CinemachineImpulseSource>();

            UnityEditor.EditorUtility.SetDirty(this);
            Debug.Log("[CinemachineImpulseHelper] Impulse Sources 설정 완료");
        }
#endif
    }
}
