# MVP_Core

Unity용 범용 MVP (Model-View-Presenter) UI 프레임워크

## 특징

- **프로젝트 독립적**: 특정 게임 로직에 의존하지 않음
- **테스트 가능**: Presenter는 Pure C#로 Unity 없이 테스트 가능
- **확장 가능**: 기본 클래스 상속으로 쉽게 확장
- **Unity 6 호환**: Awaitable 기반 비동기 애니메이션

## 구조

```
MVP_Core/
├── Core/                    # 핵심 인터페이스
│   ├── IView.cs            # View 인터페이스
│   ├── IPresenter.cs       # Presenter 인터페이스
│   └── ViewModelBase.cs    # ViewModel 기본 클래스
├── Views/                   # View 베이스 클래스
│   ├── ViewBase.cs         # MonoBehaviour 기반 View
│   └── IResourceBar.cs     # 리소스 바 인터페이스
├── ViewModels/             # 범용 ViewModel
│   ├── ResourceBarViewModel.cs
│   ├── BuffIconViewModel.cs
│   └── SlotViewModel.cs
├── Presenters/             # Presenter 베이스 클래스
│   └── PresenterBase.cs
├── Config/                 # 설정 ScriptableObject
│   ├── ResourceBarConfig.cs
│   └── UIConfig.cs
├── Animation/              # 애니메이션 유틸리티
│   └── UIAnimationHelper.cs
├── Extensions/             # 확장 메서드
│   └── UIExtensions.cs
└── Examples/               # 사용 예제
    ├── ExampleResourceBarView.cs
    ├── ExampleResourceBarPresenter.cs
    └── ExampleSlotView.cs
```

## 사용 방법

### 1. ResourceBar 구현

```csharp
// View 구현
public class HPBarView : ViewBase, IResourceBar
{
    [SerializeField] private Slider slider;
    [SerializeField] private ResourceBarConfig config;

    public void UpdateBar(ResourceBarViewModel viewModel)
    {
        slider.value = viewModel.Ratio;
    }

    public void FlashColor(Color flash, Color normal, float duration)
    {
        // 플래시 효과
    }

    public void SetBarColor(Color color) { }
}

// Presenter 구현
public class HPBarPresenter : PresenterBase<IResourceBar, PlayerStats>
{
    private readonly ResourceBarConfig config;

    public HPBarPresenter(IResourceBar view, ResourceBarConfig config)
        : base(view)
    {
        this.config = config;
    }

    protected override void OnInitialize() { RefreshView(); }
    protected override void OnDispose() { }

    protected override void RefreshView()
    {
        var vm = new ResourceBarViewModel(
            Model.CurrentHP,
            Model.MaxHP,
            config.GetColorForRatio(Model.CurrentHP / (float)Model.MaxHP),
            "HP");

        View.UpdateBar(vm);
    }
}
```

### 2. 슬롯 구현

```csharp
// View 구현
public class InventorySlotView : ViewBase, ISlotView
{
    [SerializeField] private Image iconImage;

    public int SlotIndex { get; private set; }
    public bool IsSelected { get; private set; }

    public void UpdateSlot(SlotViewModel vm)
    {
        iconImage.sprite = vm.Icon;
        iconImage.enabled = !vm.IsEmpty;
    }

    public void Select() { IsSelected = true; }
    public void Deselect() { IsSelected = false; }
    public void Clear() { iconImage.enabled = false; }
}
```

### 3. 토글 가능한 패널

```csharp
// View 구현
public class InventoryView : ToggleableViewBase, IMessageView
{
    public void ShowError(string msg) { /* ... */ }
    public void ShowSuccess(string msg) { /* ... */ }
    public void ShowInfo(string msg) { /* ... */ }
}

// Presenter 구현
public class InventoryPresenter : ToggleablePresenterBase<InventoryView>
{
    protected override void OnBeforeShow()
    {
        // 데이터 로드
    }
}
```

## 애니메이션 사용

```csharp
// 확장 메서드 사용
await image.FlashColorAsync(Color.red, Color.white, 0.3f);
await canvasGroup.FadeInAsync(0.2f);
await transform.PulseAsync(1.2f, 0.15f);

// 유틸리티 클래스 직접 사용
await UIAnimationHelper.BounceScaleAsync(transform, Vector3.one, 0.3f, 1.1f);
```

## 설정 파일 생성

1. Project 창에서 우클릭
2. Create → MVP_Core → ResourceBarConfig 또는 UIConfig

## 다른 프로젝트에서 사용

1. `MVP_Core` 폴더 전체를 복사
2. 프로젝트의 Model에 맞게 Presenter 구현
3. View 인터페이스를 구현하는 MonoBehaviour 작성

## 라이선스

MIT License
