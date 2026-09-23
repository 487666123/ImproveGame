using ImproveGame.Common.GlobalItems;
using ImproveGame.Common.ModPlayers;
using ImproveGame.Common.ModSystems;
using ImproveGame.Packets.NetAutofisher;
using ImproveGame.UIFramework;
using ImproveGame.UIFramework.BaseViews;
using ImproveGame.UIFramework.Common;
using ImproveGame.UIFramework.SUIElements;
using ImproveGame.UIFramework.UIElements;
using Terraria.UI.Chat;

namespace ImproveGame.UI.Autofisher;

public partial class AutofisherGUI : BaseBody, ISidedView
{
    public override bool Enabled { get => true; set { } }

    public static bool Visible => SidedEventTrigger.IsOpened(UISystem.Instance.AutofisherGUI);

    public float SwapSlideFactor { get; set; }

    private static float panelLeft;
    private static float panelWidth;
    private static float panelTop;
    private static float panelHeight;

    private Asset<Texture2D> selectPoolOff;
    private Asset<Texture2D> selectPoolOn;

    private SUIPanel basePanel;
    private UIImage _buttonLootAll;
    private ModItemSlot accessorySlot = new();
    private ModItemSlot fishingPoleSlot = new();
    private ModItemSlot baitSlot = new();
    private FishItemSlot[] fishSlot = new FishItemSlot[40];
    private UIText tipText;
    private UIImage relocateButton;
    private SUIPanel textPanel;

    internal static bool RequireRefresh;

