using ImproveGame.Content;
using ImproveGame.Content.Items.Globes;
using Terraria.Chat;
using Terraria.DataStructures;

namespace ImproveGame.Common.Packets.WorldFeatures;

[AutoSync]
public class RevealPlanteraPacket : NetModule
{
    private Point16 _position;

    public static bool Reveal(Player player)
    {
        if (Main.netMode is NetmodeID.MultiplayerClient)
            return true;

        var playerPosition = player.position.ToTileCoordinates().ToVector2();
        Point16 position = default;
        float currentDistance = float.MaxValue;
        for (int i = 10; i < Main.maxTilesX - 10; i++)
        {
            for (int j = 10; j < Main.maxTilesY - 10; j++)
            {
                var tile = Framing.GetTileSafely(i, j);
                if (!tile.HasTile || tile.type is not TileID.PlanteraBulb ||
                    tile.TileFrameX is not 18 || tile.TileFrameY is not 18)
                    continue;

                var tilePosition = new Vector2(i, j);
                if (StructureDatas.PlanteraPositions.Contains(tilePosition.ToPoint16()))
                    continue;
                
                var newDistance = tilePosition.Distance(playerPosition);
                if (newDistance < currentDistance)
                {
                    currentDistance = newDistance;
                    position = tilePosition.ToPoint16();
                }
            }
        }

        if (position == default)
        {
            ChatHelper.SendChatMessageToClient(NetworkText.FromLiteral(GetText("Items.PlanteraGlobe.NotFound")),
                Color.PaleVioletRed, player.whoAmI);
            
            // 服务器给玩家生成物品，补回消耗
            if (Main.netMode is NetmodeID.Server)
            {
                var globeId = ModContent.ItemType<PlanteraGlobe>();
                player.QuickSpawnItem(player.GetSource_ItemUse(player.HeldItem), globeId);
                return true;
            }
            return false;
        }

        var module = NetModuleLoader.Get<RevealPlanteraPacket>();
        module._position = position;
        module.Send(runLocally: true);

        var text = GetText("Items.GlobeBase.Reveal",
            Language.GetTextValue("NPCName.Plantera"), player.name);
        ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(text), Color.Pink);
        return true;
    }

    public override void Receive()
    {
        StructureDatas.PlanteraPositions.Add(_position);
    }
}