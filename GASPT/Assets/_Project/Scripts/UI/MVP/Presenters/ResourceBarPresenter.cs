using System;
using UnityEngine;
using GASPT.Stats;
using Core.Enums;

namespace GASPT.UI
{
    /// <summary>
    /// ResourceBar Presenter (MVP 패턴)
    /// Pure C# - Unity 없이 테스트 가능
    /// 비즈니스 로직: Model(PlayerStats) ↔ View(IResourceBarView) 조율
    /// </summary>
    public class ResourceBarPresenter
    {
        // ====== 참조 ======

        private readonly IResourceBarView view;
        private readonly ResourceBarConfig config;
        private PlayerStats playerStats;

        // ====== 내부 상태 ======

        private int lastValue; // 이전 리소스 값 (증감 판단용)

        // ====== 생성자 ======

        /// <summary>
        /// Presenter 생성자
        /// </summary>
        /// <param name="view">View 인터페이스</param>
        /// <param name="config">리소스 바 설정</param>
        public ResourceBarPresenter(IResourceBarView view, ResourceBarConfig config)
        {
            this.view = view ?? throw new ArgumentNullException(nameof(view));
            this.config = config ?? throw new ArgumentNullException(nameof(config));
        }

        // ====== 초기화 ======

        /// <summary>
        /// Presenter 초기화
        /// </summary>
        public void Initialize(PlayerStats stats)
        {
            if (stats == null)
            {
                Debug.LogError("[ResourceBarPresenter] PlayerStats가 null입니다!");
                return;
            }

            playerStats = stats;

            // 초기값 설정
            int currentValue = GetCurrentValue();
            int maxValue = GetMaxValue();
            lastValue = currentValue;

            // 초기 UI 갱신
            RefreshResourceBar(currentValue, maxValue, false, false);

            // 이벤트 구독
            SubscribeToEvents();

            Debug.Log($"[ResourceBarPresenter] 초기화 완료: Type={config.resourceType}, Value={currentValue}/{maxValue}");
        }

        /// <summary>
        /// Presenter 해제
        /// </summary>
        public void Dispose()
        {
            UnsubscribeFromEvents();
            playerStats = null;
        }

        // ====== 이벤트 구독 ======

