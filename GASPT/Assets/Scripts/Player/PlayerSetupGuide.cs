using UnityEngine;
using GAS.Core;

namespace Player
{
    /// <summary>
    /// 플레이어 오브젝트 설정 가이드
    /// Unity 에디터에서 플레이어를 올바르게 설정하는 방법을 제공
    /// </summary>
    public class PlayerSetupGuide : MonoBehaviour
    {
        [Header("자동 설정")]
        [SerializeField] private bool autoSetupOnAwake = false;

        [Header("설정 확인")]
        [SerializeField] private bool validateSetup = true;

        private void Awake()
        {
            if (autoSetupOnAwake)
            {
                SetupPlayerComponents();
            }

            if (validateSetup)
            {
                ValidatePlayerSetup();
            }
        }

        /// <summary>
        /// 플레이어 컴포넌트 자동 설정
        /// </summary>
        [ContextMenu("플레이어 컴포넌트 자동 설정")]
        public void SetupPlayerComponents()
        {
            GameObject playerGO = gameObject;

            Debug.Log("[PlayerSetup] 플레이어 컴포넌트 자동 설정 시작");

            // 1. PlayerController 추가 (메인 컴포넌트)
            if (playerGO.GetComponent<PlayerController>() == null)
            {
                playerGO.AddComponent<PlayerController>();
                Debug.Log("- PlayerController 추가됨");
            }

            // 2. Rigidbody2D 추가
            Rigidbody2D rb = playerGO.GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                rb = playerGO.AddComponent<Rigidbody2D>();
                Debug.Log("- Rigidbody2D 추가됨");
            }

            // Rigidbody2D 설정
            rb.gravityScale = 3f;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            // 3. Collider2D 추가 (CapsuleCollider2D 권장)
            if (playerGO.GetComponent<Collider2D>() == null)
            {
                CapsuleCollider2D collider = playerGO.AddComponent<CapsuleCollider2D>();
                collider.size = new Vector2(0.8f, 1.8f);
                Debug.Log("- CapsuleCollider2D 추가됨");
            }

            // 4. SpriteRenderer 추가
            SpriteRenderer spriteRenderer = playerGO.GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = playerGO.AddComponent<SpriteRenderer>();
                Debug.Log("- SpriteRenderer 추가됨");
            }

            // 기본 스프라이트 설정
            if (spriteRenderer.sprite == null)
            {
                CreateBasicPlayerSprite(spriteRenderer);
            }

            // 5. AbilitySystem 추가 (GAS 연동)
            if (playerGO.GetComponent<AbilitySystem>() == null)
            {
                playerGO.AddComponent<AbilitySystem>();
                Debug.Log("- AbilitySystem 추가됨");
            }

            // 6. PlayerControllerTest 추가 (선택사항)
            if (playerGO.GetComponent<PlayerControllerTest>() == null)
            {
                playerGO.AddComponent<PlayerControllerTest>();
                Debug.Log("- PlayerControllerTest 추가됨 (테스트용)");
            }

            // 7. 태그 설정
            if (playerGO.tag != "Player")
            {
                playerGO.tag = "Player";
                Debug.Log("- Player 태그 설정됨");
            }

            Debug.Log("[PlayerSetup] 플레이어 컴포넌트 자동 설정 완료");
        }

        /// <summary>
        /// 플레이어 설정 검증
        /// </summary>
        [ContextMenu("플레이어 설정 검증")]
        public void ValidatePlayerSetup()
        {
            GameObject playerGO = gameObject;
            bool isValid = true;

            Debug.Log("[PlayerSetup] 플레이어 설정 검증 시작");

            // 필수 컴포넌트 확인
            var requiredComponents = new System.Type[]
            {
                typeof(PlayerController),
                typeof(Rigidbody2D),
                typeof(Collider2D),
                typeof(SpriteRenderer),
                typeof(AbilitySystem)
            };

            foreach (var componentType in requiredComponents)
            {
                var component = playerGO.GetComponent(componentType);
                if (component == null)
                {
                    Debug.LogError($"- 필수 컴포넌트 누락: {componentType.Name}");
                    isValid = false;
                }
                else
                {
                    Debug.Log($"✓ {componentType.Name} 확인됨");
                }
            }

            // 태그 확인
            if (playerGO.tag != "Player")
            {
                Debug.LogWarning("- Player 태그가 설정되지 않음");
                isValid = false;
            }
            else
            {
                Debug.Log("✓ Player 태그 확인됨");
            }

            // 레이어 확인 (권장사항)
            if (playerGO.layer == 0) // Default 레이어
            {
                Debug.LogWarning("- Player 전용 레이어 설정 권장 (충돌 관리를 위해)");
            }

            if (isValid)
            {
                Debug.Log("[PlayerSetup] ✅ 플레이어 설정이 올바릅니다!");
            }
            else
            {
                Debug.LogError("[PlayerSetup] ❌ 플레이어 설정에 문제가 있습니다. 자동 설정을 실행해주세요.");
            }
        }

        /// <summary>
        /// 기본 플레이어 스프라이트 생성
        /// </summary>
        private void CreateBasicPlayerSprite(SpriteRenderer spriteRenderer)
        {
            // 간단한 파란색 사각형 스프라이트 생성
            Texture2D texture = new Texture2D(80, 180);
            Color[] pixels = new Color[texture.width * texture.height];

            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Color.blue;
            }

            texture.SetPixels(pixels);
            texture.Apply();

            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                100f // pixels per unit
            );

            spriteRenderer.sprite = sprite;
            spriteRenderer.sortingOrder = 10; // 다른 오브젝트보다 앞에 표시

            Debug.Log("- 기본 플레이어 스프라이트 생성됨");
        }

        /// <summary>
        /// PlayerStats ScriptableObject 생성 도우미
        /// </summary>
        [ContextMenu("PlayerStats 생성 가이드")]
        public void CreatePlayerStatsGuide()
        {
            Debug.Log("=== PlayerStats 생성 방법 ===");
            Debug.Log("1. Project 윈도우에서 우클릭");
            Debug.Log("2. Create > GASPT > Player > Player Stats 선택");
            Debug.Log("3. 생성된 PlayerStats 에셋을 PlayerController의 Player Stats 필드에 할당");
            Debug.Log("4. Inspector에서 플레이어 능력치 조정");
        }

        /// <summary>
        /// 플레이어 설정 완성도 체크
        /// </summary>
        [ContextMenu("설정 완성도 체크")]
        public void CheckSetupCompleteness()
        {
            PlayerController controller = GetComponent<PlayerController>();
            if (controller == null)
            {
                Debug.LogError("PlayerController가 없습니다!");
                return;
            }

            Debug.Log("=== 플레이어 설정 완성도 ===");

            // PlayerStats 확인
            // Note: PlayerStats는 private이므로 직접 확인 불가, 실제 구현 시 public getter 추가 고려

            Debug.Log("✓ 기본 컴포넌트 설정 완료");
            Debug.Log("- PlayerStats 할당 여부는 Inspector에서 확인하세요");
            Debug.Log("- Ground Layer Mask 설정을 확인하세요");
            Debug.Log("- 테스트를 위해 Ground 오브젝트를 씬에 배치하세요");
        }
    }
}