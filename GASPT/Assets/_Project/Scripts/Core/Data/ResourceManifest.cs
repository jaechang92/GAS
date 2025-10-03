using UnityEngine;
using System.Collections.Generic;
using Core.Enums;

namespace Core.Data
{
    /// <summary>
    /// 특정 카테고리의 리소스 목록을 정의하는 ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "ResourceManifest", menuName = "GASPT/Resource Manifest", order = 0)]
    public class ResourceManifest : ScriptableObject
    {
        [Header("카테고리 정보")]
        [Tooltip("이 매니페스트가 관리하는 리소스 카테고리")]
        public ResourceCategory category;

        [Tooltip("카테고리 표시 이름")]
        public string displayName;

        [Header("리소스 목록")]
        [Tooltip("로드할 리소스 목록")]
        public List<ResourceEntry> resources = new List<ResourceEntry>();

        [Header("로딩 설정")]
        [Tooltip("비동기 로딩 사용 여부")]
        public bool useAsyncLoading = true;

        [Tooltip("로딩 시뮬레이션 최소 시간 (초) - 테스트용")]
        public float minimumLoadTime = 0f;

        /// <summary>
        /// 리소스 개수
        /// </summary>
        public int ResourceCount => resources?.Count ?? 0;

        /// <summary>
        /// 특정 경로의 리소스가 포함되어 있는지 확인
        /// </summary>
        public bool ContainsResource(string path)
        {
            return resources?.Exists(r => r.path == path) ?? false;
        }

        /// <summary>
        /// 리소스 추가
        /// </summary>
        public void AddResource(string path, string typeName, string description = "")
        {
            if (resources == null)
                resources = new List<ResourceEntry>();

            if (!ContainsResource(path))
            {
                resources.Add(new ResourceEntry(path, typeName, description));
            }
        }

        /// <summary>
        /// 리소스 제거
        /// </summary>
        public void RemoveResource(string path)
        {
            resources?.RemoveAll(r => r.path == path);
        }
    }
}
