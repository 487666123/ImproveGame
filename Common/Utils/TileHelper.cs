using ImproveGame.Common.Configs;
using ImproveGame.Common.ConstructCore;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.ObjectData;
using Terraria.UI.Gamepad;

namespace ImproveGame
{
    partial class MyUtils
    {
        public static int GetItemTile(int itemType)
        {
            if (MaterialCore.FinishSetup && MaterialCore.ItemToTile.TryGetValue(itemType, out int tileType))
            {
                return tileType;
            }
            return -1;
        }

        public static int GetTileItem(int tileType, int tileFrameX, int tileFrameY)
        {
            int getItemTileType = tileType; // 用于找到物品的type（主要是给门用），到时候placeStyle是用tileType
            if (getItemTileType is TileID.OpenDoor)
            {
                getItemTileType = TileID.ClosedDoor;
            }
            if (MaterialCore.FinishSetup && MaterialCore.TileToItem.TryGetValue(getItemTileType, out List<int> itemTypes))
            {
                return itemTypes.FirstOrDefault(i => (MaterialCore.ItemToPlaceStyle[i] == TileFrameToPlaceStyle(tileType, tileFrameX, tileFrameY) || ItemLoader.IsModItem(i)), -1);
            }
            return -1;
        }

        public static int GetItemWall(int itemType)
        {
            if (MaterialCore.FinishSetup && MaterialCore.ItemToWall.TryGetValue(itemType, out int wallType))
            {
                return MaterialCore.ItemToWall[wallType];
            }
            return -1;
        }

