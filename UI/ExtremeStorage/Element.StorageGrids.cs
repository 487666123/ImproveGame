using ImproveGame.Common.ModSystems;
using ImproveGame.UI.ExtremeStorage.ToolButtons;
using ImproveGame.UI.ExtremeStorage.ToolButtons.FilterButtons;
using ImproveGame.UIFramework;
using ImproveGame.UIFramework.BaseViews;
using ImproveGame.UIFramework.SUIElements;
using ImproveGame.UIFramework.UIElements;

namespace ImproveGame.UI.ExtremeStorage;

public class StorageGrids : ModItemGrid
{
    public bool ScrollBarFilled => Scrollbar.InnerFilled;
    public string SearchContent => SearchBar.SearchContent ?? "";
    private float SearchBarHeight => SearchBar.Visible ? SearchBar.Height() + 6 : 0;

    internal static IEnumerable<FilterButton> FilterButtons = [];

    /// <summary> 缓存一个所有被使用的箱子的列表，用于物品栏检测是否可用 </summary>
    internal static HashSet<int> ChestsThatBeingUsedCache;

    internal bool Hide;
    public readonly SUISearchBar SearchBar;
    private readonly View _toolSection;
    private readonly View _contentSection;

    public StorageGrids()
    {
        RemoveAllChildren(); // 将父类实例化添加的统统移除

        ShowSize = ModItemList.GetSize(10, 4, 40, 4f);

        Scrollbar = new SUIScrollBar
        {
            HAlign = 1f,
            Height = new StyleDimension(0f, 1f),
            HideIfFilled = true
        };
        Scrollbar.JoinParent(this);

        _contentSection = new View
        {
            Width = StyleDimension.FromPixels(ShowSize.X),
            Left = StyleDimension.FromPixels(0f),
            IsAdaptiveHeight = true,
            PaddingTop = 4
        };
        _contentSection.JoinParent(this);

        SearchBar = new SUISearchBar(true, false)
        {
            Width = StyleDimension.FromPixels(ShowSize.X - 4f),
            Left = StyleDimension.FromPixels(2f),
            RelativeMode = RelativeMode.Vertical
        };
        SearchBar.OnSearchContentsChanged += _ => UISystem.Instance.ExtremeStorageGUI.FindChestsAndPopulate(true);
        SearchBar.JoinParent(_contentSection);

        _toolSection = new View
        {
            Width = StyleDimension.FromPixels(ShowSize.X - 4f),
            Left = StyleDimension.FromPixels(2f),
            Height = StyleDimension.FromPixels(32f),
            Spacing = new Vector2(0f, 4f),
            RelativeMode = RelativeMode.Vertical
        };
        _toolSection.SetPadding(0f);
        _toolSection.JoinParent(_contentSection);

        _contentSection.MakeHorizontalSeparator(-12);

        ItemList = new ItemSlotList
        {
            DragIgnore = true,
            RelativeMode = RelativeMode.Vertical
        };
        ItemList.JoinParent(_contentSection);

        Width.Pixels = ShowSize.X + Scrollbar.Width.Pixels + 11f;
        Height = new StyleDimension(0f, 1f);
    }

    public override void Update(GameTime gameTime)
    {
        if (Hide) return;

        ChestsThatBeingUsedCache = [];
        for (int i = 0; i < Main.maxPlayers; i++)
        {
            var plr = Main.player[i];
            if (plr is null || !plr.active || plr.dead || plr.chest < 0) continue;
            ChestsThatBeingUsedCache.Add(plr.chest);
        }

        base.Update(gameTime);

        // 移出范围以免影响其他UI
        if (Scrollbar.InnerFilled && Scrollbar.Left.Pixels is not 30f)
        {
            Scrollbar.Left.Pixels = 30f;
            Scrollbar.Recalculate();
        }

        if (!Scrollbar.InnerFilled && Scrollbar.Left.Pixels is not 0f)
        {
            Scrollbar.Left.Pixels = 0f;
            Scrollbar.Recalculate();
        }

        ref string placeholder = ref SearchBar.SearchBarInner.InnerText.Placeholder;
        placeholder = MyUtils.GetText("Search");
        if (string.Equals(Language.ActiveCulture.Name, "zh-Hans"))
            placeholder = "搜索名称(支持拼音或首字母): ";
        if (Main.HoverItem is { } item && !item.IsAir && UISystem.Instance.ExtremeStorageGUI is not null && MyUtils.TryGetKeybindString(KeybindSystem.ExtremeStorageSearch, out string keybindName))
        {
            placeholder = MyUtils.GetText("UI.ExtremeStorage.QuickSearch", keybindName);
        }
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        if (!Hide) base.Draw(spriteBatch);

        if (!SearchBar.IsWritingText)
            return;

        Vector2 position = SearchBar.GetDimensions().ToRectangle().BottomLeft();
        position.Y += 32f;
        Main.instance.SetIMEPanelAnchor(position, 0f);
    }

