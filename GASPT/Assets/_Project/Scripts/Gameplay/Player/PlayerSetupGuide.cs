using UnityEngine;
using GAS.Core;
using Player.Physics;

namespace Player
{
    /// <summary>
    /// 플레이어 오브젝트 설정 가이드
    /// Skul 스타일 Rigidbody2D 기반 물리 시스템을 사용하는 플레이어 설정 방법을 제공
    /// CharacterPhysics + SkulPhysicsConfig 기반 시스템 사용
    /// Unity Physics2D 엔진을 활용한 즉시 반응형 플랫폼 액션 시스템
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
        /// 플레이어 컴포넌트 자동 설정 (Skul 스타일 물리 시스템)
        /// </summary>
        [ContextMenu("플레이어 컴포넌트 자동 설정")]
        public void SetupPlayerComponents()
        {
            GameObject playerGO = gameObject;

            if (playerGO == null)
            {
                Debug.LogError("[PlayerSetup] GameObject가 null입니다!");
                return;
            }

            Debug.Log("[PlayerSetup] Skul 스타일 플레이어 컴포넌트 자동 설정 시작");

            // 1. Rigidbody2D 추가 (CharacterPhysics 의존성)
            Rigidbody2D rigidbody = playerGO.GetComponent<Rigidbody2D>();
            if (rigidbody == null)
            {
                try
                {
                    rigidbody = playerGO.AddComponent<Rigidbody2D>();
                    if (rigidbody == null)
                    {
                        Debug.LogError("[PlayerSetup] Rigidbody2D 추가 실패 - Skul 물리 시스템이 작동하지 않습니다!");
                        return;
                    }
                    Debug.Log("- Rigidbody2D 추가됨 (Unity Physics2D 기반)");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[PlayerSetup] Rigidbody2D 추가 실패: {e.Message}");
                    return;
                }
            }

            // Rigidbody2D 설정 (Skul 스타일)
            rigidbody.gravityScale = 0f; // CharacterPhysics가 중력 제어
            rigidbody.freezeRotation = true;
            rigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            // 2. BoxCollider2D 추가 (필수 - 충돌 감지용)
            BoxCollider2D boxCollider = playerGO.GetComponent<BoxCollider2D>();
            if (boxCollider == null)
            {
                try
                {
                    boxCollider = playerGO.AddComponent<BoxCollider2D>();
                    if (boxCollider == null)
                    {
                        Debug.LogError("[PlayerSetup] BoxCollider2D 추가 실패!");
                        return;
                    }
                    Debug.Log("- BoxCollider2D 추가됨 (충돌 감지용)");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[PlayerSetup] BoxCollider2D 추가 실패: {e.Message}");
                    return;
                }
            }

            // BoxCollider2D 설정 (Skul 스타일)
            boxCollider.size = new Vector2(0.8f, 0.9f);
            boxCollider.offset = new Vector2(0, -0.05f);
            boxCollider.isTrigger = false;

            // 3. CharacterPhysics 추가 (새로운 Skul 스타일 물리 시스템)
            if (playerGO.GetComponent<CharacterPhysics>() == null)
            {
                playerGO.AddComponent<CharacterPhysics>();
                Debug.Log("- CharacterPhysics 추가됨 (Skul 스타일 통합 물리 시스템)");
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

            // 5. AbilitySystem 추가 (PlayerController 의존성)
            if (playerGO.GetComponent<AbilitySystem>() == null)
            {
                playerGO.AddComponent<AbilitySystem>();
                Debug.Log("- AbilitySystem 추가됨");
            }

            // 6. InputHandler 추가
            if (playerGO.GetComponent<InputHandler>() == null)
            {
                playerGO.AddComponent<InputHandler>();
                Debug.Log("- InputHandler 추가됨");
            }

            // 7. AnimationController 추가
            if (playerGO.GetComponent<AnimationController>() == null)
            {
                playerGO.AddComponent<AnimationController>();
                Debug.Log("- AnimationController 추가됨");
            }

            // 8. PlayerController 추가 (모든 의존성 해결 후 마지막에 추가)
            if (playerGO.GetComponent<PlayerController>() == null)
            {
                playerGO.AddComponent<PlayerController>();
                Debug.Log("- PlayerController 추가됨");
            }

            // 9. SkulPhysicsTestRunner 추가 (테스트용)
            if (playerGO.GetComponent<SkulPhysicsTestRunner>() == null)
            {
                playerGO.AddComponent<SkulPhysicsTestRunner>();
                Debug.Log("- SkulPhysicsTestRunner 추가됨 (테스트용)");
            }

            // 10. 태그 설정
            if (playerGO.tag != "Player")
            {
                playerGO.tag = "Player";
                Debug.Log("- Player 태그 설정됨");
            }

            Debug.Log("[PlayerSetup] Skul 스타일 플레이어 컴포넌트 자동 설정 완료!");
            Debug.Log("⚠️ SkulPhysicsConfig ScriptableObject 할당이 필요합니다! (CharacterPhysics에)");
        }

        /// <summary>
        /// 플레이어 설정 검증 (Skul 스타일 물리 시스템)
        /// </summary>
        [ContextMenu("플레이어 설정 검증")]
        public void ValidatePlayerSetup()
        {
            GameObject playerGO = gameObject;
            bool isValid = true;

            Debug.Log("[PlayerSetup] Skul 스타일 플레이어 설정 검증 시작");

            // 필수 컴포넌트 확인 (Skul 스타일 물리 시스템)
            var requiredComponents = new System.Type[]
            {
                typeof(PlayerController),
                typeof(CharacterPhysics),       // 새로운 Skul 스타일 물리 엔진
                typeof(Rigidbody2D),           // Unity Physics2D 기반
                typeof(BoxCollider2D),         // 충돌 감지용
                typeof(SpriteRenderer),
                typeof(AbilitySystem),
                typeof(InputHandler),
                typeof(AnimationController)
            };

            // 구 시스템 컴포넌트 (제거되어야 함)
            var deprecatedComponents = new System.Type[]
            {
                typeof(CharacterController),
                typeof(Rigidbody),             // 3D Rigidbody
                typeof(CapsuleCollider2D)     // Skul 스타일은 BoxCollider2D만 사용
            };

            foreach (var componentType in requiredComponents)
            {
                var component = playerGO.GetComponent(componentType);
                if (component == null)
                {
                    Debug.LogError($"❌ 필수 컴포넌트 누락: {componentType.Name}");
                    isValid = false;
                }
                else
                {
                    Debug.Log($"✅ {componentType.Name} 확인됨");
                }
            }

            // 구 시스템 컴포넌트 확인
            foreach (var componentType in deprecatedComponents)
            {
                var component = playerGO.GetComponent(componentType);
                if (component != null)
                {
                    Debug.LogError($"❌ {componentType.Name} 발견됨 - 구 시스템 컴포넌트입니다. 제거 필요!");
                    isValid = false;
                }
                else
                {
                    Debug.Log($"✅ {componentType.Name} 없음 (정상)");
                }
            }

            // Rigidbody2D 설정 검증
            var rigidbody = playerGO.GetComponent<Rigidbody2D>();
            if (rigidbody != null)
            {
                if (rigidbody.gravityScale != 0f)
                {
                    Debug.LogWarning("⚠️ Rigidbody2D gravityScale이 0이 아닙니다. CharacterPhysics가 중력을 제어하므로 0으로 설정해야 합니다.");
                    isValid = false;
                }
                else
                {
                    Debug.Log("✅ Rigidbody2D gravityScale 설정 정상 (0)");
                }

                if (!rigidbody.freezeRotation)
                {
                    Debug.LogWarning("⚠️ Rigidbody2D freezeRotation이 비활성화되어 있습니다. 활성화를 권장합니다.");
                }
                else
                {
                    Debug.Log("✅ Rigidbody2D freezeRotation 설정 정상");
                }
            }

            // BoxCollider2D 추가 검증
            var boxCollider = playerGO.GetComponent<BoxCollider2D>();
            if (boxCollider != null)
            {
                if (boxCollider.isTrigger)
                {
                    Debug.LogWarning("⚠️ BoxCollider2D가 Trigger로 설정됨 - 물리 충돌용이므로 Trigger를 해제해야 합니다.");
                    isValid = false;
                }
                else
                {
                    Debug.Log("✅ BoxCollider2D Trigger 설정 정상 (물리 충돌용)");
                }
            }

            // CharacterPhysics 설정 확인
            var characterPhysics = playerGO.GetComponent<CharacterPhysics>();
            if (characterPhysics != null)
            {
                // Reflection을 통해 private config 필드 확인
                var configField = typeof(CharacterPhysics).GetField("config",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (configField != null)
                {
                    var config = configField.GetValue(characterPhysics) as SkulPhysicsConfig;
                    if (config == null)
                    {
                        Debug.LogError("❌ CharacterPhysics에 SkulPhysicsConfig가 할당되지 않았습니다!");
                        isValid = false;
                    }
                    else
                    {
                        Debug.Log($"✅ SkulPhysicsConfig 할당됨");
                        Debug.Log($"  - 이동 속도: {config.moveSpeed}");
                        Debug.Log($"  - 점프 속도: {config.jumpVelocity}");
                        Debug.Log($"  - 대시 속도: {config.dashSpeed}");
                    }
                }
            }

            // 태그 확인
            if (playerGO.tag != "Player")
            {
                Debug.LogWarning("⚠️ Player 태그가 설정되지 않음");
                isValid = false;
            }
            else
            {
                Debug.Log("✅ Player 태그 확인됨");
            }

            // 레이어 확인 (권장사항)
            if (playerGO.layer == 0) // Default 레이어
            {
                Debug.LogWarning("⚠️ Player 전용 레이어 설정 권장 (충돌 관리를 위해)");
            }

            // 테스트 컴포넌트 확인
            var testRunner = playerGO.GetComponent<SkulPhysicsTestRunner>();
            if (testRunner != null)
            {
                Debug.Log("✅ SkulPhysicsTestRunner 확인됨 (테스트 기능 사용 가능)");
            }

            if (isValid)
            {
                Debug.Log("[PlayerSetup] 🎯 Skul 스타일 플레이어 설정이 올바릅니다!");
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
            // Skul 스타일의 파란색 사각형 스프라이트 생성
            Texture2D texture = new Texture2D(64, 128);
            Color[] pixels = new Color[texture.width * texture.height];

            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = new Color(0.3f, 0.7f, 1f, 1f); // 하늘색
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

            Debug.Log("- 기본 플레이어 스프라이트 생성됨 (Skul 스타일)");
        }

        /// <summary>
        /// SkulPhysicsConfig ScriptableObject 생성 도우미
        /// </summary>
        [ContextMenu("SkulPhysicsConfig 생성 가이드")]
        public void CreateSkulPhysicsConfigGuide()
        {
            Debug.Log("=== SkulPhysicsConfig 생성 방법 ===");
            Debug.Log("1. Project 윈도우에서 우클릭");
            Debug.Log("2. Create > Skul > Physics Config 선택");
            Debug.Log("3. 생성된 SkulPhysicsConfig 에셋 선택");
            Debug.Log("4. Inspector에서 'Apply Perfect Skul Preset' 버튼 클릭");
            Debug.Log("5. CharacterPhysics의 Config 필드에 할당");
            Debug.Log("6. 필요시 값 조정:");
            Debug.Log("   - Jump Velocity: 점프 속도 (기본: 16f)");
            Debug.Log("   - Move Speed: 이동 속도 (기본: 12f)");
            Debug.Log("   - Dash Speed: 대시 속도 (기본: 28f)");
            Debug.Log("   - Gravity: 중력 강도 (기본: 32f)");
        }

        /// <summary>
        /// Skul 스타일 물리 시스템 설정 가이드
        /// </summary>
        [ContextMenu("Skul 물리 시스템 가이드")]
        public void SkulPhysicsGuide()
        {
            Debug.Log("=== Skul 스타일 물리 시스템 설정 가이드 ===");
            Debug.Log("📌 Rigidbody2D 설정:");
            Debug.Log("  · Gravity Scale: 0 (CharacterPhysics가 중력 제어)");
            Debug.Log("  · Freeze Rotation: ✓ (회전 방지)");
            Debug.Log("  · Collision Detection: Continuous (정밀한 충돌)");
            Debug.Log("");
            Debug.Log("📌 BoxCollider2D 설정:");
            Debug.Log("  · Size: (0.8, 0.9) (캐릭터 크기)");
            Debug.Log("  · Offset: (0, -0.05) (중심점 조정)");
            Debug.Log("  · Is Trigger: ✗ (물리 충돌 활성화)");
            Debug.Log("");
            Debug.Log("⚙️ SkulPhysicsConfig 파라미터:");
            Debug.Log("  · Move Speed: 12f (즉시 반응 이동)");
            Debug.Log("  · Jump Velocity: 16f (정밀한 점프)");
            Debug.Log("  · Gravity: 32f (자연스러운 중력)");
            Debug.Log("  · Dash Speed: 28f (빠른 대시)");
            Debug.Log("  · Coyote Time: 0.12f (관대한 점프)");
            Debug.Log("  · Jump Buffer: 0.15f (입력 버퍼)");
            Debug.Log("");
            Debug.Log("🎯 Layer 설정 권장:");
            Debug.Log("  · Player Layer: 플레이어 전용");
            Debug.Log("  · Ground Layer: 지형용");
            Debug.Log("  · Wall Layer: 벽 전용");
        }


        /// <summary>
        /// 안전한 컴포넌트 추가 테스트
        /// </summary>
        [ContextMenu("컴포넌트 추가 테스트")]
        public void TestSafeAddComponent()
        {
            if (gameObject == null)
            {
                Debug.LogError("GameObject가 null입니다!");
                return;
            }

            Debug.Log("=== 안전한 컴포넌트 추가 테스트 ===");

            try
            {
                // 테스트용 컴포넌트 추가 시도
                var testRigidbody = gameObject.GetComponent<Rigidbody2D>();
                if (testRigidbody == null)
                {
                    Debug.Log("Rigidbody2D 추가 시도...");
                    testRigidbody = gameObject.AddComponent<Rigidbody2D>();

                    if (testRigidbody != null)
                    {
                        Debug.Log("✅ Rigidbody2D 추가 성공!");
                        testRigidbody.gravityScale = 0f;
                        testRigidbody.freezeRotation = true;
                    }
                    else
                    {
                        Debug.LogError("❌ Rigidbody2D 추가 후 null 반환!");
                    }
                }
                else
                {
                    Debug.Log("Rigidbody2D가 이미 존재합니다.");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"컴포넌트 추가 실패: {e.Message}\n스택 트레이스: {e.StackTrace}");
            }
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

            CharacterPhysics characterPhysics = GetComponent<CharacterPhysics>();
            if (characterPhysics == null)
            {
                Debug.LogError("CharacterPhysics가 없습니다!");
                return;
            }

            Debug.Log("=== 플레이어 설정 완성도 ===");
            Debug.Log("✓ 기본 컴포넌트 설정 완료 (Skul 스타일 물리 시스템)");
            Debug.Log("- SkulPhysicsConfig 할당 여부는 Inspector에서 확인하세요");
            Debug.Log("- Ground Layer Mask 설정을 확인하세요");
            Debug.Log("- Wall Layer Mask 설정을 확인하세요");
            Debug.Log("- Rigidbody2D 설정값들을 확인하세요:");
            Debug.Log("  · Gravity Scale: 0 (필수)");
            Debug.Log("  · Freeze Rotation: ✓ (권장)");
            Debug.Log("  · Collision Detection: Continuous (권장)");
            Debug.Log("- Skul 물리 파라미터를 조정하세요:");
            Debug.Log("  · Move Speed: 이동 속도 (기본값: 12f)");
            Debug.Log("  · Jump Velocity: 점프 속도 (기본값: 16f)");
            Debug.Log("  · Dash Speed: 대시 속도 (기본값: 28f)");
            Debug.Log("  · Gravity: 중력 강도 (기본값: 32f)");
            Debug.Log("- 테스트를 위해 Ground 오브젝트를 씬에 배치하세요");
            Debug.Log("- 키보드 1-6번으로 물리 시스템 테스트 가능합니다");
        }

        /// <summary>
        /// GameObject 상태 디버깅
        /// </summary>
        [ContextMenu("GameObject 상태 확인")]
        public void DebugGameObjectState()
        {
            Debug.Log("=== GameObject 상태 디버깅 ===");
            Debug.Log($"GameObject: {gameObject?.name ?? "NULL"}");
            Debug.Log($"GameObject null?: {gameObject == null}");
            Debug.Log($"GameObject active?: {gameObject?.activeInHierarchy ?? false}");
            Debug.Log($"GameObject activeSelf?: {gameObject?.activeSelf ?? false}");
            Debug.Log($"Application.isPlaying: {Application.isPlaying}");
            Debug.Log($"Application.isEditor: {Application.isEditor}");

            if (gameObject != null)
            {
                var components = gameObject.GetComponents<Component>();
                Debug.Log($"총 컴포넌트 개수: {components.Length}");
                foreach (var comp in components)
                {
                    Debug.Log($"- {comp?.GetType().Name ?? "NULL Component"}");
                }
            }
        }
    }
}
