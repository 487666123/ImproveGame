using ImproveGame.Content.Tiles;

namespace ImproveGame.Packets.NetStorager
{
    [AutoSync]
    public class SyncDataPacket : NetModule
    {
        private ushort _tileEntityID;

        public static SyncDataPacket Get(int tileEntityID)
        {
            var packet = NetModuleLoader.Get<SyncDataPacket>();
            packet._tileEntityID = (ushort)tileEntityID;
            return packet;
        }

        public override void Send(ModPacket p)
        {
            if (!MyUtils.TryGetTileEntityAs<TEExtremeStorage>(_tileEntityID, out var tileEntity))
            {
                return;
            }

            tileEntity.NetSend(p);
        }

        public override void Read(BinaryReader r)
        {
            if (!MyUtils.TryGetTileEntityAs<TEExtremeStorage>(_tileEntityID, out var tileEntity))
            {
                return;
            }

            tileEntity.NetReceive(r);
        }

        public override void Receive()
        {
            if (Main.netMode is NetmodeID.Server)
            {
                Send(); // 转发给所有客户端
            }
        }
    }
}