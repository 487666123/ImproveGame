using ImproveGame.Packets.NetChest;
using ImproveGame.UI.ExtremeStorage;
using ImproveGame.UIFramework;
using ImproveGame.UIFramework.BaseViews;
using ImproveGame.UIFramework.Common;
using ImproveGame.UIFramework.SUIElements;
using Terraria.GameInput;

namespace ImproveGame.UI.ItemSearcher;

[AutoCreateGUI(LayerName.Vanilla.RadialHotbars, "Item Searcher GUI")]
public class ItemSearcherGUI : BaseBody
{
    public static ItemSearcherGUI Instance { get; private set; }

    public ItemSearcherGUI() => Instance = this;

    public override bool IsNotSelectable => StartTimer.AnyClose;

    public override bool Enabled
    {
        get => StartTimer.Closing || _enabled;
        set => _enabled = value;
    }

    private bool _enabled;

    public override bool CanSetFocusTarget(UIElement target)
        => (target != this && MainPanel.IsMouseHovering) || MainPanel.IsLeftMousePressed;

    /// <summary>
    /// 启动关闭动画计时器
    /// </summary>
    public AnimationTimer StartTimer = new(3);

    // 服务器相关，是否正在向服务器请求物品
    private int _requestsSent; // 发送了多少个还没有结果的请求，只有该值为0才会更新物品列表
    private int _refreshTimer; // 如果请求超过6秒还没有清零，则强制清零且重新请求
    private int _updateTimer; // 每 1s 更新一次，单人模式下是 1/4 秒

    private HashSet<int> _oldItemTypes; // 记录上一次的ItemTypes以决定要不要更新UI

    private HashSet<short> _matchedChests; // 查找到的箱子

    // 给玩家显示的查找到的箱子，如果没搜索文字当然没东西咯
    public static HashSet<short> MatchedChests
    {
        get
        {
            if (Instance is null) return new HashSet<short>();

            var gui = Instance;
            if (string.IsNullOrEmpty(gui.SearchContent) || gui.SearchContent.Length is 0)
                return [];

            return gui._matchedChests;
        }
    }

    // 主面板
    public SUIPanel MainPanel;

    // 标题面板
    private View TitlePanel;

    // 银行槽
    private List<BankSlot> BankSlots;

    // 搜索到的物品和滚动条(隐藏)
    public BaseGrid ItemsFoundGrid;
    public SUIScrollBar Scrollbar;
    public UIText TipText;

    // 搜索栏
    public string SearchContent => _searchBar.SearchContent;
    private SUISearchBar _searchBar;

    // 搜索选项开关
    private View ButtonsBox;
    private LongSwitch FuzzySwitch, TooltipSwitch;

