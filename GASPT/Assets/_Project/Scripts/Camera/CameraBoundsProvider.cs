using UnityEngine;

namespace GASPT.CameraSystem
{
    /// <summary>
    /// 카메라 경계 제공자
    /// Content Scene의 특정 영역(Room, Zone)에 배치하여 카메라 경계를 정의
    /// Background SpriteRenderer 또는 Collider2D에서 자동으로 경계 계산 가능
    /// </summary>
    public class CameraBoundsProvider : MonoBehaviour
    {
        // ====== 경계 소스 타입 ======

        public enum BoundsSourceType
        {
            Manual,           // 수동 설정
            Collider2D,       // Collider2D에서 계산
            SpriteRenderer,   // SpriteRenderer에서 계산 (Background용)
            CompositeSprites  // 여러 SpriteRenderer 합산
        }


        // ====== 경계 설정 ======

        [Header("경계 소스")]
        [Tooltip("경계 계산 방식")]
        [SerializeField] private BoundsSourceType boundsSource = BoundsSourceType.Manual;

        [Header("경계 설정")]
        [Tooltip("카메라 경계 (Manual 모드에서 사용)")]
        [SerializeField] private CameraBounds bounds = new CameraBounds(-20f, -10f, 20f, 10f);

        [Tooltip("경계 패딩 (모든 방향에 추가)")]
        [SerializeField] private float boundsPadding = 0f;

        [Header("소스 참조")]
        [Tooltip("경계 자동 계산에 사용할 Collider (null이면 자신의 Collider 사용)")]
        [SerializeField] private Collider2D boundsCollider;

        [Tooltip("경계 자동 계산에 사용할 SpriteRenderer (Background)")]
        [SerializeField] private SpriteRenderer backgroundSprite;

        [Tooltip("여러 SpriteRenderer를 합산 (CompositeSprites 모드)")]
        [SerializeField] private SpriteRenderer[] compositeSprites;

        // 레거시 호환성
        [HideInInspector]
        [SerializeField] private bool useColliderBounds = false;


        // ====== 자동 등록 ======

        [Header("자동 등록")]
        [Tooltip("활성화 시 CameraManager에 자동 등록")]
        [SerializeField] private bool autoRegisterOnEnable = true;

        [Tooltip("우선순위 (높을수록 우선)")]
        [SerializeField] private int priority = 0;


        // ====== Gizmo ======

        [Header("Gizmo")]
        [Tooltip("Gizmo 표시 여부")]
        [SerializeField] private bool showGizmos = true;

        [Tooltip("Gizmo 색상")]
        [SerializeField] private Color gizmoColor = new Color(0f, 1f, 1f, 0.5f);


        // ====== 프로퍼티 ======

        /// <summary>
        /// 현재 경계
        /// </summary>
        public CameraBounds Bounds => bounds;

        /// <summary>
        /// 우선순위
        /// </summary>
        public int Priority => priority;


        // ====== Unity 생명주기 ======

        private void Awake()
        {
            // 레거시 호환성: useColliderBounds가 true면 Collider2D 모드로 변환
            if (useColliderBounds && boundsSource == BoundsSourceType.Manual)
            {
                boundsSource = BoundsSourceType.Collider2D;
            }

            // 경계 자동 계산
            CalculateBounds();
        }

        private void OnValidate()
        {
            // 에디터에서 값 변경 시 자동 계산
            if (!Application.isPlaying)
            {
                CalculateBounds();
            }
        }

        private void OnEnable()
        {
            if (autoRegisterOnEnable)
            {
                RegisterToCameraManager();
            }
        }

        private void OnDisable()
        {
            if (autoRegisterOnEnable)
            {
                UnregisterFromCameraManager();
            }
        }


        // ====== 등록/해제 ======

        /// <summary>
        /// CameraManager에 등록
        /// </summary>
        public void RegisterToCameraManager()
        {
            if (CameraManager.HasInstance)
            {
                CameraManager.Instance.RegisterBoundsProvider(this);
                Debug.Log($"[CameraBoundsProvider] {name} 등록 완료 (Priority: {priority})");
            }
        }

