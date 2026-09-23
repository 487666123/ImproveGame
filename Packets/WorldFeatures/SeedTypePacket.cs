using ImproveGame.Common.ModSystems;
using ImproveGame.UI.WorldFeature;
using Terraria.Chat;

namespace ImproveGame.Packets.WorldFeatures;

[AutoSync]
public class SeedTypePacket : NetModule
{
    private byte _seedType;
    private byte _setMode; // 0: 关 | 1: 开 | 2: 切换

    public static void ToggleSeedFlag(SeedType seedType)
    {
        var module = NetModuleLoader.Get<SeedTypePacket>();
        module._seedType = (byte)seedType;
        module._setMode = 2;
        module.Send(runLocally: Main.netMode is NetmodeID.SinglePlayer);
    }

    public override void Receive()
    {
        if (!NetPasswordSystem.Permitted(Sender))
        {
            ChatHelper.SendChatMessageToClient(NetworkText.FromKey("Mods.ImproveGame.UI.WorldFeature.NotPermitted"),
                Color.Red, Sender);
            return;
        }

        ref bool featureFlag = ref MyUtils.GetSeedFeatureFlag(_seedType);
        featureFlag = _setMode switch
        {
            0 => false,
            1 => true,
            2 => !featureFlag,
            _ => featureFlag
        };

        // 转发
        if (Main.netMode is NetmodeID.Server)
        {
            _setMode = (byte)featureFlag.ToInt();
            Send();
        }
    }
}