    public override void OnInitialize()
    {
        void MakeSeparator()
        {
            View searchArea = new()
            {
                Height = new StyleDimension(10f, 0f),
                Width = new StyleDimension(-16f, 1f),
                HAlign = 0.5f,
                DragIgnore = true,
                RelativeMode = RelativeMode.Vertical,
                Spacing = new Vector2(0, 6)
            };
            searchArea.JoinParent(MainPanel);
            searchArea.Append(new UIHorizontalSeparator
            {
                Width = StyleDimension.FromPercent(1f),
                Color = Color.Lerp(Color.White, new Color(63, 65, 151, 255), 0.85f) * 0.9f
            });
        }

        // 主面板
        MainPanel = new SUIPanel(UIStyle.PanelBorder, UIStyle.PanelBg)
        {
            Shaded = true,
            Draggable = true,
            IsAdaptiveHeight = true
        };
        MainPanel.SetPadding(0f);
        MainPanel.SetPosPixels(620, 400)
            .SetSizePixels(356, 0)
            .JoinParent(this);

        TitlePanel = ViewHelper.CreateHead(Color.Black * 0.25f, 45f, 10f);
        TitlePanel.SetPadding(0f);
        TitlePanel.JoinParent(MainPanel);

        // 标题
        var title = new SUIText
        {
            IsLarge = true,
            UseKey = true,
            TextOrKey = "Mods.ImproveGame.UI.ItemSearcher.Title",
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

        _searchBar = new SUISearchBar(false, false)
        {
            Height = new StyleDimension(28f, 0f),
            Width = new StyleDimension(-26f, 1f),
            HAlign = 0.5f,
            DragIgnore = true,
            RelativeMode = RelativeMode.Vertical,
            Spacing = new Vector2(0, 6)
        };
        // _searchBar.OnDraw += SearchBarOnDraw;
        _searchBar.OnSearchContentsChanged += _ => SetupSearchResults();
        _searchBar.JoinParent(MainPanel);
        SetupButtonsBox();

        // 分割
        MakeSeparator();

        var banksPanel = new View
        {
            DragIgnore = true,
            RelativeMode = RelativeMode.Vertical
        };
        banksPanel.SetPadding(20, 6, 16, 4);
        banksPanel.SetSize(0f, 80f, 1f, 0f);
        banksPanel.JoinParent(MainPanel);
        SetupBanks(banksPanel);

        // 分割
        MakeSeparator();

        var itemsPanel = new View
        {
            DragIgnore = true,
            RelativeMode = RelativeMode.Vertical,
            OverflowHidden = true
        };
        itemsPanel.SetPadding(15, 0, 14, 14);
        itemsPanel.SetSize(0f, 138f, 1f, 0f);
        itemsPanel.JoinParent(MainPanel);

        // 没搜到物品时显示的提示，这里先Append，要用到的时候调一下Left就行
        TipText = new UIText(MyUtils.GetText("UI.ItemSearcher.TipText"))
        {
            Width = { Percent = 1f },
            Height = { Percent = 1f },
            TextOriginX = 0.5f,
            TextOriginY = 0.5f
        };
        itemsPanel.Append(TipText);

        ItemsFoundGrid = new BaseGrid();
        ItemsFoundGrid.SetBaseValues(-1, 7, new Vector2(4f), new Vector2(43));
        ItemsFoundGrid.JoinParent(itemsPanel);

        Scrollbar = new SUIScrollBar { HAlign = 1f };
        Scrollbar.Left.Pixels = -1;
        Scrollbar.Height.Pixels = ItemsFoundGrid.Height();
        Scrollbar.SetView(itemsPanel.GetInnerSizePixels().Y, ItemsFoundGrid.Height.Pixels);
        Scrollbar.JoinParent(this);
        RefreshItemsFoundGrid(new HashSet<int>());
    }

    private void SetupButtonsBox()
    {
        ButtonsBox = new View
        {
            Height = new StyleDimension(76f, 0f),
            Width = new StyleDimension(-26f, 1f),
            HAlign = 0.5f,
            DragIgnore = true,
            RelativeMode = RelativeMode.Vertical,
            Spacing = new Vector2(0, 8)
        };
        ButtonsBox.SetPadding(0f);
        ButtonsBox.JoinParent(MainPanel);

        // 开关
        UIPlayerSetting setting = Main.LocalPlayer.GetModPlayer<UIPlayerSetting>();
        Vector2 switchSpacing = new(0, 6);
        FuzzySwitch = new LongSwitch(
            () => setting.FuzzySearch,
            state => setting.FuzzySearch = state,
            "UI.ItemSearcher.FuzzySearch")
        {
            ResetAnotherPosition = true,
            RelativeMode = RelativeMode.Vertical,
            Spacing = switchSpacing,
            Height = { Pixels = 34f }
        };
        FuzzySwitch.JoinParent(ButtonsBox);

        TooltipSwitch = new LongSwitch(
            () => setting.SearchTooltip,
            state => setting.SearchTooltip = state,
            "UI.ItemSearcher.SearchTooltip")
        {
            RelativeMode = RelativeMode.Vertical,
            Spacing = switchSpacing,
            Height = { Pixels = 34f }
        };
        TooltipSwitch.JoinParent(ButtonsBox);
    }

    private void SetupBanks(UIElement parent)
    {
        BankSlots = new List<BankSlot>
        {
            new(BankType.Piggy, 0, this),
            new(BankType.Safe, 82, this),
            new(BankType.Forge, 164, this),
            new(BankType.Void, 246, this)
        };
        BankSlots.ForEach(parent.Append);
    }

    private void SetupSearchResults()
    {
        HandleSearchContentUpdate();
    }

    /// <summary>
    /// 客户端请求搜索
    /// </summary>
    private void HandleSearchContentUpdate()
    {
        if (!string.IsNullOrEmpty(SearchContent) && SearchContent.Length > 0)
        {
            _requestsSent++;
            SearchItemsNearbyPacket.Request(SearchContent.ToLower().Replace(" ", "", StringComparison.Ordinal));
        }
        else
        {
            RefreshItemsFoundGrid(new HashSet<int>());
        }
    }

    public static void TryUpdateItems(HashSet<short> matchedChests, HashSet<int> itemTypes)
    {
        // Main.NewText("Update " + matchedChests.Count);
        if (Instance is null) return;

        var gui = Instance;
        gui._requestsSent--;
        if (gui._requestsSent > 0)
            return;

        gui._matchedChests = matchedChests;

        // 没有搜索内容就直接不发包了，所以这里为了避免延迟冲突要再次检测
        if (string.IsNullOrEmpty(gui.SearchContent) || gui.SearchContent.Length is 0)
            return;

        if (gui._oldItemTypes is not null && gui._oldItemTypes.SequenceEqual(itemTypes))
            return;

        gui._requestsSent = 0;
        gui.RefreshItemsFoundGrid(itemTypes);
    }

    private void RefreshItemsFoundGrid(HashSet<int> itemTypes)
    {
        float oldHeight = ItemsFoundGrid.Height.Pixels;
        _oldItemTypes = itemTypes;

        ItemsFoundGrid.RemoveAllChildren();

        foreach (int type in itemTypes)
        {
            var itemSlot = new BaseItemSlot();
            itemSlot.SetBaseItemSlotValues(true, false);
            itemSlot.SetSizePixels(43, 43);
            itemSlot.ItemIconMaxWidthAndHeight = 27;
            itemSlot.AirItem.SetDefaults(type);
            itemSlot.JoinParent(ItemsFoundGrid);
        }

        // 控制提示文本是否显示及其内容
        if (itemTypes.Count is 0)
        {
            TipText.SetText(MyUtils.GetText("UI.ItemSearcher.NotFoundText"));
            TipText.Left.Pixels = 0;
        }
        else
        {
            TipText.Left.Pixels = -8888;
        }

        if (string.IsNullOrEmpty(SearchContent) || SearchContent.Length <= 0)
        {
            TipText.Left.Pixels = 0;
            TipText.SetText(MyUtils.GetText("UI.ItemSearcher.TipText"));
        }

        TipText.Recalculate();
        ItemsFoundGrid.CalculateAndSetSize();
        ItemsFoundGrid.CalculateAndSetChildrenPosition();
        ItemsFoundGrid.Recalculate();
        if (oldHeight != ItemsFoundGrid.Height.Pixels)
            Scrollbar.SetView(ItemsFoundGrid.Parent.GetInnerSizePixels().Y, ItemsFoundGrid.Height.Pixels);
    }

    public override void ScrollWheel(UIScrollWheelEvent evt)
    {
        base.ScrollWheel(evt);

        if (!MainPanel.IsMouseHovering)
            return;
        // 下滑: -, 上滑: +
        Scrollbar.BarTopBuffer += evt.ScrollWheelValue * 0.4f;
    }

    public override void DrawSelf(SpriteBatch spriteBatch)
    {
        if (-Scrollbar.BarTop == ItemsFoundGrid.DatumPoint.Y)
            return;

        ItemsFoundGrid.DatumPoint.Y = -Scrollbar.BarTop;
        ItemsFoundGrid.Recalculate();
    }

    public override void DrawChildren(SpriteBatch spriteBatch)
    {
        base.DrawChildren(spriteBatch);

        // 不被遮挡
        BankSlots.ForEach(s => s.DrawItems());

        if (_searchBar.IsWritingText)
        {
            Vector2 position = _searchBar.GetDimensions().Position();
            position.Y += 60f;
            Main.instance.SetIMEPanelAnchor(position, 0f);
        }
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        StartTimer.Update();

        if (_requestsSent > 0)
        {
            // 超时刷新
            _refreshTimer++;
            if (_refreshTimer > 60 * 6)
            {
                _refreshTimer = 0;
                _requestsSent = 0;
                HandleSearchContentUpdate();
            }
        }
        else
        {
            _refreshTimer = 0;
            // 每 1s 更新一次，单人模式下是 1/4s
            int updateDelay = Main.netMode is NetmodeID.SinglePlayer ? 15 : 60;
            _updateTimer++;
            if (_updateTimer > updateDelay)
            {
                _updateTimer = 0;
                _requestsSent = 0;
                HandleSearchContentUpdate();
            }
        }

        // 如果玩家打开了箱子，就在箱子里高亮显示
        var player = Main.LocalPlayer;
        switch (player.chest)
        {
            case >= 0 and < 8000 when Main.chest[player.chest] != null:
                ColorChestSlots(Main.chest[player.chest]);
                break;
            case -2:
                ColorChestSlots(player.bank);
                break;
            case -3:
                ColorChestSlots(player.bank2);
                break;
            case -4:
                ColorChestSlots(player.bank3);
                break;
            case -5:
                ColorChestSlots(player.bank4);
                break;
        }

        if (!MainPanel.IsMouseHovering)
            return;

        PlayerInput.LockVanillaMouseScroll("ImproveGame: Item Searcher GUI");
        Main.LocalPlayer.mouseInterface = true;
    }

    private void ColorChestSlots(Chest chest)
    {
        if (chest.item[0] is null)
            return;

        for (var i = 0; i < chest.item.Length; i++)
        {
            var item = chest.item[i];
            if (item is null) continue;

            if (_oldItemTypes.Contains(item.type))
            {
                Terraria.UI.ItemSlot.inventoryGlowTimeChest[i] = 300;
                Terraria.UI.ItemSlot.inventoryGlowHueChest[i] = 0.125f;
            }
            else if (Terraria.UI.ItemSlot.inventoryGlowHueChest[i] is 0.125f)
            {
                Terraria.UI.ItemSlot.inventoryGlowTimeChest[i] = 0;
                Terraria.UI.ItemSlot.inventoryGlowHueChest[i] = 0f;
            }
        }
    }

    public void Open()
    {
        UIPlayer.CheckPositionValid(ref UIPlayer.ItemSearcherPosition, UIPlayer.ItemSearcherDefPosition);
        MainPanel.SetPos(UIPlayer.ItemSearcherPosition).Recalculate();

        SoundEngine.PlaySound(SoundID.MenuOpen);
        Enabled = true;
        StartTimer.Open();
    }

    public void Close()
    {
        SoundEngine.PlaySound(SoundID.MenuClose);
        Enabled = false;
        StartTimer.Close();
    }

    public override bool RenderTarget2DDraw => !StartTimer.Opened;
    public override float RenderTarget2DOpacity => StartTimer.Schedule;
    public override Vector2 RenderTarget2DOrigin => MainPanel.GetDimensionsCenter();
    public override Vector2 RenderTarget2DPosition => MainPanel.GetDimensionsCenter();
    public override Vector2 RenderTarget2DScale => new Vector2(0.95f + StartTimer.Lerp(0, 0.05f));
}