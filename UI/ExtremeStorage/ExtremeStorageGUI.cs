using ImproveGame.Content.Tiles;
using ImproveGame.Packets.NetStorager;
using ImproveGame.UIFramework;
using ImproveGame.UIFramework.BaseViews;
using ImproveGame.UIFramework.Common;
using ImproveGame.UIFramework.SUIElements;
using PinyinNet;
using System.Reflection;
using System.Text.RegularExpressions;
using Terraria.GameInput;

namespace ImproveGame.UI.ExtremeStorage;

public class ExtremeStorageGUI : BaseBody, ISidedView
{
    // 用于捕获分辨率变化，以便 Recalculate 并且重新计算位置
    private class CaptureResolutionChange : ILoadable
    {
        public void Load(Mod mod)
        {
            Main.OnResolutionChanged += OnResolutionChangedHandler;
        }

        public void Unload()
        {
            Main.OnResolutionChanged -= OnResolutionChangedHandler;
        }

        private void OnResolutionChangedHandler(Vector2 vector)
        {
            _recalculateNextTick = true;
        }
    }

    public override bool Enabled { get => Player.Settings.CraftingGridControl != Player.Settings.CraftingGridMode.Classic || !Main.PipsUseGrid; set { } }

    public static bool Visible => SidedEventTrigger.IsOpened(UISystem.Instance.ExtremeStorageGUI);

    public static bool VisibleAndExpanded => Visible && !UISystem.Instance.ExtremeStorageGUI._foldTimer.Closed;

    public static ItemGroup CurrentGroup { get; private set; } = ItemGroup.Everything;

    /// <summary>
    /// 真正的组，除了某几个特定组外，其他都是筛选器，要重定向到Everything组。
    /// 视情况使用，比如找合成的时候应该用RealGroup
    /// </summary>
    public static ItemGroup RealGroup => CurrentGroup.RedirectGroupToCategory();

    public static TEExtremeStorage Storage { get; set; }
    public static Item[] AllItemsCached { get; private set; } // 缓存的所有附近物品，用于减少获取全物品时的卡顿
    public float SwapSlideFactor { get; set; }

    internal static Dictionary<int, float[]> ChestSlotsGlowHue;
    internal static int ChestSlotsGlowTimer;
    internal static bool DisplayCrafting;

    private static List<int> _prevChestIndexes;
    private static int _findChestCountdown;
    private static bool _recalculateNextTick;

    private AnimationTimer _foldTimer, _scrollBarTimer;
    private UIPanel _totalPanel; // 透明的用于定位的面板
    private SUIPanel _mainPanel;
    private StorageGrids _itemGrid;

    public override void OnInitialize()
    {
        ChestSlotsGlowHue = new Dictionary<int, float[]>();
        _scrollBarTimer = new AnimationTimer();
        _foldTimer = new AnimationTimer();

        _totalPanel = new UIPanel
        {
            BackgroundColor = Color.Transparent,
            BorderColor = Color.Transparent
        };
        _totalPanel.SetPos(60f, Main.instance.invBottom + 40).SetSize(560f, 600f);
        Append(_totalPanel);

        _mainPanel = new SUIPanel(UIStyle.PanelBorder, UIStyle.PanelBg)
        {
            Shaded = true
        };
        _mainPanel.SetPos(0, 34).SetSize(-78f, 0f, 1f, 1f);
        _totalPanel.Append(_mainPanel);

        _mainPanel.Append(_itemGrid = new StorageGrids
        {
            ResetAnotherPosition = true,
            RelativeMode = RelativeMode.Vertical,
            Spacing = new Vector2(10, 15)
        });

        _totalPanel.Append(new GroupTab(ItemGroup.Everything));
        _totalPanel.Append(new GroupTab(ItemGroup.Weapon));
        _totalPanel.Append(new GroupTab(ItemGroup.Tool));
        _totalPanel.Append(new GroupTab(ItemGroup.Ammo));
        _totalPanel.Append(new GroupTab(ItemGroup.Armor));
        _totalPanel.Append(new GroupTab(ItemGroup.Accessory));
        _totalPanel.Append(new GroupTab(ItemGroup.Block));
        _totalPanel.Append(new GroupTab(ItemGroup.Misc));
        _totalPanel.Append(new GroupTab(ItemGroup.Furniture));
        _totalPanel.Append(new GroupTab(ItemGroup.Alchemy));
        _totalPanel.Append(new GroupTab(ItemGroup.Setting));

        _mainPanel.Append(new SettingsPage());

        // 初始数据设置
        _foldTimer.Timer = _foldTimer.TimerMax;
        _foldTimer.State = AnimationState.Opened;
        _totalPanel.Left.Pixels = 20;
    }

    public override void DrawSelf(SpriteBatch spriteBatch)
    {
        base.DrawSelf(spriteBatch);

        // 是否开启制作栏侧栏，在这里而不是Update里写，支持High FPS Support Mod
        Main.hidePlayerCraftingMenu |= !DisplayCrafting;
    }

