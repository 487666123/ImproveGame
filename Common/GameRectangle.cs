using ImproveGame.Common.ModSystems;
using Terraria.UI.Chat;

namespace ImproveGame.Common;

public enum TextDisplayType { None, Width, Height, All }

public class GameRectangle
{
    public int WhoAmI;
    public Rectangle Rectangle; // √
    public Color BorderColor; // √
    public Color BackgroundColor; // √
    public TextDisplayType TextDisplayType; // √
    public Texture2D Texture2D; // x
    /// <summary>
    /// 杀死 Box 的条件, 达到之后会从 DrawSystem.boxs 删除
    /// </summary>
    public Func<bool> NeedKill; // √
    public Action OnUpdate;
    public ModItem Parent; // √

    public GameRectangle(ModItem parent, Func<bool> needKill, Rectangle rectangle, Color backgroundColor, Color borderColor, TextDisplayType textDisplayMode)
    {
        Parent = parent;
        NeedKill = needKill;
        Rectangle = rectangle;
        BorderColor = borderColor;
        BackgroundColor = backgroundColor;
        TextDisplayType = textDisplayMode;
    }

    /// <summary>
    /// 创建, 如果你创建的Box已经存在 (通过 ModItem 来判断是否存在), 则直接修改已经存在的Box <para/>
    /// 第一个参数是 Box 对应的 ModItem, 每一个 Box 都必须绑定一个 ModItem <para/>
    /// 第二个参数是用来判断是否绘制的, 如果返回值为 <see langword="false"/> 会直接删除 box 对象再 <see cref="GameRectangleSystem.GameRectangles"/> 中的引用 <para/>
    /// </summary>
    /// <param name="rectangle"></param>
    /// <returns>它在 <see cref="GameRectangleSystem.GameRectangles"/> 的下标. 返回 -1 代表没有位置了, 那样子就不会生成了</returns>
    public static int Create(ModItem parent, Func<bool> needKill, Rectangle rectangle, Color backgroundColor, Color borderColor, TextDisplayType textDisplayMode = TextDisplayType.None)
    {
        GameRectangle[] gameRectangles = GameRectangleSystem.GameRectangles;
        int index = HasGameRectangle(parent);
        if (gameRectangles.IndexInRange(HasGameRectangle(parent)))
        {
            GameRectangle gameRectangle = gameRectangles[index];
            gameRectangle.NeedKill = needKill;
            gameRectangle.Rectangle = rectangle;
            gameRectangle.BackgroundColor = backgroundColor;
            gameRectangle.BorderColor = borderColor;
            gameRectangle.TextDisplayType = textDisplayMode;
            return index;
        }
        else
        {
            GameRectangle box = new(parent, needKill, rectangle, backgroundColor, borderColor, textDisplayMode);
            for (int i = 0; i < GameRectangleSystem.GameRectangles.Length; i++)
            {
                if (GameRectangleSystem.GameRectangles[i] == null)
                {
                    GameRectangleSystem.GameRectangles[i] = box;
                    box.WhoAmI = i;
                    return i;
                }
            }
        }
        return -1;
    }

    public static int Create(ModItem Parent, Func<bool> NeedKill, Point start, Point end, Color backgroundColor, Color borderColor, TextDisplayType textDisplayMode = TextDisplayType.None)
    {
        return Create(Parent, NeedKill, new((int)MathF.Min(start.X, end.X), (int)MathF.Min(start.Y, end.Y), (int)MathF.Abs(start.X - end.X) + 1, (int)MathF.Abs(start.Y - end.Y) + 1), backgroundColor, borderColor, textDisplayMode);
    }

    public static int HasGameRectangle(ModItem item)
    {
        GameRectangle[] gameRectangles = GameRectangleSystem.GameRectangles;
        for (int i = 0; i < gameRectangles.Length; i++)
        {
            GameRectangle gameRectangle = gameRectangles[i];
            if (gameRectangle != null && gameRectangle.Parent.Type == item.Type)
                return i;
        }
        return -1;
    }

    public void Update()
    {
        GameRectangle[] boxs = GameRectangleSystem.GameRectangles;
        OnUpdate?.Invoke();
        if ((NeedKill?.Invoke() ?? true) || Parent.Type != Main.LocalPlayer.HeldItem.type)
        {
            boxs[WhoAmI] = null;
        }
    }

    public void DrawRectangle()
    {
        Vector2 pos = Rectangle.TopLeft() * 16 + new Vector2(-2) - Main.screenPosition;
        Vector2 size = Rectangle.Size() * 16 + new Vector2(4);
        if (Main.LocalPlayer.gravDir is -1f)
            pos.Y = Main.screenHeight - pos.Y - size.Y;
        MyUtils.DrawBorder(pos, size.X, size.Y, BackgroundColor, BorderColor);
        //SDFRectangle.HasBorder(pos, size,
        //    new Vector4(2f), BackgroundColor, 2f, BorderColor, Main.GameViewMatrix.ZoomMatrix);
        // DrawBorderRect(Rectangle, backgroundColor, borderColor);
    }

    public void DrawPreView()
    {
        if (Texture2D is not null)
        {
            Vector2 pos = new Vector2(Rectangle.X, Rectangle.Y) * 16f - Main.screenPosition;
            bool reversal = Main.LocalPlayer.gravDir == -1;
            if (reversal)
                pos = new Vector2(pos.X + Texture2D.Size().X, Main.screenHeight - pos.Y);
            Main.spriteBatch.Draw(Texture2D, pos + new Vector2(-2), null, Color.White * 0.5f, (reversal ? 3.14f : 0), Vector2.Zero, 1f, (reversal ? SpriteEffects.FlipHorizontally : 0), 0);
        }
    }

    public void DrawString()
    {
        if (TextDisplayType is not TextDisplayType.None)
        {
            string text = string.Empty;

            switch (TextDisplayType)
            {
                case TextDisplayType.All:
                    text = $"{Rectangle.Width}×{Rectangle.Height}";
                    break;
                case TextDisplayType.Width:
                    text = Rectangle.Width.ToString();
                    break;
                case TextDisplayType.Height:
                    text = Rectangle.Height.ToString();
                    break;
            }

            Vector2 size = FontAssets.MouseText.Value.MeasureString(Rectangle.Width.ToString()) * 1.2f;
            Vector2 position = Main.MouseScreen + new Vector2(16, -size.Y + 6);
            ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, FontAssets.MouseText.Value, text,
                position, BorderColor, 0f, Vector2.Zero, new Vector2(1.2f));
            // Utils.DrawBorderString(Main.spriteBatch, text, position, BorderColor, 1.2f);
        }
    }
}