        /// <summary>
        /// CameraManager에서 해제
        /// </summary>
        public void UnregisterFromCameraManager()
        {
            if (CameraManager.HasInstance)
            {
                CameraManager.Instance.UnregisterBoundsProvider(this);
                Debug.Log($"[CameraBoundsProvider] {name} 해제 완료");
            }
        }


        // ====== 경계 계산 ======

        /// <summary>
        /// 설정된 소스에서 경계 계산
        /// </summary>
        public void CalculateBounds()
        {
            switch (boundsSource)
            {
                case BoundsSourceType.Manual:
                    // 수동 설정은 그대로 사용
                    break;

                case BoundsSourceType.Collider2D:
                    CalculateBoundsFromCollider();
                    break;

                case BoundsSourceType.SpriteRenderer:
                    CalculateBoundsFromSprite();
                    break;

                case BoundsSourceType.CompositeSprites:
                    CalculateBoundsFromCompositeSprites();
                    break;
            }

            // 패딩 적용
            if (boundsPadding != 0f)
            {
                bounds = bounds.Expand(-boundsPadding); // 음수로 확장 = 축소
            }
        }

        /// <summary>
        /// Collider2D에서 경계 자동 계산
        /// </summary>
        public void CalculateBoundsFromCollider()
        {
            Collider2D col = boundsCollider != null ? boundsCollider : GetComponent<Collider2D>();

            if (col == null)
            {
                Debug.LogWarning($"[CameraBoundsProvider] {name}: Collider2D를 찾을 수 없습니다!");
                return;
            }

            UnityEngine.Bounds unityBounds = col.bounds;
            bounds = new CameraBounds(
                new Vector2(unityBounds.min.x, unityBounds.min.y),
                new Vector2(unityBounds.max.x, unityBounds.max.y)
            );

            Debug.Log($"[CameraBoundsProvider] {name}: Collider에서 경계 계산 완료 - {bounds}");
        }

        /// <summary>
        /// SpriteRenderer에서 경계 자동 계산 (Background용)
        /// </summary>
        public void CalculateBoundsFromSprite()
        {
            SpriteRenderer sr = backgroundSprite != null ? backgroundSprite : GetComponent<SpriteRenderer>();

            if (sr == null)
            {
                Debug.LogWarning($"[CameraBoundsProvider] {name}: SpriteRenderer를 찾을 수 없습니다!");
                return;
            }

            if (sr.sprite == null)
            {
                Debug.LogWarning($"[CameraBoundsProvider] {name}: Sprite가 없습니다!");
                return;
            }

            UnityEngine.Bounds spriteBounds = sr.bounds;
            bounds = new CameraBounds(
                new Vector2(spriteBounds.min.x, spriteBounds.min.y),
                new Vector2(spriteBounds.max.x, spriteBounds.max.y)
            );

            Debug.Log($"[CameraBoundsProvider] {name}: SpriteRenderer에서 경계 계산 완료 - {bounds}");
        }

        /// <summary>
        /// 여러 SpriteRenderer를 합산하여 경계 계산
        /// </summary>
        public void CalculateBoundsFromCompositeSprites()
        {
            if (compositeSprites == null || compositeSprites.Length == 0)
            {
                Debug.LogWarning($"[CameraBoundsProvider] {name}: compositeSprites가 비어있습니다!");
                return;
            }

            Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 max = new Vector2(float.MinValue, float.MinValue);

            int validCount = 0;
            foreach (var sr in compositeSprites)
            {
                if (sr == null || sr.sprite == null) continue;

                UnityEngine.Bounds spriteBounds = sr.bounds;
                min.x = Mathf.Min(min.x, spriteBounds.min.x);
                min.y = Mathf.Min(min.y, spriteBounds.min.y);
                max.x = Mathf.Max(max.x, spriteBounds.max.x);
                max.y = Mathf.Max(max.y, spriteBounds.max.y);
                validCount++;
            }

            if (validCount == 0)
            {
                Debug.LogWarning($"[CameraBoundsProvider] {name}: 유효한 SpriteRenderer가 없습니다!");
                return;
            }

            bounds = new CameraBounds(min, max);
            Debug.Log($"[CameraBoundsProvider] {name}: {validCount}개 Sprite에서 경계 계산 완료 - {bounds}");
        }

