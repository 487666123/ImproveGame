using ImproveGame.Core;
using ImproveGame.UI.ExtremeStorage;
using ImproveGame.UIFramework;
using ImproveGame.UIFramework.BaseViews;
using ImproveGame.UIFramework.Common;
using ImproveGame.UIFramework.SUIElements;
using System.Collections;
using Terraria.GameInput;

namespace ImproveGame.UI.QuickShimmer;

[AutoCreateGUI(LayerName.Vanilla.RadialHotbars, "Quick Shimmer GUI")]
public class QuickShimmerGUI : BaseBody
{
    private const string _context = $"{nameof(ImproveGame)}: {nameof(QuickShimmerGUI)}";

    public static QuickShimmerGUI Instance { get; private set; }

    public QuickShimmerGUI() => Instance = this;

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
            TextOrKey = "Mods.ImproveGame.UI.QuickShimmer.Title",
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
        TipText = new UIText(MyUtils.GetText("UI.QuickShimmer.TipText"))
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
        bagPanel.SetSize(0f, 150, 1f, 0f);
        bagPanel.JoinParent(MainPanel);

        var itemSlot = MyUtils.CreateItemSlot(20f, 6f, onItemChanged: (item, _) =>
        {
            if (Main.LocalPlayer is not null && Main.LocalPlayer.TryGetModPlayer(out ShimmerLootKeeper keeper))
                keeper.targetItem = item;
        },
            parent: bagPanel,
            iconTextureName: "Bag",
            emptyText: () => MyUtils.GetText("UI.QuickShimmer.EmptyText"));
        itemSlot.OnUpdate += _ =>
        {
            if (Main.LocalPlayer is not null && Main.LocalPlayer.TryGetModPlayer(out ShimmerLootKeeper keeper))
                itemSlot.Item = keeper.targetItem;
        };

        var openButton = new SUIButton(ModAsset.Open.Value, MyUtils.GetText("UI.QuickShimmer.Open"))
        {
            Left = { Pixels = 80f },
            Top = { Pixels = 8f }
        };
        openButton.OnLeftMouseDown += (_, _) =>
        {
            if (CoroutineSystem.QuickShimmerRunner.Count > 0)
            {
                CoroutineSystem.QuickShimmerRunner.StopAll();
                return;
            }
            if (Main.LocalPlayer is null || !Main.LocalPlayer.TryGetModPlayer(out ShimmerLootKeeper keeper))
                return;

            // 钱币幸运直接换完
            var item = keeper.targetItem;
            int coinValue = ItemID.Sets.CoinLuckValue[item.type];
            if (coinValue > 0)
            {
                SoundEngine.PlaySound(SoundID.Item176);
                float coinLuckOld = Main.LocalPlayer.coinLuck;
                Main.LocalPlayer.AddCoinLuck(Main.LocalPlayer.Center, coinValue * item.stack);
                float coinLuckGain = Main.LocalPlayer.coinLuck - coinLuckOld;
                MyUtils.AddNotification(MyUtils.GetText("UI.QuickShimmer.CoinLuck", coinLuckGain), Color.Pink, item.type);
                item.TurnToAir();
                return;
            }

            // 否则进入正常转化
            var items = CollectHelper.GetShimmerResult(item, out int stackRequired, out int decraftingRecipeIndex);
            if (items is null)
                return;

            CoroutineSystem.QuickShimmerRunner.Run(QuickShimmerRunner(keeper, items, stackRequired,
                decraftingRecipeIndex));
        };
        openButton.OnUpdate += _ =>
        {
            openButton.Text = MyUtils.GetText(CoroutineSystem.QuickShimmerRunner.Count > 0
                ? "UI.QuickShimmer.Stop"
                : "UI.QuickShimmer.Open");
            //openButton.SetIcon(CoroutineSystem.QuickShimmerRunner.Count > 0 ? ModAsset.Shimmer_Pause.Value : ModAsset.Shimmer_Start.Value);
            if (Main.LocalPlayer is null || !Main.LocalPlayer.TryGetModPlayer(out ShimmerLootKeeper keeper))
                return;

            if (CoroutineSystem.QuickShimmerRunner.Count > 0 && keeper.targetItem.IsAir)
                CoroutineSystem.QuickShimmerRunner.StopAll();
            if (CoroutineSystem.QuickShimmerRunner.Count > 0 || keeper.targetItem.IsAir || !AutoStart)
                return;
            // 钱币幸运直接换完
            var item = keeper.targetItem;
            int coinValue = ItemID.Sets.CoinLuckValue[item.type];
            if (coinValue > 0)
            {
                SoundEngine.PlaySound(SoundID.Item176);
                float coinLuckOld = Main.LocalPlayer.coinLuck;
                Main.LocalPlayer.AddCoinLuck(Main.LocalPlayer.Center, coinValue * item.stack);
                float coinLuckGain = Main.LocalPlayer.coinLuck - coinLuckOld;
                MyUtils.AddNotification(MyUtils.GetText("UI.QuickShimmer.CoinLuck", coinLuckGain), Color.Pink, item.type);
                item.TurnToAir();
                return;
            }

            // 否则进入正常转化
            var items = CollectHelper.GetShimmerResult(item, out int stackRequired, out int decraftingRecipeIndex);
            if (items is null)
                return;

            CoroutineSystem.QuickShimmerRunner.Run(QuickShimmerRunner(keeper, items, stackRequired,
                decraftingRecipeIndex));
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
            if (Main.LocalPlayer is null || !Main.LocalPlayer.TryGetModPlayer(out ShimmerLootKeeper keeper))
                return;

            keeper.Loots.ForEach(l => Main.LocalPlayer.QuickSpawnItem(Main.LocalPlayer.GetSource_Loot(), l));
            keeper.Loots.Clear();
            RefreshGrid();

            // 控制提示文本是否显示
            TipText.Left.Pixels = 0;
        };
        depositButton.JoinParent(bagPanel);

