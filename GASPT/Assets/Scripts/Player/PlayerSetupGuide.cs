using UnityEngine;
using GAS.Core;
using Character.Physics;

namespace Player
{
    /// <summary>
    /// 플레이어 오브젝트 설정 가이드
    /// Skul 스타일 무중력 물리 시스템을 사용하는 플레이어 설정 방법을 제공
    /// MovementCalculator + RaycastController + CharacterPhysicsConfig 기반 시스템 사용
    /// 중력 없는 고정 속도 기반 플랫폼 액션 시스템
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

            // GameObject null 체크
            if (playerGO == null)
            {
                Debug.LogError("[PlayerSetup] GameObject가 null입니다!");
                return;
            }

            Debug.Log("[PlayerSetup] Skul 스타일 플레이어 컴포넌트 자동 설정 시작");

            // Unity 물리 컴포넌트들 제거 (Skul 스타일은 순수 Transform 기반)
            RemoveUnityPhysicsComponents(playerGO);

            // 1. BoxCollider2D 추가 (Raycast용 - 필수)
            BoxCollider2D boxCollider = playerGO.GetComponent<BoxCollider2D>();
            if (boxCollider == null)
            {
                try
                {
                    boxCollider = playerGO.AddComponent<BoxCollider2D>();
                    if (boxCollider == null)
                    {
                        Debug.LogError("[PlayerSetup] BoxCollider2D 추가 실패 - Skul 물리 시스템이 작동하지 않습니다!");
                        return;
                    }
                    Debug.Log("- BoxCollider2D 추가됨 (Raycast 충돌 검사용)");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[PlayerSetup] BoxCollider2D 추가 실패: {e.Message}");
                    return;
                }
            }

            // BoxCollider2D 설정 (Skul 스타일)
            boxCollider.size = new Vector2(0.8f, 1.8f);
            boxCollider.isTrigger = false; // Raycast용이므로 트리거 아님

            // 2. RaycastController 추가 (정밀 충돌 검사)
            if (playerGO.GetComponent<RaycastController>() == null)
            {
                playerGO.AddComponent<RaycastController>();
                Debug.Log("- RaycastController 추가됨 (정밀 충돌 검사 시스템)");
            }

            // 3. MovementCalculator 추가 (Skul 스타일 물리 엔진)
            if (playerGO.GetComponent<MovementCalculator>() == null)
            {
                playerGO.AddComponent<MovementCalculator>();
                Debug.Log("- MovementCalculator 추가됨 (Skul 스타일 물리 엔진)");
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

            // 6. PlayerController 추가 (모든 의존성 해결 후 마지막에 추가)
            if (playerGO.GetComponent<PlayerController>() == null)
            {
                playerGO.AddComponent<PlayerController>();
                Debug.Log("- PlayerController 추가됨");
            }

            // 7. 태그 설정
            if (playerGO.tag != "Player")
            {
                playerGO.tag = "Player";
                Debug.Log("- Player 태그 설정됨");
            }

            Debug.Log("[PlayerSetup] Skul 스타일 플레이어 컴포넌트 자동 설정 완료!");
            Debug.Log("⚠️ CharacterPhysicsConfig ScriptableObject 할당이 필요합니다!");
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
                typeof(MovementCalculator),    // Skul 스타일 물리 엔진
                typeof(RaycastController),     // 정밀 충돌 검사
                typeof(BoxCollider2D),         // Skul 스타일은 Box 형태
                typeof(SpriteRenderer),
                typeof(AbilitySystem)
            };

