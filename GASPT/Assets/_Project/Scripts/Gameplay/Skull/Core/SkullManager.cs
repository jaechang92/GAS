using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Skull.Data;
using Skull.Core;
using System.Threading;
using System;

namespace Skull.Core
{
    /// <summary>
    /// 스컬 인벤토리 및 교체 관리 시스템
    /// 플레이어가 보유한 스컬들을 관리하고 교체 기능 제공
    /// </summary>
    public class SkullManager : MonoBehaviour
    {
        [Header("스컬 관리 설정")]
        [SerializeField] private int maxSkullSlots = 2;
        [SerializeField] private bool enableDebugLogs = true;
        [SerializeField] private float switchCooldown = 0.5f;

        [Header("기본 스컬 설정")]
        [SerializeField] private SkullData[] availableSkulls;
        [SerializeField] private SkullType defaultSkullType = SkullType.Default;

        [Header("UI 및 이벤트")]
        [SerializeField] private bool showSkullUI = true;

        // 스컬 슬롯 관리
        private readonly Dictionary<int, ISkullController> skullSlots = new Dictionary<int, ISkullController>();
        private readonly Dictionary<SkullType, ISkullController> skullInstances = new Dictionary<SkullType, ISkullController>();
        private int currentSlotIndex = 0;
        private ISkullController currentSkull;

        // 교체 상태 관리
        private bool isSwitching = false;
        private float lastSwitchTime = 0f;
        private CancellationTokenSource switchCancellationSource;

        // 이벤트
        public event Action<ISkullController, ISkullController> OnSkullChanged;
        public event Action<ISkullController> OnSkullEquipped;
        public event Action<ISkullController> OnSkullUnequipped;
        public event Action<int, ISkullController> OnSlotChanged;

        #region 프로퍼티

        /// <summary>
        /// 현재 활성화된 스컬
        /// </summary>
        public ISkullController CurrentSkull => currentSkull;

        /// <summary>
        /// 현재 슬롯 인덱스
        /// </summary>
        public int CurrentSlotIndex => currentSlotIndex;

        /// <summary>
        /// 스컬 교체 중인지
        /// </summary>
        public bool IsSwitching => isSwitching;

        /// <summary>
        /// 스컬 교체 가능한지
        /// </summary>
        public bool CanSwitch => !isSwitching && Time.time >= lastSwitchTime + switchCooldown;

        /// <summary>
        /// 보유 중인 스컬 개수
        /// </summary>
        public int SkullCount => skullSlots.Count;

        /// <summary>
        /// 최대 스컬 슬롯 수
        /// </summary>
        public int MaxSlots => maxSkullSlots;

        #endregion

        #region Unity 생명주기

        private void Awake()
        {
            InitializeSkullSlots();
        }

        private void Start()
        {
            LoadDefaultSkulls();
            EquipDefaultSkull();
        }

        private void Update()
        {
            HandleInput();
            UpdateCurrentSkull();
        }

        private void OnDestroy()
        {
            CancelCurrentSwitch();
        }

        #endregion

        #region 초기화

        /// <summary>
        /// 스컬 슬롯 초기화
        /// </summary>
        private void InitializeSkullSlots()
        {
            for (int i = 0; i < maxSkullSlots; i++)
            {
                skullSlots[i] = null;
            }

            LogDebug($"스컬 슬롯 초기화 완료: {maxSkullSlots}개 슬롯");
        }

        /// <summary>
        /// 기본 스컬들 로드
        /// </summary>
        private void LoadDefaultSkulls()
        {
            if (availableSkulls == null || availableSkulls.Length == 0)
            {
                LogWarning("사용 가능한 스컬이 없습니다.");
                return;
            }

            int slotIndex = 0;
            foreach (var skullData in availableSkulls)
            {
                if (skullData == null || slotIndex >= maxSkullSlots)
                {
                    continue;
                }

                var skull = CreateSkullInstance(skullData);
                if (skull != null)
                {
                    AddSkullToSlot(slotIndex, skull);
                    slotIndex++;
                }
            }

            LogDebug($"기본 스컬 로드 완료: {slotIndex}개");
        }

        /// <summary>
        /// 기본 스컬 장착
        /// </summary>
        private async void EquipDefaultSkull()
        {
            var defaultSkull = GetSkullByType(defaultSkullType) ?? GetSkullInSlot(0);
            if (defaultSkull != null)
            {
                await SwitchToSkull(defaultSkull);
            }
            else
            {
                LogWarning("장착할 기본 스컬이 없습니다.");
            }
        }

        #endregion

        #region 스컬 인스턴스 관리