    public override void OnInitialize()
    {
        panelTop = Main.instance.invBottom + 60;
        panelLeft = 100f;
        panelHeight = 356f;
        panelWidth = 420f;

        basePanel = new SUIPanel(UIStyle.PanelBorder, UIStyle.PanelBg)
        {
            Shaded = true
        };
        basePanel.SetPos(panelLeft, panelTop).SetSize(panelWidth, panelHeight);
        Append(basePanel);

        accessorySlot = MyUtils.CreateItemSlot(
            100f, 0f,
            canPlace: (i, item) => MyUtils.SlotPlace(i, item) || ModIntegrationsSystem.FishingStatLookup.ContainsKey(item.type),
            onItemChanged: ChangeAccessorySlot,
            emptyText: () => MyUtils.GetText("UI.Autofisher.Accessory"),
            parent: basePanel,
            folderName: "Autofisher",
            iconTextureName: "Slot_Accessory"
        );
        accessorySlot.AllowFavorite = false;

        fishingPoleSlot = MyUtils.CreateItemSlot(
            150, 0f,
            canPlace: (i, item) => MyUtils.SlotPlace(i, item) || item.fishingPole > 0,
            onItemChanged: ChangeFishingPoleSlot,
            emptyText: () => MyUtils.GetText("UI.Autofisher.FishingPole"),
            parent: basePanel,
            folderName: "Autofisher",
            iconTextureName: "Slot_FishingPole"
        );
        fishingPoleSlot.AllowFavorite = false;

        baitSlot = MyUtils.CreateItemSlot(
            200f, 0f,
            canPlace: (i, item) => MyUtils.SlotPlace(i, item) || item.bait > 0,
            onItemChanged: ChangeBaitSlot,
            emptyText: () => MyUtils.GetText("UI.Autofisher.Bait"),
            parent: basePanel,
            folderName: "Autofisher",
            iconTextureName: "Slot_Bait"
        );
        baitSlot.OnRightClickItemChange += ChangeBaitSlotStack;
        baitSlot.AllowFavorite = false;

        const int slotFirst = 50;
        for (int i = 0; i < fishSlot.Length; i++)
        {
            int x = i % 8 * slotFirst;
            int y = i / 8 * slotFirst + slotFirst;
            fishSlot[i] = new(i);
            fishSlot[i].SetPos(x, y);
            fishSlot[i].SetSize(46f, 46f);
            // fishSlot[i].AllowFavorite = false;
            fishSlot[i].OnFishChange += ChangeFishSlot;
            fishSlot[i].OnFishRightClickChange += ChangeFishSlotStack;
            basePanel.Append(fishSlot[i]);
        }

        textPanel = new(UIStyle.TitleBg, UIStyle.TitleBg, rounded: 10)
        {
            HAlign = 0.5f,
            Top = { Pixels = -34f, Percent = 1f },
            Width = StyleDimension.FromPercent(1f),
            Height = StyleDimension.FromPixels(30f)
        };
        textPanel.SetPadding(0f);
        basePanel.Append(textPanel);
        tipText = new("Error", 0.8f)
        {
            HAlign = 0.5f,
            VAlign = 0.5f
        };
        textPanel.Append(tipText);

        selectPoolOff = MyUtils.GetTexture("UI/Autofisher/SelectPoolOff");
        selectPoolOn = MyUtils.GetTexture("UI/Autofisher/SelectPoolOn");
        relocateButton = new UIImage(selectPoolOff);
        relocateButton.Left.Set(250f, 0f);
        relocateButton.Top.Set(0f, 0f);
        relocateButton.Width.Set(46f, 0f);
        relocateButton.Height.Set(46f, 0f);
        relocateButton.AllowResizingDimensions = false;
        relocateButton.OnLeftMouseDown += (_, _) => ToggleSelectPool();
        basePanel.Append(relocateButton);

        _buttonLootAll = new UIImage(ModAsset.FisherLootAll.Value);
        _buttonLootAll.AllowResizingDimensions = false;
        _buttonLootAll.Left.Set(0f, 0f);
        _buttonLootAll.Top.Set(4f, 0f);
        _buttonLootAll.Width.Set(46f, 0f);
        _buttonLootAll.Height.Set(46f, 0f);
        _buttonLootAll.HAlign = 1f;
        _buttonLootAll.OnLeftMouseDown += (_, _) => LootAll();
        basePanel.Append(_buttonLootAll);

        float filtersX = panelLeft + panelWidth + 10f;
        float filtersY = panelTop + 8f;
        AddToolButton(new FreeFilterButton(basePanel));
        AddToolButton(new CatchCratesFilter(basePanel));
        AddToolButton(new CatchAccessoriesFilter(basePanel));
        AddToolButton(new CatchToolsFilter(basePanel));
        AddToolButton(new CatchWhiteRarityCatchesFilter(basePanel));
        AddToolButton(new CatchNormalCatchesFilter(basePanel));
        AddToolButton(new AutoDepositToggle(basePanel));
        return;

        void AddToolButton(UIElement button)
        {
            var filter = button.SetPos(filtersX, filtersY);
            filtersY += filter.Height.Pixels + 8f;
            Append(filter);
        }
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        relocateButton.SetImage(WandSystem.SelectPoolMode
            ? selectPoolOn
            : selectPoolOff);

        _buttonLootAll.SetImage(_buttonLootAll.IsMouseHovering
            ? ModAsset.FisherLootAll_Hover.Value
            : ModAsset.FisherLootAll.Value);

        // 出现和退出动画
        float oldLeft = basePanel.Left.Pixels;
        float widthNext = basePanel.GetDimensions().Width;
        float shownPositionNext = panelLeft;
        float hiddenPositionNext = -widthNext - 40;

        basePanel.Left.Set((int)MathHelper.Lerp(hiddenPositionNext, shownPositionNext, SwapSlideFactor), 0f);
        if (oldLeft != basePanel.Left.Pixels)
            basePanel.Recalculate();
    }

    public void ToggleSelectPool()
    {
        WandSystem.SelectPoolMode = !WandSystem.SelectPoolMode;
    }

