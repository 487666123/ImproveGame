using ImproveGame.Content;
using Terraria.DataStructures;

namespace ImproveGame.Common.Packets.WorldFeatures;

public class PlanteraPositionsPacket : NetModule
{
    private List<Point16> _positions;
    
    public static void Sync()
    {
        if (Main.netMode is NetmodeID.MultiplayerClient)
            return;

        var module = NetModuleLoader.Get<PlanteraPositionsPacket>();
        module._positions = StructureDatas.PlanteraPositions;
        module.Send();
    }

    public override void Send(ModPacket p)
    {
        p.Write(_positions);
    }

    public override void Read(BinaryReader r)
    {
        _positions = r.ReadPoint16List().ToList();
    }

    public override void Receive()
    {
        StructureDatas.PlanteraPositions = _positions;
    }
}