        /// <summary>
        /// 스컬 인스턴스 생성
        /// </summary>
        private ISkullController CreateSkullInstance(SkullData skullData)
        {
            if (skullData?.SkullPrefab == null)
            {
                LogWarning($"스컬 프리팹이 없습니다: {skullData?.name}");
                return null;
            }

            try
            {
                var skullObj = Instantiate(skullData.SkullPrefab, transform);
                var skullController = skullObj.GetComponent<ISkullController>();

                if (skullController == null)
                {
                    LogError($"스컬 프리팹에 ISkullController가 없습니다: {skullData.name}");
                    Destroy(skullObj);
                    return null;
                }

                skullInstances[skullData.Type] = skullController;
                LogDebug($"스컬 인스턴스 생성 완료: {skullData.SkullName}");

                return skullController;
            }
            catch (Exception e)
            {
                LogError($"스컬 인스턴스 생성 실패: {skullData.name}, 오류: {e.Message}");
                return null;
            }
        }

        #endregion

        #region 슬롯 관리

        /// <summary>
        /// 스컬을 특정 슬롯에 추가
        /// </summary>
        public bool AddSkullToSlot(int slotIndex, ISkullController skull)
        {
            if (slotIndex < 0 || slotIndex >= maxSkullSlots)
            {
                LogWarning($"잘못된 슬롯 인덱스: {slotIndex}");
                return false;
            }

            if (skull == null)
            {
                LogWarning("null 스컬은 추가할 수 없습니다.");
                return false;
            }

            skullSlots[slotIndex] = skull;
            OnSlotChanged?.Invoke(slotIndex, skull);

            LogDebug($"스컬 슬롯 추가: [{slotIndex}] {skull.SkullData?.SkullName}");
            return true;
        }

        /// <summary>
        /// 특정 슬롯의 스컬 제거
        /// </summary>
        public bool RemoveSkullFromSlot(int slotIndex)
        {
            if (!skullSlots.ContainsKey(slotIndex))
            {
                return false;
            }

            var removedSkull = skullSlots[slotIndex];
            skullSlots[slotIndex] = null;
            OnSlotChanged?.Invoke(slotIndex, null);

            LogDebug($"스컬 슬롯 제거: [{slotIndex}] {removedSkull?.SkullData?.SkullName}");
            return true;
        }

        /// <summary>
        /// 특정 슬롯의 스컬 가져오기
        /// </summary>
        public ISkullController GetSkullInSlot(int slotIndex)
        {
            return skullSlots.ContainsKey(slotIndex) ? skullSlots[slotIndex] : null;
        }

        /// <summary>
        /// 특정 타입의 스컬 가져오기
        /// </summary>
        public ISkullController GetSkullByType(SkullType skullType)
        {
            return skullInstances.ContainsKey(skullType) ? skullInstances[skullType] : null;
        }

        /// <summary>
        /// 모든 보유 스컬 가져오기
        /// </summary>
        public IEnumerable<ISkullController> GetAllSkulls()
        {
            return skullSlots.Values.Where(skull => skull != null);
        }

        #endregion

        #region 스컬 교체

        /// <summary>
        /// 다음 슬롯으로 교체
        /// </summary>
        public async Awaitable SwitchToNextSlot()
        {
            if (!CanSwitch)
            {
                return;
            }

            int nextSlot = (currentSlotIndex + 1) % maxSkullSlots;
            var nextSkull = GetSkullInSlot(nextSlot);

            if (nextSkull != null)
            {
                await SwitchToSlot(nextSlot);
            }
        }

        /// <summary>
        /// 이전 슬롯으로 교체
        /// </summary>
        public async Awaitable SwitchToPreviousSlot()
        {
            if (!CanSwitch)
            {
                return;
            }

            int prevSlot = (currentSlotIndex - 1 + maxSkullSlots) % maxSkullSlots;
            var prevSkull = GetSkullInSlot(prevSlot);

            if (prevSkull != null)
            {
                await SwitchToSlot(prevSlot);
            }
        }

        /// <summary>
        /// 특정 슬롯으로 교체
        /// </summary>
        public async Awaitable SwitchToSlot(int slotIndex)
        {
            var targetSkull = GetSkullInSlot(slotIndex);
            if (targetSkull == null)
            {
                LogWarning($"슬롯 {slotIndex}에 스컬이 없습니다.");
                return;
            }

            currentSlotIndex = slotIndex;
            await SwitchToSkull(targetSkull);
        }

        /// <summary>
        /// 특정 스컬로 교체
        /// </summary>
        public async Awaitable SwitchToSkull(ISkullController targetSkull)
        {
            if (targetSkull == null || targetSkull == currentSkull)
            {
                return;
            }

            if (!CanSwitch)
            {
                LogDebug("스컬 교체 쿨다운 중입니다.");
                return;
            }

            await PerformSkullSwitch(targetSkull);
        }

