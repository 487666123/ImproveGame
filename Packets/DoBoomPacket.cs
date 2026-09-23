using ImproveGame.Common.Configs;

namespace ImproveGame.Packets;

[AutoSync]
public class DoBoomPacket : NetModule
{
    private ushort _x;
    private ushort _y;
    private ushort _width;
    private ushort _height;

    public static void Send(Rectangle rect, bool runLocally = false) =>
        Get(rect.X, rect.Y, rect.Width, rect.Height).Send(runLocally: runLocally);

    public static void Send(int x, int y, int width, int height, bool runLocally = false) =>
        Get(x, y, width, height).Send(runLocally: runLocally);

    public static DoBoomPacket Get(int x, int y, int width, int height)
    {
        var packet = NetModuleLoader.Get<DoBoomPacket>();
        packet._x = (ushort)x;
        packet._y = (ushort)y;
        packet._width = (ushort)width;
        packet._height = (ushort)height;
        return packet;
    }

    public override void Receive()
    {
        if (Main.netMode is NetmodeID.Server)
        {
            Send(-1, Sender);
            return;
        }

        if (!UIConfigs.Instance.ExplosionEffect)
            return;

        var rectangle = new Rectangle(_x, _y, _width, _height);
        MyUtils.ForeachTile(rectangle, (x, y) =>
        {
            MyUtils.BongBong(new Vector2(x, y) * 16f, 16, 16);
        });
    }
}