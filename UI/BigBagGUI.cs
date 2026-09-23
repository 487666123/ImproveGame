using ImproveGame.Common.Configs;
using ImproveGame.Packets;
using ImproveGame.UIFramework;
using ImproveGame.UIFramework.BaseViews;
using ImproveGame.UIFramework.Common;
using ImproveGame.UIFramework.SUIElements;
using ImproveGame.UIFramework.UIElements;
using Terraria.GameInput;

namespace ImproveGame.UI;

[AutoCreateGUI(LayerName.Vanilla.RadialHotbars, "Big Bag")]
public class BigBagGUI : BaseBody
{
    public static BigBagGUI Instance { get; private set; }
    public BigBagGUI() => Instance = this;
    public override bool IsNotSelectable => StartTimer.AnyClose;

    public override bool Enabled
    {
        get
        {
            if (field && !Main.playerInventory)
            {
                field = false;
                StartTimer.Close();
            }

            return StartTimer.Closing || field;
        }
        set;
    }

    public override bool CanSetFocusTarget(UIElement target)
        => target != this && (MainPanel.IsMouseHovering || MainPanel.IsLeftMousePressed);

    /// <summary>
    /// 启动关闭动画计时器
    /// </summary>
    public AnimationTimer StartTimer = new(3);

    #region Components
    // 主面板
    public SUIPanel MainPanel;

    // 标题面板
    private View TitlePanel;

    // 内容面板
    private View ButtonPanel;

    // 标题
    private SUIText Title;

    // 关闭按钮
    private SUICross Cross;

    // 控制开关
    private SUISwitch RecipesSwitch, SmartGrabSwitch, AutoGrabSwitch;

    // 按钮
    private SUIButton QuickButton, PutButton, ReplenishButton, SortButton;

    // 物品列表
    public ModItemGrid ItemGrid;
    #endregion

