using UnityEngine;
using UnityEditor;
using GASPT.Gameplay.Environment;

namespace GASPT.Editor
{
    public class SceneSetupTool
    {
        [MenuItem("Tools/GASPT/Scenes/Setup Parallax Demo (Stage 1)")]
        public static void SetupParallaxDemo()
        {
            // Create root
            GameObject root = new GameObject("Background_ToxicSwamp");
            
            // Camera setup
            if (Camera.main == null)
            {
                GameObject camObj = new GameObject("Main Camera");
                camObj.AddComponent<Camera>();
                camObj.tag = "MainCamera";
                camObj.transform.position = new Vector3(0, 0, -10);
            }
            
            // Layers Config
            // Layer4_Sky -> Parallax 1.0, Sorting Order -4
            // Layer3_Far -> Parallax 0.8, Sorting Order -3
            // Layer2_Mid -> Parallax 0.5, Sorting Order -2
            // Layer1_Foreground -> Parallax 0.0 (or 0.1), Sorting Order -1
            // Layer0_Overlay -> Optional, Parallax 1.2? (If existing)

            CreateLayer(root.transform, "Layer4_Sky", 1.0f, -40);
            CreateLayer(root.transform, "Layer3_Far", 0.8f, -30);
            CreateLayer(root.transform, "Layer2_Mid", 0.5f, -20);
            CreateLayer(root.transform, "Layer1_Foreground", 0.0f, -10);
            
            // Add a simple camera mover for testing
            GameObject mover = new GameObject("CameraMover [Debug]");
            var moverScript = mover.AddComponent<SimpleCameraMover>();
        }

        private static void CreateLayer(Transform parent, string textureName, float parallaxEffect, int sortingOrder)
        {
            // Find texture
            string[] guids = AssetDatabase.FindAssets($"{textureName} t:Texture2D");
            if (guids.Length == 0)
            {
                Debug.LogWarning($"Could not find texture: {textureName}. Skipping layer.");
                return;
            }

            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            
            // Create Object
            GameObject obj = new GameObject(textureName);
            obj.transform.SetParent(parent);
            obj.transform.localPosition = Vector3.zero;

            // Sprite Renderer
            SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
            
            // Create Sprite from texture if needed, or load if it's already a sprite
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null)
            {
                // If the asset is just a texture, we might need to change import settings or just create a sprite on the fly (less ideal for loops)
                // Assuming they are imported as Sprites.
                // If not, we try to set it.
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer.textureType != TextureImporterType.Sprite)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.SaveAndReimport();
                    sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                }
            }
            sr.sprite = sprite;
            sr.sortingOrder = sortingOrder;

            // Parallax
            ParallaxBackground pb = obj.AddComponent<ParallaxBackground>();
            // Use SerializedObject to set private fields if needed, or expose public setter.
            // Since we defined fields as SerializeField private, we can use SerializedObject.
            SerializedObject so = new SerializedObject(pb);
            so.FindProperty("parallaxEffect").floatValue = parallaxEffect;
            so.FindProperty("infiniteLoop").boolValue = true;
            so.ApplyModifiedProperties();
        }
    }

    // Simple script for verify movement, embedded here or separate?
    // Let's create a separate temporary component or just adding a simple logic here is hard.
    // I'll create a separate file for the mover or just tell user to move camera.
    // Actually, let's inject a component that moves camera with arrow keys.
}

// Minimal Camera Mover for testing
public class SimpleCameraMover : MonoBehaviour
{
    public float speed = 5f;
    void Update()
    {
        if (Camera.main != null)
        {
            float x = Input.GetAxis("Horizontal") * speed * Time.deltaTime;
            Camera.main.transform.Translate(x, 0, 0);
        }
    }
}
