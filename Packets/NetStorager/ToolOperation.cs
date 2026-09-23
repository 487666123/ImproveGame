using ImproveGame.Common.ModPlayers;
using ImproveGame.Content.Tiles;
using ImproveGame.Core;
using ImproveGame.Packets.Items;
using ImproveGame.Packets.NetChest;
using ImproveGame.UI.ExtremeStorage;
using System.Collections;
using System.Diagnostics;
using ItemSlot = Terraria.UI.ItemSlot;

namespace ImproveGame.Packets.NetStorager;

public class ToolOperation : NetModule
{
    public enum OperationType : byte
    {
        SortStorage,
        StackToInventory,
        StackToStorage,
        DepositAll,
        LootAll
    }

    private static List<int> _chests = new();
    private static bool _customRunning;
    [AutoSync] private ushort _tileEntityID;
    [AutoSync] private byte _group;
    [AutoSync] private byte _operationType;

    public static void Send(OperationType operationType)
    {
        Debug.Assert(Main.netMode is not NetmodeID.Server, "Packet cannot be sent by server");

        var packet = NetModuleLoader.Get<ToolOperation>();

        packet._tileEntityID = (ushort)ExtremeStorageGUI.Storage.ID;
        packet._group = (byte)ExtremeStorageGUI.RealGroup;
        packet._operationType = (byte)operationType;
        // runLocally 使其在客户端和单人模式都执行 Receive，可以帮忙设置
        packet.Send(runLocally: true);
    }

    /// <summary>
    /// 返回值为 (箱子索引, 物品在箱子中的索引, (物品ID, 物品数量, 物品前缀))
    /// </summary>
    public List<(int, int, (int, int, int))> GetItemInfosWithChestIndex(List<(int, Item[])> itemsWithChestIndex)
    {
        var items = new List<(int, int, (int, int, int))>();
        foreach ((int chestIndex, _) in itemsWithChestIndex)
        {
            var chest = Main.chest[chestIndex];
            for (int i = 0; i < chest.maxItems; i++)
            {
                var item = chest.item[i];
                items.Add((chestIndex, i, (item.type, item.stack, item.prefix)));
            }
        }

        return items;
    }

    public override void Receive()
    {
        if (!MyUtils.TryGetTileEntityAs<TEExtremeStorage>(_tileEntityID, out var tileEntity)) return;

        // 获取所有箱子
        var group = (ItemGroup)_group;
        _chests = tileEntity.FindAllNearbyChestsWithGroup(group).Where(i => !Chest.IsPlayerInChest(i)).ToList();

        if (_chests.Count is 0) return;

        // 获取所有箱子中的物品
        var itemsWithChestIndex = new List<(int, Item[])>();
        _chests.ForEach(i => itemsWithChestIndex.Add((i, Main.chest[i].item)));
        var items = itemsWithChestIndex.SelectMany(i => i.Item2).ToArray();

        // 保存操作前的物品信息
        var oldItems = GetItemInfosWithChestIndex(itemsWithChestIndex);

        // 获取TE坐标，用于快速堆叠时的可见物品传输
        var storagePosition = tileEntity.Position.ToWorldCoordinates() + new Vector2(16f);

        switch ((OperationType)_operationType)
        {
            // 排列物品
            case OperationType.SortStorage:
                _customRunning = true;
                ItemSorting.Sort(true, items);
                _customRunning = false;
                break;
            // 向物品栏堆叠 - 补货
            case OperationType.StackToInventory:
                StackToInventory(items, out _);
                break;
            // 向储存堆叠 - 快速堆叠
            case OperationType.StackToStorage:
                StackToStorage(items, storagePosition, out _);
                break;
            // 向储存存入所有物品
            case OperationType.DepositAll:
                DepositAll(items, storagePosition);
                break;
            // 从储存中取出所有物品
            case OperationType.LootAll:
                LootAll(items);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        // 将结果应用到箱内物品
        for (int i = 0; i < itemsWithChestIndex.Count; i++)
        {
            (int chestIndex, _) = itemsWithChestIndex[i];
            var chest = Main.chest[chestIndex];
            for (int j = 0; j < chest.maxItems; j++)
            {
                chest.item[j] = items[i * chest.maxItems + j];
            }
        }

        // 保存操作后的物品信息
        var newItems = GetItemInfosWithChestIndex(itemsWithChestIndex);

        // 联机同步
        if (Main.netMode is not NetmodeID.Server)
            return;

        // 发送物品变化的箱子槽位
        for (int k = 0; k < oldItems.Count && k < newItems.Count; k++)
        {
            var oldItem = oldItems[k];
            var newItem = newItems[k];

            // 如果物品没有变化，那么就不发送
            if (oldItem.Item3 == newItem.Item3)
            {
                continue;
            }

            // 发送数据
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                if (Main.player[i].active && !Main.player[i].dead &&
                    Main.player[i].TryGetModPlayer<ExtremeStoragePlayer>(out var modPlayer) &&
                    modPlayer.UsingStorage == _tileEntityID)
                {
                    ChestItemOperation.SendItem(oldItem.Item1, oldItem.Item2, i);
                }
            }
        }
    }