        /// <summary>
        /// 특정 타입의 스컬로 교체
        /// </summary>
        public async Awaitable SwitchToSkullType(SkullType skullType)
        {
            var targetSkull = GetSkullByType(skullType);
            if (targetSkull != null)
            {
                await SwitchToSkull(targetSkull);
            }
            else
            {
                LogWarning($"스컬 타입 {skullType}을 찾을 수 없습니다.");
            }
        }

        /// <summary>
        /// 실제 스컬 교체 수행
        /// </summary>
        private async Awaitable PerformSkullSwitch(ISkullController targetSkull)
        {
            if (isSwitching)
            {
                CancelCurrentSwitch();
            }

            isSwitching = true;
            switchCancellationSource = new CancellationTokenSource();
            var cancellationToken = switchCancellationSource.Token;

            try
            {
                LogDebug($"스컬 교체 시작: {currentSkull?.SkullData?.SkullName} → {targetSkull.SkullData?.SkullName}");

                var previousSkull = currentSkull;

                // 이전 스컬 비활성화
                if (currentSkull != null)
                {
                    currentSkull.OnDeactivate();
                    await currentSkull.OnUnequip(cancellationToken);
                    OnSkullUnequipped?.Invoke(currentSkull);
                }

                // 새 스컬 활성화
                currentSkull = targetSkull;
                await currentSkull.OnEquip(cancellationToken);
                currentSkull.OnActivate();

                // 이벤트 발생
                OnSkullEquipped?.Invoke(currentSkull);
                OnSkullChanged?.Invoke(previousSkull, currentSkull);

                lastSwitchTime = Time.time;

                LogDebug($"스컬 교체 완료: {currentSkull.SkullData?.SkullName}");
            }
            catch (OperationCanceledException)
            {
                LogDebug("스컬 교체가 취소되었습니다.");
            }
            catch (Exception e)
            {
                LogError($"스컬 교체 중 오류 발생: {e.Message}");
            }
            finally
            {
                isSwitching = false;
                switchCancellationSource?.Dispose();
                switchCancellationSource = null;
            }
        }

        /// <summary>
        /// 현재 진행 중인 교체 취소
        /// </summary>
        private void CancelCurrentSwitch()
        {
            if (switchCancellationSource != null)
            {
                switchCancellationSource.Cancel();
                switchCancellationSource.Dispose();
                switchCancellationSource = null;
            }
        }

        #endregion

        #region 입력 처리

        /// <summary>
        /// 입력 처리
        /// </summary>
        private void HandleInput()
        {
            if (isSwitching)
            {
                return;
            }

            // Q키로 이전 스컬
            if (Input.GetKeyDown(KeyCode.Q))
            {
                _ = SwitchToPreviousSlot();
            }
            // E키로 다음 스컬
            else if (Input.GetKeyDown(KeyCode.E))
            {
                _ = SwitchToNextSlot();
            }
            // 숫자 키로 직접 선택
            else if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                _ = SwitchToSlot(0);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                _ = SwitchToSlot(1);
            }
        }

        #endregion

        #region 업데이트

        /// <summary>
        /// 현재 스컬 업데이트
        /// </summary>
        private void UpdateCurrentSkull()
        {
            if (currentSkull != null && currentSkull.IsActive)
            {
                currentSkull.OnUpdate();
            }
        }

        #endregion

        #region 공개 API

        /// <summary>
        /// 스컬 추가 (런타임)
        /// </summary>
        public bool AddSkull(SkullData skullData)
        {
            if (skullData == null)
            {
                return false;
            }

            // 빈 슬롯 찾기
            for (int i = 0; i < maxSkullSlots; i++)
            {
                if (GetSkullInSlot(i) == null)
                {
                    var skull = CreateSkullInstance(skullData);
                    if (skull != null)
                    {
                        AddSkullToSlot(i, skull);
                        return true;
                    }
                }
            }

            LogWarning("빈 슬롯이 없어 스컬을 추가할 수 없습니다.");
            return false;
        }

        /// <summary>
        /// 스컬 제거
        /// </summary>
        public bool RemoveSkull(SkullType skullType)
        {
            var skull = GetSkullByType(skullType);
            if (skull == null)
            {
                return false;
            }

            // 현재 활성화된 스컬이면 다른 스컬로 교체
            if (skull == currentSkull)
            {
                var nextSkull = GetAllSkulls().FirstOrDefault(s => s != skull);
                if (nextSkull != null)
                {
                    _ = SwitchToSkull(nextSkull);
                }
            }

            // 슬롯에서 제거
            for (int i = 0; i < maxSkullSlots; i++)
            {
                if (GetSkullInSlot(i) == skull)
                {
                    RemoveSkullFromSlot(i);
                    break;
                }
            }

            // 인스턴스에서 제거
            skullInstances.Remove(skullType);

            // GameObject 파괴
            if (skull is MonoBehaviour mb)
            {
                Destroy(mb.gameObject);
            }

            return true;
        }

