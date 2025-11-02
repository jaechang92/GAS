using System.Collections.Generic;
using UnityEngine;
using GASPT.ResourceManagement;

namespace GASPT.UI
{
    /// <summary>
    /// DamageNumber 오브젝트 풀링 시스템
    /// 성능 최적화를 위해 공용 Canvas 사용 및 DamageNumber 인스턴스 재사용
    /// </summary>
    public class DamageNumberPool : SingletonManager<DamageNumberPool>
    {
        // ====== 풀 설정 ======

        [Header("Pool Settings")]
        [Tooltip("DamageNumber 프리팹 (자동 로딩)")]
        private DamageNumber damageNumberPrefab;

        [SerializeField] [Tooltip("초기 풀 크기")]
        private int initialPoolSize = 20;

        [SerializeField] [Tooltip("최대 풀 크기 (0 = 무제한)")]
        private int maxPoolSize = 50;


        // ====== 공용 Canvas ======

        [Header("Shared Canvas")]
        [SerializeField] [Tooltip("카메라 빌보드 업데이트 주기 (초)")]
        private float billboardUpdateInterval = 0.05f;


        // ====== 풀 관리 ======

        private Queue<DamageNumber> pool = new Queue<DamageNumber>();
        private List<DamageNumber> activeObjects = new List<DamageNumber>();
        private Transform poolParent;
        private Canvas sharedCanvas;
        private Camera mainCamera;
        private float lastBillboardUpdate;


        // ====== Unity 생명주기 ======

        protected override void OnAwake()
        {
            mainCamera = Camera.main;
            LoadDamageNumberPrefab();
            ValidateReferences();
            InitializeSharedCanvas();
            InitializePool();
        }

        private void LateUpdate()
        {
            // 카메라 빌보드 업데이트 (최적화를 위해 일정 주기마다만)
            if (Time.time - lastBillboardUpdate > billboardUpdateInterval)
            {
                UpdateCanvasBillboard();
                lastBillboardUpdate = Time.time;
            }
        }


        // ====== 초기화 ======

        /// <summary>
        /// DamageNumber 프리팹 자동 로드
        /// </summary>
        private void LoadDamageNumberPrefab()
        {
            GameObject prefabObj = GameResourceManager.Instance.LoadPrefab(ResourcePaths.Prefabs.UI.DamageNumber);

            if (prefabObj != null)
            {
                damageNumberPrefab = prefabObj.GetComponent<DamageNumber>();

                if (damageNumberPrefab != null)
                {
                    Debug.Log("[DamageNumberPool] DamageNumber 프리팹 자동 로드 완료");
                }
                else
                {
                    Debug.LogError("[DamageNumberPool] 로드된 프리팹에 DamageNumber 컴포넌트가 없습니다.");
                }
            }
            else
            {
                Debug.LogError($"[DamageNumberPool] DamageNumber 프리팹 로드 실패: {ResourcePaths.Prefabs.UI.DamageNumber}");
            }
        }

        /// <summary>
        /// 참조 유효성 검증
        /// </summary>
        private void ValidateReferences()
        {
            if (damageNumberPrefab == null)
            {
                Debug.LogError("[DamageNumberPool] damageNumberPrefab 로드 실패. Resources 폴더 구조를 확인하세요.");
            }

            if (mainCamera == null)
            {
                Debug.LogWarning("[DamageNumberPool] Main Camera를 찾을 수 없습니다. 빌보드 효과가 작동하지 않을 수 있습니다.");
            }
        }

        /// <summary>
        /// 공용 Canvas 초기화 (성능 최적화: Canvas는 하나만)
        /// </summary>
        private void InitializeSharedCanvas()
        {
            GameObject canvasObj = new GameObject("DamageNumberCanvas");
            canvasObj.transform.SetParent(transform);

            sharedCanvas = canvasObj.AddComponent<Canvas>();
            sharedCanvas.renderMode = RenderMode.WorldSpace;

            // Canvas 크기 설정
            RectTransform canvasRect = canvasObj.GetComponent<RectTransform>();
            canvasRect.sizeDelta = new Vector2(100f, 100f);
            canvasRect.localScale = Vector3.one * 0.01f;

            // 풀 부모를 Canvas로 설정
            poolParent = canvasObj.transform;

            Debug.Log("[DamageNumberPool] 공용 Canvas 생성 완료 (성능 최적화)");
        }

        /// <summary>
        /// Canvas 빌보드 효과 (카메라를 향하도록)
        /// </summary>
        private void UpdateCanvasBillboard()
        {
            if (sharedCanvas != null && mainCamera != null)
            {
                sharedCanvas.transform.LookAt(
                    sharedCanvas.transform.position + mainCamera.transform.rotation * Vector3.forward,
                    mainCamera.transform.rotation * Vector3.up
                );
            }
        }

        /// <summary>
        /// 풀 초기화
        /// </summary>
        private void InitializePool()
        {
            // 초기 오브젝트 생성
            for (int i = 0; i < initialPoolSize; i++)
            {
                CreateNewObject();
            }

            Debug.Log($"[DamageNumberPool] 풀 초기화 완료: {initialPoolSize}개 오브젝트 생성");
        }

        /// <summary>
        /// 새 DamageNumber 오브젝트 생성
        /// </summary>
        private DamageNumber CreateNewObject()
        {
            if (damageNumberPrefab == null)
            {
                Debug.LogError("[DamageNumberPool] damageNumberPrefab이 null입니다.");
                return null;
            }

            DamageNumber newObj = Instantiate(damageNumberPrefab, poolParent);
            newObj.gameObject.SetActive(false);
            pool.Enqueue(newObj);

            return newObj;
        }