        public static int GetWallItem(int wallType)
        {
            if (!MaterialCore.FinishSetup)
            {
                return -1;
            }

            // 合 并 同 类 项 https://terraria.wiki.gg/zh/wiki/%E5%A2%99_ID
            if (wallType is WallID.DirtUnsafe or WallID.DirtUnsafe1 or WallID.DirtUnsafe2 or WallID.DirtUnsafe3 or WallID.DirtUnsafe4)
                wallType = WallID.Dirt;
            if (wallType is WallID.HellstoneBrickUnsafe)
                wallType = WallID.HellstoneBrick;
            if (wallType is WallID.ObsidianBrickUnsafe)
                wallType = WallID.ObsidianBrick;
            if (wallType is WallID.MudUnsafe)
                wallType = WallID.MudWallEcho;
            if (wallType is WallID.SpiderUnsafe)
                wallType = WallID.SpiderEcho;
            if (wallType is WallID.ObsidianBackUnsafe)
                wallType = WallID.ObsidianBackEcho;
            if (wallType is WallID.MushroomUnsafe)
                wallType = WallID.Mushroom;
            if (wallType is WallID.HiveUnsafe)
                wallType = WallID.Hive;
            if (wallType is WallID.LihzahrdBrickUnsafe)
                wallType = WallID.LihzahrdBrick;
            // 大理石与花岗岩
            if (wallType is WallID.MarbleUnsafe)
                wallType = WallID.Marble;
            if (wallType is WallID.GraniteUnsafe)
                wallType = WallID.Granite;
            // 普通石墙系列
            if (wallType is WallID.EbonstoneUnsafe) // 黑檀石墙
                wallType = WallID.EbonstoneEcho;
            if (wallType is WallID.CrimstoneUnsafe) // 猩红石墙
                wallType = WallID.CrimstoneEcho;
            if (wallType is WallID.PearlstoneBrickUnsafe) // 珍珠石墙
                wallType = WallID.PearlstoneEcho;
            // 草墙 丛林墙 花墙
            if (wallType is >= 63 and <= 65)
                wallType = wallType - 63 + 66;
            if (wallType is WallID.HallowedGrassUnsafe) // 神圣草墙
                wallType = WallID.HallowedGrassEcho;
            if (wallType is WallID.CrimsonGrassUnsafe) // 猩红草墙
                wallType = WallID.CrimsonGrassEcho;
            if (wallType is WallID.CorruptGrassUnsafe) // 腐化草墙
                wallType = WallID.CorruptGrassEcho;
            // https://terraria.wiki.gg/zh/wiki/%E5%AE%9D%E7%9F%B3%E5%A2%99 宝石墙
            if (wallType is >= 48 and <= 53)
                wallType = wallType - 48 + 250;
            // https://terraria.wiki.gg/zh/wiki/%E5%9C%B0%E7%89%A2%E7%A0%96%E5%A2%99 地牢砖墙
            if (wallType is >= 7 and <= 9)
                wallType = wallType - 7 + 17;
            if (wallType is >= 94 and <= 99)
                wallType = wallType - 94 + 100;
            // https://terraria.wiki.gg/zh/wiki/%E6%B2%99%E5%B2%A9%E5%A2%99 沙岩墙
            // https://terraria.wiki.gg/zh/wiki/%E8%85%90%E5%8C%96%E5%A2%99 腐化墙
            // https://terraria.wiki.gg/zh/wiki/%E7%8C%A9%E7%BA%A2%E5%A2%99 猩红墙
            // https://terraria.wiki.gg/zh/wiki/%E5%9C%9F%E5%A2%99%EF%BC%88%E5%A4%A9%E7%84%B6%EF%BC%89 斑驳的土墙
            // https://terraria.wiki.gg/zh/wiki/%E7%A5%9E%E5%9C%A3%E5%A2%99 神圣墙
            // https://terraria.wiki.gg/zh/wiki/%E4%B8%9B%E6%9E%97%E5%A2%99%EF%BC%88%E5%A4%A9%E7%84%B6%EF%BC%89 特殊丛林墙
            // https://terraria.wiki.gg/zh/wiki/%E7%86%94%E5%B2%A9%E5%A2%99 熔岩墙
            // https://terraria.wiki.gg/zh/wiki/%E6%B4%9E%E5%A3%81 洞壁 (一部分)
            // https://terraria.wiki.gg/zh/wiki/%E7%A1%AC%E5%8C%96%E6%B2%99%E5%A2%99 硬化沙墙
            // https://terraria.wiki.gg/zh/wiki/%E6%B2%99%E6%BC%A0%E5%8C%96%E7%9F%B3%E5%A2%99 沙漠化石墙
            if (wallType is >= 187 and <= 223)
                wallType = wallType - 188 + 275;
            // https://terraria.wiki.gg/zh/wiki/%E6%B4%9E%E5%A3%81 洞壁
            if (wallType is >= 54 and <= 59)
                wallType = wallType - 54 + 256;
            if (wallType is WallID.Cave7Unsafe)
                wallType = WallID.Cave7Echo;
            if (wallType is WallID.Cave8Unsafe)
                wallType = WallID.Cave8Echo;
            if (wallType is >= 170 and <= 171)
                wallType = wallType - 170 + 270;
            if (MaterialCore.WallToItem.TryGetValue(wallType, out var itemType))
                return itemType[0]; // 可以多对一，但我懒得写了
            return -1;
        }

        /// <summary>
        /// 获取多格物块的左上角位置
        /// </summary>
        /// <param name="i">物块的 X 坐标</param>
        /// <param name="j">物块的 Y 坐标</param>
        public static Point16 GetTileOrigin(int i, int j)
        {
            Tile tile = Framing.GetTileSafely(i, j);
            TileObjectData tileData = TileObjectData.GetTileData(tile.TileType, 0);
            if (tileData == null)
            {
                return Point16.NegativeOne;
            }
            int frameX = tile.TileFrameX;
            int frameY = tile.TileFrameY;
            int subX = frameX % tileData.CoordinateFullWidth;
            int subY = frameY % tileData.CoordinateFullHeight;

            Point16 coord = new(i, j);
            Point16 frame = new(subX / 18, subY / 18);

            return coord - frame;
        }

        public static bool TryGetTileEntityAs<T>(int id, out T entity) where T : TileEntity
        {
            if (TileEntity.ByID.TryGetValue(id, out TileEntity existing) && existing is T existingAsT)
            {
                entity = existingAsT;
                return true;
            }

            entity = null;
            return false;
        }

        /// <inheritdoc cref="TryGetTileEntityAs{T}(Point, out T)"/>
        public static bool TryGetTileEntityAs<T>(Point16 position, out T entity) where T : TileEntity
        {
            return TryGetTileEntityAs(position.X, position.Y, out entity);
        }