    private void LootAll()
    {
        var autoFisher = AutofishPlayer.LocalPlayer.Autofisher;
        if (autoFisher is null)
            return;

        SoundEngine.PlaySound(SoundID.Grab);
        Item[] inventory = Main.LocalPlayer.inventory;
        Item[] fish = autoFisher.fish;
        for (int i = 0; i < fish.Length; i++)
        {
            if (fish[i].IsAir || fish[i].favorited)
                continue;
            fish[i] = MyUtils.ItemStackToInventory(inventory, fish[i], false, 50);
        }
        // 同步
        SyncFromTileEntity();
        if (Main.netMode is NetmodeID.MultiplayerClient)
            ItemSyncPacket.Get(AutofishPlayer.LocalPlayer.Autofisher.ID, ItemSyncPacket.All).Send(runLocally: false);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        base.Draw(spriteBatch);

        // 处理饰品栏物品的特殊tooltip
        if (accessorySlot.IsMouseHovering && !accessorySlot.Item.IsAir &&
            (Main.mouseItem is null || Main.mouseItem.IsAir))
        {
            // 添加特殊标识，在AutofishItemListener中将根据这个标识特异性识别物品并添加tooltip
            Main.HoverItem.GetGlobalItem<AutofishItemListener>().IsInAutoFisher = true;
        }

        // 是否开启制作栏侧栏，只有在开启或正在开启状态才隐藏，正在关闭状态不隐藏
        if (SidedEventTrigger.IsOpened(this))
            Main.hidePlayerCraftingMenu = true;

        Player player = Main.LocalPlayer;

        if (AutofishPlayer.LocalPlayer.Autofisher is not null)
        {
            var autofisher = AutofishPlayer.LocalPlayer.Autofisher;

            if (baitSlot.Item.type == ItemID.TruffleWorm)
            {
                autofisher.SetFishingTip(Content.Tiles.Autofisher.TipType.FishingWarning);
            }

            if (baitSlot.Item.IsAir || fishingPoleSlot.Item.IsAir || autofisher.FishingTip == "Error")
            {
                autofisher.SetFishingTip(Content.Tiles.Autofisher.TipType.Unavailable);
            }

            tipText.SetText(autofisher.FishingTip);


            if (basePanel.ContainsPoint(Main.MouseScreen))
            {
                player.mouseInterface = true;
            }
        }

        if (_buttonLootAll.IsMouseHovering)
        {
            Main.instance.MouseText(Lang.inter[29].Value);
        }

        // 用 title.IsMouseHovering 出框之后就会没
        if (textPanel.IsMouseHovering)
        {
            var dimension = basePanel.GetDimensions();
            var position = dimension.Position() + new Vector2(dimension.Width + 20f, 0f);

            var tooltip = Lang.GetTooltip(ModContent.ItemType<Content.Items.Placeable.Autofisher>());
            int lines = tooltip.Lines;
            var font = FontAssets.MouseText.Value;
            int widthOffset = 14;
            int heightOffset = 9;
            float lengthX = 0f;
            float lengthY = 0f;

            for (int i = 0; i < lines; i++)
            {
                string line = tooltip.GetLine(i);
                var stringSize = ChatManager.GetStringSize(font, line, Vector2.One);
                lengthX = Math.Max(lengthX, stringSize.X + 8);
                lengthY += stringSize.Y;
            }

            Utils.DrawInvBG(Main.spriteBatch,
                new Rectangle((int)position.X - widthOffset, (int)position.Y - heightOffset,
                    (int)lengthX + widthOffset * 2, (int)lengthY + heightOffset + heightOffset / 2),
                new Color(23, 25, 81, 255) * 0.925f);

            for (int i = 0; i < lines; i++)
            {
                string line = tooltip.GetLine(i);
                ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, font, line, position, Color.White, 0f,
                    Vector2.Zero, Vector2.One, spread: 1.6f);
                position.Y += (int)ChatManager.GetStringSize(font, line, Vector2.One).Y;
            }
        }
    }

    /// <summary>
    /// 打开GUI界面
    /// </summary>
    public void Open()
    {
        WandSystem.SelectPoolMode = false;
        MyUtils.OperateInventory(true);
        // AutofishPlayer.LocalPlayer.SetAutofisher(point);
        RefreshItems();
    }

    public void Close()
    {
        relocateButton.SetImage(selectPoolOff);
        WandSystem.SelectPoolMode = false;
        Main.blockInput = false;
        SoundEngine.PlaySound(SoundID.MenuClose);
    }

    public override bool CanSetFocusTarget(UIElement target)
    {
        return (target != this && basePanel.IsMouseHovering) || basePanel.IsLeftMousePressed;
    }
}