using ImproveGame.Interface.GUI.ItemSearcher;

namespace ImproveGame.Common.Packets.NetChest;

public class SearchItemsNearbyPacket : NetModule
{
    private string _searchString;
    private HashSet<short> _matchedChests;
    private HashSet<int> _itemTypes;

    public static void Request(string searchString)
    {
        if (Main.netMode is NetmodeID.Server) return;

        var module = NetModuleLoader.Get<SearchItemsNearbyPacket>();
        module._searchString = searchString;
        module.Send(runLocally: true);
    }

    public override void Send(ModPacket p)
    {
        switch (Main.netMode)
        {
            case NetmodeID.MultiplayerClient:
                p.Write(_searchString);
                break;
            case NetmodeID.Server:
                p.Write(_matchedChests);
                p.Write(_itemTypes);
                break;
        }
    }

    public override void Read(BinaryReader r)
    {
        switch (Main.netMode)
        {
            case NetmodeID.MultiplayerClient:
                _matchedChests = r.ReadShortEnumerable().ToHashSet();
                _itemTypes = r.ReadIntEnumerable().ToHashSet();
                break;
            case NetmodeID.Server:
                _searchString = r.ReadString();
                break;
        }
    }

    private bool ChestWithinRange(Chest c, int range)
    {
        var player = Main.netMode is NetmodeID.SinglePlayer ? Main.LocalPlayer : Main.player[Sender];
        var chestCenter = new Vector2((c.x * 16 + 16), (c.y * 16 + 16));
        return (chestCenter - player.Center).Length() < range;
    }

    public override void Receive()
    {
        // 服务器收包，必定是客户端请求
        if (Main.netMode is NetmodeID.Server or NetmodeID.SinglePlayer)
        {
            const int itemSearchRange = 400; // 25格
            _matchedChests = new HashSet<short>();
            _itemTypes = new HashSet<int>();
            for (int chestIndex = 0; chestIndex < Main.maxChests; chestIndex++)
            {
                Chest chest = Main.chest[chestIndex];
                if (chest == null || Chest.IsLocked(chest.x, chest.y) || !ChestWithinRange(chest, itemSearchRange))
                    continue;

                if (DealWithItems(_itemTypes, chest.item))
                    _matchedChests.Add((short)chestIndex);
            }

            if (Main.netMode is not NetmodeID.SinglePlayer)
                Send();
        }

        // 本地收包，必定是服务器那边做好了传来的
        if (Main.netMode is NetmodeID.MultiplayerClient or NetmodeID.SinglePlayer)
        {
            _matchedChests ??= new HashSet<short>();
            _itemTypes ??= new HashSet<int>();

            // 加上本地私货
            var items = Main.LocalPlayer.bank.item
                .Concat(Main.LocalPlayer.bank2.item)
                .Concat(Main.LocalPlayer.bank3.item)
                .Concat(Main.LocalPlayer.bank4.item);
            DealWithItems(_itemTypes, items);
            ItemSearcherGUI.TryUpdateItems(_matchedChests, _itemTypes);
        }
    }

    /// <summary>
    /// 将匹配到的物品存到 typesCollected 内，如果有匹配到任何物品返回 true，否则返回 false
    /// </summary>
    /// <param name="typesCollected"></param>
    /// <param name="items"></param>
    /// <returns>如果有匹配到任何物品返回 true，否则返回 false</returns>
    private bool DealWithItems(ISet<int> typesCollected, IEnumerable<Item> items)
    {
        if (items is null) return false;

        bool matched = false;
        string searchContent = _searchString;
        foreach (Item item in items)
            if (item.MatchWithString(searchContent))
            {
                typesCollected.Add(item.type);
                matched = true;
            }

        return matched;
    }
}