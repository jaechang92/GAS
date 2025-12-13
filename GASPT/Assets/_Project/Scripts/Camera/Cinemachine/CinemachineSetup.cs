using UnityEngine;
using Unity.Cinemachine;

namespace GASPT.CameraSystem
{
    /// <summary>
    /// Cinemachine 초기 설정 및 관리
    /// Main Camera에 Brain이 없으면 자동 추가
    /// </summary>
    public class CinemachineSetup : MonoBehaviour
    {
        [Header("자동 설정")]
        [Tooltip("시작 시 Main Camera에 Brain 자동 추가")]
        [SerializeField] private bool autoSetupBrain = true;

        [Header("Brain 설정")]
        [SerializeField] private CinemachineBrain.UpdateMethods updateMethod = CinemachineBrain.UpdateMethods.SmartUpdate;
        [SerializeField] private CinemachineBlendDefinition defaultBlend = new CinemachineBlendDefinition(
            CinemachineBlendDefinition.Styles.EaseInOut, 0.5f);

        [Header("참조")]
        [SerializeField] private Camera mainCamera;
        [SerializeField] private CinemachineBrain brain;


        private void Awake()
        {
            if (autoSetupBrain)
            {
                SetupBrain();
            }
        }

        /// <summary>
        /// Main Camera에 Cinemachine Brain 설정
        /// </summary>
        public void SetupBrain()
        {
            // Main Camera 찾기 (CameraManager 우선, 없으면 Camera.main fallback)
            if (mainCamera == null)
            {
                mainCamera = CameraManager.Instance?.MainCamera ?? Camera.main;
            }

            if (mainCamera == null)
            {
                Debug.LogError("[CinemachineSetup] Main Camera를 찾을 수 없습니다!");
                return;
            }

            // Brain 확인/추가
            brain = mainCamera.GetComponent<CinemachineBrain>();
            if (brain == null)
            {
                brain = mainCamera.gameObject.AddComponent<CinemachineBrain>();
                Debug.Log("[CinemachineSetup] CinemachineBrain 추가됨");
            }

            // Brain 설정
            brain.UpdateMethod = updateMethod;
            brain.DefaultBlend = defaultBlend;

            Debug.Log($"[CinemachineSetup] Brain 설정 완료 - UpdateMethod: {updateMethod}");
        }

        /// <summary>
        /// Brain 참조 반환
        /// </summary>
        public CinemachineBrain GetBrain()
        {
            if (brain == null && mainCamera != null)
            {
                brain = mainCamera.GetComponent<CinemachineBrain>();
            }
            return brain;
        }

        /// <summary>
        /// 현재 활성화된 Virtual Camera 반환
        /// </summary>
        public ICinemachineCamera GetActiveVirtualCamera()
        {
            return brain?.ActiveVirtualCamera;
        }


#if UNITY_EDITOR
        [ContextMenu("Setup Brain Now")]
        private void EditorSetupBrain()
        {
            SetupBrain();
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}