        /// <inheritdoc cref="TryGetTileEntityAs{T}(int, int, out T)"/>
        /// <param name="position">物块的坐标</param>
        public static bool TryGetTileEntityAs<T>(Point position, out T entity) where T : TileEntity
        {
            return TryGetTileEntityAs(position.X, position.Y, out entity);
        }

        /// <summary>
        /// 通过 <seealso cref="GetTileOrigin(int, int)"/> 快速获取处于 (<paramref name="i"/>, <paramref name="j"/>) 的 <see cref="TileEntity"/> 实例.
        /// </summary>
        /// <typeparam name="T">实例类型，应为 <see cref="TileEntity"/></typeparam>
        /// <param name="i">物块的 X 坐标</param>
        /// <param name="j">物块的 Y 坐标</param>
        /// <param name="entity">找到的 <typeparamref name="T"/> 实例，如果没有则是null</param>
        /// <returns>返回 <see langword="true"/> 如果找到了 <typeparamref name="T"/> 实例，返回 <see langword="false"/> 如果没有实例或者该实体并非为 <typeparamref name="T"/></returns>
        public static bool TryGetTileEntityAs<T>(int i, int j, out T entity) where T : TileEntity
        {
            Point16 origin = GetTileOrigin(i, j);

            if (TileEntity.ByPosition.TryGetValue(origin, out TileEntity existing) && existing is T existingAsT)
            {
                entity = existingAsT;
                return true;
            }

            entity = null;
            return false;
        }

        /// <summary>
        /// 快捷开关箱子
        /// </summary>
        /// <param name="player">玩家实例</param>
        /// <param name="chestId">箱子ID（对于便携储存是-2/-3/-4/-5，对于其他箱子是在<see cref="Main.chest"/>的索引）</param>
        public static void ToggleChest(ref Player player, int chestId, int x = -1, int y = -1, SoundStyle? sound = null)
        {
            if (player.chest == chestId)
            {
                player.chest = -1;
                SoundEngine.PlaySound(sound ?? SoundID.MenuClose);
            }
            else
            {
                x = x == -1 ? player.Center.ToTileCoordinates().X : x;
                y = y == -1 ? player.Center.ToTileCoordinates().Y : y;
                // 以后版本TML会加的东西，只不过现在stable还没有，现在就先放在这里吧
                //player.OpenChest(x, y, chestID);
                player.chest = chestId;
                for (int i = 0; i < 40; i++)
                {
                    ItemSlot.SetGlow(i, -1f, chest: true);
                }

                player.chestX = x;
                player.chestY = y;
                player.SetTalkNPC(-1);
                Main.SetNPCShopIndex(0);

                UILinkPointNavigator.ForceMovementCooldown(120);
                if (PlayerInput.GrappleAndInteractAreShared)
                    PlayerInput.Triggers.JustPressed.Grapple = false;

                SoundEngine.PlaySound(sound ?? SoundID.MenuOpen);
            }
            Main.playerInventory = true;
            Recipe.FindRecipes();
        }

        /// <summary>
        /// 尝试破坏物块，需要有镐子，并且挖的动。
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="player"></param>
        /// <returns></returns>
        public static bool TryKillTile(int x, int y, Player player)
        {
            Tile tile = Main.tile[x, y];
            if (tile.HasTile && !Main.tileHammer[Main.tile[x, y].TileType])
            {
                if (player.HasEnoughPickPowerToHurtTile(x, y))
                {
                    if (TileID.Sets.Grass[tile.TileType] || TileID.Sets.GrassSpecial[tile.TileType] || Main.tileMoss[tile.TileType] || TileID.Sets.tileMossBrick[tile.TileType])
                    {
                        player.PickTile(x, y, 10000);
                    }
                    player.PickTile(x, y, 10000);
                }
            }
            return !Main.tile[x, y].HasTile;
        }

        public static bool CanDestroyTileAnyCases(int x, int y, Player player)
        {
            Tile tile = Main.tile[x, y];
            if (!tile.HasTile)
                return true;
            if (!Main.tileHammer[tile.TileType])
            {
                return player.HasEnoughPickPowerToHurtTile(x, y) && WorldGen.CanKillTile(x, y);
            }
            return false;
        }

