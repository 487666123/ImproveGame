using ImproveGame.Common.ModSystems;
using ImproveGame.Content.Items;
using MonoMod.Cil;
using System.Diagnostics;
using System.Reflection;
using Terraria.GameContent.Drawing;
using Terraria.ObjectData;

namespace ImproveGame.Content.Functions.Construction
{
    internal class PreviewRenderer : ModSystem
    {
        internal enum ResetState { WaitReset, Drawing, Finished }

        // 用RT2D绘制一次，之后直接放RT，就不需要一直绘制降低性能了
        internal static RenderTarget2D PreviewTarget;
        internal static ResetState ResetPreviewTarget;
        private bool _canDrawLastFrame;

        internal static string UIPreviewPath;
        internal static RenderTarget2D UIPreviewTarget;
        internal static ResetState ResetUIPreviewTarget;

        public override void Load()
        {
            if (Main.dedServ)
                return;

            On_Main.SetDisplayMode += RefreshTarget;
            IL_Main.DoDraw += DrawTarget;
        }

        private void DrawTarget(ILContext il)
        {
            var cursor = new ILCursor(il);
            if (!cursor.TryGotoNext(c => c.MatchCall(typeof(Main).GetMethod(nameof(Main.DoDraw_UpdateCameraPosition), BindingFlags.Static | BindingFlags.NonPublic))))
            {
                ILMatchLog(nameof(DrawTarget), il);
                return;
            }
            cursor.Index += 2;
            cursor.EmitDelegate(DrawTarget_Inner);
        }

        public override void Unload()
        {
            PreviewTarget = null;
            UIPreviewTarget = null;
        }

        private void RefreshTarget(On_Main.orig_SetDisplayMode orig, int width, int height, bool fullscreen)
        {
            orig(width, height, fullscreen);
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int rulerIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Ruler"));
            if (rulerIndex != -1)
            {
                layers.Insert(rulerIndex, new LegacyGameInterfaceLayer("ImproveGame: Strcture Preview", delegate
                {
                    if (Main.LocalPlayer.HeldItem?.type == ModContent.ItemType<ConstructWand>() &&
                        WandSystem.ConstructMode == WandSystem.Construct.Place &&
                        !string.IsNullOrEmpty(WandSystem.ConstructFilePath) &&
                        File.Exists(WandSystem.ConstructFilePath))
                    {
                        var color = Color.White * 0.6f;
                        var drawPosition = Main.MouseWorld;
                        drawPosition = drawPosition.ToTileCoordinates().ToWorldCoordinates(0f, 0f) - Main.screenPosition;
                        var spriteEffects = Main.LocalPlayer.gravDir is -1 ? SpriteEffects.FlipVertically : SpriteEffects.None;

                        var tag = FileOperator.GetTagFromFile(WandSystem.ConstructFilePath);

                        if (tag is null)
                            return true;

                        // 添加Origin偏移
                        drawPosition.X -= tag.GetShort("OriginX") * 16f;
                        drawPosition.Y -= tag.GetShort("OriginY") * 16f;
                        if (Main.LocalPlayer.gravDir is -1) // 做一个重力转换
                        {
                            float oppositeY = (drawPosition.Y - Main.screenHeight / 2);
                            drawPosition.Y -= oppositeY * 2f;
                            drawPosition.Y -= PreviewTarget.Height; // 由于FlipVertically特性，这里要调回来
                        }
                        drawPosition -= new Vector2(2f, 2f * Main.LocalPlayer.gravDir);

                        Main.spriteBatch.Draw(PreviewTarget, drawPosition, null, color, 0f, Vector2.Zero, 1f, spriteEffects, 0f);
                    }
                    return true;
                }, InterfaceScaleType.Game));
            }
        }

