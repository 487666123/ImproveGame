using ImproveGame.Common.ModPlayers;
using ImproveGame.Packets;
using SilkyUIFramework.Elements;

namespace ImproveGame.UserInterfaces.BigBag;

public partial class BigBagUI
{
    private const int HotbarSlotCount = 10;
    private const int MainInventorySlotCount = 50;
    private const int InventorySlotCountWithCoinsAndAmmo = 58;

    private void InitializeInventoryActions()
    {
        BindInventoryAction(QuickButton, WithdrawAll);
        BindInventoryAction(PutButton, DepositAll);
        BindInventoryAction(ReplenishButton, QuickStack);
        BindInventoryAction(SortButton, Sort);
    }

    private void BindInventoryAction(UIElementGroup button, Action action)
    {
        button.LeftMouseDown += (_, _) =>
        {
            if (Main.LocalPlayer.ItemAnimationActive) return;

            SoundEngine.PlaySound(SoundID.Grab);
            action();
            SyncInventory();
        };
    }

    private static bool CanBulkTransfer(Item item) => !item.IsAir && !item.favorited && !item.IsACoin;

    private void WithdrawAll()
    {
        var bigBag = ItemGrid.Items;
        for (int i = 0; i < bigBag.Length; i++)
        {
            if (!CanBulkTransfer(bigBag[i])) continue;
            bigBag[i] = ItemStackToInventory(Main.LocalPlayer.inventory, bigBag[i],
                hint: false, end: MainInventorySlotCount);
        }
    }

    private void DepositAll()
    {
        var inventory = Main.LocalPlayer.inventory;
        for (int i = HotbarSlotCount; i < MainInventorySlotCount; i++)
        {
            if (!CanBulkTransfer(inventory[i])) continue;
            inventory[i] = ItemStackToInventory(ItemGrid.Items, inventory[i], hint: false);
        }
    }

    private void QuickStack()
    {
        var inventory = Main.LocalPlayer.inventory;
        var bigBag = ItemGrid.Items;
        for (int i = HotbarSlotCount; i < InventorySlotCountWithCoinsAndAmmo; i++)
        {
            if (!CanBulkTransfer(inventory[i])) continue;
            if (HasItem(bigBag, -1, inventory[i].type))
                inventory[i] = ItemStackToInventory(bigBag, inventory[i], hint: false);
        }
    }

    private void Sort()
    {
        var items = ItemGrid.Items;
        List<Item> sortableItems = [];
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i].IsAir || items[i].favorited) continue;
            sortableItems.Add(items[i]);
            items[i] = new Item();
        }

        // Keep favorites in place; sort by rarity descending, type ascending, then stack descending.
        sortableItems.Sort((left, right) =>
            -left.rare.CompareTo(right.rare) * 100
            + left.type.CompareTo(right.type) * 10
            - left.stack.CompareTo(right.stack));
        foreach (var item in sortableItems)
            ItemStackToInventory(items, item, hint: false);
    }

    private void SyncInventory()
    {
        DataPlayer.RefreshRecipes = true;
        if (Main.netMode != NetmodeID.MultiplayerClient) return;

        // Sorting can replace an item without changing its stack count.
        for (int i = 0; i < ItemGrid.Items.Length; i++)
            BigBagSlotPacket.Get(ItemGrid.Items[i], Main.myPlayer, i).Send(runLocally: false);
    }
}