    public override void OnInitialize()
    {
        PlayerBigBagSettingPacket.SendMyPlayer();

        UIPlayerSetting setting = Main.LocalPlayer.GetModPlayer<UIPlayerSetting>();

        // 主面板
        MainPanel = new SUIPanel(UIStyle.PanelBorder, UIStyle.PanelBg)
        {
            Shaded = true,
            Draggable = true,
            FinallyDrawBorder = true
        };
        MainPanel.SetPadding(1f);
        MainPanel.JoinParent(this);

        TitlePanel = ViewHelper.CreateHead(UIStyle.TitleBg * 0.75f, 50f, 10f);
        TitlePanel.RelativeMode = RelativeMode.Vertical;
        TitlePanel.SetPadding(0f);
        TitlePanel.JoinParent(MainPanel);

        // 标题
        Title = new SUIText
        {
            TextOrKey = "Mods.ImproveGame.SuperVault.Name",
            UseKey = true,
            IsLarge = true,
            TextScale = 0.55f,
            VAlign = 0.5f,
            TextBorder = 2f,
            PaddingLeft = 20f,
            DragIgnore = true
        };
        Title.SetInnerPixels(Title.TextSize * Title.TextScale);
        Title.JoinParent(TitlePanel);

        // Cross
        Cross = new SUICross
        {
            HAlign = 1f,
            VAlign = 0.5f,
            Width = { Pixels = 55f, Percent = 0f },
            Height = { Pixels = 0f, Precent = 1f },
            Rounded = new Vector4(0f, 10f, 0f, 0f),
            Border = 0f,
            BorderColor = Color.Transparent,
            CrossOffset = new Vector2(1f, 0f),
            CrossRounded = UIStyle.CrossThickness * 0.95f
        };
        Cross.OnLeftMouseDown += (_, _) => Close();
        Cross.OnUpdate += (_) =>
        {
            Cross.BgColor = Cross.HoverTimer.Lerp(Color.Transparent, UIStyle.PanelBorder * 0.5f);
        };
        Cross.JoinParent(TitlePanel);

        var view = new View
        {
            Width = { Precent = 1f },
            Height = { Pixels = 2f },
            BgColor = UIStyle.PanelBorder,
            RelativeMode = RelativeMode.Vertical,
            Spacing = new Vector2(-1f),
        };
        view.JoinParent(MainPanel);

        ButtonPanel = new View
        {
            DragIgnore = true,
            RelativeMode = RelativeMode.Vertical
        };
        ButtonPanel.SetPadding(12f, 10f, 11f, 12f);
        ButtonPanel.JoinParent(MainPanel);

        #region 开关按钮
        Vector2 switchSpacing = new Vector2(10, 10);
        RecipesSwitch = new SUISwitch(() => setting.SuperVault_ParticipateSynthesis,
            state =>
            {
                setting.SuperVault_ParticipateSynthesis = state;
                // Recipe.FindRecipes();
            }, MyUtils.GetText("SuperVault.Synthesis"), 0.8f)
        {
            ResetAnotherPosition = true,
            RelativeMode = RelativeMode.Vertical,
            Spacing = switchSpacing
        };
        RecipesSwitch.JoinParent(ButtonPanel);

        SmartGrabSwitch = new SUISwitch(() => setting.SuperVault_PrioritizeGrabbing,
            state =>
            {
                setting.SuperVault_PrioritizeGrabbing = state;
                PlayerBigBagSettingPacket.SendMyPlayer();
            },
            MyUtils.GetText("SuperVault.SmartPickup"), 0.8f)
        {
            RelativeMode = RelativeMode.Horizontal,
            Spacing = switchSpacing
        };
        SmartGrabSwitch.JoinParent(ButtonPanel);

        AutoGrabSwitch = new SUISwitch(() => setting.SuperVault_GrabItemsWhenOverflowing,
            state =>
            {
                setting.SuperVault_GrabItemsWhenOverflowing = state;
                PlayerBigBagSettingPacket.SendMyPlayer();
            },
            MyUtils.GetText("SuperVault.OverflowPickup"), 0.8f)
        {
            RelativeMode = RelativeMode.Horizontal,
            Spacing = switchSpacing
        };
        AutoGrabSwitch.JoinParent(ButtonPanel);
        #endregion

        // 按钮
        Vector2 buttonSpacing = new Vector2(10, 8);
        QuickButton = new SUIButton(MyUtils.GetTexture("UI/Quick").Value, Lang.inter[29].Value)
        {
            RelativeMode = RelativeMode.Vertical,
            Spacing = buttonSpacing,
            ResetAnotherPosition = true
        };
        QuickButton.OnLeftMouseDown += (_, _) => QuickTakeOutToPlayerInventory();
        QuickButton.JoinParent(ButtonPanel);

        PutButton = new SUIButton(MyUtils.GetTexture("UI/Put").Value, Lang.inter[30].Value)
        {
            RelativeMode = RelativeMode.Horizontal,
            Spacing = buttonSpacing
        };
        PutButton.OnLeftMouseDown += (_, _) => PutAll();
        PutButton.JoinParent(ButtonPanel);

        ReplenishButton = new SUIButton(MyUtils.GetTexture("UI/Put").Value, Lang.inter[31].Value)
        {
            RelativeMode = RelativeMode.Horizontal,
            Spacing = buttonSpacing
        };
        ReplenishButton.OnLeftMouseDown += (_, _) => Replenish();
        ReplenishButton.JoinParent(ButtonPanel);

        SortButton = new SUIButton(MyUtils.GetTexture("UI/Put").Value, MyUtils.GetText("SuperVault.Sort"))
        {
            RelativeMode = RelativeMode.Horizontal,
            Spacing = buttonSpacing
        };
        SortButton.OnLeftMouseDown += (_, _) => Sort();
        SortButton.JoinParent(ButtonPanel);

        // Inventory 滚动视图
        ItemGrid = new ModItemGrid
        {
            ResetAnotherPosition = true,
            RelativeMode = RelativeMode.Vertical,
            Spacing = new Vector2(10, 15)
        };
        ItemGrid.ItemList.OnMouseDownSlot += NetSyncItem;
        ItemGrid.JoinParent(ButtonPanel);
        ButtonPanel.SetInnerPixels(ItemGrid.Width.Pixels, ItemGrid.Bottom());
        MainPanel.SetInnerPixels(ButtonPanel.Width.Pixels, ButtonPanel.Bottom());
    }

    /// <summary>
    /// 点击操作，将物品发送给服务器（因为像药水袋和旗帜盒这俩左键是不改stack的，所以这来个同步）
    /// </summary>
    /// <param name="evt"></param>
    /// <param name="listeningElement"></param>
    private static void NetSyncItem(UIMouseEvent evt, UIElement listeningElement)
    {
        if (Main.netMode != NetmodeID.MultiplayerClient || listeningElement is not BigBagItemSlot itemSlot)
        {
            return;
        }

        var packet = BigBagSlotPacket.Get(itemSlot.Item, Main.myPlayer, itemSlot.Index);
        packet.Send(runLocally: false);
    }

