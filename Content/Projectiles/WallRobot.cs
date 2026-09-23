using ImproveGame.Common.Configs;
using ImproveGame.UIFramework.Graphics2D;

namespace ImproveGame.Content.Projectiles
{
    public class WallRobot : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.timeLeft = 100000;
        }

        public List<Point> Walls = new();

        public int index
        {
            get => (int)Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }

        public Player Player => Main.player[Projectile.owner];

        public override void AI()
        {
            Item firstWall = MyUtils.FirstWall(Player);
            Lighting.AddLight(Projectile.Center, 45 / 510f, 206 / 510f, 255 / 510f);

            // 没有墙体，结束放置。
            if (firstWall is null)
            {
                CombatText.NewText(Projectile.getRect(), new Color(225, 0, 0), MyUtils.GetText("CombatText.Projectile.PlaceWall_Lack"));
                Projectile.Kill();
                return;
            }

            if (index < Walls.Count)
            {
                Point wall = Walls[index++];
                MyUtils.BongBong(wall.ToVector2() * 16, 16, 16);
                Tile tile = Main.tile[wall.X, wall.Y];
                if (firstWall.createWall != tile.WallType)
                {
                    if (tile.WallType > 0)
                    {
                        WorldGen.KillWall(wall.X, wall.Y);
                    }
                    if (tile.WallType <= 0)
                    {
                        WorldGen.PlaceWall(wall.X, wall.Y, firstWall.createWall);
                        NetMessage.SendTileSquare(Projectile.owner, wall.X, wall.Y);
                        // 大于等于 999 不消耗墙
                        // ItemLoader.ConsumeItem 判断手持物品是否是科技法杖，但是他是机器人放置墙体的，即使手持不是科技也可能不消耗
                        bool vanillaHooks = firstWall.consumable && ItemLoader.ConsumeItem(firstWall, Player);
                        bool modSpecificChecks = firstWall.stack < 999 || !ImproveConfigs.Instance.WandMaterialNoConsume;
                        if (vanillaHooks && modSpecificChecks && --firstWall.stack == 0)
                            firstWall.TurnToAir();
                    }
                }
                Projectile.rotation = (wall.ToVector2() * 16f + new Vector2(8) - Projectile.Center).ToRotation() + MathF.PI;
            }
            else
            {
                Projectile.Kill();
            }
        }

        public Color background = new(45, 206, 255);
        public Color border = new(66, 117, 186);
        public override bool PreDraw(Player player, ref Color lightColor)
        {
            if (index < Walls.Count)
            {
                Vector2 center = Projectile.Center - Main.screenPosition,
                    target = Walls[index - 1].ToVector2() * 16f + new Vector2(8) - Main.screenPosition;
                SDFGraphics.HasBorderLine(center, target, 2f, background, 1f, border, MyUtils.GetMatrix(false));
            }
            return true;
        }

        public override void PostDraw(Player player, Color lightColor)
        {
            Main.EntitySpriteDraw(ModAsset.WallRobot_Glow.Value, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, Projectile.Size / 2f, Projectile.scale, SpriteEffects.None);
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);
            for (int i = 0; i < 10; i++)
                MyUtils.BongBong(Projectile.position, Projectile.width, Projectile.height);
        }
    }
}