    public override void DrawSelf(SpriteBatch spriteBatch)
    {
        if (-Scrollbar.BarTop == _contentSection.Top.Pixels)
            return;

        _contentSection.Top.Pixels = -Scrollbar.BarTop;
        _contentSection.Recalculate();
    }

    public void ResetInventory(List<ChestItemsInfo> items)
    {
        SetInventory(items);
        Recalculate();
    }

    public void ResetToolbar(ItemGroup group)
    {
        SetupTools(group);
    }

    private void SetInventory(List<ChestItemsInfo> items)
    {
        if (ItemList is not ItemSlotList itemList) return;

        // 初始化 ItemList 的时候会计算高度, 但是计算的是显示的高度.
        // 在 SetInventory 之后还会再计算一次, 计算的是 添加 items 之后的实际高度.
        itemList.SetInventory(items, 40, 4f);
        ShowSize = itemList.GetSize();
        RecalculateScrollBar();
    }

    private void SetupTools(ItemGroup group)
    {
        _toolSection.RemoveAllChildren();

        // 啥时候都有的按钮
        AddToolButton<RecipeToggleButton>();
        AddToolButton<AddChestButton>();

        switch (group)
        {
            case ItemGroup.Everything:
                AddToolButton<SortButton>();
                AddToolButton<DepositAllButton>();
                AddToolButton<StackToInventoryButton>();
                AddToolButton<StackToStorageButton>();
                break;
            case ItemGroup.Weapon:
                break;
            case ItemGroup.Tool:
                break;
            case ItemGroup.Ammo:
                break;
            case ItemGroup.Armor:
                break;
            case ItemGroup.Accessory:
                break;
            case ItemGroup.Block:
                break;
            case ItemGroup.Misc:
                break;
            case ItemGroup.Alchemy:
                AddToolButton<SortButton>();
                AddToolButton<DepositAllButton>();
                AddToolButton<StackToInventoryButton>();
                AddToolButton<StackToStorageButton>();
                break;
            case ItemGroup.Furniture:
                AddToolButton<SortButton>();
                AddToolButton<DepositAllButton>();
                AddToolButton<StackToInventoryButton>();
                AddToolButton<StackToStorageButton>();
                break;
            case ItemGroup.Setting:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(group), group, null);
        }

        AddFilterButtonForGroup(group);
    }

    public bool CheckFiltersForItem(Item item)
    {
        var activatedButtons = FilterButtons.Where(filterButton => filterButton.Activated).ToList();
        return activatedButtons.Count is 0 ||
            // 只要有一个过了就行，这样多选的话就是并集
            activatedButtons.Any(filterButton => filterButton.Filter(item));
    }

    private void AddFilterButtonForGroup(ItemGroup group)
    {
        if (!ToolHandler.TryGetFilterButtons(group, out var buttons))
            return;
        // 不要throw，因为有些组没有筛选器
        // throw new InvalidOperationException($"FilterButtons for {group} is not registered.");

        _toolSection.MakeVerticalSeparator(heightOffset: -2, spacing: 5);
        FilterButtons = buttons;
        foreach (var button in FilterButtons)
            _toolSection.Append(button);
    }

    private void AddToolButton<T>() where T : ToolButton
    {
        if (!ToolHandler.TryGetButton<T>(out var button))
            throw new InvalidOperationException($"ToolButton {typeof(T).Name} is not registered.");
        _toolSection.Append(button);
    }

    public void RecalculateScrollBar()
    {
        float highestY = ItemList.BottomPixels + 8;
        float lowestY = SearchBar.Top();
        float totalHeight = highestY - lowestY;
        Scrollbar.SetView(GetDimensions().Height, totalHeight);
    }
}