    public override void Update(GameTime gameTime)
    {
        if (_recalculateNextTick)
        {
            _recalculateNextTick = false;
            Recalculate();
        }

        base.Update(gameTime);

        // mouseInterface
        bool isMouseHovering = _totalPanel.Children.Any(element => element.IsMouseHovering);
        if (isMouseHovering)
            Main.LocalPlayer.mouseInterface = true;

        // 锁定 MouseScroll
        if (_mainPanel.IsMouseHovering)
            PlayerInput.LockVanillaMouseScroll("ImproveGame: Extreme Storage GUI");

        // 动画计时器
        UpdateAnimationTimer();

        // 物品整理的箱子槽位发光
        UpdateChestSlotGlow();

        // Setting 并不是一个实际上的 ItemGroup，所以不要也不能更新
        if (_findChestCountdown % 18 == 0 && CurrentGroup is not ItemGroup.Setting)
        {
            FindChestsAndPopulate();
        }

        _findChestCountdown++;

        // 如果打开了制作大面板，强制将DisplayCrafting设为true
        if (!DisplayCrafting && Player.Settings.CraftingGridControl == Player.Settings.CraftingGridMode.Classic && Main.PipsUseGrid)
        {
            DisplayCrafting = true;
            _foldTimer.Timer = 0;
        }

        // 根据屏幕高度重设框高度
        _itemGrid.RecalculateScrollBar();
        var screenDimensions = GetDimensions();
        int screenHeightZoomed = (int)screenDimensions.Height;
        float oldWidth = _totalPanel.Width.Pixels;
        float oldHeight = _totalPanel.Height.Pixels;
        float oldLeft = _totalPanel.Left.Pixels;

        // 应用动画，改变面板大小
        float maxPanelHeight = screenHeightZoomed - Main.instance.invBottom - 80;
        float panelHeight = MathHelper.Lerp(230, maxPanelHeight, _foldTimer.Schedule);
        float panelWidth = MathHelper.Lerp(560f, 588f, _scrollBarTimer.Schedule);
        _totalPanel.SetSize(panelWidth, panelHeight);

        float widthNext = _totalPanel.GetDimensions().Width;
        float shownPositionNext = MathHelper.Lerp(60, 20, _foldTimer.Schedule);
        float hiddenPositionNext = -widthNext - 78;
        _totalPanel.Left.Set((int)MathHelper.Lerp(hiddenPositionNext, shownPositionNext, SwapSlideFactor), 0f);

        if (oldWidth != _totalPanel.Width.Pixels || oldHeight != _totalPanel.Height.Pixels ||
            oldLeft != _totalPanel.Left.Pixels)
            _recalculateNextTick = true;
    }

    private void UpdateAnimationTimer()
    {
        if (_itemGrid.ScrollBarFilled || CurrentGroup is ItemGroup.Setting)
            _scrollBarTimer.Close();
        else
            _scrollBarTimer.Open();

        if (!DisplayCrafting)
            _foldTimer.Open();
        else
            _foldTimer.Close();

        _scrollBarTimer.Update();
        _foldTimer.Update();
    }

    private void UpdateChestSlotGlow()
    {
        if (ChestSlotsGlowTimer <= 0) return;

        ChestSlotsGlowTimer--;

        if (ChestSlotsGlowTimer is 0)
            ChestSlotsGlowHue.Clear();
    }

    // 寻找箱子并设置物品栏
    internal void FindChestsAndPopulate(bool forced = false)
    {
        // 查找名字相应的箱子
        var chestIndexes = Storage.FindAllNearbyChestsWithGroup(RealGroup);

        // 判断箱子是否相同，如果相同则不更新，用于周期性更新的(18tick一次)
        bool sameChest = _prevChestIndexes is not null && _prevChestIndexes.SequenceEqual(chestIndexes);
        if (!forced && sameChest)
            return;

        // 更新物品之前先更新工具栏，把新的筛选器选出来，这样初次切换才能正确选取筛选器选择正确物品
        _itemGrid.ResetToolbar(CurrentGroup);

        // 更新物品
        var items = new List<ChestItemsInfo>();
        chestIndexes.ForEach(i =>
        {
            var chestItems = Main.chest[i].item;
            var displayedItemIndexes = GetDisplayedItemIndexes(chestItems);
            items.Add(new ChestItemsInfo(chestItems, i, displayedItemIndexes));
        });

        _prevChestIndexes = chestIndexes;
        _itemGrid.ResetInventory(items);
        RefreshCachedAllItems();
        // Recipe.FindRecipes();
    }

