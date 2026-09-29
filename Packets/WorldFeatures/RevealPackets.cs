using ImproveGame.Content;
using Terraria.DataStructures;

namespace ImproveGame.Packets.WorldFeatures
{
    [AutoSync]
    public class RevealOnceForAllPacket : NetModule
    {
        public Point16[] _position;
        public Point16[] _positionAnother;
        public StructureDatas.UnlockID _type;
        public override void Receive()
        {
            StructureDatas.StructuresUnlocked[(byte)_type] = true;
            switch (_type)
            {
                case StructureDatas.UnlockID.Shimmer:
                    StructureDatas.ShimmerPosition = _position[0];
                    break;
                case StructureDatas.UnlockID.Temple:
                    StructureDatas.TemplePosition = _position[0];
                    break;
                case StructureDatas.UnlockID.Pyramids:
                    StructureDatas.PyramidPositions.AddRange(_position);
                    break;
                case StructureDatas.UnlockID.FloatingIslands:
                    StructureDatas.SkyHousePositions.AddRange(_position);
                    StructureDatas.SkyLakePositions.AddRange(_positionAnother);
                    break;
                case StructureDatas.UnlockID.Dungeon:
                    StructureDatas.DungeonPosition = _position[0];
                    break;

            }
        }
    }

    [AutoSync]
    public class RevealEnchantedSwordPacket : NetModule
    {
        public Point16 _position;
        public override void Receive() => StructureDatas.EnchantedSwordPositions.Add(_position);
    }

    [AutoSync]
    public class RevealPlanteraPacket : NetModule
    {
        public Point16 _position;
        public override void Receive() => StructureDatas.PlanteraPositions.Add(_position);
    }
    [AutoSync]
    public class RevealHivePacket : NetModule
    {
        public List<Point16> _allPosition;
        public Point16 _position;
        public override void Receive()
        {
            StructureDatas.AllHivePositions = _allPosition;
            StructureDatas.HivePositions.Add(_position);
        }
    }
}
