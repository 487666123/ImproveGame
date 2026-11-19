using ImproveGame.Content.Functions.WeatherControl;

namespace ImproveGame.Packets.Weather;

/// <summary>
/// 通用气候控制锁定状态同步包
/// </summary>
[AutoSync]
public class WeatherLockSyncPacket : NetModule
{
    private string _id;
    private bool _locked;

    public static void Send(string id, bool locked)
    {
        if (string.IsNullOrEmpty(id)) return;
        var module = NetModuleLoader.Get<WeatherLockSyncPacket>();
        module._id = id;
        module._locked = locked;
        module.Send(runLocally: true);
    }

    public override void Receive()
    {
        if (WeatherControlRegistry.Instance.TryGet(_id, out var control))
            control.SetLocked(_locked);

        if (Main.netMode is NetmodeID.Server)
            Send(-1, Sender);
    }
}
