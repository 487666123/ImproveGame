using ImproveGame.Common.GlobalItems;
using ImproveGame.Content.Tiles;
using Terraria.DataStructures;

namespace ImproveGame.Packets.NetAutofisher;

/// <summary>
/// 服务器发送给任意连接的客户端，询问钓鱼机“假人”所处环境
/// Player.UpdateBiomes本应是客户端执行的代码，不应该放到服务器运行
/// </summary>
[AutoSync]
public class RollItemRequest : NetModule
{
    private int _tileEntityID;
    private byte _fishingStatFlags;
    private int _fishingSkill;
    private float _speedMultiplier;

    private Point16 _locatePoint;
    private int _rolledItemDrop;

    public static void SendTo(TEAutofisher fisher, int toClient)
    {
        fisher.ResetStats();
        var module = NetModuleLoader.Get<RollItemRequest>();
        module._tileEntityID = fisher.ID;
        module._fishingStatFlags = new BitsByte(fisher.LavaFishing, fisher.TackleBox);
        module._fishingSkill = fisher.FishingSkill;
        module._speedMultiplier = fisher.SpeedMultiplier;
        module._locatePoint = fisher.locatePoint;
        module._rolledItemDrop = 0;
        module.Send(toClient);
    }

    public override void Receive()
    {
        if (!MyUtils.TryGetTileEntityAs<TEAutofisher>(_tileEntityID, out var autofisher) ||
            TileLoader.GetTile(Main.tile[autofisher.Position.ToPoint()].TileType) is not Autofisher)
        {
            return;
        }

        switch (Main.netMode)
        {
            case NetmodeID.MultiplayerClient:
                // 使用服务器传来的数据
                autofisher.ResetStats();
                ((BitsByte)_fishingStatFlags).Deconstruct(out autofisher.LavaFishing, out autofisher.TackleBox);
                autofisher.FishingSkill = _fishingSkill;
                autofisher.SpeedMultiplier = _speedMultiplier;
                autofisher.locatePoint = _locatePoint;

                var fisher = autofisher.GetFisher(out _);

                autofisher.RollItemDrop(ref fisher);

                if (fisher.rolledItemDrop != 0)
                {
                    _rolledItemDrop = fisher.rolledItemDrop;
                    Send(); // 发送回服务器
                    // Main.NewText($"[i:{fisher.rolledItemDrop}]");
                }

                break;
            case NetmodeID.Server:
                if (_rolledItemDrop != 0)
                {
                    AutofishItemListener.ListeningAutofisher = autofisher;

                    try
                    {
                        var player = TEAutofisher.GetClosestPlayer(autofisher.Position);
                        if (player != null)
                            autofisher.GiveCatchToStorage(player, _rolledItemDrop);
                    }
                    catch
                    {
                        // ignored
                    } finally
                    {
                        AutofishItemListener.ListeningAutofisher = null;
                    }
                }

                break;
            default:
                Console.WriteLine("What the fuck?");
                break;
        }
    }
}