        /// <summary>
        /// 스컬 정보 가져오기
        /// </summary>
        public SkullStatus GetSkullStatus(int slotIndex)
        {
            var skull = GetSkullInSlot(slotIndex);
            return skull?.GetStatus() ?? SkullStatus.NotReady();
        }

        #endregion

        #region 디버그 및 로깅

        private void LogDebug(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[SkullManager] {message}");
            }
        }

        private void LogWarning(string message)
        {
            Debug.LogWarning($"[SkullManager] {message}");
        }

        private void LogError(string message)
        {
            Debug.LogError($"[SkullManager] {message}");
        }

        #endregion

        #region 에디터 전용

        [ContextMenu("스컬 정보 출력")]
        private void PrintSkullInfo()
        {
            Debug.Log($"=== 스컬 관리자 정보 ===\n" +
                     $"현재 스컬: {currentSkull?.SkullData?.SkullName ?? "없음"}\n" +
                     $"현재 슬롯: {currentSlotIndex}\n" +
                     $"보유 스컬 수: {SkullCount}/{MaxSlots}\n" +
                     $"교체 중: {isSwitching}\n" +
                     $"교체 가능: {CanSwitch}");

            for (int i = 0; i < maxSkullSlots; i++)
            {
                var skull = GetSkullInSlot(i);
                Debug.Log($"슬롯 {i}: {skull?.SkullData?.SkullName ?? "비어있음"}");
            }
        }

        #endregion

        #region GUI 디버그

        private void OnGUI()
        {
            if (!showSkullUI || !enableDebugLogs)
            {
                return;
            }

            GUILayout.BeginArea(new Rect(Screen.width - 320, 10, 300, 200));
            GUILayout.Label("=== 스컬 시스템 ===", new GUIStyle(GUI.skin.label) { fontSize = 16, fontStyle = FontStyle.Bold });

            if (currentSkull != null)
            {
                GUILayout.Label($"현재 스컬: {currentSkull.SkullData?.SkullName}");
                GUILayout.Label($"슬롯: {currentSlotIndex + 1}/{maxSkullSlots}");
                GUILayout.Label($"상태: {(isSwitching ? "교체중" : "준비")}");

                var status = currentSkull.GetStatus();
                GUILayout.Label($"쿨다운: {status.cooldownRemaining:F1}초");
                GUILayout.Label($"마나: {status.manaRemaining:F0}");
            }

            GUILayout.Space(10);
            GUILayout.Label("=== 조작법 ===");
            GUILayout.Label("Q: 이전 스컬");
            GUILayout.Label("E: 다음 스컬");
            GUILayout.Label("1,2: 직접 선택");

            GUILayout.EndArea();
        }

        #endregion

        #region 테스트용 동기 메서드들

        /// <summary>
        /// 테스트용 동기 다음 슬롯 교체
        /// </summary>
        public void SwitchToNextSlotSync()
        {
            if (isSwitching)
            {
                return;
            }

            var nextIndex = (currentSlotIndex + 1) % maxSkullSlots;
            var nextSkull = GetSkullInSlot(nextIndex);

            if (nextSkull != null && nextSkull != currentSkull)
            {
                SetCurrentSlot(nextIndex);
            }
        }

        /// <summary>
        /// 테스트용 동기 이전 슬롯 교체
        /// </summary>
        public void SwitchToPreviousSlotSync()
        {
            if (isSwitching)
            {
                return;
            }

            var prevIndex = (currentSlotIndex - 1 + maxSkullSlots) % maxSkullSlots;
            var prevSkull = GetSkullInSlot(prevIndex);

            if (prevSkull != null && prevSkull != currentSkull)
            {
                SetCurrentSlot(prevIndex);
            }
        }

        /// <summary>
        /// 테스트용 동기 슬롯 설정
        /// </summary>
        public void SetCurrentSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= maxSkullSlots)
            {
                return;
            }

            var targetSkull = GetSkullInSlot(slotIndex);
            if (targetSkull == null)
            {
                return;
            }

            // 이전 스컬 비활성화
            if (currentSkull != null && currentSkull != targetSkull)
            {
                currentSkull.OnDeactivate();
            }

            // 새 스컬 활성화
            currentSkull = targetSkull;
            currentSlotIndex = slotIndex;
            currentSkull.OnActivate();

            // 이벤트 발생
            OnSkullChanged?.Invoke(null, currentSkull);

            LogDebug($"동기 스컬 교체 완료: {currentSkull.SkullData?.SkullName}");
        }

        #endregion
    }
}
