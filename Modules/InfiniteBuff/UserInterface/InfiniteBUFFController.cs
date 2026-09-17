using ImproveGame.Modules.InfiniteBuff.UserInterface.ViewModel;
using SilkyUIFramework;
using SilkyUIFramework.Attributes;
using SilkyUIFramework.Elements;
using SilkyUIFramework.Extensions;
using SilkyUIFramework.Interfaces;
using Terraria.ModLoader.UI;

namespace ImproveGame.Modules.InfiniteBuff.UserInterface;

[XmlElementMapping("SpawnRateSlider")]
public class SpawnRateSlider : SUISlider
{
    protected override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        base.Draw(gameTime, spriteBatch);

        if (!Thumb.IsMouseHovering && !Thumb.LeftMousePressed) return;
        UICommon.TooltipMouseText($"{InfiniteBuffHelper.RemapSliderToSpawnRate(Orientation == Orientation.Vertical ? Value.Y : Value.X):0.##}");
    }
}

[RegisterUI]
public partial class InfiniteBuffController : BaseBody
{
    public SUIScrollContainer BuffsContainer { get; private set; }
    public override IEnumerable<UIView> BlurElements => [MainContainer, SliderContainer];

    public override bool ContainsPoint(Vector2 point) => SliderContainer is { Invalid: false }
            ? SliderContainer.Bounds.Contains(point) || base.ContainsPoint(point)
            : base.ContainsPoint(point);

    protected override void OnEnterTree() => LocalDataContext = new InfiniteBuffViewModel();
    protected override void OnExitTree()
    {
        (LocalDataContext as IDisposable)?.Dispose();
        LocalDataContext = null;
    }

    protected override void OnInitialize()
    {
        InitializeComponent();

        MainContainer.OverflowHidden = true;
        BuffsContainer = ScrollView.Container;

        Header.ControlTarget = this;

        Title.UseDeathText();

        Close.LeftMouseDown += (_, _) => Enabled = false;
        Close.OnUpdateStatus += (_) => Close.ImageColor = Color.White * Close.HoverTimer.Lerp(0.5f, 1f);

        ResetEditText.OnUpdateStatus += (_) => ResetEditText.ImageColor = Color.White * ResetEditText.HoverTimer.Lerp(0.5f, 1f);
        ResetEditText.LeftMouseDown += (_, _) => InputBox.Text = "";

        var quantity = 3;
        for (int i = 0; i < 3; i++)
        {
            var mark = new UIScaleMarks { Progress = i / (quantity - 1f) }.Join(MarkerContainer);
            mark.Bind(nameof(InfiniteBuffViewModel.SetSliderValueCommand), nameof(UIScaleMarks.Command));
        }
    }

    protected override void UpdateStatus(GameTime gameTime)
    {
        base.UpdateStatus(gameTime);

        (LocalDataContext as IUpdatable)?.Update(gameTime);
        UpdateBuffsContainer();
    }

    private readonly BuffTypesState _typesState = new();

    /// <summary>
    /// 按当前 BuffIds 刷新滚动容器中的 Buff 项
    /// </summary>
    private void UpdateBuffsContainer()
    {
        if (BuffsContainer is null) return;
        var str = InputBox?.Text ?? string.Empty;

        _typesState.Rebuild(str);
        if (!_typesState.ConsumeDirty()) return;

        BuffsContainer.RemoveAllChildren();

        foreach (var type in _typesState.Types)
        {
            BuffsContainer.AddChild(ButtonPool.GetOrAdd(type, key => new SUIBuffButton() { BuffType = key }));
        }
    }

    private readonly Dictionary<int, SUIBuffButton> ButtonPool = [];
}

/// <summary>
/// 标记
/// </summary>
public class UIScaleMarks : UITextView
{
    public UIScaleMarks()
    {
        TextScale = 0.8f;
        TextAlign = new(0.5f);
        FitWidth = false;
        Width = new Dimension(35);
    }

    protected override object CommandParameter => Progress;

    /// <summary>
    /// 代表刻度值，修改随之改变位置和文本
    /// </summary>
    public required float Progress
    {
        get; set
        {
            field = value;
            Text = $"{InfiniteBuffHelper.RemapSliderToSpawnRate(field, 1):0.##}";
            Left = new Anchor(0f, field - 0.5f, 0.5f - field);
        }
    }

    protected override void UpdateStatus(GameTime gameTime)
    {
        base.UpdateStatus(gameTime);

        Text = $"{InfiniteBuffHelper.RemapSliderToSpawnRate(Progress, 1):0.##}";
        TextBorderColor = HoverTimer.Lerp(Color.Black, SUIColor.Highlight);
    }
}

/// <summary>
/// 维护可无限化 Buff 类型的构建状态，负责筛选、收藏优先排序与脏标记管理。
/// </summary>
public class BuffTypesState
{
    /// <summary>
    /// 当前构建得到的 Buff 类型列表。
    /// </summary>
    private readonly List<int> _types = [];

    /// <summary>
    /// 上一次构建结果的快照，用于判断列表是否变化。
    /// </summary>
    private readonly List<int> _typesCache = [];

    /// <summary>
    /// 标记当前列表是否发生变化并需要刷新 UI。
    /// </summary>
    public bool IsDirty { get; private set; }

    /// <summary>
    /// 当前可供 UI 展示的 Buff 类型序列。
    /// </summary>
    public IEnumerable<int> Types => _types;

    /// <summary>
    /// 读取并清除脏标记。
    /// </summary>
    public bool ConsumeDirty() => IsDirty && !(IsDirty = false);

    /// <summary>
    /// 根据筛选关键字重建 Buff 类型列表，并在结果变化时设置脏标记。
    /// </summary>
    /// <param name="filterString">Buff 名称筛选关键字；为空或空白时不过滤。</param>
    public void Rebuild(string filterString)
    {
        // 先重建基础列表：所有已启用“可无限化”的 Buff。
        _types.Clear();

        var player = Main.LocalPlayer;
        if (!player.TryGetModPlayer<InfiniteBuffModPlayer>(out var infinitePlayer)) return;
        var flags = infinitePlayer.ActivationFlags.AsSpan();

        for (int i = 0; i < flags.Length; i++)
        {
            // 仅保留已启用隐藏（可无限）效果的 Buff
            if (!flags[i]) continue;

            if (!string.IsNullOrWhiteSpace(filterString) &&
                !Lang.GetBuffName(i).Contains(filterString)) continue;

            _types.Add(i);
        }

        // 收藏 Buff 置顶，其他 Buff 维持相对顺序。
        var types = infinitePlayer.Favorites.GetBuffTypes();
        var array = _types.OrderBy(x => !types.Contains(x)).ToArray();
        _types.Clear();
        _types.AddRange(array);

        // 与上次快照一致则无需刷新 UI。
        if (_typesCache.SequenceEqual(_types)) return;

        _typesCache.Clear();
        _typesCache.AddRange(_types);

        IsDirty = true;
    }
}
