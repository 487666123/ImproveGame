using ImproveGame.Common.Configs;
using ImproveGame.Common.ModSystems;
using ImproveGame.Content.Tiles;
using ImproveGame.UI.ExtremeStorage;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria.DataStructures;

namespace ImproveGame.Content.Functions;

internal class PortableStationSystem : ModSystem
{
    public override void Load()
    {
        // 便携制作站
        IL_Player.AdjTiles += AddPortableStations;
        On_Recipe.UpdateRecipeList += On_Recipe_UpdateRecipeList;
    }

    private void On_Recipe_UpdateRecipeList(On_Recipe.orig_UpdateRecipeList orig)
    {
        if (!ImproveConfigs.Instance.PortableCraftingStation)
        {
            orig.Invoke();
            return;
        }
        var flag = Main.LocalPlayer.ZoneGraveyard;
        var flag2 = Main.LocalPlayer.ZoneSnow;

        CheckZoneItemFromPlayer(Main.LocalPlayer);
        if (ImproveConfigs.Instance.ShareCraftingStation)
            PlayerHelper.ForEachTeammate(Main.myPlayer, CheckZoneItemFromPlayer);

        orig.Invoke();

        Main.LocalPlayer.ZoneGraveyard = flag;
        Main.LocalPlayer.ZoneSnow = flag2;
    }
    static void CheckZoneItemFromPlayer(Player player)
    {
        int counter = 0;
        int counter2 = 0;
        foreach (var item in GetAllInventoryItemsList(player))
        {
            if (item.createTile == -1)
                continue;

            if (item.createTile == TileID.Tombstones)
            {
                counter += item.stack;
                if (counter >= 5)
                {
                    Main.LocalPlayer.ZoneGraveyard = true;
                    goto Label;
                }

            }
            else if (TileID.Sets.SnowBiome[item.createTile] > 0)
            {
                counter2 += item.stack * TileID.Sets.SnowBiome[item.createTile];
                if (counter2 >= 1500)
                {
                    Main.LocalPlayer.ZoneSnow = true;
                    goto Label;
                }
            }
            continue;
            Label:
            if (counter >= 5 && counter2 >= 1500)
                break;
        }

        if (counter >= 5 && counter2 >= 1500)
            return;

        // 从TE中获取所有的无尽Buff物品
        foreach ((int _, TileEntity tileEntity) in TileEntity.ByID)
        {
            if (tileEntity is not TEExtremeStorage { UsePortableStations: true } storage)
            {
                continue;
            }

            var alchemyItems = storage.FindAllNearbyChestsWithGroup(ItemGroup.Furniture);
            List<Item> allItems = [.. alchemyItems.SelectMany(i => Main.chest[i].item)];

            foreach (var item in allItems)
            {
                if (item is null)
                    return;

                if (item.createTile == -1)
                    continue;

                if (item.createTile == TileID.Tombstones)
                {
                    counter += item.stack;
                    if (counter >= 5)
                    {
                        Main.LocalPlayer.ZoneGraveyard = true;
                        goto Label;
                    }

                }
                else if (TileID.Sets.SnowBiome[item.createTile] > 0)
                {
                    counter2 += item.stack * TileID.Sets.SnowBiome[item.createTile];
                    if (counter2 >= 1500)
                    {
                        Main.LocalPlayer.ZoneSnow = true;
                        goto Label;
                    }
                }
                continue;
                Label:
                if (counter >= 5 && counter2 >= 1500)
                    break;
            }

            if (counter >= 5 && counter2 >= 1500)
                break;
        }

    }
    private void AddPortableStations(ILContext il)
    {
        var c = new ILCursor(il);
        if (!c.TryGotoNext(MoveType.Before, i => i.MatchRet()))
        {
            ILMatchLog(nameof(AddPortableStations), il);
            return;
        }
        c.Emit(OpCodes.Ldarg_0);
        c.EmitDelegate<Action<Player>>(player =>
        {
            if (!ImproveConfigs.Instance.PortableCraftingStation)
                return;

            // 从玩家身上获取所有的无尽Buff物品
            CheckStationsFromPlayer(player);
            if (ImproveConfigs.Instance.ShareCraftingStation)
                PlayerHelper.ForEachTeammate(player.whoAmI, CheckStationsFromPlayer);

            // 从TE中获取所有的无尽Buff物品
            foreach ((int _, TileEntity tileEntity) in TileEntity.ByID)
            {
                if (tileEntity is not TEExtremeStorage { UsePortableStations: true } storage)
                {
                    continue;
                }

                var alchemyItems = storage.FindAllNearbyChestsWithGroup(ItemGroup.Furniture);
                alchemyItems.ForEach(i => CheckStations(Main.chest[i].item));
            }
        });
    }

    internal void CheckStationsFromPlayer(Player inventorySource) =>
        CheckStations(GetAllInventoryItemsList(inventorySource));

    /// <summary>
    /// 从某个玩家的各种物品栏中拿效果
    /// </summary>
    internal static void CheckStations(IEnumerable<Item> items)
    {
        foreach (var item in items)
        {
            int tileType = item.createTile;
            if (tileType > -1 && tileType < TileLoader.TileCount)
            {
                CheckChainedStations(tileType, Main.LocalPlayer);
            }

            if (ModIntegrationsSystem.PortableStations.TryGetValue(item.type, out var tileIDs))
            {
                foreach (int tileID in tileIDs)
                {
                    CheckChainedStations(tileID, Main.LocalPlayer);
                }
            }

            if (item.type is ItemID.WaterBucket or ItemID.BottomlessBucket)
            {
                Main.LocalPlayer.adjWaterSource = true;
            }

            if (item.type is ItemID.LavaBucket or ItemID.BottomlessLavaBucket)
            {
                Main.LocalPlayer.adjLava = true;
            }

            if (item.type is ItemID.HoneyBucket /*or ItemID.BottomlessHoneyBucket*/)
            {
                Main.LocalPlayer.adjHoney = true;
            }
        }
    }

    // 一系列的Station链条操作，每个tileID都要经历一次的
    private static void CheckChainedStations(int tileType, Player player)
    {
        player.adjTile[tileType] = true;
        if (TileID.Sets.CountsAsWaterForCrafting[tileType])
        {
            player.adjWaterSource = true;
        }

        if (TileID.Sets.CountsAsLavaForCrafting[tileType])
        {
            player.adjLava = true;
        }

        if (TileID.Sets.CountsAsHoneyForCrafting[tileType])
        {
            player.adjHoney = true;
        }

        switch (tileType)
        {
            case TileID.Hellforge:
            case TileID.GlassKiln:
                player.adjTile[TileID.Furnaces] = true;
                break;
            case TileID.AdamantiteForge:
                player.adjTile[TileID.Furnaces] = true;
                player.adjTile[TileID.Hellforge] = true;
                break;
            case TileID.MythrilAnvil:
                player.adjTile[TileID.Anvils] = true;
                break;
            case TileID.BewitchingTable:
            case TileID.Tables2:
            case TileID.PicnicTable:
                player.adjTile[TileID.Tables] = true;
                break;
            case TileID.AlchemyTable:
                player.adjTile[TileID.Bottles] = true;
                player.adjTile[TileID.Tables] = true;
                player.alchemyTable = true;
                break;
        }

        TileLoader.AdjTiles(player, tileType);
    }
}