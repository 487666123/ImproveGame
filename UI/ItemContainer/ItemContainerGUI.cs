using ImproveGame.UI.ItemContainer.Elements;
using ImproveGame.UIFramework;
using ImproveGame.UIFramework.BaseViews;
using ImproveGame.UIFramework.Common;
using ImproveGame.UIFramework.SUIElements;
using Terraria.GameInput;

namespace ImproveGame.UI.ItemContainer;

[AutoCreateGUI(LayerName.Vanilla.RadialHotbars, "Item Container GUI")]
public class ItemContainerGUI : BaseBody
{
    #region Base
    public static ItemContainerGUI Instace { get; private set; }
    public ItemContainerGUI() => Instace = this;

    public override bool Enabled
    {
        get
        {
            if (_enabled && !Main.playerInventory)
            {
                _enabled = false;
                StartTimer.Close();
            }

            return StartTimer.Closing || _enabled;
        }
        set
        {
            _enabled = value;
            if (_enabled)
                StartTimer.Open();
            else
                StartTimer.Close();
        }
    }
    private static bool _enabled;
    #endregion

    public IItemContainer Container { get; private set; }

    // 窗口
    public SUIPanel Window = new SUIPanel(UIStyle.PanelBorder, UIStyle.PanelBg)
    {
        Shaded = true,
        Draggable = true,
        FinallyDrawBorder = true,
        HAlign = 0.5f, VAlign = 0.5f,
        Width = { Pixels = 300f },
        IsAdaptiveWidth = false, IsAdaptiveHeight = true,
    };

    // 标题
    public readonly View TitlePanel = ViewHelper.CreateHead(UIStyle.TitleBg2 * 0.75f, 45f, 12f);
    public readonly SUIText Title = new SUIText
    {
        DragIgnore = true,
        IsLarge = true,
        TextScale = 0.5f,
        TextAlign = new Vector2(0, 0.5f),
    };
    public readonly SUICross Cross = new SUICross
    {
        HAlign = 1f, VAlign = 0.5f,
        Rounded = new Vector4(0f, 12f, 0f, 0f),
        CrossSize = 22f, CrossRounded = 4.5f * 0.85f,
        BgColor = Color.Transparent,
        Border = 0f, BorderColor = Color.Transparent,
    };

    public readonly SUITriangleIcon SwitchesButton = new()
    {
        Left = StyleDimension.FromPixels(-50),
        HAlign = 1f, VAlign = 0.5f,
        TriangleBeginColor = Color.Gray,
        TriangleEndColor = Color.White,
        TriangleBorderColor = Color.Transparent,
        TriangleBorderHoverColor = Color.Transparent,
        BgColor = Color.Transparent,
        Border = 0f, BorderColor = Color.Transparent,
    };

    // 开关按钮
    public readonly View SwitchView = new View();

    public float SwitchViewMaxHeight;

    public readonly ItemContainerGridLayout ItemContainerGrid = new ItemContainerGridLayout
    {
        RelativeMode = RelativeMode.Vertical,
        Spacing = new Vector2(6f),
    };

    public readonly SUISearchBar SearchBar = new SUISearchBar
    {
        RelativeMode = RelativeMode.Vertical,
        Spacing = new Vector2(6f),
        MarginTop = 4,
        MarginBottom = 4,
        MarginLeft = 4,
        MarginRight = 4,
        HAlign = .5f,
        Width = StyleDimension.FromPixelsAndPercent(-16, 1),
        Height = StyleDimension.FromPixels(30),
        BgColor = Color.Black * .1f
    };