        /// <summary>
        /// 你猜干嘛用的，bongbong！！！
        /// </summary>
        /// <param name="position"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public static void BongBong(Vector2 position, int width, int height)
        {
            if (Main.rand.NextBool(6))
            {
                Gore.NewGore(null, position + new Vector2(Main.rand.Next(16), Main.rand.Next(16)), Vector2.Zero, Main.rand.Next(61, 64));
            }
            if (Main.rand.NextBool(2))
            {
                int index = Dust.NewDust(position, width, height, DustID.Smoke, 0f, 0f, 100, default, 1.5f);
                Main.dust[index].velocity *= 1.4f;
            }
            if (Main.rand.NextBool(3))
            {
                int index = Dust.NewDust(position, width, height, DustID.Torch, 0f, 0f, 100, default, 2.5f);
                Main.dust[index].noGravity = true;
                Main.dust[index].velocity *= 5f;
                index = Dust.NewDust(position, width, height, DustID.Torch, 0f, 0f, 100, default, 1.5f);
                Main.dust[index].velocity *= 3f;
            }
        }

        /// <summary>
        /// 先炸掉，然后再放物块
        /// </summary>
        public static bool BongBongPlace(int i, int j, Item item, Player player, bool mute = false, bool forced = false, bool playSound = false)
        {
            // 物块魔杖特判    
            if (item.tileWand > 0)
            {
                if (CheckWandUsability(item, player, out int index) && index != -1)
                    TryConsumeItem(ref player.inventory[index], player, true);
                else return false;
            }
            bool success;
            if (Main.tile[i, j].HasTile)
            {
                if (!ValidTileForReplacement(item, i, j))
                    return false;
                if (!CanDestroyTileAnyCases(i, j, player))
                    return false;
                if (WorldGen.ReplaceTile(i, j, (ushort)item.createTile, item.placeStyle))
                {
                    success = true;
                }
                else
                {
                    TryKillTile(i, j, player);
                    success = WorldGen.PlaceTile(i, j, item.createTile, mute, forced, player.whoAmI, item.placeStyle);
                }
            }
            else
            {
                success = WorldGen.PlaceTile(i, j, item.createTile, mute, forced, player.whoAmI, item.placeStyle);
            }
            if (success && UIConfigs.Instance.ExplosionEffect)
            {
                BongBong(new Vector2(i, j) * 16f, 16, 16);
                if (playSound)
                    SoundEngine.PlaySound(SoundID.Item14, Main.MouseWorld);
            }
            return success;
        }

        /// <summary>
        /// 放物块，但是有魔杖特判
        /// </summary>
        public static bool TryPlaceTile(int i, int j, Item item, Player player, bool mute = false, bool forced = false)
        {
            int targetTile = item.createTile;
            var tileObjectData = TileObjectData.GetTileData(targetTile, 0);
            if (tileObjectData is null || (tileObjectData.CoordinateFullWidth <= 22 && tileObjectData.CoordinateFullHeight <= 22))
            {
                if (Main.tile[i, j].HasTile)
                    return false;
            }
            else
            {
                var origin = new Point16(i, j) - tileObjectData.Origin;
                for (int m = 0; m < tileObjectData.CoordinateFullWidth / 16; m++)
                {
                    for (int n = 0; n < tileObjectData.CoordinateFullHeight / 16; n++)
                    {
                        if (Main.tile[origin.ToPoint() + new Point(m, n)].HasTile)
                            return false;
                    }
                }
            }
            // 物块魔杖特判    
            if (item.tileWand > 0)
            {
                if (CheckWandUsability(item, player, out int index) && index != -1)
                    TryConsumeItem(ref player.inventory[index], player, true);
                else return false;
            }
            bool placed = WorldGen.PlaceTile(i, j, targetTile, mute, forced, player.whoAmI, item.placeStyle);
            return placed;
        }

