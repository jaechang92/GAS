#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using Gameplay.NPC;

namespace Editor.Tools
{
    /// <summary>
    /// NPC 생성 도구의 공통 Helper 메서드
    /// </summary>
    public static class NPCCreatorHelper
    {
        public const string PREFAB_PATH = "Assets/_Project/Prefabs/NPC";
        public const string DATA_PATH = "Assets/_Project/Resources/NPCData";

        /// <summary>
        /// 폴더 존재 확인 및 생성
        /// </summary>
        public static void EnsureDirectories()
        {
            // Prefab 폴더
            if (!Directory.Exists(PREFAB_PATH))
            {
                Directory.CreateDirectory(PREFAB_PATH);
                Debug.Log($"[NPCCreator] 폴더 생성: {PREFAB_PATH}");
            }

            // NPCData 폴더
            if (!Directory.Exists(DATA_PATH))
            {
                Directory.CreateDirectory(DATA_PATH);
                Debug.Log($"[NPCCreator] 폴더 생성: {DATA_PATH}");
            }

            AssetDatabase.Refresh();
        }

        /// <summary>
        /// 임시 단색 스프라이트 생성
        /// </summary>
        public static Sprite CreateTempSprite(Color color)
        {
            Texture2D tex = new Texture2D(32, 48);
            Color[] pixels = new Color[32 * 48];

            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }

            tex.SetPixels(pixels);
            tex.Apply();

            return Sprite.Create(
                tex,
                new Rect(0, 0, 32, 48),
                new Vector2(0.5f, 0f), // Pivot at bottom center
                32f
            );
        }
    }
}
#endif
