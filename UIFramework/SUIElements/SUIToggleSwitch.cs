using ImproveGame.UIFramework.BaseViews;
using ImproveGame.UIFramework.Common;
using ImproveGame.UIFramework.Graphics2D;

namespace ImproveGame.UIFramework.SUIElements;

/// <summary>
/// 拨动开关
/// </summary>
public class SUIToggleSwitch : TimerView
{
    /// <summary>
    /// 状态
    /// </summary>
    public event Func<bool> Status;

    /// <summary>
    /// 切换
    /// </summary>
    public event Action Switch;

    /// <summary>
    /// 拨动圆的颜色
    /// </summary>
    public Color ToggleCircleColor;

    public readonly AnimationTimer SwitchTimer = new();

    public SUIToggleSwitch()
    {
        SetPadding(4f);

        ToggleCircleColor = UIStyle.SwitchRound;
        Border = 2f; BorderColor = UIStyle.SwitchBorder;
    }

    public void Toggle()
    {
        Switch?.Invoke();
        SoundEngine.PlaySound(SoundID.MenuTick);
    }

    public override void LeftMouseDown(UIMouseEvent evt)
    {
        Toggle();
        base.LeftMouseDown(evt);
    }

    public override void Update(GameTime gameTime)
    {
        if (Status?.Invoke() ?? false)
        {
            if (!SwitchTimer.AnyOpen)
            {
                SwitchTimer.Open();
            }
        }
        else
        {
            if (!SwitchTimer.AnyClose)
            {
                SwitchTimer.Close();
            }
        }

        SwitchTimer.Update();
        base.Update(gameTime);
    }

    public override void DrawSelf(SpriteBatch spriteBatch)
    {
        base.DrawSelf(spriteBatch);

        Vector2 innerPos = GetInnerDimensions().Position();
        Vector2 innerSize = GetInnerDimensionsSize();

        float circleSize = Math.Min(innerSize.X, innerSize.Y);

        SDFGraphics.NoBorderRound(innerPos + (innerSize - new Vector2(circleSize)) * SwitchTimer, default, circleSize, ToggleCircleColor, MyUtils.GetMatrix(true));
    }

    public static View CreateTextSwitch(out SUIToggleSwitch toggleSwitch, out SUIText text)
    {
        View view = new View();

        var t = new SUIText
        {
            TextAlign = new Vector2(0f, 0.5f),
            RelativeMode = RelativeMode.Horizontal,
            Spacing = new Vector2(4f)
        };
        t.JoinParent(view);

        var ts = new SUIToggleSwitch
        {
            HAlign = 1f,
        };
        ts.JoinParent(view);

        view.OnLeftMouseDown += (evt, uie) =>
        {
            if (evt.Target != ts)
                ts.Toggle();
        };

        toggleSwitch = ts;
        text = t;
        return view;
    }
}