    public override void Update(GameTime gameTime)
    {
        StartTimer.Update();
        base.Update(gameTime);
        if (MainPanel.IsMouseHovering)
        {
            PlayerInput.LockVanillaMouseScroll("ImproveGame: BigBagGUI");
            Main.LocalPlayer.mouseInterface = true;
        }

        if (Math.Abs(ButtonPanel.GetInnerSizePixels().Y - ItemGrid.Bottom()) > 0.000000001)
        {
            ButtonPanel.SetInnerPixels(ItemGrid.Width.Pixels, ItemGrid.Bottom());
            ButtonPanel.Recalculate();
        }

        if (Math.Abs(MainPanel.Height.Pixels - ButtonPanel.Bottom()) < 0.000000001)
        {
            return;
        }

        MainPanel.SetInnerPixels(ButtonPanel.Width.Pixels, ButtonPanel.Bottom());
        MainPanel.Recalculate();
    }

    #region 开关 UI

    public void Open()
    {
        UIPlayer.CheckPositionValid(ref UIPlayer.HugeInventoryUIPosition, UIPlayer.HugeInventoryDefPosition);
        MainPanel.SetPos(UIPlayer.HugeInventoryUIPosition).Recalculate();

        Enabled = true;
        StartTimer.Open();

        SoundEngine.PlaySound(SoundID.MenuOpen);
        MyUtils.OperateInventory(true);
    }

    public void Close()
    {
        Enabled = false;
        StartTimer.Close();

        SoundEngine.PlaySound(SoundID.MenuClose);
        ClientConfigCore.SaveConfig();
    }

    #endregion

    /// <summary>
    /// 整理大背包
    /// </summary>
    private void Sort()
    {
        SoundEngine.PlaySound(SoundID.Grab);
        Item[] items = ItemGrid.ItemList.items;

        // 拿出来非空非收藏的物品
        List<Item> testSort = [];

        for (int i = 0; i < items.Length; i++)
        {
            if (items[i].IsAir || items[i].favorited)
            {
                continue;
            }

            testSort.Add(items[i]);
            items[i] = new Item();
        }

        // 优先级排序
        testSort.Sort((a, b) =>
            -a.rare.CompareTo(b.rare) * 100 + a.type.CompareTo(b.type) * 10 - a.stack.CompareTo(b.stack));

        // 放入背包
        foreach (var item in testSort)
        {
            MyUtils.ItemStackToInventory(items, item, false);
        }

        // Recipe.FindRecipes();
    }

    private void Replenish()
    {
        SoundEngine.PlaySound(SoundID.Grab);
        Item[] inventory = Main.LocalPlayer.inventory;
        Item[] bigBag = ItemGrid.ItemList.items;
        for (int i = 10; i < 58; i++)
        {
            if (inventory[i].IsAir || inventory[i].favorited || inventory[i].IsACoin)
            {
                continue;
            }

            if (MyUtils.HasItem(bigBag, -1, inventory[i].type))
            {
                inventory[i] = MyUtils.ItemStackToInventory(bigBag, inventory[i], false);
            }
        }

        // Recipe.FindRecipes();
    }

    private void PutAll()
    {
        SoundEngine.PlaySound(SoundID.Grab);

        var inventory = Main.LocalPlayer.inventory;
        var bigBag = ItemGrid.ItemList.items;

        for (int i = 10; i < 50; i++)
        {
            // 物品非空，且未收藏，且不是钱币
            if (inventory[i].IsNotAir && inventory[i].NotFavorited && inventory[i].IsNotACoin)
                inventory[i] = MyUtils.ItemStackToInventory(bigBag, inventory[i], false);
        }

        // Recipe.FindRecipes();
    }

    /// <summary>
    /// 移动大背包物品到玩家背包
    /// </summary>
    private void QuickTakeOutToPlayerInventory()
    {
        SoundEngine.PlaySound(SoundID.Grab);

        var inventory = Main.LocalPlayer.inventory;
        var bigBag = ItemGrid.ItemList.items;

        for (int i = 0; i < bigBag.Length; i++)
        {
            // 物品非空，且未收藏，且不是钱币
            if (bigBag[i].IsNotAir && bigBag[i].NotFavorited && bigBag[i].IsNotACoin)
            {
                bigBag[i] = MyUtils.ItemStackToInventory(inventory, bigBag[i], false, 50);
            }
        }

        // Recipe.FindRecipes();
    }

    public override bool RenderTarget2DDraw => !StartTimer.Opened;
    public override float RenderTarget2DOpacity => StartTimer.Schedule;
    public override Vector2 RenderTarget2DOrigin => MainPanel.GetDimensionsCenter();
    public override Vector2 RenderTarget2DPosition => MainPanel.GetDimensionsCenter();
    public override Vector2 RenderTarget2DScale => new Vector2(0.95f + StartTimer.Lerp(0, 0.05f));
}