using UnityEngine;
using UnityEditor;
using System.IO;

namespace GASPT.Editor
{
    public class SpriteProcessingTool : EditorWindow
    {
        private Color keyColor = Color.black;
        private float tolerance = 0.1f;
        
        private Texture2D selectedTexture;
        private Texture2D previewTexture;
        private Vector2 scrollPosition;

        [MenuItem("Tools/GASPT/Art/Sprite Processor (Background Remover)")]
        public static void ShowWindow()
        {
            GetWindow<SpriteProcessingTool>("Sprite Processor");
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            GUILayout.Label("Sprite Background Remover", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // Settings
            EditorGUI.BeginChangeCheck();
            keyColor = EditorGUILayout.ColorField("Background Color", keyColor);
            tolerance = EditorGUILayout.Slider("Tolerance", tolerance, 0f, 1f);
            
            // Selection Handling
            Texture2D currentSelection = GetSelectedTexture();
            if (currentSelection != selectedTexture)
            {
                selectedTexture = currentSelection;
                UpdatePreview();
            }
            
            if (EditorGUI.EndChangeCheck())
            {
                UpdatePreview();
            }

            EditorGUILayout.Space();

            // Preview Area
            if (selectedTexture != null)
            {
                GUILayout.Label($"Selected: {selectedTexture.name} ({selectedTexture.width}x{selectedTexture.height})");
                
                EditorGUILayout.BeginHorizontal();
                
                // Original
                EditorGUILayout.BeginVertical(GUILayout.Width(position.width * 0.45f));
                GUILayout.Label("Original");
                Rect r1 = GUILayoutUtility.GetRect(200, 200);
                EditorGUI.DrawTextureTransparent(r1, selectedTexture, ScaleMode.ScaleToFit);
                EditorGUILayout.EndVertical();

                // Preview
                EditorGUILayout.BeginVertical(GUILayout.Width(position.width * 0.45f));
                GUILayout.Label("Preview (Transparent)");
                Rect r2 = GUILayoutUtility.GetRect(200, 200);
                if (previewTexture != null)
                {
                    // Draw checkerboard background for transparency
                    EditorGUI.DrawTextureTransparent(r2, previewTexture, ScaleMode.ScaleToFit);
                }
                EditorGUILayout.EndVertical();

                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.HelpBox("Select a Texture in the Project window to preview.", MessageType.Info);
            }

            EditorGUILayout.Space();

            // Actions
            if (GUILayout.Button("Process & Save Selected", GUILayout.Height(30)))
            {
                ProcessAndSave();
            }

            EditorGUILayout.EndScrollView();
        }

        private Texture2D GetSelectedTexture()
        {
            if (Selection.activeObject is Texture2D tex)
            {
                return tex;
            }
            return null;
        }

        private void UpdatePreview()
        {
            if (selectedTexture == null) return;
            
            // Ensure readable
            string path = AssetDatabase.GetAssetPath(selectedTexture);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null && !importer.isReadable)
            {
                // Note: We avoid force-reimporting in OnGUI for performance/UX reasons unless user acts
                // For preview, we might read a copy if possible, but simplest is asking user to set readable
                // Or we can try to copy texture if allowed
            }

            try 
            {
                if (!selectedTexture.isReadable)
                {
                    // If not readable, we can't get pixels. Ask user or skip.
                    // For specific tool usage, we can just warn or force it on 'Process'.
                    // For preview, let's try to simulate or just show original if failed.
                   return; 
                }

                if (previewTexture != null) DestroyImmediate(previewTexture);
                
                previewTexture = new Texture2D(selectedTexture.width, selectedTexture.height, TextureFormat.RGBA32, false);
                Color[] pixels = selectedTexture.GetPixels();
                
                for (int i = 0; i < pixels.Length; i++)
                {
                    if (IsColorMatch(pixels[i], keyColor, tolerance))
                    {
                        pixels[i] = new Color(0, 0, 0, 0);
                    }
                }
                
                previewTexture.SetPixels(pixels);
                previewTexture.Apply();
            }
            catch (UnityException)
            {
               // Texture not readable
            }
        }

        private void ProcessAndSave()
        {
            if (selectedTexture == null) return;

            string path = AssetDatabase.GetAssetPath(selectedTexture);
            
            // Ensure readable before processing
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.isReadable = true;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }

            // Re-run logic on fresh readable texture
            UpdatePreview(); 

            if (previewTexture == null) return;

            byte[] bytes = previewTexture.EncodeToPNG();
            string newPath = path.Replace(".png", "_Transparent.png");
            if (path == newPath) newPath = path.Replace(Path.GetExtension(path), "_Alpha" + Path.GetExtension(path));

            File.WriteAllBytes(newPath, bytes);
            AssetDatabase.Refresh();
            Debug.Log($"Saved transparent texture to: {newPath}");
        }

        private bool IsColorMatch(Color pixel, Color key, float tol)
        {
            float diff = Mathf.Abs(pixel.r - key.r) + Mathf.Abs(pixel.g - key.g) + Mathf.Abs(pixel.b - key.b);
            return diff < tol * 3;
        }

        private void OnDisable()
        {
            if (previewTexture != null) DestroyImmediate(previewTexture);
        }
    }
}
