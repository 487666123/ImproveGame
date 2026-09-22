using ImproveGame.Common.ModPlayers;
using ImproveGame.Packets;
using SilkyUIFramework.Elements;

namespace ImproveGame.UserInterfaces.BigBag;

public partial class BigBagUI
{
    // 快捷栏位于玩家物品栏的前 10 个槽位。
    private const int HotbarSlotCount = 10;

    // 原版主物品栏包含 50 个槽位，不包括钱币和弹药栏。
    private const int MainInventorySlotCount = 50;

    // 原版玩家物品栏包含主物品栏、钱币栏和弹药栏共 58 个槽位。
    private const int InventorySlotCountWithCoinsAndAmmo = 58;

    // 为界面上的批量操作按钮绑定对应的物品栏操作。
    private void InitializeInventoryActions()
    {
        BindInventoryAction(QuickButton, WithdrawAll);
        BindInventoryAction(PutButton, DepositAll);
        BindInventoryAction(ReplenishButton, QuickStack);
        BindInventoryAction(SortButton, Sort);
    }

    // 统一处理批量操作按钮的点击音效、执行逻辑和物品栏同步。
    private void BindInventoryAction(UIElementGroup button, Action action)
    {
        button.LeftMouseDown += (_, _) =>
        {
            if (Main.LocalPlayer.ItemAnimationActive) return;

            action();
            SyncInventory();
            SoundEngine.PlaySound(SoundID.Grab);
        };
    }

    #region button action

    // 批量转移时跳过空气槽、收藏物品和钱币。
    private static bool CanBulkTransfer(Item item) => !item.IsAir && !item.favorited && !item.IsACoin;

    // 将大背包中的可转移物品全部取回玩家主物品栏。
    private void WithdrawAll()
    {
        var bigBag = ItemGrid.Items;
        for (int i = 0; i < bigBag.Length; i++)
        {
            if (!CanBulkTransfer(bigBag[i])) continue;
            bigBag[i] = MyUtils.ItemStackToInventory(Main.LocalPlayer.inventory, bigBag[i],
                hint: false, end: MainInventorySlotCount);
        }
    }

    // 将玩家主物品栏中的可转移物品全部存入大背包。
    private void DepositAll()
    {
        var inventory = Main.LocalPlayer.inventory;
        for (int i = HotbarSlotCount; i < MainInventorySlotCount; i++)
        {
            if (!CanBulkTransfer(inventory[i])) continue;
            inventory[i] = MyUtils.ItemStackToInventory(ItemGrid.Items, inventory[i], hint: false);
        }
    }

    // 仅将玩家物品栏中能与大背包已有物品堆叠的物品快速存入其中。
    private void QuickStack()
    {
        var inventory = Main.LocalPlayer.inventory;
        var bigBag = ItemGrid.Items;
        for (int i = HotbarSlotCount; i < InventorySlotCountWithCoinsAndAmmo; i++)
        {
            if (!CanBulkTransfer(inventory[i])) continue;
            if (HasItem(bigBag, -1, inventory[i].type))
                inventory[i] = MyUtils.ItemStackToInventory(bigBag, inventory[i], hint: false);
        }
    }

    // 清空可排序物品后重新填充，以保留收藏物品的原槽位。
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

        // 收藏物品保留原位；按稀有度降序、物品类型升序、堆叠数量降序排序。
        sortableItems.Sort((left, right) =>
            -left.rare.CompareTo(right.rare) * 100
            + left.type.CompareTo(right.type) * 10
            - left.stack.CompareTo(right.stack));
        foreach (var item in sortableItems)
            MyUtils.ItemStackToInventory(items, item, hint: false);
    }

    #endregion

    private void SyncInventory()
    {
        DataPlayer.RefreshRecipes = true;
        if (Main.netMode != NetmodeID.MultiplayerClient) return;

        // 排序可能只替换物品实例而不改变槽位中的堆叠数量，因此需要逐槽发送同步数据。
        for (int i = 0; i < ItemGrid.Items.Length; i++)
            BigBagSlotPacket.Get(ItemGrid.Items[i], Main.myPlayer, i).Send(runLocally: false);
    }
}
