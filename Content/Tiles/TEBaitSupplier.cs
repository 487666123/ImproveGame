using ImproveGame.Packets.NetAutofisher;
using Terraria.DataStructures;

namespace ImproveGame.Content.Tiles;

public class TEBaitSupplier : ModTileEntity
{
    private static int _autofisherValidateTimer;

    #region 基本TE内容

    public override bool IsTileValidForEntity(int x, int y)
    {
        Tile tile = Main.tile[x, y];
        return tile.HasTile && tile.TileType == ModContent.TileType<BaitSupplier>();
    }

    public override int Hook_AfterPlacement(int i, int j, int placeType, int style, int direction, int alternate)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient)
        {
            NetMessage.SendTileSquare(Main.myPlayer, i, j - 2, 2, 3);
            NetMessage.SendData(MessageID.TileEntityPlacement, -1, -1, null, i, j - 2, Type);
            return -1;
        }

        int placedEntity = Place(i, j - 2);
        return placedEntity;
    }

    public override void OnNetPlace()
    {
        NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, ID, Position.X, Position.Y);
    }

    public static bool TryGet(out TEBaitSupplier storage, Point16 point)
    {
        storage = Get(point);
        if (storage is not null)
        {
            return true;
        }

        storage = new TEBaitSupplier();
        return false;
    }

    public static TEBaitSupplier Get(Point16 point)
    {
        Tile tile = Main.tile[point.ToPoint()];
        if (!tile.HasTile)
            return null;
        return !MyUtils.TryGetTileEntityAs<TEBaitSupplier>(point.X, point.Y, out var te) ? null : te;
    }

    #endregion

    public override void Update()
    {
        var tile = Main.tile[Position];
        // 没开
        if (tile.TileFrameY < 54)
            return;

        // 每 3 秒检查一次
        _autofisherValidateTimer++;
        if (_autofisherValidateTimer % 180 != 0)
            return;

        var existingBaitlessAutofishers = ByID
            .Where(pair => pair.Value is TEAutofisher { HasBait: false })
            .Select(pair => (TEAutofisher)pair.Value)
            .ToList();

        var chests = FindAllNearbyChests();

        foreach (var autofisher in existingBaitlessAutofishers)
        {
            foreach (var chest in from chestIndex in chests
                                  where !Chest.IsPlayerInChest(chestIndex) || Main.netMode is not NetmodeID.Server
                                  select Main.chest[chestIndex])
            {
                for (int i = 0; i < chest.item.Length; i++)
                {
                    if (chest.item is null)
                        break;
                    ref Item item = ref chest.item[i];
                    if (item.IsAir || item.bait <= 0)
                        continue;

                    Chest.VisualizeChestTransfer(new Point16(chest.x, chest.y).ToWorldCoordinates(),
                        Position.ToWorldCoordinates(16, 24),
                        item.type, Chest.ItemTransferVisualizationSettings.PlayerToChest);
                    Chest.VisualizeChestTransfer(autofisher.Position.ToWorldCoordinates(16, -40),
                        autofisher.Position.ToWorldCoordinates(16, 16),
                        item.type, Chest.ItemTransferVisualizationSettings.PlayerToChest);
                    autofisher.bait = ItemLoader.TransferWithLimit(item, 15);

                    ItemSyncPacket.Get(autofisher.ID, ItemSyncPacket.Bait).Send(runLocally: false);

                    // 直接break到最外层循环
                    goto LargeBreak;
                }
            }

            LargeBreak:;
        }
    }

    // 上下左右 1 格内的所有箱子即视为在范围内（也就是与其相邻的箱子了）
    public bool ChestInRange(int x, int y)
    {
        int distance = 1;
        bool inRangeX = Math.Abs(x - Position.X) <= distance + 1 || Math.Abs(x - (Position.X + 1)) <= distance;
        bool inRangeY = Math.Abs(y - Position.Y) <= distance + 1 || Math.Abs(y - (Position.Y + 2)) <= distance;
        return inRangeX && inRangeY;
    }

    public bool ChestInRange(Chest chest) => ChestInRange(chest.x, chest.y);

    public bool ChestInRange(int chestIndex)
    {
        var chest = Main.chest[chestIndex];
        return chest is not null && ChestInRange(chest);
    }

    public List<int> FindAllNearbyChests()
    {
        var chestIndexes = new List<int>();

        for (int i = 0; i < Main.maxChests; i++)
            if (ChestInRange(i))
                chestIndexes.Add(i);

        return chestIndexes;
    }
}