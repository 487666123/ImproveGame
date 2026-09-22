using ImproveGame.Common.ModHooks;
using ImproveGame.Common.ModPlayers;
using ImproveGame.Packets;
using ImproveGame.UIFramework.UIElements;
using SilkyUIFramework.Elements;
using Terraria.GameContent.UI.Chat;
using Terraria.UI.Chat;
using UIMouseEvent = SilkyUIFramework.UIMouseEvent;

namespace ImproveGame.UserInterfaces.BigBag;

public partial class BigBagItemSlot : SUIItemSlot
{
    // Preserve the custom context expected by existing item left-click hooks.
    private const int LeftClickHookContext = 114514;

    // Do not split the resulting item after a right-click item action.
    private bool _rightClickHandled;

    public Item[] Items { get; }
    public int Index { get; }

    // Read and replace the inventory entry directly, even when both values are air.
    public override Item Item
    {
        get => Items[Index];
        set
        {
            var oldItem = Items[Index];
            Items[Index] = value;
            if (!ItemsEffectivelyEqual(oldItem, value)) OnItemChanged(oldItem, value);
        }
    }

    private bool Interactable => ItemInteractive && Main.playerInventory && !PlayerInUseItem;

    public BigBagItemSlot(Item[] items, int index)
    {
        Items = items;
        Index = index;
        InitializeAppearance();
    }

    protected override void HandleItemSlotLeftClick()
    {
        if (!Interactable) return;

        UpdateCursorOverride();
        HandleLeftClick();
        // Containers can change their contents without changing their stack.
        SyncItem();
    }

    private void HandleLeftClick()
    {
        if (Item.ModItem is IItemOverrideLeftClick hook && hook.OverrideLeftClick(Items, LeftClickHookContext, Index))
            return;

        if (!Item.IsAir && TryHandleCursorAction()) return;
        if (ItemSlot.ShiftInUse) return;

        base.HandleItemSlotLeftClick();
    }

    private bool TryHandleCursorAction()
    {
        switch (Main.cursorOverride)
        {
            case CursorOverrideID.Magnifiers:
                if (ChatManager.AddChatText(FontAssets.MouseText.Value, ItemTagHandler.GenerateTag(Item), Vector2.One))
                    SoundEngine.PlaySound(SoundID.MenuTick);
                return true;
            case CursorOverrideID.FavoriteStar:
                Item.favorited = !Item.favorited;
                SoundEngine.PlaySound(SoundID.MenuTick);
                return true;
            case CursorOverrideID.TrashCan:
            case CursorOverrideID.QuickSell:
                ModItemSlot.SellOrTrash(Items, ItemSlot.Context.InventoryItem, Index);
                return true;
            case CursorOverrideID.ChestToInventory:
                Item = Main.LocalPlayer.GetItem(Item, GetItemSettings.QuickTransferFromSlot);
                SoundEngine.PlaySound(SoundID.Grab);
                return true;
            default:
                return false;
        }
    }

    public override void OnRightMouseDown(UIMouseEvent evt)
    {
        if (!Interactable || Item.IsAir) return;

        base.OnRightMouseDown(evt);
        _rightClickHandled = ItemLoader.CanRightClick(Item);
        if (!_rightClickHandled) return;

        Main.mouseRightRelease = true;
        try
        {
            if (Main.ItemDropsDB.GetRulesForItemID(Item.type).Count != 0)
                ItemSlot.TryOpenContainer(Items, ItemSlot.Context.InventoryItem, Index, Main.LocalPlayer);
            else
                ItemLoader.RightClick(Item, Main.LocalPlayer);
        } finally
        {
            Main.mouseRightRelease = false;
        }
        SyncItem();
    }

    protected override void HandleItemSlotRightLongPress()
    {
        if (!Interactable || !RightMousePressed || _rightClickHandled || Item.IsAir
            || ItemID.Sets.BossBag[Item.type] || ItemID.Sets.IsFishingCrate[Item.type] || ItemLoader.CanRightClick(Item))
            return;

        var oldStack = Item.stack;
        var wasMouseEmpty = Main.mouseItem.IsAir;
        base.HandleItemSlotRightLongPress();
        if (Item.stack == oldStack) return;

        if (wasMouseEmpty)
            ItemSlot.AnnounceTransfer(new ItemSlot.ItemTransferInfo(Main.mouseItem,
                ItemSlot.Context.InventoryItem, ItemSlot.Context.MouseItem));

        SyncItem();
    }

    private void UpdateCursorOverride()
    {
        if (Item.IsAir) return;

        if (!Item.favorited && ItemSlot.ShiftInUse)
            Main.cursorOverride = CursorOverrideID.ChestToInventory;

        if (Main.keyState.IsKeyDown(Main.FavoriteKey))
            Main.cursorOverride = Main.drawingPlayerChat
                ? CursorOverrideID.Magnifiers : CursorOverrideID.FavoriteStar;

        if (!Item.favorited && ItemSlot.ControlInUse && ItemSlot.Options.DisableLeftShiftTrashCan
            && !ItemSlot.ShiftForcedOn)
            Main.cursorOverride = Main.npcShop <= 0 ? CursorOverrideID.TrashCan : CursorOverrideID.QuickSell;
    }

    private void SyncItem()
    {
        DataPlayer.RefreshRecipes = true;
        if (Main.netMode == NetmodeID.MultiplayerClient)
            BigBagSlotPacket.Get(Item, Main.myPlayer, Index).Send(runLocally: false);
    }
}
