using ImproveGame.Common.Configs;
using ImproveGame.Content.Items;
using MonoMod.Cil;

namespace ImproveGame.Common.ModPlayers;

internal class TilePlaceBoostHandler : ModPlayer
{
    public override void Load()
    {
        IL_Player.PlaceThing_Tiles_PlaceIt += BoostPlacementItemTime;
        IL_Player.PlaceThing_Walls += BoostPlacementItemTime;
        IL_Player.PlaceThing_TryReplacingWalls += BoostPlacementItemTime;
        IL_Player.PlaceThing_PaintScrapper_TryScrapping += BoostPlacementItemTime;
        IL_Player.ApplyPaint += BoostPlacementItemTime;
        IL_Player.ApplyCoating += BoostPlacementItemTime;
    }

    private static void BoostPlacementItemTime(ILContext il)
    {
        var cursor = new ILCursor(il);
        while (cursor.TryGotoNext(MoveType.After, instruction =>
            instruction.MatchLdfld<Player>(nameof(Player.tileSpeed)) || instruction.MatchLdfld<Player>(nameof(Player.wallSpeed))))
        {
            cursor.EmitLdarg0();
            cursor.EmitDelegate(static (float multiplier, Player player) =>
                player.whoAmI == Main.myPlayer && ShouldItemAcceptSpeedBoost(player.HeldItem)
                    ? Math.Min(multiplier, 1f / 3f)
                    : multiplier);
        }
    }

    /// <summary>
    /// 设置玩家的放置范围
    /// </summary>
    public override void UpdateEquips()
    {
        if (Player.whoAmI != Main.myPlayer)
            return;

        Player.tileRangeX += ImproveConfigs.Instance.ModifyPlayerTileRange;
        Player.tileRangeY += ImproveConfigs.Instance.ModifyPlayerTileRange;
    }

    private static bool ShouldItemAcceptSpeedBoost(Item item)
    {
        if (item.IsAir || !ImproveConfigs.Instance.ModifyPlayerPlaceSpeed)
            return false;

        // 《英文名》因为没法在非英语语言获取英文名，只能用内部名了
        string internalName = ItemID.Search.GetName(item.type).ToLower();
        string currentLanguageName = Lang.GetItemNameValue(item.type).ToLower();

        if (ImproveConfigs.Instance.TileSpeed_Blacklist.Any(str => internalName.Contains(str) || currentLanguageName.Contains(str)))
            return false;

        // 是特判捏嘿嘿
        if (item.ModItem is MoveChest)
            return false;

        return true;
    }
}
