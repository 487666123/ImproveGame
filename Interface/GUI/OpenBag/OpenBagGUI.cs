using ImproveGame.Content.Patches.AutoPiggyBank;
using ImproveGame.Core;
using ImproveGame.Interface.Common;
using ImproveGame.Interface.SUIElements;
using System.Collections;
using Terraria.GameInput;

namespace ImproveGame.Interface.GUI.OpenBag;

public class OpenBagGUI : ViewBody
{
    public override bool Display { get => Visible; set => Visible = value; }
    public static bool Visible { get; private set; }

    public override bool CanPriority(UIElement target) => target != this;

    public override bool CanDisableMouse(UIElement target)
        => (target != this && MainPanel.IsMouseHovering) || MainPanel.KeepPressed;

    // 主面板
    public SUIPanel MainPanel;

    // 标题面板
    private SUIPanel TitlePanel;

    // 标题
    private SUITitle Title;

    // 关闭按钮
    private SUICross Cross;

    // 搜索到的物品和滚动条(隐藏)
    public BaseGrid LootsGrid;
    public SUIScrollbar Scrollbar;
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
                Relative = RelativeMode.Vertical,
                Spacing = new Vector2(0, 6)
            };
            searchArea.Join(MainPanel);
            searchArea.Append(new UIHorizontalSeparator
            {
                Width = StyleDimension.FromPercent(1f),
                Color = Color.Lerp(Color.White, new Color(63, 65, 151, 255), 0.85f) * 0.9f
            });
        }

        // 主面板
        MainPanel = new SUIPanel(UIColor.PanelBorder, UIColor.PanelBg)
        {
            Shaded = true,
            Draggable = true
        };
        MainPanel.SetPadding(0f);
        MainPanel.SetPosPixels(410, 360)
            .SetSizePixels(400, 366)
            .Join(this);

        TitlePanel = new SUIPanel(UIColor.PanelBorder, UIColor.TitleBg2)
        {
            DragIgnore = true,
            Width = {Pixels = 0f, Precent = 1f},
            Height = {Pixels = 50f, Precent = 0f},
            Rounded = new Vector4(10f, 10f, 0f, 0f),
            Relative = RelativeMode.Vertical
        };
        TitlePanel.SetPadding(0f);
        TitlePanel.Join(MainPanel);

        // 标题
        Title = new SUITitle(GetText("UI.OpenBag.Title"), 0.5f)
        {
            VAlign = 0.5f
        };
        Title.Join(TitlePanel);

        // Cross
        Cross = new SUICross
        {
            HAlign = 1f,
            VAlign = 0.5f,
            Height = {Pixels = 0f, Precent = 1f},
            Rounded = new Vector4(0f, 10f, 0f, 0f)
        };
        Cross.OnLeftMouseDown += (_, _) => Close();
        Cross.Join(TitlePanel);

        MakeMainSlotAndButtons();

        // 分割
        MakeSeparator();

        var itemsPanel = new View
        {
            DragIgnore = true,
            Relative = RelativeMode.Vertical,
            OverflowHidden = true
        };
        itemsPanel.SetPadding(15, 0, 14, 14);
        itemsPanel.SetSize(0f, 190f, 1f, 0f);
        itemsPanel.Join(MainPanel);

        // 没有物品时显示的提示，这里先Append，要用到的时候调一下Left就行
        TipText = new UIText(GetText("UI.OpenBag.TipText"))
        {
            Width = {Percent = 1f},
            Height = {Percent = 1f},
            TextOriginX = 0.5f,
            TextOriginY = 0.5f
        };
        itemsPanel.Append(TipText);

        LootsGrid = new BaseGrid();
        LootsGrid.SetBaseValues(-1, 8, new Vector2(4f), new Vector2(43));
        LootsGrid.Join(itemsPanel);

        Scrollbar = new SUIScrollbar {HAlign = 1f};
        Scrollbar.Left.Pixels = -1;
        Scrollbar.Height.Pixels = LootsGrid.Height();
        Scrollbar.SetView(itemsPanel.GetInnerSizePixels().Y, LootsGrid.Height.Pixels);
        Scrollbar.Join(this);
        RefreshGrid();

        var sellAll = new SellAllButton(GetText("UI.OpenBag.SellAll.Name"), SellAll)
        {
            DragIgnore = true,
            Relative = RelativeMode.Vertical,
            OverflowHidden = true,
            HAlign = 0.5f
        };
        sellAll.SetPadding(14, 14, 14, 14);
        sellAll.SetSize(-30f, 38f, 1f, 0f);
        sellAll.Join(MainPanel);
    }

    /// <summary>
    /// 放入袋子的物品框和按钮
    /// </summary>
    private void MakeMainSlotAndButtons()
    {
        var bagPanel = new View
        {
            DragIgnore = true,
            Relative = RelativeMode.Vertical
        };
        bagPanel.SetPadding(6, 6, 6, 6);
        bagPanel.SetSize(0f, 56, 1f, 0f);
        bagPanel.Join(MainPanel);

        var itemSlot = CreateItemSlot(20f, 6f, onItemChanged: (item, _) =>
            {
                if (Main.LocalPlayer is not null && Main.LocalPlayer.TryGetModPlayer(out LootKeeper keeper))
                    keeper.Bag = item;
            },
            parent: bagPanel,
            iconTextureName: "Bag",
            emptyText: () => GetText("UI.OpenBag.EmptyText"));
        itemSlot.OnUpdate += _ =>
        {
            if (Main.LocalPlayer is not null && Main.LocalPlayer.TryGetModPlayer(out LootKeeper keeper))
                itemSlot.Item = keeper.Bag;
        };

        var openButton = new SUIButton(ModAsset.UI_Open.Value, GetText("UI.OpenBag.Open"))
        {
            Left = {Pixels = 80f},
            Top = {Pixels = 8f}
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
            openButton.Text = GetText(CoroutineSystem.OpenBagRunner.Count > 0 ? "UI.OpenBag.Stop" : "UI.OpenBag.Open");
        };
        openButton.Join(bagPanel);

        var depositButton = new SUIButton(ModAsset.Quick.Value, Lang.inter[29].Value) // 强夺全部
        {
            Left = {Pixels = openButton.Left.Pixels + openButton.Width.Pixels + 10f},
            Top = {Pixels = 8f},
            Width = {Pixels = 136f}
        };
        depositButton.OnLeftMouseDown += (_, _) =>
        {
            if (Main.LocalPlayer is null || !Main.LocalPlayer.TryGetModPlayer(out LootKeeper keeper))
                return;

            keeper.Loots.ForEach(l => Main.LocalPlayer.QuickSpawnItem(Main.LocalPlayer.GetSource_Loot(), l, l.stack));
            keeper.Loots.Clear();
            RefreshGrid();

            // 控制提示文本是否显示
            TipText.Left.Pixels = 0;
        };
        depositButton.Join(bagPanel);
    }

    /// <summary>
    /// 协程开袋，在袋子很多的同时不卡，也有一个很好的动画效果
    /// </summary>
    private IEnumerator OpenBagRunner(LootKeeper keeper)
    {
        while (keeper is not null && !keeper.Bag.IsAir && ItemLoader.CanRightClick(keeper.Bag) &&
               (Main.ItemDropsDB.GetRulesForItemID(keeper.Bag.type).Any()))
        {
            LootListener._listening = keeper.Bag.type;
            ItemSlot.TryOpenContainer(keeper.Bag, Main.LocalPlayer);
            LootListener._listening = -1;
            keeper.Bag.stack--;

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

            if (item.value <= 0) return;

            const float sellPercent = 1.1f / 100f;
            totalValue += (ulong)Math.Floor(item.value * item.stack * sellPercent);
        });

        var coins = CoinUtils.ConvertCopperValueToCoins(totalValue);
        coins.ForEach(item =>
        {
            Main.LocalPlayer.QuickSpawnItem(Main.LocalPlayer.GetSource_Loot(), item, item.stack);
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
                    var tip = new UIText(GetText("UI.OpenBag.NotFullyDisplayed", keeper.Loots.Count - 800))
                    {
                        Width = {Percent = 1f},
                        Height = {Pixels = 46f},
                        TextOriginX = 0.5f,
                        TextOriginY = 0.5f
                    };
                    LootsGrid.Append(tip);
                    break;
                }

                var itemSlot = new LootItemSlot(keeper.Loots, i);
                itemSlot.Join(LootsGrid);
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
        LootsGrid.CalculateWithSetGridSize();
        LootsGrid.CalculateWithSetChildrenPosition();
        LootsGrid.Recalculate();
        Scrollbar.SetView(LootsGrid.Parent.GetInnerSizePixels().Y, LootsGrid.Height.Pixels);
    }

    public override void ScrollWheel(UIScrollWheelEvent evt)
    {
        base.ScrollWheel(evt);

        if (!MainPanel.IsMouseHovering)
            return;
        // 下滑: -, 上滑: +
        Scrollbar.BufferViewPosition += evt.ScrollWheelValue * 0.4f;
    }

    public override void DrawSelf(SpriteBatch spriteBatch)
    {
        if (-Scrollbar.ViewPosition == LootsGrid.DatumPoint.Y)
            return;

        LootsGrid.DatumPoint.Y = -Scrollbar.ViewPosition;
        LootsGrid.Recalculate();
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        if (Main.LocalPlayer is null) return;

        if (Main.LocalPlayer.TryGetModPlayer(out LootKeeper keeper) && keeper.Loots.RemoveAll(i => i.IsAir) > 0)
            RefreshGrid();

        if (!MainPanel.IsMouseHovering)
            return;

        PlayerInput.LockVanillaMouseScroll("ImproveGame: Open Bag GUI");
        Main.LocalPlayer.mouseInterface = true;
    }

    public void Open()
    {
        SoundEngine.PlaySound(SoundID.MenuOpen);
        Visible = true;
    }

    public void Close()
    {
        SoundEngine.PlaySound(SoundID.MenuClose);
        Visible = false;
    }
}