    public override void OnInitialize()
    {
        StartTimer.State = AnimationState.Closed;

        Window.SetPadding(0f);
        Window.JoinParent(this);

        #region 标题组件
        TitlePanel.SetPadding(0);
        TitlePanel.JoinParent(Window);

        Title.SetPadding(12f, 0f, 0f, 0f);
        Title.Height.Percent = 1f;
        Title.Width.Pixels = 200f;
        Title.JoinParent(TitlePanel);

        Cross.CrossOffset += Vector2.One;
        Cross.Width.Pixels = 50f;
        Cross.Height.Set(0f, 1f);
        Cross.OnUpdate += (_) =>
        {
            Cross.BgColor = Cross.HoverTimer.Lerp(Color.Transparent, Color.Black * 0.25f);
        };
        Cross.OnLeftMouseDown += (_, _) => Close();
        Cross.JoinParent(TitlePanel);

        SwitchesButton.JoinParent(TitlePanel);
        SwitchesButton.Width.Pixels = 24f;
        SwitchesButton.Height.Pixels = 24f;

        SwitchesButton.OnLeftClick += delegate
        {
            IsSwitchesVisible = !IsSwitchesVisible;
            if (IsSwitchesVisible)
                SwitchesOpenTimer.Open();
            else
                SwitchesOpenTimer.Close();
        };
        SwitchesButton.OnUpdate += delegate
        {
            if (SwitchesOpenTimer.State is AnimationState.Opened or AnimationState.Closed) return;
            UpdateSwitchView();
        };
        #endregion

        #region Switch
        SwitchView.RelativeMode = RelativeMode.Vertical;
        SwitchView.Spacing = new Vector2(6f);
        SwitchView.Width = new StyleDimension { Pixels = 0, Percent = 1f };
        SwitchView.IsAdaptiveHeight = true;
        SwitchView.SetPadding(12f, 0f);
        SwitchView.JoinParent(Window);

        View switchView1 = SUIToggleSwitch.CreateTextSwitch(out var toggleSwitch, out var text);
        switchView1.Width.Percent = 1f;
        switchView1.IsAdaptiveHeight = true;
        switchView1.RelativeMode = RelativeMode.Vertical;
        switchView1.Spacing = new Vector2(4);

        text.Height.Pixels = 20f;
        text.TextScale = 0.8f;
        text.TextOrKey = MyUtils.GetText("PackageGUI.AutoStorage");
        text.SetInnerPixels(text.TextSize.X, 20f);
        text.SetSizePixels(text.TextSize * text.TextScale);

        toggleSwitch.Status += () => Container.AutoStorage;
        toggleSwitch.Switch += () => Container.AutoStorage = !Container.AutoStorage;
        toggleSwitch.Rounded = new Vector4(10f);
        toggleSwitch.OnUpdate += _ =>
        {
            toggleSwitch.BorderColor =
                toggleSwitch.SwitchTimer.Lerp(UIStyle.PanelBorder, UIStyle.ItemSlotBorderFav);
            toggleSwitch.ToggleCircleColor =
                toggleSwitch.SwitchTimer.Lerp(UIStyle.PanelBorder, UIStyle.ItemSlotBorderFav);
        };
        toggleSwitch.SetSizePixels(32f, 20f);

        switchView1.JoinParent(SwitchView);

        View switchView2 = SUIToggleSwitch.CreateTextSwitch(out var toggleSwitch2, out var text2);
        switchView2.Width.Percent = 1f;
        switchView2.IsAdaptiveHeight = true;
        switchView2.RelativeMode = RelativeMode.Vertical;
        switchView2.Spacing = new Vector2(8);

        text2.TextScale = 0.8f;
        text2.TextOrKey = MyUtils.GetText("PackageGUI.AutoSort");
        text2.SetInnerPixels(text2.TextSize.X, 20f);
        text2.SetSizePixels(text2.TextSize * text2.TextScale);

        toggleSwitch2.Status += () => Container.AutoSort;
        toggleSwitch2.Switch += () => Container.AutoSort = !Container.AutoSort;
        toggleSwitch2.Rounded = new Vector4(10f);
        toggleSwitch2.OnUpdate += _ =>
        {
            toggleSwitch2.BorderColor =
                toggleSwitch2.SwitchTimer.Lerp(UIStyle.PanelBorder, UIStyle.ItemSlotBorderFav);
            toggleSwitch2.ToggleCircleColor =
                toggleSwitch2.SwitchTimer.Lerp(UIStyle.PanelBorder, UIStyle.ItemSlotBorderFav);
        };
        toggleSwitch2.SetSizePixels(32f, 20f);

        switchView2.JoinParent(SwitchView);

        View switchView3 = SUIToggleSwitch.CreateTextSwitch(out var toggleSwitch3, out var text3);
        switchView3.Width.Percent = 1f;
        switchView3.IsAdaptiveHeight = true;
        switchView3.RelativeMode = RelativeMode.Vertical;
        switchView3.Spacing = new Vector2(8);

        text3.TextScale = 0.8f;
        text3.TextOrKey = MyUtils.GetText("PackageGUI.Synthesis");
        text3.SetInnerPixels(text3.TextSize.X, 20f);
        text3.SetSizePixels(text3.TextSize * text3.TextScale);

        toggleSwitch3.Status += () => Container.Synthesis;
        toggleSwitch3.Switch += () => Container.Synthesis = !Container.Synthesis;
        toggleSwitch3.Rounded = new Vector4(10f);
        toggleSwitch3.OnUpdate += _ =>
        {
            toggleSwitch3.BorderColor =
                toggleSwitch3.SwitchTimer.Lerp(UIStyle.PanelBorder, UIStyle.ItemSlotBorderFav);
            toggleSwitch3.ToggleCircleColor =
                toggleSwitch3.SwitchTimer.Lerp(UIStyle.PanelBorder, UIStyle.ItemSlotBorderFav);
        };
        toggleSwitch3.SetSizePixels(32f, 20f);

        switchView3.JoinParent(SwitchView);
        #endregion

        SearchBar.JoinParent(Window);
        SearchBar.SearchBarInner.BgColor = Color.Black * .1f;
        SearchBar.OnSearchContentsChanged += content =>
        {
            if (string.IsNullOrEmpty(content))
                ItemContainerGrid.SetInventory(Container.ItemContainer);
            else
            {
                // 找出所有匹配的选项，并着色（设置Highlighted）
                var sortedOptions = new List<Item>();
                var _allOptions = Container.ItemContainer;
                // 转换成标准字符串搜索输入
                var optionNames = _allOptions
                    .Select(item => item.Name).ToList();
                // 调用DeepSeek写的搜索方法
                var results = MyUtils.TextSearch(content, optionNames);
                // 对结果进行处理
                foreach (MyUtils.SearchResult result in results)
                {
                    var option = _allOptions[result.OriginalIndex];
                    // 将allOptions里的对应元素按照次序生成排序后的列表
                    sortedOptions.Add(option);
                }

                ItemContainerGrid.SetInventory(sortedOptions);
            }
            Recalculate();
        };

        ItemContainerGrid.SetPadding(8f);
        ItemContainerGrid.PaddingTop = 0f;
        ItemContainerGrid.SetSizePixels(0, 172f);
        ItemContainerGrid.OnLeftMouseDown += (_, _) =>
        {
            if (!Main.mouseItem.IsAir && !Main.LocalPlayer.ItemAnimationActive && Container.MeetEntryCriteria(Main.mouseItem))
            {
                Container.ItemIntoContainer(Main.mouseItem);
            }
        };
        ItemContainerGrid.JoinParent(Window);
    }
    private void UpdateSwitchView()
    {
        float factor = SwitchesOpenTimer.Schedule;
        SwitchesButton.trianglePercentCoord[0] = Vector2.Lerp(new Vector2(0.5f, 0f), new Vector2(0f, 0.5f), factor);
        SwitchesButton.trianglePercentCoord[1] = Vector2.Lerp(new Vector2(0f, 0.5f), new Vector2(0.5f, 1f), factor);
        SwitchesButton.trianglePercentCoord[2] = Vector2.Lerp(new Vector2(0.5f, 1f), new Vector2(1f, 0.5f), factor);
        if (SwitchViewMaxHeight == 0)
        {
            SwitchViewMaxHeight = SwitchView.Height();
            SwitchView.IsAdaptiveHeight = false;
        }
        SwitchView.MaxHeight = StyleDimension.FromPixels(SwitchViewMaxHeight * factor);
        SwitchView.Height = StyleDimension.FromPixels(SwitchViewMaxHeight * factor);
        SwitchView.BgColor = Color.Transparent;
        SwitchView.OverflowHidden = true;
        Recalculate();
    }
    public override void Update(GameTime gameTime)
    {
        StartTimer.Update();
        SwitchesOpenTimer.Update();
        base.Update(gameTime);

        if (Window.IsMouseHovering)
        {
            PlayerInput.LockVanillaMouseScroll("ImproveGame: Package GUI");
        }
    }

