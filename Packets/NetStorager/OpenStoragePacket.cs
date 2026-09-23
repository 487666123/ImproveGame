using ImproveGame.Content.Tiles;
using ImproveGame.Packets.NetChest;

namespace ImproveGame.Packets.NetStorager;

/// <summary>
/// 开启储存时发送的数据包
/// </summary>
[AutoSync]
public class OpenStoragePacket : NetModule
{
    private ushort _tileEntityID;

    public static OpenStoragePacket Get(int tileEntityID)
    {
        var packet = NetModuleLoader.Get<OpenStoragePacket>();
        packet._tileEntityID = (ushort)tileEntityID;
        return packet;
    }

    public override void Receive()
    {
        if (!MyUtils.TryGetTileEntityAs<TEExtremeStorage>(_tileEntityID, out var tileEntity) ||
            TileLoader.GetTile(Main.tile[tileEntity.Position.ToPoint()].TileType) is not ExtremeStorage storageTile)
        {
            return;
        }

        switch (Main.netMode)
        {
            case NetmodeID.Server:
                tileEntity.FindAllNearbyChests().ForEach(i => ChestItemOperation.SendAllItems(i, Sender));
                Send(Sender);
                break;
            case NetmodeID.MultiplayerClient:
                storageTile.ServerOpenRequest = true;
                storageTile.RightClick(tileEntity.Position.X, tileEntity.Position.Y);
                // ExtremeStorageGUI.Storage = tileEntity;
                // SidedEventTrigger.ToggleViewBody(UISystem.Instance.ExtremeStorageGUI);
                break;
        }
    }
}