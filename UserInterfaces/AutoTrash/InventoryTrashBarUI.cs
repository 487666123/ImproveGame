using ImproveGame.Common.Configs;
using ImproveGame.Common.ModSystems;
using ImproveGame.UI.AutoTrash;
using SilkyUIFramework;
using SilkyUIFramework.Attributes;
using SilkyUIFramework.Elements;
using SilkyUIFramework.Extensions;
using SilkyUIFramework.Layout;
using Terraria.ModLoader.UI;

namespace ImproveGame.UserInterfaces.AutoTrash;

[RegisterUI("Vanilla: Inventory", "Inventory Trash")]
public sealed class InventoryTrashBarUI : BaseBody
{
    private static bool ChestMenuExists =>
        // 原版在制作栏/箱子界面之间会改变垃圾桶的位置和尺寸；经典制作栏且启用网格时不算作箱子布局。
        (Main.LocalPlayer.chest != -1 || Main.npcShop > 0) &&
        !(Player.Settings.CraftingGridControl == Player.Settings.CraftingGridMode.Classic && Main.PipsUseGrid);

    private readonly SUIInventoryTrashSlot[] _slots = new SUIInventoryTrashSlot[AutoTrashPlayer.MaxCapacity];

    public SUIImage SettingsButton { get; set; }
    private bool? _lastChestMenuState;

    public static bool IsVisible { get; set; }

    public override bool Enabled
    {
        // 迁移后的 UI 仍沿用旧版的显示条件，并通过旧类的 Hidden 状态保留快捷键隐藏功能。
        get => UIConfigs.Instance.QoLAutoTrash && Main.playerInventory && IsVisible &&
               (Main.LocalPlayer.chest != -1 || Main.npcShop > 0 || Main.LocalPlayer.talkNPC is -1) &&
               !Main.LocalPlayer.tileEntityAnchor.InUse;
        set { }
    }

    protected override void OnInitialize()
    {
        FlexDirection = FlexDirection.Row;

        Border = 0f;
        BackgroundColor = Color.Transparent;

        SetGap(4);
        SetSize(332f, 44f);

        // 槽位直接绑定最近丢弃列表的索引，物品数据仍由原命名空间下的 ModPlayer 负责存取。
        var player = Main.LocalPlayer.GetModPlayer<AutoTrashPlayer>();

        for (int i = 0; i < _slots.Length; i++)
        {
            _slots[i] = new SUIInventoryTrashSlot(player.RecentlyThrownAwayItems, i).Join(this);
        }

        SettingsButton = new SUIImage(ModAsset.Setting)
        {
            FitWidth = false,
            FitHeight = false,
            ImageScale = new Vector2(0.85f),
            ImageAlign = new Vector2(0.5f),
            Border = 0f,
        }.Join(this);
        SettingsButton.SetSize(44f, 44f);

        // 设置按钮只负责打开已迁移的列表窗口，悬停时切换原版两套图标资源。
        SettingsButton.MouseEnter += (_, _) => SettingsButton.Texture2D = ModAsset.SettingHover;
        SettingsButton.MouseLeave += (_, _) => SettingsButton.Texture2D = ModAsset.Setting;

        SettingsButton.LeftMouseDown += (_, _) =>
        {
            if (UISceneManager.Instance.TryGetInstance<AutoTrashListUI>(out var list))
                list.Toggle();
        };

        UpdateLayoutForInventoryContext();
    }

    protected override void UpdateStatus(GameTime gameTime)
    {
        base.UpdateStatus(gameTime);

        // 只有箱子布局状态变化时才重新计算，避免每帧触发布局树重排。
        if (_lastChestMenuState != ChestMenuExists)
            UpdateLayoutForInventoryContext();
    }

    private void UpdateLayoutForInventoryContext()
    {
        // 箱子界面使用原版较小的垃圾桶槽位；普通背包和创造模式使用较大的布局。
        var chest = ChestMenuExists;
        _lastChestMenuState = chest;

        var size = chest ? 39f : 44f;
        var gap = chest ? 3f : 4f;

        SetSize(size * AutoTrashPlayer.MaxCapacity + gap * AutoTrashPlayer.MaxCapacity + size, size);
        SetLeft(chest ? 73f : Main.GameMode is GameModeID.Creative ? 70f : 20f);
        SetTop(chest ? 426f : 258f);
        SetGap(gap);

        // 槽位和设置按钮必须同步缩放，否则切换界面时图标与容器尺寸会错位。
        foreach (var slot in _slots)
        {
            slot.SetSize(size, size);
            slot.ItemScale = chest ? 0.75f : 0.85f;
        }

        SettingsButton.SetSize(size, size);
        SettingsButton.ImageScale = new Vector2(chest ? 0.75f : 0.85f);
        MarkLayoutDirty();
    }

    protected override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        base.Draw(gameTime, spriteBatch);

        // 提示文本需要在绘制阶段根据当前绑定键动态生成，避免缓存过期的快捷键名称。
        if (SettingsButton.IsMouseHovering)
        {
            MyUtils.TryGetKeybindString(KeybindSystem.AutoTrashKeybind, out var keybind);
            UICommon.TooltipMouseText(MyUtils.GetText("UI.AutoTrash.Introduction", keybind));
        }
    }
}
