using ImproveGame.UIFramework;
using ImproveGame.UIFramework.Common;
using ImproveGame.UIFramework.Graphics2D;

namespace ImproveGame.UI.ModernConfig.OptionElements;

public sealed class OptionToggle : ModernConfigOption
{
    public override int labelReservedWidth => 60;
    protected override void OnBind()
    {
        if (VarType != typeof(bool))
            throw new Exception($"Field \"{OptionName}\" is not a bool");

        if (Enabled)
            _timer.ImmediateOpen();

    }
    public override void Update(GameTime gameTime)
    {
        _timer.Update();
        base.Update(gameTime);
        if (Enabled)
            _timer.Open();
        else
            _timer.Close();
    }

    public override void LeftMouseDown(UIMouseEvent evt)
    {
        base.LeftMouseDown(evt);
        if (!Interactable) return;
        Enabled = !Enabled;
        SoundEngine.PlaySound(SoundID.MenuTick);
    }

    public override void DrawSelf(SpriteBatch sb)
    {
        Color color = Color.Lerp(UIStyle.SwitchBg, UIStyle.SwitchBgHover, _timer.Schedule);
        Color color2 = Color.Lerp(UIStyle.SwitchBorder, UIStyle.SwitchBorderHover, _timer.Schedule);
        Color color3 = Color.Lerp(UIStyle.SwitchRound, UIStyle.SwitchRoundHover, _timer.Schedule);

        var dimensions = GetDimensions();
        var position = dimensions.Position();
        var size = dimensions.Size();

        base.DrawSelf(sb);

        if (Height.Pixels < 10)
            return;

        if (!Interactable)
        {
            color *= 0.5f;
            color2 = Color.Gray * 0.6f;
            color3 = Color.Gray * 0.6f;
        }

        // 开关
        var boxSize = new Vector2(48, 26);
        var boxPosition = new Vector2(position.X + size.X - boxSize.X - 6f, position.Y);

        Vector2 position1 = boxPosition + new Vector2(0, size.Y / 2 - boxSize.Y / 2);
        SDFRectangle.HasBorder(position1, boxSize, new Vector4(MathF.Min(boxSize.X, boxSize.Y) / 2), color, 2, color2, Main.UIScaleMatrix);

        Vector2 boxSize2 = new(boxSize.Y - 10);
        Vector2 position2 = boxPosition + Vector2.Lerp(new Vector2(3 + 2, size.Y / 2 - boxSize2.Y / 2),
            new Vector2(boxSize.X - 3 - 2 - boxSize2.X, size.Y / 2 - boxSize2.Y / 2), _timer.Schedule);
        SDFGraphics.NoBorderRound(position2, default, boxSize2.X, color3, MyUtils.GetMatrix(true));
    }

    private readonly AnimationTimer _timer = new(4);

    public bool Enabled
    {
        get => (bool)GetValue()!;
        set => SetValueDirect(value);//;ConfigHelper.SetConfigValue(Config, VariableInfo, value,Item, path: path);
    }
}