            // 금지된 컴포넌트 (Unity 물리 시스템 - Skul 스타일은 순수 Transform 기반)
            var forbiddenComponents = new System.Type[]
            {
                typeof(Rigidbody2D),
                typeof(CharacterController),
                typeof(Rigidbody),
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

            // 금지된 컴포넌트 확인
            foreach (var componentType in forbiddenComponents)
            {
                var component = playerGO.GetComponent(componentType);
                if (component != null)
                {
                    Debug.LogError($"❌ {componentType.Name} 발견됨 - Skul 스타일 물리 시스템과 충돌합니다. 제거 필요!");
                    isValid = false;
                }
                else
                {
                    Debug.Log($"✅ {componentType.Name} 없음 (정상)");
                }
            }

            // BoxCollider2D 추가 검증
            var boxCollider = playerGO.GetComponent<BoxCollider2D>();
            if (boxCollider != null)
            {
                if (boxCollider.isTrigger)
                {
                    Debug.LogWarning("⚠️ BoxCollider2D가 Trigger로 설정됨 - Raycast용이므로 Trigger를 해제해야 합니다.");
                    isValid = false;
                }
                else
                {
                    Debug.Log("✅ BoxCollider2D Trigger 설정 정상 (Raycast용)");
                }
            }

            // MovementCalculator 설정 확인
            var movementCalculator = playerGO.GetComponent<MovementCalculator>();
            if (movementCalculator != null)
            {
                // Reflection을 통해 private config 필드 확인
                var configField = typeof(MovementCalculator).GetField("config",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (configField != null)
                {
                    var config = configField.GetValue(movementCalculator) as CharacterPhysicsConfig;
                    if (config == null)
                    {
                        Debug.LogError("❌ MovementCalculator에 CharacterPhysicsConfig가 할당되지 않았습니다!");
                        isValid = false;
                    }
                    else
                    {
                        Debug.Log($"✅ CharacterPhysicsConfig 할당됨 (Skul Preset: {config.useSkulPreset})");
                        if (!config.useSkulPreset)
                        {
                            Debug.LogWarning("⚠️ Skul Preset이 비활성화되어 있습니다. 활성화를 권장합니다.");
                        }
                    }
                }
            }

            // RaycastController 설정 확인
            var raycastController = playerGO.GetComponent<RaycastController>();
            if (raycastController != null)
            {
                var maskField = typeof(RaycastController).GetField("collisionMask",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (maskField != null)
                {
                    var mask = (LayerMask)maskField.GetValue(raycastController);
                    if (mask.value == 0 || mask.value == -1)
                    {
                        Debug.LogWarning("⚠️ RaycastController의 collisionMask 설정을 확인하세요.");
                    }
                    else
                    {
                        Debug.Log("✅ RaycastController collisionMask 설정됨");
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
        /// CharacterPhysicsConfig ScriptableObject 생성 도우미
        /// </summary>
        [ContextMenu("CharacterPhysicsConfig 생성 가이드")]
        public void CreateCharacterPhysicsConfigGuide()
        {
            Debug.Log("=== CharacterPhysicsConfig 생성 방법 ===");
            Debug.Log("1. Project 윈도우에서 우클릭");
            Debug.Log("2. Create > Character > Physics Config 선택");
            Debug.Log("3. 생성된 CharacterPhysicsConfig 에셋 선택");
            Debug.Log("4. Inspector에서 'Use Skul Preset' 체크 후 'Apply Skul Preset' 버튼 클릭");
            Debug.Log("5. MovementCalculator의 Config 필드에 할당");
            Debug.Log("6. 필요시 값 조정:");
            Debug.Log("   - Jump Velocity: 점프 속도 (기본: 18f)");
            Debug.Log("   - Move Speed: 이동 속도 (기본: 10f)");
            Debug.Log("   - Fall Speeds: 낙하 속도들 (기본: -18f ~ -30f)");
            Debug.Log("   - Dash Speed: 대시 속도 (기본: 25f)");
        }

        /// <summary>
        /// 커스텀 물리 시스템 설정 가이드
        /// </summary>
        [ContextMenu("커스텀 물리 시스템 가이드")]
        public void CustomPhysicsGuide()
        {
            Debug.Log("=== 커스텀 물리 시스템 설정 가이드 ===");
            Debug.Log("📌 CharacterController 설정:");
            Debug.Log("  · Height: 1.8f (캐릭터 높이)");
            Debug.Log("  · Radius: 0.4f (캐릭터 폭)");
            Debug.Log("  · Step Offset: 0.3f (계단 오르기)");
            Debug.Log("  · Skin Width: 0.08f (충돌 감지)");
            Debug.Log("  · Min Move Distance: 0.001f (최소 이동거리)");
            Debug.Log("  · Center: (0, 0.9, 0) (콜라이더 중심)");
            Debug.Log("");
            Debug.Log("⚙️ 커스텀 물리 파라미터:");
            Debug.Log("  · Gravity: 30f (중력 강도)");
            Debug.Log("  · Max Fall Speed: 20f (최대 낙하속도)");
            Debug.Log("  · Move Speed: 8f (기본 이동속도)");
            Debug.Log("  · Air Move Speed: 6f (공중 이동속도)");
            Debug.Log("  · Jump Force: 15f (점프력)");
            Debug.Log("");
            Debug.Log("🎯 Physics2D 설정 (접지 검사용):");
            Debug.Log("  · Ground Layer: 지형용 레이어 생성");
            Debug.Log("  · Player Layer: 플레이어용 레이어 생성");
            Debug.Log("  · Layer Collision Matrix 설정");
        }

        /// <summary>
        /// Rigidbody2D 제거 도구
        /// </summary>
        [ContextMenu("Rigidbody2D 제거")]
        public void RemoveRigidbody2D()
        {
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                if (Application.isPlaying)
                    Destroy(rb);
                else
                    DestroyImmediate(rb);
                Debug.Log("✅ Rigidbody2D가 제거되었습니다. 커스텀 물리 시스템을 사용합니다.");
            }
            else
            {
                Debug.Log("ℹ️ Rigidbody2D가 이미 없습니다.");
            }
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

        /// <summary>
        /// Unity 물리 컴포넌트들 제거 (Skul 스타일은 순수 Transform 기반)
        /// </summary>
        private void RemoveUnityPhysicsComponents(GameObject playerGO)
        {
            // Rigidbody2D 제거 (Skul 스타일은 Transform 직접 조작)
            Rigidbody2D existingRb = playerGO.GetComponent<Rigidbody2D>();
            if (existingRb != null)
            {
                try
                {
                    if (Application.isPlaying)
                        Destroy(existingRb);
                    else
                        DestroyImmediate(existingRb);
                    Debug.Log("- 기존 Rigidbody2D 제거됨 (Skul 스타일은 Transform 직접 조작)");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[PlayerSetup] Rigidbody2D 제거 실패: {e.Message}");
                }
            }

            // CharacterController 제거
            CharacterController existingController = playerGO.GetComponent<CharacterController>();
            if (existingController != null)
            {
                try
                {
                    if (Application.isPlaying)
                        Destroy(existingController);
                    else
                        DestroyImmediate(existingController);
                    Debug.Log("- 기존 CharacterController 제거됨 (Skul 스타일 물리 시스템 사용)");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[PlayerSetup] CharacterController 제거 실패: {e.Message}");
                }
            }

            // Rigidbody (3D) 제거
            Rigidbody existingRb3D = playerGO.GetComponent<Rigidbody>();
            if (existingRb3D != null)
            {
                try
                {
                    if (Application.isPlaying)
                        Destroy(existingRb3D);
                    else
                        DestroyImmediate(existingRb3D);
                    Debug.Log("- 기존 Rigidbody 제거됨 (Skul 스타일은 2D 전용)");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[PlayerSetup] Rigidbody 제거 실패: {e.Message}");
                }
            }

            // CapsuleCollider2D 제거 (Skul 스타일은 BoxCollider2D만 사용)
            CapsuleCollider2D existingCapsule = playerGO.GetComponent<CapsuleCollider2D>();
            if (existingCapsule != null)
            {
                try
                {
                    if (Application.isPlaying)
                        Destroy(existingCapsule);
                    else
                        DestroyImmediate(existingCapsule);
                    Debug.Log("- 기존 CapsuleCollider2D 제거됨 (Skul 스타일은 BoxCollider2D 사용)");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[PlayerSetup] CapsuleCollider2D 제거 실패: {e.Message}");
                }
            }
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
                var testCollider = gameObject.GetComponent<BoxCollider2D>();
                if (testCollider == null)
                {
                    Debug.Log("BoxCollider2D 추가 시도...");
                    testCollider = gameObject.AddComponent<BoxCollider2D>();

                    if (testCollider != null)
                    {
                        Debug.Log("✅ BoxCollider2D 추가 성공!");
                        testCollider.isTrigger = false; // 충돌 검사용
                        testCollider.size = new Vector2(1f, 1f);
                    }
                    else
                    {
                        Debug.LogError("❌ BoxCollider2D 추가 후 null 반환!");
                    }
                }
                else
                {
                    Debug.Log("BoxCollider2D가 이미 존재합니다.");
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

            Debug.Log("=== 플레이어 설정 완성도 ===");

            // PlayerStats 확인
            // Note: PlayerStats는 private이므로 직접 확인 불가, 실제 구현 시 public getter 추가 고려

            Debug.Log("✓ 기본 컴포넌트 설정 완료 (커스텀 물리 시스템)");
            Debug.Log("- PlayerStats 할당 여부는 Inspector에서 확인하세요");
            Debug.Log("- Ground Layer Mask 설정을 확인하세요");
            Debug.Log("- CharacterController 설정값들을 조정하세요:");
            Debug.Log("  · Height: 캐릭터 높이 (기본값: 1.8f)");
            Debug.Log("  · Radius: 캐릭터 폭 (기본값: 0.4f)");
            Debug.Log("  · Step Offset: 계단 오르기 높이 (기본값: 0.3f)");
            Debug.Log("- 커스텀 물리 파라미터를 조정하세요:");
            Debug.Log("  · Gravity: 중력 강도 (기본값: 30f)");
            Debug.Log("  · Max Fall Speed: 최대 낙하속도 (기본값: 20f)");
            Debug.Log("  · Air Move Speed: 공중 이동속도 (기본값: 6f)");
            Debug.Log("- 테스트를 위해 Ground 오브젝트를 씬에 배치하세요");
        }
    }
}
