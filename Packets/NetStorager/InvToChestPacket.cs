using ImproveGame.Content.Tiles;
using ImproveGame.Packets.Items;
using ImproveGame.Packets.NetChest;
using ImproveGame.UI.ExtremeStorage;

namespace ImproveGame.Packets.NetStorager;

[AutoSync]
public class InvToChestPacket : NetModule
{
    private ushort _tileEntityID;
    private byte _inventoryIndex;
    private byte _group;

    public static void Send(int tileEntityID, int inventoryIndex, ItemGroup group)
    {
        var packet = NetModuleLoader.Get<InvToChestPacket>();
        packet._tileEntityID = (ushort)tileEntityID;
        packet._inventoryIndex = (byte)inventoryIndex;
        packet._group = (byte)group;
        packet.Send();
    }

    public override void Receive()
    {
        // 客户端不应该收到这个包
        if (Main.netMode is NetmodeID.MultiplayerClient ||
            !MyUtils.TryGetTileEntityAs<TEExtremeStorage>(_tileEntityID, out var tileEntity))
        {
            return;
        }

        var group = (ItemGroup)_group;

        // 查找名字相应的箱子
        var chestIndexes = tileEntity.FindAllNearbyChestsWithGroup(group);

        // 建立[箱子 -> 物品]对照表
        var items = new Dictionary<int, Item[]>();
        chestIndexes.ForEach(i =>
        {
            // 箱子正在被其他玩家使用，原则上不允许来自Sender玩家的操作
            if (Chest.UsingChest(i) is not -1 && Main.player[Sender].chest != i)
                return;
            items.Add(i, Main.chest[i].item);
        });

        Operate(items);

        // Recipe.FindRecipes();
    }

    /// <summary>
    /// 轮询箱子并将物品放入
    /// </summary>
    private void Operate(Dictionary<int, Item[]> itemsMap)
    {
        ref var invItem = ref Main.player[Sender].inventory[_inventoryIndex];

        // 先填充和物品相同的
        foreach ((int chestIndex, Item[] chestItems) in itemsMap)
        {
            for (int i = 0; i < chestItems.Length; i++)
            {
                int oldStack = invItem.stack;
                invItem = MyUtils.ItemStackToInventoryItem(chestItems, i, invItem, false);

                // 堆叠发生了改变，发送箱子更新包
                if (oldStack != invItem.stack)
                {
                    ChestItemOperation.SendItem(chestIndex, i);
                }
            }
        }

        // 后填充空位
        foreach ((int chestIndex, Item[] chestItems) in itemsMap)
        {
            for (int i = 0; i < chestItems.Length; i++)
            {
                if (chestItems[i] is null || chestItems[i].IsAir)
                {
                    chestItems[i] = invItem;
                    invItem = new Item();
                    ChestItemOperation.SendItem(chestIndex, i);
                }
            }
        }

        // 发包同步玩家背包内物品
        InventoryItemDataPacket.Get((byte)Sender, _inventoryIndex, false).Send();
    }
}