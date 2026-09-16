using ImproveGame.Common.Configs;
using ImproveGame.Content.Items.Globes.Core;
using ImproveGame.Core;
using System.Reflection.Emit;

namespace ImproveGame.Content.Projectiles;

public abstract class GlobeProjBase(Color mainColor) : ModProjectile
{
    public override string Texture => GetModItemDummy().Texture;

    public override LocalizedText DisplayName => GetModItemDummy().DisplayName;

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailingMode[Type] = 2;
        ProjectileID.Sets.TrailCacheLength[Type] = 30;
        Globe.GlobeLookup[GetModItemDummy().Type] = Type;
    }

    public override void SetDefaults()
    {
        Projectile.CloneDefaults(ProjectileID.MoonGlobe);
        Projectile.width = 32;
        Projectile.height = 32;
        AIType = ProjectileID.MoonGlobe;
    }

    public override void AI()
    {
        // 弹幕刚出生就进行判断是否会消耗，便于后边整活
        if (Projectile.ai[2] == 0 && Main.netMode != NetmodeID.MultiplayerClient)
        {
            if (RevealOperation(true))
                Projectile.ai[2] = 1;
            else Projectile.ai[2] = -1;
            NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, Projectile.whoAmI);
        }

        // 整活Time！
        if (Projectile.ai[2] < 0)
        {
            Projectile.aiStyle = -1;
            AIType = 0;
            Projectile.ai[2]--;
            if (Projectile.ai[2] >= -180)
            {
                Projectile.velocity.X *= 0.99f;
                Projectile.velocity.Y += 0.13f;
                Projectile.rotation += Projectile.velocity.X / 10f;
            }

            if (Projectile.ai[2] == -180)
            {
                Projectile.velocity = Projectile.rotation.ToRotationVector2() * 4;
            }
        }

        // ai进入二阶段
        if (Projectile.ai[2] < -180 && Projectile.ai[1] >= 0)
        {

            Player player = Main.player[Projectile.owner];

            // 穿墙，启动
            Projectile.tileCollide = false;

            // 目标速度
            var targetVel = Vector2.Normalize(player.Center - Projectile.Center) * (7f - Projectile.ai[2] / 60f);
            // 加权平均
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, targetVel, 0.03f);

            #region 保证一定会追上的迫真插值
            float fac = Utils.GetLerpValue(-180, -610, Projectile.ai[2], true);
            float distance = Vector2.Distance(Projectile.Center, player.Center);
            fac = MathF.Pow(fac, 5);
            fac = MathHelper.Lerp(fac, 1, MathF.Exp(-distance / 64));
            Projectile.Center = Vector2.Lerp(Projectile.Center, player.Center, fac);
            #endregion


            Projectile.rotation = Projectile.velocity.ToRotation();

            // 你给我死
            if (Projectile.owner == Main.myPlayer && Projectile.Center.Distance(player.Center) < 12)
            {
                Projectile.velocity *= 0.000000001f;
                Projectile.ai[1] = -1;
                NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, Projectile.whoAmI);
            }

            // 幽默特效
            if (Projectile.ai[2] % 12 == 0)
                SoundEngine.PlaySound(SoundID.Item13 with { MaxInstances = 114514 }, Projectile.Center);

            if (Main.rand.NextBool(2))
                Dust.NewDustPerfect(Projectile.Center + (Vector2.Normalize(Projectile.velocity) * -16), DustID.FireworksRGB,
                    -Projectile.velocity.RotatedByRandom(0.3f) * 0.5f, 0, RealColor, 2f * Main.rand.NextFloat()).noGravity = true;
        }

        // 死亡缓至——无法预料，无法逃避
        if (Projectile.ai[1] == -1)
        {
            Projectile.ai[1]--;

            // 物品只能在服务器/单人掉落
            if (Main.netMode is not NetmodeID.MultiplayerClient && !RevealOperation(false))
                Main.player[Projectile.owner].QuickSpawnItem(Projectile.GetItemSource_DropAsItem(), GetModItemDummy().Type);

            if (Projectile.ai[2] < 0)
            {
                SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);

                #region 别展开，里头有答辩

                // 你 好
                for (int i = 0; i < 10; i++)
                {
                    int dustIndex = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y),
                        Projectile.width, Projectile.height, DustID.Smoke, 0f, 0f, 100, default, 2f);
                    Main.dust[dustIndex].velocity *= 1.4f;
                }

                for (int i = 0; i < 20; i++)
                {
                    int dustIndex = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y),
                        Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 100, default, 3f);
                    Main.dust[dustIndex].noGravity = true;
                    Main.dust[dustIndex].velocity *= 5f;
                    dustIndex = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width,
                        Projectile.height, DustID.Torch, 0f, 0f, 100, default, 2f);
                    Main.dust[dustIndex].velocity *= 3f;
                }

                for (int g = 0; g < 2; g++)
                {
                    int goreIndex = Gore.NewGore(Projectile.GetSource_Death(),
                        new Vector2(Projectile.position.X + Projectile.width / 2 - 24f,
                            Projectile.position.Y + Projectile.height / 2 - 24f), default, Main.rand.Next(61, 64), 1f);
                    Main.gore[goreIndex].velocity.X += 1.5f;
                    Main.gore[goreIndex].velocity.Y += 1.5f;
                    goreIndex = Gore.NewGore(Projectile.GetSource_Death(),
                        new Vector2(Projectile.position.X + Projectile.width / 2 - 24f,
                            Projectile.position.Y + Projectile.height / 2 - 24f), default, Main.rand.Next(61, 64), 1f);
                    Main.gore[goreIndex].velocity.X -= 1.5f;
                    Main.gore[goreIndex].velocity.Y += 1.5f;
                    goreIndex = Gore.NewGore(Projectile.GetSource_Death(),
                        new Vector2(Projectile.position.X + Projectile.width / 2 - 24f,
                            Projectile.position.Y + Projectile.height / 2 - 24f), default, Main.rand.Next(61, 64), 1f);
                    Main.gore[goreIndex].velocity.X += 1.5f;
                    Main.gore[goreIndex].velocity.Y -= 1.5f;
                    goreIndex = Gore.NewGore(Projectile.GetSource_Death(),
                        new Vector2(Projectile.position.X + Projectile.width / 2 - 24f,
                            Projectile.position.Y + Projectile.height / 2 - 24f), default, Main.rand.Next(61, 64), 1f);
                    Main.gore[goreIndex].velocity.X -= 1.5f;
                    Main.gore[goreIndex].velocity.Y -= 1.5f;
                }

                #endregion

                return;
            }

            // 特效弹幕只能在服务器/单人生成
            //var effectColor = GetEffectColor();
            if (Main.netMode is not NetmodeID.MultiplayerClient)
                Projectile.NewProjectile(Projectile.GetSource_Death(), Projectile.Center, Vector2.Zero,
                    ModContent.ProjectileType<GlobeEffect>(), 0, 0, ai0: Type);

            SoundEngine.PlaySound(SoundID.Item107, Projectile.Center);
            for (int i = 0; i < 15; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Glass, 0f, -2f, 0, default,
                    1.2f);
            }
        }

        if (Projectile.ai[1] < 0)
        {
            Projectile.ai[1]--;

            // 这才是真正的死亡
            if (Projectile.ai[1] < -30)
                Projectile.Kill();
        }
    }

    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        // 爆改ai之反弹
        if (Projectile.ai[2] < 0)
        {
            if (Projectile.velocity.X != oldVelocity.X && Math.Abs(oldVelocity.X) > 0.4f)
            {
                Projectile.velocity.X = oldVelocity.X * -0.9f;
            }

            if (Projectile.velocity.Y != oldVelocity.Y && Math.Abs(oldVelocity.Y) > 1f)
            {
                Projectile.velocity.Y = oldVelocity.Y * -0.85f;
            }

            SoundEngine.PlaySound(Main.rand.Next([SoundID.Item140, SoundID.Item141, SoundID.Item142]),
                Projectile.Center);
            return false;
        }

        // 进死门（
        if (Projectile.ai[1] >= 0 && Projectile.owner == Main.myPlayer)
        {
            Projectile.velocity *= 0.000000001f;
            Projectile.tileCollide = false;
            Projectile.ai[1] = -1;
            Projectile.aiStyle = -1;
            AIType = 0;
            NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, Projectile.whoAmI);
        }

        return false;
    }

    public override bool PreDraw(Player player, ref Color lightColor)
    {
        if (!UIConfigs.Instance.GlobeEffect)
            return base.PreDraw(player, ref lightColor);

        var projTex = TextureAssets.Projectile[Type].Value;
        var cross = ModAsset.CrazyGlowCross.Value;
        var roundGlow = ModAsset.GlowOrb.Value;
        var center = Projectile.Center - Main.screenPosition;
        var glowCrossColor = RealColor;
        glowCrossColor.A = 0;

        // 顶点拖尾
        var triangleList = PrepareTriangleList();
        if (triangleList.Count >= 3)
            DrawTrail(triangleList);

        // 进入死亡阶段后不要再绘制下面了
        if (Projectile.ai[1] >= 0)
        {
            // 背后发光大圆球
            Main.spriteBatch.Draw(roundGlow, center, null, glowCrossColor * 0.7f,
                1.57f, roundGlow.Size() / 2f, 0.45f, SpriteEffects.None, 0);

            // 本体绘制
            Main.spriteBatch.Draw(projTex, center, null, Color.White,
                Projectile.rotation + 1.57f, projTex.Size() / 2f, Projectile.scale, SpriteEffects.None, 0);

            // 十字高光绘制
            Main.spriteBatch.Draw(cross, center, null, glowCrossColor,
                0f, cross.Size() / 2f, 1f, SpriteEffects.None, 0);
        }

        Main.spriteBatch.ReBegin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

        return false;
    }

    private void DrawTrail(List<VertexInfo2> triangleList)
    {
        var projTex = TextureAssets.Projectile[Type].Value;

        // Main.spriteBatch.ReBegin(SpriteSortMode.Deferred, BlendState.Additive);
        Main.spriteBatch.End();
        Main.graphics.GraphicsDevice.BlendState = BlendState.Additive;


        // 干掉注释就可以显示三角形栅格
        // RasterizerState rasterizerState = new RasterizerState();
        // rasterizerState.CullMode = CullMode.None;
        // rasterizerState.FillMode = FillMode.WireFrame;
        // Main.graphics.GraphicsDevice.RasterizerState = rasterizerState;

        var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, 0, 1);
        var model = Matrix.CreateTranslation(new Vector3(-Main.screenPosition.X, -Main.screenPosition.Y, 0));
        var trans = Main.GameViewMatrix != null ? Main.GameViewMatrix.TransformationMatrix : Matrix.Identity;

        float uTime = (Main.GameUpdateCount % 30) / 30f;

        ModAsset.GlobeTrail.Value.Parameters["uTransform"].SetValue(model * trans * projection);
        ModAsset.GlobeTrail.Value.Parameters["uTime"].SetValue(uTime);

        Main.graphics.GraphicsDevice.Textures[0] = ModAsset.TrailStyle3.Value;
        Main.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

        ModAsset.GlobeTrail.Value.CurrentTechnique.Passes[0].Apply();

        try
        {
            Main.instance.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, triangleList.ToArray(), 0, triangleList.Count / 3);
        }
        catch (Exception e)
        {
            TimeLogger.DrawException(e);
            Projectile.active = false;
            goto label;
        }

        label:
        var sb = Main.spriteBatch;
        var matrix = sb.transformMatrix;
        var effect = sb.customEffect;
        sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, sb.GraphicsDevice.SamplerStates[0],
            sb.GraphicsDevice.DepthStencilState, sb.GraphicsDevice.RasterizerState, effect, matrix);
    }

    private List<VertexInfo2> PrepareTriangleList()
    {
        List<VertexInfo2> vertices = [];
        List<VertexInfo2> triangleList = [];
        int actualLength = 0;
        foreach (var vec in Projectile.oldPos)
        {
            if (vec == Vector2.Zero)
                break;
            actualLength++;
        }
        int maxIndex = actualLength - 1;
        Color color = RealColor;
        for (var i = 0; i < maxIndex; i++)
        {
            Vector2 oldVel = Projectile.oldPos[i] - Projectile.oldPos[i + 1];
            var factor = Utils.GetLerpValue(0, maxIndex - 1, i);
            Vector2 correct = Vector2.Normalize(oldVel).RotatedBy(1.57f) * MathHelper.SmoothStep(30, 24, factor);
            //var fac2 = (1 - MathF.Cos(MathHelper.TwoPi * MathF.Pow(factor, .25f))) * .5f;
            var fac2 = i == 0 ? 0 : 1 - factor;
            vertices.Add(new VertexInfo2(Projectile.oldPos[i] + Projectile.Size / 2f + correct,
                new Vector3(factor, 0f, fac2), color));
            vertices.Add(new VertexInfo2(Projectile.oldPos[i] + Projectile.Size / 2f - correct,
                new Vector3(factor, 1f, fac2), color));
        }

        if (vertices.Count > 2)
        {
            // 按照顺序连接三角形
            for (int i = 0; i < vertices.Count - 2; i += 2)
            {
                // 这是一个四边形 [i] [i+2] [i+1] [i+3]
                triangleList.Add(vertices[i]);
                triangleList.Add(vertices[i + 2]);
                triangleList.Add(vertices[i + 1]);

                triangleList.Add(vertices[i + 1]);
                triangleList.Add(vertices[i + 2]);
                triangleList.Add(vertices[i + 3]);
            }
        }

        return triangleList;
    }

    public override bool? CanCutTiles()
    {
        return false;
    }

    public abstract Globe GetModItemDummy();

    public virtual Color? GetEffectColor() => null;

    protected GlobeProjBase() : this(Color.White)
    {

    }

    public Color mainColor = mainColor;

    public Color RealColor => GetEffectColor() ?? mainColor;

    public abstract bool RevealOperation(bool onlyJudging);
}
public abstract class GlobeProjBase<T>(Color mainColor) : GlobeProjBase(mainColor) where T : Globe
{
    public override Globe GetModItemDummy() => ModContent.GetInstance<T>();
}