        private void DrawTarget_Inner()
        {
            bool canDraw = Main.LocalPlayer.HeldItem?.type == ModContent.ItemType<ConstructWand>() &&
                WandSystem.ConstructMode == WandSystem.Construct.Place;
            if (((canDraw && !_canDrawLastFrame) || ResetPreviewTarget == ResetState.WaitReset) && !string.IsNullOrEmpty(WandSystem.ConstructFilePath) && File.Exists(WandSystem.ConstructFilePath))
            {
                if (PreviewTarget is null)
                {
                    var tag = FileOperator.GetTagFromFile(WandSystem.ConstructFilePath);

                    if (tag is not null)
                    {
                        int width = tag.GetShort("Width");
                        int height = tag.GetShort("Height");
                        PreviewTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, width * 16 + 20, height * 16 + 20, false, default, default, default, RenderTargetUsage.PreserveContents);
                    }
                }
                if (PreviewTarget is not null)
                {
                    ResetPreviewTarget = ResetState.Drawing;
                    DrawPreviewToRender(PreviewTarget, WandSystem.ConstructFilePath);
                    if (ResetPreviewTarget != ResetState.WaitReset)
                        ResetPreviewTarget = ResetState.Finished;
                }
            }
            _canDrawLastFrame = canDraw;

            if (!string.IsNullOrEmpty(UIPreviewPath) && File.Exists(UIPreviewPath) && ResetUIPreviewTarget == ResetState.WaitReset)
            {
                ResetUIPreviewTarget = ResetState.Drawing;
                DrawPreviewToRender(UIPreviewTarget, UIPreviewPath);
                if (ResetUIPreviewTarget != ResetState.WaitReset)
                    ResetUIPreviewTarget = ResetState.Finished;
            }
        }

        private static void DrawPreviewToRender(RenderTarget2D renderTarget, string filePath)
        {
            Main.spriteBatch.Begin();

            Main.graphics.GraphicsDevice.SetRenderTarget(renderTarget);
            Main.graphics.GraphicsDevice.Clear(Color.Transparent);

            var structure = new QoLStructure(filePath);

            if (structure.Tag is null)
            {
                Main.spriteBatch.End();
                Main.graphics.GraphicsDevice.SetRenderTarget(null);
                return;
            }

            var color = Color.GreenYellow;
            var position = Vector2.Zero + new Vector2(2f, 2f);
            DrawBorder(position, (structure.Width + 1) * 16f, (structure.Height + 1) * 16f, color * 0.35f, color); // 背景边框
            DrawPreviewFromTag(Main.spriteBatch, structure, position, 1f);

            Main.spriteBatch.End();
            Main.graphics.GraphicsDevice.SetRenderTarget(null);
        }

