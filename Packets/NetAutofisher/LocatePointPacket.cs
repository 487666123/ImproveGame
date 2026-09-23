using ImproveGame.Content.Tiles;
using Terraria.DataStructures;

namespace ImproveGame.Packets.NetAutofisher
{
    [AutoSync]
    public class LocatePointPacket : NetModule
    {
        private int tileEntityID;
        private Point16 locatePoint;

        public static LocatePointPacket Get(int tileEntityID, Point16 locatePoint)
        {
            var module = NetModuleLoader.Get<LocatePointPacket>();
            module.tileEntityID = tileEntityID;
            module.locatePoint = locatePoint;
            return module;
        }

        public override void Receive()
        {
            if (MyUtils.TryGetTileEntityAs<TEAutofisher>(tileEntityID, out var autofisher))
            {
                autofisher.locatePoint = new(locatePoint.X, locatePoint.Y);
            }
        }
    }
}