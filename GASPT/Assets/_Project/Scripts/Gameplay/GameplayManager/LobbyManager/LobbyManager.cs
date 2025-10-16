using UnityEngine;
using Combat.Core;
using Player;

namespace Gameplay
{
    /// <summary>
    /// Lobby 씬 관리
    /// 플레이어, 그라운드 초기화
    /// </summary>
    public class LobbyManager : MonoBehaviour
    {
        [Header("생성 설정")]
        [SerializeField] private bool autoSetup = true;
        [SerializeField] private bool createPlayer = true;
        [SerializeField] private bool createGround = true;

        [Header("스폰 위치")]
        [SerializeField] private Vector3 playerSpawnPosition = new Vector3(-8f, 1f, 0f);
        [SerializeField] private Vector3 groundPosition = new Vector3(0f, -1f, 0f);
        [SerializeField] private Vector3 groundScale = new Vector3(30f, 1f, 1f);

        private GameObject playerObject;
        private GameObject groundObject;
        private DamageSystem damageSystem;

        private void Start()
        {
            if (autoSetup)
            {
                Setup();
            }
        }

        /// <summary>
        /// Lobby 씬 초기화
        /// </summary>
        public void Setup()
        {
            Debug.Log("[LobbyManager] Lobby 씬 초기화 시작");

            // DamageSystem 초기화
            damageSystem = DamageSystem.Instance;
            if (damageSystem == null)
            {
                var dsObject = new GameObject("DamageSystem");
                damageSystem = dsObject.AddComponent<DamageSystem>();
            }

            // Ground 생성
            if (createGround)
            {
                CreateGround();
            }

            // Player 생성
            if (createPlayer)
            {
                CreatePlayer();
            }

            Debug.Log("[LobbyManager] Lobby 씬 초기화 완료");
        }

        /// <summary>
        /// 플레이어 생성
        /// </summary>
        private void CreatePlayer()
        {
            playerObject = new GameObject("Player");
            playerObject.transform.position = playerSpawnPosition;
            playerObject.layer = LayerMask.NameToLayer("Player");
            playerObject.tag = "Player"; // Player 태그 설정 (포탈 충돌 감지용)

            // SpriteRenderer
            var sr = playerObject.AddComponent<SpriteRenderer>();
            sr.sprite = CreateSimpleSprite(Color.cyan);

            // Rigidbody2D
            var rb = playerObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 3f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            // BoxCollider2D
            var col = playerObject.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.8f, 1.5f);

            // PlayerController 추가
            var controller = playerObject.AddComponent<PlayerController>();

            // HealthSystem 설정
            var health = playerObject.GetComponent<HealthSystem>();
            if (health != null)
            {
                health.SetMaxHealth(100f);
                health.SetCurrentHealth(100f);
            }

            Debug.Log("[LobbyManager] 플레이어 생성 완료");
        }

        /// <summary>
        /// Ground 생성
        /// </summary>
        private void CreateGround()
        {
            groundObject = new GameObject("Ground");
            groundObject.transform.position = groundPosition;
            groundObject.transform.localScale = groundScale;
            groundObject.layer = LayerMask.NameToLayer("Ground");

            // SpriteRenderer
            var sr = groundObject.AddComponent<SpriteRenderer>();
            sr.sprite = CreateSimpleSprite(Color.gray);

            // BoxCollider2D
            var col = groundObject.AddComponent<BoxCollider2D>();

            Debug.Log("[LobbyManager] Ground 생성 완료");
        }

        /// <summary>
        /// 간단한 Sprite 생성
        /// </summary>
        private Sprite CreateSimpleSprite(Color color)
        {
            int size = 64;
            Texture2D texture = new Texture2D(size, size);

            Color[] pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }
            texture.SetPixels(pixels);
            texture.Apply();

            return Sprite.Create(
                texture,
                new Rect(0, 0, size, size),
                new Vector2(0.5f, 0.5f),
                100f
            );
        }

        /// <summary>
        /// 플레이어 참조 반환
        /// </summary>
        public GameObject GetPlayer()
        {
            return playerObject;
        }
    }
}