        private void SubscribeToEvents()
        {
            if (playerStats == null) return;

            switch (config.resourceType)
            {
                case ResourceType.HP:
                    playerStats.OnDamaged += OnResourceDecreased;
                    playerStats.OnHealed += OnResourceIncreased;
                    playerStats.OnStatsChanged += OnStatsChanged;
                    playerStats.OnDeath += OnPlayerDeath;
                    break;

                case ResourceType.Mana:
                    playerStats.OnStatsChanged += OnStatsChanged;
                    break;
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (playerStats == null) return;

            switch (config.resourceType)
            {
                case ResourceType.HP:
                    playerStats.OnDamaged -= OnResourceDecreased;
                    playerStats.OnHealed -= OnResourceIncreased;
                    playerStats.OnStatsChanged -= OnStatsChanged;
                    playerStats.OnDeath -= OnPlayerDeath;
                    break;

                case ResourceType.Mana:
                    playerStats.OnStatsChanged -= OnStatsChanged;
                    break;
            }
        }

        // ====== 이벤트 핸들러 ======

        /// <summary>
        /// 리소스 감소 (데미지, 마나 소모)
        /// </summary>
        private void OnResourceDecreased(int amount, int currentValue, int maxValue)
        {
            RefreshResourceBar(currentValue, maxValue, true, false);
            lastValue = currentValue;
        }

        /// <summary>
        /// 리소스 증가 (회복, 마나 회복)
        /// </summary>
        private void OnResourceIncreased(int amount, int currentValue, int maxValue)
        {
            RefreshResourceBar(currentValue, maxValue, false, true);
            lastValue = currentValue;
        }

        /// <summary>
        /// 스탯 변경 (MaxHP, MaxMana, CurrentMana 등)
        /// </summary>
        private void OnStatsChanged(StatType statType, int oldValue, int newValue)
        {
            // 해당 리소스 타입만 처리
            bool isRelevant = false;

            switch (config.resourceType)
            {
                case ResourceType.HP:
                    isRelevant = (statType == StatType.HP); // MaxHP 변경
                    break;

                case ResourceType.Mana:
                    isRelevant = (statType == StatType.Mana); // CurrentMana 또는 MaxMana 변경
                    break;
            }

            if (!isRelevant) return;

            int currentValue = GetCurrentValue();
            int maxValue = GetMaxValue();

            // Mana의 경우 증감 판단 (HP는 OnDamaged/OnHealed에서 처리)
            bool isDecreased = false;
            bool isIncreased = false;

            if (config.resourceType == ResourceType.Mana)
            {
                isDecreased = (newValue < lastValue);
                isIncreased = (newValue > lastValue);
            }

            RefreshResourceBar(currentValue, maxValue, isDecreased, isIncreased);
            lastValue = currentValue;
        }

        /// <summary>
        /// 플레이어 사망 (HP = 0)
        /// </summary>
        private void OnPlayerDeath()
        {
            int maxValue = GetMaxValue();
            RefreshResourceBar(0, maxValue, false, false);

            // 사망 시 위험 색상으로 고정
            view.SetBarColor(config.criticalColor);
        }

        // ====== 비즈니스 로직 ======

        /// <summary>
        /// 리소스 바 갱신 (ViewModel 변환 + View 업데이트)
        /// </summary>
        private void RefreshResourceBar(int currentValue, int maxValue, bool isDecreased, bool isIncreased)
        {
            // 색상 계산
            float ratio = maxValue > 0 ? (float)currentValue / maxValue : 0f;
            Color barColor = config.GetColorForRatio(ratio);

            // ViewModel 생성
            var viewModel = new ResourceBarViewModel(
                currentValue,
                maxValue,
                barColor,
                config.resourceType,
                isDecreased,
                isIncreased
            );

            // View 업데이트
            view.UpdateResourceBar(viewModel);

            // 플래시 효과
            if (isDecreased)
            {
                view.FlashColor(config.decreaseFlashColor, barColor, config.flashDuration);
            }
            else if (isIncreased)
            {
                view.FlashColor(config.increaseFlashColor, barColor, config.flashDuration);
            }

            Debug.Log($"[ResourceBarPresenter] RefreshResourceBar: {viewModel}");
        }

        /// <summary>
        /// 현재 리소스 값 가져오기
        /// </summary>
        private int GetCurrentValue()
        {
            if (playerStats == null) return 0;

            return config.resourceType switch
            {
                ResourceType.HP => playerStats.CurrentHP,
                ResourceType.Mana => playerStats.CurrentMana,
                _ => 0
            };
        }

        /// <summary>
        /// 최대 리소스 값 가져오기
        /// </summary>
        private int GetMaxValue()
        {
            if (playerStats == null) return 100;

            return config.resourceType switch
            {
                ResourceType.HP => playerStats.MaxHP,
                ResourceType.Mana => playerStats.MaxMana,
                _ => 100
            };
        }

        // ====== Public 메서드 ======

        /// <summary>
        /// PlayerStats 참조 변경 (씬 전환 시)
        /// </summary>
        public void SetPlayerStats(PlayerStats stats)
        {
            // 기존 이벤트 구독 해제
            UnsubscribeFromEvents();

            playerStats = stats;

            // 새 이벤트 구독
            SubscribeToEvents();

            // UI 갱신
            if (playerStats != null)
            {
                int currentValue = GetCurrentValue();
                int maxValue = GetMaxValue();
                lastValue = currentValue;
                RefreshResourceBar(currentValue, maxValue, false, false);
            }

            Debug.Log($"[ResourceBarPresenter] PlayerStats 참조 변경: {stats?.name ?? "null"}");
        }

        /// <summary>
        /// 강제 UI 갱신
        /// </summary>
        public void ForceRefresh()
        {
            int currentValue = GetCurrentValue();
            int maxValue = GetMaxValue();
            RefreshResourceBar(currentValue, maxValue, false, false);
        }
    }
}