        // ====== 풀 관리 ======

        /// <summary>
        /// 풀에서 DamageNumber 가져오기
        /// </summary>
        /// <returns>사용 가능한 DamageNumber 인스턴스</returns>
        public DamageNumber Get()
        {
            DamageNumber obj = null;

            // 풀에 사용 가능한 오브젝트가 있으면 재사용
            if (pool.Count > 0)
            {
                obj = pool.Dequeue();
            }
            // 풀이 비어있으면 새로 생성 (최대 크기 제한 확인)
            else
            {
                if (maxPoolSize > 0 && activeObjects.Count >= maxPoolSize)
                {
                    Debug.LogWarning($"[DamageNumberPool] 최대 풀 크기({maxPoolSize})에 도달했습니다. 가장 오래된 오브젝트를 재사용합니다.");

                    // 가장 오래된 활성 오브젝트 강제 반환
                    if (activeObjects.Count > 0)
                    {
                        obj = activeObjects[0];
                        activeObjects.RemoveAt(0);
                    }
                }
                else
                {
                    obj = CreateNewObject();
                }
            }

            if (obj != null)
            {
                obj.gameObject.SetActive(true);
                activeObjects.Add(obj);
            }

            return obj;
        }

        /// <summary>
        /// 풀로 DamageNumber 반환
        /// </summary>
        /// <param name="obj">반환할 DamageNumber 인스턴스</param>
        public void Return(DamageNumber obj)
        {
            if (obj == null) return;

            obj.gameObject.SetActive(false);
            obj.transform.SetParent(poolParent);

            activeObjects.Remove(obj);
            pool.Enqueue(obj);
        }


        // ====== 편의 메서드 ======

        /// <summary>
        /// 데미지 숫자 표시 (풀에서 가져와서 자동으로 표시)
        /// </summary>
        /// <param name="damage">데미지 값</param>
        /// <param name="position">표시 위치</param>
        /// <param name="isCritical">크리티컬 여부</param>
        public void ShowDamage(int damage, Vector3 position, bool isCritical = false)
        {
            DamageNumber obj = Get();
            if (obj != null)
            {
                obj.ShowDamage(damage, position, isCritical);
            }
        }

        /// <summary>
        /// 회복 숫자 표시
        /// </summary>
        /// <param name="healAmount">회복량</param>
        /// <param name="position">표시 위치</param>
        public void ShowHeal(int healAmount, Vector3 position)
        {
            DamageNumber obj = Get();
            if (obj != null)
            {
                obj.ShowHeal(healAmount, position);
            }
        }

        /// <summary>
        /// 경험치 획득 표시
        /// </summary>
        /// <param name="exp">경험치 값</param>
        /// <param name="position">표시 위치</param>
        public void ShowExp(int exp, Vector3 position)
        {
            DamageNumber obj = Get();
            if (obj != null)
            {
                obj.ShowExp(exp, position);
            }
        }

        /// <summary>
        /// 커스텀 텍스트 표시
        /// </summary>
        /// <param name="text">표시할 텍스트</param>
        /// <param name="position">표시 위치</param>
        /// <param name="color">텍스트 색상</param>
        public void ShowText(string text, Vector3 position, Color color)
        {
            DamageNumber obj = Get();
            if (obj != null)
            {
                obj.ShowText(text, position, color);
            }
        }


        // ====== 유틸리티 ======

        /// <summary>
        /// 현재 풀 상태 출력
        /// </summary>
        [ContextMenu("Print Pool Status")]
        public void DebugPrintPoolStatus()
        {
            Debug.Log("========== DamageNumberPool Status ==========");
            Debug.Log($"Pool Count: {pool.Count}");
            Debug.Log($"Active Count: {activeObjects.Count}");
            Debug.Log($"Total Count: {pool.Count + activeObjects.Count}");
            Debug.Log($"Max Pool Size: {(maxPoolSize > 0 ? maxPoolSize.ToString() : "Unlimited")}");
            Debug.Log($"Canvas Count: 1 (Shared Canvas - 최적화됨)");
            Debug.Log("=============================================");
        }

        /// <summary>
        /// 모든 활성 오브젝트 강제 반환
        /// </summary>
        [ContextMenu("Return All Active Objects")]
        public void ReturnAllActiveObjects()
        {
            // 리스트 복사 (반환하면서 리스트가 변경되므로)
            List<DamageNumber> temp = new List<DamageNumber>(activeObjects);

            foreach (DamageNumber obj in temp)
            {
                Return(obj);
            }

            Debug.Log($"[DamageNumberPool] 모든 활성 오브젝트 반환 완료: {temp.Count}개");
        }


        // ====== Context Menu (테스트용) ======

        [ContextMenu("Test Show Damage")]
        private void TestShowDamage()
        {
            Vector3 testPosition = transform.position + Vector3.up * 2f;
            ShowDamage(100, testPosition, false);
        }

        [ContextMenu("Test Show Critical Damage")]
        private void TestShowCriticalDamage()
        {
            Vector3 testPosition = transform.position + Vector3.up * 2f;
            ShowDamage(250, testPosition, true);
        }

        [ContextMenu("Test Show Multiple Damages")]
        private void TestShowMultipleDamages()
        {
            Vector3 basePosition = transform.position + Vector3.up * 2f;

            for (int i = 0; i < 5; i++)
            {
                ShowDamage(Random.Range(50, 150), basePosition, Random.value > 0.7f);
            }
        }
    }
}
