using ImproveGame.Content.Projectiles;
using Terraria;

namespace ImproveGame.Content.Items
{
    public class WallPlace : ModItem
    {
        public static readonly int RobotType = ModContent.ProjectileType<WallRobot>();

        public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1;

        public override bool IsLoadingEnabled(Mod mod) => Config.LoadModItems.WallPlace;

        public override void SetDefaults()
        {
            Item.SetBaseValues(46, 42, ItemRarityID.Red, Item.sellPrice(0, 0, 50));
            Item.SetUseValues(ItemUseStyleID.Swing, SoundID.Item1, 15, 15);
        }

        public override void HoldItem(Player player)
        {
            Item firstWall = FirstWall(player);
            if (firstWall is null) return;

            player.cursorItemIconEnabled = true;
            player.cursorItemIconID = firstWall.type;
            player.cursorItemIconPush = 6;
        }

        /// <summary>
        /// 判断是不是实体块
        /// </summary>
        /// <param name="position"></param>
        /// <param name="Walls"></param>
        /// <param name="Walls2"></param>
        /// <returns>
        /// 0 没有方块没有墙体<br/>
        /// 1 有实体块(可以阻挡人物移动)<br/>
        /// 2 有墙体没有方块
        /// </returns>
        public static int HasBoundary(Point position, ref List<Point> Walls, ref List<Point> Walls2)
        {
            Tile tile = Main.tile[position.X, position.Y];

            if (tile.HasTile && (TileID.Sets.Platforms[tile.TileType] || Main.tileSolid[tile.TileType] ||
                                 tile.TileType == 10 || tile.TileType == 11))
                return 1;

            if (tile.WallType == 0)
                return 0;

            return 2;
        }

        public static void SearchWalls(Point pos, ref List<Point> walls1, ref List<Point> walls2)
        {
            // 不在世界内，在Wall内，大于500
            if (pos.X >= Main.maxTilesX || pos.Y >= Main.maxTilesY || pos.X <= 0 || pos.Y <= 0 ||
                walls1.Contains(pos) || walls2.Contains(pos) || walls1.Count > 2500 || walls2.Count > 2500)
                return;

            int hasTile = HasBoundary(pos, ref walls1, ref walls2);

            if (hasTile == 1)
                return;

            if (hasTile == 0)
                walls1.Add(pos);

            if (hasTile == 2)
                walls2.Add(pos);

            SearchWalls(pos + new Point(0, -1), ref walls1, ref walls2);
            SearchWalls(pos + new Point(1, 0), ref walls1, ref walls2);
            SearchWalls(pos + new Point(0, 1), ref walls1, ref walls2);
            SearchWalls(pos + new Point(-1, 0), ref walls1, ref walls2);
        }

        public override bool? UseItem(Player player)
        {
            Point point = Main.MouseWorld.ToTileCoordinates();
            if (Main.myPlayer == player.whoAmI)
            {
                foreach (Projectile proj in Main.projectile)
                {
                    if (proj.owner == player.whoAmI && proj.active && proj.type == RobotType)
                    {
                        proj.Kill();
                        CombatText.NewText(proj.getRect(), Color.Red, GetText("CombatText.Item.WallPlace_Kill"));
                        return true;
                    }
                }

                List<Point> walls1 = new(), walls2 = new();
                SearchWalls(point, ref walls1, ref walls2);

                if (walls1.Count == 0)
                    return false;

                if (walls1.Count > 2500)
                {
                    CombatText.NewText(player.getRect(), Color.Red, GetText("CombatText.Item.WallPlace_Limit"));
                    return true;
                }
                else
                {
                    if (FirstWall(player) is not null)
                    {
                        Vector2 center = Main.MouseWorld.ToTileCoordinates().ToVector2() * 16 + new Vector2(8);
                        // 角度排序
                        walls1.Sort((a, b) =>
                        {
                            float ap = (a.ToVector2() * 16f + new Vector2(8) - center).ToRotation();
                            float bp = (b.ToVector2() * 16f + new Vector2(8) - center).ToRotation();
                            if (ap > bp)
                                return 1;
                            if (ap < bp)
                                return -1;
                            return 0;
                        });
                        // 距离排序
                        /*walls1.Sort((a, b) =>
                        {
                            float ap = center.Distance(a.ToVector2() * 16f + new Vector2(8));
                            float bp = center.Distance(b.ToVector2() * 16f + new Vector2(8));
                            if (ap > bp)
                                return 1;
                            if (ap < bp)
                                return -1;
                            return 0;
                        });*/
                        CombatText.NewText(player.getRect(), new Color(0, 155, 255),
                            GetText("CombatText.Item.WallPlace_Consume") + walls1.Count);
                        Projectile proj = Projectile.NewProjectileDirect(null, center, Vector2.Zero, RobotType, 0, 0,
                            player.whoAmI);
                        proj.Center = center;
                        WallRobot robot = (WallRobot)proj.ModProjectile;
                        robot.Walls = walls1;
                    }
                }
            }

            return true;
        }

        public override bool CanUseItem(Player player)
        {
            if (player.noBuilding)
                return false;
            return base.CanUseItem(player);
        }
    }
}