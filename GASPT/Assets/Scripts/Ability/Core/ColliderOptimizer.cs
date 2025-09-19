// ���� ��ġ: Assets/Scripts/Helper/ColliderOptimizer.cs
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Helper
{
    /// <summary>
    /// BoxCollider2D�� �÷��� ������ �ڵ����� ����ȭ�ϴ� ��ƿ��Ƽ
    /// </summary>
    public class ColliderOptimizer : MonoBehaviour
    {
        [Header("Collider Settings")]
        [SerializeField] private bool autoOptimize = true;
        [SerializeField] private float edgeRadius = 0.02f;
        [SerializeField] private bool usedByComposite = false;

        [Header("Platform Settings")]
        [SerializeField] private bool isPlatform = false;
        [SerializeField] private bool isOneWayPlatform = false;
        [SerializeField] private bool usePlatformEffector = false;
        [SerializeField] private float surfaceArc = 180f;

        [Header("Tilemap Optimization")]
        [SerializeField] private bool optimizeTilemap = false;
        [SerializeField] private bool useCompositeCollider = true;

        private BoxCollider2D boxCollider;
        private CompositeCollider2D compositeCollider;
        private TilemapCollider2D tilemapCollider;
        private PlatformEffector2D platformEffector;

        private void Awake()
        {
            if (autoOptimize)
            {
                OptimizeCollider();
            }
        }

        [ContextMenu("Optimize Collider")]
        public void OptimizeCollider()
        {
            if (optimizeTilemap)
            {
                OptimizeTilemapCollider();
            }
            else
            {
                OptimizeBoxCollider();
            }

            if (isPlatform)
            {
                SetupPlatform();
            }
        }

        private void OptimizeBoxCollider()
        {
            boxCollider = GetComponent<BoxCollider2D>();
            if (boxCollider == null)
            {
                boxCollider = gameObject.AddComponent<BoxCollider2D>();
            }

            // Edge Radius ���� - �𼭸� �ɸ� ����
            boxCollider.edgeRadius = edgeRadius;
            boxCollider.usedByComposite = usedByComposite;

            // �ڵ� ũ�� ���� (Sprite ����)
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && spriteRenderer.sprite != null)
            {
                Vector2 spriteSize = spriteRenderer.sprite.bounds.size;
                boxCollider.size = spriteSize * 0.95f; // �ణ �۰� ����
            }

            Debug.Log($"[ColliderOptimizer] BoxCollider2D optimized on {gameObject.name}");
        }

        private void OptimizeTilemapCollider()
        {
            tilemapCollider = GetComponent<TilemapCollider2D>();
            if (tilemapCollider == null)
            {
                tilemapCollider = gameObject.AddComponent<TilemapCollider2D>();
            }

            if (useCompositeCollider)
            {
                // Composite Collider �߰�
                compositeCollider = GetComponent<CompositeCollider2D>();
                if (compositeCollider == null)
                {
                    compositeCollider = gameObject.AddComponent<CompositeCollider2D>();
                }

                // Rigidbody2D �ʿ� (Composite Collider �䱸����)
                Rigidbody2D rb = GetComponent<Rigidbody2D>();
                if (rb == null)
                {
                    rb = gameObject.AddComponent<Rigidbody2D>();
                }

                rb.bodyType = RigidbodyType2D.Static;

                // Tilemap Collider�� Composite�� ���
                tilemapCollider.usedByComposite = true;

                // Composite Collider ����
                compositeCollider.geometryType = CompositeCollider2D.GeometryType.Polygons;
                compositeCollider.generationType = CompositeCollider2D.GenerationType.Synchronous;
                compositeCollider.offsetDistance = 0.005f; // �ణ�� ���������� �ɸ� ����

                Debug.Log($"[ColliderOptimizer] Tilemap with Composite Collider optimized on {gameObject.name}");
            }
            else
            {
                tilemapCollider.usedByComposite = false;
                Debug.Log($"[ColliderOptimizer] Tilemap Collider optimized on {gameObject.name}");
            }
        }

        private void SetupPlatform()
        {
            if (usePlatformEffector)
            {
                platformEffector = GetComponent<PlatformEffector2D>();
                if (platformEffector == null)
                {
                    platformEffector = gameObject.AddComponent<PlatformEffector2D>();
                }

                // Platform Effector ����
                platformEffector.useOneWay = isOneWayPlatform;
                platformEffector.surfaceArc = surfaceArc;
                platformEffector.useOneWayGrouping = true;
                platformEffector.useSideBounce = false;
                platformEffector.useSideFriction = false;

                // Collider�� Effector�� ����ϵ��� ����
                if (boxCollider != null)
                {
                    boxCollider.usedByEffector = true;
                }
                else if (compositeCollider != null)
                {
                    compositeCollider.usedByEffector = true;
                }
                else if (tilemapCollider != null && !tilemapCollider.usedByComposite)
                {
                    tilemapCollider.usedByEffector = true;
                }

                Debug.Log($"[ColliderOptimizer] Platform Effector setup on {gameObject.name}");
            }

            // Layer ����
            if (isOneWayPlatform)
            {
                gameObject.layer = LayerMask.NameToLayer("OneWayPlatform");
            }
            else
            {
                gameObject.layer = LayerMask.NameToLayer("Ground");
            }
        }

        [ContextMenu("Create Layer Masks")]
        public void CreateRequiredLayers()
        {
            CreateLayer("Ground", 3);
            CreateLayer("OneWayPlatform", 8);
            CreateLayer("Player", 6);
            CreateLayer("Enemy", 7);
            CreateLayer("Wall", 9);

            Debug.Log("[ColliderOptimizer] Layer creation completed. Check Edit > Project Settings > Tags and Layers");
        }

        private void CreateLayer(string layerName, int suggestedIndex)
        {
            // ����: Unity���� ��Ÿ������ ���̾ ������ �� ����
            // �� �޼���� ���̵� �뵵
            Debug.Log($"Please create layer '{layerName}' at index {suggestedIndex} in Project Settings");
        }

        [ContextMenu("Analyze Collider Issues")]
        public void AnalyzeColliderIssues()
        {
            string report = $"=== Collider Analysis for {gameObject.name} ===\n";

            // BoxCollider2D üũ
            if (boxCollider != null)
            {
                report += $"BoxCollider2D:\n";
                report += $"  - Edge Radius: {boxCollider.edgeRadius} (Recommended: 0.02-0.05)\n";
                report += $"  - Size: {boxCollider.size}\n";
                report += $"  - Used by Effector: {boxCollider.usedByEffector}\n";

                if (boxCollider.edgeRadius < 0.01f)
                {
                    report += "  WARNING: Edge Radius too small - may cause sticking!\n";
                }
            }

            // Rigidbody2D üũ
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                report += $"\nRigidbody2D:\n";
                report += $"  - Body Type: {rb.bodyType}\n";
                report += $"  - Collision Detection: {rb.collisionDetectionMode}\n";
                report += $"  - Interpolate: {rb.interpolation}\n";

                if (rb.collisionDetectionMode != CollisionDetectionMode2D.Continuous)
                {
                    report += "  WARNING: Consider using Continuous collision detection for better physics\n";
                }
            }

            // Material üũ
            Collider2D col = GetComponent<Collider2D>();
            if (col != null && col.sharedMaterial == null)
            {
                report += "\nWARNING: No Physics Material 2D assigned - friction may cause sticking!\n";
            }

            Debug.Log(report);
        }

        private void OnValidate()
        {
            if (autoOptimize && Application.isPlaying)
            {
                OptimizeCollider();
            }
        }

        private void OnDrawGizmos()
        {
            if (boxCollider == null) return;

            // Edge Radius �ð�ȭ
            if (edgeRadius > 0)
            {
                Gizmos.color = Color.cyan;
                Vector3 pos = transform.position;
                Vector3 size = boxCollider.size;

                // �𼭸� �� �׸���
                float radius = edgeRadius;
                int segments = 8;

                for (int corner = 0; corner < 4; corner++)
                {
                    float xSign = (corner == 0 || corner == 3) ? 1 : -1;
                    float ySign = (corner < 2) ? 1 : -1;

                    Vector3 cornerPos = pos + new Vector3(
                        xSign * (size.x * 0.5f - radius),
                        ySign * (size.y * 0.5f - radius),
                        0
                    );

                    for (int i = 0; i < segments; i++)
                    {
                        float angle1 = (i / (float)segments) * 90f + (corner * 90f);
                        float angle2 = ((i + 1) / (float)segments) * 90f + (corner * 90f);

                        Vector3 p1 = cornerPos + new Vector3(
                            Mathf.Cos(angle1 * Mathf.Deg2Rad) * radius,
                            Mathf.Sin(angle1 * Mathf.Deg2Rad) * radius,
                            0
                        );

                        Vector3 p2 = cornerPos + new Vector3(
                            Mathf.Cos(angle2 * Mathf.Deg2Rad) * radius,
                            Mathf.Sin(angle2 * Mathf.Deg2Rad) * radius,
                            0
                        );

                        Gizmos.DrawLine(p1, p2);
                    }
                }
            }
        }
    }
}