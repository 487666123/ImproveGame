using ImproveGame.Content.Tiles;

namespace ImproveGame.Packets.NetAutofisher
{
    /// <summary>
    /// 由服务器执行，向所有玩家同步钓鱼信息提示
    /// </summary>
    public class FishingTipPacket : NetModule
    {
        [AutoSync] private int _tileEntityID;
        [AutoSync] private byte _tipType;
        private ushort _fishingLevel;
        private float _waterQuality;

        public static FishingTipPacket Get(int tileEntityID, Autofisher.TipType tipType, int fishingLevel,
            float waterQuality)
        {
            var module = NetModuleLoader.Get<FishingTipPacket>();
            module._tileEntityID = tileEntityID;
            module._tipType = (byte)tipType;
            module._fishingLevel = (ushort)fishingLevel;
            module._waterQuality = waterQuality;
            return module;
        }

        public override void Receive()
        {
            if (MyUtils.TryGetTileEntityAs<TEAutofisher>(_tileEntityID, out var autofisher))
            {
                autofisher.SetFishingTip((Autofisher.TipType)_tipType, _fishingLevel, _waterQuality);
            }
        }

        public override void Send(ModPacket p)
        {
            var tipType = (Autofisher.TipType)_tipType;

            if (tipType is Autofisher.TipType.FishingPower or Autofisher.TipType.FullFishingPower)
            {
                p.Write(_fishingLevel);
            }

            if (tipType is Autofisher.TipType.FullFishingPower)
            {
                p.Write(_waterQuality);
            }
        }

        public override void Read(BinaryReader r)
        {
            var tipType = (Autofisher.TipType)_tipType;

            if (tipType is Autofisher.TipType.FishingPower or Autofisher.TipType.FullFishingPower)
            {
                _fishingLevel = r.ReadUInt16();
            }

            if (tipType is Autofisher.TipType.FullFishingPower)
            {
                _waterQuality = r.ReadSingle();
            }
        }
    }
}