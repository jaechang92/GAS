using UnityEngine;

namespace GASPT.Gameplay.Environment
{
    public class ParallaxBackground : MonoBehaviour
    {
        [Header("Parallax Settings")]
        [Tooltip("0: Static (Sky), 1: Move with Camera (Foreground)")]
        [Range(0f, 1f)]
        [SerializeField] private float parallaxEffect;
        
        [Tooltip("Check if this background should loop infinitely")]
        [SerializeField] private bool infiniteLoop = true;

        private Transform cameraTransform;
        private Vector3 lastCameraPosition;
        private float textureUnitSizeX;
        
        private void Start()
        {
            if (UnityEngine.Camera.main != null)
            {
                cameraTransform = UnityEngine.Camera.main.transform;
                lastCameraPosition = cameraTransform.position;
            }
            else
            {
                Debug.LogError("Main Camera not found! ParallaxBackground needs a MainCamera to work.");
                enabled = false;
                return;
            }

            if (infiniteLoop)
            {
                SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    Texture2D texture = spriteRenderer.sprite.texture;
                    textureUnitSizeX = texture.width / spriteRenderer.sprite.pixelsPerUnit;
                    
                    // Consider scale
                    textureUnitSizeX *= transform.localScale.x; 
                }
                else
                {
                    Debug.LogWarning("ParallaxBackground: Infinite Loop requires a SpriteRenderer. Feature disabled for this object.");
                    infiniteLoop = false;
                }
            }
        }

        private void LateUpdate()
        {
            if (cameraTransform == null) return;

            Vector3 deltaMovement = cameraTransform.position - lastCameraPosition;
            
            // Move the background with parallax effect
            // If parallaxEffect is 1, it moves exactly with camera (appears static relative to camera)
            // If parallaxEffect is 0, it doesn't move (appears static in world, moving fast relative to camera)
            // Wait, usually parallaxEffect 1 means "far away" (moves slow)? 
            // Let's stick to: 0 = static in world (near), 1 = static on screen (sky/far)
            // Formula: transform.position.x += deltaMovement.x * parallaxEffect;
            
            transform.position += new Vector3(deltaMovement.x * parallaxEffect, deltaMovement.y * parallaxEffect, 0);
            
            lastCameraPosition = cameraTransform.position;

            if (infiniteLoop)
            {
                float distFromCamera = cameraTransform.position.x - transform.position.x;
                
                // If the camera has moved past the texture's width/2 (center), snap the background
                if (Mathf.Abs(distFromCamera) >= textureUnitSizeX)
                {
                    float offsetPositionX = (distFromCamera > 0) ? textureUnitSizeX : -textureUnitSizeX;
                    transform.position = new Vector3(transform.position.x + offsetPositionX, transform.position.y, transform.position.z);
                }
            }
        }
    }
}
