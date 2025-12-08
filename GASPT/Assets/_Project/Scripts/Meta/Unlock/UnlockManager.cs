using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GASPT.Forms;

namespace GASPT.Meta
{
    /// <summary>
    /// 폼/아이템 해금 관리자
    /// Soul을 사용하여 새로운 폼을 해금하고 드롭 풀에 추가
    /// </summary>
    public class UnlockManager : MonoBehaviour
    {
        // ====== 설정 ======

        [Header("리소스 경로")]
        [SerializeField]
        private string unlockablesPath = "Data/Meta/Unlockables";

        [SerializeField]
        private string formsPath = "Data/Forms";


        // ====== 런타임 데이터 ======

        private Dictionary<string, UnlockableForm> unlockables;
        private HashSet<string> unlockedIds;
        private List<FormData> dropPool;


        // ====== 이벤트 ======

        /// <summary>
        /// 폼 해금 완료 이벤트 (해금된 폼)
        /// </summary>
        public event Action<UnlockableForm> OnFormUnlocked;

        /// <summary>
        /// 드롭 풀 변경 이벤트
        /// </summary>
        public event Action OnDropPoolChanged;


        // ====== 프로퍼티 ======

        /// <summary>
        /// 현재 드롭 풀 (해금된 폼들)
        /// </summary>
        public IReadOnlyList<FormData> DropPool => dropPool;

        /// <summary>
        /// 모든 해금 가능 폼
        /// </summary>
        public IEnumerable<UnlockableForm> AllUnlockables => unlockables.Values;

        /// <summary>
        /// 해금된 폼 수
        /// </summary>
        public int UnlockedCount => unlockedIds.Count;

        /// <summary>
        /// 전체 해금 가능 폼 수
        /// </summary>
        public int TotalCount => unlockables.Count;


        // ====== 초기화 ======

        private void Awake()
        {
            unlockables = new Dictionary<string, UnlockableForm>();
            unlockedIds = new HashSet<string>();
            dropPool = new List<FormData>();

            LoadUnlockables();
            LoadUnlockedState();
            RefreshDropPool();
        }

        /// <summary>
        /// Resources에서 해금 가능 폼 로드
        /// </summary>
        private void LoadUnlockables()
        {
            unlockables.Clear();

            // 해금 가능 폼 로드
            UnlockableForm[] loaded = Resources.LoadAll<UnlockableForm>(unlockablesPath);

            foreach (var unlockable in loaded)
            {
                if (unlockable != null && unlockable.IsValid())
                {
                    if (!unlockables.ContainsKey(unlockable.unlockId))
                    {
                        unlockables[unlockable.unlockId] = unlockable;
                    }
                }
            }

            Debug.Log($"[UnlockManager] {unlockables.Count}개 해금 가능 폼 로드 완료");
        }


        // ====== 해금 상태 관리 ======

        /// <summary>
        /// 해금 상태 로드 (MetaProgressionManager에서 가져옴)
        /// </summary>
        private void LoadUnlockedState()
        {
            unlockedIds.Clear();

            var metaManager = MetaProgressionManager.Instance;
            if (metaManager != null && metaManager.Progress != null)
            {
                var savedUnlocks = metaManager.Progress.unlockedForms;
                if (savedUnlocks != null)
                {
                    foreach (string id in savedUnlocks)
                    {
                        if (!string.IsNullOrEmpty(id))
                        {
                            unlockedIds.Add(id);
                        }
                    }
                }
            }

            // 기본 폼은 항상 해금
            AddDefaultUnlocks();

            Debug.Log($"[UnlockManager] {unlockedIds.Count}개 폼 해금 상태 로드");
        }

        /// <summary>
        /// 기본 해금 폼 추가 (Common 등급)
        /// </summary>
        private void AddDefaultUnlocks()
        {
            // 기본 폼들은 해금 없이도 드롭 풀에 포함
            FormData[] allForms = Resources.LoadAll<FormData>(formsPath);

            foreach (var form in allForms)
            {
                if (form != null && form.baseRarity == FormRarity.Common)
                {
                    // Common 등급은 기본 해금
                    string defaultId = $"default_{form.formId}";
                    unlockedIds.Add(defaultId);
                }
            }
        }

