using ImproveGame.Content.Functions.AutoPiggyBank;
using ImproveGame.Core;
using ImproveGame.UI.QuickShimmer;
using ImproveGame.UIFramework;
using ImproveGame.UIFramework.BaseViews;
using ImproveGame.UIFramework.Common;
using ImproveGame.UIFramework.SUIElements;
using System.Collections;
using Terraria.GameInput;

namespace ImproveGame.UI.OpenBag;

[AutoCreateGUI(LayerName.Vanilla.RadialHotbars, "Open Bag GUI")]
public class OpenBagGUI : BaseBody
{
    private const string _context = $"{nameof(ImproveGame)}: {nameof(QuickShimmerGUI)}";

    public static OpenBagGUI Instance { get; private set; }

    public OpenBagGUI() => Instance = this;

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

    // 主面板
    public SUIPanel MainPanel;

    // 标题面板
    private View TitlePanel;

    // 搜索到的物品和滚动条(隐藏)
    public BaseGrid LootsGrid;
    public SUIScrollBar Scrollbar;
    public UIText TipText;

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
        MainPanel.SetPosPixels(410, 360)
            .SetSizePixels(404, 0)
            .JoinParent(this);

        TitlePanel = ViewHelper.CreateHead(Color.Black * 0.25f, 45f, 10f);
        TitlePanel.SetPadding(0f);
        TitlePanel.JoinParent(MainPanel);

        // 标题
        var title = new SUIText
        {
            IsLarge = true,
            UseKey = true,
            TextOrKey = "Mods.ImproveGame.UI.OpenBag.Title",
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

        MakeMainSlotAndButtons();

        // 分割
        MakeSeparator();

        var itemsPanel = new View
        {
            DragIgnore = true,
            RelativeMode = RelativeMode.Vertical,
            OverflowHidden = true
        };
        itemsPanel.SetPadding(15, 0, 14, 14);
        itemsPanel.SetSize(0f, 190f, 1f, 0f);
        itemsPanel.JoinParent(MainPanel);

        // 没有物品时显示的提示，这里先Append，要用到的时候调一下Left就行
        TipText = new UIText(MyUtils.GetText("UI.OpenBag.TipText"))
        {
            Width = { Percent = 1f },
            Height = { Percent = 1f },
            TextOriginX = 0.5f,
            TextOriginY = 0.5f
        };
        itemsPanel.Append(TipText);

        LootsGrid = new BaseGrid();
        LootsGrid.SetBaseValues(-1, 8, new Vector2(4f), new Vector2(43));
        LootsGrid.JoinParent(itemsPanel);

        Scrollbar = new SUIScrollBar { HAlign = 1f };
        Scrollbar.Left.Pixels = -1;
        Scrollbar.Height.Pixels = LootsGrid.Height();
        Scrollbar.SetView(itemsPanel.GetInnerSizePixels().Y, LootsGrid.Height.Pixels);
        Scrollbar.JoinParent(this);
        RefreshGrid();

        // 由于用了自适应高度，这个是拿来把底部往下顶14px的
        var sellAllArea = new View
        {
            DragIgnore = true,
            RelativeMode = RelativeMode.Vertical,
            HAlign = 0.5f
        };
        sellAllArea.SetPadding(0, 0, 0, 14);
        sellAllArea.SetSize(-30f, 52f, 1f, 0f);
        sellAllArea.JoinParent(MainPanel);

        var sellAll = new SellAllButton(MyUtils.GetText("UI.OpenBag.SellAll.Name"), SellAll)
        {
            DragIgnore = true,
            RelativeMode = RelativeMode.Vertical,
            HAlign = 0.5f
        };
        sellAll.SetPadding(0);
        sellAll.SetSize(Vector2.Zero, 1f, 1f);
        sellAll.JoinParent(sellAllArea);
    }

    /// <summary>
    /// 放入袋子的物品框和按钮
    /// </summary>
    private void MakeMainSlotAndButtons()
    {
        var bagPanel = new View
        {
            DragIgnore = true,
            RelativeMode = RelativeMode.Vertical
        };
        bagPanel.SetPadding(6, 6, 6, 6);
        bagPanel.SetSize(0f, 56, 1f, 0f);
        bagPanel.JoinParent(MainPanel);

        var itemSlot = MyUtils.CreateItemSlot(20f, 6f, onItemChanged: (item, _) =>
            {
                if (Main.LocalPlayer is not null && Main.LocalPlayer.TryGetModPlayer(out LootKeeper keeper))
                    keeper.Bag = item;
            },
            parent: bagPanel,
            iconTextureName: "Bag",
            emptyText: () => MyUtils.GetText("UI.OpenBag.EmptyText"));
        itemSlot.OnUpdate += _ =>
        {
            if (Main.LocalPlayer is not null && Main.LocalPlayer.TryGetModPlayer(out LootKeeper keeper))
                itemSlot.Item = keeper.Bag;
        };

        var openButton = new SUIButton(ModAsset.Open.Value, MyUtils.GetText("UI.OpenBag.Open"))
        {
            Left = { Pixels = 80f },
            Top = { Pixels = 8f }
        };
        openButton.OnLeftMouseDown += (_, _) =>
        {
            if (CoroutineSystem.OpenBagRunner.Count > 0)
            {
                CoroutineSystem.OpenBagRunner.StopAll();
                return;
            }

            if (Main.LocalPlayer is null || !Main.LocalPlayer.TryGetModPlayer(out LootKeeper keeper))
                return;

            CoroutineSystem.OpenBagRunner.Run(OpenBagRunner(keeper));
        };
        openButton.OnUpdate += _ =>
        {
            openButton.Text = MyUtils.GetText(CoroutineSystem.OpenBagRunner.Count > 0 ? "UI.OpenBag.Stop" : "UI.OpenBag.Open");
        };
        openButton.JoinParent(bagPanel);

        var depositButton = new SUIButton(ModAsset.Quick.Value, Lang.inter[29].Value) // 强夺全部
        {
            Left = { Pixels = openButton.Left.Pixels + openButton.Width.Pixels + 10f },
            Top = { Pixels = 8f },
            Width = { Pixels = 136f }
        };
        depositButton.OnLeftMouseDown += (_, _) =>
        {
            if (Main.LocalPlayer is null || !Main.LocalPlayer.TryGetModPlayer(out LootKeeper keeper))
                return;

            keeper.Loots.ForEach(l => Main.LocalPlayer.QuickSpawnItem(Main.LocalPlayer.GetSource_Loot(), l));
            keeper.Loots.Clear();
            RefreshGrid();

            // 控制提示文本是否显示
            TipText.Left.Pixels = 0;
        };
        depositButton.JoinParent(bagPanel);
    }

