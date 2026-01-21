using System;
using UnityEngine;

namespace MVP_Core.Examples
{
    /// <summary>
    /// ResourceBar Presenter 구현 예제
    /// MVP_Core를 사용한 HP/Mana 바 Presenter 구현 방법 시연
    ///
    /// 사용 방법:
    /// 1. 프로젝트의 Model(예: PlayerStats)에 맞게 GetCurrentValue(), GetMaxValue() 수정
    /// 2. Model의 이벤트(예: OnDamaged, OnHealed)에 맞게 SubscribeToModelEvents() 수정
    /// </summary>
    /// <typeparam name="TModel">연동할 Model 타입</typeparam>
    public abstract class ExampleResourceBarPresenter<TModel> : PresenterBase<IResourceBar, TModel>
    {
        // ====== 설정 ======

        protected readonly ResourceBarConfig config;

        // ====== 상태 ======

        private float lastValue;

        // ====== 생성자 ======

        protected ExampleResourceBarPresenter(IResourceBar view, ResourceBarConfig config)
            : base(view)
        {
            this.config = config ?? throw new ArgumentNullException(nameof(config));
        }

        // ====== 추상 메서드 (프로젝트별 구현) ======

        /// <summary>
        /// Model에서 현재 값 가져오기
        /// </summary>
        protected abstract float GetCurrentValue();

        /// <summary>
        /// Model에서 최대 값 가져오기
        /// </summary>
        protected abstract float GetMaxValue();

        // ====== PresenterBase 구현 ======

        protected override void OnInitialize()
        {
            // 초기값 설정
            float currentValue = GetCurrentValue();
            lastValue = currentValue;

            // 초기 UI 갱신
            RefreshView();

            Debug.Log($"[{GetType().Name}] Initialized: {config.resourceType}");
        }

        protected override void OnDispose()
        {
            base.OnDispose();
        }

        protected override void RefreshView()
        {
            if (Model == null) return;

            float currentValue = GetCurrentValue();
            float maxValue = GetMaxValue();
            float ratio = maxValue > 0 ? currentValue / maxValue : 0f;

            // 증감 판단
            bool isDecreased = currentValue < lastValue;
            bool isIncreased = currentValue > lastValue;

            // ViewModel 생성
            var viewModel = new ResourceBarViewModel(
                currentValue,
                maxValue,
                config.GetColorForRatio(ratio),
                config.resourceType,
                isDecreased,
                isIncreased);

            // View 업데이트
            View.UpdateBar(viewModel);

            // 이전 값 저장
            lastValue = currentValue;
        }

        // ====== 공용 메서드 ======

        /// <summary>
        /// Model 참조 변경 시 호출
        /// </summary>
        public void OnModelChanged()
        {
            float currentValue = GetCurrentValue();
            lastValue = currentValue;
            RefreshView();
        }

        /// <summary>
        /// 값 변경 이벤트 핸들러 (Model 이벤트에 연결)
        /// </summary>
        protected void OnValueChanged(float newValue)
        {
            RefreshView();
        }

        /// <summary>
        /// 값 변경 이벤트 핸들러 (amount, current, max 형태)
        /// </summary>
        protected void OnValueChanged(int amount, int currentValue, int maxValue)
        {
            RefreshView();
        }
    }

    /// <summary>
    /// 간단한 float 값 기반 ResourceBar Presenter 예제
    /// 테스트 및 프로토타이핑용
    /// </summary>
    public class SimpleResourceBarPresenter : PresenterBase<IResourceBar>
    {
        private readonly ResourceBarConfig config;
        private float currentValue;
        private float maxValue;
        private float lastValue;

        public SimpleResourceBarPresenter(IResourceBar view, ResourceBarConfig config, float maxValue = 100f)
            : base(view)
        {
            this.config = config;
            this.maxValue = maxValue;
            this.currentValue = maxValue;
            this.lastValue = maxValue;
        }

        protected override void OnInitialize()
        {
            RefreshView();
        }

        protected override void OnDispose() { }

        /// <summary>
        /// 값 설정 (0 ~ maxValue)
        /// </summary>
        public void SetValue(float value)
        {
            lastValue = currentValue;
            currentValue = Mathf.Clamp(value, 0f, maxValue);
            RefreshView();
        }

        /// <summary>
        /// 값 증가
        /// </summary>
        public void AddValue(float amount)
        {
            SetValue(currentValue + amount);
        }

        /// <summary>
        /// 값 감소
        /// </summary>
        public void SubtractValue(float amount)
        {
            SetValue(currentValue - amount);
        }

        /// <summary>
        /// 비율로 설정 (0.0 ~ 1.0)
        /// </summary>
        public void SetRatio(float ratio)
        {
            SetValue(ratio * maxValue);
        }

        private void RefreshView()
        {
            float ratio = maxValue > 0 ? currentValue / maxValue : 0f;
            bool isDecreased = currentValue < lastValue;
            bool isIncreased = currentValue > lastValue;

            var viewModel = new ResourceBarViewModel(
                currentValue,
                maxValue,
                config.GetColorForRatio(ratio),
                config.resourceType,
                isDecreased,
                isIncreased);

            View.UpdateBar(viewModel);
        }
    }
}