        public static bool DrawPreviewFromTag(SpriteBatch sb, QoLStructure structure, Vector2 origin, float scale = 1f)
        {
            List<TileDefinition> data = structure.StructureDatas;

            if (data is null || data.Count is 0)
            {
                // 此处应有Logger.Warn
                return false;
            }

            var spriteEffects = SpriteEffects.None;

            int width = structure.Width;
            int height = structure.Height;

            for (int x = 0; x <= width; x++)
            {
                for (int y = 0; y <= height; y++)
                {
                    int index = y + x * (height + 1);

                    TileDefinition tileData = data[index];

                    int wallType = structure.ParseWallType(tileData);

                    var position = origin + new Point(x, y).ToWorldCoordinates(0f, 8f);

                    if (wallType > 0) // Wall
                    {
                        Color color = Color.White;
                        if (wallType == WallID.RainbowBrick)
                            color = new Color(Main.DiscoR, Main.DiscoG, Main.DiscoB);

                        Texture2D textureWall = GetWallDrawTexture(tileData.WallColor, wallType);

                        int wallFrame = Main.wallFrame[wallType] * 180;
                        Rectangle value = new(tileData.WallFrameX, tileData.WallFrameY + wallFrame, 32, 32);
                        Vector2 pos = position - new Vector2(8f, 8f);
                        sb.Draw(textureWall, pos * scale, value, color, 0f, new Vector2(0f, 8f), scale, spriteEffects, 0f);
                    }
                }
            }

            for (int x = 0; x <= width; x++)
            {
                for (int y = 0; y <= height; y++)
                {
                    int index = y + x * (height + 1);

                    TileDefinition tileData = data[index];

                    int tileType = structure.ParseTileType(tileData);

                    var position = origin + new Point(x, y).ToWorldCoordinates(0f, 8f);

                    // 这绘制...不想再写第二遍了
                    if (tileType != -1) // Tile
                    {
                        Color color = Color.White;

                        if (tileData.ExtraDatas[2])
                            color = color.MultiplyRGB(Color.White * 0.4f);
                        else
                        {
                            if (TileDrawing.ShouldTileShine((ushort)tileType, tileData.TileFrameX))
                                color = Main.shine(color, tileType);
                        }
                        switch (tileType)
                        {
                            case 51:
                                color *= 0.5f;
                                break;
                            case TileID.RainbowBrick:
                            {
                                color = new Color(Main.DiscoR, Main.DiscoG, Main.DiscoB, 255);
                                if (tileData.ExtraDatas[2])
                                    color = color.MultiplyRGB(Color.White * 0.4f);
                                break;
                            }
                            case 129:
                            {
                                color = new Color(255, 255, 255, 100);
                                if (tileData.TileFrameX >= 324)
                                    color = Color.Transparent;
                                break;
                            }
                        }

                        Texture2D texture = GetTileDrawTexture(tileData.TileColor, tileType);
                        var normalTileRect = new Rectangle(tileData.TileFrameX, tileData.TileFrameY, 16, 16);

                        int tileItemType = GetTileItem(tileType, tileData.TileFrameX, tileData.TileFrameY);
                        TileObjectData tileObjectData = null;
                        if (tileItemType is not -1)
                        {
                            tileObjectData = TileObjectData.GetTileData(tileType, MaterialCore.ItemToPlaceStyle[tileItemType]);
                        }
                        bool multiTile = tileObjectData is not null && (tileObjectData.CoordinateFullWidth > 18 || tileObjectData.CoordinateFullHeight > 18);

                        if (tileData.BlockType is not BlockType.HalfBlock or BlockType.Solid && !multiTile)
                        {
                            if (TileID.Sets.Platforms[tileType])
                            {
                                int platformDrawType = tileData.GetPlatformDrawType();
                                if (platformDrawType is 0 or 1)
                                {
                                    Rectangle value = new(198, tileData.TileFrameY, 16, 16);
                                    if (platformDrawType is 1)
                                        value.X = 324;

                                    Main.spriteBatch.Draw(texture, (position + new Vector2(0f, 16f)) * scale, value, color, 0f, new Vector2(0f, 8f), scale, spriteEffects, 0f);
                                }
                                else if (platformDrawType is 2 or 3)
                                {
                                    Rectangle value = new(162, tileData.TileFrameY, 16, 16);
                                    if (platformDrawType is 3)
                                        value.X = 306;

                                    Main.spriteBatch.Draw(texture, (position + new Vector2(0f, 16f)) * scale, value, color, 0f, new Vector2(0f, 8f), scale, spriteEffects, 0f);
                                }
                                goto EndSlopeDraw;
                            }
                            if (TileID.Sets.HasSlopeFrames[tileType])
                            {
                                Main.spriteBatch.Draw(texture, position * scale, normalTileRect, color, 0f, new Vector2(0f, 8f), scale, spriteEffects, 0f);
                                continue;
                            }

                            int num2 = 2;
                            for (int i = 0; i < 8; i++)
                            {
                                int num3 = i * -2;
                                int num4 = 16 - i * 2;
                                int num5 = 16 - num4;
                                int num6;
                                switch (tileData.BlockType)
                                {
                                    case BlockType.SlopeDownLeft:
                                        num3 = 0;
                                        num6 = i * 2;
                                        num4 = 14 - i * 2;
                                        num5 = 0;
                                        break;
                                    case BlockType.SlopeDownRight:
                                        num3 = 0;
                                        num6 = 16 - i * 2 - 2;
                                        num4 = 14 - i * 2;
                                        num5 = 0;
                                        break;
                                    case BlockType.SlopeUpLeft:
                                        num6 = i * 2;
                                        break;
                                    default:
                                        num6 = 16 - i * 2 - 2;
                                        break;
                                }

                                sb.Draw(texture, (position + new Vector2(num6, i * num2 + num3)) * scale, new Rectangle(tileData.TileFrameX + num6, tileData.TileFrameY + num5, num2, num4), color, 0f, new Vector2(0f, 8f), scale, SpriteEffects.None, 0f);
                            }
                        }
                        EndSlopeDraw:;

                        // 旗帜和战争桌旗处于平台下的offsetY调节
                        if (multiTile && tileType is TileID.Banners or TileID.WarTableBanner)
                        {
                            int subX = tileData.TileFrameX % tileObjectData.CoordinateFullWidth;
                            int subY = tileData.TileFrameY % tileObjectData.CoordinateFullHeight;

                            Point coord = new(x, y);
                            Point frame = new(subX / 18, subY / 18);
                            var leftTop = coord - frame;
                            leftTop.Y -= 1; // 坐标为左上角物块的上方物块

                            if (leftTop is { X: >= 0, Y: >= 0 })
                            {
                                TileDefinition tileDataUp = data[leftTop.Y + leftTop.X * (height + 1)];
                                int tileTypeUp = structure.ParseTileType(tileDataUp);
                                if (tileTypeUp is not -1 && TileID.Sets.Platforms[tileTypeUp] &&
                                    tileDataUp.BlockType is BlockType.Solid)
                                {
                                    position.Y -= 8f;
                                }
                            }
                        }

                        if (!multiTile)
                        {
                            int num7 = ((int)tileData.BlockType <= 3) ? 14 : 0;
                            sb.Draw(texture, (position + new Vector2(0f, num7)) * scale, new Rectangle(tileData.TileFrameX, tileData.TileFrameY + num7, 16, 2), color, 0f, new Vector2(0f, 8f), scale, spriteEffects, 0f);
                        }

                        if (tileData.BlockType is BlockType.HalfBlock)
                        {
                            position.Y += 8f;
                            sb.Draw(texture, (position + new Vector2(0f, 4f)) * scale, new Rectangle(144, 66, 16, 4), color, 0f, new Vector2(0f, 8f), scale, spriteEffects, 0f);
                            sb.Draw(texture, position * scale, normalTileRect.Modified(0, 0, 0, -4), color, 0f, new Vector2(0f, 8f), scale, spriteEffects, 0f);
                        }
                        if (tileData.BlockType is BlockType.Solid || TileID.Sets.Platforms[tileType] || multiTile)
                        {
                            sb.Draw(texture, position * scale, normalTileRect, color, 0f, new Vector2(0f, 8f), scale, spriteEffects, 0f);
                        }
                    }

                    if (tileData.HasActuator)
                    {
                        Color actColor = Color.White * 0.7f;
                        sb.Draw(TextureAssets.Actuator.Value, position * scale, null, actColor, 0f, new Vector2(0f, 8f), scale, spriteEffects, 0f);
                    }
                }
            }
            return true;
        }

