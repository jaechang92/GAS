using UnityEngine;

namespace Core.Data
{
    /// <summary>
    /// 로드할 리소스의 정보를 담는 클래스
    /// </summary>
    [System.Serializable]
    public class ResourceEntry
    {
        [Tooltip("리소스의 Resources 폴더 기준 경로 (확장자 제외)")]
        public string path;

        [Tooltip("리소스 타입 이름 (예: SkulPhysicsConfig, AudioClip 등)")]
        public string typeName;

        [Tooltip("리소스 설명 (선택사항)")]
        public string description;

        public ResourceEntry(string path, string typeName, string description = "")
        {
            this.path = path;
            this.typeName = typeName;
            this.description = description;
        }
    }
}