    /// <summary>
    /// 协程开袋，在袋子很多的同时不卡，也有一个很好的动画效果
    /// </summary>
    private IEnumerator OpenBagRunner(LootKeeper keeper)
    {
        while (keeper is not null && !keeper.Bag.IsAir && ItemLoader.CanRightClick(keeper.Bag) &&
               (Main.ItemDropsDB.GetRulesForItemID(keeper.Bag.type).Count != 0))
        {
            LootListener._listening = keeper.Bag.type;
            ItemSlot.TryOpenContainer([keeper.Bag], 0, 0, Main.LocalPlayer);
            LootListener._listening = -1;

            RefreshGrid();
            yield return 1;
        }
    }

    private void SellAll()
    {
        if (Main.LocalPlayer is null || !Main.LocalPlayer.TryGetModPlayer(out LootKeeper keeper)) return;

        // 如果每次卖出一个物品就生成钱币物品，在物品太多的情况下，前面的钱币会被后面的顶掉（因为400物品上限）
        // 所以要存，最后一起生成
        ulong totalValue = 0;
        keeper.Loots.ForEach(item =>
        {
            ulong coin = CoinUtils.CalculateCoinValue(item.type, (uint)item.stack);
            totalValue += coin;

            if (coin > 0) return; // 只有是钱币，这个值才会大于0

            Main.LocalPlayer.GetItemExpectedPrice(item, out var calcForSelling, out _);
            if (calcForSelling <= 0)
                return;

            const float sellPercent = 1.1f * 0.2f; // 110%额外加成 & 售价是calcForSelling的20%
            totalValue += (ulong)Math.Floor(calcForSelling * item.stack * sellPercent);
        });

        var coins = CoinUtils.ConvertCopperValueToCoins(totalValue);
        coins.ForEach(item =>
        {
            Main.LocalPlayer.QuickSpawnItem(Main.LocalPlayer.GetSource_Loot(), item);
        });

        keeper.Loots.Clear();
        RefreshGrid();

        // 控制提示文本是否显示
        TipText.Left.Pixels = 0;
    }

    private void RefreshGrid()
    {
        LootsGrid.RemoveAllChildren();

        if (Main.LocalPlayer is not null && Main.LocalPlayer.TryGetModPlayer(out LootKeeper keeper))
        {
            for (var i = 0; i < keeper.Loots.Count; i++)
            {
                // 最多显示一百列，不然生成Children时太卡了
                if (i >= 8 * 100)
                {
                    var tip = new UIText(MyUtils.GetText("UI.OpenBag.NotFullyDisplayed", keeper.Loots.Count - 800))
                    {
                        Width = { Percent = 1f },
                        Height = { Pixels = 46f },
                        TextOriginX = 0.5f,
                        TextOriginY = 0.5f
                    };
                    LootsGrid.Append(tip);
                    break;
                }

                var itemSlot = new LootItemSlot(keeper.Loots, i, _context);
                itemSlot.JoinParent(LootsGrid);
            }

            // 控制提示文本是否显示
            if (keeper.Loots.Count is 0)
                TipText.Left.Pixels = 0;
            else
                TipText.Left.Pixels = -8888;
        }
        else
        {
            TipText.Left.Pixels = 0;
        }

        TipText.Recalculate();
        LootsGrid.CalculateAndSetSize();
        LootsGrid.CalculateAndSetChildrenPosition();
        LootsGrid.Recalculate();
        Scrollbar.SetView(LootsGrid.Parent.GetInnerSizePixels().Y, LootsGrid.Height.Pixels);
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
        if (-Scrollbar.BarTop == LootsGrid.DatumPoint.Y)
            return;

        LootsGrid.DatumPoint.Y = -Scrollbar.BarTop;
        LootsGrid.Recalculate();
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        StartTimer.Update();

        if (Main.LocalPlayer is null) return;

        if (Main.LocalPlayer.TryGetModPlayer(out LootKeeper keeper))
        {
            keeper.Loots ??= new List<Item>();
            if (keeper.Loots.RemoveAll(i => i.IsAir) > 0)
                RefreshGrid();
        }

        if (!MainPanel.IsMouseHovering)
            return;

        PlayerInput.LockVanillaMouseScroll("ImproveGame: Open Bag GUI");
        Main.LocalPlayer.mouseInterface = true;
    }

    public void Open()
    {
        UIPlayer.CheckPositionValid(ref UIPlayer.OpenBagPosition, UIPlayer.OpenBagDefPosition);
        MainPanel.SetPos(UIPlayer.OpenBagPosition).Recalculate();

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