    private void StackToInventory(Item[] items, out bool playedSound)
    {
        playedSound = false;

        // 这个是服务器执行，客户端不执行
        if (Main.netMode is NetmodeID.MultiplayerClient) return;

        bool shouldPlaySound = false;
        Item[] inventory = Main.player[Sender].inventory;
        // 速查表，用于快速判断某个物品ID是否在物品栏中拥有，且可能可以堆叠
        var inventoryItemIds = new HashSet<int>();
        // 通过物品ID对应空位
        var itemIdToInvSlot = new Dictionary<int, List<int>>();
        for (int i = 57; i >= 0; i--)
        {
            var item = inventory[i];
            bool isInCoinSlotOrCoin = i is >= 50 and <= 53 || item.IsACoin; // 堆叠不包含钱币
            if (item.IsAir || item.maxStack <= 1 || item.stack >= item.maxStack || isInCoinSlotOrCoin)
            {
                continue;
            }

            inventoryItemIds.Add(item.type);
            if (itemIdToInvSlot.ContainsKey(item.type))
                itemIdToInvSlot[item.type].Add(i);
            else
                itemIdToInvSlot.Add(item.type, new List<int> { i });
        }

        foreach (var item in items)
        {
            if (!inventoryItemIds.Contains(item.type)) continue;
            itemIdToInvSlot[item.type].ForEach(i =>
            {
                if (item.stack <= 0) return;

                var invItem = inventory[i];
                if (invItem.stack >= invItem.maxStack) return;

                int context = i switch
                {
                    >= 50 and <= 53 => ItemSlot.Context.InventoryCoin,
                    >= 54 and <= 57 => ItemSlot.Context.InventoryAmmo,
                    _ => ItemSlot.Context.InventoryItem
                };

                if (ItemSlot.PickItemMovementAction(inventory, context, i, item) == -1) return;
                if (!ItemLoader.CanStack(invItem, item)) return;

                int stack = Math.Min(invItem.maxStack - invItem.stack, item.stack);
                invItem.stack += stack;
                item.stack -= stack;
                if (item.stack <= 0)
                    item.TurnToAir();
                shouldPlaySound = true;

                if (Main.netMode is NetmodeID.Server)
                {
                    InventoryItemDataPacket.Get((byte)Sender, i, false).Send(Sender);
                    NetMessage.SendData(MessageID.SyncEquipment, -1, Sender, null, Sender, i, invItem.prefix);
                }
            });
        }

        playedSound = shouldPlaySound;

        if (shouldPlaySound)
            PlaySoundPacket.Get(LegacySoundIDs.Grab).Send(toClient: Sender, runLocally: true);
    }

