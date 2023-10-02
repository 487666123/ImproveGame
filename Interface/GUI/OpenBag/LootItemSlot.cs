using ImproveGame.Interface.Common;

namespace ImproveGame.Interface.GUI.OpenBag;

public class LootItemSlot : BaseItemSlot
{
    public readonly List<Item> Loots;
    public readonly int Index;

    public override Item Item
    {
        get
        {
            return Loots.IndexInRange(Index) ? Loots[Index] : AirItem;
        }
    }

    public LootItemSlot(List<Item> items, int index)
    {
        this.Loots = items;
        Index = index;
        SetBaseItemSlotValues(true, true);
        SetSizePixels(43, 43);
        ItemIconMaxWidthAndHeight = 27;
        SetRoundedRectangleValues(UIColor.ItemSlotBg * 0.85f, 2f, UIColor.ItemSlotBorder * 0.85f, new Vector4(10f));
    }

    public override void LeftMouseDown(UIMouseEvent evt)
    {
        if (Main.LocalPlayer.ItemAnimationActive) return;
        
        Main.playerInventory = true;

        if (Item.IsAir)
            return;

        Main.LocalPlayer.QuickSpawnItem(Main.LocalPlayer.GetSource_Loot(), Item, Item.stack);
        Item.TurnToAir();
    }
}
