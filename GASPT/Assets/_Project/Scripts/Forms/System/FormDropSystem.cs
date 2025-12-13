using System.Collections.Generic;
using UnityEngine;
using GASPT.Meta;

namespace GASPT.Gameplay.Form
{
    /// <summary>
    /// 폼 드롭 시스템
    /// UnlockManager와 연동하여 해금된 폼만 드롭
    /// </summary>
    public class FormDropSystem : MonoBehaviour
    {
        // ====== 싱글톤 ======

        private static FormDropSystem instance;
        public static FormDropSystem Instance => instance;


        // ====== 설정 ======

        [Header("드롭 설정")]
        [Tooltip("기본 드롭 확률 (0~1)")]
        [Range(0f, 1f)]
        [SerializeField] private float baseDropRate = 0.3f;

        [Tooltip("엘리트 적 드롭 확률 배율")]
        [SerializeField] private float eliteDropMultiplier = 2f;

        [Tooltip("보스 드롭 확률 배율")]
        [SerializeField] private float bossDropMultiplier = 5f;

        [Header("폼 데이터 경로")]
        [SerializeField] private string formsResourcePath = "Data/Forms";

        [Header("Fallback")]
        [Tooltip("UnlockManager 없을 때 사용할 기본 폼들")]
        [SerializeField] private List<FormData> fallbackForms;


        // ====== 런타임 ======

        private UnlockManager unlockManager;
        private List<FormData> cachedDropPool;


        // ====== Unity 생명주기 ======

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            cachedDropPool = new List<FormData>();
        }

        private void Start()
        {
            // UnlockManager 찾기
            unlockManager = FindAnyObjectByType<UnlockManager>();

            if (unlockManager != null)
            {
                unlockManager.OnDropPoolChanged += RefreshCachedPool;
                RefreshCachedPool();
            }
            else
            {
                // Fallback: 모든 Common 폼 사용
                LoadFallbackPool();
            }
        }

        private void OnDestroy()
        {
            if (unlockManager != null)
            {
                unlockManager.OnDropPoolChanged -= RefreshCachedPool;
            }

            if (instance == this)
            {
                instance = null;
            }
        }


        // ====== 드롭 풀 관리 ======

        /// <summary>
        /// 캐시된 드롭 풀 갱신
        /// </summary>
        private void RefreshCachedPool()
        {
            cachedDropPool.Clear();

            if (unlockManager != null && unlockManager.DropPool != null)
            {
                cachedDropPool.AddRange(unlockManager.DropPool);
            }
            else
            {
                LoadFallbackPool();
            }

            Debug.Log($"[FormDropSystem] 드롭 풀 갱신: {cachedDropPool.Count}개");
        }

        /// <summary>
        /// Fallback 드롭 풀 로드
        /// </summary>
        private void LoadFallbackPool()
        {
            cachedDropPool.Clear();

            // 설정된 fallback 폼 사용
            if (fallbackForms != null && fallbackForms.Count > 0)
            {
                cachedDropPool.AddRange(fallbackForms);
            }
            else
            {
                // Resources에서 Common 등급 폼 로드
                FormData[] allForms = Resources.LoadAll<FormData>(formsResourcePath);

                foreach (var form in allForms)
                {
                    if (form != null && form.baseRarity == FormRarity.Common)
                    {
                        cachedDropPool.Add(form);
                    }
                }
            }

            Debug.Log($"[FormDropSystem] Fallback 드롭 풀 로드: {cachedDropPool.Count}개");
        }


        // ====== 드롭 기능 ======

        /// <summary>
        /// 폼 드롭 시도 (일반 적)
        /// </summary>
        public bool TryDropForm(Vector3 position)
        {
            return TryDropFormInternal(position, baseDropRate);
        }

        /// <summary>
        /// 폼 드롭 시도 (엘리트 적)
        /// </summary>
        public bool TryDropFormElite(Vector3 position)
        {
            return TryDropFormInternal(position, baseDropRate * eliteDropMultiplier);
        }

        /// <summary>
        /// 폼 드롭 시도 (보스)
        /// </summary>
        public bool TryDropFormBoss(Vector3 position)
        {
            return TryDropFormInternal(position, baseDropRate * bossDropMultiplier);
        }

        /// <summary>
        /// 폼 강제 드롭 (100% 확률)
        /// </summary>
        public FormPickup ForceDropForm(Vector3 position)
        {
            return DropRandomForm(position);
        }

        /// <summary>
        /// 특정 폼 드롭
        /// </summary>
        public FormPickup DropSpecificForm(FormData formData, Vector3 position)
        {
            if (formData == null)
            {
                Debug.LogWarning("[FormDropSystem] 폼 데이터가 null입니다.");
                return null;
            }

            return FormPickup.CreateFormPickup(formData, position);
        }

        /// <summary>
        /// 내부 드롭 시도
        /// </summary>
        private bool TryDropFormInternal(Vector3 position, float dropRate)
        {
            float roll = Random.value;

            if (roll > dropRate)
            {
                return false;
            }

            var pickup = DropRandomForm(position);
            return pickup != null;
        }

        /// <summary>
        /// 랜덤 폼 드롭
        /// </summary>
        private FormPickup DropRandomForm(Vector3 position)
        {
            FormData selectedForm = GetRandomFormFromPool();

            if (selectedForm == null)
            {
                Debug.LogWarning("[FormDropSystem] 드롭 풀이 비어있습니다.");
                return null;
            }

            return FormPickup.CreateFormPickup(selectedForm, position);
        }


        // ====== 랜덤 선택 ======

        /// <summary>
        /// 드롭 풀에서 가중치 기반 랜덤 선택
        /// </summary>
        public FormData GetRandomFormFromPool()
        {
            // UnlockManager가 있으면 그쪽 메서드 사용
            if (unlockManager != null)
            {
                return unlockManager.GetRandomFormFromPool();
            }

            // Fallback: 단순 랜덤
            if (cachedDropPool.Count == 0)
            {
                return null;
            }

            return cachedDropPool[Random.Range(0, cachedDropPool.Count)];
        }

        /// <summary>
        /// 특정 등급 이상의 폼 랜덤 선택
        /// </summary>
        public FormData GetRandomFormOfRarity(FormRarity minRarity)
        {
            List<FormData> filtered = new List<FormData>();

            foreach (var form in cachedDropPool)
            {
                if (form.baseRarity >= minRarity)
                {
                    filtered.Add(form);
                }
            }

            if (filtered.Count == 0)
            {
                return null;
            }

            return filtered[Random.Range(0, filtered.Count)];
        }


        // ====== 유틸리티 ======

        /// <summary>
        /// 현재 드롭 풀 크기
        /// </summary>
        public int DropPoolCount => cachedDropPool.Count;

        /// <summary>
        /// 드롭 풀에 특정 폼이 있는지 확인
        /// </summary>
        public bool IsInDropPool(FormData form)
        {
            return cachedDropPool.Contains(form);
        }


        // ====== 디버그 ======

        [ContextMenu("Debug: Print Drop Pool")]
        private void DebugPrintDropPool()
        {
            Debug.Log("========== FormDropSystem ==========");
            Debug.Log($"드롭 풀: {cachedDropPool.Count}개");

            foreach (var form in cachedDropPool)
            {
                Debug.Log($"  - {form.formName} ({form.baseRarity})");
            }

            Debug.Log("=====================================");
        }

        [ContextMenu("Debug: Force Drop Random Form")]
        private void DebugForceDropRandomForm()
        {
            ForceDropForm(transform.position);
        }
    }
}
