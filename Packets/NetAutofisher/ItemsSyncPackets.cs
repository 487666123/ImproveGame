using ImproveGame.Content.Tiles;
using ImproveGame.UI.Autofisher;
using ImproveGame.UIFramework;
using Terraria.ModLoader.IO;

namespace ImproveGame.Packets.NetAutofisher;

[AutoSync]
public class ItemsSyncAllPacket : NetModule
{
    private int tileEntityID;
    private bool isOpenFisher;
    private Item fishingPole;
    private Item bait;
    private Item accessory;
    [ItemSync(syncFavorite: true)] private Item[] fish;

    public static ItemsSyncAllPacket Get(int tileEntityID, bool isOpenFisher, Item fishingPole, Item bait,
        Item accessory, Item[] fishes)
    {
        var module = NetModuleLoader.Get<ItemsSyncAllPacket>();
        module.tileEntityID = tileEntityID;
        module.isOpenFisher = isOpenFisher;
        module.fishingPole = fishingPole;
        module.bait = bait;
        module.accessory = accessory;
        module.fish = fishes;
        return module;
    }

    public override void Receive()
    {
        if (!MyUtils.TryGetTileEntityAs<TEAutofisher>(tileEntityID, out var autofisher) ||
            TileLoader.GetTile(Main.tile[autofisher.Position.ToPoint()].TileType) is not Autofisher fisherTile)
        {
            return;
        }

        autofisher.fishingPole = fishingPole;
        autofisher.bait = bait;
        autofisher.accessory = accessory;
        autofisher.fish = fish;

        if (Main.netMode is NetmodeID.MultiplayerClient)
        {
            AutofisherGUI.RequireRefresh = true;
            if (isOpenFisher)
            {
                fisherTile.ServerOpenRequest = true;
                fisherTile.RightClick(autofisher.Position.X, autofisher.Position.Y);
                return;
            }

            if (AutofisherGUI.Visible)
                UISystem.Instance.AutofisherGUI.RefreshItems();
        }
        else
        {
            Send(-1, Sender, false);
        }
    }
}

[AutoSync]
public class ItemsStackChangePacket : NetModule
{
    private int tileEntityID;
    private byte type;
    private int stackChange;

    public static ItemsStackChangePacket Get(int tileEntityID, byte type, int stackChange)
    {
        var module = NetModuleLoader.Get<ItemsStackChangePacket>();
        module.tileEntityID = tileEntityID;
        module.type = type;
        module.stackChange = stackChange;
        return module;
    }

    public override void Receive()
    {
        if (!MyUtils.TryGetTileEntityAs<TEAutofisher>(tileEntityID, out var autofisher))
        {
            return;
        }

        // 发送鱼
        if (type <= ItemSyncPacket.Fishes)
        {
            autofisher.fish[type].stack += stackChange;
        }

        // 发送鱼饵
        if (type == ItemSyncPacket.Bait)
        {
            autofisher.bait.stack += stackChange;
        }

        // 发送到其他的所有客户端
        if (Main.netMode is NetmodeID.Server)
        {
            ItemSyncPacket.Get(tileEntityID, type).Send(ignoreClient: Sender, runLocally: false);
        }
        // 客户端刷新框
        else
        {
            AutofisherGUI.RequireRefresh = true;
            if (AutofisherGUI.Visible)
                UISystem.Instance.AutofisherGUI.RefreshItems();
        }
    }
}

/// <summary>
/// 客户端调用: 向服务器发送获取 <seealso cref="TEAutofisher"/> 物品的请求
/// </summary>
[AutoSync]
public class RequestItemPacket : NetModule
{
    private int tileEntityID;
    private byte type;

    public static RequestItemPacket Get(int tileEntityID, byte type)
    {
        var module = NetModuleLoader.Get<RequestItemPacket>();
        module.tileEntityID = tileEntityID;
        module.type = type;
        return module;
    }