        /// <summary>
        /// 해금 상태 저장 (MetaProgressionManager에 저장)
        /// </summary>
        private void SaveUnlockedState()
        {
            var metaManager = MetaProgressionManager.Instance;
            if (metaManager != null && metaManager.Progress != null)
            {
                metaManager.Progress.unlockedForms = unlockedIds.ToList();
                metaManager.SaveProgress();
            }
        }


        // ====== 해금 기능 ======

        /// <summary>
        /// 폼 해금 가능 여부 확인
        /// </summary>
        public bool CanUnlock(string unlockId)
        {
            if (!unlockables.TryGetValue(unlockId, out var unlockable))
            {
                return false;
            }

            return CanUnlock(unlockable);
        }

        /// <summary>
        /// 폼 해금 가능 여부 확인
        /// </summary>
        public bool CanUnlock(UnlockableForm unlockable)
        {
            if (unlockable == null) return false;

            // 이미 해금됨
            if (IsUnlocked(unlockable.unlockId))
            {
                return false;
            }

            // 선행 조건 확인
            if (!CheckPrerequisites(unlockable))
            {
                return false;
            }

            // 클리어 횟수 조건
            var metaManager = MetaProgressionManager.Instance;
            if (metaManager != null)
            {
                if (metaManager.Progress.totalClears < unlockable.requiredClears)
                {
                    return false;
                }

                // Soul 확인
                if (metaManager.Currency.Soul < unlockable.soulCost)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 선행 조건 확인
        /// </summary>
        private bool CheckPrerequisites(UnlockableForm unlockable)
        {
            if (unlockable.prerequisites == null || unlockable.prerequisites.Length == 0)
            {
                return true;
            }

            foreach (var prereq in unlockable.prerequisites)
            {
                if (prereq != null && !IsUnlocked(prereq.unlockId))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 폼 해금 시도
        /// </summary>
        public bool TryUnlock(string unlockId)
        {
            if (!unlockables.TryGetValue(unlockId, out var unlockable))
            {
                Debug.LogError($"[UnlockManager] 해금 ID를 찾을 수 없습니다: {unlockId}");
                return false;
            }

            return TryUnlock(unlockable);
        }

        /// <summary>
        /// 폼 해금 시도
        /// </summary>
        public bool TryUnlock(UnlockableForm unlockable)
        {
            if (!CanUnlock(unlockable))
            {
                Debug.LogWarning($"[UnlockManager] 해금 조건 미충족: {unlockable.unlockId}");
                return false;
            }

            // Soul 소비
            var metaManager = MetaProgressionManager.Instance;
            if (metaManager != null)
            {
                if (!metaManager.Currency.TrySpend(CurrencyType.Soul, unlockable.soulCost))
                {
                    Debug.LogWarning($"[UnlockManager] Soul 부족: {unlockable.soulCost}");
                    return false;
                }
            }

            // 해금 처리
            unlockedIds.Add(unlockable.unlockId);

            Debug.Log($"[UnlockManager] 폼 해금 완료: {unlockable.formData.formName} (ID: {unlockable.unlockId})");

            // 저장
            SaveUnlockedState();

            // 드롭 풀 갱신
            RefreshDropPool();

            // 이벤트 발생
            OnFormUnlocked?.Invoke(unlockable);

            return true;
        }

        /// <summary>
        /// 해금 여부 확인
        /// </summary>
        public bool IsUnlocked(string unlockId)
        {
            return unlockedIds.Contains(unlockId);
        }


        // ====== 드롭 풀 관리 ======

        /// <summary>
        /// 드롭 풀 갱신
        /// </summary>
        public void RefreshDropPool()
        {
            dropPool.Clear();

            // 기본 폼 추가 (Common 등급)
            FormData[] allForms = Resources.LoadAll<FormData>(formsPath);

            foreach (var form in allForms)
            {
                if (form != null && form.baseRarity == FormRarity.Common)
                {
                    dropPool.Add(form);
                }
            }

            // 해금된 폼 추가
            foreach (var kvp in unlockables)
            {
                if (IsUnlocked(kvp.Key) && kvp.Value.formData != null)
                {
                    if (!dropPool.Contains(kvp.Value.formData))
                    {
                        dropPool.Add(kvp.Value.formData);
                    }
                }
            }

            Debug.Log($"[UnlockManager] 드롭 풀 갱신: {dropPool.Count}개 폼");

            OnDropPoolChanged?.Invoke();
        }

        /// <summary>
        /// 가중치 기반 랜덤 폼 선택
        /// </summary>
        public FormData GetRandomFormFromPool()
        {
            if (dropPool.Count == 0)
            {
                Debug.LogWarning("[UnlockManager] 드롭 풀이 비어있습니다.");
                return null;
            }

            // 가중치 계산
            int totalWeight = 0;
            List<(FormData form, int weight)> weightedPool = new List<(FormData, int)>();

            foreach (var form in dropPool)
            {
                int weight = form.dropWeight;

                // 해금 가능 폼인 경우 해당 가중치 사용
                var unlockable = GetUnlockableByForm(form);
                if (unlockable != null)
                {
                    weight = unlockable.dropWeight;
                }

                weightedPool.Add((form, weight));
                totalWeight += weight;
            }

            // 랜덤 선택
            int randomValue = UnityEngine.Random.Range(0, totalWeight);
            int currentWeight = 0;

            foreach (var (form, weight) in weightedPool)
            {
                currentWeight += weight;
                if (randomValue < currentWeight)
                {
                    return form;
                }
            }

            // Fallback
            return dropPool[UnityEngine.Random.Range(0, dropPool.Count)];
        }

        /// <summary>
        /// 폼으로 해금 가능 데이터 찾기
        /// </summary>
        public UnlockableForm GetUnlockableByForm(FormData form)
        {
            if (form == null) return null;

            foreach (var kvp in unlockables)
            {
                if (kvp.Value.formData == form)
                {
                    return kvp.Value;
                }
            }

            return null;
        }


        // ====== 쿼리 ======

        /// <summary>
        /// 해금 가능한 폼 목록 (아직 해금 안 됨)
        /// </summary>
        public IEnumerable<UnlockableForm> GetAvailableUnlocks()
        {
            return unlockables.Values.Where(u => !IsUnlocked(u.unlockId));
        }

        /// <summary>
        /// 구매 가능한 폼 목록 (조건 충족)
        /// </summary>
        public IEnumerable<UnlockableForm> GetPurchasableUnlocks()
        {
            return unlockables.Values.Where(CanUnlock);
        }

        /// <summary>
        /// 해금된 폼 목록
        /// </summary>
        public IEnumerable<UnlockableForm> GetUnlockedForms()
        {
            return unlockables.Values.Where(u => IsUnlocked(u.unlockId));
        }


        // ====== 디버그 ======

        [ContextMenu("Debug: Print Status")]
        private void DebugPrintStatus()
        {
            Debug.Log("========== UnlockManager ==========");
            Debug.Log($"전체: {TotalCount}개, 해금: {UnlockedCount}개");
            Debug.Log($"드롭 풀: {dropPool.Count}개");

            Debug.Log("--- 해금 가능 폼 ---");
            foreach (var unlockable in unlockables.Values)
            {
                string status = IsUnlocked(unlockable.unlockId) ? "해금됨" : "미해금";
                Debug.Log($"  {unlockable.formData?.formName ?? "NULL"}: {status} (Cost: {unlockable.soulCost} Soul)");
            }
            Debug.Log("===================================");
        }

        [ContextMenu("Debug: Unlock All")]
        private void DebugUnlockAll()
        {
            foreach (var unlockable in unlockables.Values)
            {
                if (!IsUnlocked(unlockable.unlockId))
                {
                    unlockedIds.Add(unlockable.unlockId);
                }
            }

            SaveUnlockedState();
            RefreshDropPool();

            Debug.Log("[UnlockManager] 모든 폼 해금 완료");
        }

        [ContextMenu("Debug: Reset Unlocks")]
        private void DebugResetUnlocks()
        {
            unlockedIds.Clear();
            AddDefaultUnlocks();

            SaveUnlockedState();
            RefreshDropPool();

            Debug.Log("[UnlockManager] 해금 상태 초기화");
        }
    }
}
