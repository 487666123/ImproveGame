using ImproveGame.Content.Tiles;
using Terraria.DataStructures;

namespace ImproveGame.Packets.NetAutofisher
{
    [AutoSync]
    public class FishFiltersPacket : NetModule
    {
        private Point16 position;
        private bool toggle;
        private byte filterType;

        public static FishFiltersPacket Get(Point16 position, bool toggle, byte filterType)
        {
            var module = NetModuleLoader.Get<FishFiltersPacket>();
            module.position = position;
            module.filterType = filterType;
            module.toggle = toggle;
            return module;
        }

        public override void Receive()
        {
            if (!MyUtils.TryGetTileEntityAs<TEAutofisher>(position.X, position.Y, out var autofisher))
                return;

            switch (filterType)
            {
                case 0: autofisher.CatchCrates = toggle; break;
                case 1: autofisher.CatchAccessories = toggle; break;
                case 2: autofisher.CatchTools = toggle; break;
                case 3: autofisher.CatchWhiteRarityCatches = toggle; break;
                case 4: autofisher.CatchNormalCatches = toggle; break;
                case 5:
                    autofisher.AutoDeposit = toggle;
                    if (toggle)
                        autofisher.AutoDepositTimer = 3600;
                    break;
            }
        }
    }

    /// <summary>
    /// 全部过滤器包
    /// </summary>
    [AutoSync]
    public class FisherAllFiltersPacket : NetModule
    {
        private int _tileEntityID;
        private byte _filterTypes; // 节省资源

        public static FisherAllFiltersPacket Get(int tileEntityID, bool catchCrates, bool catchAccessories, bool catchTools, bool catchWhiteRarityCatches, bool catchNormalCatches, bool autoDeposit)
        {
            var module = NetModuleLoader.Get<FisherAllFiltersPacket>();
            module._tileEntityID = tileEntityID;
            var bitsByte = new BitsByte(catchCrates, catchAccessories, catchTools, catchWhiteRarityCatches, catchNormalCatches, autoDeposit);
            module._filterTypes = bitsByte;
            return module;
        }

        public override void Receive()
        {
            if (!MyUtils.TryGetTileEntityAs<TEAutofisher>(_tileEntityID, out var autofisher))
            {
                return;
            }

            var bitsByte = (BitsByte)_filterTypes;
            bool catchCrates = bitsByte[0];
            bool catchAccessories = bitsByte[1];
            bool catchTools = bitsByte[2];
            bool catchWhiteRarityCatches = bitsByte[3];
            bool catchNormalCatches = bitsByte[4];
            bool autoDeposit = bitsByte[5];

            autofisher.CatchCrates = catchCrates;
            autofisher.CatchAccessories = catchAccessories;
            autofisher.CatchTools = catchTools;
            autofisher.CatchWhiteRarityCatches = catchWhiteRarityCatches;
            autofisher.CatchNormalCatches = catchNormalCatches;
            if (autofisher.AutoDeposit != autoDeposit)
                autofisher.AutoDepositTimer = 3600;
            autofisher.AutoDeposit = autoDeposit;
        }
    }
}