    private IEnumerable<int> GetDisplayedItemIndexes(IReadOnlyList<Item> chestItems)
    {
        string RemoveSpaces(string s) => s.Replace(" ", "", StringComparison.Ordinal);

        string searchContent = RemoveSpaces(_itemGrid.SearchContent.ToLower());

        var checkList = new List<(string, int, string)>();

        var regExp = @"\[([a-z]+)(>|<|=|>=|<=)?([0-9]+)?\]";

        foreach (var i in _itemGrid.SearchContent.ToLower().Split(" "))
        {
            if (Regex.IsMatch(i, regExp))
            {
                var group = Regex.Match(i, regExp).Groups;
                if (group[2].Value != "")
                    checkList.Add((group[1].Value, int.Parse(group[3].Value), group[2].Value));
                else
                    checkList.Add((group[1].Value, 1, ""));
                searchContent = searchContent.Replace(i, "");
            }
        }

        bool check(Item item, (string variable, int val, string check) condition)
        {
            var f = typeof(Item).GetField(condition.variable,
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            var p = typeof(Item).GetProperty(condition.variable,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (f == null && p == null)
            {
                Main.NewText($"Field or Property not found: {condition.variable}");
                checkList.Remove(condition);
                return true;
            }

            var field = f == null ? p.GetValue(item) : f.GetValue(item);

            int value = -1;

            switch (field)
            {
                case int:
                    value = (int)field;
                    break;
                case bool:
                    value = Convert.ToInt32(field);
                    break;
                default:
                    Main.NewText($"Illegal format: {condition.variable}");
                    checkList.Remove(condition);
                    return true;
            }

            switch (condition.check)
            {
                case ">":
                    return value > condition.val;
                case "<":
                    return value < condition.val;
                case "=":
                    return value == condition.val;
                case ">=":
                    return value >= condition.val;
                case "<=":
                    return value <= condition.val;
                default:
                    if (field is bool) return value == condition.val;
                    return false;
            }
        }

        bool checkAll(Item item, List<(string, int, string)> conditions)
        {
            if (conditions.Count == 0) return true;
            var conditionsCache = new List<(string, int, string)>(conditions.ToArray());
            foreach (var i in conditionsCache)
                if (!check(item, i) || item.stack == 0)
                    return false;
            return true;
        }

        for (var k = 0; k < chestItems.Count; k++)
        {
            var item = chestItems[k];
            if (!StorageHelper.CheckFromFilter(item, CurrentGroup))
                continue;
            if (!_itemGrid.CheckFiltersForItem(item))
                continue;
            if (!checkAll(item, checkList))
                continue;

            int itemType = item.type;
            string internalName =
                RemoveSpaces(ItemID.Search.GetName(itemType).ToLower()); // 《英文名》因为没法在非英语语言获取英文名，只能用内部名了
            string currentLanguageName = RemoveSpaces(Lang.GetItemNameValue(itemType).ToLower());
            if (internalName.Contains(searchContent) || currentLanguageName.Contains(searchContent))
            {
                yield return k;
                continue;
            }

            if (Language.ActiveCulture.Name is not "zh-Hans") continue;

            string pinyin = RemoveSpaces(PinyinConvert.GetPinyinForAutoComplete(currentLanguageName));
            if (pinyin.Contains(searchContent))
            {
                yield return k;
            }
        }
    }

    /// <summary>
    /// 打开GUI界面
    /// </summary>
    public void Open()
    {
        SetGroup(CurrentGroup); // 用于重置一些基础设置
        OpenStateUpdatePacket.Send(Storage.ID);

        MyUtils.OperateInventory(true);
        SoundEngine.PlaySound(SoundID.MenuOpen);

        FindChestsAndPopulate(true);

        ChestSlotsGlowHue = new Dictionary<int, float[]>();
        ChestSlotsGlowTimer = 0;

        // 在这里先调整好动画计时器
        if (_itemGrid.ScrollBarFilled)
        {
            _scrollBarTimer.Timer = 0f;
            _scrollBarTimer.State = AnimationState.Closed;
        }
        else
        {
            _scrollBarTimer.Timer = _scrollBarTimer.TimerMax;
            _scrollBarTimer.State = AnimationState.Opened;
        }
    }

    public void Close()
    {
        OpenStateUpdatePacket.SendClose();
        Main.blockInput = false;
        SoundEngine.PlaySound(SoundID.MenuClose);
        ChestSelection.IsSelecting = false;
        // Recipe.FindRecipes();
    }

    public static void SetGroup(ItemGroup group)
    {
        CurrentGroup = group;
        _findChestCountdown = 0;
        _prevChestIndexes = null; // 用于强制刷新箱子列表，强制刷新物品栏
        // Setting 并不是一个实际上的 ItemGroup
        UISystem.Instance.ExtremeStorageGUI._itemGrid.Hide = CurrentGroup is ItemGroup.Setting;
        DisplayCrafting &= CurrentGroup is not ItemGroup.Setting;
    }

    public static void SetSearchContent(string content)
    {
        UISystem.Instance.ExtremeStorageGUI._itemGrid.SearchBar.SetText(content);
    }

    public static void RefreshCachedAllItems() => AllItemsCached = GetAllItems();

    /// <summary>
    /// 根据当前group寻找附近所有箱子
    /// </summary>
    /// <returns></returns>
    public static List<int> FindChestsWithCurrentGroup() => Storage.FindAllNearbyChestsWithGroup(CurrentGroup);

    public static Item[] GetAllItems() => Storage.GetAllItemsByGroup(RealGroup);

    public override bool CanSetFocusTarget(UIElement target) =>
        (target != this && _mainPanel.IsMouseHovering) || _mainPanel.IsLeftMousePressed;
}