        public enum CheckType
        {
            TypeAndStyle,
            Type
        }
        /// <summary>
        /// 判断物块是否相同
        /// </summary>
        public static bool SameTile(int i, int j, int Type, int Style, CheckType tileSort = CheckType.TypeAndStyle)
        {
            if (tileSort == CheckType.TypeAndStyle)
            {
                if (Main.tile[i, j].TileType == Type && Main.tile[i, j].TileFrameY == Style * 18)
                {
                    return true;
                }
            }
            else if (tileSort == CheckType.Type)
            {
                if (Main.tile[i, j].TileType == Type)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 魔法移除物块方法
        /// </summary>
        public static void NormalKillTiles(Player player, Rectangle rect, Action<int, int> removeAction = null)
        {
            // 获得背包中最好的镐子
            Item item = player.GetBestPickaxe();
            int minI = rect.X;
            int maxI = rect.X + rect.Width - 1;
            int minJ = rect.Y;
            int maxJ = rect.Y + rect.Height - 1;
            for (int i = 0; i < player.hitTile.data.Length; i++)
            {
                HitTile.HitTileObject hitTileObject = player.hitTile.data[i];
                hitTileObject.timeToLive = 10000;
            }

            for (int i = minI; i <= maxI; i++)
            {
                for (int j = minJ; j <= maxJ; j++)
                {
                    if (removeAction is not null)
                    {
                        removeAction(i, j);
                    }
                    Tile tile = Main.tile[i, j];
                    if (!Main.tileAxe[tile.TileType] && !Main.tileHammer[tile.TileType])
                    {
                        player.PickTile(i, j, item != null ? item.pick : 1);
                        player.hitTile.data[player.hitTile.HitObject(i, j, 1)].timeToLive = 10000;
                    }
                }
            }
            for (int i = 0; i < player.hitTile.data.Length; i++)
            {
                HitTile.HitTileObject hitTileObject = player.hitTile.data[i];
                hitTileObject.timeToLive = 60;
            }
        }

        /// <summary>
        /// 遍历 Tile
        /// </summary>
        public static void ForeachTile(Rectangle rectangle, Action<int, int> action, Action<int, int, int, int> lastMethod = null)
        {
            int minI = rectangle.X;
            int maxI = rectangle.X + rectangle.Width - 1;
            int minJ = rectangle.Y;
            int maxJ = rectangle.Y + rectangle.Height - 1;
            for (int i = minI; i <= maxI; i++)
            {
                for (int j = minJ; j <= maxJ; j++)
                {
                    action(i, j);
                }
            }
            lastMethod?.Invoke(minI, minJ, maxI - minI + 1, maxJ - minJ + 1);
        }

        /// <summary>
        /// 原版的私有方法,作用是判断物品对应的物块和坐标的物块是不是一样的，为空间魔杖做了一些删减。
        /// <br>相同的物块返回 false</br>
        /// </summary>
        /// <param name="tileItem"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool ValidTileForReplacement(Item tileItem, int x, int y)
        {
            int createTile = tileItem.createTile;
            Tile tile = Main.tile[x, y];

            // 平台
            if (TileID.Sets.Platforms[tile.TileType] && tile.TileType == createTile)
                return tile.TileFrameY != tileItem.placeStyle * 18;

            // 箱子
            if (TileID.Sets.BasicChest[tile.TileType] && TileID.Sets.BasicChest[createTile])
            {
                if (tile.TileFrameX / 36 == tileItem.placeStyle)
                {
                    return tile.TileType != createTile;
                }
                return true;
            }

            // 梳妆台
            if (TileID.Sets.BasicDresser[tile.TileType] && TileID.Sets.BasicDresser[createTile])
            {
                if (tile.TileFrameX / 54 == tileItem.placeStyle)
                {
                    return tile.TileType != createTile;
                }
                return true;
            }

            // 是不是一样的物块
            if (Main.tile[x, y].TileType == createTile)
                return false;

            // 能否忽略掉落检查
            if (!TileID.Sets.IgnoresTileReplacementDropCheckWhenBeingPlaced[createTile])
            {
                WorldGen.KillTile_GetItemDrops(x, y, tile, out var dropItem, out var _, out var _, out var _);
                if (dropItem == tileItem.type)
                    return false;
            }
            return true;
        }
    }
}
