using ImproveGame.Common.Configs;
using ImproveGame.UIFramework.BaseViews;
using ImproveGame.UIFramework.Common;
using ImproveGame.UIFramework.Graphics2D;
using Terraria.ModLoader.UI;

namespace ImproveGame.UIFramework.SUIElements;

public class SUISwitch : View
{
    private const float InnerRowSpacing = 8;
    private readonly Func<bool> _getState;
    private readonly Action<bool> _setState;

    private bool State
    {
        get => _getState();
        set => _setState(value);
    }

    private readonly float _scale;
    private readonly string _text;
    private readonly Vector2 _textSize;
    private readonly Color _textColor = Color.White;
    private readonly Color _textBorderColor = Color.Black;
    private readonly AnimationTimer _timer = new AnimationTimer(4);
    private readonly string _hoverText;

    public SUISwitch(Func<bool> getState, Action<bool> setState, string text, float textScale = 1f, string hoverText = null)
    {
        _scale = textScale;
        _text = text;
        _hoverText = hoverText;
        _textSize = MyUtils.GetFontSize(text) * _scale;
        _getState = getState;
        _setState = setState;

        PaddingLeft = 12 * _scale;
        PaddingRight = 12 * _scale;
        PaddingTop = 5 * _scale;
        PaddingBottom = 5 * _scale;

        Width.Pixels = _textSize.X + (InnerRowSpacing + 48) * _scale + this.HPadding();
        Height.Pixels = MathF.Max(_textSize.Y, 26) + this.VPadding();
    }

    public override void Update(GameTime gameTime)
    {
        if (State)
        {
            _timer.Open();
        }
        else
        {
            _timer.Close();
        }
        _timer.Update();
        base.Update(gameTime);
    }

    public override void LeftMouseDown(UIMouseEvent evt)
    {
        State = !State;
        SoundEngine.PlaySound(SoundID.MenuTick);
        base.LeftMouseDown(evt);
    }

    public override void DrawSelf(SpriteBatch sb)
    {
        Color switchBgColor = _timer.Lerp(UIStyle.SwitchBg, UIStyle.SwitchBgHover);
        Color switchBorderColor = _timer.Lerp(UIStyle.SwitchBorder, UIStyle.SwitchBorderHover);
        Color roundColor = _timer.Lerp(UIStyle.SwitchRound, UIStyle.SwitchRoundHover);

        Vector2 innerPos = GetInnerDimensions().Position();
        Vector2 innerSize = GetInnerDimensions().Size();

        Vector2 switchSize = new Vector2(48, 26) * _scale;
        Vector2 switchPos = innerPos + new Vector2(0, (innerSize.Y - switchSize.Y) / 2);
        SDFRectangle.HasBorder(switchPos, switchSize, new Vector4(switchSize.MinXY() / 2), switchBgColor, 2, switchBorderColor, Main.UIScaleMatrix);

        Vector2 roundSize = new(switchSize.Y - 10 * _scale);
        Vector2 roundPos = innerPos + _timer.Lerp(new Vector2(3 + 2, innerSize.Y / 2 - roundSize.Y / 2),
            new Vector2(switchSize.X - 3 - 2 - roundSize.X, (innerSize.Y - roundSize.Y) / 2));
        SDFGraphics.NoBorderRound(roundPos, default, roundSize.X, roundColor, MyUtils.GetMatrix(true));

        Vector2 textPos = innerPos + new Vector2(switchSize.X + InnerRowSpacing * _scale, (innerSize.Y - _textSize.Y) / 2);
        textPos.Y += UIConfigs.Instance.GeneralFontOffsetY * _scale;
        MyUtils.DrawString(textPos, _text, _textColor, _textBorderColor, _scale);

        if (_hoverText is not null && IsMouseHovering)
            UICommon.TooltipMouseText(_hoverText);
    }
}
