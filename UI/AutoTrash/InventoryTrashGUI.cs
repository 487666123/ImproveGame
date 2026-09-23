using ImproveGame.Common.Configs;
using ImproveGame.Common.ModSystems;
using ImproveGame.UIFramework;
using ImproveGame.UIFramework.BaseViews;
using ImproveGame.UIFramework.SUIElements;
using ImproveGame.UserInterfaces.AutoTrash;
using SilkyUIFramework;
using Terraria.ModLoader.UI;

namespace ImproveGame.UI.AutoTrash;

[AutoCreateGUI(LayerName.Vanilla.Inventory, "Inventory Trash")]
public class InventoryTrashGUI : BaseBody
{
    #region 抽象实现

    public override bool Enabled
    {
        get => UIConfigs.Instance.QoLAutoTrash && Main.playerInventory && !Hidden &&
               (Main.LocalPlayer.chest != -1 || Main.npcShop > 0 || Main.LocalPlayer.talkNPC is -1) &&
               !Main.LocalPlayer.tileEntityAnchor.InUse;
        set { }
    }

    public override bool CanSetFocusTarget(UIElement target) => Window.IsMouseHovering;
    #endregion

    private static bool ChestMenuExists => (Main.LocalPlayer.chest != -1 || Main.npcShop > 0) && !(Player.Settings.CraftingGridControl == Player.Settings.CraftingGridMode.Classic && Main.PipsUseGrid);
    internal static bool Hidden = false;
    public SUIPanel Window;
    public BaseGrid TrashGrid;
    public SUIImage SettingsButton;

    public override void OnInitialize()
    {
        if (Main.LocalPlayer is not null &&
            Main.LocalPlayer.TryGetModPlayer(out AutoTrashPlayer atPlayer))
        {
            Window = new SUIPanel(Color.Transparent, Color.Transparent);
            Window.SetPosPixels(20f, 258f);
            Window.SetPadding(0);
            Window.SetSizePixels(332f, 44f);

            // 上面物品栏的网格
            TrashGrid = new BaseGrid();
            TrashGrid.SetBaseValues(1, AutoTrashPlayer.MaxCapacity, new Vector2(4f), new Vector2(44f));

            for (int i = 0; i < AutoTrashPlayer.MaxCapacity; i++)
            {
                var itemSlot = new InventoryTrashSlot(atPlayer.RecentlyThrownAwayItems, i)
                {
                    Border = 2f * 0.85f,
                    ItemIconScale = 0.85f
                };
                itemSlot.JoinParent(TrashGrid);
            }

            TrashGrid.CalculateAndSetSize();
            TrashGrid.CalculateAndSetChildrenPosition();
            TrashGrid.Recalculate();
            TrashGrid.JoinParent(Window);

            // 设置按钮
            Texture2D setting = ModAsset.Setting.Value;
            Texture2D settingHover = ModAsset.SettingHover.Value;

            SettingsButton = new SUIImage(setting);
            SettingsButton.ImagePosition.X = 4f;
            SettingsButton.ImagePercent.Y = 0.5f;
            SettingsButton.ImageOrigin.Y = 0.5f;
            SettingsButton.ImageScale = 0.85f;

            SettingsButton.SetSizePixels(ClosedChestTrashSize);
            SettingsButton.SetRoundedRectProperties(default, default, default, 0f);

            SettingsButton.JoinParent(Window);

            SettingsButton.OnMouseOver += (_, _) => SettingsButton.Texture = settingHover;
            SettingsButton.OnMouseOut += (_, _) => SettingsButton.Texture = setting;
            SettingsButton.OnLeftMouseDown += (_, _) =>
            {
                if (UISceneManager.Instance.TryGetInstance<AutoTrashListUI>(out var garbageList))
                    garbageList.Toggle();
            };

            Window.JoinParent(this);
        }
    }