    private void StackToStorage(Item[] items, Vector2 storagePosition, out bool playedSound)
    {
        playedSound = false;

        // 这个是服务器执行，客户端不执行
        if (Main.netMode is NetmodeID.MultiplayerClient) return;

        var player = Main.player[Sender];
        Item[] inventory = player.inventory;
        // 速查表，用于快速判断某个物品ID是否在物品栏中拥有，且可能可以堆叠
        var storageItemIds = new HashSet<int>();
        // 通过物品ID对应空位
        var itemIdToStorageSlot = new Dictionary<int, List<int>>();
        for (int i = 0; i < items.Length; i++)
        {
            var item = items[i];
            if (item.IsAir || item.maxStack <= 1 || item.stack >= item.maxStack)
            {
                continue;
            }

            storageItemIds.Add(item.type);
            if (itemIdToStorageSlot.ContainsKey(item.type))
                itemIdToStorageSlot[item.type].Add(i);
            else
                itemIdToStorageSlot.Add(item.type, new List<int> { i });
        }

        for (int k = 10; k < 50; k++)
        {
            var item = inventory[k];
            if (item.favorited) continue;
            if (!storageItemIds.Contains(item.type)) continue;

            bool changed = false;
            itemIdToStorageSlot[item.type].ForEach(i =>
            {
                if (item.stack <= 0) return;

                var storageItem = items[i];
                if (storageItem.stack >= storageItem.maxStack) return;

                if (!ItemLoader.TryStackItems(storageItem, item, out int numTransferred)) return;

                if (numTransferred > 0)
                    Chest.VisualizeChestTransfer(player.Center, storagePosition, item.type, Chest.ItemTransferVisualizationSettings.PlayerToChest);

                if (item.stack <= 0)
                    item.TurnToAir();

                changed = true;
                // shouldPlaySound = true;
            });
            if (changed && Main.netMode is NetmodeID.Server)
            {
                InventoryItemDataPacket.Get((byte)Sender, k, false).Send(Sender);
                NetMessage.SendData(MessageID.SyncEquipment, -1, Sender, null, Sender, k, item.prefix);
            }
        }

        // playedSound = shouldPlaySound;

        // 现在使用可见物品传输动画
        // if (shouldPlaySound)
        //     PlaySoundPacket.Get(LegacySoundIDs.Grab).Send(toClient: Sender, runLocally: true);
    }

