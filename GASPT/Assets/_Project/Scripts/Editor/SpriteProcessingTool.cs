using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace GASPT.Editor
{
    public class SpriteProcessingTool : EditorWindow
    {
        private Color keyColor = Color.green; // Default to green screen
        private float tolerance = 0.1f;
        private float smoothness = 0.05f; // Range 0-1 for edge feathering
        
        private List<Texture2D> selectedTextures = new List<Texture2D>();
        private Texture2D previewOriginal;
        private Texture2D previewResult;
        private Vector2 scrollPosition;

        [MenuItem("Tools/GASPT/Art/Sprite Processor (Background Remover)")]
        public static void ShowWindow()
        {
            GetWindow<SpriteProcessingTool>("Sprite Processor");
        }

        private void OnEnable()
        {
            // Initial selection check
            UpdateSelection();
        }

        private void OnSelectionChange()
        {
            UpdateSelection();
            Repaint();
        }

        private void UpdateSelection()
        {
            selectedTextures.Clear();
            foreach (var obj in Selection.objects)
            {
                if (obj is Texture2D tex)
                {
                    selectedTextures.Add(tex);
                }
            }
            
            // Generate preview for the first one if available
            UpdatePreview();
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            GUILayout.Label("Sprite Background Remover", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // Settings Section
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                GUILayout.Label("Settings", EditorStyles.label);
                EditorGUI.BeginChangeCheck();
                
                keyColor = EditorGUILayout.ColorField(new GUIContent("Key Color", "The background color to remove"), keyColor);
                tolerance = EditorGUILayout.Slider(new GUIContent("Tolerance", "Color matching sensitivity"), tolerance, 0f, 1f);
                smoothness = EditorGUILayout.Slider(new GUIContent("Edge Smoothing", "Softness of the transparency edge"), smoothness, 0f, 0.5f);

                if (EditorGUI.EndChangeCheck())
                {
                    UpdatePreview();
                }
            }

            EditorGUILayout.Space();

            // Status Section
            if (selectedTextures.Count > 0)
            {
                EditorGUILayout.HelpBox($"Selected {selectedTextures.Count} texture(s).", MessageType.Info);
                
                if (GUILayout.Button($"Process {selectedTextures.Count} File(s)", GUILayout.Height(30)))
                {
                    ProcessBatch();
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Select Texture(s) in the Project window to proceed.", MessageType.Warning);
            }

            EditorGUILayout.Space();

            // Preview Section (Single Item)
            if (selectedTextures.Count > 0)
            {
                GUILayout.Label($"Preview: {selectedTextures[0].name}", EditorStyles.boldLabel);
                
                EditorGUILayout.BeginHorizontal();
                
                // Original
                DisplayPreviewBox("Original", previewOriginal);
                
                GUILayout.Space(10);

                // Result
                DisplayPreviewBox("Preview", previewResult, true);

                EditorGUILayout.EndHorizontal();
                
                if (selectedTextures.Count > 1)
                {
                    EditorGUILayout.LabelField("(Previewing first selected item only)", EditorStyles.miniLabel);
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private void DisplayPreviewBox(string title, Texture2D tex, bool checkboard = false)
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(200));
            GUILayout.Label(title);
            Rect r = GUILayoutUtility.GetRect(200, 200);
            
            // Draw background for transparency
            if (checkboard)
            {
                EditorGUI.DrawRect(r, Color.gray);
                // Simple checkerboard simulation can be added here if needed, 
                // but Unity's DrawTextureTransparent handles alpha blending against the UI background.
            }

            if (tex != null)
            {
                 EditorGUI.DrawTextureTransparent(r, tex, ScaleMode.ScaleToFit);
            }
            EditorGUILayout.EndVertical();
        }

        private void UpdatePreview()
        {
            if (previewResult != null) DestroyImmediate(previewResult);
            previewOriginal = null;

            if (selectedTextures.Count == 0) return;

            Texture2D source = selectedTextures[0];
            
            // Try to make it readable temporarily for preview without altering asset setting yet if possible.
            // But realistically, we need isReadable true to access pixels.
            if (!source.isReadable)
            {
                // We show a warning or placeholder, or we can't preview.
                return;
            }

            previewOriginal = source;
            previewResult = ProcessTexture(source);
        }

        private Texture2D ProcessTexture(Texture2D source)
        {
            if (!source.isReadable) return null;

            Texture2D result = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
            Color[] pixels = source.GetPixels();
            Color[] newPixels = new Color[pixels.Length];

            for (int i = 0; i < pixels.Length; i++)
            {
                Color p = pixels[i];
                float diff = GetColorDiff(p, keyColor);
                
                // alpha calculation
                // if diff < tolerance -> alpha = 0
                // if diff > tolerance + smoothness -> alpha = 1
                // between -> interpolate
                
                float alpha = Mathf.InverseLerp(tolerance, tolerance + smoothness + 0.001f, diff);
                
                // Prepare new pixel - keep original RGB, modify Alpha
                // Optional: If alpha is low, maybe blend RGB towards white or keeping original to avoid dark edges?
                // Keeping original RGB usually works best for pre-multiplied alpha shaders or standard blending.
                newPixels[i] = new Color(p.r, p.g, p.b, alpha);
            }

            result.SetPixels(newPixels);
            result.Apply();
            return result;
        }

        private float GetColorDiff(Color c1, Color c2)
        {
            // Simple Manhattan distance or Euclidean.
            // Using average difference
            return (Mathf.Abs(c1.r - c2.r) + Mathf.Abs(c1.g - c2.g) + Mathf.Abs(c1.b - c2.b)) / 3f;
        }

        private void ProcessBatch()
        {
            int processedCount = 0;

            try
            {
                AssetDatabase.StartAssetEditing();

                foreach (var tex in selectedTextures)
                {
                    string path = AssetDatabase.GetAssetPath(tex);
                    TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                    
                    if (importer != null)
                    {
                        // 1. Force Readable & RGBA32
                        bool wasReadable = importer.isReadable;
                        TextureImporterCompression wasCompression = importer.textureCompression;
                        
                        importer.isReadable = true;
                        importer.textureCompression = TextureImporterCompression.Uncompressed;
                        importer.SaveAndReimport();

                        // 2. Process
                        Texture2D processed = ProcessTexture(tex);
                        if (processed != null)
                        {
                            byte[] bytes = processed.EncodeToPNG();
                            string newPath = path.Replace(".png", "_Transparent.png");
                            if (path == newPath) newPath = path.Replace(Path.GetExtension(path), "_Alpha" + Path.GetExtension(path));

                            File.WriteAllBytes(newPath, bytes);
                            processedCount++;
                            DestroyImmediate(processed);
                        }

                        // 3. Revert settings if desired? 
                        // Usually better to keep source clean, but we modified the importer. 
                        // User might want to revert "Read/Write" after, but let's leave it for now or restore.
                        // Restoring might be expensive if many files. Let's keep it simple.
                    }
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.Refresh();
            }

            Debug.Log($"Processed {processedCount} images successfully.");
        }

        private void OnDisable()
        {
            if (previewResult != null) DestroyImmediate(previewResult);
        }
    }
}