    /// <summary>
    /// 开启 Chest 的时候原版垃圾桶 UI 的 Top 位置
    /// </summary>
    public static readonly float OpenedChestTrashTop = 426f;
    /// <summary>
    /// 开启 Chest 的时候原版垃圾桶 UI 的大小
    /// </summary>
    public static readonly Vector2 OpenedChestTrashSize = new Vector2(39f);
    /// <summary>
    /// 开启 Chest 的时候原版垃圾桶 UI 的间距
    /// </summary>
    public static readonly Vector2 OpenedChestTrashSpacing = new Vector2(3f);

    /// <summary>
    /// 关闭 Chest 的时候原版垃圾桶 UI 的 Top 位置
    /// </summary>
    public static readonly float ClosedChestTrashTop = 258f;
    /// <summary>
    /// 关闭 Chest 的时候原版垃圾桶 UI 的大小
    /// </summary>
    public static readonly Vector2 ClosedChestTrashSize = new Vector2(44f);
    /// <summary>
    /// 关闭 Chest 的时候原版垃圾桶 UI 的间距
    /// </summary>
    public static readonly Vector2 ClosedChestTrashSpacing = new Vector2(4f);

    /// <summary>
    /// 如果两个值不同，就把第一个值设置成第二个
    /// </summary>
    public static bool SetIfDifferent<T>(ref T a, T b)
    {
        if (!EqualityComparer<T>.Default.Equals(a, b))
        {
            a = b;
            return true;
        }

        return false;
    }

    public override void Update(GameTime gameTime)
    {
        bool recalculate = false;

        float windowLeft = ChestMenuExists ? 73f : Main.GameMode is GameModeID.Creative ? 70f : 20f;
        float windowTop = ChestMenuExists ? OpenedChestTrashTop : ClosedChestTrashTop;
        Vector2 trashCellSpacing = ChestMenuExists ? OpenedChestTrashSpacing : ClosedChestTrashSpacing;

        /* Window Left */
        bool a = SetIfDifferent(ref Window.Left.Pixels, windowLeft);
        /* Window Top */
        bool b = SetIfDifferent(ref Window.Top.Pixels, windowTop);
        /* ItemSlot 间距 */
        bool c = SetIfDifferent(ref TrashGrid.CellSpacing, trashCellSpacing);

        if (a || b || c)
        {
            recalculate = true;
        }

        // ItemSlot 大小
        Vector2 trashSize = ChestMenuExists ? OpenedChestTrashSize : ClosedChestTrashSize;
        if (SetIfDifferent(ref TrashGrid.CellSize, trashSize))
        {
            recalculate = true;

            SettingsButton.SetSize(trashSize);

            // 开关 chest 时修改垃圾大小
            if (ChestMenuExists)
            {
                foreach (var item in TrashGrid.Children)
                {
                    if (item is InventoryTrashSlot itemSlot)
                    {
                        itemSlot.SetSizePixels(trashSize);
                        itemSlot.Border = 2f * 0.75f;
                        itemSlot.ItemIconScale = 0.75f;
                    }
                }

                SettingsButton.ImageScale = 0.75f;
            }
            else
            {
                foreach (var item in TrashGrid.Children)
                {
                    if (item is InventoryTrashSlot itemSlot)
                    {
                        itemSlot.SetSizePixels(trashSize);
                        itemSlot.Border = 2f * 0.85f;
                        itemSlot.ItemIconScale = 0.85f;
                    }
                }

                SettingsButton.ImageScale = 0.85f;
            }

            TrashGrid.CalculateAndSetSize();
            TrashGrid.CalculateAndSetChildrenPosition();
        }

        // 设置按钮
        if (SetIfDifferent(ref SettingsButton.Left.Pixels, TrashGrid.Right()))
        {
            recalculate = true;
        }

        if (recalculate)
        {
            Window.Recalculate();
        }

        base.Update(gameTime);
    }

    public override void DrawSelf(SpriteBatch spriteBatch)
    {
        base.DrawSelf(spriteBatch);

        if (SettingsButton.IsMouseHovering)
        {
            MyUtils.TryGetKeybindString(KeybindSystem.AutoTrashKeybind, out var keybind);
            UICommon.TooltipMouseText(MyUtils.GetText("UI.AutoTrash.Introduction", keybind));
        }
    }
}
