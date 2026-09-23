namespace ImproveGame.Content.Items;

public partial class SpaceWand
{
    private static readonly int[] GrassSeeds =
        [
        TileID.Grass,
        TileID.CorruptGrass,
        TileID.JungleGrass,
        TileID.MushroomGrass,
        TileID.ImmatureHerbs,
        TileID.HallowedGrass,
        TileID.CrimsonGrass,
        TileID.AshGrass,
        TileID.GreenMoss,
        TileID.BrownMoss,
        TileID.RedMoss,
        TileID.BlueMoss,
        TileID.PurpleMoss,
        TileID.LavaMoss,
        TileID.KryptonMoss,
        TileID.XenonMoss,
        TileID.ArgonMoss,
        TileID.VioletMoss,
        TileID.RainbowMoss
        ];

    public static void OperateTile(Player player, int x, int y, HashSet<Point> tilesHashSet, PlaceType placeType,
        BlockType blockType, ref bool playSound, Dictionary<int, int> itemsConsumed)
    {
        int oneIndex = MyUtils.EnoughItem(player, GetConditions(placeType));
        if (oneIndex <= -1)
            return;
        // 使用物块魔杖时，ignoreConsumable应为true，无论如何都消耗物品。这是原版的逻辑
        bool usingTileWand = false;
        int indexOfItemBeingConsumed = oneIndex;
        Item item = player.inventory[oneIndex];
        if (item.tileWand >= 0)
        {
            int actualItemIndex = MyUtils.EnoughItem(player, i => i.type == item.tileWand);
            if (actualItemIndex <= -1)
                return;

            usingTileWand = true;
            indexOfItemBeingConsumed = actualItemIndex;
        }

        if (!TileLoader.CanPlace(x, y, item.createTile))
            return;

        if (GrassSeeds.Contains(item.createTile))
        {
            if (WorldGen.PlaceTile(x, y, item.createTile, true, false, player.whoAmI, item.placeStyle))
            {
                playSound = true;
                // PickItemInInventory(player, GetConditions(placeType), false, out int index);
                HandleItemConsumption(player, indexOfItemBeingConsumed, itemsConsumed, usingTileWand);
            }
        }
        else
        {
            Tile originalTile = Main.tile[x, y];

            if (originalTile.HasTile)
            {
                WorldGen.SlopeTile(x, y, noEffects: true);
                // 同种类，设置个斜坡就走
                if (originalTile.TileType == item.createTile)
                {
                    SetSlopeFor(placeType, blockType, x, y, tilesHashSet);
                }

                if (player.TileReplacementEnabled)
                {
                    // 物品放置的瓷砖就是位置对应的瓷砖则无需替换
                    if (!MyUtils.ValidTileForReplacement(item, x, y))
                    {
                        // 至少还可以设置个斜坡
                        SetSlopeFor(placeType, blockType, x, y, tilesHashSet);
                        return;
                    }

                    // 如果放的是绳子，且试图替换的是平台/铁轨，那就不放（笑点解析：绳子和平台/铁轨相交原理是判断上下有没有绳子，本身还是平台/铁轨）
                    if (placeType == PlaceType.Rope && (TileID.Sets.Platforms[originalTile.TileType] || originalTile.TileType == TileID.MinecartTrack))
                        return;

                    // 有没有足够强大的镐子破坏瓷砖
                    if (!player.HasEnoughPickPowerToHurtTile(x, y))
                        return;

                    // 开始替换
                    if (WorldGen.ReplaceTile(x, y, (ushort)item.createTile, item.placeStyle))
                    {
                        playSound = true;
                        // PickItemInInventory(player, GetConditions(placeType), false, out int index);
                        HandleItemConsumption(player, indexOfItemBeingConsumed, itemsConsumed, usingTileWand);
                        SetSlopeFor(placeType, blockType, x, y, tilesHashSet);
                    }
                    else
                    {
                        // 尝试破坏
                        MyUtils.TryKillTile(x, y, player);
                        if (!originalTile.HasTile && WorldGen.PlaceTile(x, y, item.createTile, true, true,
                                player.whoAmI, item.placeStyle))
                        {
                            playSound = true;
                            // PickItemInInventory(player, GetConditions(placeType), false, out int index);
                            HandleItemConsumption(player, indexOfItemBeingConsumed, itemsConsumed, usingTileWand);
                            SetSlopeFor(placeType, blockType, x, y, tilesHashSet);
                        }
                    }
                }
            }
            else if (WorldGen.PlaceTile(x, y, item.createTile, true, true, player.whoAmI, item.placeStyle))
            {
                playSound = true;
                // PickItemInInventory(player, GetConditions(placeType), false, out int index);
                HandleItemConsumption(player, indexOfItemBeingConsumed, itemsConsumed, usingTileWand);
                SetSlopeFor(placeType, blockType, x, y, tilesHashSet);
            }
        }

        // NetMessage.SendTileSquare(player.whoAmI, x, y);
    }

    private static void HandleItemConsumption(Player player, int index, Dictionary<int, int> itemsConsumed,
        bool ignoreConsumable)
    {
        if (index is -1)
            return;

        ref Item item = ref player.inventory[index];
        int type = item.type; // TurnToAir之后type就为0了，提前存好
        if (!MyUtils.TryConsumeItem(ref item, player, ignoreConsumable))
            return;

        if (!itemsConsumed.TryAdd(type, 1))
            itemsConsumed[type]++;
    }

    private static void SetSlopeFor(PlaceType placeType, BlockType blockType, int x, int y, HashSet<Point> positions)
    {
        var tile = Main.tile[x, y];
        if (WorldGen.CanPoundTile(x, y) && placeType is PlaceType.Soild or PlaceType.Platform)
        {
            tile.BlockType = blockType;
            WorldGen.SquareTileFrame(x, y, false);
        }

        // 仅当现在在放置实心块，且斜坡类型也为实心时，才重置下方物块的斜坡状态
        if (placeType is PlaceType.Soild && blockType is BlockType.Solid && !positions.Contains(new Point(x, y + 1)))
            WorldGen.SlopeTile(x, y + 1);
    }
}