    public void Open(IItemContainer container)
    {
        Enabled = true;

        UpdateSwitchView();
        SoundEngine.PlaySound(SoundID.MenuOpen);
        MyUtils.OperateInventory(true);
        ItemContainerGrid.SetInventory(container.ItemContainer);
        Title.TextOrKey = container.Name;
        Title.SetInnerPixels(Title.TextSize);
        Container = container;
        SearchBar.SearchBarInner.Text = "";
        Recalculate();
    }

    public void Close()
    {
        Enabled = false;
        SoundEngine.PlaySound(SoundID.MenuClose);
    }

    public override bool CanSetFocusTarget(UIElement target)
    {
        return target != this && Window.IsMouseHovering || Window.IsLeftMousePressed;
    }

    public AnimationTimer StartTimer = new(3);

    public AnimationTimer SwitchesOpenTimer = new(3);
    public bool IsSwitchesVisible;
    public override bool RenderTarget2DDraw => !StartTimer.Opened;
    public override Vector2 RenderTarget2DScale => new Vector2(0.95f + StartTimer * 0.05f);
    public override float RenderTarget2DOpacity => StartTimer.Schedule;
    public override Vector2 RenderTarget2DPosition => Window.GetDimensionsCenter();
    public override Vector2 RenderTarget2DOrigin => Window.GetDimensionsCenter();
}