        /// <summary>
        /// Background 오브젝트 자동 탐색 및 설정
        /// </summary>
        public void AutoFindBackground()
        {
            // Background 태그로 찾기
            GameObject bgObj = GameObject.FindGameObjectWithTag("Background");
            if (bgObj != null)
            {
                backgroundSprite = bgObj.GetComponent<SpriteRenderer>();
                if (backgroundSprite != null)
                {
                    boundsSource = BoundsSourceType.SpriteRenderer;
                    CalculateBounds();
                    Debug.Log($"[CameraBoundsProvider] Background 자동 탐색 완료: {bgObj.name}");
                    return;
                }
            }

            // 이름으로 찾기
            GameObject[] candidates = {
                GameObject.Find("Background"),
                GameObject.Find("BG"),
                GameObject.Find("background")
            };

            foreach (var candidate in candidates)
            {
                if (candidate != null)
                {
                    backgroundSprite = candidate.GetComponent<SpriteRenderer>();
                    if (backgroundSprite != null)
                    {
                        boundsSource = BoundsSourceType.SpriteRenderer;
                        CalculateBounds();
                        Debug.Log($"[CameraBoundsProvider] Background 자동 탐색 완료: {candidate.name}");
                        return;
                    }
                }
            }

            Debug.LogWarning("[CameraBoundsProvider] Background를 찾을 수 없습니다!");
        }

        /// <summary>
        /// 경계 수동 설정
        /// </summary>
        public void SetBounds(CameraBounds newBounds)
        {
            bounds = newBounds;
        }

        /// <summary>
        /// 경계 수동 설정 (개별 값)
        /// </summary>
        public void SetBounds(float minX, float minY, float maxX, float maxY)
        {
            bounds = new CameraBounds(minX, minY, maxX, maxY);
        }


        // ====== Gizmos ======

        private void OnDrawGizmos()
        {
            if (!showGizmos) return;

            DrawBoundsGizmo(false);
        }

        private void OnDrawGizmosSelected()
        {
            DrawBoundsGizmo(true);
        }

        private void DrawBoundsGizmo(bool selected)
        {
            CameraBounds drawBounds = bounds;

            // 에디터 모드에서 실시간 업데이트
            if (!Application.isPlaying)
            {
                switch (boundsSource)
                {
                    case BoundsSourceType.Collider2D:
                        Collider2D col = boundsCollider != null ? boundsCollider : GetComponent<Collider2D>();
                        if (col != null)
                        {
                            UnityEngine.Bounds unityBounds = col.bounds;
                            drawBounds = new CameraBounds(
                                new Vector2(unityBounds.min.x, unityBounds.min.y),
                                new Vector2(unityBounds.max.x, unityBounds.max.y)
                            );
                        }
                        break;

                    case BoundsSourceType.SpriteRenderer:
                        SpriteRenderer sr = backgroundSprite != null ? backgroundSprite : GetComponent<SpriteRenderer>();
                        if (sr != null && sr.sprite != null)
                        {
                            UnityEngine.Bounds spriteBounds = sr.bounds;
                            drawBounds = new CameraBounds(
                                new Vector2(spriteBounds.min.x, spriteBounds.min.y),
                                new Vector2(spriteBounds.max.x, spriteBounds.max.y)
                            );
                        }
                        break;
                }

                // 패딩 적용
                if (boundsPadding != 0f)
                {
                    drawBounds = drawBounds.Expand(-boundsPadding);
                }
            }

            Vector3 center = new Vector3(drawBounds.Center.x, drawBounds.Center.y, 0f);
            Vector3 size = new Vector3(drawBounds.Size.x, drawBounds.Size.y, 0f);

            // 외곽선
            Gizmos.color = gizmoColor;
            Gizmos.DrawWireCube(center, size);

            // 선택 시 반투명 채움
            if (selected)
            {
                Color fillColor = gizmoColor;
                fillColor.a *= 0.3f;
                Gizmos.color = fillColor;
                Gizmos.DrawCube(center, size);

#if UNITY_EDITOR
                // 정보 표시
                string info = $"Camera Bounds: {name}\n" +
                             $"Min: ({drawBounds.min.x:F1}, {drawBounds.min.y:F1})\n" +
                             $"Max: ({drawBounds.max.x:F1}, {drawBounds.max.y:F1})\n" +
                             $"Priority: {priority}";
                UnityEditor.Handles.Label(center + Vector3.up * (size.y / 2f + 1f), info);
#endif
            }

            // 코너 표시
            float cornerSize = 0.5f;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(new Vector3(drawBounds.min.x, drawBounds.min.y, 0), cornerSize);
            Gizmos.DrawWireSphere(new Vector3(drawBounds.max.x, drawBounds.min.y, 0), cornerSize);
            Gizmos.DrawWireSphere(new Vector3(drawBounds.min.x, drawBounds.max.y, 0), cornerSize);
            Gizmos.DrawWireSphere(new Vector3(drawBounds.max.x, drawBounds.max.y, 0), cornerSize);
        }


