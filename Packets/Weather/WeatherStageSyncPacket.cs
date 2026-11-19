using ImproveGame.Content.Functions.WeatherControl;

namespace ImproveGame.Packets.Weather;

/// <summary>
/// 通用气候控制档位同步包
/// 任意注册项变更档位都走这一路，包内只带 id 与目标 stage
/// </summary>
[AutoSync]
public class WeatherStageSyncPacket : NetModule
{
    private string _id;
    private int _stage;

    public static void Send(string id, int stage)
    {
        if (string.IsNullOrEmpty(id)) return;
        var module = NetModuleLoader.Get<WeatherStageSyncPacket>();
        module._id = id;
        module._stage = stage;
        module.Send(runLocally: true);
    }

    public override void Receive()
    {
        if (WeatherControlRegistry.Instance.TryGet(_id, out var control))
            control.SetStage(_stage);

        if (Main.netMode is NetmodeID.Server)
            Send(-1, Sender);
    }
}
