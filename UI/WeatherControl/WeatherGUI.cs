using ImproveGame.Content.Functions.WeatherControl;
using ImproveGame.UI.WeatherControl.Components;
using ImproveGame.UIFramework;
using ImproveGame.UIFramework.BaseViews;
using ImproveGame.UIFramework.Common;
using ImproveGame.UIFramework.SUIElements;

namespace ImproveGame.UI.WeatherControl;

[AutoCreateGUI(LayerName.Vanilla.RadialHotbars, "Weather Control")]
public class WeatherGUI : BaseBody
{
    public static WeatherGUI Instance { get; private set; }
    public WeatherGUI() => Instance = this;
    public override bool Enabled { get => Visible || StartTimer.Closing; set => Visible = value; }
    public static bool Visible { get; private set; }

    public override bool CanSetFocusTarget(UIElement target)
        => (target != this && MainPanel.IsMouseHovering) || MainPanel.IsLeftMousePressed;

    /// <summary>
    /// 启动关闭动画计时器
    /// </summary>
    public AnimationTimer StartTimer = new(3);

    // 主面板
    public SUIPanel MainPanel;
    // 标题面板
    private View TitlePanel;

    // 天气环境
    private WeatherAmbientElement WeatherAmbient;

    // 外部模组项默认网格栏
    private ModdedWeatherItemsView ModdedItemsView;

    // 注册的外部内容栏
    private readonly List<UIElement> _customGroupViews = [];

    public override void OnInitialize()
    {
        // 主面板
        MainPanel = new SUIPanel(UIStyle.PanelBorder, UIStyle.PanelBg)
        {
            Rounded = new Vector4(10f),
            Shaded = true,
            Draggable = true,
            FinallyDrawBorder = true,
            IsAdaptiveHeight = true
        };
        MainPanel.SetPadding(1.5f);
        MainPanel.SetPosPixels(630, 400)
            .SetSizePixels(403, 0)
            .JoinParent(this);

        TitlePanel = ViewHelper.CreateHead(Color.Black * 0.25f, 45f, 10f);
        TitlePanel.SetPadding(0f);
        TitlePanel.JoinParent(MainPanel);

        // 标题
        var title = new SUIText
        {
            IsLarge = true,
            UseKey = true,
            TextOrKey = "Mods.ImproveGame.UI.WeatherGUI.Title",
            TextAlign = new Vector2(0f, 0.5f),
            TextScale = 0.45f,
            Height = StyleDimension.Fill,
            Width = StyleDimension.Fill,
            DragIgnore = true,
            Left = new StyleDimension(16f, 0f)
        };
        title.JoinParent(TitlePanel);

        var cross = new SUICross
        {
            HAlign = 1f,
            Rounded = new Vector4(0f, 10f, 0f, 0f),
            CrossSize = 20f,
            CrossRounded = 4.5f * 0.85f,
            Border = 0f,
            BorderColor = Color.Transparent,
            BgColor = Color.Transparent,
        };
        cross.CrossOffset.X = 1f;
        cross.Width.Pixels = 46f;
        cross.Height.Set(0f, 1f);
        cross.OnUpdate += _ =>
        {
            cross.BgColor = cross.HoverTimer.Lerp(Color.Transparent, Color.Black * 0.25f);
        };
        cross.OnLeftMouseDown += (_, _) => Close();
        cross.JoinParent(TitlePanel);

        var bottomArea = new View
        {
            DragIgnore = true,
            RelativeMode = RelativeMode.Vertical,
            OverflowHidden = true
        };
        bottomArea.SetPadding(0f);
        bottomArea.SetSize(0f, 208f, 1f, 0f);
        bottomArea.JoinParent(MainPanel);

        WeatherAmbient = new WeatherAmbientElement
        {
            RelativeMode = RelativeMode.Vertical
        };
        WeatherAmbient.JoinParent(bottomArea);

        // 把模组项浮窗直接挂到艺术画面里，让外部图标融入场景
        ModdedItemsView = new ModdedWeatherItemsView();
        ModdedItemsView.JoinParent(bottomArea);

        BuildCustomGroupViews();

        Recalculate();
    }

    private void BuildCustomGroupViews()
    {
        foreach (var view in _customGroupViews)
            MainPanel.RemoveChild(view);
        _customGroupViews.Clear();

        var registry = WeatherControlRegistry.Instance;
        if (registry is null) return;

        foreach (var group in registry.SortedGroups())
        {
            if (!group.IsAvailable) continue;
            var controls = registry.ControlsInGroup(group.Id).ToList();
            var view = group.CreateView(controls);
            if (view is null) continue;

            if (view is View v) v.RelativeMode = RelativeMode.Vertical;
            view.Recalculate();

            MainPanel.Append(view);
            _customGroupViews.Add(view);
        }
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        StartTimer.Update();

        if (!MainPanel.IsMouseHovering)
            return;

        Main.LocalPlayer.mouseInterface = true;
    }

    public void Open()
    {
        SoundEngine.PlaySound(SoundID.MenuOpen);
        Visible = true;
        BuildCustomGroupViews();
        Recalculate();
        StartTimer.Open();
    }

    public void Close()
    {
        SoundEngine.PlaySound(SoundID.MenuClose);
        Visible = false;
        WeatherAmbientElement.StarRandomSeed = Main.rand.Next(10000000);
        StartTimer.Close();
    }

    public override bool RenderTarget2DDraw => !StartTimer.Opened;
    public override float RenderTarget2DOpacity => StartTimer.Schedule;
    public override Vector2 RenderTarget2DOrigin => MainPanel.GetDimensionsCenter();
    public override Vector2 RenderTarget2DPosition => MainPanel.GetDimensionsCenter();
    public override Vector2 RenderTarget2DScale => new Vector2(0.95f + StartTimer.Lerp(0, 0.05f));
}