        // ====== 에디터 유틸리티 ======

#if UNITY_EDITOR
        [ContextMenu("🔄 Recalculate Bounds")]
        private void EditorRecalculateBounds()
        {
            CalculateBounds();
            UnityEditor.EditorUtility.SetDirty(this);
        }

        [ContextMenu("🔍 Auto Find Background")]
        private void EditorAutoFindBackground()
        {
            AutoFindBackground();
            UnityEditor.EditorUtility.SetDirty(this);
        }

        [ContextMenu("Calculate Bounds From Collider")]
        private void EditorCalculateBoundsFromCollider()
        {
            boundsSource = BoundsSourceType.Collider2D;
            CalculateBoundsFromCollider();
            UnityEditor.EditorUtility.SetDirty(this);
        }

        [ContextMenu("Calculate Bounds From SpriteRenderer")]
        private void EditorCalculateBoundsFromSprite()
        {
            boundsSource = BoundsSourceType.SpriteRenderer;
            CalculateBoundsFromSprite();
            UnityEditor.EditorUtility.SetDirty(this);
        }

        [ContextMenu("Set Bounds From Scene View")]
        private void EditorSetBoundsFromSceneView()
        {
            var sceneView = UnityEditor.SceneView.lastActiveSceneView;
            if (sceneView != null && sceneView.camera != null)
            {
                var cam = sceneView.camera;
                float height = cam.orthographicSize * 2f;
                float width = height * cam.aspect;
                Vector3 center = sceneView.pivot;

                boundsSource = BoundsSourceType.Manual;
                bounds = new CameraBounds(
                    center.x - width / 2f,
                    center.y - height / 2f,
                    center.x + width / 2f,
                    center.y + height / 2f
                );

                UnityEditor.EditorUtility.SetDirty(this);
                Debug.Log($"[CameraBoundsProvider] Scene View에서 경계 설정: {bounds}");
            }
        }

        [ContextMenu("Print Bounds Info")]
        private void PrintBoundsInfo()
        {
            Debug.Log($"[CameraBoundsProvider] {name}\n" +
                     $"Source: {boundsSource}\n" +
                     $"Bounds: {bounds}\n" +
                     $"Center: {bounds.Center}\n" +
                     $"Size: {bounds.Size}\n" +
                     $"Padding: {boundsPadding}\n" +
                     $"Priority: {priority}\n" +
                     $"Auto Register: {autoRegisterOnEnable}\n" +
                     $"Background Sprite: {(backgroundSprite != null ? backgroundSprite.name : "null")}");
        }
#endif
    }
}