        public static Texture2D GetTileDrawTexture(int paintColor, int tileType)
        {
            Main.instance.LoadTiles(tileType);
            Texture2D result = TextureAssets.Tile[tileType].Value;
            int tileStyle = 0;
            switch (tileType)
            {
                case TileID.Trees:
                    tileStyle = -1;
                    break;
                case TileID.PalmTree:
                    tileStyle = 0;
                    break;
            }

            Texture2D texture2D = Main.instance.TilePaintSystem.TryGetTileAndRequestIfNotReady(tileType, tileStyle, paintColor);
            if (texture2D is not null)
            {
                result = texture2D;
            }
            else // 下一帧重设，给这些Request一帧时间让他们加载
            {
                if (ResetPreviewTarget == ResetState.Drawing)
                {
                    ResetPreviewTarget = ResetState.WaitReset;
                }
                if (ResetUIPreviewTarget == ResetState.Drawing)
                {
                    ResetUIPreviewTarget = ResetState.WaitReset;
                }
            }

            return result;
        }

        public static Texture2D GetWallDrawTexture(int paintColor, int wallType)
        {
            Main.instance.LoadWall(wallType);
            Texture2D result = TextureAssets.Wall[wallType].Value;

            Texture2D texture2D = Main.instance.TilePaintSystem.TryGetWallAndRequestIfNotReady(wallType, paintColor);
            if (texture2D is not null)
            {
                result = texture2D;
            }
            else // 下一帧重设，给这些Request一帧时间让他们加载
            {
                if (ResetPreviewTarget == ResetState.Drawing)
                {
                    ResetPreviewTarget = ResetState.WaitReset;
                }
                if (ResetUIPreviewTarget == ResetState.Drawing)
                {
                    ResetUIPreviewTarget = ResetState.WaitReset;
                }
            }

            return result;
        }
    }
}
