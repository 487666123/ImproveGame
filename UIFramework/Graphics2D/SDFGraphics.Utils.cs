//本文件中的形状绘制全部改自iq大佬的SDF https://iquilezles.org/articles/distfunctions2d/

namespace ImproveGame.UIFramework.Graphics2D;

partial class SDFGraphics
{
    const float root2Over2 = 1.414213562373f / 2f;
    static Effect SDF_Effect => ModAsset.SDFGraphics.Value;
    static bool DrawFrame => false;

    private struct VertexPosCoord(Vector2 pos, Vector2 coord) : IVertexType
    {
        private static readonly VertexDeclaration _vertexDeclaration = new(
        [
            new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0),
            new VertexElement(8, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0)
        ]);

        public Vector2 Pos = pos;
        public Vector2 Coord = coord;

        public readonly VertexDeclaration VertexDeclaration => _vertexDeclaration;
    }

    private static void GetSDFVertexInfo(ref List<VertexPosCoord> vertices, Vector2 pos, Vector2 size)
    {
        vertices.Add(new VertexPosCoord(pos, new Vector2(0, 0)));
        vertices.Add(new VertexPosCoord(pos + new Vector2(size.X, 0), new Vector2(size.X, 0)));
        vertices.Add(new VertexPosCoord(pos + new Vector2(0, size.Y), new Vector2(0, size.Y)));
        vertices.Add(new VertexPosCoord(pos + new Vector2(0, size.Y), new Vector2(0, size.Y)));
        vertices.Add(new VertexPosCoord(pos + new Vector2(size.X, 0), new Vector2(size.X, 0)));
        vertices.Add(new VertexPosCoord(pos + size, size));
    }
    static void SetBackground(Color background) => SDF_Effect.Parameters["uBackgroundColor"].SetValue(background.ToVector4());
    static void SetBorder(float border, Color borderColor)
    {
        Effect effect = SDF_Effect;
        effect.Parameters["uBorder"].SetValue(border);
        effect.Parameters["uBorderColor"].SetValue(borderColor.ToVector4());
    }
    static void SetCommon(Matrix matrix)
    {
        Effect effect = SDF_Effect;
        effect.Parameters["uTransition"].SetValue(new Vector2(-root2Over2, root2Over2) / Main.UIScale);
        effect.Parameters["uTransform"].SetValue(matrix);

    }
    static void SetBar(Texture2D barTexture, float time, float distanceScaler)
    {
        Effect effect = SDF_Effect;
        effect.Parameters["uTime"].SetValue(time);
        effect.Parameters["uValueScaler"].SetValue(distanceScaler);
        Main.instance.GraphicsDevice.Textures[0] = barTexture;
        Main.instance.GraphicsDevice.SamplerStates[0] = SamplerState.AnisotropicWrap;
    }
    static void ApplyPass(string passName) => SDF_Effect.CurrentTechnique.Passes[passName].Apply();
    private static void BaseDraw(Vector2 pos, Vector2 size)
    {
        //List<VertexPosCoord> vertices = [];
        //GetSDFVertexInfo(ref vertices, pos, size);
        VertexPosCoord[] triangles = [  new(pos, new(0, 0)),
                                        new(pos + new Vector2(size.X, 0), new(size.X, 0)),
                                        new(pos + new Vector2(0, size.Y), new(0, size.Y)),
                                        new(pos + new Vector2(0, size.Y), new(0, size.Y)),
                                        new(pos + new Vector2(size.X, 0), new(size.X, 0)),
                                        new(pos + size, size)];//.. vertices
        Main.graphics.GraphicsDevice.DrawUserPrimitives(0, triangles, 0, 2);
        Main.spriteBatch.spriteEffectPass.Apply();


        //DEBUG用代码
        if (DrawFrame)
            for (int n = 0; n < triangles.Length / 3; n++)
            {
                DrawLine(triangles[3 * n].Pos, triangles[3 * n + 1].Pos, 1);
                DrawLine(triangles[3 * n + 1].Pos, triangles[3 * n + 2].Pos, 1);
                DrawLine(triangles[3 * n + 2].Pos, triangles[3 * n].Pos, 1);
            }
    }
    static void DrawLine(Vector2 start, Vector2 end, float LineWidth)
    {
        Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, (start + end) * .5f, new Rectangle(0, 0, 1, 1), Color.White, (start - end).ToRotation(), new Vector2(.5f), new Vector2(Vector2.Distance(start, end), LineWidth), 0, 0);
    }

    public static void FumoFumoKoishi(Vector2 position)
    {
        //NoBorderBox(default, default, new Vector2(10000, 10000), Color.White, GetMatrix(true));
        Matrix matrix = MyUtils.GetMatrix(true);
        float border = 2f;
        Color borderColor = Color.Black;

        #region 帽子
        Color hatColor = new Color(38, 52, 89);
        Color beltColor = new Color(244, 233, 167);
        HasBorderEllipse(position + new Vector2(20, -45), new Vector2(.5f), new Vector2(130, 100), hatColor, border, borderColor, matrix);

        HasBorderEllipse(position + new Vector2(100, -75), new(.5f), new Vector2(40, 20), beltColor, border, borderColor, matrix);
        HasBorderChainedQuadraticBezier(position + new Vector2(20, 10), [new(150, -110), new(185, -130), new(200, -120), new(190, -100), new(205, -80), new(220, -60), new(210, -30), new(195, -20), new(160, -30)], beltColor, border, borderColor, matrix);
        HasBorderChainedQuadraticBezier(position + new Vector2(20, 10), [new(110, -90), new(120, -140), new(150, -150), new(140, -120), new(150, -110), new(160, -100), new(170, -70), new(180, -40), new(150, -50)], beltColor, border, borderColor, matrix);
        HasBorderEllipse(position + new Vector2(20, -5), new Vector2(.5f), new Vector2(170, 100), hatColor, border, borderColor, matrix);

        #endregion

        #region 裙子
        Color clothColor = new Color(0, 132, 134);

        HasBorderQuadraticCircle(position + new Vector2(20, 260) + new Vector2(-50, -10), new Vector2(.5f), 30, new Color(252, 244, 241), border, borderColor, matrix);
        HasBorderQuadraticCircle(position + new Vector2(20, 260) + new Vector2(50, -10), new Vector2(.5f), 30, new Color(252, 244, 241), border, borderColor, matrix);

        HasBorderChainedQuadraticBezier(position + new Vector2(20, 190), [new(-60, 0), new(-70, 45), new(-80, 40), new(0, 100), new(80, 40), new(70, 45), new(60, 0)], clothColor, border, borderColor, matrix);

        NoBorderRing(position + new Vector2(20, 230), new Vector2(.5f), 80, MathHelper.Pi / 3, 4, Color.White, matrix);

        HasBorderQuadraticCircle(position + new Vector2(20, 260) + new Vector2(-60, 0), new Vector2(.5f), 30, hatColor, border, borderColor, matrix);
        HasBorderQuadraticCircle(position + new Vector2(20, 260) + new Vector2(60, 0), new Vector2(.5f), 30, hatColor, border, borderColor, matrix);
        #endregion

        #region 衣服
        //Color clothColor = new Color(0, 132, 134);
        Color lightHat = new(74, 81, 120);
        HasBorderRound(position + new Vector2(-70, 228), new Vector2(.5f), 50, new Color(252, 244, 241), border, borderColor, matrix);
        HasBorderRound(position + new Vector2(110, 228), new Vector2(.5f), 50, new Color(252, 244, 241), border, borderColor, matrix);
        HasBorderQuadraticCircle(position + new Vector2(20, 140), new(.5f), 70, beltColor, border, borderColor, matrix);

        HasBorderChainedQuadraticBezier(position + new Vector2(-100, 208), [default, new(5, 10), new(-10, 20), new(25, 30), new(60, 20), new(50, 10), new(60, 0)], lightHat, border, borderColor, matrix);
        HasBorderChainedQuadraticBezier(position + new Vector2(140, 208), [default, new(-5, 10), new(10, 20), new(-25, 30), new(-60, 20), new(-50, 10), new(-60, 0)], lightHat, border, borderColor, matrix);
        HasBorderChainedQuadraticBezier(position + new Vector2(-40, 88), [default, new(-80, 60), new(-60, 120), new(-35, 140), new(0, 120), new(20, 90), new(20, 60)], beltColor, border, borderColor, matrix);
        HasBorderChainedQuadraticBezier(position + new Vector2(80, 88), [default, new(80, 60), new(60, 120), new(35, 140), new(0, 120), new(-20, 90), new(-20, 60)], beltColor, border, borderColor, matrix);


        HasBorderPolygon(position + new Vector2(20, 100), [new(-45, -9), new(-75, -15), new(-50, 20), new(-40, 10)], lightHat, border, borderColor, matrix);
        HasBorderPolygon(position + new Vector2(20, 100), [new(-45, -9), new(75, -15), new(50, 20), new(40, 10)], lightHat, border, borderColor, matrix);
        HasBorderTriangle(position + new Vector2(20, 100), position + new Vector2(-20, 110), position + new Vector2(-40, 88), clothColor, border, borderColor, matrix);
        HasBorderTriangle(position + new Vector2(20, 100), position + new Vector2(60, 110), position + new Vector2(80, 88), clothColor, border, borderColor, matrix);

        //水晶
        HasBorderRhombus(position + new Vector2(20, 120), new Vector2(.5f), new Vector2(20, 40), new Color(170, 233, 246), border, borderColor, matrix);
        HasBorderRhombus(position + new Vector2(20, 160), new Vector2(.5f), new Vector2(15, 30), new Color(170, 233, 246), border, borderColor, matrix);
        #endregion

        #region 脸
        HasBorderQuadraticCircle(position + new Vector2(20, 0), new(.5f), 100, new Color(252, 244, 241), border, borderColor, matrix);
        //HasBorderEllipse(position + new Vector2(20,10), new Vector2(.5f), new Vector2(110, 90), new Color(252, 244, 241), border, borderColor, matrix);
        #endregion

        #region 腮红
        NoBorderEllipse(position + new Vector2(-50, 50), new Vector2(.5f), new Vector2(15, 10), new Color(253, 190, 181), matrix);
        NoBorderEllipse(position + new Vector2(90, 50), new Vector2(.5f), new Vector2(15, 10), new Color(253, 190, 181), matrix);
        #endregion

        #region 头发
        Color hairColor = new Color(218, 241, 235);
        Color hairColor_Dark = new Color(177, 215, 226);
        //中
        HasBorderChainedQuadraticBezier(position + new Vector2(20, 10), [new(-45, -60), new(-30, 0), new(5, 10), new(25, -50), new(10, -70)], hairColor, border, borderColor, matrix);

        //左下
        HasBorderChainedQuadraticBezier(position + new Vector2(20, 10), [new(-100, 60), new(-95, 75), new(-100, 90), new(-120, 80), new(-120, 50), new(-135, 65), new(-125, 80), new(-160, 65), new(-110, -10)], hairColor_Dark, border, borderColor, matrix);

        //左下-正下
        HasBorderChainedQuadraticBezier(position + new Vector2(20, 10), [new(-95, 15), new(-85, 50), new(-50, 60), new(-75, 70), new(-90, 65), new(-95, 75), new(-80, 80), new(-130, 65), new(-110, 10)], hairColor, border, borderColor, matrix);

        //左上
        HasBorderChainedQuadraticBezier(position + new Vector2(20, 10), [new(-45, -60), new(-85, -20), new(-90, 20), new(-115, 20), new(-120, -20)], hairColor, border, borderColor, matrix);
        HasBorderChainedQuadraticBezier(position + new Vector2(20, 10), [new(-120, -20), new(-130, -10), new(-140, -15), new(-120, -30), new(-120, -50), new(-120, -80), new(-90, -100)], hairColor, border, borderColor, matrix);

        //右下

        HasBorderChainedQuadraticBezier(position + new Vector2(20, 10), [new(80, 70), new(95, 77), new(90, 80), new(110, 70), new(110, 65), new(112, 70), new(120, 75), new(145, 60), new(130, 45)], hairColor_Dark, border, borderColor, matrix);
        HasBorderChainedQuadraticBezier(position + new Vector2(20, 10), [new(130, 45), new(145, 40), new(150, 30), new(135, 20), new(130, 5), new(125, -10), new(105, -65)], hairColor_Dark, border, borderColor, matrix);
        NoBorderPolygon(position + new Vector2(20, 10), [new(80, 70), new(132, 47), new(105, -65)], hairColor_Dark, matrix);
        //右下-前
        HasBorderChainedQuadraticBezier(position + new Vector2(20, 10), [new(90, -15), new(95, 40), new(70, 75), new(90, 70), new(100, 50), new(105, 65), new(125, 60), new(110, 20), new(115, -25)], hairColor, border, borderColor, matrix);

        //右上
        HasBorderChainedQuadraticBezier(position + new Vector2(20, 10), [new(10, -70), new(50, -20), new(110, -5), new(130, -50), new(90, -90), new(0, -120), new(-90, -100)], hairColor, border, borderColor, matrix);

        //填充
        NoBorderPolygon(position + new Vector2(20, 10), [new(-45, -60), new(10, -70), new(-90, -101), new(-122, -18)], hairColor, matrix);
        #endregion

        #region 帽子前沿
        HasBorderChainedQuadraticBezier(position + new Vector2(20, 10), [new(-130, -80), new(15, -160), new(140, -70), new(-30, -140), new(-130, -80)], hatColor, border, borderColor, matrix);

        #endregion

        #region 眼睛
        NoBorderEllipse(position + new Vector2(-50, 20), new Vector2(.5f), new Vector2(10, 20), Color.White, matrix);
        NoBorderEllipse(position + new Vector2(90, 20), new Vector2(.5f), new Vector2(10, 20), Color.White, matrix);
        HasBorderTunnel(position + new Vector2(-50, 0), default, new Vector2(20, 30), new Color(54, 181, 190), border, borderColor, matrix);
        HasBorderTunnel(position + new Vector2(50, 0), default, new Vector2(20, 30), new Color(54, 181, 190), border, borderColor, matrix);
        HasBorderRound(position + new Vector2(-50, 5), default, 20f, Color.White, border, borderColor, matrix);
        HasBorderRound(position + new Vector2(50, 5), default, 20f, Color.White, border, borderColor, matrix);
        #endregion

        #region 眉毛
        NoBorderQuadraticBezier(position + new Vector2(-60, 10), position + new Vector2(-30, -5), position + new Vector2(-15, 5), 4, borderColor, matrix);
        NoBorderQuadraticBezier(position + new Vector2(100, 10), position + new Vector2(70, -5), position + new Vector2(55, 5), 4, borderColor, matrix);

        NoBorderQuadraticBezier(position + new Vector2(-40, -8), position + new Vector2(-30, -12), position + new Vector2(-15, -5), border * .5f, borderColor, matrix);
        NoBorderQuadraticBezier(position + new Vector2(80, -8), position + new Vector2(70, -12), position + new Vector2(55, -5), border * .5f, borderColor, matrix);

        NoBorderQuadraticBezier(position + new Vector2(-55, -30), position + new Vector2(-40, -37), position + new Vector2(-25, -30), border * .5f, borderColor, matrix);
        NoBorderQuadraticBezier(position + new Vector2(95, -30), position + new Vector2(80, -37), position + new Vector2(65, -30), border * .5f, borderColor, matrix);

        #endregion

        #region 小嘴
        NoBorderArc(position + new Vector2(0, 60), default, 20, MathHelper.Pi / 4f, border * .5f, borderColor, matrix);
        #endregion
    }
    public static void Gallery(Vector2 position, Vector2 percentOrigin, Vector2 unitSize, Color backgroundColor, float border, Color borderColor, Texture2D barTexture, float time, float distanceScaler, Matrix matrix)
    {
        NoBorderBox(default, default, new Vector2(10000, 10000), Color.Black, MyUtils.GetMatrix(true));

        Vector2 start = position;
        Vector2 stepY = Vector2.UnitY * 2 * unitSize;
        Vector2 stepX = Vector2.UnitX * 1.5f * unitSize;

        #region 圆Circle/Round
        HasBorderRound(position, percentOrigin, unitSize.X, backgroundColor, border, borderColor, matrix);
        NoBorderRound(position + stepY, percentOrigin, unitSize.X, backgroundColor, matrix);
        BarRound(position + stepY * 2, percentOrigin, unitSize.X, barTexture, time, distanceScaler, matrix);
        position += stepX;
        #endregion

        #region 曲边矩形RoundedBox
        HasBorderRoundedBox(position, percentOrigin, unitSize, new Vector4(1, 4, 8, 16), backgroundColor, border, borderColor, matrix);
        NoBorderRoundedBox(position + stepY, percentOrigin, unitSize, new Vector4(1, 4, 8, 16), backgroundColor, matrix);
        BarRoundedBox(position + stepY * 2, percentOrigin, unitSize, new Vector4(1, 4, 8, 16), barTexture, time, distanceScaler, matrix);
        position += stepX;
        #endregion

        #region 矩形Box
        HasBorderBox(position, percentOrigin, unitSize, backgroundColor, border, borderColor, matrix);
        NoBorderBox(position + stepY, percentOrigin, unitSize, backgroundColor, matrix);
        BarBox(position + stepY * 2, percentOrigin, unitSize, barTexture, time, distanceScaler, matrix);
        position += stepX;
        #endregion

        #region 正交矩形OrientedBox
        HasBorderOrientedBox(position, position + unitSize, 20, backgroundColor, border, borderColor, matrix);
        NoBorderOrientedBox(position + stepY, position + stepY + unitSize, 20, backgroundColor, matrix);
        BarOrientedBox(position + stepY * 2, position + stepY * 2 + unitSize, 20, barTexture, time, distanceScaler, matrix);
        position += stepX;
        #endregion

        #region 线段Segment/Line
        HasBorderLine(position, position + unitSize, 10, backgroundColor, border, borderColor, matrix);
        NoBorderLine(position + stepY, position + stepY + unitSize, 10, backgroundColor, matrix);
        BarLine(position + stepY * 2, position + stepY * 2 + unitSize, 10, barTexture, time, distanceScaler, matrix);
        position += stepX;
        #endregion

        #region 菱形Rhombus
        HasBorderRhombus(position, percentOrigin, unitSize, backgroundColor, border, borderColor, matrix);
        NoBorderRhombus(position + stepY, percentOrigin, unitSize, backgroundColor, matrix);
        BarRhombus(position + stepY * 2, percentOrigin, unitSize, barTexture, time, distanceScaler, matrix);
        position += stepX;
        #endregion

        #region 梯形Trapezoid
        HasBorderTrapezoid(position, percentOrigin, unitSize, 2f, backgroundColor, border, borderColor, matrix);
        NoBorderTrapezoid(position + stepY, percentOrigin, unitSize, 2f, backgroundColor, matrix);
        BarTrapezoid(position + stepY * 2, percentOrigin, unitSize, 2f, barTexture, time, distanceScaler, matrix);
        position += stepX;
        #endregion

        #region 平行四边形Parallelogram
        HasBorderParallelogram(position, percentOrigin, unitSize, 0.5f, backgroundColor, border, borderColor, matrix);
        NoBorderParallelogram(position + stepY, percentOrigin, unitSize, 0.5f, backgroundColor, matrix);
        BarParallelogram(position + stepY * 2, percentOrigin, unitSize, 0.5f, barTexture, time, distanceScaler, matrix);
        position += stepX;
        #endregion

        #region 等边三角形EquilateralTriangle
        HasBorderEquilateralTriangle(position, percentOrigin, unitSize.X, backgroundColor, border, borderColor, matrix);
        NoBorderEquilateralTriangle(position + stepY, percentOrigin, unitSize.X, backgroundColor, matrix);
        BarEquilateralTriangle(position + stepY * 2, percentOrigin, unitSize.X, barTexture, time, distanceScaler, matrix);
        position += stepX;
        #endregion

        #region 等腰三角形TriangleIsosceles
        HasBorderTriangleIsosceles(position, percentOrigin, unitSize, backgroundColor, border, borderColor, matrix);
        NoBorderTriangleIsosceles(position + stepY, percentOrigin, unitSize, backgroundColor, matrix);
        BarTriangleIsosceles(position + stepY * 2, percentOrigin, unitSize, barTexture, time, distanceScaler, matrix);
        position += stepX;
        #endregion

        #region 三角形Triangle
        HasBorderTriangle(position, position + unitSize * new Vector2(1.0f, 0.5f), position + unitSize * new Vector2(0.5f, 1.0f), backgroundColor, border, borderColor, matrix);
        NoBorderTriangle(position + stepY, position + stepY + unitSize * new Vector2(1.0f, 0.5f), position + stepY + unitSize * new Vector2(0.5f, 1.0f), backgroundColor, matrix);
        BarTriangle(position + stepY * 2, position + stepY * 2 + unitSize * new Vector2(1.0f, 0.5f), position + stepY * 2 + unitSize * new Vector2(0.5f, 1.0f), barTexture, time, distanceScaler, matrix);
        position += stepX;
        #endregion

        #region 胶囊UnevenCapsule
        HasBorderUnevenCapsule(position, percentOrigin, unitSize.Y, unitSize.X * .5f, unitSize.X * .25f, backgroundColor, border, borderColor, matrix);
        NoBorderUnevenCapsule(position + stepY, percentOrigin, unitSize.Y, unitSize.X * .5f, unitSize.X * .25f, backgroundColor, matrix);
        BarUnevenCapsule(position + stepY * 2, percentOrigin, unitSize.Y, unitSize.X * .5f, unitSize.X * .25f, barTexture, time, distanceScaler, matrix);
        position += stepX;
        #endregion

        #region 正五边形Pentagon
        HasBorderPentagon(position, percentOrigin, unitSize.X * .5f, backgroundColor, border, borderColor, matrix);
        NoBorderPentagon(position + stepY, percentOrigin, unitSize.X * .5f, backgroundColor, matrix);
        BarPentagon(position + stepY * 2, percentOrigin, unitSize.X * .5f, barTexture, time, distanceScaler, matrix);
        position += stepX;
        #endregion

        #region 正六边形Hexagon
        HasBorderHexagon(position, percentOrigin, unitSize.X * .5f, backgroundColor, border, borderColor, matrix);
        NoBorderHexagon(position + stepY, percentOrigin, unitSize.X * .5f, backgroundColor, matrix);
        BarHexagon(position + stepY * 2, percentOrigin, unitSize.X * .5f, barTexture, time, distanceScaler, matrix);
        position += stepX;
        #endregion

        #region 正八边形Octogon
        HasBorderOctogon(position, percentOrigin, unitSize.X * .5f, backgroundColor, border, borderColor, matrix);
        NoBorderOctogon(position + stepY, percentOrigin, unitSize.X * .5f, backgroundColor, matrix);
        BarOctogon(position + stepY * 2, percentOrigin, unitSize.X * .5f, barTexture, time, distanceScaler, matrix);
        position += stepX;
        #endregion

        #region 正N边形Ngon
        HasBorderNgon(position, percentOrigin, unitSize.X * .5f, 3 + (int)Main.GlobalTimeWrappedHourly % 8, backgroundColor, border, borderColor, matrix);
        NoBorderNgon(position + stepY, percentOrigin, unitSize.X * .5f, 3 + (int)Main.GlobalTimeWrappedHourly % 8, backgroundColor, matrix);
        BarNgon(position + stepY * 2, percentOrigin, unitSize.X * .5f, 3 + (int)Main.GlobalTimeWrappedHourly % 8, barTexture, time, distanceScaler, matrix);
        position += stepX * (4 / 3f);
        #endregion

        #region 六芒星Hexagram
        HasBorderHexagram(position, percentOrigin, unitSize.X * .5f, backgroundColor, border, borderColor, matrix);
        NoBorderHexagram(position + stepY, percentOrigin, unitSize.X * .5f, backgroundColor, matrix);
        BarHexagram(position + stepY * 2, percentOrigin, unitSize.X * .5f, barTexture, time, distanceScaler, matrix);
        position += stepX;
        #endregion

        #region 五角星Star5
        HasBorderStar5(position, percentOrigin, unitSize.X * .5f, 0.5f, backgroundColor, border, borderColor, matrix);
        NoBorderStar5(position + stepY, percentOrigin, unitSize.X * .5f, 0.5f, backgroundColor, matrix);
        BarStar5(position + stepY * 2, percentOrigin, unitSize.X * .5f, 0.5f, barTexture, time, distanceScaler, matrix);
        position += stepX;
        #endregion

        #region X角星StarX
        HasBorderStarX(position, percentOrigin, unitSize.X * .5f, 3 + (int)Main.GlobalTimeWrappedHourly % 8, 0.5f, backgroundColor, border, borderColor, matrix);
        NoBorderStarX(position + stepY, percentOrigin, unitSize.X * .5f, 3 + (int)Main.GlobalTimeWrappedHourly % 8, 0.5f, backgroundColor, matrix);
        BarStarX(position + stepY * 2, percentOrigin, unitSize.X * .5f, 3 + (int)Main.GlobalTimeWrappedHourly % 8, 0.5f, barTexture, time, distanceScaler, matrix);
        position += stepX * (4 / 3f);
        #endregion

        #region 饼图Pie
        //float theta = MathHelper.Pi * (0.5f + 0.5f * MathF.Cos(Main.GlobalTimeWrappedHourly));// 
        float theta = MathHelper.Pi * 2 / 3f;
        HasBorderPie(position, percentOrigin, unitSize.X * .5f, theta, backgroundColor, border, borderColor, matrix);
        NoBorderPie(position + stepY, percentOrigin, unitSize.X * .5f, theta, backgroundColor, matrix);
        BarPie(position + stepY * 2, percentOrigin, unitSize.X * .5f, theta, barTexture, time, distanceScaler, matrix);
        position += stepX;
        #endregion

        #region 弓形CutDisk
        //float k = MathF.Cos(Main.GlobalTimeWrappedHourly);
        float k = 0.5f;
        HasBorderCutDisk(position, percentOrigin, unitSize.X * .5f, k, backgroundColor, border, borderColor, matrix);
        NoBorderCutDisk(position + stepY, percentOrigin, unitSize.X * .5f, k, backgroundColor, matrix);
        BarCutDisk(position + stepY * 2, percentOrigin, unitSize.X * .5f, k, barTexture, time, distanceScaler, matrix);
        position += stepX;
        #endregion

        #region 弦Arc
        HasBorderArc(position, percentOrigin, unitSize.X * .5f, theta, 4, backgroundColor, border, borderColor, matrix);
        NoBorderArc(position + stepY, percentOrigin, unitSize.X * .5f, theta, 4, backgroundColor, matrix);
        BarArc(position + stepY * 2, percentOrigin, unitSize.X * .5f, theta, 4, barTexture, time, distanceScaler, matrix);
        position += stepX;
        #endregion

        #region 环Ring
        HasBorderRing(position, percentOrigin, unitSize.X * .5f, theta, 7, backgroundColor, border, borderColor, matrix);
        NoBorderRing(position + stepY, percentOrigin, unitSize.X * .5f, theta, 7, backgroundColor, matrix);
        BarRing(position + stepY * 2, percentOrigin, unitSize.X * .5f, theta, 7, barTexture, time, distanceScaler, matrix);
        position += stepX;
        #endregion

        #region 马蹄铁Horseshoe
        HasBorderHorseshoe(position, percentOrigin, unitSize.X * .5f, theta, 7, 36, backgroundColor, border, borderColor, matrix);
        NoBorderHorseshoe(position + stepY, percentOrigin, unitSize.X * .5f, theta, 7, 36, backgroundColor, matrix);
        BarHorseshoe(position + stepY * 2, percentOrigin, unitSize.X * .5f, theta, 7, 36, barTexture, time, distanceScaler, matrix);
        position += stepX * 2f;
        #endregion

        #region 鱼鳔Vesica
        HasBorderVesica(position, percentOrigin, unitSize.X, unitSize.X * .5f, backgroundColor, border, borderColor, matrix);
        NoBorderVesica(position + stepY, percentOrigin, unitSize.X, unitSize.X * .5f, backgroundColor, matrix);
        BarVesica(position + stepY * 2, percentOrigin, unitSize.X, unitSize.X * .5f, barTexture, time, distanceScaler, matrix);
        position += stepX * (4 / 3f);
        #endregion

        #region 正交鱼鳔OrientedVesica
        HasBorderOrientedVesica(position, position + unitSize * .5f, unitSize.Length() * .5f, backgroundColor, border, borderColor, matrix);
        NoBorderOrientedVesica(position + stepY, position + stepY + unitSize * .5f, unitSize.Length() * .5f, backgroundColor, matrix);
        BarOrientedVesica(position + stepY * 2, position + stepY * 2 + unitSize * .5f, unitSize.Length() * .5f, barTexture, time, distanceScaler, matrix);
        //HasBorderRound(position - new Vector2(unitSize.Length()) * .5f , unitSize.Length(), Color.Transparent, border, Color.Cyan with { A = 0}, matrix);
        //HasBorderRound(position - new Vector2(unitSize.Length()) * .5f + unitSize * .5f, unitSize.Length(), Color.Transparent, border, Color.Cyan with { A = 0}, matrix);
        position += stepX;
        #endregion

        #region 月Moon
        HasBorderMoon(position, percentOrigin, unitSize.X * .5f, unitSize.X * .45f, unitSize.X * .2f, backgroundColor, border, borderColor, matrix);
        NoBorderMoon(position + stepY, percentOrigin, unitSize.X * .5f, unitSize.X * .45f, unitSize.X * .2f, backgroundColor, matrix);
        BarMoon(position + stepY * 2, percentOrigin, unitSize.X * .5f, unitSize.X * .45f, unitSize.X * .2f, barTexture, time, distanceScaler, matrix);
        position += stepX;
        #endregion

        #region 曲边四芒星CircleCross
        HasBorderCircleCross(position, percentOrigin, new Vector2(unitSize.Y, unitSize.X) * .75f, 4, backgroundColor, border, borderColor, matrix);
        NoBorderCircleCross(position + stepY, percentOrigin, new Vector2(unitSize.Y, unitSize.X) * .75f, 4, backgroundColor, matrix);
        BarCircleCross(position + stepY * 2, percentOrigin, new Vector2(unitSize.Y, unitSize.X) * .75f, 4, barTexture, time, distanceScaler, matrix);
        position += stepX * (4 / 3f);
        #endregion

        #region 蛋Egg
        HasBorderEgg(position, percentOrigin, unitSize.X * .5f, unitSize.X * .25f, unitSize.Y * .5f, backgroundColor, border, borderColor, matrix);
        NoBorderEgg(position + stepY, percentOrigin, unitSize.X * .5f, unitSize.X * .25f, unitSize.Y * .5f, backgroundColor, matrix);
        BarEgg(position + stepY * 2, percentOrigin, unitSize.X * .5f, unitSize.X * .25f, unitSize.Y * .5f, barTexture, time, distanceScaler, matrix);
        position += stepX;
        #endregion

        #region 心Heart
        HasBorderHeart(position, percentOrigin, unitSize.X, backgroundColor, border, borderColor, matrix);
        NoBorderHeart(position + stepY, percentOrigin, unitSize.X, backgroundColor, matrix);
        BarHeart(position + stepY * 2, percentOrigin, unitSize.X, barTexture, time, distanceScaler, matrix);
        position += stepX;
        #endregion

        #region 十字Cross/Plus
        HasBorderPlus(position, percentOrigin, unitSize.X * .75f, unitSize.X * .25f, unitSize.X * .15f, backgroundColor, border, borderColor, matrix);
        NoBorderPlus(position + stepY, percentOrigin, unitSize.X * .75f, unitSize.X * .25f, unitSize.X * .15f, backgroundColor, matrix);
        BarPlus(position + stepY * 2, percentOrigin, unitSize.X * .75f, unitSize.X * .25f, unitSize.X * .15f, barTexture, time, distanceScaler, matrix);
        position += stepX;
        #endregion

        #region 叉号RoundedX/Cross
        HasBorderCross(position, percentOrigin, unitSize.X, 6, backgroundColor, border, borderColor, matrix);
        NoBorderCross(position + stepY, percentOrigin, unitSize.X, 6, backgroundColor, matrix);
        BarCross(position + stepY * 2, percentOrigin, unitSize.X, 6, barTexture, time, distanceScaler, matrix);
        position += stepX;
        #endregion

        #region 多边形Polygon
        Vector2[] percents = [Vector2.Zero, new(0.5f, 1.0f), new(0.8f, 0f), new(0f, 0.6f), new(1, 0.8f)];
        HasBorderPolygon(position, unitSize, percents, backgroundColor, border, borderColor, matrix);
        NoBorderPolygon(position + stepY, unitSize, percents, backgroundColor, matrix);
        BarPolygon(position + stepY * 2, unitSize, percents, barTexture, time, distanceScaler, matrix);
        position += stepX;
        #endregion

        #region 椭圆Ellipse
        HasBorderEllipse(position, percentOrigin, unitSize * .5f, backgroundColor, border, borderColor, matrix);
        NoBorderEllipse(position + stepY, percentOrigin, unitSize * .5f, backgroundColor, matrix);
        BarEllipse(position + stepY * 2, percentOrigin, unitSize * .5f, barTexture, time, distanceScaler, matrix);
        position = start + stepY * 3;
        #endregion

        #region 抛物线Parabola
        HasBorderParabola(position, percentOrigin, unitSize, backgroundColor, border, borderColor, matrix);
        NoBorderParabola(position + stepY, percentOrigin, unitSize, backgroundColor, matrix);
        BarParabola(position + stepY * 2, percentOrigin, unitSize, barTexture, time, distanceScaler, matrix);
        position += stepX;
        #endregion

        #region 抛物线段ParabolaSegment
        HasBorderParabolaSegment(position, percentOrigin, unitSize, unitSize.Y * .5f, backgroundColor, border, borderColor, matrix);
        NoBorderParabolaSegment(position + stepY, percentOrigin, unitSize, unitSize.Y * .5f, backgroundColor, matrix);
        BarParabolaSegment(position + stepY * 2, percentOrigin, unitSize, unitSize.Y * .5f, barTexture, time, distanceScaler, matrix);
        position += stepX;
        #endregion

        #region 二阶贝塞尔曲线QuadraticBezier
        HasBorderQuadraticBezier(position, position + unitSize * new Vector2(2f, 0.5f), position + unitSize * new Vector2(0.5f, 1f), 4f, backgroundColor, border, borderColor, matrix);
        NoBorderQuadraticBezier(position + stepY, position + unitSize * new Vector2(2f, 0.5f) + stepY, position + unitSize * new Vector2(0.5f, 1f) + stepY, 4f, backgroundColor, matrix);
        BarQuadraticBezier(position + stepY * 2, position + unitSize * new Vector2(2f, 0.5f) + stepY * 2, position + unitSize * new Vector2(0.5f, 1f) + stepY * 2, 4f, barTexture, time, distanceScaler, matrix);
        position += stepX;
        #endregion

        #region 圆头十字BlobbyCross
        HasBorderBlobbyCross(position, percentOrigin, unitSize.X * .5f, 0.7f, 8f, backgroundColor, border, borderColor, matrix);
        NoBorderBlobbyCross(position + stepY, percentOrigin, unitSize.X * .5f, 0.7f, 8f, backgroundColor, matrix);
        BarBlobbyCross(position + stepY * 2, percentOrigin, unitSize.X * .5f, 0.7f, 8f, barTexture, time, distanceScaler, matrix);
        position += stepX * (4 / 3f);
        #endregion

        #region 隧道Tunnel
        HasBorderTunnel(position, percentOrigin, unitSize * .5f, backgroundColor, border, borderColor, matrix);
        NoBorderTunnel(position + stepY, percentOrigin, unitSize * .5f, backgroundColor, matrix);
        BarTunnel(position + stepY * 2, percentOrigin, unitSize * .5f, barTexture, time, distanceScaler, matrix);
        position += stepX;
        #endregion

        #region 楼梯Stairs
        HasBorderStairs(position, percentOrigin, unitSize * .2f, 5, backgroundColor, border, borderColor, matrix);
        NoBorderStairs(position + stepY, percentOrigin, unitSize * .2f, 5, backgroundColor, matrix);
        BarStairs(position + stepY * 2, percentOrigin, unitSize * .2f, 5, barTexture, time, distanceScaler, matrix);
        position += stepX;
        #endregion

        #region 四次方圆QuadraticCircle
        HasBorderQuadraticCircle(position, percentOrigin, unitSize.X * .5f, backgroundColor, border, borderColor, matrix);
        NoBorderQuadraticCircle(position + stepY, percentOrigin, unitSize.X * .5f, backgroundColor, matrix);
        BarQuadraticCircle(position + stepY * 2, percentOrigin, unitSize.X * .5f, barTexture, time, distanceScaler, matrix);
        position += stepX;
        #endregion

        #region 双曲线Hyperbola
        HasBorderHyperbola(position, percentOrigin, new Vector2(unitSize.X * 1.1f, unitSize.X), backgroundColor, border, borderColor, matrix);
        NoBorderHyperbola(position + stepY, percentOrigin, new Vector2(unitSize.X * 1.1f, unitSize.X), backgroundColor, matrix);
        BarHyperbola(position + stepY * 2, percentOrigin, new Vector2(unitSize.X * 1.1f, unitSize.X), barTexture, time, distanceScaler, matrix);
        position += stepX * (4 / 3f);
        #endregion

        #region 酷酷S CoolS
        HasBorderCoolS(position, percentOrigin, unitSize.Y, backgroundColor, border, borderColor, matrix);
        NoBorderCoolS(position + stepY, percentOrigin, unitSize.Y, backgroundColor, matrix);
        BarCoolS(position + stepY * 2f, percentOrigin, unitSize.Y, barTexture, time, distanceScaler, matrix);
        position += stepX;
        #endregion

        #region 圆波CircleWave
        HasBorderCircleWave(position, percentOrigin, unitSize.Y * 2, 0.75f, unitSize.X * .25f, 4, backgroundColor, border, borderColor, matrix);
        NoBorderCircleWave(position + stepY, percentOrigin, unitSize.Y * 2, 0.75f, unitSize.X * .25f, 4, backgroundColor, matrix);
        BarCircleWave(position + stepY * 2, percentOrigin, unitSize.Y * 2, 0.75f, unitSize.X * .25f, 4, barTexture, time, distanceScaler, matrix);
        position += stepX * 2.5f;
        #endregion

        #region 连段二阶贝塞尔曲线ChainedQuadraticBezier
        Vector2[] BPercents = [default, new(0.4f, -0.2f), new(1, .3f), new(.5f, .8f), new Vector2(0.2f, 0.6f)];
        HasBorderChainedQuadraticBezier(position, unitSize, BPercents, backgroundColor, border, borderColor, matrix);
        NoBorderChainedQuadraticBezier(position + stepY, unitSize, BPercents, backgroundColor, matrix);
        BarChainedQuadraticBezier(position + stepY * 2, unitSize, BPercents, barTexture, time, distanceScaler, matrix);
        #endregion
    }
}