    /// <summary>
    /// 理论上只有服务器调用
    /// </summary>
    public override void Receive()
    {
        ItemSyncPacket.Get(tileEntityID, type).Send(Sender, -1, false);
    }
}

/// <summary>
/// 发送物品，在服务器上Receive的话会转发到其他客户端
/// </summary>
public class ItemSyncPacket : NetModule
{
    public const int Fishes = 39;
    public const int FishingPole = 40;
    public const int Bait = 41;
    public const int Accessory = 42;
    public const int All = 43;

    [AutoSync] private int tileEntityID;
    [AutoSync] private byte type;
    private Item fishingPole = null;
    private Item bait = null;
    private Item accessory = null;
    private Item[] fish = new Item[40];

    public static ItemSyncPacket Get(int tileEntityID, byte type)
    {
        var module = NetModuleLoader.Get<ItemSyncPacket>();
        module.tileEntityID = tileEntityID;
        module.type = type;
        MyUtils.TryGetTileEntityAs<TEAutofisher>(tileEntityID, out var autofisher);
        module.fishingPole = autofisher.fishingPole;
        module.bait = autofisher.bait;
        module.accessory = autofisher.accessory;
        module.fish = autofisher.fish;
        return module;
    }

    public override void Send(ModPacket p)
    {
        // 发送鱼
        if (type <= Fishes)
        {
            if (fish[type] is null)
                ItemIO.Send(new(), p, true, true);
            else
                ItemIO.Send(fish[type], p, true, true);
        }

        // 发送鱼竿
        if (type is FishingPole)
        {
            ItemIO.Send(fishingPole, p, true);
        }

        // 发送鱼饵
        if (type is Bait)
        {
            ItemIO.Send(bait, p, true);
        }

        // 发送饰品
        if (type is Accessory)
        {
            ItemIO.Send(accessory, p, true);
        }

        // 全部发送
        if (type is All)
        {
            ItemIO.Send(fishingPole, p, true);
            ItemIO.Send(bait, p, true);
            ItemIO.Send(accessory, p, true);
            p.Write(fish, writeFavorite: true);
        }
    }

    public override void Read(BinaryReader r)
    {
        fishingPole = null;
        bait = null;
        accessory = null;
        fish = new Item[40];

        // 发送鱼
        if (type <= Fishes)
        {
            fish[type] = ItemIO.Receive(r, true, true);
        }

        // 发送鱼竿
        if (type is FishingPole)
        {
            fishingPole = ItemIO.Receive(r, true);
        }

        // 发送鱼饵
        if (type is Bait)
        {
            bait = ItemIO.Receive(r, true);
        }

        // 发送饰品
        if (type is Accessory)
        {
            accessory = ItemIO.Receive(r, true);
        }

        // 全部发送
        if (type is All)
        {
            fishingPole = ItemIO.Receive(r, true);
            bait = ItemIO.Receive(r, true);
            accessory = ItemIO.Receive(r, true);
            fish = r.ReadItemArray(readFavorite: true);
        }
    }

    public override void Receive()
    {
        if (MyUtils.TryGetTileEntityAs<TEAutofisher>(tileEntityID, out var autofisher))
        {
            if (type <= Fishes)
                autofisher.fish[type] = fish[type];
            if (type is FishingPole or All)
                autofisher.fishingPole = fishingPole;
            if (type is Bait or All)
                autofisher.bait = bait;
            if (type is Accessory or All)
                autofisher.accessory = accessory;
            if (type is All)
                autofisher.fish = fish;
            if (Main.netMode is NetmodeID.MultiplayerClient)
            {
                AutofisherGUI.RequireRefresh = true;
                if (AutofisherGUI.Visible)
                    UISystem.Instance.AutofisherGUI.RefreshItems();
            }
            else if (Main.netMode is NetmodeID.Server)
            {
                Send(ignoreClient: Sender, runLocally: false);
            }
        }
    }
}