        var autoStartToggle = new LongSwitch(
            () => AutoStart,
            state => AutoStart = state,
            "UI.QuickShimmer.AutoStart")
        {
            ResetAnotherPosition = true,
            RelativeMode = RelativeMode.Vertical,
            Spacing = new Vector2(0, 16),
            FixedAnchorPixels = 18,
            Width = { Pixels = 358, Percent = 0f },
            Height = { Pixels = 34f }
        };
        autoStartToggle.JoinParent(bagPanel);

        var quickModeToggle = new LongSwitch(
            () => QuickMode,
            state => QuickMode = state,
            "UI.QuickShimmer.ReallyQuick")
        {
            ResetAnotherPosition = true,
            RelativeMode = RelativeMode.Vertical,
            Spacing = new Vector2(0, 8),
            FixedAnchorPixels = 18,
            Width = { Pixels = 358, Percent = 0f },
            Height = { Pixels = 34f }
        };
        quickModeToggle.JoinParent(bagPanel);
    }
    public static bool AutoStart;
    public static bool QuickMode;
    /// <summary>
    /// 协程开袋，在袋子很多的同时不卡，也有一个很好的动画效果
    /// </summary>
    private IEnumerator QuickShimmerRunner(ShimmerLootKeeper keeper, List<Item> items, int stackRequired,
        int decraftingRecipeIndex)
    {
        if (keeper is null)//|| keeper.targetItem.IsAir
            yield break;

        var item = keeper.targetItem;
        // 可以转化的次数
        int decraftAmount = item.stack / stackRequired;
        // 计算步长，一帧进行多少次转化
        int step;
        if (QuickMode)
            step = decraftAmount;
        else
        {
            step = 1;
            if (decraftAmount > 100)
            {
                int counter = decraftAmount;
                while (counter > 0)
                {
                    counter /= 10;
                    step *= 10;
                }

                step /= 100;
            }
        }

        // 执行Decraft的次数
        int decraftExecuted = 0;
        while (!item.IsAir && item.CanShimmer())
        {
            // 原版写法，这里仅仅是做了点小改动
            foreach (var result in items)
            {
                if (result.type <= ItemID.None)
                    break;

                var recipe = decraftingRecipeIndex >= 0 ? Main.recipe[decraftingRecipeIndex] : null;

                int resultStack = result.stack;
                /*if (recipe is { alchemy: true })
                {
                    for (int i = resultStack; i > 0; i--)
                    {
                        if (Main.rand.NextBool(3))
                            resultStack--;
                    }
                }*/
                if (recipe != null)
                    RecipeLoader.ConsumeIngredient(recipe, result.type, ref resultStack, isDecrafting: true);
                while (resultStack > 0)
                {
                    int outputStack = resultStack;
                    if (outputStack > result.maxStack)
                        outputStack = result.maxStack;

                    resultStack -= outputStack;
                    keeper.AddToLoots(new Item(result.type, outputStack));
                }
            }

            item.stack -= stackRequired;
            if (item.stack <= 0) // 原版写法，我也不知道为什么不TurnToAir
            {
                item.stack = 0;
                item.type = ItemID.None;
            }

            RefreshGrid();
            decraftExecuted++;
            if (decraftExecuted % step == 0)
            {
                var sound = SoundID.Item176;
                sound.MaxInstances = 0; // 爽！！！！
                SoundEngine.PlaySound(sound);
                yield return 1;
            }
        }
    }

    private void RefreshGrid()
    {
        LootsGrid.RemoveAllChildren();

        if (Main.LocalPlayer is not null && Main.LocalPlayer.TryGetModPlayer(out ShimmerLootKeeper keeper))
        {
            for (var i = 0; i < keeper.Loots.Count; i++)
            {
                // 最多显示一百列，不然生成Children时太卡了
                if (i >= 8 * 100)
                {
                    var tip = new UIText(MyUtils.GetText("UI.QuickShimmer.NotFullyDisplayed", keeper.Loots.Count - 800))
                    {
                        Width = { Percent = 1f },
                        Height = { Pixels = 46f },
                        TextOriginX = 0.5f,
                        TextOriginY = 0.5f
                    };
                    LootsGrid.Append(tip);
                    break;
                }
                var itemSlot = new OpenBag.LootItemSlot(keeper.Loots, i, _context);
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

        if (Main.LocalPlayer.TryGetModPlayer(out ShimmerLootKeeper keeper))
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