using System.Collections.Generic;
using UnityEngine;
using GASPT.Core;

namespace GASPT.ResourceManagement
{
    /// <summary>
    /// 게임 리소스 관리 싱글톤
    /// Resources 폴더의 리소스를 로드하고 캐싱
    /// </summary>
    public class GameResourceManager : SingletonManager<GameResourceManager>
    {
        // ====== 캐시 ======

        /// <summary>
        /// 로드된 리소스 캐시
        /// Key: 리소스 경로, Value: 로드된 오브젝트
        /// </summary>
        private Dictionary<string, Object> resourceCache = new Dictionary<string, Object>();


        // ====== Unity 생명주기 ======

        protected override void OnAwake()
        {
            Debug.Log("[GameResourceManager] 초기화 완료");
        }


        // ====== 리소스 로딩 (제네릭) ======

        /// <summary>
        /// 리소스 로드 (제네릭, 캐싱 사용)
        /// </summary>
        /// <typeparam name="T">리소스 타입</typeparam>
        /// <param name="path">Resources 폴더 기준 경로 (확장자 제외)</param>
        /// <returns>로드된 리소스 (실패 시 null)</returns>
        public T Load<T>(string path) where T : Object
        {
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogError("[GameResourceManager] Load: path가 null이거나 비어있습니다.");
                return null;
            }

            // 캐시 확인
            if (resourceCache.TryGetValue(path, out Object cachedResource))
            {
                if (cachedResource is T typedResource)
                {
                    Debug.Log($"[GameResourceManager] 캐시에서 로드: {path}");
                    return typedResource;
                }
                else
                {
                    Debug.LogWarning($"[GameResourceManager] 캐시된 리소스 타입 불일치: {path} (요청: {typeof(T)}, 실제: {cachedResource.GetType()})");
                }
            }

            // Resources.Load 실행
            T resource = Resources.Load<T>(path);

            if (resource != null)
            {
                // 캐시에 저장
                resourceCache[path] = resource;
                Debug.Log($"[GameResourceManager] 리소스 로드 완료: {path} ({typeof(T).Name})");
                return resource;
            }
            else
            {
                Debug.LogError($"[GameResourceManager] 리소스 로드 실패: {path} ({typeof(T).Name})");
                return null;
            }
        }


        // ====== 타입별 로딩 메서드 ======

        /// <summary>
        /// GameObject (Prefab) 로드
        /// </summary>
        /// <param name="path">Resources 폴더 기준 경로</param>
        /// <returns>로드된 GameObject</returns>
        public GameObject LoadPrefab(string path)
        {
            return Load<GameObject>(path);
        }

        /// <summary>
        /// ScriptableObject 로드
        /// </summary>
        /// <typeparam name="T">ScriptableObject 타입</typeparam>
        /// <param name="path">Resources 폴더 기준 경로</param>
        /// <returns>로드된 ScriptableObject</returns>
        public T LoadScriptableObject<T>(string path) where T : ScriptableObject
        {
            return Load<T>(path);
        }

        /// <summary>
        /// AudioClip 로드
        /// </summary>
        /// <param name="path">Resources 폴더 기준 경로</param>
        /// <returns>로드된 AudioClip</returns>
        public AudioClip LoadAudioClip(string path)
        {
            return Load<AudioClip>(path);
        }

        /// <summary>
        /// Sprite 로드
        /// </summary>
        /// <param name="path">Resources 폴더 기준 경로</param>
        /// <returns>로드된 Sprite</returns>
        public Sprite LoadSprite(string path)
        {
            return Load<Sprite>(path);
        }

        /// <summary>
        /// TextAsset 로드
        /// </summary>
        /// <param name="path">Resources 폴더 기준 경로</param>
        /// <returns>로드된 TextAsset</returns>
        public TextAsset LoadTextAsset(string path)
        {
            return Load<TextAsset>(path);
        }


        // ====== 인스턴스화 ======

        /// <summary>
        /// GameObject 로드 후 인스턴스화
        /// </summary>
        /// <param name="path">Resources 폴더 기준 경로</param>
        /// <param name="parent">부모 Transform (null이면 루트)</param>
        /// <returns>생성된 GameObject 인스턴스</returns>
        public GameObject Instantiate(string path, Transform parent = null)
        {
            GameObject prefab = LoadPrefab(path);

            if (prefab == null)
            {
                Debug.LogError($"[GameResourceManager] Instantiate 실패: Prefab을 찾을 수 없습니다 - {path}");
                return null;
            }

            GameObject instance = Object.Instantiate(prefab, parent);
            Debug.Log($"[GameResourceManager] 인스턴스 생성: {path}");

            return instance;
        }

        /// <summary>
        /// GameObject 로드 후 위치/회전 지정하여 인스턴스화
        /// </summary>
        /// <param name="path">Resources 폴더 기준 경로</param>
        /// <param name="position">위치</param>
        /// <param name="rotation">회전</param>
        /// <param name="parent">부모 Transform (null이면 루트)</param>
        /// <returns>생성된 GameObject 인스턴스</returns>
        public GameObject Instantiate(string path, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            GameObject prefab = LoadPrefab(path);

            if (prefab == null)
            {
                Debug.LogError($"[GameResourceManager] Instantiate 실패: Prefab을 찾을 수 없습니다 - {path}");
                return null;
            }

            GameObject instance = Object.Instantiate(prefab, position, rotation, parent);
            Debug.Log($"[GameResourceManager] 인스턴스 생성: {path}");

            return instance;
        }


        // ====== 캐시 관리 ======

        /// <summary>
        /// 특정 리소스 캐시 제거
        /// </summary>
        /// <param name="path">리소스 경로</param>
        public void UnloadResource(string path)
        {
            if (resourceCache.ContainsKey(path))
            {
                resourceCache.Remove(path);
                Debug.Log($"[GameResourceManager] 리소스 캐시 제거: {path}");
            }
        }

        /// <summary>
        /// 모든 리소스 캐시 제거
        /// </summary>
        public void UnloadAllResources()
        {
            resourceCache.Clear();
            Debug.Log("[GameResourceManager] 모든 리소스 캐시 제거");
        }

        /// <summary>
        /// 캐시 상태 출력 (디버그용)
        /// </summary>
        [ContextMenu("Print Cache Info")]
        public void PrintCacheInfo()
        {
            Debug.Log("========== Resource Cache Info ==========");
            Debug.Log($"Total Cached Resources: {resourceCache.Count}");

            foreach (var kvp in resourceCache)
            {
                Debug.Log($"  - {kvp.Key} ({kvp.Value.GetType().Name})");
            }

            Debug.Log("=========================================");
        }


        // ====== 유효성 검증 ======

        /// <summary>
        /// 리소스 존재 여부 확인
        /// </summary>
        /// <param name="path">리소스 경로</param>
        /// <returns>존재하면 true</returns>
        public bool Exists(string path)
        {
            // 캐시 확인
            if (resourceCache.ContainsKey(path))
            {
                return true;
            }

            // Resources.Load 시도 (로드는 하지 않음)
            Object resource = Resources.Load(path);
            return resource != null;
        }
    }
}