    /// <summary>
    /// 物品栏 -> 储存
    /// </summary>
    private void DepositAll(Item[] items, Vector2 storagePosition)
    {
        // 这个是服务器执行，客户端不执行
        if (Main.netMode is NetmodeID.MultiplayerClient) return;

        var player = Main.player[Sender];

        StackToStorage(items, storagePosition, out bool playedSound);
        DepositAllInner();
        return;

        void DepositAllInner()
        {
            // bool shouldPlaySound = false;
            var emptySlots = new List<int>();
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i].IsAir)
                    emptySlots.Add(i);
            }

            for (int i = 49; i >= 10; i--)
            {
                var item = player.inventory[i];
                if (item.IsAir || item.favorited) continue;
                if (emptySlots.Count <= 0) break;

                Chest.VisualizeChestTransfer(player.Center, storagePosition, item.type, Chest.ItemTransferVisualizationSettings.PlayerToChest);

                int slot = emptySlots[0];
                emptySlots.RemoveAt(0);
                items[slot] = item.Clone();
                item.TurnToAir();

                // shouldPlaySound = true;

                if (Main.netMode is NetmodeID.Server)
                {
                    InventoryItemDataPacket.Get((byte)Sender, i, false, true).Send(Sender);
                    NetMessage.SendData(MessageID.SyncEquipment, -1, Sender, null, Sender, i, item.prefix);
                }
            }

            // 现在使用可见物品传输动画
            // if (shouldPlaySound && !playedSound)
            //     PlaySoundPacket.Get(LegacySoundIDs.Grab).Send(toClient: Sender, runLocally: true);
        }
    }

    /// <summary>
    /// 储存 -> 物品栏
    /// </summary>
    private void LootAll(Item[] items)
    {
        // 这个是服务器执行，客户端不执行
        if (Main.netMode is NetmodeID.MultiplayerClient) return;

        var player = Main.player[Sender];

        StackToInventory(items, out bool playedSound);
        LootAllInner();

        void LootAllInner()
        {
            bool shouldPlaySound = false;
            var emptySlots = new List<int>();

            // 先从快捷栏开始
            for (int i = 0; i < 10; i++)
                if (player.inventory[i].IsAir)
                    emptySlots.Add(i);

            // 再从其他物品栏倒序
            for (int i = 49; i >= 10; i--)
                if (player.inventory[i].IsAir)
                    emptySlots.Add(i);

            foreach (var item in items)
            {
                if (item.IsAir) continue;
                if (emptySlots.Count <= 0) break;

                int slot = emptySlots[0];
                emptySlots.RemoveAt(0);
                player.inventory[slot] = item.Clone();
                item.TurnToAir();

                shouldPlaySound = true;

                if (Main.netMode is NetmodeID.Server)
                {
                    InventoryItemDataPacket.Get((byte)Sender, slot, false).Send(Sender);
                    NetMessage.SendData(MessageID.SyncEquipment, -1, Sender, null, Sender, slot,
                        player.inventory[slot].prefix);
                }
            }

            if (shouldPlaySound && !playedSound)
                PlaySoundPacket.Get(LegacySoundIDs.Grab).Send(toClient: Sender, runLocally: true);
        }
    }

    public class ToolOperationHook : ILoadable
    {
        void ILoadable.Load(Mod mod)
        {
            // 让可见物品传输播放声音
            On_Chest.AskForChestToEatItem += (orig, worldPosition, duration) =>
            {
                orig.Invoke(worldPosition, duration);

                var tilePosition = (worldPosition - new Vector2(16f)).ToTileCoordinates();
                if (!MyUtils.TryGetTileEntityAs<TEExtremeStorage>(tilePosition, out _)) return;

                CoroutineSystem.MiscRunner.Run(duration - 10, DoRandomGrabSoundInner());
                return;

                IEnumerator DoRandomGrabSoundInner()
                {
                    SoundEngine.PlaySound(SoundID.Grab, worldPosition);
                    yield break;
                }
            };

            // 排序设置物品栏颜色
            On_ItemSlot.SetGlow += (orig, index, hue, isChest) =>
            {
                if (!_customRunning)
                {
                    orig.Invoke(index, hue, isChest);
                    return;
                }

                // 一般来说不可能出现负数，但是如果出现了，那么就直接返回
                if (index < 0 || Main.netMode is NetmodeID.Server) return;

                int indexInChestsList = index / 40;
                int indexInChestSlots = index % 40;

                if (!_chests.IndexInRange(indexInChestsList) || !Main.chest.IndexInRange(_chests[indexInChestsList]))
                    return;
                int chestIndex = _chests[indexInChestsList];
                var chest = Main.chest[chestIndex];

                if (!ExtremeStorageGUI.ChestSlotsGlowHue.ContainsKey(chestIndex))
                {
                    var hues = new float[40];
                    Array.Fill(hues, -1f);
                    ExtremeStorageGUI.ChestSlotsGlowHue.Add(chestIndex, hues);
                }

                var hueArray = ExtremeStorageGUI.ChestSlotsGlowHue[chestIndex];

                if (chest is null || !hueArray.IndexInRange(indexInChestSlots))
                    return;

                if (hue < 0f)
                {
                    ExtremeStorageGUI.ChestSlotsGlowTimer = 0;
                    hueArray[indexInChestSlots] = 0f;
                }
                else
                {
                    ExtremeStorageGUI.ChestSlotsGlowTimer = 300;
                    hueArray[indexInChestSlots] = hue;
                }
            };
        }
        void ILoadable.Unload()
        {

        }
    }
}

