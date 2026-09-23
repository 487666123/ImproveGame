using ImproveGame.Common.ModPlayers;
using ImproveGame.Content.Tiles;

namespace ImproveGame.Packets.NetAutofisher
{
    /// <summary>
    /// 客户端发送，问服务器可不可以开箱
    /// </summary>
    [AutoSync]
    public class OpenRequestPacket : NetModule
    {
        private int tileEntityID;

        public static OpenRequestPacket Get(TEAutofisher fisher)
        {
            var module = NetModuleLoader.Get<OpenRequestPacket>();
            module.tileEntityID = fisher.ID;
            return module;
        }

        public override void Receive()
        {
            for (int k = 0; k < Main.maxPlayers; k++)
            {
                var player = Main.player[k];
                if (player.active && !player.dead && player.TryGetModPlayer<AutofishPlayer>(out var modPlayer) &&
                    modPlayer.Autofisher is not null && modPlayer.Autofisher.ID == tileEntityID)
                {
                    return;
                }
            }

            if (!MyUtils.TryGetTileEntityAs<TEAutofisher>(tileEntityID, out var autofisher))
                return;

            // 没玩家，发开箱包
            var packets = AggregateModule.Get(
                new()
                {
                    FisherAllFiltersPacket.Get(
                        tileEntityID,
                        autofisher.CatchCrates,
                        autofisher.CatchAccessories,
                        autofisher.CatchTools,
                        autofisher.CatchWhiteRarityCatches,
                        autofisher.CatchNormalCatches,
                        autofisher.AutoDeposit
                    ),
                    LocatePointPacket.Get(
                        tileEntityID,
                        autofisher.locatePoint
                    ),
                    ItemsSyncAllPacket.Get(
                        tileEntityID,
                        true,
                        autofisher.fishingPole,
                        autofisher.bait,
                        autofisher.accessory,
                        autofisher.fish
                    )
                }
            );
            // 服务器设置好
            Main.player[Sender].GetModPlayer<AutofishPlayer>().SetAutofisher(autofisher, false);
            packets.Send(Sender, -1, false);
        }
    }

    /// <summary>
    /// 服务器发送，让其他玩家知道有个玩家开箱了
    /// </summary>
    [AutoSync]
    public class SyncOpenPacket : NetModule
    {
        private int tileEntityID;
        private byte whoAmI;

        public static SyncOpenPacket Get(int tileEntityID, int whoAmI)
        {
            var module = NetModuleLoader.Get<SyncOpenPacket>();
            module.tileEntityID = tileEntityID;
            module.whoAmI = (byte)whoAmI;
            return module;
        }

        public override void Receive()
        {
            MyUtils.TryGetTileEntityAs<TEAutofisher>(tileEntityID, out var autofisher);

            if (Main.netMode is NetmodeID.MultiplayerClient)
            {
                if (AutofishPlayer.TryGet(Main.player[whoAmI], out var modPlayer))
                {
                    modPlayer.SetAutofisher(autofisher, false);
                }
            }
            else
            {
                // 服务端的设置好
                Main.player[Sender].GetModPlayer<AutofishPlayer>().SetAutofisher(autofisher, false);
                // 给其他玩家发开关包
                Send(-1, Sender, false);
            }
        }
    }
}